using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.DTOs.Admin
{
    public class AboutUsRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string HeroTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string HeroSubtitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string HeroImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string StoryTitle { get; set; } = string.Empty;

        public string StoryDescription { get; set; } = string.Empty;

        public List<AboutUsFeatureRequestDto> Features { get; set; }
             = new List<AboutUsFeatureRequestDto>();
    }
}