//
//  ContentView.swift
//  Venus
//
//  Created by Larry on 2024/10/25.
//

import SwiftUI

struct ContentView: View {

    @EnvironmentObject private var fsManager: FileSystemManager

    var body: some View {
        NavigationStack {
            FileListView(directoryURL: FileSystemManager.rootURL, title: "文件")
                .environmentObject(fsManager)
        }
        // 接收其他 App 分享过来的文件
        .sheet(item: $fsManager.incomingURL) { url in
            ImportFileView(incomingURL: url)
                .environmentObject(fsManager)
        }
    }
}

// MARK: - 让 URL 满足 Identifiable（用于 sheet(item:)）
extension URL: @retroactive Identifiable {
    public var id: String { absoluteString }
}

#Preview {
    ContentView()
        .environmentObject(FileSystemManager())
}
