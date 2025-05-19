import QtQuick 2.15
import QtQuick.Controls 2.5
import QtQuick.Layouts 1.3

Rectangle {
    color: "#f8f8f8"
    height: 40
    width: parent.width

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
}
