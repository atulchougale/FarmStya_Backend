CREATE OR ALTER PROCEDURE dbo.usp_Occupancy_GetSummary
    @FarmHouseId int, @FromDate datetime2 = NULL, @ToDateExclusive datetime2 = NULL,
    @Search nvarchar(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @FarmHouseId <= 0 THROW 50030, 'A valid farmhouse is required.', 1;
    IF @FromDate >= @ToDateExclusive THROW 50030, 'Invalid report date range.', 1;
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
    SELECT COUNT_BIG(*) AS TotalRecords FROM Data OPTION (RECOMPILE);
END
