using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;

namespace AquaControl.Application.Features.Tanks.Commands.DeactivateTank;

public sealed record DeactivateTankCommand(Guid TankId, string Reason) : ICommand<Result>;
