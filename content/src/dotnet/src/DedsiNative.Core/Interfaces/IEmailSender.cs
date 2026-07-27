using Volo.Abp.DependencyInjection;

namespace DedsiNative.Interfaces;

public interface IEmailSender: ITransientDependency
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="email"></param>
    /// <param name="subject"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task SendEmailAsync(string email, string subject, string message);
}