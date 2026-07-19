# Luoli.Persistence

A .NET 10 persistence foundation library providing unified database operations powered by Entity Framework Core.

## Features

- ✅ Unified entity base class (audit fields + soft delete flag)
- ✅ EF Core audit interceptor for automatic timestamp & user tracking
- ✅ Generic repository pattern (CRUD + paginated queries)
- ✅ Soft delete (all deletes automatically become flag-based)
- ✅ Multi-database support (PostgreSQL, extensible to SQL Server / MySQL)
- ✅ Automatic snake_case column naming
- ✅ Queries exclude soft-deleted records by default

## Quick Start

### 1. Add Reference

```xml
<ProjectReference Include="..\Luoli.Persistence\Luoli.Persistence.csproj" />
```

### 2. Define Entity

```csharp
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

### 3. Create DbContext

```csharp
public class AppDbContext : BaseDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options, AuditInterceptor interceptor)
        : base(options, interceptor) { }

    public DbSet<Product> Products => Set<Product>();
}
```

### 4. Register Services

```csharp
builder.Services.AddLuoliPersistence<AppDbContext>(options =>
{
    options.DatabaseProvider = "PostgreSQL";
    options.ConnectionString = builder.Configuration.GetConnectionString("Default")!;
});
```

### 5. Use Repository

```csharp
var product = new Product { Name = req.Name, Price = req.Price };
await _repo.AddAsync(product, ct);
await _repo.SaveChangesAsync(ct);
```

## Soft Delete

- `IRepository<T>.SoftDeleteAsync(id)` or `context.Remove(entity)` are both converted to soft delete
- Global query filter automatically excludes `IsDeleted = true` records
- Use `context.AllWithDeleted<T>()` to query including deleted records

## Query Default Filter

All queries automatically append `IsDeleted == false`. If the caller explicitly includes an `IsDeleted` condition in the expression, the caller's condition takes precedence.

## Demo

Run `Luoli.Persistence.Demo.Host` to experience complete CRUD + pagination.
