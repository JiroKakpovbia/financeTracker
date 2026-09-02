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

                transactionViewModel.AccountName = account.Name;

                transactionViewModel.AccountInstitution = account.Institution;

                transactionViewModel.ImportedAt =
                    importBatches
                        .FirstOrDefault(
                            batch =>
                                batch.Id ==
                                transaction.ImportBatchId)?
                        .ImportedAt
                    ?? DateTime.MinValue;

                transactionViewModels.Add(transactionViewModel);
            }

            viewModel.AddTransactions(transactionViewModels);

            return viewModel;
        }
    }
}