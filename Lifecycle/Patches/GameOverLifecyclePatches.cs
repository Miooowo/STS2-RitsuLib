using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Lifecycle.Patches
{
    public class GameOverScreenLifecyclePatch : IPatchMethod
    {
        public static string PatchId => "game_over_screen_lifecycle";
        public static string Description => "Publish lifecycle events when the game over screen is created";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(NGameOverScreen), nameof(NGameOverScreen.Create),
                    [typeof(RunState), typeof(SerializableRun)]),
            ];
        }

        // ReSharper disable once InconsistentNaming
        public static void Postfix(RunState runState, SerializableRun serializableRun, NGameOverScreen? __result)
        {
            if (__result == null)
                return;

            RitsuLibFramework.PublishLifecycleEvent(
                new GameOverScreenCreatedEvent(runState, serializableRun, __result, DateTimeOffset.UtcNow),
                nameof(GameOverScreenCreatedEvent)
            );
        }
    }
}
