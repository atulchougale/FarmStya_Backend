# Phase 0: source and SP inventory

440 C# files; 23 SQL scripts. Full file/hash inventory: source-inventory.json.

## Controllers and declared routes

### FarmStay\Controllers\AboutUs\AboutUsController.cs

[HttpPost("save-aboutus")]; [HttpGet("get-aboutus")]; [HttpPut("update-aboutus")]; [HttpDelete("delete-aboutUs/{aboutUsId:int}")]

### FarmStay\Controllers\Accounts\AccountsController.cs

[HttpGet("reports/{report}")]; [HttpGet("reports/{report}/export/{format}")]; [HttpGet("vouchers/{id:long}")]; [HttpGet("{area}")]; [HttpPost("masters")]; [HttpPut("masters/{id:long}")]; [HttpPost("ledgers")]; [HttpPut("ledgers/{id:long}")]; [HttpPost("vendors")]; [HttpPut("vendors/{id:long}")]; [HttpPost("money-accounts")]; [HttpPut("money-accounts/{id:long}")]; [HttpPut("settings")]; [HttpPut("periods/{id:long}")]; [HttpPost("vouchers")]; [HttpPut("vouchers/{id:long}")]; [HttpPost("vouchers/{id:long}/post")]; [HttpPost("vouchers/{id:long}/reverse")]; [HttpPost("assets")]; [HttpPost("assets/{id:long}/{action}")]; [HttpPost("recurring")]; [HttpPut("recurring/{id:long}")]; [HttpPost("statements")]; [HttpPost("statements/{id:long}/reconcile")]; [HttpPost("vouchers/{id:long}/attachments")]; [HttpGet("attachments/{id:long}")]

### FarmStay\Controllers\Administration\AdminMenuController .cs

[Route("api/admin/menu")]

### FarmStay\Controllers\Amenity\AmenityController.cs

[HttpPost("save-amenity")]; [HttpGet("get-byid-amenity/{imageId:int}")]; [HttpGet("get-all-amenity")]; [HttpPut("update-amenity")]; [HttpDelete("delete-amenity/{imageId:int}")]

### FarmStay\Controllers\Auth\AuthController.cs

[HttpPost("register")]; [HttpGet("verify-email")]; [HttpPost("verify-otp")]; [HttpPost("login")]; [HttpPost("forgot-password")]; [HttpPost("reset-password-email")]; [HttpPost("reset-password-otp")]; [HttpPost("change-password")]; [HttpGet("profile")]; [HttpPut("profile/email")]; [HttpPost("profile/contact/send-code")]; [HttpPost("profile/contact/verify-code")]; [HttpPut("profile")]; [HttpGet("verification-status")]; [HttpPost("refresh-token")]; [HttpPost("logout")]; [HttpPost("resend-otp")]; [HttpPost("send-login-otp")]; [HttpPost("verify-login-otp")]

### FarmStay\Controllers\Booking\AdminBookingsController.cs

[Route("api/administration/bookings")]; [HttpGet("customers")]; [HttpPost("customers")]; [HttpPost("customers/{userId:int}/verify-mobile")]; [HttpPost("customers/{userId:int}/resend-verification")]; [HttpPost("reserve")]; [HttpDelete("reserve")]; [HttpGet("reserve/active")]; [HttpPost("quote")]

### FarmStay\Controllers\Booking\BookingCalendarController.cs

[Route("api/booking-calendar")]; [HttpGet("availability")]; [HttpGet("month")]; [HttpPost("reserve")]; [HttpDelete("reserve")]; [HttpGet("reserve/active")]

### FarmStay\Controllers\Booking\BookingListController.cs

