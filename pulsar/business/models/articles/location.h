#pragma once

#include "quark/build.h"
#include <chrono>
#include <string>

namespace pulsar {
class CXAPI PSLocationModel {
public:
  std::string URN;
  std::string Title;
  std::string Keywords;
  std::string Description;
  std::string Image;
  std::string Name;
  std::string Path;
  std::chrono::system_clock::time_point CreateTime;
  std::chrono::system_clock::time_point UpdateTime;
};
} // namespace pulsar
