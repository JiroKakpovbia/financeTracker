using trackr.Models;
using trackr.ViewModels;

namespace trackr.Factories
{
    public interface IBankAccountViewModelFactory
    {
        Task<BankAccountViewModel> CreateAsync(BankAccount account);
    }
}