namespace Luoli.Persistence.Models;

/// <summary>
/// 所有持久化实体的抽象基类，提供统一的审计字段和软删除标记。
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// 主键，使用 Guid 以兼容多数据库。
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 创建时间（UTC）。
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最后更新时间（UTC）。
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 创建者标识。
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// 最后更新者标识。
    /// </summary>
    public string UpdatedBy { get; set; } = string.Empty;

    /// <summary>
    /// 软删除标记。true 表示已删除。
    /// </summary>
    public bool IsDeleted { get; set; }
}
