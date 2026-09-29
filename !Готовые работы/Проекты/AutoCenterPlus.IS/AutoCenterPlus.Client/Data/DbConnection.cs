using System;
using System.Data.SqlClient;
using AutoCenterPlus.Client.Utils;

namespace AutoCenterPlus.Client.Data
{
    /// <summary>
    /// Класс подключения к MS SQL Server. Строка подключения берётся
    /// из настроек ИС (appsettings.json) — Раздел 8, Шаг 5.
    /// </summary>
    public class DbConnection
    {
        private readonly string _connectionString;

        public DbConnection()
        {
            _connectionString = ConfigManager.GetConnectionString();
        }

        public DbConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>Возвращает НЕОТКРЫТОЕ соединение. Открытие — ответственность вызывающего (using).</summary>
        public SqlConnection GetConnection() => new SqlConnection(_connectionString);

        /// <summary>Проверка доступности сервера БД.</summary>
        public bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("TestConnection: " + ex.Message);
                return false;
            }
        }
    }
}
