/* =====================================================
   SmartBizERP - Seed Data
   Sample rows so the Products screen has something
   to display and dropdowns aren't empty.
   ===================================================== */

USE SmartBizERP;
GO
-- ============================
-- Categories
-- ============================
INSERT INTO Categories (CategoryName, Description) VALUES
('Electronics', 'Computers, phones, and accessories'),
('Office Supplies', 'Stationery and general office items'),
('Furniture', 'Desks, chairs, and storage');
GO

-- ============================
-- Units
-- ============================
INSERT INTO Units (UnitName, ShortName) VALUES
('Piece', 'PCS'),
('Box', 'BOX'),
('Kilogram', 'KG');
GO

-- ============================
-- Customers
-- ============================
INSERT INTO Customers (CustomerCode, CustomerName, Phone, Email, Address, CreditLimit) VALUES
('CUST-0001', 'ABC Traders', '01700000001', 'abc@traders.com', 'Dhaka, Bangladesh', 50000),
('CUST-0002', 'Nova Retail Ltd.', '01700000002', 'contact@novaretail.com', 'Chattogram, Bangladesh', 100000);
GO

-- ============================
-- Suppliers
-- ============================
INSERT INTO Suppliers (SupplierCode, SupplierName, Phone, Email, Address) VALUES
('SUPP-0001', 'Global Tech Distributors', '01800000001', 'sales@globaltech.com', 'Dhaka, Bangladesh'),
('SUPP-0002', 'Prime Office Supply Co.', '01800000002', 'info@primeoffice.com', 'Dhaka, Bangladesh');
GO

-- ============================
-- Warehouses
-- ============================
INSERT INTO Warehouses (WarehouseCode, WarehouseName, Address) VALUES
('WH-DHAKA', 'Dhaka Main Warehouse', 'Tejgaon, Dhaka'),
('WH-SAVAR', 'Savar Storage Facility', 'Savar, Dhaka');
GO

-- ============================
-- Products (sample rows to see in the list)
-- ============================
INSERT INTO Products (ProductCode, ProductName, CategoryId, UnitId, CostPrice, SellingPrice, ReorderLevel, IsActive) VALUES
('PRD-0001', 'Dell Latitude Laptop', 1, 1, 65000, 78000, 5),
('PRD-0002', 'Wireless Mouse', 1, 1, 500, 800, 20),
('PRD-0003', 'A4 Paper Ream', 2, 2, 300, 400, 50),
('PRD-0004', 'Office Chair - Ergonomic', 3, 1, 8000, 11000, 10),
('PRD-0005', 'Mechanical Keyboard', 1, 1, 2500, 3500, 15);
GO

/* =====================================================
   Verify
   ===================================================== */
SELECT * FROM Categories;
SELECT * FROM Units;
SELECT * FROM Products;
GO
