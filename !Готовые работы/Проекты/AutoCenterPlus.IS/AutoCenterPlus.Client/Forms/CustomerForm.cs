using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using AutoCenterPlus.Client.Data;
using AutoCenterPlus.Client.Utils;

namespace AutoCenterPlus.Client.Forms
{
    /// <summary>
    /// CRUD-форма справочника «Клиенты» (Раздел 8, Шаг 2):
    /// просмотр, добавление, редактирование, удаление записей таблицы Customer.
    /// Все запросы параметризованы (защита от SQL-инъекций).
    /// </summary>
    public partial class CustomerForm : Form
    {
        private readonly DbConnection _db;
        private DataTable _data = new();

        public CustomerForm(DbConnection db)
        {
            InitializeComponent();
            _db = db;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT Id, Code AS [Код], Name AS [Наименование], INN, Address AS [Адрес], " +
                    "Phone AS [Телефон], Email, IsBuyer AS [Покупатель], IsSalesman AS [Поставщик] " +
                    "FROM Customer ORDER BY Name", conn);
                var adapter = new SqlDataAdapter(cmd);
                _data = new DataTable();
                adapter.Fill(_data);
                dataGridView1.DataSource = _data;
            }
            catch (Exception ex)
            {
                Logger.Error("LoadData: " + ex.Message);
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Простая форма ввода через диалог вопросов (для учебного проекта)
            var name = Microsoft.VisualBasic.Interaction.InputBox("Наименование клиента:", "Добавление");
            if (string.IsNullOrWhiteSpace(name)) return;
            var inn = Microsoft.VisualBasic.Interaction.InputBox("ИНН:", "Добавление");

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();
                var cmd = new SqlCommand(
                    "INSERT INTO Customer (Name, INN) VALUES (@Name, @Inn)", conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Inn", string.IsNullOrWhiteSpace(inn) ? "0" : inn);
                cmd.ExecuteNonQuery();
                LoadData();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // TC-05: клиент с таким ИНН уже существует (ограничение UNIQUE)
                MessageBox.Show("Клиент с таким ИНН уже существует.", "Нарушение уникальности",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;
            var id = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Id"].Value);
            var phone = Microsoft.VisualBasic.Interaction.InputBox("Новый телефон:", "Редактирование");

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();
                var cmd = new SqlCommand("UPDATE Customer SET Phone = @Phone WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;
            var id = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Id"].Value);

            if (MessageBox.Show("Удалить выбранного клиента?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                using var conn = _db.GetConnection();
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Customer WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
                LoadData();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Ссылочная целостность: у клиента есть заказы
                MessageBox.Show(
                    "Нельзя удалить клиента: на него ссылаются записи заказов.",
                    "Нарушение ссылочной целостности", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CustomerForm_Load(object sender, EventArgs e) { }
    }

    partial class CustomerForm
    {
        private System.ComponentModel.IContainer components = null;
        private DataGridView dataGridView1 = null!;
        private Button btnAdd = null!, btnUpdate = null!, btnDelete = null!, btnRefresh = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            dataGridView1 = new DataGridView
            {
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(760, 320),
                AllowUserToAddRows = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnAdd = new Button { Text = "Добавить", Location = new System.Drawing.Point(12, 345), Width = 100 };
            btnUpdate = new Button { Text = "Изменить", Location = new System.Drawing.Point(120, 345), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new System.Drawing.Point(228, 345), Width = 100 };
            btnRefresh = new Button { Text = "Обновить", Location = new System.Drawing.Point(336, 345), Width = 100 };
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnRefresh.Click += (s, e) => LoadData();

            Text = "Справочник клиентов";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new System.Drawing.Size(784, 389);
            Controls.AddRange(new Control[] { dataGridView1, btnAdd, btnUpdate, btnDelete, btnRefresh });
            Load += CustomerForm_Load;
        }
    }
}
