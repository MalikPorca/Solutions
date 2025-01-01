using SQLite;

namespace Solutions.Models
{
    public class User
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public int SolutionsCount { get; set; }
        public string ProfileImage { get; set; } = "default_profile";
    }
}
