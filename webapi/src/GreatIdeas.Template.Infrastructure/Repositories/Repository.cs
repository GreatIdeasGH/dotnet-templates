using System.Linq.Expressions;

namespace GreatIdeas.Template.Infrastructure.Repositories;

public class Repository<TDbContext, TEntity> : IRepositoryFactory<TEntity>
    where TDbContext : DbContext
    where TEntity : class
{
    public TDbContext DbContext { get; set; }
    protected DbSet<TEntity> DbSet { get; set; }
    private static readonly ActivitySource ActivitySource =
        new(nameof(Repository<TDbContext, TEntity>));

    public Repository(TDbContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DbSet = dbContext.Set<TEntity>();
    }

    public virtual async ValueTask<TEntity?> FindByIdAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(FindByIdAsync),
            ActivityKind.Server
        );
        activity?.Start();

        return await DbSet.FindAsync(id);
    }

    public virtual async ValueTask<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.CreateActivity(nameof(AddAsync), ActivityKind.Server);
        activity?.Start();

        ArgumentNullException.ThrowIfNull(entity);
        var result = await DbSet.AddAsync(entity, cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return result.Entity;
    }

    public virtual async ValueTask AddRangeAsync(
        List<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(AddRangeAsync),
            ActivityKind.Server
        );
        activity?.Start();

        ArgumentNullException.ThrowIfNull(entities);
        await DbSet.AddRangeAsync(entities, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async ValueTask UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(UpdateAsync),
            ActivityKind.Server
        );
        activity?.Start();

        ArgumentNullException.ThrowIfNull(entity);
        // Modify the state of the entity to modified
        DbContext.Entry(entity).State = EntityState.Modified;
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async ValueTask UpdateRangeAsync(
        List<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(UpdateRangeAsync),
            ActivityKind.Server
        );
        activity?.Start();

        ArgumentNullException.ThrowIfNull(entities);
        DbSet.UpdateRange(entities);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    //obsolete
    public virtual void Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Remove(entity);
    }

    public async ValueTask<int> DeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(DeleteRangeAsync),
            ActivityKind.Server
        );
        activity?.Start();

        var entity = await DbSet.Where(predicate).FirstOrDefaultAsync(cancellationToken);
        if (entity == null)
            return 0;
        DbSet.Remove(entity);
        return await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask<int> DeleteByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(DeleteRangeAsync),
            ActivityKind.Server
        );
        activity?.Start();

        var entity = await DbSet.FindAsync([id], cancellationToken: cancellationToken);
        if (entity == null)
            return 0;
        DbSet.Remove(entity);
        return await DbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async ValueTask<int> DeleteRangeAsync(
        List<TEntity> entities,
        CancellationToken cancellationToken
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(DeleteRangeAsync),
            ActivityKind.Server
        );
        activity?.Start();

        ArgumentNullException.ThrowIfNull(entities);
        DbContext.Entry(entities).State = EntityState.Deleted;
        DbSet.RemoveRange(entities);
        return await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Replace this with DeleteAsync or DeleteByIdAsync until deletion can be tracked for auditing
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async ValueTask<int> ExecuteDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(ExecuteDeleteAsync),
            ActivityKind.Server
        );
        activity?.Start();

        ArgumentNullException.ThrowIfNull(predicate);

        IQueryable<TEntity> source = DbSet;
        var entity = source.Where(predicate);
        return await entity.ExecuteDeleteAsync(cancellationToken);
    }

    public virtual async ValueTask<int> ExecuteDeleteRangeAsync(
        List<TEntity> entities,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(ExecuteDeleteRangeAsync),
            ActivityKind.Server
        );
        activity?.Start();

        ArgumentNullException.ThrowIfNull(entities);
        DbContext.Entry(entities).State = EntityState.Deleted;
        IQueryable<TEntity> source = DbSet;
        return await source.Where(predicate).ExecuteDeleteAsync(cancellationToken);
    }

    public virtual async ValueTask<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> selector,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(ExistsAsync),
            ActivityKind.Server
        );
        activity?.Start();

        return await DbSet.AsNoTracking().AnyAsync(selector, cancellationToken);
    }

    public virtual async ValueTask<int> CountAsync(
        Expression<Func<TEntity, bool>> selector,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = ActivitySource.CreateActivity(nameof(CountAsync), ActivityKind.Server);
        activity?.Start();

        return await DbSet.AsNoTracking().CountAsync(selector, cancellationToken);
    }

    public async ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await DbContext.SaveChangesAsync(cancellationToken);
    }
}
