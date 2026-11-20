using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

internal static class IndependentFoolSkinCompat
{
    private static readonly HashSet<Assembly> Prepared = [];
    internal static void Prepare(Assembly assembly)
    {
        if (FoolWaifuRuntimeCompat.IsProvider(assembly))
        {
            FoolWaifuRuntimeCompat.Install(assembly);
            return;
        }
        if (assembly.GetName().Name != "NailongFool" || !Prepared.Add(assembly)) return;
        var mount = assembly.GetType("NailongFool.NailongFoolMod")?.GetMethod("MountAssets", BindingFlags.Static | BindingFlags.NonPublic);
        if (mount != null) new Harmony(Entry.ModId + ".nailong-pack").Patch(mount,
            prefix: new HarmonyMethod(typeof(IndependentFoolSkinCompat), nameof(SkipAuthorMount)));
    }
    private static bool SkipAuthorMount() => false; // SC mounts the selected .assets via its ordinary isolated pack path.

    internal static MethodInfo? SelectionBackgroundContract(Assembly assembly) =>
        assembly.GetName().Name == "FoolSkinStatic" ? AccessTools.Method(
            assembly.GetType("FoolSkinStatic.Patches"), "BuildPortrait", [typeof(Node)]) : null;

    internal static void ApplySelectionBackground(Control screen, string groupId)
    {
        if (screen is MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen ||
            groupId != "fool" || SkinService.GetSelectedCreatureRuntimeProvider(groupId) is not { } provider ||
            ManagedSkinModLoader.GetActiveProviderAssembly(provider) is not { } assembly ||
            SelectionBackgroundContract(assembly) is not { } build ||
            STS2SkinChanger.Ui.CharacterSelectScreenAccess.Background(screen) is not { } container) return;
        foreach (var background in container.GetChildren().OfType<Control>().Where(c => c.Visible))
        {
            // Preserve the author's portrait animation and native mode viewport/material.
            if (BuildSelectionBackground(assembly, background))
                background.GetNodeOrNull<Node2D>("SpineSprite")?.Hide();
        }
    }

    internal static bool BuildSelectionBackground(Assembly assembly, Node background)
    {
        if (SelectionBackgroundContract(assembly) is not { } build) return false;
        var patches = build.DeclaringType!;
        AccessTools.Method(patches, "EnsurePortrait").Invoke(null, null);
        if (AccessTools.Field(patches, "_portrait").GetValue(null) is not Texture2D) return false;
        build.Invoke(null, [background]);
        return true;
    }

    internal static void ApplyPreview(NCreatureVisuals visuals, string? groupId)
    {
        if (groupId != "fool" || SkinService.GetSelectedCreatureRuntimeProvider(groupId) is not { } provider ||
            ManagedSkinModLoader.GetActiveProviderAssembly(provider) is not { } assembly) return;
        switch (assembly.GetName().Name)
        {
            case "FoolSkinStatic":
                ApplyFoolSkinStaticPreview(assembly, visuals);
                break;
            case "NonoFoolSkin":
                ApplyNonoFoolPreview(assembly, visuals);
                break;
        }
    }

    private static void ApplyFoolSkinStaticPreview(Assembly assembly, NCreatureVisuals visuals)
    {
        var patches = assembly.GetType("FoolSkinStatic.Patches", true)!;
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        patches.GetMethod("EnsureCharacter", flags)!.Invoke(null, null);
        var open = patches.GetField("_texture", flags)?.GetValue(null) as Texture2D;
        var closed = patches.GetField("_closed", flags)?.GetValue(null) as Texture2D;
        if (open == null) return;
        var sprite = patches.GetMethod("MakeSprite", flags)!.Invoke(null, [visuals, open, closed]) as Sprite2D;
        if (sprite == null) return;
        patches.GetMethod("Play", flags)!.Invoke(null, [sprite, "idle"]);
        patches.GetMethod("BlinkLoop", flags, null, [typeof(Sprite2D)], null)!.Invoke(null, [sprite]);
        // Stable exposes GetCurrentBody(); beta adds Body. Resolve the native branch contract.
        GameBranchApi.CurrentBody(visuals)?.Hide();
    }

