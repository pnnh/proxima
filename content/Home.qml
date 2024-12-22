import QtQuick 2.15
import QtQuick.Controls 2.5
import QtQuick.Layouts 1.3


Rectangle {
    anchors.fill: parent

    GridView {
        id: grid

         height: parent.height - 32
        width: parent.width - 32
        anchors.centerIn: parent
        anchors.margins: 16
        boundsBehavior: Flickable.StopAtBounds // 滑动不超出父框的大小
        cellHeight: 240
        cellWidth: 240
        clip: true // 超出边界的进行裁剪，即不显示，默认为false
        cacheBuffer: 240 * 32 // 缓存的大小，超出这个大小就会被释放

        ScrollBar.vertical: ScrollBar {
            visible: false
            background: Rectangle {
                color: "#fafafa"
                radius: 4
            }
            snapMode: ScrollBar.SnapAlways
        }

        delegate: Rectangle {
            width: 240
            height: 240

            Rectangle {
                width: parent.width - 16
                height: parent.height - 16
                radius: 4
                color:"#fff"
                anchors.horizontalCenter: parent.horizontalCenter
                anchors.verticalCenter: parent.verticalCenter

                Image {
                    anchors.top: parent.top
                    anchors.topMargin: 2
                    anchors.horizontalCenter: parent.horizontalCenter
                    source: image
                    fillMode: Image.PreserveAspectCrop
                    sourceSize.width: 128 // 预览图片的尺寸，避免图片过大影响性能
                    sourceSize.height: 128
                    width: parent.width - 48
                    height: parent.height - 48
                }
                Text {
                    text: name
                    anchors.bottom: parent.bottom
                    anchors.horizontalCenter: parent.horizontalCenter
                    anchors.bottomMargin: 16

                    font.pixelSize: 18
                }

                MouseArea {
                    anchors.fill: parent
                    onClicked: {
                        console.log(parent.width, parent.height)
                        appItem.routeUrlToItem(routeUrl)
                    }
                }
            }
        }
        model: ListModel {
            id: theModel

            ListElement {
                image: "qrc:/qt/qml/quick/content/assets/icons/product/files.png"
                name:"资源管理器"
                routeUrl: "/files"
            }
            ListElement {
                image:"qrc:/qt/qml/quick/content/assets/icons/product/notes.png"
                name:"笔记管理"
                routeUrl: "/notes"
            }
            ListElement {
                image: "qrc:/qt/qml/quick/content/assets/icons/product/images.png"
                name: "图片管理"
                routeUrl: "/images"
            }
            ListElement {
                image:"qrc:/qt/qml/quick/content/assets/icons/product/password.png"
                name:"随机密码生成"
                routeUrl: "/password"
            }
            ListElement {
                image:"qrc:/qt/qml/quick/content/assets/icons/product/uuid.png"
                name:"UUID生成"
                routeUrl: "/uuid"
            }

        }
    }
}
