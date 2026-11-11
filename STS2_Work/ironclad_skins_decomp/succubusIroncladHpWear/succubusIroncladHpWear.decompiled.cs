using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: IgnoresAccessChecksTo("sts2")]
[assembly: AssemblyCompany("succubusIroncladHpWear")]
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyFileVersion("1.0.6.0")]
[assembly: AssemblyInformationalVersion("1.0.6")]
[assembly: AssemblyProduct("succubusIroncladHpWear")]
[assembly: AssemblyTitle("succubusIroncladHpWear")]
[assembly: InternalsVisibleTo("succubusIroncladHpWear.SettingsBridge")]
[assembly: TargetFramework(".NETCoreApp,Version=v9.0", FrameworkDisplayName = ".NET 9.0")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("1.0.8.0")]
[module: UnverifiableCode]
[module: RefSafetyRules(11)]
namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	internal sealed class IgnoresAccessChecksToAttribute : Attribute
	{
		internal IgnoresAccessChecksToAttribute(string assemblyName)
		{
		}
	}
}
namespace BattleWear
{
	internal static class SpineAnimCompat
	{
		private static readonly MethodInfo? AccessSetAnimationMethod = AccessTools.Method(typeof(SpineAnimationAccess), "SetAnimation", new Type[3]
		{
			typeof(string),
			typeof(bool),
			typeof(int)
		}, (Type[])null);

		private static readonly MethodInfo? AccessAddAnimationMethod = AccessTools.Method(typeof(SpineAnimationAccess), "AddAnimation", new Type[4]
		{
			typeof(string),
			typeof(float),
			typeof(bool),
			typeof(int)
		}, (Type[])null);

		private static readonly MethodInfo? AccessGetCurrentAnimationNameMethod = AccessTools.Method(typeof(SpineAnimationAccess), "GetCurrentAnimationName", new Type[1] { typeof(int) }, (Type[])null);

		private static readonly MethodInfo? StateSetAnimationMethod = AccessTools.Method(typeof(MegaAnimationState), "SetAnimation", new Type[3]
		{
			typeof(string),
			typeof(bool),
			typeof(int)
		}, (Type[])null);

		private static readonly MethodInfo? StateGetCurrentAnimationNameMethod = AccessTools.Method(typeof(MegaAnimationState), "GetCurrentAnimationName", new Type[1] { typeof(int) }, (Type[])null);

		internal const float IdleLowHealthMixSec = 0.3f;

		internal const string IdleAnim = "idle_loop";

		internal const string LowHealthAnim = "low_health_loop";

		internal static readonly (string From, string To, float Mix)[] DefectPreviewMixes = new(string, string, float)[30]
		{
			("idle_loop", "low_health_loop", 0.3f),
			("low_health_loop", "idle_loop", 0.3f),
			("idle_loop", "attack", 0.05f),
			("attack", "attack", 0.03f),
			("hurt", "hurt", 0.03f),
			("hurt", "die", 0.1f),
			("idle_loop", "hurt", 0.03f),
			("hurt", "idle_loop", 0.1f),
			("cast", "cast", 0.1f),
			("attack", "cast", 0.05f),
			("cast", "attack", 0.05f),
			("idle_loop", "die", 0.1f),
			("idle_loop", "relaxed_loop", 0.1f),
			("relaxed_loop", "idle_loop", 0.1f),
			("attack", "process", 0.05f),
			("cast", "process", 0.05f),
			("idle_loop", "process", 0.03f),
			("process", "process", 0.1f),
			("process", "cast", 0.1f),
			("process", "idle_loop", 0.03f),
			("process", "attack", 0.03f),
			("attack", "low_health_loop", 0.43f),
			("cast", "low_health_loop", 0.47f),
			("hurt", "low_health_loop", 0.55f),
			("process", "low_health_loop", 0.5f),
			("low_health_loop", "attack", 0.1f),
			("low_health_loop", "cast", 0.1f),
			("low_health_loop", "hurt", 0.1f),
			("low_health_loop", "die", 0.1f),
			("low_health_loop", "process", 0.1f)
		};

		internal static bool IsTransientSpineError(Exception ex)
		{
			for (Exception ex2 = ex; ex2 != null; ex2 = ex2.InnerException)
			{
				if ((ex2 is EntryPointNotFoundException || ex2 is MissingMethodException) ? true : false)
				{
					return true;
				}
			}
			string text = ex.ToString();
			if (text.IndexOf("Entry point was not found", StringComparison.OrdinalIgnoreCase) < 0 && text.IndexOf("GetAnimationState()", StringComparison.OrdinalIgnoreCase) < 0 && text.IndexOf("skeleton finished initializing", StringComparison.OrdinalIgnoreCase) < 0 && text.IndexOf("IsAnimationStateReady", StringComparison.OrdinalIgnoreCase) < 0 && text.IndexOf("RunWhenSpineReady", StringComparison.OrdinalIgnoreCase) < 0 && text.IndexOf("GetCurrentAnimationName", StringComparison.OrdinalIgnoreCase) < 0 && text.IndexOf("MegaTrackEntry", StringComparison.OrdinalIgnoreCase) < 0)
			{
				return text.IndexOf("GetAnimation()", StringComparison.OrdinalIgnoreCase) >= 0;
			}
			return true;
		}

		internal static void SetAnimation(SpineAnimationAccess access, string name, bool loop, int track = 0)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			TrySetAnimation(access, name, loop, track);
		}

