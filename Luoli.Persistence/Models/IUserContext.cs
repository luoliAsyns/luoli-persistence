namespace Luoli.Persistence.Models;

/// <summary>
/// 当前用户上下文抽象，用于审计字段自动填充。
/// 业务项目需实现此接口以提供真实的用户标识。
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// 当前用户标识。未登录或系统操作时返回空字符串。
    /// </summary>
    string UserId { get; }
}
