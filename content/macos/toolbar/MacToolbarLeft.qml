import QtQuick 2.15

Rectangle {
    id: titleBar
    anchors.fill: parent
    color: "green"

    Text {
        anchors.centerIn: parent
        text: "ToolbarLeft"
        font.pixelSize: 20
        color: "#333"
    }

    property bool dragging: false
    property point dragStartPos

    MouseArea {
        anchors.fill: parent
        onPressed: {
            titleBar.dragging = true
            titleBar.dragStartPos = Qt.point(mouse.x, mouse.y)
        }
        onReleased: titleBar.dragging = false
        onPositionChanged: {
            if (titleBar.dragging) {
                // 计算偏移量，移动窗口
                let dx = mouse.x - titleBar.dragStartPos.x
                let dy = mouse.y - titleBar.dragStartPos.y

                console.log('dx:', dx, 'dy:', dy)
                // window.x += dx
                // window.y += dy
            }
        }
        cursorShape: Qt.OpenHandCursor
    }
}