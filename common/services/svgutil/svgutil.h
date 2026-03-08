#pragma once

#include <string>

#include "gliese/gliese.h"

namespace proxima::common {
    GEAPI const int SEResultOk = 0;
    GEAPI const int SEResultError = 1;
    GEAPI int MTSvgToPng(const std::string &srcFilePath, const std::string &destFilePath);
}

