using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;


namespace CsWinRTApp.Models
{
    public class GeFileInfo2 : INotifyPropertyChanged
    { 
        private string _filePath;
        private bool _isDirectory;
        private bool _isWebpPending;

        public string FilePath 
        { 
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDirectory 
        { 
            get => _isDirectory;
            set
            {
                if (_isDirectory != value)
                {
                    _isDirectory = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsWebpPending 
        { 
            get => _isWebpPending;
            set
            {
                if (_isWebpPending != value)
                {
                    _isWebpPending = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public GeFileInfo2( ) : this(string.Empty, false)
        {
        }

        public GeFileInfo2(string filePath, bool isDirectory = false, bool isWebpPending = false)
        {
            _filePath = filePath;
            _isDirectory = isDirectory;
            _isWebpPending = isWebpPending;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}