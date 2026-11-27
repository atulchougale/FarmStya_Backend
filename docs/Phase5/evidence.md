# Phase 5 completion evidence

Date: 26 November 2026, Indian server/system time. Development environment, .NET 8 / EF Core / SQL Server; Angular 19 preserved. Existing uncommitted developer and earlier-phase work retained; no commit or push performed.

## Result

| Verification | Passed | Failed |
|---|---:|---:|
| File lifecycle, concurrency, ownership, upgrade preflight and public media HTTP | 31 | 0 |
| Accounts/security regression, including HTTP | 57 | 0 |
| Booking regression, including HTTP | 39 | 0 |
| Finance regression | 42 | 0 |
| Reporting, migration rollback and PDF/Excel exports, including HTTP | 33 | 0 |
| Total backend checks | 202 | 0 |

All five verification projects built with `dotnet build tests/FarmStay.<suite>.Verification -c Accounts --no-restore -m:1`. Existing nullable/log-template warnings remain; no build errors. New Files harness was restored on its initial build and adds no external package reference.

Commands executed from backend root:

```powershell
dotnet run --no-build --project tests/FarmStay.Files.Verification -c Accounts
dotnet run --no-build --project tests/FarmStay.Accounts.Verification -c Accounts -- --integration --http
dotnet run --no-build --project tests/FarmStay.Booking.Verification -c Accounts -- --http
dotnet run --no-build --project tests/FarmStay.Finance.Verification -c Accounts
dotnet run --no-build --project tests/FarmStay.Reporting.Verification -c Accounts -- --http
```

The first Booking/Finance regression run exposed a missing IHostEnvironment registration in their manually assembled test containers. Fixtures now register a host environment like the real API host, and both final reruns passed. File tests cover separate website/API hosts, unknown public domains, legacy logos/covers, cross-tenant rejection, exact MIME/signature pairing, bounded writes, duplicate reuse, corruption detection, private root rejection, retained superseded versions, concurrent five-attachment limit and posted-voucher protection.

Test SQL databases use disposable `FSFiles_Phase5_*`, `FSAccounts_Test_*`, `FSBooking_Phase3_*`, `FSFinance_Phase4_*`, `FSReporting_Phase2_*` names. File tests use an isolated `FarmStay_Phase5_*` temporary directory. They do not contact payment/notification providers. Databases/storage remain for evidence; no production cleanup is performed.

## Migration and running applications

Reviewed `20261126054352_SecureFileOwnership` Up/Down. Removed generated seed timestamp churn and preserved the existing payment MoneyAccount lookup index. Migration adds restrictive composite attachment-to-voucher and identity-to-booking/customer ownership constraints. It changes no file content, booking amount, timestamp or payment fact.

An isolated upgrade test introduced an invalid legacy document link, proved preflight failure 51033, and verified the row was retained. Only that test fixture was removed before successful retry. Clean structural rollback/upgrade also passed.

Applied to the configured Development database with:

```powershell
dotnet run --no-build --project tests/FarmStay.Files.Verification -c Accounts -- --apply
```

Output: `Phase 5 migration applied.` Backend restarted with `dotnet run --no-build --project FarmStay/FarmStay.API.csproj --configuration Accounts --launch-profile https`.

Frontend `npx ng build --configuration development` passed. The only frontend change is a Marathi translation for duplicate front/back selection. Frontend HTTP 200 at localhost:4200, backend Swagger HTTP 200 at localhost:7081, and a real pre-existing Gallery image returned HTTP 200 after restart.

## Flow and file changes

- Public upload continues through existing Controller → Application FileUploadService → repository. Infrastructure validates content and streams to private staging before an atomic tenant/category/extension hash destination.
- Public media boundary uses Application IPublicMediaAccess with an Infrastructure implementation; middleware does not access EF directly. It checks the published tenant resolver before static serving. Public-only referring-site fallback supports image requests across separate website/API hosts.
- Existing BookingIdentityApplicationService retains tenant/customer/RBAC checks. Its repository validates uploads, handles exact retries, preserves superseded copies and verifies current private download hashes.
- Existing Accounts service/RBAC remains authoritative. Attachment repository serializes additions and checks duplicates before the limit. Database storage stays private.
- Entity configurations, migration/snapshot, shared Infrastructure FileSafety, DI and the two test fixtures changed inside their existing feature/layer structure. No new business module, duplicate entity, UTC persistence or Angular upgrade.

## Retained policies and limits

Read `file-lifecycle.md` for retention and rollback. No bulk duplicate merge, orphan deletion, physical asset cleanup or unproven SP removal was performed. JPG/JPEG extensions can keep separate public representations; deduplication is explicitly tenant/category/extension scoped. Old private versions are retained for audit but are not exposed through the customer's current-document endpoint.

`stored-procedure-inventory.md` traces source literals and SQL wrappers; absence of a literal caller is not proof of an unused procedure. Existing large feature partials were retained where no functional split was needed. No authenticated visual browser walkthrough, live provider acceptance, penetration test, restore drill or production load certification is claimed; full regression breadth and operational readiness remain Phase 6/7 work.
