

using System.Collections.ObjectModel;
using trackr.Models;

namespace trackr.Import
{
    public class TransactionImportResult
    {
        public ObservableCollection<Transaction> Added { get; set; } = [];
        public ObservableCollection<Transaction> Duplicates { get; set; } = [];
        public ObservableCollection<Transaction> PossibleDuplicates { get; set; } = [];
        public ObservableCollection<string> Errors { get; set; } = [];
    }
}