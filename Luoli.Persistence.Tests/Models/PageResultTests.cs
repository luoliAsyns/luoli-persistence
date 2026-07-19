namespace Luoli.Persistence.Tests.Models;

public class PageResultTests
{
    [Fact]
    public void Create_ShouldCalculateTotalPagesCorrectly()
    {
        var result = PageResult<string>.Create(["a", "b", "c"], 25, 1, 10);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(25, result.Total);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(3, result.TotalPages);
        Assert.False(result.HasPrevious);
        Assert.True(result.HasNext);
    }

    [Fact]
    public void Create_LastPage_ShouldHaveNoNext()
    {
        var result = PageResult<string>.Create(["a", "b"], 12, 2, 10);

        Assert.Equal(2, result.TotalPages);
        Assert.True(result.HasPrevious);
        Assert.False(result.HasNext);
    }

    [Fact]
    public void Create_SinglePage_ShouldHaveNoPreviousOrNext()
    {
        var result = PageResult<string>.Create(["a"], 1, 1, 10);

        Assert.Equal(1, result.TotalPages);
        Assert.False(result.HasPrevious);
        Assert.False(result.HasNext);
    }

    [Fact]
    public void Create_EmptyResult_ShouldHaveZeroPages()
    {
        var result = PageResult<string>.Create([], 0, 1, 10);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void Create_WithZeroPageSize_ShouldHandleGracefully()
    {
        var result = PageResult<string>.Create([], 0, 1, 0);

        Assert.Equal(0, result.TotalPages);
    }
}
