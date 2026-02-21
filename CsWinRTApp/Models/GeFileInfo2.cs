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
    public class GeFileInfo2 
    { 
        public string FilePath { get; set; }
        public bool IsDirectory { get; set; }

        public GeFileInfo2( ) : this(string.Empty, false)
        {
        }

        public GeFileInfo2(string filePath, bool isDirectory = false)
        {
            FilePath = filePath;
            IsDirectory = isDirectory;
        }

    }
}