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
}

// MARK: - 文件列表视图

struct FileListView: View {

    let title: String
    let items: [FileItem]

    @State private var sortOption: SortOption = .nameAsc
    @State private var searchText: String = ""
    @State private var viewMode: ViewMode = .list
    @State private var showSortMenu = false

    enum ViewMode { case list, grid }

    // MARK: 排序 + 搜索后的数据
    private var displayItems: [FileItem] {
        let filtered = searchText.isEmpty
            ? items
            : items.filter { $0.name.localizedCaseInsensitiveContains(searchText) }

        return filtered.sorted { a, b in
            switch sortOption {
            case .nameAsc:  return a.name < b.name
            case .nameDesc: return a.name > b.name
            case .dateDesc: return a.modifiedDate > b.modifiedDate
            case .dateAsc:  return a.modifiedDate < b.modifiedDate
            }
        }
    }

    var body: some View {
        Group {
            if viewMode == .list {
                listContent
            } else {
                gridContent
            }
        }
        .searchable(text: $searchText, prompt: "搜索")
        .navigationTitle(title)
        .navigationBarTitleDisplayMode(.large)
        .toolbar {
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
                            HStack {
                                Text(option.rawValue)
                                if sortOption == option {
                                    Image(systemName: "checkmark")
                                }
                            }
                        }
                    }
                } label: {
                    Image(systemName: "arrow.up.arrow.down")
                }
            }
        }
    }

    // MARK: - 列表模式

    private var listContent: some View {
        List {
            if displayItems.isEmpty {
                emptyView
                    .listRowBackground(Color.clear)
            } else {
                ForEach(displayItems) { item in
                    fileRow(item)
                }
            }
        }
        .listStyle(.insetGrouped)
    }

    @ViewBuilder
    private func fileRow(_ item: FileItem) -> some View {
        if item.isFolder, case .folder(let children) = item.content {
            // 文件夹 → 导航到下一级
            NavigationLink {
                FileListView(title: item.name, items: children)
            } label: {
                FileRowLabel(item: item)
            }
        } else {
            // 文件 → 导航到预览
            NavigationLink {
                FileDetailView(item: item)
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
        let destination: AnyView = {
            if item.isFolder, case .folder(let children) = item.content {
                return AnyView(FileListView(title: item.name, items: children))
            } else {
                return AnyView(FileDetailView(item: item))
            }
        }()

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
            // 图标
            ZStack {
                RoundedRectangle(cornerRadius: 10)
                    .fill(item.fileType.iconColor.opacity(0.15))
                    .frame(width: 44, height: 44)
                Image(systemName: item.fileType.iconName)
                    .font(.system(size: 22))
                    .foregroundStyle(item.fileType.iconColor)
            }

            // 文字信息
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
                        Text("·")
                            .font(.caption)
                            .foregroundStyle(.tertiary)
                        Text(size)
                            .font(.caption)
                            .foregroundStyle(.secondary)
                    }
                }
            }

            Spacer()
        }
        .padding(.vertical, 2)
    }
}

// MARK: - Preview

#Preview {
    NavigationStack {
        FileListView(title: "文件", items: FileItem.mockRoot)
    }
}
