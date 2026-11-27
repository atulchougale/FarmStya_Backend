using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed class VendorInput
{
    [Required,MaxLength(40)] public string Code { get; set; }="";
    [Required,MaxLength(150)] public string Name { get; set; }="";
    [MaxLength(20)] public string? Mobile { get; set; }
    [EmailAddress,MaxLength(150)] public string? Email { get; set; }
    [MaxLength(500)] public string? Address { get; set; }
    [MaxLength(20)] public string? GstNumber { get; set; }
    public bool IsActive { get; set; }=true;
}
