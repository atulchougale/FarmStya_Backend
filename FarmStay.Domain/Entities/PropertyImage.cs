using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace FarmStay.Domain.Entities
{
    public class PropertyImage
    {
        [Key] // 👈 ADD THIS
        public int ImageId { get; set; }

        public int PropertyId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public Property? Property { get; set; }
    }
}
