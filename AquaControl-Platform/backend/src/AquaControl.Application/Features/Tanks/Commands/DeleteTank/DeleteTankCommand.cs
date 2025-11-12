using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;

namespace AquaControl.Application.Features.Tanks.Commands.DeleteTank;

public sealed record DeleteTankCommand(Guid TankId) : ICommand<Result>;
