using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.ObjectModel;
using System.Globalization;
using trackr.Models;

public class CSVImportService
{
    public Dictionary<string, CSVColumnMap> BankMappings { get; } = new()
    {
        ["TD"] = new CSVColumnMap { DateIndex = 0, DescIndex = 1, AmountIndex = 2, BalanceIndex = 4, DateFormat = "MM/dd/yyyy" },
        ["CIBC"] = new CSVColumnMap { DateIndex = 0, DescIndex = 1, AmountIndex = 2, BalanceIndex = 999, DateFormat = "yyyy-MM-dd" },
        ["Capital One"] = new CSVColumnMap { DateIndex = 0, DescIndex = 3, AmountIndex = 5, BalanceIndex = 999, DateFormat = "yyyy-MM-dd" },
        ["RBC"] = new CSVColumnMap { DateIndex = 2, DescIndex = 1, AmountIndex = 6, BalanceIndex = 999, DateFormat = "dd/MM/yyyy" },
    };

    public static ObservableCollection<TransactionGroup> ParseTransactions(Stream csvStream, CSVColumnMap map, string bankAccountId)
    {
        ObservableCollection<TransactionGroup> transactionGroups = [];

        using StreamReader reader = new(csvStream);
        using CsvReader csv = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
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
                string rawDate = csv.GetField(map.DateIndex)!;
                string description = csv.GetField(map.DescIndex)!;
                string deposit = csv.GetField(map.AmountIndex)!;
                string credit = csv.GetField(map.AmountIndex + 1)!;
                string rawBalance = (map.BalanceIndex != 999) ? csv.GetField(map.BalanceIndex)! : "0.00";

                // Parse date
                if (!DateTime.TryParse(rawDate, out DateTime date)) continue;

                // Try parsing from deposits; fallback to credits if empty
                if (!decimal.TryParse(deposit, out decimal amount)) decimal.TryParse(credit, out amount);
                else amount *= -1;

                decimal.TryParse(rawBalance, out balance);

                transactionGroups.Add(new TransactionGroup(date,
                [
                    new Transaction
                    {
                        BankAccountId = bankAccountId,
                        Date = date,
                        Description = description,
                        Amount = amount,
                        Category = null
                    }
                ]));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing row: {ex.Message}");
            }
        }

        // // Update the credit card balances and amounts
        // if (map.BalanceIndex == 999)
        // {
        //     transactions.Reverse();
        //     balance = 0m;

        //     foreach (Transaction transaction in transactions)
        //     {
        //         balance += transaction.Amount;
        //         transaction.Category = null;
        //     }

        //     transactions.Reverse();
        // }

        return transactionGroups;
    }
}
