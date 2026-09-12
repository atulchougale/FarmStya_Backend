using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FarmStay.Domain.Entities
{
    public class Gallery
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        [MaxLength(2048)]
        [Url]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ImageName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category {  get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public int  FarmHouseId { get; set; }

        public bool IsFavorite { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public int  CreatedBy { get; set; } 
        public DateTime CreatedDate { get; set; }
        
        public int ModifyBy { get; set; }
        
        public DateTime ModifyDate {  get; set; }

        public bool IsDelete {  get; set; } 



        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }


    }
}
