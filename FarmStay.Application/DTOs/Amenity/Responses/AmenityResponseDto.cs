using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.DTOs.Admin
{
   public class AmenityResponseDto
    {
        public int ImageId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsCarasoul { get; set; }

        public bool IsAmenity { get; set; }
        public int FarmHouseId { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedDate { get; set; }

    }
}
