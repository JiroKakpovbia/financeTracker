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

        public decimal? SubCategoryId { get; set; }

        [Indexed]
        public string? ImportFingerprint { get; set; }

        public DateTime? ImportedAt { get; set; }
    }
}