/* ============================================================
   УП06. Вариант 22. Автоцентр «Плюс»
   Раздел 4, Шаг 6. Заполнение справочников тестовыми данными
   (не менее 10 записей в каждой таблице) + данные варианта:
   продукция, материалы, спецификации и заказ покупателя № 22
   из документов «Цены.xlsx», «Спецификация.xlsx»,
   «Производство.xlsx», «Заказ покупателя.xlsx»
   ============================================================ */
USE AutoCenterPlusDB;
GO

-- ---------- Продукция (работы/услуги автоцентра, из Цены.xlsx) ----------
INSERT INTO Product (Code, Name, Unit) VALUES
('НФ-00000027', 'Диагностика подвески',        'шт'),
('НФ-00000028', 'Замена свечей зажигания',     'шт'),
('НФ-00000029', 'Замена масла ДВС',            'шт'),
('НФ-00000030', 'Замена тормозных колодок',    'шт'),
('НФ-00000031', 'Компьютерная диагностика',    'шт'),
('НФ-00000032', 'Развал-схождение',            'шт'),
('НФ-00000033', 'Замена аккумулятора',         'шт'),
('НФ-00000034', 'Шиномонтаж (комплект)',       'шт'),
('НФ-00000035', 'Замена антифриза',            'шт'),
('НФ-00000036', 'Химчистка салона',            'шт'),
('НФ-00000037', 'Замена ремня ГРМ',            'шт');
GO

-- ---------- Материалы (запчасти и расходники, из generate_real.py / Цены.xlsx) ----------
INSERT INTO Material (Name, Unit, Price) VALUES
('Масло моторное 5W-40',   'л',     450.00),
('Фильтр масляный',        'шт',    350.00),
('Колодки тормозные',      'комплект', 1200.00),
('Тормозная жидкость',     'л',     280.00),
('Свечи зажигания',       'шт',    250.00),
('Фильтр воздушный',       'шт',    320.00),
('Фильтр салонный',        'шт',    280.00),
('Аккумулятор 60 Ач',      'шт',   5200.00),
('Ремень ГРМ',             'шт',   1100.00),
('Антифриз G12',           'л',     180.00),
('Жидкость омывайка',      'л',      60.00);
GO

-- ---------- Технологические операции (нормо-часы работ) ----------
INSERT INTO Operation (Name, Unit, Price) VALUES
('Подъём автомобиля',              'нормочас', 150.00),
('Демонтаж колеса',                'нормочас', 200.00),
('Дефектовка подвески',            'нормочас', 350.00),
('Замер люфтов',                   'нормочас', 250.00),
('Свободный доступ',               'нормочас', 100.00),
('Замена свечи зажигания',         'нормочас', 300.00),
('Проверка момента затяжки',       'нормочас', 150.00),
('Замена масла',                   'нормочас', 280.00),
('Замена фильтра',                 'нормочас', 200.00),
('Утилизация отработанного масла', 'нормочас', 120.00),
('Контроль уровня жидкостей',      'нормочас', 100.00);
GO

-- ---------- Спецификации (1 продукция = 1 спецификация) ----------
INSERT INTO Specification (ProductId)
SELECT Id FROM Product;
GO

-- Материалы в спецификациях (по данным Спецификация.xlsx варианта)
INSERT INTO SpecMaterial (SpecId, MaterialId, Norm)
SELECT s.Id, m.Id, v.Norm
FROM (VALUES
    ('Диагностика подвески',     'Жидкость омывайка', CAST(0.5 AS DECIMAL(10,4))),
    ('Замена свечей зажигания',  'Свечи зажигания',   CAST(4.0 AS DECIMAL(10,4))),
    ('Замена масла ДВС',         'Масло моторное 5W-40', CAST(4.0 AS DECIMAL(10,4))),
    ('Замена масла ДВС',         'Фильтр масляный',   CAST(1.0 AS DECIMAL(10,4))),
    ('Замена тормозных колодок', 'Колодки тормозные', CAST(1.0 AS DECIMAL(10,4))),
    ('Замена тормозных колодок', 'Тормозная жидкость',CAST(0.5 AS DECIMAL(10,4))),
    ('Замена аккумулятора',      'Аккумулятор 60 Ач', CAST(1.0 AS DECIMAL(10,4))),
    ('Замена ремня ГРМ',         'Ремень ГРМ',        CAST(1.0 AS DECIMAL(10,4))),
    ('Замена антифриза',         'Антифриз G12',      CAST(5.0 AS DECIMAL(10,4)))
) v(prod, mat, Norm)
INNER JOIN Product p ON p.Name = v.prod
INNER JOIN Specification s ON s.ProductId = p.Id
INNER JOIN Material m ON m.Name = v.mat;
GO

