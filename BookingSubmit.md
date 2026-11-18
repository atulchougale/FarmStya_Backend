# Booking Submit API

`POST /api/bookings` requires a customer JWT and the existing `FarmHouseId` header. The API checks the customer's active membership and the farmhouse's Bookings entitlement. If the JWT carries a farmhouse claim, it must match the header.

Send the active `selectionId` and `reservationIds` returned by the reserve or active-reservation endpoint. Use the checkout-exclusive stay range and the `pricingVersion` from `POST /api/booking-pricing/quote`. The optional frontend `pricing` object displays the customer's quote. The server calculates all monetary amounts using active database rates.

```json
{
  "selectionId": "9a4f0d21-5f7b-4f2d-9a12-9a0f7a6b1234",
  "reservationIds": [101, 102, 103],
  "fullName": "Customer Name",
  "mobileNumber": "9876543210",
  "email": "customer@example.com",
  "checkInDate": "2026-12-10",
  "checkOutDate": "2026-12-13",
  "bookingType": "NormalEvent",
  "eventDates": ["2026-12-11", "2026-12-12"],
  "adultCount": 4,
  "childrenCount": 2,
  "personCount": 6,
  "customerNotes": "Birthday celebration",
  "idempotencyKey": "d18f9a1e-5a34-4b3a-b9d1-6d9f8e2a1234",
  "pricingVersion": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
}
```

Use the same key and request when retrying an uncertain result. A first successful request returns `201`. A matching replay returns `200`. Both use the existing response wrapper and include the booking ID, reference, status, stay dates and length, booking type, total, advance, currency, and owner deadline.

For `409 PRICING_CHANGED`, review `data.quote` and submit with the new version after acknowledgement. `HOLD_EXPIRED` requires a new reservation. `DATE_UNAVAILABLE` means another booking won the dates. Reusing a key with different request details returns `DUPLICATE_SUBMISSION`.

Apply migration `20261116210519_AddBookingSubmissionIdempotency` to the intended database before accepting submissions. The migration adds four nullable booking columns and a filtered unique index. The transaction stores the booking, stay dates, event snapshots, status history, and outbox jobs together. The exact matching holds are deleted after commit. A matching replay retries cleanup if that deletion fails.
