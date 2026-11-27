using FarmStay.Application.DTOs.Admin;
using FluentValidation;

namespace FarmStay.Application.Validators.Admin
{
    public class CreateGalleryValidator : AbstractValidator<GalleryRequestDto>
    {
        public CreateGalleryValidator()
        {
            RuleFor(x => x.ImageUrl)
                .NotEmpty()
                .WithMessage("Image URL is required.")
                .Must(BeValidUrl)
                .WithMessage("Please enter a valid image URL.");

            RuleFor(x => x.ImageName)
                .NotEmpty()
                .WithMessage("Image name is required.")
                .MaximumLength(500)
                .WithMessage("Image name cannot exceed 500 characters.");

            RuleFor(x => x.Category)
                .NotEmpty()
                .WithMessage("Category is required.")
                .MaximumLength(100)
                .WithMessage("Category cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.FarmHouseId)
                .NotEmpty()
                .WithMessage("FarmHouse is required.")
                .GreaterThan(0)
                .WithMessage("Please select a valid FarmHouse.");
        }

        private bool BeValidUrl(string? url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                   && (uri.Scheme == Uri.UriSchemeHttp ||
                       uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}