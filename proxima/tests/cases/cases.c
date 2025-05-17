#include "cases.h"
#include "string.h"
#include <stdio.h>

#include <quark/services/database/sqlite_service.h>
#include "quark/core/types/string.h"


void cSelectLocations()
{
    const char* database_path = "polaris.sqlite";
    QKString* dbPath = QKStringCreate((char*)database_path);
    QKSqliteService* sqliteService = QKSqliteServiceCreate(dbPath, NULL);
    QKString* sqlText = QKStringCreate("SELECT $str as strVal, $int as intVal;");
    QKSqliteCommand* sqlCmd = QKSqliteServiceCreateCommand(sqliteService, sqlText, NULL);
    QKString* strName = QKStringCreate("$str");
    QKString* strVal = QKStringCreate("hello呀哈哈");
    int rc = QKSqliteCommandBindString(sqlCmd, strName, strVal, NULL);
    if (rc != 0)
    {
        printf("cSqliteStatParams error: %d\n", rc);
        return;
    }

    QKString* intName = QKStringCreate("$int");
    int intVal = 128;
    QKSqliteCommandBindInt(sqlCmd, intName, intVal, NULL);

    QKSqliteResult* sqlResult = QKSqliteCommandRun(sqlCmd, NULL);
    QKSqliteRow* sqlRow = QKSqliteResultGetRow(sqlResult, 0, NULL);

    QKString* strColName = QKStringCreate("strVal");
    QKSqliteColumn* strColByName = QKSqliteRowGetColumnByName(sqlRow, strColName, NULL);
    QKString* strValByName = QKSQliteColumnGetStringValue(strColByName, NULL);
    char* strValData = QKStringGetData(strValByName);
    QKSqliteColumn* intColByName = QKSqliteRowGetColumnByName(sqlRow, QKStringCreate("intVal"), NULL);
    int intValByName = QKSQliteColumnGetIntValue(intColByName, NULL);

    // QKSqliteCommandClose(sqlCmd);
    // QKSqliteServiceDelete(sqliteService);
    printf("cSqliteSelectNames: %s, %d\n", strValData, intValByName);
}
