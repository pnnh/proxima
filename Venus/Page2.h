#pragma once

#include "Page2.g.h"

namespace winrt::Venus::implementation
{
    struct Page2 : Page2T<Page2>
    {
        Page2();

        int32_t MyProperty();
        void MyProperty(int32_t value);

        void ClickHandler(winrt::Windows::Foundation::IInspectable const& sender, Microsoft::UI::Xaml::RoutedEventArgs const& args);
            void HyperlinkButton_Click(winrt::Windows::Foundation::IInspectable const& sender, winrt::Microsoft::UI::Xaml::RoutedEventArgs const& e);

    };
}

namespace winrt::Venus::factory_implementation
{
    struct Page2 : Page2T<Page2, implementation::Page2>
    {
    };
}
