using FarmStay.API.Middleware;
using FarmStay.Application.Interfaces.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;

var storage=Path.Combine(Path.GetTempPath(),"FarmStay_Media_"+Guid.NewGuid().ToString("N"));
var gallery=Path.Combine(storage,"uploads","1","Gallery");Directory.CreateDirectory(gallery);
var bytes=Enumerable.Range(0,256).Select(n=>(byte)n).ToArray();
await File.WriteAllBytesAsync(Path.Combine(gallery,"fixture.mp4"),bytes);
await File.WriteAllBytesAsync(Path.Combine(gallery,"fixture.jpg"),bytes);
var builder=WebApplication.CreateBuilder(new WebApplicationOptions{WebRootPath=storage});
builder.Logging.ClearProviders();builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddSingleton<IPublicMediaAccess,FixtureMediaAccess>();
builder.Services.AddCors(o=>o.AddPolicy("Fixture",p=>p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
await using var app=builder.Build();app.UseRouting();app.UseCors("Fixture");app.UseMiddleware<PublicMediaBoundaryMiddleware>();app.UseStaticFiles();
await app.StartAsync();using var client=new HttpClient{BaseAddress=new Uri(app.Urls.Single())};
int passed=0;
void Check(bool value,string name){if(!value)throw new Exception(name);passed++;Console.WriteLine("PASS "+name);}
async Task<HttpResponseMessage> Video(string? range=null,HttpMethod? method=null){var r=new HttpRequestMessage(method??HttpMethod.Get,"/uploads/1/Gallery/fixture.mp4?media-cache=2");r.Headers.Add("Origin","http://localhost:4200");if(range!=null)r.Headers.Range=RangeHeaderValue.Parse(range);return await client.SendAsync(r);}
try{
 using(var r=await Video()){
  Check(r.StatusCode==HttpStatusCode.OK,"Full video returns 200");
  Check((await r.Content.ReadAsByteArrayAsync()).SequenceEqual(bytes),"Full video bytes unchanged");
  Check(r.Headers.CacheControl is {NoStore:true,Private:true},"Video bypasses sparse browser disk cache");
  Check(r.Headers.GetValues("Access-Control-Allow-Origin").Single()=="http://localhost:4200","Anonymous CORS thumbnail decoding supported");
 }
 using(var r=await Video("bytes=16-31")){
  Check(r.StatusCode==HttpStatusCode.PartialContent,"Video seek returns 206");
  Check(r.Content.Headers.ContentRange?.From==16&&r.Content.Headers.ContentRange?.To==31,"Content-Range is correct");
  Check((await r.Content.ReadAsByteArrayAsync()).SequenceEqual(bytes.Skip(16).Take(16)),"Seek returns only requested bytes");
  Check(r.Headers.CacheControl?.NoStore==true,"Partial video also has no-store");
 }
 using(var r=await Video(method:HttpMethod.Head))Check(r.IsSuccessStatusCode&&r.Content.Headers.ContentLength==256,"HEAD supports metadata");
 using(var r=await Video("bytes=999-1000"))Check(r.StatusCode==HttpStatusCode.RequestedRangeNotSatisfiable,"Invalid range returns 416");
 var requests=await Task.WhenAll(Enumerable.Range(0,8).Select(_=>Video("bytes=0-15")));
 Check(requests.All(r=>r.StatusCode==HttpStatusCode.PartialContent&&r.Headers.CacheControl?.NoStore==true),"Concurrent video range requests remain valid");foreach(var r in requests)r.Dispose();
 using(var r=await client.GetAsync("/uploads/1/Gallery/fixture.jpg"))Check(r.Headers.CacheControl is {NoCache:true,NoStore:false},"Image revalidation policy unchanged");
 using(var r=await client.GetAsync("/uploads/2/Gallery/fixture.mp4"))Check(r.StatusCode==HttpStatusCode.NotFound,"Foreign farmhouse media denied");
 using(var r=await client.GetAsync("/uploads/1/Booking/fixture.mp4"))Check(r.StatusCode==HttpStatusCode.NotFound,"Private document location not exposed");
 using(var r=await Video(method:HttpMethod.Post))Check(r.StatusCode==HttpStatusCode.NotFound,"Non-read public media request denied");
 Console.WriteLine($"TOTAL passed={passed} failed=0; fixture files: {storage}");
}finally{await app.StopAsync();}
sealed class FixtureMediaAccess:IPublicMediaAccess{
 public Task<bool> CanReadAsync(string path,CancellationToken ct=default)=>Task.FromResult(path.StartsWith("/uploads/1/Gallery/fixture.",StringComparison.Ordinal));
}
