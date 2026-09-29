using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using AutoCenterPlus.Client.Data;
using AutoCenterPlus.Client.Utils;

namespace AutoCenterPlus.Client.Forms
{
    /// <summary>
    /// Форма отчёта «АнализЗаказовКлиентов» (Раздел 10, вариативная часть ДЭ).
    /// Требования:
    ///  - доступна только роли «Менеджер» (RBAC);
    ///  - фильтр по «Клиент» — необязательный ComboBox;
    ///  - DataGridView: Клиент, Товары (работы, услуги), Код, Кол-во, Ед., Цена, Скидка, Сумма;
    ///  - расчёт % скидки = (Сумма скидки / Сумма заказа) * 100;
    ///  - строка «ИТОГО»; условное оформление (зелёный фон при скидке &gt; 20%);
    ///  - обработка исключительных ситуаций.
    /// </summary>
    public partial class ClientOrderReportForm : Form
    {
        private readonly DbConnection _db;
        private readonly string _role;

        // Индексы колонок DataGridView (соответствуют требованиям п.10.2)
        private const int ColDiscountPercent = 8;   // скрытая служебная колонка «% скидки»

        public ClientOrderReportForm(DbConnection db, string role)
        {
            // П.10.6 RBAC: проверка роли ДО открытия формы
            if (role != "Manager")
            {
                MessageBox.Show(
                    "У вас недостаточно прав для просмотра данного отчета. Обратитесь к администратору",
                    "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Закрываем форму сразу после инициализации
                Load += (s, e) => Close();
            }

            _db = db;
            _role = role;
            InitializeComponent();
            LoadClients();
        }

        /// <summary>Заполнение фильтра клиентов: пустое значение = «все клиенты».</summary>
        private void LoadClients()
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();
                var cmd = new SqlCommand("SELECT Id, Name FROM Customer ORDER BY Name", conn);
                using var reader = cmd.ExecuteReader();
                cmbClient.Items.Add("");                       // пустое значение по умолчанию
                while (reader.Read())
                    cmbClient.Items.Add(reader["Name"].ToString());
                cmbClient.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Logger.Error("LoadClients: " + ex.Message);
                MessageBox.Show($"Ошибка загрузки списка клиентов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // П.10.7 Защита от повторного нажатия во время выполнения запроса
            btnGenerate.Enabled = false;
            try
            {
                var table = GetReportData(cmbClient.Text.Trim());

                if (table.Rows.Count == 0)
                {
                    // Исключительная ситуация: нет данных по фильтру
                    MessageBox.Show(
                        "Данные за выбранный период или по выбранному клиенту отсутствуют",
                        "Нет данных", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dataGridView1.DataSource = null;
                    return;
                }

                AddTotalRow(table);
                dataGridView1.DataSource = table;
                FormatColumns();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка обращения к БД: {ex.Message}", "Ошибка БД",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непредвиденная ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGenerate.Enabled = true;
            }
        }

        /// <summary>
        /// Выборка из VIEW v_ClientOrderAnalysis (SQL/10_ClientOrderAnalysis.sql).
        /// Параметризованный запрос: @Client IS NULL → все клиенты.
        /// </summary>
        private DataTable GetReportData(string clientName)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            const string sql = @"
                SELECT ClientName AS [Клиент],
                       ProductName AS [Товары (работы, услуги)],
                       ProductCode AS [Код],
                       Quantity    AS [Кол-во],
                       Unit        AS [Ед.],
                       Price       AS [Цена],
                       Discount    AS [Скидка],
                       TotalSum    AS [Сумма],
                       OrderId
                FROM v_ClientOrderAnalysis
                WHERE (@Client IS NULL OR ClientName = @Client)
                ORDER BY ClientName, ProductName";

            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Client",
                string.IsNullOrEmpty(clientName) ? (object)DBNull.Value : clientName);

            var dt = new DataTable();
            using (var reader = cmd.ExecuteReader())
                dt.Load(reader);

            // Служебный столбец процента скидки для условного оформления
            dt.Columns.Add("DiscountPercent", typeof(decimal));
            foreach (DataRow row in dt.Rows)
            {
                decimal sum = Convert.ToDecimal(row["Сумма"]);
                decimal discount = Convert.ToDecimal(row["Скидка"]);
                // П.10.3 формула: (Сумма скидки / Сумма заказа) * 100.
                // Предотвращение «деления на ноль»: если сумма = 0 → 0%.
                row["DiscountPercent"] = sum > 0
                    ? Math.Round(discount / sum * 100, 2)
                    : 0m;
            }
            return dt;
        }

