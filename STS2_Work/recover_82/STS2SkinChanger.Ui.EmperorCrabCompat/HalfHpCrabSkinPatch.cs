using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Backgrounds;

namespace STS2SkinChanger.Ui.EmperorCrabCompat;

internal static class HalfHpCrabSkinPatch
{
	private const string SkinGroupId = "kaiser_crab_boss_setup";

	private const string HurtAtlasPath = "res://animations/monsters/kaiser_crab/kaiser_crab_hurt.atlas";

	private const string HurtDataPath = "res://animations/monsters/kaiser_crab/kaiser_crab_skeleton_data_hurt.tres";

	private const string AppliedMeta = "EmperorCrabSkinFix.HalfHpApplied";

	private const string NoHalfHpResourcesMeta = "EmperorCrabSkinFix.NoHalfHpResources";

	private const string HalfHpPatchTypeName = "erowatcher.script.patch.MonsterHurtSpinePatch";

	private const string HalfHpSettingsTypeName = "erowatcher.script.card.WatcherCardArtSettings";

	private const string SkinChangerAssemblyName = "Gurio.SkinChanger";

	private static Harmony _harmony;

	private static FieldInfo _animControllerField;

	private static MethodInfo _getRuntimeResourceMethod;

	private static readonly MethodInfo SetAnimationMethod = typeof(MegaAnimationState).GetMethod("SetAnimation", BindingFlags.Instance | BindingFlags.Public, null, new Type[3]
	{
		typeof(string),
		typeof(bool),
		typeof(int)
	}, null);

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
		StartInstallRetry();
	}

	private static void InstallAnimationGuards()
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		if (_animationGuardsInstalled || _harmony == null)
		{
			return;
		}
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(NKaiserCrabBossBackground), "PlayHurtAnim", (Type[])null, (Type[])null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(NKaiserCrabBossBackground), "PlayArmDeathAnim", (Type[])null, (Type[])null);
			MethodInfo methodInfo3 = AccessTools.Method(typeof(NKaiserCrabBossBackground), "PlayBodyDeathAnim", (Type[])null, (Type[])null);
			if (methodInfo == null || methodInfo2 == null || methodInfo3 == null)
			{
				GD.PrintErr("[EmperorCrabSkinFix] 找不到帝皇蟹背景动画方法，跳过安全阀");
				return;
			}
			_harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(HalfHpCrabSkinPatch).GetMethod("BeforeHurtAnim", BindingFlags.Static | BindingFlags.NonPublic)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			_harmony.Patch((MethodBase)methodInfo2, new HarmonyMethod(typeof(HalfHpCrabSkinPatch).GetMethod("BeforeArmDeathAnim", BindingFlags.Static | BindingFlags.NonPublic)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			_harmony.Patch((MethodBase)methodInfo3, new HarmonyMethod(typeof(HalfHpCrabSkinPatch).GetMethod("BeforeBodyDeathAnim", BindingFlags.Static | BindingFlags.NonPublic)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			_animationGuardsInstalled = true;
			GD.Print("[EmperorCrabSkinFix] 已挂上帝皇蟹背景动画安全阀");
		}
		catch (Exception ex)
		{
			GD.PrintErr("[EmperorCrabSkinFix] 安装背景动画安全阀失败：" + ex);
		}
	}

	private static bool BeforeHurtAnim(NKaiserCrabBossBackground __instance, ArmSide side)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return DeferIfSpineNotReady(__instance, delegate(NKaiserCrabBossBackground background)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			background.PlayHurtAnim(side);
		});
	}

	private static bool BeforeArmDeathAnim(NKaiserCrabBossBackground __instance, ArmSide side)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return DeferIfSpineNotReady(__instance, delegate(NKaiserCrabBossBackground background)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			background.PlayArmDeathAnim(side);
		});
	}

	private static bool BeforeBodyDeathAnim(NKaiserCrabBossBackground __instance)
	{
		return DeferIfSpineNotReady(__instance, delegate(NKaiserCrabBossBackground background)
		{
			background.PlayBodyDeathAnim();
		});
	}

	private static bool DeferIfSpineNotReady(NKaiserCrabBossBackground background, Action<NKaiserCrabBossBackground> action)
	{
		try
		{
			if (background == null || !GodotObject.IsInstanceValid((GodotObject)(object)background))
			{
				return true;
			}
			object? obj = _animControllerField?.GetValue(background);
			MegaSprite val = (MegaSprite)((obj is MegaSprite) ? obj : null);
			if (val == null || ((MegaSpineBinding)val).BoundObject == null || !GodotObject.IsInstanceValid(((MegaSpineBinding)val).BoundObject))
			{
				return true;
			}
			if (val.IsAnimationStateReady())
			{
				return true;
			}
			GD.Print("[EmperorCrabSkinFix] 帝皇蟹 Spine 尚未就绪，延后播放动画");
			SpineNodeExtensions.RunWhenSpineReady((Node)(object)background, val, (Action<MegaAnimationState>)delegate
			{
				try
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)background))
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
		catch (Exception ex2)
		{
			GD.PrintErr("[EmperorCrabSkinFix] 检查帝皇蟹 Spine 就绪状态失败：" + ex2.Message);
			return true;
		}
	}

	public static void TryInstallHalfHpPrefix()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		if (_halfHpPrefixInstalled || _halfHpPrefixMissing || _harmony == null)
		{
			return;
		}
		try
		{
			Type type = FindType("erowatcher.script.patch.MonsterHurtSpinePatch");
			if (!(type == null))
			{
				MethodInfo methodInfo = AccessTools.Method(type, "Postfix", new Type[1] { typeof(Creature) }, (Type[])null);
				if (methodInfo == null)
				{
					_halfHpPrefixMissing = true;
					GD.PrintErr("[EmperorCrabSkinFix] 找不到 MonsterHurtSpinePatch.Postfix(Creature)");
					return;
				}
				_harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(HalfHpCrabSkinPatch).GetMethod("BeforeHalfHpPostfix", BindingFlags.Static | BindingFlags.NonPublic)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				_halfHpPrefixInstalled = true;
				StopInstallRetry();
				GD.Print("[EmperorCrabSkinFix] 已接管帝皇蟹半血爆衣换装");
			}
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
		string entry = ((AbstractModel)creature.Monster).Id.Entry;
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
			FieldInfo fieldInfo = FindType("erowatcher.script.card.WatcherCardArtSettings")?.GetField("EnableHurtSkins", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (fieldInfo != null && fieldInfo.FieldType == typeof(bool))
			{
				return (bool)fieldInfo.GetValue(null);
			}
		}
		catch
		{
		}
		return true;
	}

	private static bool ApplySkinHalfHp(Creature creature)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		NCombatRoom instance = NCombatRoom.Instance;
		NCombatBackground val = ((instance != null && GodotObject.IsInstanceValid((GodotObject)(object)instance)) ? instance.Background : null);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return false;
		}
		Node nodeOrNull = ((Node)val).GetNodeOrNull(NodePath.op_Implicit("%KaiserCrab"));
		Node2D val2 = ((nodeOrNull != null) ? nodeOrNull.GetNodeOrNull<Node2D>(NodePath.op_Implicit("Visuals")) : null);
		if (val2 == null || !GodotObject.IsInstanceValid((GodotObject)(object)val2))
		{
			return false;
		}
		if (((GodotObject)val2).HasMeta(StringName.op_Implicit("EmperorCrabSkinFix.HalfHpApplied")))
		{
			return true;
		}
		if (((GodotObject)val2).HasMeta(StringName.op_Implicit("EmperorCrabSkinFix.NoHalfHpResources")))
		{
			return false;
		}
		if (!FileAccess.FileExists("res://animations/monsters/kaiser_crab/kaiser_crab_skeleton_data_hurt.tres") || !FileAccess.FileExists("res://animations/monsters/kaiser_crab/kaiser_crab_hurt.atlas"))
		{
			((GodotObject)val2).SetMeta(StringName.op_Implicit("EmperorCrabSkinFix.NoHalfHpResources"), Variant.op_Implicit(true));
			GD.Print("[EmperorCrabSkinFix] 当前皮肤没有半血爆衣资源，保持正常外观");
			return false;
		}
		Variant val3 = ((GodotObject)val2).Get(StringName.op_Implicit("skeleton_data_res"));
		GodotObject val4 = ((Variant)(ref val3)).AsGodotObject();
		if (val4 == null)
		{
			return false;
		}
		val3 = val4.Get(StringName.op_Implicit("skeleton_file_res"));
		GodotObject val5 = ((Variant)(ref val3)).AsGodotObject();
		if (val5 == null)
		{
			GD.PrintErr("[EmperorCrabSkinFix] 取不到当前皮肤骨架，跳过半血换装");
			return false;
		}
		GodotObject val6 = LoadHurtAtlas();
		if (val6 == null)
		{
			GD.PrintErr("[EmperorCrabSkinFix] 找不到半血爆衣图集，跳过半血换装");
			return false;
		}
		Resource val7 = BuildHurtSkeletonData(val6, val5);
		if (val7 == null)
		{
			return false;
		}
		MegaSprite val8 = new MegaSprite(Variant.op_Implicit((GodotObject)(object)val2));
		val8.SetSkeletonDataRes(new MegaSkeletonDataResource(Variant.op_Implicit((GodotObject)(object)val7)));
		((GodotObject)val2).SetMeta(StringName.op_Implicit("EmperorCrabSkinFix.HalfHpApplied"), Variant.op_Implicit(true));
		GD.Print("[EmperorCrabSkinFix] 已应用帝皇蟹半血爆衣皮肤");
		SpineNodeExtensions.RunWhenSpineReady(nodeOrNull, val8, (Action<MegaAnimationState>)delegate(MegaAnimationState state)
		{
			try
			{
				SetAnimationMethod?.Invoke(state, new object[3] { "right/idle_loop", true, 2 });
				SetAnimationMethod?.Invoke(state, new object[3] { "body/idle_loop", true, 0 });
				SetAnimationMethod?.Invoke(state, new object[3] { "left/idle_loop", true, 1 });
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
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (!FileAccess.FileExists("res://animations/monsters/kaiser_crab/kaiser_crab_skeleton_data_hurt.tres"))
		{
			return null;
		}
		Resource val = null;
		try
		{
			val = ResourceLoader.Load<Resource>("res://animations/monsters/kaiser_crab/kaiser_crab_skeleton_data_hurt.tres", string.Empty, (CacheMode)0);
		}
		catch (Exception ex)
		{
			GD.PrintErr("[EmperorCrabSkinFix] 加载半血骨架数据失败：" + ex.Message);
		}
		if (val == null)
		{
			return null;
		}
		try
		{
			((GodotObject)val).Set(StringName.op_Implicit("atlas_res"), Variant.op_Implicit(hurtAtlas));
			((GodotObject)val).Set(StringName.op_Implicit("skeleton_file_res"), Variant.op_Implicit(skinSkeleton));
			return val;
		}
		catch (Exception ex2)
		{
			GD.PrintErr("[EmperorCrabSkinFix] 组装半血骨架数据失败：" + ex2.Message);
			return null;
		}
	}

	private static GodotObject LoadHurtAtlas()
	{
		try
		{
			Resource val = ResourceLoader.Load<Resource>("res://animations/monsters/kaiser_crab/kaiser_crab_hurt.atlas", string.Empty, (CacheMode)1);
			if (val != null)
			{
				return (GodotObject)(object)val;
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("[EmperorCrabSkinFix] 直接加载爆衣图集失败：" + ex.Message);
		}
		try
		{
			MethodInfo runtimeResourceMethod = GetRuntimeResourceMethod();
			if (runtimeResourceMethod != null)
			{
				object? obj = runtimeResourceMethod.Invoke(null, new object[3] { "kaiser_crab_boss_setup", "res://animations/monsters/kaiser_crab/kaiser_crab_hurt.atlas", true });
				GodotObject val2 = (GodotObject)((obj is GodotObject) ? obj : null);
				if (val2 != null && GodotObject.IsInstanceValid(val2))
				{
					return val2;
				}
			}
		}
		catch (Exception ex2)
		{
			GD.PrintErr("[EmperorCrabSkinFix] SkinChanger 取爆衣图集失败：" + ex2.Message);
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
			_getRuntimeResourceMethod = (FindAssembly("Gurio.SkinChanger")?.GetType("STS2SkinChanger.Core.SkinService", throwOnError: false, ignoreCase: false))?.GetMethod("GetOrLoadRuntimeResource", BindingFlags.Static | BindingFlags.Public, null, new Type[3]
			{
				typeof(string),
				typeof(string),
				typeof(bool)
			}, null);
		}
		catch (Exception ex)
		{
			GD.PrintErr("[EmperorCrabSkinFix] 反射 SkinService 资源接口失败：" + ex.Message);
		}
		return _getRuntimeResourceMethod;
	}

	private static void StartInstallRetry()
	{
		if (_halfHpPrefixInstalled || _retryTree != null)
		{
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null)
		{
			return;
		}
		_retryTree = val;
		_retryFrames = 0;
		_retryHandler = delegate
		{
			try
			{
				if (_halfHpPrefixInstalled || _halfHpPrefixMissing)
				{
					StopInstallRetry();
				}
				else if (++_retryFrames % 60 == 0)
				{
					TryInstallHalfHpPrefix();
					if (_retryFrames >= 18000)
					{
						StopInstallRetry();
					}
				}
			}
			catch
			{
				StopInstallRetry();
			}
		};
		val.ProcessFrame += _retryHandler;
	}

	private static void StopInstallRetry()
	{
		if (_retryTree != null && _retryHandler != null)
		{
			try
			{
				_retryTree.ProcessFrame -= _retryHandler;
			}
			catch
			{
			}
		}
		_retryTree = null;
		_retryHandler = null;
	}

	private static Assembly FindAssembly(string name)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
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
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			try
			{
				Type type = assembly.GetType(fullName, throwOnError: false, ignoreCase: false);
				if (type != null)
				{
					return type;
				}
			}
			catch
			{
			}
		}
		return null;
	}
}
