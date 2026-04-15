using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Content;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Characters;
using Environment = System.Environment;

namespace STS2RitsuLib.Diagnostics
{
    internal static partial class SelfCheckBundleWriter
    {
        private static readonly Regex GodotLogName = GodotLogNameRegex();

        private static readonly Regex KeywordId = KeywordIdRegex();
        private static readonly Regex PublicEntry = PublicEntryRegex();

        internal static string? TryResolveOutputDirectory(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
                return null;

            var trimmed = rawPath.Trim();
            try
            {
                if (trimmed.StartsWith("user://", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
                    return ProjectSettings.GlobalizePath(trimmed);
                return Path.GetFullPath(trimmed);
            }
            catch
            {
                return null;
            }
        }

        internal static bool TryWriteBundle(string outputDirectory, out string? zipPath, out string? errorMessage)
        {
            zipPath = null;
            errorMessage = null;
            string? bundleDir = null;
            try
            {
                Directory.CreateDirectory(outputDirectory);
                var runId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                bundleDir = Path.Combine(outputDirectory, $"ritsulib_self_check_{runId}");
                Directory.CreateDirectory(bundleDir);

                var reportPath = Path.Combine(bundleDir, "self_check_report.log");
                var dumpPath = Path.Combine(bundleDir, "harmony_patch_dump.log");
                var logDir = Path.Combine(bundleDir, "logs");
                Directory.CreateDirectory(logDir);

                var dumpOk = HarmonyPatchDumpWriter.TryWrite(dumpPath, out var dumpErr);
                var runtime = RitsuLibFramework.CaptureRuntimeSnapshot();
                var copiedLogs = CopyGameLogs(logDir, out var logErrors);
                var charCheck = CheckCharacterAssets();
                var locCheck = CheckLocalization();
                var patchHint = CheckPatchHint();

                File.WriteAllText(reportPath,
                    BuildReport(runtime, dumpOk, dumpPath, dumpErr, copiedLogs, logErrors, charCheck, locCheck,
                        patchHint),
                    new UTF8Encoding(false));

                zipPath = Path.Combine(outputDirectory, $"{Path.GetFileName(bundleDir)}.zip");
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
                ZipFile.CreateFromDirectory(bundleDir, zipPath, CompressionLevel.Optimal, false);
                if (dumpOk) return true;
                errorMessage = $"Harmony dump failed: {dumpErr}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                if (!string.IsNullOrEmpty(bundleDir) && Directory.Exists(bundleDir))
                    try
                    {
                        Directory.Delete(bundleDir, true);
                    }
                    catch
                    {
                        // ignored
                    }
            }
        }

        private static string BuildReport(FrameworkRuntimeSnapshot runtime, bool dumpOk, string dumpPath,
            string? dumpErr,
            string[] copiedLogs, IReadOnlyList<string> logErrors, CheckResult charCheck, CheckResult locCheck,
            (int Targets, int Missing, int Foreign) patchHint)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== RitsuLib Self Check Report ===");
            sb.AppendLine($"Generated: {DateTime.Now:O}");
            sb.AppendLine($"Version: {Const.Version}");
            sb.AppendLine($"Framework Active: {runtime.IsActive}");
            sb.AppendLine($"Framework Initialized: {runtime.IsInitialized}");
            sb.AppendLine();
            sb.AppendLine($"Character Asset Runtime Check: FAIL={charCheck.Failures}, WARN={charCheck.Warnings}");
            foreach (var i in charCheck.Issues.Take(80)) sb.AppendLine($"- [{i.Level}] {i.Source}: {i.Reason}");
            if (charCheck.Issues.Count == 0) sb.AppendLine("- none");
            sb.AppendLine();
            sb.AppendLine($"Localization/Entry Runtime Check: FAIL={locCheck.Failures}, WARN={locCheck.Warnings}");
            foreach (var i in locCheck.Issues.Take(120)) sb.AppendLine($"- [{i.Level}] {i.Source}: {i.Reason}");
            if (locCheck.Issues.Count == 0) sb.AppendLine("- none");
            sb.AppendLine();
            sb.AppendLine(
                $"Patch Hint (auxiliary): targets={patchHint.Targets}, missing={patchHint.Missing}, foreign={patchHint.Foreign}");
            sb.AppendLine($"Harmony Dump: {(dumpOk ? "PASS" : "FAIL")} {dumpPath} {dumpErr}");
            sb.AppendLine($"Copied Logs: {copiedLogs.Length}");
            foreach (var log in copiedLogs.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                sb.AppendLine($"- logs/{log}");
            foreach (var err in logErrors) sb.AppendLine($"- [WARN] {err}");
            return sb.ToString();
        }

        private static CheckResult CheckCharacterAssets()
        {
            var issues = new List<Issue>();
            foreach (var c in ModContentRegistry.GetModCharacters().OrderBy(x => x.Id.Entry, StringComparer.Ordinal))
            {
                if (c is not IModCharacterAssetOverrides o) continue;
                Check(c.Id.Entry, "VisualsPath", o.CustomVisualsPath, c.VisualsPath, true);
                Check(c.Id.Entry, "EnergyCounterPath", o.CustomEnergyCounterPath, c.EnergyCounterPath, true);
                Check(c.Id.Entry, "IconTexturePath", o.CustomIconTexturePath, c.IconTexturePath, true);
                Check(c.Id.Entry, "IconOutlineTexturePath", o.CustomIconOutlineTexturePath, c.IconOutlineTexturePath,
                    true);
                Check(c.Id.Entry, "CharacterSelectBg", o.CustomCharacterSelectBgPath, c.CharacterSelectBg, true);
                Check(c.Id.Entry, "CharacterSelectTransitionPath", o.CustomCharacterSelectTransitionPath,
                    c.CharacterSelectTransitionPath, true);
                Check(c.Id.Entry, "TrailPath", o.CustomTrailPath, c.TrailPath, true);
                Check(c.Id.Entry, "AttackSfx", o.CustomAttackSfx, c.AttackSfx, false);
                Check(c.Id.Entry, "CastSfx", o.CustomCastSfx, c.CastSfx, false);
                Check(c.Id.Entry, "DeathSfx", o.CustomDeathSfx, c.DeathSfx, false);
            }

            return new(issues);

            void Check(string charId, string name, string? overrideValue, string resolved, bool requireResource)
            {
                if (string.IsNullOrWhiteSpace(overrideValue)) return;
                var selected = string.Equals(overrideValue, resolved, StringComparison.Ordinal);
                var exists = !requireResource || ResourceLoader.Exists(overrideValue);
                switch (selected)
                {
                    case true when exists:
                        return;
                    case true when !exists:
                        issues.Add(new("FAIL", $"{charId}.{name}", "override_selected_but_file_missing"));
                        return;
                    case false when !exists:
                        issues.Add(new("WARN", $"{charId}.{name}", "override_file_missing_fallback_to_vanilla"));
                        return;
                    default:
                        issues.Add(new("FAIL", $"{charId}.{name}",
                            "override_not_applied_possible_patch_skip_or_overwrite"));
                        break;
                }
            }
        }

        private static CheckResult CheckLocalization()
        {
            var issues = new List<Issue>();
            foreach (var d in ModKeywordRegistry.GetDefinitionsSnapshot())
            {
                if (!KeywordId.IsMatch(d.Id))
                    issues.Add(new("FAIL", $"keyword:{d.Id}", "keyword_id_invalid_format"));
                var expectedPrefix = ModContentRegistry.GetQualifiedKeywordId(d.ModId, "probe_suffix");
                expectedPrefix = expectedPrefix[..^"probe_suffix".Length];
                if (!d.Id.StartsWith(expectedPrefix, StringComparison.Ordinal))
                    issues.Add(new("WARN", $"keyword:{d.Id}",
                        $"keyword_id_not_owned_pattern_expected_prefix={expectedPrefix}"));
                if (!LocString.Exists(d.TitleTable, d.TitleKey))
                    issues.Add(new("FAIL", $"keyword:{d.Id}",
                        $"title_loc_missing table={d.TitleTable} key={d.TitleKey}"));
                if (!LocString.Exists(d.DescriptionTable, d.DescriptionKey))
                    issues.Add(new("FAIL", $"keyword:{d.Id}",
                        $"description_loc_missing table={d.DescriptionTable} key={d.DescriptionKey}"));
            }

            foreach (var s in ModContentRegistry.GetRegisteredTypeSnapshots())
            {
                var typeName = s.ModelType.FullName ?? s.ModelType.Name;
                if (s.ModelDbId == null)
                {
                    issues.Add(new("FAIL", $"model:{typeName}", "modeldb_id_missing"));
                    continue;
                }

                var actual = s.ModelDbId.Entry;
                if (!PublicEntry.IsMatch(actual))
                    issues.Add(new("FAIL", $"model:{typeName}", $"entry_invalid_format entry={actual}"));
                if (!string.IsNullOrWhiteSpace(s.ExpectedPublicEntry) &&
                    !actual.Equals(s.ExpectedPublicEntry, StringComparison.Ordinal))
                    issues.Add(new("FAIL", $"model:{typeName}",
                        $"entry_mismatch expected={s.ExpectedPublicEntry} actual={actual}"));
            }

            return new(issues);
        }

        private static (int Targets, int Missing, int Foreign) CheckPatchHint()
        {
            var targets = 0;
            var missing = 0;
            var foreign = 0;
            foreach (var s in RitsuLibFramework.CapturePatchBindingSnapshot())
            {
                if (s.OriginalMethod == null)
                {
                    missing++;
                    continue;
                }

                targets++;
                var p = Harmony.GetPatchInfo(s.OriginalMethod);
                if (p == null)
                {
                    missing++;
                    continue;
                }

                var owners = p.Prefixes.Concat(p.Postfixes).Concat(p.Transpilers).Concat(p.Finalizers)
                    .Select(x => x.owner ?? "<null-owner>").Distinct(StringComparer.Ordinal).ToArray();
                if (owners.All(x => !x.Equals(s.PatcherId, StringComparison.Ordinal))) missing++;
                if (owners.Any(x => !x.Equals(s.PatcherId, StringComparison.Ordinal))) foreign++;
            }

            return (targets, missing, foreign);
        }

        private static string[] CopyGameLogs(string targetDirectory, out List<string> copyErrors)
        {
            copyErrors = [];
            var sourceDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SlayTheSpire2", "logs");
            if (!Directory.Exists(sourceDir))
            {
                copyErrors.Add($"logs source directory not found: {sourceDir}");
                return [];
            }

            var candidates = Directory.GetFiles(sourceDir, "*.log", SearchOption.TopDirectoryOnly)
                .Where(p => GodotLogName.IsMatch(Path.GetFileName(p)))
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var copied = new List<string>();
            foreach (var src in candidates)
                try
                {
                    var name = Path.GetFileName(src);
                    File.Copy(src, Path.Combine(targetDirectory, name), true);
                    copied.Add(name);
                }
                catch (Exception ex)
                {
                    copyErrors.Add($"failed to copy {src}: {ex.Message}");
                }

            return [.. copied];
        }

        [GeneratedRegex(@"^godot(\d{4}-\d{2}-\d{2}T\d{2}\.\d{2}\.\d{2})?\.log$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex GodotLogNameRegex();

        [GeneratedRegex("^[a-z0-9_]+$", RegexOptions.Compiled)]
        private static partial Regex KeywordIdRegex();

        [GeneratedRegex("^[A-Z0-9_]+$", RegexOptions.Compiled)]
        private static partial Regex PublicEntryRegex();

        private readonly record struct Issue(string Level, string Source, string Reason);

        private readonly record struct CheckResult
        {
            internal CheckResult(IReadOnlyList<Issue> issues)
            {
                Issues = issues;
                Failures = issues.Count(i => i.Level == "FAIL");
                Warnings = issues.Count(i => i.Level == "WARN");
            }

            internal int Failures { get; }
            internal int Warnings { get; }
            internal IReadOnlyList<Issue> Issues { get; } = [];
        }
    }
}
