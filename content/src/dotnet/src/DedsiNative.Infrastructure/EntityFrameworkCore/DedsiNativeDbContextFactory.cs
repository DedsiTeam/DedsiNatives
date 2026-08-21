using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DedsiNative.EntityFrameworkCore;

/// <summary>EF Core 设计时上下文工厂，用于在不启动宿主模块的情况下生成迁移。</summary>
public sealed class DedsiNativeDbContextFactory : IDesignTimeDbContextFactory<DedsiNativeDbContext>
{
    /// <summary>创建用于迁移的数据库上下文。</summary>
    public DedsiNativeDbContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var options = new DbContextOptionsBuilder<DedsiNativeDbContext>()
            .UseNpgsql("Host=localhost;Port=10812;Database=DedsiNativeDB;Username=DedsiCohen;Password=N9wK3vR8mY7pQ2tX4cZ6sF1b")
            .Options;
        return new DedsiNativeDbContext(options);
    }
}
