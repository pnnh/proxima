//
//  VenusApp.swift
//  Venus
//
//  Created by Larry on 2024/10/25.
//

import SwiftUI

@main
struct VenusApp: App {

    @StateObject private var fsManager = FileSystemManager()

    var body: some Scene {
        WindowGroup {
            ContentView()
                .environmentObject(fsManager)
                // 接收其他 App 通过 "Open In" / Share Sheet 传入的文件
                .onOpenURL { url in
                    fsManager.handleIncomingURL(url)
                }
        }
    }
}
