// CMakeProjectDWM.cpp : Defines the entry point for the application.
//

#include "CMakeProjectDWM.h"

#include "File.h"

#include <QWindow>
#include <QtGui>
#include <QtWidgets/QtWidgets>
#include <Windows.h>
#include <dwmapi.h>
#include <iostream>
#include <qquickview.h>
#include <stdbool.h>
#include <stdlib.h>
#include <tchar.h> // _tcslen函数需要该头文件
#include <uxtheme.h>
#include <versionhelpers.h>
#include <windef.h>
#include <windowsx.h>
#include <wingdi.h>
#include <winuser.h>

#pragma comment(lib, "Winmm.lib")

using namespace std;

#define LEFTEXTENDWIDTH 2
#define RIGHTEXTENDWIDTH 2
#define BOTTOMEXTENDWIDTH 2
#define TOPEXTENDWIDTH 60

#define BIT_COUNT 32
#define RECTWIDTH(rc) ((rc).right - (rc).left)
#define RECTHEIGHT(rc) ((rc).bottom - (rc).top)

#define TMT_CAPTIONFONT 801

class MacWindowController : public QWidget {
public:
  MacWindowController() {
    setWindowTitle("MacWindow Configurator");

    QQuickView *contentView = new QQuickView();
    QWidget *widget = QWidget::createWindowContainer(contentView, this);
    contentView->loadFromModule("quick", "App");
    contentView->setResizeMode(QQuickView::SizeRootObjectToView);
    contentView->setFlags(Qt::FramelessWindowHint); // contentView->show();
    widget->show();
  }
};

// 函数声明，窗口过程
LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

int WINAPI WinMain2(HINSTANCE hInstance, HINSTANCE hPrevInstance,
                    LPTSTR lpCmdLine, int nCmdShow) {
  WNDCLASSEX wndclass;
  TCHAR szClassName[] = TEXT("CMakeProjectWindow");
  TCHAR szAppName[] = TEXT("CMakeProjectDWM");

  HWND hwnd; // CreateWindowEx函数创建的窗口的句柄
  MSG msg;

  wndclass.cbSize = sizeof(WNDCLASSEX);     // 结构体大小
  wndclass.style = CS_HREDRAW | CS_VREDRAW; // 窗口样式
  wndclass.lpfnWndProc = WindowProc;        // 窗口过程
  wndclass.cbClsExtra = 0;                  // 窗口类的附加内存，一般设为0
  wndclass.cbWndExtra = 0;                  // 窗口的附加内存，一般设为0
  wndclass.hInstance = hInstance;           // 程序实例句柄
  wndclass.hIcon = LoadIcon(NULL, IDI_APPLICATION); // 程序图标，使用默认图标
  wndclass.hCursor = LoadCursor(NULL, IDC_ARROW);   // 鼠标光标，使用默认光标
  wndclass.hbrBackground =
      (HBRUSH)GetStockObject(GRAY_BRUSH); // 背景色，使用白色背景
  wndclass.lpszMenuName = NULL;           // 菜单名，没有菜单，设为NULL
  wndclass.lpszClassName = szClassName;   // 窗口类名
  wndclass.hIconSm = NULL;                // 小图标，使用默认图标
  RegisterClassEx(&wndclass);             // 注册窗口类

  const int windowWidth = 800;
  const int windowHeight = 600;

  hwnd = CreateWindowEx(0,                   // 扩展窗口风格
                        szClassName,         // 窗口类名
                        szAppName,           // 窗口标题
                        WS_OVERLAPPEDWINDOW, // 窗口风格
                        CW_USEDEFAULT,       // 窗口左上角横坐标
                        CW_USEDEFAULT,       // 窗口左上角纵坐标
                        windowWidth,         // 窗口宽度
                        windowHeight,        // 窗口高度
                        NULL,                // 父窗口句柄
                        NULL,                // 菜单句柄
                        hInstance,           // 应用程序实例句柄
                        NULL                 // 其他创建参数
  );

  // 通过QWidget嵌入Qt窗体
  // MacWindowController *controller = new MacWindowController();
  // controller->setWindowFlags(Qt::FramelessWindowHint);
  //
  // HWND hwndQt = reinterpret_cast<HWND>(controller->winId());
  // SetParent(hwndQt, hwnd);
  // controller->show();

  // 通过QWidget嵌入QQuickView再嵌入Qml窗体
  // auto contentWindow = new QQuickView();
  // contentWindow->loadFromModule("quick", "App");
  // contentWindow->setResizeMode(QQuickView::SizeRootObjectToView);
  // contentWindow->setFlags(Qt::FramelessWindowHint);
  // QWidget *container = QWidget::createWindowContainer(contentWindow);
  // // container->setMinimumSize(1024, 768);
  // // container->setMaximumSize(2048, 1280);
  // HWND hwndQt = reinterpret_cast<HWND>(container->winId());
  // container->setWindowFlags(Qt::FramelessWindowHint);
  // container->show();
  // SetParent(hwndQt, hwnd);
  // MoveWindow(hwndQt, 40, 40, windowWidth - 55, windowHeight - 60, TRUE);

  // 直接通过QQuickView嵌入Qt窗体
  auto contentWindow = new QQuickView();
  contentWindow->loadFromModule("quick", "App");
  contentWindow->setResizeMode(QQuickView::SizeRootObjectToView);
  contentWindow->setFlags(Qt::FramelessWindowHint);
  HWND hwndQt = reinterpret_cast<HWND>(contentWindow->winId());
  SetParent(hwndQt, hwnd);
  contentWindow->show();
  MoveWindow(hwndQt, LEFTEXTENDWIDTH, TOPEXTENDWIDTH,
             windowWidth - LEFTEXTENDWIDTH - RIGHTEXTENDWIDTH,
             windowHeight - TOPEXTENDWIDTH, TRUE);

  ShowWindow(hwnd, nCmdShow); // 显示窗口
  UpdateWindow(hwnd);         // 更新窗口

  while (GetMessage(&msg, NULL, 0, 0)) { // 消息循环
    TranslateMessage(&msg);              // 键盘消息转换
    DispatchMessage(&msg);               // 分发消息
  }

  return msg.wParam;
}

