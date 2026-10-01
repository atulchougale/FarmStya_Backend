using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Domain.Entities
{
   public  class ContactUs
    {
        [Key]

        public int ContactId { get; set; }

        [Required]
        public string FullName { get; set; }=string.Empty;

        [Required]
        public string Email { get; set; }

        public string Message  { get; set; }

        public int FarmHouseId {  get; set; }

      
        public DateTime CreatedDate { get; set; }
       
                                                  
        public bool IsDelete { get; set; }


        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }
    }
}
