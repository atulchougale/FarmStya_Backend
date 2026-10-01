using FarmStay.Application.DTOs.Auth;
using FluentValidation;

namespace FarmStay.Application.Validators.Auth
{
    public class SendLoginOtpRequestDtoValidator : AbstractValidator<SendLoginOtpRequestDto>
    {
        public SendLoginOtpRequestDtoValidator()
        {
            RuleFor(x => x.MobileNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Mobile number is required")
                .Matches(@"^[0-9]{10}$")
                .WithMessage("Mobile number must be exactly 10 digits");
        }
    }
}