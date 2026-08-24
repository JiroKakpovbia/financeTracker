using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CommunityToolkit.Maui.Core.Extensions;
using trackr.Models;

namespace trackr.Services
{
    public class TransactionImportService
    {
        public class ImportResult
        {
            public ObservableCollection<Transaction> Added { get; set; } = [];
            public ObservableCollection<Transaction> Duplicates { get; set; } = [];
            public ObservableCollection<Transaction> PossibleDuplicates { get; set; } = [];
            public ObservableCollection<string> Errors { get; set; } = [];
        }

        // Import transactions from a CSV stream for a specific bank account
        public static ImportResult ImportTransactions(IEnumerable<Transaction> importedTransactions, IEnumerable<Transaction> existingTransactions, BankAccount account)
        {
            ArgumentNullException.ThrowIfNull(importedTransactions);
            ArgumentNullException.ThrowIfNull(existingTransactions);
            ArgumentNullException.ThrowIfNull(account);

            ImportResult result = new();

            // A fingerprint is not guaranteed to be globally unique. The same merchant,
            // amount and date can legitimately occur more than once, so duplicates are
            // matched by occurrence count rather than by a simple HashSet.
            Dictionary<string, int> remainingExistingCounts = existingTransactions
                .Select(EnsureFingerprint)
                .GroupBy(t => t.ImportFingerprint!)
                .ToDictionary(g => g.Key, g => g.Count());

            Dictionary<string, int> seenInCurrentImport = new();

            // Process each imported transaction and categorize it as added, duplicate, or possible duplicate
            foreach (Transaction transaction in importedTransactions)
            {
                try
                {
                    transaction.BankAccountId = account.Id;
                    EnsureFingerprint(transaction);

                    string fingerprint = transaction.ImportFingerprint!;

                    if (remainingExistingCounts.TryGetValue(fingerprint, out int remaining) && remaining > 0)
                    {
                        result.Duplicates.Add(transaction);
                        remainingExistingCounts[fingerprint] = remaining - 1;
                        continue;
                    }

                    // We cannot know whether two identical rows in the same CSV are
                    // accidental duplicates or two legitimate same-day transactions.
                    // Do not silently discard them; import them and flag them for review.
                    if (seenInCurrentImport.TryGetValue(fingerprint, out int seen) && seen > 0)
                        result.PossibleDuplicates.Add(transaction);

                    seenInCurrentImport[fingerprint] = seen + 1;
                    result.Added.Add(transaction);
                }
                catch (Exception ex)
                {
                    result.Errors.Add(
                        $"Could not import transaction '{transaction.Description}' " +
                        $"on {transaction.Date:d}: {ex.Message}");
                }
            }

            result.Added = result.Added.OrderByDescending(t => t.Date).ToObservableCollection();
            result.Duplicates = result.Duplicates.OrderByDescending(t => t.Date).ToObservableCollection();
            result.PossibleDuplicates = result.PossibleDuplicates.OrderByDescending(t => t.Date).ToObservableCollection();
            result.Errors = result.Errors.ToObservableCollection();

            return result;
        }

        // generate a unique fingerprint for a transaction based on its key attributes.
        private static Transaction EnsureFingerprint(Transaction transaction)
        {
            if (!string.IsNullOrWhiteSpace(transaction.ImportFingerprint))
                return transaction;

            string normalizedDescription = NormalizeDescription(transaction.Description);

            string fingerprintSource = string.Join('|',
                transaction.BankAccountId,
                transaction.Date.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                transaction.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                normalizedDescription);

            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(fingerprintSource));
            transaction.ImportFingerprint = Convert.ToHexString(hash);

            return transaction;
        }

        // normalize the description by trimming whitespace, collapsing multiple spaces, and converting to uppercase for consistent fingerprinting.
        private static string NormalizeDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return string.Empty;

            return string.Join(' ',
                    description.Split(
                        (char[]?)null,
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .ToUpperInvariant();
        }
    }
}