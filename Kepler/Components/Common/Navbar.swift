import Foundation
import SwiftUI
import Combine

struct PSNavbarComponent: View {
    @EnvironmentObject var router: Router
    
    
    var body: some View {
        HStack {
            Image(.global)
                    .font(.system(size: 56))
                    .foregroundColor(.accentColor)
                    .onTapGesture {
                        
                        router.navigate(to: .home)
                    }
            Spacer()
        }
        .padding(8)
        .frame(height: 40, alignment: Alignment.center)
        .background(Color(hex: 0xFAFAFA))
        .background(.green)
            
             
    }
 
}
