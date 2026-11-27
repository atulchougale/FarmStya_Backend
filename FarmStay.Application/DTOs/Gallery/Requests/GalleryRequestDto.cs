namespace FarmStay.Application.DTOs.Admin
{
    public class GalleryRequestDto
    {
         public int ImageId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string ImageName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsFavorite { get; set; }

        public int DisplayOrder { get; set; }
        public int FarmHouseId { get; set; }
    }
}