using Luoli.Persistence.Demo.Host.Entities;

namespace Luoli.Persistence.Demo.Host.Endpoints.Products;

/// <summary>
/// 产品列表项响应。
/// </summary>
public class ProductListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 产品分页响应。
/// </summary>
public class ListProductsResponse
{
    public List<ProductListItem> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}

/// <summary>
/// GET /api/products — 分页查询产品列表。
/// </summary>
public class ListProductsEndpoint : EndpointWithoutRequest
{
    private readonly IRepository<Product> _repository;

    public ListProductsEndpoint(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Get("/api/products");
        AllowAnonymous();
        Description(d => d.WithTags("Products"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var page = Query<int>("page", isRequired: false) is > 0 and var p ? p : 1;
        var pageSize = Query<int>("pageSize", isRequired: false) is > 0 and <= 100 and var ps ? ps : 10;
        var nameFilter = Query<string?>("name", isRequired: false);

        Expression<Func<Product, bool>>? predicate = null;
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            predicate = p => p.Name.Contains(nameFilter);
        }

        var paged = await _repository.GetPagedAsync(page, pageSize, predicate, ct);

        var response = new ListProductsResponse
        {
            Items = paged.Items.Select(p => new ProductListItem
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                CreatedAt = p.CreatedAt
            }).ToList(),
            Total = paged.Total,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalPages = paged.TotalPages,
            HasNext = paged.HasNext,
            HasPrevious = paged.HasPrevious
        };

        await SendAsync(ApiResponse<ListProductsResponse>.Success(response), cancellation: ct);
    }
}
