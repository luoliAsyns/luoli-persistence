using System.Text;

namespace Luoli.Persistence.EntityFramework;

/// <summary>
/// PostgreSQL 表分区辅助方法。生成按 created_at 的 RANGE 分区 DDL。
/// </summary>
public static class PartitionHelper
{
    /// <summary>
    /// 生成创建按月分区表的 SQL（DDL）。
    /// 需在 Migration 的 Up 方法中手动执行。
    /// </summary>
    /// <param name="tableName">表名（snake_case）</param>
    /// <param name="startDate">分区起始日期</param>
    /// <param name="months">创建多少个月的分区</param>
    /// <returns>完整 DDL SQL</returns>
    public static string GenerateMonthlyPartitionDdl(
        string tableName,
        DateTime startDate,
        int months)
    {
        var sb = new StringBuilder();

        // 主表 DDL — 按 created_at RANGE 分区
        sb.AppendLine($"""
            CREATE TABLE IF NOT EXISTS {tableName}_partitioned (
                LIKE {tableName} INCLUDING ALL
            ) PARTITION BY RANGE (created_at);

            """);

        // 逐月创建分区
        for (var i = 0; i < months; i++)
        {
            var from = startDate.AddMonths(i);
            var to = from.AddMonths(1);
            var partitionName = $"{tableName}_{from:yyyyMM}";

            sb.AppendLine($"""
                CREATE TABLE IF NOT EXISTS {partitionName}
                    PARTITION OF {tableName}_partitioned
                    FOR VALUES FROM ('{from:yyyy-MM-dd}') TO ('{to:yyyy-MM-dd}');

                """);
        }

        sb.AppendLine("-- 请手动创建 future 分区或配置 pg_partman 自动管理分区。");
        sb.AppendLine($"-- 示例：CREATE TABLE {tableName}_default PARTITION OF {tableName}_partitioned DEFAULT;");

        return sb.ToString();
    }

    /// <summary>
    /// 生成单个分区的创建 SQL。
    /// </summary>
    /// <param name="tableName">主表名（snake_case）</param>
    /// <param name="from">分区起始</param>
    /// <param name="to">分区结束</param>
    /// <returns>DDL SQL</returns>
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
