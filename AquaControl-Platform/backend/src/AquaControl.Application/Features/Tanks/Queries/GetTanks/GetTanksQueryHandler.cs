using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using AquaControl.Application.Common.Specifications;
using AquaControl.Application.Features.Tanks.DTOs;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AquaControl.Application.Features.Tanks.Queries.GetTanks;

public sealed class GetTanksQueryHandler : IQueryHandler<GetTanksQuery, PagedResult<TankDto>>
{
    private readonly ITankRepository _tankRepository;
    private readonly ILogger<GetTanksQueryHandler> _logger;

    public GetTanksQueryHandler(ITankRepository tankRepository, ILogger<GetTanksQueryHandler> logger)
    {
        _tankRepository = tankRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<TankDto>>> Handle(GetTanksQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tanks with filters: {@Filters}", request);

        try
        {
            var specification = new TankFilterSpecification(
                request.SearchTerm,
                ParseTankType(request.TankType),
                request.IsActive);

            var (tanks, totalCount) = await _tankRepository.GetPagedAsync(
                specification,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortDescending,
                cancellationToken);

            var tankDtos = tanks.Select(tank => new TankDto
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
                IsMaintenanceDue = tank.IsMaintenanceDue()
            }).ToList();

            var result = new PagedResult<TankDto>
            {
                Items = tankDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tanks");
            return Error.Failure("Tank.GetFailed", "An error occurred while retrieving tanks");
        }
    }

    private static TankType? ParseTankType(string? tankType)
    {
        if (string.IsNullOrEmpty(tankType))
            return null;

        return Enum.TryParse<TankType>(tankType, true, out var result) ? result : null;
    }
}

