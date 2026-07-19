using Luoli.Persistence.EntityFramework;
using Luoli.Persistence.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Luoli.Persistence.Extensions;

/// <summary>
/// Luoli.Persistence DI 注册扩展方法。
/// </summary>
public static class PersistenceExtensions
{
    /// <summary>
    /// 注册 Luoli.Persistence 持久化服务。
    /// </summary>
    /// <typeparam name="TDbContext">业务 DbContext 类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置回调</param>
    public static IServiceCollection AddLuoliPersistence<TDbContext>(
        this IServiceCollection services,
        Action<PersistenceOptions> configure)
        where TDbContext : BaseDbContext
    {
        var options = new PersistenceOptions();
        configure(options);

        // 注册配置
        services.AddSingleton(options);

        // 注册用户上下文（默认空实现，业务项目可替换）
        services.TryAddSingleton<IUserContext, NoOpUserContext>();

        // 注册审计拦截器（Scoped，因为依赖 IUserContext）
        services.AddScoped<AuditInterceptor>();

        // 注册 DbContext
        services.AddDbContext<TDbContext>((provider, dbOptions) =>
        {
            ConfigureDatabaseProvider(dbOptions, options);

            // 注入审计拦截器
            var interceptor = provider.GetRequiredService<AuditInterceptor>();
            dbOptions.AddInterceptors(interceptor);

            // snake_case 命名
            dbOptions.UseSnakeCaseNamingConvention();
        });

        // 将抽象 BaseDbContext 转发到具体 TDbContext，供 Repository 等内部依赖使用
        services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<TDbContext>());

        // 注册开放泛型仓储
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }

    /// <summary>
    /// 根据配置选择数据库提供程序。
    /// </summary>
    private static void ConfigureDatabaseProvider(
        DbContextOptionsBuilder optionsBuilder,
        PersistenceOptions options)
    {
        switch (options.DatabaseProvider.ToLowerInvariant())
        {
            case "postgresql":
            case "postgres":
                optionsBuilder.UseNpgsql(options.ConnectionString, npgsql =>
                {
                    if (!string.IsNullOrWhiteSpace(options.MigrationsAssembly))
                        npgsql.MigrationsAssembly(options.MigrationsAssembly);
                });
                break;

            // 预留扩展点
            // case "sqlserver":
            //     optionsBuilder.UseSqlServer(options.ConnectionString);
            //     break;
            // case "mysql":
            //     optionsBuilder.UseMySql(options.ConnectionString, ServerVersion.AutoDetect(options.ConnectionString));
            //     break;

            default:
                throw new BusinessException(
                    2000,
                    $"Unsupported database provider: '{options.DatabaseProvider}'. " +
                    "Supported providers: PostgreSQL.");
        }
    }
}
