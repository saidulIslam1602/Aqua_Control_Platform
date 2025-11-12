using System.Linq.Expressions;

namespace AquaControl.Domain.Common;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
}

