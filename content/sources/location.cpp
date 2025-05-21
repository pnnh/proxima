#include "location.h"

#include <quark/services/filesystem/filesystem.h>

#include "quark/business/filesystem/file.h"
#include "proxima/services/location_service.h"
#include "quark/infra/utils/md5.h"

LocationViewModel::LocationViewModel(QObject *parent)
  : QAbstractListModel(parent) {
  int role = Qt::UserRole;
  dataNames.insert(role++, "uid");
  dataNames.insert(role++, "name");
  dataNames.insert(role++, "path");

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

void LocationViewModel::appendDirectory(QVariant qmlVar) {
  std::cerr << "appendDirectory" << std::endl;
  QVariantMap map = qmlVar.toMap();
  auto newPath = map["path"].toString();
  std::cerr << "value: " << newPath.toStdString() << std::endl;

  std::filesystem::path stdPath = newPath.toStdString();
  auto locSvc = proxima::LocationService();
  quark::PSLocationModel model;
  model.URN = quark::calcMd5(newPath.toStdString(), true);
  model.Name = stdPath.filename();
  model.Path = newPath.toStdString();

  locSvc.InsertOrUpdateLocation(model);
}

void LocationViewModel::loadData() {
  qDebug() << "loadData";

  const auto locSvc = proxima::LocationService();

  auto selectResult = locSvc.SelectLocations();
  if (!selectResult.has_value()) {
    std::cout << "loadData error: " << selectResult.error();
    return;
  }
  auto selectData = selectResult.value();
  if (selectData.empty()) {
    qDebug() << "loadData empty";
    return;
  }

  beginResetModel();
  dataList.clear();

  for (auto &model : selectData) {
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
