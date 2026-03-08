#pragma once

#include <string>

#include "gliese/gliese.h"

namespace proxima::common {
    const int SEResultOk = 0;
    const int SEResultError = 1;
    GEAPI int MTSvgToPng(const std::string &srcFilePath, const std::string &destFilePath);
}

