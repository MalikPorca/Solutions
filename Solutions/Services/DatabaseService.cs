using SQLite;
using Solutions.Models;
using System.Text.Json;

namespace Solutions.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
        private readonly string _databasePath;

        private class TagItem
        {
            [Column("tag")]
            public string Tag { get; set; } = string.Empty;
        }

        public DatabaseService()
        {
            _databasePath = Path.Combine(FileSystem.AppDataDirectory, "solutions.db");
        }

        private async Task Init()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(_databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
            
            // Create tables
            await _database.CreateTableAsync<Solution>();
            await _database.CreateTableAsync<Category>();
            
            await _database.ExecuteAsync(
                @"CREATE TABLE IF NOT EXISTS solution_tags (
                    solution_id TEXT,
                    tag TEXT,
                    FOREIGN KEY(solution_id) REFERENCES solutions(Id)
                )");

            // Add default categories if none exist
            var categoryCount = await _database.Table<Category>().CountAsync();
            if (categoryCount == 0)
            {
                var defaultCategories = new[]
                {
                    new Category { Name = "Home", Description = "Solutions for home and living", IconName = "home" },
                    new Category { Name = "Technology", Description = "Tech-related solutions", IconName = "laptop" },
                    new Category { Name = "Health", Description = "Health and wellness solutions", IconName = "heart" },
                    new Category { Name = "DIY", Description = "Do-it-yourself projects", IconName = "tools" },
                    new Category { Name = "Other", Description = "Miscellaneous solutions", IconName = "ellipsis" }
                };

                foreach (var category in defaultCategories)
                {
                    await _database.InsertAsync(category);
                }
            }

            // Add template solutions if none exist
            var solutionCount = await _database.Table<Solution>().CountAsync();
            if (solutionCount == 0)
            {
                var templateSolutions = new[]
                {
                    new Solution 
                    { 
                        Title = "How to Reset Wi-Fi Router",
                        Description = "1. Unplug the router's power cable\n2. Wait for 30 seconds\n3. Plug the power cable back in\n4. Wait for 2-3 minutes for full restart",
                        Category = "Technology",
                        Tags = new List<string> { "wifi", "internet", "troubleshooting" },
                        AuthorName = "TechHelper"
                    },
                    new Solution 
                    { 
                        Title = "Quick Pasta Sauce Recipe",
                        Description = "Ingredients:\n- 2 cans tomatoes\n- 2 cloves garlic\n- Olive oil\n- Basil\n\nSteps:\n1. Heat oil, add minced garlic\n2. Add tomatoes and simmer\n3. Add basil and season",
                        Category = "Home",
                        Tags = new List<string> { "cooking", "recipe", "quick" },
                        AuthorName = "ChefMaster"
                    },
                    new Solution 
                    { 
                        Title = "15-Minute Home Workout",
                        Description = "Circuit:\n1. 20 jumping jacks\n2. 10 pushups\n3. 15 squats\n4. 30-sec plank\nRepeat 3 times",
                        Category = "Health",
                        Tags = new List<string> { "fitness", "exercise", "home workout" },
                        AuthorName = "FitPro"
                    },
                    new Solution 
                    { 
                        Title = "Make a Phone Stand from Paper",
                        Description = "Materials needed:\n- A4 paper\n- Scissors\n\nSteps:\n1. Fold paper in half lengthwise\n2. Create triangular base\n3. Fold sides for support",
                        Category = "DIY",
                        Tags = new List<string> { "craft", "phone", "paper" },
                        AuthorName = "CraftLover"
                    },
                    new Solution 
                    { 
                        Title = "Fix Squeaky Door Hinge",
                        Description = "You'll need:\n- WD-40 or cooking oil\n\nSteps:\n1. Apply oil to hinge\n2. Move door back and forth\n3. Wipe excess oil",
                        Category = "Home",
                        Tags = new List<string> { "repair", "door", "maintenance" },
                        AuthorName = "HandyPerson"
                    }
                };

                foreach (var solution in templateSolutions)
                {
                    await SaveSolutionAsync(solution);
                }
            }
        }

        #region Solutions

        public async Task<List<Solution>> GetSolutionsAsync()
        {
            await Init();
            var solutions = await _database!.Table<Solution>().ToListAsync();
            foreach (var solution in solutions)
            {
                solution.Tags = await GetTagsForSolutionAsync(solution.Id);
            }
            return solutions;
        }

        public async Task<Solution?> GetSolutionAsync(string id)
        {
            await Init();
            var solution = await _database!.Table<Solution>().Where(s => s.Id == id).FirstOrDefaultAsync();
            if (solution != null)
            {
                solution.Tags = await GetTagsForSolutionAsync(id);
            }
            return solution;
        }

        public async Task<int> SaveSolutionAsync(Solution solution)
        {
            await Init();
            var result = await _database!.InsertOrReplaceAsync(solution);
            await SaveTagsForSolutionAsync(solution.Id, solution.Tags);
            
            // Update category solution count
            if (!string.IsNullOrEmpty(solution.Category))
            {
                await UpdateCategorySolutionCount(solution.Category);
            }
            
            return result;
        }

        public async Task<int> DeleteSolutionAsync(Solution solution)
        {
            await Init();
            await _database!.ExecuteAsync(
                "DELETE FROM solution_tags WHERE solution_id = ?", solution.Id);
            
            var result = await _database.DeleteAsync(solution);
            
            // Update category solution count
            if (!string.IsNullOrEmpty(solution.Category))
            {
                await UpdateCategorySolutionCount(solution.Category);
            }
            
            return result;
        }

        public async Task<List<Solution>> SearchSolutionsAsync(string searchTerm)
        {
            await Init();
            var solutions = await _database!.Table<Solution>()
                .Where(s => s.Title.Contains(searchTerm) 
                    || s.Description.Contains(searchTerm) 
                    || s.Category.Contains(searchTerm))
                .ToListAsync();

            foreach (var solution in solutions)
            {
                solution.Tags = await GetTagsForSolutionAsync(solution.Id);
            }
            return solutions;
        }

        private async Task<List<string>> GetTagsForSolutionAsync(string solutionId)
        {
            await Init();
            var tags = await _database!.QueryAsync<TagItem>(
                "SELECT tag as Tag FROM solution_tags WHERE solution_id = ?", solutionId);
            return tags.Select(t => t.Tag).ToList();
        }

        private async Task SaveTagsForSolutionAsync(string solutionId, List<string> tags)
        {
            await Init();
            await _database!.ExecuteAsync(
                "DELETE FROM solution_tags WHERE solution_id = ?", solutionId);

            foreach (var tag in tags)
            {
                await _database.ExecuteAsync(
                    "INSERT INTO solution_tags (solution_id, tag) VALUES (?, ?)",
                    solutionId, tag);
            }
        }

        #endregion

        #region Categories

        public async Task<List<Category>> GetCategoriesAsync()
        {
            await Init();
            return await _database!.Table<Category>().ToListAsync();
        }

        public async Task<Category?> GetCategoryAsync(string id)
        {
            await Init();
            return await _database!.Table<Category>().Where(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveCategoryAsync(Category category)
        {
            await Init();
            return await _database!.InsertOrReplaceAsync(category);
        }

        public async Task<int> DeleteCategoryAsync(Category category)
        {
            await Init();
            // Update solutions in this category to "Other" category
            var otherCategory = await _database!.Table<Category>()
                .Where(c => c.Name == "Other")
                .FirstOrDefaultAsync();

            if (otherCategory != null)
            {
                await _database.ExecuteAsync(
                    "UPDATE solutions SET Category = ? WHERE Category = ?",
                    otherCategory.Id, category.Id);
                
                await UpdateCategorySolutionCount(otherCategory.Id);
            }

            return await _database.DeleteAsync(category);
        }

        public async Task<int> UpdateCategorySolutionCount(string categoryId)
        {
            await Init();
            var count = await _database!.Table<Solution>()
                .Where(s => s.Category == categoryId)
                .CountAsync();

            await _database.ExecuteAsync(
                "UPDATE categories SET SolutionCount = ? WHERE Id = ?",
                count, categoryId);

            return count;
        }

        #endregion
    }
}
