using FarmStay.Application.DTOs.Admin;
using FluentValidation;

namespace FarmStay.Application.Validators.Admin
{
    public class CreateFarmHouseValidator : AbstractValidator<CreateFarmHouseDto>
    {
        public CreateFarmHouseValidator()
        {
            RuleFor(x => x.FarmHouseName)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.DomainName)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.SubDomain)
                .MaximumLength(100);

            RuleFor(x => x.TagLine)
                .MaximumLength(250);

            RuleFor(x => x.Description)
                .MaximumLength(500);

            RuleFor(x => x.Address)
                .NotEmpty()
                .MaximumLength(250);

            RuleFor(x => x.Village)
                .MaximumLength(100);

            RuleFor(x => x.Taluka)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.District)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.State)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Pincode)
                .MaximumLength(10);

            RuleFor(x => x.ContactPersonName)
                .NotEmpty()
                .MaximumLength(100);
                
            RuleFor(x => x.MobileNumber)
                .NotEmpty()
                .MaximumLength(15);

            RuleFor(x => x.AlternateMobileNumber)
                .MaximumLength(15);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(150);

            RuleFor(x => x.LogoUrl)
                .MaximumLength(500);

            RuleFor(x => x.CoverImageUrl)
                .MaximumLength(500);

            RuleFor(x => x.GoogleMapUrl)
                .MaximumLength(500);

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .When(x => x.Latitude.HasValue);

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .When(x => x.Longitude.HasValue);
        }
    }
}