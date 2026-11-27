using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.Common.Settings;
using FarmStay.Application.DTOs.FileUpload;
using FarmStay.Application.Interfaces.Repositories.FileUpload;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FarmStay.Infrastructure.Services.Common
{
    public class FileUploadRepository : IFileUploadRepository
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FileUploadRepository> _logger;
        private readonly FileUploadSettings _settings;
        private readonly FarmStay.Application.Interfaces.Common.ICurrentTenantContext _tenant;

        public FileUploadRepository(
            IHostEnvironment hostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            ILogger<FileUploadRepository> logger,
            IOptions<FileUploadSettings> settings, FarmStay.Application.Interfaces.Common.ICurrentTenantContext tenant)
        {
            _hostEnvironment = hostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _settings = settings.Value;
            _tenant = tenant;
        }

        public async Task<ApiResponse<FileUploadResponseDto>> UploadFileAsync(
            FileUploadRequestDto request)
        {
            try
            {
                if (request.File == null || request.File.Length == 0)
                {
                    _logger.LogWarning("File upload rejected. File is missing or empty.");

                    return new ApiResponse<FileUploadResponseDto>
                    {
                        Success = false,
                        Message = "File is required.",
                        Data = null
                    };
                }

                if (string.IsNullOrWhiteSpace(request.Folder))
                {
                    _logger.LogWarning(
                        "File upload rejected. Folder is missing. FileName: {FileName}",
                        request.File.FileName);

                    return new ApiResponse<FileUploadResponseDto>
                    {
                        Success = false,
                        Message = "Folder is required.",
                        Data = null
                    };
                }

                var farmHouseId = (await _tenant.RequireAsync()).FarmHouseId;

                var folder = request.Folder.Trim().ToLowerInvariant() switch {"gallery"=>"Gallery","amenity" or "amenities"=>"Amenity","aboutus" or "about-us"=>"AboutUs",_=>string.Empty};
                if(folder.Length==0)return new ApiResponse<FileUploadResponseDto>{Success=false,Message="Unsupported public upload category."};

                if (folder.Contains("..") ||
                    folder.Contains("/") ||
                    folder.Contains("\\"))
                {
                    _logger.LogWarning(
                        "File upload rejected. Invalid folder name. FarmHouseId: {FarmHouseId}, Folder: {Folder}",
                        farmHouseId,
                        folder);

                    return new ApiResponse<FileUploadResponseDto>
                    {
                        Success = false,
                        Message = "Invalid folder name.",
                        Data = null
                    };
                }

                _logger.LogInformation(
                    "File upload started. FarmHouseId: {FarmHouseId}, Folder: {Folder}, OriginalFileName: {FileName}, FileSize: {FileSize}, ContentType: {ContentType}",
                    farmHouseId,
                    folder,
                    request.File.FileName,
                    request.File.Length,
                    request.File.ContentType);

                try {FarmStay.Infrastructure.Services.FileUpload.FileSafety.DisplayName(request.File.FileName);}
                catch(ArgumentException){return new ApiResponse<FileUploadResponseDto>{Success=false,Message="Invalid or unsafe file name."};}
                var extension = Path.GetExtension(request.File.FileName);

                if (string.IsNullOrWhiteSpace(extension))
                {
                    _logger.LogWarning(
                        "File upload rejected. File extension is missing. FarmHouseId: {FarmHouseId}, Folder: {Folder}, FileName: {FileName}",
                        farmHouseId,
                        folder,
                        request.File.FileName);

                    return new ApiResponse<FileUploadResponseDto>
                    {
                        Success = false,
                        Message = "File extension is required.",
                        Data = null
                    };
                }

                var originalExtension = extension.ToLowerInvariant();
                var contentType = request.File.ContentType?.ToLowerInvariant() ?? string.Empty;

                // ============================================
                // Step: Determine file category (Image / Video)
                // and validate extension + MIME type + size
                // against the matching allow-list.
                // Anything not explicitly allowed is rejected
                // (unsafe types like .exe, .php are blocked
                // automatically because they are not listed).
                // ============================================

                var isImage = _settings.AllowedImageExtensions
                    .Contains(originalExtension);

                var isVideo = _settings.AllowedVideoExtensions
                    .Contains(originalExtension);

                if (!isImage && !isVideo)
                {
                    _logger.LogWarning(
                        "File upload rejected. Extension not allowed. FarmHouseId: {FarmHouseId}, Folder: {Folder}, FileName: {FileName}, Extension: {Extension}",
                        farmHouseId,
                        folder,
                        request.File.FileName,
                        originalExtension);

                    return new ApiResponse<FileUploadResponseDto>
                    {
                        Success = false,
                        Message = "This file type is not allowed.",
                        Data = null
                    };
                }

                var allowedMimeTypes = isImage
                    ? _settings.AllowedImageMimeTypes
                    : _settings.AllowedVideoMimeTypes;

                if (allowedMimeTypes.Any() &&
                    !allowedMimeTypes.Contains(contentType))
                {
                    _logger.LogWarning(
                        "File upload rejected. Content type not allowed. FarmHouseId: {FarmHouseId}, Folder: {Folder}, FileName: {FileName}, ContentType: {ContentType}",
                        farmHouseId,
                        folder,
                        request.File.FileName,
                        contentType);

                    return new ApiResponse<FileUploadResponseDto>
                    {
                        Success = false,
                        Message = "This file's content type is not allowed.",
                        Data = null
                    };
                }

                var matchingMime = originalExtension switch {
                    ".jpg" or ".jpeg" => contentType=="image/jpeg", ".png" => contentType=="image/png",
                    ".gif" => contentType=="image/gif", ".webp" => contentType=="image/webp",
                    ".mp4" => contentType=="video/mp4", ".mov" => contentType=="video/quicktime",
                    ".webm" => contentType=="video/webm", _=>false};
                if(!matchingMime)return new ApiResponse<FileUploadResponseDto>{Success=false,Message="File extension and content type do not match."};

                var maxSizeInBytes = isImage
                    ? _settings.MaxImageSizeInBytes
                    : _settings.MaxVideoSizeInBytes;

                if (request.File.Length > maxSizeInBytes)
                {
                    _logger.LogWarning(
                        "File upload rejected. File too large. FarmHouseId: {FarmHouseId}, Folder: {Folder}, FileName: {FileName}, FileSize: {FileSize}, MaxAllowed: {MaxAllowed}",
                        farmHouseId,
                        folder,
                        request.File.FileName,
                        request.File.Length,
                        maxSizeInBytes);

                    var maxSizeInMb = maxSizeInBytes / (1024 * 1024);

                    return new ApiResponse<FileUploadResponseDto>
                    {
                        Success = false,
                        Message = $"File size exceeds the allowed limit of {maxSizeInMb} MB.",
                        Data = null
                    };
                }

                if(System.Text.RegularExpressions.Regex.IsMatch(request.File.FileName,@"\.(php\d*|aspx?|exe|html?|js|svg|cmd|bat|ps1)(\.|$)",System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    return new ApiResponse<FileUploadResponseDto>{Success=false,Message="Unsafe compound filename."};
                var bytes=new byte[32];
                using(var input=request.File.OpenReadStream()) { var count=await input.ReadAtLeastAsync(bytes.AsMemory(),bytes.Length,throwOnEndOfStream:false);
                    bool valid=originalExtension.ToLowerInvariant() switch {
                        ".jpg" or ".jpeg"=>count>=3&&bytes[0]==255&&bytes[1]==216&&bytes[2]==255,
                        ".png"=>count>=8&&bytes.Take(8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}),
                        ".gif"=>count>=6&&System.Text.Encoding.ASCII.GetString(bytes,0,6) is "GIF87a" or "GIF89a",
                        ".webp"=>count>=12&&System.Text.Encoding.ASCII.GetString(bytes,0,4)=="RIFF"&&System.Text.Encoding.ASCII.GetString(bytes,8,4)=="WEBP",
                        ".mp4" or ".mov"=>count>=12&&System.Text.Encoding.ASCII.GetString(bytes,4,4)=="ftyp",
                        ".webm"=>count>=4&&bytes.Take(4).SequenceEqual(new byte[]{26,69,223,163}),
                        _=>false};
                    if(!valid)return new ApiResponse<FileUploadResponseDto>{Success=false,Message="File signature does not match an allowed public media format."};
                }
                using var validatedInput=request.File.OpenReadStream();
                var newFileName=await FarmStay.Infrastructure.Services.FileUpload.FileSafety.StorePublicAsync(
                    validatedInput,_hostEnvironment.ContentRootPath,farmHouseId,folder,originalExtension,maxSizeInBytes,
                    _httpContextAccessor.HttpContext?.RequestAborted??default);

                var requestScheme =
                    _httpContextAccessor.HttpContext?.Request.Scheme;

                var requestHost =
                    _httpContextAccessor.HttpContext?.Request.Host.Value;

                var fileUrl =
                    $"{requestScheme}://{requestHost}/uploads/{farmHouseId}/{folder}/{newFileName}";

                _logger.LogInformation(
                    "File uploaded successfully. FarmHouseId: {FarmHouseId}, Folder: {Folder}, FileName: {FileName}, FileUrl: {FileUrl}",
                    farmHouseId,
                    folder,
                    newFileName,
                    fileUrl);

                return new ApiResponse<FileUploadResponseDto>
                {
                    Success = true,
                    Message = "File uploaded successfully.",
                    Data = new FileUploadResponseDto
                    {
                        FileName = newFileName,
                        FileUrl = fileUrl
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "File upload failed. FarmHouseId: {FarmHouseId}, Folder: {Folder}, OriginalFileName: {FileName}",
                    _httpContextAccessor.HttpContext?.Request.Headers["FarmHouseId"].FirstOrDefault(),
                    request.Folder,
                    request.File?.FileName);

                return new ApiResponse<FileUploadResponseDto>
                {
                    Success = false,
                    Message = "File upload failed.",
                    Data = null
                };
            }
        }
    }
}