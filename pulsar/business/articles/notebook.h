#pragma once

#include <pulsar/business/models/articles/Notebook.h>
#include <vector>

namespace quark {
bool isNotebookDirectory(const std::string &directoryName);

class NotebookServerBusiness {
public:
  explicit NotebookServerBusiness(const std::string &baseUrl);

  [[nodiscard]] std::shared_ptr<std::vector<pulsar::PSNotebookModel>>
  selectNotebooks() const;

private:
  std::string baseUrl;
};
} // namespace quark
