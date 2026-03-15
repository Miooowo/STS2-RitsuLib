using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace STS2RitsuLib
{
    public readonly record struct CombatStartingEvent(
        IRunState RunState,
        CombatState? CombatState,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct CombatEndedEvent(
        IRunState RunState,
        CombatState? CombatState,
        CombatRoom Room,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct CombatVictoryEvent(
        IRunState RunState,
        CombatState? CombatState,
        CombatRoom Room,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct SideTurnStartingEvent(
        CombatState CombatState,
        CombatSide Side,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct SideTurnStartedEvent(
        CombatState CombatState,
        CombatSide Side,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct CardPlayingEvent(
        CombatState CombatState,
        CardPlay CardPlay,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct CardPlayedEvent(
        CombatState CombatState,
        CardPlay CardPlay,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct CardMovedBetweenPilesEvent(
        IRunState RunState,
        CombatState? CombatState,
        CardModel Card,
        PileType PreviousPile,
        AbstractModel? Source,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct CardDrawnEvent(
        CombatState CombatState,
        CardModel Card,
        bool FromHandDraw,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct CardDiscardedEvent(
        CombatState CombatState,
        CardModel Card,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct CardExhaustedEvent(
        CombatState CombatState,
        CardModel Card,
        bool CausedByEthereal,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct CardRetainedEvent(
        CombatState CombatState,
        CardModel Card,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct CreatureDyingEvent(
        IRunState RunState,
        CombatState? CombatState,
        Creature Creature,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct CreatureDiedEvent(
        IRunState RunState,
        CombatState? CombatState,
        Creature Creature,
        bool WasRemovalPrevented,
        float DeathAnimationDurationSeconds,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;
}
