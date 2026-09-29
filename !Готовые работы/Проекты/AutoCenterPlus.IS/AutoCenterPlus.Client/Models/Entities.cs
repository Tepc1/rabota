using System;

namespace AutoCenterPlus.Client.Models
{
    /// <summary>Заказчик (клиент автоцентра).</summary>
    public class Customer
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = "";
        public string INN { get; set; } = "";
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsBuyer { get; set; }
        public bool IsSalesman { get; set; }
    }

    /// <summary>Продукция (работа/услуга автоцентра).</summary>
    public class Product
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = "";
        public string Unit { get; set; } = "шт";
    }

    /// <summary>Материал (запчасть/расходник).</summary>
    public class Material
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Price { get; set; }
    }

    /// <summary>Технологическая операция.</summary>
    public class Operation
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Price { get; set; }
    }

    /// <summary>Заказ покупателя.</summary>
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>Позиция заказа.</summary>
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
    }
}
