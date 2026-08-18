using DedsiNative.Permissions.Events;
using DedsiNative.Positions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace DedsiNative.Applications.Menus.EventHandlers;

/// <summary>
/// 权限名称变更事件处理器，批量刷新岗位中的权限名称快照。
/// </summary>
/// <param name="positionRepository">岗位聚合仓储。</param>
public sealed class PermissionNameChangedEventHandler(
    IPositionRepository positionRepository)
    : ILocalEventHandler<PermissionNameChangedEvent>, ITransientDependency
{
    /// <summary>
    /// 处理权限名称变更事件。
    /// </summary>
    /// <param name="eventData">权限名称变更事件。</param>
    public async Task HandleEventAsync(PermissionNameChangedEvent eventData)
    {
        var positions = await positionRepository.GetByPermissionIdAsync(eventData.PermissionId, CancellationToken.None);
        foreach (var position in positions)
        {
            position.ChangePermissionName(eventData.PermissionId, eventData.NewName);
            await positionRepository.UpdateAsync(position, false);
        }
    }
}
