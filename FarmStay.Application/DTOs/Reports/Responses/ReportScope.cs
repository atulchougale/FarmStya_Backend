using System.Text.Json.Serialization;

namespace FarmStay.Application.Reporting;

public sealed record ReportScope(int FarmHouseId, int UserId, string FarmHouseName, string Address,
    string Contact, bool GlobalAdministrator, IReadOnlySet<string> Permissions, string CorrelationId);
