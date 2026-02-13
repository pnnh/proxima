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

        public MainPage()
        {
            this.InitializeComponent();
            _ = LoadImagesAsync(null);  // Call async load
        }
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Page2), "SomeText", new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromRight
            });
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
                    };
                    Images.Add(new GeFileView(fileInfo));
                }
            }
            else
            {
                var list = await _fileService.LoadFilesAsync(imageDir);
                foreach (var file in list)
                { 
                    Images.Add(new GeFileView(file));
                }
            }
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

                if (_fileService.IsImageFile(item.FileInfo.FilePath))
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
