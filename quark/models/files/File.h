#pragma once
#include <string>
#include <chrono>
#include "types/datetime.h"
#include "build.h"

#ifdef __cplusplus

namespace quark
{
    class MTAPI PSFileModel
    {
    public:
        explicit PSFileModel(std::string title);

        std::string URN;
        std::string Title;
        std::string Name;
        std::string Keywords;
        std::string Description;
        bool IsDir{};
        bool IsHidden{};
        bool IsIgnore{};
        PSDatetime CreateTime;
        PSDatetime UpdateTime;
    };
}

extern "C" {
#endif

    struct PSFileStruct
    {
        char* URN{};
        char* Title{};
        char* Name{};
        char* Keywords{};
        char* Description{};
        bool IsDir{};
        bool IsHidden{};
        bool IsIgnore{};
    };

    PSFileStruct* NewPSFileStruct();
    void DeletePSFileStruct(PSFileStruct* file);
    MTAPI int list_file(int input);

#ifdef __cplusplus
}
#endif

