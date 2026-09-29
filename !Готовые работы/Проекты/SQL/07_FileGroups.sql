/* ============================================================
   УП06. Вариант 22. Автоцентр «Плюс»
   Раздел 6, Шаг 2. Организация файловой структуры БД
   (файловые группы mdf/ldf/ndf)
   ============================================================ */
USE master;
GO

-- Создание файловой группы для индексов
ALTER DATABASE AutoCenterPlusDB
ADD FILEGROUP FG_Indexes;
GO

-- Добавление вторичного файла в файловую группу
ALTER DATABASE AutoCenterPlusDB
ADD FILE
(
    NAME = AutoCenterPlusDB_Indexes,
    FILENAME = 'C:\SQLData\AutoCenterPlusDB_Indexes.ndf',
    SIZE = 20MB,
    MAXSIZE = 200MB,
    FILEGROWTH = 10MB
) TO FILEGROUP FG_Indexes;
GO

-- Перенос индекса ИНН в новую файловую группу
CREATE UNIQUE INDEX IX_Customer_INN
ON AutoCenterPlusDB.dbo.Customer(INN)
WITH (DROP_EXISTING = ON)
ON FG_Indexes;
GO

-- Проверка файловой структуры
USE AutoCenterPlusDB;
GO
SELECT name, type_desc, physical_name, size/128 AS SizeMB
FROM sys.database_files;
GO
