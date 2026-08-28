using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using trackr.Factories;
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
                    c => c.Id ==
                        transaction.SubCategory?.Category.Id);

            if (SelectedCategory is not null)
            {
                LoadCurrentSubCategories(SelectedCategory);

                SelectedSubCategory =
                    CurrentSubCategories.FirstOrDefault(
                        sc => sc.Id ==
                            transaction.SubCategory?.Id);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            AllCategories.Clear();
            AllSubCategories.Clear();
            CurrentSubCategories.Clear();

            IReadOnlyList<Category> categories = await accountDataService.LoadAllCategoriesAsync();
            foreach (Category category in categories)
            {
                CategoryViewModel categoryViewModel = new(category);

                AllCategories.Add(categoryViewModel);

                IReadOnlyList<SubCategory> subCategories = await accountDataService.LoadSubCategoriesForCategoryAsync(category.Id);

                foreach (SubCategory subCategory in subCategories)
                {
                    SubCategoryViewModel subCategoryViewModel = new(subCategory)
                    {
                        Category = categoryViewModel
                    };

                    AllSubCategories.Add(subCategoryViewModel);
                }
            }

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
                            sc.Category.Id ==
                            category.Id);

            foreach (SubCategoryViewModel subCategory in subCategories)
                CurrentSubCategories.Add(subCategory);
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
            if (Transaction is null || SelectedSubCategory is null)
                return;

            Transaction.SubCategory = SelectedSubCategory;

            Transaction.Model.SubCategoryId = SelectedSubCategory.Id;

            await accountDataService.SaveTransactionAsync(
                Transaction.Model);

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