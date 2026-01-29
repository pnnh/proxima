#pragma once

#include <string>

namespace kepler
{
    class Logger
    {
    public:
        static void LogInfo(const std::string& message);
    };
}
