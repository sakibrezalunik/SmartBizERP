/* =====================================================
   SmartBizERP - Phase 3 Milestone
   Inventory Tables: WarehouseStock, StockTransactions
   ===================================================== */

USE SmartBizERP;
GO

-- ============================
-- WarehouseStock
-- Current quantity of each product in each warehouse.
-- This is a "materialized" snapshot -- it exists purely
-- for fast reads. The real source of truth is the
-- StockTransactions ledger below.
-- ============================
CREATE TABLE WarehouseStock
(
    WarehouseStockId INT IDENTITY(1,1) PRIMARY KEY,

    WarehouseId INT NOT NULL,
    ProductId INT NOT NULL,

    Quantity DECIMAL(18,2) NOT NULL DEFAULT 0,

    CONSTRAINT FK_WarehouseStock_Warehouse
        FOREIGN KEY (WarehouseId)
        REFERENCES Warehouses(WarehouseId),

    CONSTRAINT FK_WarehouseStock_Product
        FOREIGN KEY (ProductId)
        REFERENCES Products(ProductId),

    CONSTRAINT UQ_Warehouse_Product
        UNIQUE (WarehouseId, ProductId)
);
GO

-- ============================
-- StockTransactions
-- The immutable ledger. Every stock movement -- purchase,
-- sale, adjustment, transfer -- is a row here. Never
-- updated or deleted, only inserted. This is what lets you
-- answer "how did this product's stock get to this number?"
-- ============================
CREATE TABLE StockTransactions
(
    StockTransactionId BIGINT IDENTITY(1,1) PRIMARY KEY,

    ProductId INT NOT NULL,
    WarehouseId INT NOT NULL,

    TransactionType NVARCHAR(30) NOT NULL,  -- PURCHASE, SALE, ADJUSTMENT, TRANSFER

    Quantity DECIMAL(18,2) NOT NULL,        -- positive = stock in, negative = stock out

    ReferenceType NVARCHAR(50),             -- e.g. 'PurchaseOrder', 'SalesOrder', 'Manual'
    ReferenceId INT,                        -- ID of the related PO/SO/etc, if any

    TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),

    CreatedBy INT NULL,

    CONSTRAINT FK_StockTransactions_Product
        FOREIGN KEY (ProductId)
        REFERENCES Products(ProductId),

    CONSTRAINT FK_StockTransactions_Warehouse
        FOREIGN KEY (WarehouseId)
        REFERENCES Warehouses(WarehouseId)
);
GO

/* =====================================================
   Verify
   ===================================================== */
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO
