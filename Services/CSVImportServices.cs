// Services/CsvImportService.cs
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

public class CsvImportService
{
    public List<Transaction> ParseTransactions(Stream csvStream)
    {
        var transactions = new List<Transaction>();

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            HeaderValidated = null,
            MissingFieldFound = null
        });

        while (csv.Read())
        {
            try
            {
                var rawDate = csv.GetField(0);
                var description = csv.GetField(1) ?? string.Empty;
                var deposit = csv.GetField(2);
                var credit = csv.GetField(3);
                var balance = csv.GetField(4);

                // Parse date
                if (!DateTime.TryParse(rawDate, out var date))
                    continue;

                // Try parsing from index 2; fallback to index 3
                decimal amount = 0m;
                if (!(decimal.TryParse(deposit, out amount)))
                {
                    decimal.TryParse(credit, out amount);
                }
                else
                {
                    amount = amount * -1;
                }

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

        return transactions;
    }
}
