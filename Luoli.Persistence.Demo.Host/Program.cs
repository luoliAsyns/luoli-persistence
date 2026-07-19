using Luoli.Common.Logging;
using Luoli.Common.Middleware;
using Luoli.Persistence.Demo.Host.Data;
using Luoli.Persistence.Extensions;
using Luoli.Persistence.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Luoli.Common 基础设施
builder.AddLuoliCommon(options =>
{
    options.AppName = "Luoli.Persistence.Demo.Host";
    options.Logging = new LoggingOptions
    {
        LogFolder = Path.Combine(AppContext.BaseDirectory, "logs"),
        RetentionDays = 7
    };
});

// 2. Luoli.Persistence 持久化 — 默认值 + appsettings.json 覆盖
var persistenceOptions = new PersistenceOptions
{
    MigrationsAssembly = typeof(DemoDbContext).Assembly.FullName
};
builder.Configuration.GetSection("Persistence").Bind(persistenceOptions);
persistenceOptions.ConnectionString = builder.Configuration.GetConnectionString("Default")
    ?? persistenceOptions.ConnectionString;

builder.Services.AddLuoliPersistence<DemoDbContext>(persistenceOptions);

// 3. FastEndpoints
builder.Services.AddFastEndpoints();

// 4. 静态文件（单页 HTML 测试界面）
builder.Services.AddAntiforgery();

var app = builder.Build();

// 中间件管线
app.UseLuoliCommon();
app.UseLuoliFastEndpoints();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAntiforgery();

// 自动应用数据库迁移（开发环境 Code First）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();
