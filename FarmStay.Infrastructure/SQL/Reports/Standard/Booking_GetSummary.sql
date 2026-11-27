-- Booking report: submission dates; tenant-scoped; monetary values use decimal(18,2).
CREATE OR ALTER PROCEDURE dbo.usp_Booking_GetSummary
    @FarmHouseId int, @FromDate datetime2 = NULL, @ToDateExclusive datetime2 = NULL,
    @Search nvarchar(100) = NULL, @BookingType nvarchar(10) = 'all', @Source nvarchar(10) = 'all'
AS
BEGIN
    SET NOCOUNT ON;
    IF @FarmHouseId <= 0 THROW 50030, 'A valid farmhouse is required.', 1;
    IF @FromDate >= @ToDateExclusive THROW 50030, 'Invalid report date range.', 1;
    IF @BookingType NOT IN ('all','normal','event') OR @Source NOT IN ('all','online','assisted')
        THROW 50030, 'Invalid booking filters.', 1;
    DECLARE @Pattern nvarchar(205) = REPLACE(REPLACE(REPLACE(REPLACE(@Search,'~','~~'),'%','~%'),'_','~_'),'[','~[')+'%';
    ;WITH Data AS (
        SELECT b.BookingId,b.BookingReference,b.FullName,b.MobileNumber,b.CreatedDate,b.CheckInDate,b.CheckOutDate,b.BookingType,b.BookingSource,b.PersonCount,b.Status,CAST(b.TotalAmount AS decimal(18,2)) AS TotalAmount
        FROM dbo.Bookings b
        WHERE b.FarmHouseId=@FarmHouseId
          AND b.IsDelete=0
          AND (@FromDate IS NULL OR b.CreatedDate>=@FromDate)
          AND (@ToDateExclusive IS NULL OR b.CreatedDate<@ToDateExclusive)
          AND (@Search IS NULL OR b.BookingReference LIKE @Pattern ESCAPE '~' OR b.FullName LIKE @Pattern ESCAPE '~' OR b.MobileNumber LIKE @Pattern ESCAPE '~')
          AND (@BookingType='all' OR (@BookingType='normal'
          AND b.BookingType='BookingOnly') OR (@BookingType='event'
          AND b.BookingType IN('NormalEvent','DecorationEvent')))
          AND (@Source='all' OR (@Source='online'
          AND b.BookingSource='Online') OR (@Source='assisted'
          AND b.BookingSource IN('WalkIn','Phone')))
    )
    SELECT COUNT_BIG(*) AS TotalRecords FROM Data OPTION (RECOMPILE);
END
