using FarmStay.Application.DTOs.Auth;
using FluentValidation;

namespace FarmStay.Application.Validators.Auth
{
    public class ResetPasswordEmailDtoValidator : AbstractValidator<ResetPasswordByEmailDto>
    {
        public ResetPasswordEmailDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("Invalid user.");

            RuleFor(x => x.Token)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Reset token is required");

            RuleFor(x => x.NewPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("New password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long")
                .Matches("[A-Z]")
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