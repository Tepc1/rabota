using System;
using System.Windows.Forms;
using AutoCenterPlus.Client.Data;
using AutoCenterPlus.Client.Services;

namespace AutoCenterPlus.Client.Forms
{
    /// <summary>
    /// Форма входа (Раздел 8, Шаг 3). Идентификация и аутентификация.
    /// </summary>
    public partial class LoginForm : Form
    {
        private readonly AuthService _auth;

        /// <summary>Роль вошедшего пользователя (Admin/Manager/Operator).</summary>
        public string CurrentRole { get; private set; } = "";
        public string CurrentUserName { get; private set; } = "";

        public LoginForm(DbConnection db)
        {
            InitializeComponent();
            _auth = new AuthService(db);
            Text = "Вход в систему — Автоцентр «Плюс»";
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Валидация ввода: пустой логин/пароль (граничный тест TC-03)
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Заполните поле «Логин».", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Заполните поле «Пароль».", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var (success, role, fullName) = _auth.Authenticate(txtLogin.Text.Trim(), txtPassword.Text);
            if (success)
            {
                CurrentRole = role ?? "";
                CurrentUserName = fullName ?? txtLogin.Text;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                // Негативный сценарий TC-02
                MessageBox.Show("Неверный логин или пароль.", "Ошибка входа",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ===== Designer-код формы (в VS2022 генерируется конструктором форм) =====
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtLogin = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Label label1 = null!;
        private Label label2 = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            label1 = new Label { Text = "Логин:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            label2 = new Label { Text = "Пароль:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            txtLogin = new TextBox { Location = new System.Drawing.Point(100, 22), Width = 200 };
            txtPassword = new TextBox { Location = new System.Drawing.Point(100, 57), Width = 200, UseSystemPasswordChar = true };
            btnLogin = new Button { Text = "Войти", Location = new System.Drawing.Point(100, 95), Width = 100 };
            btnLogin.Click += btnLogin_Click;

            AcceptButton = btnLogin;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new System.Drawing.Size(334, 141);
            Controls.AddRange(new Control[] { label1, label2, txtLogin, txtPassword, btnLogin });
        }
    }
}
