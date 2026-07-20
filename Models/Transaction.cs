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

        // public decimal Balance { get; set; } // Optional: If you want to store the balance after this transaction

        public DateTime Date { get; set; }

        public string? Category { get; set; }

        public string? Merchant { get; set; }

        public string? Notes { get; set; }
    }
}