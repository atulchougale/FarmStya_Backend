CREATE OR ALTER TRIGGER dbo.tr_AccountingVoucher_Immutable ON dbo.AccountingVouchers AFTER UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM deleted WHERE Status='Posted') THROW 51001,'Posted vouchers are immutable; use reversal or adjustment.',1;
END
