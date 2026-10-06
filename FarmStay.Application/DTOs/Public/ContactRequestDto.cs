using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.DTOs.Public
{
    public class ContactRequestDto
    {
        public int ContactId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string MobileNo { get; set; } = string.Empty;


        public string Message { get; set; } = string.Empty;

        public int FarmHouseId {  get; set; }
    }
}
