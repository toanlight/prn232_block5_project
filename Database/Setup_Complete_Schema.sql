USE CloneEbayDB;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Bảng Category
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Category')
BEGIN
    CREATE TABLE dbo.Category (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        name NVARCHAR(100) NOT NULL
    );
END
GO

-- 2. Bảng Store
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Store')
BEGIN
    CREATE TABLE dbo.Store (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ownerId INT NOT NULL,
        name NVARCHAR(100) NOT NULL,
        description NVARCHAR(MAX) NULL,
        logoURL NVARCHAR(MAX) NULL,
        CONSTRAINT FK_Store_User FOREIGN KEY (ownerId) REFERENCES dbo.[User](id)
    );
END
GO

-- 3. Bảng Product
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Product')
BEGIN
    CREATE TABLE dbo.Product (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        sellerId INT NOT NULL,
        categoryId INT NOT NULL,
        title NVARCHAR(255) NOT NULL,
        description NVARCHAR(MAX) NULL,
        price DECIMAL(10,2) NOT NULL,
        images NVARCHAR(MAX) NULL,
        isAuction BIT NOT NULL DEFAULT 0,
        auctionEndTime DATETIME NULL,
        CONSTRAINT FK_Product_Category FOREIGN KEY (categoryId) REFERENCES dbo.Category(id),
        CONSTRAINT FK_Product_Seller FOREIGN KEY (sellerId) REFERENCES dbo.[User](id)
    );
END
GO

-- 4. Bảng CartItem
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

-- 5. Bảng Coupon
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Coupon')
BEGIN
    CREATE TABLE dbo.Coupon (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        productId INT NULL,
        code NVARCHAR(50) NOT NULL,
        discountPercent DECIMAL(5,2) NOT NULL,
        startDate DATETIME NOT NULL,
        endDate DATETIME NULL,
        maxUsage INT NULL,
        CONSTRAINT FK_Coupon_Product FOREIGN KEY (productId) REFERENCES dbo.Product(id)
    );
END
GO

-- 6. Bảng OrderTable
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderTable')
BEGIN
    CREATE TABLE dbo.OrderTable (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        buyerId INT NOT NULL,
        addressId INT NOT NULL,
        orderDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
        totalPrice DECIMAL(10,2) NOT NULL,
        status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        CONSTRAINT FK_OrderTable_Buyer FOREIGN KEY (buyerId) REFERENCES dbo.[User](id),
        CONSTRAINT FK_OrderTable_Address FOREIGN KEY (addressId) REFERENCES dbo.Address(id)
    );
END
GO

-- 7. Bảng OrderItem
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderItem')
BEGIN
    CREATE TABLE dbo.OrderItem (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        orderId INT NOT NULL,
        productId INT NOT NULL,
        quantity INT NOT NULL,
        unitPrice DECIMAL(10,2) NOT NULL,
        CONSTRAINT FK_OrderItem_Order FOREIGN KEY (orderId) REFERENCES dbo.OrderTable(id),
        CONSTRAINT FK_OrderItem_Product FOREIGN KEY (productId) REFERENCES dbo.Product(id)
    );
END
GO

-- 8. Bảng Review
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Review')
BEGIN
    CREATE TABLE dbo.Review (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        productId INT NOT NULL,
        reviewerId INT NOT NULL,
        rating INT NOT NULL CHECK (rating BETWEEN 1 AND 5),
        comment NVARCHAR(MAX) NULL,
        createdAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Review_Product FOREIGN KEY (productId) REFERENCES dbo.Product(id),
        CONSTRAINT FK_Review_Reviewer FOREIGN KEY (reviewerId) REFERENCES dbo.[User](id)
    );
END
GO

