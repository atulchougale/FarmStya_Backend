using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.Common.Settings;
using FarmStay.Application.DTOs.FileUpload;
using FarmStay.Application.Interfaces.Services.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FarmStay.Infrastructure.Services.Common
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FileUploadService> _logger;
        private readonly FileUploadSettings _settings;

        public FileUploadService(
            IHostEnvironment hostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            ILogger<FileUploadService> logger,
            IOptions<FileUploadSettings> settings)
        {
            _hostEnvironment = hostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _settings = settings.Value;
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

                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request.Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out var farmHouseId) ||
                    farmHouseId <= 0)
                {
                    _logger.LogWarning(
                        "File upload rejected. Invalid FarmHouseId header. FileName: {FileName}",
                        request.File.FileName);

                    return new ApiResponse<FileUploadResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse.",
                        Data = null
                    };
                }

                var folder = request.Folder.Trim();

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

                var dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                var randomNumber = Random.Shared.Next(1000, 10000);

                var newFileName =
                    $"farmstay_{folder}_{farmHouseId}_{dateTime}_{randomNumber}{originalExtension}";

                var uploadsFolder = Path.Combine(
                    _hostEnvironment.ContentRootPath,
                    "wwwroot",
                    "uploads",
                    farmHouseId.ToString(),
                    folder);

                Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(
                    uploadsFolder,
                    newFileName);

                await using (var stream = new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await request.File.CopyToAsync(stream);
                }

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