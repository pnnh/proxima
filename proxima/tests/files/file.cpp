
#include "file.h"
#include "quark/business/filesystem/file.h"

#include "quark/services/filesystem/filesystem.h"
#include "quark/services/logger/logger.h"

#include <string>

int quark::tests::TestSelectFiles() {
  const std::string baseUrl = quark::JoinFilePath(
      {"calieo", "telescope", "tests", "data"});

  auto filesPtr = FileServerBusiness::selectFilesVector(baseUrl);
  if (!filesPtr.has_value()) {
    std::cerr << "selectFilesVector error: " << filesPtr.error() << std::endl;
    return -1;
  }
  for (const auto &model : filesPtr.value()) {
    quark::MTLogInfo({model.URN, model.Title});
  }
  // auto filesPtr2 = fileServer->selectFilesVector("CPlus.chan/assets");
  // for (const auto &model : filesPtr2) {
  //   //quark::Logger::LogInfo({model.URN, model.Title});
  // }
  return 0;
}
