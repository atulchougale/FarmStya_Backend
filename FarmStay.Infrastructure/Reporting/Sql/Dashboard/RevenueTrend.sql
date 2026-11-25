CREATE OR ALTER PROCEDURE dbo.usp_Dashboard_GetRevenueTrend
 @FarmHouseId int, @Today date
AS
BEGIN
 SET NOCOUNT ON;
 IF @FarmHouseId<=0 OR @Today IS NULL THROW 50040,'Invalid dashboard scope.',1;
 DECLARE @Month date=DATEFROMPARTS(YEAR(@Today),MONTH(@Today),1);
 DECLARE @From date=DATEADD(month,-5,@Month), @To date=DATEADD(month,1,@Month);
 ;WITH Months AS (
  SELECT DATEADD(month,v.MonthOffset,@Month) MonthStart FROM (VALUES(-5),(-4),(-3),(-2),(-1),(0)) v(MonthOffset)
 ), Received AS (
  SELECT DATEFROMPARTS(YEAR(COALESCE(p.PaidAt,p.VerifiedAt,p.CreatedDate)),MONTH(COALESCE(p.PaidAt,p.VerifiedAt,p.CreatedDate)),1) MonthStart,
   SUM(p.Amount) Amount
  FROM dbo.PaymentTransactions p JOIN dbo.Bookings b ON b.BookingId=p.BookingId AND b.FarmHouseId=p.FarmHouseId
  WHERE p.FarmHouseId=@FarmHouseId AND p.IsDelete=0 AND b.IsDelete=0 AND p.PaymentStatus='Success' AND p.PaymentType IN('Advance','Checkout')
   AND ((p.PaidAt>=@From AND p.PaidAt<@To) OR (p.PaidAt IS NULL AND COALESCE(p.VerifiedAt,p.CreatedDate)>=@From AND COALESCE(p.VerifiedAt,p.CreatedDate)<@To))
  GROUP BY DATEFROMPARTS(YEAR(COALESCE(p.PaidAt,p.VerifiedAt,p.CreatedDate)),MONTH(COALESCE(p.PaidAt,p.VerifiedAt,p.CreatedDate)),1)
 )
 SELECT CONVERT(datetime2,m.MonthStart) MonthStart, CAST(COALESCE(r.Amount,0) AS decimal(38,2)) Amount
 FROM Months m LEFT JOIN Received r ON r.MonthStart=m.MonthStart ORDER BY m.MonthStart OPTION(RECOMPILE);
END
