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
    }
}
