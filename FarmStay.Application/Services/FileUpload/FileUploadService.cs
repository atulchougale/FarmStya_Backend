using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.FileUpload;
using FarmStay.Application.Interfaces.Services.Common;
using FarmStay.Application.Interfaces.Repositories.FileUpload;
namespace FarmStay.Application.Services.FileUpload;
public sealed class FileUploadService(IFileUploadRepository repository, FarmStay.Application.Interfaces.Common.ICurrentTenantContext tenant) : IFileUploadService
{
    public async Task<ApiResponse<FileUploadResponseDto>> UploadFileAsync(FileUploadRequestDto request)
    {
        var feature=(request.Folder??"").Trim().ToLowerInvariant() switch {"gallery"=>"Gallery","amenity" or "amenities"=>"Amenity","aboutus" or "about-us"=>"AboutUs",_=>throw new FarmStay.Application.Interfaces.Common.TenantAccessException(400,"Unsupported public upload category.")};
        request.Folder=feature;
        await tenant.DemandAsync(feature+".Create",feature+".Edit");
        return await repository.UploadFileAsync(request);
    }
}
