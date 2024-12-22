import QtQuick 2.15
import QtQuick.Controls 2.5
import QtQuick.Layouts 1.3
import "../components"

Rectangle {
    color: "green"
    anchors.left: parent.left
    anchors.right: parent.right
    anchors.bottom: parent.bottom
    height: parent.height - 40

    AppNav{}


    Text {
        anchors.centerIn: parent
        text: "资源管理器"
    }
}
