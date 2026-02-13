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
            // Get all files in the directory
            string[] files = Directory.GetFiles(imageDir);

            var list = new List<GeFileInfo2>();

            foreach (string file in files)
            { 
                // todo 暂时只加载图片文件，后续可以考虑其他类型的文件
                //if (!IsImageFile(file)) continue;
                list.Add(new GeFileInfo2(file));
                // Print the full path of the image file
                Console.WriteLine($"Found file: {file}");

            }


            return list;
        }

    }
}