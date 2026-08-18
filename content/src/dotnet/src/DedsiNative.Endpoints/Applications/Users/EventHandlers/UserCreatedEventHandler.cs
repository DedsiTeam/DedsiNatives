using DedsiNative.Interfaces;
using DedsiNative.Users.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace DedsiNative.Endpoints.Applications.Users.EventHandlers;

/// <summary>
/// 用户创建事件处理器，向新用户发送账户创建通知。
/// </summary>
/// <param name="emailSender">邮件发送服务。</param>
public sealed class UserCreatedEventHandler(IEmailSender emailSender)
    : ILocalEventHandler<UserCreatedEvent>, ITransientDependency
{
    /// <summary>
    /// 发送用户账户创建通知。
    /// </summary>
    /// <param name="eventData">用户创建事件。</param>
    /// <returns>表示异步发送操作的任务。</returns>
    public Task HandleEventAsync(UserCreatedEvent eventData)
    {
        return emailSender.SendEmailAsync(
            eventData.Email,
            "您的 DedsiNative 账户已创建",
            $"您好，{eventData.Name}，您的 DedsiNative 账户已创建。");
    }
}
