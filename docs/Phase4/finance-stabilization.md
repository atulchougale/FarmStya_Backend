# Phase 4 — Accounts and Finance Stabilization

## Approved policies

- Refunds record an actual return made by Admin/Owner with Accounts.Post permission. Partial/full returns are supported, bounded by the original successful collection and booking net collections. Super Admin remains globally authorized.
- The operator selects the actual Cash/Bank Money Account and confirms money was returned. Bank returns require a transfer reference. This action does not call Razorpay, cancel a booking, or release dates.
- A checkout with no payable balance completes without creating a payment transaction or final collection receipt.
- Default settlement accounts are explicit settings. Missing or invalid mapping goes to Review Required; the worker never guesses an account.
- Angular 19 and Indian server/system timestamps are retained.

## Operator setup and flow

1. Open `/admin/accounts/settings`, select active default Cash and Bank Money Accounts, and save.
2. Use `/admin/accounts/refunds` to find a booking by reference, choose its original payment and actual return account, and record the actual refund.
3. Review `/admin/accounts/source-reviews`. Correct settings or underlying source issues first; request a retry with a reason. Retry does not repair historical data automatically.

Existing unallocated historical refunds require review. They are not assigned an accounting treatment by guessing. Existing zero collection sources are resolved with an audit reason and no money voucher.

## Accounting and consistency

- Advance collection: debit selected money account, credit advance liability 2100.
- Refund before fulfilment: debit advance liability 2100, credit actual return account.
- Refund of earned revenue: debit revenue 4000, credit actual return account. Recognition must exist before posting that refund.
- Fulfilment uses discounted revenue and net advances after advance refunds. A fully discounted zero-revenue stay receives a resolved audit marker rather than a zero voucher.
- Customer/admin balances show net paid and refunds. Earned refunds on an already checked-out booking do not reopen a customer debt.
- Payment receipts retain their original number. Receipt reporting uses signed collections/refunds up to the receipt timestamp.
- Posted vouchers remain immutable and balanced. Expense, income, vendor-payment and opening-balance account roles are validated. Money ledgers must be asset accounts.
- Asset request IDs retain semantic hashes; retries return the original result, changed payloads reject. Depreciation remains unique per asset/month. Recurring occurrences are serialized and only advance on successful posting.
- Tenant composite foreign keys and reconciliation ownership guards prevent cross-farm references.
- Initialization uses a version marker and serialized cold initialization; normal requests do not rewrite system masters.

## Implementation locations

### Discount display follow-up

Admin and customer booking details share a settlement summary. A positive total fully covered by discount shows Fully Discounted; it takes priority over Fully Paid. Fully Paid requires an actual net paid amount covering the discounted total and no recorded refunds. Zero original totals and outstanding balances are not labeled paid. This is display-only and does not grant permission to collect a full advance or give a full discount, change status, or bypass payment verification.

Discount audit entries include recorded amount, reason, operator ID/name and system timestamp from existing CheckoutDiscount records. The read is scoped to the authorized booking and farmhouse through the existing Application/Repository chain. No database migration is required. INR comparisons use paise precision.

Application contracts and services remain in Accounts feature folders. Infrastructure implementation is in `FarmStay.Infrastructure/Repositories/Accounts`, including `Refunds.cs` and `SourceReview.cs` partials. HTTP endpoints are in the Accounts controller. Frontend refund UI and source review/settings reuse the existing Accounts module and permission flow.

Migration: `20261126000100_StabilizeAccountsFinance`. SQL preflight, reconciliation guard, and receipt procedure resources live in `FarmStay.Infrastructure/SQL/Accounts/Phase4`.

Migration preflight rejects invalid existing ownership instead of rewriting it. Down migration refuses to discard populated refund/source-review audit metadata. Back up and reconcile financial data before any production rollback.

## Verification

- Dedicated finance verification: 40 passed, 0 failed on an isolated SQL database.
- Accounts regression: 57 passed, 0 failed on an isolated SQL database.
- Booking regression including HTTP access checks: 39 passed, 0 failed.
- Reporting regression including PDF/Excel exports, migration rollback and HTTP access checks: 33 passed, 0 failed. The harness restores the current schema after its historical migration checks.
- Angular development build passed; existing unused-import warnings remain.
- Development migration applied successfully. Frontend `http://localhost:4200` and backend `https://localhost:7081/swagger/index.html` both returned HTTP 200 after restart.
- Tests do not send live provider payments or customer communications. Actual browser/operator flows and real bank/cash return evidence still require user acceptance testing.

Runnable harness: `dotnet run --project tests/FarmStay.Finance.Verification -c Accounts`. Its default creates a disposable test database; `--apply` explicitly applies migrations to the configured Development database. It does not print connection secrets.
