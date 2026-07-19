namespace Luoli.Persistence.Tests.Models;

public class TestEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

public class BaseEntityTests
{
    [Fact]
    public void NewEntity_ShouldHaveDefaultValues()
    {
        var entity = new TestEntity();

        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(default, entity.CreatedAt);
        Assert.Equal(default, entity.UpdatedAt);
        Assert.Equal(string.Empty, entity.CreatedBy);
        Assert.Equal(string.Empty, entity.UpdatedBy);
        Assert.False(entity.IsDeleted);
    }

    [Fact]
    public void SetIsDeleted_ShouldMarkEntity()
    {
        var entity = new TestEntity { IsDeleted = true };

        Assert.True(entity.IsDeleted);
    }

    [Fact]
    public void SetAuditFields_ShouldPersistValues()
    {
        var now = DateTime.UtcNow;
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            CreatedAt = now,
            CreatedBy = "user-1",
            UpdatedAt = now,
            UpdatedBy = "user-1"
        };

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("user-1", entity.CreatedBy);
        Assert.Equal("user-1", entity.UpdatedBy);
    }
}
