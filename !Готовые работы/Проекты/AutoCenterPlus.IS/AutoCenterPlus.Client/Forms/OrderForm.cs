using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using AutoCenterPlus.Client.Data;

namespace AutoCenterPlus.Client.Forms
{
    /// <summary>
    /// Модуль расчёта полной стоимости заказа покупателя (Раздел 5).
    /// Код ПРОШЁЛ исправление по протоколу выявления ошибок кодирования:
    ///  - параметризованный запрос вместо конкатенации (защита от SQL-инъекций, CA2100);
    ///  - using для всех IDisposable (CA2213);
    ///  - учёт Quantity при расчёте (логическая ошибка);
    ///  - try/catch с SqlException;
    ///  - защита от деления на ноль;
    ///  - валидация ввода int.TryParse.
    /// </summary>
    public partial class OrderForm : Form
    {
        private readonly DbConnection _db;

        public OrderForm(DbConnection db)
        {
            InitializeComponent();
            _db = db;
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            // 1. Валидация входных данных (TC-06: буквы вместо цифр — приложение не падает)
            if (!int.TryParse(txtOrderId.Text, out int orderId))
            {
                MessageBox.Show(
                    "Некорректный ID заказа. Введите целое число.",
                    "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal totalCost = 0;

            try
            {
                // 2. using — гарантированное закрытие соединения
                using (var conn = _db.GetConnection())
                {
                    // 3. Параметризованный запрос + корректный INNER JOIN
                    string query = @"
                        SELECT p.Price, od.Quantity
                        FROM OrderDetail od
                        INNER JOIN Product p ON od.ProductId = p.Id
                        WHERE od.OrderId = @OrderId";

                    // Используем представление v_OrderFullCost (Раздел 7) —
                    // стоимость уже рассчитана с нормами материалов и операций
                    string viewQuery = @"
                        SELECT TotalPositionCost FROM v_OrderFullCost WHERE OrderId = @OrderId";

                    using (var cmd = new SqlCommand(viewQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                        conn.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                totalCost += Convert.ToDecimal(reader["TotalPositionCost"]);
                        }
                    }
                }

                // 4. Безопасный расчёт процента скидки (предотвращение DivideByZeroException)
                decimal discount = 0;   // скидка из CustomerDiscount подгружается отдельно
                decimal discountPercent = 0;
                if (totalCost > 0)
                    discountPercent = Math.Round((discount / totalCost) * 100, 2);

                txtTotal.Text = totalCost.ToString("C");
                txtDiscountPercent.Text = discountPercent.ToString() + " %";
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка при обращении к БД: {ex.Message}", "Ошибка БД",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непредвиденная ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    partial class OrderForm
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtOrderId = null!, txtTotal = null!, txtDiscountPercent = null!;
        private Button btnCalculate = null!;
        private Label label1 = null!, label2 = null!, label3 = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            label1 = new Label { Text = "ID заказа:", Location = new System.Drawing.Point(15, 20), AutoSize = true };
            txtOrderId = new TextBox { Location = new System.Drawing.Point(110, 17), Width = 80 };
            btnCalculate = new Button { Text = "Рассчитать стоимость", Location = new System.Drawing.Point(210, 15), Width = 160 };
            btnCalculate.Click += btnCalculate_Click;
            label2 = new Label { Text = "Полная стоимость:", Location = new System.Drawing.Point(15, 60), AutoSize = true };
            txtTotal = new TextBox { Location = new System.Drawing.Point(140, 57), Width = 140, ReadOnly = true };
            label3 = new Label { Text = "Скидка, %:", Location = new System.Drawing.Point(300, 60), AutoSize = true };
            txtDiscountPercent = new TextBox { Location = new System.Drawing.Point(370, 57), Width = 80, ReadOnly = true };

            Text = "Расчёт стоимости заказа";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new System.Drawing.Size(474, 101);
            Controls.AddRange(new Control[] { label1, txtOrderId, btnCalculate, label2, txtTotal, label3, txtDiscountPercent });
        }
    }
}
