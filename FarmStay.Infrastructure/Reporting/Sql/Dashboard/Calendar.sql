CREATE OR ALTER PROCEDURE dbo.usp_Dashboard_GetCalendar
 @FarmHouseId int,@FromDate date,@ToDateExclusive date
AS
BEGIN
 SET NOCOUNT ON;
 IF @FarmHouseId<=0 OR @FromDate IS NULL OR @ToDateExclusive IS NULL OR @FromDate>=@ToDateExclusive OR DATEDIFF(day,@FromDate,@ToDateExclusive)>31
  THROW 50040,'Invalid dashboard calendar range.',1;
 SELECT s.StayDate,b.BookingId,b.BookingReference,b.FullName,b.Status
 FROM dbo.BookingStayDates s JOIN dbo.Bookings b ON b.BookingId=s.BookingId AND b.FarmHouseId=s.FarmHouseId
 WHERE s.FarmHouseId=@FarmHouseId AND s.IsDelete=0 AND b.IsDelete=0 AND s.StayDate>=@FromDate AND s.StayDate<@ToDateExclusive
  AND b.Status IN('Pending','Approved','Confirmed','CheckedIn','CheckedOut')
 ORDER BY s.StayDate,b.BookingId OPTION(RECOMPILE);
END
