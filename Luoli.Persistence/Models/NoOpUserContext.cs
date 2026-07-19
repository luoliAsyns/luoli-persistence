namespace Luoli.Persistence.Models;

/// <summary>
/// IUserContext 的默认空实现。未注入真实实现时使用。
/// </summary>
public class NoOpUserContext : IUserContext
{
    /// <inheritdoc />
    public string UserId => string.Empty;
}
