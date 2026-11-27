CREATE OR ALTER PROCEDURE dbo.usp_Revenue_GetSummary
    @FarmHouseId int, @FromDate datetime2 = NULL, @ToDateExclusive datetime2 = NULL,
    @Search nvarchar(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @FarmHouseId <= 0 THROW 50030, 'A valid farmhouse is required.', 1;
    IF @FromDate >= @ToDateExclusive THROW 50030, 'Invalid report date range.', 1;
    DECLARE @Pattern nvarchar(205) = REPLACE(REPLACE(REPLACE(REPLACE(@Search,'~','~~'),'%','~%'),'_','~_'),'[','~[')+'%';
    ;WITH Data AS (
        SELECT p.PaymentTransactionId,b.BookingReference,b.FullName,COALESCE(p.PaidAt,p.VerifiedAt,p.CreatedDate) ReceivedAt,p.PaymentType,p.PaymentMethod,CAST(p.Amount AS decimal(18,2)) AS Amount,COALESCE(p.ProviderPaymentId,'') ProviderPaymentId
        FROM dbo.PaymentTransactions p
        JOIN dbo.Bookings b ON b.BookingId=p.BookingId
          AND b.FarmHouseId=p.FarmHouseId
        WHERE p.FarmHouseId=@FarmHouseId
          AND p.IsDelete=0
          AND p.PaymentStatus='Success'
          AND p.PaymentType IN('Advance','Checkout')
          AND b.IsDelete=0
          AND (@FromDate IS NULL OR (p.PaidAt>=@FromDate OR (p.PaidAt IS NULL
          AND COALESCE(p.VerifiedAt,p.CreatedDate)>=@FromDate)))
          AND (@ToDateExclusive IS NULL OR (p.PaidAt<@ToDateExclusive OR (p.PaidAt IS NULL
          AND COALESCE(p.VerifiedAt,p.CreatedDate)<@ToDateExclusive)))
          AND (@Search IS NULL OR b.BookingReference LIKE @Pattern ESCAPE '~' OR b.FullName LIKE @Pattern ESCAPE '~' OR b.MobileNumber LIKE @Pattern ESCAPE '~')
    )
    SELECT COUNT_BIG(*) AS TotalRecords FROM Data OPTION (RECOMPILE);
END
