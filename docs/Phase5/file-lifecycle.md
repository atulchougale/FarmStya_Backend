# File lifecycle and retention inventory

| Feature | Storage and visibility | Replace/delete policy |
|---|---|---|
| Gallery, Amenity, About Us | Public media under tenant/category; existing static site assets also retained | Feature rows soft-delete; no physical deletion or old URL rewrite |
| Farmhouse cover and logo | Existing public FarmHouse category and legacy logos path | Public domain ownership checked; legacy logo must match farmhouse LogoUrl |
| Booking ID | Private configured root outside wwwroot; SHA256, original metadata and generated key | Current set immutable until admin requests reupload; previous rows superseded and bytes retained |
| Accounts attachments | Private database blobs; voucher ownership and Accounts permission enforced | Draft only additions; same-voucher duplicates reused, max five, serialized writes; posted voucher remains immutable |
| Payment receipts | Generated from authorized booking/payment/receipt records and active receipt SP | Stable receipt numbers; no public static PDF storage |
| Reports | Authorized tenant-scoped SQL/SP generation | Generated exports, no public static file directory |
| Contact/Feedback | Text records; no uploaded files in current flow | Existing permission, audit and soft-delete conventions |

## Duplicate policy

Public upload is validated before SHA256 hashing, streamed through private staging, then atomically renamed to a hash filename within the tenant/category folder. Concurrent identical uploads reuse that tenant/category/extension file. The validated extension remains part of the storage key (JPG/JPEG aliases can retain separate representations). Legacy filenames stay valid and are not bulk rehashed. Same hash across tenants/categories produces separate physical files; no private-file or cross-tenant deduplication occurs.

ID upload rejects identical front/back bytes. Exact retry of the accepted complete set returns its current document result without writing more bytes. A replacement request creates a new version and retains old rows/files. Current downloads verify stored length and SHA256 before serving. Old superseded versions are retained for audit and are not exposed through the customer current-document endpoint.

Accounting duplicates are compared only inside one tenant/voucher and returned before the five-attachment limit. The transaction lock serializes concurrent additions, and SQL composite foreign keys protect parent ownership. Existing duplicate historical blobs remain untouched.

## Cleanup and audit

No automated physical deletion of retained or referenced files is enabled. A failed public upload removes only its own private temporary file. Process-crash staging files are retained until an explicit retention/reference policy is approved. Identity cleanup after a known precommit failure touches only generated files from that attempt; an ambiguous commit never deletes files that may be referenced.

Existing CreatedBy/ModifiedBy and server/system timestamps are reused. No duplicate audit columns or UTC conversion is added. Large feature partials and active SPs remain in the established folders; no cosmetic restructure or unproven procedure removal.

## Rollback

20261126054352_SecureFileOwnership checks all existing ID/customer/booking and attachment/voucher ownership before constraints. Mismatches abort rather than silently rewriting or deleting records. Structural Down restores previous FKs without removing file rows/content. Take backups for a production release; application rollback must preserve newly generated hash URLs and private storage.
