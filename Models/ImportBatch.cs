using SQLite;

namespace trackr.Models
{
    [Table("ImportBatches")]
    public class ImportBatch
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public Guid BankAccountId { get; set; }

        public string? FileName { get; set; }

        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

        public int ImportedCount { get; set; }

        public int DuplicateCount { get; set; }

        public int PossibleDuplicateCount { get; set; }

        public int ErrorCount { get; set; }
    }
}