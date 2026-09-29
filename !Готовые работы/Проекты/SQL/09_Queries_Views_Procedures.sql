/* ============================================================
   УП06. Вариант 22. Автоцентр «Плюс»
   Раздел 7. Идентификация технических проблем. SQL-запросы,
   VIEW, хранимые процедуры (подготовка к ДЭ, Задание 3)
   ============================================================ */
USE AutoCenterPlusDB;
GO

/* ------------------------------------------------------------
   ШАГ 2. Запрос расчёта ПОЛНОЙ СТОИМОСТИ заказа покупателя
   с учётом: количества продукции в заказе и стоимости всех
   материалов и технологических операций с нормой расхода
   ------------------------------------------------------------ */
DECLARE @OrderId INT = 1;

SELECT
    o.Id AS OrderId,
    c.Name AS CustomerName,
    o.OrderDate,
    p.Code AS ProductCode,
    p.Name AS ProductName,
    od.Quantity,
    p.Unit,

    -- Стоимость материалов на 1 ед. продукции: SUM(Цена * Норма расхода)
    ISNULL((SELECT SUM(m.Price * sm.Norm)
            FROM SpecMaterial sm
            INNER JOIN Material m ON sm.MaterialId = m.Id
            WHERE sm.SpecId = s.Id), 0) AS MaterialsCostPerUnit,

    -- Стоимость операций на 1 ед. продукции: SUM(Цена * Норма)
    ISNULL((SELECT SUM(op.Price * so.Norm)
            FROM SpecOperation so
            INNER JOIN Operation op ON so.OperationId = op.Id
            WHERE so.SpecId = s.Id), 0) AS OperationsCostPerUnit,

    -- Итоговая стоимость позиции = Цена за ед. * Количество в заказе
    od.Quantity * (
        ISNULL((SELECT SUM(m.Price * sm.Norm)
                FROM SpecMaterial sm
                INNER JOIN Material m ON sm.MaterialId = m.Id
                WHERE sm.SpecId = s.Id), 0) +
        ISNULL((SELECT SUM(op.Price * so.Norm)
                FROM SpecOperation so
                INNER JOIN Operation op ON so.OperationId = op.Id
                WHERE so.SpecId = s.Id), 0)
    ) AS TotalPositionCost
FROM [Order] o
INNER JOIN Customer c ON o.CustomerId = c.Id
INNER JOIN OrderDetail od ON o.Id = od.OrderId
INNER JOIN Product p ON od.ProductId = p.Id
INNER JOIN Specification s ON p.Id = s.ProductId
WHERE o.Id = @OrderId;
GO

/* ------------------------------------------------------------
   ШАГ 3. VIEW для часто используемой выборки — полная стоимость
   всех заказов (используется приложением и API)
   ------------------------------------------------------------ */
IF OBJECT_ID('v_OrderFullCost') IS NOT NULL DROP VIEW v_OrderFullCost;
GO
CREATE VIEW v_OrderFullCost
AS
SELECT
    o.Id AS OrderId,
    c.Id AS CustomerId,
    c.Name AS CustomerName,
    c.INN AS CustomerINN,
    o.OrderDate,
    p.Id AS ProductId,
    p.Code AS ProductCode,
    p.Name AS ProductName,
    p.Unit,
    od.Quantity,
    ISNULL(mc.MaterialsCost, 0) AS MaterialsCostPerUnit,
    ISNULL(oc.OperationsCost, 0) AS OperationsCostPerUnit,
    od.Quantity * (ISNULL(mc.MaterialsCost, 0) + ISNULL(oc.OperationsCost, 0))
        AS TotalPositionCost
FROM [Order] o
INNER JOIN Customer c ON o.CustomerId = c.Id
INNER JOIN OrderDetail od ON o.Id = od.OrderId
INNER JOIN Product p ON od.ProductId = p.Id
INNER JOIN Specification s ON p.Id = s.ProductId
-- Оптимизация: скалярные подзапросы заменены агрегирующими JOIN
-- (устранены повторяющиеся подзапросы, запрос становится set-based)
OUTER APPLY (
    SELECT SUM(m.Price * sm.Norm) AS MaterialsCost
    FROM SpecMaterial sm
    INNER JOIN Material m ON sm.MaterialId = m.Id
    WHERE sm.SpecId = s.Id
) mc
OUTER APPLY (
    SELECT SUM(op.Price * so.Norm) AS OperationsCost
    FROM SpecOperation so
    INNER JOIN Operation op ON so.OperationId = op.Id
    WHERE so.SpecId = s.Id
) oc;
GO

-- Использование VIEW
SELECT * FROM v_OrderFullCost WHERE OrderId = 1;
GO

/* ------------------------------------------------------------
   ШАГ 4. Хранимая процедура расчёта стоимости заказа
   ------------------------------------------------------------ */
IF OBJECT_ID('sp_CalculateOrderCost') IS NOT NULL DROP PROCEDURE sp_CalculateOrderCost;
GO
CREATE PROCEDURE sp_CalculateOrderCost
    @OrderId INT,
    @TotalCost DECIMAL(14,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @TotalCost = SUM(TotalPositionCost)
    FROM v_OrderFullCost
    WHERE OrderId = @OrderId;

    IF @TotalCost IS NULL
        SET @TotalCost = 0;
END;
GO

-- Вызов процедуры
DECLARE @Result DECIMAL(14,2);
EXEC sp_CalculateOrderCost @OrderId = 1, @TotalCost = @Result OUTPUT;
PRINT 'Полная стоимость заказа: ' + CAST(@Result AS VARCHAR(20));
GO

/* ------------------------------------------------------------
   Дополнительно: процедура обновления итоговой суммы заказа
   ------------------------------------------------------------ */
IF OBJECT_ID('sp_UpdateOrderTotal') IS NOT NULL DROP PROCEDURE sp_UpdateOrderTotal;
GO
CREATE PROCEDURE sp_UpdateOrderTotal
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE o
    SET o.TotalAmount = ISNULL(t.Summ, 0)
    FROM [Order] o
    LEFT JOIN (
        SELECT OrderId, SUM(TotalPositionCost) AS Summ
        FROM v_OrderFullCost
        GROUP BY OrderId
    ) t ON t.OrderId = o.Id
    WHERE o.Id = @OrderId;
END;
GO

EXEC sp_UpdateOrderTotal @OrderId = 1;
SELECT Id, TotalAmount FROM [Order];
GO

/* ------------------------------------------------------------
   ШАГ 5. Анализ плана выполнения и оптимизация
   В SSMS: Query → Include Actual Execution Plan (Ctrl+M)
   - До оптимизации: скалярные подзапросы в SELECT выполнялись
     для каждой строки (Risk of Table Scan).
   - После: OUTER APPLY + индексы IX_OrderDetail_OrderProduct,
     IX_Product_Name → Index Seek по SpecId/ProductId.
   Сравнение стоимости:
------------------------------------------------------------- */
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
SELECT * FROM v_OrderFullCost WHERE OrderId = 1;
SET STATISTICS TIME OFF;
SET STATISTICS IO OFF;
GO

-- Обновление статистики после изменений
UPDATE STATISTICS OrderDetail;
UPDATE STATISTICS SpecMaterial;
UPDATE STATISTICS SpecOperation;
GO
