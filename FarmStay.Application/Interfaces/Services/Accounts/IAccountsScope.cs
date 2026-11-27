using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public interface IAccountsScope { Task<AccountScope> RequireAsync(string permission,CancellationToken ct); }
