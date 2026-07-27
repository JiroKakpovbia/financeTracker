using SQLite;
using System.Collections.ObjectModel;
using trackr.Models;

namespace trackr
{
    public class AccountDataService
    {
        private SQLiteAsyncConnection? _db;

        // Ensure the database is initialized before any operations
        private async Task EnsureInitializedAsync()
        {
            if (_db == null)
                await InitializeAccountDataAsync();
        }

        // Get the SQLiteAsyncConnection, initializing it if necessary
        private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
        {
            await EnsureInitializedAsync();
            return _db ?? throw new InvalidOperationException("SQLite connection was not initialized.");
        }

        // Initialize the SQLite database connection and create tables if they don't exist
        private async Task InitializeAccountDataAsync()
        {
            if (_db != null)
                return;

            Console.WriteLine("Initializing SQLite database connection...");

            try
            {
                var dbPath = Path.Combine(FileSystem.Current.AppDataDirectory, "trackr.db3");
                _db = new SQLiteAsyncConnection(
                    dbPath,
                    SQLiteOpenFlags.ReadWrite |
                    SQLiteOpenFlags.Create |
                    SQLiteOpenFlags.FullMutex);
                await _db.CreateTablesAsync<BankAccount, Transaction, Category, SubCategory>();

                Console.WriteLine($"SQLite database initialized at {dbPath}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing SQLite database: {ex.Message}\n");
                throw;
            }
        }

        // Save or update a single bank account
        public async Task SaveAccountAsync(BankAccount account)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Saving account {account.Id}");

            try
            {
                await db.RunInTransactionAsync(async conn =>
                {
                    await db.InsertOrReplaceAsync(account);
                    await db.InsertAllAsync(account.Transactions.ToList());
                });

                Console.WriteLine($"Account {account.Id} saved successfully.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving account {account.Id}: {ex.Message}\n");
                throw;
            }
        }

        // Delete a bank account and its associated transactions    
        public async Task DeleteAccountAsync(BankAccount account)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine($"Deleting account {account.Id}");

            try
            {
                await db.RunInTransactionAsync(conn =>
                {
                    conn.Execute(
                        "DELETE FROM Transactions WHERE BankAccountId = ?",
                        account.Id);
                });

                await db.DeleteAsync(account);

                Console.WriteLine($"Deleted account {account.Id}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting account {account.Id}: {ex.Message}\n");
                throw;
            }
        }

        // Load all bank accounts and their transactions
        public async Task<ObservableCollection<BankAccount>> LoadAccountsAsync()
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            Console.WriteLine("Loading accounts and transactions from database...");

            try
            {
                var accounts = await db.Table<BankAccount>().ToListAsync();
                var transactions = await db.Table<Transaction>().ToListAsync();

                foreach (var account in accounts)
                {
                    account.Transactions = transactions.Where(t => t.BankAccountId == account.Id).ToList();
                }

                Console.WriteLine($"Loaded {accounts.Count} accounts and their transactions from database.\n");

                return new ObservableCollection<BankAccount>(accounts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading accounts: {ex.Message}\n");
                throw;
            }
        }
    }
}
