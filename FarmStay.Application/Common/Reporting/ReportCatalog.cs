namespace FarmStay.Application.Reporting;

public static class ReportCatalog
{
    public static readonly IReadOnlyList<ReportDefinition> All = [
        new("bookings", "Booking Report", "Bookings by submission date, including cancelled and rejected bookings.", "BookingReport.View", [
            new("BookingReference","Booking number"), new("FullName","Customer"), new("MobileNumber","Mobile"),
            new("CreatedDate","Booked on","date"), new("CheckInDate","Check-in","date"), new("CheckOutDate","Check-out","date"),
            new("BookingType","Booking type"), new("BookingSource","Booking source"), new("PersonCount","Guests","number"),
            new("Status","Status"), new("TotalAmount","Booking amount (INR)","money")]),
        new("revenue", "Booking Collections Report", "Collections received: successful advance and checkout payments by payment date. Amounts are payments received, not booking value.", "RevenueReport.View", [
            new("PaymentTransactionId","Payment ID","number"),new("BookingReference","Booking number"),new("FullName","Customer"),
            new("ReceivedAt","Payment date","date"),new("PaymentType","Payment type"),new("PaymentMethod","Payment method"),
            new("Amount","Received (INR)","money"),new("ProviderPaymentId","Gateway reference")]),
        new("customers", "Customer Report", "Unique customers with bookings submitted in the selected period. Counts include all booking statuses.", "CustomerReport.View", [
            new("CustomerUserId","Customer ID","number"),new("FullName","Customer"),new("MobileNumber","Mobile"),new("Email","Email"),
            new("RegisteredAt","Registered on","date"),new("BookingCount","Bookings","number"),
            new("LastBookingAt","Last booking","date"),new("BookingValue","Booking value (INR)","money")]),
        new("occupancy", "Occupancy Report", "Scheduled occupied stay dates for Confirmed, CheckedIn and CheckedOut bookings. Cancelled bookings are excluded.", "OccupancyReport.View", [
            new("StayDate","Stay date","date"),new("BookingCount","Bookings","number"),new("Adults","Adults","number"),
            new("Children","Children","number"),new("Guests","Guests","number")])
    ];
    public static ReportDefinition Find(string key) => All.SingleOrDefault(r => r.Key == key)
        ?? throw new ArgumentException("Unknown report.");
}
