using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.ObjectModel;
using System.Globalization;
using financeTracker.Models;

public class CSVImportService
{
    public Dictionary<string, CSVColumnMap> BankMappings { get; } = new()
    {
        ["TD"] = new CSVColumnMap { DateIndex = 0, DescIndex = 1, AmountIndex = 2, BalanceIndex = 4, DateFormat = "MM/dd/yyyy" },
        ["CIBC"] = new CSVColumnMap { DateIndex = 0, DescIndex = 1, AmountIndex = 2, BalanceIndex = 999, DateFormat = "yyyy-MM-dd" },
        ["Capital One"] = new CSVColumnMap { DateIndex = 0, DescIndex = 3, AmountIndex = 5, BalanceIndex = 999, DateFormat = "yyyy-MM-dd" },
        // TODO: ["RBC"] = new CSVColumnMap { DateIndex = 0, DescIndex = 3, AmountIndex = 5, BalanceIndex = 999, DateFormat = "yyyy-MM-dd" },
        // TODO: ["Tangerine"] = new CSVColumnMap { DateIndex = 0, DescIndex = 3, AmountIndex = 5, BalanceIndex = 999, DateFormat = "yyyy-MM-dd" },
    };

    public ObservableCollection<Transaction> ParseTransactions(Stream csvStream, CSVColumnMap map)
    {
        var transactions = new ObservableCollection<Transaction>();

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            HeaderValidated = null,
            MissingFieldFound = null
        });

        decimal balance = 0m;

        while (csv.Read())
        {
            try
            {
                var rawDate = csv.GetField(map.DateIndex);
                var description = csv.GetField(map.DescIndex) ?? string.Empty;
                var deposit = csv.GetField(map.AmountIndex);
                var credit = csv.GetField(map.AmountIndex + 1);
                var rawBalance = (map.BalanceIndex != 999) ? csv.GetField(map.BalanceIndex) : "0.00";

                // Parse date
                if (!DateTime.TryParse(rawDate, out var date))
                    continue;

                // Try parsing from deposits; fallback to credits if empty
                decimal amount = 0m;
                if (!(decimal.TryParse(deposit, out amount)))
                {
                    decimal.TryParse(credit, out amount);
                }
                else
                {
                    amount = amount * -1;
                }

                decimal.TryParse(rawBalance, out balance);

                transactions.Add(new Transaction
                {
                    Date = date,
                    Description = description,
                    Amount = amount,
                    Balance = balance
                });
            }
            catch (Exception ex)
            {
                // Log or handle invalid rows if needed
                Console.WriteLine($"Error parsing row: {ex.Message}");
            }
        }

        // Update the credit card balances and amounts
        if (map.BalanceIndex == 999)
        {
            transactions.Reverse();

            balance = 0m;

            foreach (var transaction in transactions)
            {
                balance = balance + transaction.Amount;
                transaction.Balance = balance;
            }

            transactions.Reverse();
        }

        return transactions;
    }
}
