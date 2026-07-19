# Luoli.Persistence

.NET 10 持久化基础类库，基于 Entity Framework Core 提供统一的数据库操作能力。

## 特性

- ✅ 统一实体基类（审计字段 + 软删除标记）
- ✅ EF Core 审计拦截器，自动填充时间戳与操作者
- ✅ 泛型仓储模式（CRUD + 分页查询）
- ✅ 软删除（所有删除操作自动转为标记删除）
- ✅ 多数据库支持（PostgreSQL，可扩展 SQL Server / MySQL）
- ✅ snake_case 列名自动转换
- ✅ 查询默认排除已删除记录

## 快速开始

### 1. 引用项目

```xml
<ProjectReference Include="..\Luoli.Persistence\Luoli.Persistence.csproj" />
```

### 2. 定义实体

```csharp
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

### 3. 创建 DbContext

```csharp
public class AppDbContext : BaseDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options, AuditInterceptor interceptor)
        : base(options, interceptor) { }

    public DbSet<Product> Products => Set<Product>();
}
```

### 4. 注册服务

```csharp
builder.Services.AddLuoliPersistence<AppDbContext>(options =>
{
    options.DatabaseProvider = "PostgreSQL";
    options.ConnectionString = builder.Configuration.GetConnectionString("Default")!;
});
```

### 5. 使用仓储

```csharp
public class ProductEndpoint : Endpoint<CreateProductRequest>
{
    private readonly IRepository<Product> _repo;

    public ProductEndpoint(IRepository<Product> repo) => _repo = repo;

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var product = new Product { Name = req.Name, Price = req.Price };
        await _repo.AddAsync(product, ct);
        await _repo.SaveChangesAsync(ct);
        await SendAsync(ApiResponse<Guid>.Success(product.Id), cancellation: ct);
    }
}
```

## 软删除

- `IRepository<T>.SoftDeleteAsync(id)` 或直接 `context.Remove(entity)` 均转为软删除
- 全局查询过滤器自动排除 `IsDeleted = true` 的记录
- 需要查询已删除记录时使用 `context.AllWithDeleted<T>()`

## 查询默认过滤

所有查询默认追加 `IsDeleted == false` 条件。如果调用方在表达式中已显式指定 `IsDeleted` 条件，则以调用方输入的条件为准。

## Demo

运行 `Luoli.Persistence.Demo.Host` 体验完整 CRUD + 分页查询功能。
