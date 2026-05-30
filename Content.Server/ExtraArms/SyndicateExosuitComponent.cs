using Robust.Shared.GameObjects;

namespace Content.Server.ExtraArms;

[RegisterComponent]
public sealed partial class SyndicateExosuitProviderComponent : Component
{
    // Количество дополнительного урона (10 единиц)
    [DataField]
    public float BonusDamage = 10f;
}
