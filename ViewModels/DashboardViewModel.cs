using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using trackr.Models;
using trackr.Services;
using trackr.Views;

namespace trackr.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly AccountDataService accountDataService;

        [ObservableProperty]
        private ObservableCollection<BankAccountViewModel> bankAccounts = [];

        [ObservableProperty]
        private BankAccountViewModel? optionsShownForAccount;

        // Events for UI interactions
        public event Func<Task>? ShowAddAccountFormRequested;
        public event Func<BankAccountViewModel, Task>? ShowAccountOptionsFormRequested;
        public event Func<Task>? HideFormRequested;
        public event Func<object?, AlertEventArgs, Task<bool>>? ShowAlertRequested;
        public event Func<object?, PromptEventArgs, Task<string?>>? ShowPromptRequested;

        // Request methods for UI interactions
        private async Task<bool> RequestAlert(string title, string message)
        {
            if (ShowAlertRequested != null)
                return await ShowAlertRequested.Invoke(this, new AlertEventArgs(title, message));
            return false;
        }

        private async Task<string?> RequestPrompt(string title, string message, string? initialValue)
        {
            if (ShowPromptRequested != null)
                return await ShowPromptRequested.Invoke(this, new PromptEventArgs(title, message, initialValue));
            return null;
        }

        private async Task RequestShowAddAccountForm()
        {
            if (ShowAddAccountFormRequested != null)
                await ShowAddAccountFormRequested.Invoke();
        }

        private async Task RequestShowAccountOptionsForm(BankAccountViewModel? account)
        {
            if (account != null)
            {
                if (ShowAccountOptionsFormRequested != null)
                {
                    OptionsShownForAccount = account;
                    await ShowAccountOptionsFormRequested.Invoke(account);
                }
            }
        }

        private async Task RequestHideForm()
        {
            if (HideFormRequested != null)
            {
                await HideFormRequested.Invoke();
                OptionsShownForAccount = null; // Reset the OptionsShownForAccount after the form is closed
            }
        }

        // Event argument classes for alerts and prompts
        public class AlertEventArgs(string title, string message) : EventArgs
        {
            public string Title { get; } = title;
            public string Message { get; } = message;
        }

        public class PromptEventArgs(string title, string message, string? initialValue) : EventArgs
        {
            public string Title { get; } = title;
            public string Message { get; } = message;
            public string? InitialValue { get; } = initialValue;
        }

        // Commands for Dashboard actions
        [RelayCommand]
        private Task ShowAccountOptionsAsync(BankAccountViewModel? account) => RequestShowAccountOptionsForm(account);

        [RelayCommand]
        private Task ShowAddAccountFormAsync() => RequestShowAddAccountForm();

        [RelayCommand]
        private Task HideFormAsync() => RequestHideForm();

        [RelayCommand]
        private Task ToggleTransactionsAsync(BankAccountViewModel? account) => HandleToggleTransactions(account);

        [RelayCommand]
        private Task LogoTapAsync(BankAccountViewModel? account) => HandleLogoTap(account);

        // Commands for Account Options
        [RelayCommand]
        private Task RenameAccountAsync(BankAccountViewModel? account) => HandleRenameAccount(account);

        [RelayCommand]
        private Task ImportCSVAsync(BankAccountViewModel? account) => HandleImportCSV(account);

        [RelayCommand]
        private Task MoveAccountAsync(BankAccountViewModel? account) => HandleMoveAccount(account);

        [RelayCommand]
        private Task DeleteAccountAsync(BankAccountViewModel? account) => HandleDeleteAccount(account);

        [RelayCommand]
        private Task AddAccountAsync(AddAccountView? view) => HandleAddAccount(view);

        // Constructor for DashboardViewModel
        public DashboardViewModel(AccountDataService accountDataService)
        {
            this.accountDataService = accountDataService;
        }

        // Load accounts from the database and populate the BankAccounts collection
        public async Task LoadAccountsAsync()
        {
            try
            {
                ObservableCollection<BankAccount> accounts = await accountDataService.LoadAccountsAsync();

                BankAccounts = new ObservableCollection<BankAccountViewModel>(
                    accounts.Select(a => new BankAccountViewModel(a))
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading accounts: {ex.Message}\n");
                await RequestAlert("Error", "Failed to load accounts. Please try again.");
            }
        }

        // Handle the renaming of an account
        private async Task HandleRenameAccount(BankAccountViewModel? account)
        {
            Console.WriteLine($"Handling rename account request for account: {account?.Name} (ID: {account?.Id})");

            try
            {
                await RequestHideForm();

                if (account != null)
                {
                    string? newName = await RequestPrompt("Rename Account", "Enter the new account name:", account.Name);

                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        if (!BankAccounts.Any(a => a.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && a.BankInstitution.Equals(account.BankInstitution) && a.Type.Equals(account.Type)))
                        {
                            account.Name = newName;
                            await accountDataService.SaveAccountAsync(account.Model);

                            await RequestAlert("Success", $"Account renamed to '{newName}'.");
                        }
                        else await RequestAlert("Error", "An account with this name, bank, and type already exists.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error renaming account: {ex.Message}\n");
                await RequestAlert("Error", "Failed to rename account. Please try again.");
            }
        }

        // Handle the import of transactions from a CSV file for a specific account
        private async Task HandleImportCSV(BankAccountViewModel? account)
        {
            Console.WriteLine($"Handling CSV import for account: {account?.Name} (ID: {account?.Id})");

            try
            {
                await RequestHideForm();

                if (account != null)
                {
                    var options = new PickOptions
                    {
                        PickerTitle = "Select a CSV file",
                        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                            { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                            { DevicePlatform.Android, new[] { "text/csv" } },
                            { DevicePlatform.WinUI, new[] { ".csv" } },
                            { DevicePlatform.macOS, new[] { "public.comma-separated-values-text" } },
                        })
                    };

                    FileResult? file = await FilePicker.PickAsync(options);

                    Console.WriteLine($"Selected file: {file?.FileName}");

                    if (file != null)
                    {
                        using Stream stream = await file.OpenReadAsync();

                        CSVImportService csvService = new();

                        ObservableCollection<Transaction> transactions = csvService.ImportTransactions(stream, account.Model);

                        if (transactions.Count != 0)
                        {
                            account.UpdateTransactions(transactions);
                            account.Balance = transactions.First().AccountBalance ?? 0m;
                        }

                        await accountDataService.SaveAccountAsync(account.Model);

                        await RequestAlert("Success", "Account information imported successfully.");
                    }
                    else await RequestAlert("Error", "Could not import file.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing CSV: {ex.Message}\n");
                await RequestAlert("Error", "An unexpected error occurred while importing the CSV file.");
            }
        }

        // Handle moving an account up or down in the list
        private async Task HandleMoveAccount(BankAccountViewModel? account)
        {
            Console.WriteLine($"Handling move account request for account: {account?.Name} (ID: {account?.Id})");

            // TODO: Implement move account functionality using DisplayOrder property for sorting instead of relying on the ObservableCollection order
            // try
            // {
            //     int currentIndex = BankAccounts.IndexOf(account);
            //     int newIndex = currentIndex + direction;

            //     if (newIndex >= 0 && newIndex < BankAccounts.Count)
            //     {
            //         BankAccounts.Move(currentIndex, newIndex);
            //         await accountDataService.SaveAccountAsync(account);
            //     }
            //     else await RequestAlert("Error", "Cannot move account further than the top or bottom.");
            // }
            // catch (Exception ex)
            // {
            //     Console.WriteLine($"Error moving account: {ex.Message}\n");
            //     await RequestAlert("Error", "An unexpected error occurred while processing your request.");
            // }

            await RequestHideForm();
        }

        // Handle the deletion of an account
        private async Task HandleDeleteAccount(BankAccountViewModel? account)
        {
            Console.WriteLine($"Handling delete account request for account: {account?.Name} (ID: {account?.Id})");

            try
            {
                await RequestHideForm();

                if (account != null)
                {
                    bool confirm = await RequestAlert("Confirm Deletion", $"Are you sure you want to delete the account '{account.Name}'?");

                    if (confirm)
                    {
                        BankAccounts.Remove(account);
                        await accountDataService.DeleteAccountAsync(account.Model);

                        await RequestAlert("Success", "Account deleted successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting account: {ex.Message}\n");
                await RequestAlert("Error", "An unexpected error occurred while deleting the account.");
            }
        }

        // Handle the addition of a new account
        private async Task HandleAddAccount(AddAccountView? view)
        {
            Console.WriteLine("Add Account confirmation button clicked.");

            try
            {
                // Console.WriteLine($"Account Name: {view.AccountName}");
                // Console.WriteLine($"Selected Bank: {view.SelectedBank}");
                // Console.WriteLine($"Selected Type: {view.SelectedType}");
                // Console.WriteLine($"Balance: {view.Balance}");

                if (view != null)
                {

                    // Validate input fields
                    if (!string.IsNullOrWhiteSpace(view.AccountName) && view.SelectedBank != null && view.SelectedType != null)
                    {
                        // Check for duplicate account based on name, bank, and type
                        if (!BankAccounts.Any(a => a.Name.Equals(view.AccountName.Trim(), StringComparison.OrdinalIgnoreCase) && a.BankInstitution.Equals(view.SelectedBank) && a.Type.Equals(view.SelectedType)))
                        {
                            BankAccountViewModel account = new BankAccountViewModel(new BankAccount
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = view.AccountName.Trim(),
                                BankInstitution = (BankInstitution)view.SelectedBank,
                                Type = (AccountType)view.SelectedType,
                                Balance = view.Balance
                            });

                            await accountDataService.SaveAccountAsync(account.Model);
                            BankAccounts.Add(account);

                            await RequestHideForm();
                            await RequestAlert("Success", "Account added successfully.");
                        }
                        else
                        {
                            await RequestAlert(
                                "Error",
                                "An account with this name, bank, and type already exists. Please choose a different name or account type.");
                            return;
                        }
                    }
                    else
                    {
                        await RequestAlert(
                            "Error",
                            "All fields are required. Please fill in all details.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding account: {ex.Message}\n");
                await RequestAlert("Error", "An unexpected error occurred while adding the account.");
            }
        }

        // Handle toggling the visibility of transactions for a specific account
        private async Task HandleToggleTransactions(BankAccountViewModel? account)
        {
            // Console.WriteLine($"Handling toggle transactions for account: {account?.Name} (ID: {account?.Id}). Selected state: {account?.ShowTransactions}");

            try
            {
                if (account != null)
                {
                    if (account.TransactionGroups != null && account.TransactionGroups.Count > 0)
                        account.ShowTransactions = !account.ShowTransactions;
                    else
                        await RequestAlert("No Transactions", "This account has no transactions to show. Import a CSV to populate this account.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling transactions: {ex.Message}\n");
                await RequestAlert("Error", "An unexpected error occurred while toggling transactions.");
            }
        }

        // Handle the tap on the bank logo to open the bank's app or website
        private async Task HandleLogoTap(BankAccountViewModel? account)
        {
            Console.WriteLine($"Handling logo tap for account: {account?.Name} (ID: {account?.Id})");
            try
            {
                Uri? appUri = null;
                Uri? webUri = null;
                if (account != null)
                {
                    if (account.BankInstitution == BankInstitution.TD)
                    {
                        appUri = new Uri("td://");
                        webUri = new Uri("https://easyweb.td.com/ui/ew/fs?fsType=PFS");
                    }
                    else if (account.BankInstitution == BankInstitution.CIBC)
                    {
                        appUri = new Uri("cibc://");
                        webUri = new Uri("https://www.cibconline.cibc.com/ebm-resources/public/banking/cibc/client/web/index.html#/accounts/credit-cards/2c01046615744246b6ecadead422be4ddefd7b72ac9a7f7912f70bb70ab89bbe");
                    }
                    else if (account.BankInstitution == BankInstitution.CapitalOne)
                    {
                        appUri = new Uri("capitalone://");
                        webUri = new Uri("https://myaccounts.capitalone.com/accountSummary");
                    }
                    else if (account.BankInstitution == BankInstitution.RBC)
                    {
                        appUri = new Uri("rbc://");
                        webUri = new Uri("https://www1.royalbank.com/sgw1/olb/index-en/#/summary");
                    }

                    if (appUri != null && webUri != null)
                    {
                        bool canOpen = await Launcher.Default.CanOpenAsync(appUri);

                        if (canOpen)
                            await Launcher.Default.OpenAsync(appUri);
                        else
                            await
                            Launcher.Default.OpenAsync(webUri);
                    }
                    else
                    {
                        await RequestAlert("Error", "Failed to open bank's website or application.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening bank's website or application: {ex.Message}\n");
                await RequestAlert("Error", $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
