using Dedsi.CleanArchitecture.Infrastructure;
using DedsiNative.EntityFrameworkCore;
using DedsiNative.Permissions;
using DedsiNative.Positions;
using DedsiNative.Menus;
using DedsiNative.Users;
using DedsiNative.Dictionaries;
using DedsiNative.LoginAudits;
using DedsiNative.Organizations;
using DedsiNative.StorageFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.Minio;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;
using Volo.Abp.Timing;

namespace DedsiNative;

/// <summary>
/// DedsiNative 基础设施层模块，负责注册 EntityFrameworkCore 数据库上下文、ABP BlobStoring MinIO 对象存储及仓储实现。
/// </summary>
[DependsOn(
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpBlobStoringMinioModule),
    typeof(DedsiNativeCoreModule),
    typeof(DedsiCleanArchitectureInfrastructureModule)
)]
public class DedsiNativeInfrastructureModule : AbpModule
{
    static DedsiNativeInfrastructureModule()
    {
        // 允许直接以本地时间（北京时间）读写 PostgreSQL timestamp without time zone。
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    /// <summary>
    /// 配置基础设施层所需的服务，注册 EF Core 数据库上下文并启用默认仓储。
    /// </summary>
    /// <param name="context">服务配置上下文，提供服务注册能力。</param>
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostEnvironment = context.Services.GetAbpHostEnvironment();
        var configuration = context.Services.GetConfiguration();
        
        // EntityFrameworkCore
        context.Services.AddAbpDbContext<DedsiNativeDbContext>(options =>
        {
            options.AddDefaultRepositories(true);
        });
        
        // 审计与实体时间字段使用本地时间（北京时间）。
        Configure<AbpClockOptions>(options =>
        {
            options.Kind = DateTimeKind.Local;
        });

        // 数据库
        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(dbConfigContext =>
            {
                // 本地研发环境 - 输出到控制台
                if (hostEnvironment.IsDevelopment())
                {
                    dbConfigContext
                        .DbContextOptions
                        .LogTo(Console.WriteLine, [DbLoggerCategory.Database.Command.Name]).
                        EnableSensitiveDataLogging();
                }
                dbConfigContext.UseNpgsql();
            });
        });

        // 配置 ABP BlobStoring MinIO 默认存储容器
        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.ConfigureDefault(container =>
            {
                container.UseMinio(minio =>
                {
                    var connectionString = configuration.GetConnectionString("DedsiCohenMinio");
                    string endpoint = string.Empty;
                    string accessKey = string.Empty;
                    string secretKey = string.Empty;
                    string bucketName = configuration["Minio:BucketName"] ?? "dedsinative";
                    bool useSsl = false;

                    if (!string.IsNullOrWhiteSpace(connectionString))
                    {
                        var dict = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                            .Select(part => part.Split('=', 2))
                            .Where(parts => parts.Length == 2)
                            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim(), StringComparer.OrdinalIgnoreCase);

                        if (dict.TryGetValue("Endpoint", out var ep))
                        {
                            endpoint = ep.Replace("http://", "").Replace("https://", "").TrimEnd('/');
                        }
                        if (dict.TryGetValue("AccessKey", out var ak)) accessKey = ak;
                        if (dict.TryGetValue("SecretKey", out var sk)) secretKey = sk;
                        if (dict.TryGetValue("BucketName", out var bn)) bucketName = bn;
                        if (dict.TryGetValue("UseSSL", out var sslStr) && bool.TryParse(sslStr, out var parsedSsl)) useSsl = parsedSsl;
                    }
                    else
                    {
                        endpoint = configuration["Minio:Endpoint"] ?? string.Empty;
                        accessKey = configuration["Minio:AccessKey"] ?? string.Empty;
                        secretKey = configuration["Minio:SecretKey"] ?? string.Empty;
                        if (bool.TryParse(configuration["Minio:UseSSL"], out var parsedSsl))
                        {
                            useSsl = parsedSsl;
                        }
                    }

                    minio.EndPoint = endpoint;
                    minio.AccessKey = accessKey;
                    minio.SecretKey = secretKey;
                    minio.BucketName = bucketName;
                    minio.WithSSL = useSsl;
                    minio.CreateBucketIfNotExists = true;
                });
            });
        });

        Configure<AbpEntityOptions>(options =>
        {
            // User
            options.Entity<User>(userOptions =>
            {
                userOptions.DefaultWithDetailsFunc = query => query
                    .Include(u => u.Positions)
                    .Include(u => u.Organizations)
                    .Include(u => u.LoginInfo);
            });

            // System
            options.Entity<DedsiNative.Systems.System>(systemOptions =>
            {
                systemOptions.DefaultWithDetailsFunc = query => query;
            });

            // Permission
            options.Entity<Permission>(permissionOptions =>
            {
                permissionOptions.DefaultWithDetailsFunc = query => query;
            });

            // Position
            options.Entity<Position>(positionOptions =>
            {
                positionOptions.DefaultWithDetailsFunc = query => query
                    .Include(position => position.Permissions)
                    .Include(position => position.Organizations);
            });

            options.Entity<Menu>(menuOptions =>
            {
                menuOptions.DefaultWithDetailsFunc = query => query;
            });

            options.Entity<Dictionary>(dictionaryOptions =>
            {
                dictionaryOptions.DefaultWithDetailsFunc = query => query
                    .Include(dictionary => dictionary.Items);
            });

            // LoginAudit 没有聚合内部导航，但仍显式声明详情查询行为。
            options.Entity<LoginAudit>(loginAuditOptions =>
            {
                loginAuditOptions.DefaultWithDetailsFunc = query => query;
            });

            options.Entity<Organization>(orgOptions =>
            {
                orgOptions.DefaultWithDetailsFunc = query => query;
            });

            options.Entity<StorageFile>(storageOptions =>
            {
                storageOptions.DefaultWithDetailsFunc = query => query;
            });
        });

        // 注册 MinIO 对象存储提供者（基于 ABP Blob Storing）
        context.Services.AddTransient<DedsiNative.StorageFiles.IStorageProvider, DedsiNative.Infrastructure.StorageFiles.MinioStorageProvider>();
    }
}
