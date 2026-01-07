#pragma once
#include <vector>

#include "pulsar/business/models/articles/Channel.h"

namespace pulsar {
bool isChannelDirectory(const std::string &directoryName);

class ChannelServerBusiness {
public:
  explicit ChannelServerBusiness(const std::string &baseUrl);

  [[nodiscard]] std::shared_ptr<std::vector<pulsar::PSChannelModel>>
  selectChannels() const;

private:
  std::string baseUrl;
};
} // namespace pulsar
