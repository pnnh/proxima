#pragma once

#include "SvgConverter.g.h"

namespace winrt::CppWinRTComponent::implementation
{
    struct SvgConverter : SvgConverterT<SvgConverter>
    {
        SvgConverter() = default;

        static winrt::Windows::Foundation::IAsyncOperation<winrt::hstring> RenderSvgToPngAsync(
            winrt::hstring const& svgFilePath, uint32_t width, uint32_t height);
    };
}

namespace winrt::CppWinRTComponent::factory_implementation
{
    struct SvgConverter : SvgConverterT<SvgConverter, implementation::SvgConverter>
    {
    };
}
