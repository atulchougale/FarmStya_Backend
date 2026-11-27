CREATE OR ALTER PROCEDURE dbo.usp_Report_Revenue
 @FarmHouseId int, @FromDate datetime2=NULL, @ToDateExclusive datetime2=NULL,
 @Search nvarchar(100)=NULL, @BookingType nvarchar(10)='all', @Source nvarchar(10)='all',
 @Page int=1, @PageSize int=25
AS
BEGIN
 SET NOCOUNT ON;
 IF @FarmHouseId<=0 OR @Page<1 OR @PageSize<1 OR @PageSize>100001 THROW 50030,'Invalid report paging or tenant.',1;
 DECLARE @Offset bigint=(CONVERT(bigint,@Page)-1)*@PageSize;
 DECLARE @Pattern nvarchar(205)=REPLACE(REPLACE(REPLACE(REPLACE(@Search,'~','~~'),'%','~%'),'_','~_'),'[','~[')+'%';
 ;WITH Data AS (SELECT p.PaymentTransactionId,b.BookingReference,b.FullName,COALESCE(p.PaidAt,p.VerifiedAt,p.CreatedDate) ReceivedAt,p.PaymentType,p.PaymentMethod,p.Amount,COALESCE(p.ProviderPaymentId,'') ProviderPaymentId FROM dbo.PaymentTransactions p JOIN dbo.Bookings b ON b.BookingId=p.BookingId AND b.FarmHouseId=p.FarmHouseId WHERE p.FarmHouseId=@FarmHouseId AND p.IsDelete=0 AND p.PaymentStatus='Success' AND p.PaymentType IN('Advance','Checkout') AND b.IsDelete=0 AND (@FromDate IS NULL OR (p.PaidAt>=@FromDate OR (p.PaidAt IS NULL AND COALESCE(p.VerifiedAt,p.CreatedDate)>=@FromDate))) AND (@ToDateExclusive IS NULL OR (p.PaidAt<@ToDateExclusive OR (p.PaidAt IS NULL AND COALESCE(p.VerifiedAt,p.CreatedDate)<@ToDateExclusive))) AND (@Search IS NULL OR b.BookingReference LIKE @Pattern ESCAPE '~' OR b.FullName LIKE @Pattern ESCAPE '~' OR b.MobileNumber LIKE @Pattern ESCAPE '~'))
 SELECT PaymentTransactionId,BookingReference,FullName,ReceivedAt,PaymentType,PaymentMethod,Amount,ProviderPaymentId FROM Data ORDER BY ReceivedAt DESC,PaymentTransactionId DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY OPTION(RECOMPILE);
 ;WITH Data AS (SELECT p.PaymentTransactionId,b.BookingReference,b.FullName,COALESCE(p.PaidAt,p.VerifiedAt,p.CreatedDate) ReceivedAt,p.PaymentType,p.PaymentMethod,p.Amount,COALESCE(p.ProviderPaymentId,'') ProviderPaymentId FROM dbo.PaymentTransactions p JOIN dbo.Bookings b ON b.BookingId=p.BookingId AND b.FarmHouseId=p.FarmHouseId WHERE p.FarmHouseId=@FarmHouseId AND p.IsDelete=0 AND p.PaymentStatus='Success' AND p.PaymentType IN('Advance','Checkout') AND b.IsDelete=0 AND (@FromDate IS NULL OR (p.PaidAt>=@FromDate OR (p.PaidAt IS NULL AND COALESCE(p.VerifiedAt,p.CreatedDate)>=@FromDate))) AND (@ToDateExclusive IS NULL OR (p.PaidAt<@ToDateExclusive OR (p.PaidAt IS NULL AND COALESCE(p.VerifiedAt,p.CreatedDate)<@ToDateExclusive))) AND (@Search IS NULL OR b.BookingReference LIKE @Pattern ESCAPE '~' OR b.FullName LIKE @Pattern ESCAPE '~' OR b.MobileNumber LIKE @Pattern ESCAPE '~'))
 SELECT COUNT_BIG(*) TotalRows FROM Data OPTION(RECOMPILE);
END
