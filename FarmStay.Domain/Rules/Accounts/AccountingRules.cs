using System.ComponentModel.DataAnnotations;
namespace FarmStay.Domain.Accounts;

public static class AccountingRules
{
    public static readonly string[] Groups = ["ASSET", "LIABILITY", "EQUITY", "INCOME", "DIRECT_EXP", "INDIRECT_EXP"];
    public static readonly string[] VoucherTypes = ["Journal","Receipt","Payment","Transfer","Income","Expense","VendorBill","VendorPayment","OpeningBalance","Depreciation","Reversal","Adjustment"];
    public static void Money(decimal value, bool allowZero = false)
    {
        if (value < 0 || (!allowZero && value == 0) || value > 999999999999.99m || decimal.Round(value,2) != value)
            throw new ArgumentException("Amounts must be positive, within limits, and have at most two decimal places.");
    }
    public static void ValidateLines(IEnumerable<VoucherLine> input)
    {
        var lines = input.ToList();
        if (lines.Count < 2 || lines.Count > 200) throw new ArgumentException("A voucher needs 2–200 lines.");
        foreach(var l in lines) { Money(l.Debit,true); Money(l.Credit,true); if ((l.Debit>0)==(l.Credit>0)) throw new ArgumentException("Each line must have exactly one positive debit or credit."); }
        if (lines.Sum(l=>l.Debit)!=lines.Sum(l=>l.Credit)) throw new ArgumentException("Voucher debits and credits must balance.");
    }
    public static decimal Depreciation(decimal cost, decimal residual, decimal posted, int lifeMonths, string method, decimal rate)
    {
        var remaining = Math.Max(0,cost-residual-posted);
        if (lifeMonths <= 0 || residual < 0 || residual > cost || rate < 0 || rate > 100 || method is not ("SLM" or "WDV")) throw new ArgumentException("Invalid depreciation policy.");
        return Math.Min(remaining, decimal.Round(method=="SLM" ? (cost-residual)/lifeMonths : (cost-posted)*rate/1200m,2,MidpointRounding.AwayFromZero));
    }
}
