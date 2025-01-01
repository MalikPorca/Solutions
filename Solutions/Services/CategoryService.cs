using SQLite;
using Solutions.Models;

namespace Solutions.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetCategoriesAsync();
        Task<Category?> GetCategoryAsync(string id);
        Task<bool> AddCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(string id);
        Task<int> UpdateSolutionCount(string categoryId);
    }

    public class CategoryService : ICategoryService
    {
        private readonly DatabaseService _databaseService;

        public CategoryService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _databaseService.GetCategoriesAsync();
        }

        public async Task<Category?> GetCategoryAsync(string id)
        {
            return await _databaseService.GetCategoryAsync(id);
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            try
            {
                var result = await _databaseService.SaveCategoryAsync(category);
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                var result = await _databaseService.SaveCategoryAsync(category);
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            try
            {
                var category = await GetCategoryAsync(id);
                if (category != null)
                {
                    var result = await _databaseService.DeleteCategoryAsync(category);
                    return result > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> UpdateSolutionCount(string categoryId)
        {
            return await _databaseService.UpdateCategorySolutionCount(categoryId);
        }
    }
}
