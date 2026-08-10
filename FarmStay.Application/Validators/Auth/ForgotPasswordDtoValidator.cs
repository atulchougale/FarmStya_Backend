using FarmStay.Application.DTOs.Auth;
using FluentValidation;

namespace FarmStay.Application.Validators.Auth
{
    public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordDtoValidator()
        {
            RuleFor(x => x.MobileNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Mobile number is required")
            .Matches(@"^[0-9]{10,15}$")
            .WithMessage("Invalid mobile number");
        }
    }
}
