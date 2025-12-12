using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace OptoHelper.Log
{
    public static class Log
    {
        private static readonly object _lock = new object();

        // ================================
        //      INI 文件读取工具
        // ================================
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(
            string section, string key, string defaultValue,
            StringBuilder retVal, int size, string filePath);

        private static string IniPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");

        private static string ReadIni(string section, string key, string defaultValue)
        {
            var sb = new StringBuilder(255);
            GetPrivateProfileString(section, key, defaultValue, sb, sb.Capacity, IniPath);
            return sb.ToString();
        }

        // ================================
        //      配置参数读取
        // ================================
        private static bool EnableLog =>
            ReadIni("Log", "EnableLog", "true").ToLower() == "true";

        private static int SaveDays =>
            int.TryParse(ReadIni("Log", "SaveDays", "7"), out int days) ? days : 7;

        // 日志路径 = 程序目录 / OperationLogs
        private static readonly string logDirectory =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OperationLogs");

        private static string LogFilePath =>
            Path.Combine(logDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");

        // ================================
        //      对外方法：异步
        // ================================
        public static Task InfoAsync(string message) =>
            WriteLogAsync("INFO", message);

        public static Task WarningAsync(string message) =>
            WriteLogAsync("WARNING", message);

        public static Task ErrorAsync(string message) =>
            WriteLogAsync("ERROR", message);

        // ================================
        //      对外方法：同步
        // ================================
        public static void Info(string message) =>
            WriteLog("INFO", message);

        public static void Warning(string message) =>
            WriteLog("WARNING", message);

        public static void Error(string message) =>
            WriteLog("ERROR", message);

        // ================================
        //      日志写入（异步）
        // ================================
        private static async Task WriteLogAsync(string level, string message)
        {
            if (!EnableLog) return;

            try
            {
                Directory.CreateDirectory(logDirectory);
                string path = LogFilePath;
                string txt = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";

                await Task.Run(() =>
                {
                    lock (_lock)
                    {
                        File.AppendAllText(path, txt, Encoding.UTF8);
                    }
                });

                CleanupOldLogs();
            }
            catch (Exception ex)
            {
                Console.WriteLine("异步日志失败: " + ex.Message);
            }
        }

        // ================================
        //      日志写入（同步）
        // ================================
        private static void WriteLog(string level, string message)
        {
            if (!EnableLog) return;

            lock (_lock)
            {
                try
                {
                    Directory.CreateDirectory(logDirectory);
                    string path = LogFilePath;
                    string txt = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";
                    File.AppendAllText(path, txt, Encoding.UTF8);

                    CleanupOldLogs();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("日志写入失败: " + ex.Message);
                }
            }
        }

        // ================================
        //      自动删除过期日志
        // ================================
        private static void CleanupOldLogs()
        {
            try
            {
                if (!Directory.Exists(logDirectory)) return;

                var files = Directory.GetFiles(logDirectory, "*.log");

                foreach (var file in files)
                {
                    var createTime = File.GetCreationTime(file);

                    if ((DateTime.Now - createTime).TotalDays > SaveDays)
                        File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("日志清理失败: " + ex.Message);
            }
        }
    }
}
