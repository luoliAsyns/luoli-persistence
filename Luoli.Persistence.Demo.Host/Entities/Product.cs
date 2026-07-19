namespace Luoli.Persistence.Demo.Host.Entities;

/// <summary>
/// 示例产品实体。
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// 产品名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 产品描述。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 价格。
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 库存数量。
    /// </summary>
    public int Stock { get; set; }
}
