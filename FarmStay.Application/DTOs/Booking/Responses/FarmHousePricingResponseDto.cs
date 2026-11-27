namespace FarmStay.Application.DTOs.Booking
{
    public class FarmHousePricingResponseDto
    {
        public int PricingId { get; set; }

        public string PricingKey { get; set; } = string.Empty;

        public string PricingName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string CurrencyCode { get; set; } = "INR";
    }
}