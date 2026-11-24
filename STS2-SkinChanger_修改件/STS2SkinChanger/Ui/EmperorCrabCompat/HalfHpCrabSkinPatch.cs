#nullable disable
using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Backgrounds;
using STS2SkinChanger.Core;
using FileAccess = Godot.FileAccess;

namespace STS2SkinChanger.Ui.EmperorCrabCompat
{
    /// <summary>
    /// 帝皇蟹「半血爆衣」皮肤兼容补丁。
    ///
    /// 半血爆衣由皮肤自带的 erowatcher 行为补丁在 Creature.set_CurrentHp 后处理：
    /// 它固定加载 res://.../kaiser_crab_skeleton_data_hurt.tres，再 set_skeleton_data_res。
    /// 但 SkinChanger 会把自己皮肤包里的骨架/图集隔离成独立资源，于是这一步实际拿到的是
    /// “基础游戏的蟹骨架（有 arm_l_bottom）+ 皮肤的爆衣图集（没有该区域）”，
    /// Spine 读不到贴图 → 纹理整块消失；同时换骨架是异步的，
    /// 紧接着 Rocket.AfterCurrentHpChanged 调 PlayHurtAnim 时 GetAnimationState() 还没就绪，
    /// 抛出的异常会打断伤害结算，表现就是卡牌卡在半空。
    ///
    /// 这里做两件事：
    /// 1. 用“当前皮肤自己的骨架 + 皮肤包里的爆衣图集”重新组装 SkeletonDataResource，
    ///    替换掉那个坏掉的换装；两张图集的区域布局完全一致，只换贴图页。
    /// 2. 给帝皇蟹背景的受伤/死亡动画加安全阀：Spine 没就绪时延后到就绪再播，绝不再抛异常。
    /// </summary>
    internal static class HalfHpCrabSkinPatch
    {
        private const string SkinGroupId = "kaiser_crab_boss_setup";
        private const string HurtAtlasPath = "res://animations/monsters/kaiser_crab/kaiser_crab_hurt.atlas";
        private const string HurtDataPath = "res://animations/monsters/kaiser_crab/kaiser_crab_skeleton_data_hurt.tres";
        private const string AppliedMeta = "EmperorCrabSkinFix.HalfHpApplied";
        private const string NoHalfHpResourcesMeta = "EmperorCrabSkinFix.NoHalfHpResources";
        private const string TargetModToken = "erowatcher";
        private const string HalfHpPatchTypeName = "erowatcher.script.patch.MonsterHurtSpinePatch";
        private const string HalfHpSettingsTypeName = "erowatcher.script.card.WatcherCardArtSettings";
        private const string SkinChangerAssemblyName = "Gurio.SkinChanger";

        private static Harmony _harmony;
        private static FieldInfo _animControllerField;
        private static MethodInfo _getRuntimeResourceMethod;
        private static readonly MethodInfo SetAnimationMethod = typeof(MegaAnimationState).GetMethod(
            "SetAnimation",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(string), typeof(bool), typeof(int) },
            null);
        private static bool _animationGuardsInstalled;
        private static bool _halfHpPrefixInstalled;
        private static bool _halfHpPrefixMissing;

        private static SceneTree _retryTree;
        private static Action _retryHandler;
        private static int _retryFrames;

        public static void Install(Harmony harmony)
        {
            _harmony = harmony;
            _animControllerField = AccessTools.Field(typeof(NKaiserCrabBossBackground), "_animController");
            InstallAnimationGuards();
            TryInstallHalfHpPrefix();
            if (!_halfHpPrefixInstalled && !_halfHpPrefixMissing)
            {
                if (IsTargetModLoaded())
                {
                    StartInstallRetry();
                }
                else
                {
                    GD.Print("[EmperorCrabSkinFix] 未检测到目标 Mod（erowatcher），跳过半血爆衣 5 分钟重试");
                }
            }
        }

