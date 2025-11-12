using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Domain.Aggregates.TankAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AquaControl.Application.Features.Tanks.Commands.ActivateTank;

public sealed class ActivateTankCommandHandler : IRequestHandler<ActivateTankCommand, Result>
{
    private readonly ITankRepository _tankRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateTankCommandHandler> _logger;

    public ActivateTankCommandHandler(
        ITankRepository tankRepository,
        IUnitOfWork unitOfWork,
        ILogger<ActivateTankCommandHandler> logger)
    {
        _tankRepository = tankRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ActivateTankCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Activating tank with ID: {TankId}", request.TankId);

            var tankId = TankId.Create(request.TankId);
            var tank = await _tankRepository.GetByIdAsync(tankId, cancellationToken);

            if (tank is null)
            {
                _logger.LogWarning("Tank not found: {TankId}", request.TankId);
                return Error.NotFound("Tank.NotFound", $"Tank with ID {request.TankId} was not found");
            }

            // Domain method handles business rules
            tank.Activate();

            await _tankRepository.UpdateAsync(tank, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tank activated successfully: {TankId}", request.TankId);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot activate tank: {TankId}", request.TankId);
            return Error.Conflict("Tank.CannotActivate", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating tank: {TankId}", request.TankId);
            return Error.Failure("Tank.ActivationFailed", "Failed to activate tank");
        }
    }
}
