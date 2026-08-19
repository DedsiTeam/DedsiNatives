using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;

namespace DedsiNative.Exceptions;

/// <summary>
/// 自定义内部错误响应模型。
/// </summary>
public sealed class CustomInternalErrorResponse
{
    /// <summary>
    /// HTTP 状态码或业务错误码。
    /// </summary>
    public int Code { get; set; } = StatusCodes.Status500InternalServerError;

    /// <summary>
    /// 错误消息提示。
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 服务器时间。
    /// </summary>
    public DateTime ServiceTime { get; set; } = DateTime.Now;
}

/// <summary>
/// FastEndpoints 自定义异常处理中间件扩展。
/// 基于 FastEndpoints 官方异常处理器源码定制，支持结构化日志、环境自适应与安全原因过滤。
/// </summary>
public static class CustomExceptionHandlerExtensions
{
    /// <summary>
    /// 注册自定义未捕获异常处理中间件。
    /// </summary>
    /// <param name="app">应用程序构建器。</param>
    /// <param name="logger">可选的自定义日志记录器；为空时从请求服务容器解析。</param>
    /// <param name="logStructuredException">是否以结构化格式记录异常日志（默认 true）。</param>
    /// <param name="useGenericReason">是否使用泛化的安全错误原因（生产环境推荐 true，避免泄露内部堆栈或敏感信息）。</param>
    /// <returns>当前应用程序构建器。</returns>
    public static IApplicationBuilder UseCustomExceptionHandler(
        this IApplicationBuilder app,
        ILogger? logger = null,
        bool logStructuredException = true,
        bool useGenericReason = true)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseExceptionHandler(errApp =>
        {
            errApp.Run(async ctx =>
            {
                var exHandlerFeature = ctx.Features.Get<IExceptionHandlerFeature>();
                if (exHandlerFeature is null)
                {
                    return;
                }

                var ex = exHandlerFeature.Error;
                var log = logger ?? ctx.RequestServices.GetService<ILogger<CustomInternalErrorResponse>>()
                    ?? ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(CustomExceptionHandlerExtensions));

                if (logStructuredException)
                {
                    log.LogError(
                        ex,
                        "HTTP: {Method} {Path} | TYPE: {Type} | REASON: {Reason}",
                        ctx.Request.Method,
                        ctx.Request.Path,
                        ex.GetType().Name,
                        ex.Message);
                }
                else
                {
                    log.LogError(
                        """
                        =================================
                        HTTP: {Method} {Path}
                        TYPE: {Type}
                        REASON: {Reason}
                        ---------------------------------
                        {StackTrace}
                        """,
                        ctx.Request.Method,
                        ctx.Request.Path,
                        ex.GetType().Name,
                        ex.Message,
                        ex.StackTrace);
                }

                if (ctx.Response.HasStarted)
                {
                    return;
                }

                ctx.Response.Clear();
                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ctx.Response.ContentType = "application/problem+json; charset=utf-8";

                var isUserFriendly = ex is UserFriendlyException || ex is IBusinessException;
                var errorMessage = isUserFriendly || !useGenericReason
                    ? (string.IsNullOrWhiteSpace(ex.Message) ? "发生未知系统异常，请稍后重试。" : ex.Message)
                    : "发生未知系统异常，请稍后重试。";

                var errorResponse = new CustomInternalErrorResponse
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = errorMessage,
                    ServiceTime = DateTime.Now
                };

                await ctx.Response.WriteAsJsonAsync(errorResponse, ctx.RequestAborted);
            });
        });

        return app;
    }
}
