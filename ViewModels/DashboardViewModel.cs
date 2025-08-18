using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using trackr.Models;

namespace trackr.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
    private readonly AccountDataService accountDataService;
        private ObservableCollection<BankAccount> _bankAccounts = new();

        public ObservableCollection<BankAccount> BankAccounts
        {
            get => _bankAccounts;
            set { _bankAccounts = value; OnPropertyChanged(); }
        }

        public event Func<Task<BankAccount?>>? ShowAddAccountPopupRequested;
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

        public class AlertEventArgs : EventArgs
        {
            public string Title { get; }
            public string Message { get; }
            public AlertEventArgs(string title, string message)
            {
                Title = title;
                Message = message;
            }
        }

        public class PromptEventArgs : EventArgs
        {
            public string Title { get; }
            public string Message { get; }
            public string? InitialValue { get; }

            public PromptEventArgs(string title, string message, string? initialValue)
            {
                Title = title;
                Message = message;
                InitialValue = initialValue;
            }
        }

        public class ActionSheetEventArgs : EventArgs
        {
            public string Title { get; }
            public string Cancel { get; }
            public string? Destruction { get; }
            public string[] Options { get; }

            public ActionSheetEventArgs(string title, string cancel, string? destruction, params string[] options)
            {
                Title = title;
                Cancel = cancel;
                Destruction = destruction;
                Options = options;
            }
        }

        public ICommand LoadAccountsCommand { get; }
        public ICommand ShowMenuCommand { get; }
        public ICommand AddAccountCommand { get; }
        public ICommand ToggleTransactionsCommand { get; }
        public ICommand LogoTapCommand { get; }

        public DashboardViewModel(AccountDataService accountDataService)
        {
            this.accountDataService = accountDataService;
            LoadAccountsCommand = new AsyncRelayCommand(LoadAccounts);
            ShowMenuCommand = new AsyncRelayCommand<BankAccount?>(HandleShowMenu);
            AddAccountCommand = new AsyncRelayCommand(HandleAddAccount);
            ToggleTransactionsCommand = new AsyncRelayCommand<BankAccount?>(HandleToggleTransactions);
            LogoTapCommand = new AsyncRelayCommand<BankAccount?>(HandleLogoTap);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));

        private async Task LoadAccounts()
        {
            ObservableCollection<BankAccount> accounts = await accountDataService.LoadAccounts();
            BankAccounts = new ObservableCollection<BankAccount>(accounts);
        }

        private async Task HandleRenameAccount(BankAccount account)
        {
            if (account != null)
            {
                string? newName = await RequestPrompt("Rename Account", "Enter the new account name:", account.Name);

                if (!string.IsNullOrWhiteSpace(newName))
                {
                    if (!BankAccounts.Any(a => a.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && a.Bank.Equals(account.Bank, StringComparison.OrdinalIgnoreCase) && a.Type.Equals(account.Type, StringComparison.OrdinalIgnoreCase)))
                    {
                        account.Name = newName;
                        await RequestAlert("Success", $"Account renamed to '{newName}'.");
                    }
                    else await RequestAlert("Error", "An account with this name, bank, and type already exists.");
                }
            }
            else await RequestAlert("Error", "Account not found.");

            await accountDataService.SaveAccounts(BankAccounts);
        }

        private async Task HandleShowMenu(BankAccount? account)
        {
            if (account != null)
            {
                string? action = await RequestActionSheet("Options", "Cancel", null, new[] { "Rename Account", "Move Account Up", "Move Account Down", "Import CSV", "Delete Account" });

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
                }
            }
            else await RequestAlert("Error", "Account not found.");

            await accountDataService.SaveAccounts(BankAccounts);
        }

        private async Task HandleMoveAccount(BankAccount account, int direction)
        {
            if (account != null)
            {
                int currentIndex = BankAccounts.IndexOf(account);
                int newIndex = currentIndex + direction;

                if (newIndex >= 0 && newIndex < BankAccounts.Count) BankAccounts.Move(currentIndex, newIndex);
                else await RequestAlert("Error", "Cannot move account further than the top or bottom.");
            }
            else await RequestAlert("Error", "Account not found.");

            await accountDataService.SaveAccounts(BankAccounts);
        }

        private async Task HandleImportCSV(BankAccount account)
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

            if (result != null)
            {
                using Stream stream = await result.OpenReadAsync();
                using StreamReader reader = new StreamReader(stream);

                CSVImportService csvService = new CSVImportService();

                if (account != null)
                {
                    csvService.BankMappings.TryGetValue(account.Bank, out CSVColumnMap? config);
                    if (config != null)
                    {
                        account.Transactions = csvService.ParseTransactions(stream, config);
                        account.Balance = account.Transactions.FirstOrDefault()?.Balance ?? 0;
                    }
                    else await RequestAlert("Error", $"No CSV configuration found for {account.Bank}.");
                }
                else await RequestAlert("Error", "Account not found.");
            }
            else await RequestAlert("Error", "Could not import file.");

            await accountDataService.SaveAccounts(BankAccounts);
            await RequestAlert("Success", "Account information imported successfully.");
        }

        private async Task HandleAddAccount()
        {
            if (ShowAddAccountPopupRequested != null)
            {
                BankAccount? newAccount = await ShowAddAccountPopupRequested.Invoke();

                if (newAccount != null)
                {
                    if (!BankAccounts.Any(a => a.Name.Equals(newAccount.Name, StringComparison.OrdinalIgnoreCase) && a.Bank.Equals(newAccount.Bank, StringComparison.OrdinalIgnoreCase) && a.Type.Equals(newAccount.Type, StringComparison.OrdinalIgnoreCase)))
                    {
                        BankAccounts.Add(newAccount);
                    }
                    else await RequestAlert("Error", "An account with this name, bank, and type already exists.");
                }
            }
            await accountDataService.SaveAccounts(BankAccounts);
        }

        private async Task HandleDeleteAccount(BankAccount account)
        {
            if (account != null)
            {
                bool confirm = await RequestAlert("Confirm Deletion", $"Are you sure you want to delete the account '{account.Name}'?");

                if (confirm)
                {
                    BankAccounts.Remove(account);
                    await accountDataService.SaveAccounts(BankAccounts);
                    await RequestAlert("Success", "Account deleted successfully.");
                }
            }
            else await RequestAlert("Error", "Account not found.");
        }

        private async Task HandleToggleTransactions(BankAccount? account)
        {
            if (account != null)
            {
                if (account.Transactions.Count != 0) account.ShowTransactions = !account.ShowTransactions;
                else await RequestAlert("No Transactions", "This account has no transactions to show. Import a CSV to populate this account.");
            }
            else await RequestAlert("Error", "Account not found.");

            await accountDataService.SaveAccounts(BankAccounts);
        }

        private async Task HandleLogoTap(BankAccount? account)
        {
            try
            {
                if (account != null)
                {
                    Uri? appUri = null;
                    Uri? webUri = null;

                    if (account.Bank == "TD")
                    {
                        appUri = new Uri("td://");
                        webUri = new Uri("https://easyweb.td.com/ui/ew/fs?fsType=PFS");
                    }
                    else if (account.Bank == "CIBC")
                    {
                        appUri = new Uri("cibc://");
                        webUri = new Uri("https://www.cibconline.cibc.com/ebm-resources/public/banking/cibc/client/web/index.html#/accounts/credit-cards/2c01046615744246b6ecadead422be4ddefd7b72ac9a7f7912f70bb70ab89bbe");
                    }
                    else if (account.Bank == "Capital One")
                    {
                        appUri = new Uri("capitalone://");
                        webUri = new Uri("https://myaccounts.capitalone.com/accountSummary");
                    }
                    else if (account.Bank == "RBC")
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
                await RequestAlert("Error", $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
