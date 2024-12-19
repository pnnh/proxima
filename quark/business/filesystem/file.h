#pragma once
#include <vector>
#include <memory>
#include <string>
#include "models/files/File.h"
#include "build.h"

namespace quark
{
    class MTAPI FileServerBusiness
    {
    public:
        explicit FileServerBusiness(const std::string& baseUrl);

        [[nodiscard]] std::shared_ptr<std::vector<PSFileModel>> selectFiles() const;
        [[nodiscard]] std::shared_ptr<std::vector<PSFileModel>> selectFiles(std::string parentPath) const;
        [[nodiscard]] std::vector<PSFileModel> selectFilesVector() const;
        [[nodiscard]] std::vector<PSFileModel> selectFilesVector(std::string parentPath) const;

    private:
        std::string baseUrl;
    };
}
