using CommunityToolkit.Mvvm.ComponentModel;
using trackr.Models;

namespace trackr.ViewModels
{
    public partial class ImportBatchViewModel(ImportBatch model) : ObservableObject
    {
        public ImportBatch Model { get; } = model;

        public int Id => Model.Id;

        public Guid BankAccountId
        {
            get => Model.BankAccountId;
            set => SetProperty(Model.BankAccountId, value, Model, (m, v) => m.BankAccountId = v);
        }

        public string? FileName
        {
            get => Model.FileName;
            set => SetProperty(Model.FileName, value, Model, (m, v) => m.FileName = v);
        }

        public DateTime ImportedAt
        {
            get => Model.ImportedAt;
            set => SetProperty(Model.ImportedAt, value, Model, (m, v) => m.ImportedAt = v);
        }

        [ObservableProperty]
        private int importedCount;

        [ObservableProperty]
        private int duplicateCount;
    }
}