namespace FarmStay.Application.Reporting;

public sealed record ReportDownload(byte[] Content, string ContentType, string FileName);
