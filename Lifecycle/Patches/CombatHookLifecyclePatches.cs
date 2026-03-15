using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Lifecycle.Patches
{
    public class CombatHookLifecyclePatch : IPatchMethod
    {
        public static string PatchId => "combat_hook_lifecycle";
        public static string Description => "Publish fine-grained combat, card, turn, and death lifecycle events";
        public static bool IsCritical => false;

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(Hook), nameof(Hook.BeforeCombatStart), [typeof(IRunState), typeof(CombatState)]),
                new(typeof(Hook), nameof(Hook.AfterCombatEnd),
                    [typeof(IRunState), typeof(CombatState), typeof(CombatRoom)]),
                new(typeof(Hook), nameof(Hook.AfterCombatVictory),
                    [typeof(IRunState), typeof(CombatState), typeof(CombatRoom)]),
                new(typeof(Hook), nameof(Hook.BeforeSideTurnStart), [typeof(CombatState), typeof(CombatSide)]),
                new(typeof(Hook), nameof(Hook.AfterSideTurnStart), [typeof(CombatState), typeof(CombatSide)]),
                new(typeof(Hook), nameof(Hook.BeforeCardPlayed), [typeof(CombatState), typeof(CardPlay)]),
                new(typeof(Hook), nameof(Hook.AfterCardPlayed),
                    [typeof(CombatState), typeof(PlayerChoiceContext), typeof(CardPlay)]),
                new(typeof(Hook), nameof(Hook.AfterCardChangedPiles),
                [
                    typeof(IRunState), typeof(CombatState), typeof(CardModel), typeof(PileType), typeof(AbstractModel),
                ]),
                new(typeof(Hook), nameof(Hook.AfterCardDrawn),
                    [typeof(CombatState), typeof(PlayerChoiceContext), typeof(CardModel), typeof(bool)]),
                new(typeof(Hook), nameof(Hook.AfterCardDiscarded),
                    [typeof(CombatState), typeof(PlayerChoiceContext), typeof(CardModel)]),
                new(typeof(Hook), nameof(Hook.AfterCardExhausted),
                    [typeof(CombatState), typeof(PlayerChoiceContext), typeof(CardModel), typeof(bool)]),
                new(typeof(Hook), nameof(Hook.AfterCardRetained), [typeof(CombatState), typeof(CardModel)]),
                new(typeof(Hook), nameof(Hook.BeforeDeath), [typeof(IRunState), typeof(CombatState), typeof(Creature)]),
                new(typeof(Hook), nameof(Hook.AfterDeath),
                    [typeof(IRunState), typeof(CombatState), typeof(Creature), typeof(bool), typeof(float)]),
            ];
        }

        // ReSharper disable InconsistentNaming
        public static void Prefix(MethodBase __originalMethod, object[] __args)
            // ReSharper restore InconsistentNaming
        {
            switch (__originalMethod.Name)
            {
                case nameof(Hook.BeforeCombatStart):
                    RitsuLibFramework.PublishLifecycleEvent(
                        new CombatStartingEvent(
                            (IRunState)__args[0],
                            (CombatState?)__args[1],
                            DateTimeOffset.UtcNow
                        ),
                        nameof(CombatStartingEvent)
                    );
                    break;
                case nameof(Hook.BeforeSideTurnStart):
                    RitsuLibFramework.PublishLifecycleEvent(
                        new SideTurnStartingEvent(
                            (CombatState)__args[0],
                            (CombatSide)__args[1],
                            DateTimeOffset.UtcNow
                        ),
                        nameof(SideTurnStartingEvent)
                    );
                    break;
                case nameof(Hook.BeforeCardPlayed):
                    RitsuLibFramework.PublishLifecycleEvent(
                        new CardPlayingEvent(
                            (CombatState)__args[0],
                            (CardPlay)__args[1],
                            DateTimeOffset.UtcNow
                        ),
                        nameof(CardPlayingEvent)
                    );
                    break;
                case nameof(Hook.BeforeDeath):
                    RitsuLibFramework.PublishLifecycleEvent(
                        new CreatureDyingEvent(
                            (IRunState)__args[0],
                            (CombatState?)__args[1],
                            (Creature)__args[2],
                            DateTimeOffset.UtcNow
                        ),
                        nameof(CreatureDyingEvent)
                    );
                    break;
            }
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(MethodBase __originalMethod, object[] __args, ref Task __result)
            // ReSharper restore InconsistentNaming
        {
            switch (__originalMethod.Name)
            {
                case nameof(Hook.AfterCombatEnd):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new CombatEndedEvent(
                                (IRunState)__args[0],
                                (CombatState?)__args[1],
                                (CombatRoom)__args[2],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(CombatEndedEvent)
                        ));
                    break;
                case nameof(Hook.AfterCombatVictory):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new CombatVictoryEvent(
                                (IRunState)__args[0],
                                (CombatState?)__args[1],
                                (CombatRoom)__args[2],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(CombatVictoryEvent)
                        ));
                    break;
                case nameof(Hook.AfterSideTurnStart):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new SideTurnStartedEvent(
                                (CombatState)__args[0],
                                (CombatSide)__args[1],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(SideTurnStartedEvent)
                        ));
                    break;
                case nameof(Hook.AfterCardPlayed):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new CardPlayedEvent(
                                (CombatState)__args[0],
                                (CardPlay)__args[2],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(CardPlayedEvent)
                        ));
                    break;
                case nameof(Hook.AfterCardChangedPiles):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new CardMovedBetweenPilesEvent(
                                (IRunState)__args[0],
                                (CombatState?)__args[1],
                                (CardModel)__args[2],
                                (PileType)__args[3],
                                (AbstractModel?)__args[4],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(CardMovedBetweenPilesEvent)
                        ));
                    break;
                case nameof(Hook.AfterCardDrawn):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new CardDrawnEvent(
                                (CombatState)__args[0],
                                (CardModel)__args[2],
                                (bool)__args[3],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(CardDrawnEvent)
                        ));
                    break;
                case nameof(Hook.AfterCardDiscarded):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new CardDiscardedEvent(
                                (CombatState)__args[0],
                                (CardModel)__args[2],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(CardDiscardedEvent)
                        ));
                    break;
                case nameof(Hook.AfterCardExhausted):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new CardExhaustedEvent(
                                (CombatState)__args[0],
                                (CardModel)__args[2],
                                (bool)__args[3],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(CardExhaustedEvent)
                        ));
                    break;
                case nameof(Hook.AfterCardRetained):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new CardRetainedEvent(
                                (CombatState)__args[0],
                                (CardModel)__args[1],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(CardRetainedEvent)
                        ));
                    break;
                case nameof(Hook.AfterDeath):
                    __result = LifecyclePatchTaskBridge.After(__result, () =>
                        RitsuLibFramework.PublishLifecycleEvent(
                            new CreatureDiedEvent(
                                (IRunState)__args[0],
                                (CombatState?)__args[1],
                                (Creature)__args[2],
                                (bool)__args[3],
                                (float)__args[4],
                                DateTimeOffset.UtcNow
                            ),
                            nameof(CreatureDiedEvent)
                        ));
                    break;
            }
        }
    }
}
