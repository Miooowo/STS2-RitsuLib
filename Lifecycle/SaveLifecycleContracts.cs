using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves;

namespace STS2RitsuLib
{
    public readonly record struct ProfileIdInitializedEvent(
        SaveManager SaveManager,
        int ProfileId,
        DateTimeOffset OccurredAtUtc
    ) : IReplayableFrameworkLifecycleEvent;

    public readonly record struct ProfileSwitchingEvent(
        SaveManager SaveManager,
        int? PreviousProfileId,
        int NextProfileId,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct ProfileSwitchedEvent(
        SaveManager SaveManager,
        int? PreviousProfileId,
        int CurrentProfileId,
        DateTimeOffset OccurredAtUtc
    ) : IReplayableFrameworkLifecycleEvent;

    public readonly record struct RunSavingEvent(
        SaveManager SaveManager,
        AbstractRoom? PreFinishedRoom,
        bool SaveProgress,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct RunSavedEvent(
        SaveManager SaveManager,
        AbstractRoom? PreFinishedRoom,
        bool SaveProgress,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct ProgressSavingEvent(
        SaveManager SaveManager,
        int? ProfileId,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct ProgressSavedEvent(
        SaveManager SaveManager,
        int? ProfileId,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct ProfileDeletingEvent(
        SaveManager SaveManager,
        int ProfileId,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct ProfileDeletedEvent(
        SaveManager SaveManager,
        int ProfileId,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;
}
