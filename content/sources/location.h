#pragma once

#include <qabstractitemmodel.h>
#include <QtQml/qqmlregistration.h>

typedef QVector<QString> FileViewData;

class LocationViewModel : public QAbstractListModel {
  Q_OBJECT
  QML_ELEMENT

  Q_PROPERTY(bool directories READ directories WRITE setDirectories)
  Q_PROPERTY(bool files READ files WRITE setFiles)
  Q_PROPERTY(QString parentPath READ parentPath WRITE setParentPath)

public:
  explicit LocationViewModel(QObject *parent = nullptr);
  ~LocationViewModel() override;

  LocationViewModel(const LocationViewModel &) = delete;
  LocationViewModel &operator=(const LocationViewModel &) = delete;
  LocationViewModel(LocationViewModel &&) = delete;
  LocationViewModel &operator=(LocationViewModel &&) = delete;

  [[nodiscard]] bool directories() const;
  void setDirectories(const bool &directories);
  [[nodiscard]] bool files() const;
  void setFiles(const bool &files);
  [[nodiscard]] QString parentPath() const;
  void setParentPath(const QString &parentPath);
  Q_INVOKABLE void appendDirectory(QVariant var);

  [[nodiscard]] int rowCount(const QModelIndex &parent) const override;
  [[nodiscard]] QHash<int, QByteArray> roleNames() const override;
  [[nodiscard]] QVariant
  data(const QModelIndex &index, int role) const override;

private:
  bool m_directories{true};
  bool m_files{true};
  QString m_parentPath{""};
  void loadData();
  QHash<int, QByteArray> dataNames;
  QVector<FileViewData> dataList;
};
