SET NOCOUNT ON;
DECLARE @module int=(SELECT ModuleId FROM dbo.Modules WHERE ModuleName='Accounts');
IF @module IS NULL THROW 51000,'Existing Accounts module is required',1;
UPDATE dbo.Modules SET Icon='bi bi-wallet2',Route='/accounts' WHERE ModuleId=@module;
INSERT dbo.FarmHouseModules(FarmHouseId,ModuleId,IsEnabled,IsDeleted,CreatedDate)
 SELECT f.FarmHouseId,@module,1,0,GETDATE() FROM dbo.FarmHouses f WHERE f.IsDeleted=0 AND NOT EXISTS(SELECT 1 FROM dbo.FarmHouseModules x WHERE x.FarmHouseId=f.FarmHouseId AND x.ModuleId=@module);
INSERT dbo.RoleModules(RoleId,ModuleId,IsActive,CreatedDate)
 SELECT r.RoleId,@module,1,GETDATE() FROM dbo.Roles r WHERE r.RoleName IN('Admin','Farm Owner','FarmOwner') AND NOT EXISTS(SELECT 1 FROM dbo.RoleModules x WHERE x.RoleId=r.RoleId AND x.ModuleId=@module);
DECLARE @p TABLE(Name nvarchar(150));
INSERT @p VALUES ('Accounts.View'),('Accounts.Create'),('Accounts.Edit'),('Accounts.Post'),('Accounts.Reverse'),('Accounts.Export'),('Accounts.ManageMasters'),('Accounts.ManagePeriods'),('Accounts.ManageAssets');
INSERT dbo.Permissions(PermissionName,Description,DisplayOrder,IsSystemPermission,IsActive,CreatedDate)
 SELECT Name,Name,1,1,1,GETDATE() FROM @p WHERE NOT EXISTS(SELECT 1 FROM dbo.Permissions x WHERE x.PermissionName=Name);
INSERT dbo.RoleModulePermissions(RoleModuleId,PermissionId,IsAllowed,IsActive,IsDeleted,CreatedDate)
 SELECT rm.RoleModuleId,p.PermissionId,1,1,0,GETDATE() FROM dbo.RoleModules rm JOIN dbo.Roles r ON r.RoleId=rm.RoleId CROSS JOIN dbo.Permissions p JOIN @p v ON v.Name=p.PermissionName
 WHERE rm.ModuleId=@module AND r.RoleName IN('Admin','Farm Owner','FarmOwner') AND NOT EXISTS(SELECT 1 FROM dbo.RoleModulePermissions x WHERE x.RoleModuleId=rm.RoleModuleId AND x.PermissionId=p.PermissionId);
