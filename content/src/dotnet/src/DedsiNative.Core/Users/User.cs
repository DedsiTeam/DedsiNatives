using Dedsi.Ddd.Domain.Entities;
using DedsiNative.Users.Events;
using Volo.Abp;

namespace DedsiNative.Users;

/// <summary>
/// 用户聚合根实体，包含用户基本信息及相关业务操作。
/// </summary>
public class User : DedsiAggregateRoot<string>
{
    /// <summary>
    /// 用户名称允许的最大长度，与数据库字段约束保持一致。
    /// </summary>
    public const int MaxNameLength = 64;

    /// <summary>
    /// 用户邮箱允许的最大长度，与数据库字段约束保持一致。
    /// </summary>
    public const int MaxEmailLength = 256;

    /// <summary>
    /// 受保护的无参构造函数，供 ORM 框架反射实例化使用，禁止业务代码直接调用。
    /// </summary>
    protected User()
    {
    }

    /// <summary>
    /// 创建用户实体的业务构造函数。
    /// </summary>
    /// <param name="id">用户唯一标识（ULID 字符串）。</param>
    /// <param name="name">用户名称，不能为空或纯空白字符。</param>
    /// <param name="email">用户邮箱地址，不能为空或纯空白字符。</param>
    public User(string id, string name, string email) : base(id)
    {
        ChangeName(name);
        ChangeEmail(email);
        AddLocalEvent(new UserCreatedEvent(Id, Name, Email));
    }

    /// <summary>
    /// 用户名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 用户邮箱地址。
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// 修改用户名称。
    /// </summary>
    /// <param name="name">新的用户名称，不能为空或纯空白字符。</param>
    /// <returns>返回当前用户实体，支持链式调用。</returns>
    public User ChangeName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(
            name,
            nameof(name),
            MaxNameLength);
        return this;
    }

    /// <summary>
    /// 修改用户邮箱地址。
    /// </summary>
    /// <param name="email">新的邮箱地址，不能为空或纯空白字符。</param>
    /// <returns>返回当前用户实体，支持链式调用。</returns>
    public User ChangeEmail(string email)
    {
        Email = Check.NotNullOrWhiteSpace(
            email,
            nameof(email),
            MaxEmailLength);
        return this;
    }
}
