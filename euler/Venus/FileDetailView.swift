//
//  FileDetailView.swift
//  Venus
//
//  Created by Larry on 2024/10/25.
//

import SwiftUI

/// 根据文件类型路由到对应的预览视图
struct FileDetailView: View {

    let item: FileItem
    @EnvironmentObject private var fsManager: FileSystemManager

    // 分享
    @State private var showShareSheet = false

    private static let dateFormatter: DateFormatter = {
        let f = DateFormatter()
        f.dateStyle = .long
        f.timeStyle = .short
        f.locale = Locale(identifier: "zh_CN")
        return f
    }()

    var body: some View {
        Group {
            switch item.fileType {
            case .image:
                ImagePreviewView(url: item.url, fileName: item.name)
            case .text:
                TextPreviewView(url: item.url, fileName: item.name)
            case .folder:
                EmptyView()
            default:
                unsupportedView
            }
        }
        .toolbar {
            ToolbarItem(placement: .navigationBarTrailing) {
                Button {
                    showShareSheet = true
                } label: {
                    Image(systemName: "square.and.arrow.up")
                }
            }
        }
        .sheet(isPresented: $showShareSheet) {
            ShareSheetView(url: item.url)
        }
    }

    // MARK: - 不支持预览的文件

    private var unsupportedView: some View {
        VStack(spacing: 24) {
            ZStack {
                RoundedRectangle(cornerRadius: 24)
                    .fill(item.fileType.iconColor.opacity(0.12))
                    .frame(width: 100, height: 100)
                Image(systemName: item.fileType.iconName)
                    .font(.system(size: 50))
                    .foregroundStyle(item.fileType.iconColor)
            }

            Text(item.name)
                .font(.title2.bold())
                .multilineTextAlignment(.center)

            VStack(spacing: 0) {
                infoRow(label: "类型", value: fileTypeLabel)
                Divider().padding(.leading, 16)
                if let size = item.size {
                    infoRow(label: "大小", value: size)
                    Divider().padding(.leading, 16)
                }
                infoRow(label: "修改日期", value: Self.dateFormatter.string(from: item.modifiedDate))
                Divider().padding(.leading, 16)
                infoRow(label: "路径", value: item.url.path)
            }
            .background(Color(.secondarySystemGroupedBackground))
            .clipShape(RoundedRectangle(cornerRadius: 12))
            .padding(.horizontal)

            Text("此文件类型暂不支持预览")
                .font(.subheadline)
                .foregroundStyle(.secondary)
        }
        .padding()
        .frame(maxWidth: .infinity, maxHeight: .infinity)
        .background(Color(.systemGroupedBackground))
        .navigationTitle(item.name)
        .navigationBarTitleDisplayMode(.inline)
    }

    private func infoRow(label: String, value: String) -> some View {
        HStack(alignment: .top) {
            Text(label)
                .font(.subheadline)
                .foregroundStyle(.secondary)
                .frame(width: 70, alignment: .leading)
            Spacer()
            Text(value)
                .font(.subheadline)
                .foregroundStyle(.primary)
                .multilineTextAlignment(.trailing)
        }
        .padding(.horizontal, 16)
        .padding(.vertical, 12)
    }

    private var fileTypeLabel: String {
        switch item.fileType {
        case .folder:  return "文件夹"
        case .image:   return "图片 (\(item.ext.uppercased()))"
        case .text:    return "文本文件 (\(item.ext.uppercased()))"
        case .pdf:     return "PDF 文档"
        case .audio:   return "音频文件 (\(item.ext.uppercased()))"
        case .video:   return "视频文件 (\(item.ext.uppercased()))"
        case .unknown: return item.ext.isEmpty ? "未知类型" : item.ext.uppercased()
        }
    }
}

// MARK: - Preview

#Preview("文本文件") {
    NavigationStack {
        FileDetailView(item: FileItem(
            url: FileSystemManager.rootURL.appendingPathComponent("文档/README.md"),
            name: "README.md",
            fileType: .text,
            size: "1.2 KB",
            modifiedDate: Date()
        ))
    }
    .environmentObject(FileSystemManager())
}

#Preview("不支持预览") {
    NavigationStack {
        FileDetailView(item: FileItem(
            url: FileSystemManager.rootURL.appendingPathComponent("报告.pdf"),
            name: "报告.pdf",
            fileType: .pdf,
            size: "4.5 MB",
            modifiedDate: Date()
        ))
    }
    .environmentObject(FileSystemManager())
}
