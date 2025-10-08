using System.Linq.Expressions;

namespace Company.Project.Application.Abstractions.Services;

public interface IRepositoryFactory<TEntity>
    where TEntity : class
{
    ValueTask<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    ValueTask<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    ValueTask AddRangeAsync(List<TEntity> entities, CancellationToken cancellationToken = default);

    ValueTask UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    ValueTask UpdateRangeAsync(
        List<TEntity> entities,
        CancellationToken cancellationToken = default
    );

    ValueTask<int> DeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );
    ValueTask<int> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<int> DeleteRangeAsync(List<TEntity> entities, CancellationToken cancellationToken);

    ValueTask<int> ExecuteDeleteRangeAsync(
        List<TEntity> entities,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    ValueTask<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> selector,
        CancellationToken cancellationToken = default
    );

    ValueTask<int> CountAsync(
        Expression<Func<TEntity, bool>> selector,
        CancellationToken cancellationToken = default
    );

    ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken);
}
