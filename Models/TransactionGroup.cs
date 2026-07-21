namespace trackr.Models
{
    public class TransactionGroup(DateTime date, IEnumerable<Transaction> transactions) : List<Transaction>(transactions)
    {
        public DateTime Date { get; } = date;

        public string Header =>
            Date.Date == DateTime.Today ? "Today" :
            Date.Date == DateTime.Today.AddDays(-1) ? "Yesterday" :
            Date.ToString("dddd, MMMM d, yyyy");
    }
}