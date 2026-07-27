using DedsiNative;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

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
    
    #region DedsiNative
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            var scheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "请输入 JWT Token (格式: Bearer {token})"
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = scheme;

            var requirement = new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
            };
            document.Security ??= new List<OpenApiSecurityRequirement>();
            document.Security.Add(requirement);

            return Task.CompletedTask;
        });
    });
    #endregion
    
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
            options.AddPreferredSecuritySchemes("Bearer");
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