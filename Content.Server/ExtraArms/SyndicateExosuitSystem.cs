using Content.Shared.Inventory;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Server.ExtraArms;

public sealed class SyndicateExosuitSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return;

        var attacker = args.User;

        if (!_inventorySystem.TryGetSlotEntity(attacker, "back", out var backItem))
            return;

        if (!TryComp<SyndicateExosuitProviderComponent>(backItem, out var suitComp))
            return;

        var damageSpec = new DamageSpecifier();
        damageSpec.DamageDict.Add("Blunt", suitComp.BonusDamage);

        foreach (var target in args.HitEntities)
        {
            // Самый стабильный метод вашей DamageableSystem: принимает чистый EntityUid,
            // наносит урон и автоматически прерывает DoAfter, как прописано на вашем ск личном скрине.
            _damageableSystem.TryChangeDamage(target, damageSpec, interruptsDoAfters: true, origin: attacker);
        }
    }
}
