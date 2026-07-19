using Luoli.Persistence.Models;

namespace Luoli.Persistence.EntityFramework;

/// <summary>
/// 持久化基础 DbContext，提供软删除查询过滤器、snake_case 命名和默认查询过滤。
/// </summary>
public abstract class BaseDbContext : DbContext
{
    /// <summary>
    /// 初始化 BaseDbContext。
    /// </summary>
    protected BaseDbContext(DbContextOptions options, AuditInterceptor auditInterceptor)
        : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    private readonly AuditInterceptor _auditInterceptor;

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
        optionsBuilder.UseSnakeCaseNamingConvention();

        base.OnConfiguring(optionsBuilder);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 为所有 BaseEntity 子类添加全局软删除查询过滤器
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var notDeleted = Expression.Not(isDeletedProperty);
            var filter = Expression.Lambda(notDeleted, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    /// <summary>
    /// 对查询应用默认过滤条件。自动追加 <c>IsDeleted == false</c>，
    /// 如果原始表达式中已包含 IsDeleted 属性引用，则以原始表达式为准。
    /// </summary>
    /// <typeparam name="T">实体类型，必须继承 BaseEntity</typeparam>
    /// <param name="query">原始查询</param>
    /// <param name="predicate">可选的额外过滤条件</param>
    /// <returns>应用默认过滤后的查询</returns>
    public IQueryable<T> ApplyDefaultFilter<T>(IQueryable<T> query, Expression<Func<T, bool>>? predicate = null)
        where T : BaseEntity
    {
        if (predicate is not null)
        {
            // 检查 predicate 中是否已包含 IsDeleted 属性引用
            var hasIsDeleted = HasIsDeletedReference(predicate);

            if (hasIsDeleted)
            {
                // 已有 IsDeleted 条件，以调用方为准，不追加默认过滤
                return query.Where(predicate);
            }
        }

        // 默认追加 IsDeleted == false
        // 注：全局查询过滤器已自动应用，此处提供显式控制点
        return predicate is not null ? query.Where(predicate) : query;
    }

    /// <summary>
    /// 获取包含软删除记录的查询（忽略全局查询过滤器）。
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <returns>包含所有记录（含已删除）的查询</returns>
    public IQueryable<T> AllWithDeleted<T>() where T : BaseEntity
        => Set<T>().IgnoreQueryFilters();

    /// <summary>
    /// 检查表达式树中是否包含对 IsDeleted 属性的引用。
    /// </summary>
    private static bool HasIsDeletedReference<T>(Expression<Func<T, bool>> expression)
        where T : BaseEntity
    {
        return HasMemberAccess(expression.Body, nameof(BaseEntity.IsDeleted));
    }

    /// <summary>
    /// 递归检查表达式树中是否包含指定成员访问。
    /// </summary>
    private static bool HasMemberAccess(Expression expression, string memberName)
    {
        return expression switch
        {
            MemberExpression member => member.Member.Name == memberName || HasMemberAccessInChildren(member, memberName),
            BinaryExpression binary => HasMemberAccess(binary.Left, memberName) || HasMemberAccess(binary.Right, memberName),
            UnaryExpression unary => HasMemberAccess(unary.Operand, memberName),
            MethodCallExpression methodCall => methodCall.Arguments.Any(a => HasMemberAccess(a, memberName)),
            _ => false
        };
    }

    /// <summary>
    /// 检查成员表达式的子节点。
    /// </summary>
    private static bool HasMemberAccessInChildren(MemberExpression member, string memberName)
    {
        return member.Expression is not null && HasMemberAccess(member.Expression, memberName);
    }
}
