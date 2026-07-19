using Luoli.Persistence.Demo.Host.Entities;
using Luoli.Persistence.EntityFramework;

namespace Luoli.Persistence.Demo.Host.Data;

/// <summary>
/// Demo 数据库上下文。Code First 方式定义实体映射。
/// </summary>
public class DemoDbContext : BaseDbContext
{
    /// <summary>
    /// 初始化 DemoDbContext。
    /// </summary>
    public DemoDbContext(DbContextOptions<DemoDbContext> options, AuditInterceptor auditInterceptor)
        : base(options, auditInterceptor)
    {
    }

    /// <summary>
    /// 产品表。
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });
    }
}
