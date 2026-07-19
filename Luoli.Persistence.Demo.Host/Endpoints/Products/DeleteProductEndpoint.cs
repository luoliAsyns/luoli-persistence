using Luoli.Persistence.Demo.Host.Entities;

namespace Luoli.Persistence.Demo.Host.Endpoints.Products;

/// <summary>
/// DELETE /api/products/{id} — 软删除产品。
/// </summary>
public class DeleteProductEndpoint : EndpointWithoutRequest
{
    private readonly IRepository<Product> _repository;

    public DeleteProductEndpoint(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Delete("/api/products/{id}");
        AllowAnonymous();
        Description(d => d.WithTags("Products"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        await _repository.SoftDeleteAsync(id, ct);
        await _repository.SaveChangesAsync(ct);

        await SendAsync(ApiResponse.Success("Deleted"), cancellation: ct);
    }
}
