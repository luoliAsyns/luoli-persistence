namespace Luoli.Persistence.Models;

/// <summary>
/// 持久化配置选项。
/// </summary>
public class PersistenceOptions
{
    /// <summary>
    /// 数据库提供程序标识，如 "PostgreSQL"、"SqlServer"、"MySQL"。
    /// </summary>
    public string DatabaseProvider { get; set; } = "PostgreSQL";

    /// <summary>
    /// 数据库连接字符串。
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Migration 程序集名称（可选）。
    /// </summary>
    public string? MigrationsAssembly { get; set; }

    /// <summary>
    /// 是否按 created_at 创建表分区（PostgreSQL 原生分区）。
    /// 启用后需通过 Migration 手动创建分区。
    /// </summary>
    public bool PartitionByCreatedAt { get; set; }
}
