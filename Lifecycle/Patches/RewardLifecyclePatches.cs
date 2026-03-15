using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Lifecycle.Patches
{
    public class RewardHookLifecyclePatch : IPatchMethod
    {
        public static string PatchId => "reward_hook_lifecycle";
        public static string Description => "Publish reward, potion, and gold gain lifecycle events";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(Hook), nameof(Hook.AfterGoldGained), [typeof(IRunState), typeof(Player)]),
                new(typeof(Hook), nameof(Hook.AfterPotionProcured),
                    [typeof(IRunState), typeof(CombatState), typeof(PotionModel)]),
                new(typeof(Hook), nameof(Hook.AfterPotionDiscarded),
                    [typeof(IRunState), typeof(CombatState), typeof(PotionModel)]),
                new(typeof(Hook), nameof(Hook.AfterRewardTaken), [typeof(IRunState), typeof(Player), typeof(Reward)]),
            ];
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(MethodBase __originalMethod, object[] __args, ref Task __result)
            // ReSharper restore InconsistentNaming
        {
            switch (__originalMethod.Name)
            {
                case nameof(Hook.AfterGoldGained):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new GoldGainedEvent(
                                (IRunState)__args[0],
                                (Player)__args[1],
                                ((Player)__args[1]).Gold,
                                DateTimeOffset.UtcNow
                            ),
                            nameof(GoldGainedEvent)
                        ));
                    break;
                case nameof(Hook.AfterPotionProcured):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new PotionProcuredEvent(
                                (IRunState)__args[0],
                                (CombatState?)__args[1],
                                (PotionModel)__args[2],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(PotionProcuredEvent)
                        ));
                    break;
                case nameof(Hook.AfterPotionDiscarded):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new PotionDiscardedEvent(
                                (IRunState)__args[0],
                                (CombatState?)__args[1],
                                (PotionModel)__args[2],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(PotionDiscardedEvent)
                        ));
                    break;
                case nameof(Hook.AfterRewardTaken):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new RewardTakenEvent(
                                (IRunState)__args[0],
                                (Player)__args[1],
                                (Reward)__args[2],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(RewardTakenEvent)
                        ));
                    break;
            }
        }
    }

    public class GoldLossLifecyclePatch : IPatchMethod
    {
        public static string PatchId => "gold_loss_lifecycle";
        public static string Description => "Publish gold loss lifecycle events";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(PlayerCmd), nameof(PlayerCmd.LoseGold),
                    [typeof(decimal), typeof(Player), typeof(GoldLossType)]),
            ];
        }

        public static void Postfix(decimal amount, Player player, GoldLossType goldLossType)
        {
            RitsuLibFramework.PublishLifecycleEvent(
                new GoldLostEvent(player, amount, goldLossType, player.Gold, DateTimeOffset.UtcNow),
                nameof(GoldLostEvent)
            );
        }
    }

    public class RelicObtainedLifecyclePatch : IPatchMethod
    {
        public static string PatchId => "relic_obtained_lifecycle";
        public static string Description => "Publish relic obtain lifecycle events";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(RelicCmd), nameof(RelicCmd.Obtain), [typeof(RelicModel), typeof(Player), typeof(int)]),
            ];
        }

        // ReSharper disable once InconsistentNaming
        public static void Postfix(Player player, ref Task<RelicModel> __result)
        {
            __result = LifecyclePatchTaskBridge.After(__result, relic =>
                RitsuLibFramework.PublishLifecycleEvent(
                    new RelicObtainedEvent(player, relic, DateTimeOffset.UtcNow),
                    nameof(RelicObtainedEvent)
                ));
        }
    }

    public class RelicRemovedLifecyclePatch : IPatchMethod
    {
        public static string PatchId => "relic_removed_lifecycle";
        public static string Description => "Publish relic removal lifecycle events";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(RelicCmd), nameof(RelicCmd.Remove), [typeof(RelicModel)]),
            ];
        }

        // ReSharper disable once InconsistentNaming
        public static void Postfix(RelicModel relic, ref Task __result)
        {
            var owner = relic.Owner;
            __result = LifecyclePatchTaskBridge.After(__result, () =>
                RitsuLibFramework.PublishLifecycleEvent(
                    new RelicRemovedEvent(owner, relic, DateTimeOffset.UtcNow),
                    nameof(RelicRemovedEvent)
                ));
        }
    }
}
