using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed class AccountsException(int status,string message) : Exception(message) { public int Status { get; }=status; }
