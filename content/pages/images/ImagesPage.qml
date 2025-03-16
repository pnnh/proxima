import QtQuick 2.15
import QtQuick.Controls 2.5
import QtQuick.Layouts 1.3
import "../../components"

Rectangle {
    Layout.preferredHeight: parent.height
    Layout.preferredWidth: parent.width - 48 

    AppNav {}

    RowLayout {
        height: parent.height
        width: parent.width
        spacing: 0

        Rectangle {
            Layout.preferredWidth: 240
            visible: true
            Layout.preferredHeight: parent.height
            color: "transparent"
        }
        Rectangle {
            Layout.alignment: Qt.AlignLeft
            Layout.preferredHeight: parent.height
            Layout.preferredWidth: 1
            color: "#e2e2e2"
        }
        Rectangle {
            Layout.fillWidth: true
            Layout.fillHeight: true
            color: "#FFFFFF"

            RowLayout {
                height: parent.height
                width: parent.width
                spacing: 0

                Rectangle {
                    Layout.fillHeight: true
                    Layout.preferredWidth: parent.width / 2 - 0.5
                    color: "#FFFFFF"
                    Text {
                        text: qsTr("图片库主页") 
                        anchors.centerIn: parent
                    }
                }
                Rectangle {
                    Layout.preferredHeight: parent.height
                    Layout.preferredWidth: 1
                    color: "#e2e2e2"
                }

            }
        }
    }
}
