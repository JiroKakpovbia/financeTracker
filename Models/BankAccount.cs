using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace trackr.Models
{
    public enum AccountType
    {
        VISADebit,
        VISACredit,
        MasterCardDebit,
        MasterCardCredit,
        Savings,
        HISA,
        Other
    }

    public enum BankInstitution
    {
        CapitalOne,
        CIBC,
        RBC,
        TD
    }
    
    [Table("BankAccounts")]
    public class BankAccount
    {
        [PrimaryKey]
        public string Id { get; set; }

        [Indexed]
        public string Name { get; set; }

        [Indexed]
        public string BankInstitution { get; set; }

        public AccountType Type { get; set; }

        public decimal Balance { get; set; }

        [Ignore]
        public ObservableCollection<Transaction>? Transactions { get; set; }

        [Ignore]
        public bool ShowTransactions { get; set; }
    }
}