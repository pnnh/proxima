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

        public bool IsHiddenFile(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);

                // 检查是否以点或下划线开头
                if (!string.IsNullOrEmpty(fileName) && 
                    (fileName.StartsWith(".") || fileName.StartsWith("_")))
                {
                    return true;
                }

                // 检查 Windows 文件隐藏属性
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists && (fileInfo.Attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden)
                {
                    return true;
                }

                // 检查目录的隐藏属性
                var dirInfo = new DirectoryInfo(filePath);
                if (dirInfo.Exists && (dirInfo.Attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden)
                {
                    return true;
                }
            }
            catch
            {
                // 如果出错，默认不是隐藏文件
                return false;
            }

            return false;
        }

        public bool IsExcludedFile(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);

                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                // 定义排除的文件/文件夹名称列表
                string[] excludedNames = 
                {
                    "node_modules",
                    "_build",
                    "obj",
                    "bin",
                    ".vs",
                    ".git",
                    ".vscode",
                    "dist",
                    "build",
                    "out",
                    "target",
                    ".idea",
                    "packages",
                    ".nuget",
                    "Debug",
                    "Release",
                    "x64",
                    "x86",
                    "ARM64",
                    ".angular",
                    ".next",
                    "coverage",
                    ".pytest_cache",
                    "__pycache__",
                    "vendor"
                };

                // 不区分大小写比较
                return excludedNames.Any(excluded => 
                    fileName.Equals(excluded, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }


        public async Task<List<GeFileInfo2>> LoadFilesAsync(string imageDir, bool showHiddenFiles = false, bool showExcludedFiles = true)
        {
            var list = new List<GeFileInfo2>();

            try
            {
                // Get all directories first
                string[] directories = Directory.GetDirectories(imageDir);
                foreach (string directory in directories)
                {
                    // 如果不显示隐藏文件，且当前是隐藏文件，则跳过
                    if (!showHiddenFiles && IsHiddenFile(directory))
                    {
                        continue;
                    }

                    // 如果不显示排除文件，且当前是排除文件，则跳过
                    if (!showExcludedFiles && IsExcludedFile(directory))
                    {
                        continue;
                    }

                    list.Add(new GeFileInfo2(directory, true));
                    Console.WriteLine($"Found directory: {directory}");
                }

                // Then get all files in the directory
                string[] files = Directory.GetFiles(imageDir);
                foreach (string file in files)
                {
                    // 如果不显示隐藏文件，且当前是隐藏文件，则跳过
                    if (!showHiddenFiles && IsHiddenFile(file))
                    {
                        continue;
                    }

                    // 如果不显示排除文件，且当前是排除文件，则跳过
                    if (!showExcludedFiles && IsExcludedFile(file))
                    {
                        continue;
                    }

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