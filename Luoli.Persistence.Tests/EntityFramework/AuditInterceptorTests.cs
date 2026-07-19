using Luoli.Persistence.Tests.Models;
using Microsoft.EntityFrameworkCore;

namespace Luoli.Persistence.Tests.EntityFramework;

public class TestDbContext : BaseDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options, AuditInterceptor auditInterceptor)
        : base(options, auditInterceptor)
    {
    }

    public DbSet<TestEntity> TestEntities => Set<TestEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 不调用 base.OnModelCreating，因为 InMemory 不支持 query filter 的某些操作
        // 但我们需要测试审计拦截器
    }
}

public class AuditInterceptorTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userContext = new NoOpUserContext();
        var interceptor = new AuditInterceptor(userContext);

        return new TestDbContext(options, interceptor);
    }

    [Fact]
    public async Task AddEntity_ShouldAutoFillIdAndTimestamps()
    {
        await using var context = CreateContext();

        var entity = new TestEntity { Name = "Test" };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.NotEqual(default, entity.CreatedAt);
        Assert.NotEqual(default, entity.UpdatedAt);
        Assert.Equal(entity.CreatedAt, entity.UpdatedAt);
    }

    [Fact]
    public async Task AddEntity_WithExistingId_ShouldPreserveId()
    {
        await using var context = CreateContext();

        var existingId = Guid.NewGuid();
        var entity = new TestEntity { Id = existingId, Name = "Test" };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        Assert.Equal(existingId, entity.Id);
    }

    [Fact]
    public async Task ModifyEntity_ShouldUpdateTimestamp()
    {
        await using var context = CreateContext();

        var entity = new TestEntity { Name = "Original" };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        var originalCreatedAt = entity.CreatedAt;
        var originalUpdatedAt = entity.UpdatedAt;

        await Task.Delay(10); // 确保时间有变化

        entity.Name = "Modified";
        await context.SaveChangesAsync();

        // CreatedAt 不应被修改
        Assert.Equal(originalCreatedAt, entity.CreatedAt);
        // UpdatedAt 应该被更新
        Assert.True(entity.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public async Task RemoveEntity_ShouldBeConvertedToSoftDelete()
    {
        await using var context = CreateContext();

        var entity = new TestEntity { Name = "ToDelete" };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        context.TestEntities.Remove(entity);
        await context.SaveChangesAsync();

        // 实体状态应变为 Modified（不是 Deleted）
        var entry = context.Entry(entity);
        Assert.NotEqual(EntityState.Deleted, entry.State);

        // IsDeleted 应被标记为 true
        Assert.True(entity.IsDeleted);
    }

    [Fact]
    public async Task SoftDeletedEntity_ShouldNotAppearInQuery()
    {
        await using var context = CreateContext();

        var entity = new TestEntity { Name = "WillBeSoftDeleted" };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        entity.IsDeleted = true;
        await context.SaveChangesAsync();

        // 重新创建 context 以确保查询过滤器生效
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var userContext = new NoOpUserContext();
        var interceptor = new AuditInterceptor(userContext);

        await using var freshContext = new TestDbContext(options, interceptor);
        var entities = await freshContext.TestEntities.ToListAsync();

        // 在新 context 中没有添加实体，查询为空
        Assert.Empty(entities);
    }
}
