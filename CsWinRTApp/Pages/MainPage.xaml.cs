using CsWinRTApp.Models;
using CsWinRTApp.Pages;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using CsWinRTApp.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CsWinRTApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<GeFileView> Images { get; } = new();
        private FileService _fileService = new FileService();
        private string _currentDirectory = string.Empty;
        private string _rootDirectory = string.Empty;
        private bool _isGridView = true;
        private bool _showHiddenFiles = false;
        private bool _showExcludedFiles = true;

        public MainPage()
        {
            this.InitializeComponent();
            UpdateViewModeButtons();
            _ = LoadImagesAsync(null);  // Call async load
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentDirectory) && _currentDirectory != _rootDirectory)
            {
                try
                {
                    var parentDir = Directory.GetParent(_currentDirectory);
                    if (parentDir != null && !string.IsNullOrEmpty(parentDir.FullName))
                    {
                        _ = NavigateToDirectoryAsync(parentDir.FullName);
                    }
                }
                catch (Exception ex)
                {
                    StatusBarText.Text = $"错误: 无法返回上一级 - {ex.Message}";
                }
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentDirectory))
            {
                try
                {
                    var parentDir = Directory.GetParent(_currentDirectory);
                    if (parentDir != null && !string.IsNullOrEmpty(parentDir.FullName))
                    {
                        // 检查是否超出根目录
                        if (string.IsNullOrEmpty(_rootDirectory) || 
                            parentDir.FullName.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
                        {
                            _ = NavigateToDirectoryAsync(parentDir.FullName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    StatusBarText.Text = $"错误: 无法进入上级目录 - {ex.Message}";
                }
            }
        }

        private void GridViewButton_Click(object sender, RoutedEventArgs e)
        {
            _isGridView = true;
            ImageGridView.Visibility = Visibility.Visible;
            ImageListView.Visibility = Visibility.Collapsed;
            UpdateViewModeButtons();
        }

        private void ListViewButton_Click(object sender, RoutedEventArgs e)
        {
            _isGridView = false;
            ImageGridView.Visibility = Visibility.Collapsed;
            ImageListView.Visibility = Visibility.Visible;
            UpdateViewModeButtons();
        }

        private void UpdateViewModeButtons()
        {
            GridViewButton.IsEnabled = !_isGridView;
            ListViewButton.IsEnabled = _isGridView;
        }

        private async void ShowHiddenFilesButton_Click(object sender, RoutedEventArgs e)
        {
            _showHiddenFiles = ShowHiddenFilesButton.IsChecked ?? false;

            // 重新加载当前目录
            if (!string.IsNullOrEmpty(_currentDirectory))
            {
                Images.Clear();
                await LoadImagesAsync(_currentDirectory);
            }
        }

        private async void ShowExcludedFilesButton_Click(object sender, RoutedEventArgs e)
        {
            _showExcludedFiles = ShowExcludedFilesButton.IsChecked ?? true;

            // 重新加载当前目录
            if (!string.IsNullOrEmpty(_currentDirectory))
            {
                Images.Clear();
                await LoadImagesAsync(_currentDirectory);
            }
        }

        private void UpdateBackButtonState()
        {
            // 如果当前目录为空或等于根目录，禁用返回按钮
            if (string.IsNullOrEmpty(_currentDirectory) || 
                string.IsNullOrEmpty(_rootDirectory) ||
                _currentDirectory == _rootDirectory)
            {
                BackButton.IsEnabled = false;
            }
            else
            {
                // 检查是否有父目录
                try
                {
                    var parentDir = Directory.GetParent(_currentDirectory);
                    BackButton.IsEnabled = (parentDir != null && !string.IsNullOrEmpty(parentDir.FullName));
                }
                catch
                {
                    BackButton.IsEnabled = false;
                }
            }

            // 更新上级按钮状态
            UpdateUpButtonState();
        }

        private void UpdateUpButtonState()
        {
            // 检查当前目录是否有父目录，且父目录在根目录范围内
            if (string.IsNullOrEmpty(_currentDirectory))
            {
                UpButton.IsEnabled = false;
                return;
            }

            try
            {
                var parentDir = Directory.GetParent(_currentDirectory);
                if (parentDir == null || string.IsNullOrEmpty(parentDir.FullName))
                {
                    UpButton.IsEnabled = false;
                    return;
                }

                // 如果有根目录限制，检查父目录是否在根目录范围内
                if (!string.IsNullOrEmpty(_rootDirectory))
                {
                    UpButton.IsEnabled = parentDir.FullName.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase) ||
                                        parentDir.FullName.Equals(_rootDirectory, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    UpButton.IsEnabled = true;
                }
            }
            catch
            {
                UpButton.IsEnabled = false;
            }
        }

        private void ImageGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageGridView.SelectedItem is GeFileView selectedFile)
            {
                StatusBarText.Text = $"选中: {selectedFile.FileInfo.FilePath}";
            }
            else
            {
                UpdateStatusBar();
            }
        }

        private void ImageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageListView.SelectedItem is GeFileView selectedFile)
            {
                StatusBarText.Text = $"选中: {selectedFile.FileInfo.FilePath}";
            }
            else
            {
                UpdateStatusBar();
            }
        }

        private void UpdateStatusBar()
        {
            if (!string.IsNullOrEmpty(_currentDirectory))
            {
                StatusBarText.Text = $"当前位置: {_currentDirectory}";
            }
            else
            {
                StatusBarText.Text = "就绪";
            }

            UpdateBackButtonState();
        }

        private void ImageContainer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                // 尝试从 Tag 属性获取数据
                if (border.Tag is GeFileView fileView && fileView.FileInfo.IsDirectory)
                {
                    _ = NavigateToDirectoryAsync(fileView.FileInfo.FilePath);
                }
            }
        }

        private void ListItemContainer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is GeFileView fileView)
            {
                if (fileView.FileInfo.IsDirectory)
                {
                    _ = NavigateToDirectoryAsync(fileView.FileInfo.FilePath);
                }
                else
                {
                    Frame.Navigate(typeof(Page2), fileView.FileInfo, new SlideNavigationTransitionInfo
                    {
                        Effect = SlideNavigationTransitionEffect.FromRight
                    });
                }
            }
        }

        private void ImageListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (ImageListView.SelectedItem is GeFileView fileView)
            {
                if (fileView.FileInfo.IsDirectory)
                {
                    _ = NavigateToDirectoryAsync(fileView.FileInfo.FilePath);
                }
                else
                {
                    Frame.Navigate(typeof(Page2), fileView.FileInfo, new SlideNavigationTransitionInfo
                    {
                        Effect = SlideNavigationTransitionEffect.FromRight
                    });
                }
            }
        }

        private async Task NavigateToDirectoryAsync(string directoryPath)
        {
            try
            {
                Images.Clear();
                _currentDirectory = directoryPath;
                await LoadImagesAsync(directoryPath);
                // LoadImagesAsync 内部会调用 UpdateStatusBar，所以这里不需要再调用
            }
            catch (Exception ex)
            {
                StatusBarText.Text = $"错误: 无法打开文件夹 - {ex.Message}";
            }
        }

        private void ImageGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is GeFileView fileView)
            {
                if (!fileView.FileInfo.IsDirectory)
                {
                    Frame.Navigate(typeof(Page2), fileView.FileInfo, new SlideNavigationTransitionInfo
                    {
                        Effect = SlideNavigationTransitionEffect.FromRight
                    });
                }
            }
        }

        private void ImageListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is GeFileView fileView)
            {
                if (!fileView.FileInfo.IsDirectory)
                {
                    Frame.Navigate(typeof(Page2), fileView.FileInfo, new SlideNavigationTransitionInfo
                    {
                        Effect = SlideNavigationTransitionEffect.FromRight
                    });
                }
            }
        }

        private async Task LoadImagesAsync(string imageDir)
        {
            IReadOnlyList<StorageFile> filesList;

            if (string.IsNullOrEmpty(imageDir))
            {
                imageDir = "Assets\\Images";

                var appFolder = Package.Current.InstalledLocation;
                var samplesFolder = await appFolder.GetFolderAsync(imageDir);  // Adjust path as needed
                var queryOptions = new QueryOptions(CommonFileQuery.OrderByName, new[]
                {
                    ".jpg", ".jpeg", ".png", ".gif", ".bmp"
                });
                var query = samplesFolder.CreateFileQueryWithOptions(queryOptions);
                filesList = await query.GetFilesAsync();

                foreach (var file in filesList)
                {
                    var fileInfo = new GeFileInfo2
                    {
                        FilePath = file.Path,
                        IsDirectory = false
                    };
                    Images.Add(new GeFileView(fileInfo));
                }

                _currentDirectory = samplesFolder.Path;
            }
            else
            {
                var list = await _fileService.LoadFilesAsync(imageDir, _showHiddenFiles, _showExcludedFiles);
                foreach (var file in list)
                { 
                    Images.Add(new GeFileView(file));
                }

                _currentDirectory = imageDir;
            }

            UpdateStatusBar();
        }


        private async Task SelectAndProcessDirectoryAsync()
        {
            // Create and configure the FolderPicker
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Downloads; // Optional: Start in Pictures folder
            folderPicker.FileTypeFilter.Add("*"); // Required for FolderPicker, but doesn't affect folder selection

            var windowId = this.XamlRoot.ContentIslandEnvironment.AppWindowId; // 'this' refers to the Page
            var hWnd = Win32Interop.GetWindowFromWindowId(windowId);
            // Initialize the picker with the current window handle (required for WinUI3)
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

            // Show the picker and get the selected folder
            StorageFolder selectedFolder = await folderPicker.PickSingleFolderAsync();

            if (selectedFolder != null)
            {
                // Print the full path of the selected directory to the console
                Console.WriteLine($"Selected directory: {selectedFolder.Path}");

                Images.Clear();
                // 重置根目录
                _rootDirectory = selectedFolder.Path;
                // Call the function to traverse and print image files
                await LoadImagesAsync(selectedFolder.Path);
            }
            else
            {
                Console.WriteLine("No directory selected.");
            }
        }

        private async void ImageGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Phase == 0)
            {
                args.RegisterUpdateCallback(LoadImage);
                args.Handled = true;
            }
        }

        private async void ImageListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Phase == 0)
            {
                args.RegisterUpdateCallback(LoadListViewImage);
                args.Handled = true;
            }
        }

        private async void LoadImage(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Phase != 1)
            {
                return;
            }

            var item = args.Item as GeFileView;
            if (item == null) return;
            var ctrBorder = args.ItemContainer.ContentTemplateRoot as Border;
            if (ctrBorder == null) return;

            // 设置 Border 的 Tag 为数据项，以便在 DoubleTapped 事件中访问
            ctrBorder.Tag = item;

            if (item.FileInfo.IsDirectory)
            {
                var stackPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Spacing = 4
                };

                var folderIcon = new FontIcon
                {
                    Glyph = "\uE8B7",
                    FontSize = 48,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var folderName = new TextBlock
                {
                    Text = Path.GetFileName(item.FileInfo.FilePath),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center
                };

                stackPanel.Children.Add(folderIcon);
                stackPanel.Children.Add(folderName);
                ctrBorder.Child = stackPanel;
            }
            else if (_fileService.IsImageFile(item.FileInfo.FilePath))
            {
                var ctrImage = new Image
                {
                    Source = await item.GetThumbnailAsync(),
                    Stretch = Stretch.UniformToFill
                };
                ctrBorder.Child = ctrImage;
            }
            else
            {
                var ctrText = new TextBlock
                {
                    Text = Path.GetFileName(item.FileInfo.FilePath),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
                ctrBorder.Child = ctrText;
            }
        }

        private async void LoadListViewImage(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Phase != 1)
            {
                return;
            }

            var item = args.Item as GeFileView;
            if (item == null) return;

            var grid = args.ItemContainer.ContentTemplateRoot as Grid;
            if (grid == null) return;

            var iconContainer = grid.FindName("ListIconContainer") as Border;
            var fileNameText = grid.FindName("ListFileName") as TextBlock;
            var fileTypeText = grid.FindName("ListFileType") as TextBlock;

            if (fileNameText != null)
            {
                fileNameText.Text = Path.GetFileName(item.FileInfo.FilePath);
            }

            if (item.FileInfo.IsDirectory)
            {
                if (iconContainer != null)
                {
                    iconContainer.Child = new FontIcon
                    {
                        Glyph = "\uE8B7",
                        FontSize = 20,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                }
                if (fileTypeText != null)
                {
                    fileTypeText.Text = "文件夹";
                }
            }
            else if (_fileService.IsImageFile(item.FileInfo.FilePath))
            {
                if (iconContainer != null)
                {
                    try
                    {
                        var image = new Image
                        {
                            Source = await item.GetThumbnailAsync(),
                            Stretch = Stretch.UniformToFill,
                            Width = 32,
                            Height = 32
                        };
                        iconContainer.Child = image;
                    }
                    catch
                    {
                        iconContainer.Child = new FontIcon
                        {
                            Glyph = "\uE91B",
                            FontSize = 20,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                    }
                }
                if (fileTypeText != null)
                {
                    fileTypeText.Text = Path.GetExtension(item.FileInfo.FilePath).ToUpper();
                }
            }
            else
            {
                if (iconContainer != null)
                {
                    iconContainer.Child = new FontIcon
                    {
                        Glyph = "\uE8A5",
                        FontSize = 20,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                }
                if (fileTypeText != null)
                {
                    var ext = Path.GetExtension(item.FileInfo.FilePath);
                    fileTypeText.Text = string.IsNullOrEmpty(ext) ? "文件" : ext.ToUpper();
                }
            }
        }

        private void ImageGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Calculate item width based on GridView width and column count
            // Assuming 4 columns by default, adjust as needed
            const int columnCount = 8;
            double itemWidth = (ImageGridView.ActualWidth - 4) / columnCount; // 4px for margins

            if (ImageGridView.ItemsPanelRoot is ItemsWrapGrid wrapGrid)
            {
                wrapGrid.ItemWidth = itemWidth;
                wrapGrid.ItemHeight = itemWidth; // Keep 1:1 aspect ratio
            }
        }

        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            //myButton.Content = "Clicked";

            //var cls = new CppWinRTComponent.Class();
            //var sum = cls.Add(2, 3);
            //Console.WriteLine($"sum: {sum}");

            await SelectAndProcessDirectoryAsync();


            //var stackPanel = new StackPanel();
            //stackPanel.Orientation = Orientation.Horizontal;
            //stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
            //stackPanel.VerticalAlignment = VerticalAlignment.Top;

            //var newButton = new Button();
            //newButton.Content = "NewButton";

            //stackPanel.Children.Add(newButton);

            //FilesStack.Children.Add(stackPanel);

            //var aaa = Logger.Info("xxxxxx333");
            //Console.WriteLine($"aaa = {aaa}");

        }
    }
}
