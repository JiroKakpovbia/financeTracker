namespace trackr.ViewModels
{
    public class TransactionGroupViewModel(DateTime date, IEnumerable<TransactionViewModel> transactions) : List<TransactionViewModel>
    {
        public DateTime Date { get; } = date;

        public string Header =>
            Date.Date == DateTime.Today ? "Today" :
            Date.Date == DateTime.Today.AddDays(-1) ? "Yesterday" :
            Date.ToString("dddd, MMMM d, yyyy");
    
        public List<TransactionViewModel> Transactions { get; } = [.. transactions];
    }
}