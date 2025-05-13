#include "macwindow.h"

#import <AppKit/AppKit.h>

// An NSTitlebarAccessoryViewController that controls a programatically created view
@interface ProgramaticViewController : NSTitlebarAccessoryViewController
@end

@implementation ProgramaticViewController
- (id)initWithView: (NSView *)aView
{
    self = [self init];
    self.view = aView;
    return self;
}
@end

MacWindow::MacWindow(QWindow *window, QWindow *accessoryWindow)
:m_window(window)
,m_accessoryWindow(accessoryWindow)
{

}

void MacWindow::setContentWindow(QWindow *contentWindow)
{
    m_window = contentWindow;
    scheduleRecreateNSWindow();
}

void MacWindow::setLeftAcccessoryWindow(QWindow *leftAccessoryWindow)
{
    m_accessoryWindow = leftAccessoryWindow;
    scheduleRecreateNSWindow();
}

void MacWindow::setRightAcccessoryWindow(QWindow *rightAccessoryWindow)
{
    m_rightAccessoryWindow = rightAccessoryWindow;
    scheduleRecreateNSWindow();
}

void MacWindow::createNSWindow()
{
    qDebug() << "createNSWindow";
    if (m_nsWindow)
        return;
    
    auto styleMask = NSWindowStyleMaskTitled | NSWindowStyleMaskClosable |
                     NSWindowStyleMaskMiniaturizable | NSWindowStyleMaskResizable;

                
    NSRect frame = NSMakeRect(200, 200, 320, 200);
    m_nsWindow =
        [[NSWindow alloc] initWithContentRect:frame
                                    styleMask:styleMask
                                      backing:NSBackingStoreBuffered
                                        defer:NO];

    m_nsWindow.titleVisibility = NSWindowTitleHidden;
    QString winTitle{"hello"};
    m_nsWindow.title = winTitle.toNSString();
    m_nsWindow.titlebarAppearsTransparent = true;

        NSToolbar *toolbar = [[NSToolbar alloc] initWithIdentifier:@"main"];
        toolbar.showsBaselineSeparator = false;
        m_nsWindow.toolbar = toolbar;

    m_nsWindow.contentView = (__bridge NSView *)reinterpret_cast<void *>(m_window->winId());

        {
        NSView *leftView = (__bridge NSView *)reinterpret_cast<void *>(m_accessoryWindow->winId());
        ProgramaticViewController *leftViewController = [[ProgramaticViewController alloc] initWithView:leftView];
        leftViewController.layoutAttribute = NSLayoutAttributeLeft;
        [m_nsWindow addTitlebarAccessoryViewController:leftViewController];
        }
        {
        NSView *rightView = (__bridge NSView *)reinterpret_cast<void *>(m_rightAccessoryWindow->winId());
        ProgramaticViewController *rightViewController = [[ProgramaticViewController alloc] initWithView:rightView];
        rightViewController.layoutAttribute = NSLayoutAttributeRight;
        [m_nsWindow addTitlebarAccessoryViewController:rightViewController];

        }
}

void MacWindow::destroyNSWindow()
{
    @autoreleasepool {
        [m_nsWindow close];
        m_nsWindow = nil;
    }
}

void MacWindow::recreateNSWindow()
{
    if (!m_nsWindow)
        return;
    
    destroyNSWindow();
    createNSWindow();
    
    if (m_visible)
        [m_nsWindow makeKeyAndOrderFront:nil];
}

void MacWindow::scheduleRecreateNSWindow()
{
    QTimer::singleShot(200, [this](){
        recreateNSWindow();
    });
}

void MacWindow::setGeometry(QRect geometry)
{
    
}

QRect MacWindow::geometry() const
{
    return QRect();
}

void MacWindow::setVisible(bool visible)
{
    qDebug() << "setVisible" << visible;
    m_visible = visible;
    if (visible) {
        createNSWindow();
        [m_nsWindow makeKeyAndOrderFront:nil];
    } else {
        
    }
}

void MacWindow::show()
{
    setVisible(true);
}

