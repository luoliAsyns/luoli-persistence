using System.Text;
using Luoli.Persistence.Models;

namespace Luoli.Persistence.EntityFramework;

/// <summary>
/// PostgreSQL 表分区辅助方法。生成按 created_at 的 RANGE 分区 DDL（day / week / month）。
/// </summary>
public static class PartitionHelper
{
    /// <summary>
    /// 根据粒度计算下一个分区边界。
    /// </summary>
    private static DateTime NextBoundary(DateTime from, PartitionGranularity granularity) => granularity switch
    {
        PartitionGranularity.Day => from.AddDays(1),
        PartitionGranularity.Week => from.AddDays(7),
        PartitionGranularity.Month => from.AddMonths(1),
        _ => throw new ArgumentOutOfRangeException(nameof(granularity))
    };

    /// <summary>
    /// 根据粒度生成分区名称后缀。
    /// </summary>
    private static string PartitionSuffix(DateTime from, PartitionGranularity granularity) => granularity switch
    {
        PartitionGranularity.Day => from.ToString("yyyyMMdd"),
        PartitionGranularity.Week => $"{from:yyyyMMdd}",
        PartitionGranularity.Month => from.ToString("yyyyMM"),
        _ => throw new ArgumentOutOfRangeException(nameof(granularity))
    };

    /// <summary>
    /// 生成分区 DDL（主表 + N 个分区）。
    /// </summary>
    /// <param name="tableName">表名（snake_case）</param>
    /// <param name="granularity">分区粒度</param>
    /// <param name="startDate">第一个分区的起始日期</param>
    /// <param name="count">创建多少个分区</param>
    /// <returns>完整 DDL SQL</returns>
    public static string GeneratePartitionDdl(
        string tableName,
        PartitionGranularity granularity,
        DateTime startDate,
        int count)
    {
        if (granularity == PartitionGranularity.None)
            return "-- PartitionGranularity.None: no partition DDL generated.";

        var sb = new StringBuilder();

        sb.AppendLine($"""
            -- ===================================================
            -- {tableName} — RANGE PARTITION BY created_at ({granularity})
            -- 主表
            -- ===================================================
            CREATE TABLE IF NOT EXISTS {tableName}_partitioned (
                LIKE {tableName} INCLUDING ALL
            ) PARTITION BY RANGE (created_at);

            """);

        var from = startDate;
        for (var i = 0; i < count; i++)
        {
            var to = NextBoundary(from, granularity);
            var suffix = PartitionSuffix(from, granularity);
            var partitionName = $"{tableName}_{suffix}";

            sb.AppendLine($"""
                CREATE TABLE IF NOT EXISTS {partitionName}
                    PARTITION OF {tableName}_partitioned
                    FOR VALUES FROM ('{from:yyyy-MM-dd}') TO ('{to:yyyy-MM-dd}');

                """);

            from = to;
        }

        var desc = granularity switch
        {
            PartitionGranularity.Day => "daily",
            PartitionGranularity.Week => "weekly",
            PartitionGranularity.Month => "monthly",
            _ => "periodic"
        };

        sb.AppendLine($"-- 建议配置 pg_partman 自动管理 {desc} 分区。");
        sb.AppendLine($"-- CREATE TABLE {tableName}_default PARTITION OF {tableName}_partitioned DEFAULT;");

        return sb.ToString();
    }

    /// <summary>
    /// 生成单个分区的创建 SQL。
    /// </summary>
    /// <param name="tableName">主表名（snake_case）</param>
    /// <param name="from">分区起始</param>
    /// <param name="to">分区结束</param>
    public static string CreatePartition(string tableName, DateTime from, DateTime to)
    {
        var partitionName = $"{tableName}_{from:yyyyMMdd}_{to:yyyyMMdd}";
        return $"""
            CREATE TABLE IF NOT EXISTS {partitionName}
                PARTITION OF {tableName}
                FOR VALUES FROM ('{from:yyyy-MM-dd}') TO ('{to:yyyy-MM-dd}');

            """;
    }
}
