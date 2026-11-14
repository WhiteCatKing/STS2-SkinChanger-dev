#nullable disable
using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Backgrounds;
using MegaCrit.Sts2.Core.Rooms;

namespace STS2SkinChanger.Ui.EmperorCrabCompat
{
    /// <summary>
    /// 局内帝皇蟹背景补丁（保守版）。
    ///
    /// 战斗房间的背景由 NCombatBackground.Create(BackgroundAssets) 创建。
    /// 对帝皇蟹来说，背景场景 kaiser_crab_boss_background.tscn 里实例化了
    /// %KaiserCrab（kaiser_crab_boss_setup.tscn），这就是战斗中可见的 boss 本体。
    ///
    /// 本补丁只对“帝皇蟹背景场景”接管 Create：
    /// - 用 SkinChanger 的运行时资源包生成一份皮肤版背景场景；
    /// - 按原版流程调用 SetLayers 填充背景层；
    /// - 然后返回给游戏，后续 AddChild、可见性、层级全部还是原版逻辑。
    ///
    /// 不监听换肤事件、不替换战斗节点、不触碰卡牌和行动队列。
    /// </summary>
    internal static class CombatCrabSkinPatch
    {
        private const string SkinChangerAssemblyName = "Gurio.SkinChanger";
        private const string SkinGroupId = "kaiser_crab_boss_setup";
        private const string BackgroundScenePath =
            "res://scenes/backgrounds/kaiser_crab_boss/kaiser_crab_boss_background.tscn";

        private static readonly object Sync = new object();
        private static bool _installed;
        private static bool _skinReflectionReady;
        private static MethodInfo _getRuntimeSceneMethod;
        private static MethodInfo _setLayersMethod;

        public static void Install(Harmony harmony)
        {
            lock (Sync)
            {
                if (_installed)
                {
                    return;
                }

                try
                {
                    MethodInfo create = AccessTools.Method(typeof(NCombatBackground), "Create");
                    if (create == null)
                    {
                        GD.PrintErr("[EmperorCrabSkinFix] 找不到 NCombatBackground.Create");
                        return;
                    }

                    _setLayersMethod = AccessTools.Method(typeof(NCombatBackground), "SetLayers");
                    if (_setLayersMethod == null)
                    {
                        GD.PrintErr("[EmperorCrabSkinFix] 找不到 NCombatBackground.SetLayers");
                        return;
                    }

                    harmony.Patch(
                        create,
                        prefix: new HarmonyMethod(
                            typeof(CombatCrabSkinPatch).GetMethod(
                                nameof(BeforeCreate),
                                BindingFlags.Static | BindingFlags.NonPublic)),
                        postfix: new HarmonyMethod(
                            typeof(CombatCrabSkinPatch).GetMethod(
                                nameof(AfterCreate),
                                BindingFlags.Static | BindingFlags.NonPublic)));

                    _installed = true;
                    GD.Print("[EmperorCrabSkinFix] 已挂上局内帝皇蟹背景补丁");
                }
                catch (Exception ex)
                {
                    GD.PrintErr("[EmperorCrabSkinFix] 安装局内帝皇蟹背景补丁失败：" + ex);
                }
            }
        }

