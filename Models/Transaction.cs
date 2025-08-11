namespace financeTracker.Models
{
    public class Transaction
    {
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
    }
}