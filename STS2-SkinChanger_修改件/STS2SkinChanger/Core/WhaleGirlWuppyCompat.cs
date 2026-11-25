using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

// This author constructs SpriteFrames from its private atlas in code and owns the separate
// rest-site presentation. Leave its initializer native and scope only its combat body hook.
internal static class WhaleGirlWuppyCompat
{
    internal const string ProviderId = "WhaleGirlWuppy";

    internal static void Install(Harmony harmony)
    {
        void Bind(Assembly assembly)
        {
            if (assembly.GetName().Name != ProviderId) return;
            var target = AccessTools.Method(assembly.GetType(ProviderId + ".WhaleAnimation"),
                "TryReplaceBody", [typeof(NCreatureVisuals), typeof(bool)]);
            if (target == null || Harmony.GetPatchInfo(target)?.Owners.Contains(harmony.Id) == true) return;
            harmony.Patch(target, prefix: new HarmonyMethod(typeof(WhaleGirlWuppyCompat), nameof(AllowCombatBody)));
        }
        AppDomain.CurrentDomain.AssemblyLoad += (_, args) => Bind(args.LoadedAssembly);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) Bind(assembly);
    }

    private static bool AllowCombatBody() =>
        SkinService.Catalog?.Groups.Any(group => group.Id == "wuppy" &&
            group.Options.Any(option => option.Id == ProviderId)) != true ||
        SkinService.GetDisplayedSelection("wuppy") == ProviderId;

    internal static AnimatedSprite2D? CreatePreview()
    {
        if (SkinService.GetDisplayedSelection("wuppy") != ProviderId) return null;
        try
        {
            var artType = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == ProviderId)
                ?.GetType(ProviderId + ".WhaleArt");
            if (artType == null) return null;
            var lazy = AccessTools.Field(artType, "Combat")?.GetValue(null);
            var art = lazy?.GetType().GetProperty("Value")?.GetValue(lazy);
            if (art == null || AccessTools.Property(artType, "Frames")?.GetValue(art) is not SpriteFrames frames)
                return null;
            return new AnimatedSprite2D { SpriteFrames = frames, Animation = "idle", Offset = new Vector2(0, -48) };
        }
        catch (Exception error)
        {
            ModLog.Warn("加载鲸鱼娘瓦皮预览失败：" + error.GetBaseException().Message);
            return null;
        }
    }
}
