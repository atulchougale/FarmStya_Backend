using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Domain.Entities
{
    public class AboutUs
    {
        [Key]
        public int AboutUsId { get; set; }

        [Required]
        [MaxLength(200)]
        public string HeroTitle { get; set; }=string.Empty;
       
        
        [Required]
        [MaxLength(500)]
        public string HeroSubtitle { get; set; }= string.Empty;

        [Required]
        [MaxLength(1000)]
        public string HeroImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string StoryTitle { get; set; }

        [MaxLength]
        public string StoryDescription { get; set; }


        public int FarmHouseId {  get; set; }
        public bool IsDelete { get; set; }=false;

        public int CreatedBy {  get; set; }

        public DateTime CreatedDate { get; set; }

        public int ModifyBy { get; set; }
        public DateTime ModifyDate { get; set; }



        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }
        // AboutUs -> Features
        public ICollection<AboutUsFeature> Features { get; set; }
            = new List<AboutUsFeature>();
    }
}
