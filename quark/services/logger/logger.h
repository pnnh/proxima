#pragma once

#include <string>
#include "build.h"

namespace quark
{
    class MTAPI Logger
    {
    public:
        static void LogInfo(const std::string& message);
        static void LogInfo(std::initializer_list<std::string> messageList);
    };
}
