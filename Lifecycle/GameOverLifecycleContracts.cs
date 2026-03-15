using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace STS2RitsuLib
{
    public readonly record struct GameOverScreenCreatedEvent(
        RunState RunState,
        SerializableRun SerializableRun,
        NGameOverScreen Screen,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;
}
