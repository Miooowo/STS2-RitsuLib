using MegaCrit.Sts2.Core.Saves;

namespace STS2RitsuLib
{
    public readonly record struct EpochObtainedEvent(
        SaveManager SaveManager,
        string EpochId,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct EpochRevealedEvent(
        SaveManager SaveManager,
        string EpochId,
        bool IsDebug,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct UnlockIncrementedEvent(
        SaveManager SaveManager,
        int TotalUnlocks,
        string? PendingEpochId,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;
}
