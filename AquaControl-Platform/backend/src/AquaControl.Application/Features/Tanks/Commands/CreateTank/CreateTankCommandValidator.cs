using FluentValidation;

namespace AquaControl.Application.Features.Tanks.Commands.CreateTank;

public sealed class CreateTankCommandValidator : AbstractValidator<CreateTankCommand>
{
    public CreateTankCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tank name is required")
            .MaximumLength(100).WithMessage("Tank name must not exceed 100 characters")
            .Matches("^[a-zA-Z0-9\\s\\-_]+$").WithMessage("Tank name contains invalid characters");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero")
            .LessThanOrEqualTo(1000000).WithMessage("Capacity cannot exceed 1,000,000 liters");

        RuleFor(x => x.CapacityUnit)
            .NotEmpty().WithMessage("Capacity unit is required")
            .Must(BeValidUnit).WithMessage("Invalid capacity unit. Allowed: L, ML, GAL");

        RuleFor(x => x.Building)
            .NotEmpty().WithMessage("Building is required")
            .MaximumLength(50).WithMessage("Building name must not exceed 50 characters");

        RuleFor(x => x.Room)
            .NotEmpty().WithMessage("Room is required")
            .MaximumLength(50).WithMessage("Room name must not exceed 50 characters");

        RuleFor(x => x.Zone)
            .MaximumLength(50).WithMessage("Zone name must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Zone));

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.TankType)
            .IsInEnum().WithMessage("Invalid tank type");
    }

    private static bool BeValidUnit(string unit)
    {
        var validUnits = new[] { "L", "ML", "GAL" };
        return validUnits.Contains(unit.ToUpperInvariant());
    }
}

