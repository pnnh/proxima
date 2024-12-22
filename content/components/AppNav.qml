import QtQuick 2.15
import QtQuick.Controls 2.5
import QtQuick.Layouts 1.3

    Rectangle {
        color: "#f8f8f8"
        height: 40
        width: parent.width


        Image {
            anchors.left: parent.left
            anchors.leftMargin: 8
            anchors.verticalCenter: parent.verticalCenter
            width: 20
            height: 20
            source: "qrc:/qt/qml/quick/content/assets/icons/global.png"

            MouseArea {
                anchors.fill: parent
                onClicked: {
                    console.log(parent.width, parent.height)
                    appItem.routeUrlToItem('/')
                }
            }
        }
    }
