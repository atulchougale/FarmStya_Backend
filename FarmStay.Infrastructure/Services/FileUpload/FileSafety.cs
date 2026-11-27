using System.Security.Cryptography;
using System.Text.RegularExpressions;
namespace FarmStay.Infrastructure.Services.FileUpload;

public static class FileSafety
{
    public static string DisplayName(string name)
    {
        var clean=Path.GetFileName(name.Replace('\\','/'));
        if(string.IsNullOrWhiteSpace(clean)||clean.Length>255||clean.Any(char.IsControl)||
           Regex.IsMatch(clean,@"\.(php\d*|aspx?|exe|html?|js|svg|cmd|bat|ps1)(\.|$)",RegexOptions.IgnoreCase))
            throw new ArgumentException("Invalid or unsafe file name.");
        return clean;
    }
    public static bool ValidProof(string name,string type,byte[] bytes)
    {
        if(bytes.Length is <=0 or >5242880)return false;
        var extension=Path.GetExtension(name).ToLowerInvariant();
        return extension switch {
            ".pdf"=>type=="application/pdf"&&bytes.Length>=5&&System.Text.Encoding.ASCII.GetString(bytes,0,5)=="%PDF-",
            ".jpg" or ".jpeg"=>type=="image/jpeg"&&bytes.Length>=3&&bytes[0]==255&&bytes[1]==216&&bytes[2]==255,
            ".png"=>type=="image/png"&&bytes.Length>=8&&bytes.Take(8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}),
            _=>false};
    }
    public static string PrivateRoot(string root,string publicRoot)
    {
        var full=Path.GetFullPath(root);var web=Path.GetFullPath(publicRoot).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
        if(full.Equals(web,StringComparison.OrdinalIgnoreCase)||full.StartsWith(web+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Private file storage must be outside the public web root.");
        return full;
    }
    // Hash only validated media; staging is private. Rename is atomic and never overwrites an existing asset.
    public static async Task<string> StorePublicAsync(Stream input,string contentRoot,int farm,string category,string extension,long max,CancellationToken ct)
    {
        if(farm<=0||category is not ("Gallery" or "Amenity" or "AboutUs")||!Regex.IsMatch(extension,@"^\.[a-z0-9]+$"))throw new ArgumentException("Invalid media destination.");
        var stageRoot=Path.Combine(contentRoot,"App_Data","UploadStaging");Directory.CreateDirectory(stageRoot);
        var stage=Path.Combine(stageRoot,Guid.NewGuid().ToString("N")+".tmp");
        try {
            using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);long length=0;var buffer=new byte[81920];
            await using(var output=new FileStream(stage,FileMode.CreateNew,FileAccess.Write,FileShare.None,81920,true)) {
                int read;while((read=await input.ReadAsync(buffer,ct))>0){length+=read;if(length>max)throw new ArgumentException("File exceeds the allowed size.");hash.AppendData(buffer,0,read);await output.WriteAsync(buffer.AsMemory(0,read),ct);}
                await output.FlushAsync(ct);
            }
            if(length==0)throw new ArgumentException("File is empty.");
            var file=Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant()+extension;
            var folder=Path.Combine(contentRoot,"wwwroot","uploads",farm.ToString(),category);Directory.CreateDirectory(folder);var destination=Path.Combine(folder,file);
            try{File.Move(stage,destination);}catch(IOException)when(File.Exists(destination)) {
                await using var existing=File.OpenRead(destination);var digest=Convert.ToHexString(await SHA256.HashDataAsync(existing,ct)).ToLowerInvariant()+extension;
                if(digest!=file)throw new IOException("Stored media integrity check failed.");
            }
            return file;
        } finally {if(File.Exists(stage))File.Delete(stage);}
    }
}
