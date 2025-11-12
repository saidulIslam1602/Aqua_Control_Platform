using AquaControl.Application.Features.Tanks.Commands.CreateTank;
using AquaControl.Application.Features.Tanks.Commands.UpdateTank;
using AquaControl.Application.Features.Tanks.Commands.DeleteTank;
using AquaControl.Application.Features.Tanks.Commands.ActivateTank;
using AquaControl.Application.Features.Tanks.Commands.DeactivateTank;
using AquaControl.Application.Features.Tanks.Commands.ScheduleMaintenance;
using AquaControl.Application.Features.Tanks.Queries.GetTanks;
using AquaControl.Application.Features.Tanks.Queries.GetTankById;
using AquaControl.Application.Common.Models;
using AquaControl.Application.Features.Tanks.DTOs;
using AquaControl.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AquaControl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TanksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TanksController> _logger;

    public TanksController(IMediator mediator, ILogger<TanksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all tanks with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<TankDto>>> GetTanks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] TankType? tankType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting tanks with page={Page}, pageSize={PageSize}", page, pageSize);

        var query = new GetTanksQuery(page, pageSize, searchTerm, tankType?.ToString(), isActive, sortBy, sortDescending);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get a specific tank by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<ActionResult<TankDto>> GetTankById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting tank with ID: {TankId}", id);

        var query = new GetTankByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new tank
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<ActionResult<TankDto>> CreateTank(
        [FromBody] CreateTankRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new tank: {TankName}", request.Name);

        var command = new CreateTankCommand(
            request.Name,
            request.Capacity,
            request.CapacityUnit,
            request.Building,
            request.Room,
            request.Zone,
            request.Latitude,
            request.Longitude,
            request.TankType);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        var tankDto = await GetTankById(result.Value, cancellationToken);
        return CreatedAtAction(nameof(GetTankById), new { id = result.Value }, tankDto.Value);
    }

    /// <summary>
    /// Update an existing tank
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<ActionResult<TankDto>> UpdateTank(
        Guid id,
        [FromBody] UpdateTankRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating tank with ID: {TankId}", id);

        var command = new UpdateTankCommand(
            id,
            request.Name,
            request.Capacity,
            request.CapacityUnit,
            request.Building,
            request.Room,
            request.Zone,
            request.Latitude,
            request.Longitude,
            request.TankType);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Validation => BadRequest(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        var tankDto = await GetTankById(id, cancellationToken);
        return Ok(tankDto.Value);
    }

    /// <summary>
    /// Delete a tank
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<ActionResult> DeleteTank(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting tank with ID: {TankId}", id);

        var command = new DeleteTankCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return NoContent();
    }

    /// <summary>
    /// Activate a tank
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<ActionResult> ActivateTank(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating tank with ID: {TankId}", id);

        var command = new ActivateTankCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Conflict => Conflict(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok();
    }

    /// <summary>
    /// Deactivate a tank
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<ActionResult> DeactivateTank(
        Guid id,
        [FromBody] DeactivateTankRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating tank with ID: {TankId}, Reason: {Reason}", id, request.Reason);

        var command = new DeactivateTankCommand(id, request.Reason);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok();
    }

    /// <summary>
    /// Schedule maintenance for a tank
    /// </summary>
    [HttpPost("{id:guid}/schedule-maintenance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<ActionResult> ScheduleMaintenance(
        Guid id,
        [FromBody] ScheduleMaintenanceRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Scheduling maintenance for tank with ID: {TankId}, Date: {MaintenanceDate}", id, request.MaintenanceDate);

        var command = new ScheduleMaintenanceCommand(id, request.MaintenanceDate, request.Notes);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Validation => BadRequest(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok();
    }
}

// Request/Response DTOs
public record CreateTankRequest(
    string Name,
    decimal Capacity,
    string CapacityUnit,
    string Building,
    string Room,
    string? Zone,
    decimal? Latitude,
    decimal? Longitude,
    TankType TankType);

public record UpdateTankRequest(
    string Name,
    decimal Capacity,
    string CapacityUnit,
    string Building,
    string Room,
    string? Zone,
    decimal? Latitude,
    decimal? Longitude,
    TankType TankType);

public record DeactivateTankRequest(string Reason);

public record ScheduleMaintenanceRequest(DateTime MaintenanceDate, string? Notes);

