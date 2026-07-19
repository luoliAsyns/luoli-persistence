using Luoli.Persistence.Demo.Host.Entities;

namespace Luoli.Persistence.Demo.Host.Endpoints.Products;

/// <summary>
/// 新增产品请求。
/// </summary>
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

/// <summary>
/// 新增产品响应。
/// </summary>
public class CreateProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// POST /api/products — 新增产品。
/// </summary>
public class CreateProductEndpoint : Endpoint<CreateProductRequest>
{
    private readonly IRepository<Product> _repository;

    public CreateProductEndpoint(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Post("/api/products");
        AllowAnonymous();
        Description(d => d.WithTags("Products"));
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var product = new Product
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            Stock = req.Stock
        };

        await _repository.AddAsync(product, ct);
        await _repository.SaveChangesAsync(ct);

        var response = new CreateProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt
        };

        await SendAsync(ApiResponse<CreateProductResponse>.Success(response, "Created"), 201, ct);
    }
}
