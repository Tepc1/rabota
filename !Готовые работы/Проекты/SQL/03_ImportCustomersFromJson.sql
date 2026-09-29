/* ============================================================
   УП06. Вариант 22. Автоцентр «Плюс»
   Раздел 4, Шаг 4. Импорт данных из файла Заказчики.json
   (документ предоставлен в папке Приложения для УП06\Модели)
   Требование SQL Server 2016+ (функция OPENJSON)
   ============================================================ */
USE AutoCenterPlusDB;
GO

-- Файл должен быть доступен на сервере, например C:\Data\Заказчики.json
DECLARE @json NVARCHAR(MAX);
SELECT @json = BulkColumn
FROM OPENROWSET(BULK 'C:\Data\Заказчики.json', SINGLE_CLOB, CODEPAGE = '65001') AS j;

INSERT INTO Customer (Code, Name, INN, Address, Phone, IsBuyer, IsSalesman)
SELECT
    Code,
    Name,
    NULLIF(INN, ''),          -- в исходном JSON у части заказчиков ИНН пустой
    Address,
    Phone,
    Buyer,
    Salesman
FROM OPENJSON(@json)
WITH (
    Code     VARCHAR(20)     '$.id',
    Name     NVARCHAR(200)   '$.name',
    INN      VARCHAR(12)     '$.inn',
    Address  NVARCHAR(500)   '$.addres',   -- в файле ключ написан "addres"
    Phone    NVARCHAR(30)    '$.phone',
    Buyer    BIT             '$.buyer',
    Salesman BIT             '$.salesman'
);
GO

-- Проверка результата импорта
SELECT Id, Code, Name, INN, Address, Phone, IsBuyer, IsSalesman
FROM Customer
ORDER BY Id;
GO
