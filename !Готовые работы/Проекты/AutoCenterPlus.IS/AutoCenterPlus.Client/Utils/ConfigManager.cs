using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace AutoCenterPlus.Client.Utils
{
    /// <summary>
    /// Менеджер параметров ИС (Раздел 8, Шаг 5):
    /// строка подключения, уровень/путь логирования, язык интерфейса, тема оформления.
    /// </summary>
    public static class ConfigManager
    {
        public static string ConnectionString { get; private set; } =
            @"Server=(localdb)\mssqllocaldb;Database=AutoCenterPlusDB;Trusted_Connection=True;";
        public static LogLevel LogLevel { get; private set; } = LogLevel.Info;
        public static string LogPath { get; private set; } = "logs/app.log";
        public static string Language { get; private set; } = "ru";
        public static string Theme { get; private set; } = "Light";

        public static void Load()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (!File.Exists(path)) return;   // используются значения по умолчанию

            var json = JObject.Parse(File.ReadAllText(path));
            ConnectionString = json["ConnectionStrings"]?["Default"]?.ToString() ?? ConnectionString;
            LogPath          = json["Logging"]?["Path"]?.ToString() ?? LogPath;
            Language         = json["Ui"]?["Language"]?.ToString() ?? Language;
            Theme            = json["Ui"]?["Theme"]?.ToString() ?? Theme;

            if (Enum.TryParse<LogLevel>(json["Logging"]?["Level"]?.ToString() ?? "", true, out var lvl))
                LogLevel = lvl;
        }

        public static string GetConnectionString() => ConnectionString;
    }
}