[Route("api/booking")]; [HttpGet("{bookingId:long}/receipts")]; [HttpGet("my-bookings/{bookingId:long}/receipts")]; [HttpGet("{bookingId:long}/receipts/{receiptId:long}/pdf")]; [HttpGet("my-bookings/{bookingId:long}/receipts/{receiptId:long}/pdf")]; [HttpPost("{bookingId:long}/check-in")]; [HttpGet("{bookingId:long}/checkout")]; [HttpPost("{bookingId:long}/checkout")]; [HttpGet("my-bookings/{bookingId:long}/checkout")]; [HttpPost("my-bookings/{bookingId:long}/checkout-order")]; [HttpPost("my-bookings/{bookingId:long}/verify-checkout")]; [HttpGet("administration-list")]; [HttpGet("list")]; [HttpGet("latest")]; [HttpGet("{bookingId:long}")]; [HttpGet("my-bookings")]; [HttpGet("my-bookings/{bookingId:long}")]; [HttpPost("my-bookings/{bookingId:long}/advance-order")]; [HttpPost("my-bookings/{bookingId:long}/verify-advance")]; [HttpGet("my-bookings/{bookingId:long}/id-proof")]; [HttpGet("my-bookings/{bookingId:long}/id-proof/{documentId:long}/content")]; [HttpPost("my-bookings/{bookingId:long}/id-proof")]; [HttpPost("{bookingId:long}/id-proof")]; [HttpPost("{bookingId:long}/advance-cash")]; [HttpPost("{bookingId:long}/advance-reminder")]; [HttpGet("{bookingId:long}/id-proof")]; [HttpGet("{bookingId:long}/id-proof/{documentId:long}/content")]; [HttpPost("{bookingId:long}/id-proof/{documentId:long}/review")]; [HttpPost("{bookingId:long}/approve")]; [HttpPost("{bookingId:long}/reject")]

### FarmStay\Controllers\Booking\BookingPricingController.cs

[Route("api/booking-pricing")]; [HttpPost("quote")]

### FarmStay\Controllers\Booking\BookingsController.cs

[Route("api/bookings")]; [HttpGet("mine")]

### FarmStay\Controllers\Contact\ContactController.cs

[HttpPost("save-contact")]; [HttpGet("get-contact")]; [HttpDelete("delete-contact/{contactId:int}")]

### FarmStay\Controllers\Dashboard\DashboardController.cs

[Route("api/administration/dashboard")]; [HttpGet("calendar")]

### FarmStay\Controllers\FarmHouse\FarmHouseController.cs

[HttpPost("save-farmhouse")]; [HttpPut("update-farmhouse")]; [HttpDelete("delete-farmhouse/{id:int}")]; [HttpGet("get-byid-farmhouse/{id:int}")]; [HttpGet("current")]; [HttpGet("get-all-farmhouse")]

### FarmStay\Controllers\Feedback\FeedbackController.cs

[HttpPost("save-feedback")]; [HttpPost("booking/{bookingId:long}")]; [HttpGet("get-byid-feedback/{feedbackId:int}")]; [HttpGet("get-all-feedback")]; [HttpDelete("delete-feedback/{feedbackId:int}")]

### FarmStay\Controllers\FileUpload\FileUploadController.cs

[HttpPost("upload")]

### FarmStay\Controllers\Gallery\GalleryController.cs

[HttpPost("savegallery")]; [HttpGet("get-byid-gallery/{imageId:int}")]; [HttpGet("get-all-gallery")]; [HttpPut("update-gallery")]; [HttpDelete("delete-gallery/{imageId:int}")]; [HttpGet("gallery-categories")]

### FarmStay\Controllers\Pricing\PricingManagementController.cs

[Route("api/administration/website-settings/pricing")]; [HttpGet("entries")]; [HttpGet("entries/{pricingId:int}")]; [HttpPost("entries")]; [HttpPut("entries/{pricingId:int}")]; [HttpDelete("entries/{pricingId:int}")]

### FarmStay\Controllers\Public\PublicController.cs

[Route("api/public")]; [HttpGet("site")]

### FarmStay\Controllers\Public\PublicPageController.cs

[HttpGet("amenities")]; [HttpGet("carousel")]; [HttpGet("top-feedback")]; [HttpGet("gallery")]; [HttpGet("aboutus")]; [HttpGet("contactInfo")]

### FarmStay\Controllers\Reporting\ReportsController.cs

[Route("api/administration/reports")]; [HttpGet("{report}")]; [HttpGet("{report}/export/{format}")]

### FarmStay\Controllers\WhatsApp\WhatsAppWebhookController.cs



## SQL scripts and source callers

