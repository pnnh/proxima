import Combine
import Foundation
import SwiftUI

struct PSNotesPage: View {
    @EnvironmentObject var router: Router
    var ownerName: String

    var body: some View {

        NotesGridComponent(dir: ownerName)
            .frame(
                minWidth: 0,
                maxWidth: .infinity,
                minHeight: 0,
                maxHeight: .infinity,
                alignment: .topLeading
            )
            .padding(0)
            .background(Color.yellow)
            .toolbar {
                ToolbarItemGroup {
                    Button {

                        router.navigate(to: .home)
                    } label: {
                        Label("New", systemImage: "house")
                    }
                }
            }.toolbarBackground(
                Color.green,
            )
    }

}
