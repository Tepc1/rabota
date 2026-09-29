using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using AutoCenterPlus.Client.Data;
using AutoCenterPlus.Client.Utils;

namespace AutoCenterPlus.Client.Services
{
    /// <summary>
    /// Сервис идентификации и аутентификации пользователей (Раздел 8, Шаг 3).
    /// Пароли хранятся только в виде хеша SHA-256 с солью.
    /// </summary>
    public class AuthService
    {
        private readonly DbConnection _db;

        public AuthService(DbConnection db) => _db = db;

        /// <summary>Хеширование пароля: SHA-256(соль + пароль), результат — Base64.</summary>
        public string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(salt + password));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Аутентификация по логину/паролю.
        /// Возвращает кортеж (успех, роль, ФИО).
        /// </summary>
        public (bool success, string? role, string? fullName) Authenticate(string login, string password)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT PasswordHash, Salt, Role, FullName FROM [User] WHERE Login = @Login", conn);
                cmd.Parameters.AddWithValue("@Login", login);   // параметризованный запрос — защита от SQL-инъекций

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var storedHash = reader["PasswordHash"].ToString();
                    var salt = reader["Salt"].ToString();
                    var inputHash = HashPassword(password, salt ?? "");

                    if (storedHash == inputHash)
                    {
                        Logger.Info($"Успешный вход: {login} ({reader["Role"]})");
                        return (true, reader["Role"].ToString(), reader["FullName"].ToString());
                    }
                }
                Logger.Warning($"Неудачная попытка входа: {login}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка аутентификации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return (false, null, null);
        }
    }
}
