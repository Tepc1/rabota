/* ============================================================
   УП06. Вариант 22. Автоцентр «Плюс»
   Раздел 10. Подготовка данных для модуля «АнализЗаказовКлиентов»
   (вариативная часть ДЭ, КИМ 09.02.07-5-2027)
   ============================================================ */
USE AutoCenterPlusDB;
GO

-- Таблица скидок по заказам клиентов
IF OBJECT_ID('CustomerDiscount') IS NOT NULL DROP TABLE CustomerDiscount;
GO
CREATE TABLE CustomerDiscount (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    DiscountAmount DECIMAL(12,2) NOT NULL CHECK (DiscountAmount >= 0),
    OrderId INT NOT NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customer(Id),
    FOREIGN KEY (OrderId) REFERENCES [Order](Id)
);
GO

-- Тестовые данные скидок (для сценариев тестирования: 0%, <20%, >20%)
INSERT INTO CustomerDiscount (CustomerId, DiscountAmount, OrderId)
SELECT c.Id, v.Discount, o.Id
FROM (VALUES
    (N'ООО "Фермер"',      CAST(8000  AS DECIMAL(12,2))),  -- ~4% (скидка < 20%)
    (N'ООО "Ромашка"',     CAST(15000 AS DECIMAL(12,2))),  -- > 20% (зелёное оформление)
    (N'ООО "Ассоль"',      CAST(0     AS DECIMAL(12,2)))   -- 0% (проверка деления на ноль)
) v(cust, Discount)
INNER JOIN Customer c ON c.Name = v.cust
INNER JOIN [Order] o ON o.CustomerId = c.Id;
GO

-- VIEW для отчёта «АнализЗаказовКлиентов»
IF OBJECT_ID('v_ClientOrderAnalysis') IS NOT NULL DROP VIEW v_ClientOrderAnalysis;
GO
CREATE VIEW v_ClientOrderAnalysis
AS
SELECT
    c.Id   AS ClientId,
    c.Name AS ClientName,
    p.Name AS ProductName,
    p.Id   AS ProductCode,
    od.Quantity,
    p.Unit,
    ISNULL(mc.MaterialsCost, 0) + ISNULL(oc.OperationsCost, 0) AS Price,   -- цена за ед.
    ISNULL(cd.DiscountAmount, 0) AS Discount,                               -- скидка по заказу
    od.Quantity * (ISNULL(mc.MaterialsCost, 0) + ISNULL(oc.OperationsCost, 0)) AS TotalSum,
    o.Id AS OrderId
FROM [Order] o
INNER JOIN Customer c ON o.CustomerId = c.Id
INNER JOIN OrderDetail od ON o.Id = od.OrderId
INNER JOIN Product p ON od.ProductId = p.Id
INNER JOIN Specification s ON p.Id = s.ProductId
LEFT JOIN CustomerDiscount cd ON cd.CustomerId = c.Id AND cd.OrderId = o.Id
OUTER APPLY (
    SELECT SUM(m.Price * sm.Norm) AS MaterialsCost
    FROM SpecMaterial sm INNER JOIN Material m ON sm.MaterialId = m.Id
    WHERE sm.SpecId = s.Id
) mc
OUTER APPLY (
    SELECT SUM(op.Price * so.Norm) AS OperationsCost
    FROM SpecOperation so INNER JOIN Operation op ON so.OperationId = op.Id
    WHERE so.SpecId = s.Id
) oc;
GO

-- Проверка работы VIEW
SELECT * FROM v_ClientOrderAnalysis ORDER BY ClientName, ProductName;
GO
