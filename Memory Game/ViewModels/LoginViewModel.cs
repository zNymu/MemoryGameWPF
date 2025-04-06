using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.Drawing;
using MemoryGame.Commands;
using MemoryGame.Models;
using MemoryGame.Services;

namespace MemoryGame.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly UserService _userService;
        private UserModel _selectedUser;
        private string _newUsername;
        private string _newUserImagePath;
        private bool _isCreatingNewUser;
        private bool _isLoading = true;

        public ObservableCollection<UserModel> Users { get; private set; }

        public UserModel SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    ((RelayCommand)PlayCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteUserCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string NewUsername
        {
            get => _newUsername;
            set
            {
                if (SetProperty(ref _newUsername, value))
                {
                    ((RelayCommand)CreateUserCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string NewUserImagePath
        {
            get => _newUserImagePath;
            set
            {
                if (SetProperty(ref _newUserImagePath, value))
                {
                    ((RelayCommand)CreateUserCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsCreatingNewUser
        {
            get => _isCreatingNewUser;
            set => SetProperty(ref _isCreatingNewUser, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand PlayCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ShowNewUserFormCommand { get; }
        public ICommand BrowseImageCommand { get; }
        public ICommand SelectRandomImageCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand CancelCreateUserCommand { get; }

        public event Action<UserModel> PlayRequested;

        public LoginViewModel(UserService userService)
        {
            _userService = userService;
            Users = new ObservableCollection<UserModel>();

            PlayCommand = new RelayCommand(
                param => StartGame(),
                param => SelectedUser != null
            );

            DeleteUserCommand = new RelayCommand(
                async param => await DeleteSelectedUser(),
                param => SelectedUser != null
            );

            ShowNewUserFormCommand = new RelayCommand(
                param => ShowNewUserForm()
            );

            BrowseImageCommand = new RelayCommand(
                param => BrowseForImage()
            );

            SelectRandomImageCommand = new RelayCommand(
                param => SelectRandomImage()
            );

            CreateUserCommand = new RelayCommand(
                async param => await CreateNewUser(),
                param => !string.IsNullOrWhiteSpace(NewUsername) && !string.IsNullOrWhiteSpace(NewUserImagePath)
            );

            CancelCreateUserCommand = new RelayCommand(
                param => CancelNewUserCreation()
            );

            LoadUsers();
        }

        private void StartGame()
        {
            if (SelectedUser != null)
            {
                PlayRequested?.Invoke(SelectedUser);
            }
        }

        private async void LoadUsers()
        {
            IsLoading = true;
            var users = await _userService.GetAllUsersAsync();

            if (users.Count == 0)
            {
                await CreateDefaultUser();
                users = await _userService.GetAllUsersAsync();
            }

            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }

            if (Users.Count > 0)
            {
                SelectedUser = Users[0];
            }

            IsLoading = false;
        }

        private async Task CreateDefaultUser()
        {
            try
            {
                string defaultImagePath = _userService.GetRandomDefaultImagePath();

                if (string.IsNullOrEmpty(defaultImagePath))
                {
                    string appDataFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "MemoryGame", "UserImages");

                    if (!Directory.Exists(appDataFolder))
                    {
                        Directory.CreateDirectory(appDataFolder);
                    }

                    string defaultImageFilePath = Path.Combine(appDataFolder, "default_user.png");
                    if (!File.Exists(defaultImageFilePath))
                    {
                        using (var bitmap = new Bitmap(100, 100))
                        {
                            using (var g = Graphics.FromImage(bitmap))
                            {
                                g.Clear(Color.SkyBlue);
                            }
                            bitmap.Save(defaultImageFilePath, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }

                    defaultImagePath = defaultImageFilePath;
                }
                else
                {
                    defaultImagePath = _userService.CopyImageToUserFolder(defaultImagePath, "DefaultUser");
                }

                var defaultUser = new UserModel
                {
                    Username = "DefaultUser",
                    ImagePath = defaultImagePath,
                    GamesPlayed = 0,
                    GamesWon = 0
                };

                await _userService.AddUserAsync(defaultUser);
                MessageBox.Show("Created default user as no users were found.", "Default User Created", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating default user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteSelectedUser()
        {
            if (SelectedUser != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete user '{SelectedUser.Username}'?\nThis will also delete all their game statistics.",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _userService.DeleteUserAsync(SelectedUser.Username);
                        Users.Remove(SelectedUser);

                        SelectedUser = Users.Count > 0 ? Users[0] : null;

                        MessageBox.Show($"User '{SelectedUser.Username}' has been deleted.", "User Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ShowNewUserForm()
        {
            IsCreatingNewUser = true;

            SelectRandomImage();
        }

        private void BrowseForImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif)|*.jpg;*.jpeg;*.png;*.gif",
                Title = "Select an image for the user"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                NewUserImagePath = openFileDialog.FileName;
            }
        }

        private void SelectRandomImage()
        {
            string randomImage = _userService.GetRandomDefaultImagePath();
            if (!string.IsNullOrEmpty(randomImage))
            {
                NewUserImagePath = randomImage;
            }
            else
            {
                MessageBox.Show("No avatar images found in the Assets/Avatars folder.",
                    "No Avatars Available", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task CreateNewUser()
        {
            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewUserImagePath))
            {
                MessageBox.Show("Username and avatar image are required.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewUsername.Contains(" "))
            {
                MessageBox.Show("Username cannot contain spaces.", "Invalid Username", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var existingUsers = await _userService.GetAllUsersAsync();
                if (existingUsers.Exists(u => u.Username.Equals(NewUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("This username already exists.", "Username Taken", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string destinationPath = _userService.CopyImageToUserFolder(NewUserImagePath, NewUsername);

                if (string.IsNullOrEmpty(destinationPath))
                {
                    MessageBox.Show("Failed to copy user image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newUser = new UserModel
                {
                    Username = NewUsername,
                    ImagePath = destinationPath,
                    GamesPlayed = 0,
                    GamesWon = 0
                };

                await _userService.AddUserAsync(newUser);
                Users.Add(newUser);
                CancelNewUserCreation();
                SelectedUser = newUser;

                MessageBox.Show($"User '{NewUsername}' created successfully!", "User Created", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelNewUserCreation()
        {
            IsCreatingNewUser = false;
            NewUsername = string.Empty;
            NewUserImagePath = string.Empty;
        }
    }
}