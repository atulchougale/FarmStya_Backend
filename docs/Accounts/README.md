# Accounts & Finance — implementation and operations

## Architecture and source map

- Domain: `FarmStay.Domain/Accounts/AccountingEntities.cs` — decimal accounting invariants and entities.
- Application: `FarmStay.Application/Accounts` — request DTOs, scope/repository/service interfaces and use cases.
- Infrastructure: `FarmStay.Infrastructure/Accounts` — tenant-scoped repositories, transactions, idempotent seeds, source worker, SQL report and RDLC template.
- API: `FarmStay/Controllers/Accounts/AccountsController.cs` — thin authenticated endpoints.
- Angular: `Frontend/src/app/features/admin/accounts` — typed HTTP service, guard, page schemas, responsive list/form, voucher and report components.
- Existing Accounts module is reused. No replacement Booking, PaymentTransaction, NotificationOutbox, BookingActionToken or FarmHousePaymentConfiguration tables/workflows are introduced.

## Data model

AccountingSettings, AccountingMasters, LedgerAccounts, AccountingFinancialYears, AccountingPeriods, AccountingVouchers, AccountingVoucherLines, AccountingVendors, AccountingMoneyAccounts, AccountingFixedAssets, AccountingAssetEvents, AccountingRecurringExpenses, AccountingBankStatements, AccountingAttachments, AccountingAudits.

`AccountingMasters.Kind` consolidates global and tenant category/type/unit/payment-mode/cost-centre/tax catalogues. Global system rows are read-only; tenant records can be edited/inactivated. There are no delete endpoints. Tenant ledger instances map the protected default chart. Opening balances are balanced `OpeningBalance` vouchers, not an independent duplicate balance store. Cashbook, bankbook, ageing and dashboard derive from posted voucher lines.

## Routes and API map

Base API: `/api/administration/accounts`.

| Angular route under `/admin/accounts` | GET area | Write operation |
|---|---|---|
| overview | overview | Read only |
| ledgers | ledgers | POST, PUT ledgers/{id} |
| vouchers, vouchers/new, vouchers/{id} | vouchers, vouchers/{id} | Draft POST/PUT; POST {id}/post or reverse |
| income, expenses, payables | same area | Shared voucher editor with selected type |
| vendors | vendors | POST/PUT vendors |
| money-accounts | money-accounts | POST/PUT money-accounts |
| assets | assets | POST acquisition; POST assets/{id}/preview, depreciation, maintenance, transfer, disposal |
| masters | masters | POST/PUT masters |
| periods, financial-years | same area | PUT periods/{id} with reason/version; amount 1 closes, 0 reopens |
| recurring | recurring | POST/PUT recurring |
| statements | statements | POST statement; POST statements/{id}/reconcile |
| settings | settings | PUT settings |
| reports, reports/{report} | reports/{report} | GET reports/{report}/export/pdf or excel |
| audit | audit | Read only |

Attachments: POST `vouchers/{id}/attachments`, GET `attachments/{id}`. Actual PDF/JPEG/PNG signatures are checked; five files per draft voucher, 5 MB per file. Downloads are tenant-scoped attachments, never public filesystem URLs. Only bank account last four digits are stored.

Lists: page >= 1, page size 1–100, bounded search, allowlisted sorting. Reports: from/to/account filters, bounded pages; exports limited to 10,000 matching rows to bound RDLC memory. UI supports range/month/financial-year/all, separate PDF and Excel actions. Downloads use submitted filters.

## Permissions

All are under the existing Accounts module: `Accounts.View`, `Accounts.Create`, `Accounts.Edit`, `Accounts.Post`, `Accounts.Reverse`, `Accounts.Export`, `Accounts.ManageMasters`, `Accounts.ManagePeriods`, `Accounts.ManageAssets`. Admin and Farm Owner receive seeds; Super Admin uses its authenticated active membership and global role. No role-number shortcut is used in the Accounts scope.

Scope resolution validates JWT `UserMembershipId` and user ID against active membership, active user, role and farmhouse. Request body/query/header farmhouse IDs do not select a tenant. Legacy User.FarmHouseId is not tenant authority. Backend permissions protect every entry point independently of the UI. Rate limiting: 120 requests/minute per authenticated user (anonymous fallback remote IP); 6 MB request cap.

## Posting and reconciliation rules

