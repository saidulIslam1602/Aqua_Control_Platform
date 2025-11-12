using AquaControl.Application.Common.Interfaces;
using AquaControl.Domain.Enums;

namespace AquaControl.Application.Features.Tanks.Commands.UpdateTank;

public sealed record UpdateTankCommand(
    Guid TankId,
    string Name,
    decimal Capacity,
    string CapacityUnit,
    string Building,
    string Room,
    string? Zone,
    decimal? Latitude,
    decimal? Longitude,
    TankType TankType
) : ICommand;

