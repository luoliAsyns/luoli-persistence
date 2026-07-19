namespace Luoli.Persistence.Models;

/// <summary>
/// 分页查询结果。
/// </summary>
/// <typeparam name="T">数据项类型</typeparam>
public class PageResult<T>
{
    /// <summary>
    /// 当前页数据。
    /// </summary>
    public List<T> Items { get; set; } = [];

    /// <summary>
    /// 总记录数。
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页码（从 1 开始）。
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页大小。
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数。
    /// </summary>
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling((double)Total / PageSize)
        : 0;

    /// <summary>
    /// 是否有上一页。
    /// </summary>
    public bool HasPrevious => Page > 1;

    /// <summary>
    /// 是否有下一页。
    /// </summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>
    /// 创建分页结果。
    /// </summary>
    public static PageResult<T> Create(List<T> items, int total, int page, int pageSize)
        => new()
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
}
