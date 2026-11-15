using Godot;

namespace STS2SkinChanger.Core;

/// <summary>
/// 仅用于自动化复测：外部往 %TEMP%\sts2_snap.req 写文件后，把当前视口画面存成 PNG。
/// Vulkan 下外部截图只能拿到黑屏，但从引擎内部读视口纹理始终有效。
/// 注意：运行时加载的 Mod DLL 里的 Node 虚方法（_Ready/_Process）不会被引擎回调，
/// 因此这里挂在 SceneTree.ProcessFrame 原生信号上，不依赖虚方法派发。
/// </summary>
internal static class DebugSnapshot
{
    private static readonly string RequestPath =
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sts2_snap.req");

    private static bool _installed;

    internal static void EnsureInstalled()
    {
        if (_installed)
        {
            return;
        }

        // 交付版本默认完全关闭：只有显式带 STS2SC_DEBUG=1 启动游戏时才开放截图与下拉切换通道，
        // 不留任何能被外部文件驱动的调试后门。
        if (!string.Equals(
                System.Environment.GetEnvironmentVariable("STS2SC_DEBUG"),
                "1",
                System.StringComparison.Ordinal))
        {
            _installed = true;
            return;
        }

        try
        {
            if (Engine.GetMainLoop() is not SceneTree tree)
            {
                return;
            }

            _installed = true;
            tree.ProcessFrame += OnFrame;
            ModLog.Info("[调试] 截图钩子已接入 ProcessFrame。");
        }
        catch (System.Exception ex)
        {
            ModLog.Warn($"[调试] 接入截图钩子失败：{ex.GetType().Name}: {ex.Message}");
        }
    }

    private static bool _pending;

    private static readonly string WatchPath =
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sts2_watch.txt");

    private static long _watchUntilMs;
    private static long _watchNextMs;
    private static bool _watchLoggedTree;

    /// <summary>
    /// 自动化用：写 "%TEMP%\sts2_watch.txt"，内容是监视秒数。期间每 ~80 ms 把场上每个
    /// 召唤物的根节点/视觉节点/血条坐标写进 "%TEMP%\sts2ctl\watch.log"，用于判断
    /// 「模型实际坐标在动」到底动的是哪一层。
    /// </summary>
    private static void PollWatch()
    {
        if (System.IO.File.Exists(WatchPath))
        {
            try
            {
                var text = System.IO.File.ReadAllText(WatchPath).Trim();
                System.IO.File.Delete(WatchPath);
                if (double.TryParse(text, out var seconds) && seconds > 0)
                {
                    _watchUntilMs = System.Environment.TickCount64 + (long)(seconds * 1000);
                    _watchNextMs = 0;
                    _watchLoggedTree = false;
                    ModLog.Info($"[调试] 开始监视召唤物坐标 {seconds:F1} 秒。");
                }
            }
            catch
            {
                // 下一帧再试。
            }
        }

        if (_watchUntilMs == 0 || System.Environment.TickCount64 > _watchUntilMs)
        {
            return;
        }

        var now = System.Environment.TickCount64;
        if (now < _watchNextMs)
        {
            return;
        }

        _watchNextMs = now + 80;
        try
        {
            var room = MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance;
            if (room == null)
            {
                return;
            }

            var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sts2ctl");
            System.IO.Directory.CreateDirectory(directory);
            var file = System.IO.Path.Combine(directory, "watch.log");
            var builder = new System.Text.StringBuilder();
            if (!_watchLoggedTree)
            {
                _watchLoggedTree = true;
                foreach (var creature in room.CreatureNodes)
                {
                    if (creature.Entity.PetOwner == null)
                    {
                        continue;
                    }

                    builder.AppendLine($"# 节点树 {creature.Entity.ModelId.Entry}");
                    DescribeTree(creature, builder, 0);
                }
            }

            foreach (var creature in room.CreatureNodes)
            {
                if (creature.Entity.PetOwner == null)
                {
                    continue;
                }

                var visuals = creature.Visuals;
                var bounds = creature.GetNodeOrNull<Node2D>("%Bounds");
                builder.AppendLine(
                    $"t={now} {creature.Entity.ModelId.Entry} root=({creature.Position.X:F1},{creature.Position.Y:F1}) " +
                    $"visuals=({visuals.Position.X:F1},{visuals.Position.Y:F1}) vscale=({visuals.Scale.X:F2},{visuals.Scale.Y:F2}) " +
                    $"bounds={(bounds == null ? "-" : $"({bounds.Position.X:F1},{bounds.Position.Y:F1})")} " +
                    $"hpbar={DescribeHealthBar(creature)}");
            }

            System.IO.File.AppendAllText(file, builder.ToString());
        }
        catch (System.Exception ex)
        {
            ModLog.Warn($"[调试] 监视召唤物坐标失败：{ex.GetType().Name}: {ex.Message}");
            _watchUntilMs = 0;
        }
    }

