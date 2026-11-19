# Reporting

Application contracts and filter rules: `FarmStay.Application/Reporting`.
HTTP endpoints: `FarmStay/Controllers/Reporting`.
SQL procedures and RBAC/index provisioning: `Reporting/Sql` (embedded resources applied by the reporting migration).
Report layouts: `Reporting/Templates` (four embedded RDLC files).

Routes: `/api/administration/reports`, `/{bookings|revenue|customers|occupancy}`,
and `/{report}/export/{pdf|excel}`.

## Semantics

- Booking and Customer periods use booking submission dates. A Customer row counts all matching bookings, including rejected/cancelled bookings; booking value is not revenue.
- Revenue includes successful Advance and Checkout receipts, by PaidAt (legacy fallback: VerifiedAt / CreatedDate). Pending, failed and refunded payments are not received revenue.
- Occupancy groups scheduled stay dates for Confirmed, CheckedIn and CheckedOut bookings. It is a scheduled booking occupancy report, not an actual-arrival attendance report or a room-capacity percentage.
- Date ranges include the full To date; SQL receives an exclusive upper boundary. Server-local timestamps are preserved.
- Booking filters: normal = BookingOnly; event = NormalEvent/DecorationEvent. Assisted = WalkIn/Phone.
- Search is literal prefix search, not a wildcard or arbitrary SQL expression. Occupancy searches ISO stay dates.

## Scale and exports

Filtering, deterministic ordering, aggregate counts and paging run in parameterized stored procedures. Covering/filtered indexes scope work by farmhouse/date. Optional predicates use OPTION(RECOMPILE) so All and selective date filters can use different plans. No browser-side loading of the entire report occurs.

PDF and real XLSX both use RDLC; exports repeat the applied filters/search independently of UI pagination. There is no CSV substitution. Financial report exports include totals.

Synchronous PDF is limited to 10,000 matching rows, XLSX to 100,000, with an explicit error and no silent truncation. At most two complete exports run in parallel per API process. For a larger export, narrow dates/search. Higher limits require measured memory/CPU capacity or a queued reporting job design.

Query timings and match counts are logged. Sub-second performance is not guaranteed without measuring production data volumes, hardware, query plans and concurrent load. Validate with actual large-volume data before setting an SLA; exact All counts and very deep OFFSET pages remain proportional to the matching data.

## Access

Admin and Owner receive separate View/Export permissions per report. Customer roles have no reporting rights. Super Admin retains global access. The API checks active user/membership/role, tenant scope, module and permission; frontend route guards are supplementary.

When changing a deployed procedure, add a new migration with versioned SQL resources; do not rely on re-running an already-applied migration.
