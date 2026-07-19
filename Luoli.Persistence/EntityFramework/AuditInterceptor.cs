using Luoli.Persistence.Models;

namespace Luoli.Persistence.EntityFramework;

/// <summary>
/// EF Core 审计拦截器：自动填充审计字段，并将硬删除转换为软删除。
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IUserContext _userContext;

    /// <summary>
    /// 初始化审计拦截器。
    /// </summary>
    public AuditInterceptor(IUserContext userContext)
    {
        _userContext = userContext;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ProcessAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ProcessAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// 处理审计字段和软删除转换。
    /// </summary>
    private void ProcessAuditFields(DbContext? context)
    {
        if (context is null) return;

        var utcNow = DateTime.UtcNow;
        var userId = _userContext.UserId;

        var entries = context.ChangeTracker.Entries<BaseEntity>().ToList();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    HandleAdded(entry, utcNow, userId);
                    break;

                case EntityState.Modified:
                    HandleModified(entry, utcNow, userId);
                    break;

                case EntityState.Deleted:
                    HandleDeleted(entry, utcNow, userId);
                    break;
            }
        }
    }

    /// <summary>
    /// 处理新增实体：自动填充 Id、审计字段。
    /// </summary>
    private static void HandleAdded(EntityEntry<BaseEntity> entry, DateTime utcNow, string userId)
    {
        if (entry.Entity.Id == Guid.Empty)
        {
            entry.Entity.Id = Guid.NewGuid();
        }

        entry.Entity.CreatedAt = utcNow;
        entry.Entity.CreatedBy = userId;
        entry.Entity.UpdatedAt = utcNow;
        entry.Entity.UpdatedBy = userId;
    }

    /// <summary>
    /// 处理修改实体：更新 UpdatedAt/UpdatedBy，保护创建字段不被覆盖。
    /// </summary>
    private static void HandleModified(EntityEntry<BaseEntity> entry, DateTime utcNow, string userId)
    {
        // 防止创建字段被意外覆盖
        entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
        entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;

        entry.Entity.UpdatedAt = utcNow;
        entry.Entity.UpdatedBy = userId;
    }

    /// <summary>
    /// 将硬删除转换为软删除：标记 IsDeleted = true，更新审计字段。
    /// </summary>
    private static void HandleDeleted(EntityEntry<BaseEntity> entry, DateTime utcNow, string userId)
    {
        // 将删除操作转换为更新操作
        entry.State = EntityState.Modified;

        entry.Entity.IsDeleted = true;
        entry.Entity.UpdatedAt = utcNow;
        entry.Entity.UpdatedBy = userId;

        // 只标记软删除相关字段为已修改，避免不必要的 UPDATE
        entry.Property(nameof(BaseEntity.IsDeleted)).IsModified = true;
        entry.Property(nameof(BaseEntity.UpdatedAt)).IsModified = true;
        entry.Property(nameof(BaseEntity.UpdatedBy)).IsModified = true;
    }
}
