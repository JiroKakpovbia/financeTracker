using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using trackr.Messages;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class CategorySelectorViewModel(IAccountDataService accountDataService) : ObservableObject
    {
        [ObservableProperty]
        private TransactionViewModel? transaction;

        [ObservableProperty]
        private CategoryViewModel? selectedCategory;

        [ObservableProperty]
        private SubCategoryViewModel? selectedSubCategory;

        public ObservableCollection<CategoryViewModel> AllCategories { get; } = [];

        private List<SubCategoryViewModel> AllSubCategories { get; } = [];

        public ObservableCollection<SubCategoryViewModel> CurrentSubCategories { get; } = [];

        public event Func<Task>? CloseRequested;

        public async Task InitializeAsync(TransactionViewModel transaction)
        {
            Transaction = transaction;

            await LoadCategoriesAsync();

            SelectedCategory =
                AllCategories.FirstOrDefault(
                    c => c.Model.Id ==
                        transaction.SubCategory?.Category.Model.Id);

            if (SelectedCategory is not null)
            {
                LoadCurrentSubCategories(SelectedCategory);

                SelectedSubCategory =
                    CurrentSubCategories.FirstOrDefault(
                        sc => sc.Model.Id ==
                            transaction.SubCategory?.Model.Id);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            AllCategories.Clear();
            AllSubCategories.Clear();
            CurrentSubCategories.Clear();

            IReadOnlyList<Category> categories = await accountDataService.GetAllCategoriesAsync();

            foreach (Category category in categories.OrderBy(c => c.Name))
            {
                CategoryViewModel categoryViewModel = new(category);

                AllCategories.Add(categoryViewModel);

                IReadOnlyList<SubCategory> subCategories = await accountDataService.GetSubCategoriesForCategoryAsync(category.Id);

                foreach (SubCategory subCategory in subCategories.OrderBy(sc => sc.Name))
                {
                    SubCategoryViewModel subCategoryViewModel = new(subCategory)
                    {
                        Category = categoryViewModel
                    };

                    AllSubCategories.Add(subCategoryViewModel);
                }
            };

            // Add a default "Uncategorized" category to the list of categories
            AllCategories.Add(new CategoryViewModel(new Category
            {
                Name = "Uncategorized"
            }));

            if (SelectedCategory is not null)
                OnSelectedCategoryChanged(SelectedCategory);
        }

        private void LoadCurrentSubCategories(
            CategoryViewModel category)
        {
            CurrentSubCategories.Clear();

            IEnumerable<SubCategoryViewModel>
                subCategories =
                    AllSubCategories.Where(
                        sc =>
                            sc.Model.Id ==
                            category.Model.Id);

            foreach (SubCategoryViewModel subCategory in subCategories)
                CurrentSubCategories.Add(subCategory);

            SelectedSubCategory = CurrentSubCategories.FirstOrDefault();
        }

        partial void OnSelectedCategoryChanged(CategoryViewModel? value)
        {
            CurrentSubCategories.Clear();

            if (value is null)
                return;

            LoadCurrentSubCategories(value);
        }

        [RelayCommand]
        private async Task Save()
        {
            if (Transaction is null)
                return;

            // Default to "Uncategorized" if no subcategory is selected
            Transaction.SubCategory = SelectedSubCategory ?? new(new SubCategory()) 
            {
                Name = "Uncategorized",
                Category = new CategoryViewModel(new Category
                {
                    Name = "Uncategorized",
                })
            };

            Transaction.Model.SubCategoryId = SelectedSubCategory?.Model.Id ?? null;

            await accountDataService.InsertTransactionAsync(
                Transaction.Model);

            // Tell the rest of the application this transaction changed.
            WeakReferenceMessenger.Default.Send(
                new TransactionUpdatedMessage(Transaction.Model.Id));

            await Close();
        }

        // Handle the closing of the Category Selector form, resetting the form and notifying any subscribers
        [RelayCommand]
        private async Task Close()
        {
            Console.WriteLine("Closing Category Selector form...");

            if (CloseRequested is not null)
                await CloseRequested.Invoke();
        }
    }
}