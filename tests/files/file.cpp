
#include "file.h"
#include "business/filesystem/file.h"

#include "build.h"
#include "services/filesystem/filesystem.h"

int quark::examples::TestSelectFiles()
{
    const std::string baseUrl = quark::JoinFilePath({PROJECT_SOURCE_DIR, "calieo", "telescope", "tests", "data"});
    auto fileServer = std::make_shared<quark::FileServerBusiness>(baseUrl);
    auto filesPtr = fileServer->selectFiles();
    for (const auto& model : *filesPtr)
    {
        //quark::Logger::LogInfo({model.URN, model.Title});
    }
    auto filesPtr2 = fileServer->selectFiles("CPlus.chan/assets");
    auto size = filesPtr2->size();
    for (const auto& model : *filesPtr2)
    {
        //quark::Logger::LogInfo({model.URN, model.Title});
    }
    return 0;
}
