#include <iostream>
#include "cases.h"

#include <functional>
#include <unordered_map>

#include "files/file.h"
#include "sqlite/sqlite.h"


int quark::tests::TestHelloWorld() {
  std::cout << "TestHelloWorld: OK" << std::endl;
  return 0;
}

const std::unordered_map<std::string, std::function<int()>> caseMap{
    {"TestHelloWorld", quark::tests::TestHelloWorld},
    {"TestSqliteSelect", quark::tests::TestSqliteSelect},
    {"TestSqliteVersion", quark::tests::TestSqliteVersion},
    {"TestSelectFiles", quark::tests::TestSelectFiles}};

int quark::tests::runCase(const std::string &caseName) {
  const auto end = caseMap.end();
  auto it = caseMap.find(caseName);
  if (it != end) {
    return it->second();
  }
  return -1; // case not found
}
