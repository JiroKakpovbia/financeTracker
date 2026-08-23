using SQLite;

namespace trackr.Models
{
    [Table("ImportBatches")]
    public class ImportBatch
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public string BankAccountId { get; set; } = string.Empty;

        public string? FileName { get; set; }

        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

        public int ImportedCount { get; set; }

        public int DuplicateCount { get; set; }
    }
}