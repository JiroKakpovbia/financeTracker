using CommunityToolkit.Mvvm.ComponentModel;
using trackr.Models;

namespace trackr.ViewModels
{
    public partial class TransactionViewModel(Transaction model) : ObservableObject
    {
        public Transaction Model { get; } = model;

        public int Id => Model.Id;

        public string? Description
        {
            get => Model.Description;
            set => SetProperty(Model.Description, value, Model, (m, v) => m.Description = v);
        }

        public decimal Amount
        {
            get => Model.Amount;
            set => SetProperty(Model.Amount, value, Model, (m, v) => m.Amount = v);
        }

        public DateTime Date
        {
            get => Model.Date;
            set => SetProperty(Model.Date, value, Model, (m, v) => m.Date = v);
        }
        public string AccountName { get; set; } = string.Empty;

        public BankInstitution AccountInstitution { get; set; }

        public DateTime ImportedAt { get; set; }

        [ObservableProperty]
        private SubCategoryViewModel subCategory = new(new SubCategory())
        {
            Name = "Uncategorized",
            Category = new CategoryViewModel(new Category
            {
                Name = "Uncategorized",
            })
        };
    }
}