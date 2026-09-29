# Раздел 4. Создание базы данных в MS SQL Server

**Тема:** Автоцентр «Плюс». БД: **AutoCenterPlusDB**
Скрипты: `Проекты/SQL/01…05`

## 4.1. Создание базы данных

Файл `01_CreateDatabase.sql` — создание БД с явным размещением файлов данных и журнала:

- `AutoCenterPlusDB_Data` (mdf): 50 МБ, рост по 10 МБ, максимум 500 МБ;
- `AutoCenterPlusDB_Log` (ldf): 20 МБ, рост по 5 МБ, максимум 200 МБ.

```sql
CREATE DATABASE AutoCenterPlusDB
ON PRIMARY
(
    NAME = AutoCenterPlusDB_Data,
    FILENAME = 'C:\SQLData\AutoCenterPlusDB.mdf',
    SIZE = 50MB, MAXSIZE = 500MB, FILEGROWTH = 10MB
)
LOG ON
(
    NAME = AutoCenterPlusDB_Log,
    FILENAME = 'C:\SQLData\AutoCenterPlusDB_log.ldf',
    SIZE = 20MB, MAXSIZE = 200MB, FILEGROWTH = 5MB
);
```

## 4.2. Создание таблиц с ограничениями

Файл `02_CreateTables.sql` — 10 таблиц в 3НФ (см. Раздел 3). Ключевые ограничения:

| Таблица | Ограничения |
|---|---|
| Customer | PK, UNIQUE(INN) — бизнес-правило уникальности ИНН |
| Product | PK, UNIQUE(Code), UNIQUE(Name), CHECK(Unit IN ('шт','кг','л','м','т','мин','ч','нормочас')) |
| Material / Operation | PK, CHECK(Price >= 0) |
| Specification | PK, UNIQUE(ProductId) — связь 1:1, FK ON DELETE CASCADE |
| SpecMaterial / SpecOperation | PK, FK, UNIQUE(SpecId, MaterialId/OperationId), CHECK(Norm > 0) |
| Order / OrderDetail | PK, FK, CHECK(Quantity > 0), ON DELETE CASCADE для позиций |
| User | PK, UNIQUE(Login), CHECK(Role IN ('Admin','Manager','Operator')) |

## 4.3. Импорт данных из Заказчики.json

Файл `03_ImportCustomersFromJson.sql` — импорт 6 контрагентов через OPENROWSET + OPENJSON (SQL Server 2016+). Особенности исходного файла учтены: пустой ИНН обрабатывается через `NULLIF`, ключ адреса в JSON — `"addres"` (опечатка в исходнике).

## 4.4. Индексы

Файл `04_Indexes.sql` — индексы для часто используемых полей поиска:

- IX_Order_OrderDate — фильтр по дате в отчётах;
- IX_Customer_INN — поиск по реквизиту;
- IX_Product_Name — поиск по наименованию;
- IX_OrderDetail_OrderProduct (OrderId, ProductId) — составной для позиций;
- IX_Customer_Code — поиск по коду контрагента.

## 4.5. Наполнение тестовыми данными

Файл `05_SeedData.sql` — не менее 10 записей в каждой таблице:

- 11 позиций продукции (Замена масла ДВС, Замена тормозных колодок, Замена свечей зажигания, Диагностика подвески и др.);
- 11 материалов с ценами (масло 5W-40 — 450 руб., фильтр — 350 руб. и т.д.);
- 11 технологических операций в нормо-часах;
- спецификации для всей продукции, нормы расхода материалов и трудоёмкости по данным варианта;
- заказчики из документов варианта (в т.ч. ООО «Фермер»), заказ покупателя от 01.07.2025 с тремя позициями;
- 3 пользователя системы (Admin/Manager/Operator, пароли — SHA-256 + соль).

## 4.6. Выводы по разделу

БД AutoCenterPlusDB создана в соответствии с ER-моделью (Раздел 3): 10 таблиц в 3НФ, полный набор ограничений целостности (PK, FK, UNIQUE, CHECK, DEFAULT), индексы под поисковые запросы, данные варианта импортированы. Проверочный запрос подтверждает ≥10 записей в каждой таблице.