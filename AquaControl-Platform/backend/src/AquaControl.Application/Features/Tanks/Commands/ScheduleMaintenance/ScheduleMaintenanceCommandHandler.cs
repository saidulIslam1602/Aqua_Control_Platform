using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Domain.Aggregates.TankAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AquaControl.Application.Features.Tanks.Commands.ScheduleMaintenance;

public sealed class ScheduleMaintenanceCommandHandler : IRequestHandler<ScheduleMaintenanceCommand, Result>
{
    private readonly ITankRepository _tankRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScheduleMaintenanceCommandHandler> _logger;

    public ScheduleMaintenanceCommandHandler(
        ITankRepository tankRepository,
        IUnitOfWork unitOfWork,
        ILogger<ScheduleMaintenanceCommandHandler> logger)
    {
        _tankRepository = tankRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ScheduleMaintenanceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Scheduling maintenance for tank with ID: {TankId}, Date: {MaintenanceDate}", 
                request.TankId, request.MaintenanceDate);

            var tankId = TankId.Create(request.TankId);
            var tank = await _tankRepository.GetByIdAsync(tankId, cancellationToken);

            if (tank is null)
            {
                _logger.LogWarning("Tank not found: {TankId}", request.TankId);
                return Error.NotFound("Tank.NotFound", $"Tank with ID {request.TankId} was not found");
            }

            // Validate maintenance date
            if (request.MaintenanceDate <= DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid maintenance date: {MaintenanceDate} for tank: {TankId}", 
                    request.MaintenanceDate, request.TankId);
                return Error.Validation("Tank.InvalidMaintenanceDate", "Maintenance date must be in the future");
            }

            // Domain method handles business rules
            tank.ScheduleMaintenance(request.MaintenanceDate);

            await _tankRepository.UpdateAsync(tank, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Maintenance scheduled successfully for tank: {TankId}", request.TankId);
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid maintenance scheduling for tank: {TankId}", request.TankId);
            return Error.Validation("Tank.InvalidMaintenance", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling maintenance for tank: {TankId}", request.TankId);
            return Error.Failure("Tank.MaintenanceSchedulingFailed", "Failed to schedule maintenance");
        }
    }
}
