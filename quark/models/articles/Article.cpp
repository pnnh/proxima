#include "Article.h"

quark::PSArticleModel::PSArticleModel() = default;

quark::PSArticleModel::PSArticleModel(const std::string& title): Title(title)
{
}

quark::PSArticleModel::PSArticleModel(const PSArticleModel& other)
= default;
