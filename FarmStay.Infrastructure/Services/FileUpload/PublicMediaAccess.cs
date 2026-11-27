using FarmStay.Application.Interfaces.Common;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
namespace FarmStay.Infrastructure.Services.FileUpload;
public sealed class PublicMediaAccess(IPublicTenantResolver tenant,AppDbContext db):IPublicMediaAccess
{
    public async Task<bool> CanReadAsync(string path,CancellationToken ct=default)
    {
        var match=Regex.Match(path,@"^/uploads/([1-9]\d*)/(Gallery|Amenity|AboutUs|FarmHouse)/[a-zA-Z0-9_-]+\.(jpg|jpeg|png|gif|webp|mp4|mov|webm)$",RegexOptions.IgnoreCase);
        var logo=Regex.IsMatch(path,@"^/uploads/logos/[a-zA-Z0-9_-]+\.(jpg|jpeg|png|gif|webp)$",RegexOptions.IgnoreCase);
        if(!match.Success&&!logo)return false;
        var farm=await tenant.ResolveAsync(ct);
        if(match.Success)return int.TryParse(match.Groups[1].Value,out var owner)&&owner==farm;
        var url=await db.FarmHouses.Where(f=>f.FarmHouseId==farm).Select(f=>f.LogoUrl).SingleAsync(ct);
        if(url==null)return false;
        return (Uri.TryCreate(url,UriKind.Absolute,out var absolute)?absolute.AbsolutePath:url)==path;
    }
}
