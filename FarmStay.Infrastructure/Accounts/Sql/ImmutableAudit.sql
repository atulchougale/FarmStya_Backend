CREATE OR ALTER TRIGGER dbo.tr_AccountingAudit_Immutable ON dbo.AccountingAudits AFTER UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 THROW 51003,'Accounting audit records are immutable.',1;
END
