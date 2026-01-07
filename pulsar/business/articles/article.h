#pragma once

#include "pulsar/business/models/articles/article.h"
#include <memory>
#include <vector>

namespace pulsar {
bool isArticleDirectory(const std::string &directoryName);

class ArticleFileService {
public:
  explicit ArticleFileService(const std::string &baseUrl);

  [[nodiscard]] std::shared_ptr<std::vector<PSArticleModel>>
  scanArticles(const std::string &chanURN, const std::string &chanPath) const;
  [[nodiscard]] PSArticleModel ParseArticle(const std::string &chanURN,
                                            const std::string &fullPath) const;

private:
  std::string baseUrl;
};

class ArticleSqliteService {
public:
  explicit ArticleSqliteService(std::string dbPath);

  [[nodiscard]] std::shared_ptr<std::vector<PSArticleModel>>
  selectArticles(const std::string &chanURN) const;
  [[nodiscard]] std::shared_ptr<PSArticleModel>
  getArticle(const std::string &noteURN) const;

private:
  std::string dbPath;
};
} // namespace pulsar
