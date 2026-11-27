namespace FarmStay.Application.Reporting;
public interface IReportAccess
{
    Task<IReadOnlySet<string>> PermissionsAsync(int farmhouse, int user, CancellationToken ct);
}
