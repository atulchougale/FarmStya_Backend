CREATE OR ALTER PROCEDURE dbo.usp_Occupancy_GetReport
    @FarmHouseId int, @FromDate datetime2 = NULL, @ToDateExclusive datetime2 = NULL,
    @Search nvarchar(100) = NULL,
    @Page int = 1, @PageSize int = 25
AS
BEGIN
    SET NOCOUNT ON;
    IF @FarmHouseId <= 0 THROW 50030, 'A valid farmhouse is required.', 1;
    IF @FromDate >= @ToDateExclusive THROW 50030, 'Invalid report date range.', 1;
    IF @Page < 1 OR @PageSize < 1 OR @PageSize > 100001 THROW 50030, 'Invalid report paging.', 1;
    DECLARE @Offset bigint = (CONVERT(bigint,@Page)-1)*@PageSize;
    DECLARE @Pattern nvarchar(205) = REPLACE(REPLACE(REPLACE(REPLACE(@Search,'~','~~'),'%','~%'),'_','~_'),'[','~[')+'%';
    ;WITH Data AS (
        SELECT s.StayDate,COUNT_BIG(*) BookingCount,SUM(CONVERT(bigint,b.AdultCount)) Adults,SUM(CONVERT(bigint,b.ChildrenCount)) Children,SUM(CONVERT(bigint,b.PersonCount)) Guests
        FROM dbo.BookingStayDates s
        JOIN dbo.Bookings b ON b.BookingId=s.BookingId
          AND b.FarmHouseId=s.FarmHouseId
        WHERE s.FarmHouseId=@FarmHouseId
          AND s.IsDelete=0
          AND b.IsDelete=0
          AND b.Status IN('Confirmed','CheckedIn','CheckedOut')
          AND (@FromDate IS NULL OR s.StayDate>=@FromDate)
          AND (@ToDateExclusive IS NULL OR s.StayDate<@ToDateExclusive)
          AND (@Search IS NULL OR CONVERT(varchar(10),s.StayDate,23) LIKE @Pattern ESCAPE '~')
        GROUP BY s.StayDate
    )
    SELECT StayDate,BookingCount,Adults,Children,Guests
    FROM Data ORDER BY StayDate DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY OPTION (RECOMPILE);
END
