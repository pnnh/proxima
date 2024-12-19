#include "SqliteRow.h"
#include <algorithm>

void quark::SqliteRow::appendColumn(const quark::SqliteColumn&& column)
{
    columnValues[column.getColIndex()] = column;
    colNames.push_back(column.getColName());
}

std::optional<quark::SqliteColumn> quark::SqliteRow::getColumn(const std::string&& colName)
{
    auto indexIt = std::find(colNames.begin(), colNames.end(), colName);

    if (indexIt == colNames.end())
    {
        return std::nullopt;
    }

    auto index = std::distance(colNames.begin(), indexIt);
    return getColumn(static_cast<int>(index));
}

std::optional<quark::SqliteColumn> quark::SqliteRow::getColumn(int colIndex)
{
    auto it = columnValues.find(colIndex);
    if (it == columnValues.end())
        return std::nullopt;
    return it->second;
}
