using System;
using System.IO;

namespace AutoCenterPlus.Client.Utils
{
    /// <summary>Простой файловый логгер с настраиваемым уровнем (Раздел 8, Шаг 5 — параметр логирования).</summary>
    public enum LogLevel { Debug = 0, Info = 1, Warning = 2, Error = 3 }

    public static class Logger
    {
        private static readonly object Lock = new object();

        public static void Info(string msg)    => Write(LogLevel.Info, msg);
        public static void Warning(string msg) => Write(LogLevel.Warning, msg);
        public static void Error(string msg)   => Write(LogLevel.Error, msg);

        private static void Write(LogLevel level, string msg)
        {
            if (level < ConfigManager.LogLevel) return;
            try
            {
                lock (Lock)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ConfigManager.LogPath) ?? ".");
                    File.AppendAllText(ConfigManager.LogPath,
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {msg}{Environment.NewLine}");
                }
            }
            catch { /* лог не критичен для работы приложения */ }
        }
    }
}
