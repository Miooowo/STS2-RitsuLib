using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace STS2RitsuLib.Content.Patches
{
    /// <summary>
    ///     Force RitsuLib-registered content to use one fixed public entry format.
    ///     This keeps game localization keys and default asset paths predictable without extra rewrite patches.
    /// </summary>
    public class ModelDbModdedEntryPatch : IPatchMethod
    {
        public static string PatchId => "modeldb_modded_entry_identity";

        public static string Description =>
            "Force RitsuLib-registered models to use a fixed mod-scoped ModelDb entry format";

        public static bool IsCritical => true;

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(ModelDb), nameof(ModelDb.GetEntry), [typeof(Type)])];
        }

        // ReSharper disable once InconsistentNaming
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Type type, ref string __result)
        {
            if (!ModContentRegistry.TryGetFixedPublicEntry(type, out var fixedEntry))
                return;

            __result = fixedEntry;
        }
    }
}
