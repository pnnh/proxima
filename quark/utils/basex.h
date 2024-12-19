#pragma once
#include <build.h>
#include <string>

namespace quark
{
    MTAPI std::string decode64(const std::string& val);
    MTAPI std::string encode64(const std::string& val);
}
