using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System;
using System.Collections.Generic;

namespace Content.Server.ExtraArms;

public sealed class MedExosuitSystem : EntitySystem
{
    // ОБЯЗАТЕЛЬНЫЕ ЗАВИСИМОСТИ (без них строки в Update будут гореть красным)
    [Dependency] private readonly SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    // СПИСОК ФРАЗ (без него AlertPhrases на скриншоте выдает ошибку)
    private static readonly List<string> AlertPhrases = new()
    {
        "Внимание! Зафиксировано падение критических показателей! Ввожу стимуляторы.",
        "Критическое состояние носителя! Активация протокола экстренной реанимации.",
        "Обнаружена угроза остановки сердца. Эпинефрин введен.",
        "Жизненные показатели нестабильны! Не умирайте на дежурстве, сотрудник."
    };

    private readonly HashSet<EntityUid> _activeSuits = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MedExosuitProviderComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<MedExosuitProviderComponent, GotUnequippedEvent>(OnUnequipped);
    }

    private void OnEquipped(EntityUid uid, MedExosuitProviderComponent component, GotEquippedEvent args)
    {
        if (args.Slot == "back")
            _activeSuits.Add(uid);
    }

    private void OnUnequipped(EntityUid uid, MedExosuitProviderComponent component, GotUnequippedEvent args)
    {
        if (args.Slot == "back")
            _activeSuits.Remove(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _timing.CurTime;

        foreach (var suitUid in _activeSuits)
        {
            // 1. Проверяем наличие компонента нашего костюма
            if (!TryComp<MedExosuitProviderComponent>(suitUid, out var suitComp))
                continue;

            // 2. Ищем бак с лекарствами внутри костюма
            if (!_solutionSystem.TryGetSolution(suitUid, "injector_buffer", out var suitSolutionEnt, out var suitSolution))
                continue;

            // 3. Проверяем кулдаун времени между уколами
            if (currentTime < suitComp.NextInjectTime)
                continue;

            // 4. Находим сущность игрока, на которого надет костюм
            var user = Transform(suitUid).ParentUid;

            // 5. Проверяем валидность игрока и наличие у него компонента здоровья
            if (!user.IsValid() || !TryComp<MobStateComponent>(user, out var mobStateComp))
                continue;

            // 6. Проверяем, находится ли игрок строго в состоянии КРИТА
            if (mobStateComp.CurrentState != MobState.Critical)
                continue;

            // 7. Ищем кровеносную систему или бак реагентов игрока для ввода лекарства
            if (!_solutionSystem.TryGetSolution(user, "bloodstream", out var userSolutionEnt, out _))
            {
                if (!_solutionSystem.TryGetSolution(user, "reagents", out userSolutionEnt, out _))
                    continue;
            }

            // 8. Рассчитываем дозу укола
            FixedPoint2 injectValue = FixedPoint2.New(suitComp.InjectAmount);
            var splitAmount = suitSolution.Volume < injectValue ? suitSolution.Volume : injectValue;

            // 9. Вытаскиваем раствор из костюма и вводим его в игрока
            var removed = _solutionSystem.SplitSolution(suitSolutionEnt.Value, splitAmount);
            _solutionSystem.TryAddSolution(userSolutionEnt.Value, removed);

            // --- БЕСКОНЕЧНЫЙ ЗАПАС: Доливаем обратно в бак костюма столько же, сколько вкололи ---
            _solutionSystem.TryAddSolution(suitSolutionEnt.Value, removed);

            // 10. Выбираем случайную фразу реанимации и выводим её над игроком
            var alertIndex = _random.Next(0, AlertPhrases.Count);
            var text = AlertPhrases[alertIndex];
            _popupSystem.PopupEntity(text, user);

            // 11. Устанавливаем кулдаун на следующий укол
            suitComp.NextInjectTime = currentTime + suitComp.InjectCooldown;
        }
    }
}
