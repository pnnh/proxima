#pragma once

#include <QVector>
#include <optional>

#include "quark/business/models/articles/location.h"
#include "quark/infra/result/result.h"

#include <expected>

namespace proxima {
class LocationService {
public:
  LocationService();
  [[nodiscard]] std::optional<quark::PSLocationModel> FindLocation(
      const QString &uid) const;
  [[nodiscard]] std::expected<QVector<quark::PSLocationModel>, quark::MTCode>
  SelectLocations() const;
  void InsertOrUpdateLocation(
      const QVector<quark::PSLocationModel> &libraryList);
  void InsertOrUpdateLocation(
      const quark::PSLocationModel &libraryModel);

private:
  std::string dbPath;
};
}