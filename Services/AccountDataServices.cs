using SQLite;
using System.Collections.ObjectModel;
using financeTracker.Models;

namespace financeTracker
{
    public class AccountDataService
    {
        private readonly SQLiteAsyncConnection _db;

        public AccountDataService()
        {
            var dbPath = Path.Combine(FileSystem.Current.AppDataDirectory, "financeTracker.db3");
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<BankAccount>().Wait();
            _db.CreateTableAsync<Transaction>().Wait();
        }

        // Save or update all bank accounts and their transactions
        public async Task SaveAccounts(ObservableCollection<BankAccount> bankAccounts)
        {
            foreach (var account in bankAccounts)
            {
                await _db.InsertOrReplaceAsync(account);
                if (account.Transactions != null)
                {
                    // Delete all existing transactions for this account
                    await _db.ExecuteAsync("DELETE FROM Transactions WHERE BankAccountId = ?", account.Id);
                    // Insert all current transactions in a transaction block
                    await _db.RunInTransactionAsync(conn =>
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
        public async Task<ObservableCollection<BankAccount>> LoadAccounts()
        {
            var accounts = await _db.Table<BankAccount>().ToListAsync();
            foreach (var account in accounts)
            {
                var transactions = await _db.Table<Transaction>().Where(t => t.BankAccountId == account.Id).ToListAsync();
                account.Transactions = new ObservableCollection<Transaction>(transactions);
            }
            return new ObservableCollection<BankAccount>(accounts);
        }

        // Clear all account and transaction data (for debugging or reset)
        public async Task ClearData()
        {
            await _db.DeleteAllAsync<Transaction>();
            await _db.DeleteAllAsync<BankAccount>();
        }
    }
}
