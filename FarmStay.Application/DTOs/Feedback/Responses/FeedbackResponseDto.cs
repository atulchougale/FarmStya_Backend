namespace FarmStay.Application.DTOs.Public
{
    public class FeedbackResponseDto
    {

        public int FeedbackId { get; set; }

        public int FarmHouseId { get; set; }

        public string Review { get; set; } = string.Empty;

        public decimal Rating { get; set; }

        public int CreatedBy { get; set; }

        public string CreatedByName { get; set; } = string.Empty;


        public DateTime CreatedDate { get; set; }


    }
}
