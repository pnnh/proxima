#include "location_service.h"
#include "UserService.h"

#include <qdir.h>
#include <qdiriterator.h>

#include "quark/business/models/articles/location.h"
#include "quark/business/models/articles/Notebook.h"
#include "quark/services/database/sqlite_service.h"

proxima::LocationService::LocationService() {
  auto appDir = UserService::EnsureApplicationDirectory("/Polaris/Index");
  dbPath = appDir.toStdString() + "/Library.db";

  std::string createSql{"create table if not exists locations("
      "uid varchar primary key not null,"
      "name varchar(128) not null,"
      "path varchar(512) not null)"};

  auto sqliteService = quark::MTSqliteService(dbPath);
  auto sqlResult = sqliteService.runSql(createSql);
  if (!sqlResult) {
    std::cout << "run sql error" << std::endl;
    throw std::runtime_error("create table locations error");
  }

  std::string indexSql{
      "create index if not exists index_locations_path on locations(path);"};
  if (!sqliteService.runSql(indexSql)) {
    throw std::runtime_error("create index index_locations_path error");
  }
}

std::optional<quark::PSLocationModel>
proxima::LocationService::FindLocation(const QString &uid) const {
  auto findSql = QString("select * from locations where uid = :uid").
      toStdString();

  QMap<QString, QVariant> parameters = {
      {
          ":uid",
          uid,
      }
  };
  auto sqliteService = quark::MTSqliteService(dbPath);
  auto sqlCmd = sqliteService.createCommand(findSql);
  if (!sqlCmd) {
    std::cout << "run sql error" << std::endl;
    throw std::runtime_error("FindLibrary createCommand error");
  }
  sqlCmd->BindString(":uid", uid.toStdString());

  auto sqlResult = sqlCmd->Run();
  auto rowCount = sqlResult->getRowCount();

  if (rowCount < 1) {
    return std::nullopt;
  }
  auto sqlRow = sqlResult->getRow(0);
  auto model = std::make_optional<quark::PSLocationModel>();
  model->URN = sqlRow->getColumn("uid")->getStringValue();
  model->Name = sqlRow->getColumn("name")->getStringValue();
  model->Path = sqlRow->getColumn("path")->getStringValue();
  return model;
}

std::expected<QVector<quark::PSLocationModel>, quark::MTCode>
proxima::LocationService::SelectLocations() const {
  QVector<quark::PSLocationModel> libraryList;
  auto selectSql = QString("select * from locations").toStdString();
  auto sqliteService = quark::MTSqliteService(dbPath);
  auto sqlResult = sqliteService.runSql(selectSql);
  if (!sqlResult) {
    std::cout << "run sql error" << std::endl;
    return std::unexpected(quark::MTCode::SqlSelectFailed);
  }

  auto rowCount = sqlResult->getRowCount();

  for (int rowIndex = 0; rowIndex < rowCount; ++rowIndex) {
    auto sqlRow = sqlResult->getRow(rowIndex);
    auto model = quark::PSLocationModel();
    model.URN = sqlRow->getColumn("uid")->getStringValue();
    model.Name = sqlRow->getColumn("name")->getStringValue();
    model.Path = sqlRow->getColumn("path")->getStringValue();

    libraryList.push_back(model);
  }

  return libraryList;
}

void proxima::LocationService::InsertOrUpdateLocation(
    const QVector<quark::PSLocationModel> &libraryList) {

  const auto insertSql =
      QString("insert into locations(uid, name, path)"
          " values(:uid, :name, :path)"
          " on conflict (uid) do update set name = :name;").toStdString();
  auto sqliteService = quark::MTSqliteService(dbPath);
  auto sqlCmd = sqliteService.createCommand(insertSql);
  if (!sqlCmd) {
    std::cout << "run sql error" << std::endl;
    throw std::runtime_error("FindLibrary createCommand error");
  }
  for (const auto &library : libraryList) {
    sqlCmd->Reset();
    sqlCmd->BindString(":uid", library.URN);
    sqlCmd->BindString(":name", library.Name);
    sqlCmd->BindString(":path", library.Path);

    auto sqlResult = sqlCmd->Run();

    if (!sqlResult) {
      throw std::runtime_error("create table locations error");
    }
  }
}

void proxima::LocationService::InsertOrUpdateLocation(
    const quark::PSLocationModel &libraryModel) {
  QVector<quark::PSLocationModel> libraryList;
  libraryList.append(libraryModel);
  InsertOrUpdateLocation(libraryList);
}
