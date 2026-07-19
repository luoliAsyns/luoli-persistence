using Luoli.Persistence.Models;

namespace Luoli.Persistence.EntityFramework;

/// <summary>
/// 泛型仓储实现，基于 EF Core DbContext 提供基础 CRUD 操作。
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// 关联的 DbContext。
    /// </summary>
    protected readonly BaseDbContext Context;

    /// <summary>
    /// 实体对应的 DbSet。
    /// </summary>
    protected readonly DbSet<T> Set;

    /// <summary>
    /// 初始化仓储。
    /// </summary>
    public Repository(DbContext context)
    {
        Context = (BaseDbContext)context;
        Set = context.Set<T>();
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = Context.ApplyDefaultFilter(Set.AsNoTracking());
        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<PageResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.ApplyDefaultFilter(Set.AsNoTracking(), predicate);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PageResult<T>.Create(items, total, page, pageSize);
    }

    /// <inheritdoc />
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entry = await Set.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    /// <inheritdoc />
    public virtual void Update(T entity)
    {
        Set.Update(entity);
    }

    /// <inheritdoc />
    public virtual async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Set.IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity is null)
            throw new BusinessException(
                PersistenceErrorCodes.EntityNotFound,
                PersistenceErrorCodes.GetMessage(PersistenceErrorCodes.EntityNotFound));

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    /// <inheritdoc />
    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}
