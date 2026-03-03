using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace CsWinRTApp.Services
{
    public class FolderHistoryService
    {
        private readonly string _dbPath;
        private const string TableName = "FolderHistory";

        public FolderHistoryService()
        {
            // 获取应用程序数据目录
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CsWinRTApp");
            
            // 确保目录存在
            Directory.CreateDirectory(appDataPath);
            
            _dbPath = Path.Combine(appDataPath, "folderhistory.db");
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = $@"
                CREATE TABLE IF NOT EXISTS {TableName} (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FolderPath TEXT NOT NULL UNIQUE,
                    LastAccessTime TEXT NOT NULL
                )";
            command.ExecuteNonQuery();
        }

        public async Task<List<string>> GetAllFoldersAsync()
        {
            var folders = new List<string>();

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT FolderPath 
                FROM {TableName} 
                ORDER BY LastAccessTime DESC";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                folders.Add(reader.GetString(0));
            }

            return folders;
        }

        public async Task AddOrUpdateFolderAsync(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return;

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = $@"
                INSERT INTO {TableName} (FolderPath, LastAccessTime)
                VALUES (@path, @time)
                ON CONFLICT(FolderPath) 
                DO UPDATE SET LastAccessTime = @time";
            
            command.Parameters.AddWithValue("@path", folderPath);
            command.Parameters.AddWithValue("@time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            await command.ExecuteNonQueryAsync();
        }

        public async Task RemoveFolderAsync(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return;

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = $@"
                DELETE FROM {TableName} 
                WHERE FolderPath = @path";
            
            command.Parameters.AddWithValue("@path", folderPath);

            await command.ExecuteNonQueryAsync();
        }
    }
}
