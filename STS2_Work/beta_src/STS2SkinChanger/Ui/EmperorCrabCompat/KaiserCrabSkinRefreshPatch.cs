#nullable disable
using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using MegaCrit.Sts2.Core.Nodes.Vfx.Backgrounds;

namespace STS2SkinChanger.Ui.EmperorCrabCompat
{
    /// <summary>
    /// 帝皇蟹（火箭 / 碾碎爪）图鉴皮肤补丁。
    ///
    /// 图鉴里真正显示帝皇蟹身体的是 NBestiaryLayoutKaiserCrab 的 _background
    /// （%KaiserCrab，实例自 res://scenes/creature_visuals/kaiser_crab_boss_setup.tscn）。
    /// 切换图鉴条目时布局会被复用或重新创建，而缓存的布局场景里带的是默认背景，
    /// 所以会出现“换完皮肤，点别的怪再点回来又变回默认”的现象。
    ///
    /// 这里直接在每次 NBestiaryLayoutKaiserCrab.Setup 之后，按当前选择重新实例化
    /// %KaiserCrab，覆盖“切换皮肤”和“点其他怪再回来”两种情况。
    /// 资源仍走 SkinChanger 的运行时资源包，避免 Spine 的 atlas/skel/png 串贴图。
    ///
    /// 注意：整个替换节点时必须把 %KaiserCrab 这个“场景唯一名”重新注册到布局上，
    /// 因为 Rocket/Crusher.Background 的懒加载实现是
    /// NBestiary.Instance.Layout.GetNode("%KaiserCrab")。
    /// 旧节点被移除后如果不补注册，图鉴里所有会走 Background 的动作预览都会抛
    /// “Node not found: %KaiserCrab”，表现就是除“受伤/死亡”外所有动作都点不动。
    /// </summary>
    internal static class KaiserCrabSkinRefreshPatch
    {
        private const string SkinChangerAssemblyName = "Gurio.SkinChanger";
        private const string SetupScenePath = "res://scenes/creature_visuals/kaiser_crab_boss_setup.tscn";
        private const string DefaultBackgroundNodeName = "KaiserCrab";

        private static readonly object Sync = new object();
        private static bool _installed;
        private static bool _skinReflectionReady;
        private static FieldInfo _backgroundField;
        private static FieldInfo _creatureField;
        private static PropertyInfo _catalogProperty;
        private static MethodInfo _findGroupMethod;
        private static MethodInfo _getRuntimeSceneMethod;

        public static void EnsureInstalled(Harmony harmony)
        {
            lock (Sync)
            {
                if (_installed)
                {
                    return;
                }

                try
                {
                    _backgroundField = AccessTools.Field(typeof(NBestiaryLayoutKaiserCrab), "_background");
                    if (_backgroundField == null)
                    {
                        GD.PrintErr("[EmperorCrabSkinFix] 找不到 NBestiaryLayoutKaiserCrab._background");
                        return;
                    }

                    _creatureField = AccessTools.Field(typeof(NBestiaryLayoutKaiserCrab), "_creature");

                    MethodInfo setup = AccessTools.Method(typeof(NBestiaryLayoutKaiserCrab), "Setup");
                    if (setup == null)
                    {
                        GD.PrintErr("[EmperorCrabSkinFix] 找不到 NBestiaryLayoutKaiserCrab.Setup");
                        return;
                    }

                    harmony.Patch(
                        setup,
                        postfix: new HarmonyMethod(
                            typeof(KaiserCrabSkinRefreshPatch).GetMethod(
                                nameof(AfterKaiserCrabSetup),
                                BindingFlags.Static | BindingFlags.NonPublic)));

                    _installed = true;
                    GD.Print("[EmperorCrabSkinFix] 已挂上 NBestiaryLayoutKaiserCrab.Setup 补丁");
                }
                catch (Exception ex)
                {
                    GD.PrintErr("[EmperorCrabSkinFix] 安装补丁失败：" + ex);
                }
            }
        }

