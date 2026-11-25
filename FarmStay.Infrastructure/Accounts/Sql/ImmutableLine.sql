CREATE OR ALTER TRIGGER dbo.tr_AccountingLine_Immutable ON dbo.AccountingVoucherLines AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM deleted d JOIN dbo.AccountingVouchers v ON v.Id=d.VoucherId WHERE v.Status='Posted') OR EXISTS(SELECT 1 FROM inserted i JOIN dbo.AccountingVouchers v ON v.Id=i.VoucherId WHERE v.Status='Posted')
 THROW 51002,'Posted voucher lines are immutable.',1;
END
