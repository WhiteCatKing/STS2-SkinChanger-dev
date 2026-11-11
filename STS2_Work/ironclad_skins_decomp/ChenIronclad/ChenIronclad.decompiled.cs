using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using BaseLib.Config;
using ChenIronclad;
using ChenIronclad.ChenIroncladCode;
using ChenIronclad.ChenIroncladCode.core;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using thunninoiSkinManager.thunninoiSkinManagerCode;
using thunninoiSkinManager.thunninoiSkinManagerCode.Patches;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETCoreApp,Version=v9.0", FrameworkDisplayName = ".NET 9.0")]
[assembly: AssemblyCompany("ChenIronclad")]
[assembly: AssemblyConfiguration("ExportRelease")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]
[assembly: AssemblyProduct("ChenIronclad")]
[assembly: AssemblyTitle("ChenIronclad")]
[assembly: AssemblyHasScripts(new Type[]
{
	typeof(ChenCardTrail),
	typeof(ChenCardTrailVfx),
	typeof(ChenCreatureVisuals),
	typeof(ChenMerchantCharacter),
	typeof(ChenRestSiteCharacter),
	typeof(ChenSelectionReticle),
	typeof(ChenIroncladInit)
})]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("1.0.0.0")]
[module: UnverifiableCode]
[module: RefSafetyRules(11)]
namespace GodotPlugins.Game
{
	internal static class Main
	{
		[UnmanagedCallersOnly(EntryPoint = "godotsharp_game_main_init")]
		private static godot_bool InitializeFromGameProject(nint godotDllHandle, nint outManagedCallbacks, nint unmanagedCallbacks, int unmanagedCallbacksSize)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				DllImportResolver resolver = new GodotDllImportResolver((IntPtr)godotDllHandle).OnResolveDllImport;
				NativeLibrary.SetDllImportResolver(typeof(GodotObject).Assembly, resolver);
				NativeFuncs.Initialize((IntPtr)unmanagedCallbacks, unmanagedCallbacksSize);
				ManagedCallbacks.Create((IntPtr)outManagedCallbacks);
				ScriptManagerBridge.LookupScriptsInAssembly(typeof(Main).Assembly);
				return (godot_bool)1;
			}
			catch (Exception value)
			{
				Console.Error.WriteLine(value);
				return GodotBoolExtensions.ToGodotBool(false);
			}
		}
	}
}
namespace ChenIronclad
{
	[ModInitializer("Initialize")]
	[ScriptPath("res://ChenIroncladInit.cs")]
	public class ChenIroncladInit : Node
	{
		public class MethodName : MethodName
		{
			public static readonly StringName Initialize = StringName.op_Implicit("Initialize");
		}

		public class PropertyName : PropertyName
		{
		}

		public class SignalName : SignalName
		{
		}

		public const string ModId = "thunninoi.chenIronclad";

		public static Logger Logger { get; } = new Logger("thunninoi.chenIronclad", (LogType)0);

		public static void Initialize()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			Harmony val = new Harmony("thunninoi.chenIronclad");
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			ScriptManagerBridge.LookupScriptsInAssembly(executingAssembly);
			ModConfigRegistry.Register("thunninoi.chenIronclad", (ModConfig)(object)new ChenConfig());
			val.PatchAll(executingAssembly);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static List<MethodInfo> GetGodotMethodList()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			return new List<MethodInfo>(1)
			{
				new MethodInfo(MethodName.Initialize, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, (List<PropertyInfo>)null, (List<Variant>)null)
			};
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			if ((ref method) == MethodName.Initialize && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				Initialize();
				ret = default(godot_variant);
				return true;
			}
			return ((Node)this).InvokeGodotClassMethod(ref method, args, ref ret);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			if ((ref method) == MethodName.Initialize && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				Initialize();
				ret = default(godot_variant);
				return true;
			}
			ret = default(godot_variant);
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool HasGodotClassMethod(in godot_string_name method)
		{
			if ((ref method) == MethodName.Initialize)
			{
				return true;
			}
			return ((Node)this).HasGodotClassMethod(ref method);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void SaveGodotObjectData(GodotSerializationInfo info)
		{
			((GodotObject)this).SaveGodotObjectData(info);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			((GodotObject)this).RestoreGodotObjectData(info);
		}
	}
}
namespace ChenIronclad.ChenIroncladCode
{
	[ConfigHoverTipsByDefault]
	internal class ChenConfig : SimpleModConfig
	{
		public static bool UseChenCardFrame { get; set; } = true;

