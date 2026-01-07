#include "article.h"

pulsar::PSArticleModel::PSArticleModel() = default;

pulsar::PSArticleModel::PSArticleModel(const std::string &title)
    : Title(title) {}

pulsar::PSArticleModel::PSArticleModel(const PSArticleModel &other) = default;
