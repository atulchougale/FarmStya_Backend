CREATE OR ALTER PROCEDURE dbo.usp_Customer_GetReport
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
        SELECT b.CustomerUserId,u.FullName,u.MobileNumber,COALESCE(u.Email,'') Email,u.CreatedDate RegisteredAt,COUNT_BIG(*) BookingCount,MAX(b.CreatedDate) LastBookingAt,CAST(SUM(b.TotalAmount) AS decimal(38,2)) AS BookingValue
        FROM dbo.Bookings b
        JOIN dbo.Users u ON u.UserId=b.CustomerUserId
          AND u.FarmHouseId=b.FarmHouseId
        WHERE b.FarmHouseId=@FarmHouseId
          AND b.IsDelete=0
          AND (@FromDate IS NULL OR b.CreatedDate>=@FromDate)
          AND (@ToDateExclusive IS NULL OR b.CreatedDate<@ToDateExclusive)
          AND (@Search IS NULL OR u.FullName LIKE @Pattern ESCAPE '~' OR u.MobileNumber LIKE @Pattern ESCAPE '~' OR u.Email LIKE @Pattern ESCAPE '~')
        GROUP BY b.CustomerUserId,u.FullName,u.MobileNumber,u.Email,u.CreatedDate
    )
    SELECT CustomerUserId,FullName,MobileNumber,Email,RegisteredAt,BookingCount,LastBookingAt,BookingValue
    FROM Data ORDER BY LastBookingAt DESC,CustomerUserId DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY OPTION (RECOMPILE);
END
