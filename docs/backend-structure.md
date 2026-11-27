# Backend folder structure alignment

Scope: backend only. Frontend, Angular v19, server/system time, API routes and database table identities remain unchanged.

## Target responsibilities

Controller → Application service interface → Application service → Application repository interface → Infrastructure repository → AppDbContext / SQL.

- Controllers/{Feature}: HTTP endpoints.
- Application/DTOs/{Feature}/Requests and Responses: transport contracts.
- Application/Interfaces/Services/{Feature}: service contracts.
- Application/Services/{Feature}: application use cases.
- Application/Interfaces/Repositories/{Feature}: persistence contracts.
- Domain/Entities/{Feature}, Enums/{Feature}, Rules/{Feature}: domain responsibilities.
- Infrastructure/Repositories/{Feature}: persistence.
- Infrastructure/Persistence/Configurations/{Feature}: mappings.
- Infrastructure/Services/{Feature}: technical adapters.
- Infrastructure/BackgroundServices/{Feature}: workers.
- Infrastructure/SQL/{Feature}: SQL/SP resources.
- Infrastructure/Reports/{Feature}: RDLC resources.

Existing CLR namespaces are intentionally retained to avoid breaking EF model snapshot/migration identities and public contracts. Physical file locations are the authoritative navigation structure for this arrangement. Migrations and AppDbContext stay in their existing central folders. Shared auth/communication/tenant primitives remain common where appropriate.

Mixed Accounts contracts/entities and Reporting contracts have been split into individual files. DI registrations continue resolving the same implementations. Historical SQL/RDLC logical resource names are explicitly retained in the project file; migrations need not be rewritten.

This work is structural alignment, not Phase 0/security policy changes. Existing controller security helpers and business-flow deviations must still be inventoried in Phase 0; this report does not certify every controller as HTTP-only.

## Move/split inventory

259 move/split entries are recorded in `Accounts/backend-folder-moves.json`.

File Upload now explicitly follows IFileUploadService → Application FileUploadService → IFileUploadRepository → Infrastructure FileUploadRepository. The existing upload behaviour is unchanged; upload security policy is a later phase.

Validation: relocated backend build passed and 41/41 disposable SQL/HTTP regression checks passed, including clean migrations and PDF/XLSX resource loading. Final FileUpload forwarding layer is checked by the final API build.

Booking Submit now resolves IBookingSubmission → Application BookingSubmissionService → IBookingSubmissionRepository → existing Infrastructure BookingSubmission. Existing transaction/idempotency logic stays intact. This is an additional forwarding boundary; broader extraction of existing Infrastructure business orchestration and controller authorization helpers remains a Phase 0 inventory item, not silently claimed complete.
