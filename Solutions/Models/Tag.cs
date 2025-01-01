using SQLite;

namespace Solutions.Models
{
    [Table("tags")]
    public class Tag
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [NotNull]
        public string Name { get; set; } = string.Empty;
        
        [NotNull]
        public string SolutionId { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
