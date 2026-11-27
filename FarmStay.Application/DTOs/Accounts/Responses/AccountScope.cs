using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed record AccountScope(int FarmHouseId,int UserId,string CorrelationId,IReadOnlySet<string> Permissions);
