using FastEndpoints;
using FluentValidation;
using OpenIddict.Abstractions;
using Volo.Abp;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.ScopeEndpoints;

/// <summary>
/// 创建 OpenIddict 作用域请求。
/// </summary>
public record CreateOpenIddictScopeRequest(
    string Name,
    string? DisplayName,
    string? Description,
    string[]? Resources);

/// <summary>
/// 作用域创建验证器。
/// </summary>
public class CreateOpenIddictScopeValidator : Validator<CreateOpenIddictScopeRequest>
{
    public CreateOpenIddictScopeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("作用域 Name 不能为空")
            .MaximumLength(200).WithMessage("作用域 Name 最大长度为 200 字符");
    }
}

/// <summary>
/// 创建 SSO 作用域端点。
/// </summary>
public class CreateOpenIddictScopeEndpoint(IOpenIddictScopeManager scopeManager)
    : Endpoint<CreateOpenIddictScopeRequest, string>
{
    public override void Configure()
    {
        Post("/api/openiddict/scopes");
        Policies(OpenIddict.OpenIddictPermissions.Manage);
        Description(x => x.WithTags("SSO 作用域管理"));
        Summary(s =>
        {
            s.Summary = "创建 SSO 作用域";
            s.Description = "注册新的 OpenIddict 作用域 (Scope) 及关联的目标资源。";
        });
    }

    public override async Task HandleAsync(CreateOpenIddictScopeRequest req, CancellationToken ct)
    {
        var existing = await scopeManager.FindByNameAsync(req.Name.Trim(), ct);
        if (existing is not null)
        {
            throw new UserFriendlyException($"作用域 '{req.Name}' 已存在。");
        }

        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = req.Name.Trim(),
            DisplayName = req.DisplayName?.Trim(),
            Description = req.Description?.Trim()
        };

        if (req.Resources != null)
        {
            foreach (var res in req.Resources.Where(r => !string.IsNullOrWhiteSpace(r)))
            {
                descriptor.Resources.Add(res.Trim());
            }
        }

        var scope = await scopeManager.CreateAsync(descriptor, ct);
        var id = await scopeManager.GetIdAsync(scope, ct);

        await Send.OkAsync(id ?? req.Name, ct);
    }
}
