using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;

namespace AquaControl.Application.Features.Tanks.Commands.ActivateTank;

public sealed record ActivateTankCommand(Guid TankId) : ICommand<Result>;
