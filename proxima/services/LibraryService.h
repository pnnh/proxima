#pragma once

#include <QVector>

#include "quark/business/models/articles/Library.h"
#include "quark/business/models/articles/Notebook.h"

class LibraryService
{
public:
  LibraryService();


  std::optional<quark::PSLibraryModel> FindLibrary(const QString& uid) const;
  QVector<quark::PSLibraryModel> SelectLibraries() const;
  static QVector<quark::PSNotebookModel> SelectPartitions(
    const quark::PSLibraryModel& libraryModel);
  void InsertOrUpdateLibrary(const QVector<quark::PSLibraryModel>& libraryList);

private:
  QString dbPath;
};
