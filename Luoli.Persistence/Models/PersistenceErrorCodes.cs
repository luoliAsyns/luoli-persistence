namespace Luoli.Persistence.Models;

/// <summary>
/// Luoli.Persistence 业务错误码定义。范围 5000-5099。
/// 遵循 Luoli.Common ErrorCodes 的分段规则：4000-8999 为业务项目自定义范围。
/// </summary>
public static class PersistenceErrorCodes
{
    // -- 实体操作 (5000-5009) --
    /// <summary>实体不存在。</summary>
    public const int EntityNotFound = 5000;

    /// <summary>实体已存在（唯一约束冲突）。</summary>
    public const int EntityAlreadyExists = 5001;

    // -- 数据库操作 (5010-5019) --
    /// <summary>数据库操作失败。</summary>
    public const int DbOperationFailed = 5010;

    /// <summary>并发冲突（乐观锁）。</summary>
    public const int ConcurrencyConflict = 5011;

    // -- 基础设施 (5020-5029) --
    /// <summary>数据库连接失败。</summary>
    public const int ConnectionFailed = 5020;

    /// <summary>数据库迁移失败。</summary>
    public const int MigrationFailed = 5021;

    // -- 查询与校验 (5030-5039) --
    /// <summary>查询参数校验失败。</summary>
    public const int QueryValidationFailed = 5030;

    /// <summary>
    /// 根据错误码获取默认描述。
    /// </summary>
    public static string GetMessage(int errorCode) => errorCode switch
    {
        EntityNotFound => "Entity not found.",
        EntityAlreadyExists => "Entity already exists.",
        DbOperationFailed => "Database operation failed.",
        ConcurrencyConflict => "Concurrency conflict detected. Please refresh and try again.",
        ConnectionFailed => "Database connection failed.",
        MigrationFailed => "Database migration failed.",
        QueryValidationFailed => "Query parameter validation failed.",
        _ => "Unknown persistence error."
    };
}
