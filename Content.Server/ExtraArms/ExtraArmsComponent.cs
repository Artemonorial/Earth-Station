using Robust.Shared.GameObjects;
using System.Collections.Generic;

namespace Content.Server.ExtraArms;

/// <summary>
/// Компонент для предметов одежды, которые предоставляют дополнительные руки.
/// </summary>
[RegisterComponent]
[ComponentProtoName("ExtraArmsProvider")] // Явно указываем имя для YAML
public sealed partial class ExtraArmsProviderComponent : Component
{
    /// <summary>
    /// Список уникальных ID слотов рук, которые были выданы пользователю.
    /// </summary>
    [ViewVariables]
    public List<string> SpawnedHandIds = new();
}
