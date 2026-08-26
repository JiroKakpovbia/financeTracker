using AuthenticationServices;
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

        public int ImportedCount
        {
            get => Model.ImportedCount;
            set => SetProperty(Model.ImportedCount, value, Model, (m, v) => m.ImportedCount = v);
        }

        public int DuplicateCount
        {
            get => Model.DuplicateCount;
            set => SetProperty(Model.DuplicateCount, value, Model, (m, v) => m.DuplicateCount = v);
        }

        public int PossibleDuplicateCount
        {
            get => Model.PossibleDuplicateCount;
            set => SetProperty(Model.PossibleDuplicateCount, value, Model, (m, v) => m.PossibleDuplicateCount = v);
        }

        public int ErrorCount
        {
            get => Model.ErrorCount;
            set => SetProperty(Model.ErrorCount, value, Model, (m, v) => m.ErrorCount = v);
        }
    }
}