DECLARE @masters TABLE(Kind nvarchar(200),Code nvarchar(40),Name nvarchar(200),Ledger nvarchar(200));
INSERT @masters VALUES
(N'Group',N'ASSET',N'ASSET',NULL),
(N'Group',N'LIABILITY',N'LIABILITY',NULL),
(N'Group',N'EQUITY',N'EQUITY',NULL),
(N'Group',N'INCOME',N'INCOME',NULL),
(N'Group',N'DIRECT_EXP',N'DIRECT_EXP',NULL),
(N'Group',N'INDIRECT_EXP',N'INDIRECT_EXP',NULL),
(N'ExpenseCategory',N'EXPENSECATEGORY_01',N'Electricity',N'6000'),
(N'ExpenseCategory',N'EXPENSECATEGORY_02',N'Water',N'6010'),
(N'ExpenseCategory',N'EXPENSECATEGORY_03',N'Rent / Lease',N'6020'),
(N'ExpenseCategory',N'EXPENSECATEGORY_04',N'Gas / Fuel',N'6030'),
(N'ExpenseCategory',N'EXPENSECATEGORY_05',N'Repairs & Maintenance',N'6040'),
(N'ExpenseCategory',N'EXPENSECATEGORY_06',N'Labour / Wages',N'6050'),
(N'ExpenseCategory',N'EXPENSECATEGORY_07',N'Cleaning / Housekeeping',N'6060'),
(N'ExpenseCategory',N'EXPENSECATEGORY_08',N'Internet / Telephone',N'6070'),
(N'ExpenseCategory',N'EXPENSECATEGORY_09',N'Equipment Maintenance',N'6040'),
(N'ExpenseCategory',N'EXPENSECATEGORY_10',N'Consumables / Supplies',N'6120'),
(N'ExpenseCategory',N'EXPENSECATEGORY_11',N'Travel / Conveyance',N'6110'),
(N'ExpenseCategory',N'EXPENSECATEGORY_12',N'Bank Charges',N'6100'),
(N'ExpenseCategory',N'EXPENSECATEGORY_13',N'Office / Administration',N'6080'),
(N'ExpenseCategory',N'EXPENSECATEGORY_14',N'Insurance',N'6130'),
(N'ExpenseCategory',N'EXPENSECATEGORY_15',N'Taxes / Licences',N'6130'),
(N'ExpenseCategory',N'EXPENSECATEGORY_16',N'Miscellaneous',N'6130'),
(N'IncomeCategory',N'INCOMECATEGORY_01',N'Booking Income',N'4000'),
(N'IncomeCategory',N'INCOMECATEGORY_02',N'Event / Decoration Income',N'4000'),
(N'IncomeCategory',N'INCOMECATEGORY_03',N'Other Operating Income',N'4010'),
(N'IncomeCategory',N'INCOMECATEGORY_04',N'Miscellaneous Income',N'4020'),
(N'AssetCategory',N'ASSETCATEGORY_01',N'Furniture & Fixtures',NULL),
(N'AssetCategory',N'ASSETCATEGORY_02',N'Electrical Equipment',NULL),
(N'AssetCategory',N'ASSETCATEGORY_03',N'Kitchen Equipment',NULL),
(N'AssetCategory',N'ASSETCATEGORY_04',N'Refrigeration',NULL),
(N'AssetCategory',N'ASSETCATEGORY_05',N'Air Conditioning',NULL),
(N'AssetCategory',N'ASSETCATEGORY_06',N'Laundry Equipment',NULL),
(N'AssetCategory',N'ASSETCATEGORY_07',N'Computers & Office Equipment',NULL),
(N'AssetCategory',N'ASSETCATEGORY_08',N'Vehicles',NULL),
(N'AssetCategory',N'ASSETCATEGORY_09',N'Building & Improvements',NULL),
(N'AssetCategory',N'ASSETCATEGORY_10',N'Water Systems & Pumps',NULL),
(N'AssetCategory',N'ASSETCATEGORY_11',N'Generator & Power Backup',NULL),
(N'AssetCategory',N'ASSETCATEGORY_12',N'Other Fixed Assets',NULL),
(N'AssetType',N'ASSETTYPE_01',N'Refrigerator',NULL),
(N'AssetType',N'ASSETTYPE_02',N'AC',NULL),
(N'AssetType',N'ASSETTYPE_03',N'Washing Machine',NULL),
(N'AssetType',N'ASSETTYPE_04',N'Mixer Grinder',NULL),
(N'AssetType',N'ASSETTYPE_05',N'TV',NULL),
(N'AssetType',N'ASSETTYPE_06',N'Water Pump',NULL),
(N'AssetType',N'ASSETTYPE_07',N'Generator',NULL),
(N'AssetType',N'ASSETTYPE_08',N'Inverter',NULL),
(N'AssetType',N'ASSETTYPE_09',N'Furniture Set',NULL),
(N'AssetType',N'ASSETTYPE_10',N'Computer',NULL),
(N'AssetType',N'ASSETTYPE_11',N'Printer',NULL),
(N'AssetType',N'ASSETTYPE_12',N'Solar Equipment',NULL),
(N'PaymentMode',N'PAYMENTMODE_01',N'Cash',NULL),
(N'PaymentMode',N'PAYMENTMODE_02',N'UPI',NULL),
(N'PaymentMode',N'PAYMENTMODE_03',N'Bank Transfer',NULL),
(N'PaymentMode',N'PAYMENTMODE_04',N'Debit Card',NULL),
(N'PaymentMode',N'PAYMENTMODE_05',N'Credit Card',NULL),
(N'PaymentMode',N'PAYMENTMODE_06',N'Cheque',NULL),
(N'PaymentMode',N'PAYMENTMODE_07',N'Other',NULL),
(N'Unit',N'UNIT_01',N'Unit',NULL),
(N'Unit',N'UNIT_02',N'Piece',NULL),
(N'Unit',N'UNIT_03',N'Set',NULL),
(N'Unit',N'UNIT_04',N'Litre',NULL),
(N'Unit',N'UNIT_05',N'Millilitre',NULL),
(N'Unit',N'UNIT_06',N'Kilogram',NULL),
(N'Unit',N'UNIT_07',N'Gram',NULL),
(N'Unit',N'UNIT_08',N'Metre',NULL),
(N'Unit',N'UNIT_09',N'Square Metre',NULL),
(N'Unit',N'UNIT_10',N'Month',NULL),
(N'Unit',N'UNIT_11',N'Day',NULL),
(N'Unit',N'UNIT_12',N'Hour',NULL),
(N'Unit',N'UNIT_13',N'Service',NULL),
(N'DepreciationMethod',N'DEPRECIATIONMETHOD_01',N'SLM',NULL),
(N'DepreciationMethod',N'DEPRECIATIONMETHOD_02',N'WDV',NULL),
(N'CostCentre',N'COSTCENTRE_01',N'Administration',NULL),
(N'CostCentre',N'COSTCENTRE_02',N'Maintenance',NULL),
(N'CostCentre',N'COSTCENTRE_03',N'Housekeeping',NULL),
(N'CostCentre',N'COSTCENTRE_04',N'Operations',NULL),
(N'CostCentre',N'COSTCENTRE_05',N'Kitchen',NULL),
(N'CostCentre',N'COSTCENTRE_06',N'Marketing',NULL),
(N'VoucherType',N'VOUCHERTYPE_01',N'Journal',NULL),
(N'VoucherType',N'VOUCHERTYPE_02',N'Receipt',NULL),
(N'VoucherType',N'VOUCHERTYPE_03',N'Payment',NULL),
(N'VoucherType',N'VOUCHERTYPE_04',N'Transfer',NULL),
(N'VoucherType',N'VOUCHERTYPE_05',N'Income',NULL),
(N'VoucherType',N'VOUCHERTYPE_06',N'Expense',NULL),
(N'VoucherType',N'VOUCHERTYPE_07',N'VendorBill',NULL),
(N'VoucherType',N'VOUCHERTYPE_08',N'VendorPayment',NULL),
(N'VoucherType',N'VOUCHERTYPE_09',N'OpeningBalance',NULL),
(N'VoucherType',N'VOUCHERTYPE_10',N'Depreciation',NULL),
(N'VoucherType',N'VOUCHERTYPE_11',N'Reversal',NULL),
(N'VoucherType',N'VOUCHERTYPE_12',N'Adjustment',NULL);
INSERT dbo.AccountingMasters(Kind,Code,Name,DefaultLedgerCode,IsSystemDefined,IsActive,CreatedBy,CreatedDate)
 SELECT m.Kind,m.Code,m.Name,m.Ledger,1,1,0,GETDATE() FROM @masters m WHERE NOT EXISTS(SELECT 1 FROM dbo.AccountingMasters a WHERE a.FarmHouseId IS NULL AND a.Kind=m.Kind AND a.Code=m.Code);
