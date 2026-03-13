//
//  ImagePreviewView.swift
//  Venus
//
//  Created by Larry on 2024/10/25.
//

import SwiftUI

struct ImagePreviewView: View {

    let symbolName: String      // 用 SF Symbol 模拟图片
    let fileName: String

    // 缩放 & 拖拽状态
    @State private var scale: CGFloat = 1.0
    @State private var lastScale: CGFloat = 1.0
    @State private var offset: CGSize = .zero
    @State private var lastOffset: CGSize = .zero

    // 工具栏显示
    @State private var showBars = true

    private let minScale: CGFloat = 1.0
    private let maxScale: CGFloat = 5.0

    var body: some View {
        GeometryReader { geo in
            ZStack {
                Color.black.ignoresSafeArea()

                imageContent(in: geo)
            }
            .gesture(tapGesture)
            .navigationTitle(fileName)
            .navigationBarTitleDisplayMode(.inline)
            .toolbarBackground(showBars ? .visible : .hidden, for: .navigationBar)
            .toolbar(showBars ? .visible : .hidden, for: .navigationBar)
            .statusBarHidden(!showBars)
            .animation(.easeInOut(duration: 0.2), value: showBars)
        }
    }

    // MARK: - 图片内容

    @ViewBuilder
    private func imageContent(in geo: GeometryProxy) -> some View {
        Image(systemName: symbolName)
            .resizable()
            .scaledToFit()
            .foregroundStyle(.white.opacity(0.85))
            .padding(40)
            .scaleEffect(scale)
            .offset(offset)
            // 捏合缩放
            .gesture(
                MagnificationGesture()
                    .onChanged { value in
                        let proposed = lastScale * value
                        scale = min(max(proposed, minScale), maxScale)
                    }
                    .onEnded { _ in
                        lastScale = scale
                        if scale <= minScale {
                            withAnimation(.spring()) {
                                scale = minScale
                                offset = .zero
                            }
                            lastScale = minScale
                            lastOffset = .zero
                        }
                    }
            )
            // 拖动平移（仅放大后允许）
            .simultaneousGesture(
                DragGesture()
                    .onChanged { value in
                        guard scale > 1.01 else { return }
                        offset = CGSize(
                            width:  lastOffset.width  + value.translation.width,
                            height: lastOffset.height + value.translation.height
                        )
                    }
                    .onEnded { _ in
                        lastOffset = offset
                    }
            )
    }

    // MARK: - 手势

    /// 单击：切换工具栏显示
    /// 双击：在 1x 和 2.5x 之间切换
    private var tapGesture: some Gesture {
        let doubleTap = TapGesture(count: 2)
            .onEnded {
                withAnimation(.spring(response: 0.35, dampingFraction: 0.7)) {
                    if scale > 1.5 {
                        scale = minScale
                        offset = .zero
                        lastScale = minScale
                        lastOffset = .zero
                    } else {
                        scale = 2.5
                        lastScale = 2.5
                    }
                }
            }

        let singleTap = TapGesture(count: 1)
            .onEnded {
                withAnimation { showBars.toggle() }
            }

        return doubleTap.exclusively(before: singleTap)
    }
}

// MARK: - Preview

#Preview {
    NavigationStack {
        ImagePreviewView(symbolName: "photo", fileName: "风景.jpg")
    }
}
