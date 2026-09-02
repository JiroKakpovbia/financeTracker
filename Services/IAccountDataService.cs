using trackr.Models;

namespace trackr.Services
{
    public interface IAccountDataService
    {
        Task<IReadOnlyList<Category>> GetAllCategoriesAsync();

        Task<Category> GetCategoryAsync(int categoryId);

        Task InsertCategoryAsync(Category category);

        Task UpdateCategoryAsync(Category category);

        Task<IReadOnlyList<SubCategory>> GetSubCategoriesForCategoryAsync(int categoryId);

        Task<SubCategory> GetSubCategoryAsync(int subCategoryId);

        Task InsertSubCategoryAsync(SubCategory subCategory);

        Task UpdateSubCategoryAsync(SubCategory subCategory);

        Task<IReadOnlyList<BankAccount>> GetAllAccountsAsync();

        Task<BankAccount> GetAccountAsync(Guid accountId);

        Task InsertAccountAsync(BankAccount account);

        Task UpdateAccountAsync(BankAccount account);

        Task DeleteAccountAsync(Guid accountId);

        Task<bool> AccountExistsAsync(BankAccount account);

        Task<IReadOnlyList<Transaction>> GetTransactionsForAccountAsync(Guid accountId);

        Task<Transaction?> GetTransactionAsync(int transactionId);

        Task InsertTransactionAsync(Transaction transaction);

        Task UpdateTransactionAsync(Transaction transaction);

        Task<IReadOnlyList<ImportBatch>> GetImportBatchesForAccountAsync(Guid accountId);

        Task<ImportBatch?> GetImportBatchAsync(int importBatchId);

        Task InsertImportBatchAsync(ImportBatch importBatch);
    }
}