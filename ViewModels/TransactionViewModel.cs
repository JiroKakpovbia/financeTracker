using CommunityToolkit.Mvvm.ComponentModel;
using trackr.Models;

namespace trackr.ViewModels
{
    public partial class TransactionViewModel(Transaction model) : ObservableObject
    {
        public Transaction Model { get; } = model;

        public int Id => Model.Id;

        public string BankAccountId
        {
            get => Model.BankAccountId;
            set => SetProperty(Model.BankAccountId, value, Model, (m, v) => m.BankAccountId = v);
        }

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

        public decimal? SubCategoryId
        {
            get => Model.SubCategoryId;
            set => SetProperty(Model.SubCategoryId, value, Model, (m, v) => m.SubCategoryId = v);
        }

        [ObservableProperty]
        private decimal? accountBalance;
    }
}