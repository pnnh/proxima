import AppKit
import Cocoa
import Logging
import MTKepler
import SwiftData
import SwiftUI

@main
struct KeplerApp: App {
    @NSApplicationDelegateAdaptor(AppDelegate.self) var appDelegate

    @ObservedObject var router = Router()
    var sharedModelContainer: ModelContainer = {
        let schema = Schema([
            Item.self
        ])
        let modelConfiguration = ModelConfiguration(
            schema: schema,
            isStoredInMemoryOnly: false
        )

        do {
            return try ModelContainer(
                for: schema,
                configurations: [modelConfiguration]
            )
        } catch {
            fatalError("Could not create ModelContainer: \(error)")
        }
    }()
    var body: some Scene {
        WindowGroup {
            NavigationStack(path: $router.navPath) {
                PSMainPage()
                    .navigationDestination(for: Router.Destination.self) {
                        destination in
                        switch destination {
                        case .files:
                            PSFilesView().navigationBarBackButtonHidden(true)
                        case .notes(let owner):
                            PSNotesPage(ownerName: owner)
                                .navigationBarBackButtonHidden(true)
                        case .images:
                            PSImagePage().navigationBarBackButtonHidden(true)
                        default:
                            PSMainPage().navigationBarBackButtonHidden(true)
                        }
                    }
            }.padding(0)
                .frame(
                    minWidth: 0,
                    maxWidth: .infinity,
                    minHeight: 0,
                    maxHeight: .infinity,
                    alignment: .topLeading
                )
                .environmentObject(router)
                .background(Color.gray)
        }
//        .windowResizability(.contentMinSize)
        .defaultSize(width: 1024, height: 768)
        .windowStyle(.hiddenTitleBar)
        .modelContainer(sharedModelContainer)

    }
}

class AppDelegate: NSObject, NSApplicationDelegate {

    func applicationDidFinishLaunching(_ notification: Notification) {
        // 尝试调用C++侧的日志打印
        MTKepler.kepler.Logger.LogInfo("Call From Swift")

        // 尝试调用SPM里的Swift-log来打印日志
        let logger = Logger(label: "xyz.huable.kepler.main")
        logger.info("Hello Kepler")

    }
}
