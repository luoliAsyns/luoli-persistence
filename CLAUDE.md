# Luoli.Persistence

> .NET 10 持久化基础类库 — 为所有业务项目提供统一的数据库操作能力。

## 项目职责

- 提供统一实体基类 `BaseEntity`（Id / CreatedAt / UpdatedAt / CreatedBy / UpdatedBy / IsDeleted）
- 提供 EF Core 审计拦截器（自动填充审计字段 + 软删除转换）
- 提供 `BaseDbContext`（全局软删除查询过滤器 + snake_case 命名 + 默认查询过滤）
- 提供泛型仓储 `IRepository<T>` / `Repository<T>`（CRUD + 分页查询）
- 提供多数据库支持抽象（首期 PostgreSQL）
- 软删除：所有删除操作转为 `IsDeleted = true`，全局查询过滤器自动排除已删除记录

## 技术栈

- .NET 10
- Microsoft.EntityFrameworkCore
- Npgsql.EntityFrameworkCore.PostgreSQL
- EFCore.NamingConventions（snake_case）

## 目录结构

```
Luoli.Persistence/
├── Models/                    # BaseEntity, PageResult<T>, IUserContext, PersistenceOptions
├── EntityFramework/           # BaseDbContext, AuditInterceptor, IRepository<T>, Repository<T>
└── Extensions/                # PersistenceExtensions（DI 注册）

Luoli.Persistence.Demo.Host/   # Demo WebAPI，FastEndpoints + Code First
Luoli.Persistence.Tests/       # 单元测试 (xUnit)
```

## 关键约定

- 所有实体继承 `BaseEntity`，主键为 `Guid`
- 软删除通过 `AuditInterceptor` 自动拦截 `Remove()` → 转为 `IsDeleted = true`
- 查询默认追加 `IsDeleted == false` 条件；如表达式已含 `IsDeleted` 则以调用方为准
- 列名全局使用 snake_case（通过 `EFCore.NamingConventions`）
- 审计字段 `CreatedBy` / `UpdatedBy` 通过 `IUserContext` 接口获取
- 路由不使用版本号前缀，直接 `/api/{resource}`
- 引用 Luoli.Common 项目（`E:\Code\repos\Luoli.Common`）
