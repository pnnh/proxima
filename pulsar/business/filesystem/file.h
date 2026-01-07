#pragma once

#include "quark/build.h"
#include "quark/infra/result/result.h"

#include <expected>

#ifdef __cplusplus

extern "C" {
#endif

CXAPI bool IsFileExist(const char *filePath);

#ifdef __cplusplus
}

#include "pulsar/business/models/files/file.h"
#include <vector>

namespace pulsar {
class CXAPI FileServerBusiness {
public:
  struct SelectFilesOptions {
    SelectFilesOptions();

    bool directories;
    bool files;
    bool hidden;
    bool ignore;
  };

  [[nodiscard]] static std::expected<std::vector<pulsar::PSFileModel>,
                                     quark::MTCode>
  selectFilesVector(std::string parentPath,
                    SelectFilesOptions options = SelectFilesOptions());
};
} // namespace pulsar

#endif
