using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.ObjectModel;
using System.Globalization;
using trackr.Models;

namespace trackr.Services
{
    public class CSVImportService
    {
        private enum DateSortOrder
        {
            Ascending,
            Descending
        }

        private class CSVColumnMap
        {
            public int DateIndex { get; set; }
            public int DescIndex { get; set; }
            public int DebitIndex { get; set; }
            public int CreditIndex { get; set; }
            public int? BalanceIndex { get; set; }
            public string DateFormat { get; set; }
            public DateSortOrder SortOrder { get; set; } = DateSortOrder.Descending;
        }

        private class BankCSVProfile
        {
            public AccountBankInstitution Bank { get; set; }
            public AccountType Type { get; set; }
            public CSVColumnMap Mapping { get; set; }
        }

        private IReadOnlyList<BankCSVProfile> Profiles { get; } = [
            new()
            {
                Bank = AccountBankInstitution.TD,
                Type = AccountType.Chequing,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 0,
                    DescIndex = 1,
                    DebitIndex = 2,
                    CreditIndex = 3,
                    BalanceIndex = 4,
                    DateFormat = "MM/dd/yyyy",
                    SortOrder = DateSortOrder.Ascending
                }
            },
            new()
            {
                Bank = AccountBankInstitution.TD,
                Type = AccountType.CreditCard,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 0,
                    DescIndex = 1,
                    DebitIndex = 2,
                    CreditIndex = 3,
                    BalanceIndex = 4,
                    DateFormat = "MM/dd/yyyy",
                    SortOrder = DateSortOrder.Descending
                }
            },
            new()
            {
                Bank = AccountBankInstitution.CIBC,
                Type = AccountType.Chequing,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 0,
                    DescIndex = 1,
                    DebitIndex = 2,
                    CreditIndex = 3,
                    BalanceIndex = null,
                    DateFormat = "yyyy-MM-dd",
                    SortOrder = DateSortOrder.Descending
                }
            },
            new()
            {
                Bank = AccountBankInstitution.CIBC,
                Type = AccountType.CreditCard,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 0,
                    DescIndex = 1,
                    DebitIndex = 2,
                    CreditIndex = 3,
                    BalanceIndex = null,
                    DateFormat = "yyyy-MM-dd",
                    SortOrder = DateSortOrder.Descending
                }
            },
            new()
            {
                Bank = AccountBankInstitution.CapitalOne,
                Type = AccountType.Chequing,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 0,
                    DescIndex = 3,
                    DebitIndex = 2,
                    CreditIndex = 3,
                    BalanceIndex = null,
                    DateFormat = "yyyy-MM-dd",
                    SortOrder = DateSortOrder.Descending
                }
            },
            new()
            {
                Bank = AccountBankInstitution.CapitalOne,
                Type = AccountType.CreditCard,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 0,
                    DescIndex = 3,
                    DebitIndex = 2,
                    CreditIndex = 3,
                    BalanceIndex = null,
                    DateFormat = "yyyy-MM-dd",
                    SortOrder = DateSortOrder.Descending
                }
            },
            new()
            {
                Bank = AccountBankInstitution.RBC,
                Type = AccountType.Chequing,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 2,
                    DescIndex = 1,
                    DebitIndex = 6,
                    CreditIndex = 7,
                    BalanceIndex = null,
                    DateFormat = "dd/MM/yyyy",
                    SortOrder = DateSortOrder.Descending
                }
            },
            new()
            {
                Bank = AccountBankInstitution.RBC,
                Type = AccountType.CreditCard,
                Mapping = new CSVColumnMap
                {
                    DateIndex = 2,
                    DescIndex = 1,
                    DebitIndex = 6,
                    CreditIndex = 7,
                    BalanceIndex = null,
                    DateFormat = "dd/MM/yyyy",
                    SortOrder = DateSortOrder.Descending
                }
            }
        ];

        public ObservableCollection<TransactionGroup> ImportTransactions(Stream csvStream, BankAccount account)
        {
            var profile = GetProfile(account);

            ObservableCollection<Transaction> transactions = ParseTransactions(
                csvStream,
                profile,
                account);

            ApplyBalances(transactions, profile.Mapping);

            UpdateAccountBalance(account, transactions, profile.Mapping);

            return GroupTransactions(transactions);
        }

        private BankCSVProfile GetProfile(BankAccount account)
        {

            var profile = Profiles.FirstOrDefault(p =>
                p.Bank == account.BankInstitution &&
                p.Type == account.Type) ?? throw new InvalidOperationException(
                    $"No CSV profile exists for {account.BankInstitution} ({account.Type}).");
            return profile;
        }

        private static ObservableCollection<Transaction> ParseTransactions(Stream csvStream, BankCSVProfile profile, BankAccount account)
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
                    CSVColumnMap map = profile.Mapping;
                    string rawDate = csv.GetField(map.DateIndex)!;
                    string description = csv.GetField(map.DescIndex)!;
                    string deposit = csv.GetField(map.DebitIndex)!;
                    string credit = csv.GetField(map.CreditIndex)!;
                    string rawBalance = map.BalanceIndex.HasValue ? csv.GetField(map.BalanceIndex.Value)! : string.Empty;

                    // Try parsing from deposits; fallback to credits if empty
                    if (!decimal.TryParse(deposit, out decimal amount)) decimal.TryParse(credit, out amount);
                    else amount *= -1;


                    // Parse date
                    if (!DateTime.TryParse(rawDate, out DateTime date)) continue;

                    // Parse balance if available
                    decimal? balance = null;
                    if (!string.IsNullOrWhiteSpace(rawBalance) && decimal.TryParse(rawBalance, out decimal parsedBalance))
                    {
                        balance = parsedBalance;
                    }

                    transactions.Add(new Transaction
                    {
                        BankAccountId = account.Id,
                        Date = date,
                        Description = description,
                        Amount = amount,
                        AccountBalance = balance,
                        Category = null
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing row: {ex.Message}");
                }
            }

            // Ensure descending order for transactions
            if (profile.Mapping.SortOrder == DateSortOrder.Ascending)
            {
                transactions = new ObservableCollection<Transaction>(
                    transactions.Reverse());
            }

            return transactions;
        }

        private static void ApplyBalances(ObservableCollection<Transaction> transactions, CSVColumnMap map)
        {
            if (transactions.Count == 0)
                return;

            if (map.BalanceIndex.HasValue)
            {
                // Bank provides balance
                var latest = transactions
                    .OrderByDescending(t => t.Date)
                    .First();

                foreach (var transaction in transactions)
                {
                    transaction.AccountBalance = latest.AccountBalance;
                }
            }
            else
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

        private static void UpdateAccountBalance(BankAccount account, IEnumerable<Transaction> transactions, CSVColumnMap map)
        {
            if (!transactions.Any())
                return;

            if (map.BalanceIndex.HasValue)
            {
                Transaction latest = map.SortOrder == DateSortOrder.Descending
                    ? transactions.First()
                    : transactions.Last();

                account.Balance = latest.AccountBalance ?? 0m;
            }
            else
            {
                account.Balance = transactions
                    .OrderBy(t => t.Date)
                    .Last()
                    .AccountBalance ?? 0m;
            }
        }

        private static ObservableCollection<TransactionGroup> GroupTransactions(IEnumerable<Transaction> transactions)
        {
            return new ObservableCollection<TransactionGroup>(
                transactions
                    .GroupBy(t => t.Date.Date)
                    .OrderByDescending(g => g.Key)
                    .Select(g => new TransactionGroup(
                        g.Key,
                        g.OrderByDescending(t => t.Date))));
        }
    }
}