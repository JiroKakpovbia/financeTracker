using SQLite;
using System.ComponentModel;

namespace trackr.Models
{
    public enum AccountType
    {
        [Description("Chequing")]
        Chequing,
        [Description("Savings")]
        Savings,
        [Description("Credit Card")]
        CreditCard,
        [Description("Cash")]
        Cash,
        // [Description("Investment")]
        // Investment,
        // [Description("Loan")]
        // Loan,
        // [Description("Mortgage")]
        // Mortgage,
        // [Description("Line Of Credit")]
        // LineOfCredit,
        [Description("Other")]
        Other
    }

    public enum BankInstitution
    {
        [Description("Capital One")]
        CapitalOne,
        [Description("CIBC")]
        CIBC,
        [Description("RBC Royal Bank")]
        RBC,
        [Description("TD Canada Trust")]
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
        public BankInstitution Institution { get; set; }

        public AccountType Type { get; set; }

        public decimal ReconciledBalance { get; set; }

        public DateTime ReconciledThroughDate { get; set; }

        [Ignore]
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();

        [Ignore]
        public List<ImportBatch> ImportBatches { get; set; } = new List<ImportBatch>();
    }
}