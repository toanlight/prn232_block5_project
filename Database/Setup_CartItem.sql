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
