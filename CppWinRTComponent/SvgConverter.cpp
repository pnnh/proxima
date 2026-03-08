#include "pch.h"
#include "SvgConverter.h"
#if __has_include("SvgConverter.g.cpp")
#include "SvgConverter.g.cpp"
#endif

#include <string>
#include <functional>
#include "common/services/svgutil/svgutil.h"

namespace winrt::CppWinRTComponent::implementation
{
    winrt::Windows::Foundation::IAsyncOperation<winrt::hstring> SvgConverter::RenderSvgToPngAsync(
        winrt::hstring const& svgFilePath, uint32_t width, uint32_t height)
    {
        // Capture parameters before switching to a background thread
        std::string filePath = winrt::to_string(svgFilePath);
        uint32_t w = width  > 0 ? width  : 128u;
        uint32_t h = height > 0 ? height : 128u;

        co_await winrt::resume_background();

        try
        {
            // Build a per-app cache directory inside %TEMP%
            char tempDirA[MAX_PATH];
            GetTempPathA(MAX_PATH, tempDirA);
            std::string cacheDir = std::string(tempDirA) + "CsWinRTApp\\Thumbnails\\";
            CreateDirectoryA(cacheDir.c_str(), nullptr); // no-op if already exists

            // Cache key == hash(filePath + dimensions) so different sizes don't collide
            std::string hashInput = filePath + "_" + std::to_string(w) + "x" + std::to_string(h);
            size_t hashVal = std::hash<std::string>{}(hashInput);
            std::string cachePath = cacheDir + "svg_" + std::to_string(hashVal) + ".png";

            // Return the cached PNG when it already exists
            DWORD attrs = GetFileAttributesA(cachePath.c_str());
            if (attrs != INVALID_FILE_ATTRIBUTES && !(attrs & FILE_ATTRIBUTE_DIRECTORY))
            {
                co_return winrt::to_hstring(cachePath);
            }

            auto retCode = proxima::common::MTSvgToPng(filePath, cachePath);
            if (retCode != 0) 
              co_return {};

            co_return winrt::to_hstring(cachePath);
        }
        catch (...)
        {
            co_return {};
        }
    }
}
