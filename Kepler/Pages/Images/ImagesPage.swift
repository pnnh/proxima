import Combine
import Foundation
import SwiftUI

struct PSImagePage: View {
    @EnvironmentObject var router: Router
    @StateObject private var viewModel = SharedViewModel()

    var body: some View {
        
            ImagesGridComponent(viewModel: viewModel).frame(
                minWidth: 0,
                maxWidth: .infinity,
                minHeight: 0,
                maxHeight: .infinity,
                alignment: .topLeading
            ).background(Color.blue)

        .toolbar {
            ToolbarItemGroup {
                Button {

                    router.navigate(to: .home)
                } label: {
                    Label("New", systemImage: "house")
                }
                Button {

                    print("follow")

                    let manager = FileManager.default
                    let urlForDocument = manager.urls(
                        for: .documentDirectory,
                        in: .userDomainMask
                    )
                    let url = urlForDocument[0] as URL
//                    promptForWorkingDirectoryPermission()
                    
                    print("获取到的url: \(url)")

                    let openPanel = NSOpenPanel()
                    openPanel.allowsMultipleSelection = false
                    openPanel.canChooseDirectories = true
                    openPanel.canChooseFiles = true
                    openPanel.showsHiddenFiles = false
                    openPanel.message = "本应用需要访问该目录，请点击允许按钮"
                    openPanel.prompt = "允许"
                    openPanel.directoryURL = URL.init(string: NSHomeDirectory())

                    if openPanel.runModal() == NSApplication.ModalResponse.OK {
                        if let selectedURL = openPanel.url {
                            let selectedPath = selectedURL.path
                            print("Selected directory: \(selectedPath)")
                            viewModel.selectedImagePath = selectedPath
                        }
                    } else {
                        print("User canceled the selection")
                    }

                } label: {
                    Label("New", systemImage: "folder.badge.plus")
                }
            }
        }.toolbarBackground(
            Color.green,
        )
    }

}
