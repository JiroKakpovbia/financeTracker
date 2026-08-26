
using trackr.Models;

namespace trackr.Import
{
    public class TransactionImportResult
    {
        public IReadOnlyList<Transaction> Added { get; set; } = [];
        public IReadOnlyList<Transaction> Duplicates { get; set; } = [];
        public IReadOnlyList<Transaction> PossibleDuplicates { get; set; } = [];
        public IReadOnlyList<string> Errors { get; set; } = [];
    }
}