
#include "library.h"
#include <string>
#include <filesystem>
#include "types/String.h"

#include "types/Exception.h"
#include "services/filesystem/filesystem.h"
#include "services/logger/logger.h"
#include "services/yaml/yaml.h"
#include "utils/md5.h"

quark::LibraryServerBusiness::LibraryServerBusiness(const std::string& baseUrl)
{
    this->baseUrl = baseUrl;
}

std::shared_ptr<std::vector<quark::PSLibraryModel>>
quark::LibraryServerBusiness::selectLibraries() const
{
    auto libraries = std::make_shared<std::vector<quark::PSLibraryModel>>();

    for (const auto& entry : std::filesystem::directory_iterator(this->baseUrl))
    {
        auto dirName = entry.path().filename();
        if (entry.path() == "." || entry.path() == ".." || !entry.is_directory())
        {
            continue;
        }

        auto filePath = dirName.string();
        if (!isLibraryDirectory(filePath))
        {
            continue;
        }
        auto libraryModel = quark::PSLibraryModel(filePath);
        auto metadataFilePath = quark::JoinFilePath({this->baseUrl, filePath, "metadata.yaml"});
        if (quark::IsFileExist(metadataFilePath))
        {
            auto yamlHandler = quark::YamlHandler(metadataFilePath);
            libraryModel.URN = yamlHandler.getString("metadata.urn").value_or("");
            libraryModel.Title = yamlHandler.getString("metadata.title").value_or(filePath);
            libraryModel.Description = yamlHandler.getString("metadata.description").value_or("");
            libraryModel.Image = yamlHandler.getString("metadata.image").value_or("");
        }
        if (libraryModel.URN.empty())
        {
            libraryModel.URN = quark::calcMd5(entry.path().string());
        }
        libraries->emplace_back(libraryModel);
    }

    return libraries;
}

bool quark::isLibraryDirectory(const std::string& directoryName)
{
    return quark::PSString::EndsWith(directoryName, ".notelibrary");
}
