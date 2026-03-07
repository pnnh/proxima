using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using CsWinRTApp.Services;


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
            if (BitmapImage == null)
            {
                try
                {
                    // 检查是否为 WebP 文件
                    if (WebPImageService.IsWebPFile(FileInfo.FilePath))
                    {
                        System.Diagnostics.Debug.WriteLine($"[GetThumbnailAsync] WebP file detected, returning null (will use icon): {FileInfo.FilePath}");

                        // 暂时禁用WebP缩略图预览，避免UI资源耗尽
                        // TODO: 实现磁盘缓存或按需加载机制后再启用
                        return null; // 返回null会让LoadImage()显示文件图标
                    }
                    else
                    {
                        // 对于其他格式，使用标准方法
                        if (ImageFile == null)
                        {
                            ImageFile = await StorageFile.GetFileFromPathAsync(FileInfo.FilePath);
                        }

                        var thumbnail = await ImageFile.GetThumbnailAsync(ThumbnailMode.PicturesView, 200);
                        var bitmap = new BitmapImage();
                        bitmap.SetSource(thumbnail);
                        BitmapImage = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[GetThumbnailAsync] ERROR loading thumbnail for {FileInfo.FilePath}");
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
                    LogService.Error($"Failed to load thumbnail: {FileInfo.FilePath}", ex);

                    // 返回null让UI显示图标
                    return null;
                }
            }
            return BitmapImage;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
