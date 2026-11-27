using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed record AccountPage<T>(IReadOnlyList<T> Items,int Total,int Page,int PageSize);
