using Microsoft.UI.Xaml.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Security.Cryptography;

namespace CsWinRTApp.Services
{
    /// <summary>
    /// WebP 图像处理服务
    /// 需要安装 NuGet 包: SixLabors.ImageSharp
    /// </summary>
    public class WebPImageService
    {
        // 限制并发 WebP 解码操作数量，防止资源耗尽
        // 同时最多处理 1 个 WebP 文件，避免内存/线程池压力过大
        private static readonly SemaphoreSlim _decodeSemaphore = new SemaphoreSlim(1, 1);

        // 用于诊断并发问题
        private static int _activeDecodes = 0;

        // 跟踪总解码次数，用于内存管理
        private static int _totalDecodes = 0;

        // 文件大小限制（10MB），跳过过大的文件防止内存溢出
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;


        /// <summary>
        /// 从 WebP 文件加载为 BitmapImage(用于 WinUI 显示)
        /// </summary>
        public static async Task<BitmapImage> LoadWebPImageAsync(string filePath, int? maxWidth = null, int? maxHeight = null)
        {
            // 检查文件是否存在和大小
            try
            {
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebPImageService] File not found: {filePath}");
                    return null;
                }

                if (fileInfo.Length > MaxFileSizeBytes)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebPImageService] File too large ({fileInfo.Length / 1024 / 1024}MB), skipping: {Path.GetFileName(filePath)}");
                    LogService.Warning($"WebP file too large (>{MaxFileSizeBytes / 1024 / 1024}MB), skipped: {filePath}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WebPImageService] File check failed: {ex.Message}");
                return null;
            }

            // 获取信号量，限制并发解码数量
            await _decodeSemaphore.WaitAsync();

            try
            {
                var currentActive = Interlocked.Increment(ref _activeDecodes);
                var totalCount = Interlocked.Increment(ref _totalDecodes);

                System.Diagnostics.Debug.WriteLine($"[WebPImageService] Starting decode #{currentActive} (total: {totalCount}): {Path.GetFileName(filePath)}");

                // 每解码10个文件后强制GC，防止内存累积
                if (totalCount % 10 == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebPImageService] Forcing GC after {totalCount} decodes...");
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

                using (var image = await Image.LoadAsync<Rgba32>(filePath))
                {
                    // 如果指定了最大尺寸，进行缩放
                    if (maxWidth.HasValue || maxHeight.HasValue)
                    {
                        int targetWidth = maxWidth ?? image.Width;
                        int targetHeight = maxHeight ?? image.Height;

                        // 保持宽高比
                        if (maxWidth.HasValue && maxHeight.HasValue)
                        {
                            double ratio = Math.Min((double)maxWidth.Value / image.Width, 
                                                   (double)maxHeight.Value / image.Height);
                            targetWidth = (int)(image.Width * ratio);
                            targetHeight = (int)(image.Height * ratio);
                        }
                        else if (maxWidth.HasValue)
                        {
                            targetHeight = (int)(image.Height * ((double)maxWidth.Value / image.Width));
                        }
                        else
                        {
                            targetWidth = (int)(image.Width * ((double)maxHeight.Value / image.Height));
                        }

                        image.Mutate(x => x.Resize(targetWidth, targetHeight));
                    }

                    // 转换为 PNG 格式的内存流（WinUI 原生支持）
                    using (var memoryStream = new InMemoryRandomAccessStream())
                    {
                        // 将 ImageSharp 图像编码为 PNG
                        using (var ms = new MemoryStream())
                        {
                            await image.SaveAsPngAsync(ms);
                            ms.Position = 0;

                            // 复制到 InMemoryRandomAccessStream
                            await RandomAccessStream.CopyAsync(ms.AsInputStream(), memoryStream);
                            memoryStream.Seek(0);

                            // 创建 BitmapImage
                            var bitmapImage = new BitmapImage();
                            await bitmapImage.SetSourceAsync(memoryStream);

                            System.Diagnostics.Debug.WriteLine($"[WebPImageService] Completed decode #{currentActive}: {Path.GetFileName(filePath)}");

                            // 添加小延迟，让GC有机会清理
                            await Task.Delay(5);

                            return bitmapImage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WebPImageService] Failed to load WebP image '{filePath}': {ex.Message}");
                LogService.Error($"WebP decode failed: {filePath}", ex);
                return null;
            }
            finally
            {
                // 释放信号量，允许下一个解码操作
                var currentActive = Interlocked.Decrement(ref _activeDecodes);
                System.Diagnostics.Debug.WriteLine($"[WebPImageService] Active decodes: {currentActive}");
                _decodeSemaphore.Release();
            }
        }

        /// <summary>
        /// 从 WebP 文件加载缩略图
        /// </summary>
        public static async Task<BitmapImage> LoadWebPThumbnailAsync(string filePath, int thumbnailSize = 128)
        {
            return await LoadWebPImageAsync(filePath, thumbnailSize, thumbnailSize);
        }

        /// <summary>
        /// 检查文件是否为 WebP 格式（包括动画 WebP）
        /// </summary>
        public static bool IsWebPFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".webp" || extension == ".awebp";
        }

        /// <summary>
        /// 从 WebP 文件转换为 SoftwareBitmap（可用于更高级的图像处理）
        /// </summary>
        public static async Task<SoftwareBitmap> LoadWebPAsSoftwareBitmapAsync(string filePath)
        {
            try
            {
                using (var image = await Image.LoadAsync<Rgba32>(filePath))
                {
                    using (var memoryStream = new InMemoryRandomAccessStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            await image.SaveAsPngAsync(ms);
                            ms.Position = 0;
                            await RandomAccessStream.CopyAsync(ms.AsInputStream(), memoryStream);
                            memoryStream.Seek(0);

                            // 使用 BitmapDecoder 创建 SoftwareBitmap
                            var decoder = await BitmapDecoder.CreateAsync(memoryStream);
                            var softwareBitmap = await decoder.GetSoftwareBitmapAsync(
                                BitmapPixelFormat.Bgra8, 
                                BitmapAlphaMode.Premultiplied);
                            
                            return softwareBitmap;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load WebP as SoftwareBitmap: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取或生成WebP缩略图的磁盘缓存路径
        /// </summary>
        private static string GetThumbnailCachePath(string filePath, int size)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "CsWinRTApp", "Thumbnails");
            Directory.CreateDirectory(tempDir);
            // 用文件绝对路径+修改时间+尺寸做hash，防止重名和缓存失效
            string hashInput = filePath + File.GetLastWriteTimeUtc(filePath).Ticks + size;
            using var sha1 = SHA1.Create();
            var hash = BitConverter.ToString(sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hashInput))).Replace("-", "");
            return Path.Combine(tempDir, $"{hash}_{size}.png");
        }

        /// <summary>
        /// 获取或生成WebP缩略图的BitmapImage（带磁盘缓存）
        /// </summary>
        public static async Task<BitmapImage> GetOrCreateCachedThumbnailAsync(string filePath, int size = 128)
        {
            string cachePath = GetThumbnailCachePath(filePath, size);
            if (File.Exists(cachePath))
            {
                try
                {
                    // 直接从缓存PNG加载
                    using var stream = File.OpenRead(cachePath);
                    var memStream = new InMemoryRandomAccessStream();
                    await stream.CopyToAsync(memStream.AsStreamForWrite());
                    memStream.Seek(0);
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(memStream);
                    System.Diagnostics.Debug.WriteLine($"[WebPImageService] Loaded thumbnail from cache: {cachePath}");
                    return bitmap;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebPImageService] Failed to load cache, will regenerate: {ex.Message}");
                }
            }

            // 缓存不存在或损坏，重新生成
            var thumb = await LoadWebPImageAsync(filePath, size, size);
            if (thumb != null)
            {
                try
                {
                    // 重新解码后保存为PNG
                    using var image = await Image.LoadAsync<Rgba32>(filePath);
                    image.Mutate(x => x.Resize(new SixLabors.ImageSharp.Processing.ResizeOptions
                    {
                        Size = new SixLabors.ImageSharp.Size(size, size),
                        Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max
                    }));
                    using var ms = new MemoryStream();
                    await image.SaveAsPngAsync(ms);
                    ms.Position = 0;
                    File.WriteAllBytes(cachePath, ms.ToArray());
                    System.Diagnostics.Debug.WriteLine($"[WebPImageService] Saved thumbnail to cache: {cachePath}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebPImageService] Failed to save thumbnail cache: {ex.Message}");
                }
            }
            return thumb;
        }
    }
}
