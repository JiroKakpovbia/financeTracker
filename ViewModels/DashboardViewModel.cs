using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using financeTracker.Models;

namespace financeTracker.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly AccountDataService accountDataService = new();
        private ObservableCollection<BankAccount> _bankAccounts = new();

        public ObservableCollection<BankAccount> BankAccounts
        {
            get => _bankAccounts;
            set { _bankAccounts = value; OnPropertyChanged(); }
        }

        public event Func<Task<BankAccount?>>? ShowAddAccountPopupRequested;
        public event EventHandler<AlertEventArgs>? ShowAlertRequested;
        public event Func<object?, PromptEventArgs, Task<string?>>? ShowPromptRequested;
        public event Func<object?, ActionSheetEventArgs, Task<string?>>? ShowActionSheetRequested;


        public void RequestAlert(string title, string message)
        {
            ShowAlertRequested?.Invoke(this, new AlertEventArgs(title, message));
        }

        public async Task<string?> RequestPrompt(string title, string message, string? initialValue)
        {
            if (ShowPromptRequested != null)
                return await ShowPromptRequested.Invoke(this, new PromptEventArgs(title, message, initialValue));
            return null;
        }

        public async Task<string?> RequestActionSheet(string title, string cancel, string? destruction, params string[] buttons)
        {
            if (ShowActionSheetRequested != null)
                return await ShowActionSheetRequested.Invoke(this, new ActionSheetEventArgs(title, cancel, destruction, buttons));
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

        public DashboardViewModel()
        {
            LoadAccountsCommand = new AsyncRelayCommand(LoadAccounts);
            ShowMenuCommand = new AsyncRelayCommand<string>(HandleShowMenu);
            AddAccountCommand = new AsyncRelayCommand(HandleAddAccount);
            ToggleTransactionsCommand = new AsyncRelayCommand<string>(HandleToggleTransactions);
            LogoTapCommand = new AsyncRelayCommand<string>(HandleLogoTap);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));

        private async Task LoadAccounts()
        {
            var accounts = await accountDataService.LoadAccounts();
            BankAccounts = new ObservableCollection<BankAccount>(accounts);
        }

        private async Task HandleRenameAccount(string? accountId)
        {
            var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);

            if (account != null)
            {
                string? newName = await RequestPrompt("Rename Account", "Enter the new account name:", account.Name);

                if (!string.IsNullOrWhiteSpace(newName))
                {
                    if (!BankAccounts.Any(a => a.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && a.Bank.Equals(account.Bank, StringComparison.OrdinalIgnoreCase) && a.Type.Equals(account.Type, StringComparison.OrdinalIgnoreCase)))
                    {
                        account.Name = newName;
                        RequestAlert("Success", $"Account renamed to '{newName}'.");
                    }
                    else RequestAlert("Error", "An account with this name, bank, and type already exists.");
                }
            }
            else RequestAlert("Error", "Account not found.");

            await accountDataService.SaveAccounts(BankAccounts);
        }

        private async Task HandleShowMenu(string? accountId)
        {
            var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);

            if (account != null)
            {
                string? action = await RequestActionSheet("Options", "Cancel", null, new[] { "Rename Account", "Move Account Up", "Move Account Down", "Import CSV", "Delete Account" });

                if (action != null)
                {
                    switch (action)
                    {
                        case "Rename Account":
                            await HandleRenameAccount(accountId);
                            break;
                        case "Move Account Up":
                            await HandleMoveAccount(accountId, -1);
                            break;
                        case "Move Account Down":
                            await HandleMoveAccount(accountId, 1);
                            break;
                        case "Import CSV":
                            await HandleImportCSV(accountId);
                            break;
                        case "Delete Account":
                            await HandleDeleteAccount(accountId);
                            break;
                    }
                }
            }
            else RequestAlert("Error", "Account not found.");

            await accountDataService.SaveAccounts(BankAccounts);
        }

        private Task HandleMoveAccount(string? accountId, int direction)
        {
            var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);

            if (account != null)
            {
                int currentIndex = BankAccounts.IndexOf(account);
                int newIndex = currentIndex + direction;

                if (newIndex >= 0 && newIndex < BankAccounts.Count) BankAccounts.Move(currentIndex, newIndex);
                else RequestAlert("Error", "Cannot move account.");
            }
            else RequestAlert("Error", "Account not found.");

            return Task.CompletedTask;
        }

        private async Task HandleImportCSV(string? accountId)
        {
            var result = await FilePicker.PickAsync(new PickOptions
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
                using var stream = await result.OpenReadAsync();
                using var reader = new StreamReader(stream);

                var csvService = new CSVImportService();

                var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);

                if (account != null)
                {
                    csvService.BankMappings.TryGetValue(account.Bank, out var config);
                    if (config != null)
                    {
                        account.Transactions = csvService.ParseTransactions(stream, config);
                        account.Balance = account.Transactions.FirstOrDefault()?.Balance ?? 0;
                    }
                    else RequestAlert("Error", $"No CSV configuration found for {account.Bank}.");
                }
                else RequestAlert("Error", "Account not found.");
            }
            else RequestAlert("Error", "Could not import file.");

            await accountDataService.SaveAccounts(BankAccounts);
        }

        private async Task HandleAddAccount()
        {
            // string name = await DisplayPromptAsync("Add Account", "Enter the account name:", "OK", "Cancel");
            // if (string.IsNullOrWhiteSpace(name))
            // {
            //     return;
            // }

            // string bank = await DisplayPromptAsync("Bank", "Enter the bank name:", "OK", "Cancel");
            // if (string.IsNullOrWhiteSpace(bank))
            // {
            //     return;
            // }

            // string type = await DisplayPromptAsync("Type", "Enter the account type:", "OK", "Cancel");
            // if (string.IsNullOrWhiteSpace(type))
            // {
            //     return;
            // }

            // var newAccount = new BankAccount
            // {
            //     Name = name,
            //     Bank = bank,
            //     Type = type,
            //     Id = Guid.NewGuid().ToString(),
            //     Balance = 0.00m
            // };

            // BankAccounts.Add(newAccount);
            // await accountDataService.SaveAccounts(BankAccounts);

            if (ShowAddAccountPopupRequested != null)
            {
                var newAccount = await ShowAddAccountPopupRequested.Invoke();

                if (newAccount != null)
                {
                    if (!BankAccounts.Any(a => a.Name.Equals(newAccount.Name, StringComparison.OrdinalIgnoreCase) && a.Bank.Equals(newAccount.Bank, StringComparison.OrdinalIgnoreCase) && a.Type.Equals(newAccount.Type, StringComparison.OrdinalIgnoreCase)))
                    {
                        BankAccounts.Add(newAccount);
                    }
                    else RequestAlert("Error", "An account with this name, bank, and type already exists.");
                }
            }

            await accountDataService.SaveAccounts(BankAccounts);
        }

        private async Task HandleDeleteAccount(string? accountId)
        {
            var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);

            if (account != null)
            {
                var confirm = await RequestPrompt("Confirm Deletion", $"Are you sure you want to delete the account '{account.Name}'?", null);

                if (confirm != null)
                {
                    BankAccounts.Remove(account);
                    await accountDataService.SaveAccounts(BankAccounts);
                    RequestAlert("Success", "Account deleted successfully.");
                }
            }
            else RequestAlert("Error", "Account not found.");
        }

        private async Task HandleToggleTransactions(string? accountId)
        {
            var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);
            if (account != null)
            {
                if (account.Transactions.Count != 0) account.ShowTransactions = !account.ShowTransactions;
                else RequestAlert("No Transactions", "This account has no transactions to show. Import a CSV to populate this account.");
            }
            else RequestAlert("Error", "Account not found.");

            await accountDataService.SaveAccounts(BankAccounts);
        }

        private async Task HandleLogoTap(string? accountId)
        {
            var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);
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

                if (appUri != null && webUri != null)
                {
                    bool canOpen = await Launcher.Default.CanOpenAsync(appUri);

                    if (canOpen) await Launcher.Default.OpenAsync(appUri);
                    else await Launcher.Default.OpenAsync(webUri);
                }
                else
                {
                    RequestAlert("Error", "Failed to open bank's website or application.");
                }

            }
        }
    }
}
