#pragma once

#include <string>

namespace gliese
{
    class Logger
    {
    public:
        static void LogInfo(const std::string& message);
    };
}