        /// <summary>
        /// 返回 false 表示跳过原版 Create，直接用我们构造的皮肤版背景。
        /// 只有帝皇蟹背景才会接管；其他战斗一律走原版。
        /// </summary>
        private static bool BeforeCreate(BackgroundAssets bg, ref NCombatBackground __result)
        {
            try
            {
                if (bg == null ||
                    !string.Equals(bg.BackgroundScenePath, BackgroundScenePath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                EnsureSkinChangerReflection();
                if (!_skinReflectionReady)
                {
                    GD.PrintErr("[EmperorCrabSkinFix] SkinService 资源包接口不可用，回退原版帝皇蟹背景");
                    return true;
                }

                PackedScene scene = _getRuntimeSceneMethod.Invoke(
                    null,
                    new object[] { SkinGroupId, BackgroundScenePath }) as PackedScene;

                if (scene == null)
                {
                    GD.PrintErr("[EmperorCrabSkinFix] 无法生成帝皇蟹皮肤背景场景，group=" + SkinGroupId);
                    return true;
                }

                NCombatBackground background =
                    scene.Instantiate<NCombatBackground>(PackedScene.GenEditState.Disabled);

                if (background == null)
                {
                    GD.PrintErr("[EmperorCrabSkinFix] 实例化帝皇蟹皮肤背景失败");
                    return true;
                }

                // 原版 Create 里紧接着会调 SetLayers(bg)，这里提前做完。
                _setLayersMethod.Invoke(background, new object[] { bg });
                __result = background;

                // 重建出来的 %KaiserCrab 会在自己的 _Ready 里 SetVisible(false)。
                // 战斗中途换肤没有“开场动画”再把它显示出来，所以这里在背景 Ready
                // 之后强制恢复可见；只影响帝皇蟹背景。
                background.Ready += () =>
                {
                    ForceCrabVisible(background);
                };

                GD.Print("[EmperorCrabSkinFix] 已用皮肤版场景创建局内帝皇蟹背景，group=" + SkinGroupId);

                // 有的 Harmony 配置下 prefix 跳过原方法后 postfix 不一定会跑，
                // 这里也排一次引用更新，确保 Rocket/Crusher 不会继续拿旧背景。
                Callable.From(() =>
                {
                    NKaiserCrabBossBackground crab =
                        background.GetNodeOrNull<NKaiserCrabBossBackground>("%KaiserCrab");
                    if (crab != null)
                    {
                        UpdateMonsterBackgroundReferences(crab);
                    }

                    // 皮肤行为 DLL 可能刚被 SkinChanger 载入，这里补一次安装。
                    HalfHpCrabSkinPatch.TryInstallHalfHpPrefix();
                }).CallDeferred();

                return false;
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 创建皮肤版帝皇蟹背景失败，回退原版：" + ex);
                return true;
            }
        }

        private static void ForceCrabVisible(NCombatBackground background)
        {
            if (background == null || !GodotObject.IsInstanceValid(background))
            {
                return;
            }

            NKaiserCrabBossBackground crab =
                background.GetNodeOrNull<NKaiserCrabBossBackground>("%KaiserCrab");

            if (crab == null)
            {
                return;
            }

            crab.Visible = true;
            GD.Print("[EmperorCrabSkinFix] 已恢复局内帝皇蟹背景可见性");

            Callable.From(() =>
            {
                if (GodotObject.IsInstanceValid(crab))
                {
                    crab.Visible = true;
                }
            }).CallDeferred();
        }

        private static void AfterCreate(NCombatBackground __result)
        {
            try
            {
                if (__result == null || !GodotObject.IsInstanceValid(__result))
                {
                    return;
                }

                NKaiserCrabBossBackground crab =
                    __result.GetNodeOrNull<NKaiserCrabBossBackground>("%KaiserCrab");

                if (crab == null)
                {
                    return;
                }

                // Create 返回时背景还没挂到房间里，延迟一帧等 CreatureNodes 稳定后再改引用。
                Callable.From(() =>
                {
                    UpdateMonsterBackgroundReferences(crab);
                    HalfHpCrabSkinPatch.TryInstallHalfHpPrefix();
                }).CallDeferred();
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 更新帝皇蟹背景引用失败：" + ex);
            }
        }

        /// <summary>
        /// Rocket / Crusher 会把 %KaiserCrab 缓存在私有字段 _background 里。
        /// 背景被重建后必须把引用指向新节点，否则受击/死亡动画访问旧节点会抛
        /// ObjectDisposedException，并打断卡牌动作。
        /// </summary>
        private static void UpdateMonsterBackgroundReferences(NKaiserCrabBossBackground crab)
        {
            if (crab == null || !GodotObject.IsInstanceValid(crab))
            {
                return;
            }

            NCombatRoom room = NCombatRoom.Instance;
            if (room == null || !GodotObject.IsInstanceValid(room))
            {
                return;
            }

            foreach (NCreature creature in room.CreatureNodes)
            {
                if (creature == null || !GodotObject.IsInstanceValid(creature))
                {
                    continue;
                }

                MonsterModel monster = creature.Entity != null ? creature.Entity.Monster : null;
                if (monster == null)
                {
                    continue;
                }

                string typeName = monster.GetType().Name;
                if (typeName != "Rocket" && typeName != "Crusher")
                {
                    continue;
                }

                FieldInfo field = AccessTools.Field(monster.GetType(), "_background");
                if (field != null && field.FieldType == typeof(NKaiserCrabBossBackground))
                {
                    field.SetValue(monster, crab);
                    GD.Print("[EmperorCrabSkinFix] 已更新 " + typeName + " 的背景引用");
                }
            }
        }

        private static void EnsureSkinChangerReflection()
        {
            if (_skinReflectionReady)
            {
                return;
            }

            try
            {
                Assembly skinChanger = null;
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == SkinChangerAssemblyName)
                    {
                        skinChanger = assembly;
                        break;
                    }
                }

                if (skinChanger == null)
                {
                    return;
                }

                Type skinServiceType = skinChanger.GetType("STS2SkinChanger.Core.SkinService", false, false);
                if (skinServiceType == null)
                {
                    return;
                }

                _getRuntimeSceneMethod = skinServiceType.GetMethod(
                    "GetOrLoadRuntimeScene",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(string), typeof(string) },
                    null);

                _skinReflectionReady = _getRuntimeSceneMethod != null;
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 初始化 SkinService 反射失败：" + ex.Message);
            }
        }
    }
}