		public static bool UseChenEnergy { get; set; } = true;

		public static bool UseChenMultArm { get; set; } = true;

		public static bool UseChixiaoHellraiser { get; set; } = true;

		public static bool DebugMode { get; set; } = false;
	}
	public class ChenUtils
	{
		public static bool IsIronclad(CharacterModel instance)
		{
			ModelId id = ((AbstractModel)instance).Id;
			string entry = id.Entry;
			if (ChenConfig.DebugMode)
			{
				Log.Info("[CHEN] Utils has been called to check " + id.Entry + " - " + entry, 2);
			}
			if (string.IsNullOrWhiteSpace(entry))
			{
				return false;
			}
			if (!string.Equals(entry, "ironclad", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			return true;
		}
	}
	[HarmonyPatch(typeof(SkinRegistry), "SkinDbSetup")]
	public class SkinRegister
	{
		[HarmonyPostfix]
		private static void RegisterSkin()
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Expected O, but got Unknown
			ChenIroncladInit.Logger.Info("Registering skin", 1);
			SkinData val = new SkinData(((AbstractModel)ModelDb.Character<Ironclad>()).Id, "chen", "Dawnstreak");
			val.RegisterCharacter((CharacterSkin)(object)new Chen_Skin());
			val.RegisterConfig("UseCardFrame", (Func<bool>)(() => ChenConfig.UseChenCardFrame));
			val.RegisterConfig("UseEnergy", (Func<bool>)(() => ChenConfig.UseChenEnergy));
			val.RegisterConfig("UseHands", (Func<bool>)(() => ChenConfig.UseChenMultArm));
			SkinRegistry.Register(val);
		}
	}
	public class Chen_Skin : CharacterSkin<Ironclad>
	{
		public override string CombatVisual => "res://ChenIronclad/scenes/character/chen_dawnstreak.tscn";

		public override string MerchantVisual => "res://ChenIronclad/scenes/character/chen_dawnstreak_merchant.tscn";

		public override string RestVisual => "res://ChenIronclad/scenes/character/chen_dawnstreak_rest_site.tscn";

		public override string CharacterSelectBg => "res://ChenIronclad/scenes/ui/chen_charselect_bg.tscn";

		public override string CharacterSelectPortrait => "res://ChenIronclad/assets/ui/characterSelect/chen_portrait.png";

		public override string CharacterSelectTransition => "res://materials/transitions/silent_transition_mat.tres";

		public override string CharacterIcon => "res://ChenIronclad/assets/ui/icon/character_icon_chen.png";

		public override string CharacterIconOutline => "res://ChenIronclad/assets/ui/icon/character_icon_outline_chen.png";

		public override string CharacterIconScene => "res://ChenIronclad/scenes/ui/chen_icon.tscn";

		public override string CharacterMapMarker => "res://ChenIronclad/assets/ui/icon/map_icon_chen.png";

		public override string CardFrameMaterial => "res://ChenIronclad/assets/ui/cards/frames/card_frame_chen.tres";

		public override string CardTrail => "res://ChenIronclad/scenes/ui/chen_card_trail.tscn";

		public override string EnergyIcon => "res://ChenIronclad/assets/ui/energy/chen_energy_icon.png";

		public override string[]? EnergyLayers => new string[5] { "res://ChenIronclad/assets/ui/energy/chen_orb_layer_1.png", "res://ChenIronclad/assets/ui/energy/chen_orb_layer_2.png", "res://ChenIronclad/assets/ui/energy/chen_orb_layer_3.png", "res://ChenIronclad/assets/ui/energy/chen_orb_layer_4.png", "res://ChenIronclad/assets/ui/energy/chen_orb_layer_5.png" };

