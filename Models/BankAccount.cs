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

    [AttributeUsage(AttributeTargets.Field)]
    public class BankInstitutionInfoAttribute(string shortName, string longName) : Attribute
    {
        public string ShortName { get; } = shortName;
        public string LongName { get; } = longName;
    }


    public enum BankInstitution
    {
        [BankInstitutionInfo("Capital One", "Capital One Canada")]
        CapitalOne,
        [BankInstitutionInfo("CIBC", "Canadian Imperial Bank of Commerce")]
        CIBC,
        [BankInstitutionInfo("RBC", "RBC Royal Bank")]
        RBC,
        [BankInstitutionInfo("TD", "TD Canada Trust")]
        TD
    }

    [Table("BankAccounts")]
    public class BankAccount
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Indexed]
        public string Name { get; set; } = string.Empty;

        [Indexed]
        public BankInstitution Institution { get; set; }

        public AccountType Type { get; set; }

        public decimal ReconciledBalance { get; set; }

        public DateTime ReconciledThroughDate { get; set; }
    }
}