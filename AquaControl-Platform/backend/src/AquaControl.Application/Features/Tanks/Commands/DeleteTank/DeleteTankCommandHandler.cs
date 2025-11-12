using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Domain.Aggregates.TankAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AquaControl.Application.Features.Tanks.Commands.DeleteTank;

public sealed class DeleteTankCommandHandler : IRequestHandler<DeleteTankCommand, Result>
{
    private readonly ITankRepository _tankRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteTankCommandHandler> _logger;

    public DeleteTankCommandHandler(
        ITankRepository tankRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteTankCommandHandler> logger)
    {
        _tankRepository = tankRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTankCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting tank with ID: {TankId}", request.TankId);

            var tankId = TankId.Create(request.TankId);
            var tank = await _tankRepository.GetByIdAsync(tankId, cancellationToken);

            if (tank is null)
            {
                _logger.LogWarning("Tank not found: {TankId}", request.TankId);
                return Error.NotFound("Tank.NotFound", $"Tank with ID {request.TankId} was not found");
            }

            // Business rule: Cannot delete active tank
            if (tank.Status == Domain.Enums.TankStatus.Active)
            {
                _logger.LogWarning("Cannot delete active tank: {TankId}", request.TankId);
                return Error.Conflict("Tank.CannotDeleteActive", "Cannot delete an active tank. Deactivate it first.");
            }

            await _tankRepository.DeleteAsync(tank, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tank deleted successfully: {TankId}", request.TankId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tank: {TankId}", request.TankId);
            return Error.Failure("Tank.DeletionFailed", "Failed to delete tank");
        }
    }
}
