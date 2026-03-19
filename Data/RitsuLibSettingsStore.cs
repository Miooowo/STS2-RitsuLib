using STS2RitsuLib.Data.Models;
using STS2RitsuLib.Utils.Persistence;

namespace STS2RitsuLib.Data
{
    internal static class RitsuLibSettingsStore
    {
        private static readonly ModDataStore Store = ModDataStore.For(Const.ModId);

        private static readonly Lock InitLock = new();
        private static bool _initialized;

        internal static void Initialize()
        {
            lock (InitLock)
            {
                if (_initialized)
                    return;

                using (RitsuLibFramework.BeginModDataRegistration(Const.ModId, false))
                {
                    Store.Register<RitsuLibSettings>(
                        Const.SettingsKey,
                        Const.SettingsFileName,
                        SaveScope.Global,
                        () => new(),
                        true
                    );
                }

                NormalizeSchemaVersionIfNeeded();

                _initialized = true;
                var enabled = GetSettings().DebugCompatibilityMode;
                RitsuLibFramework.Logger.Info(
                    $"[Config] Debug compatibility mode is {(enabled ? "enabled" : "disabled")}. " +
                    $"Config file: {ProfileManager.GetFilePath(Const.SettingsFileName, SaveScope.Global, 0, Const.ModId)}");
            }
        }

        internal static bool IsDebugCompatibilityModeEnabled()
        {
            Initialize();
            return GetSettings().DebugCompatibilityMode;
        }

        private static RitsuLibSettings GetSettings()
        {
            return Store.Get<RitsuLibSettings>(Const.SettingsKey);
        }

        private static void NormalizeSchemaVersionIfNeeded()
        {
            var settings = GetSettings();
            if (settings.SchemaVersion >= RitsuLibSettings.CurrentSchemaVersion)
                return;

            Store.Modify<RitsuLibSettings>(Const.SettingsKey,
                model => model.SchemaVersion = RitsuLibSettings.CurrentSchemaVersion);
            Store.Save(Const.SettingsKey);
        }
    }
}
