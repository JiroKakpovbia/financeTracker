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

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private decimal? budgetLimit;

        [ObservableProperty]
        private string? colour;

        [ObservableProperty]
        private string? icon;
    }
}