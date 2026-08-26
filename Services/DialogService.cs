namespace trackr.Services
{
    public class DialogService : IDialogService
    {
        private Page? CurrentPage =>
            Application.Current?.Windows.FirstOrDefault()?.Page;

        public async Task ShowAlertAsync(
            string title,
            string message)
        {
            if (CurrentPage is null)
                return;

            await CurrentPage.DisplayAlertAsync(
                title,
                message,
                "OK");
        }

        public async Task<string?> ShowPromptAsync(
            string title,
            string message,
            string? initialValue = null)
        {
            if (CurrentPage is null)
                return null;

            return await CurrentPage.DisplayPromptAsync(
                title,
                message,
                "OK",
                "Cancel",
                initialValue: initialValue);
        }

        public async Task<bool> ShowConfirmationAsync(
            string title,
            string message)
        {
            if (CurrentPage is null)
                return false;

            return await CurrentPage.DisplayAlertAsync(
                title,
                message,
                "Yes",
                "Cancel");
        }
    }
}