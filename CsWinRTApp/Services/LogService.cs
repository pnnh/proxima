using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace CsWinRTApp.Services
{
    /// <summary>
    /// 日志服务 - 用于记录应用运行时信息和错误
    /// </summary>
    public static class LogService
    {
        private static readonly string LogFolder;
        private static readonly object LogLock = new object();

        static LogService()
        {
            System.Diagnostics.Debug.WriteLine("=== LogService Static Constructor START ===");

            try
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                System.Diagnostics.Debug.WriteLine($"LocalApplicationData: {localAppData}");

                LogFolder = Path.Combine(localAppData, "CsWinRTApp", "Logs");
                System.Diagnostics.Debug.WriteLine($"LogFolder will be: {LogFolder}");

                if (!Directory.Exists(LogFolder))
                {
                    System.Diagnostics.Debug.WriteLine($"Creating LogFolder...");
                    Directory.CreateDirectory(LogFolder);
                    System.Diagnostics.Debug.WriteLine($"LogFolder created successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"LogFolder already exists");
                }

                // 测试写入
                var testFile = Path.Combine(LogFolder, "test.txt");
                File.WriteAllText(testFile, $"Test at {DateTime.Now}");
                System.Diagnostics.Debug.WriteLine($"Test file written: {testFile}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR in LogService constructor: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                // 尝试使用临时目录
                try
                {
                    LogFolder = Path.Combine(Path.GetTempPath(), "CsWinRTApp", "Logs");
                    System.Diagnostics.Debug.WriteLine($"Falling back to temp folder: {LogFolder}");
                    Directory.CreateDirectory(LogFolder);
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create temp folder: {ex2.Message}");
                    LogFolder = null;
                }
            }

            System.Diagnostics.Debug.WriteLine("=== LogService Static Constructor END ===");
        }

        public static void Info(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        {
            WriteLog("INFO", message, callerName, callerFile, callerLine);
        }

        public static void Warning(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        {
            WriteLog("WARN", message, callerName, callerFile, callerLine);
        }

        public static void Error(string message, Exception exception = null, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        {
            var fullMessage = message;
            if (exception != null)
            {
                fullMessage += $"\nException: {exception.GetType().Name}: {exception.Message}\nStackTrace:\n{exception.StackTrace}";
                if (exception.InnerException != null)
                {
                    fullMessage += $"\nInner Exception: {exception.InnerException.Message}";
                }
            }

            WriteLog("ERROR", fullMessage, callerName, callerFile, callerLine);
        }

        public static void Debug(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        {
#if DEBUG
            WriteLog("DEBUG", message, callerName, callerFile, callerLine);
#endif
        }

        private static void WriteLog(string level, string message, string callerName, string callerFile, int callerLine)
        {
            var fileName = Path.GetFileName(callerFile);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logMessage = $"[{timestamp}] [{level}] [{fileName}:{callerLine} {callerName}] {message}";

            // 输出到调试窗口（始终执行）
            System.Diagnostics.Debug.WriteLine(logMessage);

            // 写入文件（如果可用）
            if (string.IsNullOrEmpty(LogFolder))
            {
                System.Diagnostics.Debug.WriteLine("Warning: LogFolder is null, skipping file write");
                return;
            }

            try
            {
                lock (LogLock)
                {
                    var logFile = Path.Combine(LogFolder, $"app_log_{DateTime.Now:yyyyMMdd}.txt");
                    File.AppendAllText(logFile, logMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write log to file: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取日志文件夹路径
        /// </summary>
        public static string GetLogFolderPath()
        {
            return LogFolder;
        }

        /// <summary>
        /// 清理超过指定天数的日志文件
        /// </summary>
        public static void CleanOldLogs(int daysToKeep = 7)
        {
            try
            {
                var files = Directory.GetFiles(LogFolder, "*.txt");
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        File.Delete(file);
                        System.Diagnostics.Debug.WriteLine($"Deleted old log file: {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clean old logs: {ex.Message}");
            }
        }
    }
}
