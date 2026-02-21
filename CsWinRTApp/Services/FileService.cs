using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using CsWinRTApp.Models;


namespace CsWinRTApp.Services
{
    public sealed partial class FileService : Window
    {
        public FileService()
        {

        }

        public bool IsImageFile(string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            string extension = Path.GetExtension(filePath);
            return imageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }


        public async Task<List<GeFileInfo2>> LoadFilesAsync(string imageDir)
        {
            var list = new List<GeFileInfo2>();

            try
            {
                // Get all directories first
                string[] directories = Directory.GetDirectories(imageDir);
                foreach (string directory in directories)
                {
                    list.Add(new GeFileInfo2(directory, true));
                    Console.WriteLine($"Found directory: {directory}");
                }

                // Then get all files in the directory
                string[] files = Directory.GetFiles(imageDir);
                foreach (string file in files)
                {
                    list.Add(new GeFileInfo2(file, false));
                    Console.WriteLine($"Found file: {file}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading files: {ex.Message}");
            }

            return list;
        }

    }
}