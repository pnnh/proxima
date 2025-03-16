import QtQuick 2.15
import QtQuick.Controls 2.5
import QtQuick.Layouts 1.3
import quick 1.0

Rectangle {
    anchors.fill: parent 

    AppNav {}

    RowLayout {
        height: parent.height
        width: parent.width
        spacing: 0

        Rectangle {
            Layout.preferredWidth: 240
            Layout.preferredHeight: parent.height
            color: "transparent"

        }
        Rectangle {
            Layout.alignment: Qt.AlignLeft
            Layout.preferredHeight: parent.height

            Layout.preferredWidth: 1
            color: "#e2e2e2"
            visible: true
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
                        text: qsTr("Hello Designw222") 
                        anchors.centerIn: parent
                    }
                }
                Rectangle {
                    Layout.preferredHeight: parent.height
                    Layout.preferredWidth: 1
                    color: "#e2e2e2"
                }

                Editor {}
            }
        }
    }
}
