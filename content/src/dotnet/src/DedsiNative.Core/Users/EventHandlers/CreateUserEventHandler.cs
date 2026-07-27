using DedsiNative.Interfaces;
using DedsiNative.Users.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace DedsiNative.Users.EventHandlers;

public class CreateUserEventHandler(IEmailSender emailSender) : ILocalEventHandler<CreateUserEvent>, ITransientDependency
{
    public Task HandleEventAsync(CreateUserEvent eventData)
    {
        return emailSender.SendEmailAsync(eventData.User.Email, "测试发送邮件", $"您好啊 {eventData.User.Name}");
    }
}