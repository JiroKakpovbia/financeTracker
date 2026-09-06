using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using trackr.Factories;
using trackr.Messages;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class BudgetPageViewModel : ObservableObject
    {
        private readonly IAccountDataService accountDataService;

        public ObservableCollection<CategoryViewModel> Categories { get; set; } = [];

        // Load categories from the database and populate the Categories list
        public async Task LoadCategoriesAsync()
        {
            try
            {
                // Clear the existing Categories list to avoid duplicates when reloading
                Categories.Clear();

                IReadOnlyList<Category> categories = await accountDataService.GetAllCategoriesAsync();

                // Create CategoryViewModel instances for each category and add them to the Categories list
                foreach (Category category in categories.OrderBy(c => c.Name))
                {
                    CategoryViewModel categoryViewModel = new(category);

                    Categories.Add(categoryViewModel);
                }

                // Add a default "Uncategorized" category to the list of categories
                Categories.Add(new CategoryViewModel(new Category
                {
                    Name = "Uncategorized"
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading categories: {ex.Message}\n");
            }
        }

        // Constructor for BudgetPageViewModel
        public BudgetPageViewModel(IAccountDataService accountDataService)
        {
            this.accountDataService = accountDataService;
        }
    }
}
