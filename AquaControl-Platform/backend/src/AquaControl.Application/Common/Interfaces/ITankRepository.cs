using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.Common;

namespace AquaControl.Application.Common.Interfaces;

public interface ITankRepository
{
    Task<Tank?> GetByIdAsync(TankId id, CancellationToken cancellationToken = default);
    Task<Tank?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Tank> Tanks, int TotalCount)> GetPagedAsync(
        ISpecification<Tank> specification,
        int page,
        int pageSize,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default);
    Task AddAsync(Tank tank, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tank tank, CancellationToken cancellationToken = default);
    Task DeleteAsync(Tank tank, CancellationToken cancellationToken = default);
}

