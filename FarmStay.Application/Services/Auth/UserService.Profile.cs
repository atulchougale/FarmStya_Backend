using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FarmStay.Application.BackgroundJobs;
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.Common.Constants;
using FarmStay.Application.DTOs.Auth;
using FarmStay.Domain.Entities;
using FarmStay.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FarmStay.Application.Services.Auth;

public partial class UserService
{
    private async Task<User?> ProfileUserAsync()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (!int.TryParse(principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ||
            !int.TryParse(principal?.FindFirst(JwtClaimNames.FarmHouseId)?.Value, out var farmId)) return null;
        var user = await _userRepository.GetByIdAsync(id);
        var membership = await _userMembershipRepository.GetMembershipAsync(id, farmId);
        return user is { IsActive: true, IsDeleted: false } && membership is { IsActive: true, IsDeleted: false, Role: { IsActive: true, IsDeleted: false } } ? user : null;
    }

    private static string ContactValue(string? value, bool mobile) => mobile ? value?.Trim() ?? "" : value?.Trim().ToLowerInvariant() ?? "";
    private static bool ValidContact(string value, bool mobile) => mobile ? Regex.IsMatch(value, @"^[0-9]{10}$") :
        value.Length is > 0 and <= 150 && new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(value);
    private static string ProfileCodeHash(string code) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
    private static OtpPurpose ProfilePurpose(bool mobile) => mobile ? OtpPurpose.ProfileMobile : OtpPurpose.ProfileEmail;
    private async Task<bool> ContactAvailableAsync(User user, string value, bool mobile)
    {
        var other = mobile ? await _userRepository.GetByMobileNumberAsync(value, user.FarmHouseId) : await _userRepository.GetByEmailAsync(value, user.FarmHouseId);
        return other == null || other.UserId == user.UserId;
    }

    public async Task<ApiResponse<bool>> RequestProfileContactAsync(ProfileContactRequest request)
    {
        var user = await ProfileUserAsync();
        if (user == null) return new() { Message = "Unauthorized." };
        if (request.Channel is not ("mobile" or "email")) return new() { Message = "Invalid verification channel." };
        var mobile = request.Channel == "mobile";
        var value = ContactValue(request.Value, mobile);
        if (!ValidContact(value, mobile)) return new() { Message = "Enter a valid contact value." };
        if (!await ContactAvailableAsync(user, value, mobile)) return new() { Message = "This contact is already registered." };
        var existing = await _userOtpRepository.GetActiveOtpAsync(user.UserId, ProfilePurpose(mobile));
        if (existing != null && existing.CreatedDate > DateTime.Now.AddSeconds(-60)) return new() { Message = "Please wait 60 seconds before requesting another code." };
        if (existing != null) { existing.IsActive = false; await _userOtpRepository.UpdateAsync(existing); }
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        await _userOtpRepository.AddAsync(new UserOtp {
            UserId = user.UserId, MobileNumber = mobile ? value : user.MobileNumber, Target = value,
            OtpCode = "", OtpHash = ProfileCodeHash(code), Purpose = ProfilePurpose(mobile),
            ExpiryDate = DateTime.Now.AddMinutes(10), CreatedDate = DateTime.Now, CreatedBy = user.UserId
        });
        await _unitOfWork.SaveChangesAsync();
        var message = $"Your FarmStay profile verification code is {code}. Valid for 10 minutes. Do not share it. / तुमचा FarmStay प्रोफाइल पडताळणी कोड {code} आहे. वैधता १० मिनिटे. हा कोड कोणालाही सांगू नका.";
        if (mobile) _whatsAppQueue.Enqueue(new WhatsAppJob { MobileNumber = value, Message = message });
        else _emailQueue.Enqueue(new EmailJob { To = value, Subject = "FarmStay profile verification / प्रोफाइल पडताळणी", HtmlBody = message, IsHtml = false });
        _logger.LogInformation("Profile {Channel} verification queued for user {UserId}.", request.Channel, user.UserId);
        return new() { Success = true, Data = true, Message = "Verification code sent. It is valid for 10 minutes." };
    }

    public async Task<ApiResponse<bool>> VerifyProfileContactAsync(VerifyProfileContactRequest request)
    {
        var user = await ProfileUserAsync();
        if (user == null) return new() { Message = "Unauthorized." };
        if (request.Channel is not ("mobile" or "email")) return new() { Message = "Invalid verification channel." };
        var mobile = request.Channel == "mobile";
        var otp = await _userOtpRepository.GetActiveOtpAsync(user.UserId, ProfilePurpose(mobile));
        if (otp == null || otp.ExpiryDate <= DateTime.Now || otp.FailedAttempts >= 5 || otp.Target != ContactValue(request.Value, mobile))
            return new() { Message = "Code expired or invalid. Request a new code." };
        if (!Regex.IsMatch(request.Code ?? "", @"^[0-9]{6}$") || otp.OtpHash == null ||
            !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(otp.OtpHash), Encoding.UTF8.GetBytes(ProfileCodeHash(request.Code!))))
        {
            otp.FailedAttempts++;
            await _userOtpRepository.UpdateAsync(otp);
            await _unitOfWork.SaveChangesAsync();
            return new() { Message = "Incorrect verification code." };
        }
        otp.VerifiedAt = DateTime.Now;
        await _userOtpRepository.UpdateAsync(otp);
        await _unitOfWork.SaveChangesAsync();
        return new() { Success = true, Data = true, Message = "Contact verified. Save Changes to apply it." };
    }

    public async Task<ApiResponse<bool>> UpdateProfileAsync(UpdateProfileRequest request)
    {
        var user = await ProfileUserAsync();
        if (user == null) return new() { Message = "Unauthorized." };
        var name = request.FullName?.Trim() ?? "";
        var mobile = ContactValue(request.MobileNumber, true);
        var email = ContactValue(request.Email, false);
        if (name.Length is < 1 or > 150 || !ValidContact(mobile, true) || (email.Length > 0 && !ValidContact(email, false)))
            return new() { Message = "Enter valid profile details." };
        var proofs = new List<UserOtp>();
        foreach (var channel in new[] { true, false })
        {
            var value = channel ? mobile : email;
            if (!channel && value.Length == 0) continue;
            var current = ContactValue(channel ? user.MobileNumber : user.Email, channel);
            var verified = channel ? user.IsMobileVerified : user.IsEmailVerified;
            if (current == value && verified) continue;
            var proof = await _userOtpRepository.GetActiveOtpAsync(user.UserId, ProfilePurpose(channel));
            if (proof?.Target != value || proof.VerifiedAt == null || proof.ExpiryDate <= DateTime.Now || proof.FailedAttempts >= 5)
                return new() { Message = "Verify the changed mobile number and email before saving." };
            proofs.Add(proof);
        }
        if (!await ContactAvailableAsync(user, mobile, true) || (email.Length > 0 && !await ContactAvailableAsync(user, email, false)))
            return new() { Message = "This contact is already registered." };
        user.FullName = name; user.MobileNumber = mobile; user.IsMobileVerified = true;
        user.Email = email.Length == 0 ? null : email; user.IsEmailVerified = email.Length > 0;
        user.EmailVerificationToken = null; user.VerificationTokenExpiry = null;
        user.ModifiedBy = user.UserId; user.ModifiedDate = DateTime.Now;
        foreach (var proof in proofs) { proof.IsUsed = true; proof.IsActive = false; await _userOtpRepository.UpdateAsync(proof); }
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Profile updated for user {UserId}.", user.UserId);
        return new() { Success = true, Data = true, Message = "Profile updated successfully." };
    }
}
