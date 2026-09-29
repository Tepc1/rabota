/* ============================================================
   УП06. ПМ.06 «Сопровождение информационных систем»
   Вариант 22. Тема: Автоцентр «Плюс»
   Раздел 4. Проектирование и создание БД в MS SQL Server
   Файл 01 — создание базы данных AutoCenterPlusDB
   ============================================================ */

-- Создание базы данных AutoCenterPlusDB
CREATE DATABASE AutoCenterPlusDB
ON PRIMARY
(
    NAME = AutoCenterPlusDB_Data,
    FILENAME = 'C:\SQLData\AutoCenterPlusDB.mdf',
    SIZE = 50MB,
    MAXSIZE = 500MB,
    FILEGROWTH = 10MB
)
LOG ON
(
    NAME = AutoCenterPlusDB_Log,
    FILENAME = 'C:\SQLData\AutoCenterPlusDB_log.ldf',
    SIZE = 20MB,
    MAXSIZE = 200MB,
    FILEGROWTH = 5MB
);
GO

USE AutoCenterPlusDB;
GO
