using DedsiNative.Users;

namespace DedsiNative.Endpoints.UserEndpoints;

/// <summary>用户登录信息输入。</summary>
/// <param name="Account">登录账号。</param>
/// <param name="Password">登录密码；更新时为空则保留原密码。</param>
/// <param name="Status">账户状态。</param>
public sealed record UserLoginInfoRequest(string Account, string? Password, AccountStatus Status = AccountStatus.Normal);

/// <summary>用户登录信息响应，不包含密码哈希和盐值。</summary>
/// <param name="Account">登录账号。</param>
/// <param name="Status">账户状态。</param>
public sealed record UserLoginInfoResponse(string Account, AccountStatus Status);