        private static void AfterKaiserCrabSetup(NBestiaryLayoutKaiserCrab __instance)
        {
            try
            {
                if (__instance == null ||
                    !GodotObject.IsInstanceValid(__instance) ||
                    __instance.IsQueuedForDeletion())
                {
                    return;
                }

                EnsureSkinChangerReflection();

                NKaiserCrabBossBackground oldBackground =
                    _backgroundField.GetValue(__instance) as NKaiserCrabBossBackground;

                bool hasOld =
                    oldBackground != null &&
                    GodotObject.IsInstanceValid(oldBackground) &&
                    !oldBackground.IsQueuedForDeletion();

                PackedScene scene = LoadSetupScene();
                if (scene == null)
                {
                    GD.PrintErr("[EmperorCrabSkinFix] 无法加载 " + SetupScenePath);
                    return;
                }

                NKaiserCrabBossBackground newBackground =
                    scene.Instantiate<NKaiserCrabBossBackground>(PackedScene.GenEditState.Disabled);

                if (newBackground == null)
                {
                    GD.PrintErr("[EmperorCrabSkinFix] 实例化帝皇蟹背景失败");
                    return;
                }

                int index = hasOld ? oldBackground.GetIndex() : __instance.GetChildCount();

                // 布局场景里这个节点名字就是 %KaiserCrab 的唯一名 key，必须原样保留。
                StringName backgroundName = hasOld
                    ? oldBackground.Name
                    : new StringName(DefaultBackgroundNodeName);

                if (hasOld)
                {
                    newBackground.Position = oldBackground.Position;
                    newBackground.Scale = oldBackground.Scale;
                    newBackground.Rotation = oldBackground.Rotation;
                    newBackground.Modulate = oldBackground.Modulate;
                    newBackground.SelfModulate = oldBackground.SelfModulate;
                    newBackground.ZIndex = oldBackground.ZIndex;
                    newBackground.ZAsRelative = oldBackground.ZAsRelative;
                }

                // NKaiserCrabBossBackground._Ready 会 SetVisible(false)。
                // Setup 已经让背景可见，这里在 AddChild 前后都把可见性写回去。
                newBackground.Ready += () =>
                {
                    if (GodotObject.IsInstanceValid(newBackground))
                    {
                        newBackground.Visible = true;
                    }
                };

                __instance.AddChild(newBackground);
                if (hasOld && index >= 0)
                {
                    __instance.MoveChild(newBackground, index);
                }

                // 先让旧节点从“布局的场景唯一名表”里退出，并把 KaiserCrab 这个名字让出来。
                // 注意 QueueFree 必须在节点仍在树上时调用，保证它一定会被调度删除。
                if (hasOld)
                {
                    try
                    {
                        oldBackground.UniqueNameInOwner = false;
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr("[EmperorCrabSkinFix] 解除旧背景唯一名失败：" + ex.Message);
                    }

                    try
                    {
                        // 先把名字让给新节点，避免同帧内两个 KaiserCrab 子节点重名。
                        oldBackground.Name = "__kaiser_crab_old";
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr("[EmperorCrabSkinFix] 重命名旧背景失败：" + ex.Message);
                    }

                    try
                    {
                        oldBackground.Owner = null;
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr("[EmperorCrabSkinFix] 清空旧背景 owner 失败：" + ex.Message);
                    }

                    oldBackground.Visible = false;
                    oldBackground.QueueFree();
                }

                // 关键：重新占据 %KaiserCrab，恢复 Rocket/Crusher.Background 的懒加载路径。
                newBackground.Name = backgroundName;
                try
                {
                    newBackground.Owner = __instance;
                    newBackground.UniqueNameInOwner = true;
                }
                catch (Exception ex)
                {
                    GD.PrintErr("[EmperorCrabSkinFix] 注册 %KaiserCrab 唯一名失败：" + ex.Message);
                }

                _backgroundField.SetValue(__instance, newBackground);
                newBackground.Visible = true;

                Callable.From(() =>
                {
                    if (GodotObject.IsInstanceValid(newBackground))
                    {
                        newBackground.Visible = true;
                    }
                }).CallDeferred();

                // 图鉴里当前这只怪（Rocket / Crusher）可能已经缓存过旧 _background，
                // 直接换成新节点，避免访问已释放对象导致动作预览中断。
                RebindBestiaryMonsterBackground(__instance, newBackground);

                GD.Print("[EmperorCrabSkinFix] 已按当前选择重建帝皇蟹图鉴背景");
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 重建帝皇蟹图鉴背景失败：" + ex);
            }
        }

