CREATE OR ALTER PROCEDURE dbo.usp_Accounts_Report
 @FarmHouseId int, @Report nvarchar(40), @From date, @To date, @AccountId bigint=NULL,
 @Search nvarchar(100)=N'', @Offset int=0, @Take int=100
AS
BEGIN
 SET NOCOUNT ON;
 IF @Take NOT BETWEEN 1 AND 10001 OR @Offset<0 THROW 51000,'Invalid report limits',1;
 IF @Report NOT IN('ledger','trial-balance','profit-loss','balance-sheet','cashbook','bankbook','daybook','income-expense','receivables','payables','assets','depreciation','reconciliation','tax') THROW 51000,'Unknown report',1;
 DECLARE @Rows TABLE(Id bigint,Code nvarchar(200),Name nvarchar(200),GroupCode nvarchar(200),[Date] datetime2,Reference nvarchar(200),Narration nvarchar(1000),Opening decimal(18,2),Debit decimal(18,2),Credit decimal(18,2),Balance decimal(18,2),Bucket nvarchar(200));
 IF @Report IN('ledger','cashbook','bankbook','daybook')
 BEGIN
  ;WITH Activity AS (
   SELECT l.Id,a.Id AccountId,a.Code,a.Name,a.GroupCode,v.VoucherDate [Date],v.VoucherNumber Reference,v.Narration,l.Debit,l.Credit,
    SUM(l.Debit-l.Credit) OVER(PARTITION BY a.Id ORDER BY v.VoucherDate,v.Id,l.LineNumber ROWS UNBOUNDED PRECEDING) Balance,
    SUM(CASE WHEN v.VoucherDate<@From THEN l.Debit-l.Credit ELSE 0 END) OVER(PARTITION BY a.Id) Opening
   FROM dbo.AccountingVoucherLines l JOIN dbo.AccountingVouchers v ON v.Id=l.VoucherId JOIN dbo.LedgerAccounts a ON a.Id=l.AccountId
   WHERE v.FarmHouseId=@FarmHouseId AND a.FarmHouseId=@FarmHouseId AND v.Status='Posted' AND v.VoucherDate<DATEADD(day,1,@To)
    AND (@AccountId IS NULL OR a.Id=@AccountId)
    AND (@Report IN('ledger','daybook') OR (@Report='cashbook' AND (a.Code='1000' OR EXISTS(SELECT 1 FROM dbo.AccountingMoneyAccounts m WHERE m.FarmHouseId=@FarmHouseId AND m.AccountId=a.Id AND m.Kind='Cash')))
     OR (@Report='bankbook' AND (a.Code='1010' OR EXISTS(SELECT 1 FROM dbo.AccountingMoneyAccounts m WHERE m.FarmHouseId=@FarmHouseId AND m.AccountId=a.Id AND m.Kind='Bank'))))
  ) INSERT @Rows SELECT Id,Code,Name,GroupCode,[Date],Reference,Narration,Opening,Debit,Credit,Balance,N'' FROM Activity WHERE [Date]>=@From;
  IF @Report<>'daybook' INSERT @Rows
   SELECT -a.Id,a.Code,a.Name,a.GroupCode,@From,N'OPENING',N'Opening balance',SUM(l.Debit-l.Credit),0,0,SUM(l.Debit-l.Credit),N''
   FROM dbo.LedgerAccounts a JOIN dbo.AccountingVoucherLines l ON l.AccountId=a.Id JOIN dbo.AccountingVouchers v ON v.Id=l.VoucherId
   WHERE a.FarmHouseId=@FarmHouseId AND v.FarmHouseId=@FarmHouseId AND v.Status='Posted' AND v.VoucherDate<@From AND (@AccountId IS NULL OR a.Id=@AccountId)
    AND (@Report='ledger' OR (@Report='cashbook' AND (a.Code='1000' OR EXISTS(SELECT 1 FROM dbo.AccountingMoneyAccounts m WHERE m.FarmHouseId=@FarmHouseId AND m.AccountId=a.Id AND m.Kind='Cash')))
     OR (@Report='bankbook' AND (a.Code='1010' OR EXISTS(SELECT 1 FROM dbo.AccountingMoneyAccounts m WHERE m.FarmHouseId=@FarmHouseId AND m.AccountId=a.Id AND m.Kind='Bank'))))
   GROUP BY a.Id,a.Code,a.Name,a.GroupCode HAVING SUM(l.Debit-l.Credit)<>0;
 END
 ELSE IF @Report IN('trial-balance','profit-loss','balance-sheet','income-expense','tax')
 BEGIN
  IF @Report='tax' AND NOT EXISTS(SELECT 1 FROM dbo.AccountingSettings WHERE FarmHouseId=@FarmHouseId AND GstEnabled=1) THROW 51000,'GST is disabled',1;
  INSERT @Rows SELECT a.Id,a.Code,a.Name,a.GroupCode,@To,N'',N'',SUM(CASE WHEN v.VoucherDate<@From THEN l.Debit-l.Credit ELSE 0 END),
    SUM(CASE WHEN @Report='balance-sheet' OR v.VoucherDate>=@From THEN l.Debit ELSE 0 END),
    SUM(CASE WHEN @Report='balance-sheet' OR v.VoucherDate>=@From THEN l.Credit ELSE 0 END),SUM(l.Debit-l.Credit),N''
   FROM dbo.LedgerAccounts a JOIN dbo.AccountingVoucherLines l ON l.AccountId=a.Id JOIN dbo.AccountingVouchers v ON v.Id=l.VoucherId
   WHERE a.FarmHouseId=@FarmHouseId AND v.FarmHouseId=@FarmHouseId AND v.Status='Posted' AND v.VoucherDate<DATEADD(day,1,@To)
    AND (@Report='trial-balance' OR (@Report IN('profit-loss','income-expense') AND a.GroupCode IN('INCOME','DIRECT_EXP','INDIRECT_EXP') AND v.VoucherDate>=@From)
     OR (@Report='balance-sheet' AND a.GroupCode IN('ASSET','LIABILITY','EQUITY')) OR (@Report='tax' AND a.IsTaxAccount=1))
   GROUP BY a.Id,a.Code,a.Name,a.GroupCode;
  IF @Report='balance-sheet'
   INSERT @Rows SELECT 0,N'CURRENT-RESULT',N'Current accumulated profit / loss',N'EQUITY',@To,N'',N'',0,
    CASE WHEN SUM(l.Debit-l.Credit)>0 THEN SUM(l.Debit-l.Credit) ELSE 0 END,CASE WHEN SUM(l.Credit-l.Debit)>0 THEN SUM(l.Credit-l.Debit) ELSE 0 END,SUM(l.Debit-l.Credit),N''
   FROM dbo.AccountingVoucherLines l JOIN dbo.AccountingVouchers v ON v.Id=l.VoucherId JOIN dbo.LedgerAccounts a ON a.Id=l.AccountId
   WHERE v.FarmHouseId=@FarmHouseId AND a.FarmHouseId=@FarmHouseId AND v.Status='Posted' AND v.VoucherDate<DATEADD(day,1,@To) AND a.GroupCode IN('INCOME','DIRECT_EXP','INDIRECT_EXP') HAVING COUNT(*)>0;
  IF @Report IN('profit-loss','income-expense') INSERT @Rows
   SELECT 0,N'NET-RESULT',N'Net profit / loss',N'RESULT',@To,N'',N'',0,0,0,SUM(Credit-Debit),N'' FROM @Rows HAVING COUNT(*)>0;
 END
 ELSE IF @Report IN('receivables','payables')
 BEGIN
  INSERT @Rows SELECT l.Id,a.Code,a.Name,a.GroupCode,v.VoucherDate,v.VoucherNumber,v.Narration,0,l.Debit,l.Credit,
   CASE WHEN @Report='payables' THEN l.Credit-l.Debit-ISNULL(p.Paid,0) ELSE l.Debit-l.Credit-ISNULL(c.Paid,0) END,
   CASE WHEN DATEDIFF(day,ISNULL(v.DueDate,v.VoucherDate),@To)<=0 THEN N'Not due' WHEN DATEDIFF(day,ISNULL(v.DueDate,v.VoucherDate),@To)<=30 THEN N'1-30' WHEN DATEDIFF(day,ISNULL(v.DueDate,v.VoucherDate),@To)<=60 THEN N'31-60' WHEN DATEDIFF(day,ISNULL(v.DueDate,v.VoucherDate),@To)<=90 THEN N'61-90' ELSE N'90+' END
  FROM dbo.AccountingVoucherLines l JOIN dbo.AccountingVouchers v ON v.Id=l.VoucherId JOIN dbo.LedgerAccounts a ON a.Id=l.AccountId
  OUTER APPLY(SELECT SUM(pl.Debit-pl.Credit) Paid FROM dbo.AccountingVouchers pv JOIN dbo.AccountingVoucherLines pl ON pl.VoucherId=pv.Id WHERE pv.FarmHouseId=@FarmHouseId AND pv.Status='Posted' AND pv.SettlesVoucherId=v.Id AND pv.VoucherDate<DATEADD(day,1,@To) AND pl.AccountId=l.AccountId) p
  OUTER APPLY(SELECT SUM(cl.Credit-cl.Debit) Paid FROM dbo.AccountingVoucherLines cl JOIN dbo.AccountingVouchers cv ON cv.Id=cl.VoucherId WHERE cv.FarmHouseId=@FarmHouseId AND cv.Status='Posted' AND cv.VoucherDate<DATEADD(day,1,@To) AND cl.AccountId=l.AccountId AND cl.CustomerId=l.CustomerId AND cv.SourceReference=v.SourceReference AND (cv.SourceKey LIKE 'Payment:%' OR cv.VoucherType='Receipt')) c
  WHERE v.FarmHouseId=@FarmHouseId AND a.FarmHouseId=@FarmHouseId AND v.Status='Posted' AND v.VoucherDate<DATEADD(day,1,@To)
    AND ((@Report='payables' AND v.VoucherType='VendorBill' AND l.Credit>0 AND a.GroupCode='LIABILITY') OR (@Report='receivables' AND a.Code='1100' AND l.Debit>0));
  DELETE FROM @Rows WHERE Balance<=0;
 END
 ELSE IF @Report IN('assets','depreciation')
 BEGIN
  IF @Report='assets'
   INSERT @Rows SELECT a.Id,a.Code,a.Name,N'ASSET',a.PurchaseDate,a.Status,a.Location,0,a.Cost,ISNULL(d.Amount,0),a.Cost-ISNULL(d.Amount,0),N'' FROM dbo.AccountingFixedAssets a
    OUTER APPLY(SELECT SUM(Amount) Amount FROM dbo.AccountingAssetEvents e WHERE e.FarmHouseId=@FarmHouseId AND e.AssetId=a.Id AND e.Kind='Depreciation' AND e.EventDate<=@To) d WHERE a.FarmHouseId=@FarmHouseId AND a.PurchaseDate<=@To;
  ELSE BEGIN
   INSERT @Rows SELECT e.Id,a.Code,a.Name,N'ASSET',e.EventDate,ISNULL(v.VoucherNumber,N''),e.Notes,0,0,e.Amount,e.Amount,N'Posted' FROM dbo.AccountingAssetEvents e JOIN dbo.AccountingFixedAssets a ON a.Id=e.AssetId LEFT JOIN dbo.AccountingVouchers v ON v.Id=e.VoucherId WHERE e.FarmHouseId=@FarmHouseId AND a.FarmHouseId=@FarmHouseId AND e.Kind='Depreciation' AND e.EventDate BETWEEN @From AND @To;
   ;WITH Months AS (SELECT DATEFROMPARTS(YEAR(@From),MONTH(@From),1) StartDate UNION ALL SELECT DATEADD(month,1,StartDate) FROM Months WHERE DATEADD(month,1,StartDate)<=@To)
   INSERT @Rows SELECT -a.Id,a.Code,a.Name,N'ASSET',EOMONTH(m.StartDate),N'Planned',N'Policy projection; posting remains explicit',0,0,
    ROUND(CASE WHEN a.DepreciationMethod='SLM' THEN (a.Cost-a.ResidualValue)/NULLIF(a.UsefulLifeMonths,0) ELSE CASE WHEN a.Cost*POWER(CONVERT(decimal(18,8),1-a.AnnualRate/1200),DATEDIFF(month,a.InServiceDate,m.StartDate))<=a.ResidualValue THEN 0 WHEN a.Cost*POWER(CONVERT(decimal(18,8),1-a.AnnualRate/1200),DATEDIFF(month,a.InServiceDate,m.StartDate))-a.Cost*POWER(CONVERT(decimal(18,8),1-a.AnnualRate/1200),DATEDIFF(month,a.InServiceDate,m.StartDate))*a.AnnualRate/1200<a.ResidualValue THEN a.Cost*POWER(CONVERT(decimal(18,8),1-a.AnnualRate/1200),DATEDIFF(month,a.InServiceDate,m.StartDate))-a.ResidualValue ELSE a.Cost*POWER(CONVERT(decimal(18,8),1-a.AnnualRate/1200),DATEDIFF(month,a.InServiceDate,m.StartDate))*a.AnnualRate/1200 END END,2),
    ROUND(CASE WHEN a.DepreciationMethod='SLM' THEN (a.Cost-a.ResidualValue)/NULLIF(a.UsefulLifeMonths,0) ELSE CASE WHEN a.Cost*POWER(CONVERT(decimal(18,8),1-a.AnnualRate/1200),DATEDIFF(month,a.InServiceDate,m.StartDate))<=a.ResidualValue THEN 0 WHEN a.Cost*POWER(CONVERT(decimal(18,8),1-a.AnnualRate/1200),DATEDIFF(month,a.InServiceDate,m.StartDate))-a.Cost*POWER(CONVERT(decimal(18,8),1-a.AnnualRate/1200),DATEDIFF(month,a.InServiceDate,m.StartDate))*a.AnnualRate/1200<a.ResidualValue THEN a.Cost*POWER(CONVERT(decimal(18,8),1-a.AnnualRate/1200),DATEDIFF(month,a.InServiceDate,m.StartDate))-a.ResidualValue ELSE a.Cost*POWER(CONVERT(decimal(18,8),1-a.AnnualRate/1200),DATEDIFF(month,a.InServiceDate,m.StartDate))*a.AnnualRate/1200 END END,2),N'Planned'
   FROM dbo.AccountingFixedAssets a CROSS JOIN Months m WHERE a.FarmHouseId=@FarmHouseId AND a.Status='Active' AND EOMONTH(m.StartDate)>=a.InServiceDate AND EOMONTH(m.StartDate)<=@To AND DATEDIFF(month,a.InServiceDate,m.StartDate)<a.UsefulLifeMonths
   AND NOT EXISTS(SELECT 1 FROM dbo.AccountingAssetEvents e WHERE e.FarmHouseId=@FarmHouseId AND e.AssetId=a.Id AND e.Kind='Depreciation' AND e.EventDate=EOMONTH(m.StartDate)) OPTION(MAXRECURSION 32767);
  END
 END
 ELSE IF @Report='reconciliation' 
 BEGIN
  INSERT @Rows SELECT s.Id,a.Code,a.Name,N'BANK',s.StatementDate,s.Reference,N'Bank statement',0,s.Debit,s.Credit,s.Debit-s.Credit,CASE WHEN s.VoucherLineId IS NULL THEN N'Unmatched statement' ELSE N'Matched' END FROM dbo.AccountingBankStatements s JOIN dbo.LedgerAccounts a ON a.Id=s.AccountId WHERE s.FarmHouseId=@FarmHouseId AND a.FarmHouseId=@FarmHouseId AND s.StatementDate BETWEEN @From AND @To AND (@AccountId IS NULL OR s.AccountId=@AccountId);
  INSERT @Rows SELECT l.Id,a.Code,a.Name,N'BANK',v.VoucherDate,v.VoucherNumber,v.Narration,0,l.Debit,l.Credit,l.Debit-l.Credit,N'Unmatched book' FROM dbo.AccountingVoucherLines l JOIN dbo.AccountingVouchers v ON v.Id=l.VoucherId JOIN dbo.LedgerAccounts a ON a.Id=l.AccountId WHERE v.FarmHouseId=@FarmHouseId AND a.FarmHouseId=@FarmHouseId AND v.Status='Posted' AND l.ReconciledDate IS NULL AND v.VoucherDate BETWEEN @From AND @To AND (@AccountId IS NULL OR a.Id=@AccountId) AND (a.Code='1010' OR EXISTS(SELECT 1 FROM dbo.AccountingMoneyAccounts m WHERE m.FarmHouseId=@FarmHouseId AND m.AccountId=a.Id AND m.Kind='Bank'));
 END
 SELECT * FROM @Rows WHERE @Search=N'' OR Name LIKE @Search+N'%' OR Code LIKE @Search+N'%' OR Reference LIKE @Search+N'%' ORDER BY [Date],Code,Id OFFSET @Offset ROWS FETCH NEXT @Take ROWS ONLY;
END
