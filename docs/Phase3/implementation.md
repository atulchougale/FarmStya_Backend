# Phase 3 — Booking stabilization using existing Flow B

Date reservation is already implemented and tested by the owner. Its algorithms, API contract, hold duration, serializable submission transaction, occupied-date checks, application locks and filtered unique constraints are preserved. No replacement reservation mechanism or schema migration is introduced.

## Application flow

Controller -> Application service interface -> Application service -> Application repository interface -> Infrastructure repository -> AppDbContext / existing SQL transaction.

- BookingSubmissionService now resolves the exact customer/administration actor, validates assisted source and validates the submission before invoking the existing transaction-owning repository.
- BookingActorContext reuses ICurrentTenantContext for actor/tenant identity and exact-membership permissions. Customer methods cannot impersonate another customer; administration methods require the relevant Bookings.Create/View/Decide/Confirm permission.
- Approve/reject, stay operations, cash confirmation/reminders, advance orders/verification and identity operations now have Application services and repository contracts. Infrastructure retains the existing EF operations, provider calls, locks, persistence and transactional outbox staging.
- BookingListService authorizes list/detail scope and computes administrative confirmation/check-in/checkout/document capabilities. Controllers do HTTP parsing and response translation.
- BookingListController is split into main, Identity and Receipts partial files within Controllers/Booking. Route attributes and HTTP contracts remain on the same compiled controller type.
- The existing SubmitBookingValidator moved from the DTO file to Validators/Booking. Its CLR name and assembly registration remain compatible. Supported booking type validation is also enforced before repository invocation.

## Preserved financial and booking behavior

Pending -> Approved -> Confirmed -> CheckedIn -> CheckedOut, including existing reject/cancel release behavior. Approval and confirmation deadlines are server/system time. No UTC conversion, Angular upgrade or new turnover/booking-edit policy is introduced.

The existing check-in window (scheduled arrival through scheduled departure) remains unchanged; no new 10 AM rule is enabled. The PDF turnover/edit-consent items require a separate owner policy decision before changing behavior. Refund, zero-payable accounting and settlement-mapping decisions remain Phase 4 work.

Pricing quotes and verification stay within their existing transaction-sensitive flow. The repository checks saved pricing version, registered customer contact data, hold coverage/expiry and active occupied dates. Existing booking reference sequencing is retained. Submission replay additionally rejects reuse under a different Online/WalkIn/Phone source; hashes and keys stored for existing submissions remain compatible.

Razorpay verification queries captured payments from the server using saved order identity and verifies order, amount, currency, capture state and refunded amount. Frontend success is not authoritative. The existing verification endpoint does not accept a client callback as proof of payment; no new signature-only shortcut is added. Payment receipts/history/outbox changes remain in their original transactions.

Notification tests validate logical outbox uniqueness/staging, not exactly-once delivery by external WhatsApp/email providers. Provider failures and network retry limitations remain explicit.

## File map

- Application/Interfaces/Services/Booking/IBookingActorContext.cs and Services/Booking/BookingActorContext.cs
- Application/Services/Booking/BookingSubmissionService.cs
- Application/Services/Booking/BookingDecisionApplicationService.cs
- Application/Services/Booking/BookingStayApplicationService.cs
- Application/Services/Booking/AdminBookingConfirmationService.cs
- Application/Services/Booking/AdvancePaymentApplicationService.cs
- Application/Services/Booking/BookingIdentityApplicationService.cs
- Application/Interfaces/Repositories/Booking: decision, stay, admin confirmation, advance payment and identity repository contracts
- Application/Services/Booking/BookingListService.cs
- Application/Validators/Booking/SubmitBookingValidator.cs
- API Controllers/Booking and existing DependencyInjection registrations
- Existing Infrastructure/Repositories/Booking implementations retain transactional ownership

## Verification approach

A disposable SQL Server fixture executes real application services/repositories with a local fake Razorpay handler. Hosted workers are not started in the test host, so notification staging can be checked without sending messages. Existing reservation records are supplied as fixtures; reservation concurrency is not rebuilt.

Coverage includes submit response-loss replay, concurrent double-click, changed-key/source conflicts, expired holds, customer/tenant isolation, approve/reject retry, gateway capture mismatches and errors, identity privacy, cash advance retry, check-in retry, discounted checkout retry, receipts, actual timestamps and administrative capabilities. HTTP checks exercise real controllers with JWT authentication and shared tenant middleware.

No live booking/payment records are used as test fixtures. No GitHub push is part of Phase 3. Final check results are recorded in evidence.md.
