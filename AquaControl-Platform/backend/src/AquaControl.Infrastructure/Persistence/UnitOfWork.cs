using Microsoft.Extensions.Logging;
using AquaControl.Application.Common.Interfaces;
using AquaControl.Infrastructure.EventStore;
using AquaControl.Infrastructure.ReadModels;
using AquaControl.Infrastructure.Persistence;
using MediatR;

namespace AquaControl.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _applicationContext;
    private readonly EventStoreDbContext _eventStoreContext;
    private readonly ReadModelDbContext _readModelContext;
    private readonly IMediator _mediator;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(
        ApplicationDbContext applicationContext,
        EventStoreDbContext eventStoreContext,
        ReadModelDbContext readModelContext,
        IMediator mediator,
        ILogger<UnitOfWork> logger)
    {
        _applicationContext = applicationContext;
        _eventStoreContext = eventStoreContext;
        _readModelContext = readModelContext;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Saving changes to database");

        using var transaction = await _applicationContext.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Save application context changes (Users, RefreshTokens, etc.)
            var applicationResult = await _applicationContext.SaveChangesAsync(cancellationToken);
            
            // Save event store changes
            var eventStoreResult = await _eventStoreContext.SaveChangesAsync(cancellationToken);
            
            // Save read model changes
            var readModelResult = await _readModelContext.SaveChangesAsync(cancellationToken);
            
            // Publish domain events
            await PublishDomainEventsAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Successfully saved {ApplicationChanges} application, {EventStoreChanges} event store, and {ReadModelChanges} read model changes",
                applicationResult, eventStoreResult, readModelResult);
            
            return applicationResult + eventStoreResult + readModelResult;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to save changes, transaction rolled back");
            throw;
        }
    }

    private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
    {
        // This is a simplified implementation
        // In practice, you'd collect domain events from aggregates and publish them
        // For now, events are published by the application layer after saving
        await Task.CompletedTask;
    }
}

