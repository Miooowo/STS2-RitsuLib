using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace STS2RitsuLib
{
    public readonly record struct GoldGainedEvent(
        IRunState RunState,
        Player Player,
        int GoldTotal,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct GoldLostEvent(
        Player Player,
        decimal Amount,
        GoldLossType LossType,
        int GoldTotal,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct PotionProcuredEvent(
        IRunState RunState,
        CombatState? CombatState,
        PotionModel Potion,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct PotionDiscardedEvent(
        IRunState RunState,
        CombatState? CombatState,
        PotionModel Potion,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct RelicObtainedEvent(
        Player Player,
        RelicModel Relic,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct RelicRemovedEvent(
        Player Player,
        RelicModel Relic,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct RewardTakenEvent(
        IRunState RunState,
        Player Player,
        Reward Reward,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;
}
