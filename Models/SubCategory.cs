using SQLite;

namespace trackr.Models
{
    [Table("SubCategories")]
    public class SubCategory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed("IX_SubCategory_Category_Name", 1, Unique = true)]
        public int CategoryId { get; set; }

        [Indexed("IX_SubCategory_Category_Name", 2, Unique = true)]
        public string Name { get; set; } = string.Empty;

        public decimal? BudgetLimit { get; set; }
    }
}