using Solutions.Models;

namespace Solutions.Services
{
    public interface IStatisticsService
    {
        Task<Dictionary<string, int>> GetSolutionsPerCategoryAsync();
        Task<int> GetTotalSolutionsCountAsync();
        Task<List<string>> GetPopularTagsAsync(int limit = 10);
        Task<Dictionary<DateTime, int>> GetSolutionsPerDayAsync(int days = 30);
    }

    public class StatisticsService : IStatisticsService
    {
        private readonly DatabaseService _databaseService;

        public StatisticsService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<Dictionary<string, int>> GetSolutionsPerCategoryAsync()
        {
            var categories = await _databaseService.GetCategoriesAsync();
            var result = new Dictionary<string, int>();

            foreach (var category in categories)
            {
                result[category.Name] = category.SolutionCount;
            }

            return result;
        }

        public async Task<int> GetTotalSolutionsCountAsync()
        {
            var solutions = await _databaseService.GetSolutionsAsync();
            return solutions.Count;
        }

        public async Task<List<string>> GetPopularTagsAsync(int limit = 10)
        {
            var solutions = await _databaseService.GetSolutionsAsync();
            return solutions
                .SelectMany(s => s.Tags)
                .GroupBy(t => t)
                .OrderByDescending(g => g.Count())
                .Take(limit)
                .Select(g => g.Key)
                .ToList();
        }

        public async Task<Dictionary<DateTime, int>> GetSolutionsPerDayAsync(int days = 30)
        {
            var solutions = await _databaseService.GetSolutionsAsync();
            var startDate = DateTime.Now.Date.AddDays(-days);
            
            return solutions
                .Where(s => s.CreatedDate >= startDate)
                .GroupBy(s => s.CreatedDate.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
