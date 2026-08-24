-- ==========================================================
-- CSDL ĐẦY ĐỦ CHO DỰ ÁN CLONE EBAY (PRN232 BLOCK 5)
-- File gộp duy nhất: Bao gồm CSDL gốc, Bảng CartItem & Dữ liệu mẫu
-- ==========================================================

USE [master];
GO

IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'CloneEbayDB')
BEGIN
    ALTER DATABASE [CloneEbayDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [CloneEbayDB];
END
GO

CREATE DATABASE [CloneEbayDB];
GO

USE [CloneEbayDB];
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ==================== 1. SCHEMA CSDL GỐC ====================

USE [master]
GO
/****** Object:  Database [CloneEbayDB]    Script Date: 8/24/2026 3:07:55 PM ******/
CREATE DATABASE [CloneEbayDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'CloneEbayDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\CloneEbayDB.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'CloneEbayDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\CloneEbayDB_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [CloneEbayDB] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [CloneEbayDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [CloneEbayDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [CloneEbayDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [CloneEbayDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [CloneEbayDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [CloneEbayDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [CloneEbayDB] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [CloneEbayDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [CloneEbayDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [CloneEbayDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [CloneEbayDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [CloneEbayDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [CloneEbayDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [CloneEbayDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [CloneEbayDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [CloneEbayDB] SET  ENABLE_BROKER 
GO
ALTER DATABASE [CloneEbayDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [CloneEbayDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [CloneEbayDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [CloneEbayDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [CloneEbayDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [CloneEbayDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [CloneEbayDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [CloneEbayDB] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [CloneEbayDB] SET  MULTI_USER 
GO
ALTER DATABASE [CloneEbayDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [CloneEbayDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [CloneEbayDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [CloneEbayDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [CloneEbayDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [CloneEbayDB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [CloneEbayDB] SET QUERY_STORE = ON
GO
ALTER DATABASE [CloneEbayDB] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [CloneEbayDB]
GO
/****** Object:  Table [dbo].[Address]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Address](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[userId] [int] NULL,
	[fullName] [nvarchar](100) NULL,
	[phone] [nvarchar](20) NULL,
	[street] [nvarchar](100) NULL,
	[city] [nvarchar](50) NULL,
	[state] [nvarchar](50) NULL,
	[country] [nvarchar](50) NULL,
	[isDefault] [bit] NULL,
	[postalCode] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bid]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bid](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[productId] [int] NULL,
	[bidderId] [int] NULL,
	[amount] [decimal](10, 2) NULL,
	[bidTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Category]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Category](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Coupon]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Coupon](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[code] [nvarchar](50) NULL,
	[discountPercent] [decimal](5, 2) NULL,
	[startDate] [datetime] NULL,
	[endDate] [datetime] NULL,
	[maxUsage] [int] NULL,
	[productId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Dispute]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Dispute](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orderId] [int] NULL,
	[raisedBy] [int] NULL,
	[description] [nvarchar](max) NULL,
	[status] [nvarchar](20) NULL,
	[resolution] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Feedback]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Feedback](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[sellerId] [int] NULL,
	[averageRating] [decimal](3, 2) NULL,
	[totalReviews] [int] NULL,
	[positiveRate] [decimal](5, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Inventory]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Inventory](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[productId] [int] NULL,
	[quantity] [int] NULL,
	[lastUpdated] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Message]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Message](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[senderId] [int] NULL,
	[receiverId] [int] NULL,
	[content] [nvarchar](max) NULL,
	[timestamp] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItem]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItem](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orderId] [int] NULL,
	[productId] [int] NULL,
	[quantity] [int] NULL,
	[unitPrice] [decimal](10, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderTable]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderTable](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[buyerId] [int] NULL,
	[addressId] [int] NULL,
	[orderDate] [datetime] NULL,
	[totalPrice] [decimal](10, 2) NULL,
	[status] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payment]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payment](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orderId] [int] NULL,
	[userId] [int] NULL,
	[amount] [decimal](10, 2) NULL,
	[method] [nvarchar](50) NULL,
	[status] [nvarchar](20) NULL,
	[paidAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Product]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Product](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[title] [nvarchar](255) NULL,
	[description] [nvarchar](max) NULL,
	[price] [decimal](10, 2) NULL,
	[images] [nvarchar](max) NULL,
	[categoryId] [int] NULL,
	[sellerId] [int] NULL,
	[isAuction] [bit] NULL,
	[auctionEndTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReturnRequest]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReturnRequest](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orderId] [int] NULL,
	[userId] [int] NULL,
	[reason] [nvarchar](max) NULL,
	[status] [nvarchar](20) NULL,
	[createdAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Review]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Review](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[productId] [int] NULL,
	[reviewerId] [int] NULL,
	[rating] [int] NULL,
	[comment] [nvarchar](max) NULL,
	[createdAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShippingInfo]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShippingInfo](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orderId] [int] NULL,
	[carrier] [nvarchar](100) NULL,
	[trackingNumber] [nvarchar](100) NULL,
	[status] [nvarchar](50) NULL,
	[estimatedArrival] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Store]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Store](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[sellerId] [int] NULL,
	[storeName] [nvarchar](100) NULL,
	[description] [nvarchar](max) NULL,
	[bannerImageURL] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 8/24/2026 3:07:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[username] [nvarchar](100) NULL,
	[email] [nvarchar](100) NULL,
	[password] [nvarchar](255) NULL,
	[role] [nvarchar](20) NULL,
	[avatarURL] [nvarchar](max) NULL,
	[fullName] [nvarchar](255) NULL,
	[phone] [nvarchar](20) NULL,
	[createdAt] [datetime2](7) NOT NULL,
	[updatedAt] [datetime2](7) NULL,
	[isEmailVerified] [bit] NOT NULL,
	[refreshToken] [nvarchar](max) NULL,
	[refreshTokenExpiryTime] [datetime2](7) NULL,
	[verificationCode] [nvarchar](20) NULL,
	[verificationExpiry] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_createdAt]  DEFAULT (getdate()) FOR [createdAt]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_isEmailVerified]  DEFAULT ((0)) FOR [isEmailVerified]
GO
ALTER TABLE [dbo].[Address]  WITH CHECK ADD FOREIGN KEY([userId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Bid]  WITH CHECK ADD FOREIGN KEY([bidderId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Bid]  WITH CHECK ADD FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[Coupon]  WITH CHECK ADD FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[Dispute]  WITH CHECK ADD FOREIGN KEY([orderId])
REFERENCES [dbo].[OrderTable] ([id])
GO
ALTER TABLE [dbo].[Dispute]  WITH CHECK ADD FOREIGN KEY([raisedBy])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Feedback]  WITH CHECK ADD FOREIGN KEY([sellerId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Inventory]  WITH CHECK ADD FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[Message]  WITH CHECK ADD FOREIGN KEY([receiverId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Message]  WITH CHECK ADD FOREIGN KEY([senderId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD FOREIGN KEY([orderId])
REFERENCES [dbo].[OrderTable] ([id])
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[OrderTable]  WITH CHECK ADD FOREIGN KEY([addressId])
REFERENCES [dbo].[Address] ([id])
GO
ALTER TABLE [dbo].[OrderTable]  WITH CHECK ADD FOREIGN KEY([buyerId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD FOREIGN KEY([orderId])
REFERENCES [dbo].[OrderTable] ([id])
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD FOREIGN KEY([userId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD FOREIGN KEY([categoryId])
REFERENCES [dbo].[Category] ([id])
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD FOREIGN KEY([sellerId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[ReturnRequest]  WITH CHECK ADD FOREIGN KEY([orderId])
REFERENCES [dbo].[OrderTable] ([id])
GO
ALTER TABLE [dbo].[ReturnRequest]  WITH CHECK ADD FOREIGN KEY([userId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Review]  WITH CHECK ADD FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[Review]  WITH CHECK ADD FOREIGN KEY([reviewerId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[ShippingInfo]  WITH CHECK ADD FOREIGN KEY([orderId])
REFERENCES [dbo].[OrderTable] ([id])
GO
ALTER TABLE [dbo].[Store]  WITH CHECK ADD FOREIGN KEY([sellerId])
REFERENCES [dbo].[User] ([id])
GO
USE [master]
GO
ALTER DATABASE [CloneEbayDB] SET  READ_WRITE 
GO


-- ==================== 2. BẢNG CARTITEM (GIỎ HÀNG) ====================

-- Chạy sau khi đã tạo các bảng User và Product trong CloneEbayDB.
USE CloneEbayDB;
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CartItem')
BEGIN
    CREATE TABLE dbo.CartItem (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        userId INT NOT NULL,
        productId INT NOT NULL,
        quantity INT NOT NULL CHECK (quantity BETWEEN 1 AND 99),
        createdAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        updatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_CartItem_User_Product UNIQUE (userId, productId),
        CONSTRAINT FK_CartItem_User FOREIGN KEY (userId) REFERENCES dbo.[User](id),
        CONSTRAINT FK_CartItem_Product FOREIGN KEY (productId) REFERENCES dbo.Product(id)
    );
END
GO


-- ==================== 3. DỮ LIỆU MẪU (SEED DATA) ====================

-- ==========================================================
-- SCRIPT TẠO DỮ LIỆU MẪU (SEED DATA) CHO CLONE EBAY
-- Chạy để tạo Sản phẩm, Danh mục, Người bán & Đánh giá
-- ==========================================================

USE CloneEbayDB;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- 1. XÓA DỮ LIỆU CŨ NẾU CÓ ĐỂ SEED MỚI SẠCH SE
DELETE FROM [dbo].[Review];
DELETE FROM [dbo].[CartItem];
DELETE FROM [dbo].[Product];
DELETE FROM [dbo].[Category];

-- 2. THÊM NGƯỜI BÁN (SELLERS) VÀ NGƯỜI DÙNG MẪU (nếu chưa có)
IF NOT EXISTS (SELECT 1 FROM [dbo].[User] WHERE [username] = 'seller_apple')
BEGIN
    INSERT INTO [dbo].[User] ([username], [email], [password], [role], [fullName], [phone], [avatarURL], [isEmailVerified], [createdAt])
    VALUES ('seller_apple', 'apple.store@ebay.com', '$2a$11$q9hK.sK7R3b9x3Z.5e1U3e.j3Z1.K2L3M4N5O6P7Q8R9S0T1U2V3W', 'Seller', N'Apple Official Store', '0901234567', 'https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?w=150', 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[User] WHERE [username] = 'seller_fashion')
BEGIN
    INSERT INTO [dbo].[User] ([username], [email], [password], [role], [fullName], [phone], [avatarURL], [isEmailVerified], [createdAt])
    VALUES ('seller_fashion', 'fashion.hub@ebay.com', '$2a$11$q9hK.sK7R3b9x3Z.5e1U3e.j3Z1.K2L3M4N5O6P7Q8R9S0T1U2V3W', 'Seller', N'Thời Trang Cao Cấp', '0987654321', 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150', 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[User] WHERE [username] = 'seller_tech')
BEGIN
    INSERT INTO [dbo].[User] ([username], [email], [password], [role], [fullName], [phone], [avatarURL], [isEmailVerified], [createdAt])
    VALUES ('seller_tech', 'tech.world@ebay.com', '$2a$11$q9hK.sK7R3b9x3Z.5e1U3e.j3Z1.K2L3M4N5O6P7Q8R9S0T1U2V3W', 'Seller', N'Thế Giới Công Nghệ', '0912345678', 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150', 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[User] WHERE [username] = 'buyer1')
BEGIN
    INSERT INTO [dbo].[User] ([username], [email], [password], [role], [fullName], [phone], [avatarURL], [isEmailVerified], [createdAt])
    VALUES ('buyer1', 'buyer1@gmail.com', '$2a$11$q9hK.sK7R3b9x3Z.5e1U3e.j3Z1.K2L3M4N5O6P7Q8R9S0T1U2V3W', 'Buyer', N'Nguyễn Văn A', '0933445566', NULL, 1, GETUTCDATE());
END
GO

-- 3. THÊM DANH MỤC SẢN PHẨM (CATEGORIES)
INSERT INTO [dbo].[Category] ([name]) VALUES
(N'Điện thoại & Phụ kiện'),
(N'Máy tính & Laptop'),
(N'Thời trang & Phụ kiện'),
(N'Đồng hồ & Thiết bị đeo'),
(N'Âm thanh & Tai nghe'),
(N'Đồ gia dụng & Đời sống');
GO

-- 4. THÊM SẢN PHẨM MẪU (PRODUCTS)
DECLARE @SellerAppleId INT = (SELECT TOP 1 [id] FROM [dbo].[User] WHERE [username] = 'seller_apple');
DECLARE @SellerFashionId INT = (SELECT TOP 1 [id] FROM [dbo].[User] WHERE [username] = 'seller_fashion');
DECLARE @SellerTechId INT = (SELECT TOP 1 [id] FROM [dbo].[User] WHERE [username] = 'seller_tech');

-- Lấy ID danh mục theo thứ tự chèn
DECLARE @MinCatId INT = (SELECT MIN(id) FROM [dbo].[Category]);
DECLARE @CatPhone INT = @MinCatId;
DECLARE @CatLaptop INT = @MinCatId + 1;
DECLARE @CatFashion INT = @MinCatId + 2;
DECLARE @CatWatch INT = @MinCatId + 3;
DECLARE @CatAudio INT = @MinCatId + 4;
DECLARE @CatHome INT = @MinCatId + 5;

INSERT INTO [dbo].[Product] ([title], [description], [price], [images], [categoryId], [sellerId], [isAuction], [auctionEndTime]) VALUES
-- 1. iPhone 15 Pro Max
(N'iPhone 15 Pro Max 256GB - Chính hãng VN/A',
 N'Màn hình Super Retina XDR 6.7 inch với ProMotion. Khung vỏ Titanium siêu bền nhẹ. Chip A17 Pro đỉnh cao đồ họa.',
 29990000,
 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=600',
 @CatPhone, @SellerAppleId, 0, NULL),

-- 2. MacBook Pro M3
(N'MacBook Pro 14 inch M3 Pro 18GB/512GB Space Black',
 N'Cấu hình chip Apple M3 Pro mạnh mẽ cho lập trình và đồ họa chuyên nghiệp. Màn hình Liquid Retina XDR sắc nét.',
 45990000,
 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=600',
 @CatLaptop, @SellerAppleId, 0, NULL),

-- 3. Sony WH-1000XM5
(N'Tai nghe chụp tai Chống ồn Sony WH-1000XM5',
 N'Công nghệ chống ồn hàng đầu thế giới với 8 micro và chip V1. Thời lượng pin lên tới 30 giờ liên tục.',
 6990000,
 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600',
 @CatAudio, @SellerTechId, 0, NULL),

-- 4. Apple Watch Series 9 (Đấu giá)
(N'[ĐẤU GIÁ] Apple Watch Series 9 GPS 45mm Nhôm Đen',
 N'Đấu giá sản phẩm mới 100% nguyên seal. Tính năng chạm 2 lần (Double Tap) độc đáo, chip S9 SIP siêu mượt.',
 8500000,
 'https://images.unsplash.com/photo-1546868871-7041f2a55e12?w=600',
 @CatWatch, @SellerAppleId, 1, DATEADD(DAY, 3, GETUTCDATE())),

-- 5. Áo khoác Blazer Nam
(N'Áo khoác Blazer Nam Phong cách Hàn Quốc',
 N'Chất liệu vải cao cấp chống nhăn, phom dáng suông hiện đại thích hợp đi làm và đi chơi.',
 890000,
 'https://images.unsplash.com/photo-1591047139829-d91aecb6caea?w=600',
 @CatFashion, @SellerFashionId, 0, NULL),

-- 6. Máy ảnh Sony Alpha A7 IV
(N'Máy ảnh Mirrorless Sony Alpha A7 IV (Body)',
 N'Cảm biến Full-frame Exmor R CMOS 33.0 MP. Quay phim 4K 60p 10-bit 4:2:2 chuyên nghiệp.',
 54990000,
 'https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=600',
 @CatLaptop, @SellerTechId, 0, NULL),

-- 7. Giày Sneaker Nike Air Force 1
(N'Giày Sneaker Nike Air Force 1 ''07 White Classic',
 N'Thiết kế huyền thoại sắc trắng thanh lịch, đệm Air mang lại cảm giác êm ái suốt cả ngày.',
 2690000,
 'https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a?w=600',
 @CatFashion, @SellerFashionId, 0, NULL),

-- 8. Đồng hồ Rolex Submariner (Đấu giá)
(N'[ĐẤU GIÁ] Đồng hồ Nam Rolex Submariner Date Black Dial',
 N'Sản phẩm sưu tầm đã qua sử dụng, độ mới 98%. Khả năng chống nước 300m, kính Sapphire chống xước.',
 89900000,
 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600',
 @CatWatch, @SellerFashionId, 1, DATEADD(DAY, 5, GETUTCDATE())),

-- 9. Loa Bluetooth Marshall Stanmore III
(N'Loa Bluetooth Marshall Stanmore III - Hàng chính hãng',
 N'Âm thanh đậm chất Marshall với dải âm rộng hơn. Kết nối Bluetooth 5.2 và AUX 3.5mm.',
 8290000,
 'https://images.unsplash.com/photo-1545454675-3531b543be5d?w=600',
 @CatAudio, @SellerTechId, 0, NULL),

-- 10. Máy pha Cà phê Nespresso
(N'Máy pha Cà phê viên nén Nespresso Essenza Mini',
 N'Thiết kế nhỏ gọn, áp suất bơm 19 bar chuẩn Espresso Ý. Pha cà phê thơm ngon chỉ sau 25 giây.',
 3490000,
 'https://images.unsplash.com/photo-1517668808822-9ebe02f2a6e8?w=600',
 @CatHome, @SellerTechId, 0, NULL);
GO

-- 5. THÊM ĐÁNH GIÁ MẪU (REVIEWS)
DECLARE @UserReviewerId INT = (SELECT TOP 1 [id] FROM [dbo].[User] WHERE [username] NOT LIKE 'seller_%');
DECLARE @ProdIPhoneId INT = (SELECT TOP 1 [id] FROM [dbo].[Product] WHERE [title] LIKE '%iPhone 15%');
DECLARE @ProdMacBookId INT = (SELECT TOP 1 [id] FROM [dbo].[Product] WHERE [title] LIKE '%MacBook%');
DECLARE @ProdSonyId INT = (SELECT TOP 1 [id] FROM [dbo].[Product] WHERE [title] LIKE '%Sony WH-1000XM5%');

IF @UserReviewerId IS NOT NULL AND @ProdIPhoneId IS NOT NULL
BEGIN
    INSERT INTO [dbo].[Review] ([productId], [reviewerId], [rating], [comment], [createdAt]) VALUES
    (@ProdIPhoneId, @UserReviewerId, 5, N'Sản phẩm tuyệt vời, máy chạy rất mượt, đóng gói cẩn thận!', DATEADD(DAY, -2, GETUTCDATE())),
    (@ProdIPhoneId, @UserReviewerId, 5, N'Hàng chuẩn chính hãng VN/A, giao hàng siêu nhanh.', DATEADD(DAY, -1, GETUTCDATE())),
    (@ProdMacBookId, @UserReviewerId, 5, N'Màn hình quá đẹp, chip M3 Pro cân mọi tác vụ code và làm phim.', DATEADD(DAY, -3, GETUTCDATE())),
    (@ProdSonyId, @UserReviewerId, 4, N'Chống ồn cực đỉnh, nghe nhạc êm tai nhưng hơi nóng khi đeo lâu.', DATEADD(DAY, -4, GETUTCDATE()));
END
GO

PRINT N'Đã nạp dữ liệu mẫu thành công!';
GO
