using Content.Shared.Inventory.Events;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Hands;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Server.ExtraArms;

public sealed class ExtraArmsSystem : EntitySystem
{
    // Возвращаем официальную систему рук вашей сборки
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExtraArmsProviderComponent, GotEquippedEvent>(OnClothingEquipped);
        SubscribeLocalEvent<ExtraArmsProviderComponent, GotUnequippedEvent>(OnClothingUnequipped);
    }

    private void OnClothingEquipped(EntityUid uid, ExtraArmsProviderComponent component, GotEquippedEvent args)
    {
        if (args.Slot != "back")
            return;

        // Чистое и правильное получение сущности игрока из ивента
        var user = args.EquipTarget;

        if (!TryComp<HandsComponent>(user, out var handsComp))
            return;

        string leftHandId = $"ex_l_{uid}";
        string rightHandId = $"ex_r_{uid}";

        // Успешно прошедшее проверку приведение типов через кортеж
        Entity<HandsComponent?> targetEntity = (user, (HandsComponent?) handsComp);

        // Вызываем официальные методы вашей системы рук
        _handsSystem.AddHand(targetEntity, leftHandId, HandLocation.Left, "hands-comp-left-hand");
        component.SpawnedHandIds.Add(leftHandId);

        _handsSystem.AddHand(targetEntity, rightHandId, HandLocation.Right, "hands-comp-right-hand");
        component.SpawnedHandIds.Add(rightHandId);
    }

    private void OnClothingUnequipped(EntityUid uid, ExtraArmsProviderComponent component, GotUnequippedEvent args)
    {
        if (args.Slot != "back")
            return;

        var user = args.EquipTarget;

        if (!TryComp<HandsComponent>(user, out var handsComp))
            return;

        Entity<HandsComponent?> targetEntity = (user, (HandsComponent?) handsComp);

        foreach (var handId in component.SpawnedHandIds)
        {
            _handsSystem.RemoveHand(targetEntity, handId);
        }

        component.SpawnedHandIds.Clear();
    }
}
