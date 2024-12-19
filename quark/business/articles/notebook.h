#pragma once

#include <vector>
#include "models/articles/Notebook.h"

namespace quark
{
    bool isNotebookDirectory(const std::string& directoryName);

    class NotebookServerBusiness
    {
    public:
        explicit NotebookServerBusiness(const std::string& baseUrl);

        [[nodiscard]] std::shared_ptr<std::vector<quark::PSNotebookModel>> selectNotebooks() const;

    private:
        std::string baseUrl;
    };
}
