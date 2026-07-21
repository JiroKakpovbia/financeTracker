using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using trackr.Models;

namespace trackr.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly AccountDataService accountDataService;
        private ObservableCollection<BankAccount> _bankAccounts = [];

        public ObservableCollection<BankAccount> BankAccounts
        {
            get => _bankAccounts;
            set { _bankAccounts = value; OnPropertyChanged(); }
        }

        public event Func<Task<BankAccount>>? ShowAddAccountPopupRequested;
        public event Func<object?, AlertEventArgs, Task<bool>>? ShowAlertRequested;
        public event Func<object?, PromptEventArgs, Task<string?>>? ShowPromptRequested;
        public event Func<object?, ActionSheetEventArgs, Task<string?>>? ShowActionSheetRequested;

        public async Task<bool> RequestAlert(string title, string message)
        {
            if (ShowAlertRequested != null) return await ShowAlertRequested.Invoke(this, new AlertEventArgs(title, message));
            return false;
        }

        public async Task<string?> RequestPrompt(string title, string message, string? initialValue)
        {
            if (ShowPromptRequested != null) return await ShowPromptRequested.Invoke(this, new PromptEventArgs(title, message, initialValue));
            return null;
        }

        public async Task<string?> RequestActionSheet(string title, string cancel, string? destruction, params string[] buttons)
        {
            if (ShowActionSheetRequested != null) return await ShowActionSheetRequested.Invoke(this, new ActionSheetEventArgs(title, cancel, destruction, buttons));
            return null;
        }

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

        public class ActionSheetEventArgs(string title, string cancel, string? destruction, params string[] options) : EventArgs
        {
            public string Title { get; } = title;
            public string Cancel { get; } = cancel;
            public string? Destruction { get; } = destruction;
            public string[] Options { get; } = options;
        }

        public ICommand ShowMenuCommand { get; }
        public ICommand AddAccountCommand { get; }
        public ICommand ToggleTransactionsCommand { get; }
        public ICommand LogoTapCommand { get; }

        public DashboardViewModel(AccountDataService accountDataService)
        {
            this.accountDataService = accountDataService;
            ShowMenuCommand = new AsyncRelayCommand<BankAccount>(HandleShowMenu);
            AddAccountCommand = new AsyncRelayCommand(HandleAddAccount);
            ToggleTransactionsCommand = new AsyncRelayCommand<BankAccount>(HandleToggleTransactions);
            LogoTapCommand = new AsyncRelayCommand<BankAccount>(HandleLogoTap);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));

        public async Task LoadAccountsAsync()
        {
            try
            {
                BankAccounts = await accountDataService.LoadAccountsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading accounts: {ex.Message}\n");
                await RequestAlert("Error", "Failed to load accounts. Please try again.");
            }
        }

        private async Task HandleRenameAccount(BankAccount? account)
        {
            Console.WriteLine($"Handling rename account request for account: {account?.Name} (ID: {account?.Id})");
            try
            {
                if (account != null)
                {
                    string? newName = await RequestPrompt("Rename Account", "Enter the new account name:", account.Name);

                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        if (!BankAccounts.Any(a => a.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && a.BankInstitution.Equals(account.BankInstitution, StringComparison.OrdinalIgnoreCase) && a.Type.Equals(account.Type)))
                        {
                            account.Name = newName;
                            await accountDataService.SaveAccountAsync(account);
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

        private async Task HandleShowMenu(BankAccount? account)
        {
            try
            {
                Console.WriteLine($"Handling show menu request for account: {account?.Name} (ID: {account?.Id})");
                string? action = await RequestActionSheet("Options", "Cancel", null, ["Rename Account", "Move Account Up", "Move Account Down", "Import CSV", "Delete Account"]);
                if (account != null)
                {
                    if (action != null)
                    {
                        switch (action)
                        {
                            case "Rename Account":
                                await HandleRenameAccount(account);
                                break;
                            case "Move Account Up":
                                await HandleMoveAccount(account, -1);
                                break;
                            case "Move Account Down":
                                await HandleMoveAccount(account, 1);
                                break;
                            case "Import CSV":
                                await HandleImportCSV(account);
                                break;
                            case "Delete Account":
                                await HandleDeleteAccount(account);
                                break;
                        }
                        await LoadAccountsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling menu action: {ex.Message}\n");
                await RequestAlert("Error", "An unexpected error occurred while processing your request.");
            }
        }

        private async Task HandleMoveAccount(BankAccount account, int direction)
        {
            Console.WriteLine($"Handling move account request for account: {account.Name} (ID: {account.Id}), direction: {(direction < 0 ? "up" : "down")}");
            try
            {
                int currentIndex = BankAccounts.IndexOf(account);
                int newIndex = currentIndex + direction;

                if (newIndex >= 0 && newIndex < BankAccounts.Count)
                {
                    BankAccounts.Move(currentIndex, newIndex); // TODO: Update to use DisplayOrder property for sorting instead of relying on the ObservableCollection order
                    await accountDataService.SaveAccountAsync(account);
                }
                else await RequestAlert("Error", "Cannot move account further than the top or bottom.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving account: {ex.Message}\n");
                await RequestAlert("Error", "An unexpected error occurred while processing your request.");
            }
        }

        private async Task HandleImportCSV(BankAccount account)
        {
            Console.WriteLine($"Handling CSV import for account: {account.Name} (ID: {account.Id})");
            try
            {
                FileResult? result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a CSV file",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
                    { DevicePlatform.Android, new[] { "text/csv" } },
                    { DevicePlatform.WinUI, new[] { ".csv" } },
                    { DevicePlatform.macOS, new[] { "csv" } },
                })
                });

                Console.WriteLine($"CSV import result: {(result != null ? result.FileName : "No file selected")}");

                if (result != null)
                {
                    using Stream stream = await result.OpenReadAsync();

                    CSVImportService csvService = new();

                    csvService.BankMappings.TryGetValue(account.BankInstitution, out CSVColumnMap? config);
                    if (config != null)
                    {
                        account.Transactions = CSVImportService.ParseTransactions(stream, config, account.Id);
                        account.Balance = 0.00m; // TODO: determine if this is the correct way to calculate balance, or if we should be using a different method (e.g., fetching from bank API)
                        await accountDataService.SaveAccountAsync(account);
                        await RequestAlert("Success", "Account information imported successfully.");
                    }
                    else await RequestAlert("Error", $"No CSV configuration found for {account.BankInstitution}.");
                }
                else await RequestAlert("Error", "Could not import file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing CSV: {ex.Message}\n");
                await RequestAlert("Error", "An unexpected error occurred while importing the CSV file.");
            }
        }

        private async Task HandleAddAccount()
        {
            Console.WriteLine("Handling add account request...");

            try
            {
                if (ShowAddAccountPopupRequested != null)
                {
                    BankAccount? newAccount = await ShowAddAccountPopupRequested.Invoke();
                    if (newAccount != null)
                    {
                        if (!BankAccounts.Any(a => a.Name.Equals(newAccount.Name, StringComparison.OrdinalIgnoreCase) && a.BankInstitution.Equals(newAccount.BankInstitution, StringComparison.OrdinalIgnoreCase) && a.Type.Equals(newAccount.Type)))
                        {
                            await accountDataService.SaveAccountAsync(newAccount);
                            BankAccounts.Add(newAccount);
                            await RequestAlert("Success", "Account added successfully.");
                        }
                        else await RequestAlert("Error", "An account with this name, bank, and type already exists.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding account: {ex.Message}\n");
                await RequestAlert("Error", "An unexpected error occurred while adding the account.");
            }
        }

        private async Task HandleDeleteAccount(BankAccount account)
        {
            Console.WriteLine($"Handling delete account request for account: {account.Name} (ID: {account.Id})");
            try
            {
                bool confirm = await RequestAlert("Confirm Deletion", $"Are you sure you want to delete the account '{account.Name}'?");

                if (confirm)
                {
                    BankAccounts.Remove(account);
                    await accountDataService.DeleteAccountAsync(account);
                    await RequestAlert("Success", "Account deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting account: {ex.Message}\n");
                await RequestAlert("Error", "An unexpected error occurred while deleting the account.");
            }
        }

        private async Task HandleToggleTransactions(BankAccount? account)
        {
            Console.WriteLine($"Handling toggle transactions for account: {account?.Name} (ID: {account?.Id}). Selected state: {account?.ShowTransactions}");
            try
            {
                if (account != null)
                {
                    if (account.Transactions != null && account.Transactions.Count > 0) account.ShowTransactions = !account.ShowTransactions;
                    else await RequestAlert("No Transactions", "This account has no transactions to show. Import a CSV to populate this account.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling transactions: {ex.Message}\n");
                await RequestAlert("Error", "An unexpected error occurred while toggling transactions.");
            }
        }

        private async Task HandleLogoTap(BankAccount? account)
        {
            Console.WriteLine($"Handling logo tap for account: {account?.Name} (ID: {account?.Id})");
            try
            {
                Uri? appUri = null;
                Uri? webUri = null;
                if (account != null)
                {
                    if (account.BankInstitution == "TD")
                    {
                        appUri = new Uri("td://");
                        webUri = new Uri("https://easyweb.td.com/ui/ew/fs?fsType=PFS");
                    }
                    else if (account.BankInstitution == "CIBC")
                    {
                        appUri = new Uri("cibc://");
                        webUri = new Uri("https://www.cibconline.cibc.com/ebm-resources/public/banking/cibc/client/web/index.html#/accounts/credit-cards/2c01046615744246b6ecadead422be4ddefd7b72ac9a7f7912f70bb70ab89bbe");
                    }
                    else if (account.BankInstitution == "Capital One")
                    {
                        appUri = new Uri("capitalone://");
                        webUri = new Uri("https://myaccounts.capitalone.com/accountSummary");
                    }
                    else if (account.BankInstitution == "RBC")
                    {
                        appUri = new Uri("rbc://");
                        webUri = new Uri("https://www1.royalbank.com/sgw1/olb/index-en/#/summary");
                    }

                    if (appUri != null && webUri != null)
                    {
                        bool canOpen = await Launcher.Default.CanOpenAsync(appUri);

                        if (canOpen) await Launcher.Default.OpenAsync(appUri);
                        else await Launcher.Default.OpenAsync(webUri);
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
