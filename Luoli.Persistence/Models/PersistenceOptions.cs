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
    /// created_at 分区粒度。默认 <see cref="PartitionGranularity.None"/>（不分区）。
    /// 启用后通过 <see cref="EntityFramework.PartitionHelper"/> 生成 DDL，在 Migration 中手动执行。
    /// </summary>
    public PartitionGranularity CreatedAtPartitionGranularity { get; set; } = PartitionGranularity.None;
}
