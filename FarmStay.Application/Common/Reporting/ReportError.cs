using System.Text.Json.Serialization;

namespace FarmStay.Application.Reporting;

public enum ReportError { Unauthorized, Forbidden, NotFound, InvalidRequest, ExportLimit }