		internal static void EnsureIdleLowHealthMixes(Node? spine, Resource? skeletonData)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Expected O, but got Unknown
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			if (!string.Equals(ModEntry.ModId, "defectHpWear", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			GodotObject val = (GodotObject)(object)skeletonData;
			if (val == null && spine != null && GodotObject.IsInstanceValid((GodotObject)(object)spine))
			{
				try
				{
					Variant val2 = ((GodotObject)spine).Get(StringName.op_Implicit("skeleton_data_res"));
					val = ((Variant)(ref val2)).AsGodotObject();
				}
				catch
				{
				}
			}
			if (val == null || !GodotObject.IsInstanceValid(val))
			{
				return;
			}
			try
			{
				Variant val3 = val.Get(StringName.op_Implicit("animation_mixes"));
				Array val4 = ((Variant)(ref val3)).AsGodotArray();
				Array val5 = new Array();
				HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				if (val4 != null)
				{
					foreach (Variant item6 in val4)
					{
						Variant current = item6;
						GodotObject val6 = ((Variant)(ref current)).AsGodotObject();
						if (val6 == null)
						{
							val5.Add(current);
							continue;
						}
						string text = ReadMixString(val6, "from", "get_from");
						string text2 = ReadMixString(val6, "to", "get_to");
						string item = text + "\0" + text2;
						float? num = MixSecInPreview(text, text2);
						if (num.HasValue)
						{
							WriteMixSeconds(val6, num.Value);
							hashSet.Add(item);
						}
						val5.Add(current);
					}
				}
				(string, string, float)[] defectPreviewMixes = DefectPreviewMixes;
				for (int i = 0; i < defectPreviewMixes.Length; i++)
				{
					(string, string, float) tuple = defectPreviewMixes[i];
					string item2 = tuple.Item1;
					string item3 = tuple.Item2;
					float item4 = tuple.Item3;
					string item5 = item2 + "\0" + item3;
					if (!hashSet.Contains(item5))
					{
						Variant? val7 = TryCreateMix(item2, item3, item4);
						if (val7.HasValue)
						{
							val5.Add(val7.Value);
							hashSet.Add(item5);
						}
					}
				}
				val.Set(StringName.op_Implicit("animation_mixes"), Variant.op_Implicit(val5));
				if (val.HasMethod(StringName.op_Implicit("update_mixes")))
				{
					val.Call(StringName.op_Implicit("update_mixes"), Array.Empty<Variant>());
				}
			}
			catch
			{
			}
		}

		internal static float? MixSecInPreview(string? from, string? to)
		{
			if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
			{
				return null;
			}
			(string, string, float)[] defectPreviewMixes = DefectPreviewMixes;
			for (int i = 0; i < defectPreviewMixes.Length; i++)
			{
				var (text, text2, value) = defectPreviewMixes[i];
				if (text.Equals(from, StringComparison.OrdinalIgnoreCase) && text2.Equals(to, StringComparison.OrdinalIgnoreCase))
				{
					return value;
				}
			}
			return null;
		}

		internal static float MixDurationFor(string? from, string to)
		{
			float? num = MixSecInPreview(from, to);
			if (num.HasValue)
			{
				return num.Value;
			}
			if (IsIdleLowPair(from, to))
			{
				return 0.3f;
			}
			return -1f;
		}

		internal static bool IsIdleLowPair(string? from, string to)
		{
			if (!IsIdleName(from) || !IsLowHealthName(to))
			{
				if (IsLowHealthName(from))
				{
					return IsIdleName(to);
				}
				return false;
			}
			return true;
		}

		private static bool IsIdleName(string? name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				return name.Equals("idle_loop", StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		private static bool IsLowHealthName(string? name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				return name.Equals("low_health_loop", StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		private static Variant? TryCreateMix(string from, string to, float mix)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Variant value = ClassDB.Instantiate(StringName.op_Implicit("SpineAnimationMix"));
				GodotObject val = ((Variant)(ref value)).AsGodotObject();
				if (val == null)
				{
					return null;
				}
				if (val.HasMethod(StringName.op_Implicit("set_from")))
				{
					val.Call(StringName.op_Implicit("set_from"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(from) });
					val.Call(StringName.op_Implicit("set_to"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(to) });
					val.Call(StringName.op_Implicit("set_mix"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(mix) });
				}
				else
				{
					val.Set(StringName.op_Implicit("from"), Variant.op_Implicit(from));
					val.Set(StringName.op_Implicit("to"), Variant.op_Implicit(to));
					val.Set(StringName.op_Implicit("mix"), Variant.op_Implicit(mix));
				}
				return value;
			}
			catch
			{
				return null;
			}
		}

		private static void WriteMixSeconds(GodotObject mixObj, float mix)
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (mixObj.HasMethod(StringName.op_Implicit("set_mix")))
				{
					mixObj.Call(StringName.op_Implicit("set_mix"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(mix) });
				}
				else
				{
					mixObj.Set(StringName.op_Implicit("mix"), Variant.op_Implicit(mix));
				}
			}
			catch
			{
			}
		}

		private static string ReadMixString(GodotObject mixObj, string prop, string getter)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Variant val;
				if (mixObj.HasMethod(StringName.op_Implicit(getter)))
				{
					val = mixObj.Call(StringName.op_Implicit(getter), Array.Empty<Variant>());
					return ((Variant)(ref val)).AsString();
				}
				val = mixObj.Get(StringName.op_Implicit(prop));
				return ((Variant)(ref val)).AsString();
			}
			catch
			{
				return "";
			}
		}

		internal static bool TrySetAnimation(SpineAnimationAccess access, string name, bool loop, int track = 0, float mixSec = -1f, bool allowPromote = true)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			if (AccessSetAnimationMethod == null)
			{
				return false;
			}
			name = LowHpAnimRedirectPatch.Redirect(name, allowPromote);
			try
			{
				string currentAnimationName = GetCurrentAnimationName(access, track);
				AccessSetAnimationMethod.Invoke(access, new object[3] { name, loop, track });
				float num = ((mixSec >= 0f) ? mixSec : MixDurationFor(currentAnimationName, name));
				if (num >= 0f)
				{
					TrySetMixOnAccess(access, track, num);
				}
				return true;
			}
			catch (Exception ex) when (IsTransientSpineError(ex))
			{
				return false;
			}
		}

		internal static void SetAnimation(MegaAnimationState state, string name, bool loop, int track = 0)
		{
			TrySetAnimation(state, name, loop, track);
		}

		internal static bool TrySetAnimation(MegaAnimationState state, string name, bool loop, int track = 0, float mixSec = -1f, bool allowPromote = true)
		{
			if (StateSetAnimationMethod == null || state == null)
			{
				return false;
			}
			name = LowHpAnimRedirectPatch.Redirect(name, allowPromote);
			try
			{
				string currentAnimationName = GetCurrentAnimationName(state, track);
				StateSetAnimationMethod.Invoke(state, new object[3] { name, loop, track });
				float num = ((mixSec >= 0f) ? mixSec : MixDurationFor(currentAnimationName, name));
				if (num >= 0f)
				{
					TrySetMixOnState(state, track, num);
				}
				return true;
			}
			catch (Exception ex) when (IsTransientSpineError(ex))
			{
				return false;
			}
		}

		internal static bool TrySetAnimation(MegaSprite sprite, string name, bool loop, int track = 0, float mixSec = -1f, bool allowPromote = true)
		{
			try
			{
				MegaAnimationState val = sprite.TryGetAnimationState();
				return val != null && TrySetAnimation(val, name, loop, track, mixSec, allowPromote);
			}
			catch (Exception ex) when (IsTransientSpineError(ex))
			{
				return false;
			}
		}

		private static void TrySetMixOnAccess(SpineAnimationAccess access, int track, float mixSec)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				SetEntryMixDuration((AccessTools.Method(((object)access).GetType(), "GetCurrentTrack", new Type[1] { typeof(int) }, (Type[])null) ?? AccessTools.Method(((object)access).GetType(), "GetCurrent", new Type[1] { typeof(int) }, (Type[])null))?.Invoke(access, new object[1] { track }), mixSec);
			}
			catch
			{
			}
		}

		private static void TrySetMixOnState(MegaAnimationState state, int track, float mixSec)
		{
			try
			{
				SetEntryMixDuration(AccessTools.Method(typeof(MegaAnimationState), "GetCurrent", new Type[1] { typeof(int) }, (Type[])null)?.Invoke(state, new object[1] { track }), mixSec);
			}
			catch
			{
			}
		}

		private static void SetEntryMixDuration(object? entry, float mix)
		{
			if (entry == null || mix < 0.01f)
			{
				return;
			}
			Type type = entry.GetType();
			try
			{
				MethodInfo methodInfo = AccessTools.Method(type, "SetMixDuration", new Type[1] { typeof(float) }, (Type[])null) ?? AccessTools.Method(type, "set_mix_duration", new Type[1] { typeof(float) }, (Type[])null);
				if (methodInfo != null)
				{
					methodInfo.Invoke(entry, new object[1] { mix });
				}
				else
				{
					(AccessTools.Property(type, "MixDuration") ?? AccessTools.Property(type, "mix_duration"))?.SetValue(entry, mix, null);
				}
			}
			catch
			{
			}
		}

		private static object? GetTrackEntry(object host, int track)
		{
			try
			{
				return (AccessTools.Method(host.GetType(), "GetCurrent", new Type[1] { typeof(int) }, (Type[])null) ?? AccessTools.Method(host.GetType(), "GetCurrentTrack", new Type[1] { typeof(int) }, (Type[])null))?.Invoke(host, new object[1] { track });
			}
			catch
			{
				return null;
			}
		}

		private static void SeekTrackNearEnd(object? entry, float keepTailSec)
		{
			if (entry == null)
			{
				return;
			}
			Type type = entry.GetType();
			try
			{
				float num = 1f;
				float val = 0f;
				MethodInfo methodInfo = AccessTools.Method(type, "GetAnimationEnd", Type.EmptyTypes, (Type[])null) ?? AccessTools.Method(type, "get_AnimationEnd", Type.EmptyTypes, (Type[])null);
				MethodInfo methodInfo2 = AccessTools.Method(type, "GetAnimationStart", Type.EmptyTypes, (Type[])null) ?? AccessTools.Method(type, "get_AnimationStart", Type.EmptyTypes, (Type[])null);
				if (methodInfo != null)
				{
					num = Convert.ToSingle(methodInfo.Invoke(entry, null));
				}
				if (methodInfo2 != null)
				{
					val = Convert.ToSingle(methodInfo2.Invoke(entry, null));
				}
				float num2 = Math.Max(num - Math.Max(keepTailSec, 0.02f), val);
				MethodInfo methodInfo3 = AccessTools.Method(type, "SetTrackTime", new Type[1] { typeof(float) }, (Type[])null) ?? AccessTools.Method(type, "set_TrackTime", new Type[1] { typeof(float) }, (Type[])null);
				MethodInfo methodInfo4 = AccessTools.Method(type, "SetAnimationLast", new Type[1] { typeof(float) }, (Type[])null) ?? AccessTools.Method(type, "set_AnimationLast", new Type[1] { typeof(float) }, (Type[])null);
				methodInfo3?.Invoke(entry, new object[1] { num2 });
				methodInfo4?.Invoke(entry, new object[1] { num2 });
			}
			catch
			{
			}
		}

		internal static float BeginBridgeToLowHealth(SpineAnimationAccess access, MegaSprite? sprite, string? fromAnim)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			string text = (string.IsNullOrEmpty(fromAnim) ? GetCurrentAnimationName(access) : fromAnim);
			if (!string.IsNullOrEmpty(text) && text.Equals("low_health_loop", StringComparison.OrdinalIgnoreCase))
			{
				return 0f;
			}
			float num = MixDurationFor(text, "low_health_loop");
			if (num < 0f || num <= 0.05f)
			{
				return 0f;
			}
			TrySetAnimation(access, "low_health_loop", loop: true);
			if (sprite != null)
			{
				TrySetAnimation(sprite, "low_health_loop", loop: true);
			}
			ForceTrackMix(access, sprite, num);
			TryFlush(sprite);
			return num;
		}

		internal static float ReadDefaultMixSeconds(Node? spine)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			if (spine == null || !GodotObject.IsInstanceValid((GodotObject)(object)spine))
			{
				return 0.2f;
			}
			try
			{
				Variant val = ((GodotObject)spine).Get(StringName.op_Implicit("skeleton_data_res"));
				GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
				if (val2 == null)
				{
					return 0.2f;
				}
				Variant val3 = val2.Get(StringName.op_Implicit("default_mix"));
				float num = ((Variant)(ref val3)).AsSingle();
				return (num > 0.01f) ? num : 0.2f;
			}
			catch
			{
				return 0.2f;
			}
		}

		internal static void AddAnimation(SpineAnimationAccess access, string name, float delay, bool loop, int track = 0)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			if (AccessAddAnimationMethod == null)
			{
				return;
			}
			name = LowHpAnimRedirectPatch.Redirect(name, allowPromote: false);
			try
			{
				AccessAddAnimationMethod.Invoke(access, new object[4] { name, delay, loop, track });
			}
			catch (Exception ex) when (IsTransientSpineError(ex))
			{
			}
		}

		internal static string? GetCurrentAnimationName(SpineAnimationAccess access, int track = 0)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			if (AccessGetCurrentAnimationNameMethod == null)
			{
				return null;
			}
			try
			{
				return AccessGetCurrentAnimationNameMethod.Invoke(access, new object[1] { track }) as string;
			}
			catch
			{
				return null;
			}
		}

		internal static string? GetCurrentAnimationName(MegaAnimationState state, int track = 0)
		{
			if (StateGetCurrentAnimationNameMethod == null || state == null)
			{
				return null;
			}
			try
			{
				return StateGetCurrentAnimationNameMethod.Invoke(state, new object[1] { track }) as string;
			}
			catch
			{
				return null;
			}
		}

		internal static bool TryFlush(MegaSprite? sprite, Node? spineNode = null)
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				GodotObject val = (GodotObject)(object)spineNode;
				if (val == null && sprite != null)
				{
					val = ((MegaSpineBinding)sprite).BoundObject;
				}
				if (val != null && GodotObject.IsInstanceValid(val) && val.HasMethod(StringName.op_Implicit("update_skeleton")))
				{
					val.Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
					return true;
				}
			}
			catch
			{
			}
			if (sprite == null)
			{
				return false;
			}
			try
			{
				MegaAnimationState val2 = sprite.TryGetAnimationState();
				if (val2 == null)
				{
					return false;
				}
				AccessTools.Method(typeof(MegaAnimationState), "Update", new Type[1] { typeof(float) }, (Type[])null)?.Invoke(val2, new object[1] { 0f });
				object obj2 = AccessTools.Method(typeof(MegaSprite), "GetSkeleton", (Type[])null, (Type[])null)?.Invoke(sprite, null);
				if (obj2 == null)
				{
					return false;
				}
				MethodInfo methodInfo = AccessTools.Method(typeof(MegaAnimationState), "Apply", new Type[1] { obj2.GetType() }, (Type[])null);
				methodInfo?.Invoke(val2, new object[1] { obj2 });
				return methodInfo != null;
			}
			catch
			{
				return false;
			}
		}

		private static void ForceTrackMix(SpineAnimationAccess? access, MegaSprite? sprite, float mixSec)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			object obj = null;
			try
			{
				object obj2;
				if (!access.HasValue)
				{
					obj2 = null;
				}
				else
				{
					SpineAnimationAccess valueOrDefault = access.GetValueOrDefault();
					obj2 = ((SpineAnimationAccess)(ref valueOrDefault)).GetCurrentTrack(0);
				}
				obj = obj2;
			}
			catch
			{
			}
			if (obj == null && sprite != null)
			{
				try
				{
					MegaAnimationState val = sprite.TryGetAnimationState();
					if (val != null)
					{
						obj = AccessTools.Method(typeof(MegaAnimationState), "GetCurrent", new Type[1] { typeof(int) }, (Type[])null)?.Invoke(val, new object[1] { 0 });
					}
				}
				catch
				{
				}
			}
			if (obj == null)
			{
				return;
			}
			try
			{
				Type type = obj.GetType();
				MethodInfo methodInfo = AccessTools.Method(type, "SetMixDuration", new Type[1] { typeof(float) }, (Type[])null) ?? AccessTools.Method(type, "set_mix_duration", new Type[1] { typeof(float) }, (Type[])null);
				if (methodInfo != null)
				{
					methodInfo.Invoke(obj, new object[1] { mixSec });
				}
				else
				{
					(AccessTools.Property(type, "MixDuration") ?? AccessTools.Property(type, "mix_duration"))?.SetValue(obj, mixSec, null);
				}
			}
			catch
			{
			}
		}

		internal static bool HardSetCombatLoop(SpineAnimationAccess access, MegaSprite? sprite, string name, Node? spineNode = null)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			TrySetAnimation(access, name, loop: true);
			if (sprite != null)
			{
				TrySetAnimation(sprite, name, loop: true);
			}
			ForceTrackMix(access, sprite, 0f);
			bool flag = TryFlush(sprite, spineNode);
			if (spineNode != null)
			{
				return TryFlush(sprite, spineNode) || flag;
			}
			return TryFlush(sprite) || flag;
		}

		internal static float SoftMixCombatLoop(SpineAnimationAccess access, MegaSprite? sprite, string name)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			float num = MixDurationFor(GetCurrentAnimationName(access), name);
			if (num < 0f || num <= 0.05f)
			{
				return 0f;
			}
			TrySetAnimation(access, name, loop: true);
			if (sprite != null)
			{
				TrySetAnimation(sprite, name, loop: true);
			}
			ForceTrackMix(access, sprite, num);
			return num;
		}
	}
	internal static class BattleWear
	{
		internal sealed class WearSet
		{
			public string PathBase = "";

			public string Prefix = "";

			public int PageW;

			public int PageH;

			public Resource? Pristine;

			public readonly Resource?[] Stage = (Resource[])(object)new Resource[6];
		}

		internal sealed class WearProfile
		{
			public string Name = "";

			public Type CharacterType;

			public string[] SkinPckCandidates = Array.Empty<string>();

			public float[] Thresholds = Array.Empty<float>();

			public WearSet Combat;

			public WearSet? Rest;

			public WearSet? Merchant;

			public string FallbackIdle = "idle";

			public bool SkinChecked;

			public bool SkinInstalled;
		}

		internal sealed class CombatWearState
		{
			public NCreature Node;

			public WearProfile Profile;

			public int Stage = 1;

			public bool ScaleApplied;

			public float AppliedScale = 1f;

			public int IdleWaitTicks;

			public bool Fresh = true;

			public int LastHp = -1;

			public int LastMaxHp = -1;

			public ulong AppliedSkelId;

			public int PendingStage;

			public int MixDeferTicks;

			public long MixDeferUntilMs;

			public bool LowBridgeDone;

			public string? PendingAnimHint;

			public int IdleFixCooldown;
		}

		internal sealed class RestWearState
		{
			public NRestSiteCharacter Node;

			public Node? Spine;

			public int Stage = 1;

			public bool Fresh = true;

			public int LastHp = -1;

			public int LastMaxHp = -1;

			public ulong AppliedSkelId;
		}

		internal sealed class MerchantWearState
		{
			public NMerchantCharacter Node;

			public Node? Spine;

			public int Stage = 1;

			public bool Fresh = true;

			public int LastHp = -1;

			public int LastMaxHp = -1;

			public ulong AppliedSkelId;
		}

		internal readonly struct MegaScope : IDisposable
		{
			private readonly MegaSprite _sprite;

			public MegaScope(Node spine)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Expected O, but got Unknown
				_sprite = new MegaSprite(Variant.op_Implicit((GodotObject)spine));
			}

			public static implicit operator MegaSprite(MegaScope s)
			{
				return s._sprite;
			}

			public void Dispose()
			{
			}
		}

		[CompilerGenerated]
		private sealed class <EnumerateSpineSprites>d__45 : IEnumerable<Node2D>, IEnumerable, IEnumerator<Node2D>, IEnumerator, IDisposable
		{
			private int <>1__state;

			private Node2D <>2__current;

			private int <>l__initialThreadId;

			private Node root;

			public Node <>3__root;

			private IEnumerator<Node> <>7__wrap1;

			private Node <child>5__3;

			private IEnumerator<Node2D> <>7__wrap3;

			Node2D IEnumerator<Node2D>.Current
			{
				[DebuggerHidden]
				get
				{
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <EnumerateSpineSprites>d__45(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				int num = <>1__state;
				if ((uint)(num - -4) <= 1u || (uint)(num - 1) <= 1u)
				{
					try
					{
						if (num == -4 || num == 2)
						{
							try
							{
							}
							finally
							{
								<>m__Finally2();
							}
						}
					}
					finally
					{
						<>m__Finally1();
					}
				}
				<>7__wrap1 = null;
				<child>5__3 = null;
				<>7__wrap3 = null;
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				try
				{
					switch (<>1__state)
					{
					default:
						return false;
					case 0:
						<>1__state = -1;
						<>7__wrap1 = root.GetChildren(false).GetEnumerator();
						<>1__state = -3;
						goto IL_0101;
					case 1:
						<>1__state = -3;
						goto IL_009a;
					case 2:
						{
							<>1__state = -4;
							goto IL_00e0;
						}
						IL_0101:
						if (<>7__wrap1.MoveNext())
						{
							<child>5__3 = <>7__wrap1.Current;
							Node obj = <child>5__3;
							Node2D val = (Node2D)(object)((obj is Node2D) ? obj : null);
							if (val != null && ((GodotObject)val).GetClass() == "SpineSprite")
							{
								<>2__current = val;
								<>1__state = 1;
								return true;
							}
							goto IL_009a;
						}
						<>m__Finally1();
						<>7__wrap1 = null;
						return false;
						IL_009a:
						<>7__wrap3 = EnumerateSpineSprites(<child>5__3).GetEnumerator();
						<>1__state = -4;
						goto IL_00e0;
						IL_00e0:
						if (<>7__wrap3.MoveNext())
						{
							Node2D current = <>7__wrap3.Current;
							<>2__current = current;
							<>1__state = 2;
							return true;
						}
						<>m__Finally2();
						<>7__wrap3 = null;
						<child>5__3 = null;
						goto IL_0101;
					}
				}
				catch
				{
					//try-fault
					((IDisposable)this).Dispose();
					throw;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			private void <>m__Finally1()
			{
				<>1__state = -1;
				if (<>7__wrap1 != null)
				{
					<>7__wrap1.Dispose();
				}
			}

			private void <>m__Finally2()
			{
				<>1__state = -3;
				if (<>7__wrap3 != null)
				{
					<>7__wrap3.Dispose();
				}
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Node2D> IEnumerable<Node2D>.GetEnumerator()
			{
				<EnumerateSpineSprites>d__45 <EnumerateSpineSprites>d__;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					<EnumerateSpineSprites>d__ = this;
				}
				else
				{
					<EnumerateSpineSprites>d__ = new <EnumerateSpineSprites>d__45(0);
				}
				<EnumerateSpineSprites>d__.root = <>3__root;
				return <EnumerateSpineSprites>d__;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Node2D>)this).GetEnumerator();
			}
		}

		private const int IdleWaitMaxTicks = 50;

		private static readonly FieldInfo? MerchantPlayersField = AccessTools.Field(typeof(NMerchantRoom), "_players");

		private static readonly List<WearProfile> Profiles = new List<WearProfile>();

		private static bool _profilesBuilt;

		internal static readonly Dictionary<ulong, CombatWearState> CombatStates = new Dictionary<ulong, CombatWearState>();

		internal static readonly Dictionary<ulong, RestWearState> RestStates = new Dictionary<ulong, RestWearState>();

		internal static readonly Dictionary<ulong, MerchantWearState> MerchantStates = new Dictionary<ulong, MerchantWearState>();

		internal static readonly List<NRestSiteCharacter> RestChars = new List<NRestSiteCharacter>();

		private static readonly List<ulong> Stale = new List<ulong>();

		internal static bool _fuse;

		internal static bool SuppressLowHealthPromote;

		private static readonly HashSet<string> KnownCharacters = new HashSet<string>(StringComparer.Ordinal) { "defect", "ironclad", "regent" };

		private static bool IsWearIdleAnimation(string? anim)
		{
			if (string.IsNullOrEmpty(anim))
			{
				return true;
			}
			if (anim.Contains("idle", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			return anim.Equals("low_health_loop", StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsCombatBusyAnimation(string? anim)
		{
			if (string.IsNullOrEmpty(anim))
			{
				return true;
			}
			if (!anim.Contains("attack", StringComparison.OrdinalIgnoreCase) && !anim.Contains("cast", StringComparison.OrdinalIgnoreCase) && !anim.Equals("hurt", StringComparison.OrdinalIgnoreCase) && !anim.Equals("hit", StringComparison.OrdinalIgnoreCase) && !anim.Contains("die", StringComparison.OrdinalIgnoreCase))
			{
				return anim.Contains("death", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}

		private static bool IsLowHealthForIdle(Creature creature)
		{
			return LowHealthIdleRules.MatchesCreature(creature);
		}

		private static bool IsLowHealthIdleName(string? anim)
		{
			if (!string.IsNullOrEmpty(anim))
			{
				return anim.Equals("low_health_loop", StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		private static bool CanPlayLowHealth(MegaSprite? sprite)
		{
			try
			{
				return sprite != null && sprite.HasAnimation("low_health_loop");
			}
			catch
			{
				return false;
			}
		}

		private static string NormalizeIdleAnimation(string? anim, string fallback)
		{
			if (string.IsNullOrEmpty(anim))
			{
				return fallback;
			}
			return anim;
		}

		internal static void BuildProfiles()
		{
			if (_profilesBuilt)
			{
				return;
			}
			_profilesBuilt = true;
			try
			{
				WearProfile wearProfile = new WearProfile();
				wearProfile.Name = "ironclad";
				wearProfile.CharacterType = typeof(Ironclad);
				wearProfile.Thresholds = new float[1] { 0.5f };
				wearProfile.FallbackIdle = "idle_loop";
				wearProfile.Combat = new WearSet
				{
					PathBase = "succubus_ironclad_hpwear/ironclad",
					Prefix = "stage",
					PageW = 2177,
					PageH = 2177
				};
				wearProfile.Rest = new WearSet
				{
					PathBase = "succubus_ironclad_hpwear/ironclad",
					Prefix = "rest_stage",
					PageW = 3207,
					PageH = 3207
				};
				wearProfile.Merchant = new WearSet
				{
					PathBase = "succubus_ironclad_hpwear/ironclad",
					Prefix = "shop_stage",
					PageW = 2177,
					PageH = 2177
				};
				List<string> list = new List<string>();
				AddOwnPckCandidate(list);
				wearProfile.SkinPckCandidates = list.ToArray();
				if (OwnsCharacter("ironclad"))
				{
					Profiles.Add(wearProfile);
				}
			}
			catch (Exception value)
			{
				Fail($"profile setup failed: {value}");
			}
		}

		internal static bool OwnsCharacter(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}
			if (!ModEntry.ModId.Equals("succubusIroncladHpWear", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			return name.Equals("ironclad", StringComparison.OrdinalIgnoreCase);
		}

		private static void AddOwnPckCandidate(List<string> candidates)
		{
			try
			{
				string directoryName = Path.GetDirectoryName(typeof(BattleWear).Assembly.Location);
				if (!string.IsNullOrEmpty(directoryName))
				{
					candidates.Add(Path.Combine(directoryName, ModEntry.ModId + ".pck"));
				}
			}
			catch
			{
			}
		}

		internal static bool SkinReady(WearProfile p)
		{
			if (!p.SkinChecked)
			{
				p.SkinChecked = true;
				try
				{
					string text = Array.Find(p.SkinPckCandidates, File.Exists);
					p.SkinInstalled = text != null;
					ModEntry.Log.Info(p.SkinInstalled ? (p.Name + ": skin pck found: " + text) : (p.Name + ": skin pck not found; looked at: " + string.Join(" | ", p.SkinPckCandidates) + " — wear disabled for this character."), 1);
				}
				catch
				{
					p.SkinInstalled = false;
				}
			}
			return p.SkinInstalled;
		}

		internal static string DescribeSkins()
		{
			BuildProfiles();
			List<string> list = new List<string>();
			foreach (WearProfile profile in Profiles)
			{
				list.Add(profile.Name + "=" + (SkinReady(profile) ? "armed" : "no-skin"));
			}
			return string.Join(", ", list);
		}

		internal static WearProfile? ProfileFor(Player? player)
		{
			if (!SkinSelectionGate.ForTarget(player))
			{
				return null;
			}
			if (((player != null) ? player.Character : null) == null)
			{
				return null;
			}
			foreach (WearProfile profile in Profiles)
			{
				if (profile.CharacterType.IsInstanceOfType(player.Character) && OwnsCharacter(profile.Name))
				{
					return profile;
				}
			}
			return null;
		}

		internal static void Tick()
		{
			if (!SkinSelectionGate.BeforeTick())
			{
				return;
			}
			try
			{
				WearStageCrossfade.Tick(0.12);
				WearSafetyHotkey.EnsurePollerFromTree("tick");
				if (_fuse)
				{
					return;
				}
				BuildProfiles();
				if (Profiles.Count == 0)
				{
					return;
				}
				bool flag = NCombatRoom.Instance != null;
				bool flag2 = NRestSiteRoom.Instance != null;
				bool flag3 = NMerchantRoom.Instance != null;
				if (flag || flag2 || flag3)
				{
					if (flag)
					{
						TickCombat();
					}
					else if (CombatStates.Count > 0)
					{
						CombatStates.Clear();
						WearStageRuntime.ClearCombatSnapshot();
					}
					if (!_fuse && flag2)
					{
						TickRestSite();
					}
					else if (RestStates.Count > 0)
					{
						RestStates.Clear();
					}
					if (!_fuse && flag3)
					{
						TickMerchant();
					}
					else if (MerchantStates.Count > 0)
					{
						MerchantStates.Clear();
					}
				}
			}
			catch (Exception ex)
			{
				FailOrRetry($"tick failed: {ex}", ex);
			}
		}

		private static void TickCombat()
		{
			//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Expected O, but got Unknown
			//IL_0632: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Expected O, but got Unknown
			//IL_0cbd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a6d: Unknown result type (might be due to invalid IL or missing references)
			if (!SkinSelectionGate.BeforeTick())
			{
				return;
			}
			if (NCombatRoom.Instance == null)
			{
				if (CombatStates.Count > 0)
				{
					CombatStates.Clear();
				}
				WearStageRuntime.ClearCombatSnapshot();
				return;
			}
			foreach (KeyValuePair<ulong, CombatWearState> combatState in CombatStates)
			{
				if (combatState.Value.Node == null || !GodotObject.IsInstanceValid((GodotObject)combatState.Value.Node))
				{
					Stale.Add(combatState.Key);
				}
			}
			foreach (ulong item in Stale)
			{
				CombatStates.Remove(item);
			}
			Stale.Clear();
			RunManager instance = RunManager.Instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				RunState val = instance.DebugOnlyGetState();
				obj = ((val != null) ? val.Players : null);
			}
			IReadOnlyList<Player> readOnlyList = (IReadOnlyList<Player>)obj;
			if (readOnlyList == null)
			{
				return;
			}
			foreach (Player item2 in readOnlyList)
			{
				WearProfile wearProfile = ProfileFor(item2);
				if (wearProfile == null || !SkinReady(wearProfile))
				{
					continue;
				}
				Creature creature = item2.Creature;
				if (creature == null || creature.IsDead || creature.CurrentHp <= 0)
				{
					if (creature != null && creature.CurrentHp <= 0)
					{
						LowWearAttackGate.BypassForDeath("tick-hp0");
					}
					continue;
				}
				NCombatRoom instance2 = NCombatRoom.Instance;
				NCreature val2 = ((instance2 != null) ? instance2.GetCreatureNode(creature) : null);
				if (val2 == null || !GodotObject.IsInstanceValid((GodotObject)val2))
				{
					continue;
				}
				NCreatureVisuals visuals = val2.Visuals;
				MegaSprite val3 = ((visuals != null) ? visuals.SpineBody : null);
				if (visuals == null || val3 == null)
				{
					continue;
				}
				ulong instanceId = ((GodotObject)val2).GetInstanceId();
				int currentHp = creature.CurrentHp;
				int maxHp = creature.MaxHp;
				int num = StageFor(wearProfile, creature);
				if (!CombatStates.TryGetValue(instanceId, out CombatWearState value))
				{
					WearStageRuntime.CaptureCombatSnapshot();
					value = new CombatWearState
					{
						Node = val2,
						Profile = wearProfile
					};
					CombatStates[instanceId] = value;
				}
				float num2 = (float)WearStageRuntime.GetLive().VisualScale;
				if (num2 < 0.5f)
				{
					num2 = 0.5f;
				}
				if (num2 > 2f)
				{
					num2 = 2f;
				}
				if (!value.ScaleApplied || Math.Abs(value.AppliedScale - num2) > 0.001f)
				{
					try
					{
						val2.SetScaleAndHue(num2, 0f);
						value.ScaleApplied = true;
						value.AppliedScale = num2;
						if (Math.Abs(num2 - 1f) > 0.001f)
						{
							ModEntry.Log.Info($"{wearProfile.Name}: visual scale set to {num2:0.00}x.", 1);
						}
					}
					catch (Exception ex)
					{
						ModEntry.Log.Warn(wearProfile.Name + ": SetScaleAndHue failed: " + ex.Message, 1);
					}
				}
				GodotObject boundObject = ((MegaSpineBinding)val3).BoundObject;
				Node val4 = (Node)(object)((boundObject is Node) ? boundObject : null);
				bool flag = value.AppliedSkelId == 0L && !SpinePageMatches(val4, wearProfile.Combat);
				ulong num3 = SpineSkelId(val4);
				bool flag2 = value.AppliedSkelId != 0L && num3 != 0L && num3 != value.AppliedSkelId;
				SpineAnimationAccess spineAnimation = visuals.SpineAnimation;
				string currentAnimationName = SpineAnimCompat.GetCurrentAnimationName(spineAnimation);
				ReconcileLowHealthIdle(creature, value, currentAnimationName);
				bool flag3 = IsLowHealthForIdle(creature);
				bool flag4 = IsWearIdleAnimation(currentAnimationName);
				bool flag5 = !string.IsNullOrEmpty(currentAnimationName) && (currentAnimationName.Equals("hurt", StringComparison.OrdinalIgnoreCase) || currentAnimationName.Equals("hit", StringComparison.OrdinalIgnoreCase));
				bool flag6 = string.Equals(ModEntry.ModId, "defectHpWear", StringComparison.OrdinalIgnoreCase);
				bool flag7 = WearStageRuntime.NeedsCombatSkeletonApply(value.Stage, num, value.AppliedSkelId, num3, !flag);
				bool flag8 = flag3 && CanPlayLowHealth(val3);
				bool flag9 = flag6 && flag3 && !flag7 && value.PendingStage == 0 && !flag5 && !IsAttackOrCastBusy(currentAnimationName) && !IsLowHealthIdleName(currentAnimationName);
				if (value.IdleFixCooldown > 0)
				{
					value.IdleFixCooldown--;
				}
				bool flag10 = flag3 && LowWearAttackGate.ShouldHoldLowBridge(currentAnimationName, creature);
				if (value.PendingStage == 0 && !flag10 && !value.Fresh && value.LastHp == currentHp && value.LastMaxHp == maxHp && !flag7 && !flag9)
				{
					continue;
				}
				value.LastHp = currentHp;
				value.LastMaxHp = maxHp;
				if (flag3 && !value.Fresh && (flag7 || flag9) && value.IdleWaitTicks < 50 && LowWearAttackGate.ShouldHoldLowBridge(currentAnimationName, creature))
				{
					value.IdleWaitTicks++;
					continue;
				}
				if (flag6 && !flag7 && value.PendingStage == 0 && !value.Fresh && flag9 && value.IdleFixCooldown <= 0)
				{
					if (val4 != null)
					{
						SpineAnimCompat.EnsureIdleLowHealthMixes(val4, null);
					}
					float num4 = SpineAnimCompat.MixDurationFor(currentAnimationName, "low_health_loop");
					if (num4 < 0f)
					{
						num4 = 0.3f;
					}
					if (SpineAnimCompat.TrySetAnimation(spineAnimation, "low_health_loop", loop: true, 0, num4) || SpineAnimCompat.TrySetAnimation(val3, "low_health_loop", loop: true, 0, num4))
					{
						value.IdleFixCooldown = 6;
						ModEntry.Log.Info($"{wearProfile.Name}: ≤25% no-swap — mix {num4:0.##}s → low_health (was '{currentAnimationName}', hp {currentHp}/{maxHp}).", 1);
					}
					value.IdleWaitTicks = 0;
					continue;
				}
				if (!flag7 && value.PendingStage == 0 && !value.Fresh)
				{
					value.IdleWaitTicks = 0;
					continue;
				}
				if (flag2)
				{
					ModEntry.Log.Info($"{wearProfile.Name}: combat skeleton replaced after apply — reapply stage {num}.", 1);
				}
				if (flag3)
				{
					if (!IsSafeToSwapSkeleton(currentAnimationName) && value.IdleWaitTicks < 50)
					{
						value.IdleWaitTicks++;
						continue;
					}
				}
				else if (!IsSafeToSwapSkeleton(currentAnimationName) && value.IdleWaitTicks < 50)
				{
					value.IdleWaitTicks++;
					continue;
				}
				value.IdleWaitTicks = 0;
				if (val4 != null && WearStageCrossfade.IsActive(val4))
				{
					continue;
				}
				string text = (flag5 ? "hurt" : (flag8 ? "low_health_loop" : ((!flag4) ? null : NormalizeIdleAnimation(currentAnimationName, wearProfile.FallbackIdle))));
				if (value.PendingStage != 0)
				{
					value.PendingStage = num;
					long num5 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
					if (value.MixDeferUntilMs > 0 && num5 < value.MixDeferUntilMs)
					{
						continue;
					}
					if (value.MixDeferUntilMs <= 0 && value.MixDeferTicks > 0)
					{
						value.MixDeferTicks--;
						continue;
					}
					string text2 = value.PendingAnimHint ?? (flag3 ? "low_health_loop" : wearProfile.FallbackIdle);
					int pendingStage = value.PendingStage;
					value.PendingStage = 0;
					value.PendingAnimHint = null;
					value.MixDeferTicks = 0;
					value.MixDeferUntilMs = 0L;
					SuppressLowHealthPromote = false;
					ModEntry.Log.Info($"{wearProfile.Name}: deferred skel swap → stage {pendingStage} (bridge mix done → ghost '{text2}').", 1);
					ApplyCombatStage(wearProfile, creature, visuals, val3, value, pendingStage, text2);
					if (_fuse)
					{
						break;
					}
				}
				else if (!value.Fresh && flag7)
				{
					string text3 = (string.IsNullOrEmpty(currentAnimationName) ? wearProfile.FallbackIdle : currentAnimationName);
					if (flag8)
					{
						if (IsLowHealthIdleName(currentAnimationName))
						{
							ModEntry.Log.Info($"{wearProfile.Name}: skel {value.Stage}->{num} — already low_health, ghost swap (no re-mix).", 1);
							ApplyCombatStage(wearProfile, creature, visuals, val3, value, num, "low_health_loop");
							if (!_fuse)
							{
								continue;
							}
							break;
						}
						if (val4 != null)
						{
							SpineAnimCompat.EnsureIdleLowHealthMixes(val4, null);
						}
						SuppressLowHealthPromote = true;
						float num6 = SpineAnimCompat.BeginBridgeToLowHealth(spineAnimation, val3, text3);
						if (num6 <= 0.05f)
						{
							SuppressLowHealthPromote = false;
							ModEntry.Log.Info($"{wearProfile.Name}: skel {value.Stage}->{num} — no usable mix, ghost swap low_health.", 1);
							ApplyCombatStage(wearProfile, creature, visuals, val3, value, num, "low_health_loop");
							if (_fuse)
							{
								break;
							}
						}
						else
						{
							float num7 = num6 + 0.05f;
							int mixDeferTicks = Math.Max(1, (int)Math.Ceiling((double)num7 / 0.12));
							value.PendingStage = num;
							value.PendingAnimHint = "low_health_loop";
							value.MixDeferTicks = mixDeferTicks;
							value.MixDeferUntilMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)((double)num7 * 1000.0);
							value.LowBridgeDone = true;
							ModEntry.Log.Info($"{wearProfile.Name}: defer skel {value.Stage}->{num} — skin mix '{text3}'→low_health {num6:0.##}s then swap.", 1);
						}
					}
					else if (flag3)
					{
						ModEntry.Log.Info($"{wearProfile.Name}: skel {value.Stage}->{num} — ≤25% but no low_health clip, direct ghost idle.", 1);
						ApplyCombatStage(wearProfile, creature, visuals, val3, value, num, wearProfile.FallbackIdle);
						if (_fuse)
						{
							break;
						}
					}
					else if (IsLowHealthIdleName(currentAnimationName))
					{
						float num8 = SpineAnimCompat.SoftMixCombatLoop(spineAnimation, val3, wearProfile.FallbackIdle);
						if (num8 <= 0.05f)
						{
							ModEntry.Log.Info($"{wearProfile.Name}: skel {value.Stage}->{num} — demote no mix, direct ghost idle.", 1);
							ApplyCombatStage(wearProfile, creature, visuals, val3, value, num, wearProfile.FallbackIdle);
							if (_fuse)
							{
								break;
							}
						}
						else
						{
							float num9 = num8 + 0.05f;
							int mixDeferTicks2 = Math.Max(1, (int)Math.Ceiling((double)num9 / 0.12));
							value.PendingStage = num;
							value.PendingAnimHint = wearProfile.FallbackIdle;
							value.MixDeferTicks = mixDeferTicks2;
							value.MixDeferUntilMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)((double)num9 * 1000.0);
							ModEntry.Log.Info($"{wearProfile.Name}: defer skel {value.Stage}->{num} — demote skin mix {num8:0.##}s then ghost.", 1);
						}
					}
					else
					{
						ModEntry.Log.Info($"{wearProfile.Name}: skel {value.Stage}->{num} — need-swap + >25% direct swap hint='{text}'.", 1);
						ApplyCombatStage(wearProfile, creature, visuals, val3, value, num, text);
						if (_fuse)
						{
							break;
						}
					}
				}
				else
				{
					ApplyCombatStage(wearProfile, creature, visuals, val3, value, num, text);
					if (_fuse)
					{
						break;
					}
				}
			}
		}

		private static bool IsAttackOrCastBusy(string? anim)
		{
			if (string.IsNullOrEmpty(anim))
			{
				return false;
			}
			if (!anim.Contains("attack", StringComparison.OrdinalIgnoreCase) && !anim.Contains("cast", StringComparison.OrdinalIgnoreCase) && !anim.Equals("hurt", StringComparison.OrdinalIgnoreCase) && !anim.Equals("hit", StringComparison.OrdinalIgnoreCase) && !anim.Contains("die", StringComparison.OrdinalIgnoreCase))
			{
				return anim.Contains("death", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}

		private static bool IsSafeToSwapSkeleton(string? anim)
		{
			if (string.IsNullOrEmpty(anim))
			{
				return true;
			}
			if (anim.Equals("relaxed_loop", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (IsLowHealthIdleName(anim))
			{
				return true;
			}
			return IsWearIdleAnimation(anim);
		}

		internal static void ApplyCombatStage(WearProfile profile, Creature creature, NCreatureVisuals visuals, MegaSprite sprite, CombatWearState st, int stage, string? currentAnim)
		{
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Expected O, but got Unknown
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Expected O, but got Unknown
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Expected O, but got Unknown
			//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0305: Unknown result type (might be due to invalid IL or missing references)
			//IL_037c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0386: Expected O, but got Unknown
			//IL_0381: Unknown result type (might be due to invalid IL or missing references)
			if (!SkinSelectionGate.ForTarget(creature))
			{
				return;
			}
			Creature creature2 = creature;
			Node val;
			Resource val5;
			bool wantLow;
			bool replayHurt;
			try
			{
				GodotObject boundObject = ((MegaSpineBinding)sprite).BoundObject;
				val = (Node)((boundObject is Node) ? boundObject : null);
				if (val == null || !GodotObject.IsInstanceValid((GodotObject)val))
				{
					return;
				}
				Variant val2 = ((GodotObject)val).Get(StringName.op_Implicit("skeleton_data_res"));
				GodotObject val3 = ((Variant)(ref val2)).AsGodotObject();
				Resource val4 = (Resource)((val3 is Resource) ? val3 : null);
				if (val4 == null)
				{
					Fail(profile.Name + ": SpineSprite has no skeleton_data_res");
					return;
				}
				WearSet combat = profile.Combat;
				if (combat.Pristine == null)
				{
					(int, int)? tuple = PageSizeOf(val4);
					if (tuple.HasValue)
					{
						var (num, num2) = tuple.GetValueOrDefault();
						if (num == combat.PageW && num2 == combat.PageH)
						{
							combat.Pristine = val4;
							SpineAnimCompat.EnsureIdleLowHealthMixes(val, val4);
							ModEntry.Log.Info(profile.Name + ": combat pristine skeleton data captured (page size verified).", 1);
						}
						else
						{
							ModEntry.Log.Info($"{profile.Name}: live combat atlas page is {num}x{num2}, expected {combat.PageW}x{combat.PageH} — fallback apply (SkinChanger switch).", 1);
						}
					}
					else if (stage <= 1)
					{
						ModEntry.Log.Warn(profile.Name + ": live combat skeleton data has no readable atlas page (will retry).", 1);
						return;
					}
				}
				val5 = ((stage <= 1) ? combat.Pristine : GetStageData(profile, combat, stage));
				if (val5 != null)
				{
					wantLow = IsLowHealthForIdle(creature2);
					replayHurt = !string.IsNullOrEmpty(currentAnim) && (currentAnim.Equals("hurt", StringComparison.OrdinalIgnoreCase) || currentAnim.Equals("hit", StringComparison.OrdinalIgnoreCase));
					bool flag = ((GodotObject)val4).GetInstanceId() != ((GodotObject)val5).GetInstanceId();
					if (!(!st.Fresh && flag) || WearStageCrossfade.SmoothUnavailable || !WearStageCrossfade.TryBegin(val, val5, ApplyAfterSwap))
					{
						((GodotObject)val).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)val5));
						ApplyAfterSwap();
					}
				}
			}
			catch (Exception ex)
			{
				FailOrRetry($"{profile.Name}: combat stage apply failed: {ex}", ex);
			}
			void ApplyAfterSwap()
			{
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_008a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0051: Unknown result type (might be due to invalid IL or missing references)
				//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
				//IL_0105: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
				//IL_0189: Unknown result type (might be due to invalid IL or missing references)
				//IL_0148: Unknown result type (might be due to invalid IL or missing references)
				//IL_019d: Unknown result type (might be due to invalid IL or missing references)
				//IL_015c: Unknown result type (might be due to invalid IL or missing references)
				SpineAnimCompat.EnsureIdleLowHealthMixes(val, val5);
				LowHpAnimRedirectPatch.NoteLowHealthClip(sprite);
				if (wantLow)
				{
					LowHpAnimRedirectPatch.NoteLowHealthClip(ready: true);
				}
				SpineAnimationAccess spineAnimation = visuals.SpineAnimation;
				string text;
				if (wantLow && CanPlayLowHealth(sprite))
				{
					text = "low_health_loop";
					SpineAnimCompat.HardSetCombatLoop(spineAnimation, sprite, text, val);
					st.IdleFixCooldown = 6;
				}
				else if (wantLow)
				{
					text = profile.FallbackIdle;
					SpineAnimCompat.HardSetCombatLoop(spineAnimation, sprite, text, val);
					st.IdleFixCooldown = 6;
				}
				else if (replayHurt)
				{
					text = currentAnim;
					if (SpineAnimCompat.TrySetAnimation(sprite, text, loop: false, 0, -1f, allowPromote: false))
					{
						_ = 1;
					}
					else
						SpineAnimCompat.TrySetAnimation(spineAnimation, text, loop: false, 0, -1f, allowPromote: false);
				}
				else if (IsLowHealthIdleName(currentAnim))
				{
					text = profile.FallbackIdle;
					SpineAnimCompat.HardSetCombatLoop(spineAnimation, sprite, text, val);
				}
				else if (!string.IsNullOrEmpty(currentAnim))
				{
					text = currentAnim;
					if (!SpineAnimCompat.TrySetAnimation(sprite, text, loop: true) && !SpineAnimCompat.TrySetAnimation(spineAnimation, text, loop: true))
					{
						SpineAnimCompat.SetAnimation(spineAnimation, text, loop: true);
					}
				}
				else
				{
					text = profile.FallbackIdle;
					if (!SpineAnimCompat.TrySetAnimation(sprite, text, loop: true) && !SpineAnimCompat.TrySetAnimation(spineAnimation, text, loop: true))
					{
						SpineAnimCompat.SetAnimation(spineAnimation, text, loop: true);
					}
				}
				int stage2 = st.Stage;
				st.Stage = stage;
				st.AppliedSkelId = ((GodotObject)val5).GetInstanceId();
				Logger log = ModEntry.Log;
				string text2;
				if (!st.Fresh)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 7);
					defaultInterpolatedStringHandler.AppendFormatted(profile.Name);
					defaultInterpolatedStringHandler.AppendLiteral(": wear stage ");
					defaultInterpolatedStringHandler.AppendFormatted(stage2);
					defaultInterpolatedStringHandler.AppendLiteral("->");
					defaultInterpolatedStringHandler.AppendFormatted(stage);
					defaultInterpolatedStringHandler.AppendLiteral(" anim='");
					defaultInterpolatedStringHandler.AppendFormatted(text);
					defaultInterpolatedStringHandler.AppendLiteral("' (hp ");
					defaultInterpolatedStringHandler.AppendFormatted(creature2.CurrentHp);
					defaultInterpolatedStringHandler.AppendLiteral("/");
					defaultInterpolatedStringHandler.AppendFormatted(creature2.MaxHp);
					defaultInterpolatedStringHandler.AppendLiteral(") for player ");
					Player player = creature2.Player;
					defaultInterpolatedStringHandler.AppendFormatted((player != null) ? new ulong?(player.NetId) : ((ulong?)null));
					defaultInterpolatedStringHandler.AppendLiteral(".");
					text2 = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(70, 6);
					defaultInterpolatedStringHandler2.AppendFormatted(profile.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(": aligned to wear stage ");
					defaultInterpolatedStringHandler2.AppendFormatted(stage);
					defaultInterpolatedStringHandler2.AppendLiteral(" anim='");
					defaultInterpolatedStringHandler2.AppendFormatted(text);
					defaultInterpolatedStringHandler2.AppendLiteral("' on combat start (hp ");
					defaultInterpolatedStringHandler2.AppendFormatted(creature2.CurrentHp);
					defaultInterpolatedStringHandler2.AppendLiteral("/");
					defaultInterpolatedStringHandler2.AppendFormatted(creature2.MaxHp);
					defaultInterpolatedStringHandler2.AppendLiteral(") for player ");
					Player player2 = creature2.Player;
					defaultInterpolatedStringHandler2.AppendFormatted((player2 != null) ? new ulong?(player2.NetId) : ((ulong?)null));
					defaultInterpolatedStringHandler2.AppendLiteral(".");
					text2 = defaultInterpolatedStringHandler2.ToStringAndClear();
				}
				log.Info(text2, 1);
				st.Fresh = false;
			}
		}

		internal static void OnCreatureDeath(NCreature node)
		{
			if (SkinSelectionGate.ForTarget(node))
			{
				DeathForceWear.TryForceOnCombatNode(node, "OnCreatureDeath");
			}
		}

		private static void TickRestSite()
		{
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Expected O, but got Unknown
			if (!SkinSelectionGate.BeforeTick())
			{
				return;
			}
			if (NRestSiteRoom.Instance == null)
			{
				if (RestStates.Count > 0)
				{
					RestStates.Clear();
				}
				if (RestChars.Count > 0)
				{
					RestChars.Clear();
				}
				return;
			}
			RestChars.RemoveAll((NRestSiteCharacter c) => c == null || !GodotObject.IsInstanceValid((GodotObject)c));
			foreach (KeyValuePair<ulong, RestWearState> restState in RestStates)
			{
				if (restState.Value.Node == null || !GodotObject.IsInstanceValid((GodotObject)restState.Value.Node))
				{
					Stale.Add(restState.Key);
				}
			}
			foreach (ulong item in Stale)
			{
				RestStates.Remove(item);
			}
			Stale.Clear();
			foreach (NRestSiteCharacter restChar in RestChars)
			{
				UpdateRestCharacter(restChar);
			}
		}

		internal static void UpdateRestCharacter(NRestSiteCharacter restChar)
		{
			if (_fuse || restChar == null || !GodotObject.IsInstanceValid((GodotObject)(object)restChar))
			{
				return;
			}
			BuildProfiles();
			WearProfile wearProfile = ProfileFor(restChar.Player);
			if (wearProfile?.Rest == null || !SkinReady(wearProfile))
			{
				return;
			}
			Creature creature = restChar.Player.Creature;
			if (creature != null)
			{
				int num = StageFor(wearProfile, creature);
				ulong instanceId = ((GodotObject)restChar).GetInstanceId();
				if (!RestStates.TryGetValue(instanceId, out RestWearState value))
				{
					value = new RestWearState
					{
						Node = restChar
					};
					RestStates[instanceId] = value;
				}
				if (value.Spine == null || !GodotObject.IsInstanceValid((GodotObject)(object)value.Spine) || (value.AppliedSkelId != 0L && SpineSkelId(value.Spine) != value.AppliedSkelId))
				{
					value.Fresh = true;
				}
				value.LastHp = creature.CurrentHp;
				value.LastMaxHp = creature.MaxHp;
				if (value.Stage != num || value.Fresh)
				{
					ApplyRestStage(wearProfile, creature, restChar, value, num);
				}
			}
		}

		internal static void ApplyRestStage(WearProfile profile, Creature creature, NRestSiteCharacter ch, RestWearState st, int stage)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Expected O, but got Unknown
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				WearSet rest = profile.Rest;
				Node val = st.Spine;
				if (val == null || !GodotObject.IsInstanceValid((GodotObject)val))
				{
					val = (st.Spine = FindRestSpine(profile, ch));
				}
				if (val == null)
				{
					return;
				}
				using MegaScope megaScope = new MegaScope(val);
				SpineAnimationAccess access = new SpineAnimationAccess((MegaSprite)megaScope);
				string currentAnimationName = SpineAnimCompat.GetCurrentAnimationName(access);
				if (!st.Fresh && !string.IsNullOrEmpty(currentAnimationName) && !currentAnimationName.Contains("idle", StringComparison.OrdinalIgnoreCase) && !currentAnimationName.Contains("loop", StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
				Resource stageData = GetStageData(profile, rest, stage);
				if (stageData != null)
				{
					((GodotObject)val).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)stageData));
					if (!string.IsNullOrEmpty(currentAnimationName))
					{
						SpineAnimCompat.SetAnimation(access, currentAnimationName, loop: true);
					}
					int stage2 = st.Stage;
					st.Stage = stage;
					st.AppliedSkelId = ((GodotObject)stageData).GetInstanceId();
					Logger log = ModEntry.Log;
					string text;
					if (!st.Fresh)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 6);
						defaultInterpolatedStringHandler.AppendFormatted(profile.Name);
						defaultInterpolatedStringHandler.AppendLiteral(": rest-site wear stage ");
						defaultInterpolatedStringHandler.AppendFormatted(stage2);
						defaultInterpolatedStringHandler.AppendLiteral("->");
						defaultInterpolatedStringHandler.AppendFormatted(stage);
						defaultInterpolatedStringHandler.AppendLiteral(" (hp ");
						defaultInterpolatedStringHandler.AppendFormatted(creature.CurrentHp);
						defaultInterpolatedStringHandler.AppendLiteral("/");
						defaultInterpolatedStringHandler.AppendFormatted(creature.MaxHp);
						defaultInterpolatedStringHandler.AppendLiteral(") for player ");
						Player player = ch.Player;
						defaultInterpolatedStringHandler.AppendFormatted((player != null) ? new ulong?(player.NetId) : ((ulong?)null));
						defaultInterpolatedStringHandler.AppendLiteral(".");
						text = defaultInterpolatedStringHandler.ToStringAndClear();
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(54, 5);
						defaultInterpolatedStringHandler2.AppendFormatted(profile.Name);
						defaultInterpolatedStringHandler2.AppendLiteral(": rest-site aligned to wear stage ");
						defaultInterpolatedStringHandler2.AppendFormatted(stage);
						defaultInterpolatedStringHandler2.AppendLiteral(" (hp ");
						defaultInterpolatedStringHandler2.AppendFormatted(creature.CurrentHp);
						defaultInterpolatedStringHandler2.AppendLiteral("/");
						defaultInterpolatedStringHandler2.AppendFormatted(creature.MaxHp);
						defaultInterpolatedStringHandler2.AppendLiteral(") for player ");
						Player player2 = ch.Player;
						defaultInterpolatedStringHandler2.AppendFormatted((player2 != null) ? new ulong?(player2.NetId) : ((ulong?)null));
						defaultInterpolatedStringHandler2.AppendLiteral(".");
						text = defaultInterpolatedStringHandler2.ToStringAndClear();
					}
					log.Info(text + " | " + WearStageRuntime.BuildStatusLine(), 1);
					st.Fresh = false;
				}
			}
			catch (Exception ex)
			{
				FailOrRetry($"{profile.Name}: rest-site stage apply failed: {ex}", ex);
			}
		}

		private static Node? FindRestSpine(WearProfile profile, NRestSiteCharacter ch)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Expected O, but got Unknown
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Expected O, but got Unknown
			WearSet rest = profile.Rest;
			foreach (Node child in ((Node)ch).GetChildren(false))
			{
				Node2D val = (Node2D)((child is Node2D) ? child : null);
				if (val == null || ((GodotObject)val).GetClass() != "SpineSprite")
				{
					continue;
				}
				Variant val2 = ((GodotObject)val).Get(StringName.op_Implicit("skeleton_data_res"));
				GodotObject val3 = ((Variant)(ref val2)).AsGodotObject();
				Resource val4 = (Resource)((val3 is Resource) ? val3 : null);
				if (val4 == null)
				{
					continue;
				}
				(int, int)? tuple = PageSizeOf(val4);
				if (!tuple.HasValue)
				{
					continue;
				}
				var (num, num2) = tuple.GetValueOrDefault();
				if (num == rest.PageW && num2 == rest.PageH)
				{
					if (rest.Pristine == null)
					{
						rest.Pristine = val4;
						ModEntry.Log.Info(profile.Name + ": rest-site pristine skeleton data captured (page size verified).", 1);
					}
					return (Node)val;
				}
			}
			return null;
		}

		private static void TickMerchant()
		{
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Expected O, but got Unknown
			if (!SkinSelectionGate.BeforeTick())
			{
				return;
			}
			NMerchantRoom instance = NMerchantRoom.Instance;
			if (instance == null)
			{
				if (MerchantStates.Count > 0)
				{
					MerchantStates.Clear();
				}
				return;
			}
			foreach (KeyValuePair<ulong, MerchantWearState> merchantState in MerchantStates)
			{
				if (merchantState.Value.Node == null || !GodotObject.IsInstanceValid((GodotObject)merchantState.Value.Node))
				{
					Stale.Add(merchantState.Key);
				}
			}
			foreach (ulong item in Stale)
			{
				MerchantStates.Remove(item);
			}
			Stale.Clear();
			RefreshMerchantRoom(instance);
		}

		internal static void RefreshMerchantRoom(NMerchantRoom instance)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Expected O, but got Unknown
			if (_fuse || instance == null || !GodotObject.IsInstanceValid((GodotObject)(object)instance))
			{
				return;
			}
			IReadOnlyList<NMerchantCharacter> playerVisuals = instance.PlayerVisuals;
			List<Player> list = MerchantPlayersField?.GetValue(instance) as List<Player>;
			if (playerVisuals == null || list == null)
			{
				return;
			}
			int num = Math.Min(playerVisuals.Count, list.Count);
			for (int i = 0; i < num; i++)
			{
				NMerchantCharacter val = playerVisuals[i];
				if (val != null && GodotObject.IsInstanceValid((GodotObject)val))
				{
					UpdateMerchantCharacter(list[i], val);
				}
			}
		}

		internal static void UpdateMerchantCharacter(Player player, NMerchantCharacter visual)
		{
			if (_fuse || player == null || visual == null || !GodotObject.IsInstanceValid((GodotObject)(object)visual))
			{
				return;
			}
			BuildProfiles();
			WearProfile wearProfile = ProfileFor(player);
			if (wearProfile?.Merchant == null || !SkinReady(wearProfile))
			{
				return;
			}
			Creature creature = player.Creature;
			if (creature != null)
			{
				int num = StageFor(wearProfile, creature);
				ulong instanceId = ((GodotObject)visual).GetInstanceId();
				if (!MerchantStates.TryGetValue(instanceId, out MerchantWearState value))
				{
					value = new MerchantWearState
					{
						Node = visual
					};
					MerchantStates[instanceId] = value;
				}
				if (value.Spine == null || !GodotObject.IsInstanceValid((GodotObject)(object)value.Spine) || (value.AppliedSkelId != 0L && SpineSkelId(value.Spine) != value.AppliedSkelId))
				{
					value.Fresh = true;
				}
				value.LastHp = creature.CurrentHp;
				value.LastMaxHp = creature.MaxHp;
				if (value.Stage != num || value.Fresh)
				{
					ApplyMerchantStage(wearProfile, creature, visual, value, num);
				}
			}
		}

		internal static void ApplyMerchantStage(WearProfile profile, Creature creature, NMerchantCharacter visual, MerchantWearState st, int stage)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Expected O, but got Unknown
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				WearSet merchant = profile.Merchant;
				Node val = st.Spine;
				if (val == null || !GodotObject.IsInstanceValid((GodotObject)val))
				{
					val = (st.Spine = FindMerchantSpine(profile, visual));
				}
				if (val == null)
				{
					return;
				}
				using MegaScope megaScope = new MegaScope(val);
				SpineAnimationAccess access = new SpineAnimationAccess((MegaSprite)megaScope);
				string currentAnimationName = SpineAnimCompat.GetCurrentAnimationName(access);
				if (!st.Fresh && !string.IsNullOrEmpty(currentAnimationName) && !currentAnimationName.Contains("idle", StringComparison.OrdinalIgnoreCase) && !currentAnimationName.Contains("loop", StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
				Resource val2 = ((stage <= 1) ? merchant.Pristine : GetStageData(profile, merchant, stage));
				if (val2 != null)
				{
					((GodotObject)val).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)val2));
					SpineAnimCompat.SetAnimation(access, string.IsNullOrEmpty(currentAnimationName) ? "relaxed_loop" : currentAnimationName, loop: true);
					int stage2 = st.Stage;
					st.Stage = stage;
					st.AppliedSkelId = ((GodotObject)val2).GetInstanceId();
					Logger log = ModEntry.Log;
					string text;
					if (!st.Fresh)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 6);
						defaultInterpolatedStringHandler.AppendFormatted(profile.Name);
						defaultInterpolatedStringHandler.AppendLiteral(": merchant wear stage ");
						defaultInterpolatedStringHandler.AppendFormatted(stage2);
						defaultInterpolatedStringHandler.AppendLiteral("->");
						defaultInterpolatedStringHandler.AppendFormatted(stage);
						defaultInterpolatedStringHandler.AppendLiteral(" (hp ");
						defaultInterpolatedStringHandler.AppendFormatted(creature.CurrentHp);
						defaultInterpolatedStringHandler.AppendLiteral("/");
						defaultInterpolatedStringHandler.AppendFormatted(creature.MaxHp);
						defaultInterpolatedStringHandler.AppendLiteral(") for player ");
						Player player = creature.Player;
						defaultInterpolatedStringHandler.AppendFormatted((player != null) ? new ulong?(player.NetId) : ((ulong?)null));
						defaultInterpolatedStringHandler.AppendLiteral(".");
						text = defaultInterpolatedStringHandler.ToStringAndClear();
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(53, 5);
						defaultInterpolatedStringHandler2.AppendFormatted(profile.Name);
						defaultInterpolatedStringHandler2.AppendLiteral(": merchant aligned to wear stage ");
						defaultInterpolatedStringHandler2.AppendFormatted(stage);
						defaultInterpolatedStringHandler2.AppendLiteral(" (hp ");
						defaultInterpolatedStringHandler2.AppendFormatted(creature.CurrentHp);
						defaultInterpolatedStringHandler2.AppendLiteral("/");
						defaultInterpolatedStringHandler2.AppendFormatted(creature.MaxHp);
						defaultInterpolatedStringHandler2.AppendLiteral(") for player ");
						Player player2 = creature.Player;
						defaultInterpolatedStringHandler2.AppendFormatted((player2 != null) ? new ulong?(player2.NetId) : ((ulong?)null));
						defaultInterpolatedStringHandler2.AppendLiteral(".");
						text = defaultInterpolatedStringHandler2.ToStringAndClear();
					}
					log.Info(text + " | " + WearStageRuntime.BuildStatusLine(), 1);
					st.Fresh = false;
				}
			}
			catch (Exception ex)
			{
				FailOrRetry($"{profile.Name}: merchant stage apply failed: {ex}", ex);
			}
		}

		private static Node? FindMerchantSpine(WearProfile profile, NMerchantCharacter visual)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Expected O, but got Unknown
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Expected O, but got Unknown
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Expected O, but got Unknown
			WearSet merchant = profile.Merchant;
			Node2D val = null;
			Node2D val2 = null;
			int value = 0;
			int value2 = 0;
			foreach (Node2D item in EnumerateSpineSprites((Node)visual))
			{
				Variant val3 = ((GodotObject)item).Get(StringName.op_Implicit("skeleton_data_res"));
				GodotObject val4 = ((Variant)(ref val3)).AsGodotObject();
				Resource val5 = (Resource)((val4 is Resource) ? val4 : null);
				if (val5 == null)
				{
					if (val2 == null)
					{
						val2 = item;
					}
					continue;
				}
				(int, int)? tuple = PageSizeOf(val5);
				if (!tuple.HasValue)
				{
					if (val2 == null)
					{
						val2 = item;
					}
					continue;
				}
				var (num, num2) = tuple.GetValueOrDefault();
				if (num == merchant.PageW && num2 == merchant.PageH)
				{
					if (merchant.Pristine == null)
					{
						merchant.Pristine = val5;
						ModEntry.Log.Info(profile.Name + ": merchant pristine skeleton data captured (page size verified).", 1);
					}
					val = item;
					break;
				}
				if (val2 == null)
				{
					val2 = item;
				}
				value = num;
				value2 = num2;
			}
			if (val != null)
			{
				return (Node)val;
			}
			if (val2 != null)
			{
				ModEntry.Log.Info($"{profile.Name}: merchant spine page {value}x{value2} != {merchant.PageW}x{merchant.PageH} — fallback apply (SkinChanger switch).", 1);
				return (Node)val2;
			}
			ModEntry.Log.Error(profile.Name + ": merchant SpineSprite not found under NMerchantCharacter.", 1);
			return null;
		}

		[IteratorStateMachine(typeof(<EnumerateSpineSprites>d__45))]
		private static IEnumerable<Node2D> EnumerateSpineSprites(Node root)
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <EnumerateSpineSprites>d__45(-2)
			{
				<>3__root = root
			};
		}

		private static int StageFor(WearProfile profile, Creature creature)
		{
			if (!creature.IsDead)
			{
				return WearStageRuntime.StageFor(creature.CurrentHp, creature.MaxHp, useCombatSnapshot: false);
			}
			return WearStageRuntime.StageForDeath(creature.CurrentHp, creature.MaxHp);
		}

		private static (int, int)? PageSizeOf(Resource skeletonData)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Invalid comparison between Unknown and I8
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			Variant val = ((GodotObject)skeletonData).Get(StringName.op_Implicit("atlas_res"));
			GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
			Resource val3 = (Resource)((val2 is Resource) ? val2 : null);
			if (val3 == null)
			{
				return null;
			}
			Variant val4 = ((GodotObject)val3).Get(StringName.op_Implicit("textures"));
			if ((long)((Variant)(ref val4)).VariantType != 28 || ((Variant)(ref val4)).AsGodotArray().Count < 1)
			{
				return null;
			}
			val = ((Variant)(ref val4)).AsGodotArray()[0];
			Texture2D val5 = ((Variant)(ref val)).As<Texture2D>();
			if (val5 != null)
			{
				return (val5.GetWidth(), val5.GetHeight());
			}
			return null;
		}

		private static Resource? GetStageData(WearProfile profile, WearSet set, int stage)
		{
			return GetStageData(set, stage, profile.Name);
		}

		internal static Resource? GetStageData(WearSet set, int stage, string logName)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Expected O, but got Unknown
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Expected O, but got Unknown
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Expected O, but got Unknown
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0303: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Unknown result type (might be due to invalid IL or missing references)
			if (stage == 1 && set.Pristine != null && set.Prefix != "cs_stage" && set.Prefix != "rest_stage")
			{
				return set.Pristine;
			}
			if (set.Stage[stage] != null)
			{
				return set.Stage[stage];
			}
			if (set.Pristine == null)
			{
				return null;
			}
			Variant val = ClassDB.Instantiate(StringName.op_Implicit("SpineAtlasResource"));
			GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
			Resource val3 = (Resource)((val2 is Resource) ? val2 : null);
			if (val3 == null)
			{
				Fail("cannot instantiate SpineAtlasResource");
				return null;
			}
			if (!((GodotObject)val3).HasMethod(StringName.op_Implicit("load_from_atlas_file")))
			{
				Fail("SpineAtlasResource.load_from_atlas_file not found (game build changed?)");
				return null;
			}
			string text = $"res://{set.PathBase}/{set.Prefix}{stage}.atlas";
			val = ((GodotObject)val3).Call(StringName.op_Implicit("load_from_atlas_file"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(text) });
			long num = ((Variant)(ref val)).AsInt64();
			if (num != 0L)
			{
				Fail($"load_from_atlas_file({text}) failed: {num} — is this mod's pck mounted?");
				return null;
			}
			val = ClassDB.Instantiate(StringName.op_Implicit("SpineSkeletonDataResource"));
			GodotObject val4 = ((Variant)(ref val)).AsGodotObject();
			Resource val5 = (Resource)((val4 is Resource) ? val4 : null);
			if (val5 == null)
			{
				Fail("cannot instantiate SpineSkeletonDataResource");
				return null;
			}
			((GodotObject)val5).Set(StringName.op_Implicit("atlas_res"), Variant.op_Implicit((GodotObject)val3));
			Resource val6 = TryLoadStageSkeleton(set, stage, logName);
			if (val6 != null)
			{
				((GodotObject)val5).Set(StringName.op_Implicit("skeleton_file_res"), Variant.op_Implicit((GodotObject)val6));
			}
			else
			{
				if (set.Prefix == "cs_stage")
				{
					ModEntry.Log.Error($"{logName}: independent selection skeleton missing for stage {stage}; not using native skeleton.", 1);
					return null;
				}
				((GodotObject)val5).Set(StringName.op_Implicit("skeleton_file_res"), ((GodotObject)set.Pristine).Get(StringName.op_Implicit("skeleton_file_res")));
			}
			((GodotObject)val5).Set(StringName.op_Implicit("default_mix"), ((GodotObject)set.Pristine).Get(StringName.op_Implicit("default_mix")));
			((GodotObject)val5).Set(StringName.op_Implicit("animation_mixes"), ((GodotObject)set.Pristine).Get(StringName.op_Implicit("animation_mixes")));
			SpineAnimCompat.EnsureIdleLowHealthMixes(null, val5);
			if (((GodotObject)val5).HasMethod(StringName.op_Implicit("is_skeleton_data_loaded")))
			{
				val = ((GodotObject)val5).Call(StringName.op_Implicit("is_skeleton_data_loaded"), Array.Empty<Variant>());
				if (!((Variant)(ref val)).AsBool())
				{
					Fail($"{logName}: {set.Prefix}{stage} skeleton data did not finish loading (atlas ok, parse failed)");
					return null;
				}
			}
			set.Stage[stage] = val5;
			ModEntry.Log.Info($"{logName}: built {set.Prefix}{stage} skeleton data from {text}.", 1);
			return val5;
		}

		private static Resource TryLoadStageSkeleton(WearSet set, int stage, string logName)
		{
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Expected O, but got Unknown
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			string text = $"res://{set.PathBase}/{set.Prefix}{stage}.skel";
			string text2 = $"res://{set.PathBase}/nsfw.skel";
			string text3 = $"res://{set.PathBase}/nsfw_rest.skel";
			string[] array = new string[2] { text, text2 };
			if (set.Prefix != null && set.Prefix.IndexOf("rest") >= 0)
			{
				array = new string[3] { text, text3, text2 };
			}
			string[] array2 = array;
			foreach (string text4 in array2)
			{
				try
				{
					if (!FileAccess.FileExists(text4))
					{
						continue;
					}
					Variant val = ClassDB.Instantiate(StringName.op_Implicit("SpineSkeletonFileResource"));
					GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
					Resource val3 = (Resource)((val2 is Resource) ? val2 : null);
					if (val3 == null)
					{
						continue;
					}
					string[] array3 = new string[3] { "load_from_binary_file", "load_from_file", "load_binary_file" };
					foreach (string text5 in array3)
					{
						if (((GodotObject)val3).HasMethod(StringName.op_Implicit(text5)))
						{
							Variant val4 = ((GodotObject)val3).Call(StringName.op_Implicit(text5), (Variant[])(object)new Variant[1] { Variant.op_Implicit(text4) });
							if (((Variant)(ref val4)).AsInt64() == 0L)
							{
								ModEntry.Log.Info($"{logName}: loaded stage skeleton via {text5}({text4}).", 1);
								return val3;
							}
						}
					}
				}
				catch (Exception ex)
				{
					ModEntry.Log.Info($"{logName}: stage skel load failed for {text4}: {ex.Message}", 1);
				}
			}
			return null;
		}

		private static bool SpinePageMatches(Node? spine, WearSet? set)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			if (spine == null || set == null || !GodotObject.IsInstanceValid((GodotObject)spine))
			{
				return false;
			}
			try
			{
				Variant val = ((GodotObject)spine).Get(StringName.op_Implicit("skeleton_data_res"));
				GodotObject obj = ((Variant)(ref val)).AsGodotObject();
				Resource val2 = (Resource)(object)((obj is Resource) ? obj : null);
				if (val2 == null)
				{
					return false;
				}
				(int, int)? tuple = PageSizeOf(val2);
				if (!tuple.HasValue)
				{
					return false;
				}
				var (num, num2) = tuple.GetValueOrDefault();
				return num == set.PageW && num2 == set.PageH;
			}
			catch
			{
				return false;
			}
		}

		private static ulong SpineSkelId(Node? spine)
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (spine == null || !GodotObject.IsInstanceValid((GodotObject)spine))
			{
				return 0uL;
			}
			try
			{
				Variant val = ((GodotObject)spine).Get(StringName.op_Implicit("skeleton_data_res"));
				GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
				return (val2 != null) ? val2.GetInstanceId() : 0;
			}
			catch
			{
				return 0uL;
			}
		}

		private static void FailOrRetry(string message, Exception ex)
		{
			if (SpineAnimCompat.IsTransientSpineError(ex) || ex is EntryPointNotFoundException)
			{
				ModEntry.Log.Warn(message + " (spine not ready / non-fatal, will retry)", 1);
			}
			else
			{
				Fail(message);
			}
		}

		internal static void ForceReapplyWear(bool preferCrossfade = false)
		{
			foreach (KeyValuePair<ulong, CombatWearState> combatState in CombatStates)
			{
				combatState.Value.Stage = -1;
				combatState.Value.Fresh = !preferCrossfade;
				combatState.Value.ScaleApplied = false;
				combatState.Value.PendingStage = 0;
				combatState.Value.IdleWaitTicks = 99;
				combatState.Value.MixDeferTicks = 0;
				combatState.Value.MixDeferUntilMs = 0L;
				combatState.Value.PendingAnimHint = null;
				combatState.Value.LowBridgeDone = false;
			}
			foreach (KeyValuePair<ulong, RestWearState> restState in RestStates)
			{
				restState.Value.Stage = -1;
				restState.Value.Fresh = !preferCrossfade;
			}
			foreach (KeyValuePair<ulong, MerchantWearState> merchantState in MerchantStates)
			{
				merchantState.Value.Stage = -1;
				merchantState.Value.Fresh = !preferCrossfade;
			}
			try
			{
				Tick();
			}
			catch
			{
			}
		}

		private static void Fail(string message)
		{
			if (!_fuse)
			{
				_fuse = true;
				ModEntry.Log.Error(message + " — battle wear disabled for this session.", 1);
			}
		}

		internal static void ReconcileLowHealthIdle(Creature creature, CombatWearState state, string animation)
		{
			if (SkinSelectionGate.ForTarget(creature) && !(ModEntry.ModId == "ironcladHpWear") && state.Stage > 0 && !state.Fresh && state.PendingStage == 0 && state.IdleFixCooldown <= 0 && state.MixDeferTicks <= 0 && state.MixDeferUntilMs <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
			{
				bool flag = LowHealthIdleRules.MatchesCreature(creature);
				bool flag2 = animation == "low_health_loop";
				bool flag3 = animation == "idle_loop" || animation == "idle";
				if ((flag && flag3) || (!flag && flag2))
				{
					state.Stage = -1;
				}
			}
		}

		internal static void RefreshLowHealthIdleCore()
		{
			foreach (CombatWearState value in CombatStates.Values)
			{
				value.Stage = -1;
			}
		}
	}
	[HarmonyPatch(typeof(NCharacterSelectScreen), "SelectCharacter")]
	internal static class SuccubusCharacterSelect
	{
		[CompilerGenerated]
		private sealed class <>c__DisplayClass3_0
		{
			public Node spine;

			public BattleWear.WearSet set;

			public int stage;

			public Control background;

			public Control catcher;

			internal void <Attach>b__1(InputEvent input)
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Invalid comparison between Unknown and I8
				if (!SkinSelectionGate.MenuAllowed() || !WearStageRuntime.GetLive().WearEnabled)
				{
					return;
				}
				InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
				if (val == null || (long)val.ButtonIndex != 1 || !val.Pressed || !((CanvasItem)background).IsVisibleInTree())
				{
					return;
				}
				catcher.AcceptEvent();
				try
				{
					int target2 = ((stage != 1) ? 1 : 2);
					if (Apply(target2))
					{
						ModEntry.Log.Info("char-select: preview " + ((stage == 1) ? "原版" : "改版"), 1);
					}
				}
				catch (Exception ex)
				{
					ModEntry.Log.Warn("char-select preview switch failed: " + ex.Message, 1);
				}
				bool Apply(int target)
				{
					//IL_0049: Unknown result type (might be due to invalid IL or missing references)
					if (!SkinSelectionGate.MenuAllowed())
					{
						return false;
					}
					if (!GodotObject.IsInstanceValid((GodotObject)(object)spine))
					{
						return false;
					}
					Resource stageData = BattleWear.GetStageData(set, target, "char-select/ironclad");
					if (stageData == null)
					{
						return false;
					}
					((GodotObject)spine).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)stageData));
					using BattleWear.MegaScope megaScope = new BattleWear.MegaScope(spine);
					SpineAnimCompat.TrySetAnimation((MegaSprite)megaScope, "animation", loop: true, 0, -1f, allowPromote: false);
					stage = target;
					return true;
				}
			}

			internal void <Attach>b__2()
			{
				if (!SkinSelectionGate.MenuAllowed() || ((CanvasItem)background).IsVisibleInTree() || stage == 1)
				{
					return;
				}
				try
				{
					Apply(1);
				}
				catch (Exception ex)
				{
					ModEntry.Log.Warn("char-select preview reset failed: " + ex.Message, 1);
				}
				bool Apply(int target)
				{
					//IL_0049: Unknown result type (might be due to invalid IL or missing references)
					if (!SkinSelectionGate.MenuAllowed())
					{
						return false;
					}
					if (!GodotObject.IsInstanceValid((GodotObject)(object)spine))
					{
						return false;
					}
					Resource stageData = BattleWear.GetStageData(set, target, "char-select/ironclad");
					if (stageData == null)
					{
						return false;
					}
					((GodotObject)spine).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)stageData));
					using BattleWear.MegaScope megaScope = new BattleWear.MegaScope(spine);
					SpineAnimCompat.TrySetAnimation((MegaSprite)megaScope, "animation", loop: true, 0, -1f, allowPromote: false);
					stage = target;
					return true;
				}
			}
		}

		private const string CatcherName = "SuccubusSkinPreviewClick";

		private static readonly FieldInfo? Backgrounds = AccessTools.Field(typeof(NCharacterSelectScreen), "_bgContainer");

		[HarmonyPostfix]
		private static void Postfix(NCharacterSelectScreen __instance, CharacterModel characterModel)
		{
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			if (!SkinSelectionGate.MenuAllowed() || !BattleWear.OwnsCharacter("ironclad") || characterModel == null || !string.Equals(((AbstractModel)characterModel).Id.Entry, "ironclad", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			try
			{
				object? obj = Backgrounds?.GetValue(__instance);
				Control val = (Control)((obj is Control) ? obj : null);
				Control background = ((val != null) ? ((Node)val).GetNodeOrNull<Control>(NodePath.op_Implicit(((AbstractModel)characterModel).Id.Entry + "_bg")) : null);
				if (background == null)
				{
					return;
				}
				Callable val2 = Callable.From((Action)delegate
				{
					if (SkinSelectionGate.MenuAllowed())
					{
						Attach(background);
					}
				});
				((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
			}
			catch (Exception ex)
			{
				ModEntry.Log.Error("char-select attach failed: " + ex.Message, 1);
			}
		}

		private static void Attach(Control background)
		{
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Expected O, but got Unknown
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Expected O, but got Unknown
			if (!SkinSelectionGate.MenuAllowed())
			{
				return;
			}
			<>c__DisplayClass3_0 CS$<>8__locals28 = new <>c__DisplayClass3_0();
			CS$<>8__locals28.background = background;
			if (!GodotObject.IsInstanceValid((GodotObject)(object)CS$<>8__locals28.background) || !((Node)CS$<>8__locals28.background).IsInsideTree() || ((Node)CS$<>8__locals28.background).GetNodeOrNull<Control>(NodePath.op_Implicit("SuccubusSkinPreviewClick")) != null)
			{
				return;
			}
			CS$<>8__locals28.spine = FindSpine((Node)(object)CS$<>8__locals28.background);
			Node obj = CS$<>8__locals28.spine;
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				Variant val = ((GodotObject)obj).Get(StringName.op_Implicit("skeleton_data_res"));
				obj2 = ((Variant)(ref val)).AsGodotObject();
			}
			Resource val2 = (Resource)((obj2 is Resource) ? obj2 : null);
			if (CS$<>8__locals28.spine == null || val2 == null)
			{
				ModEntry.Log.Warn("char-select: no SpineSprite/skeleton resource; preview toggle skipped.", 1);
				return;
			}
			CS$<>8__locals28.set = new BattleWear.WearSet
			{
				PathBase = "succubus_ironclad_hpwear/ironclad",
				Prefix = "cs_stage",
				Pristine = val2,
				PageW = 3802,
				PageH = 3802
			};
			CS$<>8__locals28.stage = 1;
			if (!Apply(1))
			{
				ModEntry.Log.Error("char-select: packaged original resource failed; preview toggle not attached.", 1);
				return;
			}
			CS$<>8__locals28.catcher = new Control
			{
				Name = StringName.op_Implicit("SuccubusSkinPreviewClick"),
				MouseFilter = (MouseFilterEnum)0,
				TooltipText = "点击切换：原版 / 改版（仅选人预览）"
			};
			((Node)CS$<>8__locals28.background).AddChild((Node)(object)CS$<>8__locals28.catcher, false, (InternalMode)0);
			CS$<>8__locals28.catcher.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
			CS$<>8__locals28.catcher.GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Invalid comparison between Unknown and I8
				if (SkinSelectionGate.MenuAllowed() && WearStageRuntime.GetLive().WearEnabled)
				{
					InputEventMouseButton val3 = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
					if (val3 != null && (long)val3.ButtonIndex == 1 && val3.Pressed && ((CanvasItem)CS$<>8__locals28.background).IsVisibleInTree())
					{
						CS$<>8__locals28.catcher.AcceptEvent();
						try
						{
							int target2 = ((CS$<>8__locals28.stage != 1) ? 1 : 2);
							if (Apply(target2))
							{
								ModEntry.Log.Info("char-select: preview " + ((CS$<>8__locals28.stage == 1) ? "原版" : "改版"), 1);
							}
						}
						catch (Exception ex)
						{
							ModEntry.Log.Warn("char-select preview switch failed: " + ex.Message, 1);
						}
					}
				}
			};
			((CanvasItem)CS$<>8__locals28.background).VisibilityChanged += delegate
			{
				if (!SkinSelectionGate.MenuAllowed() || ((CanvasItem)CS$<>8__locals28.background).IsVisibleInTree() || CS$<>8__locals28.stage == 1)
				{
					return;
				}
				try
				{
					Apply(1);
				}
				catch (Exception ex2)
				{
					ModEntry.Log.Warn("char-select preview reset failed: " + ex2.Message, 1);
				}
			};
			CharSelectSafety.Register(background, CS$<>8__locals28);
			ModEntry.Log.Info("char-select: independent original atlas/texture/skeleton loaded (1.0.6); click background to toggle modified skin.", 1);
			bool Apply(int target)
			{
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				if (!SkinSelectionGate.MenuAllowed())
				{
					return false;
				}
				if (!GodotObject.IsInstanceValid((GodotObject)(object)CS$<>8__locals28.spine))
				{
					return false;
				}
				Resource stageData = BattleWear.GetStageData(CS$<>8__locals28.set, target, "char-select/ironclad");
				if (stageData == null)
				{
					return false;
				}
				((GodotObject)CS$<>8__locals28.spine).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)stageData));
				using BattleWear.MegaScope megaScope = new BattleWear.MegaScope(CS$<>8__locals28.spine);
				SpineAnimCompat.TrySetAnimation((MegaSprite)megaScope, "animation", loop: true, 0, -1f, allowPromote: false);
				CS$<>8__locals28.stage = target;
				return true;
			}
		}

		private static Node? FindSpine(Node root)
		{
			foreach (Node child in root.GetChildren(false))
			{
				if (((GodotObject)child).GetClass() == "SpineSprite")
				{
					return child;
				}
				Node val = FindSpine(child);
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}
	}
	internal static class WearStageCrossfade
	{
		private enum Stage
		{
			WaitingOverlay,
			Fading
		}

		private sealed class Transition
		{
			internal required Node2D Body;

			internal CanvasGroup? Group;

			internal required Node2D Ghost;

			internal required Resource TargetSkel;

			internal required Action OnSwapped;

			internal Stage Stage;

			internal double Age;

			internal double Progress;

			internal long FadeStartMs;
		}

		internal const double TransitionSeconds = 0.25;

		private const double ReadyTimeoutSeconds = 0.75;

		private const double OverlayReadySeconds = 0.02;

		private const string GroupName = "HpWearSkinTransition";

		private static readonly Dictionary<ulong, Transition> Active = new Dictionary<ulong, Transition>();

		private static SceneTree? _tree;

		private static bool _hooked;

		private static bool _smoothUnavailable;

		private static bool _warned;

		private static bool _softFailWarned;

		private static int _failStreak;

		internal static bool SmoothUnavailable => _smoothUnavailable;

		internal static bool IsActive(Node body)
		{
			if (body == null || !GodotObject.IsInstanceValid((GodotObject)(object)body))
			{
				return false;
			}
			return Active.ContainsKey(((GodotObject)body).GetInstanceId());
		}

		internal static void Cancel(Node? body)
		{
			if (body != null && GodotObject.IsInstanceValid((GodotObject)(object)body))
			{
				ulong instanceId = ((GodotObject)body).GetInstanceId();
				if (Active.TryGetValue(instanceId, out Transition value))
				{
					Active.Remove(instanceId);
					FreeGroup(value);
				}
			}
		}

		internal static void CancelAll()
		{
			foreach (Transition value in Active.Values)
			{
				FreeGroup(value);
			}
			Active.Clear();
		}

		internal static void Tick(double dt)
		{
			if (SkinSelectionGate.BeforeAdvance() && Active.Count != 0)
			{
				if (_hooked)
				{
					Advance(0.0);
				}
				else if (dt > 0.0)
				{
					Advance(dt);
				}
			}
		}

		internal static bool TryBegin(Node bodyNode, Resource targetSkel, Action onSwapped)
		{
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Expected O, but got Unknown
			if (!SkinSelectionGate.SceneAllowed())
			{
				return false;
			}
			if (_smoothUnavailable || bodyNode == null || targetSkel == null || onSwapped == null)
			{
				return false;
			}
			if (!GodotObject.IsInstanceValid((GodotObject)(object)bodyNode) || ((GodotObject)bodyNode).IsQueuedForDeletion() || !bodyNode.IsInsideTree())
			{
				WarnSoftFail("body not in tree");
				return false;
			}
			Node2D val = (Node2D)(object)((bodyNode is Node2D) ? bodyNode : null);
			if (val == null)
			{
				WarnSoftFail("body is not Node2D (" + ((object)bodyNode).GetType().Name + ")");
				return false;
			}
			ulong instanceId = ((GodotObject)val).GetInstanceId();
			Cancel((Node?)(object)val);
			Variant val2 = ((GodotObject)val).Get(StringName.op_Implicit("skeleton_data_res"));
			GodotObject obj = ((Variant)(ref val2)).AsGodotObject();
			Resource val3 = (Resource)(object)((obj is Resource) ? obj : null);
			Node parent = ((Node)val).GetParent();
			if (val3 == null || parent == null)
			{
				WarnSoftFail((val3 == null) ? "skeleton_data_res null" : "parent null");
				return false;
			}
			if (((GodotObject)val3).GetInstanceId() == ((GodotObject)targetSkel).GetInstanceId())
			{
				return false;
			}
			CanvasGroup val4 = null;
			try
			{
				val4 = new CanvasGroup
				{
					Name = StringName.op_Implicit("HpWearSkinTransition"),
					Transform = val.Transform,
					Visible = false,
					ZIndex = Math.Max(((CanvasItem)val).ZIndex, 0) + 1,
					ZAsRelative = ((CanvasItem)val).ZAsRelative,
					ShowBehindParent = false,
					LightMask = ((CanvasItem)val).LightMask,
					VisibilityLayer = ((CanvasItem)val).VisibilityLayer,
					TextureFilter = ((CanvasItem)val).TextureFilter,
					TextureRepeat = ((CanvasItem)val).TextureRepeat
				};
				Node2D val5 = CreateGhost(val, val3);
				((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
				parent.AddChild((Node)(object)val4, false, (InternalMode)0);
				int index = ((Node)val).GetIndex(false);
				parent.MoveChild((Node)(object)val4, Math.Min(index + 1, parent.GetChildCount(false) - 1));
				EnsureProcessHook((Node)(object)val);
				Active[instanceId] = new Transition
				{
					Body = val,
					Group = val4,
					Ghost = val5,
					TargetSkel = targetSkel,
					OnSwapped = onSwapped,
					Stage = Stage.WaitingOverlay,
					Age = 0.0,
					Progress = 0.0,
					FadeStartMs = 0L
				};
				_failStreak = 0;
				try
				{
					ModEntry.Log.Info("[HpWear] crossfade WaitingOverlay→0.25s fade (ThreeStages order)", 1);
				}
				catch
				{
				}
				return true;
			}
			catch (Exception ex)
			{
				if (val4 != null && GodotObject.IsInstanceValid((GodotObject)(object)val4))
				{
					try
					{
						((Node)val4).QueueFree();
					}
					catch
					{
					}
				}
				DisableSmooth(ex);
				return false;
			}
		}

		private static Node2D CreateGhost(Node2D body, Resource source)
		{
			string anim = ReadNodeAnim(body);
			Node2D obj = CreateFreshGhost(body, source);
			SyncGhostAnim(obj, anim);
			return obj;
		}

		private static Node2D CreateFreshGhost(Node2D body, Resource source)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			Variant val = ClassDB.Instantiate(StringName.op_Implicit("SpineSprite"));
			GodotObject obj = ((Variant)(ref val)).AsGodotObject();
			Node2D val2 = (Node2D)(object)((obj is Node2D) ? obj : null);
			if (val2 == null)
			{
				throw new InvalidOperationException("SpineSprite could not be instantiated.");
			}
			((Node)val2).Name = StringName.op_Implicit("OldRig");
			val2.Transform = Transform2D.Identity;
			((CanvasItem)val2).Modulate = ((CanvasItem)body).Modulate;
			((CanvasItem)val2).SelfModulate = ((CanvasItem)body).SelfModulate;
			((CanvasItem)val2).TextureFilter = ((CanvasItem)body).TextureFilter;
			((CanvasItem)val2).TextureRepeat = ((CanvasItem)body).TextureRepeat;
			((CanvasItem)val2).Material = ((CanvasItem)body).Material;
			((CanvasItem)val2).Visible = true;
			CopySpineProperties(body, val2);
			((GodotObject)val2).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)source));
			TryUpdateSkeleton(val2);
			return val2;
		}

		private static string? ReadNodeAnim(Node2D node)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				MegaAnimationState val = new MegaSprite(Variant.op_Implicit((GodotObject)(object)node)).TryGetAnimationState();
				if (val == null)
				{
					return null;
				}
				return SpineAnimCompat.GetCurrentAnimationName(val);
			}
			catch
			{
				return null;
			}
		}

		private static bool SyncGhostAnim(Node2D ghost, string? anim)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			if (string.IsNullOrEmpty(anim))
			{
				TryUpdateSkeleton(ghost);
				return false;
			}
			try
			{
				MegaSprite sprite = new MegaSprite(Variant.op_Implicit((GodotObject)(object)ghost));
				if (!SpineAnimCompat.TrySetAnimation(sprite, anim, loop: true))
				{
					TryUpdateSkeleton(ghost);
					return false;
				}
				SpineAnimCompat.TryFlush(sprite, (Node?)(object)ghost);
				TryUpdateSkeleton(ghost);
				return true;
			}
			catch
			{
				TryUpdateSkeleton(ghost);
				return false;
			}
		}

		private static void TryUpdateSkeleton(Node2D node)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)node) && ((GodotObject)node).HasMethod(StringName.op_Implicit("update_skeleton")))
				{
					((GodotObject)node).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
				}
			}
			catch
			{
			}
		}

		private static void WarnSoftFail(string reason)
		{
			if (_softFailWarned)
			{
				return;
			}
			_softFailWarned = true;
			try
			{
				ModEntry.Log.Warn("[HpWear] crossfade soft-fail: " + reason, 1);
			}
			catch
			{
			}
		}

		private static void EnsureProcessHook(Node body)
		{
			SceneTree tree = body.GetTree();
			if (tree == null)
			{
				return;
			}
			if (_tree != null && _hooked && _tree != tree)
			{
				try
				{
					_tree.ProcessFrame -= OnProcessFrame;
				}
				catch
				{
				}
				_hooked = false;
			}
			_tree = tree;
			if (!_hooked)
			{
				_tree.ProcessFrame += OnProcessFrame;
				_hooked = true;
			}
		}

		private static void OnProcessFrame()
		{
			if (Active.Count != 0 && _tree != null)
			{
				double num = ((_tree.Root != null) ? ((Node)_tree.Root).GetProcessDeltaTime() : 0.0);
				if (num <= 0.0)
				{
					num = 1.0 / 60.0;
				}
				Advance(num);
			}
		}

		private static void Advance(double dt)
		{
			if (!SkinSelectionGate.BeforeAdvance())
			{
				return;
			}
			List<ulong> list = new List<ulong>();
			foreach (ulong item in new List<ulong>(Active.Keys))
			{
				if (!Active.TryGetValue(item, out Transition value))
				{
					continue;
				}
				try
				{
					if (!GodotObject.IsInstanceValid((GodotObject)(object)value.Body) || ((GodotObject)value.Body).IsQueuedForDeletion() || !((Node)value.Body).IsInsideTree() || !GodotObject.IsInstanceValid((GodotObject)(object)value.Ghost) || ((GodotObject)value.Ghost).IsQueuedForDeletion())
					{
						list.Add(item);
						FreeGroup(value);
						continue;
					}
					if (dt > 0.0)
					{
						value.Age += dt;
					}
					SyncTransform(value);
					if (value.Stage == Stage.WaitingOverlay)
					{
						if (value.Age > 0.75)
						{
							throw new TimeoutException("HpWear crossfade overlay did not become ready.");
						}
						if (!(value.Age < 0.02) || !(dt > 0.0))
						{
							CommitSwap(value);
						}
						continue;
					}
					long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
					if (value.FadeStartMs <= 0)
					{
						value.FadeStartMs = num;
					}
					double num2 = Math.Max(0.0, (double)(num - value.FadeStartMs) / 1000.0);
					value.Progress = Math.Clamp(num2 / 0.25, 0.0, 1.0);
					float num3 = (float)value.Progress;
					float alpha = 1f - num3 * num3 * (3f - 2f * num3);
					ApplyAlpha(value, alpha);
					if (value.Progress >= 1.0)
					{
						list.Add(item);
						FreeGroup(value);
					}
				}
				catch (Exception ex)
				{
					list.Add(item);
					FreeGroup(value);
					DisableSmooth(ex);
				}
			}
			foreach (ulong item2 in list)
			{
				Active.Remove(item2);
			}
		}

		private static void CommitSwap(Transition t)
		{
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			if (SkinSelectionGate.ForTarget(t))
			{
				string anim = ReadNodeAnim(t.Body);
				SyncGhostAnim(t.Ghost, anim);
				if (t.Group != null && GodotObject.IsInstanceValid((GodotObject)(object)t.Group))
				{
					((CanvasItem)t.Group).Visible = true;
					((CanvasItem)t.Group).SelfModulate = new Color(1f, 1f, 1f, 1f);
				}
				((CanvasItem)t.Ghost).Visible = true;
				((CanvasItem)t.Ghost).SelfModulate = new Color(1f, 1f, 1f, 1f);
				TryUpdateSkeleton(t.Ghost);
				((GodotObject)t.Body).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)t.TargetSkel));
				try
				{
					t.OnSwapped();
				}
				catch
				{
					FreeGroup(t);
					throw;
				}
				TryUpdateSkeleton(t.Body);
				t.Stage = Stage.Fading;
				t.Progress = 0.0;
				t.FadeStartMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			}
		}

		private static void ApplyAlpha(Transition t, float alpha)
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (t.Group != null && GodotObject.IsInstanceValid((GodotObject)(object)t.Group))
			{
				((CanvasItem)t.Group).SelfModulate = new Color(1f, 1f, 1f, alpha);
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)t.Ghost))
			{
				((CanvasItem)t.Ghost).SelfModulate = new Color(1f, 1f, 1f, alpha);
				Color modulate = ((CanvasItem)t.Ghost).Modulate;
				((CanvasItem)t.Ghost).Modulate = new Color(modulate.R, modulate.G, modulate.B, alpha);
			}
		}

		private static void SyncTransform(Transition t)
		{
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			if (t.Group != null && GodotObject.IsInstanceValid((GodotObject)(object)t.Group))
			{
				((Node2D)t.Group).Position = t.Body.Position;
				((Node2D)t.Group).Rotation = t.Body.Rotation;
				((Node2D)t.Group).Skew = t.Body.Skew;
				((Node2D)t.Group).Scale = t.Body.Scale;
				if (t.Stage != 0)
				{
					((CanvasItem)t.Group).Visible = ((CanvasItem)t.Body).Visible;
				}
			}
			else if (GodotObject.IsInstanceValid((GodotObject)(object)t.Ghost))
			{
				t.Ghost.GlobalTransform = t.Body.GlobalTransform;
				((CanvasItem)t.Ghost).Visible = ((CanvasItem)t.Body).Visible;
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)t.Ghost))
			{
				((CanvasItem)t.Ghost).Modulate = new Color(((CanvasItem)t.Body).Modulate.R, ((CanvasItem)t.Body).Modulate.G, ((CanvasItem)t.Body).Modulate.B, ((CanvasItem)t.Ghost).Modulate.A);
			}
		}

		private static void CopySpineProperties(Node2D source, Node2D target)
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal) { "normal_material", "additive_material", "flip_h", "flip_v", "update_mode" };
			foreach (Dictionary property in ((GodotObject)source).GetPropertyList())
			{
				Variant val = property[Variant.op_Implicit("name")];
				string text = ((Variant)(ref val)).AsString();
				if (hashSet.Contains(text))
				{
					((GodotObject)target).Set(StringName.op_Implicit(text), ((GodotObject)source).Get(StringName.op_Implicit(text)));
				}
			}
		}

		private static void FreeGroup(Transition t)
		{
			try
			{
				if (t.Group != null && GodotObject.IsInstanceValid((GodotObject)(object)t.Group) && !((GodotObject)t.Group).IsQueuedForDeletion())
				{
					((CanvasItem)t.Group).Visible = false;
					((Node)t.Group).QueueFree();
					return;
				}
			}
			catch
			{
			}
			try
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)t.Ghost) && !((GodotObject)t.Ghost).IsQueuedForDeletion())
				{
					((CanvasItem)t.Ghost).Visible = false;
					((Node)t.Ghost).QueueFree();
				}
			}
			catch
			{
			}
		}

		private static void DisableSmooth(Exception ex)
		{
			_failStreak++;
			if (_failStreak < 3)
			{
				if (!_warned)
				{
					try
					{
						ModEntry.Log.Warn("[HpWear] Smooth stage crossfade failed (retry allowed): " + ex.Message, 1);
						return;
					}
					catch
					{
						return;
					}
				}
				return;
			}
			_smoothUnavailable = true;
			if (_warned)
			{
				return;
			}
			_warned = true;
			try
			{
				ModEntry.Log.Warn("[HpWear] Smooth stage crossfade unavailable; keeping direct skeleton swap: " + ex.Message, 1);
			}
			catch
			{
			}
		}
	}
	public static class AbandonWearTarget
	{
		public const int None = 0;

		public const int Combat = 1;

		public const int Rest = 2;

		public const int Merchant = 3;

		public const int DeathSpine = 4;
	}
	public readonly record struct AbandonWearDecision(int Target, int MaxStage, bool ShouldSwap);
	public readonly record struct DeathWearDecision(int MaxStage, bool ShouldSwap);
	internal static class WearStageRuntime
	{
		private static WearStageSettings? _live;

		private static WearStageSettings? _combatSnapshot;

		private static Func<WearStageSettings>? _provider;

		private static bool _loggedBadConfig;

		private static bool _thresholdReapplyQueued;

		internal static bool HasProvider => _provider != null;

		internal static bool ForceDeepestOnDeath
		{
			get
			{
				if (GetLive().WearEnabled)
				{
					return GetLive().ForceDeepestOnDeath;
				}
				return false;
			}
		}

		internal static int GetDeepestEnabledStage()
		{
			return StageFor(0, int.MaxValue, useCombatSnapshot: false);
		}

		internal static void SetProvider(Func<WearStageSettings>? provider)
		{
			_provider = provider;
			_live = null;
		}

		internal static WearStageSettings GetLive()
		{
			if (_provider != null)
			{
				try
				{
					WearStageSettings wearStageSettings = _provider();
					if (wearStageSettings != null)
					{
						NormalizeAndWarn(wearStageSettings);
						_live = wearStageSettings;
						return wearStageSettings;
					}
				}
				catch
				{
				}
			}
			return _live ?? (_live = WearStageSettings.CreateDefault());
		}

		internal static void InvalidateLiveCache()
		{
			_live = null;
		}

		internal static void CaptureCombatSnapshot()
		{
			_combatSnapshot = GetLive().Clone();
		}

		internal static void ClearCombatSnapshot()
		{
			_combatSnapshot = null;
		}

		internal static void ApplyThresholdsNow()
		{
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			InvalidateLiveCache();
			ClearCombatSnapshot();
			if (!GetLive().WearEnabled)
			{
				try
				{
					ModEntry.Log.Info("[wear-stage] 换装总开关为关（恒第1档）。按安全键或在设置里打开「启用血量换装」即可恢复。", 1);
				}
				catch
				{
				}
			}
			try
			{
				BattleWear.ForceReapplyWear();
			}
			catch
			{
			}
			if (_thresholdReapplyQueued)
			{
				return;
			}
			_thresholdReapplyQueued = true;
			try
			{
				Callable val = Callable.From((Action)delegate
				{
					_thresholdReapplyQueued = false;
					try
					{
						BattleWear.ForceReapplyWear();
					}
					catch
					{
					}
				});
				((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			}
			catch
			{
				_thresholdReapplyQueued = false;
			}
		}

		internal static WearStageSettings GetForCombat()
		{
			return _combatSnapshot ?? GetLive();
		}

		internal static int StageFor(int currentHp, int maxHp, bool useCombatSnapshot)
		{
			if (!GetLive().WearEnabled)
			{
				return 1;
			}
			WearStageSettings wearStageSettings = (useCombatSnapshot ? GetForCombat() : GetLive());
			NormalizeAndWarn(wearStageSettings);
			if (!wearStageSettings.UseAbsoluteHp)
			{
				return StageForPercent(wearStageSettings, currentHp, maxHp);
			}
			return StageForAbsolute(wearStageSettings, currentHp);
		}

		internal static int StageForDeath(int currentHp, int maxHp)
		{
			WearStageSettings live = GetLive();
			if (!live.WearEnabled)
			{
				return 1;
			}
			if (!live.ForceDeepestOnDeath)
			{
				return StageFor(currentHp, maxHp, useCombatSnapshot: false);
			}
			return GetDeepestEnabledStage();
		}

		internal static bool NeedsCombatSkeletonApply(int appliedStage, int targetStage, ulong appliedSkeletonId, ulong liveSkeletonId, bool pristinePageMatches)
		{
			if (appliedStage != targetStage)
			{
				return true;
			}
			if (appliedSkeletonId != 0L && liveSkeletonId != 0L)
			{
				return appliedSkeletonId != liveSkeletonId;
			}
			return !pristinePageMatches;
		}

		private static void NormalizeAndWarn(WearStageSettings s)
		{
			string error;
			string error2;
			bool num = s.TryValidatePercent(out error) & s.TryValidateAbsolute(out error2);
			bool flag = double.IsFinite(s.VisualScale) && s.VisualScale >= 0.5 && s.VisualScale <= 2.0;
			if ((!num || !flag) && !_loggedBadConfig)
			{
				_loggedBadConfig = true;
				TryLogWarn("[wear-stage] 非法配置已回退对应默认值（50% / 40血 / 1.00倍）: " + error + " " + error2 + (flag ? "" : " 缩放非法"));
			}
			s.Normalize();
		}

		private static void TryLogWarn(string message)
		{
			try
			{
				ModEntry.Log.Warn(message, 1);
			}
			catch
			{
			}
		}

		private static int StageForPercent(WearStageSettings s, int currentHp, int maxHp)
		{
			if (maxHp <= 0)
			{
				return 1;
			}
			if (s.PercentThreshold <= 0 || (long)currentHp * 100L > (long)s.PercentThreshold * (long)maxHp)
			{
				return 1;
			}
			return 2;
		}

		private static int StageForAbsolute(WearStageSettings s, int currentHp)
		{
			if (s.AbsoluteThreshold <= 0 || currentHp > s.AbsoluteThreshold)
			{
				return 1;
			}
			return 2;
		}

		internal static string BuildStatusLine()
		{
			WearStageSettings live = GetLive();
			StringBuilder stringBuilder = new StringBuilder();
			if (live.UseAbsoluteHp)
			{
				stringBuilder.Append(live.TryValidateAbsolute(out string error) ? live.FormatPreviewAbsolute() : ("校验失败：" + error));
			}
			else
			{
				stringBuilder.Append(live.TryValidatePercent(out string error2) ? live.FormatPreviewPercent() : ("校验失败：" + error2));
			}
			stringBuilder.Append(live.WearEnabled ? " | 换装：开" : " | 换装：关（恒第1档）");
			stringBuilder.Append(" | 死亡强制最低有效血量档：").Append(live.ForceDeepestOnDeath ? "开" : "关");
			return stringBuilder.ToString();
		}
	}
	[HarmonyPatch(typeof(NRun), "_Ready")]
	internal static class BattleWearTimerPatch
	{
		internal const double TickIntervalSec = 0.12;

		[HarmonyPostfix]
		private static void Postfix(NRun __instance)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Expected O, but got Unknown
			try
			{
				Node val = (Node)__instance.GlobalUi;
				string text = "BattleWearTimer_" + ModEntry.ModId;
				if (val != null && !val.HasNode(NodePath.op_Implicit(text)))
				{
					Timer val2 = new Timer();
					((Node)val2).Name = StringName.op_Implicit(text);
					val2.WaitTime = 0.12;
					val2.OneShot = false;
					val2.Autostart = true;
					val2.Timeout += BattleWear.Tick;
					val.AddChild((Node)val2, false, (InternalMode)0);
					val2.Start(-1.0);
					ModEntry.Log.Info("wear timer attached.", 1);
				}
			}
			catch (Exception value)
			{
				ModEntry.Log.Error($"failed to attach wear timer: {value}", 1);
			}
		}
	}
	internal static class LowHpAnimRedirectPatch
	{
		internal const string LowHpAnim = "low_health_loop";

		internal const string IdleAnim = "idle_loop";

		internal const string BlockedAnim = "low_health_loop";

		internal const string ReplacementAnim = "idle_loop";

		internal static bool LowHealthClipReady { get; private set; }

		private static bool IsDefectAssembly => string.Equals(ModEntry.ModId, "defectHpWear", StringComparison.OrdinalIgnoreCase);

		internal static void ClearLowHealthClipReady()
		{
			LowHealthClipReady = false;
		}

		internal static void NoteLowHealthClip(MegaSprite? sprite)
		{
			try
			{
				if (sprite != null && sprite.HasAnimation("low_health_loop"))
				{
					LowHealthClipReady = true;
				}
			}
			catch
			{
			}
		}

		internal static void NoteLowHealthClip(bool ready)
		{
			if (ready)
			{
				LowHealthClipReady = true;
			}
			else
			{
				LowHealthClipReady = false;
			}
		}

		internal static string Redirect(string? name, bool allowPromote = true)
		{
			if (!SkinSelectionGate.SceneAllowed())
			{
				return name;
			}
			if (name == null)
			{
				return "idle_loop";
			}
			return name;
		}

		private static string RedirectDefectPromote(string? name, bool allowPromote)
		{
			if (name == null)
			{
				return "idle_loop";
			}
			return name;
		}

		internal static bool HasOwnedDefectPlayerAtLowHealth()
		{
			return false;
		}

		private static bool ShouldBlockOwnedLowHealthLegacy()
		{
			try
			{
				RunManager instance = RunManager.Instance;
				object obj;
				if (instance == null)
				{
					obj = null;
				}
				else
				{
					RunState val = instance.DebugOnlyGetState();
					obj = ((val != null) ? val.Players : null);
				}
				IReadOnlyList<Player> readOnlyList = (IReadOnlyList<Player>)obj;
				if (readOnlyList == null)
				{
					return false;
				}
				foreach (Player item in readOnlyList)
				{
					CharacterModel val2 = ((item != null) ? item.Character : null);
					if (val2 is Ironclad && BattleWear.OwnsCharacter("ironclad"))
					{
						return true;
					}
					if (val2 is Defect && BattleWear.OwnsCharacter("defect"))
					{
						return true;
					}
					if (val2 is Regent && BattleWear.OwnsCharacter("regent"))
					{
						return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}
	}
	internal static class LowWearAttackGate
	{
		private static Creature? _lastAttacker;

		private static string? _lastAttackerMonsterId;

		private static bool _hasAttackIntents;

		private static bool _lastMoveDone;

		private static bool _sawPerforming;

		private static bool _sawHurtAfterArm;

		private static bool _releasedLogged;

		private static bool _deathBypass;

		private static bool _releasedThisRound;

		private static int _armedRound = -1;

		private static bool _patchesApplied;

		private static readonly HashSet<Creature> LowAtEnemyTurnStart = new HashSet<Creature>();

		private static CombatState? _snapshotState;

		internal static bool HasPendingAttackGate => ShouldHoldLowBridge(TryGetOwnedPlayerAnim());

		internal static bool ShouldBlockLowHealthPromote(Creature? owner = null)
		{
			EnsureArmedFromCombatTick();
			if (owner != null && !owner.IsDead && owner.MaxHp > 0 && LowHealthIdleRules.MatchesCreature(owner) && LowAtEnemyTurnStart.Contains(owner))
			{
				return false;
			}
			return ShouldHoldLowBridge(TryGetOwnedPlayerAnim());
		}

		internal static void BypassForDeath(string? reason = null)
		{
			_deathBypass = true;
			_hasAttackIntents = false;
			_lastAttacker = null;
			_lastAttackerMonsterId = null;
			_lastMoveDone = true;
			_sawPerforming = false;
			_sawHurtAfterArm = false;
			_releasedLogged = true;
			_releasedThisRound = true;
			ModEntry.Log.Info("AttackGate: death bypass (" + (reason ?? "death") + ").", 1);
		}

		internal static void Reset(string? reason = null)
		{
			if ((_hasAttackIntents || _deathBypass || _releasedThisRound) && !string.IsNullOrEmpty(reason))
			{
				ModEntry.Log.Info("AttackGate: reset (" + reason + ").", 1);
			}
			LowAtEnemyTurnStart.Clear();
			_snapshotState = null;
			_lastAttacker = null;
			_lastAttackerMonsterId = null;
			_hasAttackIntents = false;
			_lastMoveDone = false;
			_sawPerforming = false;
			_sawHurtAfterArm = false;
			_releasedLogged = false;
			_deathBypass = false;
			_releasedThisRound = false;
			_armedRound = -1;
		}

		internal static void OnEnemyTurnEnding()
		{
			if (_deathBypass)
			{
				return;
			}
			if (!_hasAttackIntents || _releasedThisRound)
			{
				_releasedThisRound = true;
				return;
			}
			if (!OwnsLivingCombatPlayer())
			{
				_releasedThisRound = true;
				LogRelease("enemy-turn-end — not our combatant");
				return;
			}
			if (!_lastMoveDone)
			{
				_lastMoveDone = true;
				_releasedThisRound = true;
				LogRelease("enemy-turn-end force (last attack never detected)");
				return;
			}
			string text = TryGetOwnedPlayerAnim();
			if (IsHurtAnim(text) || (text == null && _sawHurtAfterArm))
			{
				if (IsHurtAnim(text))
				{
					_sawHurtAfterArm = true;
				}
				ModEntry.Log.Info("AttackGate: enemy turn ended — keep hold (still hurt / await Tick).", 1);
			}
			else
			{
				_releasedThisRound = true;
				LogRelease(_sawHurtAfterArm ? "enemy-turn-end — hurt already finished" : "enemy-turn-end after last-move (not hurt)");
			}
		}

		internal static void EnsureArmedFromCombatTick()
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Invalid comparison between Unknown and I4
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Invalid comparison between Unknown and I4
			if (!SkinSelectionGate.SceneAllowed())
			{
				return;
			}
			try
			{
				CombatManager instance = CombatManager.Instance;
				if (instance == null)
				{
					return;
				}
				CombatState val = instance.DebugOnlyGetState();
				if (val == null)
				{
					object? obj = AccessTools.Field(typeof(CombatManager), "_state")?.GetValue(instance);
					val = (CombatState)((obj is CombatState) ? obj : null);
				}
				if (val == null)
				{
					return;
				}
				if ((int)val.CurrentSide != 2)
				{
					if (_armedRound >= 0 && (int)val.CurrentSide == 1 && (!_hasAttackIntents || _releasedThisRound))
					{
						_armedRound = -1;
						_hasAttackIntents = false;
						_lastAttacker = null;
						_lastAttackerMonsterId = null;
						_deathBypass = false;
					}
				}
				else if (!_deathBypass && _armedRound != val.RoundNumber)
				{
					SnapshotEnemyTurn(val, val.RoundNumber);
				}
			}
			catch (Exception ex)
			{
				ModEntry.Log.Error("AttackGate EnsureArmed failed: " + ex.Message, 1);
			}
		}

		internal static void SnapshotEnemyTurn(CombatState? state, int? round = null)
		{
			int num = round ?? ((state != null) ? state.RoundNumber : (-1));
			if (state != null && state == _snapshotState && _armedRound == num)
			{
				return;
			}
			_snapshotState = state;
			LowAtEnemyTurnStart.Clear();
			RunManager instance = RunManager.Instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				RunState obj2 = instance.DebugOnlyGetState();
				obj = ((obj2 != null) ? obj2.Players : null);
			}
			if (obj == null)
			{
				obj = new List<Player>();
			}
			foreach (Player item in (IEnumerable<Player>)obj)
			{
				Creature creature = item.Creature;
				if (BattleWear.ProfileFor(item) != null && creature != null && !creature.IsDead && creature.MaxHp > 0 && LowHealthIdleRules.MatchesCreature(creature))
				{
					LowAtEnemyTurnStart.Add(creature);
				}
			}
			_deathBypass = false;
			_releasedThisRound = false;
			_lastAttacker = null;
			_lastAttackerMonsterId = null;
			_hasAttackIntents = false;
			_lastMoveDone = false;
			_sawPerforming = false;
			_sawHurtAfterArm = false;
			_releasedLogged = false;
			_armedRound = round ?? ((state != null) ? state.RoundNumber : (-1));
			if (!OwnsLivingCombatPlayer())
			{
				_releasedThisRound = true;
			}
			else
			{
				if (state == null)
				{
					return;
				}
				Creature val = null;
				foreach (Creature enemy in state.Enemies)
				{
					if (enemy != null && !enemy.IsDead)
					{
						MonsterModel monster = enemy.Monster;
						if (monster != null && monster.IntendsToAttack)
						{
							val = enemy;
						}
					}
				}
				_lastAttacker = val;
				object lastAttackerMonsterId;
				if (val == null)
				{
					lastAttackerMonsterId = null;
				}
				else
				{
					MonsterModel monster2 = val.Monster;
					lastAttackerMonsterId = ((monster2 != null) ? ((AbstractModel)monster2).Id.Entry : null);
				}
				_lastAttackerMonsterId = (string?)lastAttackerMonsterId;
				_hasAttackIntents = val != null;
				if (_hasAttackIntents)
				{
					ModEntry.Log.Info($"AttackGate: armed round {_armedRound} — last attack intent = '{_lastAttackerMonsterId}'.", 1);
				}
				else
				{
					_releasedThisRound = true;
					ModEntry.Log.Info($"AttackGate: armed round {_armedRound} — no attack intents (no hold).", 1);
				}
			}
		}

		internal static void OnMonsterMovePerformed(MonsterModel monster)
		{
			if (!_deathBypass && !_releasedThisRound && _hasAttackIntents && monster != null)
			{
				string entry = ((AbstractModel)monster).Id.Entry;
				if (_lastAttacker != null && (monster.Creature == _lastAttacker || monster == _lastAttacker.Monster))
				{
					MarkLastMoveDone("history:" + entry);
				}
			}
		}

		private static void PollLastAttackerProgress()
		{
			if (_deathBypass || _releasedThisRound || !_hasAttackIntents || _lastMoveDone)
			{
				return;
			}
			Creature? lastAttacker = _lastAttacker;
			MonsterModel val = ((lastAttacker != null) ? lastAttacker.Monster : null);
			if (val == null)
			{
				return;
			}
			try
			{
				if (val.IsPerformingMove)
				{
					_sawPerforming = true;
				}
				else if (_sawPerforming)
				{
					MarkLastMoveDone("poll-perform-end:" + ((AbstractModel)val).Id.Entry);
				}
			}
			catch
			{
			}
		}

		private static void MarkLastMoveDone(string why)
		{
			if (!_lastMoveDone)
			{
				_lastMoveDone = true;
				_releasedLogged = false;
				ModEntry.Log.Info("AttackGate: last attacker move done (" + why + ") — wait hurt end.", 1);
			}
		}

		internal static bool ShouldHoldLowBridge(string? currentAnim, Creature? creature = null)
		{
			if (!SkinSelectionGate.ForTarget(creature))
			{
				return false;
			}
			if (!OwnsLivingCombatPlayer())
			{
				QuietDisarmForeign();
				return false;
			}
			EnsureArmedFromCombatTick();
			PollLastAttackerProgress();
			if (_deathBypass || _releasedThisRound)
			{
				return false;
			}
			if (creature != null && (creature.IsDead || creature.CurrentHp <= 0))
			{
				BypassForDeath(creature.IsDead ? "creature-dead" : "hp0");
				return false;
			}
			if (!_hasAttackIntents)
			{
				return false;
			}
			if (_lastAttacker != null && (_lastAttacker.IsDead || !IsStillInCombat(_lastAttacker)))
			{
				MarkLastMoveDone("attacker-gone");
			}
			if (!_lastMoveDone)
			{
				if (_sawPerforming && IsHurtAnim(currentAnim))
				{
					_sawHurtAfterArm = true;
				}
				return true;
			}
			if (string.IsNullOrEmpty(currentAnim))
			{
				return true;
			}
			if (IsHurtAnim(currentAnim))
			{
				_sawHurtAfterArm = true;
				return true;
			}
			if (_sawHurtAfterArm)
			{
				_releasedThisRound = true;
				LogRelease("last hurt ended");
				return false;
			}
			_releasedThisRound = true;
			LogRelease("post-move not-hurt (hurt finished in-move or none)");
			return false;
		}

		private static void QuietDisarmForeign()
		{
			if (_hasAttackIntents || _releasedThisRound || _armedRound >= 0)
			{
				_hasAttackIntents = false;
				_releasedThisRound = true;
				_lastAttacker = null;
				_lastAttackerMonsterId = null;
				_armedRound = -1;
			}
		}

		private static bool OwnsLivingCombatPlayer()
		{
			try
			{
				if (!DriverAllowsGate())
				{
					return false;
				}
				RunManager instance = RunManager.Instance;
				RunState obj = ((instance != null) ? instance.DebugOnlyGetState() : null);
				IReadOnlyList<Player> readOnlyList = ((obj != null) ? obj.Players : null);
				if (readOnlyList == null)
				{
					return false;
				}
				foreach (Player item in readOnlyList)
				{
					if (BattleWear.ProfileFor(item) != null)
					{
						Creature creature = item.Creature;
						if (creature != null && !creature.IsDead && creature.CurrentHp > 0)
						{
							return true;
						}
					}
				}
			}
			catch
			{
			}
			return false;
		}

		private static bool DriverAllowsGate()
		{
			try
			{
				Type type = typeof(LowWearAttackGate).Assembly.GetType("BattleWear.WearActivation");
				if (type == null)
				{
					return true;
				}
				MethodInfo methodInfo = AccessTools.Method(type, "IsActiveDriver", (Type[])null, (Type[])null);
				if (methodInfo == null)
				{
					return true;
				}
				object obj = methodInfo.Invoke(null, null);
				bool flag = default(bool);
				int num;
				if (obj is bool)
				{
					flag = (bool)obj;
					num = 1;
				}
				else
				{
					num = 0;
				}
				return (byte)((uint)num & (flag ? 1u : 0u)) != 0;
			}
			catch
			{
				return true;
			}
		}

		private static string? TryGetOwnedPlayerAnim()
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				foreach (KeyValuePair<ulong, BattleWear.CombatWearState> combatState in BattleWear.CombatStates)
				{
					NCreature node = combatState.Value.Node;
					if (node != null && GodotObject.IsInstanceValid((GodotObject)(object)node))
					{
						NCreatureVisuals visuals = node.Visuals;
						if (visuals != null)
						{
							return SpineAnimCompat.GetCurrentAnimationName(visuals.SpineAnimation);
						}
					}
				}
			}
			catch
			{
			}
			return null;
		}

		private static void LogRelease(string why)
		{
			if (!_releasedLogged)
			{
				_releasedLogged = true;
				ModEntry.Log.Info("AttackGate: release — " + why + ".", 1);
			}
		}

		private static bool IsHurtAnim(string? anim)
		{
			if (string.IsNullOrEmpty(anim))
			{
				return false;
			}
			if (!anim.Equals("hurt", StringComparison.OrdinalIgnoreCase))
			{
				return anim.Equals("hit", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}

		private static bool IsStillInCombat(Creature creature)
		{
			try
			{
				ICombatState combatState = creature.CombatState;
				if (combatState == null)
				{
					return false;
				}
				foreach (Creature enemy in combatState.Enemies)
				{
					if (enemy == creature)
					{
						return true;
					}
				}
				return false;
			}
			catch
			{
				return true;
			}
		}

		internal static int ApplyHarmony(Harmony harmony)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Expected O, but got Unknown
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Expected O, but got Unknown
			if (_patchesApplied)
			{
				return 0;
			}
			int num = 0;
			try
			{
				MethodInfo methodInfo = AccessTools.Method(typeof(CombatManager), "ExecuteEnemyTurn", (Type[])null, (Type[])null);
				if (methodInfo != null)
				{
					harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(LowWearAttackGatePatches), "ExecuteEnemyTurnPrefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					num++;
				}
				else
				{
					ModEntry.Log.Warn("AttackGate: ExecuteEnemyTurn not found.", 1);
				}
				MethodInfo methodInfo2 = AccessTools.Method(typeof(CombatManager), "EndEnemyTurn", (Type[])null, (Type[])null);
				if (methodInfo2 != null)
				{
					harmony.Patch((MethodBase)methodInfo2, new HarmonyMethod(typeof(LowWearAttackGatePatches), "EndEnemyTurnPrefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					num++;
				}
				MethodInfo methodInfo3 = AccessTools.Method(typeof(CombatHistory), "MonsterPerformedMove", (Type[])null, (Type[])null);
				if (methodInfo3 != null)
				{
					harmony.Patch((MethodBase)methodInfo3, (HarmonyMethod)null, new HarmonyMethod(typeof(LowWearAttackGatePatches), "MonsterPerformedMovePostfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
					num++;
				}
				_patchesApplied = true;
				ModEntry.Log.Info($"AttackGate: harmony patches applied ({num}).", 1);
			}
			catch (Exception ex)
			{
				ModEntry.Log.Error("AttackGate: harmony apply failed: " + ex.Message, 1);
			}
			return num;
		}

		internal static void BypassForOwnedPlayerDeath(NCreature? node, string? reason)
		{
			if (node != null && GodotObject.IsInstanceValid((GodotObject)(object)node))
			{
				Creature entity = node.Entity;
				if (entity != null && entity.IsPlayer && entity.CurrentHp <= 0 && entity.Player != null && DriverAllowsGate() && BattleWear.ProfileFor(entity.Player) != null)
				{
					BypassForDeath(reason);
				}
			}
		}
	}
	internal static class LowWearAttackGatePatches
	{
		public static void ExecuteEnemyTurnPrefix(CombatManager __instance)
		{
			if (!SkinSelectionGate.ForTarget(__instance))
			{
				return;
			}
			try
			{
				CombatState val = __instance.DebugOnlyGetState();
				if (val == null)
				{
					object? obj = AccessTools.Field(typeof(CombatManager), "_state")?.GetValue(__instance);
					val = (CombatState)((obj is CombatState) ? obj : null);
				}
				LowWearAttackGate.SnapshotEnemyTurn(val, (val != null) ? new int?(val.RoundNumber) : ((int?)null));
			}
			catch (Exception ex)
			{
				ModEntry.Log.Error("AttackGate snapshot failed: " + ex.Message, 1);
				LowWearAttackGate.Reset("snapshot-error");
			}
		}

		public static void EndEnemyTurnPrefix()
		{
			if (SkinSelectionGate.SceneAllowed())
			{
				LowWearAttackGate.OnEnemyTurnEnding();
			}
		}

		public static void MonsterPerformedMovePostfix(MonsterModel monster)
		{
			LowWearAttackGate.OnMonsterMovePerformed(monster);
		}
	}
	internal static class PatchBootstrap
	{
		[CompilerGenerated]
		private sealed class <EnumeratePlayers>d__19 : IEnumerable<Player>, IEnumerable, IEnumerator<Player>, IEnumerator, IDisposable
		{
			private int <>1__state;

			private Player <>2__current;

			private int <>l__initialThreadId;

			private IEnumerator<Player> <>7__wrap1;

			Player IEnumerator<Player>.Current
			{
				[DebuggerHidden]
				get
				{
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <EnumeratePlayers>d__19(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				int num = <>1__state;
				if (num == -3 || num == 1)
				{
					try
					{
					}
					finally
					{
						<>m__Finally1();
					}
				}
				<>7__wrap1 = null;
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				try
				{
					switch (<>1__state)
					{
					default:
						return false;
					case 0:
					{
						<>1__state = -1;
						IReadOnlyList<Player> readOnlyList = null;
						try
						{
							RunManager instance = RunManager.Instance;
							object obj;
							if (instance == null)
							{
								obj = null;
							}
							else
							{
								RunState val = instance.DebugOnlyGetState();
								obj = ((val != null) ? val.Players : null);
							}
							readOnlyList = (IReadOnlyList<Player>)obj;
						}
						catch
						{
						}
						if (readOnlyList == null)
						{
							return false;
						}
						<>7__wrap1 = readOnlyList.GetEnumerator();
						<>1__state = -3;
						break;
					}
					case 1:
						<>1__state = -3;
						break;
					}
					if (<>7__wrap1.MoveNext())
					{
						Player current = <>7__wrap1.Current;
						<>2__current = current;
						<>1__state = 1;
						return true;
					}
					<>m__Finally1();
					<>7__wrap1 = null;
					return false;
				}
				catch
				{
					//try-fault
					((IDisposable)this).Dispose();
					throw;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			private void <>m__Finally1()
			{
				<>1__state = -1;
				if (<>7__wrap1 != null)
				{
					<>7__wrap1.Dispose();
				}
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Player> IEnumerable<Player>.GetEnumerator()
			{
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					return this;
				}
				return new <EnumeratePlayers>d__19(0);
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Player>)this).GetEnumerator();
			}
		}

		private static readonly AnimState LowHealthLoopState;

		private static bool BlockLowForOwner(object instance)
		{
			try
			{
				object obj = ((!(instance is CreatureAnimator)) ? instance : AccessTools.Field(typeof(CreatureAnimator), "_spineController")?.GetValue(instance));
				ulong num = BoundId((MegaSpineBinding?)((obj is MegaSpineBinding) ? obj : null));
				if (num == 0L)
				{
					return false;
				}
				foreach (Player item in EnumeratePlayers())
				{
					if (BattleWear.ProfileFor(item) == null)
					{
						continue;
					}
					Creature creature = item.Creature;
					object obj2;
					if (creature == null)
					{
						obj2 = null;
					}
					else
					{
						NCreature creatureNode = creature.GetCreatureNode();
						if (creatureNode == null)
						{
							obj2 = null;
						}
						else
						{
							NCreatureVisuals visuals = creatureNode.Visuals;
							obj2 = ((visuals != null) ? visuals.SpineBody : null);
						}
					}
					MegaSprite val = (MegaSprite)obj2;
					if (val != null && (BoundId((MegaSpineBinding?)(object)val) == num || BoundId((MegaSpineBinding?)(object)val.TryGetAnimationState()) == num))
					{
						return LowWearAttackGate.ShouldBlockLowHealthPromote(item.Creature);
					}
				}
			}
			catch
			{
			}
			return false;
		}

		public static int ApplyOptional(Harmony harmony)
		{
			int num = 0;
			num += TryPatchIsLowHealth(harmony);
			num += TryPatchMegaAnimationRedirect(harmony, "SetAnimation", true, typeof(string), typeof(bool), typeof(int));
			num += TryPatchMegaAnimationRedirect(harmony, "SetAnimation", true, typeof(string), typeof(bool));
			num += TryPatchMegaAnimationRedirect(harmony, "AddAnimation", false, typeof(string), typeof(float), typeof(bool), typeof(int));
			num += TryPatchMegaAnimationRedirect(harmony, "AddAnimationTracked", false, typeof(string), typeof(float), typeof(bool), typeof(int));
			num += TryPatchSpineAnimationAccessRedirect(harmony, "SetAnimation", true, typeof(string), typeof(bool), typeof(int));
			num += TryPatchSpineAnimationAccessRedirect(harmony, "AddAnimation", false, typeof(string), typeof(float), typeof(bool), typeof(int));
			if (string.Equals(ModEntry.ModId, "defectHpWear", StringComparison.OrdinalIgnoreCase))
			{
				num += TryPatchCreatureAnimatorIdle(harmony);
				num += TryPatchCreatureAnimatorTransitions(harmony);
			}
			return num;
		}

		private static int TryPatchIsLowHealth(Harmony harmony)
		{
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Expected O, but got Unknown
			try
			{
				MethodInfo methodInfo = AccessTools.PropertyGetter(typeof(CharacterModel), "IsLowHealth");
				if (methodInfo == null)
				{
					ModEntry.Log.Info("IsLowHealth not found — low-HP suppress skipped (pre-v0.111).", 1);
					return 0;
				}
				if (methodInfo.ReturnType != typeof(Func<Creature, bool>))
				{
					ModEntry.Log.Info("IsLowHealth return type " + methodInfo.ReturnType.Name + " unsupported — low-HP suppress skipped.", 1);
					return 0;
				}
				harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(typeof(PatchBootstrap), "IsLowHealthPostfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
				ModEntry.Log.Info("patched CharacterModel.IsLowHealth for wear characters.", 1);
				return 1;
			}
			catch (Exception ex)
			{
				ModEntry.Log.Error("IsLowHealth patch failed: " + ex.Message, 1);
				return 0;
			}
		}

		public static void IsLowHealthPostfix(CharacterModel __instance, ref Func<Creature, bool> __result)
		{
			if (__result != null && SuppressLowHpAnimPatch.ShouldSuppress(__instance))
			{
				__result = (Creature _) => false;
			}
		}

		private static int TryPatchMegaAnimationRedirect(Harmony harmony, string methodName, bool promote, params Type[] args)
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			try
			{
				MethodInfo methodInfo = AccessTools.Method(typeof(MegaAnimationState), methodName, args, (Type[])null);
				if (methodInfo == null)
				{
					return 0;
				}
				string text = (promote ? "RedirectSetPrefix" : "RedirectQueuePrefix");
				harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(PatchBootstrap), text, (Type[])null)
				{
					priority = 0
				}, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				ModEntry.Log.Info($"patched MegaAnimationState.{methodName} low-HP (promote={promote}).", 1);
				return 1;
			}
			catch (Exception ex)
			{
				ModEntry.Log.Error("MegaAnimationState." + methodName + " patch failed: " + ex.Message, 1);
				return 0;
			}
		}

		private static int TryPatchSpineAnimationAccessRedirect(Harmony harmony, string methodName, bool promote, params Type[] args)
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			try
			{
				MethodInfo methodInfo = AccessTools.Method(typeof(SpineAnimationAccess), methodName, args, (Type[])null);
				if (methodInfo == null)
				{
					return 0;
				}
				string text = (promote ? "RedirectSetPrefix" : "RedirectQueuePrefix");
				harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(PatchBootstrap), text, (Type[])null)
				{
					priority = 0
				}, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				ModEntry.Log.Info($"patched SpineAnimationAccess.{methodName} low-HP (promote={promote}).", 1);
				return 1;
			}
			catch (Exception ex)
			{
				ModEntry.Log.Error("SpineAnimationAccess." + methodName + " patch failed: " + ex.Message, 1);
				return 0;
			}
		}

		private static int TryPatchCreatureAnimatorIdle(Harmony harmony)
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			try
			{
				MethodInfo methodInfo = AccessTools.Method(typeof(CreatureAnimator), "SetTrigger", new Type[1] { typeof(string) }, (Type[])null);
				if (methodInfo == null)
				{
					return 0;
				}
				harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(PatchBootstrap), "CreatureAnimatorSetTriggerPrefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				ModEntry.Log.Info("patched CreatureAnimator.SetTrigger for defect low-HP Idle/Revive.", 1);
				return 1;
			}
			catch (Exception ex)
			{
				ModEntry.Log.Error("CreatureAnimator.SetTrigger patch failed: " + ex.Message, 1);
				return 0;
			}
		}

		private static int TryPatchCreatureAnimatorTransitions(Harmony harmony)
		{
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Expected O, but got Unknown
			int num = 0;
			string[] array = new string[2] { "AddNextState", "SetNextState" };
			foreach (string text in array)
			{
				try
				{
					MethodInfo methodInfo = AccessTools.Method(typeof(CreatureAnimator), text, new Type[1] { typeof(AnimState) }, (Type[])null);
					if (!(methodInfo == null))
					{
						harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(PatchBootstrap), "CreatureAnimatorIdleStatePrefix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
						ModEntry.Log.Info("patched CreatureAnimator." + text + " for defect low-HP next-state.", 1);
						num++;
					}
				}
				catch (Exception ex)
				{
					ModEntry.Log.Error("CreatureAnimator." + text + " patch failed: " + ex.Message, 1);
				}
			}
			return (num > 0) ? 1 : 0;
		}

		public static void RedirectSetPrefix(object __instance, ref string __0)
		{
			if (!SkinSelectionGate.ForTarget(__instance) || BattleWear.SuppressLowHealthPromote)
			{
				return;
			}
			if (BlockLowForOwner(__instance))
			{
				if (!string.IsNullOrEmpty(__0) && __0.Equals("low_health_loop", StringComparison.OrdinalIgnoreCase))
				{
					__0 = "idle_loop";
				}
				return;
			}
			string text = __0;
			if (string.IsNullOrEmpty(text) || (!text.Equals("idle", StringComparison.OrdinalIgnoreCase) && !text.Equals("idle_loop", StringComparison.OrdinalIgnoreCase) && !text.Equals("low_health_loop", StringComparison.OrdinalIgnoreCase)))
			{
				return;
			}
			try
			{
				switch (OwnerLowHealthState(__instance))
				{
				case 1:
					if (text != null && text.Equals("low_health_loop", StringComparison.OrdinalIgnoreCase))
					{
						text = "idle_loop";
						goto IL_00f2;
					}
					break;
				case 2:
					{
						if (text.Equals("idle_loop", StringComparison.OrdinalIgnoreCase) || text.Equals("idle", StringComparison.OrdinalIgnoreCase))
						{
							text = "low_health_loop";
							goto IL_00f2;
						}
						break;
					}
					IL_00f2:
					__0 = text;
					break;
				}
			}
			catch
			{
			}
		}

		public static void RedirectQueuePrefix(object __instance, ref string __0)
		{
			if (!SkinSelectionGate.ForTarget(__instance) || BattleWear.SuppressLowHealthPromote)
			{
				return;
			}
			if (BlockLowForOwner(__instance))
			{
				if (!string.IsNullOrEmpty(__0) && __0.Equals("low_health_loop", StringComparison.OrdinalIgnoreCase))
				{
					__0 = "idle_loop";
				}
				return;
			}
			string text = __0;
			if (string.IsNullOrEmpty(text) || (!text.Equals("idle", StringComparison.OrdinalIgnoreCase) && !text.Equals("idle_loop", StringComparison.OrdinalIgnoreCase) && !text.Equals("low_health_loop", StringComparison.OrdinalIgnoreCase)))
			{
				return;
			}
			try
			{
				switch (OwnerLowHealthState(__instance))
				{
				case 1:
					if (text != null && text.Equals("low_health_loop", StringComparison.OrdinalIgnoreCase))
					{
						text = "idle_loop";
						goto default;
					}
					break;
				default:
					__0 = text;
					break;
				case 0:
					break;
				}
			}
			catch
			{
			}
		}

		public static void CreatureAnimatorIdleStatePrefix(CreatureAnimator __instance, ref AnimState state)
		{
			if (!SkinSelectionGate.ForTarget(__instance) || BattleWear.SuppressLowHealthPromote || BlockLowForOwner(__instance) || state == null)
			{
				return;
			}
			string id = state.Id;
			if (!id.Equals("idle_loop", StringComparison.OrdinalIgnoreCase) && !id.Equals("idle", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			try
			{
				if (OwnerLowHealthState(__instance) == 2)
				{
					state = LowHealthLoopState;
				}
			}
			catch
			{
			}
		}

		public static bool CreatureAnimatorSetTriggerPrefix(CreatureAnimator __instance, string trigger)
		{
			if (!SkinSelectionGate.ForTarget(__instance))
			{
				return true;
			}
			return true;
		}

		private static bool IsOwnedAnimator(CreatureAnimator animator)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			object obj = AccessTools.Field(typeof(CreatureAnimator), "_spineController")?.GetValue(animator);
			MegaSprite val = (MegaSprite)((obj is MegaSprite) ? obj : null);
			if (val != null)
			{
				return SpriteBelongsToOwned(val);
			}
			return false;
		}

		private static bool IsOwnedSpine(object? instance)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected O, but got Unknown
			MegaAnimationState val = (MegaAnimationState)((instance is MegaAnimationState) ? instance : null);
			if (val != null)
			{
				return StateBelongsToOwned(val);
			}
			MegaSprite val2 = SpriteFrom(instance);
			if (val2 != null)
			{
				return SpriteBelongsToOwned(val2);
			}
			return false;
		}

		private static bool StateBelongsToOwned(MegaAnimationState state)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Expected O, but got Unknown
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected O, but got Unknown
			ulong num = BoundId((MegaSpineBinding)state);
			if (num == 0L)
			{
				return false;
			}
			foreach (Player item in EnumeratePlayers())
			{
				if (!IsOwnedPlayer(item))
				{
					continue;
				}
				Creature creature = item.Creature;
				object obj;
				if (creature == null)
				{
					obj = null;
				}
				else
				{
					NCreature creatureNode = creature.GetCreatureNode();
					if (creatureNode == null)
					{
						obj = null;
					}
					else
					{
						NCreatureVisuals visuals = creatureNode.Visuals;
						if (visuals == null)
						{
							obj = null;
						}
						else
						{
							MegaSprite spineBody = visuals.SpineBody;
							obj = ((spineBody != null) ? spineBody.TryGetAnimationState() : null);
						}
					}
				}
				MegaAnimationState val = (MegaAnimationState)obj;
				if (val != null && BoundId((MegaSpineBinding)val) == num)
				{
					return true;
				}
			}
			return false;
		}

		private static bool SpriteBelongsToOwned(MegaSprite sprite)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Expected O, but got Unknown
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			ulong num = BoundId((MegaSpineBinding)sprite);
			if (num == 0L)
			{
				return false;
			}
			foreach (Player item in EnumeratePlayers())
			{
				if (!IsOwnedPlayer(item))
				{
					continue;
				}
				Creature creature = item.Creature;
				object obj;
				if (creature == null)
				{
					obj = null;
				}
				else
				{
					NCreature creatureNode = creature.GetCreatureNode();
					if (creatureNode == null)
					{
						obj = null;
					}
					else
					{
						NCreatureVisuals visuals = creatureNode.Visuals;
						obj = ((visuals != null) ? visuals.SpineBody : null);
					}
				}
				MegaSprite val = (MegaSprite)obj;
				if (val != null && BoundId((MegaSpineBinding)val) == num)
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsOwnedPlayer(Player? player)
		{
			if (!SkinSelectionGate.ForTarget(player))
			{
				return false;
			}
			CharacterModel val = ((player != null) ? player.Character : null);
			if (val is Ironclad && BattleWear.OwnsCharacter("ironclad"))
			{
				return true;
			}
			if (val is Defect && BattleWear.OwnsCharacter("defect"))
			{
				return true;
			}
			if (val is Regent && BattleWear.OwnsCharacter("regent"))
			{
				return true;
			}
			return false;
		}

		private static MegaSprite? SpriteFrom(object? instance)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			if (instance == null)
			{
				return null;
			}
			try
			{
				object obj = instance.GetType().GetField("_sprite", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(instance);
				return (MegaSprite)((obj is MegaSprite) ? obj : null);
			}
			catch
			{
				return null;
			}
		}

		[IteratorStateMachine(typeof(<EnumeratePlayers>d__19))]
		private static IEnumerable<Player> EnumeratePlayers()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <EnumeratePlayers>d__19(-2);
		}

		private static ulong BoundId(MegaSpineBinding? binding)
		{
			try
			{
				GodotObject val = ((binding != null) ? binding.BoundObject : null);
				if (val == null || !GodotObject.IsInstanceValid(val))
				{
					return 0uL;
				}
				return val.GetInstanceId();
			}
			catch
			{
				return 0uL;
			}
		}

		static PatchBootstrap()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			LowHealthLoopState = new AnimState("low_health_loop", true);
		}

		public static int OwnerLowHealthState(object instance)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Expected O, but got Unknown
			int result;
			try
			{
				result = 0;
				MegaSprite val = SpriteFrom(instance);
				if (val != null)
				{
					ulong num = BoundId((MegaSpineBinding)val);
					foreach (Player item in EnumeratePlayers())
					{
						if (!IsOwnedPlayer(item))
						{
							continue;
						}
						Creature creature = item.Creature;
						if (creature == null)
						{
							continue;
						}
						NCreature creatureNode = creature.GetCreatureNode();
						if (creatureNode == null)
						{
							continue;
						}
						NCreatureVisuals visuals = creatureNode.Visuals;
						if (visuals == null)
						{
							continue;
						}
						MegaSprite spineBody = visuals.SpineBody;
						if (spineBody == null || (spineBody != val && (num == 0L || BoundId((MegaSpineBinding)spineBody) != num)))
						{
							continue;
						}
						if (!creature.IsDead)
						{
							int maxHp = creature.MaxHp;
							if (maxHp > 0 && LowHealthIdleRules.MatchesCreature(creature))
							{
								result = 2;
								break;
							}
						}
						result = 1;
						break;
					}
				}
			}
			catch
			{
				result = 0;
			}
			return result;
		}
	}
	internal static class SuppressLowHpAnimPatch
	{
		internal static bool ShouldSuppress(CharacterModel model)
		{
			if (model is Ironclad)
			{
				return BattleWear.OwnsCharacter("ironclad");
			}
			if (model is Defect)
			{
				return BattleWear.OwnsCharacter("defect");
			}
			return false;
		}
	}
	internal static class CreatureDeathPatch
	{
	}
	public static class DeathWearPlan
	{
		public static DeathWearDecision Resolve(int currentStage, int thresholdCount)
		{
			int deepestEnabledStage = WearStageRuntime.GetDeepestEnabledStage();
			return new DeathWearDecision(deepestEnabledStage, currentStage != deepestEnabledStage);
		}
	}
	public static class AbandonWearPlan
	{
		public static AbandonWearDecision Resolve(bool hasCombatNode, bool hasRestState, bool hasMerchantState, int currentStage, int thresholdCount)
		{
			int deepestEnabledStage = WearStageRuntime.GetDeepestEnabledStage();
			if (hasCombatNode)
			{
				return new AbandonWearDecision(1, deepestEnabledStage, currentStage != deepestEnabledStage);
			}
			if (hasRestState)
			{
				return new AbandonWearDecision(2, deepestEnabledStage, currentStage != deepestEnabledStage);
			}
			if (hasMerchantState)
			{
				return new AbandonWearDecision(3, deepestEnabledStage, currentStage != deepestEnabledStage);
			}
			return new AbandonWearDecision(4, deepestEnabledStage, ShouldSwap: true);
		}
	}
	internal static class DeathForceWear
	{
		private static readonly FieldInfo? MerchantPlayersField = AccessTools.Field(typeof(NMerchantRoom), "_players");

		internal static void TryForceForCreature(Creature? creature, string reason)
		{
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Expected O, but got Unknown
			if (!SkinSelectionGate.ForTarget(creature))
			{
				return;
			}
			try
			{
				if (creature != null && creature.IsPlayer && !BattleWear._fuse)
				{
					NCreature val = null;
					try
					{
						NCombatRoom instance = NCombatRoom.Instance;
						val = ((instance != null) ? instance.GetCreatureNode(creature) : null);
					}
					catch
					{
					}
					if (val != null && GodotObject.IsInstanceValid((GodotObject)val))
					{
						TryForceOnCombatNode(val, reason);
					}
					else if (!TryForceRest(creature, reason))
					{
						TryForceMerchant(creature, reason);
					}
				}
			}
			catch (Exception ex)
			{
				try
				{
					ModEntry.Log.Error("death force for-creature failed (" + reason + "): " + ex.GetBaseException().Message, 1);
				}
				catch
				{
				}
			}
		}

		internal static void TryForceOnCombatNode(NCreature node, string reason)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			if (!SkinSelectionGate.ForTarget(node))
			{
				return;
			}
			try
			{
				LowWearAttackGate.BypassForOwnedPlayerDeath(node, reason);
				if (node == null || !GodotObject.IsInstanceValid((GodotObject)node) || BattleWear._fuse)
				{
					return;
				}
				Creature entity = node.Entity;
				if (entity == null || !entity.IsPlayer)
				{
					return;
				}
				ulong instanceId = ((GodotObject)node).GetInstanceId();
				if (!BattleWear.CombatStates.TryGetValue(instanceId, out BattleWear.CombatWearState value))
				{
					BattleWear.WearProfile wearProfile = BattleWear.ProfileFor(entity.Player);
					if (wearProfile == null)
					{
						return;
					}
					value = new BattleWear.CombatWearState
					{
						Node = node,
						Profile = wearProfile,
						Stage = 1,
						Fresh = true
					};
					BattleWear.CombatStates[instanceId] = value;
				}
				BattleWear.WearProfile profile = value.Profile;
				if (profile.Thresholds == null || profile.Thresholds.Length == 0)
				{
					return;
				}
				value.PendingStage = 0;
				value.MixDeferTicks = 0;
				value.MixDeferUntilMs = 0L;
				value.PendingAnimHint = null;
				BattleWear.SuppressLowHealthPromote = false;
				int num = WearStageRuntime.StageForDeath(entity.CurrentHp, entity.MaxHp);
				NCreatureVisuals visuals = node.Visuals;
				MegaSprite val = ((visuals != null) ? visuals.SpineBody : null);
				if (visuals != null && val != null)
				{
					GodotObject boundObject = ((MegaSpineBinding)val).BoundObject;
					Node val2 = (Node)(object)((boundObject is Node) ? boundObject : null);
					if (val2 != null)
					{
						WearStageCrossfade.Cancel(val2);
					}
					if (value.Stage != num)
					{
						int stage = value.Stage;
						value.Fresh = true;
						BattleWear.ApplyCombatStage(profile, entity, visuals, val, value, num, profile.FallbackIdle);
						ModEntry.Log.Info($"{profile.Name}: death force wear stage {stage}->{num} ({reason})", 1);
					}
					if (reason != "StartDeathAnim")
					{
						ReplayDie(node);
					}
				}
			}
			catch (Exception ex)
			{
				try
				{
					ModEntry.Log.Error("death force max stage failed (" + reason + "): " + ex.GetBaseException().Message, 1);
				}
				catch
				{
				}
			}
		}

		private static void ReplayDie(NCreature node)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				NCreatureVisuals visuals = node.Visuals;
				if (visuals != null)
				{
					SpineAnimCompat.SetAnimation(visuals.SpineAnimation, "die", loop: false);
				}
			}
			catch
			{
			}
		}

		private static bool TryForceRest(Creature creature, string reason)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			if (!SkinSelectionGate.ForTarget(creature))
			{
				return false;
			}
			try
			{
				foreach (BattleWear.RestWearState value in BattleWear.RestStates.Values)
				{
					NRestSiteCharacter node = value.Node;
					if (node == null || !GodotObject.IsInstanceValid((GodotObject)node))
					{
						continue;
					}
					Player player = node.Player;
					if (((player != null) ? player.Creature : null) == creature)
					{
						BattleWear.WearProfile wearProfile = BattleWear.ProfileFor(player);
						if (wearProfile?.Thresholds == null || wearProfile.Thresholds.Length == 0)
						{
							return true;
						}
						int num = WearStageRuntime.StageForDeath(creature.CurrentHp, creature.MaxHp);
						if (value.Stage != num)
						{
							int stage = value.Stage;
							value.Fresh = true;
							BattleWear.ApplyRestStage(wearProfile, creature, node, value, num);
							ModEntry.Log.Info($"{wearProfile.Name}: death force rest wear stage {stage}->{num} ({reason})", 1);
						}
						return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}

		private static bool TryForceMerchant(Creature creature, string reason)
		{
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Expected O, but got Unknown
			if (!SkinSelectionGate.ForTarget(creature))
			{
				return false;
			}
			try
			{
				NMerchantRoom instance = NMerchantRoom.Instance;
				if (instance == null)
				{
					return false;
				}
				IReadOnlyList<NMerchantCharacter> playerVisuals = instance.PlayerVisuals;
				List<Player> list = MerchantPlayersField?.GetValue(instance) as List<Player>;
				if (playerVisuals == null || list == null)
				{
					return false;
				}
				int num = Math.Min(playerVisuals.Count, list.Count);
				for (int i = 0; i < num; i++)
				{
					Player obj = list[i];
					if (((obj != null) ? obj.Creature : null) == creature)
					{
						NMerchantCharacter val = playerVisuals[i];
						if (val == null || !GodotObject.IsInstanceValid((GodotObject)val))
						{
							return true;
						}
						ulong instanceId = ((GodotObject)val).GetInstanceId();
						if (!BattleWear.MerchantStates.TryGetValue(instanceId, out BattleWear.MerchantWearState value))
						{
							value = new BattleWear.MerchantWearState
							{
								Node = val
							};
							BattleWear.MerchantStates[instanceId] = value;
						}
						BattleWear.WearProfile wearProfile = BattleWear.ProfileFor(list[i]);
						if (wearProfile?.Thresholds == null || wearProfile.Thresholds.Length == 0)
						{
							return true;
						}
						int num2 = WearStageRuntime.StageForDeath(creature.CurrentHp, creature.MaxHp);
						if (value.Stage != num2)
						{
							int stage = value.Stage;
							value.Fresh = true;
							BattleWear.ApplyMerchantStage(wearProfile, creature, val, value, num2);
							ModEntry.Log.Info($"{wearProfile.Name}: death force merchant wear stage {stage}->{num2} ({reason})", 1);
						}
						return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}

		internal static void TryApplyMaxStageDeathSpine(Player? player, NCreatureVisuals? visuals, string reason)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Expected O, but got Unknown
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Expected O, but got Unknown
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			if (!SkinSelectionGate.ForTarget(player))
			{
				return;
			}
			try
			{
				if (player == null || visuals == null || !GodotObject.IsInstanceValid((GodotObject)visuals) || BattleWear._fuse)
				{
					return;
				}
				BattleWear.BuildProfiles();
				BattleWear.WearProfile wearProfile = BattleWear.ProfileFor(player);
				if (wearProfile == null || !BattleWear.SkinReady(wearProfile) || wearProfile.Thresholds == null || wearProfile.Thresholds.Length == 0)
				{
					return;
				}
				int num = WearStageRuntime.StageForDeath(player.Creature.CurrentHp, player.Creature.MaxHp);
				BattleWear.WearSet combat = wearProfile.Combat;
				if (combat == null)
				{
					return;
				}
				MegaSprite spineBody = visuals.SpineBody;
				if (spineBody == null)
				{
					return;
				}
				GodotObject boundObject = ((MegaSpineBinding)spineBody).BoundObject;
				Node val = (Node)(object)((boundObject is Node) ? boundObject : null);
				if (val != null && GodotObject.IsInstanceValid((GodotObject)val))
				{
					Variant val2 = ((GodotObject)val).Get(StringName.op_Implicit("skeleton_data_res"));
					GodotObject obj = ((Variant)(ref val2)).AsGodotObject();
					Resource val3 = (Resource)(object)((obj is Resource) ? obj : null);
					if (combat.Pristine == null && val3 != null)
					{
						combat.Pristine = val3;
					}
					int num2 = num;
					Resource val4 = ((num2 == 1) ? combat.Pristine : BattleWear.GetStageData(combat, num2, wearProfile.Name));
					if (val4 == null)
					{
						ModEntry.Log.Info(wearProfile.Name + ": abandon death spine GetStageData returned null (" + reason + ")", 1);
						return;
					}
					((GodotObject)val).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)val4));
					SpineAnimCompat.SetAnimation(visuals.SpineAnimation, "die", loop: false);
					ModEntry.Log.Info($"{wearProfile.Name}: abandon death spine stage {num2} ({reason})", 1);
				}
			}
			catch (Exception ex)
			{
				try
				{
					ModEntry.Log.Error("abandon death spine failed (" + reason + "): " + ex.GetBaseException().Message, 1);
				}
				catch
				{
				}
			}
		}
	}
	[HarmonyPatch(typeof(NCreature), "StartDeathAnim")]
	[HarmonyPriority(800)]
	internal static class DeathForceMaxStagePatch
	{
		[HarmonyPrefix]
		private static bool Prefix(NCreature __instance)
		{
			if (!SkinSelectionGate.ForTarget(__instance))
			{
				return true;
			}
			try
			{
				DeathForceWear.TryForceOnCombatNode(__instance, "StartDeathAnim");
			}
			catch
			{
			}
			return true;
		}
	}
	[HarmonyPatch(typeof(CreatureCmd), "KillWithoutCheckingWinCondition")]
	[HarmonyPriority(800)]
	internal static class DeathForceOnKillPatch
	{
		[HarmonyPrefix]
		private static void Prefix(Creature creature, bool force)
		{
			if (SkinSelectionGate.ForTarget(creature))
			{
			}
		}
	}
	[HarmonyPatch(typeof(NGameOverScreen), "MoveCreaturesToDifferentLayerAndDisableUi")]
	[HarmonyPriority(0)]
	internal static class GameOverDeathSpinePatch
	{
		private static readonly FieldInfo? RunStateField = AccessTools.Field(typeof(NGameOverScreen), "_runState");

		private static readonly FieldInfo? CreatureContainerField = AccessTools.Field(typeof(NGameOverScreen), "_creatureContainer");

		[HarmonyPostfix]
		private static void Postfix(NGameOverScreen __instance)
		{
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Expected O, but got Unknown
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Expected O, but got Unknown
			if (!SkinSelectionGate.ForTarget(__instance))
			{
				return;
			}
			try
			{
				if (__instance == null)
				{
					return;
				}
				bool flag = false;
				try
				{
					flag = NCombatRoom.Instance != null;
				}
				catch
				{
				}
				if (flag)
				{
					return;
				}
				object? obj2 = RunStateField?.GetValue(__instance);
				RunState val = (RunState)((obj2 is RunState) ? obj2 : null);
				if (val == null)
				{
					return;
				}
				object? obj3 = CreatureContainerField?.GetValue(__instance);
				Node val2 = (Node)((obj3 is Node) ? obj3 : null);
				if (val2 == null || !GodotObject.IsInstanceValid((GodotObject)val2))
				{
					return;
				}
				List<NCreatureVisuals> list = new List<NCreatureVisuals>();
				foreach (Node child in val2.GetChildren(false))
				{
					NCreatureVisuals val3 = (NCreatureVisuals)(object)((child is NCreatureVisuals) ? child : null);
					if (val3 != null && GodotObject.IsInstanceValid((GodotObject)val3))
					{
						list.Add(val3);
					}
				}
				if (list.Count == 0)
				{
					return;
				}
				int num = 0;
				foreach (Player player in val.Players)
				{
					if (num < list.Count)
					{
						DeathForceWear.TryApplyMaxStageDeathSpine(player, list[num], "GameOver");
						num++;
						continue;
					}
					break;
				}
			}
			catch
			{
			}
		}
	}
	[HarmonyPatch(typeof(CreatureCmd), "KillWithoutCheckingWinCondition")]
	[HarmonyPriority(0)]
	internal static class NativeNonCombatDeathPatch
	{
		[HarmonyPostfix]
		private static void Postfix(Creature creature, ref Task __result)
		{
			if (!SkinSelectionGate.ForTarget(creature))
			{
				return;
			}
			Creature creature2 = creature;
			if (__result == null || creature2 == null || !creature2.IsPlayer)
			{
				return;
			}
			__result = NativeDeathCompletion.After(__result, () => creature2.IsDead, delegate
			{
				if (NCombatRoom.Instance == null)
				{
					DeathForceWear.TryForceForCreature(creature2, "Kill completed (non-combat)");
				}
			});
		}
	}
	internal static class NativeDeathCompletion
	{
		internal static async Task After(Task nativeDeath, Func<bool> isDead, Action applyFinalWear)
		{
			await nativeDeath;
			try
			{
				if (isDead())
				{
					applyFinalWear();
				}
			}
			catch
			{
			}
		}
	}
	public static class CardAnimDefault
	{
		public static string? Resolve(string? requested)
		{
			if (string.IsNullOrEmpty(requested))
			{
				return "Cast";
			}
			if (requested.Equals("none", StringComparison.OrdinalIgnoreCase))
			{
				return "Cast";
			}
			return requested;
		}

		public static string ResolveMissingForCard(string cardTypeName)
		{
			if (string.Equals(cardTypeName, "Omnislice", StringComparison.Ordinal))
			{
				return "Attack";
			}
			return "Cast";
		}
	}
	public static class CardPlayAnimTracker
	{
		private static readonly ConcurrentDictionary<object, bool> SawByPlay = new ConcurrentDictionary<object, bool>();

		private static object? _current;

		public static void Begin(object play)
		{
			ArgumentNullException.ThrowIfNull(play, "play");
			_current = play;
			SawByPlay[play] = false;
		}

		public static void NotePlayerAnim()
		{
			object current = _current;
			if (current != null)
			{
				SawByPlay[current] = true;
			}
		}

		public static bool NeedsCast(object? play)
		{
			if (play == null)
			{
				return false;
			}
			if (SawByPlay.TryGetValue(play, out var value))
			{
				return !value;
			}
			return false;
		}

		public static bool ShouldInjectCast(object? play)
		{
			if (play == null)
			{
				return false;
			}
			if (!SawByPlay.TryRemove(play, out var value))
			{
				return false;
			}
			if (_current == play)
			{
				_current = null;
			}
			return !value;
		}
	}
	[HarmonyPatch]
	internal static class CardPlayNoneCastPatch
	{
		private static readonly FieldInfo? VisualAttackerField = AccessTools.Field(typeof(AttackCommand), "_visualAttacker");

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Hook), "BeforeCardPlayed")]
		private static void BeforeCardPlayedPrefix(CardPlay cardPlay)
		{
			if (SkinSelectionGate.ForTarget(cardPlay) && cardPlay != null)
			{
				CardPlayAnimTracker.Begin(cardPlay);
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CreatureCmd), "TriggerAnim")]
		private static void TriggerAnimPrefix(Creature creature, ref string triggerName)
		{
			if (SkinSelectionGate.ForTarget(creature) && creature != null && creature.IsPlayer)
			{
				string text = CardAnimDefault.Resolve(triggerName);
				if (text != null && !string.Equals(text, triggerName, StringComparison.Ordinal))
				{
					triggerName = text;
				}
				CardPlayAnimTracker.NotePlayerAnim();
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AttackCommand), "Execute")]
		private static void AttackCommandExecutePrefix(AttackCommand __instance)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Expected O, but got Unknown
			if (!SkinSelectionGate.ForTarget(__instance))
			{
				return;
			}
			try
			{
				if (((__instance != null) ? __instance.Attacker : null) != null)
				{
					object obj = VisualAttackerField?.GetValue(__instance);
					Creature val = (Creature)(((obj is Creature) ? obj : null) ?? __instance.Attacker);
					if (val != null && !val.IsPlayer)
					{
						CardPlayAnimTracker.NotePlayerAnim();
					}
				}
			}
			catch
			{
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CreatureCmd), "GainBlock", new Type[]
		{
			typeof(Creature),
			typeof(decimal),
			typeof(ValueProp),
			typeof(CardPlay),
			typeof(bool)
		})]
		private static void GainBlockPrefix(Creature creature, CardPlay cardPlay)
		{
			if (!SkinSelectionGate.ForTarget(creature))
			{
				return;
			}
			try
			{
				if (((cardPlay != null) ? cardPlay.Card : null) != null && creature != null && CardPlayAnimTracker.NeedsCast(cardPlay) && ShouldHandle(cardPlay.Card))
				{
					Player owner = cardPlay.Card.Owner;
					if (creature == ((owner != null) ? owner.Creature : null))
					{
						InjectDefaultAnim(creature, cardPlay.Card, "GainBlock");
						CardPlayAnimTracker.NotePlayerAnim();
					}
				}
			}
			catch
			{
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CombatHistory), "CardPlayFinished")]
		private static void CardPlayFinishedPrefix(CardPlay cardPlay)
		{
			TryInjectAfterPlay(cardPlay, "CardPlayFinished");
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Hook), "AfterCardPlayed")]
		private static void AfterCardPlayedPrefix(ICombatState combatState, CardPlay cardPlay)
		{
			if (SkinSelectionGate.ForTarget(cardPlay))
			{
				TryInjectAfterPlay(cardPlay, "AfterCardPlayed");
			}
		}

		private static void TryInjectAfterPlay(CardPlay cardPlay, string reason)
		{
			if (!SkinSelectionGate.ForTarget(cardPlay))
			{
				return;
			}
			try
			{
				if (((cardPlay != null) ? cardPlay.Card : null) == null)
				{
					CardPlayAnimTracker.ShouldInjectCast(cardPlay);
				}
				else if (!ShouldHandle(cardPlay.Card))
				{
					CardPlayAnimTracker.ShouldInjectCast(cardPlay);
				}
				else if (CardPlayAnimTracker.ShouldInjectCast(cardPlay))
				{
					Player owner = cardPlay.Card.Owner;
					Creature val = ((owner != null) ? owner.Creature : null);
					if (val != null && !val.IsDead)
					{
						InjectDefaultAnim(val, cardPlay.Card, reason);
					}
				}
			}
			catch
			{
			}
		}

		private static void InjectDefaultAnim(Creature creature, CardModel card, [Optional] string reason)
		{
			string text = CardAnimDefault.ResolveMissingForCard(((object)card).GetType().Name);
			NCreature creatureNode = creature.GetCreatureNode();
			if (creatureNode == null)
			{
				return;
			}
			try
			{
				if (creature.IsPlayer)
				{
					if (string.Equals(text, "Attack", StringComparison.Ordinal))
					{
						SfxCmd.Play(creature.Player.Character.AttackSfx, 1f);
					}
					else
					{
						SfxCmd.Play(creature.Player.Character.CastSfx, 1f);
					}
				}
			}
			catch
			{
			}
			creatureNode.SetAnimationTrigger(text);
			try
			{
				ModEntry.Log.Info($"[none-cast] {((object)card).GetType().Name} → {text} ({reason})", 1);
			}
			catch
			{
			}
		}

		private static bool ShouldHandle(CardModel card)
		{
			if (!SkinSelectionGate.ForTarget(card))
			{
				return false;
			}
			try
			{
				Player owner = card.Owner;
				if (((owner != null) ? owner.Creature : null) == null || !owner.Creature.IsPlayer)
				{
					return false;
				}
				CharacterModel character = owner.Character;
				if (character is Ironclad)
				{
					return BattleWear.OwnsCharacter("ironclad");
				}
				if (character is Silent)
				{
					return BattleWear.OwnsCharacter("silent");
				}
				if (character is Defect)
				{
					return BattleWear.OwnsCharacter("defect");
				}
				if (character is Regent)
				{
					return BattleWear.OwnsCharacter("regent");
				}
				if (character is Necrobinder)
				{
					return BattleWear.OwnsCharacter("necrobinder");
				}
				return false;
			}
			catch
			{
				return false;
			}
		}
	}
	[HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
	internal static class RestSiteCharacterTrackPatch
	{
		[HarmonyPostfix]
		private static void Postfix(NRestSiteCharacter __instance)
		{
			try
			{
				if (__instance != null && !BattleWear.RestChars.Contains(__instance))
				{
					BattleWear.RestChars.Add(__instance);
					BattleWear.UpdateRestCharacter(__instance);
				}
			}
			catch
			{
			}
		}
	}
	[HarmonyPatch(typeof(NMerchantRoom), "AfterRoomIsLoaded")]
	internal static class MerchantRoomInitialWearPatch
	{
		[HarmonyPostfix]
		private static void Postfix(NMerchantRoom __instance)
		{
			try
			{
				BattleWear.RefreshMerchantRoom(__instance);
			}
			catch (Exception ex)
			{
				ModEntry.Log.Warn("merchant initial wear: " + ex.Message, 1);
			}
		}
	}
	[ModInitializer("Initialize")]
	public static class ModEntry
	{
		public static readonly string ModId;

		public static readonly Logger Log;

		private static bool _hookNeutralized;

		public static void Initialize()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			try
			{
				Harmony val = new Harmony(ModId);
				int num = 0;
				int num2 = 0;
				Type[] types = Assembly.GetExecutingAssembly().GetTypes();
				foreach (Type type in types)
				{
					if (!(type == typeof(PatchBootstrap)) && !(type == typeof(SuppressLowHpAnimPatch)) && !(type == typeof(LowHpAnimRedirectPatch)) && type.GetCustomAttributes(typeof(HarmonyPatch), inherit: false).Length != 0)
					{
						try
						{
							val.CreateClassProcessor(type).Patch();
							num++;
						}
						catch (Exception ex)
						{
							num2++;
							Log.Error($"[{ModId}] patch failed for {type.Name}: {ex.Message}", 1);
						}
					}
				}
				int num3 = 0;
				try
				{
					num3 = PatchBootstrap.ApplyOptional(val);
				}
				catch (Exception ex2)
				{
					Log.Error("[" + ModId + "] optional patches failed: " + ex2.Message, 1);
				}
				try
				{
					num3 += LowWearAttackGate.ApplyHarmony(val);
				}
				catch (Exception ex3)
				{
					Log.Error("[" + ModId + "] AttackGate patches failed: " + ex3.Message, 1);
				}
				TryRegisterWearStageSettings();
				Log.Info($"[{ModId}] initialized — battle-wear driver armed; patches {num} ok, {num2} failed, {num3} optional; skins: {BattleWear.DescribeSkins()}.", 1);
			}
			catch (Exception value)
			{
				Log.Error($"[{ModId}] initialize failed: {value}", 1);
			}
		}

		private static void TryRegisterWearStageSettings()
		{
			if (!string.Equals(ModId, "succubusIroncladHpWear", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			try
			{
				WearStageSettingsRitsu.TryRegister();
			}
			catch (Exception ex)
			{
				Log.Error("[" + ModId + "] RitsuLib wear-stage settings page FAILED to register — 设置界面不会出现: " + ex.GetBaseException().Message, 1);
			}
		}

		internal static void TryNeutralizeMesugakiGlobalTouchHook()
		{
		}

		private static int TryPatchVoid(Harmony harmony, MethodInfo m, HarmonyMethod prefix)
		{
			if (m.IsAbstract || m.ContainsGenericParameters || m.ReturnType != typeof(void) || m.IsSpecialName)
			{
				return 0;
			}
			try
			{
				harmony.Patch((MethodBase)m, prefix, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				return 1;
			}
			catch
			{
				return 0;
			}
		}

		public static bool SkipGlobalTouchHookMethod()
		{
			return false;
		}

		static ModEntry()
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Expected O, but got Unknown
			ModId = Assembly.GetExecutingAssembly().GetName().Name ?? "BattleWear";
			Log = new Logger(ModId, (LogType)0);
		}
	}
	public sealed class WearStageSettings
	{
		public const int AssumedMaxHpIronclad = 80;

		public const int DeepestStage = 2;

		private int _lowHealthIdlePercent;

		public bool WearEnabled { get; set; } = true;

		public bool UseAbsoluteHp { get; set; }

		public int PercentThreshold { get; set; } = 50;

		public int AbsoluteThreshold { get; set; } = 40;

		public bool ForceDeepestOnDeath { get; set; } = true;

		public string SafetyHotkey { get; set; } = "F6";

		public double VisualScale { get; set; } = 1.0;

		public int LowHealthIdlePercent
		{
			get
			{
				return _lowHealthIdlePercent;
			}
			set
			{
				_lowHealthIdlePercent = LowHealthIdleRules.Normalize(value);
			}
		}

		public static int AssumedMaxHpForCurrentMod()
		{
			return 80;
		}

		public static WearStageSettings CreateDefault()
		{
			WearStageSettings wearStageSettings = new WearStageSettings();
			wearStageSettings.Normalize();
			return wearStageSettings;
		}

		public void ResetToDefault()
		{
			WearEnabled = true;
			UseAbsoluteHp = false;
			PercentThreshold = 50;
			AbsoluteThreshold = 40;
			ForceDeepestOnDeath = true;
			SafetyHotkey = "F6";
			VisualScale = 1.0;
			LowHealthIdlePercent = 25;
		}

		public void Normalize()
		{
			if (PercentThreshold < 0 || PercentThreshold > 100)
			{
				PercentThreshold = 50;
			}
			if (AbsoluteThreshold < 0 || AbsoluteThreshold > 999)
			{
				AbsoluteThreshold = 40;
			}
			if (!double.IsFinite(VisualScale) || VisualScale < 0.5 || VisualScale > 2.0)
			{
				VisualScale = 1.0;
			}
			if (string.IsNullOrWhiteSpace(SafetyHotkey))
			{
				SafetyHotkey = "F6";
			}
		}

		public bool TryValidatePercent(out string error)
		{
			if (PercentThreshold < 0 || PercentThreshold > 100)
			{
				error = $"百分比分界非法：需 0≤T≤100，当前 T={PercentThreshold}";
				return false;
			}
			error = "";
			return true;
		}

		public bool TryValidateAbsolute(out string error)
		{
			if (AbsoluteThreshold < 0 || AbsoluteThreshold > 999)
			{
				error = $"绝对血分界非法：需 0≤T≤999，当前 T={AbsoluteThreshold}";
				return false;
			}
			error = "";
			return true;
		}

		public string FormatPreviewPercent()
		{
			return $"百分比分界 {PercentThreshold}% → (> {PercentThreshold}% 第1档, ≤ {PercentThreshold}% 第2档)；0=存活恒第1档，100=恒第2档";
		}

		public string FormatPreviewAbsolute()
		{
			return $"绝对血分界 {AbsoluteThreshold} → (> {AbsoluteThreshold} 第1档, ≤ {AbsoluteThreshold} 第2档)";
		}

		public WearStageSettings Clone()
		{
			return new WearStageSettings
			{
				WearEnabled = WearEnabled,
				UseAbsoluteHp = UseAbsoluteHp,
				PercentThreshold = PercentThreshold,
				AbsoluteThreshold = AbsoluteThreshold,
				ForceDeepestOnDeath = ForceDeepestOnDeath,
				SafetyHotkey = SafetyHotkey,
				VisualScale = VisualScale,
				LowHealthIdlePercent = LowHealthIdlePercent
			};
		}

		public WearStageSettings()
		{
			LowHealthIdlePercent = 25;
		}
	}
	internal static class WearStageSettingsRitsu
	{
		internal static Action<bool>? PersistEnabled;

		internal static string LastStatus = "not initialized";

		private static bool _registered;

		private static bool _watching;

		private static bool _retryQueued;

		private static bool _registering;

		private static Assembly? _bridge;

		internal static Action<Action>? ScheduleRetry;

		private static readonly object RetryLock = new object();

		internal static bool ToggleEnabled()
		{
			WearStageSettings live = WearStageRuntime.GetLive();
			bool result = (live.WearEnabled = !live.WearEnabled);
			if (WearStageRuntime.HasProvider)
			{
				WearStageRuntime.InvalidateLiveCache();
			}
			WearStageRuntime.ClearCombatSnapshot();
			return result;
		}

		private static bool Compatible(AssemblyName expected, AssemblyName actual)
		{
			if (expected.Name == actual.Name && actual.Version != null && expected.Version != null && actual.Version.Major == expected.Version.Major && actual.Version.Minor == expected.Version.Minor)
			{
				return actual.Version >= expected.Version;
			}
			return false;
		}

		private static Assembly? ResolveLoaded(AssemblyLoadContext context, AssemblyName name)
		{
			if (name.Name == typeof(WearStageSettingsRitsu).Assembly.GetName().Name)
			{
				return typeof(WearStageSettingsRitsu).Assembly;
			}
			string? name2 = name.Name;
			if (name2 == null || !name2.StartsWith("STS2-RitsuLib", StringComparison.Ordinal))
			{
				return null;
			}
			return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => Compatible(name, a.GetName()));
		}

		internal static Assembly LoadCompatibleBridge()
		{
			if (_bridge != null)
			{
				return _bridge;
			}
			Assembly assembly = typeof(WearStageSettingsRitsu).Assembly;
			AssemblyLoadContext loadContext = AssemblyLoadContext.GetLoadContext(assembly);
			using Stream stream = assembly.GetManifestResourceStream("BattleWear.RitsuDependencies") ?? throw new InvalidOperationException("Settings dependency manifest missing");
			using StreamReader streamReader = new StreamReader(stream);
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					AssemblyName assemblyName = new AssemblyName(text);
					Assembly assembly2 = ResolveLoaded(loadContext, assemblyName) ?? loadContext.LoadFromAssemblyName(new AssemblyName(assemblyName.Name));
					if (!Compatible(assemblyName, assembly2.GetName()))
					{
						throw new InvalidOperationException("Settings require compatible " + assemblyName.FullName + "; found " + assembly2.FullName);
					}
				}
			}
			loadContext.Resolving -= ResolveLoaded;
			loadContext.Resolving += ResolveLoaded;
			using Stream assembly3 = assembly.GetManifestResourceStream("BattleWear.RitsuBridge") ?? throw new InvalidOperationException("Settings bridge missing");
			return _bridge = loadContext.LoadFromStream(assembly3);
		}

		private static void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
		{
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			string? name = args.LoadedAssembly.GetName().Name;
			if (name == null || !name.StartsWith("STS2-RitsuLib", StringComparison.Ordinal))
			{
				return;
			}
			lock (RetryLock)
			{
				if (_registered || _retryQueued)
				{
					return;
				}
				_retryQueued = true;
			}
			try
			{
				if (ScheduleRetry != null)
				{
					ScheduleRetry(RetryDeferred);
					return;
				}
				Callable val = Callable.From((Action)RetryDeferred);
				((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			}
			catch
			{
				lock (RetryLock)
				{
					_retryQueued = false;
				}
			}
		}

		private static void RetryDeferred()
		{
			lock (RetryLock)
			{
				_retryQueued = false;
			}
			TryRegister();
		}

		internal static void TryRegister()
		{
			if (_registered || _registering)
			{
				return;
			}
			if (!_watching)
			{
				AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
				_watching = true;
			}
			_registering = true;
			try
			{
				LoadCompatibleBridge().GetType("BattleWear.RitsuSettingsBridge", throwOnError: true).GetMethod("TryRegister", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
				_registered = true;
				LastStatus = "registered";
				AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
				_watching = false;
			}
			catch (Exception ex)
			{
				PersistEnabled = null;
				if (WearStageRuntime.HasProvider)
				{
					WearStageRuntime.SetProvider(null);
				}
				WearStageRuntime.ClearCombatSnapshot();
				LastStatus = "settings skipped; default HP wear active: " + ex.GetBaseException().Message;
				try
				{
					ModEntry.Log.Info("[optional-ritsu] " + LastStatus, 1);
				}
				catch
				{
				}
			}
			finally
			{
				_registering = false;
			}
		}
	}
	internal static class WearSafetyHotkey
	{
		private const string DataKey = "wear_stage_settings";

		internal const double PollIntervalSec = 0.03;

		private static bool _wasDown;

		private static bool _busy;

		internal static void EnsurePollerAttached(Node? host, string reason = "safety")
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Expected O, but got Unknown
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Expected O, but got Unknown
			if (host != null && GodotObject.IsInstanceValid((GodotObject)host))
			{
				string text = "WearSafetyPoller_" + ModEntry.ModId;
				if (!host.HasNode(NodePath.op_Implicit(text)))
				{
					Timer val = new Timer
					{
						Name = StringName.op_Implicit(text),
						WaitTime = 0.03,
						OneShot = false,
						Autostart = true,
						ProcessCallback = (TimerProcessCallback)1,
						ProcessMode = (ProcessModeEnum)3
					};
					val.Timeout += Poll;
					host.AddChild((Node)val, false, (InternalMode)0);
					val.Start(-1.0);
					ModEntry.Log.Info($"wear safety poller attached ({reason}, {0.03:0.##}s).", 1);
				}
			}
		}

		internal static void EnsurePollerFromTree(string reason = "tree")
		{
			try
			{
				MainLoop mainLoop = Engine.GetMainLoop();
				SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
				if (val != null && val.Root != null)
				{
					EnsurePollerAttached((Node?)(object)val.Root, reason);
				}
			}
			catch
			{
			}
		}

		internal static void Poll()
		{
			if (!SkinSelectionGate.MenuAllowed())
			{
				return;
			}
			if (!CharSelectSafety.CanPoll())
			{
				_wasDown = true;
			}
			else
			{
				if (_busy)
				{
					return;
				}
				try
				{
					WearStageSettings live = WearStageRuntime.GetLive();
					if (!TryIsBindingDown(string.IsNullOrWhiteSpace(live.SafetyHotkey) ? "F6" : live.SafetyHotkey.Trim(), out var down))
					{
						_wasDown = false;
						return;
					}
					if (down && !_wasDown)
					{
						_wasDown = true;
						_busy = true;
						try
						{
							ToggleWearEnabled();
							return;
						}
						finally
						{
							_busy = false;
						}
					}
					_wasDown = down;
				}
				catch (Exception ex)
				{
					_busy = false;
					ModEntry.Log.Warn("[wear-safety] poll failed: " + ex.Message, 1);
				}
			}
		}

		private static void ToggleWearEnabled()
		{
			if (SkinSelectionGate.MenuAllowed())
			{
				bool flag = WearStageSettingsRitsu.ToggleEnabled();
				BattleWear.ForceReapplyWear(preferCrossfade: true);
				CharSelectSafety.ResetIfDisabled(flag);
				ModEntry.Log.Info("[wear-safety] 换装 → " + (flag ? "开（血量分级，渐变）" : "关（恒第1档底皮，渐变）"), 1);
				PersistWearEnabledDeferred(flag);
			}
		}

		private static void PersistWearEnabledDeferred(bool enabled)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Callable val = Callable.From((Action)Save);
				((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			}
			catch
			{
				Save();
			}
			void Save()
			{
				if (!WearStageRuntime.HasProvider)
				{
					return;
				}
				try
				{
					WearStageSettingsRitsu.PersistEnabled?.Invoke(enabled);
				}
				catch (Exception ex)
				{
					ModEntry.Log.Warn("[wear-safety] save deferred failed: " + ex.Message, 1);
				}
			}
		}

		private static bool TryIsBindingDown(string binding, out bool down)
		{
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Unknown result type (might be due to invalid IL or missing references)
			down = false;
			string[] array = binding.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			if (array.Length == 0)
			{
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			Key? val = null;
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text = array2[i].Trim();
				string text2 = text.ToLowerInvariant();
				bool flag5;
				switch (text2)
				{
				case "ctrl":
				case "control":
					flag = true;
					continue;
				default:
					switch (text2.Length)
					{
					case 3:
					{
						char c = text2[0];
						if (c == 'a')
						{
							goto IL_00d2;
						}
						if (c != 'c')
						{
							if (c != 'w' || !(text2 == "win"))
							{
								break;
							}
						}
						else if (!(text2 == "cmd"))
						{
							break;
						}
						goto IL_014a;
					}
					case 5:
					{
						char c = text2[1];
						if (c == 'h')
						{
							goto IL_0102;
						}
						if (c != 'u' || !(text2 == "super"))
						{
							break;
						}
						goto IL_014a;
					}
					case 4:
						if (!(text2 == "meta"))
						{
							break;
						}
						goto IL_014a;
					case 7:
						{
							if (!(text2 == "command"))
							{
								break;
							}
							goto IL_014a;
						}
						IL_014a:
						flag5 = true;
						goto end_IL_0050;
					}
					goto case null;
				case null:
					{
						flag5 = false;
						break;
					}
					IL_0102:
					if (!(text2 == "shift"))
					{
						goto case null;
					}
					flag3 = true;
					continue;
					IL_00d2:
					if (!(text2 == "alt"))
					{
						goto case null;
					}
					flag2 = true;
					continue;
					end_IL_0050:
					break;
				}
				Key result;
				if (flag5)
				{
					flag4 = true;
				}
				else if (Enum.TryParse<Key>(text, ignoreCase: true, out result))
				{
					val = result;
				}
			}
			if (!val.HasValue)
			{
				return false;
			}
			if (!IsKeyDown(val.Value))
			{
				down = false;
				return true;
			}
			if (flag && !IsKeyDown((Key)4194326))
			{
				down = false;
				return true;
			}
			if (flag2 && !IsKeyDown((Key)4194328))
			{
				down = false;
				return true;
			}
			if (flag3 && !IsKeyDown((Key)4194325))
			{
				down = false;
				return true;
			}
			if (flag4 && !IsKeyDown((Key)4194327))
			{
				down = false;
				return true;
			}
			down = true;
			return true;
		}

		private static bool IsKeyDown(Key key)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			if (Input.IsKeyPressed(key))
			{
				return true;
			}
			try
			{
				return Input.IsPhysicalKeyPressed(key);
			}
			catch
			{
				return false;
			}
		}
	}
	internal static class LowHealthIdleRules
	{
		internal static int Normalize(int value)
		{
			return (value >= 0 && value <= 100) ? value : 25;
		}

		internal static bool Matches(int hp, int maxHp, bool dead)
		{
			if (ModEntry.ModId == "ironcladHpWear")
			{
				return false;
			}
			int lowHealthIdlePercent = WearStageRuntime.GetLive().LowHealthIdlePercent;
			return !dead && hp > 0 && maxHp > 0 && lowHealthIdlePercent > 0 && (long)hp * 100L <= (long)maxHp * (long)lowHealthIdlePercent;
		}

		internal static bool MatchesCreature(Creature creature)
		{
			if (!SkinSelectionGate.ForTarget(creature))
			{
				return false;
			}
			return Matches(creature.CurrentHp, creature.MaxHp, creature.IsDead);
		}
	}
	internal static class CharSelectSafety
	{
		private static readonly ConditionalWeakTable<Control, object> Bindings = new ConditionalWeakTable<Control, object>();

		private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		internal static void Register(Control bg, object state)
		{
			if (bg != null && state != null && GodotObject.IsInstanceValid((GodotObject)(object)bg))
			{
				Bindings.Remove(bg);
				Bindings.Add(bg, state);
				WearSafetyHotkey.EnsurePollerAttached((Node?)(object)bg, "character-select");
			}
		}

		internal static void ResetIfDisabled(bool enabled)
		{
			if (enabled)
			{
				return;
			}
			foreach (KeyValuePair<Control, object> item in (IEnumerable<KeyValuePair<Control, object>>)Bindings)
			{
				Control key = item.Key;
				if (GodotObject.IsInstanceValid((GodotObject)(object)key) && ((Node)key).IsInsideTree() && ((CanvasItem)key).IsVisibleInTree())
				{
					try
					{
						Reset(key, item.Value);
					}
					catch (Exception ex)
					{
						GD.PushWarning("[wear-safety] character-select reset failed: " + ex.GetBaseException().Message);
					}
				}
			}
		}

		private static void Set(object state, string field, object value)
		{
			state.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(state, value);
		}

		private static void Reset(Control bg, object state)
		{
			Assembly assembly = typeof(CharSelectSafety).Assembly;
			Type type = assembly.GetType("BattleWear.CharSelectCore");
			if (type != null)
			{
				object? obj = type.GetMethod("ResolveSpine", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, new object[1] { bg });
				Node val = (Node)((obj is Node) ? obj : null);
				if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					return;
				}
				MethodInfo method = type.GetMethod("ApplyStage", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (!(method == null))
				{
					object obj2 = method.Invoke(null, new object[5] { bg, val, state, 1, true });
					if (obj2 is bool && (bool)obj2)
					{
						Set(state, "Stage", 1);
						Set(state, "WasPressed", true);
						Set(state, "IgnoreClickUntilRelease", true);
						Set(state, "KaguyaMidHalf179", false);
						Set(state, "KaguyaSkipHalf18ToBare179", false);
					}
				}
				return;
			}
			MethodInfo[] methods = state.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.Name.Contains("g__Apply", StringComparison.Ordinal) && methodInfo.GetParameters().Length == 1)
				{
					methodInfo.Invoke(state, new object[1] { 1 });
					break;
				}
			}
		}

		internal static bool CanPoll()
		{
			if (!SkinSelectionGate.MenuAllowed())
			{
				return false;
			}
			if (IsSelectionOpen())
			{
				foreach (KeyValuePair<Control, object> item in (IEnumerable<KeyValuePair<Control, object>>)Bindings)
				{
					Control key = item.Key;
					if (GodotObject.IsInstanceValid((GodotObject)(object)key) && ((Node)key).IsInsideTree() && !((GodotObject)key).IsQueuedForDeletion() && ((CanvasItem)key).IsVisibleInTree())
					{
						return true;
					}
				}
				return false;
			}
			NGame instance = NGame.Instance;
			return ((instance != null) ? instance.MainMenu : null) == null;
		}

		internal static bool IsSelectionOpen()
		{
			NGame instance = NGame.Instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				NMainMenu mainMenu = instance.MainMenu;
				if (mainMenu == null)
				{
					obj = null;
				}
				else
				{
					NMainMenuSubmenuStack submenuStack = mainMenu.SubmenuStack;
					obj = ((submenuStack != null) ? ((NSubmenuStack)submenuStack).Peek() : null);
				}
			}
			return obj is NCharacterSelectScreen;
		}
	}
	internal static class SkinSelectionGate
	{
		private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		private static readonly Assembly Self;

		private static Type _service;

		private static MethodInfo _selected;

		private static MethodInfo _direct;

		private static MethodInfo _playerScope;

		private static bool _managerPresent;

		private static long _searchAfter;

		private static bool? _last;

		private static bool _cleaning;

		static SkinSelectionGate()
		{
			Self = typeof(SkinSelectionGate).Assembly;
			AppDomain.CurrentDomain.AssemblyLoad += AssemblyChanged;
		}

		private static void AssemblyChanged(object sender, AssemblyLoadEventArgs args)
		{
			_searchAfter = 0L;
		}

		internal static string Group()
		{
			string text = Self.GetName().Name ?? "";
			return text.StartsWith("defect", StringComparison.OrdinalIgnoreCase) ? "defect" : (text.StartsWith("regent", StringComparison.OrdinalIgnoreCase) ? "regent" : (text.StartsWith("necro", StringComparison.OrdinalIgnoreCase) ? "necrobinder" : (text.StartsWith("silent", StringComparison.OrdinalIgnoreCase) ? "silent" : "ironclad")));
		}

		internal static bool HasManager()
		{
			if (_service != null)
			{
				return true;
			}
			if (Environment.TickCount64 < _searchAfter)
			{
				return _managerPresent;
			}
			_searchAfter = Environment.TickCount64 + 1000;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type type = assembly.GetType("STS2SkinChanger.Core.SkinService", throwOnError: false);
				if (type == null)
				{
					if (assembly.GetName().Name == "Gurio.SkinChanger" || assembly.GetName().Name == "STS2SkinChanger")
					{
						_managerPresent = true;
					}
					continue;
				}
				_managerPresent = true;
				_service = type;
				_selected = type.GetMethod("GetSelectedRuntimeProvider", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(string) }, null);
				_direct = type.GetMethod("IsDirectCharacterRuntimeProviderSelected", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(string),
					typeof(string)
				}, null);
				_playerScope = assembly.GetType("STS2SkinChanger.Core.MultiplayerSkinSync", throwOnError: false)?.GetMethod("BeginPlayerSelectionScope", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(ulong) }, null);
				break;
			}
			return _managerPresent;
		}

		private static bool Query(bool player, ulong id)
		{
			if (!HasManager())
			{
				return true;
			}
			if (_selected == null || (player && _playerScope == null))
			{
				return false;
			}
			try
			{
				using (player ? (_playerScope.Invoke(null, new object[1] { id }) as IDisposable) : null)
				{
					string text = Group();
					string text2 = _selected.Invoke(null, new object[1] { text }) as string;
					string name = Self.GetName().Name;
					if (string.IsNullOrEmpty(text2) || (!text2.Equals(name, StringComparison.OrdinalIgnoreCase) && !text2.StartsWith(name + "::source:", StringComparison.OrdinalIgnoreCase)))
					{
						return false;
					}
					return true;
				}
			}
			catch
			{
				return false;
			}
		}

		internal static bool MenuAllowed()
		{
			return Query(player: false, 0uL);
		}

		internal static bool AllowPlayer(ulong id)
		{
			return Query(player: true, id);
		}

		private static object Member(object value, string name)
		{
			try
			{
				return value.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(value) ?? value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(value);
			}
			catch
			{
				return null;
			}
		}

		private static IEnumerable Players()
		{
			try
			{
				Type type = typeof(Player).Assembly.GetType("MegaCrit.Sts2.Core.Runs.RunManager");
				object obj = type?.GetProperty("Instance", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
				object obj2 = type?.GetMethod("DebugOnlyGetState", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(obj, null);
				return (obj2 == null) ? null : (Member(obj2, "Players") as IEnumerable);
			}
			catch
			{
				return null;
			}
		}

		internal static bool SceneAllowed()
		{
			if (MenuAllowed())
			{
				return true;
			}
			IEnumerable enumerable = Players();
			if (enumerable == null)
			{
				return false;
			}
			foreach (object item in enumerable)
			{
				Player val = (Player)((item is Player) ? item : null);
				if (val != null && val.Character != null && ((object)val.Character).GetType().Name.Equals(Group(), StringComparison.OrdinalIgnoreCase) && AllowPlayer(val.NetId))
				{
					return true;
				}
			}
			return false;
		}

		internal static bool ForTarget(object target)
		{
			if (!HasManager())
			{
				return true;
			}
			try
			{
				int num = 0;
				while (target != null && num < 5)
				{
					Player val = (Player)((target is Player) ? target : null);
					if (val != null)
					{
						return AllowPlayer(val.NetId);
					}
					Creature val2 = (Creature)((target is Creature) ? target : null);
					if (val2 != null)
					{
						return (val2.Player != null) ? AllowPlayer(val2.Player.NetId) : SceneAllowed();
					}
					CardModel val3 = (CardModel)((target is CardModel) ? target : null);
					if (val3 != null)
					{
						return val3.Owner != null && AllowPlayer(val3.Owner.NetId);
					}
					MegaSpineBinding val4 = (MegaSpineBinding)((target is MegaSpineBinding) ? target : null);
					if (val4 != null)
					{
						target = val4.BoundObject;
					}
					else
					{
						Node val5 = (Node)((target is Node) ? target : null);
						if (val5 != null)
						{
							if (!GodotObject.IsInstanceValid((GodotObject)(object)val5) || ((GodotObject)val5).IsQueuedForDeletion())
							{
								return false;
							}
							bool flag = false;
							int num2 = 0;
							while (val5 != null && num2 < 14)
							{
								NCreature val6 = (NCreature)(object)((val5 is NCreature) ? val5 : null);
								if (val6 != null)
								{
									Creature entity = val6.Entity;
									return ((entity != null) ? entity.Player : null) != null && AllowPlayer(val6.Entity.Player.NetId);
								}
								if (((object)val5).GetType().Name.Contains("CharacterSelect", StringComparison.Ordinal))
								{
									flag = true;
								}
								num2++;
								val5 = val5.GetParent();
							}
							return flag ? MenuAllowed() : SceneAllowed();
						}
						object obj = Member(target, "Card") ?? Member(target, "Attacker") ?? Member(target, "Caster") ?? Member(target, "Owner") ?? Member(target, "Player") ?? Member(target, "Creature") ?? Member(target, "Body") ?? Member(target, "Node") ?? Member(target, "body") ?? Member(target, "bg");
						if (obj == target)
						{
							break;
						}
						target = obj;
					}
					num++;
				}
			}
			catch
			{
				return false;
			}
			return SceneAllowed();
		}

		private static void InvokeOptional(string typeName, string method, [Optional] object[] args)
		{
			try
			{
				Self.GetType(typeName)?.GetMethod(method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, args);
			}
			catch
			{
			}
		}

		private static void ResetState(bool active)
		{
			if (_cleaning)
			{
				return;
			}
			_cleaning = true;
			try
			{
				InvokeOptional("BattleWear.WearStageCrossfade", "CancelAll");
				InvokeOptional("BattleWear.WearStageRuntime", "ClearCombatSnapshot");
				Type type = Self.GetType("BattleWear.CardPlayAnimTracker");
				object obj = type?.GetField("SawByPlay", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
				obj?.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null)?.Invoke(obj, null);
				type?.GetField("_current", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(null, null);
				object obj2 = Self.GetType("BattleWear.FearAttackOnlyPatch")?.GetField("FearPlayActive", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
				obj2?.GetType().GetProperty("Value")?.SetValue(obj2, false);
				InvokeOptional("BattleWear.LowWearAttackGate", "Reset", new object[1] { "SkinChanger selection changed" });
				Type type2 = Self.GetType("BattleWear.BattleWear");
				MethodInfo methodInfo = type2?.GetMethod("OnActivationChanged", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (methodInfo != null)
				{
					methodInfo.Invoke(null, new object[1] { active });
				}
				else if (type2 != null)
				{
					string[] array = new string[4] { "CombatStates", "RestStates", "MerchantStates", "RestChars" };
					foreach (string name in array)
					{
						object obj3 = type2.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
						if (obj3 is IDictionary dictionary)
						{
							dictionary.Clear();
						}
						else if (obj3 is IList list)
						{
							list.Clear();
						}
					}
					if (type2.GetField("Profiles", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) is IEnumerable enumerable)
					{
						foreach (object item in enumerable)
						{
							item.GetType().GetField("SkinChecked", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(item, false);
							string[] array2 = new string[3] { "Combat", "Rest", "Merchant" };
							foreach (string name2 in array2)
							{
								object obj4 = Member(item, name2);
								if (obj4 != null)
								{
									obj4.GetType().GetField("Pristine", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(obj4, null);
									if (Member(obj4, "Stage") is Array array3)
									{
										Array.Clear(array3, 0, array3.Length);
									}
								}
							}
						}
					}
					type2.GetField("_fuse", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(null, false);
				}
				Type type3 = Self.GetType("BattleWear.LowHealthActionFace");
				if (!(type3?.GetField("Owners", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) is IEnumerable enumerable2))
				{
					return;
				}
				foreach (object item2 in enumerable2)
				{
					object obj5 = Member(item2, "Key");
					GodotObject val = (GodotObject)((obj5 is GodotObject) ? obj5 : null);
					object obj6 = Member(item2, "Value");
					if (val == null || obj6 == null)
					{
						continue;
					}
					try
					{
						if (GodotObject.IsInstanceValid(val))
						{
							type3.GetMethod("BeforeApply", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, new object[1] { val });
						}
					}
					catch
					{
					}
					try
					{
						obj6.GetType().GetMethod("Invalidated", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(obj6, null);
					}
					catch
					{
					}
				}
			}
			finally
			{
				_cleaning = false;
			}
		}

		internal static bool BeforeTick()
		{
			bool flag = SceneAllowed();
			if (HasManager() && _last != flag)
			{
				_last = flag;
				ResetState(flag);
			}
			return flag;
		}

		internal static bool BeforeAdvance()
		{
			if (!BeforeTick())
			{
				return false;
			}
			if (!HasManager())
			{
				return true;
			}
			try
			{
				Type type = Self.GetType("BattleWear.WearStageCrossfade");
				if (type?.GetField("Active", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) is IDictionary dictionary)
				{
					List<object> list = new List<object>();
					foreach (DictionaryEntry item in dictionary)
					{
						object obj = ((item.Value == null) ? null : Member(item.Value, "Body"));
						if (obj != null && !ForTarget(obj))
						{
							list.Add(obj);
						}
					}
					foreach (object item2 in list)
					{
						type.GetMethod("Cancel", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, new object[1] { item2 });
					}
				}
			}
			catch
			{
			}
			return true;
		}
	}
}
