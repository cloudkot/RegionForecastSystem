USE master;
GO

-- Удаляем базу, если она уже есть, чтобы начать с чистого листа
IF DB_ID('RegionForecastDB') IS NOT NULL
    DROP DATABASE RegionForecastDB;
GO

CREATE DATABASE RegionForecastDB;
GO

USE RegionForecastDB;
GO

-- 1. Таблица пользователей
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(50) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('User', 'Manager', 'Admin'))
);
GO

-- 2. Регионы
CREATE TABLE Regions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    FederalDistrict NVARCHAR(100) NOT NULL
);
GO

-- 3. Районы (для 3НФ)
CREATE TABLE Districts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RegionId INT NOT NULL FOREIGN KEY REFERENCES Regions(Id),
    Name NVARCHAR(100) NOT NULL
);
GO

-- 4. Отрасли экономики
CREATE TABLE EconomicSectors (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);
GO

-- 5. Предприятия
CREATE TABLE Enterprises (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DistrictId INT NOT NULL FOREIGN KEY REFERENCES Districts(Id),
    SectorId INT NOT NULL FOREIGN KEY REFERENCES EconomicSectors(Id),
    Name NVARCHAR(200) NOT NULL
);
GO

-- 6. Демография
CREATE TABLE Demographics (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RegionId INT NOT NULL FOREIGN KEY REFERENCES Regions(Id),
    Year INT NOT NULL,
    Population BIGINT NOT NULL,
    BirthRate DECIMAL(5,2) NOT NULL,
    DeathRate DECIMAL(5,2) NOT NULL
);
GO

-- 7. Рынок труда
CREATE TABLE LaborMarket (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RegionId INT NOT NULL FOREIGN KEY REFERENCES Regions(Id),
    Year INT NOT NULL,
    UnemploymentRate DECIMAL(5,2) NOT NULL,
    AverageSalary DECIMAL(12,2) NOT NULL
);
GO

-- 8. Макроэкономические показатели (ВРП, инфляция)
CREATE TABLE MacroIndicators (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RegionId INT NOT NULL FOREIGN KEY REFERENCES Regions(Id),
    Year INT NOT NULL,
    GRP DECIMAL(18,2) NOT NULL,
    InflationRate DECIMAL(5,2) NOT NULL
);
GO

-- 9. Доходы бюджета
CREATE TABLE BudgetRevenues (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RegionId INT NOT NULL FOREIGN KEY REFERENCES Regions(Id),
    Year INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Source NVARCHAR(100) NOT NULL
);
GO

-- 10. Расходы бюджета
CREATE TABLE BudgetExpenses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RegionId INT NOT NULL FOREIGN KEY REFERENCES Regions(Id),
    Year INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Category NVARCHAR(100) NOT NULL
);
GO

-- 11. Инвесторы
CREATE TABLE Investors (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Type NVARCHAR(50) NOT NULL
);
GO

-- 12. Инвестиции
CREATE TABLE Investments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RegionId INT NOT NULL FOREIGN KEY REFERENCES Regions(Id),
    InvestorId INT NOT NULL FOREIGN KEY REFERENCES Investors(Id),
    Year INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL
);
GO

-- 13. Модели прогнозирования
CREATE TABLE ForecastModels (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(300) NULL
);
GO

-- 14. Прогнозы
CREATE TABLE Forecasts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RegionId INT NOT NULL FOREIGN KEY REFERENCES Regions(Id),
    ModelId INT NOT NULL FOREIGN KEY REFERENCES ForecastModels(Id),
    Year INT NOT NULL,
    PredictedGRP DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- ==========================================
-- ЗАПОЛНЕНИЕ ДАННЫМИ (SEED DATA)
-- ==========================================

-- Пользователи (пароли пока в открытом виде для лабы)
INSERT INTO Users (Username, Password, Role) VALUES
('admin', 'admin123', 'Admin'),
('manager', 'manager123', 'Manager'),
('user', 'user123', 'User');
GO

