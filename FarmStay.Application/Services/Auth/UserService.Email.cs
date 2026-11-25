using System.Security.Claims;
using System.Net;
using FarmStay.Application.BackgroundJobs;
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.Common.Constants;
using FarmStay.Application.DTOs.Auth;
using FarmStay.Domain.Entities;

namespace FarmStay.Application.Services.Auth;

public partial class UserService
{
    public async Task<ApiResponse<VerificationStatusResponseDto>> VerificationStatusAsync(int farmHouseId, int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.FarmHouseId != farmHouseId || !user.IsActive || user.IsDeleted)
            return new() { Message = "User not found." };
        return new() { Success = true, Data = EmailStatus(user) };
    }

    public async Task<ApiResponse<VerificationStatusResponseDto>> UpdateEmailAsync(UpdateEmailRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (!int.TryParse(context?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ||
            !int.TryParse(context?.User.FindFirst(JwtClaimNames.FarmHouseId)?.Value, out var farmHouseId))
            return new() { Message = "Unauthorized." };
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.FarmHouseId != farmHouseId || !user.IsActive || user.IsDeleted)
            return new() { Message = "User not found." };
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        if (email != null && (email.Length > 150 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email)))
            return new() { Message = "Enter a valid email address." };
        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            return new() { Message = "Use Edit Profile to verify and save a new email address." };
        var duplicate = email == null ? null : await _userRepository.GetByEmailAsync(email, farmHouseId);
        if (duplicate != null && duplicate.UserId != userId)
            return new() { Message = "Email is already registered. Please use a different email." };
        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            user.Email = email;
            user.IsEmailVerified = false;
            user.EmailVerificationToken = null;
            user.VerificationTokenExpiry = null;
        }
        if (email != null && !user.IsEmailVerified)
        {
            user.EmailVerificationToken = Guid.NewGuid().ToString("N");
            user.VerificationTokenExpiry = DateTime.Now.AddHours(24);
        }
        user.ModifiedBy = userId;
        user.ModifiedDate = DateTime.Now;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        if (email != null && !user.IsEmailVerified)
        {
            var farm = await _farmHouseRepository.GetByIdAsync(farmHouseId);
            if (farm == null) return new() { Message = "Farmhouse not found." };
            var baseUrl = context!.Request.Host.Host == "localhost" ? "http://localhost:4200" : $"https://{farm.DomainName}";
            var link = $"{baseUrl}/verify-email?farmHouseId={farmHouseId}&userId={userId}&token={Uri.EscapeDataString(user.EmailVerificationToken!)}";
            _emailQueue.Enqueue(new EmailJob {
                To = email, Subject = "Verify your FarmStay email / ईमेल पडताळणी",
                HtmlBody = $"<p>Hello {WebUtility.HtmlEncode(user.FullName)},</p><p><a href='{WebUtility.HtmlEncode(link)}'>Verify email / ईमेल पडताळा</a></p><p>This link expires in 24 hours. / ही लिंक २४ तासांसाठी वैध आहे.</p>",
                IsHtml = true });
        }
        return new() { Success = true, Message = email == null ? "Email removed." :
            user.IsEmailVerified ? "Email is verified." : "Verification email sent. Please verify before creating a booking.", Data = EmailStatus(user) };
    }

    private static VerificationStatusResponseDto EmailStatus(User user) => new()
    {
        HasEmail = !string.IsNullOrWhiteSpace(user.Email),
        IsEmailVerified = user.IsEmailVerified,
        IsMobileVerified = user.IsMobileVerified
    };
}
