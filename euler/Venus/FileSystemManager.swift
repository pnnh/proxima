//
//  FileSystemManager.swift
//  Venus
//
//  Created by Larry on 2024/10/25.
//

import SwiftUI
import Combine
import UniformTypeIdentifiers

// MARK: - FileSystemManager

/// 负责所有真实文件系统 I/O 操作，以 App Documents 目录为根目录。
@MainActor
final class FileSystemManager: ObservableObject {

    // MARK: Published

    /// 待导入文件的 URL（由外部通过 handleIncomingURL 注入）
    @Published var incomingURL: URL? = nil

    // MARK: 根目录

    static let rootURL: URL = {
        FileManager.default.urls(for: .documentDirectory, in: .userDomainMask)[0]
    }()

    private let fm = FileManager.default

    // MARK: Init

    init() {
        seedIfNeeded()
    }

    // MARK: - 初始化模拟数据

    /// 首次启动时将模拟目录 & 文件写入 Documents
    private func seedIfNeeded() {
        let sentinel = Self.rootURL.appendingPathComponent(".seeded")
        guard !fm.fileExists(atPath: sentinel.path) else { return }

        let tree: [(path: String, isDir: Bool, content: String?)] = [
            ("图片",                               true,  nil),
            ("图片/壁纸",                           true,  nil),
            ("图片/风景.jpg",                       false, placeholder("photo")),
            ("图片/肖像.png",                       false, placeholder("person.crop.rectangle")),
            ("图片/截图.png",                       false, placeholder("iphone")),
            ("图片/壁纸/山脉.jpg",                  false, placeholder("mountain.2.fill")),
            ("图片/壁纸/海滩.jpg",                  false, placeholder("water.waves")),
            ("文档",                               true,  nil),
            ("文档/工作文档",                        true,  nil),
            ("文档/README.md",                     false,
             "# 项目简介\n\n这是一个用 SwiftUI 构建的文件管理器。\n\n## 功能特性\n- 文件夹导航（支持多级目录）\n- 图片预览（支持捏合缩放、双击还原）\n- 文本文件预览\n- 文件导入 / 导出分享\n\n## 技术栈\n- SwiftUI\n- iOS 17+"),
            ("文档/笔记.txt",                       false,
             "今日待办：\n1. 完成 SwiftUI 文件管理器\n2. 整理设计稿\n3. 与团队同步进度\n\n想法记录：\n- 支持文件排序（按名称 / 大小 / 日期）\n- 长按文件显示操作菜单"),
            ("文档/工作文档/季度总结.txt",            false,
             "Q1 销售额：¥1,200,000\nQ2 目标：¥1,500,000\n\n主要亮点：\n- 新客户增长 23%\n- 复购率提升至 68%"),
            ("音乐",                               true,  nil),
            ("音乐/晨间冥想.mp3",                   false, ""),
            ("音乐/背景音乐.m4a",                   false, ""),
            ("视频",                               true,  nil),
            ("视频/演示录屏.mp4",                   false, ""),
            ("视频/教程.mov",                       false, ""),
            ("下载记录.txt",                        false,
             "2025-04-10  SwiftUI教程.pdf  已完成\n2025-04-09  Xcode16.dmg   已完成\n2025-04-08  素材包.zip     已完成"),
        ]

        do {
            for entry in tree {
                let url = Self.rootURL.appendingPathComponent(entry.path)
                if entry.isDir {
                    try fm.createDirectory(at: url, withIntermediateDirectories: true)
                } else if let text = entry.content {
                    try text.write(to: url, atomically: true, encoding: .utf8)
                }
            }
            fm.createFile(atPath: sentinel.path, contents: nil)
        } catch {
            print("[FileSystemManager] seed error: \(error)")
        }
    }

    /// 图片占位符内容：写入 SF Symbol 名称，读取时解析
    private func placeholder(_ symbol: String) -> String {
        "__symbol__\(symbol)"
    }

    // MARK: - 读取目录

    /// 返回指定 URL 下的直接子条目（已排序：文件夹优先，名称次之）
    func loadItems(at url: URL) throws -> [FileItem] {
        let contents = try fm.contentsOfDirectory(
            at: url,
            includingPropertiesForKeys: [.fileSizeKey, .contentModificationDateKey, .isDirectoryKey],
            options: [.skipsHiddenFiles]
        )
        return try contents.compactMap { try makeFileItem(url: $0) }
    }

