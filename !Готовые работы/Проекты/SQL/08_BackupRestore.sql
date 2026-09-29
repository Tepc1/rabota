/* ============================================================
   УП06. Вариант 22. Автоцентр «Плюс»
   Раздел 6, Шаги 4–5. Резервное копирование и тестовое восстановление
   Перед выполнением создать папку C:\SQLBackups\
   ============================================================ */
USE master;
GO

-- 1. Полное резервное копирование (FULL) — еженедельно, воскресенье 02:00
BACKUP DATABASE AutoCenterPlusDB
TO DISK = 'C:\SQLBackups\AutoCenterPlusDB_FULL.bak'
WITH FORMAT, INIT, NAME = 'AutoCenterPlusDB Full Backup',
     STATS = 10, COMPRESSION;
GO

-- 2. Разностное резервное копирование (DIFFERENTIAL) — ежедневно, кроме воскресенья
BACKUP DATABASE AutoCenterPlusDB
TO DISK = 'C:\SQLBackups\AutoCenterPlusDB_DIFF.bak'
WITH DIFFERENTIAL, FORMAT, INIT, NAME = 'AutoCenterPlusDB Differential Backup',
     STATS = 10, COMPRESSION;
GO

-- 3. Резервное копирование журнала транзакций (TRANSACTION LOG) — каждые 2 часа
BACKUP LOG AutoCenterPlusDB
TO DISK = 'C:\SQLBackups\AutoCenterPlusDB_LOG.trn'
WITH FORMAT, INIT, NAME = 'AutoCenterPlusDB Log Backup',
     STATS = 10, COMPRESSION;
GO

-- 4. Проверка целостности backup-файла
RESTORE VERIFYONLY
FROM DISK = 'C:\SQLBackups\AutoCenterPlusDB_FULL.bak';
GO

-- 5. Тестовое восстановление БД в новую базу AutoCenterPlusDB_Test
RESTORE DATABASE AutoCenterPlusDB_Test
FROM DISK = 'C:\SQLBackups\AutoCenterPlusDB_FULL.bak'
WITH
    MOVE 'AutoCenterPlusDB_Data' TO 'C:\SQLData\AutoCenterPlusDB_Test.mdf',
    MOVE 'AutoCenterPlusDB_Log'  TO 'C:\SQLData\AutoCenterPlusDB_Test_log.ldf',
    RECOVERY, REPLACE, STATS = 10;
GO

-- 6. Проверка восстановленной БД
USE AutoCenterPlusDB_Test;
GO
SELECT COUNT(*) AS CustomerCount FROM Customer;
SELECT COUNT(*) AS OrderCount FROM [Order];
SELECT COUNT(*) AS ProductCount FROM Product;
GO

-- Цепочка восстановления на момент времени (PITR):
-- RESTORE DATABASE ... WITH NORECOVERY (FULL)
-- RESTORE DATABASE ... WITH DIFFERENTIAL, NORECOVERY (DIFF)
-- RESTORE LOG ... WITH RECOVERY, STOPAT = '2026-10-12T14:30:00' (LOG)
