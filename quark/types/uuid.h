#pragma once

#include <regex>
#include <string>
#include "build.h"

#ifdef __cplusplus

namespace quark {

    class MTAPI MTUuid
    {
    public:
        static bool isUUID(const std::string &uuid_string);

    private:
        static std::regex uuid_regex;
    };

}

extern "C" {
#endif


#ifdef __cplusplus
}
#endif
