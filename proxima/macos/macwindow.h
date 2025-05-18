#pragma once

#include <QtCore/QtCore>
#include <QtGui/QtGui>


Q_FORWARD_DECLARE_OBJC_CLASS(NSWindow);

class MacWindow {
public:
  MacWindow(QWindow *contentWindow, QWindow *leftAccessoryWindow,
            QWindow *rightAccessoryWindow);
  void setContentWindow(QWindow *contentWindow);

  void setGeometry(QRect geometry);
  QRect geometry() const;
  void setSize(QSize &size);
  QSize size() const;
  void setVisible(bool visible);
  void show();
  void setPosition(QPoint &position);

protected:
  void createNSWindow();
  void destroyNSWindow();
  void recreateNSWindow();
  void scheduleRecreateNSWindow();

private:
  QWindow *m_window = nullptr;
  QWindow *m_leftAccessoryWindow = nullptr;
  QWindow *m_rightAccessoryWindow = nullptr;
  NSWindow *m_nsWindow = nullptr;
  QRect m_geometry;
  bool m_visible;
};
