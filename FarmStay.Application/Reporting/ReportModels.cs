namespace FarmStay.Application.Reporting;

public sealed class ReportFilter
{
    public string Period { get; set; } = "month";
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string BookingType { get; set; } = "all";
    public string Source { get; set; } = "all";
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;

    public (DateTime? Start, DateTime? End, string Label) Resolve()
    {
        if (Page < 1 || Page > 1000000 || PageSize is < 1 or > 100 || Search?.Length > 100)
            throw new ArgumentException("Use a valid page, page size and search of up to 100 characters.");
        if (BookingType is not ("all" or "normal" or "event") || Source is not ("all" or "online" or "assisted"))
            throw new ArgumentException("Select valid booking type and booking source filters.");
        DateOnly start, end;
        switch (Period)
        {
            case "all": return (null, null, "All dates");
            case "range":
                if (!FromDate.HasValue || !ToDate.HasValue || ToDate < FromDate || ToDate.Value.Year >= 9999)
                    throw new ArgumentException("Choose a valid From date and To date.");
                start = FromDate.Value; end = ToDate.Value.AddDays(1); break;
            case "month":
                if (Year is < 1900 or > 9998 || !Year.HasValue || Month is < 1 or > 12 || !Month.HasValue)
                    throw new ArgumentException("Choose a valid month and year.");
                start = new(Year.Value, Month.Value, 1); end = start.AddMonths(1); break;
            case "year":
                if (Year is < 1900 or > 9998 || !Year.HasValue) throw new ArgumentException("Choose a valid year.");
                start = new(Year.Value, 1, 1); end = start.AddYears(1); break;
            default: throw new ArgumentException("Select Date range, Month, Year or All.");
        }
        return (start.ToDateTime(TimeOnly.MinValue), end.ToDateTime(TimeOnly.MinValue), $"{start:dd MMM yyyy} to {end.AddDays(-1):dd MMM yyyy}");
    }
}

public sealed record ReportColumn(string Key, string Label, string Kind = "text");
public sealed record ReportDefinition(string Key, string Title, string Description, string Permission, IReadOnlyList<ReportColumn> Columns);
public static class ReportCatalog
{
    public static readonly IReadOnlyList<ReportDefinition> All = [
        new("bookings", "Booking Report", "Bookings by submission date, including cancelled and rejected bookings.", "BookingReport.View", [
            new("BookingReference","Booking number"), new("FullName","Customer"), new("MobileNumber","Mobile"),
            new("CreatedDate","Booked on","date"), new("CheckInDate","Check-in","date"), new("CheckOutDate","Check-out","date"),
            new("BookingType","Booking type"), new("BookingSource","Booking source"), new("PersonCount","Guests","number"),
            new("Status","Status"), new("TotalAmount","Booking amount (INR)","money")]),
        new("revenue", "Revenue Report", "Successful advance and checkout payments by payment date. Amounts are payments received, not booking value.", "RevenueReport.View", [
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
public sealed record ReportDownload(byte[] Content, string ContentType, string FileName);
