using System.ComponentModel.DataAnnotations;
using FarmStay.Domain.Accounts;
namespace FarmStay.Application.Accounts;
public sealed class AccountQuery
{
    public long? Id { get; set; }
    public int Page { get; set; }=1;
    public int PageSize { get; set; }=25;
    public string Search { get; set; }="";
    public string Sort { get; set; }="date";
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public long? AccountId { get; set; }
    public string? Status { get; set; }
    public string? Kind { get; set; }
    public void Validate() { if(Id is <=0||Page<1||Page>100000||PageSize<1||PageSize>100||Search.Length>100||Sort is not ("date" or "name" or "code")) throw new AccountsException(400,"Invalid pagination, search or sort."); if(From>To) throw new AccountsException(400,"From date must not follow To date."); }
}
