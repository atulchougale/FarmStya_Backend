namespace FarmStay.Application.Common.Settings
{
    public class FileUploadSettings
    {
        public const string SectionName = "FileUpload";

        public long MaxImageSizeInBytes { get; set; }
            = 10 * 1024 * 1024; // 10 MB default

        public long MaxVideoSizeInBytes { get; set; }
            = 200 * 1024 * 1024; // 200 MB default

        public List<string> AllowedImageExtensions { get; set; }
            = new();

        public List<string> AllowedVideoExtensions { get; set; }
            = new();

        public List<string> AllowedImageMimeTypes { get; set; }
            = new();

        public List<string> AllowedVideoMimeTypes { get; set; }
            = new();
    }
}