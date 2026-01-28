#include "pch.h"
#include "MainPage.h"


#if __has_include("MainPage.g.cpp")
#include "MainPage.g.cpp"
#endif

using namespace winrt;

using namespace Microsoft::UI::Xaml;
using namespace Microsoft::UI::Xaml::Controls;

namespace winrt::Venus::implementation
{
    MainPage::MainPage(): MainPageT<MainPage>()
    {
	    
    }

    int32_t MainPage::MyProperty()
    {
        throw hresult_not_implemented();
    }

    void MainPage::MyProperty(int32_t /* value */)
    {
        throw hresult_not_implemented();
    }

    void MainPage::ClickHandler(winrt::Windows::Foundation::IInspectable const& sender, Microsoft::UI::Xaml::RoutedEventArgs const& args)
    {
	    Microsoft::UI::Xaml::Controls::Button().Content(box_value(L"Clicked"));
    }

    void winrt::Venus::implementation::MainPage::HyperlinkButton_Click(winrt::Windows::Foundation::IInspectable const& sender, winrt::Microsoft::UI::Xaml::RoutedEventArgs const& e)
    {
        Frame().Navigate(winrt::xaml_typename<Venus::Page2>());
    }
}
