
#include "quark/core/types/datetime.h"
#include <chrono>
#include <string>

namespace pulsar {
class PSChannelModel {
public:
  explicit PSChannelModel();
  explicit PSChannelModel(const std::string &name);

  std::string URN;
  std::string Name;
  std::string Title;
  std::string Keywords;
  std::string Description;
  std::string Image;
  std::string Path;
  quark::PSDatetime CreateTime;
  quark::PSDatetime UpdateTime;
};
} // namespace pulsar
