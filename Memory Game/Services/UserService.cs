using System.IO;
using System.Text.Json;
using MemoryGame.Models;

namespace MemoryGame.Services
{
    public class UserService
    {
        private readonly string _usersFilePath;
        private readonly string _userImagesFolder;

        public UserService()
        {
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MemoryGame");

            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            _usersFilePath = Path.Combine(appDataFolder, "users.json");
            _userImagesFolder = Path.Combine(appDataFolder, "UserImages");

            if (!Directory.Exists(_userImagesFolder))
            {
                Directory.CreateDirectory(_userImagesFolder);
            }
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            if (!File.Exists(_usersFilePath))
            {
                return new List<UserModel>();
            }

            try
            {
                string json = await File.ReadAllTextAsync(_usersFilePath);
                return JsonSerializer.Deserialize<List<UserModel>>(json) ?? new List<UserModel>();
            }
            catch (Exception)
            {
                return new List<UserModel>();
            }
        }

        public async Task SaveUsersAsync(List<UserModel> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_usersFilePath, json);
        }

        public async Task AddUserAsync(UserModel user)
        {
            var users = await GetAllUsersAsync();

            if (users.Find(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)) != null)
            {
                throw new InvalidOperationException("Username already exists!");
            }

            users.Add(user);
            await SaveUsersAsync(users);
        }

        public async Task DeleteUserAsync(string username)
        {
            var users = await GetAllUsersAsync();
            var user = users.Find(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                if (!string.IsNullOrEmpty(user.ImagePath) &&
                    File.Exists(user.ImagePath) &&
                    user.ImagePath.StartsWith(_userImagesFolder))
                {
                    try
                    {
                        File.Delete(user.ImagePath);
                    }
                    catch (Exception)
                    {
                        //TODO
                    }
                }

                users.Remove(user);
                await SaveUsersAsync(users);
            }
        }

        public async Task UpdateUserStatsAsync(string username, bool won)
        {
            var users = await GetAllUsersAsync();
            var user = users.Find(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                user.GamesPlayed++;
                if (won)
                {
                    user.GamesWon++;
                }

                await SaveUsersAsync(users);
            }
        }

        public string GetRandomDefaultImagePath()
        {
            string assetsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Avatars");

            if (!Directory.Exists(assetsFolder))
            {
                Directory.CreateDirectory(assetsFolder);
                return null;
            }

            string[] imageFiles = Directory.GetFiles(assetsFolder, "*.png")
                .Concat(Directory.GetFiles(assetsFolder, "*.jpg"))
                .Concat(Directory.GetFiles(assetsFolder, "*.jpeg"))
                .ToArray();

            if (imageFiles.Length == 0)
            {
                return null;
            }

            Random random = new Random();
            return imageFiles[random.Next(imageFiles.Length)];
        }

        public string CopyImageToUserFolder(string sourceImagePath, string username)
        {
            if (string.IsNullOrEmpty(sourceImagePath) || !File.Exists(sourceImagePath))
            {
                return null;
            }

            string fileName = $"{username}_{Path.GetFileName(sourceImagePath)}";
            string destinationPath = Path.Combine(_userImagesFolder, fileName);

            File.Copy(sourceImagePath, destinationPath, true);

            return destinationPath;
        }
    }
}