using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed class LedgerInput
{
    [Required,MaxLength(40)] public string Code { get; set; }="";
    [Required,MaxLength(150)] public string Name { get; set; }="";
    [Required] public string GroupCode { get; set; }="ASSET";
    public long? ParentId { get; set; }
    public bool IsActive { get; set; }=true;
}
