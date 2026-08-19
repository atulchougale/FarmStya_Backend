using FarmStay.Application.DTOs.Auth;
using FarmStay.Domain.Enums;
using FluentValidation;

namespace FarmStay.Application.Validators.Auth
{
    public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordDtoValidator()
        {
            RuleFor(x => x.Method)
                .IsInEnum()
                .WithMessage("Invalid password reset method.");

            When(x => x.Method == ForgotPasswordMethod.Email, () =>
            {
                RuleFor(x => x.Email)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage("Email is required")
                    .EmailAddress()
                    .WithMessage("Invalid email address");
            });

            When(x => x.Method == ForgotPasswordMethod.WhatsAppOtp, () =>
            {
                RuleFor(x => x.MobileNumber)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage("Mobile number is required")
                    .Matches(@"^[0-9]{10,15}$")
                    .WithMessage("Invalid mobile number");
            });
        }
    }
}