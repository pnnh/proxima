import Combine
import Foundation
import SwiftUI

struct ImagesGridComponent: View {
    @State var active: (Int, Int) = (0, 0)
    @ObservedObject var viewModel: SharedViewModel = SharedViewModel()
     
    var body: some View {

        ScrollView {
            LazyVGrid(columns: getColumns(), spacing: 10) {  // spacing: Gap between items
                ForEach(loadImages(), id: \.self.Path) { item in
                    ImageCard(
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
                Spacer()
            }.background(Color.red)
            .padding(0)
            Spacer()
        }.frame(
            minWidth: 0,
            maxWidth: .infinity,
            minHeight: 0,
            maxHeight: .infinity,
            alignment: .topLeading
        )
        .padding(0)
        .background(Color.brown)
    }
    
   
   func loadImages()->[ImageModel] {
       var images: [ImageModel] = []
           print("ImagesGridComponent init: \(viewModel.selectedImagePath ?? "default value")")
           if let selectedPath = viewModel.selectedImagePath, !selectedPath.isEmpty {
           
               let directoryURL = URL(fileURLWithPath: selectedPath)
               let imageFiles = getImageFileURLs(in: directoryURL)
               for file in imageFiles {
                   let imageModel = ImageModel(Path: file.path, Text: "新")
                   images.append(imageModel)
                   print("Loaded image file: \(file.path)")
               }
               
           } else {
               images.append(ImageModel(Path: "/Users/Larry/Pictures/bear.jpg", Text: "新"))
               images.append(ImageModel(Path: "/Users/Larry/Pictures/dog.png", Text: "新"))
               images.append(ImageModel(Path: "/Users/Larry/Pictures/cplus.jpg", Text: "新"))
               print("No valid selectedImagePath, loaded default images.")
           }
       return images
   }
    
   func getColumns() -> [GridItem] {
       print("getColumns called: \(viewModel.selectedImagePath ?? "default value")")
       var cols: [GridItem] = []
       for _ in 0..<6 {
           cols.append(GridItem(.flexible()))
       }
       return cols
   }

}

class Model: ObservableObject {
    init() {
        print("Model Created")
    }
    @Published var imageText: String = "图片备注"
    @Published var show: Bool = false
}

struct ImageCard: View {

    private var columnWidth: Double
    @State private var model: ImageModel
    @State private var show: Bool = false
    private var path: String

    @State private var windowRef: NSWindow?

    @ObservedObject var model2 = Model()

    init(path: String, colWidth: Double) {
        self.path = path
        self.columnWidth = colWidth
        self.model = ImageModel(Path: path, Text: "新")
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
            rootView: PSImageComponent(imagePath: imgPath)
        )

        newWindow.center()
        newWindow.isReleasedWhenClosed = false  // 阻止窗口关闭时被释放，否则可能遇到空指针错误
        newWindow.makeKeyAndOrderFront(nil)
        windowRef = newWindow  // Store the strong reference

    }

    func getNSImage() -> ImageWrapper? {

        if model.Path == "" {
            return nil
        }
        var imgPath = self.path
        if !imgPath.hasPrefix("/") {
            imgPath = "Documents/\(self.model.Path)"
        }
        print("aaa \(self.model) \(imgPath)")

        if let nsImg = NSImage(contentsOfFile: imgPath) {
            //print("width height \(nsImg.size.width) \(nsImg.size.height)")
            let width = CGFloat(columnWidth)
            let height =
                CGFloat(columnWidth) / nsImg.size.width * nsImg.size.height
            //print("width2 \(height)")

            return ImageWrapper(
                Image: nsImg,
                width: width,
                height: height,
                filePath: imgPath
            )

        }

        return nil

    }

    var body: some View {
        VStack(alignment: .center, spacing: nil) {

            if let nsImg = getNSImage() {
                Spacer(minLength: 0)
                HStack(alignment: .center) {
                    Spacer(minLength: 0)
                    Button(action: {
                        print("jjjdfs3")
                        self.model2.show.toggle()
                        self.show.toggle()
                        self.showWindow(imgPath: self.path)
                    }) {
                        Image(nsImage: nsImg.Image)
                            .resizable()
                            .aspectRatio(contentMode: .fit)  //.background(Color.pink)
//                            .frame(maxWidth: .infinity)  //.border(Color.gray, width: 0.5)
                        //.frame(width: nsImg.width, height: nsImg.height)

                    }.buttonStyle(EmptyButtonStyle())
                    Spacer(minLength: 0)
                }  //
                Spacer(minLength: 0)
                HStack {
                    Text("图片备注").foregroundColor(Color.gray)
                        .font(Font.system(size: 10, design: .default))
                    Spacer()
                }.padding(2)  //.frame(height:20)
            }
        }

    }

}

struct ImageWrapper {
    var Image: NSImage
    var width: CGFloat
    var height: CGFloat
    var filePath: String
}

struct EmptyButtonStyle: ButtonStyle {
    func makeBody(configuration: Self.Configuration) -> some View {
        configuration.label
    }
}
