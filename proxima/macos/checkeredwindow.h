#pragma once

#include <QtGui>

class CheckeredWindow : public QRasterWindow
{
Q_OBJECT
public:
    CheckeredWindow();

    void setOpaqueFormat(bool enable);
    void setColor(QColor color);

    void paintEvent(QPaintEvent * event) override;
private:
    int m_drawAlpha;
    bool m_emitCloseOnClick;

    QPoint m_offset;
    QPoint m_lastPos;
    QColor m_color;
};

void showMacOSCheckeredWindow();
