using FarmStay.Application.DTOs.Auth;
using FluentValidation;

namespace FarmStay.Application.Validators.Auth
{
    public class LoginDtoValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email or mobile number is required")
                .Must(value => System.Text.RegularExpressions.Regex.IsMatch(value ?? "", @"^[0-9]{10}$") ||
                    new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(value))
                .WithMessage("Enter a valid email or 10-digit mobile number");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required");
        }
    }
}