        /// <summary>
        /// П.10.4 Строка «ИТОГО» с суммарными значениями по колонкам «Скидка» и «Сумма».
        /// Алгоритм расчёта средневзвешенного процента в строке «ИТОГО»:
        /// ИТОГО % = (Σ скидок / Σ сумм) * 100, т.е. процент считается ОТ ОБЩЕЙ суммы
        /// всех заказов, а не как среднее арифметическое процентов по строкам.
        /// Это корректно, т.к. крупные заказы «весят» больше мелких.
        /// </summary>
        private void AddTotalRow(DataTable table)
        {
            decimal totalDiscount = 0, totalSum = 0;
            foreach (DataRow row in table.Rows)
            {
                totalDiscount += Convert.ToDecimal(row["Скидка"]);
                totalSum += Convert.ToDecimal(row["Сумма"]);
            }

            var total = table.NewRow();
            total["Клиент"] = "ИТОГО";
            total["Скидка"] = totalDiscount;
            total["Сумма"] = totalSum;
            // Защита от деления на ноль при нулевой итоговой сумме
            total["DiscountPercent"] = totalSum > 0
                ? Math.Round(totalDiscount / totalSum * 100, 2)
                : 0m;
            table.Rows.Add(total);
        }

        private void FormatColumns()
        {
            dataGridView1.Columns["OrderId"].Visible = false;
            dataGridView1.Columns["DiscountPercent"].Visible = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ClearSelection();
        }

        /// <summary>
        /// П.10.5 Условное оформление: если % скидки &gt; 20%, ячейка выделяется зелёным фоном.
        /// Логика: событие CellFormatting срабатывает при каждой перерисовке ячейки;
        /// берём из строки служебное значение DiscountPercent и закрашиваем ячейки
        /// «Скидка» и «% скидки», если порог превышен.
        /// </summary>
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dataGridView1.Columns[e.ColumnIndex].Name != "Скидка")
                return;

            var row = dataGridView1.Rows[e.RowIndex];
            if (row.IsNewRow) return;

            object raw = row.Cells["DiscountPercent"].Value;
            if (raw == null || raw == DBNull.Value) return;

            decimal percent = Convert.ToDecimal(raw);
            if (percent > 20)
            {
                row.Cells["Скидка"].Style.BackColor = Color.LightGreen;
                row.Cells["Сумма"].Style.BackColor = Color.LightGreen;
            }
        }
    }

    partial class ClientOrderReportForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label label1 = null!;
        private ComboBox cmbClient = null!;
        private Button btnGenerate = null!;
        private DataGridView dataGridView1 = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            label1 = new Label { Text = "Клиент:", Location = new Point(12, 15), AutoSize = true };
            cmbClient = new ComboBox
            {
                Location = new Point(70, 12),
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            btnGenerate = new Button { Text = "Сформировать", Location = new Point(350, 10), Width = 120 };
            btnGenerate.Click += btnGenerate_Click;
            dataGridView1 = new DataGridView
            {
                Location = new Point(12, 45),
                Size = new Size(960, 460),
                AllowUserToAddRows = false,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;

            Text = "Отчёт «АнализЗаказовКлиентов»";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(984, 511);
            Controls.AddRange(new Control[] { label1, cmbClient, btnGenerate, dataGridView1 });
        }
    }
}
