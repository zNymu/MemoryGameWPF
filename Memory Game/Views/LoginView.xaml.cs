using System.Windows;
using MemoryGame.ViewModels;
using MemoryGame.Services;
using MemoryGame.Models;

namespace MemoryGame.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            var userService = new UserService();
            var viewModel = new LoginViewModel(userService);

            viewModel.PlayRequested += OnPlayRequested;

            DataContext = viewModel;
        }

        private void OnPlayRequested(UserModel user)
        {
            var gameWindow = new GameView(user);
            gameWindow.Show();

            this.Close();
        }
    }
}