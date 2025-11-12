using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;

namespace AquaControl.Application.Features.Tanks.Commands.ScheduleMaintenance;

public sealed record ScheduleMaintenanceCommand(Guid TankId, DateTime MaintenanceDate, string? Notes) : ICommand<Result>;
