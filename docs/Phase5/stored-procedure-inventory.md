# Stored procedure source inventory — Phase 5

Static exact-name caller trace; SQL wrapper/SP-to-SP calls are included separately. No procedure is removed solely because a runtime C# literal is absent.

| Procedure | C# callers | SQL references |
|---|---|---|
| usp_Accounts_Report | FarmStay.Infrastructure/Repositories/Accounts/AccountsRepository.Reports.cs | FarmStay.Infrastructure/SQL/Accounts/AccountsReport.sql |
| usp_Booking_GetReport | FarmStay.Infrastructure/Repositories/Reports/Booking/BookingReportRepository.cs | FarmStay.Infrastructure/SQL/Reports/Standard/Booking_GetReport.sql |
| usp_Booking_GetSummary | FarmStay.Infrastructure/Repositories/Reports/Booking/BookingReportRepository.cs | FarmStay.Infrastructure/SQL/Reports/Standard/Booking_GetSummary.sql |
| usp_Customer_GetReport | FarmStay.Infrastructure/Repositories/Reports/Customer/CustomerReportRepository.cs | FarmStay.Infrastructure/SQL/Reports/Standard/Customer_GetReport.sql |
| usp_Customer_GetSummary | FarmStay.Infrastructure/Repositories/Reports/Customer/CustomerReportRepository.cs | FarmStay.Infrastructure/SQL/Reports/Standard/Customer_GetSummary.sql |
| usp_Dashboard_GetBookingSources | FarmStay.Infrastructure/Repositories/Dashboard/DashboardRepository.cs | FarmStay.Infrastructure/SQL/Reports/Dashboard/BookingSources.sql |
| usp_Dashboard_GetCalendar | FarmStay.Infrastructure/Repositories/Dashboard/DashboardRepository.cs | FarmStay.Infrastructure/SQL/Reports/Dashboard/Calendar.sql |
| usp_Dashboard_GetRevenueTrend | FarmStay.Infrastructure/Repositories/Dashboard/DashboardRepository.cs | FarmStay.Infrastructure/SQL/Reports/Dashboard/RevenueTrend.sql |
| usp_Dashboard_GetSummary | FarmStay.Infrastructure/Repositories/Dashboard/DashboardRepository.cs | FarmStay.Infrastructure/SQL/Reports/Dashboard/Summary.sql |
| usp_Occupancy_GetReport | FarmStay.Infrastructure/Repositories/Reports/Occupancy/OccupancyReportRepository.cs | FarmStay.Infrastructure/SQL/Reports/Standard/Occupancy_GetReport.sql |
| usp_Occupancy_GetSummary | FarmStay.Infrastructure/Repositories/Reports/Occupancy/OccupancyReportRepository.cs | FarmStay.Infrastructure/SQL/Reports/Standard/Occupancy_GetSummary.sql |
| usp_Report_BookingPaymentReceipt | FarmStay.Infrastructure/Services/Reporting/BookingReceipts.cs | FarmStay.Infrastructure/SQL/Accounts/Phase4/Receipt.sql; FarmStay.Infrastructure/SQL/Accounts/Phase4/ReceiptBefore.sql |
| usp_Report_Bookings | No literal caller; retain pending dynamic/wrapper verification | FarmStay.Infrastructure/SQL/Reports/Bookings.sql |
| usp_Report_Customers | No literal caller; retain pending dynamic/wrapper verification | FarmStay.Infrastructure/SQL/Reports/Customers.sql |
| usp_Report_Occupancy | No literal caller; retain pending dynamic/wrapper verification | FarmStay.Infrastructure/SQL/Reports/Occupancy.sql |
| usp_Report_Revenue | No literal caller; retain pending dynamic/wrapper verification | FarmStay.Infrastructure/SQL/Reports/Revenue.sql |
| usp_Revenue_GetReport | FarmStay.Infrastructure/Repositories/Reports/Revenue/RevenueReportRepository.cs | FarmStay.Infrastructure/SQL/Reports/Standard/Revenue_GetReport.sql |
| usp_Revenue_GetSummary | FarmStay.Infrastructure/Repositories/Reports/Revenue/RevenueReportRepository.cs | FarmStay.Infrastructure/SQL/Reports/Standard/Revenue_GetSummary.sql |
