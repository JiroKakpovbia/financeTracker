using SQLite;

namespace trackr.Models
{
    [Table("SubCategories")]
    public class SubCategory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int CategoryId { get; set; }

        [Indexed]
        public string Name { get; set; } = string.Empty;

        public decimal? BudgetLimit { get; set; }
    }
}