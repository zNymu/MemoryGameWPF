using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MemoryGame.Models;
using MemoryGame.Services;

namespace MemoryGame.Views
{
    public partial class StatisticsView : Window
    {
        private readonly UserService _userService;

        public StatisticsView()
        {
            InitializeComponent();
            _userService = new UserService();
            LoadStatistics();
        }

        private async void LoadStatistics()
        {
            var users = await _userService.GetAllUsersAsync();
            var statsViewModels = users.Select(u => new StatisticsViewModel
            {
                Username = u.Username,
                GamesPlayed = u.GamesPlayed,
                GamesWon = u.GamesWon,
                WinPercentage = u.GamesPlayed > 0 ? (double)u.GamesWon / u.GamesPlayed * 100 : 0
            }).ToList();

            statsDataGrid.ItemsSource = statsViewModels;

            GridView gridView = new GridView();
            statsDataGrid.View = gridView;

            GridViewColumn usernameColumn = new GridViewColumn
            {
                Header = "Username",
                DisplayMemberBinding = new Binding("Username")
            };
            gridView.Columns.Add(usernameColumn);

            GridViewColumn gamesPlayedColumn = new GridViewColumn
            {
                Header = "Games Played",
                DisplayMemberBinding = new Binding("GamesPlayed")
            };
            gridView.Columns.Add(gamesPlayedColumn);

            GridViewColumn gamesWonColumn = new GridViewColumn
            {
                Header = "Games Won",
                DisplayMemberBinding = new Binding("GamesWon")
            };
            gridView.Columns.Add(gamesWonColumn);

            GridViewColumn winPercentageColumn = new GridViewColumn
            {
                Header = "Win %",
                DisplayMemberBinding = new Binding("WinPercentage") { StringFormat = "{0:F1}%" }
            };
            gridView.Columns.Add(winPercentageColumn);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class StatisticsViewModel
    {
        public string Username { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public double WinPercentage { get; set; }
    }
}