CREATE OR ALTER TRIGGER dbo.TR_AccountingBankStatementTenant ON dbo.AccountingBankStatements AFTER INSERT,UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted s JOIN dbo.AccountingVoucherLines l ON l.Id=s.VoucherLineId JOIN dbo.AccountingVouchers v ON v.Id=l.VoucherId WHERE v.FarmHouseId<>s.FarmHouseId)
 THROW 51032,'Bank statement and voucher line must belong to the same farmhouse.',1;
END
