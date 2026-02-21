using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Imaging;
using CsWinRTApp.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CsWinRTApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page2 : Page
    {
        private GeFileInfo2 _fileInfo;

        public Page2()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage), new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromLeft
            });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is GeFileInfo2 fileInfo)
            {
                _fileInfo = fileInfo;
                await LoadFilePreviewAsync();
            }
        }

        private async Task LoadFilePreviewAsync()
        {
            if (_fileInfo == null || string.IsNullOrEmpty(_fileInfo.FilePath))
                return;

            try
            {
                var fileName = Path.GetFileName(_fileInfo.FilePath);
                FileNameText.Text = fileName;

                var extension = Path.GetExtension(_fileInfo.FilePath).ToLower();

                if (IsImageFile(extension))
                {
                    await ShowImagePreviewAsync();
                }
                else if (IsTextFile(extension))
                {
                    await ShowTextPreviewAsync();
                }
                else
                {
                    await ShowFileInfoAsync();
                }
            }
            catch (Exception ex)
            {
                TextViewer.Text = $"无法加载文件预览: {ex.Message}";
                TextViewer.Visibility = Visibility.Visible;
            }
        }

        private bool IsImageFile(string extension)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".ico", ".svg" };
            return imageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        private bool IsTextFile(string extension)
        {
            string[] textExtensions = { ".txt", ".cs", ".xaml", ".xml", ".json", ".md", ".log", 
                                       ".config", ".html", ".css", ".js", ".py", ".java", ".cpp", 
                                       ".h", ".c", ".ts", ".jsx", ".tsx", ".yml", ".yaml" };
            return textExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        private async Task ShowImagePreviewAsync()
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(_fileInfo.FilePath);
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream);

                ImageViewer.Source = bitmap;
                ImageViewer.Visibility = Visibility.Visible;
                TextViewer.Visibility = Visibility.Collapsed;
                FileInfoViewer.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                TextViewer.Text = $"无法加载图片: {ex.Message}";
                TextViewer.Visibility = Visibility.Visible;
                ImageViewer.Visibility = Visibility.Collapsed;
                FileInfoViewer.Visibility = Visibility.Collapsed;
            }
        }

        private async Task ShowTextPreviewAsync()
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(_fileInfo.FilePath);
                var text = await FileIO.ReadTextAsync(file);

                const int maxLength = 100000;
                if (text.Length > maxLength)
                {
                    text = text.Substring(0, maxLength) + "\n\n... (文件内容过长，已截断)";
                }

                TextViewer.Text = text;
                TextViewer.Visibility = Visibility.Visible;
                ImageViewer.Visibility = Visibility.Collapsed;
                FileInfoViewer.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                TextViewer.Text = $"无法读取文本文件: {ex.Message}";
                TextViewer.Visibility = Visibility.Visible;
                ImageViewer.Visibility = Visibility.Collapsed;
                FileInfoViewer.Visibility = Visibility.Collapsed;
            }
        }

        private async Task ShowFileInfoAsync()
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(_fileInfo.FilePath);
                var properties = await file.GetBasicPropertiesAsync();

                FileInfoTitle.Text = file.Name;
                FileInfoSize.Text = $"大小: {FormatFileSize((long)properties.Size)}";
                FileInfoPath.Text = $"路径: {file.Path}";
                FileInfoExtension.Text = $"类型: {file.FileType}";
                FileInfoCreated.Text = $"创建时间: {properties.DateModified:yyyy-MM-dd HH:mm:ss}";
                FileInfoModified.Text = $"修改时间: {properties.DateModified:yyyy-MM-dd HH:mm:ss}";

                FileInfoViewer.Visibility = Visibility.Visible;
                ImageViewer.Visibility = Visibility.Collapsed;
                TextViewer.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                TextViewer.Text = $"无法读取文件信息: {ex.Message}";
                TextViewer.Visibility = Visibility.Visible;
                ImageViewer.Visibility = Visibility.Collapsed;
                FileInfoViewer.Visibility = Visibility.Collapsed;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