// Paint the title on the custom frame.
void PaintCustomCaption(HWND hWnd, HDC hdc) {
  RECT rcClient;
  GetClientRect(hWnd, &rcClient);
  LPCWSTR szTitle = L"CMakeProjectDWM";

  HTHEME hTheme = OpenThemeData(NULL, L"CompositedWindow::Window");
  if (hTheme) {
    HDC hdcPaint = CreateCompatibleDC(hdc);
    if (hdcPaint) {
      int cx = RECTWIDTH(rcClient);
      int cy = RECTHEIGHT(rcClient);

      // Define the BITMAPINFO structure used to draw text.
      // Note that biHeight is negative. This is done because
      // DrawThemeTextEx() needs the bitmap to be in top-to-bottom
      // order.
      BITMAPINFO dib = {0};
      dib.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
      dib.bmiHeader.biWidth = cx;
      dib.bmiHeader.biHeight = -cy;
      dib.bmiHeader.biPlanes = 1;
      dib.bmiHeader.biBitCount = BIT_COUNT;
      dib.bmiHeader.biCompression = BI_RGB;

      HBITMAP hbm = CreateDIBSection(hdc, &dib, DIB_RGB_COLORS, NULL, NULL, 0);
      if (hbm) {
        HBITMAP hbmOld = (HBITMAP)SelectObject(hdcPaint, hbm);

        // Setup the theme drawing options.
        DTTOPTS DttOpts = {sizeof(DTTOPTS)};
        DttOpts.dwFlags = DTT_COMPOSITED | DTT_GLOWSIZE;
        DttOpts.iGlowSize = 15;

#if UNICODE
        int a = 0;
#endif

        // Select a font.
        LOGFONT lgFont;
        HFONT hFontOld = NULL;
        if (SUCCEEDED(GetThemeSysFont(hTheme, TMT_CAPTIONFONT, &lgFont))) {
          HFONT hFont = CreateFontIndirect(&lgFont);
          hFontOld = (HFONT)SelectObject(hdcPaint, hFont);
        }

        // Draw the title.
        RECT rcPaint = rcClient;
        rcPaint.top += 8;
        rcPaint.right -= 125;
        rcPaint.left += 8;
        rcPaint.bottom = 50;
        DrawThemeTextEx(hTheme, hdcPaint, 0, 0, szTitle, -1,
                        DT_LEFT | DT_WORD_ELLIPSIS, &rcPaint, &DttOpts);

        // Blit text to the frame.
        BitBlt(hdc, 0, 0, cx, cy, hdcPaint, 0, 0, SRCCOPY);

        SelectObject(hdcPaint, hbmOld);
        if (hFontOld) {
          SelectObject(hdcPaint, hFontOld);
        }
        DeleteObject(hbm);
      }
      DeleteDC(hdcPaint);
    }
    CloseThemeData(hTheme);
  }
}

