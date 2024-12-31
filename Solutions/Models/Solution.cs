using System;
using SQLite;

namespace Solutions.Models
{
    [Table("solutions")]
    public class Solution
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [NotNull]
        public string Title { get; set; } = string.Empty;
        
        [NotNull]
        public string Description { get; set; } = string.Empty;
        
        public string Category { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int Likes { get; set; }
        
        [Ignore]
        public List<string> Tags { get; set; } = new List<string>();
    }
}
