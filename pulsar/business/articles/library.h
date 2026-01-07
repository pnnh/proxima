#pragma once
#include <pulsar/business/models/articles/location.h>
#include <vector>

namespace pulsar {
bool isLibraryDirectory(const std::string &directoryName);

class LibraryServerBusiness {
public:
  explicit LibraryServerBusiness(const std::string &baseUrl);

  [[nodiscard]] std::shared_ptr<std::vector<pulsar::PSLocationModel>>
  selectLibraries() const;

private:
  std::string baseUrl;
};
} // namespace pulsar
