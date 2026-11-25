# Accounts implementation map

Baseline: supplied Accounts & Finance specification, 24 November 2026.

| Area | Repository finding | Implementation location |
|---|---|---|
| RBAC | Accounts module already seeded (module 3); permissions grouped by prefix | Existing module, new Accounts.* grants |
| Tenant | JWT contains UserMembershipId; membership has active/deleted user/role/farm flags | Accounts scope resolver validates membership, ignores client tenant headers |
| Data | No accounting ledger/voucher/asset tables | Domain/Accounts and Infrastructure/Accounts |
| Booking | Success payments; fulfilled status CheckedOut; cash/online; discounts separate | Read-only source polling with unique source key and balanced postings |
| Time | Project uses DateTime.Now | System/server time for new audits |
| Reports | EF Core FromSql, RDLC PDF/XLSX | Accounts SQL result DTOs and report template |
| Frontend | Angular standalone components, dynamic RBAC sidebar, theme tokens | features/admin/accounts |

Existing BookingActionToken, NotificationOutbox and FarmHousePaymentConfiguration already exist and will not be recreated or changed. Existing Booking and PaymentTransaction workflows remain intact.

Implementation evidence and remaining verification are tracked in the final runbook.
