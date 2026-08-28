using trackr.Models;

namespace trackr.Services
{
    public interface IAccountDataService
    {
        Task SaveCategoryAsync(Category category);

        Task<Category> LoadCategoryAsync(int categoryId);

        Task SaveSubCategoryAsync(SubCategory subCategory);

        Task<SubCategory> LoadSubCategoryAsync(int categoryId);

        Task SaveAccountAsync(BankAccount account);

        Task<IReadOnlyList<BankAccount>> LoadAccountsAsync();

        Task DeleteAccountAsync(Guid accountId);

        Task<bool> AccountExistsAsync(BankAccount account);

        Task SaveTransactionsAsync(
            IEnumerable<Transaction> transactions);

        Task<IReadOnlyList<Transaction>> LoadTransactionsAsync(
            Guid? accountId);

        Task SaveImportBatchAsync(ImportBatch importBatch);

        Task<IReadOnlyList<ImportBatch>> LoadImportBatchesAsync(
            Guid accountId);
    }
}