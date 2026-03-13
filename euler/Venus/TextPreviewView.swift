//
//  TextPreviewView.swift
//  Venus
//
//  Created by Larry on 2024/10/25.
//

import SwiftUI

struct TextPreviewView: View {

    let text: String
    let fileName: String

    @State private var fontSize: CGFloat = 15
    @State private var showCopiedToast = false

    private let minFontSize: CGFloat = 11
    private let maxFontSize: CGFloat = 28

    var body: some View {
        ScrollView {
            Text(text)
                .font(.system(size: fontSize, design: .monospaced))
                .foregroundStyle(.primary)
                .frame(maxWidth: .infinity, alignment: .leading)
                .padding()
                .textSelection(.enabled)
        }
        .background(Color(.systemGroupedBackground))
        .navigationTitle(fileName)
        .navigationBarTitleDisplayMode(.inline)
        .toolbar {
            ToolbarItemGroup(placement: .navigationBarTrailing) {
                // 字体缩小
                Button {
                    withAnimation { fontSize = max(fontSize - 2, minFontSize) }
                } label: {
                    Image(systemName: "textformat.size.smaller")
                }
                .disabled(fontSize <= minFontSize)

                // 字体放大
                Button {
                    withAnimation { fontSize = min(fontSize + 2, maxFontSize) }
                } label: {
                    Image(systemName: "textformat.size.larger")
                }
                .disabled(fontSize >= maxFontSize)

                // 复制全文
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
                toastView
                    .padding(.bottom, 30)
                    .transition(.move(edge: .bottom).combined(with: .opacity))
            }
        }
    }

    private var toastView: some View {
        Label("已复制到剪贴板", systemImage: "checkmark.circle.fill")
            .font(.subheadline.bold())
            .foregroundStyle(.white)
            .padding(.horizontal, 16)
            .padding(.vertical, 10)
            .background(
                Capsule().fill(Color.black.opacity(0.75))
            )
    }
}

// MARK: - Preview

#Preview {
    NavigationStack {
        TextPreviewView(
            text: "# Hello\n\n这是一段示例文本内容。\n\n- 支持字体大小调节\n- 支持全文复制\n- 支持文本选择",
            fileName: "README.md"
        )
    }
}
