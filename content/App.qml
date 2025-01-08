import QtQuick 2.15
import QtQuick.Controls 2.5
import QtQuick.Layouts 1.3
import "files"
import "notes"
import "images"
import "password"
import "pages/uuid"

Rectangle {
    id: appItem
    anchors.fill: parent
    anchors.top: parent.top
    color: "#f8f8f8"
    radius: 8
    opacity: 1

    property string routeUrl: "/"

    function routeUrlToItem(url: string): Item {
        console.log('routeUrlToItem', url)
        if (url === "/")
        {
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

    Loader{
           id:myLoader
           width: parent.width
           height: parent.height
    }
    // todo: 为方便开发和调试，暂时修改启动页，后续需要修改为homePage
    Component.onCompleted: myLoader.sourceComponent = filesPage

   Component{
       id:homePage
       Home { }
   }
   Component{
       id:filesPage
      FilesPage { }
   }
   Component{
       id:notesPage
       NotesPage { }
   }
   Component{
       id:imagesPage
       ImagesPage { }
   }
   Component{
       id:passwordPage
       PasswordPage { }
   }
   Component{
       id:uuidPage
       UUIDPage { }
   }
}
