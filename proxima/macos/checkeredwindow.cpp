#include "checkeredwindow.h"
#include <QtWidgets/QtWidgets>
#include <QQuickView>
#include <QUrl>
#include <QDebug>

#include "macwindow.h"

CheckeredWindow::CheckeredWindow()
:m_color(60, 40, 750, 255)
{

}

void CheckeredWindow::setOpaqueFormat(bool enable)
{
    // Opaque windows do not have an alpha channel and are guaranteed
    // to fill their entire content area. This guarantee is propagated
    // to Cocoa via the NSView opaque property.
    QSurfaceFormat format;
    format.setAlphaBufferSize(enable ? 0 : 8);
    setFormat(format);
}

void CheckeredWindow::setColor(QColor color)
{
    m_color = color;
}

void CheckeredWindow::paintEvent(QPaintEvent * event)
{
    QRect r = event->rect();

    QPainter p(this);

    QColor fillColor = m_color;
    QColor fillColor2 = fillColor.darker();

    int tileSize = 40;
    for (int i = -tileSize * 2; i < r.width() + tileSize * 2; i += tileSize) {
        for (int j = -tileSize * 2; j < r.height() + tileSize * 2; j += tileSize) {
            QRect rect(i + (m_offset.x() % tileSize * 2), j + (m_offset.y() % tileSize * 2), tileSize, tileSize);
            int colorIndex = abs((i/tileSize - j/tileSize) % 2);
            p.fillRect(rect, colorIndex == 0 ? fillColor : fillColor2);
        }
    }

    QRect g = geometry();
    QString text;
    text += QString("Size: %1 %2\n").arg(g.width()).arg(g.height());

    QPen pen;
    pen.setColor(QColor(150, 100, 25, 100));
    p.setPen(pen);
    p.drawText(QRectF(0, 0, width(), height()), Qt::AlignCenter, text);
}

void showMacOSCheckeredWindow() {

  // Create the application content windows
  auto contetWindow = new CheckeredWindow();
  contetWindow->setColor(QColor(10, 60, 130));
  auto leftAccessoryWindow = new CheckeredWindow();
  leftAccessoryWindow->resize(80, 1);
  leftAccessoryWindow->setColor(QColor(60, 100, 80, 150));
  auto rigthAccessoryWindow = new CheckeredWindow();
  rigthAccessoryWindow->resize(80, 1);
  rigthAccessoryWindow->setColor(QColor(70, 40, 50, 150));

  //  Wrap the appplication content window in a MacWindow
  MacWindow *macWindow = new MacWindow(contetWindow, leftAccessoryWindow);
  macWindow->setContentWindow(contetWindow);
  macWindow->setLeftAcccessoryWindow(leftAccessoryWindow);
  macWindow->setRightAcccessoryWindow(rigthAccessoryWindow);

  macWindow->show();

  // 此处需要调用以下3个Window的show方法，否则不会显示
  contetWindow -> show();
  leftAccessoryWindow -> show();
  rigthAccessoryWindow -> show();
}