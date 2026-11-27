namespace FarmStay.Application.Interfaces.Common;
public sealed record VerifiedTenant(int FarmHouseId,int UserId,int MembershipId,string Role,string CorrelationId);
public interface ICurrentTenantContext
{
 Task<VerifiedTenant> RequireAsync(CancellationToken ct=default);
 Task DemandAsync(params string[] anyPermissions);
}
public interface ICurrentUserContext { Task<VerifiedTenant> RequireAsync(CancellationToken ct=default); }
public interface IPublicTenantResolver {Task<int> ResolveAsync(CancellationToken ct=default);}
public sealed class TenantAccessException(int status,string message):Exception(message){public int Status{get;}=status;}
