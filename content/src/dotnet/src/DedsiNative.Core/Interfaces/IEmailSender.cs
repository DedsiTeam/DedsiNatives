using Volo.Abp.DependencyInjection;

namespace DedsiNative.Interfaces;

/// <summary>
/// 邮件发送服务契约。
/// </summary>
public interface IEmailSender : ITransientDependency
{
    /// <summary>
    /// 向指定邮箱发送邮件。
    /// </summary>
    /// <param name="email">收件人邮箱地址。</param>
    /// <param name="subject">邮件主题。</param>
    /// <param name="message">邮件正文。</param>
    /// <returns>表示异步发送操作的任务。</returns>
    Task SendEmailAsync(string email, string subject, string message);
}