- Vouchers require 2–200 one-sided positive decimal lines, at least two distinct accounts and identical total debit/credit.
- Validate same-tenant active accounts, vendors, optional customer membership, category, cost centre, GST/effective tax date, financial year and open period.
- Serializable transactions and row versions protect posting, reversals, bills/payments, asset actions, periods and source processing. Database source uniqueness makes retries safe.
- Posted headers/lines and audit records are protected by database immutability triggers. Save attachments before posting. Corrections are linked reversals/adjustments, never edits or deletes.
- Vendor payments reference a posted vendor bill and cannot exceed its outstanding amount. Reverse linked payments before reversing a bill.
- Optional GST is disabled initially; no statutory tax rates, useful lives or depreciation rates are assumed.
- Assets post acquisition cost to fixed assets, depreciation expense to accumulated depreciation, and disposal gain/loss separately. Depreciation is posted chronologically at completed month ends. Maintenance is an expense; location changes are audited events.
- An approved recurring schedule generates one posted expense per due occurrence; a stable source key prevents repeat posting. Creating/updating schedules requires posting permission as well as create/edit rights.

## Booking source integration

`AccountingWorker` polls once a minute, bounded to 100 payments/100 fulfilled bookings/50 schedules per tenant per pass. It waits while migrations are pending. It reads existing source records and does not change booking/payment status or call payment gateways/messages.

1. Successful advance: debit cash (1000) or bank control (1010), credit customer advances (2100).
2. Successful checkout payment: debit cash/bank, credit receivable (1100).
3. `CheckedOut` fulfilment: debit advances applied plus remaining receivable, credit booking revenue (4000), net of checkout discount.
4. Recognition waits for corresponding successful payment postings. Source keys are `Payment:{id}`, `BookingRevenue:{id}`, `Recurring:{id}:{date}`. Booking source references connect individual receivables to their payments for ageing.

The default recognition policy is checkout, reflecting the current application lifecycle. Existing payment methods map cash vs bank-control; actual banking transfers/reconciliation can be recorded against the real tenant money accounts. A failed source (including closed historical period) is logged and retried, never silently moved to another date or counted twice.

## Reporting

`dbo.usp_Accounts_Report` is Infrastructure-only, parameterized and executed with EF Core `FromSqlInterpolated`. Its allowlist covers ledger, trial-balance, profit-loss, balance-sheet, cashbook, bankbook, daybook, income-expense, receivables, payables, assets, depreciation, reconciliation and tax. Source is posted voucher lines; asset/depreciation and statement reports additionally use their corresponding linked registers. Bank reconciliation distinguishes unmatched statement, unmatched book and matched entries. Depreciation reports distinguish policy projections from posted events. `AccountingReport.rdlc` generates PDF and XLSX.

## Run and verification commands

Run from `F:/Project/FasrmStya/Program`:

```powershell
dotnet build FarmStay/FarmStay.API.csproj -c Accounts -m:1
dotnet ef database update --configuration Accounts --project FarmStay.Infrastructure --startup-project FarmStay -- --environment Development
dotnet build tests/FarmStay.Accounts.Verification/FarmStay.Accounts.Verification.csproj -c Accounts -m:1
dotnet run --no-build --project tests/FarmStay.Accounts.Verification -c Accounts -- --integration --http
```

The verification harness uses development connection configuration only to locate SQL Server, then creates an independently named `FSAccounts_Test_<guid>` database. It never resets/deletes the development database. Evidence databases are retained for inspection. `--http` starts an isolated API on port 7193 against that disposable database and stops only its own process. No real booking or gateway transaction is submitted by tests.

From `F:/Project/FasrmStya/Frontend`:

```powershell
npm test -- --watch=false --browsers=ChromeHeadless --include=src/app/features/admin/accounts/accounts.spec.ts
npm run build -- --configuration production
```

Executed counts, failures repaired, reports and limitations are recorded in `verification.md` after the final runs.

## Operational boundaries

- Use development/staging migration and source-posting checks before enabling live use. Old successful development sources will be accounted for by the new worker after deployment; this is accounting backfill, not payment replay.
- An accountant must approve opening balances, recognition policy, capitalisation threshold, depreciation life/rates and GST treatment before statutory use.
- Real bank credentials are neither required nor stored. Reconciliation is manual; no bank-feed integration is claimed.
- The performance ceiling at production scale requires representative load tests and execution-plan review; passing fixture tests is not a production-volume latency guarantee.
- Full authenticated browser walkthrough and manual mobile visual QA require a usable signed-in admin session. Automated Angular/API checks are reported separately.
