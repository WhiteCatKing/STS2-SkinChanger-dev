using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;

namespace STS2SkinChanger.Core;

// ============================================================================================
// 假商人“事件”构图补丁（只为重绘假商人帐篷的事件外观包服务，与背景切换功能互相独立）
//
// 作用：把假商人事件画面里的动画帐篷按“战斗背景”的规则重新摆放（等比铺满事件画面、居中、
//       超出裁剪），再整体右移 HorizontalShift 像素，用来把原画右侧那条偏白的空余推出屏幕。
//
// 位置：
//   本文件 = 全部实现（约 150 行），游戏侧只有 1 个 Harmony 补丁：NFakeMerchant._Ready 的 Postfix。
//
// 以后不需要时的删除方式：
//   1. 删掉本文件 `STS2SkinChanger/Core/FakeMerchantEventFrame.cs` 即可 —— Entry 的补丁安装器
//      按 [HarmonyPatch] 类逐个扫描，文件删掉后补丁不会再生效，事件恢复游戏原构图。
//   2. 顺手删掉文档里“假商人事件构图补丁”一节的说明
//      （docs/issues/ISSUE_2026-11-07_fake_merchant_background.md）。
//   没有任何存档、配置或外部文件依赖；事件外观包本身不受影响。
//
// 实测数据（1920×1080 画布）：骨架范围 = (-1151.28,-358.47) + (4866,2880)，骨架 scale 0.5，
// 铺满缩放 0.789，原始居中原点 (454.27,113.26)；右移 41px 后白边刚好出屏（作者实机确认）。
// ============================================================================================

/// <summary>
/// Re-frames the fake merchant event stage so the redrawn tent fills the screen like the battle
/// background does. See the block comment above for the removal instructions.
/// </summary>
internal static class FakeMerchantEventFrame
{
    private const string SpritePath = "SceneContainer/BgContainer/FakeMerchantBackground";
    private const string ContainerPath = "SceneContainer/BgContainer";
    private const string CutterPath = "SceneContainer/SpineSprite2";

    /// <summary>
    /// Canvas pixels to move the whole background (and its decorations) to the right after the
    /// cover fit. The redrawn tent's art stops short of the skeleton bounds on the right, which
    /// left a pale strip on screen; 41 px pushes exactly that strip off the screen edge.
    /// </summary>
    private const float HorizontalShift = 41f;

    private const int MaxWaitFrames = 300;
    private const float MinArtSize = 8f;

    public static void Schedule(NFakeMerchant screen)
    {
        try
        {
            _ = FitAsync(screen);
        }
        catch (Exception exception)
        {
            ModLog.Warn(
                "假商人事件构图补丁未能启动，保持原构图：" +
                exception.GetBaseException().Message);
        }
    }

