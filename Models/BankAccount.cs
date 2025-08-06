public class BankAccount
{
    public required string Name { get; set; }
    public required string Id { get; set; }
    public required string Bank { get; set; }
    public required string Type { get; set; }
    public List<Transaction> Transactions { get; set; } = new();
}
