using Luoli.Persistence.Models;

namespace Luoli.Persistence.EntityFramework;

/// <summary>
/// 泛型仓储接口，提供基础 CRUD 和分页查询能力。
/// </summary>
/// <typeparam name="T">实体类型，必须继承 BaseEntity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// 根据主键获取实体。
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有未删除的实体。
    /// </summary>
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询。
    /// </summary>
    /// <param name="page">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="predicate">可选的过滤条件。如包含 IsDeleted 条件则以调用方为准</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<PageResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增实体。
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新实体（标记为 Modified）。
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// 软删除实体。
    /// </summary>
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存所有变更。
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
