using trackr.Models;
using trackr.Services;
using trackr.ViewModels;

namespace trackr.Factories
{
    public class BankAccountViewModelFactory(
        IAccountDataService accountDataService,
        ITransactionViewModelFactory transactionViewModelFactory)
        : IBankAccountViewModelFactory
    {
        public async Task<BankAccountViewModel> CreateAsync(
            BankAccount account)
        {
            BankAccountViewModel viewModel = new(account);

            IReadOnlyList<ImportBatch> importBatches =
                await accountDataService.GetImportBatchesForAccountAsync(account.Id);

            foreach (ImportBatch importBatch in importBatches)
                viewModel.AddImportBatch(new ImportBatchViewModel(importBatch));

            IReadOnlyList<Transaction> transactions =
                await accountDataService.GetTransactionsForAccountAsync(account.Id);

            List<TransactionViewModel> transactionViewModels = [];

            foreach (Transaction transaction in transactions)
            {
                TransactionViewModel transactionViewModel =
                    await transactionViewModelFactory.CreateAsync(transaction);

                transactionViewModels.Add(transactionViewModel);
            }

            viewModel.AddTransactions(transactionViewModels);

            return viewModel;
        }
    }
}