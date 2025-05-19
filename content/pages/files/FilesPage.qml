import QtQuick 2.15
import QtQuick.Controls 2.5
import QtQuick.Layouts 1.3
import quick 1.0

Rectangle {
    id: filesPage
    color: "#FFFFFF"
    anchors.left: parent.left
    anchors.right: parent.right
    anchors.bottom: parent.bottom
    height: parent.height

    signal pathChanged(string name)

    property int topNavHeight: 40

    Rectangle {
        id: leftNavView
        color: "#FFFFFF"
        anchors.left: parent.left
        anchors.bottom: parent.bottom
        height: parent.height
        width: 200


        Rectangle {
            id: topNav
            color: "#E0E0E0"
            anchors.top: parent.top
            anchors.left: parent.left
            width: parent.width
            height: topNavHeight
            Row {
                width: parent.width / 2
                height: 20
                anchors.verticalCenter: parent.verticalCenter
                anchors.right: parent.right
                Image {
                    width: 20
                    height: 20
                    anchors.verticalCenter: parent.verticalCenter
                    anchors.right: parent.right
                    anchors.rightMargin: 8
                    source: "qrc:/qt/qml/quick/content/assets/icons/files/add.svg"
                }
            }
        }

        Rectangle {
            color: "#E0E0E0"
            anchors.top: parent.top
            anchors.right: parent.right
            width: 1
            height: parent.height
        }

        ListView {
            id: filesList
            anchors.top: topNav.bottom
            width: parent.width
            height: parent.height - topNavHeight
            model: FileViewModel {
                files: false
            }
            anchors.rightMargin: 1
            clip: true
            boundsBehavior: Flickable.StopAtBounds
            delegate: Item {
                width: parent.width
                height: 40
                Rectangle {
                    color: "#FFFFFF"
                    anchors.fill: parent

                    Text {
                        anchors.left: parent.left
                        anchors.horizontalCenter: parent.horizontalCenter
                        anchors.verticalCenter: parent.verticalCenter
                        anchors.margins: 16
                        width: parent.width
                        clip: true
                        text: model.name
                    }
                    MouseArea {
                        hoverEnabled: true
                        anchors.fill: parent
                        onEntered: {
                            parent.color = "#F0F0F0"
                        }
                        onExited: {
                            parent.color = "#FFFFFF"
                        }
                        onClicked: {
                            console.log('click')
                            filesPage.pathChanged(model.path)
                        }
                    }
                }

            }
        }
    }

    Loader {
        id: myLoader
        width: parent.width - leftNavView.width
        height: parent.height
        anchors.right: parent.right
        anchors.bottom: parent.bottom
    }
    Component {
        id: detailComponent
        FilesDetail {
        }
    }
    Component {
        id: emptyComponent

        Text {
            anchors.centerIn: parent
            text: "暂无内容"
        }
    }
    Component.onCompleted: () => {

        myLoader.sourceComponent = detailComponent
    }
}

