using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using CsWinRTApp.Models;
using SixLabors.ImageSharp;


namespace CsWinRTApp.Services
{
    public sealed partial class FileService : Window
    {
        public FileService()
        {

        }

        // 生成基于文件路径的唯一PNG文件名
        private string GetUniquePngFileName(string originalWebpPath)
        {
            // 使用SHA256哈希确保文件名唯一
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(originalWebpPath));
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 16);

            // 保留原始文件名前缀以便识别
            var originalName = Path.GetFileNameWithoutExtension(originalWebpPath);
            if (originalName.Length > 20)
                originalName = originalName.Substring(0, 20);

            return $"{originalName}_{hashString}.png";
        }

        public bool IsImageFile(string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".awebp" };
            string extension = Path.GetExtension(filePath);
            return imageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsSvgFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return extension.Equals(".svg", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsHtmlFile(string filePath)
        {
            string[] htmlExtensions = { ".html", ".htm" };
            string extension = Path.GetExtension(filePath);
            return htmlExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
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


        public async Task<List<GeFileInfo2>> LoadFilesAsync(string imageDir, DispatcherQueue dispatcherQueue, bool showHiddenFiles = false, bool showExcludedFiles = true)
        {
            var list = new List<GeFileInfo2>();
            string tempPngDir = Path.Combine(Path.GetTempPath(), "CsWinRTApp", "WebpToPng");
            Directory.CreateDirectory(tempPngDir);
            var webpConvertTasks = new List<(GeFileInfo2, string, string)>();

            try
            {
                // Get all directories first
                string[] directories = Directory.GetDirectories(imageDir);
                foreach (string directory in directories)
                {
                    if (!showHiddenFiles && IsHiddenFile(directory))
                        continue;
                    if (!showExcludedFiles && IsExcludedFile(directory))
                        continue;
                    list.Add(new GeFileInfo2(directory, true));
                }

                // Then get all files in the directory
                string[] files = Directory.GetFiles(imageDir);
                foreach (string file in files)
                {
                    if (!showHiddenFiles && IsHiddenFile(file))
                        continue;
                    if (!showExcludedFiles && IsExcludedFile(file))
                        continue;

                    string ext = Path.GetExtension(file).ToLowerInvariant();
                    if (ext == ".webp" || ext == ".awebp")
                    {
                        // 使用哈希生成唯一的PNG文件名，避免同名冲突
                        string pngName = GetUniquePngFileName(file);
                        string pngPath = Path.Combine(tempPngDir, pngName);
                        if (!File.Exists(pngPath))
                        {
                            var info = new GeFileInfo2(file, false, true); // pending
                            list.Add(info);
                            webpConvertTasks.Add((info, file, pngPath));
                        }
                        else
                        {
                            // PNG已存在，直接使用
                            list.Add(new GeFileInfo2(pngPath, false));
                        }
                    }
                    else if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".gif")
                    {
                        list.Add(new GeFileInfo2(file, false));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading files: {ex.Message}");
            }

            // 异步后台批量转换webp - 每张图片转换完成后立即更新UI
            if (webpConvertTasks.Count > 0 && dispatcherQueue != null)
            {
                _ = Task.Run(async () =>
                {
                    int total = webpConvertTasks.Count;
                    int completed = 0;

                    foreach (var (info, webp, png) in webpConvertTasks)
                    {
                        try
                        {
                            using var image = await SixLabors.ImageSharp.Image.LoadAsync<SixLabors.ImageSharp.PixelFormats.Rgba32>(webp);
                            await image.SaveAsPngAsync(png);

                            completed++;

                            // 显式调度到UI线程，确保每张图片转换完成后立即更新
                            bool dispatched = dispatcherQueue.TryEnqueue(() =>
                            {
                                info.FilePath = png;
                                info.IsWebpPending = false;
                                System.Diagnostics.Debug.WriteLine($"[FileService] WebP converted ({completed}/{total}): {Path.GetFileName(webp)} => {Path.GetFileName(png)}");
                            });

                            if (!dispatched)
                            {
                                System.Diagnostics.Debug.WriteLine($"[FileService] Failed to dispatch UI update for: {Path.GetFileName(webp)}");
                            }

                            // 短暂延迟以确保UI有时间刷新
                            await Task.Delay(10);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[FileService] Failed to convert webp: {webp} => {ex.Message}");
                            LogService.Error($"WebP conversion failed: {webp}", ex);
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"[FileService] All WebP conversions completed: {completed}/{total}");
                });
            }

            return list;
        }

    }
}