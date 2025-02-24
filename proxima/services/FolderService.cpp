#include "FolderService.h"

#include <QDir>

FolderService::FolderService() = default;

QList<QString> FolderService::SelectDirectories(const QString &prefixPath) {
  QList<QString> directories;
  QDir dir(prefixPath);
  if (!dir.exists()) {
    return directories;
  }

  auto list = dir.entryList(QDir::Dirs | QDir::NoDotAndDotDot);
  for (const auto &subDir : list) {
    directories.append(subDir);
  }
  return directories;
}

auto FolderService::HasChildDirectory(const QString &prefixPath) -> bool {
  QDir dir(prefixPath);
  if (!dir.exists()) {
    return false;
  }

  auto list = dir.entryList(QDir::Dirs | QDir::NoDotAndDotDot);
  return !list.empty();
}
