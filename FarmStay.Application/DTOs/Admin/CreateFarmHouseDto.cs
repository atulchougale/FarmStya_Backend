namespace FarmStay.Application.DTOs.Admin
{
    public class CreateFarmHouseDto
    {
        // ================= Owner =================

        public int OwnerUserId { get; set; }

        // ================= Basic Information =================

        public string FarmHouseName { get; set; } = string.Empty;

        public string DomainName { get; set; } = string.Empty;

        public string? SubDomain { get; set; }

        public bool IsPrimaryDomain { get; set; } = false;

        public string? TagLine { get; set; }

        public string? Description { get; set; }

        // ================= Address =================

        public string Address { get; set; } = string.Empty;

        public string? Village { get; set; }

        public string Taluka { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string? Pincode { get; set; }

        // ================= Contact =================

        public string ContactPersonName { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public string? AlternateMobileNumber { get; set; }

        public string Email { get; set; } = string.Empty;

        // ================= Media =================

        public string? LogoUrl { get; set; }

        public string? CoverImageUrl { get; set; }

        public string? GoogleMapUrl { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        // ================= Website =================

        public bool IsWebsitePublished { get; set; } = true;

        // ================= Status =================

        public bool IsActive { get; set; } = true;
    }
}