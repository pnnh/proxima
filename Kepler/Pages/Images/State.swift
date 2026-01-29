import Foundation
import Combine

class SharedViewModel: ObservableObject {
    @Published var selectedImagePath: String? = nil
}
