namespace Luoli.Persistence.Models;

/// <summary>
/// created_at 分区粒度。
/// </summary>
public enum PartitionGranularity
{
    /// <summary>不分区。</summary>
    None = 0,

    /// <summary>按天分区。</summary>
    Day = 1,

    /// <summary>按周分区。</summary>
    Week = 2,

    /// <summary>按月分区。</summary>
    Month = 3,
}
