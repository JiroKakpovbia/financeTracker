namespace trackr.ViewModels
{
    public class TransactionGroupViewModel(DateTime date, IEnumerable<TransactionViewModel> transactions) : List<TransactionViewModel>(transactions)
    {
        public DateTime Date { get; } = date;

        public string Header =>
            Date.Date == DateTime.Today ? "Today" :
            Date.Date == DateTime.Today.AddDays(-1) ? "Yesterday" :
            Date.ToString("dddd, MMMM d, yyyy");
    }
}