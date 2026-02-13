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
    public class GeFileView : INotifyPropertyChanged
    {
        public StorageFile? ImageFile { get; set; }
        public BitmapImage? BitmapImage { get; set; }

        public GeFileInfo2 FileInfo { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public GeFileView(GeFileInfo2 fileInfo)
        {
            FileInfo = fileInfo;

        }

        public async Task<BitmapImage> GetThumbnailAsync()
        {
            if (ImageFile == null)
            {
                ImageFile = await StorageFile.GetFileFromPathAsync(FileInfo.FilePath);

            }

            if (BitmapImage == null)
            {
                var thumbnail = await ImageFile.GetThumbnailAsync(ThumbnailMode.PicturesView, 200);
                var bitmap = new BitmapImage();
                bitmap.SetSource(thumbnail);
                BitmapImage = bitmap;

            }
            return BitmapImage;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
