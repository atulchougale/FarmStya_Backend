CREATE OR ALTER PROCEDURE dbo.usp_Dashboard_GetBookingSources
 @FarmHouseId int
AS
BEGIN
 SET NOCOUNT ON;
 IF @FarmHouseId<=0 THROW 50040,'Invalid dashboard scope.',1;
 ;WITH Counts AS (
  SELECT CASE WHEN BookingSource IN('Online','Phone','WalkIn') THEN BookingSource ELSE 'Other' END Source,COUNT_BIG(*) BookingCount
  FROM dbo.Bookings WHERE FarmHouseId=@FarmHouseId AND IsDelete=0
  GROUP BY CASE WHEN BookingSource IN('Online','Phone','WalkIn') THEN BookingSource ELSE 'Other' END
 ), Channels AS (
  SELECT v.Source,v.SortOrder FROM (VALUES('Online',1),('Phone',2),('WalkIn',3),('Other',4)) v(Source,SortOrder)
 )
 SELECT c.Source,COALESCE(n.BookingCount,CONVERT(bigint,0)) BookingCount
 FROM Channels c LEFT JOIN Counts n ON n.Source=c.Source
 WHERE c.Source<>'Other' OR n.BookingCount>0 ORDER BY c.SortOrder OPTION(RECOMPILE);
END
