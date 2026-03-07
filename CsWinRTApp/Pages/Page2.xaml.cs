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
using CsWinRTApp.Services;
using Microsoft.Web.WebView2.Core;

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
        private bool _isWebViewInitialized = false;

        private enum ImageDisplayMode
        {
            FitToWindow,
            ActualSize
        }

        private ImageDisplayMode _currentImageMode = ImageDisplayMode.FitToWindow;

        public Page2()
        {
            try
            {
                LogService.Info("Page2 constructor started");
                this.InitializeComponent();
                LogService.Info("InitializeComponent completed");
                InitializeWebView();
                LogService.Info("Page2 constructor completed");
            }
            catch (Exception ex)
            {
                LogService.Error("Page2 constructor failed", ex);
                throw;
            }
        }

        private async void InitializeWebView()
        {
            try
            {
                LogService.Info("Starting WebView2 initialization");
                await WebViewer.EnsureCoreWebView2Async();
                _isWebViewInitialized = true;
                LogService.Info("WebView2 initialization completed successfully");
            }
            catch (Exception ex)
            {
                LogService.Error("WebView2 initialization failed", ex);
                System.Diagnostics.Debug.WriteLine($"WebView2 initialization error: {ex.Message}");
            }
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
            if (Frame.CanGoBack)
            {
                Frame.GoBack(new SuppressNavigationTransitionInfo());
            }
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
                else if (IsSvgFile(extension))
                {
                    await ShowSvgPreviewAsync();
                }
                else if (IsHtmlFile(extension))
                {
                    await ShowHtmlPreviewAsync();
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
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".ico", ".webp", ".awebp" };
            return imageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        private bool IsSvgFile(string extension)
        {
            return extension.Equals(".svg", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsHtmlFile(string extension)
        {
            string[] htmlExtensions = { ".html", ".htm" };
            return htmlExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
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

                BitmapImage bitmap;

                // 检查是否为 WebP 文件
                if (WebPImageService.IsWebPFile(_fileInfo.FilePath))
                {
                    // 使用 WebPImageService 加载 WebP 图像（不限制尺寸，加载完整图像）
                    bitmap = await WebPImageService.LoadWebPImageAsync(_fileInfo.FilePath);
                    if (bitmap == null)
                    {
                        throw new Exception("无法加载 WebP 图像");
                    }
                }
                else
                {
                    // 对于其他格式，使用标准方法
                    var file = await StorageFile.GetFileFromPathAsync(_fileInfo.FilePath);
                    var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                }

                ImageViewer.Source = bitmap;
                ImageViewer.Visibility = Visibility.Visible;
                TextViewer.Visibility = Visibility.Collapsed;
                FileInfoViewer.Visibility = Visibility.Collapsed;
                WebViewer.Visibility = Visibility.Collapsed;

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
                WebViewer.Visibility = Visibility.Collapsed;
                _isImageFile = false;
                ImageControlPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async Task ShowSvgPreviewAsync()
        {
            try
            {
                if (!_isWebViewInitialized)
                {
                    TextViewer.Text = "WebView2 未初始化，无法显示 SVG";
                    TextViewer.Visibility = Visibility.Visible;
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"ShowSvgPreviewAsync: Loading SVG from {_fileInfo.FilePath}");

                var file = await StorageFile.GetFileFromPathAsync(_fileInfo.FilePath);
                var svgContent = await FileIO.ReadTextAsync(file);

                // 创建一个包装 SVG 的 HTML 页面，使其居中显示
                var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{
            margin: 0;
            padding: 20px;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            background-color: #f5f5f5;
        }}
        svg {{
            max-width: 100%;
            max-height: 100%;
            object-fit: contain;
        }}
    </style>
</head>
<body>
    {svgContent}
</body>
</html>";

                WebViewer.NavigateToString(htmlContent);
                WebViewer.Visibility = Visibility.Visible;
                ImageViewer.Visibility = Visibility.Collapsed;
                TextViewer.Visibility = Visibility.Collapsed;
                FileInfoViewer.Visibility = Visibility.Collapsed;

                System.Diagnostics.Debug.WriteLine($"ShowSvgPreviewAsync: SVG loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowSvgPreviewAsync error: {ex.Message}");
                TextViewer.Text = $"无法加载 SVG: {ex.Message}";
                TextViewer.Visibility = Visibility.Visible;
                ImageViewer.Visibility = Visibility.Collapsed;
                WebViewer.Visibility = Visibility.Collapsed;
                FileInfoViewer.Visibility = Visibility.Collapsed;
            }
        }

        private async Task ShowHtmlPreviewAsync()
        {
            try
            {
                if (!_isWebViewInitialized)
                {
                    TextViewer.Text = "WebView2 未初始化，无法显示 HTML";
                    TextViewer.Visibility = Visibility.Visible;
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"ShowHtmlPreviewAsync: Loading HTML from {_fileInfo.FilePath}");

                // 使用 Navigate 方法加载本地 HTML 文件
                var fileUri = new Uri($"file:///{_fileInfo.FilePath.Replace("\\", "/")}");
                WebViewer.Source = fileUri;

                WebViewer.Visibility = Visibility.Visible;
                ImageViewer.Visibility = Visibility.Collapsed;
                TextViewer.Visibility = Visibility.Collapsed;
                FileInfoViewer.Visibility = Visibility.Collapsed;

                System.Diagnostics.Debug.WriteLine($"ShowHtmlPreviewAsync: HTML loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowHtmlPreviewAsync error: {ex.Message}");
                TextViewer.Text = $"无法加载 HTML: {ex.Message}";
                TextViewer.Visibility = Visibility.Visible;
                ImageViewer.Visibility = Visibility.Collapsed;
                WebViewer.Visibility = Visibility.Collapsed;
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
                WebViewer.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                TextViewer.Text = $"无法读取文本文件: {ex.Message}";
                TextViewer.Visibility = Visibility.Visible;
                ImageViewer.Visibility = Visibility.Collapsed;
                FileInfoViewer.Visibility = Visibility.Collapsed;
                WebViewer.Visibility = Visibility.Collapsed;
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
                WebViewer.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                TextViewer.Text = $"无法读取文件信息: {ex.Message}";
                TextViewer.Visibility = Visibility.Visible;
                ImageViewer.Visibility = Visibility.Collapsed;
                FileInfoViewer.Visibility = Visibility.Collapsed;
                WebViewer.Visibility = Visibility.Collapsed;
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
