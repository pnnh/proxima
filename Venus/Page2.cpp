#include "pch.h"
#include "Page2.h"

#include <winrt/windows.ui.xaml.interop.h>

#if __has_include("Page2.g.cpp")
#include "Page2.g.cpp"
#endif

using namespace winrt;

namespace winrt::Venus::implementation
{

	Page2::Page2():Page2T<Page2>()
	{

	}

    int32_t Page2::MyProperty()
    {
        throw hresult_not_implemented();
    }

    void Page2::MyProperty(int32_t /* value */)
    {
        throw hresult_not_implemented();
    }

    void Page2::ClickHandler(winrt::Windows::Foundation::IInspectable const&, Microsoft::UI::Xaml::RoutedEventArgs const&)
    {
        Microsoft::UI::Xaml::Controls::Button().Content(box_value(L"Clicked"));
    }

    void Page2::HyperlinkButton_Click(winrt::Windows::Foundation::IInspectable const& sender, winrt::Microsoft::UI::Xaml::RoutedEventArgs const& e)
    {
        Frame().Navigate(winrt::xaml_typename<Venus::MainPage>());
    }
}
