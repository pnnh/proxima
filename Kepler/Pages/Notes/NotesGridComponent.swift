import Combine
import Foundation
import SwiftUI

struct NotesGridComponent: View {
    @State var active: (Int, Int) = (0, 0)
    let notesDir: String
    
    init(dir: String) {
        self.notesDir = dir
    }
     
    

    var body: some View {

        ScrollView {
            LazyVGrid(columns: getColumns(), spacing: 10) {  // spacing: Gap between items
                ForEach(loadNotes(), id: \.self.Path) { item in
                    NoteCard(
                        path: item.Path,
                        colWidth: 10
                    )
                    .frame(maxWidth: .infinity)
                    .background(Color.white).cornerRadius(2).overlay(

                        RoundedRectangle(cornerRadius: 2)
                            .stroke(
                                Color.blue,
                                lineWidth: self.active == (0, 0) ? 2 : 0
                            )
                    )
                }
            }.frame(
                minWidth: 0,
                maxWidth: .infinity,
                minHeight: 0,
                maxHeight: .infinity,
                alignment: .topLeading
            )
            .padding()
        }
        .padding()
        .frame(
            minWidth: 0,
            maxWidth: .infinity,
            minHeight: 0,
            maxHeight: .infinity,
            alignment: .topLeading
        ).background(Color.purple)
    }
    
    func loadNotes()->[NoteModel] {
        var notes: [NoteModel] = []
            print("NotesGridComponent init: \(self.notesDir)")
                let directoryURL = URL(fileURLWithPath: self.notesDir)
                let noteFiles = getNoteFileURLs(in: directoryURL)
                for file in noteFiles {
                    let noteModel = NoteModel(Path: file.path, Text: "新")
                    notes.append(noteModel)
                    print("Loaded note file: \(file.path)")
                }
                 
        return notes
    }
     
    func getColumns() -> [GridItem] {
        print("getColumns called: \(self.notesDir)")
        var cols: [GridItem] = []
        for _ in 0..<6 {
            cols.append(GridItem(.flexible()))
        }
        return cols
    }
}

class NoteViewModel: ObservableObject {
    init() {
        print("Model Created")
    }
    @Published var noteText: String = "图片备注"
    @Published var show: Bool = false
}

struct NoteCard: View {

    private var columnWidth: Double
    @State private var model: NoteModel
    @State private var show: Bool = false
    private var path: String

    @State private var windowRef: NSWindow?

    @ObservedObject var model2 = Model()

    init(path: String, colWidth: Double) {
        self.path = path
        self.columnWidth = colWidth
        self.model = NoteModel(Path: path, Text: "新")
        _show = State(initialValue: false)
        print("onAppear \(path)")
    }

    @MainActor func showWindow(imgPath: String) {
        if let existingWindow = windowRef {
            existingWindow.makeKeyAndOrderFront(nil)
            return
        }

        let newWindow = NSWindow(
            contentRect: NSRect(x: 0, y: 0, width: 600, height: 600),
            styleMask: [
                .titled, .closable, .miniaturizable, .resizable,
                .fullSizeContentView,
            ],
            backing: .buffered,
            defer: false
        )
        newWindow.contentView = NSHostingView(
            rootView: PSNoteComponent(notePath: imgPath)
        )

        newWindow.center()
        newWindow.isReleasedWhenClosed = false  // 阻止窗口关闭时被释放，否则可能遇到空指针错误
        newWindow.makeKeyAndOrderFront(nil)
        windowRef = newWindow  // Store the strong reference

    }

    func getNSNote() -> NoteModel {

        var notePath = self.path
        if !notePath.hasPrefix("/") {
            notePath = "Documents/\(self.model.Path)"
        }
        print("aaa \(self.model) \(notePath)")

        let nsImg = NoteModel(Path: notePath, Text: notePath)
        
return        nsImg

    }

    var body: some View {
        VStack(alignment: .center, spacing: nil) {

                Spacer(minLength: 0)
                HStack(alignment: .center) {
                    Spacer(minLength: 0)
                    Button(action: {
                        print("jjjdfs3")
                        self.model2.show.toggle()
                        self.show.toggle()
                        self.showWindow(imgPath: self.path)
                    }) {
                        Text(self.model.Path)
                    }.buttonStyle(EmptyButtonStyle())
                    Spacer(minLength: 0)
                }
                Spacer(minLength: 0)
                HStack {
                    Text("笔记备注").foregroundColor(Color.gray)
                        .font(Font.system(size: 10, design: .default))
                    Spacer()
                }.padding(2)
        }

    }

}

struct NoteWrapper {
    var width: CGFloat
    var height: CGFloat
    var filePath: String
}

struct NoteButtonStyle: ButtonStyle {
    func makeBody(configuration: Self.Configuration) -> some View {
        configuration.label
    }
}
