#include "cases.h"
#include "quark/services/database/sqlite_service.h"
#include <iostream>

void cxxSelectLocations()
{
#if !defined(_WIN32)
    return;
#endif
    auto sqlSvc = quark::MTSqliteService("C:\\Users\\linya\\Documents\\databases\\suzaku\\main.db");
    auto sqlText = "select * from locations;";
    auto sqlCmd{sqlSvc.createCommand(sqlText)};
    auto sqlResult = sqlCmd->Run();
    auto rowCount = sqlResult->getRowCount();
    std::cout << "cSqliteStatParams: " << rowCount << std::endl;

    // std::string userInput;
    // std::getline(std::cin, userInput);
}
