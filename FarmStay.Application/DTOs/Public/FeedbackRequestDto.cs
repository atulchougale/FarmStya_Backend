

namespace FarmStay.Application.DTOs.Public
{
    public class FeedbackRequestDto
    {

        public int FeedBackId { get; set; }


        public int FarmHouseId { get; set; }

        public string Review { get; set; } = string.Empty;

        public decimal Rating { get; set; }
    }
}
