-- ==========================================================
-- THIẾT LẬP CRUD ĐỊA CHỈ GIAO HÀNG VÀ ĐỊA CHỈ MẶC ĐỊNH
-- Chạy sau Database/Setup_CloneEbayDB_Auth.sql
-- Script có thể chạy lại nhiều lần an toàn.
-- ==========================================================

USE CloneEbayDB;
GO

IF OBJECT_ID(N'[dbo].[User]', N'U') IS NULL
BEGIN
    THROW 50001, N'Bảng [User] chưa tồn tại. Hãy chạy Setup_CloneEbayDB_Auth.sql trước.', 1;
END
GO

IF OBJECT_ID(N'[dbo].[Address]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Address]
    (
        [id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Address] PRIMARY KEY,
        [userId] INT NOT NULL,
        [fullName] NVARCHAR(100) NOT NULL,
        [phone] NVARCHAR(20) NOT NULL,
        [street] NVARCHAR(200) NOT NULL,
        [city] NVARCHAR(100) NOT NULL,
        [state] NVARCHAR(100) NULL,
        [country] NVARCHAR(100) NOT NULL CONSTRAINT [DF_Address_Country] DEFAULT N'Việt Nam',
        [postalCode] NVARCHAR(20) NULL,
        [isDefault] BIT NOT NULL CONSTRAINT [DF_Address_IsDefault] DEFAULT 0,
        CONSTRAINT [FK_Address_User] FOREIGN KEY ([userId]) REFERENCES [dbo].[User]([id])
    );
END
ELSE
BEGIN
    IF COL_LENGTH(N'[dbo].[Address]', N'fullName') IS NULL
        ALTER TABLE [dbo].[Address] ADD [fullName] NVARCHAR(100) NULL;

    IF COL_LENGTH(N'[dbo].[Address]', N'phone') IS NULL
        ALTER TABLE [dbo].[Address] ADD [phone] NVARCHAR(20) NULL;

    IF COL_LENGTH(N'[dbo].[Address]', N'street') IS NULL
        ALTER TABLE [dbo].[Address] ADD [street] NVARCHAR(200) NULL;

    IF COL_LENGTH(N'[dbo].[Address]', N'city') IS NULL
        ALTER TABLE [dbo].[Address] ADD [city] NVARCHAR(100) NULL;

    IF COL_LENGTH(N'[dbo].[Address]', N'state') IS NULL
        ALTER TABLE [dbo].[Address] ADD [state] NVARCHAR(100) NULL;

    IF COL_LENGTH(N'[dbo].[Address]', N'country') IS NULL
        ALTER TABLE [dbo].[Address] ADD [country] NVARCHAR(100) NULL;

    IF COL_LENGTH(N'[dbo].[Address]', N'postalCode') IS NULL
        ALTER TABLE [dbo].[Address] ADD [postalCode] NVARCHAR(20) NULL;

    IF COL_LENGTH(N'[dbo].[Address]', N'isDefault') IS NULL
        ALTER TABLE [dbo].[Address] ADD [isDefault] BIT NULL;
END
GO

-- Đồng bộ độ dài cột với validation/API mới và giữ nguyên nullable của schema hiện tại.
IF EXISTS (SELECT 1 FROM sys.columns WHERE [object_id] = OBJECT_ID(N'[dbo].[Address]') AND [name] = N'street' AND [is_nullable] = 0)
    ALTER TABLE [dbo].[Address] ALTER COLUMN [street] NVARCHAR(200) NOT NULL;
ELSE
    ALTER TABLE [dbo].[Address] ALTER COLUMN [street] NVARCHAR(200) NULL;

IF EXISTS (SELECT 1 FROM sys.columns WHERE [object_id] = OBJECT_ID(N'[dbo].[Address]') AND [name] = N'city' AND [is_nullable] = 0)
    ALTER TABLE [dbo].[Address] ALTER COLUMN [city] NVARCHAR(100) NOT NULL;
ELSE
    ALTER TABLE [dbo].[Address] ALTER COLUMN [city] NVARCHAR(100) NULL;

IF EXISTS (SELECT 1 FROM sys.columns WHERE [object_id] = OBJECT_ID(N'[dbo].[Address]') AND [name] = N'state' AND [is_nullable] = 0)
    ALTER TABLE [dbo].[Address] ALTER COLUMN [state] NVARCHAR(100) NOT NULL;
ELSE
    ALTER TABLE [dbo].[Address] ALTER COLUMN [state] NVARCHAR(100) NULL;

IF EXISTS (SELECT 1 FROM sys.columns WHERE [object_id] = OBJECT_ID(N'[dbo].[Address]') AND [name] = N'country' AND [is_nullable] = 0)
    ALTER TABLE [dbo].[Address] ALTER COLUMN [country] NVARCHAR(100) NOT NULL;
ELSE
    ALTER TABLE [dbo].[Address] ALTER COLUMN [country] NVARCHAR(100) NULL;
GO

-- Chuẩn hóa dữ liệu cũ: mỗi user có đúng một địa chỉ mặc định nếu họ có địa chỉ.
UPDATE [dbo].[Address]
SET [isDefault] = 0
WHERE [isDefault] IS NULL;
GO

;WITH RankedAddresses AS
(
    SELECT
        [id],
        ROW_NUMBER() OVER
        (
            PARTITION BY [userId]
            ORDER BY CASE WHEN [isDefault] = 1 THEN 0 ELSE 1 END, [id]
        ) AS [rowNumber]
    FROM [dbo].[Address]
    WHERE [userId] IS NOT NULL
)
UPDATE addressTable
SET [isDefault] = CASE WHEN ranked.[rowNumber] = 1 THEN 1 ELSE 0 END
FROM [dbo].[Address] AS addressTable
INNER JOIN RankedAddresses AS ranked ON ranked.[id] = addressTable.[id];
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Address]')
      AND [name] = N'isDefault'
      AND [is_nullable] = 1
)
BEGIN
    ALTER TABLE [dbo].[Address] ALTER COLUMN [isDefault] BIT NOT NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.[object_id] = dc.[parent_object_id]
       AND c.[column_id] = dc.[parent_column_id]
    WHERE dc.[parent_object_id] = OBJECT_ID(N'[dbo].[Address]')
      AND c.[name] = N'isDefault'
)
BEGIN
    ALTER TABLE [dbo].[Address]
    ADD CONSTRAINT [DF_Address_IsDefault] DEFAULT 0 FOR [isDefault];
END
GO

-- SQL Server filtered unique index bảo đảm tối đa một địa chỉ mặc định/user.
IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Address]')
      AND [name] = N'UX_Address_User_Default'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Address_User_Default]
        ON [dbo].[Address]([userId])
        WHERE [isDefault] = 1 AND [userId] IS NOT NULL;
END
GO

PRINT N'Đã thiết lập bảng Address và ràng buộc địa chỉ mặc định.';
GO
