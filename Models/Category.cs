using SQLite;

namespace trackr.Models
{
    [Table("Categories")]
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public string Name { get; set; } = string.Empty;

        public string? Colour { get; set; }

        public string? Icon { get; set; }

        public decimal? BudgetLimit { get; set; }
    }
}