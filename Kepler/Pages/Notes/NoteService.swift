import Foundation

func getNoteFileURLs(in directoryURL: URL) -> [URL] {
    let fileManager = FileManager.default
    
    // Enumerate only the immediate contents of the directory (no recursion)
    guard let fileURLs = try? fileManager.contentsOfDirectory(
        at: directoryURL,
        includingPropertiesForKeys: nil,  // No specific properties needed
        options: [.skipsHiddenFiles]      // Optional: Skip hidden files like .DS_Store
    ) else {
        print("Error: Could not access directory at \(directoryURL.path)")
        return []
    }
    
    // Filter for .jpg and .png files (case-insensitive)
    let imageURLs: [URL] = fileURLs.filter { url in
        let pathExtension = url.pathExtension.lowercased()
        return pathExtension == "md" || pathExtension == "markdown"
    }
    
    return imageURLs
}