- `dbo.usp_Accounts_Report` — `FarmStay.Infrastructure\SQL\Accounts\AccountsReport.sql`; references: FarmStay.Infrastructure\Migrations\20261124183817_AddAccountsFinanceModule.cs, FarmStay.Infrastructure\Repositories\Accounts\AccountsRepository.Reports.cs
- `dbo.usp_Report_Bookings` — `FarmStay.Infrastructure\SQL\Reports\Bookings.sql`; references: No literal C# reference found; dynamic/resource callers need manual verification.
- `dbo.usp_Report_Customers` — `FarmStay.Infrastructure\SQL\Reports\Customers.sql`; references: No literal C# reference found; dynamic/resource callers need manual verification.
- `dbo.usp_Report_Occupancy` — `FarmStay.Infrastructure\SQL\Reports\Occupancy.sql`; references: No literal C# reference found; dynamic/resource callers need manual verification.
- `dbo.usp_Report_Revenue` — `FarmStay.Infrastructure\SQL\Reports\Revenue.sql`; references: No literal C# reference found; dynamic/resource callers need manual verification.
- `dbo.usp_Dashboard_GetBookingSources` — `FarmStay.Infrastructure\SQL\Reports\Dashboard\BookingSources.sql`; references: FarmStay.Infrastructure\Migrations\20261122172524_AddDashboardRevenueAndSourceCharts.cs, FarmStay.Infrastructure\Repositories\Dashboard\DashboardRepository.cs
- `dbo.usp_Dashboard_GetCalendar` — `FarmStay.Infrastructure\SQL\Reports\Dashboard\Calendar.sql`; references: FarmStay.Infrastructure\Migrations\20261120145755_AddDynamicAdministrationDashboard.cs, FarmStay.Infrastructure\Repositories\Dashboard\DashboardRepository.cs
- `dbo.usp_Dashboard_GetRevenueTrend` — `FarmStay.Infrastructure\SQL\Reports\Dashboard\RevenueTrend.sql`; references: FarmStay.Infrastructure\Migrations\20261122172524_AddDashboardRevenueAndSourceCharts.cs, FarmStay.Infrastructure\Repositories\Dashboard\DashboardRepository.cs
- `dbo.usp_Dashboard_GetSummary` — `FarmStay.Infrastructure\SQL\Reports\Dashboard\Summary.sql`; references: FarmStay.Infrastructure\Migrations\20261120145755_AddDynamicAdministrationDashboard.cs, FarmStay.Infrastructure\Repositories\Dashboard\DashboardRepository.cs
- `dbo.usp_Booking_GetReport` — `FarmStay.Infrastructure\SQL\Reports\Standard\Booking_GetReport.sql`; references: FarmStay.Infrastructure\Repositories\Reports\Booking\BookingReportRepository.cs
- `dbo.usp_Booking_GetSummary` — `FarmStay.Infrastructure\SQL\Reports\Standard\Booking_GetSummary.sql`; references: FarmStay.Infrastructure\Repositories\Reports\Booking\BookingReportRepository.cs
- `dbo.usp_Customer_GetReport` — `FarmStay.Infrastructure\SQL\Reports\Standard\Customer_GetReport.sql`; references: FarmStay.Infrastructure\Repositories\Reports\Customer\CustomerReportRepository.cs
- `dbo.usp_Customer_GetSummary` — `FarmStay.Infrastructure\SQL\Reports\Standard\Customer_GetSummary.sql`; references: FarmStay.Infrastructure\Repositories\Reports\Customer\CustomerReportRepository.cs
- `dbo.usp_Occupancy_GetReport` — `FarmStay.Infrastructure\SQL\Reports\Standard\Occupancy_GetReport.sql`; references: FarmStay.Infrastructure\Repositories\Reports\Occupancy\OccupancyReportRepository.cs
- `dbo.usp_Occupancy_GetSummary` — `FarmStay.Infrastructure\SQL\Reports\Standard\Occupancy_GetSummary.sql`; references: FarmStay.Infrastructure\Repositories\Reports\Occupancy\OccupancyReportRepository.cs
- `dbo.usp_Revenue_GetReport` — `FarmStay.Infrastructure\SQL\Reports\Standard\Revenue_GetReport.sql`; references: FarmStay.Infrastructure\Repositories\Reports\Revenue\RevenueReportRepository.cs
- `dbo.usp_Revenue_GetSummary` — `FarmStay.Infrastructure\SQL\Reports\Standard\Revenue_GetSummary.sql`; references: FarmStay.Infrastructure\Repositories\Reports\Revenue\RevenueReportRepository.cs