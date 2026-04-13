using System.Collections;
using System.Globalization;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace STS2RitsuLib.Settings
{
    /// <summary>
    ///     Optional behavior for ModConfig mirror registration; pass to the four-parameter
    ///     <c>TryRegisterMirroredPages</c> overload on <see cref="ModSettingsModConfigReflectionMirror" />.
    /// </summary>
    public sealed class ModConfigMirrorRegistrationOptions
    {
        /// <summary>
        ///     Default options used when none are passed.
        /// </summary>
        public static ModConfigMirrorRegistrationOptions Default { get; } = new();

        /// <summary>
        ///     Forwarded to <see cref="ModSettingsSectionBuilder.AddKeyBinding" /> for mirrored ModConfig KeyBind rows.
        /// </summary>
        public bool KeyBindAllowModifierCombos { get; init; } = true;

        /// <summary>
        ///     Forwarded to <see cref="ModSettingsSectionBuilder.AddKeyBinding" /> for mirrored ModConfig KeyBind rows.
        /// </summary>
        public bool KeyBindAllowModifierOnly { get; init; } = true;

        /// <summary>
        ///     Forwarded to <see cref="ModSettingsSectionBuilder.AddKeyBinding" /> for mirrored ModConfig KeyBind rows.
        /// </summary>
        public bool KeyBindDistinguishModifierSides { get; init; } = false;
    }

    /// <summary>
    ///     Mirrors the optional <c>ModConfig-STS2</c> (<c>ModConfig.ModConfigApi</c>) registrations into RitsuLib&apos;s
    ///     mod settings UI via reflection (no compile-time reference). <c>TryRegisterMirroredPages</c> is
    ///     invoked from <see cref="RitsuModSettingsSubmenu" /> on rebuilds so mods that register after the first paint
    ///     are picked up.
    /// </summary>
    public static class ModSettingsModConfigReflectionMirror
    {
        private const string ApiTypeName = "ModConfig.ModConfigApi";
        private const string ManagerTypeName = "ModConfig.ModConfigManager";
        private const string I18nTypeName = "ModConfig.I18n";

        private static readonly Lock Gate = new();

        private static Type? _cachedApiType;
        private static MethodInfo? _cachedGetValueOpen;
        private static MethodInfo? _cachedSetValue;
        private static MethodInfo? _cachedResetDefaults;
        private static MethodInfo? _cachedI18nGet;
        private static PropertyInfo? _cachedRegistrationsProp;

        /// <summary>
        ///     True when a loaded assembly exposes <c>ModConfig.ModConfigApi</c>.
        /// </summary>
        public static bool IsModConfigPresent => ResolveType(ApiTypeName) != null;

        /// <summary>
        ///     Registers one RitsuLib settings page per ModConfig-registered mod that does not yet have page
        ///     <paramref name="pageId" />. Returns how many new pages were added this call.
        /// </summary>
        /// <param name="pageId">Stable page id (default <c>modconfig</c>).</param>
        /// <param name="sortOrder">Sidebar ordering among sibling pages.</param>
        /// <param name="pageTitle">Optional page title.</param>
        public static int TryRegisterMirroredPages(
            string pageId = "modconfig",
            int sortOrder = 10_020,
            ModSettingsText? pageTitle = null)
        {
            return TryRegisterMirroredPages(pageId, sortOrder, pageTitle, null);
        }

        /// <summary>
        ///     Registers mirrored pages with optional KeyBind / future mirror behavior.
        /// </summary>
        /// <param name="pageId">Stable page id under each mod.</param>
        /// <param name="sortOrder">Sidebar ordering among sibling pages.</param>
        /// <param name="pageTitle">Optional page title.</param>
        /// <param name="options">When null, <see cref="ModConfigMirrorRegistrationOptions.Default" /> is used.</param>
        public static int TryRegisterMirroredPages(
            string pageId,
            int sortOrder,
            ModSettingsText? pageTitle,
            ModConfigMirrorRegistrationOptions? options)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pageId);

            var o = options ?? ModConfigMirrorRegistrationOptions.Default;

            lock (Gate)
            {
                if (!TryResolveInterop(out _, out var getValueOpen, out var setValue, out var resetDefaults,
                        out var registrationsProp, out var i18nGet))
                    return 0;

                var registrations = registrationsProp.GetValue(null);
                if (registrations is not IEnumerable enumerable)
                    return 0;

                pageTitle ??= ModSettingsText.I18N(ModSettingsLocalization.Instance, "modconfig.mirroredPage.title",
                    "Mod config (ModConfig)");
                var pageDescription = ModSettingsText.I18N(ModSettingsLocalization.Instance,
                    "modconfig.mirroredPage.description",
                    "Proxy page for a mod registered with the ModConfig library. Values are stored by ModConfig.");

                var added = 0;
                foreach (var item in enumerable)
                {
                    if (!TryUnwrapDictionaryItem(item, out var modId, out var reg))
                        continue;

                    if (ModSettingsRegistry.TryGetPage(modId, pageId, out _))
                        continue;

                    if (!TryRegisterOneMod(modId, pageId, sortOrder, pageTitle, pageDescription, reg,
                            getValueOpen, setValue, resetDefaults, i18nGet, o))
                        continue;

                    added++;
                }

                return added;
            }
        }

        /// <summary>
        ///     Clears cached ModConfig reflection handles (e.g. after late-loaded assemblies in non-standard hosts).
        /// </summary>
        public static void ClearModConfigReflectionCache()
        {
            lock (Gate)
            {
                _cachedApiType = null;
                _cachedGetValueOpen = null;
                _cachedSetValue = null;
                _cachedResetDefaults = null;
                _cachedRegistrationsProp = null;
                _cachedI18nGet = null;
            }
        }

        private static bool TryResolveInterop(
            out Type apiType,
            out MethodInfo getValueOpen,
            out MethodInfo setValue,
            out MethodInfo resetDefaults,
            out PropertyInfo registrationsProp,
            out MethodInfo? i18nGet)
        {
            if (_cachedApiType != null && _cachedGetValueOpen != null && _cachedSetValue != null &&
                _cachedResetDefaults != null && _cachedRegistrationsProp != null)
            {
                apiType = _cachedApiType;
                getValueOpen = _cachedGetValueOpen;
                setValue = _cachedSetValue;
                resetDefaults = _cachedResetDefaults;
                registrationsProp = _cachedRegistrationsProp;
                i18nGet = _cachedI18nGet;
                return true;
            }

            var resolvedApi = ResolveType(ApiTypeName);
            if (resolvedApi == null)
            {
#pragma warning disable CS8601
                apiType = null!;
                getValueOpen = null!;
                setValue = null!;
                resetDefaults = null!;
                registrationsProp = null!;
#pragma warning restore CS8601
                i18nGet = null;
                return false;
            }

            apiType = resolvedApi;
            var asm = apiType.Assembly;

            var resolvedGetValue = apiType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "GetValue" && m.IsGenericMethodDefinition &&
                                     m.GetParameters() is [{ ParameterType: var p0 }, { ParameterType: var p1 }] &&
                                     p0 == typeof(string) && p1 == typeof(string));
            var resolvedSetValue = apiType.GetMethod("SetValue",
                BindingFlags.Public | BindingFlags.Static, null, [typeof(string), typeof(string), typeof(object)],
                null);

            var managerType = asm.GetType(ManagerTypeName);
            var resolvedReset = managerType?.GetMethod("ResetToDefaults",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, [typeof(string)], null);
            var resolvedRegs =
                managerType?.GetProperty("Registrations",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (resolvedGetValue == null || resolvedSetValue == null || resolvedReset == null || resolvedRegs == null)
            {
#pragma warning disable CS8601
                getValueOpen = null!;
                setValue = null!;
                resetDefaults = null!;
                registrationsProp = null!;
#pragma warning restore CS8601
                i18nGet = null;
                return false;
            }

            getValueOpen = resolvedGetValue;
            setValue = resolvedSetValue;
            resetDefaults = resolvedReset;
            registrationsProp = resolvedRegs;

            var i18nType = asm.GetType(I18nTypeName);
            i18nGet = i18nType?.GetMethod("Get",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null,
                [typeof(string), typeof(string)], null);

            _cachedApiType = apiType;
            _cachedGetValueOpen = getValueOpen;
            _cachedSetValue = setValue;
            _cachedResetDefaults = resetDefaults;
            _cachedRegistrationsProp = registrationsProp;
            _cachedI18nGet = i18nGet;

            return true;
        }

        private static bool TryUnwrapDictionaryItem(object item, out string modId, out object reg)
        {
            modId = "";
            reg = null!;

            switch (item)
            {
                case DictionaryEntry de:
                {
                    if (de.Key is not string ks || string.IsNullOrWhiteSpace(ks) || de.Value == null)
                        return false;

                    modId = ks;
                    reg = de.Value;
                    break;
                }
                default:
                {
                    var t = item.GetType();
                    if (!t.IsGenericType || !t.Name.StartsWith("KeyValuePair`", StringComparison.Ordinal))
                        return false;

                    var k = t.GetProperty("Key")?.GetValue(item) as string;
                    var v = t.GetProperty("Value")?.GetValue(item);
                    if (string.IsNullOrWhiteSpace(k) || v == null)
                        return false;

                    modId = k;
                    reg = v;
                    break;
                }
            }

            return true;
        }

        private static bool TryRegisterOneMod(
            string modId,
            string pageId,
            int sortOrder,
            ModSettingsText pageTitle,
            ModSettingsText pageDescription,
            object registration,
            MethodInfo getValueOpen,
            MethodInfo setValue,
            MethodInfo resetDefaults,
            MethodInfo? i18nGet,
            ModConfigMirrorRegistrationOptions mirrorOptions)
        {
            var regType = registration.GetType();
            var entriesObj = regType.GetProperty("Entries")?.GetValue(registration);
            if (entriesObj is not Array entries || entries.Length == 0)
                return false;

            var hasRenderable = false;
            for (var i = 0; i < entries.Length; i++)
            {
                if (entries.GetValue(i) is not { } entry)
                    continue;
                if (IsRenderableConfigEntry(entry))
                {
                    hasRenderable = true;
                    break;
                }
            }

            if (!hasRenderable)
                return false;

            try
            {
                ModSettingsRegistry.Register(modId, builder =>
                {
                    builder
                        .WithTitle(pageTitle)
                        .WithDescription(pageDescription)
                        .WithSortOrder(sortOrder)
                        .WithModDisplayName(ModSettingsText.Dynamic(() =>
                            ResolveRegistrationDisplayName(registration, i18nGet)));

                    builder.AddSection("modconfig_main", section =>
                    {
                        for (var i = 0; i < entries.Length; i++)
                        {
                            if (entries.GetValue(i) is not { } entry)
                                continue;

                            AppendConfigEntry(section, modId, entry, i, getValueOpen, setValue, i18nGet,
                                mirrorOptions);
                        }

                        section.AddButton(
                            "modconfig_restore_defaults",
                            ModSettingsText.Literal(ModSettingsLocalization.Get("modconfig.restoreDefaults.row",
                                "Defaults")),
                            ModSettingsText.Literal(ModSettingsLocalization.Get("modconfig.restoreDefaults.button",
                                "Restore defaults")),
                            () => ConfirmAndResetModConfig(modId, resetDefaults),
                            ModSettingsButtonTone.Danger);
                    });
                }, pageId);

                return true;
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Warn(
                    $"[ModSettingsModConfigReflectionMirror] Failed to register page '{modId}::{pageId}': {ex.Message}");
                return false;
            }
        }

        private static bool IsRenderableConfigEntry(object entry)
        {
            var typeName = ResolveModConfigEntryTypeName(entry);
            return typeName is not ("" or "Header" or "Separator");
        }

        private static string ResolveModConfigEntryTypeName(object entry)
        {
            var t = entry.GetType();
            var typeObj = t.GetProperty("Type")?.GetValue(entry);
            if (typeObj == null)
                return "";

            if (typeObj is Enum e)
            {
                var et = e.GetType();
                if (Enum.IsDefined(et, e))
                    return e.ToString();
            }

            var name = typeObj.ToString();
            if (!string.IsNullOrEmpty(name) && int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out var asInt))
                return MapModConfigTypeCodeToName(asInt);

            return name ?? "";
        }

        private static string MapModConfigTypeCodeToName(int code)
        {
            return code switch
            {
                0 => "Toggle",
                1 => "Slider",
                2 => "Dropdown",
                3 => "KeyBind",
                4 => "TextInput",
                5 => "Header",
                6 => "Separator",
                7 => "Button",
                8 => "ColorPicker",
                _ => "",
            };
        }

        private static void AppendConfigEntry(
            ModSettingsSectionBuilder section,
            string modId,
            object entry,
            int index,
            MethodInfo getValueOpen,
            MethodInfo setValue,
            MethodInfo? i18nGet,
            ModConfigMirrorRegistrationOptions mirrorOptions)
        {
            var t = entry.GetType();
            var typeName = ResolveModConfigEntryTypeName(entry);

            switch (typeName)
            {
                case "Header":
                {
                    var id = $"mc_hdr_{index}";
                    var label = ModSettingsText.Dynamic(() => ResolveEntryLabel(entry, i18nGet));
                    section.AddHeader(id, label);
                    return;
                }
                case "Separator":
                {
                    section.AddHeader($"mc_sep_{index}",
                        ModSettingsText.Literal(ModSettingsLocalization.Get("modconfig.separator", "—")));
                    return;
                }
            }

            var key = t.GetProperty("Key")?.GetValue(entry) as string;
            if (string.IsNullOrWhiteSpace(key))
                key = $"entry_{index}";

            var slug = StringHelper.Slugify(key);
            var idBase = $"mc_{slug}";
            var labelText = ModSettingsText.Dynamic(() => ResolveEntryLabel(entry, i18nGet));
            var descText = ResolveEntryDescription(entry, i18nGet);
            var dataKey = $"modconfig::{key}";

            switch (typeName)
            {
                case "Toggle":
                {
                    var binding = ModSettingsBindings.Callback(modId, dataKey,
                        () => CoerceModConfigToggle(getValueOpen, modId, key),
                        v => InvokeSetValue(setValue, modId, key, v),
                        () => { });
                    section.AddToggle(idBase, labelText, binding, descText);
                    return;
                }
                case "Slider":
                {
                    var min = Convert.ToDouble(t.GetProperty("Min")?.GetValue(entry) ?? 0d);
                    var max = Convert.ToDouble(t.GetProperty("Max")?.GetValue(entry) ?? 100d);
                    var step = Convert.ToDouble(t.GetProperty("Step")?.GetValue(entry) ?? 1d);
                    if (max < min)
                        (min, max) = (max, min);
                    if (step <= 0d)
                        step = 1d;

                    var format = t.GetProperty("Format")?.GetValue(entry) as string ?? "F0";
                    Func<double, string> fmt = v =>
                    {
                        try
                        {
                            return ((float)v).ToString(format, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            return v.ToString("F2", CultureInfo.InvariantCulture);
                        }
                    };

                    var binding = ModSettingsBindings.Callback(modId, dataKey,
                        () => CoerceModConfigSliderDouble(getValueOpen, modId, key),
                        v => InvokeSetValue(setValue, modId, key, (float)v),
                        () => { });
                    section.AddSlider(idBase, labelText, binding, min, max, step, fmt, descText);
                    return;
                }
                case "Dropdown":
                {
                    var optValues = t.GetProperty("Options")?.GetValue(entry) as string[] ?? [];
                    if (optValues.Length == 0)
                        return;

                    var rawCurrent = InvokeGetValue<string>(getValueOpen, modId, key);
                    var orphan = !string.IsNullOrEmpty(rawCurrent) &&
                                 Array.IndexOf(optValues, rawCurrent) < 0;

                    ModSettingsChoiceOption<string>[] choiceOptions;
                    if (orphan && rawCurrent != null)
                    {
                        choiceOptions = new ModSettingsChoiceOption<string>[optValues.Length + 1];
                        choiceOptions[0] = new(rawCurrent,
                            ModSettingsText.Literal(rawCurrent + ModSettingsLocalization.Get(
                                "modconfig.dropdown.orphanSuffix", " (saved)")));
                        for (var o = 0; o < optValues.Length; o++)
                        {
                            var captured = o;
                            var optVal = optValues[captured];
                            choiceOptions[captured + 1] = new(optVal, ModSettingsText.Dynamic(() =>
                                ResolveOptionLabel(entry, captured, optVal, i18nGet)));
                        }
                    }
                    else
                    {
                        choiceOptions = new ModSettingsChoiceOption<string>[optValues.Length];
                        for (var o = 0; o < optValues.Length; o++)
                        {
                            var captured = o;
                            var optVal = optValues[captured];
                            choiceOptions[captured] = new(optVal, ModSettingsText.Dynamic(() =>
                                ResolveOptionLabel(entry, captured, optVal, i18nGet)));
                        }
                    }

                    var binding = ModSettingsBindings.Callback(modId, dataKey,
                        () =>
                        {
                            var cur = InvokeGetValue<string>(getValueOpen, modId, key) ?? "";
                            if (string.IsNullOrEmpty(cur))
                                return optValues[0];
                            return cur;
                        },
                        v => InvokeSetValue(setValue, modId, key, v ?? optValues[0]),
                        () => { });

                    var presentation = choiceOptions.Length > 5
                        ? ModSettingsChoicePresentation.Dropdown
                        : ModSettingsChoicePresentation.Stepper;

                    section.AddChoice(idBase, labelText, binding, choiceOptions, descText, presentation);
                    return;
                }
                case "KeyBind":
                {
                    var binding = ModSettingsBindings.Callback(modId, dataKey,
                        () => KeyCodeToBindingString(CoerceModConfigKeyCode(getValueOpen, modId, key)),
                        v =>
                        {
                            var code = ParseBindingStringToKeyCode(v);
                            InvokeSetValue(setValue, modId, key, code);
                        },
                        () => { });
                    section.AddKeyBinding(idBase, labelText, binding,
                        mirrorOptions.KeyBindAllowModifierCombos,
                        mirrorOptions.KeyBindAllowModifierOnly,
                        mirrorOptions.KeyBindDistinguishModifierSides,
                        descText);
                    return;
                }
                case "TextInput":
                {
                    var maxLen = t.GetProperty("MaxLength")?.GetValue(entry) as int? ?? 64;
                    var placeholderRaw = t.GetProperty("Placeholder")?.GetValue(entry) as string;
                    var placeholder = string.IsNullOrEmpty(placeholderRaw)
                        ? null
                        : ModSettingsText.Literal(placeholderRaw);

                    var validatorDel = t.GetProperty("Validator")?.GetValue(entry) as Delegate;
                    Func<string, bool>? validationVisual = null;
                    if (validatorDel != null)
                        validationVisual = text =>
                        {
                            try
                            {
                                var r = validatorDel.DynamicInvoke(text);
                                return r is bool ok && ok;
                            }
                            catch
                            {
                                return false;
                            }
                        };

                    var binding = ModSettingsBindings.Callback(modId, dataKey,
                        () => InvokeGetValue<string>(getValueOpen, modId, key) ?? "",
                        v => InvokeSetValue(setValue, modId, key, v ?? ""),
                        () => { });
                    section.AddString(idBase, labelText, binding, placeholder,
                        maxLen > 0 ? maxLen : null, descText, validationVisual);
                    return;
                }
                case "Button":
                {
                    var buttonText = ModSettingsText.Dynamic(() => ResolveButtonText(entry, i18nGet));
                    section.AddButton(idBase, labelText, buttonText,
                        () => InvokeConfigButtonCallback(entry),
                        ModSettingsButtonTone.Normal,
                        descText);
                    return;
                }
                case "ColorPicker":
                {
                    var editAlpha = t.GetProperty("EditAlpha")?.GetValue(entry) as bool? ?? true;
                    var editIntensity = t.GetProperty("EditIntensity")?.GetValue(entry) as bool? ?? false;
                    var binding = ModSettingsBindings.Callback(modId, dataKey,
                        () => InvokeGetValue<string>(getValueOpen, modId, key) ?? "#FFFFFF",
                        v => InvokeSetValue(setValue, modId, key, v ?? "#FFFFFF"),
                        () => { });
                    section.AddColor(idBase, labelText, binding, descText, editAlpha, editIntensity);
                    return;
                }
            }
        }

        private static string ResolveRegistrationDisplayName(object registration, MethodInfo? i18nGet)
        {
            var regType = registration.GetType();
            var fallback = regType.GetProperty("DisplayName")?.GetValue(registration) as string ?? "";
            var map = regType.GetProperty("DisplayNames")?.GetValue(registration) as IDictionary;
            if (map != null && map.Count > 0)
            {
                var lang = TryModConfigCurrentLang();
                if (!string.IsNullOrEmpty(lang) && map.Contains(lang) && map[lang] is string exact)
                    return exact;

                foreach (DictionaryEntry e in map)
                {
                    if (e.Key is not string k || e.Value is not string v)
                        continue;
                    if (lang != null && (lang.StartsWith(k, StringComparison.OrdinalIgnoreCase) ||
                                         k.StartsWith(lang, StringComparison.OrdinalIgnoreCase)))
                        return v;
                }

                if (map.Contains("en") && map["en"] is string en)
                    return en;
            }

            return string.IsNullOrEmpty(fallback) ? "Mod" : fallback;
        }

        private static string? TryModConfigCurrentLang()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? i18n;
                try
                {
                    i18n = asm.GetType(I18nTypeName, false);
                }
                catch
                {
                    continue;
                }

                if (i18n == null)
                    continue;

                var p = i18n.GetProperty("CurrentLang",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (p?.GetValue(null) is string lang)
                    return lang;
            }

            return null;
        }

        private static string ResolveEntryLabel(object entry, MethodInfo? i18nGet)
        {
            var t = entry.GetType();
            var fallback = t.GetProperty("Label")?.GetValue(entry) as string ?? "";
            var labelKey = t.GetProperty("LabelKey")?.GetValue(entry) as string;
            var fromI18n = TryI18n(labelKey, fallback, i18nGet);
            if (fromI18n != null)
                return fromI18n;

            return ResolveLangMap(t.GetProperty("Labels")?.GetValue(entry) as IDictionary, fallback);
        }

        private static ModSettingsText? ResolveEntryDescription(object entry, MethodInfo? i18nGet)
        {
            var t = entry.GetType();
            var body = t.GetProperty("Description")?.GetValue(entry) as string;
            if (string.IsNullOrWhiteSpace(body))
            {
                var dk = t.GetProperty("DescriptionKey")?.GetValue(entry) as string;
                body = TryI18n(dk, "", i18nGet);
            }

            if (string.IsNullOrWhiteSpace(body))
                body = ResolveLangMap(t.GetProperty("Descriptions")?.GetValue(entry) as IDictionary, "");

            return string.IsNullOrWhiteSpace(body) ? null : ModSettingsText.Literal(body);
        }

        private static string ResolveButtonText(object entry, MethodInfo? i18nGet)
        {
            var t = entry.GetType();
            var fallback = t.GetProperty("ButtonText")?.GetValue(entry) as string ?? "";
            return ResolveLangMap(t.GetProperty("ButtonTexts")?.GetValue(entry) as IDictionary, fallback);
        }

        private static string ResolveOptionLabel(object entry, int index, string fallback, MethodInfo? i18nGet)
        {
            var t = entry.GetType();
            var keys = t.GetProperty("OptionsKeys")?.GetValue(entry) as string[];
            if (keys != null && index >= 0 && index < keys.Length)
            {
                var k = keys[index];
                var fromI18n = TryI18n(k, fallback, i18nGet);
                if (fromI18n != null)
                    return fromI18n;
            }

            return fallback;
        }

        private static string? TryI18n(string? key, string fallback, MethodInfo? i18nGet)
        {
            if (i18nGet == null || string.IsNullOrWhiteSpace(key))
                return null;

            try
            {
                return i18nGet.Invoke(null, [key, fallback]) as string;
            }
            catch
            {
                return null;
            }
        }

        private static string ResolveLangMap(IDictionary? map, string fallback)
        {
            if (map == null || map.Count == 0)
                return fallback;

            var lang = TryModConfigCurrentLang();
            if (!string.IsNullOrEmpty(lang) && map.Contains(lang) && map[lang] is string exact)
                return exact;

            foreach (DictionaryEntry e in map)
            {
                if (e.Key is not string k || e.Value is not string v)
                    continue;
                if (lang != null && (lang.StartsWith(k, StringComparison.OrdinalIgnoreCase) ||
                                     k.StartsWith(lang, StringComparison.OrdinalIgnoreCase)))
                    return v;
            }

            if (map.Contains("en") && map["en"] is string en)
                return en;

            foreach (DictionaryEntry e in map)
                if (e.Value is string v)
                    return v;

            return fallback;
        }

        private static T InvokeGetValue<T>(MethodInfo getValueOpen, string modId, string key)
        {
            try
            {
                var m = getValueOpen.MakeGenericMethod(typeof(T));
                var r = m.Invoke(null, [modId, key]);
                if (r is T ok)
                    return ok;
                if (r == null)
                    return default!;

                return (T)Convert.ChangeType(r, typeof(T))!;
            }
            catch
            {
                return default!;
            }
        }

        private static void InvokeSetValue(MethodInfo setValue, string modId, string key, object value)
        {
            try
            {
                setValue.Invoke(null, [modId, key, value]);
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Warn(
                    $"[ModSettingsModConfigReflectionMirror] SetValue failed [{modId}.{key}]: {ex.Message}");
            }
        }

        private static void InvokeConfigButtonCallback(object entry)
        {
            var onChanged = entry.GetType().GetProperty("OnChanged")?.GetValue(entry) as Delegate;
            if (onChanged == null)
                return;

            try
            {
                var inv = onChanged.Method;
                var ps = inv.GetParameters();
                switch (ps.Length)
                {
                    case 0:
                        _ = onChanged.DynamicInvoke();
                        return;
                    case 1:
                    {
                        var p0 = ps[0].ParameterType;
                        object? arg;
                        if (p0 == typeof(bool))
                            arg = false;
                        else if (p0 == typeof(object))
                            arg = false;
                        else if (!p0.IsValueType)
                            arg = null;
                        else
                            arg = Activator.CreateInstance(p0);

                        _ = onChanged.DynamicInvoke(arg);
                        return;
                    }
                    default:
                        var args = new object?[ps.Length];
                        for (var i = 0; i < ps.Length; i++)
                        {
                            var pi = ps[i].ParameterType;
                            if (pi == typeof(bool))
                                args[i] = false;
                            else if (!pi.IsValueType)
                                args[i] = null;
                            else
                                args[i] = Activator.CreateInstance(pi);
                        }

                        _ = onChanged.DynamicInvoke(args);
                        return;
                }
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Warn(
                    $"[ModSettingsModConfigReflectionMirror] Config button callback failed: {ex.Message}");
            }
        }

        private static bool CoerceModConfigToggle(MethodInfo getValueOpen, string modId, string key)
        {
            try
            {
                var m = getValueOpen.MakeGenericMethod(typeof(object));
                var r = m.Invoke(null, [modId, key]);
                if (r == null)
                    return false;
                if (r is bool b)
                    return b;
                if (r is string s)
                    return bool.TryParse(s, out var bs) && bs;

                return Convert.ToBoolean(r, CultureInfo.InvariantCulture);
            }
            catch
            {
                return false;
            }
        }

        private static double CoerceModConfigSliderDouble(MethodInfo getValueOpen, string modId, string key)
        {
            try
            {
                var m = getValueOpen.MakeGenericMethod(typeof(object));
                var r = m.Invoke(null, [modId, key]);
                if (r == null)
                    return 0d;
                if (r is double d)
                    return d;
                if (r is float f)
                    return f;
                if (r is int i)
                    return i;
                if (r is long l)
                    return l;

                return Convert.ToDouble(r, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0d;
            }
        }

        private static long CoerceModConfigKeyCode(MethodInfo getValueOpen, string modId, string key)
        {
            try
            {
                var m = getValueOpen.MakeGenericMethod(typeof(object));
                var r = m.Invoke(null, [modId, key]);
                if (r == null)
                    return 0;
                if (r is long L)
                    return L;
                if (r is int i)
                    return i;
                if (r is double d)
                    return (long)Math.Round(d);

                return Convert.ToInt64(r, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        private static string KeyCodeToBindingString(long code)
        {
            if (code == 0)
                return string.Empty;

            try
            {
                var key = (Key)code;
                var s = OS.GetKeycodeString(key);
                return string.IsNullOrEmpty(s) ? key.ToString() : s;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static long ParseBindingStringToKeyCode(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return 0;

            var tail = s.Contains('+', StringComparison.Ordinal)
                ? s.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last()
                : s.Trim();

            if (Enum.TryParse<Key>(tail, true, out var direct))
                return (long)direct;

            foreach (var candidate in Enum.GetValues<Key>())
            {
                if (string.Equals(candidate.ToString(), tail, StringComparison.OrdinalIgnoreCase))
                    return (long)candidate;

                try
                {
                    var label = OS.GetKeycodeString(candidate);
                    if (!string.IsNullOrEmpty(label) &&
                        string.Equals(label, tail, StringComparison.OrdinalIgnoreCase))
                        return (long)candidate;
                }
                catch
                {
                    // ignored
                }
            }

            return 0;
        }

        private static void ConfirmAndResetModConfig(string modId, MethodInfo resetDefaults)
        {
            if (Engine.GetMainLoop() is not SceneTree { Root: { } root })
            {
                TryResetDefaults(resetDefaults, modId);
                return;
            }

            var body = ModSettingsLocalization.Get("modconfig.restoreDefaults.body",
                "Reset all ModConfig options for this mod to their default values?");
            var header = ModSettingsLocalization.Get("modconfig.restoreDefaults.header", "Restore defaults");
            var submenu = FindRitsuModSettingsSubmenu(root);
            var attachParent = (Node?)submenu ?? root;
            var cancelText = ModSettingsLocalization.Get("modconfig.restoreDefaults.cancel", "Cancel");
            var confirmText = ModSettingsLocalization.Get("modconfig.restoreDefaults.confirm", "Restore defaults");

            ModSettingsUiFactory.ShowStyledConfirm(
                attachParent,
                header,
                body,
                cancelText,
                confirmText,
                true,
                () =>
                {
                    TryResetDefaults(resetDefaults, modId);
                    submenu?.RequestRefresh();
                });
        }

        private static void TryResetDefaults(MethodInfo resetDefaults, string modId)
        {
            try
            {
                _ = resetDefaults.Invoke(null, [modId]);
            }
            catch (Exception ex)
            {
                RitsuLibFramework.Logger.Warn(
                    $"[ModSettingsModConfigReflectionMirror] ResetToDefaults failed for '{modId}': {ex.Message}");
            }
        }

        private static RitsuModSettingsSubmenu? FindRitsuModSettingsSubmenu(Node root)
        {
            var queue = new Queue<Node>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var n = queue.Dequeue();
                if (n is RitsuModSettingsSubmenu sm)
                    return sm;

                foreach (var child in n.GetChildren())
                    queue.Enqueue(child);
            }

            return null;
        }

        private static Type? ResolveType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? t = null;
                try
                {
                    t = asm.GetType(fullName, false);
                }
                catch
                {
                    // ignored
                }

                if (t != null)
                    return t;
            }

            return null;
        }
    }
}