    private static string DescribeHealthBar(Node creature)
    {
        foreach (var child in creature.GetChildren())
        {
            if (child is Node2D node && child.Name.ToString().Contains("Health", StringComparison.OrdinalIgnoreCase))
            {
                return $"{child.Name}({node.Position.X:F1},{node.Position.Y:F1}|g{node.GlobalPosition.X:F1},{node.GlobalPosition.Y:F1})";
            }
        }

        var named = creature.GetNodeOrNull<Control>("%HealthBar") ?? creature.GetNodeOrNull<Control>("%HealthDisplay");
        if (named != null)
        {
            return $"{named.Name}({named.Position.X:F1},{named.Position.Y:F1}|g{named.GlobalPosition.X:F1},{named.GlobalPosition.Y:F1})";
        }

        return "-";
    }

    private static void DescribeTree(Node node, System.Text.StringBuilder builder, int depth)
    {
        var position = node is Node2D node2D
            ? $" pos=({node2D.Position.X:F1},{node2D.Position.Y:F1})"
            : node is Control control
                ? $" pos=({control.Position.X:F1},{control.Position.Y:F1})"
                : string.Empty;
        builder.AppendLine($"{new string(' ', depth * 2)}{node.Name} [{node.GetType().Name}]{position}");
        if (depth >= 3)
        {
            return;
        }

        foreach (var child in node.GetChildren())
        {
            DescribeTree(child, builder, depth + 1);
        }
    }

    private static void OnFrame()
    {
        PollPick();
        PollWatch();

        if (_pending || !System.IO.File.Exists(RequestPath))
        {
            return;
        }

        _pending = true;
        try
        {
            System.IO.File.Delete(RequestPath);
        }
        catch
        {
            // 删不掉也继续，下一帧再试。
        }

        // 视口纹理只有在一帧画完之后才有内容，必须等 FramePostDraw。
        RenderingServer.Singleton.FramePostDraw += OnPostDraw;
    }

    private static readonly string PickPath =
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sts2_pick.txt");

    /// <summary>
    /// 自动化用：写 "%TEMP%\sts2_pick.txt"，内容 "<分组>|<选项ID>"，等价于在局内外观面板里
    /// 点那一条皮肤（含独立召唤物 Mod 的身体重建与重新套用），用于无人值守复现切换问题。
    /// </summary>
    private static void PollPick()
    {
        if (!System.IO.File.Exists(PickPath))
        {
            return;
        }

        string request;
        try
        {
            request = System.IO.File.ReadAllText(PickPath);
            System.IO.File.Delete(PickPath);
        }
        catch
        {
            return;
        }

        var parts = request.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length < 2 || parts[0].Length == 0 || parts[1].Length == 0)
        {
            ModLog.Warn("[调试] 切换请求格式不对，应为 <分组>|<选项ID>：" + request);
            return;
        }

        var groupId = parts[0];
        var optionId = parts[1];
        try
        {
            SkinService.ClearCompanionRandom();
            var impostor = OstyImpostorBridge.IsImpostorOption(optionId);
            if (!SkinService.ApplyCompanionSelection(groupId, optionId))
            {
                ModLog.Warn($"[调试] 切换失败 {groupId} → {optionId}：{SkinService.LastError}");
                return;
            }

            if (impostor)
            {
                Ui.CharacterAppearanceRuntime.RebuildSummonBodyForImpostor(groupId);
                SkinService.ReapplyCompanionImpostorSkin();
            }
            else
            {
                Ui.CharacterAppearanceRuntime.RefreshCompanionGroup(groupId);
                if (!string.IsNullOrEmpty(SkinService.Config.CompanionImpostorSkinId))
                {
                    Ui.CharacterAppearanceRuntime.RebuildSummonBodyForImpostor(groupId);
                }

                SkinService.ReapplyCompanionImpostorSkin();
            }

            ModLog.Info(
                $"[调试] 已切换 {groupId} → {optionId}（配置={SkinService.Config.GetSelection(groupId)}，独立Mod={SkinService.Config.CompanionImpostorSkinId ?? "<无>"}）");
        }
        catch (System.Exception ex)
        {
            ModLog.Warn($"[调试] 切换异常 {groupId} → {optionId}：{ex.GetBaseException().Message}");
        }
    }

    private static void OnPostDraw()
    {
        RenderingServer.Singleton.FramePostDraw -= OnPostDraw;
        _pending = false;
        try
        {
            var tree = (SceneTree)Engine.GetMainLoop();
            var image = tree.Root.GetTexture().GetImage();
            var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sts2ctl");
            System.IO.Directory.CreateDirectory(directory);
            var file = System.IO.Path.Combine(directory, "snap.png");
            image.SavePng(file);
            ModLog.Info($"[调试] 截图已保存：{file}；场景={tree.CurrentScene?.SceneFilePath ?? "(无)"}；尺寸={image.GetWidth()}x{image.GetHeight()}");
        }
        catch (System.Exception ex)
        {
            ModLog.Warn($"[调试] 截图失败：{ex.GetType().Name}: {ex.Message}");
        }
    }
}