    private func makeFileItem(url: URL) throws -> FileItem? {
        let resourceValues = try url.resourceValues(forKeys: [
            .fileSizeKey, .contentModificationDateKey, .isDirectoryKey
        ])
        let isDir    = resourceValues.isDirectory ?? false
        let fileSize = resourceValues.fileSize
        let modDate  = resourceValues.contentModificationDate ?? Date()

        let sizeStr: String? = fileSize.map { formatBytes($0) }
        let fileType = isDir ? FileType.folder : FileType.detect(url: url)

        return FileItem(
            url: url,
            name: url.lastPathComponent,
            fileType: fileType,
            size: sizeStr,
            modifiedDate: modDate
        )
    }

    // MARK: - 创建目录

    func createDirectory(named name: String, in parent: URL) throws {
        let dest = parent.appendingPathComponent(name)
        try fm.createDirectory(at: dest, withIntermediateDirectories: false)
    }

    // MARK: - 删除

    func delete(item: FileItem) throws {
        try fm.removeItem(at: item.url)
    }

    // MARK: - 重命名

    func rename(item: FileItem, to newName: String) throws {
        let dest = item.url.deletingLastPathComponent().appendingPathComponent(newName)
        try fm.moveItem(at: item.url, to: dest)
    }

    // MARK: - 导入（接收分享）

    /// 将外部 URL 的文件复制到指定目录
    func importFile(from sourceURL: URL, to destinationDir: URL) throws -> URL {
        let accessing = sourceURL.startAccessingSecurityScopedResource()
        defer { if accessing { sourceURL.stopAccessingSecurityScopedResource() } }

        var destName = sourceURL.lastPathComponent
        var destURL  = destinationDir.appendingPathComponent(destName)

        // 处理重名
        var idx = 1
        while fm.fileExists(atPath: destURL.path) {
            let base = (destName as NSString).deletingPathExtension
            let ext  = (destName as NSString).pathExtension
            let candidate = ext.isEmpty ? "\(base) \(idx)" : "\(base) \(idx).\(ext)"
            destURL = destinationDir.appendingPathComponent(candidate)
            idx += 1
        }

        try fm.copyItem(at: sourceURL, to: destURL)
        return destURL
    }

    // MARK: - 读取文件内容（用于预览）

    func readText(at url: URL) throws -> String {
        try String(contentsOf: url, encoding: .utf8)
    }

    func readImage(at url: URL) -> UIImage? {
        // 检查是否是占位符
        if let text = try? String(contentsOf: url, encoding: .utf8),
           text.hasPrefix("__symbol__") {
            let symbol = String(text.dropFirst("__symbol__".count))
            return UIImage(systemName: symbol)
        }
        return UIImage(contentsOfFile: url.path)
    }

    // MARK: - Helpers

    private func formatBytes(_ bytes: Int) -> String {
        let formatter = ByteCountFormatter()
        formatter.allowedUnits = [.useKB, .useMB, .useGB]
        formatter.countStyle   = .file
        return formatter.string(fromByteCount: Int64(bytes))
    }

    // MARK: - 接收外部文件

    func handleIncomingURL(_ url: URL) {
        incomingURL = url
    }
}

// MARK: - FileItem（基于真实 URL）

struct FileItem: Identifiable {
    let id   = UUID()
    let url  : URL
    var name : String
    var fileType    : FileType
    var size        : String?
    var modifiedDate: Date

    var isFolder: Bool { fileType == .folder }
    var ext: String { url.pathExtension.lowercased() }
}

// MARK: - FileType

enum FileType: Equatable {
    case folder
    case image
    case text
    case pdf
    case audio
    case video
    case unknown

    static func detect(url: URL) -> FileType {
        let ext = url.pathExtension.lowercased()
        switch ext {
        case "jpg", "jpeg", "png", "gif", "heic", "webp", "bmp", "tiff":
            // 也检测占位符文本文件（以 .jpg/.png 命名但内容是 symbol）
            return .image
        case "txt", "md", "markdown", "swift", "py", "js", "ts", "html",
             "css", "json", "xml", "yaml", "yml", "sh", "c", "cpp", "h":
            return .text
        case "pdf":
            return .pdf
        case "mp3", "m4a", "aac", "wav", "flac", "ogg":
            return .audio
        case "mp4", "mov", "m4v", "avi", "mkv":
            return .video
        default:
            return .unknown
        }
    }

    var iconName: String {
        switch self {
        case .folder:  return "folder.fill"
        case .image:   return "photo.fill"
        case .text:    return "doc.text.fill"
        case .pdf:     return "doc.richtext.fill"
        case .audio:   return "music.note"
        case .video:   return "film.fill"
        case .unknown: return "doc.fill"
        }
    }

    var iconColor: Color {
        switch self {
        case .folder:  return .blue
        case .image:   return .green
        case .text:    return .gray
        case .pdf:     return .red
        case .audio:   return .purple
        case .video:   return .orange
        case .unknown: return Color(.secondaryLabel)
        }
    }
}
