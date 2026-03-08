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

namespace CsWinRTApp
{
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<GeFileView> FilesCollection { get; } = new();
        public ObservableCollection<FolderItem> FolderHistory { get; } = new();
        private FileService _fileService = new FileService();
        private FolderHistoryService _folderHistoryService = new FolderHistoryService();
        private string _currentDirectory = string.Empty;
        private string _rootDirectory = string.Empty;
        private bool _isGridView = true;
        private bool _showHiddenFiles = false;
        private bool _showExcludedFiles = true;
        private bool _isResizing = false;
        private double _startX = 0;
        private double _startWidth = 0;
        private const double MinSidebarWidth = 256;
        private const double MaxSidebarWidth = 512;

        public MainPage()
        {
            System.Diagnostics.Debug.WriteLine("=== MainPage Constructor START ===");

            try
            {
                System.Diagnostics.Debug.WriteLine("MainPage.InitializeComponent() starting...");
                this.InitializeComponent();
                System.Diagnostics.Debug.WriteLine("MainPage.InitializeComponent() completed");

                System.Diagnostics.Debug.WriteLine("UpdateViewModeButtons() starting...");
                UpdateViewModeButtons();
                System.Diagnostics.Debug.WriteLine("UpdateViewModeButtons() completed");

                System.Diagnostics.Debug.WriteLine("InitializeAsync() starting...");
                _ = InitializeAsync();
                System.Diagnostics.Debug.WriteLine("InitializeAsync() dispatched");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in MainPage constructor: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }

            System.Diagnostics.Debug.WriteLine("=== MainPage Constructor END ===");
        }

        private async Task InitializeAsync()
        {
            await LoadFolderHistoryAsync();
            await LoadFilesAsync(null);
        }

