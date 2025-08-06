using System.Text.Json;

namespace financeTracker
{
    public class AccountDataService
    {
        private readonly string _filePath;

        // Constructor
        public AccountDataService()
        {
            _filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "BankAccounts.json");
        }

        // Save the list of bank accounts to a JSON file
        public async Task SaveAccountsAsync(List<BankAccount> bankAccounts)
        {
            try
            {
                string json = JsonSerializer.Serialize(bankAccounts, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }

        // Load the list of bank accounts from the JSON file
        public async Task<List<BankAccount>> LoadAccountsAsync()
        {
            if (!File.Exists(_filePath))
                return new List<BankAccount>();

            try
            {
                string json = await File.ReadAllTextAsync(_filePath);
                var bankAccounts = JsonSerializer.Deserialize<List<BankAccount>>(json);
                return bankAccounts ?? new List<BankAccount>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                return new List<BankAccount>();
            }
        }

        public async Task DeleteAccountAsync(string accountId)
        {
            var bankAccounts = await LoadAccountsAsync();

            // Find the account to delete
            var account = bankAccounts.FirstOrDefault(a => $"{a.Bank}-{a.Type}-{a.Name}" == accountId);
            if (account == null)
                return;

            // Remove the account from the list
            bankAccounts.Remove(account);

            // Save the updated list back to the file
            await SaveAccountsAsync(bankAccounts);
        }

        // Clear all account data (for debugging or reset)
        public void ClearData()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
