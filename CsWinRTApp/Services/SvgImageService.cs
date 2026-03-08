using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using CppWinRTComponent;

namespace CsWinRTApp.Services
{
    /// <summary>
    /// SVG 图像处理服务
    /// 批量预览请通过 FileService.LoadFilesAsync 的后台预转换流程处理。
    /// 此服务仅用于单文件场景（如 PreviewPage）。
    /// </summary>
    public static class SvgImageService
    {
        /// <summary>
        /// 检查文件是否为 SVG 格式
        /// </summary>
        public static bool IsSvgFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            return Path.GetExtension(filePath).Equals(".svg", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 将 SVG 文件渲染为 BitmapImage（仅用于单文件预览，不应在批量列表中调用）。
        /// 内部通过 C++ lunasvg 组件渲染，结果缓存到磁盘。
        /// </summary>
        public static async Task<BitmapImage?> LoadSvgImageAsync(string filePath, int width = 512, int height = 512)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var pngPath = await SvgConverter.RenderSvgToPngAsync(filePath, (uint)width, (uint)height);
                if (string.IsNullOrEmpty(pngPath) || !File.Exists(pngPath))
                    return null;

                var storageFile = await StorageFile.GetFileFromPathAsync(pngPath);
                using var stream = await storageFile.OpenReadAsync();
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(stream);
                return bitmapImage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SvgImageService] Failed to render SVG '{filePath}': {ex.Message}");
                LogService.Error($"SVG render failed: {filePath}", ex);
                return null;
            }
        }
    }
}
