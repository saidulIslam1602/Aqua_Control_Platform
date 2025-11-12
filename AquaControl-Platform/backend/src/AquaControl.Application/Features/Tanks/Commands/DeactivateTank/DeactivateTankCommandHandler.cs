using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Domain.Aggregates.TankAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AquaControl.Application.Features.Tanks.Commands.DeactivateTank;

public sealed class DeactivateTankCommandHandler : IRequestHandler<DeactivateTankCommand, Result>
{
    private readonly ITankRepository _tankRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeactivateTankCommandHandler> _logger;

    public DeactivateTankCommandHandler(
        ITankRepository tankRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeactivateTankCommandHandler> logger)
    {
        _tankRepository = tankRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeactivateTankCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deactivating tank with ID: {TankId}, Reason: {Reason}", request.TankId, request.Reason);

            var tankId = TankId.Create(request.TankId);
            var tank = await _tankRepository.GetByIdAsync(tankId, cancellationToken);

            if (tank is null)
            {
                _logger.LogWarning("Tank not found: {TankId}", request.TankId);
                return Error.NotFound("Tank.NotFound", $"Tank with ID {request.TankId} was not found");
            }

            // Domain method handles business rules
            tank.Deactivate(request.Reason);

            await _tankRepository.UpdateAsync(tank, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tank deactivated successfully: {TankId}", request.TankId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating tank: {TankId}", request.TankId);
            return Error.Failure("Tank.DeactivationFailed", "Failed to deactivate tank");
        }
    }
}
