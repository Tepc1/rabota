/* ============================================================================
   УП06. Вариант 22. Автоцентр «Плюс». РАЗДЕЛ 5.
   «ЭТАЛОННЫЙ» МОДУЛЬ С НАМЕРЕННО ВНЕСЁННЫМИ ОШИБКАМИ (код «ДО» исправления).
   Получен от руководителя практики. Статический анализ VS2022/SonarLint +
   отладчик выявили 8 ошибок (см. Протокол исправлений, файл 5.docx).
   НЕ ВКЛЮЧЁН в сборку решения — хранится как листинг для отчёта.
   ============================================================================ */
using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace AutoCenterPlus.Client.Forms
{
    public class OrderForm_Before : Form
    {
        // ОШИБКА 1 (CA2213): соединение объявлено полем и никогда не закрывается — утечка ресурсов
        private SqlConnection connection = new SqlConnection(
            @"Server=(localdb)\mssqllocaldb;Database=AutoCenterPlusDB;Trusted_Connection=True;");

        private TextBox txtOrderId;
        private TextBox txtTotal;

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            // ОШИБКА 2 (отсутствие обработки исключений): преобразование без проверки —
            // FormatException при вводе "abc" → приложение падает
            int orderId = int.Parse(txtOrderId.Text);

            // ОШИБКА 3 (CA2100, SQL-инъекция): конкатенация вместо параметризованного запроса
            string query = "SELECT SUM(TotalPositionCost) FROM v_OrderFullCost WHERE OrderId = " + orderId;

            // ОШИБКА 4 (некорректный SQL): в БД нет таблицы ProductPrice — ошибка выполнения
            string query2 = "SELECT * FROM ProductPrice";

            // ОШИБКА 5 (бизнес-логика): стоимость считалась БЕЗ учёта Quantity —
            // итог занижен (в ТЗ: «с учётом количества продукции в заказе»)
            decimal totalCost = 0;
            var cmd = new SqlCommand(query, connection);
            connection.Open();                      // ОШИБКА 6: повторное открытие открытого соединения → InvalidOperationException
            var reader = cmd.ExecuteReader();       // ОШИБКА 7: SqlDataReader не закрыт (нет using)
            while (reader.Read())
                totalCost += reader.IsDBNull(0) ? 0 : Convert.ToDecimal(reader[0]);

            // ОШИБКА 8 (деление на ноль): расчёт процента скидки без проверки знаменателя
            decimal discount = 100;
            decimal percent = discount / totalCost * 100;   // DivideByZeroException при totalCost == 0

            txtTotal.Text = totalCost.ToString("C");
        }
    }
}
