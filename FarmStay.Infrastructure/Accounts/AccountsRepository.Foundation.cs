using Microsoft.Extensions.Logging;
using FarmStay.Domain.Accounts;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FarmStay.Application.Accounts;
namespace FarmStay.Infrastructure.Accounts;
public sealed partial class AccountsRepository(AppDbContext db,Microsoft.Extensions.Logging.ILogger<AccountsRepository> logger):IAccountsRepository
{
 public static readonly (string Code,string Name,string Group)[] Chart = [
 ("1000","Cash in Hand","ASSET"),("1010","Bank Accounts Control","ASSET"),("1100","Accounts Receivable - Customers","ASSET"),("1200","Input GST","ASSET"),("1500","Fixed Assets","ASSET"),("1590","Accumulated Depreciation","ASSET"),("2000","Accounts Payable - Vendors","LIABILITY"),("2100","Customer Advances","LIABILITY"),("2200","Output GST","LIABILITY"),("3000","Owner Capital","EQUITY"),("3100","Retained Earnings / Opening Equity","EQUITY"),("4000","Booking Revenue","INCOME"),("4010","Other Operating Income","INCOME"),("4020","Miscellaneous Income","INCOME"),("5000","Direct Booking / Service Costs","DIRECT_EXP"),("6000","Electricity Expense","INDIRECT_EXP"),("6010","Water Expense","INDIRECT_EXP"),("6020","Rent Expense","INDIRECT_EXP"),("6030","Gas / Fuel Expense","INDIRECT_EXP"),("6040","Repairs & Maintenance","INDIRECT_EXP"),("6050","Labour / Wages Expense","INDIRECT_EXP"),("6060","Cleaning & Housekeeping","INDIRECT_EXP"),("6070","Internet & Telephone","INDIRECT_EXP"),("6080","Office & Administration","INDIRECT_EXP"),("6090","Depreciation Expense","INDIRECT_EXP"),("6100","Bank Charges","INDIRECT_EXP"),("6110","Travel & Conveyance","INDIRECT_EXP"),("6120","Consumables & Supplies","INDIRECT_EXP"),("6130","Miscellaneous Expense","INDIRECT_EXP") ];
 public static readonly IReadOnlyDictionary<string,string[]> Catalogue=new Dictionary<string,string[]> {
 ["Group"]=["ASSET","LIABILITY","EQUITY","INCOME","DIRECT_EXP","INDIRECT_EXP"],
 ["ExpenseCategory"]=["Electricity|6000","Water|6010","Rent / Lease|6020","Gas / Fuel|6030","Repairs & Maintenance|6040","Labour / Wages|6050","Cleaning / Housekeeping|6060","Internet / Telephone|6070","Equipment Maintenance|6040","Consumables / Supplies|6120","Travel / Conveyance|6110","Bank Charges|6100","Office / Administration|6080","Insurance|6130","Taxes / Licences|6130","Miscellaneous|6130"],
 ["IncomeCategory"]=["Booking Income|4000","Event / Decoration Income|4000","Other Operating Income|4010","Miscellaneous Income|4020"],
 ["AssetCategory"]=["Furniture & Fixtures","Electrical Equipment","Kitchen Equipment","Refrigeration","Air Conditioning","Laundry Equipment","Computers & Office Equipment","Vehicles","Building & Improvements","Water Systems & Pumps","Generator & Power Backup","Other Fixed Assets"],
 ["AssetType"]=["Refrigerator","AC","Washing Machine","Mixer Grinder","TV","Water Pump","Generator","Inverter","Furniture Set","Computer","Printer","Solar Equipment"],
 ["PaymentMode"]=["Cash","UPI","Bank Transfer","Debit Card","Credit Card","Cheque","Other"],
 ["Unit"]=["Unit","Piece","Set","Litre","Millilitre","Kilogram","Gram","Metre","Square Metre","Month","Day","Hour","Service"],
 ["DepreciationMethod"]=["SLM","WDV"], ["CostCentre"]=["Administration","Maintenance","Housekeeping","Operations","Kitchen","Marketing"],
 ["VoucherType"]=AccountingRules.VoucherTypes };
 public async Task InitializeAsync(int tenant,int actor,CancellationToken ct)
 {
  await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
  var module=await db.Modules.SingleAsync(x=>x.ModuleName=="Accounts",ct);
  if(!await db.farmHouseModules.AnyAsync(x=>x.FarmHouseId==tenant&&x.ModuleId==module.ModuleId,ct)) db.Add(new FarmStay.Domain.Entities.FarmHouseModule{FarmHouseId=tenant,ModuleId=module.ModuleId,CreatedBy=actor});
  var globals=await db.Set<AccountingMaster>().Where(x=>x.FarmHouseId==null).ToListAsync(ct);
  foreach(var (kind,values) in Catalogue) foreach(var (value,index) in values.Select((v,i)=>(v,i))) {
   var parts=value.Split('|'); var code=kind=="Group"?parts[0]:$"{kind.ToUpperInvariant()}_{index+1:D2}";
   if(!globals.Any(x=>x.Kind==kind&&x.Code==code)) db.Add(new AccountingMaster{Kind=kind,Code=code,Name=parts[0],DefaultLedgerCode=parts.ElementAtOrDefault(1),IsSystemDefined=true,CreatedBy=actor});
  }
  if(!await db.Set<AccountingSettings>().AnyAsync(x=>x.FarmHouseId==tenant,ct)) db.Add(new AccountingSettings{FarmHouseId=tenant,CreatedBy=actor});
  var existing=await db.Set<LedgerAccount>().Where(x=>x.FarmHouseId==tenant).Select(x=>x.Code).ToListAsync(ct);
  foreach(var (code,name,group) in Chart) if(!existing.Contains(code)) db.Add(new LedgerAccount{FarmHouseId=tenant,Code=code,Name=name,GroupCode=group,NormalBalance=code=="1590"||group is "LIABILITY" or "EQUITY" or "INCOME"?"Credit":"Debit",IsSystemDefined=true,IsTaxAccount=code is "1200" or "2200",CreatedBy=actor});
  await EnsureYear(tenant,actor,DateTime.Today,ct); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
 }
 async Task<FinancialYear> EnsureYear(int tenant,int actor,DateTime date,CancellationToken ct)
 {
  var start=new DateTime(date.Month<4?date.Year-1:date.Year,4,1);
  var y=await db.Set<FinancialYear>().Include(x=>x.Periods).SingleOrDefaultAsync(x=>x.FarmHouseId==tenant&&x.FromDate==start,ct);
  if(y!=null)return y;
  y=new(){FarmHouseId=tenant,CreatedBy=actor,Name=$"{start.Year}-{start.Year+1}",FromDate=start,ToDate=start.AddYears(1).AddDays(-1)};
  for(int i=0;i<12;i++)y.Periods.Add(new(){FarmHouseId=tenant,CreatedBy=actor,FromDate=start.AddMonths(i),ToDate=start.AddMonths(i+1).AddDays(-1)});
  db.Add(y); await db.SaveChangesAsync(ct); return y;
 }
 void Audit(AccountScope s,string action,string entity,long id,string summary) {
  db.Add(new AccountingAudit{FarmHouseId=s.FarmHouseId,CreatedBy=s.UserId,Action=action,Entity=entity,EntityId=id,CorrelationId=s.CorrelationId,Summary=summary});
  logger.LogInformation("Accounts {Action} {Entity} {EntityId}; tenant {Tenant}, actor {Actor}, correlation {Correlation}",action,entity,id,s.FarmHouseId,s.UserId,s.CorrelationId);
 }
 static void Require(bool condition,string message,int status=400) { if(!condition)throw new AccountsException(status,message); }
 async Task<T> Owned<T>(AccountScope s,long id,CancellationToken ct) where T:AccountRecord => await db.Set<T>().SingleOrDefaultAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.Id==id,ct)??throw new AccountsException(404,"Record not found.");
 async Task<LedgerAccount> Account(AccountScope s,long id,CancellationToken ct) { var a=await Owned<LedgerAccount>(s,id,ct); Require(a.IsActive,"Inactive account cannot be used."); if(a.IsTaxAccount)Require(await db.Set<AccountingSettings>().AnyAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.GstEnabled,ct),"GST is disabled."); return a; }
 async Task<LedgerAccount> Code(AccountScope s,string code,CancellationToken ct)=>await db.Set<LedgerAccount>().SingleAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.Code==code,ct);
 async Task<AccountingMaster?> Master(AccountScope s,long? id,string kind,CancellationToken ct) {
  if(id==null)return null;
  var m=await db.Set<AccountingMaster>().SingleOrDefaultAsync(x=>x.Id==id&&(x.FarmHouseId==null||x.FarmHouseId==s.FarmHouseId)&&x.Kind==kind&&x.IsActive,ct);
  Require(m!=null,"Invalid or unavailable "+kind+".");return m;
 }
 async Task<AccountPage<T>> Paged<T>(IQueryable<T> query,AccountQuery q,CancellationToken ct) where T:class { if(q.Sort is "name" or "code") {var property=q.Sort=="name"?"Name":"Code";Require(typeof(T).GetProperty(property)!=null,"Sorting is unavailable for this list.");query=query.OrderBy(x=>EF.Property<object>(x,property));} var total=await query.CountAsync(ct); return new(await query.Skip((q.Page-1)*q.PageSize).Take(q.PageSize).ToListAsync(ct),total,q.Page,q.PageSize); }
}
