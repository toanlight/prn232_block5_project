-- ==========================================================
-- SQL SCRIPT: THIẾT LẬP VÀ CẬP NHẬT BẢNG USER CHO AUTH VÀ PROFILE
-- Database: CloneEbayDB
-- ==========================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CloneEbayDB')
BEGIN
    CREATE DATABASE CloneEbayDB;
END
GO

USE CloneEbayDB;
GO

-- 1. Kiểm tra và tạo bảng [User] nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'User')
BEGIN
    CREATE TABLE [dbo].[User] (
        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [username] NVARCHAR(100) NOT NULL,
        [email] NVARCHAR(100) NOT NULL UNIQUE,
        [password] NVARCHAR(255) NOT NULL,
        [fullName] NVARCHAR(100) NULL,
        [phone] NVARCHAR(20) NULL,
        [role] NVARCHAR(20) DEFAULT 'User' NOT NULL,
        [avatarURL] NVARCHAR(MAX) NULL,
        [isEmailVerified] BIT DEFAULT 0 NOT NULL,
        [verificationCode] NVARCHAR(50) NULL,
        [verificationExpiry] DATETIME NULL,
        [refreshToken] NVARCHAR(255) NULL,
        [refreshTokenExpiryTime] DATETIME NULL,
        [createdAt] DATETIME DEFAULT GETUTCDATE() NOT NULL,
        [updatedAt] DATETIME DEFAULT GETUTCDATE() NOT NULL
    );
END
ELSE
BEGIN
    -- Nếu bảng [User] đã có, tự động bổ sung các cột mới nếu thiếu
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND name = 'fullName')
        ALTER TABLE [dbo].[User] ADD [fullName] NVARCHAR(100) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND name = 'phone')
        ALTER TABLE [dbo].[User] ADD [phone] NVARCHAR(20) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND name = 'isEmailVerified')
        ALTER TABLE [dbo].[User] ADD [isEmailVerified] BIT DEFAULT 0 NOT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND name = 'verificationCode')
        ALTER TABLE [dbo].[User] ADD [verificationCode] NVARCHAR(50) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND name = 'verificationExpiry')
        ALTER TABLE [dbo].[User] ADD [verificationExpiry] DATETIME NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND name = 'refreshToken')
        ALTER TABLE [dbo].[User] ADD [refreshToken] NVARCHAR(255) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND name = 'refreshTokenExpiryTime')
        ALTER TABLE [dbo].[User] ADD [refreshTokenExpiryTime] DATETIME NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND name = 'createdAt')
        ALTER TABLE [dbo].[User] ADD [createdAt] DATETIME DEFAULT GETUTCDATE() NOT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND name = 'updatedAt')
        ALTER TABLE [dbo].[User] ADD [updatedAt] DATETIME DEFAULT GETUTCDATE() NOT NULL;
END
GO
