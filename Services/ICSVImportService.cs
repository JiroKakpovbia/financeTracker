using trackr.Models;

namespace trackr.Services
{
    public interface ICSVImportService
    {
        Task<FileResult?> PickCSVFileAsync();

        Task<IReadOnlyList<Transaction>> ParseTransactions(
            Stream csvStream,
            BankAccount account);
    }
}