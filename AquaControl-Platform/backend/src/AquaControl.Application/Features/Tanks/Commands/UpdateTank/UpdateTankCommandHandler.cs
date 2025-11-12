using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AquaControl.Application.Features.Tanks.Commands.UpdateTank;

public sealed class UpdateTankCommandHandler : ICommandHandler<UpdateTankCommand>
{
    private readonly ITankRepository _tankRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTankCommandHandler> _logger;

    public UpdateTankCommandHandler(
        ITankRepository tankRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateTankCommandHandler> logger)
    {
        _tankRepository = tankRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTankCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating tank with ID: {TankId}", request.TankId);

        try
        {
            // Get existing tank
            var tank = await _tankRepository.GetByIdAsync(TankId.Create(request.TankId), cancellationToken);
            if (tank == null)
            {
                return Error.NotFound("Tank.NotFound", $"Tank with ID '{request.TankId}' was not found");
            }

            // Check if another tank with same name exists
            var existingTankWithName = await _tankRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingTankWithName != null && existingTankWithName.Id != tank.Id)
            {
                return Error.Conflict(
                    "Tank.NameAlreadyExists",
                    $"Another tank with name '{request.Name}' already exists");
            }

            // Create value objects
            var capacity = TankCapacity.Create(request.Capacity, request.CapacityUnit);
            var location = Location.Create(
                request.Building,
                request.Room,
                request.Zone,
                request.Latitude,
                request.Longitude);

            // Update tank properties
            tank.UpdateName(request.Name);
            tank.UpdateCapacity(capacity);
            tank.Relocate(location);

            // Save changes
            await _tankRepository.UpdateAsync(tank, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tank updated successfully: {TankId}", request.TankId);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating tank: {Error}", ex.Message);
            return Error.Validation("Tank.ValidationError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tank: {TankId}", request.TankId);
            return Error.Failure("Tank.UpdateFailed", "An error occurred while updating the tank");
        }
    }
}

