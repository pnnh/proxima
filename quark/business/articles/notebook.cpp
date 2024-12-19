#include "notebook.h"

#include <string>
#include <filesystem>
#include "services/filesystem/filesystem.h"
#include "services/yaml/yaml.h"
#include "utils/md5.h"
#include "types/String.h"

quark::NotebookServerBusiness::NotebookServerBusiness(const std::string& baseUrl)
{
    this->baseUrl = baseUrl;
}

std::shared_ptr<std::vector<quark::PSNotebookModel>>
quark::NotebookServerBusiness::selectNotebooks() const
{
    auto libraries = std::make_shared<std::vector<quark::PSNotebookModel>>();

    for (const auto& entry : std::filesystem::directory_iterator(this->baseUrl))
    {
        auto dirName = entry.path().filename();
        if (entry.path() == "." || entry.path() == ".." || !entry.is_directory())
        {
            continue;
        }

        auto filePath = dirName.string();
        if (!isNotebookDirectory(filePath))
        {
            continue;
        }
        auto notebookModel = quark::PSNotebookModel(filePath);
        auto metadataFilePath = quark::JoinFilePath({this->baseUrl, filePath, "metadata.yaml"});
        if (quark::IsFileExist(metadataFilePath))
        {
            auto yamlHandler = quark::YamlHandler(metadataFilePath);
            notebookModel.URN = yamlHandler.getString("metadata.urn").value_or("");
            notebookModel.Title = yamlHandler.getString("metadata.title").value_or(filePath);
            notebookModel.Description = yamlHandler.getString("metadata.description").value_or("");
            notebookModel.Image = yamlHandler.getString("metadata.image").value_or("");
        }
        if (notebookModel.URN.empty())
        {
            notebookModel.URN = quark::calcMd5(entry.path().string());
        }
        libraries->emplace_back(notebookModel);
    }

    return libraries;
}

bool quark::isNotebookDirectory(const std::string& directoryName)
{
    return quark::PSString::EndsWith(directoryName, ".notebook");
}
