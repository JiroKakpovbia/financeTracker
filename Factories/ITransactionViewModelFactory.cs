using trackr.Models;
using trackr.ViewModels;

namespace trackr.Factories
{
    public interface ITransactionViewModelFactory
    {
        Task<TransactionViewModel> CreateAsync(Transaction transaction);
    }
}