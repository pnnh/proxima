//
//  FileListView.swift
//  Venus
//
//  Created by Larry on 2024/10/25.
//

import SwiftUI

// MARK: - 排序方式

enum SortOption: String, CaseIterable {
    case nameAsc   = "名称 ↑"
    case nameDesc  = "名称 ↓"
    case dateDesc  = "日期 ↓"
    case dateAsc   = "日期 ↑"
    case sizeDesc  = "大小 ↓"
}

// MARK: - 文件列表视图

struct FileListView: View {

    let directoryURL: URL
    let title: String

    @EnvironmentObject private var fsManager: FileSystemManager

    @State private var items: [FileItem] = []
    @State private var loadError: String? = nil

    @State private var sortOption: SortOption = .nameAsc
    @State private var searchText: String = ""
    @State private var viewMode: ViewMode = .list

    // 新建文件夹
    @State private var showNewFolderAlert = false
    @State private var newFolderName = ""

    // 删除确认
    @State private var itemToDelete: FileItem? = nil
    @State private var showDeleteConfirm = false

    // 分享
    @State private var shareURL: URL? = nil
    @State private var showShareSheet = false

    enum ViewMode { case list, grid }

    // MARK: 排序 + 搜索后的数据
    private var displayItems: [FileItem] {
        let filtered = searchText.isEmpty
            ? items
            : items.filter { $0.name.localizedCaseInsensitiveContains(searchText) }

        return filtered.sorted { a, b in
            // 文件夹始终在前
            if a.isFolder != b.isFolder { return a.isFolder }
            switch sortOption {
            case .nameAsc:  return a.name.localizedCompare(b.name) == .orderedAscending
            case .nameDesc: return a.name.localizedCompare(b.name) == .orderedDescending
            case .dateDesc: return a.modifiedDate > b.modifiedDate
            case .dateAsc:  return a.modifiedDate < b.modifiedDate
            case .sizeDesc:
                let sa = sizeBytes(a.size), sb = sizeBytes(b.size)
                return sa > sb
            }
        }
    }

    var body: some View {
        Group {
            if let err = loadError {
                errorView(message: err)
            } else if viewMode == .list {
                listContent
            } else {
                gridContent
            }
        }
        .searchable(text: $searchText, prompt: "搜索")
        .navigationTitle(title)
        .navigationBarTitleDisplayMode(.large)
        .toolbar { toolbarContent }
        .onAppear { loadItems() }
        .refreshable { loadItems() }
        // 新建文件夹 Alert
        .alert("新建文件夹", isPresented: $showNewFolderAlert) {
            TextField("文件夹名称", text: $newFolderName)
            Button("取消", role: .cancel) { newFolderName = "" }
            Button("创建") { createFolder() }
        } message: {
            Text("请输入新文件夹名称")
        }
        // 删除确认
        .confirmationDialog(
            "删除「\(itemToDelete?.name ?? "")」？",
            isPresented: $showDeleteConfirm,
            titleVisibility: .visible
        ) {
            Button("删除", role: .destructive) {
                if let item = itemToDelete { deleteItem(item) }
            }
            Button("取消", role: .cancel) {}
        } message: {
            Text("此操作无法撤销。")
        }
        // 分享 Sheet
        .sheet(isPresented: $showShareSheet) {
            if let url = shareURL {
                ShareSheetView(url: url)
            }
        }
    }

    // MARK: - Toolbar

    @ToolbarContentBuilder
    private var toolbarContent: some ToolbarContent {
        ToolbarItemGroup(placement: .navigationBarTrailing) {
            // 视图切换
            Button {
                withAnimation { viewMode = viewMode == .list ? .grid : .list }
            } label: {
                Image(systemName: viewMode == .list ? "square.grid.2x2" : "list.bullet")
            }

            // 排序菜单
            Menu {
                ForEach(SortOption.allCases, id: \.self) { option in
                    Button {
                        sortOption = option
                    } label: {
                        Label(option.rawValue,
                              systemImage: sortOption == option ? "checkmark" : "")
                    }
                }
            } label: {
                Image(systemName: "arrow.up.arrow.down")
            }

            // 新建文件夹
            Button {
                newFolderName = ""
                showNewFolderAlert = true
            } label: {
                Image(systemName: "folder.badge.plus")
            }
        }
    }

    // MARK: - 列表模式

    private var listContent: some View {
        List {
            if displayItems.isEmpty {
                emptyView.listRowBackground(Color.clear)
            } else {
                ForEach(displayItems) { item in
                    fileRow(item)
                        .swipeActions(edge: .trailing, allowsFullSwipe: false) {
                            Button(role: .destructive) {
                                itemToDelete = item
                                showDeleteConfirm = true
                            } label: {
                                Label("删除", systemImage: "trash")
                            }

                            if !item.isFolder {
                                Button {
                                    shareURL = item.url
                                    showShareSheet = true
                                } label: {
                                    Label("分享", systemImage: "square.and.arrow.up")
                                }
                                .tint(.blue)
                            }
                        }
                }
            }
        }
        .listStyle(.insetGrouped)
    }

    @ViewBuilder
    private func fileRow(_ item: FileItem) -> some View {
        if item.isFolder {
            NavigationLink {
                FileListView(directoryURL: item.url, title: item.name)
                    .environmentObject(fsManager)
            } label: {
                FileRowLabel(item: item)
            }
        } else {
            NavigationLink {
                FileDetailView(item: item)
                    .environmentObject(fsManager)
            } label: {
                FileRowLabel(item: item)
            }
        }
    }

    // MARK: - 网格模式

