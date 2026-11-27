IF EXISTS(SELECT 1 FROM dbo.AccountingBankStatements x LEFT JOIN dbo.LedgerAccounts a ON a.Id=x.AccountId AND a.FarmHouseId=x.FarmHouseId WHERE a.Id IS NULL)
 THROW 51031,'Bank statement account ownership must be repaired before adding finance constraints.',1;
IF EXISTS(SELECT 1 FROM dbo.AccountingRecurringExpenses x LEFT JOIN dbo.LedgerAccounts a ON a.Id=x.ExpenseAccountId AND a.FarmHouseId=x.FarmHouseId LEFT JOIN dbo.LedgerAccounts c ON c.Id=x.CreditAccountId AND c.FarmHouseId=x.FarmHouseId LEFT JOIN dbo.AccountingVendors v ON v.Id=x.VendorId AND v.FarmHouseId=x.FarmHouseId WHERE a.Id IS NULL OR c.Id IS NULL OR (x.VendorId IS NOT NULL AND v.Id IS NULL))
 THROW 51031,'Recurring expense account/vendor ownership must be repaired before adding finance constraints.',1;
IF EXISTS(SELECT 1 FROM dbo.AccountingAssetEvents e LEFT JOIN dbo.AccountingFixedAssets a ON a.Id=e.AssetId AND a.FarmHouseId=e.FarmHouseId LEFT JOIN dbo.AccountingVouchers v ON v.Id=e.VoucherId AND v.FarmHouseId=e.FarmHouseId WHERE a.Id IS NULL OR (e.VoucherId IS NOT NULL AND v.Id IS NULL))
 THROW 51031,'Asset event ownership must be repaired before adding finance constraints.',1;
IF EXISTS(SELECT 1 FROM dbo.AccountingBankStatements s JOIN dbo.AccountingVoucherLines l ON l.Id=s.VoucherLineId JOIN dbo.AccountingVouchers v ON v.Id=l.VoucherId WHERE v.FarmHouseId<>s.FarmHouseId)
 THROW 51031,'Bank reconciliation ownership must be repaired before adding finance constraints.',1;
