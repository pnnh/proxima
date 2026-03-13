//
//  ContentView.swift
//  Venus
//
//  Created by Larry on 2024/10/25.
//

import SwiftUI

struct ContentView: View {
    var body: some View {
        NavigationStack {
            FileListView(title: "文件", items: FileItem.mockRoot)
        }
    }
}

#Preview {
    ContentView()
}
