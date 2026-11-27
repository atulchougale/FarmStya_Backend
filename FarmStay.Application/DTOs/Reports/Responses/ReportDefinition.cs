namespace FarmStay.Application.Reporting;

public sealed record ReportDefinition(string Key, string Title, string Description, string Permission, IReadOnlyList<ReportColumn> Columns);
