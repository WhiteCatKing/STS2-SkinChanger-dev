using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Ui;

internal static class HinaUiOstyRegressionTests
{
    private const string VanillaFoolStrikePath = "res://images/atlases/card_atlas.sprites/fool/strike_fool.tres";
    private const string FakeHinaStrikePath = "res://SorasakiMana/images/cards/fool/strike_fool.png";

    internal static void Run()
    {
        CheckCardModelPortraitPatchesAreManaged();
        CheckCardLibraryToolbarAvoidsBackButton();
        CheckIndependentSummonOwnershipDoesNotReportFailure();
        Console.WriteLine("Hina card-patch isolation, CardLibrary toolbar layout and Osty status messaging passed.");
    }

    private static void CheckCardModelPortraitPatchesAreManaged()
    {
        var isManagedMethod = AccessTools.Method(typeof(ManagedSkinModLoader), "IsManagedResourceOwnershipPatch") ??
                              throw new InvalidOperationException("找不到 ManagedSkinModLoader.IsManagedResourceOwnershipPatch。");
        var patchType = typeof(ManagedSkinModLoader).GetNestedType("ProviderPatch", BindingFlags.NonPublic) ??
                        throw new InvalidOperationException("找不到 ManagedSkinModLoader.ProviderPatch。");
        var kindType = typeof(ManagedSkinModLoader).GetNestedType("ProviderPatchKind", BindingFlags.NonPublic) ??
                       throw new InvalidOperationException("找不到 ManagedSkinModLoader.ProviderPatchKind。");
        var postfixKind = Enum.Parse(kindType, "Postfix");
        var callback = AccessTools.Method(typeof(HinaUiOstyRegressionTests), nameof(FakeHinaPortraitPostfix));

        object MakePatch(MethodBase target) =>
            Activator.CreateInstance(
                patchType,
                target,
                callback,
                postfixKind,
                Priority.Normal,
                Array.Empty<string>(),
                Array.Empty<string>(),
                false)!;

        bool IsManaged(MethodBase target) => (bool)isManagedMethod.Invoke(null, [MakePatch(target)])!;

        var portraitPathGetter = AccessTools.PropertyGetter(typeof(CardModel), nameof(CardModel.PortraitPath)) ??
                                 throw new InvalidOperationException("游戏缺少 CardModel.PortraitPath getter。");
        Require(IsManaged(portraitPathGetter),
            "必须隔离日奈用于改写游戏默认卡图路径的 CardModel.PortraitPath 后置。");

        var portraitGetter = AccessTools.PropertyGetter(typeof(CardModel), nameof(CardModel.Portrait)) ??
                             throw new InvalidOperationException("游戏缺少 CardModel.Portrait getter。");
        Require(!IsManaged(portraitGetter),
            "卡图路径隔离应只匹配实际基线 getter，不能一并剥除 Portrait 贴图 getter。");

        foreach (var member in new[] { nameof(CharacterModel.Icon), nameof(CharacterModel.IconTexture) })
        {
            var iconGetter = AccessTools.PropertyGetter(typeof(CharacterModel), member) ??
                             throw new InvalidOperationException($"游戏缺少 CharacterModel.{member} getter。");
            Require(!IsManaged(iconGetter),
                $"必须保留选中日奈的 CharacterModel.{member} 光环图标后置，图鉴卡池头像依赖 IconTexture。");
        }

        var deckColorGetter = AccessTools.PropertyGetter(typeof(CardPoolModel), nameof(CardPoolModel.DeckEntryCardColor)) ??
                              throw new InvalidOperationException("游戏缺少 CardPoolModel.DeckEntryCardColor getter。");
        Require(!IsManaged(deckColorGetter),
            "CardPoolModel.DeckEntryCardColor 等非卡图路径补丁不应被误杀。");

        // End-to-end verification: installing a CardModel.PortraitPath postfix reproduces the exact bug
        // (baseline path hijacked to res://SorasakiMana/...), and stripping managed patches restores vanilla Game Default.
        var harmony = new Harmony("Gurio.SkinChanger.Tests.HinaPortraitPatch");
        var card = InitCard(new HinaRegressionFoolStrike(), new HinaRegressionFoolPool(), "STRIKE_FOOL");
        try
        {
            harmony.Patch(portraitPathGetter, postfix: new HarmonyMethod(callback));
            Require(FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out var hijacked) &&
                    hijacked == FakeHinaStrikePath,
                "复现前提：未剥离 CardModel.PortraitPath 补丁时，基线路径会被劫持为提供者路径。");

            var unpatch = AccessTools.Method(typeof(ManagedSkinModLoader), "UnpatchProviderCallbacks") ??
                          throw new InvalidOperationException("找不到 ManagedSkinModLoader.UnpatchProviderCallbacks。");
            var arr = Array.CreateInstance(patchType, 1);
            arr.SetValue(MakePatch(portraitPathGetter), 0);
            unpatch.Invoke(null, [arr]);

            Require(FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out var restored) &&
                    restored == VanillaFoolStrikePath,
                "剥离受管理的 CardModel.PortraitPath 补丁后，游戏默认卡图基线必须恢复为游戏原版路径。");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
        }
    }

    private static void CheckCardLibraryToolbarAvoidsBackButton()
    {
        const string topPath = "Sidebar/MarginContainer/TopVBox";
        var attach = PatchProcessor.GetOriginalInstructions(
            AccessTools.Method(typeof(CardSkinControls), nameof(CardSkinControls.Attach)));
        Require(attach.Any(instruction => Equals(instruction.operand, topPath)),
            "皮肤控件必须挂进卡牌总览原生左侧栏的顶部容器。");
        Require(!attach.Any(instruction => instruction.operand is string path &&
                    (path == "CardGrid" || path.StartsWith("CardGrid/", StringComparison.Ordinal))),
            "卡牌总览皮肤控件不得访问或挪动原生卡牌网格。");

        var methods = attach.Select(instruction => instruction.operand).OfType<MethodInfo>().ToList();
        var mountToolbar = methods.FindIndex(method => method.Name == nameof(Node.AddChild));
        var moveToTop = methods.FindIndex(method => method.Name == nameof(Node.MoveChild));
        var instantiateFilter = methods.FindIndex(method => method.Name == nameof(PackedScene.Instantiate));
        var mountFilter = instantiateFilter < 0 ? -1 : methods.FindIndex(
            instantiateFilter + 1, method => method.Name == nameof(Node.AddChild));
        var setLabel = methods.FindIndex(method => method.DeclaringType == typeof(NLibraryStatTickbox) &&
                                                   method.Name == nameof(NLibraryStatTickbox.SetLabel));
        Require(mountToolbar >= 0 && mountToolbar < moveToTop && moveToTop < instantiateFilter &&
                instantiateFilter < mountFilter && mountFilter < setLabel,
            "必须先把工具栏挂进场景树，再挂入筛选框触发 _Ready，最后才能调用 SetLabel。");
        var moveInstruction = attach.FindIndex(instruction =>
            instruction.operand is MethodInfo method && method.Name == nameof(Node.MoveChild));
        Require(moveInstruction > 0 && attach[moveInstruction - 1].opcode == System.Reflection.Emit.OpCodes.Ldc_I4_0,
            "工具栏应插在搜索框和原版筛选项之前，避免随卡池数量变化而下移到返回箭头。");

        // Both archived scenes have a 288-wide sidebar with 16px margins, and
        // the BackButton (including its shadow) spans x=[-40, 218], y=[H-354, H-205].
        // The first two rows are 40px and 42px high, with the default 4px separation.
        const float toolbarLeft = 16f;
        const float toolbarTop = 16f;
        const float toolbarRight = 288f - 16f;
        const float toolbarBottom = toolbarTop + 40f + 4f + 42f;
        foreach (var (viewportWidth, viewportHeight) in new[] { (1920f, 1080f), (1280f, 720f) })
        {
            var overlapsBack = toolbarLeft < 218f && toolbarRight > -40f &&
                               toolbarTop < viewportHeight - 205f && toolbarBottom > viewportHeight - 354f;
            Require(!overlapsBack,
                $"视口 {viewportWidth}x{viewportHeight} 下皮肤控件不得与左下角返回箭头重叠。");
            Require(toolbarRight < 288f,
                "皮肤控件必须留在原生侧栏内，不得遮挡卡牌或滚动条。");
        }
    }

    private static void CheckIndependentSummonOwnershipDoesNotReportFailure()
    {
        Require(CharacterAppearanceScreen.FormatAppliedStatus(null) == null &&
                CharacterAppearanceScreen.FormatAppliedStatus("   ") == null,
            "无延迟刷新项时不应显示状态提示。");
        Require(CharacterAppearanceScreen.FormatAppliedStatus("OSTY: independent summon mod owns this body") == null,
            "独立召唤 Mod 接管奥斯提身体时，角色切肤已生效，不应弹出失败提示。");
        Require(CharacterAppearanceScreen.FormatAppliedStatus(
                    "OSTY: independent summon mod owns this body; OSTY: independent summon mod owns this body") == null,
            "多个独立召唤接管项也不应弹出失败提示。");

        string[] languages =
        [
            "eng", "zhs", "zht", "deu", "esp", "fra", "ita", "jpn",
            "kor", "pol", "ptb", "rus", "spa", "tha", "tur"
        ];
        var previousLanguage = ModLocalization.CurrentLanguage;
        var setLanguage = AccessTools.PropertySetter(typeof(ModLocalization), nameof(ModLocalization.CurrentLanguage)) ??
                          AccessTools.Field(typeof(ModLocalization), "<CurrentLanguage>k__BackingField") as MemberInfo;
        try
        {
            foreach (var lang in languages)
            {
                if (setLanguage is PropertyInfo prop) prop.SetValue(null, lang);
                else if (setLanguage is MethodInfo setter) setter.Invoke(null, [lang]);
                else if (setLanguage is FieldInfo field) field.SetValue(null, lang);

                var deferred = CharacterAppearanceScreen.FormatAppliedStatus("NECROBINDER: death animation");
                var expected = ModLocalization.Get(ModText.AppearanceSavedNextTime);
                var failed = ModLocalization.Get(ModText.AppearanceFailed);
                Require(!string.IsNullOrWhiteSpace(expected) &&
                        deferred == expected &&
                        !string.Equals(expected, failed, StringComparison.OrdinalIgnoreCase),
                    $"语言 {lang} 必须提供非空且区别于失败提示的 AppearanceSavedNextTime 文案。");
            }
        }
        finally
        {
            if (setLanguage is PropertyInfo prop) prop.SetValue(null, previousLanguage);
            else if (setLanguage is MethodInfo setter) setter.Invoke(null, [previousLanguage]);
            else if (setLanguage is FieldInfo field) field.SetValue(null, previousLanguage);
        }
    }

    private static void FakeHinaPortraitPostfix(CardModel __instance, ref string __result)
    {
        if (__instance is HinaRegressionFoolStrike)
        {
            __result = FakeHinaStrikePath;
        }
    }

    private static T InitCard<T>(T card, CardPoolModel pool, string id) where T : CardModel
    {
        AccessTools.Field(typeof(AbstractModel), "<Id>k__BackingField").SetValue(card, new ModelId("CARD", id));
        AccessTools.Field(typeof(CardModel), "_pool").SetValue(card, pool);
        return card;
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }

    private sealed class HinaRegressionFoolPool : CardPoolModel
    {
        public override string Title => "fool";
        public override string EnergyColorName => "ironclad";
        public override string CardFrameMaterialPath => "card_frame_red";
        public override Color DeckEntryCardColor => Colors.White;
        public override bool IsColorless => false;
        protected override CardModel[] GenerateAllCards() => [];
    }

    private sealed class HinaRegressionFoolStrike : CardModel
    {
        public HinaRegressionFoolStrike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy) { }
    }
}
