#pragma once

#include <string>
#include <build.h>

#ifdef __cplusplus

namespace quark
{
    class MTAPI MTMime
    {
    public:
        static bool isImage(const std::string& path);
        static std::string getMimeType(const std::string& path);
    };
}

extern "C" {
#endif


#ifdef __cplusplus
}
#endif
