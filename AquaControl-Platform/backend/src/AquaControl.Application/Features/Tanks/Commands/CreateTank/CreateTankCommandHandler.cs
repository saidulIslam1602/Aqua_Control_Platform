using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AquaControl.Application.Features.Tanks.Commands.CreateTank;

public sealed class CreateTankCommandHandler : ICommandHandler<CreateTankCommand, Guid>
{
    private readonly ITankRepository _tankRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTankCommandHandler> _logger;

    public CreateTankCommandHandler(
        ITankRepository tankRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateTankCommandHandler> logger)
    {
        _tankRepository = tankRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateTankCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating tank with name: {TankName}", request.Name);

        try
        {
            // Check if tank with same name already exists
            var existingTank = await _tankRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingTank != null)
            {
                return Error.Conflict(
                    "Tank.NameAlreadyExists",
                    $"Tank with name '{request.Name}' already exists");
            }

            // Create value objects
            var capacity = TankCapacity.Create(request.Capacity, request.CapacityUnit);
            var location = Location.Create(
                request.Building,
                request.Room,
                request.Zone,
                request.Latitude,
                request.Longitude);

            // Create tank aggregate
            var tank = Tank.Create(request.Name, capacity, location, request.TankType);

            // Save to repository
            await _tankRepository.AddAsync(tank, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tank created successfully with ID: {TankId}", tank.Id);
            
            return Result.Success(tank.Id);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating tank: {Error}", ex.Message);
            return Error.Validation("Tank.ValidationError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tank with name: {TankName}", request.Name);
            return Error.Failure("Tank.CreationFailed", "An error occurred while creating the tank");
        }
    }
}

