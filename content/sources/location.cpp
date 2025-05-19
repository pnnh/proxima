#include "location.h"

#include <quark/services/filesystem/filesystem.h>

#include "quark/business/filesystem/file.h"

LocationViewModel::LocationViewModel(QObject *parent)
  : QAbstractListModel(parent) {
  int role = Qt::UserRole;
  dataNames.insert(role++, "uid");
  dataNames.insert(role++, "name");
  dataNames.insert(role++, "path");
  m_parentPath = QString::fromStdString(quark::UserHomeDirectory());

  loadData();
}

LocationViewModel::~LocationViewModel() = default;

bool LocationViewModel::directories() const { return this->m_directories; }

void LocationViewModel::setDirectories(const bool &directories) {
  this->m_directories = directories;
  loadData();
}

bool LocationViewModel::files() const { return this->m_files; }

void LocationViewModel::setFiles(const bool &files) {
  this->m_files = files;
  loadData();
}

QString LocationViewModel::parentPath() const { return this->m_parentPath; }

void LocationViewModel::setParentPath(const QString &parentPath) {
  this->m_parentPath = parentPath;
  loadData();
}

void LocationViewModel::appendDirectory(QVariant qmlVar) {
  std::cerr << "appendDirectory" << std::endl;
  QVariantMap map = qmlVar.toMap();
  auto newPath = map["path"].toString();
  std::cerr << "value: " << newPath.toStdString() << std::endl;
}

void LocationViewModel::loadData() {
  qDebug() << "loadData";
  if (m_parentPath.isEmpty()) {
    return;
  }
  auto options = quark::FileServerBusiness::SelectFilesOptions();
  options.directories = this->m_directories;
  options.files = this->m_files;

  auto selectResult = quark::FileServerBusiness::selectFilesVector(
      this->m_parentPath.toStdString(), options);
  if (!selectResult.has_value()) {
    std::cout << "loadData error: " << selectResult.error();
    return;
  }

  beginResetModel();
  dataList.clear();
  auto fileList = selectResult.value();

  for (auto &model : fileList) {
    auto dataPtr = FileViewData();

    QString uid = QString::fromStdString(model.URN);
    QString name = QString::fromStdString(model.Name);
    QString path = QString::fromStdString(model.Path);

    dataPtr.append(uid);
    dataPtr.append(name);
    dataPtr.append(path);
    dataList.append(dataPtr);
  }
  endResetModel();
}

int LocationViewModel::rowCount(const QModelIndex &parent) const {
  auto size = dataList.size();
  return static_cast<int>(size);
}

QVariant LocationViewModel::data(const QModelIndex &index, int role) const {
  FileViewData dataPtr = dataList[index.row()];
  const auto &value = dataPtr.at(role - Qt::UserRole);
  return value;
}

QHash<int, QByteArray> LocationViewModel::roleNames() const {
  return dataNames;
}
