using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using AutoCenterPlus.Api.Models;

namespace AutoCenterPlus.Api.Controllers
{
    /// <summary>
    /// API расчёта стоимости заказов (Раздел 9).
    /// Возвращает обработанные (агрегированные) данные из VIEW v_OrderFullCost.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        /// <summary>
        /// GET /api/orders/{id}/cost — полная стоимость заказа.
        /// Тест TC-10: id=1 → 200 + sum &gt; 0. TC-12: id=9999 → сумма 0 (пустая выборка).
        /// </summary>
        [HttpGet("{id:int}/cost")]
        public IActionResult GetOrderCost(int id)
        {
            // Агрегация выполняется хранимой процедурью sp_CalculateOrderCost (Раздел 7)
            try
            {
                using var conn = new SqlConnection(DbSettings.ConnectionString);
                conn.Open();
                using var cmd = new SqlCommand("sp_CalculateOrderCost", conn)
                { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@OrderId", id);
                var outp = new SqlParameter("@TotalCost", System.Data.SqlDbType.Decimal, 14, 2)
                { Direction = System.Data.ParameterDirection.Output };
                cmd.Parameters.Add(outp);
                cmd.ExecuteNonQuery();

                decimal total = outp.Value == DBNull.Value ? 0 : Convert.ToDecimal(outp.Value);
                if (total == 0)
                    return Ok(new { orderId = id, totalCost = 0m, message = "Заказ не найден или позиции отсутствуют" });

                return Ok(new { orderId = id, totalCost = total });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { error = "Ошибка БД: " + ex.Message });
            }
        }

        /// <summary>
        /// GET /api/orders/analysis?client=... — данные отчёта «АнализЗаказовКлиентов»
        /// (подготовка к заданию ДЭ, п.10). Фильтрация по клиенту необязательна.
        /// </summary>
        [HttpGet("analysis")]
        public IActionResult GetAnalysis([FromQuery] string? client = null)
        {
            const string sql = @"
                SELECT ClientName, ProductName, ProductCode, Quantity, Unit,
                       Price, Discount, TotalSum
                FROM v_ClientOrderAnalysis
                WHERE (@Client IS NULL OR ClientName = @Client)";
            try
            {
                using var conn = new SqlConnection(DbSettings.ConnectionString);
                conn.Open();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Client",
                    string.IsNullOrEmpty(client) ? (object)DBNull.Value : client);

                var rows = new List<object>();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        rows.Add(new
                        {
                            client = r.GetString(0),
                            product = r.GetString(1),
                            code = r.GetInt32(2),
                            quantity = r.GetDecimal(3),
                            unit = r.GetString(4),
                            price = r.GetDecimal(5),
                            discount = r.GetDecimal(6),
                            sum = r.GetDecimal(7)
                        });

                // Обработка данных перед отправкой: расчёт процента скидки
                decimal totalDiscount = rows.Sum(x => (decimal)x.GetType().GetProperty("discount")!.GetValue(x)!);
                decimal totalSum = rows.Sum(x => (decimal)x.GetType().GetProperty("sum")!.GetValue(x)!);

                return Ok(new
                {
                    items = rows,
                    total = new
                    {
                        discount = totalDiscount,
                        sum = totalSum,
                        // Защита от деления на ноль (п.10.7)
                        discountPercent = totalSum > 0
                            ? Math.Round(totalDiscount / totalSum * 100, 2) : 0
                    }
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { error = "Ошибка БД: " + ex.Message });
            }
        }
    }
}
