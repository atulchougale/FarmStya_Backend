using FarmStay.Application.DTOs.Auth;
using FluentValidation;

namespace FarmStay.Application.Validators.Auth
{
    public class ResetPasswordOtpDtoValidator : AbstractValidator<ResetPasswordOtpDto>
    {
        public ResetPasswordOtpDtoValidator()
        {
            RuleFor(x => x.MobileNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Mobile number is required")
                .Matches(@"^[0-9]{10,15}$")
                .WithMessage("Invalid mobile number");

            RuleFor(x => x.OtpCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("OTP is required")
                .Length(6)
                .WithMessage("OTP must be 6 digits");

            RuleFor(x => x.NewPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("New password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long")
                .Matches("[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter")
                .WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]")
                .WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]")
                .WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]")
                .WithMessage("Password must contain at least one special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Confirm password is required")
                .Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match");
        }
    }
}