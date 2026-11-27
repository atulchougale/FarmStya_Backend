# Phase 2 — Reporting consolidation

Owner decision: canonical module **Reporting**, canonical Angular URL **/admin/reporting**. Existing `/admin/reports` and report-specific child URLs redirect to the canonical routes. Backend API URLs `/api/administration/reports` remain compatible. Angular stays 19 and stored/system timestamps remain Indian server time.

## Identity migration

`20261125000100_ConsolidateReportingIdentity` reuses the existing Reporting module (ID 4 in Development). It merges role, permission and farmhouse assignments from Reports. Permission names already have a unique index; the migration rejects ambiguous module identities or duplicate permission names instead of guessing.

The old Reports module and assignment IDs remain as inactive audit records. Exactly one reporting identity is active. Historical migration SQL resources remain unchanged so historical migrations can still replay; the consolidation is the final canonicalization step on both clean and existing databases.

The migration captures four pre/post metadata sets in `dbo.__Phase2ReportingBackup`. Effective farmhouse/role/permission tuples must match exactly before and after consolidation: either a gain or loss aborts the transaction. Disabled and denied assignments are included in the backup. No booking/payment/customer data is updated.

Development migration was applied successfully. Read-only verification queries are in `verification.sql`.

## Request and reporting flow

- ReportingService and DashboardService resolve the shared ICurrentTenantContext instead of accepting a client farmhouse header as authority.
- ReportAccess restricts permission queries to the exact authenticated membership ID, active role/user, canonical Reporting module and enabled farmhouse assignment. View and Export remain separate permissions.
- Reporting routes use authentication plus report-specific catalog guards. They no longer additionally require unrelated booking-list access.
- Receipt queries resolve membership and validate tenant/customer scope before SQL. The payment receipt SP now uses a keyless DTO with EF Core FromSqlInterpolated; RDLC formatting still reuses the existing renderer. No ADO.NET command/reader remains in this path.
- PDF and XLSX exports remain in-memory responses with no public export file storage. Existing filters, bounds and typed precision are retained.

## Financial meaning

Operational successful Advance/Checkout payments are labelled **Booking Collections Report**, **Collections received**, and **Collections Trend**. They are cash/payment collections, not earned revenue. Accounts Profit and Loss remains based on posted accounting vouchers and its existing recognition policy. Existing API keys, JSON property names and report key `revenue` are retained for compatibility. RDLC/PDF/XLSX captions use the corrected meaning.

## Stored procedures

See `inventory.md` for canonical callers and retained compatibility candidates. Legacy `usp_Report_*` procedures have different multi-result contracts from the canonical row/summary pair. Absence of a C# literal caller does not prove the absence of external SQL jobs or integrations. No destructive SP retirement is included without that evidence and parity approval.

## Rollback

Stop application writes before rollback. Use the normal EF migration rollback to `20261124190200_HardenAccountsIntegrity` after reviewing newer migrations, if any. Down restores original reporting assignment IDs and flags from the captured snapshot. If reporting metadata changed after consolidation, Down deliberately refuses to overwrite it; reconcile those changes before a DBA-reviewed rollback. Do not delete the snapshot table to bypass this protection.

The disposable database tests exercise exact original-row restoration, reapplication, idempotent rerun, guarded rollback and rejection of a merge that would expand tenant permissions.

## Verification boundaries

Small two-tenant fixtures verify correctness and isolation, not throughput with millions of rows. Generated Collections PDF was rendered and visually inspected. Browser login/user journeys and production proxy deployment were not independently exercised. Prior full Angular-suite failures remain outside this phase; focused route tests are recorded separately. No Git commit or push is included.
