/* ============================================================
   УП06. Вариант 22. Автоцентр «Плюс»
   Раздел 6. Регламенты обновления и технического сопровождения ИС
   Файл 06 — обновление структуры БД (имитация обновления версии ИС)
   ============================================================ */
USE AutoCenterPlusDB;
GO

-- 1. Добавление нового поля в таблицу Customer
ALTER TABLE Customer
ADD Note NVARCHAR(500) NULL;
GO

-- 2. Изменение типа данных (увеличение длины поля) — уже учтено при создании
--    (Phone NVARCHAR(30)), демонстрируем на другом поле:
ALTER TABLE Product
ALTER COLUMN Name NVARCHAR(250) NOT NULL;
GO

-- 3. Добавление новой таблицы — история изменений статусов заказов
IF OBJECT_ID('OrderHistory') IS NOT NULL DROP TABLE OrderHistory;
GO
CREATE TABLE OrderHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ChangeDate DATETIME2 DEFAULT GETDATE(),
    OldStatus NVARCHAR(50),
    NewStatus NVARCHAR(50),
    ChangedBy INT,
    FOREIGN KEY (OrderId) REFERENCES [Order](Id) ON DELETE CASCADE
);
GO

-- 4. Добавление CHECK-ограничения
ALTER TABLE [Order]
ADD CONSTRAINT CHK_Order_TotalAmount CHECK (TotalAmount >= 0);
GO

-- 5. Поле Email добавлено в Customer (см. 02_CreateTables.sql)

-- Проверка сохранности данных и целостности связей после обновления
SELECT COUNT(*) AS CustomerCount FROM Customer;
SELECT COUNT(*) AS OrderCount FROM [Order];
SELECT c.Name, o.OrderDate, o.TotalAmount
FROM [Order] o INNER JOIN Customer c ON o.CustomerId = c.Id;
GO
