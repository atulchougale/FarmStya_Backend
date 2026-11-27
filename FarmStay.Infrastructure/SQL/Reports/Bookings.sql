CREATE OR ALTER PROCEDURE dbo.usp_Report_Bookings
 @FarmHouseId int, @FromDate datetime2=NULL, @ToDateExclusive datetime2=NULL,
 @Search nvarchar(100)=NULL, @BookingType nvarchar(10)='all', @Source nvarchar(10)='all',
 @Page int=1, @PageSize int=25
AS
BEGIN
 SET NOCOUNT ON;
 IF @FarmHouseId<=0 OR @Page<1 OR @PageSize<1 OR @PageSize>100001 THROW 50030,'Invalid report paging or tenant.',1;
 DECLARE @Offset bigint=(CONVERT(bigint,@Page)-1)*@PageSize;
 DECLARE @Pattern nvarchar(205)=REPLACE(REPLACE(REPLACE(REPLACE(@Search,'~','~~'),'%','~%'),'_','~_'),'[','~[')+'%';
 ;WITH Data AS (SELECT b.BookingId,b.BookingReference,b.FullName,b.MobileNumber,b.CreatedDate,b.CheckInDate,b.CheckOutDate,b.BookingType,b.BookingSource,b.PersonCount,b.Status,b.TotalAmount FROM dbo.Bookings b WHERE b.FarmHouseId=@FarmHouseId AND b.IsDelete=0 AND (@FromDate IS NULL OR b.CreatedDate>=@FromDate) AND (@ToDateExclusive IS NULL OR b.CreatedDate<@ToDateExclusive) AND (@Search IS NULL OR b.BookingReference LIKE @Pattern ESCAPE '~' OR b.FullName LIKE @Pattern ESCAPE '~' OR b.MobileNumber LIKE @Pattern ESCAPE '~') AND (@BookingType='all' OR (@BookingType='normal' AND b.BookingType='BookingOnly') OR (@BookingType='event' AND b.BookingType IN('NormalEvent','DecorationEvent'))) AND (@Source='all' OR (@Source='online' AND b.BookingSource='Online') OR (@Source='assisted' AND b.BookingSource IN('WalkIn','Phone'))))
 SELECT BookingReference,FullName,MobileNumber,CreatedDate,CheckInDate,CheckOutDate,BookingType,BookingSource,PersonCount,Status,TotalAmount FROM Data ORDER BY CreatedDate DESC,BookingId DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY OPTION(RECOMPILE);
 ;WITH Data AS (SELECT b.BookingId,b.BookingReference,b.FullName,b.MobileNumber,b.CreatedDate,b.CheckInDate,b.CheckOutDate,b.BookingType,b.BookingSource,b.PersonCount,b.Status,b.TotalAmount FROM dbo.Bookings b WHERE b.FarmHouseId=@FarmHouseId AND b.IsDelete=0 AND (@FromDate IS NULL OR b.CreatedDate>=@FromDate) AND (@ToDateExclusive IS NULL OR b.CreatedDate<@ToDateExclusive) AND (@Search IS NULL OR b.BookingReference LIKE @Pattern ESCAPE '~' OR b.FullName LIKE @Pattern ESCAPE '~' OR b.MobileNumber LIKE @Pattern ESCAPE '~') AND (@BookingType='all' OR (@BookingType='normal' AND b.BookingType='BookingOnly') OR (@BookingType='event' AND b.BookingType IN('NormalEvent','DecorationEvent'))) AND (@Source='all' OR (@Source='online' AND b.BookingSource='Online') OR (@Source='assisted' AND b.BookingSource IN('WalkIn','Phone'))))
 SELECT COUNT_BIG(*) TotalRows FROM Data OPTION(RECOMPILE);
END
