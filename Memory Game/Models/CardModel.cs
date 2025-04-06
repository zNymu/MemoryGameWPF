using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoryGame.Models
{
    public class CardModel : INotifyPropertyChanged
    {
        private string _frontImagePath;
        private string _backImagePath;
        private string _currentImagePath;
        private bool _isFlipped;
        private bool _isMatched;
        private int _id;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FrontImagePath
        {
            get => _frontImagePath;
            set
            {
                if (_frontImagePath != value)
                {
                    _frontImagePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string BackImagePath
        {
            get => _backImagePath;
            set
            {
                if (_backImagePath != value)
                {
                    _backImagePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentImagePath
        {
            get => _currentImagePath;
            set
            {
                if (_currentImagePath != value)
                {
                    _currentImagePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsFlipped
        {
            get => _isFlipped;
            set
            {
                if (_isFlipped != value)
                {
                    _isFlipped = value;
                    CurrentImagePath = _isFlipped ? FrontImagePath : BackImagePath;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsMatched
        {
            get => _isMatched;
            set
            {
                if (_isMatched != value)
                {
                    _isMatched = value;
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