# Phase 0 baseline and gap report

## Scope and frozen decisions

Read-only baseline after backend folder alignment. No application source/schema/data changes are authorized in Phase 0. Indian server/system time remains the project policy; do not adopt the PDF's UTC instructions. Angular stays v19; no major upgrade. Preserve all four projects, feature folders, booking/payment flows, public anonymous access and existing financial records.

Backend HEAD: 291516234be0308cfe315a6d846639d638ad9bb2, with substantial UNCOMMITTED folder moves and forwarding layers from the preceding authorized task. They are part of the working baseline, not newly introduced Phase 0 changes. Frontend HEAD: 89fe84ef792affa79887af302a5be1a3dee43e3f, clean at inspection.

A source-only safety copy (C#, project, SQL and RDLC; excludes secrets/config/uploads) exists under `.build-phase0/source-backup`. Hash inventory and tracked patch are recorded here. This is not a SQL/file-storage backup or a proven restore drill.

## Live database metadata

- Tables: 48; stored procedures: 18.
- Pending migrations: 0.
- Existing Accounts module ID 3 is present; reuse it.
- CONFIRMED duplicate reporting identities: module 4 `Reporting` → `/reporting`, and module 6 `Reports` → `/reports`, both active.
- RBAC metadata: Roles=7, Permissions=44, RoleModules=17, RoleModulePermissions=135, FarmHouseModules=21.
- Full applied migration/table/SP/module listing: database-inventory.txt. No business rows or secrets were exported.

## Feature flow baseline

| Feature | Observed path / qualification |
|---|---|
| Accounts | Controllers/Accounts/AccountsController → Application service interface → Services/Accounts/AccountsService → repository interface → Repositories/Accounts partials → DbContext/SP. Scope implementation in Services/Accounts/AccountsScope. |
| Booking Submit | BookingsController → IBookingSubmission → Application BookingSubmissionService forwarding layer → IBookingSubmissionRepository → existing Infrastructure BookingSubmission. Main business orchestration still remains in Infrastructure; moving files alone did not extract it. |
| File Upload | Controllers/FileUpload → IFileUploadService → Application forwarding service → IFileUploadRepository → Infrastructure repository. Existing header-based tenant behaviour remains unchanged and needs Phase 1. |
| Reporting | Controllers/Reporting → Application Services/Reports → report repository interfaces/implementations → SQL/Reports and RDLC. Application reporting facade still reads HttpContext/header. |
| Gallery/Amenity/AboutUs/Contact/Feedback/Pricing | Existing feature controllers/services/repositories are physically grouped. Exact paths/routes recorded in inventory.md; DI registrations in di-map.md. Full endpoint-level security tests have not been rerun in Phase 0. |
| Other booking operations | Infrastructure DI still binds IBookingDecisionService, IBookingStayOperations, IAdminBookingConfirmation and identity/notification interfaces directly to Infrastructure implementations. Application orchestration extraction remains a phase-scoped task. |

## Confirmed source gaps and proposed priority

| ID | Priority | Finding / evidence | Target phase |
|---|---|---|---|
| SEC-01 | High priority review | FileUploadRepository reads Request.Headers["FarmHouseId"] at line 63. FileUploadController only requires authentication, with no endpoint feature permission. Header-to-membership ownership enforcement is not evident in this flow. Exploit has NOT been exercised. | 1 |
| SEC-02 | High priority review | GlobalExceptionMiddleware returns exception.Message at line 48; raw internal details can enter responses. | 1 |
| SEC-03 | High priority review | TenantResolver.GetCurrentDomain accepts X-Domain; separate trusted authenticated context and validated anonymous public-domain resolution are not established consistently. | 1 |
| SEC-04 | Review | ReportingService reads FarmHouseId header and separately validates JWT/farmhouse/membership. This differs from AccountsScope's membership-ID authority. Standardize without removing public access. | 1 |
| SEC-05 | Review | Program declares an Accounts-specific rate-limit policy. Login/OTP/upload/contact/export-specific coverage needs endpoint matrix and abuse tests. | 1 |
| REP-01 | Confirmed metadata inconsistency | Database has active Reporting and Reports modules; AppDbContext seeds Reporting while ReportAccess checks Reports. Preserve grants in any consolidation migration. | 2 |
| REP-02 | Candidate only | SQL catalog contains usp_Report_* and usp_*_GetReport/GetSummary families. Literal caller references are in inventory.md; no SP is yet proven safe to delete. | 2 |
| FLOW-01 | Confirmed source deviation | Several booking Application interfaces resolve directly to Infrastructure implementations. Submit now has a forwarding service, but workflow extraction must preserve application locks and transaction scope. | 3 |
| ACC-01 | Confirmed source limitation | Source worker supports Advance/Checkout; Refund policy/quarantine is not implemented as a complete approved flow. Inspect AssetsAndSources.cs payment type guard. | 4 |
| ACC-02 | Confirmed source limitation | Revenue recognition requires net >= advance and net > 0. Full-discount zero checkout needs an owner-approved accounting policy, not fake cash entries. | 4 |
| ACC-03 | Confirmed source limitation | Successful payment settlement uses Cash→1000, otherwise→1010 at AssetsAndSources.cs line 57. Actual tenant MoneyAccount mapping is absent. | 4 |
| ACC-04 | Confirmed source pattern | Asset action EventKey uses Guid.NewGuid at line 24. Rowversion provides a separate concurrency guard, but stable request-key retry recovery is not demonstrated. | 4/5 |
| ACC-05 | Review | InitializeAsync runs from Accounts Get/Save. Check initialization cost/concurrency and missing FKs against live constraints before changing. | 4/5 |
| FILE-01 | Unverified coverage | Public/private storage, signature checks, duplicate hashes, replace/delete reference safety and cross-tenant download matrix require targeted tests. Do not infer a passed security audit from MIME/extension checks alone. | 1/5/6 |
| OPS-01 | Historical finding, not re-audited | Previous frontend audit reported unresolved Angular 19 high advisories. Keep v19; compatible fixes/mitigations and accepted residual risk need a current audit. | 7 |

No current finding above is a completed penetration-test result. Priority labels are triage recommendations, not independently assigned CVSS scores.

## Policy decisions needed before later phases

- Refund: full/partial, discount treatment, source event, payment-method mapping and reversal rules.
- Zero checkout: fulfilment accounting when payable is zero; do not invent a successful money movement.
- Settlement: explicit tenant cash/bank/provider mapping and ambiguous/unmapped-event handling.
- Canonical reporting identity: `/reports` is currently used by Angular; approve data-preserving grants migration before modifying database identity.
- Same-day 10 AM turnover is a PDF baseline to VERIFY against current code and owner policy, not silently enable as a new rule.
- Server/system time remains authoritative per owner; test day boundaries without converting storage to UTC.

## File-wise next phase proposal — no edits applied

1. Application/Interfaces/Common: inventory/reuse current user/tenant contracts; add missing ICurrentUserContext, ICurrentTenantContext, IPublicTenantResolver only if no equivalent exists.
2. Infrastructure/Services/Common (or agreed feature subfolder): trusted membership resolver and separately validated public domain implementation.
3. Controllers/FileUpload, Application/Services/FileUpload, Interfaces/Repositories/FileUpload, Infrastructure/Repositories/FileUpload: replace header authority, enforce feature rights/resource ownership and validate safe storage.
4. API/Middleware/GlobalExceptionMiddleware.cs: safe response and correlation ID, structured internal logging.
5. API/Authorization, DependencyInjection, Program.cs: reuse RBAC; scope rate-limit/security policy and DI changes.
6. Tenant callers in AccountsScope, ReportingService, dashboard, admin menu and booking: review individually; preserve routes and JSON.
7. Tests: two-tenant read/write/upload/download/export, inactive membership/role/farmhouse, spoofed headers/domain, upload validation and anonymous public regression.

Potential schema changes are NOT approved by this baseline. Any required permission/index/FK migration needs its own reviewed Up/Down/data-preservation plan. No database object or SP should be removed during Phase 1 as incidental cleanup.

## Evidence boundaries

Historical documented 41 Accounts checks, 27 report checks and 19 Angular tests remain historical; they are not Phase 0 reruns. Current build/unit results are recorded in evidence.md after completion. Full browser, load, penetration, provider/payment, restore and accountant validation are not executed in this phase. Source-level request-scope review is not a substitute for those tests.

## Exit gate

Phase 0 delivers inventory, current uncommitted-change preservation, read-only database metadata, gaps and a next-phase file map. No fixes are applied. Owner approval is required before starting Phase 1.
