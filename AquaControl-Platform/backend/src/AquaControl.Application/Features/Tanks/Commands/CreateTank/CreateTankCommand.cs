using AquaControl.Application.Common.Interfaces;
using AquaControl.Domain.Enums;

namespace AquaControl.Application.Features.Tanks.Commands.CreateTank;

public sealed record CreateTankCommand(
    string Name,
    decimal Capacity,
    string CapacityUnit,
    string Building,
    string Room,
    string? Zone,
    decimal? Latitude,
    decimal? Longitude,
    TankType TankType
) : ICommand<Guid>;