    private static void ApplyNonoFoolPreview(Assembly assembly, NCreatureVisuals visuals)
    {
        // NonoFoolSkin applies its animated SpriteFrames rig in NCreature._Ready, which
        // FrameworkModelPreview deliberately skips for menu previews (to protect combat-bound
        // packs). Build a local preview Rig directly against the author's contract without
        // overwriting AnimDirector._rig or leaving stale entries in Rig.Rigs.
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var directorType = assembly.GetType("NonoFoolSkin.AnimDirector", false);
        var artStoreType = assembly.GetType("NonoFoolSkin.ArtStore", false);
        var rigType = assembly.GetType("NonoFoolSkin.Rig", false);
        if (directorType == null || artStoreType == null || rigType == null) return;

        var configProperty = directorType.GetProperty("Config", staticFlags);
        var config = configProperty?.GetValue(null);
        if (config == null)
        {
            directorType.GetMethod("Initialize", staticFlags, null, Type.EmptyTypes, null)?.Invoke(null, null);
            config = configProperty?.GetValue(null);
        }
        if (config == null) return;

        var layout = config.GetType().GetProperty("Layout", instanceFlags)?.GetValue(config);
        var animations = config.GetType().GetProperty("Animations", instanceFlags)?.GetValue(config)
            as System.Collections.IDictionary;
        var artGet = artStoreType.GetMethod("Get", staticFlags, null, [typeof(string)], null);
        if (layout == null || artGet == null) return;

        var idleDef = animations != null && animations.Contains("idle") ? animations["idle"] : null;
        var prefix = idleDef?.GetType().GetProperty("Prefix", instanceFlags)?.GetValue(idleDef) as string;
        var file = idleDef?.GetType().GetProperty("File", instanceFlags)?.GetValue(idleDef) as string;
        var eyesClosedName = idleDef?.GetType().GetProperty("EyesClosed", instanceFlags)?.GetValue(idleDef) as string;

        var firstFrame = !string.IsNullOrEmpty(prefix)
            ? artGet.Invoke(null, [prefix + "00.png"]) as Texture2D
            : null;
        var open = firstFrame ?? (artGet.Invoke(null, [file ?? "character.png"]) as Texture2D);
        if (open == null) return;

        var closed = firstFrame == null && !string.IsNullOrEmpty(eyesClosedName)
            ? artGet.Invoke(null, [eyesClosedName]) as Texture2D
            : null;

        var createRig = rigType.GetMethod("Create", staticFlags);
        var rig = createRig?.Invoke(null, [visuals, open, closed, layout]);
        if (rig == null) return;

        // Rig.Create registers the instance in a static Dictionary<ulong, Rig>; detach this
        // ephemeral preview immediately so switching skins leaves no stale reference behind.
        if (rigType.GetField("Rigs", staticFlags)?.GetValue(null) is System.Collections.IDictionary rigs)
        {
            object? previewKey = null;
            foreach (System.Collections.DictionaryEntry entry in rigs)
            {
                if (ReferenceEquals(entry.Value, rig))
                {
                    previewKey = entry.Key;
                    break;
                }
            }
            if (previewKey != null) rigs.Remove(previewKey);
        }

        var played = false;
        var mode = (idleDef?.GetType().GetProperty("Mode", instanceFlags)?.GetValue(idleDef) as string)
            ?.ToLowerInvariant();
        if (mode == "frames" && idleDef != null)
        {
            var buildFrames = directorType.GetMethod("BuildFrames", staticFlags);
            var frames = buildFrames?.Invoke(null, ["idle", idleDef]) as SpriteFrames;
            var showFrames = rigType.GetMethod("ShowFrames", instanceFlags);
            if (frames != null && showFrames != null)
            {
                var speed = idleDef.GetType().GetProperty("Speed", instanceFlags)?.GetValue(idleDef) is float s
                    ? s
                    : 1f;
                var loop = idleDef.GetType().GetProperty("Loop", instanceFlags)?.GetValue(idleDef) is bool l
                    ? l
                    : true;
                var targetHeight = idleDef.GetType().GetProperty("TargetHeight", instanceFlags)?.GetValue(idleDef) as float?;
                var returnTo = idleDef.GetType().GetProperty("ReturnTo", instanceFlags)?.GetValue(idleDef) as string;
                showFrames.Invoke(rig, [frames, speed, loop, targetHeight, returnTo]);
                played = true;
            }
        }

        if (!played)
        {
            var staticOpen = open;
            var fallbackName = idleDef?.GetType().GetProperty("Fallback", instanceFlags)?.GetValue(idleDef) as string;
            if (!string.IsNullOrEmpty(fallbackName) && animations != null && animations.Contains(fallbackName))
            {
                var fallbackDef = animations[fallbackName];
                var fallbackFile = fallbackDef?.GetType().GetProperty("File", instanceFlags)?.GetValue(fallbackDef) as string;
                var fallbackClosedName = fallbackDef?.GetType().GetProperty("EyesClosed", instanceFlags)?.GetValue(fallbackDef) as string;
                if (artGet.Invoke(null, [fallbackFile ?? "character.png"]) is Texture2D resolvedOpen)
                {
                    staticOpen = resolvedOpen;
                    var resolvedClosed = !string.IsNullOrEmpty(fallbackClosedName)
                        ? artGet.Invoke(null, [fallbackClosedName]) as Texture2D
                        : null;
                    rigType.GetProperty("Closed", instanceFlags)?.SetValue(rig, resolvedClosed);
                }
            }

            rigType.GetMethod("ShowStatic", instanceFlags)?.Invoke(rig, [staticOpen, true]);
        }

        GameBranchApi.CurrentBody(visuals)?.Hide();
    }
}
