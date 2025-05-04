public class BankAccount
{
    public string? Name { get; set; }
    public string? Bank { get; set; }
    public string? Type { get; set; }
    public List<Transaction> Transactions { get; set; } = new();
}
