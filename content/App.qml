import QtQuick 2.15
import QtQuick.Controls 2.5
import QtQuick.Layouts 1.3
import "pages"
import "pages/files"
import "pages/notes"
import "pages/images"
import "pages/password"
import "pages/uuid"

Rectangle {
    id: appItem
    anchors.fill: parent
    color: "#f8f8f8"
    radius: 8
    opacity: 1

    property string routeUrl: "/"

    function routeUrlToItem(url: string): Item {
        console.log('routeUrlToItem', url)
        if (url === "/") {
            myLoader.sourceComponent = homePage
        } else if (url === "/files") {
            myLoader.sourceComponent = filesPage
        } else if (url === "/notes") {
            myLoader.sourceComponent = notesPage
        } else if (url === "/images") {
            myLoader.sourceComponent = imagesPage
        } else if (url === "/password") {
            myLoader.sourceComponent = passwordPage
        } else if (url === "/uuid") {
            myLoader.sourceComponent = uuidPage
        }
    }

    Loader {
        id: myLoader
        width: parent.width
        height: parent.height
    }
    Component.onCompleted: myLoader.sourceComponent = filesPage

    Component {
        id: homePage
        Home {
        }
    }
    Component {
        id: filesPage
        FilesPage {
        }
    }
    Component {
        id: notesPage
        NotesPage {
        }
    }
    Component {
        id: imagesPage
        ImagesPage {
        }
    }
    Component {
        id: passwordPage
        PasswordPage {
        }
    }
    Component {
        id: uuidPage
        UUIDPage {
        }
    }
}
