namespace DedsiNative.Permissions.Events;

/// <summary>
/// 权限名称变更事件，通知关联岗位刷新权限名称快照。
/// </summary>
/// <param name="PermissionId">发生变更的权限标识。</param>
/// <param name="OldName">变更前的权限名称。</param>
/// <param name="NewName">变更后的权限名称。</param>
/// <param name="SystemId">权限所属系统标识。</param>
public sealed record PermissionNameChangedEvent(
    string PermissionId,
    string OldName,
    string NewName,
    string SystemId);
