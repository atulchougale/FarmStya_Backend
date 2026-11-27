using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.DTOs.Admin
{
    public class AboutUsFeatureRequestDto
    {


       
            public int FeatureId { get; set; }

            public string Title { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public string Icon { get; set; } = string.Empty;

            public int DisplayOrder { get; set; }

        
    }
    }