-- 9. Bảng ReturnRequest
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReturnRequest')
BEGIN
    CREATE TABLE dbo.ReturnRequest (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        orderId INT NOT NULL,
        userId INT NOT NULL,
        reason NVARCHAR(MAX) NOT NULL,
        status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        createdAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_ReturnRequest_Order FOREIGN KEY (orderId) REFERENCES dbo.OrderTable(id),
        CONSTRAINT FK_ReturnRequest_User FOREIGN KEY (userId) REFERENCES dbo.[User](id)
    );
END
GO

-- 10. Bảng Payment
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Payment')
BEGIN
    CREATE TABLE dbo.Payment (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        orderId INT NOT NULL,
        userId INT NOT NULL,
        amount DECIMAL(10,2) NOT NULL,
        method NVARCHAR(50) NOT NULL,
        status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        paidAt DATETIME NULL,
        CONSTRAINT FK_Payment_Order FOREIGN KEY (orderId) REFERENCES dbo.OrderTable(id),
        CONSTRAINT FK_Payment_User FOREIGN KEY (userId) REFERENCES dbo.[User](id)
    );
END
GO

-- 11. Bảng Bid
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Bid')
BEGIN
    CREATE TABLE dbo.Bid (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        productId INT NOT NULL,
        bidderId INT NOT NULL,
        amount DECIMAL(10,2) NOT NULL,
        bidTime DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Bid_Product FOREIGN KEY (productId) REFERENCES dbo.Product(id),
        CONSTRAINT FK_Bid_Bidder FOREIGN KEY (bidderId) REFERENCES dbo.[User](id)
    );
END
GO

-- 12. Bảng Dispute
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Dispute')
BEGIN
    CREATE TABLE dbo.Dispute (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        orderId INT NOT NULL,
        raisedBy INT NOT NULL,
        description NVARCHAR(MAX) NOT NULL,
        status NVARCHAR(20) NOT NULL DEFAULT 'Open',
        resolution NVARCHAR(MAX) NULL,
        CONSTRAINT FK_Dispute_Order FOREIGN KEY (orderId) REFERENCES dbo.OrderTable(id),
        CONSTRAINT FK_Dispute_RaisedBy FOREIGN KEY (raisedBy) REFERENCES dbo.[User](id)
    );
END
GO

-- 13. Bảng Feedback
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Feedback')
BEGIN
    CREATE TABLE dbo.Feedback (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        sellerId INT NOT NULL,
        averageRating DECIMAL(3,2) NULL,
        totalReviews INT NULL DEFAULT 0,
        positiveRate DECIMAL(5,2) NULL,
        CONSTRAINT FK_Feedback_Seller FOREIGN KEY (sellerId) REFERENCES dbo.[User](id)
    );
END
GO

-- 14. Bảng Inventory
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Inventory')
BEGIN
    CREATE TABLE dbo.Inventory (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        productId INT NOT NULL,
        quantity INT NOT NULL,
        lastUpdated DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Inventory_Product FOREIGN KEY (productId) REFERENCES dbo.Product(id)
    );
END
GO

-- 15. Bảng Message
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Message')
BEGIN
    CREATE TABLE dbo.Message (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        senderId INT NOT NULL,
        receiverId INT NOT NULL,
        content NVARCHAR(MAX) NOT NULL,
        timestamp DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Message_Sender FOREIGN KEY (senderId) REFERENCES dbo.[User](id),
        CONSTRAINT FK_Message_Receiver FOREIGN KEY (receiverId) REFERENCES dbo.[User](id)
    );
END
GO

-- 16. Bảng ShippingInfo
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ShippingInfo')
BEGIN
    CREATE TABLE dbo.ShippingInfo (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        orderId INT NOT NULL,
        trackingNumber NVARCHAR(100) NULL,
        carrier NVARCHAR(100) NULL,
        status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        estimatedDelivery DATETIME NULL,
        CONSTRAINT FK_ShippingInfo_Order FOREIGN KEY (orderId) REFERENCES dbo.OrderTable(id)
    );
END
GO

PRINT N'✅ Đã khởi tạo hoàn tất toàn bộ schema database CloneEbayDB.';
GO
