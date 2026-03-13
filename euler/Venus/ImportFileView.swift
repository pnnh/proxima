//
//  ImportFileView.swift
//  Venus
//
//  Created by Larry on 2024/10/25.
//

import SwiftUI

/// 接收其他 App 分享过来的文件时弹出的导入界面
/// 允许选择目标目录或新建子目录，然后执行复制导入
struct ImportFileView: View {

    let incomingURL: URL
    @EnvironmentObject private var fsManager: FileSystemManager
    @Environment(\.dismiss) private var dismiss

    // 当前浏览到的目录（从根目录开始）
    @State private var currentDir: URL = FileSystemManager.rootURL
    @State private var currentTitle: String = "文件"
    @State private var dirStack: [(url: URL, title: String)] = []

    @State private var items: [FileItem] = []
    @State private var loadError: String? = nil

    // 新建文件夹
    @State private var showNewFolderAlert = false
    @State private var newFolderName = ""

    // 导入结果
    @State private var importError: String? = nil
    @State private var showImportError = false
    @State private var importSuccess = false

    var body: some View {
        NavigationStack {
            VStack(spacing: 0) {
                // 顶部文件信息
                incomingFileInfo

                Divider()

                // 面包屑导航
                breadcrumb

                Divider()

                // 目录列表（只显示文件夹）
                folderList
            }
            .navigationTitle("选择保存位置")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .cancellationAction) {
                    Button("取消") { dismiss() }
                }
                ToolbarItem(placement: .confirmationAction) {
                    Button("存储到此处") {
                        importFile(to: currentDir)
                    }
                    .fontWeight(.semibold)
                }
                ToolbarItem(placement: .bottomBar) {
                    Button {
                        newFolderName = ""
                        showNewFolderAlert = true
                    } label: {
                        Label("新建文件夹", systemImage: "folder.badge.plus")
                    }
                }
            }
            .alert("新建文件夹", isPresented: $showNewFolderAlert) {
                TextField("文件夹名称", text: $newFolderName)
                Button("取消", role: .cancel) { newFolderName = "" }
                Button("创建") { createFolder() }
            } message: {
                Text("将在当前位置创建新文件夹")
            }
            .alert("导入失败", isPresented: $showImportError) {
                Button("好") {}
            } message: {
                Text(importError ?? "未知错误")
            }
        }
        .onAppear { loadItems() }
        .interactiveDismissDisabled(importSuccess)
    }

    // MARK: - 顶部文件信息卡片

    private var incomingFileInfo: some View {
        HStack(spacing: 14) {
            Image(systemName: FileType.detect(url: incomingURL).iconName)
                .font(.system(size: 28))
                .foregroundStyle(FileType.detect(url: incomingURL).iconColor)
                .frame(width: 44, height: 44)
                .background(FileType.detect(url: incomingURL).iconColor.opacity(0.12))
                .clipShape(RoundedRectangle(cornerRadius: 10))

            VStack(alignment: .leading, spacing: 2) {
                Text(incomingURL.lastPathComponent)
                    .font(.headline)
                    .lineLimit(1)
                Text("即将导入到所选文件夹")
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }
            Spacer()
        }
        .padding(.horizontal)
        .padding(.vertical, 12)
        .background(Color(.secondarySystemGroupedBackground))
    }

    // MARK: - 面包屑

    private var breadcrumb: some View {
        ScrollView(.horizontal, showsIndicators: false) {
            HStack(spacing: 4) {
                // 根目录
                crumbButton(title: "文件", isRoot: true) {
                    navigateToRoot()
                }

                ForEach(Array(dirStack.enumerated()), id: \.offset) { idx, entry in
                    Image(systemName: "chevron.right")
                        .font(.caption2)
                        .foregroundStyle(.tertiary)
                    crumbButton(title: entry.title, isRoot: false) {
                        navigateTo(stackIndex: idx)
                    }
                }

                // 当前目录（若非根目录）
                if !dirStack.isEmpty {
                    Image(systemName: "chevron.right")
                        .font(.caption2)
                        .foregroundStyle(.tertiary)
                    Text(currentTitle)
                        .font(.subheadline.bold())
                        .foregroundStyle(.primary)
                }
            }
            .padding(.horizontal)
            .padding(.vertical, 10)
        }
        .background(Color(.systemGroupedBackground))
    }

    private func crumbButton(title: String, isRoot: Bool, action: @escaping () -> Void) -> some View {
        Button(action: action) {
            Text(isRoot && dirStack.isEmpty ? title : title)
                .font(.subheadline)
                .foregroundColor(dirStack.isEmpty && isRoot ? .primary : .blue)
        }
        .disabled(dirStack.isEmpty && isRoot)
    }

    // MARK: - 文件夹列表

    private var folderList: some View {
        List {
            if let err = loadError {
                Text(err).font(.caption).foregroundStyle(.secondary)
            } else {
                let folders = items.filter { $0.isFolder }
                if folders.isEmpty {
                    Text("此文件夹中没有子文件夹")
                        .font(.subheadline)
                        .foregroundStyle(.secondary)
                        .frame(maxWidth: .infinity, alignment: .center)
                        .padding(.vertical, 40)
                        .listRowBackground(Color.clear)
                } else {
                    ForEach(folders) { folder in
                        Button {
                            navigateInto(folder)
                        } label: {
                            HStack(spacing: 14) {
                                Image(systemName: "folder.fill")
                                    .font(.system(size: 22))
                                    .foregroundStyle(.blue)
                                    .frame(width: 32)
                                Text(folder.name)
                                    .font(.body)
                                    .foregroundStyle(.primary)
                                Spacer()
                                Image(systemName: "chevron.right")
                                    .font(.caption)
                                    .foregroundStyle(.tertiary)
                            }
                            .padding(.vertical, 4)
                        }
                    }
                }
            }
        }
        .listStyle(.insetGrouped)
    }

    // MARK: - 导航逻辑

    private func navigateToRoot() {
        dirStack = []
        currentDir = FileSystemManager.rootURL
        currentTitle = "文件"
        loadItems()
    }

    private func navigateTo(stackIndex: Int) {
        let entry = dirStack[stackIndex]
        dirStack = Array(dirStack.prefix(stackIndex + 1))
        currentDir = entry.url
        currentTitle = entry.title

        // 弹出 stack 到 stackIndex（含），但当前目录变为该 entry
        // 其实 dirStack 应保存到达前的路径
        dirStack = Array(dirStack.prefix(stackIndex))
        currentDir = entry.url
        currentTitle = entry.title
        loadItems()
    }

    private func navigateInto(_ folder: FileItem) {
        dirStack.append((url: currentDir, title: currentTitle))
        currentDir   = folder.url
        currentTitle = folder.name
        loadItems()
    }

    private func loadItems() {
        do {
            items = try fsManager.loadItems(at: currentDir)
            loadError = nil
        } catch {
            loadError = error.localizedDescription
        }
    }

    private func createFolder() {
        let name = newFolderName.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !name.isEmpty else { return }
        do {
            try fsManager.createDirectory(named: name, in: currentDir)
            loadItems()
        } catch {
            importError = error.localizedDescription
            showImportError = true
        }
        newFolderName = ""
    }

    // MARK: - 执行导入

    private func importFile(to destDir: URL) {
        do {
            _ = try fsManager.importFile(from: incomingURL, to: destDir)
            importSuccess = true
            fsManager.incomingURL = nil
            dismiss()
        } catch {
            importError = error.localizedDescription
            showImportError = true
        }
    }
}

// MARK: - Preview

#Preview {
    ImportFileView(incomingURL: URL(fileURLWithPath: "/tmp/示例文档.txt"))
        .environmentObject(FileSystemManager())
}
