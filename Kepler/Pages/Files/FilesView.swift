import Combine
import Foundation
import MTKepler
import SwiftUI

struct PSFilesView: View {
    @EnvironmentObject var router: Router

    var body: some View {
        VStack {
            PSNavbarComponent()

            HStack {
                PSFilesNavComponent()
            }
        }
        .frame(
            minWidth: 0,
            maxWidth: .infinity,
            minHeight: 0,
            maxHeight: .infinity,
            alignment: .topLeading
        ).padding(0)
        .background(Color.purple)
    }

}

struct PSFilesNavComponent: View {
    var body: some View {
        VStack {
            Text("位置")
            HStack {
                Text("主目录")
            } .onTapGesture {
//                let fileService = MTKepler.quark.FileServerBusiness("")
//                fileService.selectFilesVector()
                print("点击主目录")
            }
        }
    }
}

#Preview {
    PSFilesView()
        .modelContainer(for: Item.self, inMemory: true)
}
