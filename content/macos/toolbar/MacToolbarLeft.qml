import QtQuick 2.15
import quick 1.0

Rectangle {
    id: titleBar
    anchors.fill: parent
    color: "green"

    Row {
        width: parent.width
        height: 20
        anchors.verticalCenter: parent.verticalCenter
        anchors.leftMargin: 8
        Image {
            width: 20
            height: 20
            source: "qrc:/qt/qml/quick/content/assets/icons/files/back.svg"
        }
        Image {
            width: 20
            height: 20
            source: "qrc:/qt/qml/quick/content/assets/icons/files/up.svg"
        }
        Image {
            width: 20
            height: 20
            source: "qrc:/qt/qml/quick/content/assets/icons/files/forward.svg"
        }
    }

    property bool dragging: false
    property point dragStartPos


    MouseArea {
        anchors.fill: parent
        onPressed: (mouse) => {
            titleBar.dragging = true
            titleBar.dragStartPos = Qt.point(mouse.x, mouse.y)
        }
        onReleased: titleBar.dragging = false
        onPositionChanged: (mouse) => {
            if (titleBar.dragging) {
                // 计算偏移量，移动窗口
                let dx = mouse.x - titleBar.dragStartPos.x
                let dy = mouse.y - titleBar.dragStartPos.y

                console.log('dx:', dx, 'dy:', dy)
                let myStruct = {
                    value: dx, name: dy
                }
                appData.receiveStruct(myStruct)
                // window.x += dx
                // window.y += dy
            }
        }
        cursorShape: Qt.OpenHandCursor
    }
}