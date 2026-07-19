using Luoli.Persistence.Demo.Host.Entities;

namespace Luoli.Persistence.Demo.Host.Endpoints.Products;

/// <summary>
/// 更新产品请求。
/// </summary>
public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

/// <summary>
/// PUT /api/products/{id} — 更新产品。
/// </summary>
public class UpdateProductEndpoint : Endpoint<UpdateProductRequest>
{
    private readonly IRepository<Product> _repository;

    public UpdateProductEndpoint(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Put("/api/products/{id}");
        AllowAnonymous();
        Description(d => d.WithTags("Products"));
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var product = await _repository.GetByIdAsync(id, ct);

        if (product is null)
            throw new BusinessException(
                PersistenceErrorCodes.EntityNotFound,
                PersistenceErrorCodes.GetMessage(PersistenceErrorCodes.EntityNotFound));

        product.Name = req.Name;
        product.Description = req.Description;
        product.Price = req.Price;
        product.Stock = req.Stock;

        _repository.Update(product);
        await _repository.SaveChangesAsync(ct);

        await SendAsync(ApiResponse<GetProductResponse>.Success(new GetProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        }, "Updated"), cancellation: ct);
    }
}
