using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

internal static class MerchantVariantScenes
{
    internal const string BlitzEntryId = "blitz_merchant";
    internal const string NativeBlitzBody = "res://animations/backgrounds/blitz_merchant_room/top/blitz_merchant_top_skel_data.tres";
    internal static bool HasBlitz => typeof(NMerchantRoom).Assembly.GetType("MegaCrit.Sts2.Core.Nodes.Rooms.NBlitzMerchantRoom") != null;
    internal static bool IsBlitzType(Type type) => type.Name is "NBlitzMerchantRoom" or "NBlitzMerchantButton" or "NBlitzMerchantVfx";
    internal static bool IsBlitz(Node node)
    {
        for (Node? current = node; current != null; current = current.GetParent())
            if (IsBlitzType(current.GetType())) return true;
        return false;
    }
    internal static MethodInfo? Skinner(Assembly assembly) => assembly.GetType("Merchant2CuteII.script.Settings.MerchantSceneSkinner") is { } type
        ? AccessTools.Method(type, "ApplyBeforeEnteringTree", [typeof(Node)]) : null;
    internal static bool HasBlitzContract(Assembly assembly) => Skinner(assembly)?.ReturnType == typeof(int) &&
        assembly.GetType("Merchant2CuteII.script.Compat.BlitzMerchantCompatibility") is { } compat &&
        AccessTools.Method(compat, "LoadCustomBody", Type.EmptyTypes)?.ReturnType == typeof(Resource);

    internal static void ApplySkinner(Assembly assembly, Node root) => Skinner(assembly)?.Invoke(null, [root]);

    internal static bool MatchesFamily(Node node, bool blitz, bool fake) => fake ? node is NFakeMerchant :
        node is NMerchantRoom && (blitz == IsBlitzType(node.GetType()));

    private static void ValidateBlitzScene(Node scene)
    {
        var button = scene.GetNodeOrNull<NMerchantButton>("SceneContainer/MerchantButton");
        if (button == null || !IsBlitzType(button.GetType()) ||
            scene.GetNodeOrNull<Node>("Inventory") == null || scene.GetNodeOrNull<Node>("ProceedButton") == null)
            throw new InvalidOperationException("极速商店缺少原生按钮、商品或前进入口。");
        foreach (var name in new[] { "MerchantVisual", "MerchantSelectionReticle", "Token1", "Token2", "Token3", "Token4", "Token5", "Token6" })
            if (button.FindChild(name, recursive: true, owned: false) == null)
                throw new InvalidOperationException("极速商店缺少必要视觉节点：" + name);
    }

    internal static Node Instantiate(string groupId, string scenePath)
    {
        var blitz = scenePath == MerchantRuntimeAppearance.BlitzMerchantRoomScenePath;
        var fake = groupId == MerchantRuntimeAppearance.FakeMerchantGroupId;
        var provider = SkinService.GetSelectedRuntimeProvider(groupId);
        Assembly? assembly = null;
        if (provider != null)
        {
            assembly = ManagedSkinModLoader.GetActiveProviderAssembly(provider);
            if (assembly == null && ManagedSkinModLoader.TryGetProviderAssemblyForInspection(provider, out var inspected))
                assembly = inspected; // Scoped node helper only; never run a global initializer or PatchAll.
        }
        var option = SkinService.Catalog?.Groups.FirstOrDefault(g => g.Id == groupId)?.Options
            .FirstOrDefault(o => o.Id == SkinService.GetDisplayedSelection(groupId));
        var supported = option?.IsMerchantVariantFallback != true && (!blitz || SkinService.SelectedSkinProvidesResource(groupId, NativeBlitzBody) ||
            (assembly != null && HasBlitzContract(assembly)));
        try
        {
            return SkinService.InstantiateMerchantVariantScene(groupId, scenePath, !supported, (node, usesSkin) =>
            {
                if (!MatchesFamily(node, blitz, fake)) throw new InvalidOperationException("皮肤商店场景类型不匹配。");
                if (blitz) ValidateBlitzScene(node);
                if (usesSkin && assembly != null) ApplySkinner(assembly, node);
            });
        }
        catch (Exception error)
        {
            ModLog.Warn("商店皮肤场景不可用，保留原生场景：" + error.GetBaseException().Message);
            return SkinService.InstantiateMerchantVariantScene(groupId, scenePath, true, (node, _) =>
            {
                if (!MatchesFamily(node, blitz, fake)) throw new InvalidOperationException("原生商店场景类型不匹配。");
                if (blitz) ValidateBlitzScene(node);
            });
        }
    }

    internal static void BindBlitzInventory(NMerchantButton button, MegaCrit.Sts2.Core.Entities.Merchant.MerchantInventory inventory)
    {
        if (!IsBlitzType(button.GetType())) return;
        var setup = AccessTools.Method(button.GetType(), "Setup", [inventory.GetType()]) ??
            AccessTools.Method(button.GetType(), "Setup", [typeof(MegaCrit.Sts2.Core.Entities.Merchant.MerchantInventory)]);
        if (setup == null) throw new MissingMethodException("极速商人缺少原生商品绑定入口。");
        setup.Invoke(button, [inventory]);
        var remaining = AccessTools.Property(inventory.GetType(), "BlitzChoicesRemaining")?.GetValue(inventory);
        if (remaining is int count && count <= 0) AccessTools.Method(button.GetType(), "CompleteShop")?.Invoke(button, null);
    }
}
