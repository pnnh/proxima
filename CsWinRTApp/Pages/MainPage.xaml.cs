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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CsWinRTApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<ImageFileInfo> Images { get; } = new();
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
            // Ïà¶ÔÄ¿Â¼£¬Ôò´ÓÓ¦ÓÃ°üÖÐ¼ÓÔØÍ¼Æ¬
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

            }
            else
            {
                // Define common image file extensions
                var imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg", ".jpeg", ".png", ".gif", ".bmp"
                };

                // Get all files in the directory (non-recursive)
                string[] files = Directory.GetFiles(imageDir);

                var list = new List<StorageFile>();

                foreach (string file in files)
                {
                    string extension = Path.GetExtension(file);
                    if (imageExtensions.Contains(extension))
                    {
                        StorageFile storageFile = await StorageFile.GetFileFromPathAsync(file);
                        list.Add(storageFile);
                        // Print the full path of the image file
                        Console.WriteLine($"Image file: {file}");

                    }
                }

                filesList = list;
            }


            foreach (var file in filesList)
            {
                Images.Add(new ImageFileInfo(file));
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
            if (args.Phase == 1)
            {
                var item = args.Item as ImageFileInfo;
                var image = args.ItemContainer.ContentTemplateRoot as Image;
                image.Source = await item.GetThumbnailAsync();
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
