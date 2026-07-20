using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace trackr.Models
{
    public enum AccountType
    {
        [Description("Visa Debit")]
        VISADebit,
        [Description("Visa Credit")]
        VISACredit,
        [Description("MasterCard Debit")]
        MasterCardDebit,
        [Description("MasterCard Credit")]
        MasterCardCredit,
        [Description("Savings")]
        Savings,
        [Description("High Interest Savings Account")]
        HISA,
        [Description("Other")]
        Other
    }

    public enum BankInstitution
    {
        [Description("Capital One")]
        CapitalOne,
        [Description("CIBC")]
        CIBC,
        [Description("RBC")]
        RBC,
        [Description("TD")]
        TD
    }
    
    [Table("BankAccounts")]
    public class BankAccount
    {
        [PrimaryKey]
        public string Id { get; set; } = string.Empty;

        [Indexed]
        public string Name { get; set; } = string.Empty;

        [Indexed]
        public string BankInstitution { get; set; } = string.Empty;

        public AccountType Type { get; set; }

        public decimal Balance { get; set; }

        [Ignore]
        public ObservableCollection<Transaction>? Transactions { get; set; }

        [Ignore]
        public bool ShowTransactions { get; set; }
    }
}