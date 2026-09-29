using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using AutoCenterPlus.Api.Models;

namespace AutoCenterPlus.Api.Controllers
{
    /// <summary>
    /// API справочника клиентов (Раздел 9, Шаг 2).
    /// Данные перед отправкой проходят обработку: фильтрация и агрегация.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        /// <summary>
        /// GET /api/customers — список клиентов с агрегатом «Сумма заказов».
        /// Негативный тест TC-11: inn="123" → 400 BadRequest.
        /// </summary>
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? name = null, [FromQuery] string? inn = null)
        {
            // Валидация параметров (агрегация/фильтрация на входе)
            if (inn != null && inn.Length is < 10 or > 12)
                return BadRequest(new { error = "ИНН должен содержать 10 или 12 цифр" });

            const string sql = @"
                SELECT c.Id, c.Name, c.INN, c.Phone,
                       ISNULL(SUM(o.TotalAmount), 0) AS OrdersSum,   -- агрегация
                       COUNT(o.Id) AS OrdersCount
                FROM Customer c
                LEFT JOIN [Order] o ON o.CustomerId = c.Id
                WHERE (@Name IS NULL OR c.Name LIKE '%' + @Name + '%')
                  AND (@Inn  IS NULL OR c.INN = @Inn)
                GROUP BY c.Id, c.Name, c.INN, c.Phone";

            try
            {
                using var conn = new SqlConnection(DbSettings.ConnectionString);
                conn.Open();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name",
                    string.IsNullOrEmpty(name) ? (object)DBNull.Value : name);
                cmd.Parameters.AddWithValue("@Inn",
                    string.IsNullOrEmpty(inn) ? (object)DBNull.Value : inn);

                var list = new List<object>();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new
                        {
                            id = r.GetInt32(0),
                            name = r.GetString(1),
                            inn = r.GetString(2),
                            phone = r.IsDBNull(3) ? null : r.GetString(3),
                            ordersSum = r.GetDecimal(4),
                            ordersCount = r.GetInt32(5)
                        });

                return Ok(list);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { error = "Ошибка БД: " + ex.Message });
            }
        }

        /// <summary>GET /api/customers/{id} — карточка клиента. Граничный тест TC-12: несуществующий id → 404.</summary>
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            const string sql = "SELECT Id, Name, INN, Address, Phone FROM Customer WHERE Id = @Id";
            try
            {
                using var conn = new SqlConnection(DbSettings.ConnectionString);
                conn.Open();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                using var r = cmd.ExecuteReader();
                if (!r.Read())
                    return NotFound(new { error = $"Клиент с Id={id} не найден" });

                return Ok(new
                {
                    id = r.GetInt32(0),
                    name = r.GetString(1),
                    inn = r.GetString(2),
                    address = r.IsDBNull(3) ? null : r.GetString(3),
                    phone = r.IsDBNull(4) ? null : r.GetString(4)
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { error = "Ошибка БД: " + ex.Message });
            }
        }
    }
}
