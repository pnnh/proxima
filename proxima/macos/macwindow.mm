#include "macwindow.h"

#import <AppKit/AppKit.h>
#include <QQuickView>

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

MacWindow::MacWindow(QWindow *window, QWindow *leftAccessoryWindow, QWindow *rightAccessoryWindow)
:m_window(window)
,m_leftAccessoryWindow(leftAccessoryWindow), m_rightAccessoryWindow(rightAccessoryWindow)
{

}

void MacWindow::setContentWindow(QWindow *contentWindow)
{
    m_window = contentWindow;
    scheduleRecreateNSWindow();
}

void MacWindow::createNSWindow()
{
    qDebug() << "createNSWindow";
    if (m_nsWindow)
        return;
    
    auto styleMask = NSWindowStyleMaskTitled | NSWindowStyleMaskClosable |
                     NSWindowStyleMaskMiniaturizable | NSWindowStyleMaskResizable;

     NSRect windowRect= NSMakeRect(200, 200, 1024, 768);

//    CGFloat screenWidth = [NSScreen mainScreen].frame.size.width;
//    CGRect screenRect = [[NSScreen mainScreen] frame];
//    CGPoint newOrigin;
//    newOrigin.x = (screenRect.size.width / 2) - (windowRect.size.width / 2);
//    newOrigin.y = (screenRect.size.height / 2) + (windowRect.size.height / 2);

    m_nsWindow =
        [[NSWindow alloc] initWithContentRect:windowRect
                                    styleMask:styleMask
                                      backing:NSBackingStoreBuffered
                                        defer:NO];

    m_nsWindow.titleVisibility = NSWindowTitleHidden;
    QString winTitle{"hello"};
    m_nsWindow.title = winTitle.toNSString();
    m_nsWindow.titlebarAppearsTransparent = false;

        NSToolbar *toolbar = [[NSToolbar alloc] initWithIdentifier:@"main"];
        toolbar.displayMode = NSToolbarDisplayModeIconOnly;
        toolbar.sizeMode = NSToolbarSizeModeRegular;
        m_nsWindow.toolbar = toolbar;

    m_nsWindow.contentView = (__bridge NSView *)reinterpret_cast<void *>(m_window->winId());
    // 添加最左侧地球图标
//    {
//
//      auto toolGlobalView = new QQuickView();
//      toolGlobalView->setResizeMode(QQuickView::SizeRootObjectToView);
//      toolGlobalView->loadFromModule("quick", "MacToolGlobal");
//
//            NSView *leftView = (__bridge NSView *)reinterpret_cast<void *>(toolGlobalView->winId());
//            ProgramaticViewController *leftViewController = [[ProgramaticViewController alloc] initWithView:leftView];
//            leftViewController.layoutAttribute = NSLayoutAttributeLeft;
//             CGRect currentFrame = leftView.frame;
//                currentFrame.size.width = 40;
//                leftView.frame = currentFrame;
//            [m_nsWindow addTitlebarAccessoryViewController:leftViewController];
//
//  toolGlobalView->show();
//    }

        {
            NSView *leftView = (__bridge NSView *)reinterpret_cast<void *>(m_leftAccessoryWindow->winId());
            ProgramaticViewController *leftViewController = [[ProgramaticViewController alloc] initWithView:leftView];
            leftViewController.layoutAttribute = NSLayoutAttributeLeft;
//             CGRect currentFrame = leftView.frame;
//    currentFrame.size.width = windowRect.size.width;
//    leftView.frame = currentFrame;
            [m_nsWindow addTitlebarAccessoryViewController:leftViewController];
        }
        {
            NSView *rightView = (__bridge NSView *)reinterpret_cast<void *>(m_rightAccessoryWindow->winId());
            ProgramaticViewController *rightViewController = [[ProgramaticViewController alloc] initWithView:rightView];
            rightViewController.layoutAttribute = NSLayoutAttributeRight;
//             CGRect currentFrame = leftView.frame;
//    currentFrame.size.width = windowRect.size.width;
//    leftView.frame = currentFrame;
            [m_nsWindow addTitlebarAccessoryViewController:rightViewController];
          }


    [m_nsWindow center];
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

