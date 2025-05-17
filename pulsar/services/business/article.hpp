#pragma once

#include "quark/business/models/articles/Article.h"

#include <optional>
#include <pqxx/pqxx>
#include <vector>

namespace pulsar {

std::vector<quark::PSArticleModel> selectArticles();
std::optional<quark::PSArticleModel> queryArticle(std::string pk);

class MessageService {
public:
  MessageService();

  std::optional<std::vector<quark::PSArticleModel>> selectMessages(int limit);

  int insertMessage(const quark::PSArticleModel &model);

  int updateMessage(const quark::PSArticleModel &model);

  int deleteMessage(const std::string &pk);

  std::optional<quark::PSArticleModel>
  findMessage(const std::optional<std::string> &uid,
              const std::optional<long> &nid);

  long count();

private:
  pqxx::connection connection;
};

} // namespace pulsar