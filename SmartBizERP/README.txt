SmartBizERP - Complete Package (Phases 2, 3, 4, 5, 6, 7)
=========================================================================================================

WHAT'S IN THIS PACKAGE
-------------------------
Phase 2 - Master Data:    Products, Categories, Units, Customers, Suppliers, Warehouses
Phase 3 - Inventory:      Stock Overview, Stock Ledger, Stock Adjustment, Stock Transfer
Phase 4 - Purchasing:     Purchase Orders (create, add line items, receive into stock)
Phase 5 - Sales:          Sales Orders (create, add line items, deliver out of stock)
Phase 6 - Security:       Login, Users, Roles, audit logging
Phase 7 - Reports:        Sales Report, Purchase Report, Inventory Report, Profit Report

HOW TO USE THIS ZIP
--------------------
Copy each file/folder into the SAME relative path inside your actual
SmartBizERP project folder (E:\SmartErp\SmartBizERP\SmartBizERP\ or
wherever yours lives), replacing files that already exist.

    Controllers/*.cs             -> YourProject/Controllers/
    Models/*.cs                  -> YourProject/Models/
    Utilities/*.cs                -> YourProject/Utilities/ (create folder if needed)
    Content/images/logo.png      -> YourProject/Content/images/logo.png (NEW - create folders if needed)
    Views/*/*.cshtml             -> YourProject/Views/ (matching folder names)

New folders in this package you may not have yet:
    Views/PurchaseOrders/
    Views/SalesOrders/
    Views/Reports/
    Views/Account/          (from Phase 6, if not already added)
    Utilities/              (from Phase 6, if not already added)

STEP-BY-STEP
--------------
1. Run these SQL scripts against your database, IN ORDER, if not
   already done:
     - 03_SmartBizERP_InventoryTables.sql
     - 02_SmartBizERP_SeedData.sql
     - 04_SmartBizERP_SecurityTables.sql
     - 05_SmartBizERP_PurchasingSalesTables.sql   (NEW)

2. Copy all files from this zip into your project as described above.

3. In Visual Studio: "Show All Files", select every new/faded file,
   right-click -> "Include In Project".

4. If you haven't already (Phase 6), make the two manual edits in
   MANUAL_EDITS_REQUIRED.txt: one line in App_Start\FilterConfig.cs,
   one block in Web.config.

5. Build (Ctrl+Shift+B). Fix any red errors.

6. Run (F5). Log in with admin / Admin@123.

7. TEST THE FULL FLOW:
   a. Sidebar -> Purchasing -> "+ New Purchase Order" -> pick a
      supplier and warehouse -> Create.
   b. On the order's Details page, add 1-2 line items (pick a
      product, quantity, unit price) -> Add.
   c. Click "Receive Goods" -> confirm it succeeds.
   d. Sidebar -> Stock Overview -> confirm the quantity increased
      for that product/warehouse.
   e. Sidebar -> Stock Ledger -> confirm a PURCHASE transaction
      appears, referencing the PO.
   f. Sidebar -> Sales -> "+ New Sales Order" -> pick a customer and
      the SAME warehouse you just stocked -> Create.
   g. Add a line item for the SAME product, with a quantity LESS
      than what's in stock -> Add.
   h. Click "Deliver Order" -> confirm it succeeds and stock drops.
   i. Sidebar -> Reports -> click through all four reports and
      confirm they show the order you just created.

WHAT'S DELIBERATELY SIMPLIFIED (be ready to explain this, not hide it)
--------------------------------------------------------------------------
Real ERPs split Purchasing into 3 separate documents (Purchase Order,
Goods Receipt, Supplier Payment) and Sales into 4 (Quotation, Sales
Order, Delivery, Invoice). This build collapses "receiving" and
"delivering" into a status change (Draft -> Received / Draft ->
Delivered) directly on the order, rather than separate child
documents. This was a scope decision to keep the project buildable
and understandable, not an oversight -- and it's a completely
reasonable thing to say plainly in an interview if asked.

Also not included: Supplier/Customer Payments (Accounts Payable/
Receivable), Quotations, partial deliveries/receipts, and a Users/
Roles MANAGEMENT screen (roles exist in the database and are seeded,
but there's no UI yet to create new users or assign roles through
the app -- that would use the same CRUD pattern as Categories/Units).

STEPS IN VISUAL STUDIO
-----------------------
1. Unzip this file somewhere on your PC.
2. In Solution Explorer, right-click Controllers -> Add -> Existing Item,
   and select all six *.cs files from the Controllers folder in this zip.
   (Alternatively, just copy the files directly into your project's
   Controllers folder using File Explorer, then click "Show All Files"
   in Solution Explorer and "Include In Project" on each one.)
3. For Views: right-click Views -> Add -> New Folder for each of:
   Products, Categories, Units, Customers, Suppliers, Warehouses
   (skip any that already exist).
4. Copy the matching .cshtml files from this zip into each folder using
   File Explorer, then in Solution Explorer click "Show All Files" and
   "Include In Project" on each .cshtml file (or right-click each folder
   -> Add -> Existing Item and select the files).
5. Replace your existing Views/Shared/_Layout.cshtml with the one in this
   zip (it adds the ERP sidebar navigation).

BEFORE YOU RUN IT
-------------------
- Make sure Entity Framework is installed (NuGet: EntityFramework 6.x)
  and your SmartBizERPModel.cs (Code First from database) already exists
  in your Models folder, mapping to your SmartBizERP database.
- Run 02_SmartBizERP_SeedData.sql (shared earlier in this chat) against
  your database first, so Products/Categories/Units have sample data and
  dropdowns aren't empty.
- Build the solution (Ctrl+Shift+B) and fix any red squiggly errors --
  these are almost always a missing `using` statement or a folder that
  wasn't marked "Include In Project."

AFTER IT RUNS
--------------
Navigate to:
    /Products
    /Categories
    /Units
    /Customers
    /Suppliers
    /Warehouses

Each should show a searchable table with Create/Edit/Delete working,
Bootstrap-styled, inside the new ERP sidebar layout.

WHAT'S NOT INCLUDED YET
-------------------------
This zip covers Phase 2 (Master Data) only, per the project roadmap.
Phase 3 (Inventory / Stock Ledger), Phase 4 (Purchasing), Phase 5 (Sales),
Phase 6 (Security/Login), and Phase 7 (Dashboard/Reports) are not in this
package -- ask for them module by module, same as this one, once Phase 2
is confirmed working end-to-end.