// Hit test the frame for resizing and moving.
LRESULT HitTestNCA(HWND hWnd, WPARAM wParam, LPARAM lParam) {
  // Get the point coordinates for the hit test.
  POINT ptMouse = {GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam)};

  // Get the window rectangle.
  RECT rcWindow;
  GetWindowRect(hWnd, &rcWindow);

  // Get the frame rectangle, adjusted for the style without a caption.
  RECT rcFrame = {0};
  DWORD dwStyle = WS_OVERLAPPEDWINDOW & ~WS_CAPTION;
  AdjustWindowRectEx(&rcFrame, dwStyle, FALSE, NULL);

  // Determine if the hit test is for resizing. Default middle (1,1).
  USHORT uRow = 1;
  USHORT uCol = 1;
  bool fOnResizeBorder = false;

  // Determine if the point is at the top or bottom of the window.
  if (ptMouse.y >= rcWindow.top && ptMouse.y < rcWindow.top + TOPEXTENDWIDTH) {
    fOnResizeBorder = (ptMouse.y < (rcWindow.top - rcFrame.top));
    uRow = 0;
  } else if (ptMouse.y < rcWindow.bottom &&
             ptMouse.y >= rcWindow.bottom - BOTTOMEXTENDWIDTH) {
    uRow = 2;
  }

  // Determine if the point is at the left or right of the window.
  if (ptMouse.x >= rcWindow.left &&
      ptMouse.x < rcWindow.left + LEFTEXTENDWIDTH) {
    uCol = 0; // left side
  } else if (ptMouse.x < rcWindow.right &&
             ptMouse.x >= rcWindow.right - RIGHTEXTENDWIDTH) {
    uCol = 2; // right side
  }

  // Hit test (HTTOPLEFT, ... HTBOTTOMRIGHT)
  LRESULT hitTests[3][3] = {
      {HTTOPLEFT, fOnResizeBorder ? HTTOP : HTCAPTION, HTTOPRIGHT},
      {HTLEFT, HTNOWHERE, HTRIGHT},
      {HTBOTTOMLEFT, HTBOTTOM, HTBOTTOMRIGHT},
  };

  return hitTests[uRow][uCol];
}