DECLARE @chart TABLE(Code nvarchar(40),Name nvarchar(200),GroupCode nvarchar(200));
INSERT @chart VALUES
(N'1000',N'Cash in Hand',N'ASSET'),
(N'1010',N'Bank Accounts Control',N'ASSET'),
(N'1100',N'Accounts Receivable - Customers',N'ASSET'),
(N'1200',N'Input GST',N'ASSET'),
(N'1500',N'Fixed Assets',N'ASSET'),
(N'1590',N'Accumulated Depreciation',N'ASSET'),
(N'2000',N'Accounts Payable - Vendors',N'LIABILITY'),
(N'2100',N'Customer Advances',N'LIABILITY'),
(N'2200',N'Output GST',N'LIABILITY'),
(N'3000',N'Owner Capital',N'EQUITY'),
(N'3100',N'Retained Earnings / Opening Equity',N'EQUITY'),
(N'4000',N'Booking Revenue',N'INCOME'),
(N'4010',N'Other Operating Income',N'INCOME'),
(N'4020',N'Miscellaneous Income',N'INCOME'),
(N'5000',N'Direct Booking / Service Costs',N'DIRECT_EXP'),
(N'6000',N'Electricity Expense',N'INDIRECT_EXP'),
(N'6010',N'Water Expense',N'INDIRECT_EXP'),
(N'6020',N'Rent Expense',N'INDIRECT_EXP'),
(N'6030',N'Gas / Fuel Expense',N'INDIRECT_EXP'),
(N'6040',N'Repairs & Maintenance',N'INDIRECT_EXP'),
(N'6050',N'Labour / Wages Expense',N'INDIRECT_EXP'),
(N'6060',N'Cleaning & Housekeeping',N'INDIRECT_EXP'),
(N'6070',N'Internet & Telephone',N'INDIRECT_EXP'),
(N'6080',N'Office & Administration',N'INDIRECT_EXP'),
(N'6090',N'Depreciation Expense',N'INDIRECT_EXP'),
(N'6100',N'Bank Charges',N'INDIRECT_EXP'),
(N'6110',N'Travel & Conveyance',N'INDIRECT_EXP'),
(N'6120',N'Consumables & Supplies',N'INDIRECT_EXP'),
(N'6130',N'Miscellaneous Expense',N'INDIRECT_EXP');
INSERT dbo.LedgerAccounts(FarmHouseId,Code,Name,GroupCode,NormalBalance,IsSystemDefined,IsActive,IsTaxAccount,CreatedBy,CreatedDate)
 SELECT f.FarmHouseId,c.Code,c.Name,c.GroupCode,CASE WHEN c.GroupCode IN('LIABILITY','EQUITY','INCOME') OR c.Code='1590' THEN 'Credit' ELSE 'Debit' END,1,1,CASE WHEN c.Code IN('1200','2200') THEN 1 ELSE 0 END,0,GETDATE()
 FROM dbo.FarmHouses f CROSS JOIN @chart c WHERE f.IsDeleted=0 AND NOT EXISTS(SELECT 1 FROM dbo.LedgerAccounts a WHERE a.FarmHouseId=f.FarmHouseId AND a.Code=c.Code);
