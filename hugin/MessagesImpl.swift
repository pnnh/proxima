import Cocoa
import Foundation

public struct DirectoryResponse {
    var absoluteUrl: String? = nil
    var bookmarkString: String? = nil
}

@MainActor
public class HuginImpl {
    func getHostLanguage() throws -> String {
        return "Swift"
    }
    func add(_ a: Int64, to b: Int64) throws -> Int64 {
        return a + b;
    }
    func sendMessage(message: String, completion: @escaping (Result<Bool, Error>) -> Void) {
        completion(.success(true))
    }

    func chooseDirectory() throws -> DirectoryResponse? {

        let openPanel = NSOpenPanel()
        openPanel.message = "Choose your directory"
        openPanel.prompt = "Choose"
        openPanel.allowedFileTypes = ["none"]
        openPanel.allowsOtherFileTypes = false
        openPanel.canChooseFiles = false
        openPanel.canChooseDirectories = true

        let response = openPanel.runModal()
        print("openPanel.urls \(openPanel.urls)")
        let firstUrl = openPanel.urls.first

        if (firstUrl == nil) {
            return nil
        }
        let url = firstUrl!
        let bookmarkData = try url.bookmarkData(
            options: .withSecurityScope,
            includingResourceValuesForKeys: nil,
            relativeTo: nil
        )

        let bookmarkStr = bookmarkData.base64EncodedString()
        var dirResp = DirectoryResponse()
        dirResp.bookmarkString = bookmarkStr
        dirResp.absoluteUrl = url.absoluteString.removingPercentEncoding
        return dirResp
    }

    func startAccessingSecurityScopedResource(bookmarkString: String) throws -> String? {
        let decodedData = Data(base64Encoded: bookmarkString)
        if (decodedData == nil) {
            fatalError("decode bookmark failed")
        }
        var isStale = false
        let newURL = try? URL(
            resolvingBookmarkData: decodedData!,
            options: .withSecurityScope,
            relativeTo: nil,
            bookmarkDataIsStale: &isStale
        )

        var newBookmarkData: Data?
        if let url = newURL, isStale {
            do {
                newBookmarkData = try url.bookmarkData(
                    options: .withSecurityScope,
                    includingResourceValuesForKeys: nil,
                    relativeTo: nil
                )
            } catch { fatalError("Remaking bookmark failed") }
        }

        _ = newURL!.startAccessingSecurityScopedResource()
        // < do whatever to file >
        //        newURL?.stopAccessingSecurityScopedResource()

        if (newBookmarkData != nil) {
            return newBookmarkData!.base64EncodedString()
        }
        return nil
    }
}

public func fibonacciSwift2(_ x: CInt) -> CInt {
    print("x [swift]: \(x)")
    if x <= 1 {
        return 1
    }
    return 111
}
