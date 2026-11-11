using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using MegaCrit.Sts2.Core.Nodes.Vfx.Backgrounds;

namespace STS2SkinChanger.Ui.EmperorCrabCompat;

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
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
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
				MethodInfo methodInfo = AccessTools.Method(typeof(NBestiaryLayoutKaiserCrab), "Setup", (Type[])null, (Type[])null);
				if (methodInfo == null)
				{
					GD.PrintErr("[EmperorCrabSkinFix] 找不到 NBestiaryLayoutKaiserCrab.Setup");
					return;
				}
				harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(typeof(KaiserCrabSkinRefreshPatch).GetMethod("AfterKaiserCrabSetup", BindingFlags.Static | BindingFlags.NonPublic)), (HarmonyMethod)null, (HarmonyMethod)null);
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
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (__instance == null || !GodotObject.IsInstanceValid((GodotObject)(object)__instance) || ((GodotObject)__instance).IsQueuedForDeletion())
			{
				return;
			}
			EnsureSkinChangerReflection();
			object? value = _backgroundField.GetValue(__instance);
			NKaiserCrabBossBackground val = (NKaiserCrabBossBackground)((value is NKaiserCrabBossBackground) ? value : null);
			bool flag = val != null && GodotObject.IsInstanceValid((GodotObject)(object)val) && !((GodotObject)val).IsQueuedForDeletion();
			PackedScene val2 = LoadSetupScene();
			if (val2 == null)
			{
				GD.PrintErr("[EmperorCrabSkinFix] 无法加载 res://scenes/creature_visuals/kaiser_crab_boss_setup.tscn");
				return;
			}
			NKaiserCrabBossBackground newBackground = val2.Instantiate<NKaiserCrabBossBackground>((GenEditState)0);
			if (newBackground == null)
			{
				GD.PrintErr("[EmperorCrabSkinFix] 实例化帝皇蟹背景失败");
				return;
			}
			int num = (flag ? ((Node)val).GetIndex(false) : ((Node)__instance).GetChildCount(false));
			StringName name = (StringName)(flag ? ((object)((Node)val).Name) : ((object)new StringName("KaiserCrab")));
			if (flag)
			{
				((Node2D)newBackground).Position = ((Node2D)val).Position;
				((Node2D)newBackground).Scale = ((Node2D)val).Scale;
				((Node2D)newBackground).Rotation = ((Node2D)val).Rotation;
				((CanvasItem)newBackground).Modulate = ((CanvasItem)val).Modulate;
				((CanvasItem)newBackground).SelfModulate = ((CanvasItem)val).SelfModulate;
				((CanvasItem)newBackground).ZIndex = ((CanvasItem)val).ZIndex;
				((CanvasItem)newBackground).ZAsRelative = ((CanvasItem)val).ZAsRelative;
			}
			((Node)newBackground).Ready += delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)newBackground))
				{
					((CanvasItem)newBackground).Visible = true;
				}
			};
			((Node)__instance).AddChild((Node)(object)newBackground, false, (InternalMode)0);
			if (flag && num >= 0)
			{
				((Node)__instance).MoveChild((Node)(object)newBackground, num);
			}
			if (flag)
			{
				try
				{
					((Node)val).UniqueNameInOwner = false;
				}
				catch (Exception ex)
				{
					GD.PrintErr("[EmperorCrabSkinFix] 解除旧背景唯一名失败：" + ex.Message);
				}
				try
				{
					((Node)val).Name = StringName.op_Implicit("__kaiser_crab_old");
				}
				catch (Exception ex2)
				{
					GD.PrintErr("[EmperorCrabSkinFix] 重命名旧背景失败：" + ex2.Message);
				}
				try
				{
					((Node)val).Owner = null;
				}
				catch (Exception ex3)
				{
					GD.PrintErr("[EmperorCrabSkinFix] 清空旧背景 owner 失败：" + ex3.Message);
				}
				((CanvasItem)val).Visible = false;
				((Node)val).QueueFree();
			}
			((Node)newBackground).Name = name;
			try
			{
				((Node)newBackground).Owner = (Node)(object)__instance;
				((Node)newBackground).UniqueNameInOwner = true;
			}
			catch (Exception ex4)
			{
				GD.PrintErr("[EmperorCrabSkinFix] 注册 %KaiserCrab 唯一名失败：" + ex4.Message);
			}
			_backgroundField.SetValue(__instance, newBackground);
			((CanvasItem)newBackground).Visible = true;
			Callable val3 = Callable.From((Action)delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)newBackground))
				{
					((CanvasItem)newBackground).Visible = true;
				}
			});
			((Callable)(ref val3)).CallDeferred(Array.Empty<Variant>());
			RebindBestiaryMonsterBackground(__instance, newBackground);
			GD.Print("[EmperorCrabSkinFix] 已按当前选择重建帝皇蟹图鉴背景");
		}
		catch (Exception ex5)
		{
			GD.PrintErr("[EmperorCrabSkinFix] 重建帝皇蟹图鉴背景失败：" + ex5);
		}
	}

	private static void RebindBestiaryMonsterBackground(NBestiaryLayoutKaiserCrab layout, NKaiserCrabBossBackground background)
	{
		try
		{
			if (background == null || !GodotObject.IsInstanceValid((GodotObject)(object)background) || _creatureField == null)
			{
				return;
			}
			object? value = _creatureField.GetValue(layout);
			NCreature val = (NCreature)((value is NCreature) ? value : null);
			object obj;
			if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				obj = null;
			}
			else
			{
				Creature entity = val.Entity;
				obj = ((entity != null) ? entity.Monster : null);
			}
			MonsterModel val2 = (MonsterModel)obj;
			if (val2 == null)
			{
				return;
			}
			string name = ((object)val2).GetType().Name;
			if (!(name != "Rocket") || !(name != "Crusher"))
			{
				FieldInfo fieldInfo = AccessTools.Field(((object)val2).GetType(), "_background");
				if (!(fieldInfo == null) && !(fieldInfo.FieldType != typeof(NKaiserCrabBossBackground)))
				{
					fieldInfo.SetValue(val2, background);
					GD.Print("[EmperorCrabSkinFix] 已更新图鉴怪物 " + name + " 的背景引用");
				}
			}
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
			Assembly assembly = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly2 in assemblies)
			{
				if (assembly2.GetName().Name == "Gurio.SkinChanger")
				{
					assembly = assembly2;
					break;
				}
			}
			if (assembly == null)
			{
				return;
			}
			Type type = assembly.GetType("STS2SkinChanger.Core.SkinService", throwOnError: false, ignoreCase: false);
			if (!(type == null))
			{
				_catalogProperty = type.GetProperty("Catalog", BindingFlags.Static | BindingFlags.Public);
				_getRuntimeSceneMethod = type.GetMethod("GetOrLoadRuntimeScene", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
				{
					typeof(string),
					typeof(string)
				}, null);
				Type type2 = ((_catalogProperty != null) ? _catalogProperty.PropertyType : null);
				_findGroupMethod = ((type2 != null) ? type2.GetMethod("FindGroupIdForResourcePath", new Type[1] { typeof(string) }) : null);
				_skinReflectionReady = _catalogProperty != null && _getRuntimeSceneMethod != null && _findGroupMethod != null;
				if (!_skinReflectionReady)
				{
					GD.PrintErr("[EmperorCrabSkinFix] 未能反射 SkinService 资源包接口，将回退直接加载");
				}
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("[EmperorCrabSkinFix] 初始化 SkinService 反射失败：" + ex.Message);
		}
	}

	private static PackedScene LoadSetupScene()
	{
		if (_skinReflectionReady)
		{
			try
			{
				object value = _catalogProperty.GetValue(null);
				if (value != null)
				{
					string text = _findGroupMethod.Invoke(value, new object[1] { "res://scenes/creature_visuals/kaiser_crab_boss_setup.tscn" }) as string;
					if (!string.IsNullOrWhiteSpace(text))
					{
						object? obj = _getRuntimeSceneMethod.Invoke(null, new object[2] { text, "res://scenes/creature_visuals/kaiser_crab_boss_setup.tscn" });
						PackedScene val = (PackedScene)((obj is PackedScene) ? obj : null);
						if (val != null)
						{
							GD.Print("[EmperorCrabSkinFix] 通过 SkinChanger 资源包加载帝皇蟹背景，group=" + text);
							return val;
						}
					}
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[EmperorCrabSkinFix] SkinChanger 资源包加载失败，回退直接加载：" + ex.Message);
			}
		}
		return ResourceLoader.Load<PackedScene>("res://scenes/creature_visuals/kaiser_crab_boss_setup.tscn", (string)null, (CacheMode)3);
	}
}
