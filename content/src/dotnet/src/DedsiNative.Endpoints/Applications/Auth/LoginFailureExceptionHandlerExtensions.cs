using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Volo.Abp;

namespace DedsiNative.Applications.Auth;

/// <summary>
/// 登录失败异常响应处理中间件扩展。
/// </summary>
public static class LoginFailureExceptionHandlerExtensions
{
    /// <summary>
    /// 将认证流程抛出的空消息 <see cref="UserFriendlyException"/> 稳定转换为 HTTP 500，
    /// 同时避免向客户端返回内部登录失败原因。
    /// </summary>
    /// <param name="app">ASP.NET Core 应用构建器。</param>
    /// <returns>当前应用构建器。</returns>
    public static IApplicationBuilder UseLoginFailureExceptionHandler(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.Use(async (httpContext, next) =>
        {
            try
            {
                await next(httpContext);
            }
            catch (UserFriendlyException exception)
                when (string.IsNullOrEmpty(exception.Message))
            {
                if (httpContext.Response.HasStarted)
                {
                    throw;
                }

                httpContext.Response.Clear();
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                httpContext.Response.ContentType = "application/json; charset=utf-8";
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    statusCode = StatusCodes.Status500InternalServerError,
                    message = string.Empty
                }, httpContext.RequestAborted);
            }
        });
    }
}
