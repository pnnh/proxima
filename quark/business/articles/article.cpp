#include "article.h"

#include <string>
#include <filesystem>
#include <utility>
#include <services/database/SqliteService.h>
#include "types/Exception.h"
#include "services/filesystem/filesystem.h"
#include "services/yaml/yaml.h"
#include "utils/md5.h"
#include "types/String.h"

quark::ArticleFileService::ArticleFileService(const std::string& baseUrl)
{
    this->baseUrl = baseUrl;
}

quark::PSArticleModel quark::ArticleFileService::ParseArticle(
    const std::string& chanURN, const std::string& fullPath) const
{
    auto articleModel = quark::PSArticleModel("");
    auto metadataFilePath = quark::JoinFilePath({fullPath, "metadata.yaml"});
    if (quark::IsFileExist(metadataFilePath))
    {
        auto yamlHandler = quark::YamlHandler(metadataFilePath);
        articleModel.URN = quark::PSString::toLower(yamlHandler.getString("metadata.urn").value_or(""));
        articleModel.Title = yamlHandler.getString("metadata.title").value_or("");
        articleModel.Description = yamlHandler.getString("metadata.description").value_or("");
        articleModel.Cover = yamlHandler.getString("metadata.cover").value_or("");
    }
    if (articleModel.URN.empty())
    {
        articleModel.URN = quark::PSString::toLower(quark::calcMd5(fullPath));
    }
    if (articleModel.Title.empty())
    {
        articleModel.Title = quark::PathFileName(fullPath);
    }
    articleModel.Channel = chanURN;
    articleModel.Path = fullPath;
    articleModel.UpdateTime = quark::fileLastModifyTime(fullPath);


    auto contentFilePath = quark::JoinFilePath({fullPath, "index.md"});
    if (IsFileExist(contentFilePath))
    {
        articleModel.Body = quark::filesystem::ReadAllText(contentFilePath);
    }
    return articleModel;
}

std::shared_ptr<std::vector<quark::PSArticleModel>>
quark::ArticleFileService::scanArticles(const std::string& chanURN, const std::string& chanPath) const
{
    auto libraries = std::make_shared<std::vector<PSArticleModel>>();

    auto chanFullPath = quark::JoinFilePath({this->baseUrl, chanPath});
    for (const auto& entry : std::filesystem::directory_iterator(chanFullPath))
    {
        auto dirName = entry.path().filename();
        if (entry.path() == "." || entry.path() == ".." || !entry.is_directory())
        {
            continue;
        }

        auto filePath = dirName.string();
        if (!isArticleDirectory(filePath))
        {
            continue;
        }
        auto noteFullPath = quark::JoinFilePath({this->baseUrl, chanPath, filePath});
        auto articleModel = ParseArticle(chanURN, noteFullPath);

        libraries->emplace_back(articleModel);
    }

    return libraries;
}

bool quark::isArticleDirectory(const std::string& directoryName)
{
    return quark::PSString::EndsWith(directoryName, ".note");
}

quark::ArticleSqliteService::ArticleSqliteService(std::string dbPath): dbPath(std::move(dbPath))
{
}

std::shared_ptr<std::vector<quark::PSArticleModel>> quark::ArticleSqliteService::selectArticles(
    const std::string& chanURN) const
{
    auto sqliteService = quark::SqliteService(this->dbPath);

    std::string sqlText = "SELECT * FROM articles ";
    auto sqlCommand = sqliteService.createCommand(sqlText);
    if (!chanURN.empty())
    {
        sqlText += " where channel=$chan ";
        sqlCommand->ChangeSqlText(sqlText);
        sqlCommand->BindString("$chan", chanURN);
    }
    auto libraries = std::make_shared<std::vector<PSArticleModel>>();
    auto sqlResult = sqlCommand->Run();
    if (sqlResult == nullptr)
    {
        std::cout << "sqlResult is empty" << std::endl;
        return nullptr;
    }
    auto rowCount = sqlResult->getRowCount();
    if (rowCount < 1)
    {
        std::cout << "table is empty" << std::endl;
        return libraries;
    }

    for (auto rowIndex = 0; rowIndex < rowCount; ++rowIndex)
    {
        auto model = PSArticleModel();
        model.URN = sqlResult->getColumn(rowIndex, "urn").value().getStringValue();
        model.Title = sqlResult->getColumn(rowIndex, "title").value().getStringValue();
        model.Header = sqlResult->getColumn(rowIndex, "header").value().getStringValue();
        model.Body = sqlResult->getColumn(rowIndex, "body").value().getStringValue();
        model.Keywords = sqlResult->getColumn(rowIndex, "keywords").value().getStringValue();
        model.Description = sqlResult->getColumn(rowIndex, "description").value().getStringValue();
        model.Channel = sqlResult->getColumn(rowIndex, "channel").value().getStringValue();
        model.Cover = sqlResult->getColumn(rowIndex, "cover").value().getStringValue();

        libraries->emplace_back(model);
    }

    return libraries;
}

std::shared_ptr<quark::PSArticleModel> quark::ArticleSqliteService::getArticle(
    const std::string& noteURN) const
{
    auto sqliteService = quark::SqliteService(this->dbPath);

    std::string sqlText = "SELECT * FROM articles where urn=$urn ";
    auto sqlCommand = sqliteService.createCommand(sqlText);
    sqlCommand->BindString("$urn", noteURN);
    auto sqlResult = sqlCommand->Run();
    if (sqlResult == nullptr)
    {
        std::cout << "sqlResult is empty" << std::endl;
        return nullptr;
    }
    auto rowCount = sqlResult->getRowCount();
    if (rowCount < 1)
    {
        std::cout << "table is empty" << std::endl;
        return nullptr;
    }

    auto model = PSArticleModel();
    model.URN = sqlResult->getColumn(0, "urn").value().getStringValue();
    model.Title = sqlResult->getColumn(0, "title").value().getStringValue();

    return std::make_shared<PSArticleModel>(model);
}


