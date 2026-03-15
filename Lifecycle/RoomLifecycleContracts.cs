using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace STS2RitsuLib
{
    public readonly record struct RoomEnteringEvent(
        IRunState RunState,
        AbstractRoom Room,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct RoomEnteredEvent(
        IRunState RunState,
        AbstractRoom Room,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct RoomExitedEvent(
        RunManager RunManager,
        AbstractRoom Room,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct ActEnteringEvent(
        RunManager RunManager,
        int TargetActIndex,
        bool DoTransition,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct ActEnteredEvent(
        IRunState RunState,
        int CurrentActIndex,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct RewardsScreenContinuingEvent(
        RunManager RunManager,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;
}
