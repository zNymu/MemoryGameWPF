using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoryGame.Models
{
    public class UserModel : INotifyPropertyChanged
    {
        private string _username;
        private string _imagePath;
        private int _gamesPlayed;
        private int _gamesWon;

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                if (_imagePath != value)
                {
                    _imagePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public int GamesPlayed
        {
            get => _gamesPlayed;
            set
            {
                if (_gamesPlayed != value)
                {
                    _gamesPlayed = value;
                    OnPropertyChanged();
                }
            }
        }

        public int GamesWon
        {
            get => _gamesWon;
            set
            {
                if (_gamesWon != value)
                {
                    _gamesWon = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}