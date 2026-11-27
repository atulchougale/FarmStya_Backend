using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

public abstract class AccountRecord
{
    public long Id { get; set; }
    public int FarmHouseId { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    [Timestamp] public byte[] Version { get; set; } = [];
}
