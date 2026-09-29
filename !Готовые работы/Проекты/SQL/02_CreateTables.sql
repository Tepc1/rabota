/* ============================================================
   УП06. Вариант 22. Автоцентр «Плюс»
   Раздел 4. Создание таблиц с ограничениями
   (PK, FK, UNIQUE, CHECK, DEFAULT), 3НФ, единая схема именования
   (таблицы — единственное число, PascalCase, PK = Id, FK = <Таблица>Id)
   ============================================================ */
USE AutoCenterPlusDB;
GO

-- Таблица заказчиков (клиентов автоцентра)
IF OBJECT_ID('Customer') IS NOT NULL DROP TABLE Customer;
GO
CREATE TABLE Customer (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code VARCHAR(20) NULL,                    -- код контрагента из справочника заказчика
    Name NVARCHAR(200) NOT NULL,              -- наименование
    INN VARCHAR(12) NOT NULL UNIQUE,          -- бизнес-правило: уникальный ИНН
    Address NVARCHAR(500),
    Phone NVARCHAR(30),
    Email NVARCHAR(100) NULL,
    IsBuyer BIT NOT NULL DEFAULT 0,           -- признак «покупатель» (из Заказчики.json)
    IsSalesman BIT NOT NULL DEFAULT 0,        -- признак «поставщик» (из Заказчики.json)
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
GO

-- Справочник продукции (работ и услуг автоцентра)
IF OBJECT_ID('Product') IS NOT NULL DROP TABLE Product;
GO
CREATE TABLE Product (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code VARCHAR(20) NULL UNIQUE,             -- код номенклатуры (НФ-00000027 и т.п.)
    Name NVARCHAR(200) NOT NULL UNIQUE,
    Unit NVARCHAR(20) NOT NULL CHECK (Unit IN ('шт', 'кг', 'л', 'м', 'т', 'мин', 'ч', 'нормочас'))
);
GO

-- Справочник материалов (запчасти, расходники)
IF OBJECT_ID('Material') IS NOT NULL DROP TABLE Material;
GO
CREATE TABLE Material (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Unit NVARCHAR(20) NOT NULL,
    Price DECIMAL(12,2) NOT NULL CHECK (Price >= 0)
);
GO

-- Справочник технологических операций (нормо-часы работ)
IF OBJECT_ID('Operation') IS NOT NULL DROP TABLE Operation;
GO
CREATE TABLE Operation (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Unit NVARCHAR(20) NOT NULL,
    Price DECIMAL(12,2) NOT NULL CHECK (Price >= 0)
);
GO

-- Спецификация продукции: 1 продукция = 1 спецификация (1:1)
IF OBJECT_ID('SpecOperation') IS NOT NULL DROP TABLE SpecOperation;
IF OBJECT_ID('SpecMaterial') IS NOT NULL DROP TABLE SpecMaterial;
IF OBJECT_ID('Specification') IS NOT NULL DROP TABLE Specification;
GO
CREATE TABLE Specification (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL UNIQUE,            -- 1:1 с Product
    FOREIGN KEY (ProductId) REFERENCES Product(Id) ON DELETE CASCADE
);
GO

-- Материалы в спецификации (M:N через ассоциативную таблицу)
CREATE TABLE SpecMaterial (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SpecId INT NOT NULL,
    MaterialId INT NOT NULL,
    Norm DECIMAL(10,4) NOT NULL CHECK (Norm > 0),     -- норма расхода > 0
    FOREIGN KEY (SpecId) REFERENCES Specification(Id) ON DELETE CASCADE,
    FOREIGN KEY (MaterialId) REFERENCES Material(Id),
    UNIQUE (SpecId, MaterialId)
);
GO

-- Операции в спецификации
CREATE TABLE SpecOperation (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SpecId INT NOT NULL,
    OperationId INT NOT NULL,
    Norm DECIMAL(10,4) NOT NULL CHECK (Norm > 0),     -- норма (трудоемкость) > 0
    FOREIGN KEY (SpecId) REFERENCES Specification(Id) ON DELETE CASCADE,
    FOREIGN KEY (OperationId) REFERENCES Operation(Id),
    UNIQUE (SpecId, OperationId)
);
GO

-- Заказы покупателей (M:1 к Customer)
IF OBJECT_ID('OrderDetail') IS NOT NULL DROP TABLE OrderDetail;
IF OBJECT_ID('[Order]') IS NOT NULL DROP TABLE [Order];
GO
CREATE TABLE [Order] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    OrderDate DATE NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(14,2) NOT NULL DEFAULT 0,
    FOREIGN KEY (CustomerId) REFERENCES Customer(Id)
);
GO

-- Позиции заказа (M:N: Order <-> Product через OrderDetail)
CREATE TABLE OrderDetail (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity DECIMAL(10,2) NOT NULL CHECK (Quantity > 0),
    FOREIGN KEY (OrderId) REFERENCES [Order](Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Product(Id),
    UNIQUE (OrderId, ProductId)
);
GO

-- Пользователи системы (для аутентификации, Разделы 8, 10)
IF OBJECT_ID('[User]') IS NOT NULL DROP TABLE [User];
GO
CREATE TABLE [User] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Login NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,      -- только хеш (SHA-256 + соль)
    Salt NVARCHAR(64) NOT NULL,
    Role NVARCHAR(20) NOT NULL
        CHECK (Role IN ('Admin', 'Manager', 'Operator')),
    FullName NVARCHAR(200)
);
GO
