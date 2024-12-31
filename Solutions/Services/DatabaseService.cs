using SQLite;
using Solutions.Models;
using System.Text.Json;

namespace Solutions.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
        private readonly string _databasePath;

        public DatabaseService()
        {
            _databasePath = Path.Combine(FileSystem.AppDataDirectory, "solutions.db");
        }

        private async Task Init()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(_databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
            
            // Create the solutions table
            await _database.CreateTableAsync<Solution>();
            
            // Create the tags table if you want to store tags separately
            await _database.ExecuteAsync(
                @"CREATE TABLE IF NOT EXISTS solution_tags (
                    solution_id TEXT,
                    tag TEXT,
                    FOREIGN KEY(solution_id) REFERENCES solutions(Id)
                )");
        }

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

        private async Task<List<string>> GetTagsForSolutionAsync(string solutionId)
        {
            await Init();
            var tags = await _database!.QueryAsync<string>(
                "SELECT tag FROM solution_tags WHERE solution_id = ?", solutionId);
            return tags.ToList();
        }

        private async Task SaveTagsForSolutionAsync(string solutionId, List<string> tags)
        {
            await Init();
            // Delete existing tags
            await _database!.ExecuteAsync(
                "DELETE FROM solution_tags WHERE solution_id = ?", solutionId);

            // Insert new tags
            foreach (var tag in tags)
            {
                await _database.ExecuteAsync(
                    "INSERT INTO solution_tags (solution_id, tag) VALUES (?, ?)",
                    solutionId, tag);
            }
        }

        public async Task<int> SaveSolutionAsync(Solution solution)
        {
            await Init();
            var result = await _database!.InsertOrReplaceAsync(solution);
            await SaveTagsForSolutionAsync(solution.Id, solution.Tags);
            return result;
        }

        public async Task<int> DeleteSolutionAsync(Solution solution)
        {
            await Init();
            await _database!.ExecuteAsync(
                "DELETE FROM solution_tags WHERE solution_id = ?", solution.Id);
            return await _database.DeleteAsync(solution);
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
    }
}
