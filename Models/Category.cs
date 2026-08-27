using SQLite;

namespace trackr.Models
{
    [Table("Categories")]
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed(Unique = true)]
        public string Name { get; set; } = string.Empty;

        public string? Colour { get; set; }
    }
}