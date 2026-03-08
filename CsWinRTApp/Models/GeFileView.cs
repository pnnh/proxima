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

        public GeFileInfo FileInfo { get; set; }

        public bool IsWebpPending
        {
            get => FileInfo.IsWebpPending;
            set
            {
                if (FileInfo.IsWebpPending != value)
                {
                    FileInfo.IsWebpPending = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSvgPending
        {
            get => FileInfo.IsSvgPending;
            set
            {
                if (FileInfo.IsSvgPending != value)
                {
                    FileInfo.IsSvgPending = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public GeFileView(GeFileInfo fileInfo)
        {
            FileInfo = fileInfo;
            // 订阅FileInfo的PropertyChanged事件，转发给UI
            FileInfo.PropertyChanged += (s, e) =>
            {
                // 当FileInfo的任何属性改变时，通知UI相关属性也改变了
                if (e.PropertyName == nameof(GeFileInfo.IsWebpPending))
                {
                    OnPropertyChanged(nameof(IsWebpPending));
                    // WebP转换完成，清除缓存的BitmapImage以便重新加载
                    if (!FileInfo.IsWebpPending)
                    {
                        BitmapImage = null;
                        OnPropertyChanged(nameof(BitmapImage));
                    }
                }
                else if (e.PropertyName == nameof(GeFileInfo.IsSvgPending))
                {
                    OnPropertyChanged(nameof(IsSvgPending));
                    // SVG转换完成，清除缓存的BitmapImage以便重新加载
                    if (!FileInfo.IsSvgPending)
                    {
                        BitmapImage = null;
                        OnPropertyChanged(nameof(BitmapImage));
                    }
                }
                else if (e.PropertyName == nameof(GeFileInfo.FilePath))
                {
                    OnPropertyChanged(nameof(FileInfo));
                    BitmapImage = null;
                    OnPropertyChanged(nameof(BitmapImage));
                }
            };
        }

        public void UpdateFilePath(string newPath)
        {
            FileInfo.FilePath = newPath;
            OnPropertyChanged(nameof(FileInfo));
            BitmapImage = null;
        }

        public async Task<BitmapImage> GetThumbnailAsync()
        {
            if (BitmapImage == null)
            {
                try
                {
                    if (WebPImageService.IsWebPFile(FileInfo.FilePath))
                    {
                        // 如果还未转换，直接返回null（显示图标）
                        if (IsWebpPending)
                            return null;
                        // 优先用磁盘缓存
                        BitmapImage = await WebPImageService.GetOrCreateCachedThumbnailAsync(FileInfo.FilePath, 128);
                        if (BitmapImage == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[GetThumbnailAsync] WebP load failed, fallback to icon: {FileInfo.FilePath}");
                            return null;
                        }
                    }
                    else
                    {
                        // 其他格式
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
                    return null;
                }
            }
            return BitmapImage;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
