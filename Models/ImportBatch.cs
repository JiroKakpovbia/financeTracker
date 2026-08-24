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
    }
}