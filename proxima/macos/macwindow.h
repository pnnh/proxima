#pragma once

#include <QtCore/QtCore>
#include <QtGui/QtGui>


Q_FORWARD_DECLARE_OBJC_CLASS(NSWindow);

class MacWindow
{
public:
    MacWindow(QWindow *contentWindow, QWindow *accessoryWindow = nullptr);
    
    // Content Windows
    void setContentWindow(QWindow *contentWindow);
    void setLeftAcccessoryWindow(QWindow *leftAccessoryWindow);
    void setRightAcccessoryWindow(QWindow *rightAccessoryWindow);

    bool acceoryViews() const;

    // Duplicated QWindow API
    void setGeometry(QRect geometry);
    QRect geometry() const;
    void setSize(QSize &size);
    QSize size() const;
    void setVisible(bool visible);
    void show();
protected:
    void createNSWindow();
    void destroyNSWindow();
    void recreateNSWindow();
    void scheduleRecreateNSWindow();

private:
    QWindow *m_window = nullptr;
    QWindow *m_accessoryWindow = nullptr;
    QWindow *m_rightAccessoryWindow = nullptr;
    NSWindow *m_nsWindow = nullptr;
    QRect m_geometry;
    bool m_visible;
};
