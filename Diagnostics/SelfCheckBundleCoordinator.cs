using Godot;
using STS2RitsuLib.Data;

namespace STS2RitsuLib.Diagnostics
{
    internal static class SelfCheckBundleCoordinator
    {
        private static int _autoRunIssuedForSession;

        internal static void TryAutoRunOnFirstMainMenu()
        {
            var (outputPath, runOnFirstMainMenu) = RitsuLibSettingsStore.GetSelfCheckOptions();
            if (!runOnFirstMainMenu)
                return;

            if (Interlocked.CompareExchange(ref _autoRunIssuedForSession, 1, 0) != 0)
                return;

            TryRunWithConfiguredPath(outputPath, "[SelfCheck][Auto]");
        }

        internal static void TryManualRunFromSettings()
        {
            var (outputPath, _) = RitsuLibSettingsStore.GetSelfCheckOptions();
            TryRunWithConfiguredPath(outputPath, "[SelfCheck][Manual]");
        }

        internal static void TryOpenOutputFolderFromSettings()
        {
            var (outputPath, _) = RitsuLibSettingsStore.GetSelfCheckOptions();
            var resolvedOutputDirectory = SelfCheckBundleWriter.TryResolveOutputDirectory(outputPath);
            if (string.IsNullOrEmpty(resolvedOutputDirectory))
            {
                RitsuLibFramework.Logger.Warn(
                    "[SelfCheck][OpenFolder] Output folder is empty or invalid. Configure a valid path in RitsuLib settings.");
                return;
            }

            try
            {
                Directory.CreateDirectory(resolvedOutputDirectory);
                var uri = new Uri(resolvedOutputDirectory + Path.DirectorySeparatorChar).AbsoluteUri;
                var shellOpenError = OS.ShellOpen(uri);
                if (shellOpenError != Error.Ok)
                {
                    RitsuLibFramework.Logger.Warn(
                        $"[SelfCheck][OpenFolder] Failed to open folder '{resolvedOutputDirectory}' (Error: {shellOpenError}).");
                    return;
                }

                RitsuLibFramework.Logger.Info(
                    $"[SelfCheck][OpenFolder] Opened output folder: {resolvedOutputDirectory}");
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Warn(
                    $"[SelfCheck][OpenFolder] Failed to open output folder '{resolvedOutputDirectory}': {ex.Message}");
            }
        }

        private static void TryRunWithConfiguredPath(string outputPath, string logPrefix)
        {
            var resolvedOutputDirectory = SelfCheckBundleWriter.TryResolveOutputDirectory(outputPath);
            if (string.IsNullOrEmpty(resolvedOutputDirectory))
            {
                RitsuLibFramework.Logger.Warn(
                    $"{logPrefix} Output folder is empty or invalid. Configure a valid path in RitsuLib settings.");
                return;
            }

            RitsuLibFramework.Logger.Info($"{logPrefix} Starting self-check bundle export...");

            if (!SelfCheckBundleWriter.TryWriteBundle(resolvedOutputDirectory, out var zipPath, out var error))
            {
                RitsuLibFramework.Logger.Warn($"{logPrefix} Export failed: {error}");
                return;
            }

            RitsuLibFramework.Logger.Info($"{logPrefix} Export complete. Zip: {zipPath}");
        }
    }
}
