//
//  FileItem.swift
//  Venus
//
//  Created by Larry on 2024/10/25.
//

import SwiftUI

// MARK: - 文件类型

enum FileType {
    case folder
    case image(String)   // SF Symbol name
    case text
    case pdf
    case audio
    case video
    case unknown

    var iconName: String {
        switch self {
        case .folder:          return "folder.fill"
        case .image:           return "photo.fill"
        case .text:            return "doc.text.fill"
        case .pdf:             return "doc.richtext.fill"
        case .audio:           return "music.note"
        case .video:           return "film.fill"
        case .unknown:         return "doc.fill"
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
        case .unknown: return .secondary
        }
    }
}

// MARK: - 文件内容（模拟）

enum FileContent {
    case folder([FileItem])
    case image(String)       // SF Symbol or system image name (模拟图片)
    case text(String)
    case unsupported
}

// MARK: - 文件条目

struct FileItem: Identifiable {
    let id = UUID()
    var name: String
    var fileType: FileType
    var content: FileContent
    var size: String?         // e.g. "128 KB"
    var modifiedDate: Date

    var isFolder: Bool {
        if case .folder = fileType { return true }
        return false
    }

    /// 文件扩展名
    var ext: String {
        (name as NSString).pathExtension.lowercased()
    }
}

// MARK: - 模拟数据

extension FileItem {

    /// 递归生成模拟文件树
    static var mockRoot: [FileItem] {
        [
            FileItem(
                name: "图片",
                fileType: .folder,
                content: .folder([
                    FileItem(name: "风景.jpg",   fileType: .image("photo"), content: .image("photo"),                           size: "2.4 MB",  modifiedDate: date(2025, 3, 1)),
                    FileItem(name: "肖像.png",   fileType: .image("person.crop.rectangle"), content: .image("person.crop.rectangle"), size: "1.1 MB",  modifiedDate: date(2025, 2, 14)),
                    FileItem(name: "截图.png",   fileType: .image("iphone"), content: .image("iphone"),                         size: "512 KB",  modifiedDate: date(2025, 4, 10)),
                    FileItem(name: "壁纸",
                             fileType: .folder,
                             content: .folder([
                                FileItem(name: "山脉.jpg", fileType: .image("mountain.2"), content: .image("mountain.2"),        size: "3.2 MB",  modifiedDate: date(2025, 1, 20)),
                                FileItem(name: "海滩.jpg", fileType: .image("water.waves"), content: .image("water.waves"),      size: "2.8 MB",  modifiedDate: date(2025, 1, 21)),
                             ]),
                             size: nil,
                             modifiedDate: date(2025, 1, 20))
                ]),
                size: nil,
                modifiedDate: date(2025, 4, 10)
            ),
            FileItem(
                name: "文档",
                fileType: .folder,
                content: .folder([
                    FileItem(name: "README.md",
                             fileType: .text,
                             content: .text("""
# 项目简介

这是一个用 SwiftUI 构建的模拟文件管理器。

## 功能特性
- 文件夹导航（支持多级目录）
- 图片预览（支持捏合缩放、双击还原）
- 文本文件预览
- 文件信息展示

## 技术栈
- SwiftUI
- Combine
- iOS 17+
"""),
                             size: "1.2 KB",
                             modifiedDate: date(2025, 3, 15)),
                    FileItem(name: "笔记.txt",
                             fileType: .text,
                             content: .text("""
今日待办：
1. 完成 SwiftUI 文件管理器原型
2. 整理设计稿
3. 与团队同步进度

想法记录：
- 可以考虑添加搜索功能
- 支持文件排序（按名称 / 大小 / 日期）
- 长按文件显示操作菜单（重命名、删除、分享）
"""),
                             size: "320 B",
                             modifiedDate: date(2025, 4, 5)),
                    FileItem(name: "报告.pdf",  fileType: .pdf,     content: .unsupported, size: "4.5 MB",  modifiedDate: date(2025, 2, 28)),
                    FileItem(name: "工作文档",
                             fileType: .folder,
                             content: .folder([
                                FileItem(name: "季度总结.txt",
                                         fileType: .text,
                                         content: .text("Q1 销售额：¥1,200,000\nQ2 目标：¥1,500,000\n\n主要亮点：\n- 新客户增长 23%\n- 复购率提升至 68%"),
                                         size: "256 B",
                                         modifiedDate: date(2025, 4, 1)),
                                FileItem(name: "合同模板.pdf", fileType: .pdf, content: .unsupported, size: "128 KB", modifiedDate: date(2025, 3, 10)),
                             ]),
                             size: nil,
                             modifiedDate: date(2025, 4, 1))
                ]),
                size: nil,
                modifiedDate: date(2025, 4, 5)
            ),
            FileItem(
                name: "音乐",
                fileType: .folder,
                content: .folder([
                    FileItem(name: "晨间冥想.mp3", fileType: .audio, content: .unsupported, size: "8.3 MB",  modifiedDate: date(2025, 1, 5)),
                    FileItem(name: "背景音乐.m4a", fileType: .audio, content: .unsupported, size: "5.1 MB",  modifiedDate: date(2025, 2, 20)),
                ]),
                size: nil,
                modifiedDate: date(2025, 2, 20)
            ),
            FileItem(
                name: "视频",
                fileType: .folder,
                content: .folder([
                    FileItem(name: "演示录屏.mp4", fileType: .video, content: .unsupported, size: "120 MB", modifiedDate: date(2025, 4, 8)),
                    FileItem(name: "教程.mov",    fileType: .video, content: .unsupported, size: "250 MB", modifiedDate: date(2025, 3, 22)),
                ]),
                size: nil,
                modifiedDate: date(2025, 4, 8)
            ),
            FileItem(name: "下载记录.txt",
                     fileType: .text,
                     content: .text("2025-04-10  SwiftUI教程.pdf  已完成\n2025-04-09  Xcode16.dmg   已完成\n2025-04-08  素材包.zip     已完成"),
                     size: "180 B",
                     modifiedDate: date(2025, 4, 10)),
            FileItem(name: "未知文件.bin",  fileType: .unknown, content: .unsupported, size: "64 KB",  modifiedDate: date(2025, 1, 1)),
        ]
    }

    // MARK: - Helper
    private static func date(_ year: Int, _ month: Int, _ day: Int) -> Date {
        var c = DateComponents()
        c.year = year; c.month = month; c.day = day
        return Calendar.current.date(from: c) ?? Date()
    }
}
