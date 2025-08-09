using System.Collections.ObjectModel;
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
        public async Task SaveAccountsAsync(ObservableCollection<BankAccount> BankAccounts)
        {
            try
            {
                string json = JsonSerializer.Serialize(BankAccounts.ToList(), new JsonSerializerOptions
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
        public async Task<ObservableCollection<BankAccount>> LoadAccountsAsync()
        {
            if (!File.Exists(_filePath))
                return new ObservableCollection<BankAccount>();

            try
            {
                string json = await File.ReadAllTextAsync(_filePath);
                var BankAccounts = JsonSerializer.Deserialize<List<BankAccount>>(json);
                return new ObservableCollection<BankAccount>(BankAccounts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                return new ObservableCollection<BankAccount>();
            }
        }

        public async Task DeleteAccountAsync(string accountId)
        {
            var BankAccounts = await LoadAccountsAsync();

            // Find the account to delete
            var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null)
                return;

            // Remove the account from the list
            BankAccounts.Remove(account);

            // Save the updated list back to the file
            await SaveAccountsAsync(BankAccounts);
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
