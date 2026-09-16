/* =====================================================
   SmartBizERP - Phase 4 & 5 Milestone
   Purchasing: PurchaseOrders, PurchaseOrderDetails
   Sales:      SalesOrders, SalesOrderDetails
   ===================================================== */

USE SmartBizERP;
GO

-- ============================
-- PurchaseOrders (header)
-- ============================
CREATE TABLE PurchaseOrders
(
    PurchaseOrderId INT IDENTITY(1,1) PRIMARY KEY,
    PurchaseOrderNo NVARCHAR(30) NULL UNIQUE,

    SupplierId INT NOT NULL,
    WarehouseId INT NOT NULL,

    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(30) NOT NULL DEFAULT 'Draft',   -- Draft, Received
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,

    CreatedBy INT NULL,

    CONSTRAINT FK_PurchaseOrders_Supplier FOREIGN KEY (SupplierId) REFERENCES Suppliers(SupplierId),
    CONSTRAINT FK_PurchaseOrders_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT FK_PurchaseOrders_User FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);
GO

-- ============================
-- PurchaseOrderDetails (line items)
-- ============================
CREATE TABLE PurchaseOrderDetails
(
    PurchaseOrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    PurchaseOrderId INT NOT NULL,
    ProductId INT NOT NULL,

    Quantity DECIMAL(18,2) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalAmount AS (Quantity * UnitPrice) PERSISTED,

    CONSTRAINT FK_PODetails_Header FOREIGN KEY (PurchaseOrderId) REFERENCES PurchaseOrders(PurchaseOrderId),
    CONSTRAINT FK_PODetails_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
GO

-- ============================
-- SalesOrders (header)
-- ============================
CREATE TABLE SalesOrders
(
    SalesOrderId INT IDENTITY(1,1) PRIMARY KEY,
    SalesOrderNo NVARCHAR(30) NULL UNIQUE,

    CustomerId INT NOT NULL,
    WarehouseId INT NOT NULL,

    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(30) NOT NULL DEFAULT 'Draft',   -- Draft, Delivered
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,

    CreatedBy INT NULL,

    CONSTRAINT FK_SalesOrders_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_SalesOrders_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT FK_SalesOrders_User FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);
GO

-- ============================
-- SalesOrderDetails (line items)
-- ============================
CREATE TABLE SalesOrderDetails
(
    SalesOrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    SalesOrderId INT NOT NULL,
    ProductId INT NOT NULL,

    Quantity DECIMAL(18,2) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalAmount AS (Quantity * UnitPrice) PERSISTED,

    CONSTRAINT FK_SODetails_Header FOREIGN KEY (SalesOrderId) REFERENCES SalesOrders(SalesOrderId),
    CONSTRAINT FK_SODetails_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
GO

/* =====================================================
   Verify
   ===================================================== */
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;
GO
