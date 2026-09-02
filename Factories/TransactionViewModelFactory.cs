using trackr.Models;
using trackr.Services;
using trackr.ViewModels;

namespace trackr.Factories
{
    public class TransactionViewModelFactory(
        IAccountDataService accountDataService)
        : ITransactionViewModelFactory
    {
        public async Task<TransactionViewModel> CreateAsync(
            Transaction transaction)
        {
            TransactionViewModel viewModel = new(transaction);

            // If there is no subcategory, return the view model without setting the SubCategory property, which will default to "Uncategorized"
            if (transaction.SubCategoryId is not int subCategoryId)
                return viewModel;

            // Load the SubCategory for the transaction
            SubCategory subCategory =
                await accountDataService.GetSubCategoryAsync(subCategoryId);

            // Load the Category for the transaction
            Category category =
                await accountDataService.GetCategoryAsync(
                    subCategory.CategoryId);

            // If there is a subcategory, create a SubCategoryViewModel and associate it with the corresponding CategoryViewModel
            SubCategoryViewModel subCategoryViewModel =
                new(subCategory)
                {
                    Category =
                    new CategoryViewModel(category)
                };

            viewModel.SubCategory = subCategoryViewModel;

            // Load the BankAccount for the transaction
            BankAccount account =
                await accountDataService.GetAccountAsync(transaction.BankAccountId);

            // Set the AccountName and AccountInstitution properties of the TransactionViewModel based on the associated BankAccount
            viewModel.AccountName = account.Name;

            viewModel.AccountInstitution = account.Institution;

            // Load the ImportBatch for the transaction and set the ImportedAt property of the TransactionViewModel
            IReadOnlyList<ImportBatch> importBatches =
                await accountDataService.GetImportBatchesForAccountAsync(account.Id);

            viewModel.ImportedAt =
                importBatches
                    .First(
                        batch =>
                            batch.Id ==
                            transaction.ImportBatchId)
                    .ImportedAt;

            return viewModel;
        }
    }
}