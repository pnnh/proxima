import Foundation
import SwiftUI

@Observable
final class Router: ObservableObject {
    
    public enum Destination: Codable, Hashable {
        case home
        case files
        case notes(owner: String)
        case images
        case password
        case uuid
    }
    
     var navPath = NavigationPath()
    
    func navigate(to destination: Destination) {
        navPath.append(destination)
    }
    
    func navigateBack() {
        navPath.removeLast()
    }
    
    func navigateToRoot() {
        navPath.removeLast(navPath.count)
    }
}
