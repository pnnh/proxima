
#include <QQmlDebuggingEnabler>
#include <QtQuick>
#include <iostream>
#include <spdlog/spdlog.h>

#ifdef __APPLE__
#include "TargetConditionals.h"
#ifdef TARGET_OS_MAC
#include "macos/checkeredwindow.h"
#endif
#elif WIN32
#include "windows/CMakeProjectDWM.h"
#endif

#include <QApplication>

int showQtWindow() {

  QQmlApplicationEngine engine;

  engine.loadFromModule("quick", "Main");

  const auto &rootObjects = engine.rootObjects();
  if (rootObjects.isEmpty()) {
    return -1;
  }
  const auto &rootObject = rootObjects.first();
  // if (rootObject != nullptr) {
  QMetaObject::invokeMethod(rootObject, "sayHello");
  //}

  //    QQuickWindow *mainWindow = qobject_cast<QQuickWindow *>(rootObject);

  //    QQuickItem *rect = mainWindow->findChild<QQuickItem *>("myItem");
  //    qDebug() << "rect: " << rect;

  //    if (mainWindow != nullptr) {
  //        QMetaObject::invokeMethod(mainWindow, "sayHello");
  //    }
  return 0;
}

int main(int argc, char *argv[]) {
#ifndef NDEBUG
  spdlog::set_level(spdlog::level::info);
#endif
  QQmlDebuggingEnabler::enableDebugging(true);
  QLoggingCategory::defaultCategory()->setEnabled(QtDebugMsg, true);

  spdlog::debug("i love c++1");
  spdlog::info("i love c++2");
  spdlog::error("i love c++3");
  qInfo() << "test info";
  qWarning() << "test warning";
  std::cerr << "Hello, World333333!" << std::endl;
  qDebug() << "Hello, World444444!";

  QApplication app(argc, argv);
  QApplication::setApplicationDisplayName(
      QStringLiteral("This example is powered by qmltc!"));

  // showQtWindow();

#ifdef __APPLE__
#include "TargetConditionals.h"
#ifdef TARGET_OS_MAC
  showMacOSCheckeredWindow();
#endif
#elif WIN32
  showWindowsDWMWindow();
#endif

  return QApplication::exec();
}
