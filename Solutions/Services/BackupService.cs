using System.Text.Json;
using Solutions.Models;

namespace Solutions.Services
{
    public interface IBackupService
    {
        Task<bool> ExportDataAsync(string filePath);
        Task<bool> ImportDataAsync(string filePath);
        Task<string> GetBackupFolderPath();
    }

    public class BackupService : IBackupService
    {
        private readonly DatabaseService _databaseService;

        public BackupService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<string> GetBackupFolderPath()
        {
            var backupFolder = Path.Combine(FileSystem.AppDataDirectory, "Backups");
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }
            return backupFolder;
        }

        public async Task<bool> ExportDataAsync(string filePath)
        {
            try
            {
                var backupData = new
                {
                    Solutions = await _databaseService.GetSolutionsAsync(),
                    Categories = await _databaseService.GetCategoriesAsync()
                };

                var jsonString = JsonSerializer.Serialize(backupData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(filePath, jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ImportDataAsync(string filePath)
        {
            try
            {
                var jsonString = await File.ReadAllTextAsync(filePath);
                var backupData = JsonSerializer.Deserialize<dynamic>(jsonString);

                // Import categories first
                foreach (var category in backupData.Categories.EnumerateArray())
                {
                    await _databaseService.SaveCategoryAsync(category.Deserialize<Category>());
                }

                // Then import solutions
                foreach (var solution in backupData.Solutions.EnumerateArray())
                {
                    await _databaseService.SaveSolutionAsync(solution.Deserialize<Solution>());
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
