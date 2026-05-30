using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using System;

namespace Content.Server.ExtraArms;

[RegisterComponent]
public sealed partial class MedExosuitProviderComponent : Component
{
    /// <summary>
    /// Сколько реагента вводить за один раз.
    /// </summary>
    [DataField]
    public float InjectAmount = 5f;

    /// <summary>
    /// Кулдаун между автоматическими уколами (в секундах).
    /// </summary>
    [DataField]
    public TimeSpan InjectCooldown = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Время следующего возможного укола.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextInjectTime;

    /// <summary>
    /// Время, когда костюм должен выдать следующую фразу периодической диагностики.
    /// </summary>
    public TimeSpan NextDiagnosticTime;
}
