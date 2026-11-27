using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.DTOs.Admin
{
  public class AboutUsResponseDto
    {
        public int AboutUsId { get; set; }

    public string HeroTitle { get; set; } = string.Empty;

    public string HeroSubtitle { get; set; } = string.Empty;

    public string HeroImageUrl { get; set; } = string.Empty;

    public string StoryTitle { get; set; } = string.Empty;

    public string StoryDescription { get; set; } = string.Empty;

    public int FarmHouseId { get; set; }

        public List<AboutUsFeatureResponseDto> Features { get; set; }
                = new List<AboutUsFeatureResponseDto>();
    }



   
        public class AboutUsFeatureResponseDto
        {
            public int FeatureId { get; set; }

            public int AboutUsId { get; set; }

            public string Title { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public string Icon { get; set; } = string.Empty;

            public int DisplayOrder { get; set; }
        }
    }

 