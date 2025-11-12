using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Features.Tanks.DTOs;

namespace AquaControl.Application.Features.Tanks.Queries.GetTankById;

public sealed record GetTankByIdQuery(Guid TankId) : IQuery<TankDto>;

