using System.Text.Json.Serialization;

namespace FarmStay.Application.Reporting;

public sealed class ReportException(ReportError error, string message) : Exception(message)
{
    public ReportError Error { get; } = error;
}
