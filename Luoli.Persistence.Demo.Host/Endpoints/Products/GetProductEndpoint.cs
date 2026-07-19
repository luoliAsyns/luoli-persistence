using Luoli.Persistence.Demo.Host.Entities;

namespace Luoli.Persistence.Demo.Host.Endpoints.Products;

/// <summary>
/// 产品详情响应。
/// </summary>
public class GetProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// GET /api/products/{id} — 获取产品详情。
/// </summary>
public class GetProductEndpoint : EndpointWithoutRequest
{
    private readonly IRepository<Product> _repository;

    public GetProductEndpoint(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Get("/api/products/{id}");
        AllowAnonymous();
        Description(d => d.WithTags("Products"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var product = await _repository.GetByIdAsync(id, ct);

        if (product is null)
            throw new BusinessException(
                PersistenceErrorCodes.EntityNotFound,
                PersistenceErrorCodes.GetMessage(PersistenceErrorCodes.EntityNotFound));

        var response = new GetProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };

        await SendAsync(ApiResponse<GetProductResponse>.Success(response), cancellation: ct);
    }
}
