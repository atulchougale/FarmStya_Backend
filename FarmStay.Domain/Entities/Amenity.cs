using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
  public class Amenity
    {
        [Key]
        public int ImageId {  get; set; }
        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int FarmHouseId { get; set; }
        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int ModifyBy { get; set; }
        public DateTime ModifyDate { get; set; }
        public bool IsDelete { get; set; }

        public bool IsAmenity { get; set; }

        public bool IsCarasoul { get; set; }



        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

    }
}
