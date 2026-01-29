import Foundation
import SwiftUI
import Cocoa


struct PSNoteComponent: NSViewRepresentable {
    typealias NSViewType = NSTextView
    var noteModel: NoteModel
    
    init(model: NoteModel) {
        self.noteModel = model
    }
    init(notePath: String) {
        self.noteModel = NoteModel(Path: notePath, Text: notePath, Show: true)
    }
    
    func updateNSView(_ nsView: NSTextView, context: Context) {
        print("PSNoteComponent updateView")
    }
    func makeNSView(context: Context) -> NSTextView {
        
        let rect = NSRect(x: 0, y: 0, width: 36, height: 36)
        let view = NSTextView(frame: rect)
        view.isEditable = false
        view.string = self.noteModel.Text
         
        return view
    }
    
     
}
