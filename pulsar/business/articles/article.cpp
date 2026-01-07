#include "article.h"

#include "quark/core/string/string.h"
#include "quark/infra/utils/md5.h"
#include "quark/services/database/sqlite_service.h"
#include "quark/services/filesystem/filesystem.h"
#include "quark/services/yaml/yaml.h"
#include <filesystem>
#include <quark/services/logger/logger.h>
#include <string>
#include <utility>

pulsar::ArticleFileService::ArticleFileService(const std::string &baseUrl) {
  this->baseUrl = baseUrl;
}

pulsar::PSArticleModel
pulsar::ArticleFileService::ParseArticle(const std::string &chanURN,
                                         const std::string &fullPath) const {
  auto articleModel = pulsar::PSArticleModel("");
  auto metadataFilePath = quark::JoinFilePath({fullPath, "metadata.yaml"});
  if (quark::IsFileExist(metadataFilePath)) {
    auto yamlHandler = quark::YamlHandler(metadataFilePath);
    articleModel.Uid =
        quark::MTString::toLower(yamlHandler.getString("metadata.urn"));
    articleModel.Title = yamlHandler.getString("metadata.title");
    articleModel.Description = yamlHandler.getString("metadata.description");
    articleModel.Cover = yamlHandler.getString("metadata.cover");
  }
  if (articleModel.Uid.empty()) {
    articleModel.Uid = quark::MTString::toLower(quark::calcMd5(fullPath));
  }
  if (articleModel.Title.empty()) {
    articleModel.Title = quark::PathFileName(fullPath);
  }
  articleModel.Channel = chanURN;
  articleModel.Path = fullPath;
  articleModel.UpdateTime = quark::fileLastModifyTime(fullPath);

  auto contentFilePath = quark::JoinFilePath({fullPath, "index.md"});
  if (IsFileExist(contentFilePath.c_str())) {
    articleModel.Body = quark::filesystem::ReadAllText(contentFilePath);
  }
  return articleModel;
}

std::shared_ptr<std::vector<pulsar::PSArticleModel>>
pulsar::ArticleFileService::scanArticles(const std::string &chanURN,
                                         const std::string &chanPath) const {
  auto libraries = std::make_shared<std::vector<PSArticleModel>>();

  auto chanFullPath = quark::JoinFilePath({this->baseUrl, chanPath});
  for (const auto &entry : std::filesystem::directory_iterator(chanFullPath)) {
    auto dirName = entry.path().filename();
    if (entry.path() == "." || entry.path() == ".." || !entry.is_directory()) {
      continue;
    }

    auto filePath = dirName.string();
    if (!isArticleDirectory(filePath)) {
      continue;
    }
    auto noteFullPath =
        quark::JoinFilePath({this->baseUrl, chanPath, filePath});
    auto articleModel = ParseArticle(chanURN, noteFullPath);

    libraries->emplace_back(articleModel);
  }

  return libraries;
}

bool pulsar::isArticleDirectory(const std::string &directoryName) {
  return quark::MTString::EndsWith(directoryName, ".note");
}

pulsar::ArticleSqliteService::ArticleSqliteService(std::string dbPath)
    : dbPath(std::move(dbPath)) {}

std::shared_ptr<std::vector<pulsar::PSArticleModel>>
pulsar::ArticleSqliteService::selectArticles(const std::string &chanURN) const {
  auto sqliteService = quark::MTSqliteService(this->dbPath);

  std::string sqlText = "SELECT * FROM articles ";
  if (!chanURN.empty()) {
    sqlText += " where channel=$chan ";
  }
  std::unique_ptr<quark::MTSqliteCommand> sqlCommand{
      sqliteService.createCommand(sqlText)};
  if (!chanURN.empty()) {
    sqlCommand->BindString("$chan", chanURN);
  }
  auto libraries = std::make_shared<std::vector<PSArticleModel>>();
  std::shared_ptr<quark::MTSqliteResult> sqlResult{sqlCommand->Run()};
  if (sqlResult == nullptr) {
    quark::MTLogInfo("sqlResult is empty");
    return nullptr;
  }
  auto rowCount = sqlResult->getRowCount();
  if (rowCount < 1) {
    quark::MTLogInfo("table is empty");
    return libraries;
  }

  for (auto rowIndex = 0; rowIndex < rowCount; ++rowIndex) {
    auto row = sqlResult->getRow(rowIndex);
    auto model = PSArticleModel();
    model.Uid = row->getColumn("urn")->getStringValue();
    model.Title = row->getColumn("title")->getStringValue();
    model.Header = row->getColumn("header")->getStringValue();
    model.Body = row->getColumn("body")->getStringValue();
    model.Keywords = row->getColumn("keywords")->getStringValue();
    model.Description = row->getColumn("description")->getStringValue();
    model.Channel = row->getColumn("channel")->getStringValue();
    model.Cover = row->getColumn("cover")->getStringValue();

    libraries->emplace_back(model);
  }

  return libraries;
}

std::shared_ptr<pulsar::PSArticleModel>
pulsar::ArticleSqliteService::getArticle(const std::string &noteURN) const {
  auto sqliteService = quark::MTSqliteService(this->dbPath);

  std::string sqlText = "SELECT * FROM articles where urn=$urn ";
  std::unique_ptr<quark::MTSqliteCommand> sqlCommand{
      sqliteService.createCommand(sqlText)};
  sqlCommand->BindString("$urn", noteURN);
  std::shared_ptr<quark::MTSqliteResult> sqlResult{sqlCommand->Run()};
  if (sqlResult == nullptr) {
    quark::MTLogInfo("sqlResult is empty");
    return nullptr;
  }
  auto rowCount = sqlResult->getRowCount();
  if (rowCount < 1) {
    quark::MTLogInfo("table is empty");
    return nullptr;
  }

  auto model = PSArticleModel();
  auto row = sqlResult->getRow(0);
  model.Uid = row->getColumn("urn")->getStringValue();
  model.Title = row->getColumn("title")->getStringValue();

  return std::make_shared<PSArticleModel>(model);
}
