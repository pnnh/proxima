#include "logger.h"

#include <iostream>

#ifdef WIN32
#include <hstring.h>
#include <tchar.h>
#endif

void gliese::Logger::LogInfo(const std::string& message)
{
#ifdef WIN32
    OutputDebugString(_T(message.c_str()));
#endif

    int a = 10;
    std::cout << "[INFO] " << message << a << std::endl;
}
