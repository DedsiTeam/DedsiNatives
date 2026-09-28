using DedsiNative.AuthServer;
using Serilog;
using Serilog.Events;
using DedsiNative.Time;

BeijingTimeZoneGuard.EnsureConfigured();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Async(c => c.File(path: "Logs/logs.txt", rollingInterval: RollingInterval.Hour, retainedFileCountLimit: 168))
    .WriteTo.Async(c => c.Console())
    .CreateBootstrapLogger();

try
{
    Log.Information("DedsiNative AuthServer Starting...");
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();
    builder.Host
        .AddAppSettingsSecretsJson()
        .UseAutofac()
        .UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("OpenIddict", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Async(c => c.File(path: "Logs/logs.txt", rollingInterval: RollingInterval.Hour, retainedFileCountLimit: 168))
                .WriteTo.Async(c => c.Console())
                .WriteTo.Async(c => c.OpenTelemetry());

            // Aspire 只在引用 Seq 资源时注入地址；独立运行仍保留现有日志输出。
            var seqUri = context.Configuration["SEQ_URI"];
            if (!string.IsNullOrWhiteSpace(seqUri))
            {
                loggerConfiguration.WriteTo.Seq(seqUri);
            }
        });

    await builder.AddApplicationAsync<DedsiNativeAuthServerModule>();

    var app = builder.Build();

    app.MapDefaultEndpoints();
    await app.InitializeApplicationAsync();

    await app.RunAsync();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "DedsiNative AuthServer terminated unexpectedly!");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
