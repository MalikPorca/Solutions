using Solutions.Models;
using System.Security.Cryptography;
using System.Text;

namespace Solutions.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(string username, string email, string password, string firstName, string lastName);
        Task<User> LoginAsync(string email, string password);
        Task<User> GetCurrentUserAsync();
        Task LogoutAsync();
        Task<bool> UpdateProfileAsync(User user);
        Task<bool> ChangePasswordAsync(string oldPassword, string newPassword);
        bool IsAuthenticated { get; }
    }

    public class AuthService : IAuthService
    {
        private readonly DatabaseService _databaseService;
        private User _currentUser;

        public bool IsAuthenticated => _currentUser != null;

        public AuthService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<User> RegisterAsync(string username, string email, string password, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty");
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty");
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty");
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty");
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty");

            // Check if user already exists
            var existingUser = await _databaseService.GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                throw new Exception("User with this email already exists");
            }

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = username.Trim(),
                Email = email.Trim().ToLower(),
                PasswordHash = HashPassword(password),
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                JoinDate = DateTime.Now,
                Bio = string.Empty,
                SolutionsCount = 0,
                ProfileImage = "default_profile"
            };

            var result = await _databaseService.SaveUserAsync(user);
            if (result <= 0)
                throw new Exception("Failed to create user");

            _currentUser = user;
            await SecureStorage.Default.SetAsync("current_user_id", user.Id);

            return user;
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            var user = await _databaseService.GetUserByEmailAsync(email);
            if (user == null || user.PasswordHash != HashPassword(password))
            {
                throw new Exception("Invalid email or password");
            }

            _currentUser = user;
            await SecureStorage.Default.SetAsync("current_user_id", user.Id);

            return user;
        }

        public async Task<User> GetCurrentUserAsync()
        {
            if (_currentUser != null)
                return _currentUser;

            var userId = await SecureStorage.Default.GetAsync("current_user_id");
            if (string.IsNullOrEmpty(userId))
                return null;

            _currentUser = await _databaseService.GetUserAsync(userId);
            return _currentUser;
        }

        public async Task LogoutAsync()
        {
            _currentUser = null;
            await SecureStorage.Default.SetAsync("current_user_id", string.Empty);
        }

        public async Task<bool> UpdateProfileAsync(User user)
        {
            if (_currentUser?.Id != user.Id)
                throw new UnauthorizedAccessException("Cannot update another user's profile");

            var success = await _databaseService.SaveUserAsync(user);
            if (success > 0)
            {
                _currentUser = user;
                return true;
            }
            return false;
        }

        public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
        {
            if (_currentUser == null)
                throw new UnauthorizedAccessException("User not authenticated");

            var user = await _databaseService.GetUserAsync(_currentUser.Id);
            if (user.PasswordHash != HashPassword(oldPassword))
                throw new Exception("Invalid current password");

            user.PasswordHash = HashPassword(newPassword);
            var success = await _databaseService.SaveUserAsync(user);
            return success > 0;
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty");

            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
