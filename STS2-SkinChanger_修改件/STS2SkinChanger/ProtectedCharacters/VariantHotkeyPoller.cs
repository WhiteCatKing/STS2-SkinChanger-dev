// 受保护兼容模块：十字军/贞德及同包变体。修改前阅读本目录 AGENTS.md 和 README.md。
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using System.Reflection;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

/// <summary>
/// Drives the skin pack's <b>own</b> appearance hotkey (Joan: “切换外观差分快捷键”, default <c>=</c>,
/// freely rebound in the pack's RitsuLib page — the author uses <c>Key1</c>).
/// <para>
/// Why the Skin Changer has to do this at all: the binding belongs to the pack, and the pack reads it
/// itself, but a pack that only listens through a Godot <c>_input</c> node loses the key whenever the
/// game's UI consumes the event first — exactly the 0.3.5-S build, while 0.3.6 added a
/// <c>get_window().window_input</c> bridge. And when both packs are installed .NET keeps only one
/// assembly, so the other pack's handler does not exist at all.
/// </para>
/// <para>
/// The poller therefore polls the physical key state instead of listening for events (nothing can
/// consume that), matches the binding the pack's own settings hold for the <b>currently selected</b>
/// skin, and advances inside that skin's variant family only. The pack's own appearance hotkey is
/// suppressed for exactly these packages (see <see cref="PackHotkeySuppression"/>) so one press can
/// never advance two steps.
/// </para>
/// </summary>
internal static class VariantHotkeyPoller
{
    private const double PollInterval = 0.05;
    private const string DefaultBinding = "=";

    private static double _elapsed;
    private static readonly HashSet<string> DownBindings = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> LoggedBindings = new(StringComparer.OrdinalIgnoreCase);

    private static WeakReference<MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen>?
        _characterSelect;

    private static string? _characterSelectGroupId;

    internal static void NoteCharacterSelectScreen(
        MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen screen,
        string groupId)
    {
        _characterSelect = new WeakReference<MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen>(screen);
        _characterSelectGroupId = groupId;
    }

    internal static void Tick(double delta)
    {
        _elapsed += delta;
        if (_elapsed < PollInterval)
        {
            return;
        }

        _elapsed = 0;

        // Packs with a variant contract gate their own menu/runtime portrait and their own appearance
        // hotkey on "is the Skin Changer showing MY id"; let a same-code-base re-export through.
        ProviderMenuGateCompat.Tick(PollInterval);

        // Reset the "was down" set when no binding is held any more, so a press that was swallowed
        // while another window had focus cannot stay stuck as "down" forever.
        if (DownBindings.Count > 0 && !AnyBindingStillDown(DownBindings))
        {
            DownBindings.Clear();
        }

        foreach (var (groupId, current, contract) in ActiveFamilies())
        {
            try
            {
                TryAdvance(groupId, current, contract);
            }
            catch (Exception exception)
            {
                ModLog.Warn("外观变体快捷键处理失败：" + exception.GetBaseException().Message);
            }
        }
    }

