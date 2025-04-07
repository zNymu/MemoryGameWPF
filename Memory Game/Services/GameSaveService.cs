using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MemoryGame.Models;

namespace MemoryGame.Services
{
    public class GameSaveService
    {
        private readonly string _savesFolder;

        public GameSaveService()
        {
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MemoryGame");

            _savesFolder = Path.Combine(appDataFolder, "Saves");

            if (!Directory.Exists(_savesFolder))
            {
                Directory.CreateDirectory(_savesFolder);
            }
        }

        public async Task SaveGameAsync(GameSaveModel gameSave)
        {
            string fileName = $"{gameSave.Username}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string filePath = Path.Combine(_savesFolder, fileName);

            string json = JsonSerializer.Serialize(gameSave, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<List<GameSaveModel>> GetSavedGamesForUserAsync(string username)
        {
            List<GameSaveModel> savedGames = new List<GameSaveModel>();

            string[] saveFiles = Directory.GetFiles(_savesFolder, $"{username}_*.json");
            foreach (string file in saveFiles)
            {
                try
                {
                    string json = await File.ReadAllTextAsync(file);
                    var gameSave = JsonSerializer.Deserialize<GameSaveModel>(json);
                    if (gameSave != null)
                    {
                        gameSave.FilePath = file;
                        savedGames.Add(gameSave);
                    }
                }
                catch (Exception)
                {
                }
            }

            return savedGames;
        }

        public async Task<GameSaveModel> LoadGameAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Save file not found", filePath);
            }

            string json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<GameSaveModel>(json);
        }

        public async Task DeleteSaveAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                await Task.CompletedTask;
            }
        }
    }
}