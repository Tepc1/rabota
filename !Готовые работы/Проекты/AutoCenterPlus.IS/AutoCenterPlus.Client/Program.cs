using System;
using System.Windows.Forms;
using AutoCenterPlus.Client.Forms;

namespace AutoCenterPlus.Client
{
    /// <summary>
    /// Точка входа клиентского приложения ИС «Автоцентр Плюс» (Раздел 8).
    /// </summary>
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Загрузка параметров ИС из appsettings.json (строка подключения, логирование)
            Utils.ConfigManager.Load();

            // Проверка доступности БД при запуске
            var db = new Data.DbConnection();
            if (!db.TestConnection())
            {
                MessageBox.Show(
                    "Не удалось подключиться к базе данных AutoCenterPlusDB.\n" +
                    "Проверьте параметры подключения в appsettings.json.",
                    "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Идентификация и аутентификация пользователя
            using (var loginForm = new LoginForm(db))
            {
                if (loginForm.ShowDialog() != DialogResult.OK) return;

                // Главная форма с учётом роли вошедшего пользователя (RBAC)
                Application.Run(new MainForm(db, loginForm.CurrentRole, loginForm.CurrentUserName));
            }
        }
    }
}
