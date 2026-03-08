using CsWinRTApp.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace CsWinRTApp
{
    public partial class App : Application
    {
        public App()
        {
            // 最早期的诊断 - 使用原始 Debug.WriteLine
            Debug.WriteLine("=== App Constructor START ===");
            Debug.WriteLine($"Current Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            Debug.WriteLine($"Current User: {Environment.UserName}");
            Debug.WriteLine($"AppData Path: {Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}");

            try
            {
                Debug.WriteLine("InitializeComponent() starting...");
                this.InitializeComponent();

                Debug.WriteLine("InitializeComponent() completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CRITICAL ERROR in InitializeComponent: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }

            try
            {
                Debug.WriteLine("Setting up exception handlers...");

                // 捕获所有未处理的异常
                this.UnhandledException += App_UnhandledException;

                // 捕获 AppDomain 级别的未处理异常
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                // 捕获任务未观察到的异常
                System.Threading.Tasks.TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

                Debug.WriteLine("Exception handlers registered");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR setting up exception handlers: {ex.Message}");
            }

            Debug.WriteLine("=== Application Started ===");

            try
            {
                // 获取日志文件夹位置
                var logPath = LogService.GetLogFolderPath();
                Debug.WriteLine($"LogService initialized, path: {logPath}");
                LogService.Info($"Log folder: {logPath}");

                // 清理旧日志（保留最近7天）
                LogService.CleanOldLogs(7);
                Debug.WriteLine("Old logs cleaned");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR initializing LogService: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            Debug.WriteLine("=== App Constructor END ===");
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // 记录异常详细信息
            var errorMessage = $@"
=== UNHANDLED EXCEPTION ===
Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}
Message: {e.Message}
Exception Type: {e.Exception?.GetType().FullName}
Stack Trace:
{e.Exception?.StackTrace}

Inner Exception:
{e.Exception?.InnerException?.Message}
{e.Exception?.InnerException?.StackTrace}
========================
";

            Debug.WriteLine(errorMessage);

            // 写入文件日志
            WriteErrorLog(errorMessage);

            // 显示友好的错误对话框
            ShowErrorDialog(e.Exception);

            // 标记为已处理，防止程序崩溃
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            var errorMessage = $@"
=== APPDOMAIN UNHANDLED EXCEPTION ===
Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}
Is Terminating: {e.IsTerminating}
Message: {exception?.Message}
Exception Type: {exception?.GetType().FullName}
Stack Trace:
{exception?.StackTrace}
========================
";

            Debug.WriteLine(errorMessage);
            WriteErrorLog(errorMessage);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            var errorMessage = $@"
=== UNOBSERVED TASK EXCEPTION ===
Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}
Message: {e.Exception?.Message}
Exceptions:
{string.Join("\n", e.Exception?.InnerExceptions.Select(ex => $"  - {ex.GetType().Name}: {ex.Message}"))}
Stack Trace:
{e.Exception?.StackTrace}
========================
";

            Debug.WriteLine(errorMessage);
            WriteErrorLog(errorMessage);

            // 标记为已观察，防止程序终止
            e.SetObserved();
        }

        private void WriteErrorLog(string errorMessage)
        {
            try
            {
                var logFolder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CsWinRTApp",
                    "Logs");

                Directory.CreateDirectory(logFolder);

                var logFile = System.IO.Path.Combine(logFolder, $"error_log_{DateTime.Now:yyyyMMdd}.txt");
                File.AppendAllText(logFile, errorMessage + "\n");

                Debug.WriteLine($"Error logged to: {logFile}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to write error log: {ex.Message}");
            }
        }

        private async void ShowErrorDialog(Exception exception)
        {
            try
            {
                if (m_window != null)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "程序错误",
                        Content = $"发生了一个错误：\n\n{exception?.Message}\n\n详细信息已记录到日志文件。",
                        CloseButtonText = "确定",
                        XamlRoot = m_window.Content.XamlRoot
                    };

                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to show error dialog: {ex.Message}");
            }
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Debug.WriteLine("=== OnLaunched START ===");

            try
            {
                Debug.WriteLine("Creating MainWindow...");
                m_window = new MainWindow();
                Debug.WriteLine("MainWindow created");

                Debug.WriteLine("Creating Frame...");
                Frame rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Debug.WriteLine("Frame created, NavigationFailed handler attached");

                Debug.WriteLine($"Navigating to MainPage with args: {args.Arguments}");
                rootFrame.Navigate(typeof(MainPage), args.Arguments);
                Debug.WriteLine("Navigation completed");

                Debug.WriteLine("Setting window content...");
                m_window.Content = rootFrame;
                Debug.WriteLine("Window content set");

                Debug.WriteLine("Activating window...");
                m_window.Activate();
                Debug.WriteLine("Window activated");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CRITICAL ERROR in OnLaunched: {ex.GetType().Name}");
                Debug.WriteLine($"Message: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }

            Debug.WriteLine("=== OnLaunched END ===");
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private Window m_window;
    }
}
