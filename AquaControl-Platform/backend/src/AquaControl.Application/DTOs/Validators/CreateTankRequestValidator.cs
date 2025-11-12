using FluentValidation;
using AquaControl.Application.Features.Tanks.Commands.CreateTank;

namespace AquaControl.Application.DTOs.Validators;

public class CreateTankRequestValidator : AbstractValidator<CreateTankCommand>
{
    public CreateTankRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tank name is required")
            .Length(2, 100)
            .WithMessage("Tank name must be between 2 and 100 characters")
            .Matches("^[a-zA-Z0-9\\s._-]+$")
            .WithMessage("Tank name can only contain letters, numbers, spaces, dots, underscores, and hyphens");

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .WithMessage("Tank capacity must be greater than 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Tank capacity cannot exceed 1,000,000");

        RuleFor(x => x.CapacityUnit)
            .NotEmpty()
            .WithMessage("Capacity unit is required")
            .Length(1, 10)
            .WithMessage("Capacity unit must be between 1 and 10 characters");

        RuleFor(x => x.Building)
            .NotEmpty()
            .WithMessage("Building is required")
            .Length(1, 100)
            .WithMessage("Building must be between 1 and 100 characters");

        RuleFor(x => x.Room)
            .NotEmpty()
            .WithMessage("Room is required")
            .Length(1, 100)
            .WithMessage("Room must be between 1 and 100 characters");

        RuleFor(x => x.Zone)
            .Length(0, 50)
            .WithMessage("Zone cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Zone));

        RuleFor(x => x.TankType)
            .IsInEnum()
            .WithMessage("Invalid tank type specified");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90 degrees")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180 degrees")
            .When(x => x.Longitude.HasValue);
    }
}
