# Phase 1 — trusted request boundary and upload hardening

Implemented on 25 November 2026. Indian server/system time is retained. Angular remains version 19; this phase makes no frontend or database schema changes.

## Changes

- Shared Application contracts: ICurrentTenantContext, ICurrentUserContext and IPublicTenantResolver. Infrastructure resolves the authenticated user and active membership against the database, including role, user and farmhouse eligibility. Conflicting token roles or tenant selectors are rejected.
- TenantBoundaryMiddleware runs after authentication and before authorization. Protected endpoints validate membership and reject conflicting farmhouse selectors in headers, query, route and JSON. A validated tenant header bridges existing downstream scope implementations without rewriting booking transactions.
- Anonymous public endpoints resolve a published, active and subscription-eligible farmhouse by domain separately from authenticated membership authority.
- Generic media uploads allow only Gallery, Amenity and AboutUs categories and demand matching Create/Edit permission. Category traversal, unsupported private categories, dangerous compound filenames, mismatched MIME/extensions and invalid file signatures are rejected. Generated filenames use GUID suffixes. Existing size limits remain.
- Generic permission lookup recognizes Super Admin by role name instead of assuming role ID 1.
- Unexpected exception responses expose a generic message and correlation reference; detailed exceptions remain in internal logs. Known tenant-access failures use safe status/message responses.
- Auth, upload, public writes and report exports have dedicated rate-limit policies. Response headers include nosniff, referrer policy and correlation ID. Production requires explicit host and CORS configuration and enables HSTS.

## Configuration

Production must configure AllowedHosts with intended hostnames and Cors:AllowedOrigins with explicit HTTP/HTTPS origins. Wildcard origins are rejected; absent production CORS origins fail startup.

For the existing local frontend, Development on localhost permits public domain selection from FarmStay__DevelopmentPublicDomains (semicolon-separated, default greenvalley.com). This accommodates the existing frontend X-Domain header. It does not bypass database publication, active-state or subscription checks. Production does not use this development allowance. Configure the intended domain explicitly for other local sites.

## Verification evidence

- Backend build: succeeded, 0 errors, 11 warnings (nullable/logging warnings remain).
- Verification project build: succeeded, 0 errors.
- Accounts integration/HTTP/security harness: **57 passed, 0 failed**.
- Disposable database retained by the harness: FSAccounts_Test_e125cd61defa458b973813b340636829. Existing development booking data was not used as test fixtures.
- Coverage includes clean migrations, accounting posting/reversal/reconciliation, receipts PDF/XLSX, cross-tenant resource access, anonymous/customer restrictions, spoofed tenant headers/query/JSON, stale role claims, actor-membership mismatch, public domain resolution and upload rejection cases.
- Local raw logs: .phase1-build.log, .phase1-test-build.log, .phase1-tests.log. These are build artifacts, not deployment configuration.
- Frontend working tree checked clean.

## Limits and follow-up

This is not a penetration-test or performance certification. Dedicated rate-limit exhaustion, production proxy/host deployment and browser end-to-end flows have not been exercised. Existing module-specific scope implementations remain behind the shared request boundary; per-resource ownership must continue to be enforced by repositories. Account exports retain their Accounts rate policy. Broad per-module IDOR and private-document lifecycle testing remain future verification work.

No Reporting/Reports module consolidation, accounting policy changes, stored-procedure deletion, schema migration, frontend upgrade, UTC conversion or GitHub push is included. Earlier authorized backend folder moves remain uncommitted alongside these changes. Phase 2 requires its own authorization.
