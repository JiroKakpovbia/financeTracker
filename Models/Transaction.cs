using SQLite;

namespace trackr.Models
{
    [Table("Transactions")]
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public string BankAccountId { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        [Indexed]
        public string ImportFingerprint { get; set; } = string.Empty;

        public int ImportBatchId { get; set; }

        public int? SubCategoryId { get; set; }
    }
}