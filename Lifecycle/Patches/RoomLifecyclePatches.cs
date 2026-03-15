using System.Reflection;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Lifecycle.Patches
{
    public class RoomHookLifecyclePatch : IPatchMethod
    {
        public static string PatchId => "room_hook_lifecycle";
        public static string Description => "Publish room entry lifecycle events";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(Hook), nameof(Hook.BeforeRoomEntered), [typeof(IRunState), typeof(AbstractRoom)]),
                new(typeof(Hook), nameof(Hook.AfterRoomEntered), [typeof(IRunState), typeof(AbstractRoom)]),
            ];
        }

        public static void Prefix(IRunState runState, AbstractRoom room)
        {
            RitsuLibFramework.PublishLifecycleEvent(
                new RoomEnteringEvent(runState, room, DateTimeOffset.UtcNow),
                nameof(RoomEnteringEvent)
            );
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(MethodBase __originalMethod, object[] __args, ref Task __result)
            // ReSharper restore InconsistentNaming
        {
            switch (__originalMethod.Name)
            {
                case nameof(Hook.AfterRoomEntered):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new RoomEnteredEvent(
                                (IRunState)__args[0],
                                (AbstractRoom)__args[1],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(RoomEnteredEvent)
                        ));
                    break;
            }
        }
    }

    public class ActHookLifecyclePatch : IPatchMethod
    {
        public static string PatchId => "act_hook_lifecycle";
        public static string Description => "Publish act entry lifecycle events";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(Hook), nameof(Hook.AfterActEntered), [typeof(IRunState)]),
            ];
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(IRunState runState, ref Task __result)
            // ReSharper restore InconsistentNaming
        {
            __result = LifecyclePatchTaskBridge.After(__result, () =>
                RitsuLibFramework.PublishLifecycleEvent(
                    new ActEnteredEvent(runState, runState.CurrentActIndex, DateTimeOffset.UtcNow),
                    nameof(ActEnteredEvent)
                ));
        }
    }

    public class RoomExitLifecyclePatch : IPatchMethod
    {
        public static string PatchId => "room_exit_lifecycle";
        public static string Description => "Publish room exit lifecycle events";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(RunManager), "ExitCurrentRoom"),
            ];
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(RunManager __instance, ref Task<AbstractRoom?> __result)
            // ReSharper restore InconsistentNaming
        {
            __result = LifecyclePatchTaskBridge.After(__result, room =>
            {
                if (room == null)
                    return;

                RitsuLibFramework.PublishLifecycleEvent(
                    new RoomExitedEvent(__instance, room, DateTimeOffset.UtcNow),
                    nameof(RoomExitedEvent)
                );
            });
        }
    }

    public class ActTransitionLifecyclePatch : IPatchMethod
    {
        public static string PatchId => "act_transition_lifecycle";
        public static string Description => "Publish act transition and rewards continuation lifecycle events";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(RunManager), nameof(RunManager.EnterAct), [typeof(int), typeof(bool)]),
                new(typeof(RunManager), nameof(RunManager.ProceedFromTerminalRewardsScreen), Type.EmptyTypes),
            ];
        }

        // ReSharper disable InconsistentNaming
        public static void Prefix(MethodBase __originalMethod, RunManager __instance, object[] __args)
            // ReSharper restore InconsistentNaming
        {
            if (__originalMethod.Name != nameof(RunManager.EnterAct))
                return;

            RitsuLibFramework.PublishLifecycleEvent(
                new ActEnteringEvent(__instance, (int)__args[0], (bool)__args[1], DateTimeOffset.UtcNow),
                nameof(ActEnteringEvent)
            );
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(MethodBase __originalMethod, RunManager __instance, ref Task __result)
            // ReSharper restore InconsistentNaming
        {
            if (__originalMethod.Name != nameof(RunManager.ProceedFromTerminalRewardsScreen))
                return;

            __result = LifecyclePatchTaskBridge.After(__result, () =>
                RitsuLibFramework.PublishLifecycleEvent(
                    new RewardsScreenContinuingEvent(__instance, DateTimeOffset.UtcNow),
                    nameof(RewardsScreenContinuingEvent)
                ));
        }
    }
}