-- Регионы
INSERT INTO Regions (Name, FederalDistrict) VALUES
(N'Владимирская область', N'Центральный'),
(N'Московская область', N'Центральный'),
(N'Ленинградская область', N'Северо-Западный');
GO

-- Районы
INSERT INTO Districts (RegionId, Name) VALUES
(1, N'Владимир'),
(1, N'Ковров'),
(2, N'Одинцово'),
(3, N'Всеволожск');
GO

-- Отрасли
INSERT INTO EconomicSectors (Name) VALUES
(N'Машиностроение'),
(N'Сельское хозяйство'),
(N'IT и связь'),
(N'Торговля');
GO

-- Предприятия
INSERT INTO Enterprises (DistrictId, SectorId, Name) VALUES
(1, 1, N'Владимирский тракторный завод'),
(2, 1, N'Ковровский механический завод'),
(3, 3, N'ООО Софт'),
(4, 4, N'ООО ТоргСервис');
GO

-- Демография
INSERT INTO Demographics (RegionId, Year, Population, BirthRate, DeathRate) VALUES
(1, 2022, 1320000, 8.5, 15.1),
(1, 2023, 1309942, 8.4, 15.0),
(2, 2023, 7750000, 11.0, 13.2),
(3, 2023, 1880000, 9.2, 14.3);
GO

-- Рынок труда
INSERT INTO LaborMarket (RegionId, Year, UnemploymentRate, AverageSalary) VALUES
(1, 2022, 3.5, 45000),
(1, 2023, 3.1, 52000),
(2, 2023, 3.2, 72000),
(3, 2023, 3.9, 63000);
GO

-- Макроэкономика (ВРП) - данные для графика и ИИ
INSERT INTO MacroIndicators (RegionId, Year, GRP, InflationRate) VALUES
(1, 2022, 850000000000.00, 12.5),
(1, 2023, 893400000000.00, 7.4),
(1, 2024, 943000000000.00, 6.1),
(2, 2023, 1300000000000.00, 7.4),
(3, 2023, 860000000000.00, 7.8);
GO

-- Доходы бюджета
INSERT INTO BudgetRevenues (RegionId, Year, Amount, Source) VALUES
(1, 2024, 90400000000.00, N'Налоговые доходы'),
(1, 2024, 26200000000.00, N'Трансферты'),
(2, 2023, 800000000000.00, N'Налоги'),
(3, 2023, 450000000000.00, N'Налоги');
GO

-- Расходы бюджета
INSERT INTO BudgetExpenses (RegionId, Year, Amount, Category) VALUES
(1, 2024, 40000000000.00, N'Образование'),
(1, 2024, 35000000000.00, N'Здравоохранение'),
(2, 2023, 300000000000.00, N'Образование'),
(3, 2023, 200000000000.00, N'Инфраструктура');
GO

-- Инвесторы
INSERT INTO Investors (Name, Type) VALUES
(N'Государственный фонд развития', N'Государственный'),
(N'Частная инвестиционная компания', N'Частный');
GO

-- Инвестиции
INSERT INTO Investments (RegionId, InvestorId, Year, Amount) VALUES
(1, 1, 2024, 143200000000.00),
(1, 2, 2024, 50000000000.00),
(2, 1, 2023, 500000000000.00),
(3, 2, 2023, 200000000000.00);
GO

-- Модели прогнозирования
INSERT INTO ForecastModels (Name, Description) VALUES
(N'Линейная регрессия', N'Прогноз на основе среднего темпа роста'),
(N'Экспоненциальное сглаживание', N'Прогноз с учетом сезонности');
GO

-- Прогнозы
INSERT INTO Forecasts (RegionId, ModelId, Year, PredictedGRP) VALUES
(1, 1, 2025, 995000000000.00),
(2, 1, 2025, 1500000000000.00),
(3, 1, 2025, 920000000000.00);
GO
