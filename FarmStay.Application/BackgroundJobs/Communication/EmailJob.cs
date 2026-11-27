namespace FarmStay.Application.BackgroundJobs
{
    public class EmailJob
    {
        public string To { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string HtmlBody { get; set; } = string.Empty;

        public bool IsHtml { get; set; } = true;
    }
}