//
// Message handler for handling the custom caption messages.
//
LRESULT CustomCaptionProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam,
                          bool *pfCallDWP) {
  LRESULT lRet = 0;
  HRESULT hr = S_OK;
  bool fCallDWP = true; // Pass on to DefWindowProc?

  fCallDWP = !DwmDefWindowProc(hWnd, message, wParam, lParam, &lRet);

  // Handle window creation.
  if (message == WM_CREATE) {
    RECT rcClient;
    GetWindowRect(hWnd, &rcClient);

    // Inform application of the frame change.
    SetWindowPos(hWnd, NULL, rcClient.left, rcClient.top, RECTWIDTH(rcClient),
                 RECTHEIGHT(rcClient), SWP_FRAMECHANGED);

    fCallDWP = true;
    lRet = 0;
  }

  // Handle window activation.
  if (message == WM_ACTIVATE) {
    SetWindowLong(hWnd, GWL_STYLE,
                  GetWindowLong(hWnd, GWL_STYLE) & ~WS_SYSMENU);

    // Extend the frame into the client area.
    MARGINS margins;

    margins.cxLeftWidth = LEFTEXTENDWIDTH;
    margins.cxRightWidth = RIGHTEXTENDWIDTH;
    margins.cyBottomHeight = BOTTOMEXTENDWIDTH;
    margins.cyTopHeight = TOPEXTENDWIDTH;

    hr = DwmExtendFrameIntoClientArea(hWnd, &margins);

    if (!SUCCEEDED(hr)) {
      // Handle error.
    }

    fCallDWP = true;
    lRet = 0;
  }

  if (message == WM_PAINT) {
    HDC hdc;
    {
      PAINTSTRUCT ps;
      hdc = BeginPaint(hWnd, &ps);
      PaintCustomCaption(hWnd, hdc);
      EndPaint(hWnd, &ps);
    }

    fCallDWP = true;
    lRet = 0;
  }

  int customTitleHeight = 40;

  // Handle the non-client size message.
  if ((message == WM_NCCALCSIZE) && (wParam == TRUE)) {
    // Calculate new NCCALCSIZE_PARAMS based on custom NCA inset.
    NCCALCSIZE_PARAMS *pncsp = reinterpret_cast<NCCALCSIZE_PARAMS *>(lParam);

    pncsp->rgrc[0].left = pncsp->rgrc[0].left + 0;
    pncsp->rgrc[0].top = pncsp->rgrc[0].top + customTitleHeight;
    pncsp->rgrc[0].right = pncsp->rgrc[0].right - 0;
    pncsp->rgrc[0].bottom = pncsp->rgrc[0].bottom - 0;

    lRet = 0;

    // No need to pass the message on to the DefWindowProc.
    fCallDWP = false;
  }

  // Handle hit testing in the NCA if not handled by DwmDefWindowProc.
  if ((message == WM_NCHITTEST) && (lRet == 0)) {
    lRet = HitTestNCA(hWnd, wParam, lParam);

    // 获取鼠标位置
    POINT pt = {GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam)};
    ScreenToClient(hWnd, &pt);

    // 定义标题栏区域
    RECT titleBarRect = {80, 0, 200, 30};
    if (PtInRect(&titleBarRect, pt)) {
      std::cerr << "Hello, PtInRect" << std::endl;
      lRet = HTMAXBUTTON;
    }

    if (lRet != HTNOWHERE) {
      fCallDWP = false;
    }
  }

  *pfCallDWP = fCallDWP;

  return lRet;
}

//
// Message handler for the application.
//
LRESULT AppWinProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam) {
  int wmId, wmEvent;
  PAINTSTRUCT ps;
  HDC hdc;
  HRESULT hr;
  LRESULT result = 0;

  switch (message) {
  case WM_CREATE: {
  } break;
  case WM_COMMAND:
    wmId = LOWORD(wParam);
    wmEvent = HIWORD(wParam);

    // Parse the menu selections:
    switch (wmId) {
    default:
      return DefWindowProc(hWnd, message, wParam, lParam);
    }
    break;
  case WM_PAINT: {
    hdc = BeginPaint(hWnd, &ps);
    PaintCustomCaption(hWnd, hdc);

    // Add any drawing code here...

    EndPaint(hWnd, &ps);
  } break;
  case WM_DESTROY:
    PostQuitMessage(0);
    break;
  default:
    return DefWindowProc(hWnd, message, wParam, lParam);
  }
  return 0;
}

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam,
                            LPARAM lParam) {
  bool fCallDWP = true;
  BOOL fDwmEnabled = FALSE;
  LRESULT lRet = 0;
  HRESULT hr = S_OK;

  // Winproc worker for custom frame issues.
  hr = DwmIsCompositionEnabled(&fDwmEnabled);
  if (SUCCEEDED(hr)) {
    lRet = CustomCaptionProc(hwnd, uMsg, wParam, lParam, &fCallDWP);
  }

  // Winproc worker for the rest of the application.
  if (fCallDWP) {
    lRet = AppWinProc(hwnd, uMsg, wParam, lParam);
  }
  return lRet;
}

int showWindowsDWMWindow() {

  HINSTANCE hInstance = GetModuleHandle(NULL); // 获取程序本身的实例句柄
  int nCmdShow = SW_SHOWNORMAL;                // 定义窗口显示模式
  LPTSTR lpCmdLine = GetCommandLine();         // 获取命令行字符串
  HINSTANCE hPrevInstance = NULL;              // 一般程序用不到这个参数
  return WinMain2(hInstance, hPrevInstance, lpCmdLine, nCmdShow);
}