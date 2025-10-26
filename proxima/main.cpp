
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

#include "macos/fibonacci.h"
#include "tests/cases.h"

#include <QApplication>

#include <thread>

// import quark.logger;

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

  std::cout << "Hello, World!" << std::endl;
  // MXLogger::LogInfo("Hello, World22!");
#ifdef __APPLE__
  // auto abc = SwiftFibonacci::fibonacciSwift(5);
  std::cout << "abc2: " << fibo::calcFibonacci() << std::endl;
#endif
  std::cout << "current path: " << std::filesystem::current_path() << std::endl;
  printf("arguments: %d,%s\n", argc, argv[1]);
  if (argc > 1) {
    std::string caseName = argv[1];

    std::cout << "selected case: " << caseName << " " << caseName << std::endl;

    auto caseResult = quark::tests::runCase(caseName);
    if (caseResult >= 0) {
      return 0;
    }
  }

#ifndef NDEBUG
  spdlog::set_level(spdlog::level::info);
#endif
  // QQmlDebuggingEnabler::enableDebugging(true);
  // QLoggingCategory::defaultCategory()->setEnabled(QtDebugMsg, true);

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
