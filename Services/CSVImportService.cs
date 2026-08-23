using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.ObjectModel;
using System.Globalization;
using trackr.Models;

namespace trackr.Services
{
    public class CSVImportService
    {
        // Enum to define the sort order for dates
        private enum DateSortOrder
        {
            Ascending,
            Descending
        }

        // Class to map CSV columns to transaction properties
        private class CSVColumnMap
        {
            public bool HasHeader { get; set; } = false;
            public int DateIndex { get; set; }
            public int DescIndex { get; set; }
            public int DebitIndex { get; set; }
            public int CreditIndex { get; set; }
            public int DebitMultiplier { get; set; } = 1;
            public int CreditMultiplier { get; set; } = -1;
            public string DateFormat { get; set; } = string.Empty;
            public DateSortOrder DateSort { get; set; } = DateSortOrder.Descending;
        }

        // Class to define a CSV profile for a specific bank and account type
        private class CSVProfile
        {
            public BankInstitution Bank { get; set; }
            public AccountType Type { get; set; }
            public CSVColumnMap Mapping { get; set; } = new CSVColumnMap();
        }

        // Predefined CSV profiles for different banks and account types
        private static IReadOnlyList<CSVProfile> Profiles { get; } = [
            new()
            {
                Bank = BankInstitution.TD,
                Type = AccountType.Chequing,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 0,
                    DescIndex = 1,
                    DebitIndex = 2,
                    CreditIndex = 3,
                    DateFormat = "yyyy-dd-MM",
                    DateSort = DateSortOrder.Ascending
                }
            },
            new()
            {
                Bank = BankInstitution.TD,
                Type = AccountType.CreditCard,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 0,
                    DescIndex = 1,
                    DebitIndex = 2,
                    CreditIndex = 3,
                    DateFormat = "MM/dd/yyyy",
                }
            },
            new()
            {
                Bank = BankInstitution.CIBC,
                Type = AccountType.Chequing,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 0,
                    DescIndex = 1,
                    DebitIndex = 2,
                    CreditIndex = 3,
                    DateFormat = "yyyy-MM-dd",
                }
            },
            new()
            {
                Bank = BankInstitution.CIBC,
                Type = AccountType.CreditCard,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 0,
                    DescIndex = 1,
                    DebitIndex = 2,
                    CreditIndex = 3,
                    DateFormat = "yyyy-MM-dd",
                }
            },
            new()
            {
                Bank = BankInstitution.CapitalOne,
                Type = AccountType.Chequing,
                Mapping = new CSVColumnMap
                {
                    HasHeader = true,
                    DateIndex = 0,
                    DescIndex = 3,
                    DebitIndex = 2,
                    CreditIndex = 6,
                    DateFormat = "yyyy-MM-dd",
                }
            },
            new()
            {
                Bank = BankInstitution.CapitalOne,
                Type = AccountType.CreditCard,
                Mapping = new CSVColumnMap
                {
                    HasHeader = true,
                    DateIndex = 0,
                    DescIndex = 3,
                    DebitIndex = 2,
                    CreditIndex = 6,
                    DateFormat = "yyyy-MM-dd",
                }
            },
            new()
            {
                Bank = BankInstitution.RBC,
                Type = AccountType.Chequing,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 2,
                    DescIndex = 1,
                    DebitIndex = 6,
                    CreditIndex = 6,
                    DateFormat = "d/M/yyyy",
                }
            },
            new()
            {
                Bank = BankInstitution.RBC,
                Type = AccountType.CreditCard,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 2,
                    DescIndex = 1,
                    DebitIndex = 6,
                    CreditIndex = 6,
                    DateFormat = "d/M/yyyy",
                }
            }
        ];

        // Get the CSV profile for a specific bank account
        private static CSVProfile GetProfile(BankAccount account)
        {
            return Profiles.FirstOrDefault(p =>
                       p.Bank == account.Institution &&
                       p.Type == account.Type)
                   ?? throw new InvalidOperationException(
                       $"No CSV profile exists for {account.Institution} ({account.Type}).");
        }

        // Parse transactions from the CSV stream based on the provided profile
        public static ObservableCollection<Transaction> ParseTransactions(Stream csvStream, BankAccount account, int importBatchId = 0)
        {
            ArgumentNullException.ThrowIfNull(csvStream);
            ArgumentNullException.ThrowIfNull(account);

            CSVProfile profile = GetProfile(account);
            CSVColumnMap map = profile.Mapping;

            Console.WriteLine($"Using CSV profile for {profile.Bank} ({profile.Type})");

            var parsedRows = new List<(Transaction Transaction, int RowOrder)>();

            using StreamReader reader = new(csvStream);
            using CsvReader csv = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = map.HasHeader,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim
            });

            if (map.HasHeader)
            {
                if (!csv.Read()) return [];
                csv.ReadHeader();
            }

            int rowOrder = 0;

            while (csv.Read())
            {
                rowOrder++;

                try
                {
                    string rawDate = csv.GetField(map.DateIndex) ?? string.Empty;
                    string description = csv.GetField(map.DescIndex) ?? string.Empty;
                    string debit = csv.GetField(map.DebitIndex) ?? string.Empty;
                    string credit = csv.GetField(map.CreditIndex) ?? string.Empty;

                    // parse the date using the specified format and ensure it is valid
                    if (!DateTime.TryParseExact(
                        rawDate.Trim(),
                        map.DateFormat,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime date))
                    {
                        Console.WriteLine($"Skipping row {rowOrder}: invalid date '{rawDate}'.");
                        continue;
                    }

                    // parse the amount from either the debit or credit column, applying the appropriate multiplier
                    if (!GetTransactionAmount(debit, credit, map, out decimal amount))
                    {
                        Console.WriteLine($"Skipping row {rowOrder}: no valid debit/credit amount.");
                        continue;
                    }

                    // create a new transaction object and add it to the parsed rows list
                    parsedRows.Add((new Transaction
                    {
                        BankAccountId = account.Id,
                        Date = date.Date,
                        Description = description.Trim(),
                        Amount = amount,
                        ImportBatchId = importBatchId
                    }, rowOrder));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing CSV row {rowOrder}: {ex.Message}");
                }
            }

            IEnumerable<Transaction> orderedTransactions = map.DateSort switch
            {
                DateSortOrder.Ascending => parsedRows
                    .OrderByDescending(r => r.Transaction.Date)
                    .ThenByDescending(r => r.RowOrder)
                    .Select(r => r.Transaction),

                _ => parsedRows
                    .OrderByDescending(r => r.Transaction.Date)
                    .ThenBy(r => r.RowOrder)
                    .Select(r => r.Transaction)
            };

            return new ObservableCollection<Transaction>(orderedTransactions);
        }
        private static bool GetTransactionAmount(string debit, string credit, CSVColumnMap map, out decimal amount)
        {
            if (decimal.TryParse(
                debit,
                NumberStyles.Number | NumberStyles.AllowCurrencySymbol,
                CultureInfo.InvariantCulture,
                out decimal debitAmount))
            {
                amount = Math.Abs(debitAmount) * map.DebitMultiplier;
                return true;
            }

            if (decimal.TryParse(
                credit,
                NumberStyles.Number | NumberStyles.AllowCurrencySymbol,
                CultureInfo.InvariantCulture,
                out decimal creditAmount))
            {
                amount = Math.Abs(creditAmount) * map.CreditMultiplier;
                return true;
            }

            amount = 0m;
            return false;
        }

    }
}