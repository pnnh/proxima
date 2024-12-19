#include "SqliteColumn.h"

#include <sqlite3.h>

#include "types/Exception.h"


quark::SqliteColumn::SqliteColumn() : colType(), colIndex()
{
}

quark::SqliteColumn::SqliteColumn(const int colType, const int colIndex, const std::string&& colName) :
    colType(colType), colIndex(colIndex),
    colName(colName)
{
    if (colIndex < 0) throw quark::PSException("列索引必须大于等于0");
}

quark::SqliteColumn::~SqliteColumn()
{
}

quark::SqliteColumn::SqliteColumn(const SqliteColumn& other) :
    colType(other.colType), colIndex(other.colIndex),
    colName(other.colName), colIsNull(other.colIsNull)
{
    // if (colType == SQLITE_TEXT)
    // {
    //     stringValue = other.stringValue;
    // }
    // else if (colType == SQLITE_INTEGER)
    // {
    //     intValue = other.intValue;
    // }
    // else if (colType == SQLITE_FLOAT)
    // {
    //     floatValue = other.floatValue;
    // }
    variantValue = other.variantValue;
}

quark::SqliteColumn& quark::SqliteColumn::operator=(const SqliteColumn& other)
{
    colType = other.colType;
    colIndex = other.colIndex;
    colName = other.colName;
    colIsNull = other.colIsNull;
    // if (colType == SQLITE_TEXT)
    // {
    //     stringValue = other.stringValue;
    // }
    // else if (colType == SQLITE_INTEGER)
    // {
    //     intValue = other.intValue;
    // }
    // else if (colType == SQLITE_FLOAT)
    // {
    //     floatValue = other.floatValue;
    // }
    variantValue = other.variantValue;
    return *this;
}

quark::SqliteColumn::SqliteColumn(SqliteColumn&& other) noexcept :
    colType(other.colType), colIndex(other.colIndex),
    colName(std::move(other.colName)), colIsNull(other.colIsNull)
{
    // if (colType == SQLITE_TEXT)
    // {
    //     stringValue = std::move(other.stringValue);
    // }
    // else if (colType == SQLITE_INTEGER)
    // {
    //     intValue = other.intValue;
    // }
    // else if (colType == SQLITE_FLOAT)
    // {
    //     floatValue = other.floatValue;
    // }
    variantValue = std::move(other.variantValue);
}

quark::SqliteColumn& quark::SqliteColumn::operator=(SqliteColumn&& other) noexcept
{
    colType = other.colType;
    colIndex = other.colIndex;
    colName = std::move(other.colName);
    colIsNull = other.colIsNull;

    // if (colType == SQLITE_TEXT)
    // {
    //     stringValue = std::move(other.stringValue);
    // }
    // else if (colType == SQLITE_INTEGER)
    // {
    //     intValue = other.intValue;
    // }
    // else if (colType == SQLITE_FLOAT)
    // {
    //     floatValue = other.floatValue;
    // }
    variantValue = std::move(other.variantValue);
    other.colType = 0;
    other.colIndex = 0;
    other.colName.clear();
    other.colIsNull = false;
    // other.stringValue.clear();
    // other.intValue = 0;
    // other.floatValue = 0;
    return *this;
}

int quark::SqliteColumn::getColIndex() const
{
    return colIndex;
}

std::string quark::SqliteColumn::getColName() const
{
    if (colName.empty()) return "column" + std::to_string(colIndex);
    return colName;
}

std::string quark::SqliteColumn::getStringValue() const
{
    return variantValue.index() == 0 ? std::get<std::string>(variantValue) : "";
}

void quark::SqliteColumn::setStringValue(const char* value)
{
    // stringValue = value;
    variantValue = std::string(value);
}

void quark::SqliteColumn::setStringValue(const std::string& value)
{
    // stringValue = value;
    variantValue = value;
}

void quark::SqliteColumn::setStringValue(const std::string&& value)
{
    // stringValue = value;
    variantValue = value;
}

int quark::SqliteColumn::getIntValue() const
{
    // return intValue;
    return variantValue.index() == 1 ? static_cast<int>(std::get<long>(variantValue)) : 0;
}

void quark::SqliteColumn::setIntValue(int value)
{
    // intValue = value;
    variantValue = static_cast<long>(value);
}

double quark::SqliteColumn::getFloatValue() const
{
    // return floatValue;
    return variantValue.index() == 2 ? std::get<double>(variantValue) : 0;
}

void quark::SqliteColumn::setFloatValue(double value)
{
    // floatValue = value;
    variantValue = value;
}

int quark::SqliteColumn::getColType() const
{
    return colType;
}

bool quark::SqliteColumn::isNull() const
{
    return colIsNull;
}

void quark::SqliteColumn::setNull()
{
    colIsNull = true;
}
