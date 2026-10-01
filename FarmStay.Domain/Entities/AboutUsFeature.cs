using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Domain.Entities
{
    public class AboutUsFeature
    {
        [Key]
        public int FeatureId { get; set; }
        public int AboutUsId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(100)]
        public string Icon { get; set; }

        public int DisplayOrder { get; set; } 

       


        public bool IsDelete { get; set; } = false;

        [ForeignKey(nameof(AboutUsId))]
        public AboutUs? AboutUs { get; set; }

    }
}
