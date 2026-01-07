#pragma once

#include <QVector>
#include <optional>

#include "pulsar/business/models/articles/location.h"
#include "quark/infra/result/result.h"

#include <expected>

namespace proxima {
    class LocationService {
    public:
        LocationService();

        [[nodiscard]] std::optional<pulsar::PSLocationModel> FindLocation(
            const QString &uid) const;

        [[nodiscard]] std::expected<QVector<pulsar::PSLocationModel>, quark::MTCode>
        SelectLocations() const;

        void InsertOrUpdateLocation(
            const QVector<pulsar::PSLocationModel> &libraryList);

        void InsertOrUpdateLocation(
            const pulsar::PSLocationModel &libraryModel);

    private:
        std::string dbPath;
    };
}
