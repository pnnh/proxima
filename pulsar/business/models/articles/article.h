#pragma once

#include "quark/build.h"
#include "quark/core/types/datetime.h"
#include <chrono>
#include <string>

namespace pulsar {
class CXAPI PSArticleModel {
public:
  PSArticleModel();
  explicit PSArticleModel(const std::string &title);

  PSArticleModel(const PSArticleModel &other);

  std::string Uid;
  std::string Title;
  std::string Header;
  std::string Body;
  std::string Keywords;
  std::string Description;
  std::string Path;
  std::string Channel;
  std::string Cover;
  quark::PSDatetime CreateTime;
  quark::PSDatetime UpdateTime;
};
} // namespace pulsar
