#pragma once

#include "proxima/macos/macwindow.h"

#include <qabstractitemmodel.h>
#include <QtQml/qqmlregistration.h>


// class MyStruct {
//   Q_GADGET
//   Q_PROPERTY(int value MEMBER value)
//   Q_PROPERTY(QString name MEMBER name)
//
// public:
//   int value;
//   QString name;
// };
//
// Q_DECLARE_METATYPE(MyStruct)


class ToolbarModel : public QObject {
  Q_OBJECT

public:
  Q_INVOKABLE void receiveStruct(QVariant var);

  void setMacWindow(MacWindow *macWindow);

private:
  MacWindow *m_macWindow;
};
