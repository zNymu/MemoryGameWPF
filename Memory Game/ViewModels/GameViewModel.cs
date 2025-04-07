using System.Collections.ObjectModel;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using MemoryGame.Models;
using MemoryGame.Commands;
using MemoryGame.Services;
using MemoryGame.Views;

namespace MemoryGame.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private readonly UserService _userService;
        private readonly GameSaveService _gameSaveService;
        private readonly string _imagesBasePath;
        private string _selectedCategory = "Drinks";
        private bool _isStandardMode = true;
        private bool _isCustomMode;
        private int _rows = 4;
        private int _columns = 4;
        private int _customRows = 4;
        private int _customColumns = 4;
        private ObservableCollection<CardModel> _gameCards;
        private bool _isGameActive;
        private UserModel _currentUser;
        private int _gameTimeInSeconds = 120;
        private string _timeLeft = "2:00";
        private System.Timers.Timer _gameTimer;
        private int _remainingSeconds;
        private List<CardModel> _flippedCards = new List<CardModel>();

        public ObservableCollection<CardModel> GameCards
        {
            get => _gameCards;
            set => SetProperty(ref _gameCards, value);
        }

        public UserModel CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public bool IsStandardMode
        {
            get => _isStandardMode;
            set => SetProperty(ref _isStandardMode, value);
        }

        public bool IsCustomMode
        {
            get => _isCustomMode;
            set => SetProperty(ref _isCustomMode, value);
        }

        public int Rows
        {
            get => _rows;
            set => SetProperty(ref _rows, value);
        }

        public int Columns
        {
            get => _columns;
            set => SetProperty(ref _columns, value);
        }

        public int CustomRows
        {
            get => _customRows;
            set => SetProperty(ref _customRows, value);
        }

        public int CustomColumns
        {
            get => _customColumns;
            set => SetProperty(ref _customColumns, value);
        }

        public bool IsGameActive
        {
            get => _isGameActive;
            set => SetProperty(ref _isGameActive, value);
        }

        public int GameTimeInSeconds
        {
            get => _gameTimeInSeconds;
            set => SetProperty(ref _gameTimeInSeconds, value);
        }

        public string TimeLeft
        {
            get => _timeLeft;
            set => SetProperty(ref _timeLeft, value);
        }

        public List<int> AvailableDimensions { get; } = new List<int> { 2, 3, 4, 5, 6 };

        public ICommand SelectCategoryCommand { get; }
        public ICommand NewGameCommand { get; }
        public ICommand OpenGameCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand StatisticsCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand SetGameModeCommand { get; }
        public ICommand ApplyCustomSettingsCommand { get; }
        public ICommand SelectCardCommand { get; }
        public ICommand AboutCommand { get; }

        public event EventHandler CloseRequested;

        public GameViewModel(UserModel currentUser, UserService userService)
        {
            _currentUser = currentUser;
            _userService = userService;
            _gameSaveService = new GameSaveService();

            _imagesBasePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, "Assets", "Images");

            SelectCategoryCommand = new RelayCommand(SelectCategory);
            NewGameCommand = new RelayCommand(StartNewGame);
            OpenGameCommand = new RelayCommand(OpenGame);
            SaveGameCommand = new RelayCommand(SaveGame);
            StatisticsCommand = new RelayCommand(ShowStatistics);
            ExitCommand = new RelayCommand(Exit);
            SetGameModeCommand = new RelayCommand(SetGameMode);
            ApplyCustomSettingsCommand = new RelayCommand(ApplyCustomSettings);
            SelectCardCommand = new RelayCommand(SelectCard);
            AboutCommand = new RelayCommand(ShowAbout);

            GameCards = new ObservableCollection<CardModel>();
        }

        private void SelectCategory(object parameter)
        {
            SelectedCategory = parameter.ToString();
            MessageBox.Show($"Selected category: {SelectedCategory}", "Category");
        }

        private void StartNewGame(object parameter)
        {
            StopTimer();

            int totalCards = Rows * Columns;
            if (totalCards % 2 != 0)
            {
                MessageBox.Show("The number of cards must be even.", "Invalid Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string categoryPath = Path.Combine(_imagesBasePath, SelectedCategory);
            if (!Directory.Exists(categoryPath))
            {
                MessageBox.Show($"Image category not found: {SelectedCategory}\nPlease create the directory: {categoryPath}",
                    "Missing Images", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string[] imagePaths = Directory.GetFiles(categoryPath, "*.png")
                .Concat(Directory.GetFiles(categoryPath, "*.jpg"))
                .Concat(Directory.GetFiles(categoryPath, "*.jpeg"))
                .Concat(Directory.GetFiles(categoryPath, "*.gif"))
                .ToArray();

            int pairsNeeded = totalCards / 2;
            if (imagePaths.Length < pairsNeeded)
            {
                MessageBox.Show($"Not enough images in category '{SelectedCategory}'. Need {pairsNeeded} images, found {imagePaths.Length}.",
                    "Not Enough Images", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Random random = new Random();
            var selectedImages = imagePaths.OrderBy(x => random.Next()).Take(pairsNeeded).ToList();

            var cards = new List<CardModel>();
            string backImagePath = Path.Combine(_imagesBasePath, "back.png");
            if (!File.Exists(backImagePath))
            {
                MessageBox.Show($"Card back image not found: {backImagePath}",
                    "Missing Image", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            for (int i = 0; i < pairsNeeded; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    var card = new CardModel
                    {
                        Id = i,
                        FrontImagePath = selectedImages[i],
                        BackImagePath = backImagePath,
                        IsFlipped = false,
                        IsMatched = false
                    };
                    card.CurrentImagePath = card.BackImagePath;
                    cards.Add(card);
                }
            }

            var shuffledCards = cards.OrderBy(x => random.Next()).ToList();

            GameCards.Clear();
            foreach (var card in shuffledCards)
            {
                GameCards.Add(card);
            }

            _flippedCards.Clear();

            _remainingSeconds = GameTimeInSeconds;
            UpdateTimeDisplay();
            StartTimer();

            IsGameActive = true;
        }

        private void LoadGame(GameSaveModel savedGame)
        {
            try
            {
                StopTimer();

                SelectedCategory = savedGame.Category;
                Rows = savedGame.Rows;
                Columns = savedGame.Columns;
                GameTimeInSeconds = savedGame.TotalGameTimeInSeconds;
                _remainingSeconds = savedGame.RemainingTimeInSeconds;

                if (Rows == 4 && Columns == 4)
                {
                    IsStandardMode = true;
                    IsCustomMode = false;
                }
                else
                {
                    IsStandardMode = false;
                    IsCustomMode = true;
                    CustomRows = Rows;
                    CustomColumns = Columns;
                }

                GameCards.Clear();

                var sortedCards = savedGame.Cards
                    .OrderBy(c => c.GridRow)
                    .ThenBy(c => c.GridColumn)
                    .ToList();

                string backImagePath = Path.Combine(_imagesBasePath, "back.png");

                foreach (var savedCard in sortedCards)
                {
                    var card = new CardModel
                    {
                        Id = savedCard.Id,
                        FrontImagePath = savedCard.FrontImagePath,
                        BackImagePath = backImagePath,
                        IsFlipped = savedCard.IsFlipped,
                        IsMatched = savedCard.IsMatched
                    };

                    card.CurrentImagePath = savedCard.IsFlipped ? card.FrontImagePath : card.BackImagePath;

                    GameCards.Add(card);
                }

                _flippedCards.Clear();

                UpdateTimeDisplay();

                StartTimer();

                IsGameActive = true;

                MessageBox.Show("Game loaded successfully!", "Load Game", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load game: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenGame(object parameter)
        {
            StopTimer();

            try
            {
                if (IsGameActive)
                {
                    MessageBoxResult result = MessageBox.Show(
                        "Do you want to save the current game before opening another one?",
                        "Save Current Game",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        SaveGame(null);
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        StartTimer();
                        return;
                    }
                }

                var openGameWindow = new OpenGameView(CurrentUser, this);

                openGameWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                bool? dialogResult = openGameWindow.ShowDialog();

                if (dialogResult == true && openGameWindow.SelectedGame != null)
                {
                    LoadGame(openGameWindow.SelectedGame);
                }
                else
                {
                    if (IsGameActive)
                    {
                        StartTimer();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open game: {ex.Message}", "Open Game Error", MessageBoxButton.OK, MessageBoxImage.Error);

                if (IsGameActive)
                {
                    StartTimer();
                }
            }
        }

        private void SaveGame(object parameter)
        {
            if (!IsGameActive)
            {
                MessageBox.Show("No active game to save.", "Save Game", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            StopTimer();

            try
            {
                var flippedButUnmatchedCards = GameCards.Where(c => c.IsFlipped && !c.IsMatched).ToList();

                if (flippedButUnmatchedCards.Count % 2 != 0)
                {
                    foreach (var card in flippedButUnmatchedCards)
                    {
                        card.IsFlipped = false;
                    }
                    _flippedCards.Clear();

                    MessageBox.Show("Unmatched flipped cards have been reset to ensure a valid game state.",
                                   "Save Game", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                var gameSave = new GameSaveModel
                {
                    Username = CurrentUser.Username,
                    Category = SelectedCategory,
                    Rows = Rows,
                    Columns = Columns,
                    TotalGameTimeInSeconds = GameTimeInSeconds,
                    RemainingTimeInSeconds = _remainingSeconds,
                    SaveDateTime = DateTime.Now,
                    Cards = new List<SavedCardModel>()
                };

                for (int i = 0; i < GameCards.Count; i++)
                {
                    var card = GameCards[i];
                    gameSave.Cards.Add(new SavedCardModel
                    {
                        Id = card.Id,
                        FrontImagePath = card.FrontImagePath,
                        IsMatched = card.IsMatched,
                        IsFlipped = card.IsFlipped,
                        GridRow = i / Columns,
                        GridColumn = i % Columns
                    });
                }

                _gameSaveService.SaveGameAsync(gameSave).ContinueWith(task =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (task.IsCompletedSuccessfully)
                        {
                            MessageBox.Show("Game saved successfully.", "Save Game", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Failed to save game: {task.Exception?.InnerException?.Message}",
                                "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        if (IsGameActive && _gameTimer == null)
                        {
                            StartTimer();
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save game: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (IsGameActive && _gameTimer == null)
                {
                    StartTimer();
                }
            }
        }

        private void ShowStatistics(object parameter)
        {
            StopTimer();

            try
            {
                var statisticsWindow = new StatisticsView();
                statisticsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                statisticsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to show statistics: {ex.Message}", "Statistics Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (IsGameActive)
            {
                StartTimer();
            }
        }

        private void Exit(object parameter)
        {
            StopTimer();
            Application.Current.Shutdown();
        }

        private void SetGameMode(object parameter)
        {
            string mode = parameter?.ToString();
            if (mode == "Standard")
            {
                IsStandardMode = true;
                IsCustomMode = false;
                Rows = 4;
                Columns = 4;
            }
            else if (mode == "Custom")
            {
                IsStandardMode = false;
                IsCustomMode = true;
            }
        }

        private void ApplyCustomSettings(object parameter)
        {
            if (CustomRows * CustomColumns % 2 != 0)
            {
                MessageBox.Show("The total number of cards (rows × columns) must be even.",
                    "Invalid Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Rows = CustomRows;
            Columns = CustomColumns;
            MessageBox.Show($"Applied custom game settings: {Rows}×{Columns}", "Custom Settings");
        }

        private async void SelectCard(object parameter)
        {
            if (parameter is CardModel card)
            {
                if (card.IsMatched || card.IsFlipped || _flippedCards.Count >= 2)
                    return;

                card.IsFlipped = true;
                _flippedCards.Add(card);

                if (_flippedCards.Count == 2)
                {
                    if (_flippedCards[0].Id == _flippedCards[1].Id)
                    {
                        foreach (var flippedCard in _flippedCards)
                        {
                            flippedCard.IsMatched = true;
                        }
                        _flippedCards.Clear();

                        if (GameCards.All(c => c.IsMatched))
                        {
                            GameWon();
                        }
                    }
                    else
                    {
                        var flippedCardsCopy = new List<CardModel>(_flippedCards);
                        _flippedCards.Clear();

                        await Task.Delay(300);

                        foreach (var flippedCard in flippedCardsCopy)
                        {
                            flippedCard.IsFlipped = false;
                        }
                    }
                }

            }
        }
            


        private void ShowAbout(object parameter)
        {
            MessageBox.Show("WPF Memory Game\nDeveloped by: Gelegram Alexandru\nGroup: LF232",
                "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StartTimer()
        {
            StopTimer();
            _gameTimer = new System.Timers.Timer(1000);
            _gameTimer.Elapsed += GameTimer_Elapsed;
            _gameTimer.Start();
        }

        private void StopTimer()
        {
            if (_gameTimer != null)
            {
                _gameTimer.Stop();
                _gameTimer.Elapsed -= GameTimer_Elapsed;
                _gameTimer.Dispose();
                _gameTimer = null;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void GameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _remainingSeconds--;
                UpdateTimeDisplay();

                if (_remainingSeconds <= 0)
                {
                    StopTimer();
                    GameLost();
                }
            });
        }

        private void UpdateTimeDisplay()
        {
            int minutes = _remainingSeconds / 60;
            int seconds = _remainingSeconds % 60;
            TimeLeft = $"{minutes}:{seconds:D2}";
        }

        private async void GameWon()
        {
            StopTimer();
            MessageBox.Show("Congratulations! You've won the game!", "Victory", MessageBoxButton.OK, MessageBoxImage.Information);
            IsGameActive = false;

            await _userService.UpdateUserStatsAsync(CurrentUser.Username, true);
        }

        private async void GameLost()
        {
            MessageBox.Show("Time's up! You've lost the game.", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
            IsGameActive = false;

            await _userService.UpdateUserStatsAsync(CurrentUser.Username, false);
        }
    }
}