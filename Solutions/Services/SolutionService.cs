using Solutions.Models;

namespace Solutions.Services
{
    public interface ISolutionService
    {
        Task<List<Solution>> GetSolutionsAsync();
        Task<Solution> GetSolutionByIdAsync(string id);
        Task<bool> AddSolutionAsync(Solution solution);
        Task<bool> UpdateSolutionAsync(Solution solution);
        Task<bool> DeleteSolutionAsync(string id);
        Task<List<Solution>> SearchSolutionsAsync(string searchTerm);
        Task<List<Solution>> FilterSolutionsAsync(string? category = null, List<string>? tags = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<Solution>> GetSolutionsByCategoryAsync(string category);
    }

    public class SolutionService : ISolutionService
    {
        private readonly DatabaseService _databaseService;

        public SolutionService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Solution>> GetSolutionsAsync()
        {
            return await _databaseService.GetSolutionsAsync();
        }

        public async Task<Solution> GetSolutionByIdAsync(string id)
        {
            return await _databaseService.GetSolutionAsync(id);
        }

        public async Task<bool> AddSolutionAsync(Solution solution)
        {
            try
            {
                solution.CreatedDate = DateTime.Now;
                var result = await _databaseService.SaveSolutionAsync(solution);
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSolutionAsync(Solution solution)
        {
            try
            {
                var result = await _databaseService.SaveSolutionAsync(solution);
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSolutionAsync(string id)
        {
            try
            {
                var solution = await GetSolutionByIdAsync(id);
                if (solution != null)
                {
                    var result = await _databaseService.DeleteSolutionAsync(solution);
                    return result > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Solution>> SearchSolutionsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetSolutionsAsync();

            return await _databaseService.SearchSolutionsAsync(searchTerm);
        }

        public async Task<List<Solution>> FilterSolutionsAsync(string? category = null, List<string>? tags = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var solutions = await GetSolutionsAsync();

            // Apply category filter
            if (!string.IsNullOrEmpty(category))
            {
                solutions = solutions.Where(s => s.Category == category).ToList();
            }

            // Apply tags filter
            if (tags != null && tags.Any())
            {
                solutions = solutions.Where(s => s.Tags.Any(t => tags.Contains(t))).ToList();
            }

            // Apply date range filter
            if (fromDate.HasValue)
            {
                solutions = solutions.Where(s => s.CreatedDate >= fromDate.Value).ToList();
            }

            if (toDate.HasValue)
            {
                solutions = solutions.Where(s => s.CreatedDate <= toDate.Value).ToList();
            }

            return solutions;
        }

        public async Task<List<Solution>> GetSolutionsByCategoryAsync(string category)
        {
            var solutions = await GetSolutionsAsync();
            return solutions.Where(s => s.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
        }
    }
}
