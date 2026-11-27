namespace FarmStay.Application.DTOs.Public
{
    public class PublicSiteDto
    {
        public int FarmHouseId { get; set; }

        public string FarmHouseName { get; set; } = string.Empty;

        public string DomainName { get; set; } = string.Empty;

        public string? SubDomain { get; set; }

        public bool IsPrimaryDomain { get; set; }

        public string? TagLine { get; set; }

        public string? Description { get; set; }

        public string Address { get; set; } = string.Empty;

        public string? Village { get; set; }

        public string Taluka { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string? Pincode { get; set; }

        public string ContactPersonName { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public string? AlternateMobileNumber { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? LogoUrl { get; set; }

        public string? CoverImageUrl { get; set; }

        public string? GoogleMapUrl { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
    }
}