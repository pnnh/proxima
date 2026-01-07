#include "notebook.h"

#include "pulsar/business/models/articles/Notebook.h"
#include "quark/core/string/string.h"
#include "quark/infra/utils/md5.h"
#include "quark/services/filesystem/filesystem.h"
#include "quark/services/yaml/yaml.h"
#include <filesystem>
#include <string>

quark::NotebookServerBusiness::NotebookServerBusiness(
    const std::string &baseUrl) {
  this->baseUrl = baseUrl;
}

std::shared_ptr<std::vector<pulsar::PSNotebookModel>>
quark::NotebookServerBusiness::selectNotebooks() const {
  auto libraries = std::make_shared<std::vector<pulsar::PSNotebookModel>>();

  for (const auto &entry : std::filesystem::directory_iterator(this->baseUrl)) {
    auto dirName = entry.path().filename();
    if (entry.path() == "." || entry.path() == ".." || !entry.is_directory()) {
      continue;
    }

    auto filePath = dirName.string();
    if (!isNotebookDirectory(filePath)) {
      continue;
    }
    auto notebookModel = pulsar::PSNotebookModel(filePath);
    auto metadataFilePath =
        quark::JoinFilePath({this->baseUrl, filePath, "metadata.yaml"});
    if (quark::IsFileExist(metadataFilePath)) {
      auto yamlHandler = quark::YamlHandler(metadataFilePath);
      notebookModel.URN = yamlHandler.getString("metadata.urn");
      notebookModel.Title = yamlHandler.getString("metadata.title");
      notebookModel.Description = yamlHandler.getString("metadata.description");
      notebookModel.Image = yamlHandler.getString("metadata.image");
    }
    if (notebookModel.URN.empty()) {
      notebookModel.URN = quark::calcMd5(entry.path().string());
    }
    libraries->emplace_back(notebookModel);
  }

  return libraries;
}

bool quark::isNotebookDirectory(const std::string &directoryName) {
  return quark::MTString::EndsWith(directoryName, ".notebook");
}
