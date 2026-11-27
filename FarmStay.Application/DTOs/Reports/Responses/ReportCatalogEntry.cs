using System.Text.Json.Serialization;

namespace FarmStay.Application.Reporting;

public sealed record ReportCatalogEntry(string Key, string Title, string Description,
    IReadOnlyList<ReportColumn> Columns, bool CanExport);
