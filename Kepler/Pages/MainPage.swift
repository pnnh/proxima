import SwiftData
import SwiftUI

struct PSMainPage: View {
    @Environment(\.modelContext) private var modelContext
    @EnvironmentObject var router: Router

    var body: some View {

        Grid {
            GridRow {
                VStack { 
                    Image(.images)
                    Text("图片管理")
                        .onTapGesture {
                            print("Double tapped!")
                            router.navigate(to: .images)
                        }
                }
            }
            GridRow {
                VStack {
                    Image(.images)
                    Text("打开笔记库")
                        .onTapGesture {
                            print("Double tapped!2")
                            
                            
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
                                    
                                    router.navigate(to: .notes(owner: selectedPath))
                                }
                            } else {
                                print("User canceled the selection")
                            }
                            
                            
                            
                        }
                }
            }
            GridRow {
                VStack {
                    Image(.images)
                    Text("打开文件夹")
                        .onTapGesture {
                            print("Double tapped!3")
                            router.navigate(to: .images)
                        }
                }
            }
        }
        .padding(0)
        .frame(
            minWidth: 0,
            maxWidth: .infinity,
            minHeight: 0,
            maxHeight: .infinity,
            alignment: .topLeading
        )
        .background(Color.orange)
        .toolbar { 
            ToolbarItemGroup {
                Button {
                    // Action for help
                } label: {
                    Label("Help", systemImage: "questionmark.circle")
                }
            }
        }.toolbarBackground(Color.green)
    }
}

@Model
final class Item {
    var timestamp: Date

    init(timestamp: Date) {
        self.timestamp = timestamp
    }
}