    private static IEnumerable<(string GroupId, string Current, ProviderSettingsVariantOptions.SettingsVariantContract Contract)>
        ActiveFamilies()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var groupId in ActiveGroupIds())
        {
            if (!seen.Add(groupId))
            {
                continue;
            }

            var optionId = SkinService.GetCharacterSelectionOptionId(groupId);
            if (!ProviderSettingsVariantOptions.IsSettingsVariant(optionId))
            {
                continue;
            }

            var contract = ProviderSettingsVariantOptions.ContractOf(optionId);
            if (contract == null || ProviderSettingsVariantOptions.Family(optionId).Count < 2)
            {
                continue;
            }

            yield return (groupId, optionId!, contract);
        }
    }

    /// <summary>
    /// Which character group the hotkey may act on: the target picked in the appearance screen first,
    /// then the character the player is currently choosing, then the local run character. Never more
    /// than one group, so pressing the key with two packs installed still touches only one family.
    /// </summary>
    private static IEnumerable<string> ActiveGroupIds()
    {
        var localGroup = CharacterAppearanceRuntime.GetLocalPlayer()?.Character.Id.Entry.ToLowerInvariant();
        var target = CharacterAppearanceScreen.HotkeyTargetGroupId;
        if (!string.IsNullOrWhiteSpace(target) &&
            (localGroup == null || target!.Equals(localGroup, StringComparison.OrdinalIgnoreCase)))
        {
            // The appearance screen can point at a teammate; their palette is still the one global
            // value, so only the local player's own group is cycled from here.
            yield return target!;
            yield break;
        }

        if (_characterSelect is { } reference &&
            reference.TryGetTarget(out var screen) &&
            GodotObject.IsInstanceValid(screen) &&
            !string.IsNullOrWhiteSpace(_characterSelectGroupId))
        {
            yield return _characterSelectGroupId!;
            yield break;
        }

        if (localGroup != null)
        {
            yield return localGroup;
        }
    }

    private static void TryAdvance(
        string groupId,
        string current,
        ProviderSettingsVariantOptions.SettingsVariantContract contract)
    {
        var binding = ProviderSettingsVariantOptions.ReadHotkeyBinding(contract) ?? DefaultBinding;

        // Mute the pack's own branch for *every* loaded variant pack, not just this one: the poller is
        // the single source of truth for the appearance hotkey, so a press can never advance twice and
        // two packs can never both react to the same key.
        PackHotkeySuppression.SuppressAll();

        var down = IsPressed(binding);
        if (!down)
        {
            DownBindings.Remove(binding);
            return;
        }

        // Edge trigger: a held key must not spin through the whole family.
        if (!DownBindings.Add(binding))
        {
            return;
        }

        if (LoggedBindings.Add(binding + "\n" + groupId))
        {
            ModLog.Info($"外观变体快捷键已接管：{binding}（{groupId}，由皮肤包设置读取）。");
        }

        var next = ProviderSettingsVariantOptions.NextFamilyOption(current, 1);
        if (next == null)
        {
            return;
        }

        ApplyNext(groupId, current, next);
    }

    private static void ApplyNext(string groupId, string current, string next)
    {
        if (_characterSelect is { } reference &&
            reference.TryGetTarget(out var screen) &&
            GodotObject.IsInstanceValid(screen) &&
            ContextualSkinControls.RequestFrameworkSelection(screen, groupId, next))
        {
            CharacterAppearanceScreen.RefreshLiveSkinDropdowns(groupId);
        }
        else if (NRun.Instance != null)
        {
            CharacterAppearanceRuntime.RequestSelection(groupId, next);
            ContextualSkinControls.RefreshCharacterSelectDropdown(groupId);
            CharacterAppearanceScreen.RefreshLiveSkinDropdowns(groupId);
        }
        else if (!SkinService.ApplySelection(groupId, next))
        {
            ModLog.Warn($"外观变体快捷键没有生效：{SkinService.LastError}");
            return;
        }

        // Portraits are memoised per selection; drop them so the next icon getter sees the new one.
        VariantPortraits.ClearCache();
        ModLog.Info($"外观变体快捷键：{current} → {next}（{groupId}，同一皮肤包的变体）。");
    }

    private static bool AnyBindingStillDown(HashSet<string> bindings)
    {
        foreach (var binding in bindings)
        {
            if (IsPressed(binding))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Whether the binding is physically held. Polling is deliberate: the game's own UI can consume
    /// the key event before a pack's <c>_input</c> node sees it, which is why the hotkey looked dead.
    /// </summary>
    private static bool IsPressed(string binding)
    {
        var wantCtrl = false;
        var wantAlt = false;
        var wantShift = false;
        var wantMeta = false;
        var key = Key.None;

        foreach (var token in binding.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            switch (token.ToLowerInvariant())
            {
                case "ctrl":
                case "control":
                    wantCtrl = true;
                    continue;
                case "alt":
                    wantAlt = true;
                    continue;
                case "shift":
                    wantShift = true;
                    continue;
                case "meta":
                case "command":
                case "cmd":
                case "win":
                case "super":
                    wantMeta = true;
                    continue;
            }

            if (key != Key.None)
            {
                return false;
            }

            if (token == "=")
            {
                key = Key.Equal;
                continue;
            }

            if (!Enum.TryParse<Key>(token, ignoreCase: true, out key) || key == Key.None)
            {
                return false;
            }
        }

        if (key == Key.None)
        {
            return false;
        }

        if (Input.IsKeyPressed(Key.Ctrl) != wantCtrl ||
            Input.IsKeyPressed(Key.Alt) != wantAlt ||
            Input.IsKeyPressed(Key.Shift) != wantShift ||
            Input.IsKeyPressed(Key.Meta) != wantMeta)
        {
            return false;
        }

        return Input.IsKeyPressed(key) || Input.IsPhysicalKeyPressed(key);
    }
}

/// <summary>
/// Suppresses the pack's own appearance-hotkey branch for the packages this Mod drives, so one press
/// cannot advance two steps. Only the appearance call is suppressed: the method is matched by its
/// binding argument, so a pack's other hotkey (Joan 0.3.6's <c>F6</c> safety swap) keeps working.
/// </summary>
internal static class PackHotkeySuppression
{
    private static readonly HashSet<string> Patched = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<(string Binding, bool Ignored)> Suppressed = [];

    internal static void Suppress(ProviderSettingsVariantOptions.SettingsVariantContract contract)
    {
        // Record the binding so the prefix can tell the appearance call from every other one.
        var binding = ProviderSettingsVariantOptions.ReadHotkeyBinding(contract);
        if (string.IsNullOrWhiteSpace(binding))
        {
            return;
        }

        if (Suppressed.All(entry => !entry.Binding.Equals(binding, StringComparison.OrdinalIgnoreCase)))
        {
            Suppressed.Add((binding, true));
        }

        if (!ProviderSettingsVariantOptions.TryGetExecutorContract(contract, out var executor) ||
            !ManagedSkinModLoader.TryGetLoadedProviderAssembly(executor.AssemblyPath, out var assembly) ||
            assembly == null)
        {
            return;
        }

        var key = assembly.FullName ?? assembly.GetName().Name ?? string.Empty;
        if (!Patched.Add(key))
        {
            return;
        }

        var target = FindMatchesMethod(assembly);
        if (target == null)
        {
            return;
        }

        try
        {
            var harmony = new Harmony(Entry.ModId + ".pack-hotkey");
            harmony.Patch(
                target,
                prefix: new HarmonyMethod(typeof(PackHotkeySuppression), nameof(SkipAppearanceHotkeyPrefix)));
            ModLog.Info(
                "已接管皮肤包自己的外观快捷键（同一按键只前进一格）；皮肤包的其他按键照旧生效。");
        }
        catch (Exception exception)
        {
            ModLog.Warn("接管皮肤包外观快捷键失败，可能与包自身同时响应：" + exception.GetBaseException().Message);
        }
    }

    /// <summary>Mutes the appearance hotkey of every variant package that is currently loaded.</summary>
    internal static void SuppressAll()
    {
        foreach (var contract in ProviderSettingsVariantOptions.Slots()
                     .Select(slot => slot.Contract)
                     .DistinctBy(candidate => candidate.AssemblyPath, StringComparer.OrdinalIgnoreCase))
        {
            Suppress(contract);
        }
    }

    private static MethodInfo? FindMatchesMethod(Assembly assembly)
    {
        try
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(
                             BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!method.Name.Equals("Matches", StringComparison.Ordinal) || !method.IsStatic)
                    {
                        continue;
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length == 2 &&
                        parameters[0].ParameterType == typeof(string) &&
                        parameters[1].ParameterType.IsAssignableTo(typeof(InputEventKey)) &&
                        method.ReturnType == typeof(bool))
                    {
                        return method;
                    }
                }
            }
        }
        catch
        {
            // The pack may fail to load its types; suppression is optional.
        }

        return null;
    }

    private static bool SkipAppearanceHotkeyPrefix(string __0, ref bool __result)
    {
        if (string.IsNullOrWhiteSpace(__0))
        {
            return true;
        }

        foreach (var (binding, _) in Suppressed)
        {
            if (binding.Equals(__0, StringComparison.OrdinalIgnoreCase))
            {
                __result = false;
                return false;
            }
        }

        return true;
    }
}
