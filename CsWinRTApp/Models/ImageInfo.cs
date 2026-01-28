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
    public class ImageFileInfo : INotifyPropertyChanged
    {
        public StorageFile ImageFile { get; }
        public string ImageName { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        public ImageFileInfo(StorageFile file)
        {
            ImageFile = file;
            ImageName = file.Name;
        }

        public async Task<BitmapImage> GetThumbnailAsync()
        {
            var thumbnail = await ImageFile.GetThumbnailAsync(ThumbnailMode.PicturesView, 200);
            var bitmap = new BitmapImage();
            bitmap.SetSource(thumbnail);
            return bitmap;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
