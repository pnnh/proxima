
#include "library.h"
#include "quark/core/string/string.h"
#include "quark/infra/utils/md5.h"
#include "quark/services/filesystem/filesystem.h"
#include "quark/services/yaml/yaml.h"
#include <filesystem>
#include <string>

pulsar::LibraryServerBusiness::LibraryServerBusiness(
    const std::string &baseUrl) {
  this->baseUrl = baseUrl;
}

std::shared_ptr<std::vector<pulsar::PSLocationModel>>
pulsar::LibraryServerBusiness::selectLibraries() const {
  auto libraries = std::make_shared<std::vector<pulsar::PSLocationModel>>();

  for (const auto &entry : std::filesystem::directory_iterator(this->baseUrl)) {
    auto dirName = entry.path().filename();
    if (entry.path() == "." || entry.path() == ".." || !entry.is_directory()) {
      continue;
    }

    auto filePath = dirName.string();
    if (!isLibraryDirectory(filePath)) {
      continue;
    }
    auto libraryModel = pulsar::PSLocationModel(filePath);
    auto metadataFilePath =
        quark::JoinFilePath({this->baseUrl, filePath, "metadata.yaml"});
    if (quark::IsFileExist(metadataFilePath)) {
      auto yamlHandler = quark::YamlHandler(metadataFilePath);
      libraryModel.URN = yamlHandler.getString("metadata.urn");
      libraryModel.Title = yamlHandler.getString("metadata.title");
      libraryModel.Description = yamlHandler.getString("metadata.description");
      libraryModel.Image = yamlHandler.getString("metadata.image");
    }
    if (libraryModel.URN.empty()) {
      libraryModel.URN = quark::calcMd5(entry.path().string());
    }
    libraries->emplace_back(libraryModel);
  }

  return libraries;
}

bool pulsar::isLibraryDirectory(const std::string &directoryName) {
  return quark::MTString::EndsWith(directoryName, ".notelibrary");
}