        private async Task LoadFolderHistoryAsync()
        {
            try
            {
                var folders = await _folderHistoryService.GetAllFoldersAsync();
                FolderHistory.Clear();
                foreach (var folder in folders)
                {
                    // 检查文件夹是否仍然存在
                    if (Directory.Exists(folder))
                    {
                        FolderHistory.Add(new FolderItem(folder));
                    }
                    else
                    {
                        // 如果文件夹不存在，从数据库中移除
                        await _folderHistoryService.RemoveFolderAsync(folder);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading folder history: {ex.Message}");
            }
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
            FileGridView.Visibility = Visibility.Visible;
            FileListView.Visibility = Visibility.Collapsed;
            UpdateViewModeButtons();
        }

        private void ListViewButton_Click(object sender, RoutedEventArgs e)
        {
            _isGridView = false;
            FileGridView.Visibility = Visibility.Collapsed;
            FileListView.Visibility = Visibility.Visible;
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
                FilesCollection.Clear();
                await LoadFilesAsync(_currentDirectory);
            }
        }

        private async void ShowExcludedFilesButton_Click(object sender, RoutedEventArgs e)
        {
            _showExcludedFiles = ShowExcludedFilesButton.IsChecked ?? true;

            // 重新加载当前目录
            if (!string.IsNullOrEmpty(_currentDirectory))
            {
                FilesCollection.Clear();
                await LoadFilesAsync(_currentDirectory);
            }
        }

        private async void FolderHistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FolderHistoryListView.SelectedItem is FolderItem folderItem)
            {
                if (Directory.Exists(folderItem.FullPath))
                {
                    FilesCollection.Clear();
                    _rootDirectory = folderItem.FullPath;
                    await LoadFilesAsync(folderItem.FullPath);

                    // 更新访问时间
                    await _folderHistoryService.AddOrUpdateFolderAsync(folderItem.FullPath);
                }
                else
                {
                    StatusBarText.Text = $"错误: 文件夹不存在 - {folderItem.FullPath}";
                    // 从历史记录中移除
                    await _folderHistoryService.RemoveFolderAsync(folderItem.FullPath);
                    await LoadFolderHistoryAsync();
                }
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

        private void FileGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileGridView.SelectedItem is GeFileView selectedFile)
            {
                StatusBarText.Text = $"选中: {selectedFile.FileInfo.FilePath}";
            }
            else
            {
                UpdateStatusBar();
            }
        }

        private void FileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileListView.SelectedItem is GeFileView selectedFile)
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

        private void FileContainer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
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
                    Frame.Navigate(typeof(Page2), fileView.FileInfo, new SuppressNavigationTransitionInfo());
                }
            }
        }

        private void FileListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (FileListView.SelectedItem is GeFileView fileView)
            {
                if (fileView.FileInfo.IsDirectory)
                {
                    _ = NavigateToDirectoryAsync(fileView.FileInfo.FilePath);
                }
                else
                {
                    Frame.Navigate(typeof(Page2), fileView.FileInfo, new SuppressNavigationTransitionInfo());
                }
            }
        }

        private async Task NavigateToDirectoryAsync(string directoryPath)
        {
            try
            {
                FilesCollection.Clear();
                _currentDirectory = directoryPath;
                await LoadFilesAsync(directoryPath);
                // LoadFilesAsync 内部会调用 UpdateStatusBar，所以这里不需要再调用
            }
            catch (Exception ex)
            {
                StatusBarText.Text = $"错误: 无法打开文件夹 - {ex.Message}";
            }
        }

        private void FileGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is GeFileView fileView)
            {
                if (!fileView.FileInfo.IsDirectory)
                {
                    Frame.Navigate(typeof(Page2), fileView.FileInfo, new SuppressNavigationTransitionInfo());
                }
            }
        }

        private void FileListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is GeFileView fileView)
            {
                if (!fileView.FileInfo.IsDirectory)
                {
                    Frame.Navigate(typeof(Page2), fileView.FileInfo, new SuppressNavigationTransitionInfo());
                }
            }
        }

        private async Task LoadFilesAsync(string fileDir)
        {
            IReadOnlyList<StorageFile> filesList;

            if (string.IsNullOrEmpty(fileDir))
            {
                //imageDir = "Assets\\Images";

                //var appFolder = Package.Current.InstalledLocation;
                //var samplesFolder = await appFolder.GetFolderAsync(imageDir);  // Adjust path as needed
                //var queryOptions = new QueryOptions(CommonFileQuery.OrderByName, new[]
                //{
                //    ".jpg", ".jpeg", ".png", ".gif", ".bmp"
                //});
                //var query = samplesFolder.CreateFileQueryWithOptions(queryOptions);
                //filesList = await query.GetFilesAsync();

                //foreach (var file in filesList)
                //{
                //    var fileInfo = new GeFileInfo
                //    {
                //        FilePath = file.Path,
                //        IsDirectory = false
                //    };
                //    FilesCollection.Add(new GeFileView(fileInfo));
                //}

                //_currentDirectory = samplesFolder.Path;
                //throw new InvalidOperationException("初始目录未指定");
                return;
            }
            
            var list = await _fileService.LoadFilesAsync(fileDir, DispatcherQueue, _showHiddenFiles, _showExcludedFiles);
            foreach (var file in list)
            { 
                FilesCollection.Add(new GeFileView(file));
            }

            _currentDirectory = fileDir;
            

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

                FilesCollection.Clear();
                // 重置根目录
                _rootDirectory = selectedFolder.Path;

                // 保存到历史记录
                await _folderHistoryService.AddOrUpdateFolderAsync(selectedFolder.Path);
                await LoadFolderHistoryAsync();

                // Call the function to traverse and print files
                await LoadFilesAsync(selectedFolder.Path);
            }
            else
            {
                Console.WriteLine("No directory selected.");
            }
        }

        private async void FileGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Phase == 0)
            {
                args.RegisterUpdateCallback(LoadFile);
                args.Handled = true;
            }
        }

        private async void FileListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Phase == 0)
            {
                args.RegisterUpdateCallback(LoadListViewFile);
                args.Handled = true;
            }
        }

        private async void LoadFile(ListViewBase sender, ContainerContentChangingEventArgs args)
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
            else if (_fileService.IsSvgFile(item.FileInfo.FilePath))
            {
                // SVG 文件尚未完成后台转换，显示占位图标。
                // 转换完成后 FileInfo.FilePath 会更新为 PNG 路径，
                // 届时 IsImageFile 分支将处理实际缩略图的加载。
                var stackPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Spacing = 4
                };

                var svgIcon = new FontIcon
                {
                    Glyph = "\uE91B",
                    FontSize = 48,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 140, 0))
                };

                var fileName = new TextBlock
                {
                    Text = Path.GetFileName(item.FileInfo.FilePath),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    MaxWidth = 150
                };

                stackPanel.Children.Add(svgIcon);
                stackPanel.Children.Add(fileName);
                ctrBorder.Child = stackPanel;
            }
            else if (_fileService.IsImageFile(item.FileInfo.FilePath))
            {
                var thumbnail = await item.GetThumbnailAsync();

                // 如果缩略图为null（例如WebP文件），显示图片图标
                if (thumbnail == null)
                {
                    var stackPanel = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Spacing = 4
                    };

                    var imageIcon = new FontIcon
                    {
                        Glyph = "\uEB9F", // 图片图标
                        FontSize = 48,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    var fileName = new TextBlock
                    {
                        Text = Path.GetFileName(item.FileInfo.FilePath),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center,
                        MaxWidth = 150
                    };

                    stackPanel.Children.Add(imageIcon);
                    stackPanel.Children.Add(fileName);
                    ctrBorder.Child = stackPanel;
                }
                else
                {
                    var ctrFile = new Image
                    {
                        Source = thumbnail,
                        Stretch = Stretch.UniformToFill
                    };
                    ctrBorder.Child = ctrFile;
                }
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

        private async void LoadListViewFile(ListViewBase sender, ContainerContentChangingEventArgs args)
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
            else if (_fileService.IsSvgFile(item.FileInfo.FilePath))
            {
                if (iconContainer != null)
                {
                    iconContainer.Child = new FontIcon
                    {
                        Glyph = "\uE91B",
                        FontSize = 20,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 140, 0))
                    };
                }
                if (fileTypeText != null)
                {
                    fileTypeText.Text = ".SVG";
                }
            }
            else if (_fileService.IsImageFile(item.FileInfo.FilePath))
            {
                if (iconContainer != null)
                {
                    try
                    {
                        var thumbnail = await item.GetThumbnailAsync();

                        // 如果缩略图为null（例如WebP文件），显示图标
                        if (thumbnail == null)
                        {
                            iconContainer.Child = new FontIcon
                            {
                                Glyph = "\uEB9F", // 图片图标
                                FontSize = 20,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                        }
                        else
                        {
                            var image = new Image
                            {
                                Source = thumbnail,
                                Stretch = Stretch.UniformToFill,
                                Width = 32,
                                Height = 32
                            };
                            iconContainer.Child = image;
                        }
                    }
                    catch
                    {
                        iconContainer.Child = new FontIcon
                        {
                            Glyph = "\uEB9F", // 图片图标
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

        private void FileGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Calculate item width based on GridView width and column count
            // Assuming 4 columns by default, adjust as needed
            const int columnCount = 8;
            double itemWidth = (FileGridView.ActualWidth - 4) / columnCount; // 4px for margins

            if (FileGridView.ItemsPanelRoot is ItemsWrapGrid wrapGrid)
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

        #region Sidebar Resize Logic

        private void ResizeGripper_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                // 使用更明显的视觉反馈
                border.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(128, 200, 200, 200));
            }
        }

        private void ResizeGripper_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!_isResizing)
            {
                if (sender is Border border)
                {
                    border.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                }
            }
        }

        private void ResizeGripper_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                _isResizing = true;
                _startX = e.GetCurrentPoint(this).Position.X;
                _startWidth = SidebarColumn.ActualWidth;
                border.CapturePointer(e.Pointer);
                // 拖动时在Page级别设置光标
                ProtectedCursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.SizeWestEast);
                e.Handled = true;
            }
        }

        private void ResizeGripper_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isResizing)
            {
                var currentX = e.GetCurrentPoint(this).Position.X;
                var delta = currentX - _startX;
                var newWidth = _startWidth + delta;

                // 限制宽度在最小和最大值之间
                newWidth = Math.Max(MinSidebarWidth, Math.Min(MaxSidebarWidth, newWidth));

                SidebarColumn.Width = new GridLength(newWidth);
                e.Handled = true;
            }
        }

        private void ResizeGripper_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_isResizing && sender is Border border)
            {
                _isResizing = false;
                border.ReleasePointerCapture(e.Pointer);
                border.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                // 恢复默认光标
                ProtectedCursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.Arrow);
                e.Handled = true;
            }
        }

        private void ResizeGripper_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (_isResizing)
            {
                _isResizing = false;
                if (sender is Border border)
                {
                    border.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                }
                // 恢复默认光标
                ProtectedCursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.Arrow);
            }
        }

        #endregion
    }
}
