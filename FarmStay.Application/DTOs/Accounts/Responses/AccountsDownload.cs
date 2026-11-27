using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed record AccountsDownload(byte[] Content,string ContentType,string FileName);
