//
//  ImagePreviewView.swift
//  Venus
//
//  Created by Larry on 2024/10/25.
//

import SwiftUI

struct ImagePreviewView: View {

    let url: URL
    let fileName: String

    @EnvironmentObject private var fsManager: FileSystemManager

    @State private var image: UIImage? = nil
    @State private var loadError = false

    // 缩放 & 拖拽状态
    @State private var scale: CGFloat = 1.0
    @State private var lastScale: CGFloat = 1.0
    @State private var offset: CGSize = .zero
    @State private var lastOffset: CGSize = .zero
    @State private var showBars = true

    private let minScale: CGFloat = 1.0
    private let maxScale: CGFloat = 5.0

    var body: some View {
        GeometryReader { geo in
            ZStack {
                Color.black.ignoresSafeArea()

                if let img = image {
                    imageContent(img, in: geo)
                } else if loadError {
                    VStack(spacing: 12) {
                        Image(systemName: "photo.badge.exclamationmark")
                            .font(.system(size: 48))
                            .foregroundStyle(.secondary)
                        Text("无法加载图片")
                            .foregroundStyle(.secondary)
                    }
                } else {
                    ProgressView().tint(.white)
                }
            }
            .gesture(tapGesture)
        }
        .navigationTitle(fileName)
        .navigationBarTitleDisplayMode(.inline)
        .toolbarBackground(showBars ? .visible : .hidden, for: .navigationBar)
        .toolbar(showBars ? .visible : .hidden, for: .navigationBar)
        .statusBarHidden(!showBars)
        .animation(.easeInOut(duration: 0.2), value: showBars)
        .onAppear { loadImage() }
    }

    // MARK: - 图片内容

    @ViewBuilder
    private func imageContent(_ img: UIImage, in geo: GeometryProxy) -> some View {
        Image(uiImage: img)
            .resizable()
            .scaledToFit()
            .scaleEffect(scale)
            .offset(offset)
            .gesture(
                MagnificationGesture()
                    .onChanged { value in
                        scale = min(max(lastScale * value, minScale), maxScale)
                    }
                    .onEnded { _ in
                        lastScale = scale
                        if scale <= minScale {
                            withAnimation(.spring()) { scale = minScale; offset = .zero }
                            lastScale = minScale; lastOffset = .zero
                        }
                    }
            )
            .simultaneousGesture(
                DragGesture()
                    .onChanged { value in
                        guard scale > 1.01 else { return }
                        offset = CGSize(
                            width: lastOffset.width + value.translation.width,
                            height: lastOffset.height + value.translation.height
                        )
                    }
                    .onEnded { _ in lastOffset = offset }
            )
    }

    // MARK: - 手势

    private var tapGesture: some Gesture {
        let doubleTap = TapGesture(count: 2).onEnded {
            withAnimation(.spring(response: 0.35, dampingFraction: 0.7)) {
                if scale > 1.5 {
                    scale = minScale; offset = .zero
                    lastScale = minScale; lastOffset = .zero
                } else {
                    scale = 2.5; lastScale = 2.5
                }
            }
        }
        let singleTap = TapGesture(count: 1).onEnded {
            withAnimation { showBars.toggle() }
        }
        return doubleTap.exclusively(before: singleTap)
    }

    // MARK: - 加载

    private func loadImage() {
        DispatchQueue.global(qos: .userInitiated).async {
            let img = fsManager.readImage(at: url)
            DispatchQueue.main.async {
                if let img {
                    self.image = img
                } else {
                    self.loadError = true
                }
            }
        }
    }
}

#Preview {
    NavigationStack {
        ImagePreviewView(url: FileSystemManager.rootURL.appendingPathComponent("图片/风景.jpg"),
                         fileName: "风景.jpg")
            .environmentObject(FileSystemManager())
    }
}
