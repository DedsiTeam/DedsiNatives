using FastEndpoints;
using FluentValidation;
using OpenIddict.Abstractions;
using Volo.Abp;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.ApplicationEndpoints;

/// <summary>
/// 创建 OpenIddict 客户端请求。
/// </summary>
public record CreateOpenIddictApplicationRequest(
    string ClientId,
    string DisplayName,
    string ClientType,
    string? ClientSecret,
    string? ConsentType,
    string[]? RedirectUris,
    string[]? PostLogoutRedirectUris,
    string[]? Permissions,
    string[]? Requirements);

/// <summary>
/// 客户端创建验证器。
/// </summary>
public class CreateOpenIddictApplicationValidator : Validator<CreateOpenIddictApplicationRequest>
{
    public CreateOpenIddictApplicationValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId 不能为空")
            .MaximumLength(100).WithMessage("ClientId 最大长度为 100 字符");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("客户端显示名称不能为空");

        RuleFor(x => x.ClientType)
            .Must(t => t == OpenIddictConstants.ClientTypes.Public || t == OpenIddictConstants.ClientTypes.Confidential)
            .WithMessage("客户端类型必须为 public 或 confidential");
    }
}

/// <summary>
/// 创建 SSO 客户端端点。
/// </summary>
public class CreateOpenIddictApplicationEndpoint(IOpenIddictApplicationManager applicationManager)
    : Endpoint<CreateOpenIddictApplicationRequest, string>
{
    public override void Configure()
    {
        Post("/api/openiddict/applications");
        Policies(OpenIddict.OpenIddictPermissions.Manage);
        Description(x => x.WithTags("SSO 客户端管理"));
        Summary(s =>
        {
            s.Summary = "创建 SSO 客户端应用";
            s.Description = "注册新的 OpenIddict 客户端应用，配置重定向地址、授权类型与作用域权限。";
        });
    }

    public override async Task HandleAsync(CreateOpenIddictApplicationRequest req, CancellationToken ct)
    {
        var existing = await applicationManager.FindByClientIdAsync(req.ClientId.Trim(), ct);
        if (existing is not null)
        {
            throw new UserFriendlyException($"ClientId '{req.ClientId}' 已存在，请使用其他标识。");
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = req.ClientId.Trim(),
            DisplayName = req.DisplayName.Trim(),
            ClientType = req.ClientType,
            ClientSecret = string.IsNullOrWhiteSpace(req.ClientSecret) ? null : req.ClientSecret.Trim(),
            ConsentType = string.IsNullOrWhiteSpace(req.ConsentType) ? OpenIddictConstants.ConsentTypes.Explicit : req.ConsentType
        };

        if (req.RedirectUris != null)
        {
            foreach (var uri in req.RedirectUris.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                if (Uri.TryCreate(uri.Trim(), UriKind.Absolute, out var parsed))
                {
                    descriptor.RedirectUris.Add(parsed);
                }
            }
        }

        if (req.PostLogoutRedirectUris != null)
        {
            foreach (var uri in req.PostLogoutRedirectUris.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                if (Uri.TryCreate(uri.Trim(), UriKind.Absolute, out var parsed))
                {
                    descriptor.PostLogoutRedirectUris.Add(parsed);
                }
            }
        }

        if (req.Permissions != null)
        {
            foreach (var perm in req.Permissions.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                descriptor.Permissions.Add(perm.Trim());
            }
        }

        if (req.Requirements != null)
        {
            foreach (var reqItem in req.Requirements.Where(r => !string.IsNullOrWhiteSpace(r)))
            {
                descriptor.Requirements.Add(reqItem.Trim());
            }
        }

        var app = await applicationManager.CreateAsync(descriptor, ct);
        var id = await applicationManager.GetIdAsync(app, ct);

        await Send.OkAsync(id ?? req.ClientId, ct);
    }
}