		public override Color? EnergyLabelColor => new Color("0000AA");

		public override Color? EnergyLabelOutlineColor => new Color("0000AA");

		public override string? HandPoint => "res://ChenIronclad/assets/ui/arm/chen_pointing.png";

		public override string? HandRock => "res://ChenIronclad/assets/ui/arm/chen_rock.png";

		public override string? HandPaper => "res://ChenIronclad/assets/ui/arm/chen_paper.png";

		public override string? HandScissors => "res://ChenIronclad/assets/ui/arm/chen_scissor.png";
	}
}
namespace ChenIronclad.ChenIroncladCode.core
{
	[GlobalClass]
	[ScriptPath("res://ChenIroncladCode/core/ChenCardTrail.cs")]
	public class ChenCardTrail : NCardTrail
	{
		public class MethodName : MethodName
		{
		}

		public class PropertyName : PropertyName
		{
		}

		public class SignalName : SignalName
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void SaveGodotObjectData(GodotSerializationInfo info)
		{
			((NCardTrail)this).SaveGodotObjectData(info);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			((NCardTrail)this).RestoreGodotObjectData(info);
		}
	}
	[GlobalClass]
	[ScriptPath("res://ChenIroncladCode/core/ChenCardTrailVfx.cs")]
	public class ChenCardTrailVfx : NCardTrailVfx
	{
		public class MethodName : MethodName
		{
		}

		public class PropertyName : PropertyName
		{
		}

		public class SignalName : SignalName
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void SaveGodotObjectData(GodotSerializationInfo info)
		{
			((NCardTrailVfx)this).SaveGodotObjectData(info);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			((NCardTrailVfx)this).RestoreGodotObjectData(info);
		}
	}
	[GlobalClass]
	[ScriptPath("res://ChenIroncladCode/core/ChenCreatureVisuals.cs")]
	public class ChenCreatureVisuals : NCreatureVisuals
	{
		public class MethodName : MethodName
		{
		}

		public class PropertyName : PropertyName
		{
		}

		public class SignalName : SignalName
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void SaveGodotObjectData(GodotSerializationInfo info)
		{
			((NCreatureVisuals)this).SaveGodotObjectData(info);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			((NCreatureVisuals)this).RestoreGodotObjectData(info);
		}
	}
	[GlobalClass]
	[ScriptPath("res://ChenIroncladCode/core/ChenMerchantCharacter.cs")]
	public class ChenMerchantCharacter : NMerchantCharacter
	{
		public class MethodName : MethodName
		{
		}

		public class PropertyName : PropertyName
		{
		}

		public class SignalName : SignalName
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void SaveGodotObjectData(GodotSerializationInfo info)
		{
			((NMerchantCharacter)this).SaveGodotObjectData(info);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			((NMerchantCharacter)this).RestoreGodotObjectData(info);
		}
	}
	[GlobalClass]
	[ScriptPath("res://ChenIroncladCode/core/ChenRestSiteCharacter.cs")]
	public class ChenRestSiteCharacter : NRestSiteCharacter
	{
		public class MethodName : MethodName
		{
		}

		public class PropertyName : PropertyName
		{
		}

		public class SignalName : SignalName
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void SaveGodotObjectData(GodotSerializationInfo info)
		{
			((NRestSiteCharacter)this).SaveGodotObjectData(info);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			((NRestSiteCharacter)this).RestoreGodotObjectData(info);
		}
	}
	[GlobalClass]
	[ScriptPath("res://ChenIroncladCode/core/ChenSelectionReticle.cs")]
	public class ChenSelectionReticle : NSelectionReticle
	{
		public class MethodName : MethodName
		{
		}

		public class PropertyName : PropertyName
		{
		}

		public class SignalName : SignalName
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void SaveGodotObjectData(GodotSerializationInfo info)
		{
			((NSelectionReticle)this).SaveGodotObjectData(info);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			((NSelectionReticle)this).RestoreGodotObjectData(info);
		}
	}
}
