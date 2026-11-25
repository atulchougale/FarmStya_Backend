CREATE OR ALTER PROCEDURE dbo.usp_Dashboard_GetSummary
 @FarmHouseId int, @Today date
AS
BEGIN
 SET NOCOUNT ON;
 IF @FarmHouseId<=0 OR @Today IS NULL THROW 50040,'Invalid dashboard scope.',1;
 DECLARE @MonthStart date=DATEFROMPARTS(YEAR(@Today),MONTH(@Today),1);
 DECLARE @MonthEnd date=DATEADD(month,1,@MonthStart);
 DECLARE @WeekStart date=DATEADD(day,-(DATEDIFF(day,CONVERT(date,'19000101'),@Today)%7),@Today);
 DECLARE @WeekEnd date=DATEADD(day,7,@WeekStart);
 DECLARE @First date=CASE WHEN @WeekStart<@MonthStart THEN @WeekStart ELSE @MonthStart END;
 DECLARE @Last date=CASE WHEN @WeekEnd>@MonthEnd THEN @WeekEnd ELSE @MonthEnd END;
 ;WITH Submitted AS (
   SELECT COALESCE(SUM(CASE WHEN CreatedDate>=@WeekStart AND CreatedDate<@WeekEnd THEN CONVERT(bigint,1) ELSE 0 END),0) WeekCount,
     COALESCE(SUM(CASE WHEN CreatedDate>=@MonthStart AND CreatedDate<@MonthEnd THEN CONVERT(bigint,1) ELSE 0 END),0) MonthCount
   FROM dbo.Bookings WHERE FarmHouseId=@FarmHouseId AND IsDelete=0 AND CreatedDate>=@First AND CreatedDate<@Last
 ), Received AS (
   SELECT COALESCE(SUM(p.Amount),0) Total,
    COALESCE(SUM(CASE WHEN COALESCE(p.PaidAt,p.VerifiedAt,p.CreatedDate)>=@MonthStart AND COALESCE(p.PaidAt,p.VerifiedAt,p.CreatedDate)<@MonthEnd THEN p.Amount ELSE 0 END),0) ThisMonth
   FROM dbo.PaymentTransactions p JOIN dbo.Bookings b ON b.BookingId=p.BookingId AND b.FarmHouseId=p.FarmHouseId
   WHERE p.FarmHouseId=@FarmHouseId AND p.IsDelete=0 AND b.IsDelete=0 AND p.PaymentStatus='Success' AND p.PaymentType IN('Advance','Checkout')
 ), Occupied AS (
   SELECT COUNT(DISTINCT s.StayDate) Days FROM dbo.BookingStayDates s
   JOIN dbo.Bookings b ON b.BookingId=s.BookingId AND b.FarmHouseId=s.FarmHouseId
   WHERE s.FarmHouseId=@FarmHouseId AND s.IsDelete=0 AND b.IsDelete=0
    AND s.StayDate>=@MonthStart AND s.StayDate<@MonthEnd AND b.Status IN('Confirmed','CheckedIn','CheckedOut')
 )
 SELECT CONVERT(datetime2,@Today) ServerToday, WeekCount BookingsThisWeek, MonthCount BookingsThisMonth,
   CAST(Total AS decimal(38,2)) TotalRevenue,CAST(ThisMonth AS decimal(38,2)) RevenueThisMonth,
   Days OccupiedDays, DAY(EOMONTH(@Today)) DaysInMonth,
   CAST(Days*100.0/DAY(EOMONTH(@Today)) AS decimal(5,2)) OccupancyPercent
 FROM Submitted CROSS JOIN Received CROSS JOIN Occupied OPTION(RECOMPILE);
END
