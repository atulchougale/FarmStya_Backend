namespace FarmStay.Application.DTOs.Reports.Receipts.Responses;
public sealed class PaymentReceiptReportRow
{
    public string? ReceiptNumber { get; set; }
    public string? BookingReference { get; set; }
    public string? FarmHouseName { get; set; }
    public string? Address { get; set; }
    public string? FarmPhone { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public string? CheckInDate { get; set; }
    public string? CheckOutDate { get; set; }
    public string? BookingType { get; set; }
    public string? PaymentType { get; set; }
    public string? PaymentMethod { get; set; }
    public string? CurrencyCode { get; set; }
    public string? IssuedAt { get; set; }
    public string? Amount { get; set; }
    public string? TotalAmount { get; set; }
    public string? Discount { get; set; }
    public string? TotalPaid { get; set; }
    public string? Remaining { get; set; }
    public string? PaymentReference { get; set; }
}
