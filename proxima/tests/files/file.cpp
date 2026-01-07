
#include "file.h"
#include "quark/services/filesystem/filesystem.h"
#include <vector>
#include "quark/services/logger/logger.h"

#include <string>

int quark::tests::TestSelectFiles() {
  const std::string baseUrl = quark::JoinFilePath(
    {"calieo", "telescope", "tests", "data"});

  auto selectResult = quark::filesystem::MTFileServerBusiness::selectFilesVector(baseUrl);
  if (!selectResult.has_value()) {
    std::cerr << "selectFilesVector error: " << selectResult.error() << std::endl;
    return -1;
  }
  for (const auto &model: selectResult.value()) {
    quark::MTLogInfo({model.URN, model.Title});
  }
  // auto filesPtr2 = fileServer->selectFilesVector("CPlus.chan/assets");
  // for (const auto &model : filesPtr2) {
  //   //quark::Logger::LogInfo({model.URN, model.Title});
  // }
  return 0;
}
