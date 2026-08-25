using System.Collections.ObjectModel;
using trackr.Models;

namespace trackr.Services
{
    public interface ICSVImportService
    {
        Task<FileResult?> PickCSVFileAsync();

        Task<ObservableCollection<Transaction>> ParseTransactions(
            Stream csvStream,
            BankAccount account);
    }
}