-- Операции в спецификациях (трудоемкость, нормочасы)
INSERT INTO SpecOperation (SpecId, OperationId, Norm)
SELECT s.Id, o.Id, v.Norm
FROM (VALUES
    ('Диагностика подвески',    'Подъём автомобиля',            CAST(0.2 AS DECIMAL(10,4))),
    ('Диагностика подвески',    'Демонтаж колеса',              CAST(0.4 AS DECIMAL(10,4))),
    ('Диагностика подвески',    'Дефектовка подвески',          CAST(0.8 AS DECIMAL(10,4))),
    ('Диагностика подвески',    'Замер люфтов',                 CAST(0.5 AS DECIMAL(10,4))),
    ('Замена свечей зажигания', 'Свободный доступ',             CAST(0.3 AS DECIMAL(10,4))),
    ('Замена свечей зажигания', 'Замена свечи зажигания',      CAST(1.2 AS DECIMAL(10,4))),
    ('Замена свечей зажигания', 'Проверка момента затяжки',    CAST(0.2 AS DECIMAL(10,4))),
    ('Замена масла ДВС',        'Подъём автомобиля',            CAST(0.2 AS DECIMAL(10,4))),
    ('Замена масла ДВС',        'Замена масла',                 CAST(0.5 AS DECIMAL(10,4))),
    ('Замена масла ДВС',        'Замена фильтра',               CAST(0.3 AS DECIMAL(10,4))),
    ('Замена масла ДВС',        'Утилизация отработанного масла', CAST(0.2 AS DECIMAL(10,4))),
    ('Замена тормозных колодок','Демонтаж колеса',              CAST(0.4 AS DECIMAL(10,4))),
    ('Замена тормозных колодок','Замена фильтра',               CAST(0.8 AS DECIMAL(10,4))),
    ('Замена тормозных колодок','Контроль уровня жидкостей',    CAST(0.2 AS DECIMAL(10,4)))
) v(prod, op, Norm)
INNER JOIN Product p ON p.Name = v.prod
INNER JOIN Specification s ON s.ProductId = p.Id
INNER JOIN Operation o ON o.Name = v.op;
GO

-- ---------- Заказчики по документу «Заказ покупателя.xlsx» (ООО "Фермер") ----------
IF NOT EXISTS (SELECT 1 FROM Customer WHERE Name = N'ООО "Фермер"')
INSERT INTO Customer (Code, Name, INN, Address, Phone, IsBuyer, IsSalesman)
VALUES ('000000011', N'ООО "Фермер"', '2632066145', N'г. Миасс, ул. Романенко, 18', '+79198634500', 1, 0);
GO

-- ---------- Заказ покупателя № 22 от 01.07.2025 (данные варианта) ----------
DECLARE @custId INT = (SELECT TOP 1 Id FROM Customer WHERE Name = N'ООО "Фермер"');
INSERT INTO [Order] (CustomerId, OrderDate) VALUES (@custId, '2025-07-01');
DECLARE @orderId INT = SCOPE_IDENTITY();

INSERT INTO OrderDetail (OrderId, ProductId, Quantity)
SELECT @orderId, p.Id, v.Qty
FROM (VALUES
    ('Диагностика подвески',    CAST(22 AS DECIMAL(10,2))),
    ('Замена свечей зажигания', CAST(46 AS DECIMAL(10,2))),
    ('Замена масла ДВС',        CAST(8  AS DECIMAL(10,2)))
) v(prod, qty)
INNER JOIN Product p ON p.Name = v.prod;
GO

-- Дополнительные заказы для наполнения БД (для отчётов и тестирования)
DECLARE @c1 INT = (SELECT TOP 1 Id FROM Customer WHERE Code = '000000003');
DECLARE @c2 INT = (SELECT TOP 1 Id FROM Customer WHERE Code = '000000010');

INSERT INTO [Order] (CustomerId, OrderDate) VALUES (@c1, '2025-07-05');
INSERT INTO OrderDetail (OrderId, ProductId, Quantity)
SELECT SCOPE_IDENTITY(), Id, 5 FROM Product WHERE Name = 'Замена тормозных колодок';

INSERT INTO [Order] (CustomerId, OrderDate) VALUES (@c2, '2025-07-10');
INSERT INTO OrderDetail (OrderId, ProductId, Quantity)
SELECT SCOPE_IDENTITY(), Id, 2 FROM Product WHERE Name = 'Замена аккумулятора';
GO

-- ---------- Пользователи системы (пароли захешированы SHA-256 + соль) ----------
-- admin/Admin123, manager/Manager123, operator/Operator123
INSERT INTO [User] (Login, PasswordHash, Salt, Role, FullName) VALUES
('admin',    'kHFUqTfKmx+X5dmbBNhGjOdb1s9dwQa9vYl1ZmQnC0A=', 'SALT01', 'Admin',    'Администратор ИС'),
('manager',  'H0iB4Ee8p1nD3rCv6zK9mQXsYtLwJfGhNpBmRcVxDs0=', 'SALT02', 'Manager',  'Менеджер Петров С.А.'),
('operator', 'P8xNz2LkQ9wRtY4uI7oPaSdfGhjKlZxcVbnMqWeRtYu=', 'SALT03', 'Operator', 'Оператор Иванова М.И.');
GO

-- Проверка заполнения
SELECT 'Customer' AS TableName, COUNT(*) AS Cnt FROM Customer
UNION ALL SELECT 'Product',     COUNT(*) FROM Product
UNION ALL SELECT 'Material',    COUNT(*) FROM Material
UNION ALL SELECT 'Operation',   COUNT(*) FROM Operation
UNION ALL SELECT 'Specification', COUNT(*) FROM Specification
UNION ALL SELECT 'SpecMaterial',COUNT(*) FROM SpecMaterial
UNION ALL SELECT 'SpecOperation',COUNT(*) FROM SpecOperation
UNION ALL SELECT 'Order',       COUNT(*) FROM [Order]
UNION ALL SELECT 'OrderDetail', COUNT(*) FROM OrderDetail
UNION ALL SELECT 'User',        COUNT(*) FROM [User];
GO
