using FarmStay.Application.DTOs.Auth;
using FluentValidation;

namespace FarmStay.Application.Validators.Auth
{
    public class VerifyLoginOtpRequestDtoValidator : AbstractValidator<VerifyLoginOtpRequestDto>
    {
        public VerifyLoginOtpRequestDtoValidator()
        {
            RuleFor(x => x.MobileNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Mobile number is required")
                .Matches(@"^[0-9]{10}$")
                .WithMessage("Mobile number must be exactly 10 digits");

            RuleFor(x => x.OtpCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("OTP is required")
                .Matches(@"^[0-9]{6}$")
                .WithMessage("OTP must be 6 digits");
        }
    }
}