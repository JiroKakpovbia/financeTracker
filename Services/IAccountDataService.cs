using trackr.Models;

namespace trackr.Services
{
    public interface IAccountDataService
    {
        Task<IReadOnlyList<Category>> LoadCategoriesAsync();
        Task<IReadOnlyList<SubCategory>> LoadSubCategoriesAsync(Category category);

        Task SaveAccountAsync(BankAccount account);

        Task<IReadOnlyList<BankAccount>> LoadAccountsAsync();

        Task DeleteAccountAsync(Guid accountId);

        Task SaveTransactionsAsync(
            IEnumerable<Transaction> transactions);

        Task<IReadOnlyList<Transaction>> LoadTransactionsAsync(
            Guid accountId);

        Task SaveImportBatchAsync(ImportBatch importBatch);

        Task<IReadOnlyList<ImportBatch>> LoadImportBatchesAsync(
            Guid accountId);
    }
}