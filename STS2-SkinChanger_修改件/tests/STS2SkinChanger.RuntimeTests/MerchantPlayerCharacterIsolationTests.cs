using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using STS2SkinChanger.Core;

// 纯商人皮肤（如 MerchantFeminization / A大商人）误将玩家在商店左侧的 NMerchantCharacter 当成假商人，
// 给 NMerchantCharacter._Ready / PlayAnimation 打补丁并把玩家骨架替换成未缩放的假商人骨架。
// 这里验证切换器会准确剥离纯商人提供者在 NMerchantCharacter 上的补丁，同时保留其真/假商人按钮与手部补丁，
// 且不影响角色皮肤或混合包对 NMerchantCharacter 的合法补丁。
internal static class MerchantPlayerCharacterIsolationTests
{
    internal static void Run()
    {
        var charReady = AccessTools.Method(typeof(NMerchantCharacter), nameof(NMerchantCharacter._Ready));
        var charPlayAnim = AccessTools.Method(typeof(NMerchantCharacter), nameof(NMerchantCharacter.PlayAnimation));
        var buttonReady = AccessTools.Method(typeof(NMerchantButton), nameof(NMerchantButton._Ready));
        var handReady = AccessTools.Method(typeof(NMerchantHand), nameof(NMerchantHand._Ready));
        Require(charReady != null && charPlayAnim != null && buttonReady != null && handReady != null,
            "游戏应包含 NMerchantCharacter / NMerchantButton / NMerchantHand 目标方法。");

        // 1. 纯商人提供者（只服务 merchant / fake_merchant_monster）应剥离 NMerchantCharacter 补丁。
        Require(ManagedSkinModLoader.IsMerchantOnlyProviderGroups([
                ("merchant", false),
                ("fake_merchant_monster", false)
            ]),
            "只服务真/假商人分组的包应被识别为纯商人提供者。");
        Require(ManagedSkinModLoader.IsMerchantOnlyProviderGroups([
                ("merchant", false),
                ("merchant_room_bg", false)
            ]),
            "附带非角色背景分组但无角色分组的商人包也应被识别为纯商人提供者。");

        // 2. 角色皮肤或含角色分组的混合包不得被判为纯商人提供者。
        Require(!ManagedSkinModLoader.IsMerchantOnlyProviderGroups([
                ("fool", true)
            ]),
            "角色皮肤提供者不得被判为纯商人提供者。");
        Require(!ManagedSkinModLoader.IsMerchantOnlyProviderGroups([
                ("merchant", false),
                ("fool", true)
            ]),
            "同时含角色与商人分组的混合包不得被剥离 NMerchantCharacter 补丁。");
        Require(!ManagedSkinModLoader.IsMerchantOnlyProviderGroups([]),
            "无商人分组的提供者不得被判为纯商人提供者。");

        // 3. 剥离目标严格限定在 NMerchantCharacter，不误伤真/假商人按钮和手部。
        Require(ManagedSkinModLoader.ShouldStripPlayerMerchantCharacterPatch(charReady!, providerIsMerchantOnly: true),
            "纯商人提供者在 NMerchantCharacter._Ready 上的补丁必须被剥离。");
        Require(ManagedSkinModLoader.ShouldStripPlayerMerchantCharacterPatch(charPlayAnim!, providerIsMerchantOnly: true),
            "纯商人提供者在 NMerchantCharacter.PlayAnimation 上的补丁必须被剥离。");
        Require(!ManagedSkinModLoader.ShouldStripPlayerMerchantCharacterPatch(charReady!, providerIsMerchantOnly: false),
            "角色皮肤在 NMerchantCharacter._Ready 上的补丁必须保留。");
        Require(!ManagedSkinModLoader.ShouldStripPlayerMerchantCharacterPatch(buttonReady!, providerIsMerchantOnly: true),
            "纯商人提供者在 NMerchantButton._Ready 上的真/假商人补丁必须保留。");
        Require(!ManagedSkinModLoader.ShouldStripPlayerMerchantCharacterPatch(handReady!, providerIsMerchantOnly: true),
            "纯商人提供者在 NMerchantHand._Ready 上的手部补丁必须保留。");

        // 4. 验证激活隔离管线确实调用了 ShouldStripPlayerMerchantCharacterPatch。
        var activate = typeof(ManagedSkinModLoader)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            .FirstOrDefault(m => m.Name.Contains("Activate", StringComparison.Ordinal) &&
                PatchProcessor.GetOriginalInstructions(m).Any(instruction =>
                    instruction.operand is MethodInfo target &&
                    target.DeclaringType == typeof(ManagedSkinModLoader) &&
                    target.Name == nameof(ManagedSkinModLoader.ShouldStripPlayerMerchantCharacterPatch)));
        var nestedCaller = typeof(ManagedSkinModLoader)
            .GetNestedTypes(BindingFlags.NonPublic)
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            .FirstOrDefault(m => m.GetMethodBody() != null &&
                PatchProcessor.GetOriginalInstructions(m).Any(instruction =>
                    instruction.operand is MethodInfo target &&
                    target.DeclaringType == typeof(ManagedSkinModLoader) &&
                    target.Name == nameof(ManagedSkinModLoader.ShouldStripPlayerMerchantCharacterPatch)));
        Require(activate != null || nestedCaller != null,
            "ManagedSkinModLoader 激活过滤链路必须接入 ShouldStripPlayerMerchantCharacterPatch。");

        Console.WriteLine("Merchant player character isolation passed.");
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}
