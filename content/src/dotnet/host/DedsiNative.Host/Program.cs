using DedsiNative;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Async(c => c.File(path:"Logs/logs.txt", rollingInterval:RollingInterval.Hour, retainedFileCountLimit: 168))
    .WriteTo.Async(c => c.Console())
    .CreateBootstrapLogger();

try
{
    Log.Information("DedsiNative Host Starting...");
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
                .Enrich.FromLogContext()
                .WriteTo.Async(c => c.File(path:"Logs/logs.txt", rollingInterval:RollingInterval.Hour, retainedFileCountLimit: 168))
                .WriteTo.Async(c => c.Console())
                .WriteTo.Async(c => c.OpenTelemetry());
        });

    await builder.AddApplicationAsync<DedsiNativeHostModule>();

    var app = builder.Build();

    app.MapDefaultEndpoints();
    await app.InitializeApplicationAsync();

    #region DedsiNative
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.AddDocuments("v1");
            options.AddPreferredSecuritySchemes("JWTBearerAuth");
        });
    }
    #endregion


    await app.RunAsync();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "DedsiNative Host terminated unexpectedly!");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
