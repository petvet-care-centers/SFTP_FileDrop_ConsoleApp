using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SFTP_FileDrop2.Classes
{
    internal class Log
    {
        private readonly IConfiguration _config;



        private static readonly object _logLock = new object();
        private static string _timeStamp => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]";
        private static int _maxLogSizeMb;
        public static readonly string LogName = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".log");
        public static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log", LogName);
        public Log(IConfiguration config)
        {
            _config = config;
            _maxLogSizeMb = int.Parse(_config["appsettings:LogMaxSizeMB"]) * 1024 * 1024;
        }
        static Log()
        {
            var fileInfo = new FileInfo(LogPath);

            if (!Directory.Exists(fileInfo.Directory.FullName))
            {
                Directory.CreateDirectory(fileInfo.Directory.FullName);
            }
        }

        public static void Error(string message, [CallerMemberName] string method = null)
        {
            WriteToLog($"ERROR: {message}, in method: {method}");
        }

        public static void Info(string message)
        {
            WriteToLog($"Info: {message}");
        }

        public static void Exception(string message, Exception ex, [CallerMemberName] string method = null)
        {
            WriteToLog($"Exception: {message}, in method: {method}");
            WriteToLog($"Inner Exceptions: {GetInnerExceptions(ex.InnerException)}");
        }

        private static string GetInnerExceptions(Exception ex)
        {
            var message = ex.Message + "\r\n";

            if (ex.InnerException != null)
            {
                message = GetInnerExceptions(ex.InnerException);
            }

            return message;
        }

        private static void WriteToLog(string message)
        {
            lock (_logLock)
            {
                LogRolling();

                using (var sw = new StreamWriter(new FileStream(LogPath, FileMode.Append, FileAccess.Write)))
                {
                    sw.WriteLine($"{_timeStamp} | {message}");
                }
            }
        }

        private static void LogRolling()
        {
            var fileInfo = new FileInfo(LogPath);
            if (File.Exists(fileInfo.FullName) && fileInfo.Length > _maxLogSizeMb)
            {
                var today = DateTime.Today.ToString("yyyy-MM-dd");
                var newName = fileInfo.FullName.Replace(".log", $"_{today}.log");
                File.Move(fileInfo.FullName, newName);
            }
        }
    }
}
