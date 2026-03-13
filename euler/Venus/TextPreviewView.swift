//
//  TextPreviewView.swift
//  Venus
//
//  Created by Larry on 2024/10/25.
//

import SwiftUI

struct TextPreviewView: View {

    let url: URL
    let fileName: String

    @EnvironmentObject private var fsManager: FileSystemManager

    @State private var text: String = ""
    @State private var loadError: String? = nil
    @State private var fontSize: CGFloat = 15
    @State private var showCopiedToast = false

    private let minFontSize: CGFloat = 11
    private let maxFontSize: CGFloat = 28

    var body: some View {
        Group {
            if let err = loadError {
                VStack(spacing: 12) {
                    Image(systemName: "doc.badge.exclamationmark")
                        .font(.system(size: 48)).foregroundStyle(.secondary)
                    Text("无法读取文件")
                        .font(.headline)
                    Text(err).font(.caption).foregroundStyle(.secondary)
                        .multilineTextAlignment(.center)
                }
                .padding()
            } else {
                ScrollView {
                    Text(text)
                        .font(.system(size: fontSize, design: .monospaced))
                        .foregroundStyle(.primary)
                        .frame(maxWidth: .infinity, alignment: .leading)
                        .padding()
                        .textSelection(.enabled)
                }
                .background(Color(.systemGroupedBackground))
            }
        }
        .navigationTitle(fileName)
        .navigationBarTitleDisplayMode(.inline)
        .toolbar {
            ToolbarItemGroup(placement: .navigationBarTrailing) {
                Button {
                    withAnimation { fontSize = max(fontSize - 2, minFontSize) }
                } label: {
                    Image(systemName: "textformat.size.smaller")
                }
                .disabled(fontSize <= minFontSize)

                Button {
                    withAnimation { fontSize = min(fontSize + 2, maxFontSize) }
                } label: {
                    Image(systemName: "textformat.size.larger")
                }
                .disabled(fontSize >= maxFontSize)

                Button {
                    UIPasteboard.general.string = text
                    withAnimation { showCopiedToast = true }
                    DispatchQueue.main.asyncAfter(deadline: .now() + 1.5) {
                        withAnimation { showCopiedToast = false }
                    }
                } label: {
                    Image(systemName: "doc.on.doc")
                }
            }
        }
        .overlay(alignment: .bottom) {
            if showCopiedToast {
                Label("已复制到剪贴板", systemImage: "checkmark.circle.fill")
                    .font(.subheadline.bold())
                    .foregroundStyle(.white)
                    .padding(.horizontal, 16).padding(.vertical, 10)
                    .background(Capsule().fill(Color.black.opacity(0.75)))
                    .padding(.bottom, 30)
                    .transition(.move(edge: .bottom).combined(with: .opacity))
            }
        }
        .onAppear { loadText() }
    }

    private func loadText() {
        DispatchQueue.global(qos: .userInitiated).async {
            do {
                let content = try fsManager.readText(at: url)
                DispatchQueue.main.async { self.text = content }
            } catch {
                DispatchQueue.main.async { self.loadError = error.localizedDescription }
            }
        }
    }
}

// MARK: - Preview

#Preview {
    NavigationStack {
        TextPreviewView(
            url: FileSystemManager.rootURL.appendingPathComponent("文档/README.md"),
            fileName: "README.md"
        )
        .environmentObject(FileSystemManager())
    }
}
