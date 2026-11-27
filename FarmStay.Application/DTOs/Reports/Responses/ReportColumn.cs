namespace FarmStay.Application.Reporting;

public sealed record ReportColumn(string Key, string Label, string Kind = "text");
