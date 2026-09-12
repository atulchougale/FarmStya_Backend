using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class FeedBack
    {
        [Key]
        public int FeedBackId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public string Review { get; set; } = string.Empty;
        [Required]
        public decimal Rating { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int ModifyBy { get; set; }

        public DateTime ModifyDate { get; set; }

        public bool IsDelete { get; set; }



        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }
    }
}
