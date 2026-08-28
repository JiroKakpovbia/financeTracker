using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.ObjectModel;
using System.Globalization;
using trackr.Models;

namespace trackr.Services
{
    public class CSVImportService : ICSVImportService
    {
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
                    DescIndex = 4,
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
                    DescIndex = 4,
                    DebitIndex = 6,
                    CreditIndex = 6,
                    DateFormat = "d/M/yyyy",
                }
            }
        ];

        // Get the transaction amount from the debit and credit columns, applying the appropriate multiplier
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

        // Prompt the user to pick a CSV file and return the selected file result
        public async Task<FileResult?> PickCSVFileAsync()
        {
            PickOptions options = new()
            {
                PickerTitle = "Select a CSV file",

                FileTypes = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                { DevicePlatform.Android, new[] { "text/csv" } },
                { DevicePlatform.WinUI, new[] { ".csv" } },
                { DevicePlatform.macOS, new[] { "public.comma-separated-values-text" } },
                    })
            };

            return await FilePicker.PickAsync(options);
        }

        // Parse transactions from the CSV stream based on the provided profile
        public async Task<IReadOnlyList<Transaction>> ParseTransactions(Stream csvStream, BankAccount account)
        {
            ArgumentNullException.ThrowIfNull(csvStream);
            ArgumentNullException.ThrowIfNull(account);

            // Get the appropriate CSV profile and mapping for the bank account
            CSVProfile profile = Profiles.FirstOrDefault(p =>
                        p.Bank == account.Institution &&
                        p.Type == account.Type) ??
                            throw new InvalidOperationException(
                            $"No CSV profile exists for {account.Institution} ({account.Type}).");

            CSVColumnMap map = profile.Mapping;

            Console.WriteLine($"Using CSV profile for {profile.Bank} ({profile.Type})");

            List<(Transaction Transaction, int RowOrder)> parsedRows = [];

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

                    // Parse the date using the specified format and ensure it is valid
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

                    // Parse the amount from either the debit or credit column, applying the appropriate multiplier
                    if (!GetTransactionAmount(debit, credit, map, out decimal amount))
                    {
                        Console.WriteLine($"Skipping row {rowOrder}: no valid debit/credit amount.");
                        continue;
                    }

                    // Create a new transaction object and add it to the parsed rows list
                    parsedRows.Add((new Transaction
                    {
                        BankAccountId = account.Id,
                        Date = date.Date,
                        Description = description.Trim(),
                        Amount = amount,
                    }, rowOrder));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing CSV row {rowOrder}: {ex.Message}");
                }
            }

            // Sort the parsed transactions by date (descending) and row order (descending) to maintain the original order for transactions on the same date
            IEnumerable<Transaction> orderedTransactions = parsedRows
                .OrderByDescending(r => r.Transaction.Date)
                .ThenByDescending(r => r.RowOrder)
                .Select(r => r.Transaction);

            return new ObservableCollection<Transaction>(orderedTransactions);
        }
    }
}