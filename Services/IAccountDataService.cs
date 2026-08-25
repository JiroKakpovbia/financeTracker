using System.Collections.ObjectModel;
using trackr.Models;

namespace trackr.Services
{
    public interface IAccountDataService
    {
        Task SaveAccountAsync(BankAccount account);

        Task<ObservableCollection<BankAccount>> LoadAccountsAsync();

        Task DeleteAccountAsync(BankAccount account);

        Task SaveTransactionsAsync(
            ObservableCollection<Transaction> transactions);

        Task<ObservableCollection<Transaction>> LoadTransactionsAsync(
            BankAccount account);

        Task SaveImportBatchAsync(ImportBatch importBatch);

        Task<ObservableCollection<ImportBatch>> LoadImportBatchesAsync(
            BankAccount account);
    }
}