        internal static bool IsTargetModLoaded(
            IEnumerable<string> loadedAssemblyNames = null,
            IEnumerable<string> loadedModTokens = null,
            bool? hasPatchType = null)
        {
            if (hasPatchType ?? (FindType(HalfHpPatchTypeName) != null))
            {
                return true;
            }

            if (loadedAssemblyNames != null)
            {
                if (MatchesTargetToken(loadedAssemblyNames))
                {
                    return true;
                }
            }
            else
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (IsTargetModToken(assembly.GetName().Name))
                    {
                        return true;
                    }
                }
            }

            if (loadedModTokens != null)
            {
                return MatchesTargetToken(loadedModTokens);
            }

            try
            {
                FieldInfo modAssemblyField = AccessTools.Field(typeof(Mod), "assembly");
                foreach (Mod mod in ModManager.Mods)
                {
                    // Mods later in the load order are still ModLoadState.None when this initializer runs;
                    // only an explicitly disabled mod is "not installed".
                    if (mod == null || mod.state == ModLoadState.Disabled)
                    {
                        continue;
                    }

                    Assembly modAssembly = modAssemblyField?.GetValue(mod) as Assembly;
                    if (IsTargetModToken(mod.manifest?.id) ||
                        IsTargetModToken(mod.manifest?.name) ||
                        IsTargetModToken(mod.path) ||
                        IsTargetModToken(modAssembly?.GetName().Name))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // ModManager 尚未初始化或离线测试无宿主环境，按未加载处理。
            }

            return false;
        }

        private static bool MatchesTargetToken(IEnumerable<string> tokens)
        {
            foreach (string token in tokens)
            {
                if (IsTargetModToken(token))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTargetModToken(string token) =>
            !string.IsNullOrWhiteSpace(token) &&
            token.IndexOf(TargetModToken, StringComparison.OrdinalIgnoreCase) >= 0;

        // ===================== 动画安全阀 =====================

        private static void InstallAnimationGuards()
        {
            if (_animationGuardsInstalled || _harmony == null)
            {
                return;
            }

            try
            {
                MethodInfo hurt = AccessTools.Method(typeof(NKaiserCrabBossBackground), "PlayHurtAnim");
                MethodInfo armDeath = AccessTools.Method(typeof(NKaiserCrabBossBackground), "PlayArmDeathAnim");
                MethodInfo bodyDeath = AccessTools.Method(typeof(NKaiserCrabBossBackground), "PlayBodyDeathAnim");

                if (hurt == null || armDeath == null || bodyDeath == null)
                {
                    GD.PrintErr("[EmperorCrabSkinFix] 找不到帝皇蟹背景动画方法，跳过安全阀");
                    return;
                }

                _harmony.Patch(
                    hurt,
                    prefix: new HarmonyMethod(
                        typeof(HalfHpCrabSkinPatch).GetMethod(
                            nameof(BeforeHurtAnim),
                            BindingFlags.Static | BindingFlags.NonPublic)));

                _harmony.Patch(
                    armDeath,
                    prefix: new HarmonyMethod(
                        typeof(HalfHpCrabSkinPatch).GetMethod(
                            nameof(BeforeArmDeathAnim),
                            BindingFlags.Static | BindingFlags.NonPublic)));

                _harmony.Patch(
                    bodyDeath,
                    prefix: new HarmonyMethod(
                        typeof(HalfHpCrabSkinPatch).GetMethod(
                            nameof(BeforeBodyDeathAnim),
                            BindingFlags.Static | BindingFlags.NonPublic)));

                _animationGuardsInstalled = true;
                GD.Print("[EmperorCrabSkinFix] 已挂上帝皇蟹背景动画安全阀");
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 安装背景动画安全阀失败：" + ex);
            }
        }

        private static bool BeforeHurtAnim(
            NKaiserCrabBossBackground __instance,
            NKaiserCrabBossBackground.ArmSide side)
        {
            return DeferIfSpineNotReady(__instance, background => background.PlayHurtAnim(side));
        }

        private static bool BeforeArmDeathAnim(
            NKaiserCrabBossBackground __instance,
            NKaiserCrabBossBackground.ArmSide side)
        {
            return DeferIfSpineNotReady(__instance, background => background.PlayArmDeathAnim(side));
        }

        private static bool BeforeBodyDeathAnim(NKaiserCrabBossBackground __instance)
        {
            return DeferIfSpineNotReady(__instance, background => background.PlayBodyDeathAnim());
        }

        /// <summary>
        /// 返回 true 继续原方法；返回 false 表示已经延后处理。
        /// </summary>
        private static bool DeferIfSpineNotReady(
            NKaiserCrabBossBackground background,
            Action<NKaiserCrabBossBackground> action)
        {
            try
            {
                if (background == null || !GodotObject.IsInstanceValid(background))
                {
                    return true;
                }

                MegaSprite sprite = _animControllerField?.GetValue(background) as MegaSprite;
                if (sprite == null ||
                    sprite.BoundObject == null ||
                    !GodotObject.IsInstanceValid(sprite.BoundObject))
                {
                    return true;
                }

                if (sprite.IsAnimationStateReady())
                {
                    return true;
                }

                GD.Print("[EmperorCrabSkinFix] 帝皇蟹 Spine 尚未就绪，延后播放动画");
                SpineNodeExtensions.RunWhenSpineReady(background, sprite, _ =>
                {
                    try
                    {
                        if (GodotObject.IsInstanceValid(background))
                        {
                            action(background);
                        }
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr("[EmperorCrabSkinFix] 延后播放帝皇蟹动画失败：" + ex);
                    }
                });

                return false;
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 检查帝皇蟹 Spine 就绪状态失败：" + ex.Message);
                return true;
            }
        }

        // ===================== 接管半血换装 =====================

        public static void TryInstallHalfHpPrefix()
        {
            if (_halfHpPrefixInstalled || _halfHpPrefixMissing || _harmony == null)
            {
                return;
            }

            try
            {
                Type patchType = FindType(HalfHpPatchTypeName);
                if (patchType == null)
                {
                    // 皮肤行为 DLL 还没被 SkinChanger 载入，等下一次重试。
                    return;
                }

                MethodInfo postfix = AccessTools.Method(patchType, "Postfix", new[] { typeof(Creature) });
                if (postfix == null)
                {
                    _halfHpPrefixMissing = true;
                    GD.PrintErr("[EmperorCrabSkinFix] 找不到 MonsterHurtSpinePatch.Postfix(Creature)");
                    return;
                }

                _harmony.Patch(
                    postfix,
                    prefix: new HarmonyMethod(
                        typeof(HalfHpCrabSkinPatch).GetMethod(
                            nameof(BeforeHalfHpPostfix),
                            BindingFlags.Static | BindingFlags.NonPublic)));

                _halfHpPrefixInstalled = true;
                StopInstallRetry();
                GD.Print("[EmperorCrabSkinFix] 已接管帝皇蟹半血爆衣换装");
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 接管半血爆衣换装失败：" + ex.Message);
            }
        }

        private static bool BeforeHalfHpPostfix(Creature __instance)
        {
            try
            {
                if (!ShouldHandle(__instance))
                {
                    return true;
                }

                // 不管我们这次能不能成功换装，都不能再让原补丁执行：
                // 它固定用“基础蟹骨架 + 皮肤爆衣图集”的组合，必定贴图全丢。
                // 换装失败时保持当前皮肤正常外观，至少不会有视觉/卡牌问题。
                ApplySkinHalfHp(__instance);
                return false;
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 半血爆衣处理异常，跳过原补丁：" + ex);
                return false;
            }
        }

        private static bool ShouldHandle(Creature creature)
        {
            if (creature == null || creature.Monster == null || creature.Player != null)
            {
                return false;
            }

            if (creature.MaxHp <= 0 || creature.IsDead)
            {
                return false;
            }

            string entry = creature.Monster.Id.Entry;
            if (entry != "CRUSHER" && entry != "ROCKET")
            {
                return false;
            }

            if (creature.CurrentHp > creature.MaxHp / 2)
            {
                return false;
            }

            return IsHurtSkinsEnabled();
        }

        private static bool IsHurtSkinsEnabled()
        {
            try
            {
                Type settingsType = FindType(HalfHpSettingsTypeName);
                FieldInfo field = settingsType?.GetField(
                    "EnableHurtSkins",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                if (field != null && field.FieldType == typeof(bool))
                {
                    return (bool)field.GetValue(null);
                }
            }
            catch
            {
                // 忽略，默认按开启处理。
            }

            return true;
        }

        private static bool ApplySkinHalfHp(Creature creature)
        {
            NCombatRoom room = NCombatRoom.Instance;
            NCombatBackground background = room != null && GodotObject.IsInstanceValid(room)
                ? room.Background
                : null;

            if (background == null || !GodotObject.IsInstanceValid(background))
            {
                return false;
            }

            Node crab = background.GetNodeOrNull("%KaiserCrab");
            Node2D visuals = crab?.GetNodeOrNull<Node2D>("Visuals");
            if (visuals == null || !GodotObject.IsInstanceValid(visuals))
            {
                return false;
            }

            if (visuals.HasMeta(AppliedMeta))
            {
                return true;
            }

            if (visuals.HasMeta(NoHalfHpResourcesMeta))
            {
                return false;
            }

            // 当前皮肤根本不含半血爆衣资源：不要再尝试，也不让原补丁去换坏资源。
            if (!FileAccess.FileExists(HurtDataPath) || !FileAccess.FileExists(HurtAtlasPath))
            {
                visuals.SetMeta(NoHalfHpResourcesMeta, true);
                GD.Print("[EmperorCrabSkinFix] 当前皮肤没有半血爆衣资源，保持正常外观");
                return false;
            }

            GodotObject currentData = visuals.Get("skeleton_data_res").AsGodotObject();
            if (currentData == null)
            {
                return false;
            }

            // 当前骨架就是 SkinChanger 隔离后的皮肤骨架，直接复用它。
            GodotObject skinSkeleton = currentData.Get("skeleton_file_res").AsGodotObject();
            if (skinSkeleton == null)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 取不到当前皮肤骨架，跳过半血换装");
                return false;
            }

            GodotObject hurtAtlas = LoadHurtAtlas();
            if (hurtAtlas == null)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 找不到半血爆衣图集，跳过半血换装");
                return false;
            }

            Resource hurtData = BuildHurtSkeletonData(hurtAtlas, skinSkeleton);
            if (hurtData == null)
            {
                return false;
            }

            MegaSprite sprite = new MegaSprite(visuals);
            sprite.SetSkeletonDataRes(new MegaSkeletonDataResource(hurtData));
            visuals.SetMeta(AppliedMeta, true);

            GD.Print("[EmperorCrabSkinFix] 已应用帝皇蟹半血爆衣皮肤");

            // 换骨架后原来的轨道动画会丢，按 NKaiserCrabBossBackground._Ready 的配置重新拉起来。
            SpineNodeExtensions.RunWhenSpineReady(crab, sprite, state =>
            {
                try
                {
                    // Do not bind this call at compile time: SetAnimation's return type differs
                    // between the stable and beta game branches (the same reason the main project
                    // uses ManagedCharacterAnimationBridge). Reflection works on both.
                    SetAnimationMethod?.Invoke(state, ["right/idle_loop", true, 2]);
                    SetAnimationMethod?.Invoke(state, ["body/idle_loop", true, 0]);
                    SetAnimationMethod?.Invoke(state, ["left/idle_loop", true, 1]);
                }
                catch (Exception ex)
                {
                    GD.PrintErr("[EmperorCrabSkinFix] 半血换装后恢复待机动画失败：" + ex.Message);
                }
            });

            return true;
        }

        private static Resource BuildHurtSkeletonData(GodotObject hurtAtlas, GodotObject skinSkeleton)
        {
            // 当前皮肤没有半血爆衣资源时直接放弃，避免让 Godot 打一堆“文件不存在”的错。
            if (!FileAccess.FileExists(HurtDataPath))
            {
                return null;
            }

            Resource hurtData = null;
            try
            {
                // Ignore：不要复用缓存里那个“骨架对不上”的实例。
                hurtData = ResourceLoader.Load<Resource>(HurtDataPath, string.Empty, ResourceLoader.CacheMode.Ignore);
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 加载半血骨架数据失败：" + ex.Message);
            }

            if (hurtData == null)
            {
                return null;
            }

            try
            {
                // 图集用皮肤包的爆衣图集，骨架用当前皮肤自己的骨架，保证 attachment 全部能对上。
                hurtData.Set("atlas_res", hurtAtlas);
                hurtData.Set("skeleton_file_res", skinSkeleton);
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 组装半血骨架数据失败：" + ex.Message);
                return null;
            }

            return hurtData;
        }

        private static GodotObject LoadHurtAtlas()
        {
            // 半血包里的 kaiser_crab_hurt.atlas 没有被 SkinChanger 隔离，
            // 走全局路径拿到的就是皮肤自己的爆衣图集（日志里那次 arm_l_bottom 报错也证明图集本身是皮肤的）。
            try
            {
                Resource resource = ResourceLoader.Load<Resource>(
                    HurtAtlasPath,
                    string.Empty,
                    ResourceLoader.CacheMode.Reuse);

                if (resource != null)
                {
                    return resource;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 直接加载爆衣图集失败：" + ex.Message);
            }

            // 兜底：让 SkinChanger 从当前皮肤会话里取（它会处理隔离后的资源）。
            try
            {
                MethodInfo loader = GetRuntimeResourceMethod();
                if (loader != null)
                {
                    object resource = loader.Invoke(
                        null,
                        new object[] { SkinGroupId, HurtAtlasPath, true });

                    if (resource is GodotObject godotResource && GodotObject.IsInstanceValid(godotResource))
                    {
                        return godotResource;
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] SkinChanger 取爆衣图集失败：" + ex.Message);
            }

            return null;
        }

        private static MethodInfo GetRuntimeResourceMethod()
        {
            if (_getRuntimeResourceMethod != null)
            {
                return _getRuntimeResourceMethod;
            }

            try
            {
                Assembly skinChanger = FindAssembly(SkinChangerAssemblyName);
                Type skinServiceType = skinChanger?.GetType("STS2SkinChanger.Core.SkinService", false, false);
                _getRuntimeResourceMethod = skinServiceType?.GetMethod(
                    "GetOrLoadRuntimeResource",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(string), typeof(string), typeof(bool) },
                    null);
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 反射 SkinService 资源接口失败：" + ex.Message);
            }

            return _getRuntimeResourceMethod;
        }

        // ===================== 延迟安装（皮肤行为 DLL 可能晚于本 Mod 加载） =====================

        private static void StartInstallRetry()
        {
            if (_halfHpPrefixInstalled || _retryTree != null)
            {
                return;
            }

            SceneTree tree = Engine.GetMainLoop() as SceneTree;
            if (tree == null)
            {
                return;
            }

            _retryTree = tree;
            _retryFrames = 0;
            var retrySubscribedFrame = DeferredDispatch.CurrentFrame();

            // [延迟调度/ProcessFrame] 原触发时机：皮肤行为 DLL 未加载时订阅 ProcessFrame 每 60 帧检查一次；
            // 最早可执行帧：订阅后第 60 帧（其后每 60 帧一次）；顺序依赖：主线程扫程序集并装补丁，无同帧穿插要求；
            // 能否合并：单例重试（_retryTree != null 防重）；重试上限：300 次（18000 帧 / 约 5 分钟放弃）；
            // 取消条件：_halfHpPrefixInstalled || _halfHpPrefixMissing || _retryFrames >= 18000 或异常 StopInstallRetry()。
            _retryHandler = () =>
            {
                try
                {
                    if (_halfHpPrefixInstalled || _halfHpPrefixMissing)
                    {
                        StopInstallRetry();
                        return;
                    }

                    if (++_retryFrames % 60 != 0)
                    {
                        return;
                    }

                    DeferredDispatch.RunTrackedPoll(
                        "HalfHpCrabSkinPatch.InstallRetry",
                        "EmperorCrabCompat",
                        () =>
                        {
                            TryInstallHalfHpPrefix();
                            return _halfHpPrefixInstalled || _halfHpPrefixMissing;
                        },
                        DeferredDispatchRisk.Normal,
                        retrySubscribedFrame);

                    // 5 分钟还没等到皮肤行为 DLL 就放弃，避免常驻。
                    if (_retryFrames >= 18000)
                    {
                        StopInstallRetry();
                    }
                }
                catch
                {
                    StopInstallRetry();
                }
            };

            tree.ProcessFrame += _retryHandler;
        }

        private static void StopInstallRetry()
        {
            if (_retryTree != null && _retryHandler != null)
            {
                try { _retryTree.ProcessFrame -= _retryHandler; } catch { }
            }

            _retryTree = null;
            _retryHandler = null;
        }

        private static Assembly FindAssembly(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == name)
                {
                    return assembly;
                }
            }

            return null;
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type type = assembly.GetType(fullName, false, false);
                    if (type != null)
                    {
                        return type;
                    }
                }
                catch
                {
                    // 某些动态程序集不允许 GetType，忽略。
                }
            }

            return null;
        }
    }
}

