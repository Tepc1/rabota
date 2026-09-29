# Раздел 8. Настройка параметров ИС. Модули на C#

**Тема:** Автоцентр «Плюс»
Решение: `Проекты/AutoCenterPlus.IS/AutoCenterPlus.IS.sln` (Visual Studio 2022, .NET 8)

## 8.1. Состав решения

| Проект | Тип | Назначение |
|---|---|---|
| AutoCenterPlus.Client | WinForms | Клиентское приложение ИС |
| AutoCenterPlus.Api | ASP.NET Core Web API | REST-сервис для доступа к данным |

## 8.2. Архитектура клиентского приложения

| Слой | Классы | Ответственность |
|---|---|---|
| Data | DbConnection | Фабрика SqlConnection, проверка доступности БД |
| Services | AuthService | Аутентификация, хеширование SHA-256 + соль |
| Forms | LoginForm, MainForm, CustomerForm, OrderForm, ClientOrderReportForm | Интерфейс пользователя |
| Utils | ConfigManager, Logger | Параметры ИС, логирование |
| Models | Entities | POCO-сущности |

Точка входа: `Program.Main` → загрузка ConfigManager → проверка БД (TestConnection) → LoginForm → MainForm с учётом роли (RBAC).

## 8.3. Параметры ИС (appsettings.json)

Настраиваемые параметры (Задание: настройка параметров ИС):

| Параметр | Описание | Значение по умолчанию |
|---|---|---|
| ConnectionStrings:Default | Строка подключения к AutoCenterPlusDB | Server=(localdb)\mssqllocaldb; Database=AutoCenterPlusDB; Trusted_Connection=True |
| Logging:Level | Уровень логирования (Debug/Info/Warning/Error) | Info |
| Logging:Path | Файл журнала | logs/app.log |
| Ui:Language | Язык интерфейса | ru |
| Ui:Theme | Тема оформления | Light |

При отсутствии файла используются безопасные значения по умолчанию.

## 8.4. Идентификация и аутентификация (Шаг 3)

LoginForm: валидация пустых полей, вызов AuthService.Authenticate. Пароли в БД хранятся ТОЛЬКО как SHA-256(соль + пароль) в Base64. Успешный/неудачный вход фиксируется в журнале (Logger).

## 8.5. Разграничение прав (RBAC)

Роли: Admin, Manager, Operator (CHECK-ограничение в таблице User).

- MainForm скрывает пункты меню по роли: отчёт «АнализЗаказовКлиентов» — только Manager/Admin; «Пользователи» — только Admin.
- ClientOrderReportForm дополнительно проверяет роль ДО открытия формы (двойная защита).

## 8.6. Модуль справочника клиентов (CustomerForm)

CRUD-операции с обработкой исключений БД:

- SqlException 2627/2601 — нарушение UNIQUE(INN) → понятное предупреждение;
- SqlException 547 — нарушение ссылочной целостности при удалении клиента с заказами;
- все запросы параметризованы (защита от SQL-инъекций).

## 8.7. Модуль расчёта стоимости заказа (OrderForm)

Исправленный модуль (см. Раздел 5): валидация TryParse, using, параметризованный запрос к v_OrderFullCost, защита от деления на ноль, вывод в денежном формате.

## 8.8. REST API (AutoCenterPlus.Api)

| Метод | Endpoint | Назначение |
|---|---|---|
| GET | /api/customers?name=&inn= | Список клиентов с агрегатами (сумма и число заказов), валидация ИНН (10 или 12 цифр) |
| GET | /api/customers/{id} | Карточка клиента (404 при отсутствии) |
| GET | /api/orders/{id}/cost | Полная стоимость заказа через sp_CalculateOrderCost |
| GET | /api/orders/analysis?client= | Данные отчёта «АнализЗаказовКлиентов» + итоги с % скидки |

Swagger включён во всех средах для демонстрации и автотестов Postman.

## 8.9. Логирование

Logger — файловый, потокобезопасный (lock), уровень настраивается в appsettings.json. Записи: метка времени, уровень, сообщение. Ошибки логирования не приводят к падению приложения.

## 8.10. Выводы по разделу

Реализована двухзвенная ИС: WinForms-клиент с ролевой моделью и REST API. Настройка параметров вынесена в appsettings.json (строка подключения, логирование, язык, тема). Аутентификация построена на хешах SHA-256 с солью, все запросы к БД параметризованы.