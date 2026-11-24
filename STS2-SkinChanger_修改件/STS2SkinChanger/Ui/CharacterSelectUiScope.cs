using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

// Some skin backgrounds attach a top-level CanvasLayer to escape the background clip.
// Associate those nodes with the presentation that created them, including when the game
// keeps the selection submenu cached instead of freeing it.
internal static class CharacterSelectUiScope
{
    private static readonly ConditionalWeakTable<Control, Scope> Scopes = new();

    private sealed class Scope
    {
        public HashSet<Node> ExternalUi { get; } = [];
        public bool Leaving { get; set; }
        public long Departure { get; set; }
        public CharacterModel? Character { get; set; }
    }

    internal static void Observe(Control screen, CharacterModel character)
    {
        var scope = Scopes.GetValue(screen, owner =>
        {
            var state = new Scope();
            owner.VisibilityChanged += () =>
            {
                if (!owner.IsVisibleInTree()) Leave(owner);
            };
            owner.TreeExiting += () => Leave(owner);
            return state;
        });
        scope.Character = character;
        if (screen.IsVisibleInTree()) scope.Leaving = false;
        AdoptKnownNativeLayer(screen, scope);
    }

    internal static void Reopen(Control screen)
    {
        if (!Scopes.TryGetValue(screen, out var scope) || !scope.Leaving || scope.Character == null)
            return;

        var departure = scope.Departure;
        // [延迟调度/CallDeferred] 原触发时机：缓存选人子界面 OnSubmenuOpened 后置（且处于 Leaving 状态）延迟重走呈现；
        // 最早可执行帧：本帧末 idle；顺序依赖：须晚于原版 OnSubmenuOpened 与可能同步触发的 SelectCharacter；
        // 能否合并：按 scope.Departure 代数淘汰旧回调；重试上限：1 次；
        // 取消条件：!IsInstanceValid(screen) || !screen.IsVisibleInTree() || !scope.Leaving || scope.Departure != departure || scope.Character == null。
        DeferredDispatch.CallDeferred("CharacterSelectUiScope.Reopen", screen, () =>
        {
            if (!GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree() ||
                !scope.Leaving || scope.Departure != departure || scope.Character is not { } character)
                return;

            // Cached submenus may keep the old background without firing SelectCharacter.
            // Run the normal presentation path to recreate both the background and its UI.
            ContextualSkinControls.ShowCharacter(screen, character);
        }, DeferredDispatchRisk.Normal);
    }

    internal static bool IsLeaving(Control screen) =>
        Scopes.TryGetValue(screen, out var scope) && scope.Leaving;

    internal static IDisposable? Capture(Control screen, Control? background = null)
    {
        if (!screen.IsInsideTree()) return null;
        var scope = Scopes.GetValue(screen, _ => new Scope());
        return new CaptureLease(screen.GetTree().Root, scope, background);
    }

    internal static void Leave(Control screen)
    {
        if (!Scopes.TryGetValue(screen, out var scope)) return;
        scope.Leaving = true;
        scope.Departure++;
        AdoptKnownNativeLayer(screen, scope);
        foreach (var node in scope.ExternalUi) Retire(node);
        scope.ExternalUi.Clear();
        ManagedSkinModLoader.RestoreCharacterPresentation(screen);
    }

    private static void AdoptKnownNativeLayer(Control screen, Scope scope)
    {
        if (!screen.IsInsideTree()) return;
        // The vanilla selection callback can instantiate Tiphera's wrapper before our postfix.
        // Identify its layer by the author's own audio child, even if Godot renamed the layer
        // because the previous wrapper's queued deletion has yet to run.
        foreach (var layer in screen.GetTree().Root.GetChildren().OfType<CanvasLayer>())
        {
            if (layer.GetNodeOrNull<AudioStreamPlayer>("TipheraSkinSelectVoice") != null)
                scope.ExternalUi.Add(layer);
        }
    }

    private static void Retire(Node node)
    {
        if (!GodotObject.IsInstanceValid(node)) return;
        if (node is CanvasLayer layer) layer.Hide();
        if (node is Control control) control.Hide();
        node.QueueFree();
    }

    private sealed class CaptureLease(Window root, Scope scope, Control? background) : IDisposable
    {
        private readonly HashSet<Node> _existing = root.GetChildren().ToHashSet();

        public void Dispose()
        {
            foreach (var node in root.GetChildren())
            {
                if (_existing.Contains(node) || node is not (CanvasLayer or Control)) continue;
                if (scope.Leaving)
                {
                    Retire(node);
                    continue;
                }
                scope.ExternalUi.Add(node);
                if (background != null) background.TreeExiting += () => Retire(node);
            }
            scope.ExternalUi.RemoveWhere(node => !GodotObject.IsInstanceValid(node));
        }
    }
}

[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.OnSubmenuClosed))]
internal static class CharacterSelectUiClosePatch
{
    private static void Prefix(Control __instance) =>
        UiInjectionGuard.Run("CharacterSelectUiClosePatch.Prefix", () => CharacterSelectUiScope.Leave(__instance));
}

[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.OnSubmenuOpened))]
internal static class CharacterSelectUiOpenPatch
{
    private static void Postfix(Control __instance) =>
        UiInjectionGuard.Run("CharacterSelectUiOpenPatch.Postfix", () => CharacterSelectUiScope.Reopen(__instance));
}
