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
            public int DateIndex { get; set; }
            public int DescIndex { get; set; }
            public int DebitIndex { get; set; }
            public int CreditIndex { get; set; }
            public int? BalanceIndex { get; set; }
            public string DateFormat { get; set; } = string.Empty;
            public DateSortOrder DateSort { get; set; } = DateSortOrder.Descending;
        }

        // Class to define a CSV profile for a specific bank and account type
        private class BankCSVProfile
        {
            public BankInstitution Bank { get; set; }
            public AccountType Type { get; set; }
            public CSVColumnMap Mapping { get; set; } = new CSVColumnMap();
        }

        // Predefined CSV profiles for different banks and account types
        private IReadOnlyList<BankCSVProfile> Profiles { get; } = [
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
                    BalanceIndex = 4,
                    DateFormat = "MM/dd/yyyy",
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
                    BalanceIndex = 4,
                    DateFormat = "MM/dd/yyyy",
                    DateSort = DateSortOrder.Descending
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
                    BalanceIndex = null,
                    DateFormat = "yyyy-MM-dd",
                    DateSort = DateSortOrder.Descending
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
                    BalanceIndex = null,
                    DateFormat = "yyyy-MM-dd",
                    DateSort = DateSortOrder.Descending
                }
            },
            new()
            {
                Bank = BankInstitution.CapitalOne,
                Type = AccountType.Chequing,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 0,
                    DescIndex = 3,
                    DebitIndex = 2,
                    CreditIndex = 3,
                    BalanceIndex = null,
                    DateFormat = "yyyy-MM-dd",
                    DateSort = DateSortOrder.Descending
                }
            },
            new()
            {
                Bank = BankInstitution.CapitalOne,
                Type = AccountType.CreditCard,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 0,
                    DescIndex = 3,
                    DebitIndex = 2,
                    CreditIndex = 3,
                    BalanceIndex = null,
                    DateFormat = "yyyy-MM-dd",
                    DateSort = DateSortOrder.Descending
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
                    CreditIndex = 7,
                    BalanceIndex = null,
                    DateFormat = "dd/MM/yyyy",
                    DateSort = DateSortOrder.Descending
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
                    CreditIndex = 7,
                    BalanceIndex = null,
                    DateFormat = "dd/MM/yyyy",
                    DateSort = DateSortOrder.Descending
                }
            }
        ];

        // Import transactions from a CSV stream for a specific bank account
        public ObservableCollection<Transaction> ImportTransactions(Stream csvStream, BankAccount account)
        {
            BankCSVProfile? profile = GetProfile(account);

            Console.WriteLine($"Using CSV profile for {profile.Bank} ({profile.Type})");

            ObservableCollection<Transaction> transactions = ParseTransactions(
                csvStream,
                profile.Mapping,
                account);

            if (profile.Mapping.BalanceIndex == null && account.Type == AccountType.CreditCard)
                ApplyCreditCardBalances(transactions);

            return transactions;
        }

        // Get the CSV profile for a specific bank account
        private BankCSVProfile GetProfile(BankAccount account)
        {

            BankCSVProfile? profile = Profiles.FirstOrDefault(p =>
                p.Bank == account.BankInstitution &&
                p.Type == account.Type) ?? throw new InvalidOperationException(
                    $"No CSV profile exists for {account.BankInstitution} ({account.Type}).");
            return profile;
        }

        // Parse transactions from the CSV stream based on the provided profile
        private static ObservableCollection<Transaction> ParseTransactions(Stream csvStream, CSVColumnMap map, BankAccount account)
        {
            ObservableCollection<Transaction> transactions = [];

            using StreamReader reader = new(csvStream);
            using CsvReader csv = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                HeaderValidated = null,
                MissingFieldFound = null
            });

            while (csv.Read())
            {
                try
                {
                    string rawDate = csv.GetField(map.DateIndex)!;
                    string description = csv.GetField(map.DescIndex)!;
                    string deposit = csv.GetField(map.DebitIndex)!;
                    string credit = csv.GetField(map.CreditIndex)!;
                    string rawBalance = map.BalanceIndex.HasValue ? csv.GetField(map.BalanceIndex.Value)! : string.Empty;

                    // Try parsing from deposits; fallback to credits if empty
                    if (!decimal.TryParse(deposit, out decimal amount))
                        decimal.TryParse(credit, out amount);
                    else
                        amount *= -1;


                    // Parse date
                    if (!DateTime.TryParse(rawDate, out DateTime date))
                        continue;

                    // Parse balance if available
                    decimal? balance = null;
                    if (!string.IsNullOrWhiteSpace(rawBalance) && decimal.TryParse(rawBalance, out decimal parsedBalance))
                        balance = parsedBalance;

                    transactions.Add(new Transaction
                    {
                        BankAccountId = account.Id,
                        Date = date,
                        Description = description,
                        Amount = amount,
                        AccountBalance = balance,
                        SubCategoryId = null // TODO: Implement subcategory mapping if needed
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing row: {ex.Message}");
                }
            }

            // Ensure descending order for transactions
            if (map.DateSort == DateSortOrder.Ascending)
            {
                transactions = new ObservableCollection<Transaction>(
                    transactions
                        .GroupBy(t => t.Date.Date)
                        .OrderByDescending(g => g.Key)
                        .SelectMany(g => g.Reverse()));
            }

            return transactions;
        }

        // Apply closing balances to transactions based on the CSV mapping
        private static void ApplyCreditCardBalances(ObservableCollection<Transaction> transactions)
        {
            Console.WriteLine("Applying credit card balances to transactions...");

            if (transactions.Count != 0)
            {
                // No balance provided
                decimal balance = 0;

                foreach (var transaction in transactions.OrderBy(t => t.Date))
                {
                    balance += transaction.Amount;
                    transaction.AccountBalance = balance;
                }
            }
        }
    }
}