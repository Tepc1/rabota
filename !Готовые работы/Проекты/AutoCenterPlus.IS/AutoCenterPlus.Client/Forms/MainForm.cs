using System;
using System.Windows.Forms;
using AutoCenterPlus.Client.Data;

namespace AutoCenterPlus.Client.Forms
{
    /// <summary>
    /// Главная форма ИС «Автоцентр Плюс». Меню модулей с учётом RBAC:
    /// отчёт «АнализЗаказовКлиентов» доступен только роли «Менеджер».
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly DbConnection _db;
        private readonly string _role;
        private readonly string _userName;

        public MainForm(DbConnection db, string role, string userName)
        {
            InitializeComponent();
            _db = db;
            _role = role;
            _userName = userName;
            Text = $"Автоцентр «Плюс» — ИС. Пользователь: {userName} ({role})";

            // Скрытие пунктов меню в соответствии с ролью (RBAC)
            menuReport.Visible = _role == "Manager" || _role == "Admin";
            menuUsers.Visible  = _role == "Admin";
        }

        private void menuCustomers_Click(object sender, EventArgs e)
        {
            using var f = new CustomerForm(_db);
            f.ShowDialog(this);
        }

        private void menuOrders_Click(object sender, EventArgs e)
        {
            using var f = new OrderForm(_db);
            f.ShowDialog(this);
        }

        private void menuReport_Click(object sender, EventArgs e)
        {
            // Раздел 10: модуль вариативной части ДЭ
            using var f = new ClientOrderReportForm(_db, _role);
            f.ShowDialog(this);
        }

        private void menuExit_Click(object sender, EventArgs e) => Close();
    }

    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private MenuStrip menuStrip1 = null!;
        private ToolStripMenuItem menuFile = null!, menuExit = null!;
        private ToolStripMenuItem menuRefs = null!, menuCustomers = null!, menuOrders = null!;
        private ToolStripMenuItem menuReport = null!, menuUsers = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            menuFile = new ToolStripMenuItem("&Файл");
            menuExit = new ToolStripMenuItem("&Выход");
            menuExit.Click += menuExit_Click;
            menuFile.DropDownItems.Add(menuExit);

            menuRefs = new ToolStripMenuItem("&Справочники");
            menuCustomers = new ToolStripMenuItem("Клиенты (Customer)");
            menuCustomers.Click += menuCustomers_Click;
            menuOrders = new ToolStripMenuItem("Расчёт стоимости заказа");
            menuOrders.Click += menuOrders_Click;
            menuRefs.DropDownItems.AddRange(new ToolStripItem[] { menuCustomers, menuOrders });

            menuReport = new ToolStripMenuItem("&Отчёты");
            var miAnalysis = new ToolStripMenuItem("АнализЗаказовКлиентов");
            miAnalysis.Click += menuReport_Click;
            menuReport.DropDownItems.Add(miAnalysis);

            menuUsers = new ToolStripMenuItem("&Пользователи");

            menuStrip1.Items.AddRange(new ToolStripItem[] { menuFile, menuRefs, menuReport, menuUsers });
            MainMenuStrip = menuStrip1;
            ClientSize = new System.Drawing.Size(800, 500);
            Controls.Add(menuStrip1);
        }
    }
}
