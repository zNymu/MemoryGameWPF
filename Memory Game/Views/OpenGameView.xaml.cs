using System;
using System.Windows;
using System.Windows.Controls;
using MemoryGame.Models;
using MemoryGame.Services;
using MemoryGame.ViewModels;

namespace MemoryGame.Views
{
    public partial class OpenGameView : Window
    {
        private readonly UserModel _currentUser;
        private readonly GameSaveService _gameSaveService;
        private GameViewModel _gameViewModel;

        public GameSaveModel SelectedGame { get; private set; }

        public OpenGameView(UserModel currentUser, GameViewModel gameViewModel)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _gameViewModel = gameViewModel;
            _gameSaveService = new GameSaveService();
            LoadSavedGames();
        }

        private async void LoadSavedGames()
        {
            var savedGames = await _gameSaveService.GetSavedGamesForUserAsync(_currentUser.Username);

            if (savedGames.Count == 0)
            {
                MessageBox.Show("No saved games found.", "Open Game", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                return;
            }

            savedGamesListView.ItemsSource = savedGames;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (savedGamesListView.SelectedItem is GameSaveModel selectedGame)
            {
                SelectedGame = selectedGame;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a game to open.", "Open Game", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (savedGamesListView.SelectedItem is GameSaveModel selectedGame)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete this saved game?\n{selectedGame.DisplayName}",
                    "Delete Saved Game",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _gameSaveService.DeleteSaveAsync(selectedGame.FilePath);
                    LoadSavedGames();
                }
            }
            else
            {
                MessageBox.Show("Please select a game to delete.", "Delete Game", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}