INSERT dbo.AccountingSettings(FarmHouseId,GstEnabled,CapitalisationThreshold,RevenuePolicy,CurrencyCode,CreatedBy,CreatedDate)
 SELECT f.FarmHouseId,0,0,'Checkout','INR',0,GETDATE() FROM dbo.FarmHouses f WHERE f.IsDeleted=0 AND NOT EXISTS(SELECT 1 FROM dbo.AccountingSettings s WHERE s.FarmHouseId=f.FarmHouseId);
DECLARE @start date=DATEFROMPARTS(YEAR(GETDATE())-CASE WHEN MONTH(GETDATE())<4 THEN 1 ELSE 0 END,4,1);
INSERT dbo.AccountingFinancialYears(FarmHouseId,Name,FromDate,ToDate,IsClosed,CreatedBy,CreatedDate)
 SELECT f.FarmHouseId,CONCAT(YEAR(@start),'-',YEAR(@start)+1),@start,DATEADD(day,-1,DATEADD(year,1,@start)),0,0,GETDATE() FROM dbo.FarmHouses f WHERE f.IsDeleted=0 AND NOT EXISTS(SELECT 1 FROM dbo.AccountingFinancialYears y WHERE y.FarmHouseId=f.FarmHouseId AND y.FromDate=@start);
INSERT dbo.AccountingPeriods(FarmHouseId,FinancialYearId,FromDate,ToDate,IsClosed,CreatedBy,CreatedDate)
 SELECT y.FarmHouseId,y.Id,DATEADD(month,n.i,@start),DATEADD(day,-1,DATEADD(month,n.i+1,@start)),0,0,GETDATE()
 FROM dbo.AccountingFinancialYears y CROSS JOIN (VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11)) n(i)
 WHERE y.FromDate=@start AND NOT EXISTS(SELECT 1 FROM dbo.AccountingPeriods p WHERE p.FarmHouseId=y.FarmHouseId AND p.FromDate=DATEADD(month,n.i,@start));
