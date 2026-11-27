using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed class MasterInput
{
    [Required,MaxLength(40)] public string Kind { get; set; }="";
    [Required,MaxLength(40)] public string Code { get; set; }="";
    [Required,MaxLength(150)] public string Name { get; set; }="";
    [MaxLength(1000)] public string? Description { get; set; }
    public string? DefaultLedgerCode { get; set; }
    public string? GroupCode { get; set; }
    public decimal? Rate { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }=true;
}
