using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Application.Features.Tanks.DTOs;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AquaControl.Application.Features.Tanks.Queries.GetTankById;

public sealed class GetTankByIdQueryHandler : IQueryHandler<GetTankByIdQuery, TankDto>
{
    private readonly ITankRepository _tankRepository;
    private readonly ILogger<GetTankByIdQueryHandler> _logger;

    public GetTankByIdQueryHandler(ITankRepository tankRepository, ILogger<GetTankByIdQueryHandler> logger)
    {
        _tankRepository = tankRepository;
        _logger = logger;
    }

    public async Task<Result<TankDto>> Handle(GetTankByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tank with ID: {TankId}", request.TankId);

        try
        {
            var tank = await _tankRepository.GetByIdAsync(TankId.Create(request.TankId), cancellationToken);
            
            if (tank == null)
            {
                return Error.NotFound("Tank.NotFound", $"Tank with ID '{request.TankId}' was not found");
            }

            var tankDto = new TankDto
            {
                Id = tank.Id.Value,
                Name = tank.Name,
                Capacity = tank.Capacity.Value,
                CapacityUnit = tank.Capacity.Unit,
                Location = new LocationDto
                {
                    Building = tank.Location.Building,
                    Room = tank.Location.Room,
                    Zone = tank.Location.Zone,
                    Latitude = tank.Location.Latitude,
                    Longitude = tank.Location.Longitude,
                    FullAddress = tank.Location.GetFullAddress()
                },
                TankType = tank.TankType.ToString(),
                Status = tank.Status.ToString(),
                IsActive = tank.Status == TankStatus.Active,
                SensorCount = tank.Sensors.Count,
                ActiveSensorCount = tank.GetActiveSensors().Count(),
                CreatedAt = tank.CreatedAt,
                UpdatedAt = tank.UpdatedAt,
                LastMaintenanceDate = tank.LastMaintenanceDate,
                NextMaintenanceDate = tank.NextMaintenanceDate,
                IsMaintenanceDue = tank.IsMaintenanceDue(),
                Sensors = tank.Sensors.Select(s => new SensorDto
                {
                    Id = s.Id.Value,
                    SensorType = s.SensorType.ToString(),
                    Model = s.Model,
                    Manufacturer = s.Manufacturer,
                    SerialNumber = s.SerialNumber,
                    IsActive = s.IsActive,
                    Status = s.Status.ToString(),
                    MinValue = s.MinValue,
                    MaxValue = s.MaxValue,
                    Accuracy = s.Accuracy,
                    InstallationDate = s.InstallationDate,
                    CalibrationDate = s.CalibrationDate,
                    NextCalibrationDate = s.NextCalibrationDate,
                    IsCalibrationDue = s.IsCalibrationDue()
                }).ToList()
            };

            return Result.Success(tankDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tank with ID: {TankId}", request.TankId);
            return Error.Failure("Tank.GetFailed", "An error occurred while retrieving the tank");
        }
    }
}

