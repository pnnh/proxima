#include "svgutil.h"
#include <lunasvg/lunasvg.h>
#include <iostream>
#include <filesystem>
#include <quark/core/string/string.h>

int proxima::common::MTSvgToPng(const std::string &srcFilePath, const std::string &destFilePath) {
    auto document = lunasvg::Document::loadFromFile(srcFilePath);
    if (document == nullptr) {
        std::cerr << "Failed to load SVG: " << srcFilePath << std::endl;
        return SEResultError;
    }
    // 先固定尺寸大小，后续可作为参数传入
    auto size = 128;
    auto bitmap = document->renderToBitmap(size, size);
    if (bitmap.isNull()) {
        std::cerr << "Failed to render SVG to bitmap" << std::endl;
        return SEResultError;
    }

    auto result = bitmap.writeToPng(destFilePath);
    if (!result) {
        std::cerr << "Failed to write bitmap to PNG" << std::endl;
        return SEResultError;
    }
    return SEResultOk;
}
