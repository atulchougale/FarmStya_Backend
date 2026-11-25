namespace FarmStay.Application.DTOs.Auth;
public record UpdateProfileRequest(string FullName, string MobileNumber, string? Email);
public record ProfileContactRequest(string Channel, string Value);
public record VerifyProfileContactRequest(string Channel, string Value, string Code);
