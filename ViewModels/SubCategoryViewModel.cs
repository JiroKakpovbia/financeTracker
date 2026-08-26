using CommunityToolkit.Mvvm.ComponentModel;
using trackr.Models;

namespace trackr.ViewModels
{
    public partial class SubCategoryViewModel(SubCategory model) : ObservableObject
    {
        public SubCategory Model { get; } = model;

        public int Id => Model.Id;

        public int CategoryId
        {
            get => Model.CategoryId;
            set => SetProperty(Model.CategoryId, value, Model, (m, v) => m.CategoryId = v);
        }

        public string Name
        {
            get => Model.Name;
            set => SetProperty(Model.Name, value, Model, (m, v) => m.Name = v);
        }

        public decimal? BudgetLimit
        {
            get => Model.BudgetLimit;
            set => SetProperty(Model.BudgetLimit, value, Model, (m, v) => m.BudgetLimit = v);
        }

        [ObservableProperty]
        private CategoryViewModel? category;
    }
}