/* ============================================================
   УП06. Вариант 22. Автоцентр «Плюс»
   Раздел 4, Шаг 5. Индексы для часто используемых полей поиска
   ============================================================ */
USE AutoCenterPlusDB;
GO

-- Индекс по дате заказа (частый фильтр в отчётах)
CREATE INDEX IX_Order_OrderDate ON [Order](OrderDate);
GO

-- Индекс по ИНН заказчика (поиск по реквизиту)
CREATE INDEX IX_Customer_INN ON Customer(INN);
GO

-- Индекс по наименованию продукции
CREATE INDEX IX_Product_Name ON Product(Name);
GO

-- Составной индекс для позиций заказа
CREATE INDEX IX_OrderDetail_OrderProduct ON OrderDetail(OrderId, ProductId);
GO

-- Индекс по коду контрагента
CREATE INDEX IX_Customer_Code ON Customer(Code);
GO