    private static async Task FitAsync(NFakeMerchant screen)
    {
        try
        {
            var tree = screen.GetTree();
            if (tree == null)
            {
                return;
            }

            var sprite = screen.GetNodeOrNull<Node2D>(SpritePath);
            var container = screen.GetNodeOrNull<Control>(ContainerPath);
            var cutter = screen.GetNodeOrNull<Node2D>(CutterPath);
            if (sprite == null || container == null)
            {
                ModLog.Warn($"假商人事件场景缺少 {SpritePath}，跳过构图调整。");
                return;
            }

            // The SpineSprite builds its skeleton asynchronously after _Ready, so the bounds are
            // only meaningful once an animation pose has been applied.
            var bounds = default(Rect2);
            var ready = false;
            for (var frame = 0; frame < MaxWaitFrames; frame++)
            {
                if (!GodotObject.IsInstanceValid(screen) || !GodotObject.IsInstanceValid(sprite))
                {
                    return;
                }

                var candidate = ReadBounds(sprite);
                if (candidate.Size.X > MinArtSize && candidate.Size.Y > MinArtSize)
                {
                    bounds = candidate;
                    ready = true;
                    break;
                }

                await screen.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            }

            if (!ready)
            {
                ModLog.Warn($"等不到假商人帐篷的骨架范围（{MaxWaitFrames} 帧），保持原构图。");
                return;
            }

            // 坐标校正只服务于 NSFW-Event（工坊 3765802910）那张帐篷美术的构图：
            // 没装这个包时保持原版构图，其余逻辑（美术归属判定等）不受影响。
            // 判据用「游戏自己记下的启用/加载列表」，这样本地手动装进 mods 的玩家也算装；
            // 只看磁盘目录会把「已取消订阅但文件残留」误判成已安装。
            static bool HasNsfwEventPack()
            {
                var steamRoot = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                    "SlayTheSpire2", "steam");
                if (System.IO.Directory.Exists(steamRoot))
                {
                    foreach (var settings in System.IO.Directory.EnumerateFiles(steamRoot, "settings.save", System.IO.SearchOption.AllDirectories))
                    {
                        try
                        {
                            var text = System.IO.File.ReadAllText(settings);
                            if (text.Contains("NSFW-Event", StringComparison.OrdinalIgnoreCase) ||
                                text.Contains("3765802910", StringComparison.Ordinal))
                            {
                                return true;
                            }
                        }
                        catch
                        {
                            // 读不到就当没装，宁可不动构图。
                        }
                    }
                }

                // 兜底：本地 mods 目录里存在该包的清单文件。
                foreach (var modsRoot in new[] { System.IO.Path.Combine(AppContext.BaseDirectory, "mods"), @"D:\steam\steamapps\common\Slay the Spire 2\mods" })
                {
                    if (!System.IO.Directory.Exists(modsRoot))
                    {
                        continue;
                    }

                    if (System.IO.Directory.EnumerateFiles(modsRoot, "NSFW-Event.json", System.IO.SearchOption.AllDirectories).Any())
                    {
                        return true;
                    }
                }

                return false;
            }
            if (!HasNsfwEventPack())
            {
                ModLog.Info("假商人帐篷坐标校正仅对 NSFW-Event（3765802910）启用，当前未检测到该包，保持原构图。");
                return;
            }

            // Fit to the screen's own rect instead of the viewport: the same scene is also
            // instantiated as a small compendium preview, and that one must fill its own box.
            var screenScale = screen.GetGlobalTransform().Scale;
            var target = new Rect2(screen.GetGlobalPosition(), screen.Size * screenScale);
            if (target.Size.X < 64f || target.Size.Y < 64f)
            {
                ModLog.Warn($"假商人事件画面尺寸异常（{target.Size}），保持原构图。");
                return;
            }

            var spriteScale = sprite.Scale;
            if (spriteScale.X <= 0.0001f || spriteScale.Y <= 0.0001f)
            {
                spriteScale = Vector2.One;
            }

            var localPosition = sprite.Position + bounds.Position * spriteScale;
            var localSize = bounds.Size * spriteScale;
            if (localSize.X < MinArtSize || localSize.Y < MinArtSize ||
                localSize.X > 100000f || localSize.Y > 100000f)
            {
                ModLog.Warn($"假商人帐篷范围异常（{localSize}），保持原构图。");
                return;
            }

            // Cover fit + horizontal shift; vertical framing stays untouched.
            var scale = Mathf.Max(target.Size.X / localSize.X, target.Size.Y / localSize.Y);
            var localCenter = localPosition + localSize * 0.5f;
            var origin = target.Position + target.Size * 0.5f - localCenter * scale +
                         new Vector2(HorizontalShift, 0f);

            // The tent, its glitter/fire/speck decorations all live in this container, so moving
            // and scaling the container keeps the composition intact.
            ApplyGlobalTransform(
                container,
                new Transform2D(0f, new Vector2(scale, scale), 0f, origin));

            if (cutter != null && GodotObject.IsInstanceValid(cutter))
            {
                // The front "cutter" layer sits outside the container but shares the tent's
                // skeleton space, so it gets the tent's own skeleton-space mapping.
                var cutterScale = new Vector2(
                    scale * spriteScale.X,
                    scale * spriteScale.Y);
                ApplyGlobalTransform(cutter, new Transform2D(0f, cutterScale, 0f, origin));
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn(
                "假商人事件构图调整失败，保持游戏原构图：" +
                exception.GetBaseException().Message);
        }
    }

    private static Rect2 ReadBounds(Node2D sprite)
    {
        try
        {
            var skeleton = new MegaSprite(sprite).GetSkeleton();
            return skeleton?.GetBounds() ?? default;
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// CanvasItem's global transform is read-only in Godot 4, so convert the wanted global mapping
    /// into the node's own local transform and set that.
    /// </summary>
    private static void ApplyGlobalTransform(CanvasItem item, Transform2D global)
    {
        var parentGlobal = (item.GetParent() as CanvasItem)?.GetGlobalTransform() ?? Transform2D.Identity;
        var local = parentGlobal.AffineInverse() * global;
        switch (item)
        {
            case Control control:
                control.Scale = local.Scale;
                control.Position = local.Origin;
                break;
            case Node2D node:
                node.Transform = local;
                break;
        }
    }
}

[HarmonyPatch(typeof(NFakeMerchant), "_Ready")]
internal static class FakeMerchantEventFramePatch
{
    private static void Postfix(NFakeMerchant __instance) => FakeMerchantEventFrame.Schedule(__instance);
}
