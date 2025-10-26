#include "../sources/toolbar.h"

#include <iostream>

void ToolbarModel::receiveStruct(QVariant var) {
  std::cerr << "receiveStruct" << std::endl;
  QVariantMap map = var.toMap();
  // MyStruct s;
  auto value = map["value"].toFloat();
  auto name = map["name"].toFloat();
  std::cerr << "value: " << value << "|" << name << std::endl;

  QPoint newPos = QPoint(value, name);
  if (m_macWindow != nullptr) {
    m_macWindow->setPosition(newPos);
  } else {
    std::cerr << "m_macWindow is nullptr" << std::endl;
  }

}

void ToolbarModel::setMacWindow(MacWindow *macWindow) {

  m_macWindow = macWindow;

}