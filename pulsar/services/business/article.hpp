#pragma once

#include <optional>
#include <pqxx/pqxx>
#include <pulsar/business/articles/article.h>
#include <vector>

namespace pulsar {

std::vector<pulsar::PSArticleModel> selectArticles();
std::optional<pulsar::PSArticleModel> queryArticle(std::string pk);

class MessageService {
public:
  MessageService();

  std::optional<std::vector<pulsar::PSArticleModel>> selectMessages(int limit);

  int insertMessage(const pulsar::PSArticleModel &model);

  int updateMessage(const pulsar::PSArticleModel &model);

  int deleteMessage(const std::string &pk);

  std::optional<pulsar::PSArticleModel>
  findMessage(const std::optional<std::string> &uid,
              const std::optional<long> &nid);

  long count();

private:
  pqxx::connection connection;
};

} // namespace pulsar