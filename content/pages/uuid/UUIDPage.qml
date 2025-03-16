import QtQuick 2.15
import "../../components"

Rectangle { 
    anchors.left: parent.left
    anchors.right: parent.right
    anchors.bottom: parent.bottom
    height: parent.height - 40

    AppNav{}


    Text {
        anchors.centerIn: parent
        text: "UUID生成"
    }
}
