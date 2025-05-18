import QtQuick 2.15
import QtQuick.Controls 2.5
import QtQuick.Layouts 1.3
import quick 1.0

Rectangle {
    id: filesDetail
    width: parent.width
    anchors.right: parent.right
    anchors.top: parent.top
    anchors.bottom: parent.bottom
    height: parent.height


    function changePath(path: string) {
        console.log('changePath2', path)
        filesList.model.parentPath = path
    }


    ListView {
        id: filesList
            anchors.fill: parent
            model: FileViewModel{
            }
            anchors.rightMargin: 1
            clip: true
            boundsBehavior: Flickable.StopAtBounds
            delegate: Item {
                width: filesList.width
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
                        onDoubleClicked: {
                            console.log("double clicked", model.path)
                            changePath(model.path)
                        }
                    }
                }

            }
        }


    Component.onCompleted: () => {
console.log("xxx")
                               filesPage.pathChanged.connect(filesDetail.changePath)
}
}
