using System.Windows;
using MemoryGame.Models;
using MemoryGame.Services;
using MemoryGame.ViewModels;

namespace MemoryGame.Views
{
    public partial class GameView : Window
    {
        private GameViewModel _viewModel;
        public GameView(UserModel user)
                {
                    InitializeComponent();
                    var userService = new UserService();
                    var viewModel = new GameViewModel(user, userService);
                    viewModel.CloseRequested += OnCloseRequested;
                    DataContext = viewModel;
                }

        private void OnCloseRequested(object sender, EventArgs e)
                {
                    var loginWindow = new LoginView();
                    loginWindow.Show();
                    this.Close();
                }
            
    }
    
}