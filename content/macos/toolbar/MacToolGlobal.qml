import QtQuick 2.15

Rectangle {
    anchors.fill: parent
    color: "yellow"

    Image {
        width: 20
        height: 20
        source: "qrc:/qt/qml/quick/content/assets/icons/global.png"
        anchors.centerIn: parent

        MouseArea {
            anchors.fill: parent
            onClicked: {
                console.log(parent.width, parent.height)
                appItem.routeUrlToItem('/')
            }
        }
    }
}