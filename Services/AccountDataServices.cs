using SQLite;
using System.Collections.ObjectModel;
using trackr.Models;

namespace trackr
{
    public class AccountDataService
    {
        private SQLiteAsyncConnection? _db;

        private async Task EnsureInitializedAsync()
        {
            if (_db == null)
            {
                await InitializeAccountDataAsync();
            }
        }

        private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
        {
            await EnsureInitializedAsync();
            return _db ?? throw new InvalidOperationException("SQLite connection was not initialized.");
        }

        public async Task InitializeAccountDataAsync()
        {
            if (_db != null) return;

            var dbPath = Path.Combine(FileSystem.Current.AppDataDirectory, "trackr.db3");
            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTablesAsync<BankAccount, Transaction>();
        }

        // Save or update all bank accounts and their transactions
        public async Task SaveAccounts(ObservableCollection<BankAccount> bankAccounts)
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            foreach (var account in bankAccounts)
            {
                await db.InsertOrReplaceAsync(account);
                if (account.Transactions != null)
                {
                    // Delete all existing transactions for this account
                    await db.ExecuteAsync("DELETE FROM Transactions WHERE BankAccountId = ?", account.Id);
                    // Insert all current transactions in a transaction block
                    await db.RunInTransactionAsync(conn =>
                    {
                        foreach (var transaction in account.Transactions)
                        {
                            transaction.BankAccountId = account.Id;
                            transaction.Id = 0; // Let SQLite auto-increment
                            conn.Insert(transaction);
                        }
                    });
                }
            }
        }

        // Load all bank accounts and their transactions
        public async Task<ObservableCollection<BankAccount>> LoadAccountsAsync()
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            var accounts = await db.Table<BankAccount>().ToListAsync();
            var transactions = await db.Table<Transaction>().ToListAsync();

            foreach (var account in accounts)
            {
                account.Transactions = new ObservableCollection<Transaction>(transactions.Where(t => t.BankAccountId == account.Id));
            }
            return new ObservableCollection<BankAccount>(accounts);
        }

        // Clear all account and transaction data
        public async Task ClearData()
        {
            SQLiteAsyncConnection db = await GetDatabaseAsync();

            await db.DeleteAllAsync<Transaction>();
            await db.DeleteAllAsync<BankAccount>();
        }
    }
}
