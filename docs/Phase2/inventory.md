# Reporting stored procedure and report inventory

Runtime database calls remain parameterized EF Core `FromSqlInterpolated`. Canonical report datasets use keyless DTO mappings; receipt rendering no longer creates ADO.NET commands.

| Procedure | Runtime caller | Decision |
|---|---|---|
| `dbo.usp_Accounts_Report` | FarmStay.Infrastructure/Repositories/Accounts/AccountsRepository.Reports.cs | Canonical runtime procedure; retained |
| `dbo.usp_Report_Bookings` | No literal runtime C# caller found | Legacy compatibility candidate; retained pending external-caller/parity proof |
| `dbo.usp_Report_Customers` | No literal runtime C# caller found | Legacy compatibility candidate; retained pending external-caller/parity proof |
| `dbo.usp_Dashboard_GetBookingSources` | FarmStay.Infrastructure/Repositories/Dashboard/DashboardRepository.cs | Canonical runtime procedure; retained |
| `dbo.usp_Dashboard_GetCalendar` | FarmStay.Infrastructure/Repositories/Dashboard/DashboardRepository.cs | Canonical runtime procedure; retained |
| `dbo.usp_Dashboard_GetRevenueTrend` | FarmStay.Infrastructure/Repositories/Dashboard/DashboardRepository.cs | Canonical runtime procedure; retained |
| `dbo.usp_Dashboard_GetSummary` | FarmStay.Infrastructure/Repositories/Dashboard/DashboardRepository.cs | Canonical runtime procedure; retained |
| `dbo.usp_Report_Occupancy` | No literal runtime C# caller found | Legacy compatibility candidate; retained pending external-caller/parity proof |
| `dbo.usp_Report_Revenue` | No literal runtime C# caller found | Legacy compatibility candidate; retained pending external-caller/parity proof |
| `dbo.usp_Booking_GetReport` | FarmStay.Infrastructure/Repositories/Reports/Booking/BookingReportRepository.cs | Canonical runtime procedure; retained |
| `dbo.usp_Booking_GetSummary` | FarmStay.Infrastructure/Repositories/Reports/Booking/BookingReportRepository.cs | Canonical runtime procedure; retained |
| `dbo.usp_Customer_GetReport` | FarmStay.Infrastructure/Repositories/Reports/Customer/CustomerReportRepository.cs | Canonical runtime procedure; retained |
| `dbo.usp_Customer_GetSummary` | FarmStay.Infrastructure/Repositories/Reports/Customer/CustomerReportRepository.cs | Canonical runtime procedure; retained |
| `dbo.usp_Occupancy_GetReport` | FarmStay.Infrastructure/Repositories/Reports/Occupancy/OccupancyReportRepository.cs | Canonical runtime procedure; retained |
| `dbo.usp_Occupancy_GetSummary` | FarmStay.Infrastructure/Repositories/Reports/Occupancy/OccupancyReportRepository.cs | Canonical runtime procedure; retained |
| `dbo.usp_Revenue_GetReport` | FarmStay.Infrastructure/Repositories/Reports/Revenue/RevenueReportRepository.cs | Canonical runtime procedure; retained |
| `dbo.usp_Revenue_GetSummary` | FarmStay.Infrastructure/Repositories/Reports/Revenue/RevenueReportRepository.cs | Canonical runtime procedure; retained |

Payment receipt: `dbo.usp_Report_BookingPaymentReceipt` is defined by migration `20261119001000_AddPaymentReceiptReport` and called by `Services/Reporting/BookingReceipts.cs` through EF Core.

## Contract decisions

- Booking, Customer, Occupancy and Booking Collections: independent list and export pages; date-range/month/year/all filters; page size 1–100; deterministic date/ID order; date upper bound exclusive; server/system dates unchanged.
- Booking type and source filters are booking-report-specific. SQL uses bounded parameterized queries, escaped search and explicit tenant predicates on joins/subqueries.
- Collections: successful Advance/Checkout PaymentTransactions, not booking value or earned P&L revenue. Accounts Profit and Loss remains based on posted accounting entries.
- Four legacy `usp_Report_*` procedures return a second count result set; the canonical `usp_*_GetReport`/`GetSummary` family uses separate typed results with explicit aggregate precision. A missing literal C# caller does not prove external jobs/integrations are absent. No SP is dropped or silently redefined in this phase.
- PDF limit 10,000 rows; XLSX limit 100,000. Summary plus one extra row prevents silent export truncation; exporter uses existing RDLC renderer and returns bytes with no public export file storage.
- Dashboard legacy JSON fields `totalRevenue`, `revenueThisMonth` and report key/API `revenue` remain for compatibility; visible captions now say Collections.
- Receipt numbers and payment facts are unchanged. Private receipt queries resolve membership and enforce tenant/customer selectors before SQL. Controller authorization remains in place.

## Verification limits

Source review and small two-tenant fixtures are not a production load benchmark. No claim of second-level latency for millions of rows. External SP caller discovery and decommission approval remain prerequisites to legacy SP removal.
