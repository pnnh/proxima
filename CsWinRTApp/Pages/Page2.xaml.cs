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
        private bool _isImageFile = false;

        private enum ImageDisplayMode
        {
            FitToWindow,
            ActualSize
        }

        private ImageDisplayMode _currentImageMode = ImageDisplayMode.FitToWindow;

        public Page2()
        {
            this.InitializeComponent();
        }

        private void FitToWindowButton_Click(object sender, RoutedEventArgs e)
        {
            _currentImageMode = ImageDisplayMode.FitToWindow;
            UpdateButtonStates();
            ApplyImageDisplayMode();
        }

        private void ActualSizeButton_Click(object sender, RoutedEventArgs e)
        {
            _currentImageMode = ImageDisplayMode.ActualSize;
            UpdateButtonStates();
            ApplyImageDisplayMode();
        }

        private void UpdateButtonStates()
        {
            if (FitToWindowButton == null || ActualSizeButton == null)
                return;

            try
            {
                if (_currentImageMode == ImageDisplayMode.FitToWindow)
                {
                    FitToWindowButton.IsEnabled = false;
                    ActualSizeButton.IsEnabled = true;

                    var selectedBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightBlue);
                    FitToWindowButton.Background = selectedBrush;
                    ActualSizeButton.Background = null;
                }
                else
                {
                    FitToWindowButton.IsEnabled = true;
                    ActualSizeButton.IsEnabled = false;

                    var selectedBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightBlue);
                    ActualSizeButton.Background = selectedBrush;
                    FitToWindowButton.Background = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateButtonStates error: {ex.Message}");
            }
        }

        private void ApplyImageDisplayMode()
        {
            if (!_isImageFile || ImageViewer?.Source == null)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyImageDisplayMode: skipped - _isImageFile={_isImageFile}, Source={ImageViewer?.Source != null}");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"ApplyImageDisplayMode: mode={_currentImageMode}");

            if (_currentImageMode == ImageDisplayMode.FitToWindow)
            {
                ImageViewer.Stretch = Stretch.Uniform;
                ImageViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
                ImageViewer.VerticalAlignment = VerticalAlignment.Stretch;
                ContentScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                ContentScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                ContentGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                ContentGrid.VerticalAlignment = VerticalAlignment.Stretch;
                ContentGrid.Padding = new Thickness(0);
            }
            else
            {
                ImageViewer.Stretch = Stretch.None;
                ImageViewer.HorizontalAlignment = HorizontalAlignment.Center;
                ImageViewer.VerticalAlignment = VerticalAlignment.Center;
                ContentScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                ContentScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                ContentGrid.HorizontalAlignment = HorizontalAlignment.Left;
                ContentGrid.VerticalAlignment = VerticalAlignment.Top;
                ContentGrid.Padding = new Thickness(16);
            }
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

                _isImageFile = IsImageFile(extension);
                ImageControlPanel.Visibility = _isImageFile ? Visibility.Visible : Visibility.Collapsed;

                if (_isImageFile)
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
                System.Diagnostics.Debug.WriteLine($"ShowImagePreviewAsync: Loading image from {_fileInfo.FilePath}");

                var file = await StorageFile.GetFileFromPathAsync(_fileInfo.FilePath);
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream);

                ImageViewer.Source = bitmap;
                ImageViewer.Visibility = Visibility.Visible;
                TextViewer.Visibility = Visibility.Collapsed;
                FileInfoViewer.Visibility = Visibility.Collapsed;

                System.Diagnostics.Debug.WriteLine($"ShowImagePreviewAsync: Image loaded successfully");

                _currentImageMode = ImageDisplayMode.FitToWindow;
                UpdateButtonStates();
                ApplyImageDisplayMode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowImagePreviewAsync error: {ex.Message}");
                TextViewer.Text = $"无法加载图片: {ex.Message}";
                TextViewer.Visibility = Visibility.Visible;
                ImageViewer.Visibility = Visibility.Collapsed;
                FileInfoViewer.Visibility = Visibility.Collapsed;
                _isImageFile = false;
                ImageControlPanel.Visibility = Visibility.Collapsed;
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
