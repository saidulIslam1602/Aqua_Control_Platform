using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.Enums;
using System.Linq.Expressions;

namespace AquaControl.Application.Common.Specifications;

public sealed class TankFilterSpecification : ISpecification<Tank>
{
    private readonly string? _searchTerm;
    private readonly TankType? _tankType;
    private readonly bool? _isActive;

    public TankFilterSpecification(string? searchTerm, TankType? tankType, bool? isActive)
    {
        _searchTerm = searchTerm;
        _tankType = tankType;
        _isActive = isActive;
    }

    public Expression<Func<Tank, bool>> ToExpression()
    {
        Expression<Func<Tank, bool>> expression = tank => true;

        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            var searchLower = _searchTerm.ToLowerInvariant();
            expression = expression.And(tank =>
                tank.Name.ToLower().Contains(searchLower) ||
                tank.Location.Building.ToLower().Contains(searchLower) ||
                tank.Location.Room.ToLower().Contains(searchLower) ||
                (tank.Location.Zone != null && tank.Location.Zone.ToLower().Contains(searchLower)));
        }

        if (_tankType.HasValue)
        {
            expression = expression.And(tank => tank.TankType == _tankType.Value);
        }

        if (_isActive.HasValue)
        {
            var targetStatus = _isActive.Value ? TankStatus.Active : TankStatus.Inactive;
            expression = expression.And(tank => tank.Status == targetStatus);
        }

        return expression;
    }
}

