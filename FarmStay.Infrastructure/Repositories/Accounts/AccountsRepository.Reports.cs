using FarmStay.Application.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Reporting.NETCore;
namespace FarmStay.Infrastructure.Accounts;
public sealed partial class AccountsRepository
{
 public static readonly string[] Reports=["ledger","trial-balance","profit-loss","balance-sheet","cashbook","bankbook","daybook","income-expense","receivables","payables","assets","depreciation","reconciliation","tax"];
 public async Task<IReadOnlyList<AccountReportRow>> ReportAsync(AccountScope s,string report,AccountQuery q,bool export,CancellationToken ct) {
  Require(Reports.Contains(report),"Unknown Accounts report.",404);if(q.AccountId.HasValue)await Account(s,q.AccountId.Value,ct);
  if(report=="tax")Require(await db.Set<FarmStay.Domain.Accounts.AccountingSettings>().AnyAsync(x=>x.FarmHouseId==s.FarmHouseId&&x.GstEnabled,ct),"GST is disabled.");
  var from=q.From??new DateTime(DateTime.Today.Month<4?DateTime.Today.Year-1:DateTime.Today.Year,4,1);var to=q.To??DateTime.Today;var offset=export?0:(q.Page-1)*q.PageSize;var take=export?10001:q.PageSize;
  var rows=await db.Set<AccountReportRow>().FromSqlInterpolated($"EXEC dbo.usp_Accounts_Report @FarmHouseId={s.FarmHouseId}, @Report={report}, @From={from}, @To={to}, @AccountId={q.AccountId}, @Search={q.Search}, @Offset={offset}, @Take={take}").AsNoTracking().ToListAsync(ct);
  Require(rows.Count<=10000,"Export exceeds 10,000 rows. Narrow the date range.",413);return rows;
 }
 public async Task<AccountsDownload> ExportAsync(AccountScope s,string report,AccountQuery q,string format,CancellationToken ct) {
  var rows=await ReportAsync(s,report,q,true,ct);using var renderer=new LocalReport();
  using var template=typeof(AccountsRepository).Assembly.GetManifestResourceStream("FarmStay.Infrastructure.Accounts.AccountingReport.rdlc")??throw new InvalidOperationException("Accounts report template missing.");
  renderer.LoadReportDefinition(template);renderer.DataSources.Add(new ReportDataSource("ReportData",rows));
  var farm=await db.FarmHouses.AsNoTracking().SingleAsync(x=>x.FarmHouseId==s.FarmHouseId,ct);
  var selectedFrom=q.From??new DateTime(DateTime.Today.Month<4?DateTime.Today.Year-1:DateTime.Today.Year,4,1);var selectedTo=q.To??DateTime.Today;
  renderer.SetParameters([new ReportParameter("Address",$"{farm.Address} | {farm.MobileNumber}"),new ReportParameter("Description","Posted accounting records | INR"),new ReportParameter("Generated",$"Generated {DateTime.Now:dd MMM yyyy HH:mm}"),new ReportParameter("Title",report.Replace('-',' ').ToUpperInvariant()),new ReportParameter("Farmhouse",farm.FarmHouseName),new ReportParameter("Selection",$"{selectedFrom:dd MMM yyyy} to {selectedTo:dd MMM yyyy}")]);
  var bytes=renderer.Render(format=="pdf"?"PDF":"EXCELOPENXML");Audit(s,"Export",report,0,$"{format}: {rows.Count} rows");await db.SaveChangesAsync(ct);
  return new(bytes,format=="pdf"?"application/pdf":"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",$"accounts-{report}.{(format=="pdf"?"pdf":"xlsx")}");
 }
}
