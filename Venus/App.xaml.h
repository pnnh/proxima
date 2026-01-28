#pragma once

#include "App.xaml.g.h"

namespace winrt::Venus::implementation
{
    struct App : AppT<App>
    {
        App();

        void OnLaunched(Microsoft::UI::Xaml::LaunchActivatedEventArgs const&);
        void OnNavigationFailed(IInspectable const&, Microsoft::UI::Xaml::Navigation::NavigationFailedEventArgs const&);


    private:
        winrt::Microsoft::UI::Xaml::Window window{ nullptr };
    };
}
