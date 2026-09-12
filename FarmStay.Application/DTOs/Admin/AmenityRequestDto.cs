using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.DTOs.Admin
{
     public class AmenityRequestDto
    {
        public int ImageId {  get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string Title { get; set; }= string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsAmenity { get; set; }

        public bool IsCarasoul { get; set; }

        public int FarmHouseId { get; set; }
        
    }
}
