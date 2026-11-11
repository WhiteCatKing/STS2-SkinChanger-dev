using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Backgrounds;
using MegaCrit.Sts2.Core.Rooms;

namespace STS2SkinChanger.Ui.EmperorCrabCompat;

internal static class CombatCrabSkinPatch
{
	private const string SkinChangerAssemblyName = "Gurio.SkinChanger";

	private const string SkinGroupId = "kaiser_crab_boss_setup";

	private const string BackgroundScenePath = "res://scenes/backgrounds/kaiser_crab_boss/kaiser_crab_boss_background.tscn";

	private static readonly object Sync = new object();

	private static bool _installed;

	private static bool _skinReflectionReady;

	private static MethodInfo _getRuntimeSceneMethod;

	private static MethodInfo _setLayersMethod;

	public static void Install(Harmony harmony)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Expected O, but got Unknown
		lock (Sync)
		{
			if (_installed)
			{
				return;
			}
			try
			{
				MethodInfo methodInfo = AccessTools.Method(typeof(NCombatBackground), "Create", (Type[])null, (Type[])null);
				if (methodInfo == null)
				{
					GD.PrintErr("[EmperorCrabSkinFix] 找不到 NCombatBackground.Create");
					return;
				}
				_setLayersMethod = AccessTools.Method(typeof(NCombatBackground), "SetLayers", (Type[])null, (Type[])null);
				if (_setLayersMethod == null)
				{
					GD.PrintErr("[EmperorCrabSkinFix] 找不到 NCombatBackground.SetLayers");
					return;
				}
				harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(CombatCrabSkinPatch).GetMethod("BeforeCreate", BindingFlags.Static | BindingFlags.NonPublic)), new HarmonyMethod(typeof(CombatCrabSkinPatch).GetMethod("AfterCreate", BindingFlags.Static | BindingFlags.NonPublic)), (HarmonyMethod)null, (HarmonyMethod)null);
				_installed = true;
				GD.Print("[EmperorCrabSkinFix] 已挂上局内帝皇蟹背景补丁");
			}
			catch (Exception ex)
			{
				GD.PrintErr("[EmperorCrabSkinFix] 安装局内帝皇蟹背景补丁失败：" + ex);
			}
		}
	}

	private static bool BeforeCreate(BackgroundAssets bg, ref NCombatBackground __result)
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (bg == null || !string.Equals(bg.BackgroundScenePath, "res://scenes/backgrounds/kaiser_crab_boss/kaiser_crab_boss_background.tscn", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			EnsureSkinChangerReflection();
			if (!_skinReflectionReady)
			{
				GD.PrintErr("[EmperorCrabSkinFix] SkinService 资源包接口不可用，回退原版帝皇蟹背景");
				return true;
			}
			object? obj = _getRuntimeSceneMethod.Invoke(null, new object[2] { "kaiser_crab_boss_setup", "res://scenes/backgrounds/kaiser_crab_boss/kaiser_crab_boss_background.tscn" });
			PackedScene val = (PackedScene)((obj is PackedScene) ? obj : null);
			if (val == null)
			{
				GD.PrintErr("[EmperorCrabSkinFix] 无法生成帝皇蟹皮肤背景场景，group=kaiser_crab_boss_setup");
				return true;
			}
			NCombatBackground background = val.Instantiate<NCombatBackground>((GenEditState)0);
			if (background == null)
			{
				GD.PrintErr("[EmperorCrabSkinFix] 实例化帝皇蟹皮肤背景失败");
				return true;
			}
			_setLayersMethod.Invoke(background, new object[1] { bg });
			__result = background;
			((Node)background).Ready += delegate
			{
				ForceCrabVisible(background);
			};
			GD.Print("[EmperorCrabSkinFix] 已用皮肤版场景创建局内帝皇蟹背景，group=kaiser_crab_boss_setup");
			Callable val2 = Callable.From((Action)delegate
			{
				NKaiserCrabBossBackground nodeOrNull = ((Node)background).GetNodeOrNull<NKaiserCrabBossBackground>(NodePath.op_Implicit("%KaiserCrab"));
				if (nodeOrNull != null)
				{
					UpdateMonsterBackgroundReferences(nodeOrNull);
				}
				HalfHpCrabSkinPatch.TryInstallHalfHpPrefix();
			});
			((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
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
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (background == null || !GodotObject.IsInstanceValid((GodotObject)(object)background))
		{
			return;
		}
		NKaiserCrabBossBackground crab = ((Node)background).GetNodeOrNull<NKaiserCrabBossBackground>(NodePath.op_Implicit("%KaiserCrab"));
		if (crab == null)
		{
			return;
		}
		((CanvasItem)crab).Visible = true;
		GD.Print("[EmperorCrabSkinFix] 已恢复局内帝皇蟹背景可见性");
		Callable val = Callable.From((Action)delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)crab))
			{
				((CanvasItem)crab).Visible = true;
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static void AfterCreate(NCombatBackground __result)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (__result == null || !GodotObject.IsInstanceValid((GodotObject)(object)__result))
			{
				return;
			}
			NKaiserCrabBossBackground crab = ((Node)__result).GetNodeOrNull<NKaiserCrabBossBackground>(NodePath.op_Implicit("%KaiserCrab"));
			if (crab != null)
			{
				Callable val = Callable.From((Action)delegate
				{
					UpdateMonsterBackgroundReferences(crab);
					HalfHpCrabSkinPatch.TryInstallHalfHpPrefix();
				});
				((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("[EmperorCrabSkinFix] 更新帝皇蟹背景引用失败：" + ex);
		}
	}

	private static void UpdateMonsterBackgroundReferences(NKaiserCrabBossBackground crab)
	{
		if (crab == null || !GodotObject.IsInstanceValid((GodotObject)(object)crab))
		{
			return;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null || !GodotObject.IsInstanceValid((GodotObject)(object)instance))
		{
			return;
		}
		foreach (NCreature creatureNode in instance.CreatureNodes)
		{
			if (creatureNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
			{
				continue;
			}
			MonsterModel val = ((creatureNode.Entity != null) ? creatureNode.Entity.Monster : null);
			if (val == null)
			{
				continue;
			}
			string name = ((object)val).GetType().Name;
			if (!(name != "Rocket") || !(name != "Crusher"))
			{
				FieldInfo fieldInfo = AccessTools.Field(((object)val).GetType(), "_background");
				if (fieldInfo != null && fieldInfo.FieldType == typeof(NKaiserCrabBossBackground))
				{
					fieldInfo.SetValue(val, crab);
					GD.Print("[EmperorCrabSkinFix] 已更新 " + name + " 的背景引用");
				}
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
			if (!(assembly == null))
			{
				Type type = assembly.GetType("STS2SkinChanger.Core.SkinService", throwOnError: false, ignoreCase: false);
				if (!(type == null))
				{
					_getRuntimeSceneMethod = type.GetMethod("GetOrLoadRuntimeScene", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
					{
						typeof(string),
						typeof(string)
					}, null);
					_skinReflectionReady = _getRuntimeSceneMethod != null;
				}
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("[EmperorCrabSkinFix] 初始化 SkinService 反射失败：" + ex.Message);
		}
	}
}
