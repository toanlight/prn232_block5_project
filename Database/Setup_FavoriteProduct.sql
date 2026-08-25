USE CloneEbayDB;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FavoriteProduct')
BEGIN
    CREATE TABLE dbo.FavoriteProduct (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        userId INT NOT NULL,
        productId INT NOT NULL,
        createdAt DATETIME NOT NULL CONSTRAINT DF_FavoriteProduct_createdAt DEFAULT GETUTCDATE(),
        CONSTRAINT FK_FavoriteProduct_User FOREIGN KEY (userId) REFERENCES dbo.[User](id),
        CONSTRAINT FK_FavoriteProduct_Product FOREIGN KEY (productId) REFERENCES dbo.Product(id),
        CONSTRAINT UQ_FavoriteProduct_User_Product UNIQUE (userId, productId)
    );
END
GO
