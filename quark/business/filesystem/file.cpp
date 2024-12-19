#include "file.h"
#include <string>
#include <filesystem>
#include <utility>
#include "services/filesystem/filesystem.h"
#include "types/datetime.h"
#include "utils/md5.h"
#include "types//String.h"

quark::FileServerBusiness::FileServerBusiness(const std::string& baseUrl)
{
    this->baseUrl = baseUrl;
}

std::shared_ptr<std::vector<quark::PSFileModel>>
quark::FileServerBusiness::selectFiles() const
{
    return selectFiles("");
}

std::shared_ptr<std::vector<quark::PSFileModel>>
quark::FileServerBusiness::selectFiles(std::string parentPath) const
{
    auto files = std::make_shared<std::vector<PSFileModel>>();

    const std::string fullPath = quark::JoinFilePath({this->baseUrl, std::move(parentPath)});

    for (const auto& entry : std::filesystem::directory_iterator(fullPath))
    {
        auto dirName = entry.path().filename();
        if (entry.path() == "." || entry.path() == "..")
        {
            continue;
        }

        auto filePath = dirName.string();
        auto fileModel = quark::PSFileModel(filePath);
        if (fileModel.URN.empty())
        {
            fileModel.URN = quark::calcMd5(entry.path().string());
        }
        fileModel.IsDir = entry.is_directory();
        fileModel.IsHidden = quark::isHidden(filePath);
        fileModel.IsIgnore = quark::isIgnore(filePath);
        fileModel.Title = filePath;
        fileModel.Name = filePath;

        fileModel.UpdateTime = quark::convertFilesystemTime(std::filesystem::last_write_time(entry));
        fileModel.CreateTime = fileModel.UpdateTime;
        files->emplace_back(fileModel);
    }

    return files;
}
std::vector<quark::PSFileModel> quark::FileServerBusiness::selectFilesVector() const
{
    return *this->selectFiles();
}

std::vector<quark::PSFileModel> quark::FileServerBusiness::selectFilesVector(std::string parentPath) const
{
    return *this->selectFiles(std::move(parentPath));
}