    private let gridColumns = [
        GridItem(.adaptive(minimum: 90, maximum: 120), spacing: 16)
    ]

    private var gridContent: some View {
        ScrollView {
            if displayItems.isEmpty {
                emptyView.padding(.top, 80)
            } else {
                LazyVGrid(columns: gridColumns, spacing: 20) {
                    ForEach(displayItems) { item in
                        gridCell(item)
                    }
                }
                .padding()
            }
        }
    }

    @ViewBuilder
    private func gridCell(_ item: FileItem) -> some View {
        let destination: AnyView = item.isFolder
            ? AnyView(FileListView(directoryURL: item.url, title: item.name).environmentObject(fsManager))
            : AnyView(FileDetailView(item: item).environmentObject(fsManager))

        NavigationLink { destination } label: {
            VStack(spacing: 6) {
                Image(systemName: item.fileType.iconName)
                    .font(.system(size: 40))
                    .foregroundStyle(item.fileType.iconColor)
                    .frame(width: 60, height: 60)
                Text(item.name)
                    .font(.caption)
                    .foregroundStyle(.primary)
                    .lineLimit(2)
                    .multilineTextAlignment(.center)
            }
            .padding(8)
            .background(Color(.systemBackground))
            .clipShape(RoundedRectangle(cornerRadius: 12))
            .shadow(color: .black.opacity(0.06), radius: 4, x: 0, y: 2)
        }
        .buttonStyle(.plain)
        .contextMenu {
            if !item.isFolder {
                Button {
                    shareURL = item.url
                    showShareSheet = true
                } label: {
                    Label("分享", systemImage: "square.and.arrow.up")
                }
            }
            Button(role: .destructive) {
                itemToDelete = item
                showDeleteConfirm = true
            } label: {
                Label("删除", systemImage: "trash")
            }
        }
    }

    // MARK: - 空视图

    private var emptyView: some View {
        VStack(spacing: 12) {
            Image(systemName: searchText.isEmpty ? "folder" : "magnifyingglass")
                .font(.system(size: 48))
                .foregroundStyle(.quaternary)
            Text(searchText.isEmpty ? "此文件夹为空" : "未找到匹配项")
                .font(.headline)
                .foregroundStyle(.secondary)
        }
        .frame(maxWidth: .infinity)
        .padding(.vertical, 60)
    }

    private func errorView(message: String) -> some View {
        VStack(spacing: 12) {
            Image(systemName: "exclamationmark.triangle.fill")
                .font(.system(size: 48))
                .foregroundStyle(.orange)
            Text("无法读取目录")
                .font(.headline)
            Text(message)
                .font(.caption)
                .foregroundStyle(.secondary)
                .multilineTextAlignment(.center)
        }
        .padding()
    }

    // MARK: - 文件系统操作

    private func loadItems() {
        do {
            items = try fsManager.loadItems(at: directoryURL)
            loadError = nil
        } catch {
            loadError = error.localizedDescription
        }
    }

    private func createFolder() {
        let name = newFolderName.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !name.isEmpty else { return }
        do {
            try fsManager.createDirectory(named: name, in: directoryURL)
            loadItems()
        } catch {
            loadError = error.localizedDescription
        }
        newFolderName = ""
    }

    private func deleteItem(_ item: FileItem) {
        do {
            try fsManager.delete(item: item)
            loadItems()
        } catch {
            loadError = error.localizedDescription
        }
    }

    // MARK: - Helpers

    private func sizeBytes(_ sizeStr: String?) -> Int64 {
        guard let s = sizeStr else { return 0 }
        let num = s.components(separatedBy: CharacterSet.decimalDigits.inverted)
                   .joined()
        return Int64(num) ?? 0
    }
}

// MARK: - 单行文件 Row

struct FileRowLabel: View {
    let item: FileItem

    private static let dateFormatter: DateFormatter = {
        let f = DateFormatter()
        f.dateStyle = .medium
        f.timeStyle = .none
        f.locale = Locale(identifier: "zh_CN")
        return f
    }()

    var body: some View {
        HStack(spacing: 14) {
            ZStack {
                RoundedRectangle(cornerRadius: 10)
                    .fill(item.fileType.iconColor.opacity(0.15))
                    .frame(width: 44, height: 44)
                Image(systemName: item.fileType.iconName)
                    .font(.system(size: 22))
                    .foregroundStyle(item.fileType.iconColor)
            }
            VStack(alignment: .leading, spacing: 3) {
                Text(item.name)
                    .font(.body)
                    .foregroundStyle(.primary)
                    .lineLimit(1)
                HStack(spacing: 6) {
                    Text(Self.dateFormatter.string(from: item.modifiedDate))
                        .font(.caption)
                        .foregroundStyle(.secondary)
                    if let size = item.size {
                        Text("·").font(.caption).foregroundStyle(.tertiary)
                        Text(size).font(.caption).foregroundStyle(.secondary)
                    }
                }
            }
            Spacer()
        }
        .padding(.vertical, 2)
    }
}

// MARK: - 系统分享 Sheet 包装

struct ShareSheetView: UIViewControllerRepresentable {
    let url: URL

    func makeUIViewController(context: Context) -> UIActivityViewController {
        UIActivityViewController(activityItems: [url], applicationActivities: nil)
    }
    func updateUIViewController(_ uiViewController: UIActivityViewController, context: Context) {}
}

// MARK: - Preview

#Preview {
    NavigationStack {
        FileListView(directoryURL: FileSystemManager.rootURL, title: "文件")
            .environmentObject(FileSystemManager())
    }
}
