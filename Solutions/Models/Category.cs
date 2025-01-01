using SQLite;

namespace Solutions.Models
{
    [Table("categories")]
    public class Category
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [NotNull]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public int SolutionCount { get; set; }
    }
}
