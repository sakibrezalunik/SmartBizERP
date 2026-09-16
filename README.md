# SmartBiz ERP

**A modular business management &amp; inventory system built with ASP.NET MVC 5, Entity Framework 6, and SQL Server.**

SmartBiz ERP covers master data management, warehouse inventory with a full audit ledger, purchasing, sales, role-based user security, and cross-module business reporting — all connected around a single source of truth for stock movement.

---

## ⚠️ All Rights Reserved

This is a **personal portfolio project**. The source code is publicly visible for demonstration and review purposes (e.g. job applications, interviews) only.

**No permission is granted to copy, reuse, modify, distribute, or deploy this code, in whole or in part, for any purpose.** See [LICENSE](./LICENSE) for full terms.

---

## Features

### Master Data
- Full CRUD for Products, Categories, Units, Customers, Suppliers, and Warehouses
- Soft delete (never hard-deletes records referenced by historical orders)
- Search, filtering, and pagination

### Inventory
- **Stock Overview** — real-time on-hand quantity per product/warehouse, with low-stock highlighting
- **Stock Ledger** — permanent, append-only history of every stock movement
- **Stock Adjustment** — manual correction workflow for physical counts, damage, or loss
- **Stock Transfer** — moves stock between warehouses with pre-validated availability, posted atomically

### Purchasing & Sales
- Header/detail Purchase Orders and Sales Orders
- Receiving a Purchase Order posts stock in; Delivering a Sales Order posts stock out
- Full stock-availability check before any Sales Order can be delivered
- Orders tied to the logged-in user who created them

### Security
- Custom login system (ASP.NET Forms Authentication + SHA-256 password hashing)
- Role-based access control (Admin / Staff), enforced server-side on every request
- User management module (create accounts, assign roles, reset passwords) — Admin-only
- Login/logout audit trail

### Reports
- Sales Report, Purchase Report, Low-Stock Inventory Report, Profit Report — all reading live from the same operational tables

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# |
| Web Framework | ASP.NET MVC 5 (.NET Framework 4.7.2) |
| ORM | Entity Framework 6 (Code First, mapped to an existing database) |
| Database | Microsoft SQL Server |
| Front-end | Bootstrap 5, jQuery, Razor views |
| Auth | ASP.NET Forms Authentication + custom password hashing |
| IDE | Visual Studio 2026 |

---

## Getting Started

### Prerequisites
- Visual Studio 2022/2026 with the **ASP.NET and web development** workload (including the *.NET Framework project and item templates* optional component)
- SQL Server (Developer or Standard edition), running locally
- SQL Server Management Studio or Azure Data Studio

### Database Setup

Run the scripts in `/Database`, **in this exact order**, against your local SQL Server instance:

```
01_SmartBizERP_MasterTables.sql
02_SmartBizERP_SeedData.sql
03_SmartBizERP_InventoryTables.sql
04_SmartBizERP_SecurityTables.sql
05_SmartBizERP_PurchasingSalesTables.sql
```

### Run the Project

1. Open `SmartBizERP.slnx` in Visual Studio
2. Confirm the connection string in `Web.config` points at your local SQL Server instance
3. Build the solution (`Ctrl+Shift+B`)
4. Run (`F5`) — you'll be redirected to the login page

### Default Login

| Field | Value |
|---|---|
| Username | `admin` |
| Password | `Admin@123` |

> This is a seeded demo credential for local evaluation only. Change it (or create a new account) via the Users module after first login.

---

## Project Structure

```
Controllers/    One controller per module
Models/         EF Code First POCO entities + DbContext (split by module)
Views/          Razor views, one folder per controller
Utilities/      Standalone helper classes (e.g. password hashing)
Content/images/ Static assets
Database/       SQL scripts documenting the full schema, in build order
```

---

## Design Notes

- **Why a separate `StockTransactions` ledger, not just a running total?** A single quantity column can be overwritten with no record of how it got there. The ledger lets any movement be traced back to its source.
- **Why database transactions around stock updates?** The ledger entry and the cached quantity are one logical change split across two tables — wrapping both in a transaction guarantees they can't drift out of sync.
- **Why is Sales Order delivery pre-checked in a separate pass?** Checking and updating stock line-by-line in the same loop could deliver some lines and fail partway through. Checking everything first prevents a half-delivered order.

### Known Simplifications

Real ERP systems typically split Purchasing into 3 documents (PO → Goods Receipt → Supplier Payment) and Sales into 4 (Quotation → Sales Order → Delivery → Invoice). This project collapses receiving/delivering into a status change on the order itself, to keep scope manageable while still exercising the same inventory logic. Not included: Accounts Payable/Receivable, Quotations, partial receipts/deliveries.

---

## License

Copyright © 2026. All rights reserved. See [LICENSE](./LICENSE).
