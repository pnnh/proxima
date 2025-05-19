#include <iostream>
#include "quark/services/database/sqlite_service.h"
#include "sqlite.h"

int quark::tests::TestSqliteVersion() {
  auto database_path = "polaris.sqlite";
  auto sqliteService = quark::MTSqliteService(database_path);
  auto version = sqliteService.sqliteVersion();

  std::cout << "Sqlite version: " << version << std::endl;
  return (int)version.starts_with("3.");
  return 0;
}

int quark::tests::TestSqliteSelect() {
  auto database_path = "polaris.sqlite";
  auto sqliteService = quark::MTSqliteService(database_path);
  std::string sqlText = "SELECT * FROM sqlite_master;";
  auto sqlResult = sqliteService.runSql(sqlText);
  if (!sqlResult) {
    std::cout << "run sql error" << std::endl;
    return 0;
  }
  auto rowCount = sqlResult->getRowCount();
  if (rowCount < 1) {
    std::cout << "table is empty" << std::endl;
    return 0;
  }
  auto row = sqlResult->getRow(0);
  if (!row) {
    std::cout << "row not found" << std::endl;
    return 0;
  }
  auto nameColumn = row->getColumn("name");
  if (nameColumn == nullptr) {
    std::cout << "name column not found" << std::endl;
    return 0;
  }
  auto title = nameColumn->getStringValue();

  std::cout << "table name: " << title << std::endl;
  return 0;
}

