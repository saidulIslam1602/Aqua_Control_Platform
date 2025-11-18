using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using AquaControl.Application.Common.Interfaces;
using AquaControl.Infrastructure.EventStore;
using AquaControl.Infrastructure.ReadModels;
using AquaControl.Infrastructure.Persistence;
using AquaControl.Domain.Common;
using MediatR;
using System.Data.Common;

namespace AquaControl.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _applicationContext;
    private readonly EventStoreDbContext _eventStoreContext;
    private readonly ReadModelDbContext _readModelContext;
    private readonly IMediator _mediator;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

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
        _logger.LogDebug("Starting unit of work save operation");

        // Collect domain events before saving
        var domainEvents = await CollectDomainEventsAsync();
        
        // Use the execution strategy to handle retries with transactions
        var strategy = _applicationContext.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            // Use a simple transaction for application context
            // EventStore and ReadModel will be saved separately if they have changes
            using var transaction = await _applicationContext.Database.BeginTransactionAsync(cancellationToken);
            _transaction = transaction;

            try
            {
                // Save changes to application context
                var applicationResult = await _applicationContext.SaveChangesAsync(cancellationToken);
                
                // Commit application context transaction
                await transaction.CommitAsync(cancellationToken);
                _transaction = null;

                // Save EventStore and ReadModel separately (they have their own transactions)
                var eventStoreResult = 0;
                var readModelResult = 0;
                
                try
                {
                    if (_eventStoreContext.ChangeTracker.HasChanges())
                    {
                        eventStoreResult = await _eventStoreContext.SaveChangesAsync(cancellationToken);
                    }
                    
                    if (_readModelContext.ChangeTracker.HasChanges())
                    {
                        readModelResult = await _readModelContext.SaveChangesAsync(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to save EventStore or ReadModel changes, but application changes were committed");
                }

                // Publish domain events after successful persistence
                await PublishDomainEventsAsync(domainEvents, cancellationToken);

                // Clear domain events from entities after successful publishing
                ClearDomainEvents();

                var totalChanges = applicationResult + eventStoreResult + readModelResult;
                
                _logger.LogInformation("Successfully saved {TotalChanges} changes across all contexts " +
                                     "({ApplicationChanges} application, {EventStoreChanges} event store, {ReadModelChanges} read model)",
                    totalChanges, applicationResult, eventStoreResult, readModelResult);

                return totalChanges;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save changes, rolling back transaction: {Message}", ex.Message);
                
                try
                {
                    if (_transaction != null)
                    {
                        await _transaction.RollbackAsync(cancellationToken);
                        _transaction = null;
                    }
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Failed to rollback transaction: {Message}", rollbackEx.Message);
                }

                throw;
            }
        });
    }

    private Task<List<IDomainEvent>> CollectDomainEventsAsync()
    {
        var domainEvents = new List<IDomainEvent>();

        // Collect domain events from tracked entities in application context
        var applicationEntities = _applicationContext.ChangeTracker.Entries<Entity<Guid>>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        foreach (var entity in applicationEntities)
        {
            domainEvents.AddRange(entity.DomainEvents);
        }

        _logger.LogDebug("Collected {EventCount} domain events from {EntityCount} entities", 
            domainEvents.Count, applicationEntities.Count);

        return Task.FromResult(domainEvents);
    }

    private async Task PublishDomainEventsAsync(List<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        if (!domainEvents.Any())
        {
            _logger.LogDebug("No domain events to publish");
            return;
        }

        _logger.LogDebug("Publishing {EventCount} domain events", domainEvents.Count);

        foreach (var domainEvent in domainEvents)
        {
            try
            {
                await _mediator.Publish(domainEvent, cancellationToken);
                _logger.LogDebug("Published domain event: {EventType} with ID: {EventId}", 
                    domainEvent.EventType, domainEvent.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish domain event {EventType} with ID {EventId}: {Message}",
                    domainEvent.EventType, domainEvent.EventId, ex.Message);
                // Continue publishing other events even if one fails
            }
        }

        _logger.LogInformation("Completed publishing {EventCount} domain events", domainEvents.Count);
    }

    private void ClearDomainEvents()
    {
        var applicationEntities = _applicationContext.ChangeTracker.Entries<Entity<Guid>>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        foreach (var entity in applicationEntities)
        {
            entity.ClearDomainEvents();
        }

        _logger.LogDebug("Cleared domain events from {EntityCount} entities", applicationEntities.Count);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                _transaction?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing transaction: {Message}", ex.Message);
            }
            finally
            {
                _transaction = null;
                _disposed = true;
            }
        }
    }
}

