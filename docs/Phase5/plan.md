# Phase 5 implementation plan

Preserve current project/feature folders, routes, booking/payment rules, Angular 19 and Indian server timestamps. Existing uncommitted phase work stays intact.

| Files | Responsibility / planned change |
|---|---|
| Infrastructure/Repositories/FileUpload/FileUploadRepository.cs | Validate before hashing; tenant/category-scoped deterministic public media reuse; atomic staging and bounded writes. Preserve legacy URLs. |
| API/Middleware/PublicMediaBoundaryMiddleware.cs; API/Program.cs | Restrict `/uploads/{farm}/{category}/{file}` to validated public farmhouse domain before static serving; safe response headers. |
| Infrastructure/Services/FileUpload/FileSafety.cs | Reusable private filename/signature and private-root safeguards. |
| Infrastructure/Repositories/Booking/BookingIdentityDocuments.cs | Reject duplicate front/back bytes, validate private root, preserve immutable superseded records, verify content integrity on read. |
| Infrastructure/Repositories/Accounts/AccountsRepository.Vouchers.cs | Serialize attachment validation/count/dedup/save; retain private database storage, preserve posted immutability. |
| Infrastructure/Persistence/Configurations/Booking/BookingDocumentConfiguration.cs; Accounts/AccountingModel.cs | Composite tenant parent links and document-to-booking customer consistency. |
| Infrastructure/Migrations | Data-preserving constraints with existing-data preflight; no orphan deletion or financial rewrite. |
| tests/FarmStay.Files.Verification | Repeatable upload, visibility, duplicate, root, integrity and database ownership checks on isolated storage/SQL. |
| docs/Phase5 | Inventory, duplicate/retention policy, SQL caller inventory, test evidence and limitations. |

No destructive cleanup or cross-tenant deduplication. Gallery/Amenity/AboutUs replacements retain prior files; obsolete media stays retained until an explicit reference-aware retention policy is approved. Accounts blobs/identity documents remain private; receipts/exports remain generated through authorized services. Active SQL procedures remain unless caller tracing and parity prove one unused.
