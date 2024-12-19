#pragma once

#include <string>
#include <chrono>
#include <types/datetime.h>
#include <build.h>

namespace quark
{
    class MTAPI PSArticleModel
    {
    public:
        PSArticleModel();
        explicit PSArticleModel(const std::string& title);

        PSArticleModel(const PSArticleModel& other);

        std::string URN;
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
}
