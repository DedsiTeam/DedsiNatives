namespace DedsiNative.Users.Events;

/// <summary>
/// 用户创建完成后发布的本地领域事件。
/// </summary>
/// <param name="UserId">新用户的唯一标识。</param>
/// <param name="Name">新用户的名称快照。</param>
/// <param name="Email">新用户的邮箱快照。</param>
public sealed record UserCreatedEvent(
    string UserId,
    string Name,
    string Email);
