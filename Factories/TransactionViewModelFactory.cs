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

            // Load the SubCategory and Category for the transaction
            if (transaction.SubCategoryId is not int subCategoryId)
                return viewModel; // If there is no subcategory, return the view model without setting the SubCategory property, which will default to "Uncategorized"

            SubCategory subCategory =
                await accountDataService.GetSubCategoryAsync(subCategoryId);

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

            return viewModel;
        }
    }
}