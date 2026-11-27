using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed class AccountReportRow
{
    public long Id { get; set; }
    public string Code { get; set; }="";
    public string Name { get; set; }="";
    public string GroupCode { get; set; }="";
    public DateTime Date { get; set; }
    public string Reference { get; set; }="";
    public string Narration { get; set; }="";
    public decimal Opening { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    public string Bucket { get; set; }="";
}
