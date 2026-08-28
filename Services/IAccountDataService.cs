using trackr.Models;

namespace trackr.Services
{
    public interface IAccountDataService
    {
        Task<IReadOnlyList<Category>> LoadAllCategoriesAsync();

        Task<Category> LoadCategoryAsync(int categoryId);

        Task SaveCategoryAsync(Category category);

        Task<IReadOnlyList<SubCategory>> LoadSubCategoriesForCategoryAsync(int categoryId);

        Task<SubCategory> LoadSubCategoryAsync(int subCategoryId);

        Task SaveSubCategoryAsync(SubCategory subCategory);

        Task<IReadOnlyList<BankAccount>> LoadAllAccountsAsync();

        Task<BankAccount> LoadAccountAsync(Guid accountId);

        Task SaveAccountAsync(BankAccount account);

        Task DeleteAccountAsync(Guid accountId);

        Task<bool> AccountExistsAsync(BankAccount account);

        Task<IReadOnlyList<Transaction>> LoadTransactionsForAccountAsync(Guid accountId);

        Task<Transaction?> LoadTransactionAsync(int transactionId);

        Task SaveTransactionAsync(Transaction transaction);

        Task<IReadOnlyList<ImportBatch>> LoadImportBatchesForAccountAsync(Guid accountId);

        Task<ImportBatch?> LoadImportBatchAsync(int importBatchId);

        Task SaveImportBatchAsync(ImportBatch importBatch);
    }
}