        /// <summary>
        /// 图鉴里的 Rocket / Crusher 会把背景节点缓存在私有字段 _background 里。
        /// 背景被整体替换后，把当前图鉴怪的缓存指向新节点。
        /// </summary>
        private static void RebindBestiaryMonsterBackground(
            NBestiaryLayoutKaiserCrab layout,
            NKaiserCrabBossBackground background)
        {
            try
            {
                if (background == null ||
                    !GodotObject.IsInstanceValid(background) ||
                    _creatureField == null)
                {
                    return;
                }

                NCreature creature = _creatureField.GetValue(layout) as NCreature;
                MonsterModel monster = creature != null && GodotObject.IsInstanceValid(creature)
                    ? creature.Entity?.Monster
                    : null;

                if (monster == null)
                {
                    return;
                }

                string typeName = monster.GetType().Name;
                if (typeName != "Rocket" && typeName != "Crusher")
                {
                    return;
                }

                FieldInfo field = AccessTools.Field(monster.GetType(), "_background");
                if (field == null || field.FieldType != typeof(NKaiserCrabBossBackground))
                {
                    return;
                }

                field.SetValue(monster, background);
                GD.Print("[EmperorCrabSkinFix] 已更新图鉴怪物 " + typeName + " 的背景引用");
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 更新图鉴怪物背景引用失败：" + ex.Message);
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

                _catalogProperty = skinServiceType.GetProperty(
                    "Catalog",
                    BindingFlags.Public | BindingFlags.Static);

                _getRuntimeSceneMethod = skinServiceType.GetMethod(
                    "GetOrLoadRuntimeScene",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(string), typeof(string) },
                    null);

                Type catalogType = _catalogProperty != null ? _catalogProperty.PropertyType : null;
                _findGroupMethod = catalogType != null
                    ? catalogType.GetMethod("FindGroupIdForResourcePath", new[] { typeof(string) })
                    : null;

                _skinReflectionReady =
                    _catalogProperty != null &&
                    _getRuntimeSceneMethod != null &&
                    _findGroupMethod != null;

                if (!_skinReflectionReady)
                {
                    GD.PrintErr("[EmperorCrabSkinFix] 未能反射 SkinService 资源包接口，将回退直接加载");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr("[EmperorCrabSkinFix] 初始化 SkinService 反射失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 走 SkinChanger 的运行时资源包：它会把 kaiser_crab_boss_setup.tscn 以及
        /// kaiser_crab.atlas / .skel / .png 一起改写成独立别名路径，避免 Spine 资源
        /// 在全局缓存里和默认皮肤串贴图。
        /// </summary>
        private static PackedScene LoadSetupScene()
        {
            if (_skinReflectionReady)
            {
                try
                {
                    object catalog = _catalogProperty.GetValue(null);
                    if (catalog != null)
                    {
                        string setupGroupId = _findGroupMethod.Invoke(catalog, new object[] { SetupScenePath }) as string;
                        if (!string.IsNullOrWhiteSpace(setupGroupId))
                        {
                            PackedScene scene = _getRuntimeSceneMethod.Invoke(
                                null,
                                new object[] { setupGroupId, SetupScenePath }) as PackedScene;

                            if (scene != null)
                            {
                                GD.Print("[EmperorCrabSkinFix] 通过 SkinChanger 资源包加载帝皇蟹背景，group=" + setupGroupId);
                                return scene;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr("[EmperorCrabSkinFix] SkinChanger 资源包加载失败，回退直接加载：" + ex.Message);
                }
            }

            return ResourceLoader.Load<PackedScene>(
                SetupScenePath,
                null,
                ResourceLoader.CacheMode.IgnoreDeep);
        }
    }
}


