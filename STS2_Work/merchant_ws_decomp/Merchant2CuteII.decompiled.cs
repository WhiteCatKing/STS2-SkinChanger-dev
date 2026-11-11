using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using HarmonyLib;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Random;
using Merchant2CuteII.script;
using Merchant2CuteII.script.Compat;
using Merchant2CuteII.script.Settings;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETCoreApp,Version=v9.0", FrameworkDisplayName = ".NET 9.0")]
[assembly: AssemblyCompany("Merchant2CuteII")]
[assembly: AssemblyConfiguration("ExportRelease")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+1b3920fa561c47b7a23dcfbaf4805fec214669f3")]
[assembly: AssemblyProduct("Merchant2CuteII")]
[assembly: AssemblyTitle("Merchant2CuteII")]
[assembly: AssemblyHasScripts(new Type[] { typeof(MerchantSettingsSubmenu) })]
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
namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands
{
	public class MerchantConsoleCmd : AbstractConsoleCmd
	{
		public override string CmdName => "merchant";

		public override string Args => "status | real|fake <hand|set|leg> ... | point ... | voice ... ";

		public override string Description => "Configure the cute merchant skin: per-merchant hand variant / skeleton set, voice language and volume";

		public override bool IsNetworked => false;

		static MerchantConsoleCmd()
		{
			try
			{
				ConfigStore.Load();
				SkinApplier.WarmUp();
				MerchantRuntime.RefreshAll();
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Console init failed: " + ex.Message);
			}
		}

		public override CmdResult Process(Player? issuingPlayer, string[] args)
		{
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			string text = ((args.Length != 0) ? args[0].ToLowerInvariant() : "status");
			try
			{
				return (CmdResult)(text switch
				{
					"status" => new CmdResult(true, MerchantRuntime.DescribeCurrent()), 
					"real" => ProcessMerchant(MerchantKind.Real, args), 
					"fake" => ProcessMerchant(MerchantKind.Fake, args), 
					"point" => ProcessLegacyPoint(args), 
					"voice" => ProcessVoiceCommand(args), 
					"tune" => ProcessTuneCommand(args), 
					_ => Usage(), 
				});
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[Merchant2CuteII] Console command failed: {ex}");
				return new CmdResult(false, "Command failed: " + ex.Message);
			}
		}

		private static CmdResult Usage()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return new CmdResult(false, "Usage: merchant status | merchant real|fake hand <hand|foot|white|black|toggle> | merchant real|fake set <own|other|toggle> | merchant real|fake leg <on|off|toggle> | merchant tune [offset real|fake own|other <dx> <dy> | scale ... | reset] | ");
		}

		private static CmdResult ProcessMerchant(MerchantKind kind, string[] args)
		{
			//IL_053f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0486: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0430: Unknown result type (might be due to invalid IL or missing references)
			//IL_0364: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_0539: Unknown result type (might be due to invalid IL or missing references)
			string text = ((args.Length > 1) ? args[1].ToLowerInvariant() : "status");
			MerchantSkinSettings merchantSkinSettings = SettingsHost.Current.For(kind);
			string text2 = ((kind == MerchantKind.Real) ? "real" : "fake");
			switch (text)
			{
			case "status":
				return new CmdResult(true, $"{text2}: body={merchantSkinSettings.BodySet}, handSet={merchantSkinSettings.HandSet}, hand={merchantSkinSettings.HandVariant}, leg={(merchantSkinSettings.ShowInventoryLeg ? "on" : "off")}");
			case "hand":
			{
				string text6 = ((args.Length > 2) ? args[2].ToLowerInvariant() : "toggle");
				string value = ((text6 == "toggle") ? HandVariants.NextFor(kind, merchantSkinSettings.HandSet, merchantSkinSettings.HandVariant) : text6);
				if (!HandVariants.IsAllowed(kind, merchantSkinSettings.HandSet, value))
				{
					string value2 = string.Join("|", HandVariants.Allowed(kind, merchantSkinSettings.HandSet));
					return new CmdResult(false, $"Usage: merchant {text2} hand {value2}|toggle  (allowed set depends on hand-set={merchantSkinSettings.HandSet})");
				}
				merchantSkinSettings.HandVariant = HandVariants.NormalizeFor(kind, merchantSkinSettings.HandSet, value);
				return Commit(text2 + " hand -> " + merchantSkinSettings.HandVariant);
			}
			case "body":
			case "set":
			{
				if (kind == MerchantKind.Fake)
				{
					return new CmdResult(false, "假商人的身体美术固定为本套，不能借用商店商人的身体");
				}
				string text4 = ((args.Length > 2) ? args[2].ToLowerInvariant() : "toggle");
				merchantSkinSettings.BodySet = SkeletonSets.Normalize((text4 == "toggle") ? SkeletonSets.Next(merchantSkinSettings.BodySet) : text4);
				return Commit($"{text2} body art -> {merchantSkinSettings.BodySet} ({SkeletonSetAssets.DescribePart(kind, "body", merchantSkinSettings.BodySet)})");
			}
			case "hand-set":
			case "handset":
			{
				string text5 = ((args.Length > 2) ? args[2].ToLowerInvariant() : "toggle");
				merchantSkinSettings.HandSet = SkeletonSets.Normalize((text5 == "toggle") ? SkeletonSets.Next(merchantSkinSettings.HandSet) : text5);
				merchantSkinSettings.HandVariant = HandVariants.NormalizeFor(kind, merchantSkinSettings.HandSet, merchantSkinSettings.HandVariant);
				return Commit($"{text2} hand art -> {merchantSkinSettings.HandSet} ({SkeletonSetAssets.DescribePart(kind, "hand", merchantSkinSettings.HandSet)}), hand={merchantSkinSettings.HandVariant}");
			}
			case "leg":
			{
				if (!SettingsJson.CanToggleInventoryLeg(merchantSkinSettings, MerchantKind.Real))
				{
					return new CmdResult(false, "腿部贴图只能在「本套美术 + 手部变体=手」时开关（当前：body=" + merchantSkinSettings.BodySet + ", handSet=" + merchantSkinSettings.HandSet + ", hand=" + merchantSkinSettings.HandVariant + "）");
				}
				string text3 = ((args.Length > 2) ? args[2].ToLowerInvariant() : "toggle");
				MerchantSkinSettings merchantSkinSettings2 = merchantSkinSettings;
				bool showInventoryLeg;
				switch (text3)
				{
				case "on":
				case "true":
				case "1":
					showInventoryLeg = true;
					break;
				case "off":
				case "false":
				case "0":
					showInventoryLeg = false;
					break;
				default:
					showInventoryLeg = !merchantSkinSettings.ShowInventoryLeg;
					break;
				}
				merchantSkinSettings2.ShowInventoryLeg = showInventoryLeg;
				return Commit(text2 + " inventory leg -> " + (merchantSkinSettings.ShowInventoryLeg ? "on" : "off"));
			}
			default:
				return Usage();
			}
		}

		private static CmdResult ProcessLegacyPoint(string[] args)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			string text = ((args.Length > 1) ? args[1].ToLowerInvariant() : "status");
			if (text == "status")
			{
				return new CmdResult(true, "point (both): " + SettingsHost.Current.RealMerchant.HandVariant);
			}
			string handVariant = SettingsHost.Current.RealMerchant.HandVariant;
			string value = ((!(text == "toggle")) ? text : ((handVariant == "hand") ? "foot" : "hand"));
			if (!HandVariants.IsKnown(value))
			{
				return new CmdResult(false, "Usage: merchant point hand|foot|white|black|toggle|status");
			}
			ModSettings current = SettingsHost.Current;
			current.RealMerchant.HandVariant = HandVariants.NormalizeFor(MerchantKind.Real, current.RealMerchant.HandSet, value);
			current.FakeMerchant.HandVariant = HandVariants.NormalizeFor(MerchantKind.Fake, current.FakeMerchant.HandSet, value);
			return Commit("both merchants hand -> " + HandVariants.Normalize(value) + " (legacy behaviour)");
		}

		private static CmdResult ProcessVoiceCommand(string[] args)
		{
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			string text = ((args.Length > 1) ? args[1].ToLowerInvariant() : "status");
			if (text == "status")
			{
				return new CmdResult(true, $"voice: {SettingsHost.Current.Voice.Language}, db: {SettingsHost.Current.Voice.VolumeDb:+0.##;-0.##;0}");
			}
			if (text == "db")
			{
				if (args.Length < 3)
				{
					return new CmdResult(false, "Usage: merchant voice db <value>");
				}
				if (!float.TryParse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					return new CmdResult(false, "Invalid db value");
				}
				SettingsHost.Current.Voice.VolumeDb = Mathf.Clamp(result, -12f, 12f);
				return Commit($"voice volume -> {SettingsHost.Current.Voice.VolumeDb} dB");
			}
			if (text != "toggle" && text != "default" && text != "jp" && text != "zh" && text != "cn" && text != "ja")
			{
				return new CmdResult(false, "Usage: merchant voice default|jp|zh|toggle|status|db <value>");
			}
			string language = SettingsHost.Current.Voice.Language;
			string text2 = ((!(text == "toggle")) ? VoiceLanguages.Normalize(text) : ((language == "default") ? "jp" : ((language == "jp") ? "zh" : "default")));
			string text3 = text2;
			SettingsHost.Current.Voice.Language = text3;
			return Commit("voice language -> " + text3);
		}

		private static CmdResult ProcessTuneCommand(string[] args)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			if (args.Length <= 1)
			{
				return new CmdResult(true, MerchantVisualMotion.Describe());
			}
			if (string.Equals(args[1], "probe", StringComparison.OrdinalIgnoreCase))
			{
				return new CmdResult(true, MerchantVisualMotion.ProbeScene());
			}
			string text = args[1].ToLowerInvariant();
			if (text == "reset")
			{
				if (args.Length >= 4)
				{
					MerchantVisualMotion.Reset(ParseKey(args[2], args[3]));
				}
				else
				{
					MerchantVisualMotion.Reset();
				}
				MerchantVisualMotion.Save();
				MerchantVisualMotion.ReapplyAll();
				return new CmdResult(true, "reset -> " + MerchantVisualMotion.Describe());
			}
			if (args.Length < 4)
			{
				return new CmdResult(false, "Usage: merchant tune offset|set|scale real|fake own|other <values>");
			}
			string text2 = ParseKey(args[2], args[3]);
			if (text2.Length == 0)
			{
				return new CmdResult(false, "Usage: merchant tune <offset|set|scale> real|fake own|other ...");
			}
			if ((text == "offset" || text == "set") ? true : false)
			{
				if (args.Length < 6 || !float.TryParse(args[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var result) || !float.TryParse(args[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
				{
					return new CmdResult(false, "Usage: merchant tune " + text + " " + text2 + " <dx> <dy>");
				}
				if (text == "offset")
				{
					MerchantVisualMotion.AddOffsetDelta(text2, new Vector2(result, result2));
				}
				else
				{
					MerchantVisualMotion.SetOffset(text2, new Vector2(result, result2));
				}
				MerchantVisualMotion.Save();
				MerchantVisualMotion.ReapplyAll();
				return new CmdResult(true, text2 + " -> " + MerchantVisualMotion.Describe());
			}
			if (text == "scale")
			{
				if (args.Length < 5 || !float.TryParse(args[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
				{
					return new CmdResult(false, "Usage: merchant tune scale " + text2 + " <value>");
				}
				MerchantVisualMotion.SetScale(text2, result3);
				MerchantVisualMotion.Save();
				MerchantVisualMotion.ReapplyAll();
				return new CmdResult(true, text2 + " -> " + MerchantVisualMotion.Describe());
			}
			return new CmdResult(false, "Usage: merchant tune [offset|set|scale|reset] ...");
		}

		private static string ParseKey(string merchant, string art)
		{
			string text = merchant.ToLowerInvariant();
			string text2 = art.ToLowerInvariant();
			if (text != "real" && text != "fake")
			{
				return string.Empty;
			}
			if (text2 != "own" && text2 != "other")
			{
				return string.Empty;
			}
			return text + "/" + text2;
		}

		private static CmdResult Commit(string message)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			SettingsHost.SaveAndNotify();
			TaskHelper.RunSafely(MerchantRuntime.RefreshAllNextFrame());
			return new CmdResult(true, message);
		}

		public static int ApplyNow()
		{
			SettingsHost.SaveAndNotify();
			TaskHelper.RunSafely(MerchantRuntime.RefreshAllNextFrame());
			return 0;
		}

		internal static int ApplyPointVariantNow()
		{
			return ApplyNow();
		}
	}
}
namespace Merchant2CuteII
{
	[ModInitializer("Init")]
	public static class Merchant2CuteIIMod
	{
		private const string ModName = "Merchant2CuteII";

		public static void Init()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			PatchAllSafely(new Harmony("Merchant2CuteII"));
			try
			{
				GameVersion.ForceFromEnvironment();
				GD.Print("[Merchant2CuteII] " + VersionProfile.Describe());
				SettingsHost.Load();
				MerchantVisualMotion.Load();
				SkinApplier.WarmUp();
				ResourcePreloader.PreloadAll();
				TaskHelper.RunSafely(ResourcePreloader.PreloadAudioPathsAsync(ModConfig.Paths.MerchantVoicePaths));
				TaskHelper.RunSafely(ResourcePreloader.PreloadSpinePathsAsync(ModConfig.Paths.SpinePaths));
				SettingsHost.Changed += delegate
				{
					MerchantRuntime.RefreshAll();
				};
				GD.Print("[Merchant2CuteII] Ready. " + MerchantRuntime.DescribeCurrent());
			}
			catch (Exception value)
			{
				GD.PrintErr($"[Merchant2CuteII] Initialisation failed: {value}");
			}
		}

		private static void PatchAllSafely(Harmony harmony)
		{
			int num = 0;
			int num2 = 0;
			Type[] types = typeof(Merchant2CuteIIMod).Assembly.GetTypes();
			foreach (Type type in types)
			{
				if (type.GetCustomAttributes(typeof(HarmonyPatch), inherit: true).Length != 0)
				{
					try
					{
						harmony.CreateClassProcessor(type).Patch();
						num++;
					}
					catch (Exception ex)
					{
						num2++;
						GD.PrintErr("[Merchant2CuteII] patch skipped for " + type.Name + ": " + ex.Message);
					}
				}
			}
			GD.Print($"[Merchant2CuteII] Harmony patches: applied={num}, failed={num2}");
		}
	}
}
namespace Merchant2CuteII.script
{
	internal static class AnimationHelper
	{
		public static bool TryApplyVariantToHandNode(Node handNode)
		{
			return SkinApplier.ApplyToHand(handNode);
		}

		public static bool ShouldUseLegacyHandSwitching()
		{
			return VersionProfile.LegacyAnimationSignature;
		}

		public static bool TrySetAnimationOnTarget(Node target, string animationName)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			if (target == null || string.IsNullOrEmpty(animationName))
			{
				return false;
			}
			try
			{
				MegaSprite val = new MegaSprite(Variant.op_Implicit((GodotObject)(object)target));
				if (!HasAnimation(val, animationName))
				{
					return false;
				}
				return Play(val.GetAnimationState(), animationName);
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] AnimationHelper.TrySetAnimationOnTarget failed: " + ex.Message);
			}
			return false;
		}

		public static bool TrySetFirstAvailableAnimation(Node target, IReadOnlyList<string> candidates)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			if (target == null || candidates == null || candidates.Count == 0)
			{
				return false;
			}
			try
			{
				MegaSprite val = new MegaSprite(Variant.op_Implicit((GodotObject)(object)target));
				MegaAnimationState animationState = val.GetAnimationState();
				foreach (string candidate in candidates)
				{
					if (HasAnimation(val, candidate) && Play(animationState, candidate))
					{
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] AnimationHelper.TrySetFirstAvailableAnimation failed: " + ex.Message);
			}
			return false;
		}

		public static bool HasAnimation(MegaSprite sprite, string animationName)
		{
			try
			{
				return sprite.HasAnimation(animationName);
			}
			catch
			{
				return false;
			}
		}

		private static bool Play(MegaAnimationState state, string animationName, bool loop = false, int track = 0)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			if (state == null)
			{
				return false;
			}
			((MegaSpineBinding)state).BoundObject.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
			{
				Variant.op_Implicit(animationName),
				Variant.op_Implicit(loop),
				Variant.op_Implicit(track)
			});
			return true;
		}
	}
	public static class ConfigStore
	{
		public static ModSettings Settings => SettingsHost.Current;

		public static void Load()
		{
			SettingsHost.Load();
		}

		public static void Save()
		{
			SettingsHost.SaveAndNotify();
		}
	}
	internal static class MerchantContextResolver
	{
		public static bool IsRealMerchantButton(NMerchantButton button)
		{
			return TryGetMerchantSceneRoot((Node)(object)button) is NMerchantRoom;
		}

		public static bool IsFakeMerchantButton(NMerchantButton button)
		{
			return TryGetMerchantSceneRoot((Node)(object)button) is NFakeMerchant;
		}

		private static Node? TryGetMerchantSceneRoot(Node node)
		{
			for (Node val = node; val != null; val = val.GetParent())
			{
				if (val is NMerchantRoom || val is NFakeMerchant)
				{
					return val;
				}
			}
			return null;
		}
	}
	internal static class MerchantVisualResolver
	{
		public static bool ShouldUseLegacyMerchantVisualLookup()
		{
			return VersionProfile.LegacyMerchantVisualLookup;
		}

		public static Node? TryGetMerchantVisual(Node? node)
		{
			if (node == null)
			{
				return null;
			}
			return node.GetNodeOrNull(NodePath.op_Implicit("%MerchantVisual")) ?? node.GetNodeOrNull(NodePath.op_Implicit("MerchantVisual")) ?? node.FindChild("MerchantVisual", true, false);
		}
	}
	[HarmonyPatch(typeof(NMerchantButton), "_Ready")]
	public static class MerchantButtonPatch
	{
		[HarmonyPrefix]
		public static bool Prefix(NMerchantButton __instance)
		{
			if (__instance == null)
			{
				return true;
			}
			Node val = (Node)(MerchantVisualResolver.ShouldUseLegacyMerchantVisualLookup() ? ((object)((Node)__instance).GetNodeOrNull(NodePath.op_Implicit("%MerchantVisual"))) : ((object)MerchantVisualResolver.TryGetMerchantVisual((Node?)(object)__instance)));
			if (val == null)
			{
				GD.PrintErr("[Merchant2CuteII] Cannot find MerchantVisual in MerchantButton");
				return true;
			}
			Node2D val2 = (Node2D)(object)((val is Node2D) ? val : null);
			if (val2 != null)
			{
				MerchantVisualMotion.Apply(__instance, val2);
			}
			return true;
		}
	}
	[HarmonyPatch(typeof(NMerchantRoom), "FoulPotionThrown")]
	public static class MerchantRoomFoulPotionPatch
	{
		[HarmonyPrefix]
		public static void Prefix(NMerchantRoom __instance)
		{
			try
			{
				if (__instance != null)
				{
					Node val = (Node)(MerchantVisualResolver.ShouldUseLegacyMerchantVisualLookup() ? ((object)((Node)__instance).GetNodeOrNull(NodePath.op_Implicit("%MerchantVisual"))) : ((object)(MerchantVisualResolver.TryGetMerchantVisual((Node?)(object)__instance.MerchantButton) ?? MerchantVisualResolver.TryGetMerchantVisual((Node?)(object)__instance))));
					if (val == null)
					{
						GD.PrintErr("[Merchant2CuteII] Cannot find MerchantVisual in MerchantRoom or MerchantButton");
					}
					else if (!AnimationHelper.TrySetFirstAvailableAnimation(val, AnimationLogic.FoulPotionCandidates()))
					{
						GD.PrintErr("[Merchant2CuteII] Merchant animation 'poison' does not exist or is not ready");
					}
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Error switching foul-potion animation: " + ex.Message);
			}
		}

		private static string[] FoulAnimationCandidates()
		{
			return AnimationLogic.FoulPotionCandidates();
		}
	}
	[HarmonyPatch(typeof(SfxCmd), "Play", new Type[]
	{
		typeof(string),
		typeof(float)
	})]
	public static class MerchantVoiceSfxPatch
	{
		private static readonly HashSet<string> LoggedUnmapped = new HashSet<string>();

		private static readonly HashSet<string> LoggedMapped = new HashSet<string>();

		[HarmonyPrefix]
		public static bool Prefix(string __0, float __1)
		{
			return !MerchantVoiceRouter.TryReplace(__0, __1, "SfxCmd");
		}

		public static bool TryPlayVoice(string voicePath, float volume, float extraDb = 0f)
		{
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Expected O, but got Unknown
			float num = Mathf.Max(0f, volume * Mathf.DbToLinear(extraDb));
			if (NDebugAudioManager.Instance != null)
			{
				NDebugAudioManager.Instance.Play(voicePath, num, (PitchVariance)0);
				return true;
			}
			AudioStream val = ResourceLoader.Load<AudioStream>("res://debug_audio/" + voicePath, (string)null, (CacheMode)1);
			if (val == null)
			{
				GD.PrintErr("[Merchant2CuteII] Merchant voice stream not found: res://debug_audio/" + voicePath);
				return false;
			}
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val2 = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (((val2 != null) ? val2.Root : null) == null)
			{
				GD.PrintErr("[Merchant2CuteII] Merchant voice fallback failed: SceneTree root is null");
				return false;
			}
			AudioStreamPlayer val3 = new AudioStreamPlayer
			{
				Stream = val,
				VolumeLinear = num,
				Bus = StringName.op_Implicit("SFX"),
				Name = StringName.op_Implicit("MerchantVoiceFallbackPlayer")
			};
			((Node)val2.Root).AddChild((Node)(object)val3, false, (InternalMode)0);
			val3.Finished += ((Node)val3).QueueFree;
			val3.Play(0f);
			return true;
		}
	}
	[HarmonyPatch(typeof(NAudioManager), "PlayOneShot", new Type[]
	{
		typeof(string),
		typeof(float)
	})]
	public static class MerchantVoiceAudioPatch
	{
		[HarmonyPrefix]
		public static bool Prefix(string __0, float __1)
		{
			return !MerchantVoiceRouter.TryReplace(__0, __1, "NAudioManager");
		}
	}
	[HarmonyPatch(typeof(NAudioManager), "PlayOneShot", new Type[]
	{
		typeof(string),
		typeof(Dictionary<string, float>),
		typeof(float)
	})]
	public static class MerchantVoiceAudioParamsPatch
	{
		[HarmonyPrefix]
		public static bool Prefix(string __0, float __2)
		{
			return !MerchantVoiceRouter.TryReplace(__0, __2, "NAudioManager(param)");
		}
	}
	internal static class MerchantVoiceRouter
	{
		private static readonly HashSet<string> LoggedMapped = new HashSet<string>();

		private static readonly HashSet<string> LoggedUnmapped = new HashSet<string>();

		public static bool TryReplace(string sfx, float volume, string source)
		{
			if (string.IsNullOrEmpty(sfx) || sfx.StartsWith("res://", StringComparison.Ordinal))
			{
				return false;
			}
			string merchantVoicePath = ModConfig.Voice.GetMerchantVoicePath(sfx);
			if (string.IsNullOrEmpty(merchantVoicePath))
			{
				if (ModConfig.Options.MerchantVoiceVariant != "default" && sfx.Contains("merchant", StringComparison.OrdinalIgnoreCase) && LoggedUnmapped.Add(sfx))
				{
					GD.Print("[Merchant2CuteII] voice: unmapped merchant sfx '" + sfx + "' (kept vanilla sound)");
				}
				return false;
			}
			try
			{
				if (MerchantVoiceSfxPatch.TryPlayVoice(merchantVoicePath, volume, ModConfig.Options.ExtraDb))
				{
					if (LoggedMapped.Add(sfx))
					{
						GD.Print($"[Merchant2CuteII] voice[{source}]: '{sfx}' -> {merchantVoicePath}");
					}
					return true;
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Merchant voice replacement failed: " + ex.Message);
			}
			return false;
		}
	}
	[HarmonyPatch(typeof(NMerchantCharacter), "PlayAnimation")]
	public static class MerchantCharacterPlayAnimationPatch
	{
		[HarmonyPrefix]
		public static bool Prefix(NMerchantCharacter __instance, string __0, bool __1)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			bool flag = __1;
			if (__instance == null)
			{
				return true;
			}
			try
			{
				Node val = TryGetMerchantSpineNode(__instance);
				if (val == null)
				{
					return true;
				}
				MegaSprite val2 = new MegaSprite(Variant.op_Implicit((GodotObject)(object)val));
				if (AnimationHelper.HasAnimation(val2, __0))
				{
					return true;
				}
				string[] array = FallbackAnimationCandidates(__0);
				foreach (string text in array)
				{
					if (!AnimationHelper.HasAnimation(val2, text))
					{
						continue;
					}
					MegaAnimationState animationState = val2.GetAnimationState();
					if (AnimationHelper.ShouldUseLegacyHandSwitching())
					{
						((MegaSpineBinding)animationState).BoundObject.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[2]
						{
							Variant.op_Implicit(text),
							Variant.op_Implicit(flag)
						});
					}
					else
					{
						((MegaSpineBinding)animationState).BoundObject.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
						{
							Variant.op_Implicit(text),
							Variant.op_Implicit(flag),
							Variant.op_Implicit(0)
						});
					}
					if (flag)
					{
						MegaTrackEntry current = animationState.GetCurrent(0);
						if (current != null)
						{
							current.SetTrackTime(current.GetAnimationEnd() * Rng.Chaotic.NextFloat(1f));
						}
					}
					GD.Print($"[Merchant2CuteII] Animation '{__0}' missing on current skeleton -> fallback '{text}'");
					return false;
				}
				GD.Print("[Merchant2CuteII] Animation '" + __0 + "' not present on current skeleton, skipped.");
				return false;
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Safe merchant animation failed: " + ex.Message);
				return true;
			}
		}

		private static Node? TryGetMerchantSpineNode(NMerchantCharacter merchantCharacter)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			foreach (Node child in ((Node)merchantCharacter).GetChildren(false))
			{
				try
				{
					new MegaSprite(Variant.op_Implicit((GodotObject)(object)child));
					return child;
				}
				catch
				{
				}
			}
			if (((Node)merchantCharacter).GetChildCount(false) > 0)
			{
				return ((Node)merchantCharacter).GetChild(0, false);
			}
			return null;
		}

		private static string[] FallbackAnimationCandidates(string requested)
		{
			return AnimationLogic.FallbackFor(requested);
		}
	}
	[HarmonyPatch(typeof(NMerchantHand), "_Ready")]
	public static class MerchantHandReadyPatch
	{
		[HarmonyPostfix]
		public static void Postfix(NMerchantHand __instance)
		{
			if (__instance == null)
			{
				return;
			}
			try
			{
				AnimationHelper.TryApplyVariantToHandNode((Node)(object)__instance);
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Error applying merchant hand variant on ready: " + ex.Message);
			}
		}
	}
	[HarmonyPatch(typeof(NMerchantInventory), "_Ready")]
	public static class MerchantInventoryLegPatch
	{
		[HarmonyPrefix]
		public static void Prefix(NMerchantInventory __instance)
		{
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Expected O, but got Unknown
			if (__instance == null || __instance is NFakeMerchantInventory)
			{
				return;
			}
			Control nodeOrNull = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%SlotsContainer"));
			if (nodeOrNull == null)
			{
				GD.PrintErr("[Merchant2CuteII] Cannot find %SlotsContainer in MerchantInventory");
			}
			else if (((Node)nodeOrNull).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("MerchantInventoryLeg")) == null)
			{
				Texture2D val = GD.Load<Texture2D>(ModConfig.Paths.MerchantLegTexture);
				if (val == null)
				{
					GD.PrintErr("[Merchant2CuteII] Cannot load merchant leg texture: " + ModConfig.Paths.MerchantLegTexture);
					return;
				}
				Vector2 position = default(Vector2);
				((Vector2)(ref position))..ctor(nodeOrNull.Position.X + ModConfig.Merchant.LegPosition.X, nodeOrNull.Position.Y + ModConfig.Merchant.LegPosition.Y);
				TextureRect val2 = new TextureRect
				{
					Name = StringName.op_Implicit("MerchantInventoryLeg"),
					Texture = val,
					Position = position,
					RotationDegrees = ModConfig.Merchant.LegRotationDegrees,
					Scale = ModConfig.Merchant.LegScale,
					MouseFilter = (MouseFilterEnum)2,
					Visible = MerchantRuntime.ShouldShowInventoryLeg(MerchantKind.Real),
					ZIndex = 0,
					ExpandMode = (ExpandModeEnum)1,
					StretchMode = (StretchModeEnum)5,
					CustomMinimumSize = val.GetSize()
				};
				((Node)__instance).AddChild((Node)(object)val2, false, (InternalMode)0);
				((Node)__instance).MoveChild((Node)(object)val2, ((Node)nodeOrNull).GetIndex(false) + 1);
			}
		}
	}
	[HarmonyPatch(typeof(NMerchantInventory), "Open")]
	public static class MerchantInventoryLegOpenPatch
	{
		[HarmonyPostfix]
		public static void Postfix(NMerchantInventory __instance)
		{
			MerchantInventoryLegMotion.TweenLegY(__instance, 0.699999988079071, (TransitionType)2, (EaseType)1, 80f);
		}
	}
	[HarmonyPatch(typeof(NMerchantInventory), "Close")]
	public static class MerchantInventoryLegClosePatch
	{
		[HarmonyPostfix]
		public static void Postfix(NMerchantInventory __instance)
		{
			MerchantInventoryLegMotion.TweenLegY(__instance, 0.5, (TransitionType)7, (EaseType)1, -1000f);
		}
	}
	internal static class MerchantInventoryLegMotion
	{
		public static void TweenLegY(NMerchantInventory inventory, double duration, TransitionType transition, EaseType ease, float slotsTargetY)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			if (inventory != null)
			{
				Control nodeOrNull = ((Node)inventory).GetNodeOrNull<Control>(NodePath.op_Implicit("%SlotsContainer"));
				TextureRect nodeOrNull2 = ((Node)inventory).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("MerchantInventoryLeg"));
				if (nodeOrNull != null && nodeOrNull2 != null)
				{
					Vector2 val = default(Vector2);
					((Vector2)(ref val))..ctor(nodeOrNull.Position.X + ModConfig.Merchant.LegPosition.X, slotsTargetY + ModConfig.Merchant.LegPosition.Y);
					((Node)inventory).CreateTween().TweenProperty((GodotObject)(object)nodeOrNull2, NodePath.op_Implicit("position"), Variant.op_Implicit(val), duration).SetEase(ease)
						.SetTrans(transition)
						.FromCurrent();
				}
			}
		}
	}
	public static class ModConfig
	{
		public static class Paths
		{
			public static string MerchantLegTexture => "res://animations/customs/merchant/leg.png";

			public static string[] MerchantVoicePaths => new string[7] { "res://debug_audio/jp/welcome.mp3", "res://debug_audio/jp/thanks.mp3", "res://debug_audio/jp/disapointment.mp3", "res://debug_audio/zh/welcome1.wav", "res://debug_audio/zh/welcome2.wav", "res://debug_audio/zh/thanks.wav", "res://debug_audio/zh/disappointment.wav" };

			public static string[] SpinePaths => new string[4] { "res://animations/backgrounds/fake_merchant_room/hand/fake_merchant_hand_skel_data.tres", "res://animations/backgrounds/fake_merchant_room/top/fake_merchant_top.tres", "res://animations/backgrounds/merchant_room/hand/merchant_hand_skel_data.tres", "res://animations/backgrounds/merchant_room/top/shop_merchant_top.tres" };
		}

		public static class Options
		{
			public static string HandVariant
			{
				get
				{
					return SettingsHost.Current.RealMerchant.HandVariant;
				}
				set
				{
					MerchantSkinSettings realMerchant = SettingsHost.Current.RealMerchant;
					MerchantSkinSettings fakeMerchant = SettingsHost.Current.FakeMerchant;
					realMerchant.HandVariant = HandVariants.NormalizeFor(MerchantKind.Real, realMerchant.HandSet, value);
					fakeMerchant.HandVariant = HandVariants.NormalizeFor(MerchantKind.Fake, fakeMerchant.HandSet, value);
				}
			}

			public static bool UseFoot => HandVariant == "foot";

			public static bool UseFootLike => SettingsStore.IsFootLike(HandVariant);

			public static string MerchantVoiceVariant
			{
				get
				{
					return SettingsHost.Current.Voice.Language;
				}
				set
				{
					SettingsHost.Current.Voice.Language = VoiceLanguages.Normalize(value);
				}
			}

			public static bool UseMerchantJpVoice => MerchantVoiceVariant == "jp";

			public static bool UseMerchantZhVoice => MerchantVoiceVariant == "zh";

			public static float ExtraDb
			{
				get
				{
					return SettingsHost.Current.Voice.VolumeDb;
				}
				set
				{
					SettingsHost.Current.Voice.VolumeDb = Mathf.Clamp(value, -12f, 12f);
				}
			}
		}

		public static class Voice
		{
			public static string? GetMerchantVoicePath(string sfxEvent)
			{
				string language = SettingsHost.Current.Voice.Language;
				string[] array = ((language == "jp") ? GetJpCandidates(sfxEvent) : ((!(language == "zh")) ? Array.Empty<string>() : GetZhCandidates(sfxEvent)));
				string[] array2 = array;
				if (array2.Length == 0)
				{
					return null;
				}
				array = array2;
				foreach (string text in array)
				{
					if (ResourceLoader.Exists("res://debug_audio/" + text, ""))
					{
						return text;
					}
				}
				return array2[0];
			}

			private static string[] GetJpCandidates(string sfxEvent)
			{
				if (sfxEvent.Contains("reverse_merchant"))
				{
					if (!sfxEvent.Contains("laugh"))
					{
						return new string[1] { "jp/disapointment.mp3" };
					}
					return new string[1] { "jp/welcome.mp3" };
				}
				if (sfxEvent.Contains("merchant_welcome"))
				{
					return new string[1] { "jp/welcome.mp3" };
				}
				if (sfxEvent.Contains("merchant_thank_yous") || sfxEvent.Contains("merchant_passive"))
				{
					return new string[1] { "jp/thanks.mp3" };
				}
				if (sfxEvent.Contains("merchant_dissapointment") || sfxEvent.Contains("merchant_dissapoint"))
				{
					return new string[1] { "jp/disapointment.mp3" };
				}
				return Array.Empty<string>();
			}

			private static string[] GetZhCandidates(string sfxEvent)
			{
				if (sfxEvent.Contains("reverse_merchant"))
				{
					if (!sfxEvent.Contains("laugh"))
					{
						return new string[1] { "zh/disappointment.wav" };
					}
					return new string[2] { "zh/welcome2.wav", "zh/welcome1.wav" };
				}
				if (sfxEvent.Contains("merchant_welcome"))
				{
					return new string[2] { "zh/welcome1.wav", "zh/welcome2.wav" };
				}
				if (sfxEvent.Contains("merchant_thank_yous") || sfxEvent.Contains("merchant_passive"))
				{
					return new string[1] { "zh/thanks.wav" };
				}
				if (sfxEvent.Contains("merchant_dissapointment") || sfxEvent.Contains("merchant_dissapoint"))
				{
					return new string[1] { "zh/disappointment.wav" };
				}
				return Array.Empty<string>();
			}
		}

		public static class Merchant
		{
			public static Vector2 VisualPositionOffset => new Vector2(1260f, 620f);

			public static Vector2 VisualScale => new Vector2(0.18f, 0.18f);

			public static Vector2 LegPosition => new Vector2(0f, -900f);

			public static Vector2 LegScale => new Vector2(0.8f, 0.8f);

			public static float LegRotationDegrees => -6f;
		}

		public static class FakeMerchant
		{
			public static Vector2 VisualPositionOffset => Vector2.Zero;

			public static Vector2 VisualScale => new Vector2(0.36f, 0.36f);
		}

		public static float GetMerchantVoiceVolumeLinear()
		{
			return SettingsHost.Current.Voice.LinearGain;
		}
	}
	internal static class ResourcePreloader
	{
		private static readonly Dictionary<string, Resource?> _cache = new Dictionary<string, Resource>();

		public static void PreloadAll()
		{
			Preload(ModConfig.Paths.MerchantLegTexture);
		}

		public static async Task PreloadAudioPathsAsync(IEnumerable<string> paths, int perFrame = 2)
		{
			if (paths == null)
			{
				return;
			}
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree tree = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			int batch = 0;
			int loaded = 0;
			Stopwatch sw = Stopwatch.StartNew();
			foreach (string path in paths)
			{
				if (string.IsNullOrEmpty(path) || _cache.ContainsKey(path))
				{
					continue;
				}
				try
				{
					Resource val = ResourceLoader.Load<Resource>(path, (string)null, (CacheMode)1);
					_cache[path] = val;
					if (val != null)
					{
						loaded++;
						GD.Print("[Merchant2CuteII] Preloaded audio resource: " + path);
					}
					else
					{
						GD.PrintErr("[Merchant2CuteII] Preload audio: resource not found " + path);
					}
				}
				catch (Exception ex)
				{
					GD.PrintErr("[Merchant2CuteII] Preload audio exception for " + path + ": " + ex.Message);
				}
				batch++;
				if (perFrame > 0 && batch >= perFrame && tree != null)
				{
					batch = 0;
					await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
				}
			}
			GD.Print($"[Merchant2CuteII] PreloadAudioPathsAsync completed: loaded={loaded} timeMs={sw.ElapsedMilliseconds}");
		}

		public static void PreloadSpinePaths(IEnumerable<string> paths)
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			if (paths == null)
			{
				return;
			}
			foreach (string path in paths)
			{
				if (string.IsNullOrEmpty(path) || _cache.ContainsKey(path))
				{
					continue;
				}
				try
				{
					Resource val = ResourceLoader.Load<Resource>(path, (string)null, (CacheMode)1);
					if (val != null)
					{
						try
						{
							new MegaSkeletonDataResource(Variant.op_Implicit((GodotObject)(object)val));
							_cache[path] = val;
							GD.Print("[Merchant2CuteII] Preloaded spine resource: " + path);
						}
						catch
						{
							_cache[path] = val;
							GD.Print("[Merchant2CuteII] Preloaded raw resource (no wrapper): " + path);
						}
					}
					else
					{
						_cache[path] = null;
						GD.PrintErr("[Merchant2CuteII] Preload spine: resource not found " + path);
					}
				}
				catch (Exception ex)
				{
					GD.PrintErr("[Merchant2CuteII] Preload spine exception for " + path + ": " + ex.Message);
				}
			}
		}

		public static async Task PreloadSpinePathsAsync(IEnumerable<string> paths, int perFrame = 1)
		{
			if (paths == null)
			{
				return;
			}
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree tree = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			Stopwatch sw = Stopwatch.StartNew();
			int loaded = 0;
			int batch = 0;
			foreach (string path in paths)
			{
				if (string.IsNullOrEmpty(path) || _cache.ContainsKey(path))
				{
					continue;
				}
				try
				{
					Resource val = ResourceLoader.Load<Resource>(path, (string)null, (CacheMode)1);
					if (val != null)
					{
						try
						{
							new MegaSkeletonDataResource(Variant.op_Implicit((GodotObject)(object)val));
							_cache[path] = val;
							loaded++;
							GD.Print("[Merchant2CuteII] Preloaded spine resource: " + path);
						}
						catch
						{
							_cache[path] = val;
							loaded++;
							GD.Print("[Merchant2CuteII] Preloaded raw resource (no wrapper): " + path);
						}
					}
					else
					{
						_cache[path] = null;
						GD.PrintErr("[Merchant2CuteII] Preload spine: resource not found " + path);
					}
				}
				catch (Exception ex)
				{
					GD.PrintErr("[Merchant2CuteII] Preload spine exception for " + path + ": " + ex.Message);
				}
				batch++;
				if (perFrame > 0 && batch >= perFrame && tree != null)
				{
					batch = 0;
					await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
				}
			}
			long elapsedMilliseconds = sw.ElapsedMilliseconds;
			GD.Print($"[Merchant2CuteII] PreloadSpinePathsAsync completed: loaded={loaded} timeMs={elapsedMilliseconds}");
		}

		private static void Preload(string path)
		{
			if (string.IsNullOrEmpty(path) || _cache.ContainsKey(path))
			{
				return;
			}
			try
			{
				Resource val = ResourceLoader.Load<Resource>(path, (string)null, (CacheMode)1);
				_cache[path] = val;
				if (val == null)
				{
					GD.PrintErr("[Merchant2CuteII] Preload failed: " + path + " returned null");
				}
				else
				{
					GD.Print("[Merchant2CuteII] Preloaded: " + path);
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Preload exception for " + path + ": " + ex.Message);
			}
		}
	}
}
namespace Merchant2CuteII.script.Settings
{
	public static class AnimationLogic
	{
		public const string FoulPotionAnimation = "poison";

		public static readonly string[] BodyFallbackChain = new string[2] { "idle_loop", "idle" };

		public static string[] FoulPotionCandidates()
		{
			return Build("poison", "poison_ol", "idle_loop", "idle");
		}

		public static string[] FallbackFor(string requested)
		{
			return Build(requested, requested + "_ol", "idle_loop", "idle");
		}

		private static string[] Build(params string[] candidates)
		{
			List<string> list = new List<string>(candidates.Length);
			foreach (string text in candidates)
			{
				if (!string.IsNullOrEmpty(text) && !list.Contains(text))
				{
					list.Add(text);
				}
			}
			return list.ToArray();
		}
	}
	public static class MerchantRuntime
	{
		public static bool ShouldShowInventoryLeg(MerchantKind kind)
		{
			return SettingsJson.ShouldShowInventoryLeg(SettingsHost.Current.For(kind), kind);
		}

		public static int RefreshAll()
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			MainLoop obj = ((mainLoop is SceneTree) ? mainLoop : null);
			Node val = (Node)(object)((obj != null) ? ((SceneTree)obj).Root : null);
			if (val == null)
			{
				return 0;
			}
			int hands = 0;
			new HashSet<ulong>();
			Walk(val, delegate(Node node)
			{
				NMerchantHand val2 = (NMerchantHand)(object)((node is NMerchantHand) ? node : null);
				if (val2 != null)
				{
					if (SkinApplier.ApplyToHand((Node?)(object)val2))
					{
						hands++;
					}
				}
				else
				{
					TextureRect val3 = (TextureRect)(object)((node is TextureRect) ? node : null);
					if (val3 != null && ((Node)val3).Name == StringName.op_Implicit("MerchantInventoryLeg"))
					{
						((CanvasItem)val3).Visible = ShouldShowInventoryLeg(MerchantKind.Real);
					}
				}
			});
			GD.Print($"[Merchant2CuteII] Settings refreshed: {hands} hand node(s) updated.");
			return hands;
		}

		public static async Task RefreshAllNextFrame()
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null)
			{
				await ((GodotObject)val).ToSignal((GodotObject)(object)val, SignalName.ProcessFrame);
				RefreshAll();
			}
		}

		public static int CountHands(Node root)
		{
			int count = 0;
			Walk(root, delegate(Node node)
			{
				if (node is NMerchantHand)
				{
					count++;
				}
			});
			return count;
		}

		private static void Walk(Node root, Action<Node> visit)
		{
			if (root == null)
			{
				return;
			}
			foreach (Node child in root.GetChildren(false))
			{
				if (child != null)
				{
					try
					{
						visit(child);
					}
					catch (Exception ex)
					{
						GD.PrintErr($"[Merchant2CuteII] Runtime visit failed on {child.Name}: {ex.Message}");
					}
					Walk(child, visit);
				}
			}
		}

		public static string DescribeCurrent()
		{
			ModSettings current = SettingsHost.Current;
			<>y__InlineArray9<object> buffer = default(<>y__InlineArray9<object>);
			global::<PrivateImplementationDetails>.InlineArrayElementRef<<>y__InlineArray9<object>, object>(ref buffer, 0) = current.RealMerchant.BodySet;
			global::<PrivateImplementationDetails>.InlineArrayElementRef<<>y__InlineArray9<object>, object>(ref buffer, 1) = current.RealMerchant.HandSet;
			global::<PrivateImplementationDetails>.InlineArrayElementRef<<>y__InlineArray9<object>, object>(ref buffer, 2) = current.RealMerchant.HandVariant;
			global::<PrivateImplementationDetails>.InlineArrayElementRef<<>y__InlineArray9<object>, object>(ref buffer, 3) = (current.RealMerchant.ShowInventoryLeg ? "on" : "off");
			global::<PrivateImplementationDetails>.InlineArrayElementRef<<>y__InlineArray9<object>, object>(ref buffer, 4) = current.FakeMerchant.BodySet;
			global::<PrivateImplementationDetails>.InlineArrayElementRef<<>y__InlineArray9<object>, object>(ref buffer, 5) = current.FakeMerchant.HandSet;
			global::<PrivateImplementationDetails>.InlineArrayElementRef<<>y__InlineArray9<object>, object>(ref buffer, 6) = current.FakeMerchant.HandVariant;
			global::<PrivateImplementationDetails>.InlineArrayElementRef<<>y__InlineArray9<object>, object>(ref buffer, 7) = current.Voice.Language;
			global::<PrivateImplementationDetails>.InlineArrayElementRef<<>y__InlineArray9<object>, object>(ref buffer, 8) = current.Voice.VolumeDb;
			return string.Format("real[body={0}, handSet={1}, hand={2}, leg={3}] fake[body={4}, handSet={5}, hand={6}] voice[lang={7}, db={8:+0.##;-0.##;0}]", global::<PrivateImplementationDetails>.InlineArrayAsReadOnlySpan<<>y__InlineArray9<object>, object>(in buffer, 9));
		}
	}
	public static class MerchantSceneSkinner
	{
		[CompilerGenerated]
		private sealed class <Walk>d__8 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
		{
			private int <>1__state;

			private Node <>2__current;

			private int <>l__initialThreadId;

			private Node root;

			public Node <>3__root;

			private IEnumerator<Node> <>7__wrap1;

			private Node <child>5__3;

			private IEnumerator<Node> <>7__wrap3;

			Node IEnumerator<Node>.Current
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
			public <Walk>d__8(int <>1__state)
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
						break;
					case 1:
						<>1__state = -3;
						<>7__wrap3 = Walk(<child>5__3).GetEnumerator();
						<>1__state = -4;
						goto IL_00cf;
					case 2:
						{
							<>1__state = -4;
							goto IL_00cf;
						}
						IL_00cf:
						if (<>7__wrap3.MoveNext())
						{
							Node current = <>7__wrap3.Current;
							<>2__current = current;
							<>1__state = 2;
							return true;
						}
						<>m__Finally2();
						<>7__wrap3 = null;
						<child>5__3 = null;
						break;
					}
					while (<>7__wrap1.MoveNext())
					{
						<child>5__3 = <>7__wrap1.Current;
						if (<child>5__3 != null)
						{
							<>2__current = <child>5__3;
							<>1__state = 1;
							return true;
						}
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
			IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
			{
				<Walk>d__8 <Walk>d__;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					<Walk>d__ = this;
				}
				else
				{
					<Walk>d__ = new <Walk>d__8(0);
				}
				<Walk>d__.root = <>3__root;
				return <Walk>d__;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Node>)this).GetEnumerator();
			}
		}

		private static readonly Dictionary<string, Resource> ResourceCache = new Dictionary<string, Resource>();

		public static void WarmUp()
		{
			string[] array = SkeletonSetAssets.AllTresPaths();
			for (int i = 0; i < array.Length; i++)
			{
				Load(array[i]);
			}
		}

		public static int ApplyBeforeEnteringTree(Node? root)
		{
			if (root == null || !GodotObject.IsInstanceValid((GodotObject)(object)root))
			{
				return 0;
			}
			MerchantKind? merchantKind = DetectKind(root);
			if (!merchantKind.HasValue)
			{
				return 0;
			}
			MerchantSkinSettings merchantSkinSettings = SettingsHost.Current.For(merchantKind.Value);
			int num = 0;
			string target = SkeletonSetAssets.HandTresPath(merchantKind.Value, merchantSkinSettings.HandSet);
			string target2 = SkeletonSetAssets.BodyTresPath(merchantKind.Value, merchantSkinSettings.BodySet);
			num += ApplyToHandNodes(root, merchantKind.Value, target);
			num += ApplyToBodyNodes(root, merchantKind.Value, target2);
			if (num > 0)
			{
				GD.Print($"[Merchant2CuteII] scene skin applied: {SkeletonSetAssets.DescribePart(merchantKind.Value, "hand", merchantSkinSettings.HandSet)}, {SkeletonSetAssets.DescribePart(merchantKind.Value, "body", merchantSkinSettings.BodySet)}  ({num} node(s))");
			}
			return num;
		}

		private static MerchantKind? DetectKind(Node root)
		{
			if (root is NMerchantRoom)
			{
				return MerchantKind.Real;
			}
			if (root is NFakeMerchant)
			{
				return MerchantKind.Fake;
			}
			foreach (Node child in root.GetChildren(false))
			{
				MerchantKind? result = DetectKind(child);
				if (result.HasValue)
				{
					return result;
				}
			}
			return null;
		}

		private static int ApplyToHandNodes(Node root, MerchantKind kind, string target)
		{
			int num = 0;
			foreach (Node item in Walk(root))
			{
				NMerchantHand val = (NMerchantHand)(object)((item is NMerchantHand) ? item : null);
				if (val != null)
				{
					Node parent = ((Node)val).GetParent();
					if (parent != null && TrySetSkeletonData(parent, target, "hand"))
					{
						num++;
					}
				}
			}
			return num;
		}

		private static int ApplyToBodyNodes(Node root, MerchantKind kind, string target)
		{
			int num = 0;
			foreach (Node item in Walk(root))
			{
				NMerchantButton val = (NMerchantButton)(object)((item is NMerchantButton) ? item : null);
				if (val != null)
				{
					Node val2 = ((Node)val).GetNodeOrNull(NodePath.op_Implicit("%MerchantVisual")) ?? ((Node)val).GetNodeOrNull(NodePath.op_Implicit("MerchantVisual")) ?? ((Node)val).FindChild("MerchantVisual", true, false);
					if (val2 != null && TrySetSkeletonData(val2, target, "body"))
					{
						num++;
					}
				}
			}
			return num;
		}

		private static bool TrySetSkeletonData(Node spineNode, string targetPath, string part)
		{
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (spineNode.IsInsideTree() && spineNode.IsNodeReady())
				{
					GD.Print($"[Merchant2CuteII] skip {part} skeleton swap on '{spineNode.Name}': already ready in tree");
					return false;
				}
				Resource val = Load(targetPath);
				if (val == null)
				{
					GD.PrintErr("[Merchant2CuteII] skeleton resource missing: " + targetPath);
					return false;
				}
				Variant val2 = ((GodotObject)spineNode).Get(StringName.op_Implicit("skeleton_data_res"));
				GodotObject val3 = ((Variant)(ref val2)).AsGodotObject();
				if (val3 != null && val3.GetInstanceId() == ((GodotObject)val).GetInstanceId())
				{
					return false;
				}
				((GodotObject)spineNode).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)val));
				Variant val4 = ((GodotObject)spineNode).Get(StringName.op_Implicit("skeleton_data_res"));
				if (((Variant)(ref val4)).AsGodotObject() == null || ((Variant)(ref val4)).AsGodotObject().GetInstanceId() != ((GodotObject)val).GetInstanceId())
				{
					GD.PrintErr($"[Merchant2CuteII] failed to set skeleton_data_res on {part} node '{spineNode.Name}'");
					return false;
				}
				return true;
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[Merchant2CuteII] setting {part} skeleton failed on '{spineNode.Name}': {ex.Message}");
				return false;
			}
		}

		private static Resource? Load(string path)
		{
			if (ResourceCache.TryGetValue(path, out Resource value))
			{
				return value;
			}
			Resource val = ResourceLoader.Load<Resource>(path, (string)null, (CacheMode)1);
			if (val != null)
			{
				ResourceCache[path] = val;
			}
			return val;
		}

		[IteratorStateMachine(typeof(<Walk>d__8))]
		private static IEnumerable<Node> Walk(Node root)
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Walk>d__8(-2)
			{
				<>3__root = root
			};
		}
	}
	public static class MerchantSettingsEntry
	{
		public const string RowName = "Merchant2CuteIIRow";

		public const string DividerName = "Merchant2CuteIIDivider";

		public const string PageName = "Merchant2CuteIISettingsPage";

		private const string RowLabel = "「商人外观」";

		private const string ButtonLabel = "打开设置";

		public static void Inject(Node screen)
		{
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Expected O, but got Unknown
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			if (screen == null || !GodotObject.IsInstanceValid((GodotObject)(object)screen) || screen.FindChild("Merchant2CuteIIRow", true, false) != null)
			{
				return;
			}
			Control nodeOrNull = screen.GetNodeOrNull<Control>(NodePath.op_Implicit("%Modding"));
			Control nodeOrNull2 = screen.GetNodeOrNull<Control>(NodePath.op_Implicit("%ModdingDivider"));
			if (nodeOrNull == null || ((Node)nodeOrNull).GetParent() == null)
			{
				GD.PrintErr("[Merchant2CuteII] settings screen: %Modding row not found, skip entry injection");
				return;
			}
			try
			{
				Node parent = ((Node)nodeOrNull).GetParent();
				int num = ((Node)nodeOrNull).GetIndex(false);
				if (nodeOrNull2 != null && ((Node)nodeOrNull2).GetParent() == parent)
				{
					Control val = (Control)((Node)nodeOrNull2).Duplicate(15);
					((Node)val).Name = StringName.op_Implicit("Merchant2CuteIIDivider");
					((Node)val).UniqueNameInOwner = false;
					((CanvasItem)val).Visible = true;
					parent.AddChild((Node)(object)val, false, (InternalMode)0);
					parent.MoveChild((Node)(object)val, num + 1);
					num++;
				}
				Control val2 = (Control)((Node)nodeOrNull).Duplicate(15);
				((Node)val2).Name = StringName.op_Implicit("Merchant2CuteIIRow");
				((Node)val2).UniqueNameInOwner = false;
				((CanvasItem)val2).Visible = true;
				parent.AddChild((Node)(object)val2, false, (InternalMode)0);
				parent.MoveChild((Node)(object)val2, num + 1);
				SetRowText((Node)(object)val2, "「商人外观」");
				Node nodeOrNull3 = ((Node)val2).GetNodeOrNull(NodePath.op_Implicit("ModdingButton"));
				if (nodeOrNull3 == null)
				{
					GD.PrintErr("[Merchant2CuteII] settings row: copied button not found");
					return;
				}
				nodeOrNull3.UniqueNameInOwner = false;
				WireButton(nodeOrNull3, screen, (Node)(object)val2);
				GD.Print("[Merchant2CuteII] settings entry row injected");
			}
			catch (Exception value)
			{
				GD.PrintErr($"[Merchant2CuteII] settings entry injection failed: {value}");
			}
		}

		private static void OpenSettingsPage(Node screen)
		{
			try
			{
				object? obj = typeof(NSubmenu).GetField("_stack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(screen);
				NSubmenuStack val = (NSubmenuStack)((obj is NSubmenuStack) ? obj : null);
				if (val == null)
				{
					GD.PrintErr("[Merchant2CuteII] submenu stack not found, falling back to popup panel");
					SettingsPanel.Open(screen);
					return;
				}
				MerchantSettingsSubmenu merchantSettingsSubmenu = ((Node)val).GetNodeOrNull<MerchantSettingsSubmenu>(NodePath.op_Implicit("Merchant2CuteIISettingsPage"));
				if (merchantSettingsSubmenu == null || !GodotObject.IsInstanceValid((GodotObject)(object)merchantSettingsSubmenu))
				{
					MerchantSettingsSubmenu merchantSettingsSubmenu2 = new MerchantSettingsSubmenu();
					((Node)merchantSettingsSubmenu2).Name = StringName.op_Implicit("Merchant2CuteIISettingsPage");
					merchantSettingsSubmenu = merchantSettingsSubmenu2;
					((Node)val).AddChild((Node)(object)merchantSettingsSubmenu, false, (InternalMode)0);
				}
				if (val.Peek() != merchantSettingsSubmenu)
				{
					val.Push((NSubmenu)(object)merchantSettingsSubmenu);
				}
			}
			catch (Exception value)
			{
				GD.PrintErr($"[Merchant2CuteII] opening settings page failed: {value}");
				SettingsPanel.Open(screen);
			}
		}

		private static void SetRowText(Node row, string text)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			Node nodeOrNull = row.GetNodeOrNull(NodePath.op_Implicit("Label"));
			if (nodeOrNull != null)
			{
				RichTextLabel val = (RichTextLabel)(object)((nodeOrNull is RichTextLabel) ? nodeOrNull : null);
				if (val != null)
				{
					val.Text = text;
				}
				else
				{
					((GodotObject)nodeOrNull).Set(StringName.op_Implicit("text"), Variant.op_Implicit(text));
				}
			}
		}

		private static void WireButton(Node button, Node screen, Node row)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			Callable val3 = Callable.From((Action)delegate
			{
				//IL_0086: Unknown result type (might be due to invalid IL or missing references)
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0052: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					if (!GodotObject.IsInstanceValid((GodotObject)(object)button))
					{
						return;
					}
					Node nodeOrNull = button.GetNodeOrNull(NodePath.op_Implicit("Label"));
					if (nodeOrNull != null)
					{
						RichTextLabel val = (RichTextLabel)(object)((nodeOrNull is RichTextLabel) ? nodeOrNull : null);
						if (val != null)
						{
							val.Text = "打开设置";
						}
						else
						{
							((GodotObject)nodeOrNull).Set(StringName.op_Implicit("text"), Variant.op_Implicit("打开设置"));
						}
					}
					((GodotObject)button).Connect(SignalName.Released, Callable.From<NClickableControl>((Action<NClickableControl>)delegate
					{
						OpenSettingsPage(screen);
					}), 0u);
					try
					{
						Node obj = button;
						NClickableControl val2 = (NClickableControl)(object)((obj is NClickableControl) ? obj : null);
						if (val2 != null && !val2.IsEnabled)
						{
							val2.Enable();
						}
					}
					catch (Exception ex)
					{
						GD.Print("[Merchant2CuteII] settings button enable skipped: " + ex.Message);
					}
				}
				catch (Exception ex2)
				{
					GD.PrintErr("[Merchant2CuteII] settings row wiring failed: " + ex2.Message);
				}
			});
			((Callable)(ref val3)).CallDeferred(Array.Empty<Variant>());
		}
	}
	[ScriptPath("res://script/Settings/MerchantSettingsSubmenu.cs")]
	public class MerchantSettingsSubmenu : NSubmenu
	{
		public class MethodName : MethodName
		{
			public static readonly StringName _Ready = StringName.op_Implicit("_Ready");

			public static readonly StringName _ExitTree = StringName.op_Implicit("_ExitTree");

			public static readonly StringName OnSettingsChanged = StringName.op_Implicit("OnSettingsChanged");

			public static readonly StringName BuildBackButton = StringName.op_Implicit("BuildBackButton");

			public static readonly StringName BuildLayout = StringName.op_Implicit("BuildLayout");

			public static readonly StringName BuildRows = StringName.op_Implicit("BuildRows");
		}

		public class PropertyName : PropertyName
		{
			public static readonly StringName InitialFocusedControl = StringName.op_Implicit("InitialFocusedControl");

			public static readonly StringName _content = StringName.op_Implicit("_content");
		}

		public class SignalName : SignalName
		{
		}

		private const string BackButtonScene = "res://scenes/ui/back_button.tscn";

		private VBoxContainer? _content;

		private readonly List<Action> _refreshers = new List<Action>();

		protected override Control? InitialFocusedControl => null;

		public override void _Ready()
		{
			BuildBackButton();
			BuildLayout();
			BuildRows();
			((NSubmenu)this).ConnectSignals();
			((CanvasItem)this).Visible = false;
			((Control)this).MouseFilter = (MouseFilterEnum)2;
			SettingsHost.Changed += OnSettingsChanged;
		}

		public override void _ExitTree()
		{
			SettingsHost.Changed -= OnSettingsChanged;
		}

		private void OnSettingsChanged()
		{
			foreach (Action refresher in _refreshers)
			{
				try
				{
					refresher();
				}
				catch (Exception ex)
				{
					GD.PrintErr("[Merchant2CuteII] settings page refresh failed: " + ex.Message);
				}
			}
		}

		private void BuildBackButton()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			PackedScene val = ResourceLoader.Load<PackedScene>("res://scenes/ui/back_button.tscn", (string)null, (CacheMode)1);
			if (val == null)
			{
				GD.PrintErr("[Merchant2CuteII] back button scene missing: res://scenes/ui/back_button.tscn");
				((Node)this).AddChild((Node)new Button
				{
					Name = StringName.op_Implicit("BackButton")
				}, false, (InternalMode)0);
			}
			else
			{
				Node val2 = val.Instantiate((GenEditState)0);
				val2.Name = StringName.op_Implicit("BackButton");
				((Node)this).AddChild(val2, false, (InternalMode)0);
			}
		}

		private void BuildLayout()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Expected O, but got Unknown
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Expected O, but got Unknown
			((Control)this).AnchorRight = 1f;
			((Control)this).AnchorBottom = 1f;
			MarginContainer val = new MarginContainer
			{
				MouseFilter = (MouseFilterEnum)2
			};
			((Control)val).SetAnchorsPreset((LayoutPreset)15, false);
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_left"), 200);
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), 200);
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_top"), 72);
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_bottom"), 56);
			ScrollContainer val2 = new ScrollContainer
			{
				SizeFlagsHorizontal = (SizeFlags)3,
				SizeFlagsVertical = (SizeFlags)3,
				HorizontalScrollMode = (ScrollMode)0
			};
			_content = new VBoxContainer
			{
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)_content).AddThemeConstantOverride(StringName.op_Implicit("separation"), 6);
			((Node)val2).AddChild((Node)(object)_content, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)this).AddChild((Node)(object)val, false, (InternalMode)0);
		}

		private void BuildRows()
		{
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Expected O, but got Unknown
			if (_content != null)
			{
				_refreshers.Clear();
				SettingsRows.AddTitle(_content, "商人外观设置");
				SettingsRows.AddSection(_content, SettingsText.Section("real"), SettingsText.Hint("real"));
				SettingsRows.AddMerchantRows(_content, MerchantKind.Real, _refreshers);
				SettingsRows.AddSection(_content, SettingsText.Section("fake"), SettingsText.Hint("fake"));
				SettingsRows.AddMerchantRows(_content, MerchantKind.Fake, _refreshers);
				SettingsRows.AddSection(_content, SettingsText.Section("voice"), SettingsText.Hint("voice"));
				SettingsRows.AddVoiceRows(_content, _refreshers);
				((Node)_content).AddChild((Node)new Control
				{
					CustomMinimumSize = new Vector2(0f, 24f)
				}, false, (InternalMode)0);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static List<MethodInfo> GetGodotMethodList()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			return new List<MethodInfo>(6)
			{
				new MethodInfo(MethodName._Ready, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
				new MethodInfo(MethodName._ExitTree, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
				new MethodInfo(MethodName.OnSettingsChanged, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
				new MethodInfo(MethodName.BuildBackButton, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
				new MethodInfo(MethodName.BuildLayout, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
				new MethodInfo(MethodName.BuildRows, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null)
			};
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			if ((ref method) == MethodName._Ready && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				((Node)this)._Ready();
				ret = default(godot_variant);
				return true;
			}
			if ((ref method) == MethodName._ExitTree && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				((Node)this)._ExitTree();
				ret = default(godot_variant);
				return true;
			}
			if ((ref method) == MethodName.OnSettingsChanged && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				OnSettingsChanged();
				ret = default(godot_variant);
				return true;
			}
			if ((ref method) == MethodName.BuildBackButton && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				BuildBackButton();
				ret = default(godot_variant);
				return true;
			}
			if ((ref method) == MethodName.BuildLayout && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				BuildLayout();
				ret = default(godot_variant);
				return true;
			}
			if ((ref method) == MethodName.BuildRows && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				BuildRows();
				ret = default(godot_variant);
				return true;
			}
			return ((NSubmenu)this).InvokeGodotClassMethod(ref method, args, ref ret);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool HasGodotClassMethod(in godot_string_name method)
		{
			if ((ref method) == MethodName._Ready)
			{
				return true;
			}
			if ((ref method) == MethodName._ExitTree)
			{
				return true;
			}
			if ((ref method) == MethodName.OnSettingsChanged)
			{
				return true;
			}
			if ((ref method) == MethodName.BuildBackButton)
			{
				return true;
			}
			if ((ref method) == MethodName.BuildLayout)
			{
				return true;
			}
			if ((ref method) == MethodName.BuildRows)
			{
				return true;
			}
			return ((NSubmenu)this).HasGodotClassMethod(ref method);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
		{
			if ((ref name) == PropertyName._content)
			{
				_content = VariantUtils.ConvertTo<VBoxContainer>(ref value);
				return true;
			}
			return ((NSubmenu)this).SetGodotClassPropertyValue(ref name, ref value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			if ((ref name) == PropertyName.InitialFocusedControl)
			{
				Control initialFocusedControl = ((NSubmenu)this).InitialFocusedControl;
				value = VariantUtils.CreateFrom<Control>(ref initialFocusedControl);
				return true;
			}
			if ((ref name) == PropertyName._content)
			{
				value = VariantUtils.CreateFrom<VBoxContainer>(ref _content);
				return true;
			}
			return ((NSubmenu)this).GetGodotClassPropertyValue(ref name, ref value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static List<PropertyInfo> GetGodotPropertyList()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			return new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, PropertyName._content, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
				new PropertyInfo((Type)24, PropertyName.InitialFocusedControl, (PropertyHint)0, "", (PropertyUsageFlags)4096, false)
			};
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void SaveGodotObjectData(GodotSerializationInfo info)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			((NSubmenu)this).SaveGodotObjectData(info);
			info.AddProperty(PropertyName._content, Variant.From<VBoxContainer>(ref _content));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			((NSubmenu)this).RestoreGodotObjectData(info);
			Variant val = default(Variant);
			if (info.TryGetProperty(PropertyName._content, ref val))
			{
				_content = ((Variant)(ref val)).As<VBoxContainer>();
			}
		}
	}
	public static class MerchantVisualMotion
	{
		private static readonly Dictionary<ulong, Vector2> BasePositions = new Dictionary<ulong, Vector2>();

		private static readonly Dictionary<ulong, string> LoggedKeys = new Dictionary<ulong, string>();

		private static readonly Dictionary<string, Vector2> OffsetOverrides = new Dictionary<string, Vector2>();

		private static readonly Dictionary<string, float> ScaleOverrides = new Dictionary<string, float>();

		private const string TuningPath = "user://merchant2cute_visual.json";

		public static string Key(MerchantKind kind, MerchantSkinSettings skin)
		{
			return ((kind == MerchantKind.Fake) ? "fake" : "real") + "/" + SkeletonSets.Normalize(skin.BodySet);
		}

		public static void Apply(NMerchantButton button, Node2D visual)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			if (button != null && visual != null && GodotObject.IsInstanceValid((GodotObject)(object)visual))
			{
				MerchantKind valueOrDefault = SkinApplier.ResolveKind((Node?)(object)button).GetValueOrDefault();
				string text = Key(valueOrDefault, SettingsHost.Current.For(valueOrDefault));
				ulong instanceId = ((GodotObject)visual).GetInstanceId();
				if (!BasePositions.TryGetValue(instanceId, out var value))
				{
					value = visual.Position;
					BasePositions[instanceId] = value;
				}
				Vector2 offset = GetOffset(text);
				float scale = GetScale(text);
				visual.Position = value + offset;
				visual.Scale = new Vector2(scale, scale);
				if (!LoggedKeys.TryGetValue(instanceId, out string value2) || value2 != text)
				{
					LoggedKeys[instanceId] = text;
					GD.Print($"[Merchant2CuteII] visual {text} on {((Node)visual).GetPath()}: offset=({offset.X:0.#},{offset.Y:0.#}) scale={scale:0.###}");
				}
			}
		}

		public static Vector2 GetOffset(string key)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			if (OffsetOverrides.TryGetValue(key, out var value))
			{
				return value;
			}
			return Defaults(key).Offset;
		}

		public static float GetScale(string key)
		{
			if (ScaleOverrides.TryGetValue(key, out var value))
			{
				return value;
			}
			return Defaults(key).Scale;
		}

		private static (Vector2 Offset, float Scale) Defaults(string key)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			return key switch
			{
				"real/own" => (Offset: ModConfig.Merchant.VisualPositionOffset, Scale: ModConfig.Merchant.VisualScale.X), 
				"real/other" => (Offset: ModConfig.Merchant.VisualPositionOffset + new Vector2(0f, 40f), Scale: 0.32f), 
				"fake/own" => (Offset: ModConfig.FakeMerchant.VisualPositionOffset, Scale: ModConfig.FakeMerchant.VisualScale.X), 
				"fake/other" => (Offset: ModConfig.FakeMerchant.VisualPositionOffset + new Vector2(0f, -30f), Scale: 0.22f), 
				_ => (Offset: ModConfig.Merchant.VisualPositionOffset, Scale: ModConfig.Merchant.VisualScale.X), 
			};
		}

		public static string ProbeScene()
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (((val != null) ? val.Root : null) == null)
			{
				return "no scene tree";
			}
			List<string> list = new List<string>();
			WalkWithReport((Node)(object)val.Root, list);
			if (list.Count != 0)
			{
				return string.Join(" | ", list);
			}
			return "no merchant visual found in current scene";
		}

		private static void WalkWithReport(Node node, List<string> lines)
		{
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			foreach (Node child in node.GetChildren(false))
			{
				if (child == null)
				{
					continue;
				}
				NMerchantButton val = (NMerchantButton)(object)((child is NMerchantButton) ? child : null);
				if (val != null)
				{
					Node val2 = ((Node)val).GetNodeOrNull(NodePath.op_Implicit("%MerchantVisual")) ?? ((Node)val).GetNodeOrNull(NodePath.op_Implicit("MerchantVisual")) ?? ((Node)val).FindChild("MerchantVisual", true, false);
					Node2D val3 = (Node2D)(object)((val2 is Node2D) ? val2 : null);
					if (val3 != null)
					{
						MerchantKind valueOrDefault = SkinApplier.ResolveKind((Node?)(object)val).GetValueOrDefault();
						string text = Key(valueOrDefault, SettingsHost.Current.For(valueOrDefault));
						Vector2 offset = GetOffset(text);
						float scale = GetScale(text);
						lines.Add($"{((Node)val).GetPath()}: key={text} offset=({offset.X:0.#},{offset.Y:0.#}) scale={scale:0.###} pos=({val3.Position.X:0.#},{val3.Position.Y:0.#})");
					}
					else if (val2 != null)
					{
						lines.Add($"{((Node)val).GetPath()}: visual found but not Node2D ({((object)val2).GetType().Name})");
					}
					else
					{
						lines.Add($"{((Node)val).GetPath()}: MerchantVisual NOT found");
					}
				}
				WalkWithReport(child, lines);
			}
		}

		public static void Load()
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Invalid comparison between Unknown and I8
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (!FileAccess.FileExists("user://merchant2cute_visual.json"))
				{
					return;
				}
				FileAccess val = FileAccess.Open("user://merchant2cute_visual.json", (ModeFlags)1);
				try
				{
					if (val == null)
					{
						return;
					}
					Variant val2 = Json.ParseString(val.GetAsText(false));
					if ((long)((Variant)(ref val2)).VariantType != 27)
					{
						return;
					}
					Dictionary val3 = ((Variant)(ref val2)).AsGodotDictionary();
					foreach (Variant key2 in val3.Keys)
					{
						Variant current = key2;
						string key = ((Variant)(ref current)).AsString();
						Variant val4 = val3[current];
						Dictionary val5 = ((Variant)(ref val4)).AsGodotDictionary();
						if (val5.ContainsKey(Variant.op_Implicit("x")) && val5.ContainsKey(Variant.op_Implicit("y")))
						{
							Dictionary<string, Vector2> offsetOverrides = OffsetOverrides;
							val4 = val5[Variant.op_Implicit("x")];
							float num = (float)((Variant)(ref val4)).AsDouble();
							val4 = val5[Variant.op_Implicit("y")];
							offsetOverrides[key] = new Vector2(num, (float)((Variant)(ref val4)).AsDouble());
						}
						if (val5.ContainsKey(Variant.op_Implicit("scale")))
						{
							Dictionary<string, float> scaleOverrides = ScaleOverrides;
							val4 = val5[Variant.op_Implicit("scale")];
							scaleOverrides[key] = (float)((Variant)(ref val4)).AsDouble();
						}
					}
					GD.Print("[Merchant2CuteII] visual tuning loaded: " + Describe());
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] tuning load failed: " + ex.Message);
			}
		}

		public static void Save()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Expected O, but got Unknown
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Dictionary val = new Dictionary();
				string[] array = new string[4] { "real/own", "real/other", "fake/own", "fake/other" };
				foreach (string text in array)
				{
					bool num = OffsetOverrides.ContainsKey(text);
					bool flag = ScaleOverrides.ContainsKey(text);
					if (num || flag)
					{
						Vector2 offset = GetOffset(text);
						Dictionary val5 = new Dictionary
						{
							[Variant.op_Implicit("x")] = Variant.op_Implicit(offset.X),
							[Variant.op_Implicit("y")] = Variant.op_Implicit(offset.Y),
							[Variant.op_Implicit("scale")] = Variant.op_Implicit(GetScale(text))
						};
						val[Variant.op_Implicit(text)] = Variant.op_Implicit(val5);
					}
				}
				FileAccess val6 = FileAccess.Open("user://merchant2cute_visual.json", (ModeFlags)2);
				try
				{
					if (val6 != null)
					{
						val6.StoreString(Json.Stringify(Variant.op_Implicit(val), "  ", true, false));
					}
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] tuning save failed: " + ex.Message);
			}
		}

		public static void AddOffsetDelta(string key, Vector2 delta)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			OffsetOverrides[key] = GetOffset(key) + delta;
		}

		public static void SetOffset(string key, Vector2 value)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			OffsetOverrides[key] = value;
		}

		public static void SetScale(string key, float value)
		{
			ScaleOverrides[key] = Mathf.Clamp(value, 0.02f, 3f);
		}

		public static void Reset(string? key = null)
		{
			if (key == null)
			{
				OffsetOverrides.Clear();
				ScaleOverrides.Clear();
			}
			else
			{
				OffsetOverrides.Remove(key);
				ScaleOverrides.Remove(key);
			}
		}

		public static string Describe()
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			string[] obj = new string[4] { "real/own", "real/other", "fake/own", "fake/other" };
			List<string> list = new List<string>();
			string[] array = obj;
			foreach (string text in array)
			{
				Vector2 offset = GetOffset(text);
				float scale = GetScale(text);
				bool flag = OffsetOverrides.ContainsKey(text) || ScaleOverrides.ContainsKey(text);
				list.Add($"{text}: offset=({offset.X:0.#},{offset.Y:0.#}) scale={scale:0.###}{(flag ? " *" : "")}");
			}
			return string.Join(" | ", list) + "   (* = 已手动微调)";
		}

		public static void Prune()
		{
			BasePositions.Clear();
		}

		public static void ReapplyAll()
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (((val != null) ? val.Root : null) != null)
			{
				Walk((Node)(object)val.Root);
			}
		}

		private static void Walk(Node node)
		{
			foreach (Node child in node.GetChildren(false))
			{
				if (child == null)
				{
					continue;
				}
				NMerchantButton val = (NMerchantButton)(object)((child is NMerchantButton) ? child : null);
				if (val != null)
				{
					Node obj = ((Node)val).GetNodeOrNull(NodePath.op_Implicit("%MerchantVisual")) ?? ((Node)val).GetNodeOrNull(NodePath.op_Implicit("MerchantVisual")) ?? ((Node)val).FindChild("MerchantVisual", true, false);
					Node2D val2 = (Node2D)(object)((obj is Node2D) ? obj : null);
					if (val2 != null)
					{
						Apply(val, val2);
					}
				}
				Walk(child);
			}
		}
	}
	public static class SettingsHost
	{
		private static ModSettings _current = new ModSettings();

		public static ModSettings Current => _current;

		public static event Action? Changed;

		public static void Load()
		{
			_current = SettingsStore.Load();
		}

		public static bool Save()
		{
			return SettingsStore.Save(_current);
		}

		public static void SaveAndNotify()
		{
			_current.Normalize();
			Save();
			NotifyChanged();
		}

		public static void NotifyChanged()
		{
			SettingsHost.Changed?.Invoke();
		}

		public static void Mutate(Action<ModSettings> mutate, bool save = true)
		{
			mutate(_current);
			_current.Normalize();
			if (save)
			{
				Save();
			}
			NotifyChanged();
		}
	}
	public static class SettingsJson
	{
		public const string LegacyKeyHandVariant = "HandVariant";

		public const string LegacyKeyVoiceVariant = "MerchantVoiceVariant";

		public const string LegacyKeyExtraDb = "ExtraDb";

		private const string LegacySkinSetKey = "skeletonSet";

		private static readonly JsonSerializerOptions WriteOptions = new JsonSerializerOptions
		{
			WriteIndented = true
		};

		public static ModSettings Parse(string? json, out bool wasLegacy)
		{
			wasLegacy = false;
			ModSettings modSettings = new ModSettings();
			if (string.IsNullOrWhiteSpace(json))
			{
				return modSettings;
			}
			JsonObject jsonObject;
			try
			{
				JsonDocumentOptions documentOptions = new JsonDocumentOptions
				{
					AllowTrailingCommas = true,
					CommentHandling = JsonCommentHandling.Skip
				};
				jsonObject = JsonNode.Parse(json, null, documentOptions) as JsonObject;
			}
			catch
			{
				return modSettings;
			}
			if (jsonObject == null)
			{
				return modSettings;
			}
			int num = ReadInt(jsonObject, "schema") ?? 1;
			bool flag = jsonObject.ContainsKey("realMerchant") || jsonObject.ContainsKey("fakeMerchant") || jsonObject.ContainsKey("voice");
			if (flag)
			{
				modSettings.RealMerchant = ReadSkin(jsonObject["realMerchant"] as JsonObject, MerchantKind.Real);
				modSettings.FakeMerchant = ReadSkin(jsonObject["fakeMerchant"] as JsonObject, MerchantKind.Fake);
				modSettings.Voice = ReadVoice(jsonObject["voice"] as JsonObject, modSettings.Voice);
			}
			if (num < 2 || !flag)
			{
				wasLegacy = true;
			}
			ApplyLegacyMigration(jsonObject, modSettings, flag);
			modSettings.Schema = 2;
			modSettings.Normalize();
			return modSettings;
		}

		public static string Serialize(ModSettings settings)
		{
			settings.Normalize();
			return new JsonObject((JsonNodeOptions?)null)
			{
				["schema"] = 2,
				["realMerchant"] = WriteSkin(settings.RealMerchant),
				["fakeMerchant"] = WriteSkin(settings.FakeMerchant),
				["voice"] = WriteVoice(settings.Voice),
				["HandVariant"] = settings.RealMerchant.HandVariant,
				["MerchantVoiceVariant"] = settings.Voice.Language,
				["ExtraDb"] = Math.Round(settings.Voice.VolumeDb, 2)
			}.ToJsonString(WriteOptions);
		}

		private static void ApplyLegacyMigration(JsonObject root, ModSettings settings, bool hasNewShape)
		{
			string text = ReadString(root, "HandVariant");
			if (text != null)
			{
				string value = HandVariants.Normalize(text);
				if (!hasNewShape)
				{
					settings.RealMerchant.BodySet = "own";
					settings.RealMerchant.HandSet = "own";
					settings.RealMerchant.HandVariant = HandVariants.NormalizeFor(MerchantKind.Real, "own", value);
					settings.RealMerchant.ShowInventoryLeg = true;
					settings.FakeMerchant.BodySet = "own";
					settings.FakeMerchant.HandSet = "own";
					settings.FakeMerchant.HandVariant = HandVariants.NormalizeFor(MerchantKind.Fake, "own", value);
				}
				else if (settings.RealMerchant.HandVariant == settings.FakeMerchant.HandVariant)
				{
					settings.RealMerchant.HandVariant = HandVariants.NormalizeFor(MerchantKind.Real, settings.RealMerchant.HandSet, value);
					settings.FakeMerchant.HandVariant = HandVariants.NormalizeFor(MerchantKind.Fake, settings.FakeMerchant.HandSet, value);
				}
			}
			string text2 = ReadString(root, "MerchantVoiceVariant");
			if (text2 != null && settings.Voice.Language == "default")
			{
				settings.Voice.Language = VoiceLanguages.Normalize(text2);
			}
			float? num = ReadFloat(root, "ExtraDb");
			if (num.HasValue && Math.Abs(settings.Voice.VolumeDb) < float.Epsilon)
			{
				settings.Voice.VolumeDb = num.Value;
			}
		}

		public static bool IsFootLike(string handVariant)
		{
			switch (HandVariants.Normalize(handVariant))
			{
			case "foot":
			case "white":
			case "black":
				return true;
			default:
				return false;
			}
		}

		public static bool CanToggleInventoryLeg(MerchantSkinSettings skin, MerchantKind kind)
		{
			if (UsesOwnArt(skin, kind))
			{
				return !IsFootLike(skin.HandVariant);
			}
			return false;
		}

		public static bool ShouldShowInventoryLeg(MerchantSkinSettings skin, MerchantKind kind)
		{
			if (!skin.ShowInventoryLeg)
			{
				return false;
			}
			if (IsFootLike(skin.HandVariant))
			{
				return false;
			}
			if (SkeletonSetAssets.IsFakeBodyArt(kind, skin.BodySet))
			{
				return false;
			}
			if (SkeletonSetAssets.IsFakeHandArt(kind, skin.HandSet))
			{
				return false;
			}
			return true;
		}

		public static bool UsesOwnArt(MerchantSkinSettings skin, MerchantKind kind)
		{
			if (!SkeletonSetAssets.IsFakeBodyArt(kind, skin.BodySet))
			{
				return !SkeletonSetAssets.IsFakeHandArt(kind, skin.HandSet);
			}
			return false;
		}

		private static MerchantSkinSettings ReadSkin(JsonObject? node, MerchantKind kind)
		{
			MerchantSkinSettings merchantSkinSettings = new MerchantSkinSettings();
			if (node == null)
			{
				return merchantSkinSettings;
			}
			string text = ReadString(node, "skeletonSet");
			merchantSkinSettings.BodySet = SkeletonSets.Normalize(ReadString(node, "bodySet") ?? text);
			merchantSkinSettings.HandSet = SkeletonSets.Normalize(ReadString(node, "handSet") ?? text);
			merchantSkinSettings.HandVariant = HandVariants.NormalizeFor(kind, merchantSkinSettings.HandSet, ReadString(node, "handVariant") ?? "hand");
			merchantSkinSettings.ShowInventoryLeg = ReadBool(node, "showInventoryLeg") ?? true;
			return merchantSkinSettings;
		}

		private static VoiceSettings ReadVoice(JsonObject? node, VoiceSettings fallback)
		{
			if (node == null)
			{
				return fallback;
			}
			return new VoiceSettings
			{
				Language = (ReadString(node, "language") ?? fallback.Language),
				VolumeDb = (ReadFloat(node, "volumeDb") ?? fallback.VolumeDb)
			};
		}

		private static JsonObject WriteSkin(MerchantSkinSettings skin)
		{
			return new JsonObject((JsonNodeOptions?)null)
			{
				["bodySet"] = skin.BodySet,
				["handSet"] = skin.HandSet,
				["handVariant"] = skin.HandVariant,
				["showInventoryLeg"] = skin.ShowInventoryLeg
			};
		}

		private static JsonObject WriteVoice(VoiceSettings voice)
		{
			return new JsonObject((JsonNodeOptions?)null)
			{
				["language"] = voice.Language,
				["volumeDb"] = Math.Round(voice.VolumeDb, 2)
			};
		}

		private static JsonNode? Find(JsonObject node, string key)
		{
			if (node.TryGetPropertyValue(key, out JsonNode jsonNode))
			{
				return jsonNode;
			}
			foreach (KeyValuePair<string, JsonNode> item in node)
			{
				if (string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))
				{
					return item.Value;
				}
			}
			return null;
		}

		private static string? ReadString(JsonObject node, string key)
		{
			try
			{
				string text = Find(node, key)?.GetValue<string>();
				return string.IsNullOrWhiteSpace(text) ? null : text;
			}
			catch
			{
				return null;
			}
		}

		private static int? ReadInt(JsonObject node, string key)
		{
			try
			{
				return Find(node, key)?.GetValue<int>();
			}
			catch
			{
				try
				{
					return (int)(Find(node, key)?.GetValue<double>() ?? 0.0);
				}
				catch
				{
					return null;
				}
			}
		}

		private static float? ReadFloat(JsonObject node, string key)
		{
			JsonNode jsonNode = Find(node, key);
			if (jsonNode == null)
			{
				return null;
			}
			try
			{
				return jsonNode.GetValue<float>();
			}
			catch
			{
			}
			try
			{
				return (float)jsonNode.GetValue<double>();
			}
			catch
			{
			}
			try
			{
				string value = jsonNode.GetValue<string>();
				if (value != null && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					return result;
				}
			}
			catch
			{
			}
			return null;
		}

		private static bool? ReadBool(JsonObject node, string key)
		{
			try
			{
				return Find(node, key)?.GetValue<bool>();
			}
			catch
			{
				return null;
			}
		}
	}
	public static class HandVariants
	{
		public const string Hand = "hand";

		public const string Foot = "foot";

		public const string White = "white";

		public const string Black = "black";

		public static readonly string[] FullSet = new string[4] { "hand", "foot", "white", "black" };

		public static readonly string[] HandAndFoot = new string[2] { "hand", "foot" };

		public static string Normalize(string? value)
		{
			return (value ?? string.Empty).Trim().ToLowerInvariant() switch
			{
				"foot" => "foot", 
				"white" => "white", 
				"black" => "black", 
				_ => "hand", 
			};
		}

		public static bool IsKnown(string? value)
		{
			if (value == null)
			{
				return false;
			}
			switch (value.Trim().ToLowerInvariant())
			{
			case "hand":
			case "foot":
			case "white":
			case "black":
				return true;
			default:
				return false;
			}
		}

		public static string ToAnimationName(string variant)
		{
			if (!(variant == "hand"))
			{
				return variant;
			}
			return "default";
		}

		public static string[] AnimationFallbackChain(string variant)
		{
			string text = ToAnimationName(variant);
			switch (variant)
			{
			case "white":
			case "black":
				return new string[3] { text, "foot", "default" };
			case "foot":
				return new string[2] { "foot", "default" };
			default:
				return new string[1] { "default" };
			}
		}

		public static string DisplayName(string variant)
		{
			return variant switch
			{
				"foot" => "Foot", 
				"white" => "White", 
				"black" => "Black", 
				_ => "Hand", 
			};
		}

		public static string[] Allowed(MerchantKind kind, string handSet)
		{
			if (!SkeletonSetAssets.IsFakeHandArt(kind, handSet))
			{
				return FullSet;
			}
			return HandAndFoot;
		}

		public static bool IsAllowed(MerchantKind kind, string handSet, string? value)
		{
			return Array.IndexOf(Allowed(kind, handSet), Normalize(value)) >= 0;
		}

		public static string NormalizeFor(MerchantKind kind, string handSet, string? value)
		{
			string text = Normalize(value);
			if (Array.IndexOf(Allowed(kind, handSet), text) < 0)
			{
				return "foot";
			}
			return text;
		}

		public static string NextFor(MerchantKind kind, string handSet, string current)
		{
			string[] array = Allowed(kind, handSet);
			int num = Array.IndexOf(array, NormalizeFor(kind, handSet, current));
			return array[(num + 1 + array.Length) % array.Length];
		}
	}
	public static class SkeletonSets
	{
		public const string Own = "own";

		public const string Other = "other";

		public static readonly string[] All = new string[2] { "own", "other" };

		public static string Normalize(string? value)
		{
			bool flag;
			switch ((value ?? string.Empty).Trim().ToLowerInvariant())
			{
			case "other":
			case "fake":
			case "real":
			case "cross":
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (!flag)
			{
				return "own";
			}
			return "other";
		}

		public static string DisplayName(string set)
		{
			if (!(Normalize(set) == "other"))
			{
				return "Own";
			}
			return "Other";
		}

		public static string Next(string set)
		{
			if (!(Normalize(set) == "other"))
			{
				return "other";
			}
			return "own";
		}

		public static string[] AllowedBodySets(MerchantKind kind)
		{
			if (kind == MerchantKind.Fake)
			{
				return new string[1] { "own" };
			}
			return new string[2] { "own", "other" };
		}
	}
	public static class VoiceLanguages
	{
		public const string Default = "default";

		public const string Japanese = "jp";

		public const string Chinese = "zh";

		public static readonly string[] All = new string[3] { "default", "jp", "zh" };

		public static string Normalize(string? value)
		{
			switch ((value ?? string.Empty).Trim().ToLowerInvariant())
			{
			case "jp":
			case "ja":
				return "jp";
			case "zh":
			case "cn":
			case "zho":
				return "zh";
			default:
				return "default";
			}
		}
	}
	public enum MerchantKind
	{
		Real,
		Fake
	}
	public sealed class MerchantSkinSettings
	{
		public string BodySet { get; set; } = "own";

		public string HandSet { get; set; } = "own";

		public string HandVariant { get; set; } = "hand";

		public bool ShowInventoryLeg { get; set; } = true;

		public MerchantSkinSettings Clone()
		{
			return new MerchantSkinSettings
			{
				BodySet = BodySet,
				HandSet = HandSet,
				HandVariant = HandVariant,
				ShowInventoryLeg = ShowInventoryLeg
			};
		}

		public void Normalize(MerchantKind kind)
		{
			BodySet = SkeletonSets.Normalize(BodySet);
			if (kind == MerchantKind.Fake)
			{
				BodySet = "own";
			}
			HandSet = SkeletonSets.Normalize(HandSet);
			HandVariant = HandVariants.NormalizeFor(kind, HandSet, HandVariant);
		}
	}
	public sealed class VoiceSettings
	{
		public string Language { get; set; } = "default";

		public float VolumeDb { get; set; }

		public float LinearGain => (float)Math.Pow(10.0, (double)VolumeDb / 20.0);

		public void Normalize()
		{
			Language = VoiceLanguages.Normalize(Language);
			VolumeDb = Math.Clamp(VolumeDb, -12f, 12f);
		}

		public VoiceSettings Clone()
		{
			return new VoiceSettings
			{
				Language = Language,
				VolumeDb = VolumeDb
			};
		}
	}
	public sealed class ModSettings
	{
		public const int CurrentSchema = 2;

		public int Schema { get; set; } = 2;

		public MerchantSkinSettings RealMerchant { get; set; } = new MerchantSkinSettings();

		public MerchantSkinSettings FakeMerchant { get; set; } = new MerchantSkinSettings();

		public VoiceSettings Voice { get; set; } = new VoiceSettings();

		public MerchantSkinSettings For(MerchantKind kind)
		{
			if (kind != MerchantKind.Fake)
			{
				return RealMerchant;
			}
			return FakeMerchant;
		}

		public ModSettings Clone()
		{
			return new ModSettings
			{
				Schema = Schema,
				RealMerchant = RealMerchant.Clone(),
				FakeMerchant = FakeMerchant.Clone(),
				Voice = Voice.Clone()
			};
		}

		public void Normalize()
		{
			RealMerchant.Normalize(MerchantKind.Real);
			FakeMerchant.Normalize(MerchantKind.Fake);
			Voice.Normalize();
		}
	}
	public enum HandVariantOption
	{
		Hand,
		Foot,
		White,
		Black
	}
	public enum SkeletonSetOption
	{
		ShopArt,
		EventArt
	}
	public enum VoiceLanguageOption
	{
		Vanilla,
		Japanese,
		Chinese
	}
	public static class SettingsOptions
	{
		public static string ToValue(HandVariantOption option)
		{
			return option switch
			{
				HandVariantOption.Foot => "foot", 
				HandVariantOption.White => "white", 
				HandVariantOption.Black => "black", 
				_ => "hand", 
			};
		}

		public static HandVariantOption ToHandOption(string? value)
		{
			return HandVariants.Normalize(value) switch
			{
				"foot" => HandVariantOption.Foot, 
				"white" => HandVariantOption.White, 
				"black" => HandVariantOption.Black, 
				_ => HandVariantOption.Hand, 
			};
		}

		public static string ToValue(SkeletonSetOption option)
		{
			if (option != SkeletonSetOption.EventArt)
			{
				return "own";
			}
			return "other";
		}

		public static SkeletonSetOption ToSetOption(string? value)
		{
			if (!(SkeletonSets.Normalize(value) == "other"))
			{
				return SkeletonSetOption.ShopArt;
			}
			return SkeletonSetOption.EventArt;
		}

		public static string ToValue(VoiceLanguageOption option)
		{
			return option switch
			{
				VoiceLanguageOption.Japanese => "jp", 
				VoiceLanguageOption.Chinese => "zh", 
				_ => "default", 
			};
		}

		public static VoiceLanguageOption ToLanguageOption(string? value)
		{
			string text = VoiceLanguages.Normalize(value);
			if (!(text == "jp"))
			{
				if (text == "zh")
				{
					return VoiceLanguageOption.Chinese;
				}
				return VoiceLanguageOption.Vanilla;
			}
			return VoiceLanguageOption.Japanese;
		}
	}
	public static class SettingsOptionLabels
	{
		public static string Hand(HandVariantOption option)
		{
			return option switch
			{
				HandVariantOption.Foot => "Foot / 脚", 
				HandVariantOption.White => "White / 白（需真商人手部美术）", 
				HandVariantOption.Black => "Black / 黑（需真商人手部美术）", 
				_ => "Hand / 手", 
			};
		}

		public static string Set(MerchantKind kind, string part, SkeletonSetOption option)
		{
			string text = SettingsOptions.ToValue(option);
			return SettingsText.ArtOwner((part == "body") ? SkeletonSetAssets.IsFakeBodyArt(kind, text) : SkeletonSetAssets.IsFakeHandArt(kind, text));
		}
	}
	public static class SettingsPanel
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class <>c
		{
			public static readonly <>c <>9 = new <>c();

			public static Action <>9__4_0;

			public static Func<string, string> <>9__9_9;

			public static Func<string[]> <>9__10_0;

			public static Func<string, string> <>9__10_1;

			public static Func<string> <>9__10_2;

			public static Action<string> <>9__10_3;

			public static ToggledEventHandler <>9__13_0;

			internal void <Open>b__4_0()
			{
				Refreshers.Clear();
			}

			internal string <AddMerchantRows>b__9_9(string v)
			{
				return SettingsText.Option(v);
			}

			internal string[] <AddVoiceRows>b__10_0()
			{
				return SettingsText.VoiceLanguageOptions;
			}

			internal string <AddVoiceRows>b__10_1(string v)
			{
				return SettingsText.Option(v);
			}

			internal string <AddVoiceRows>b__10_2()
			{
				return SettingsHost.Current.Voice.Language;
			}

			internal void <AddVoiceRows>b__10_3(string value)
			{
				Apply(new <>c__DisplayClass10_0
				{
					value = value
				}.<AddVoiceRows>b__4);
			}

			internal void <AddToggleRow>b__13_0(bool pressed)
			{
				Apply(new <>c__DisplayClass13_1
				{
					pressed = pressed
				}.<AddToggleRow>b__2);
			}
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass10_0
		{
			public string value;

			internal void <AddVoiceRows>b__4()
			{
				SettingsHost.Current.Voice.Language = VoiceLanguages.Normalize(value);
			}
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass13_1
		{
			public bool pressed;

			internal void <AddToggleRow>b__2()
			{
				SettingsHost.Current.RealMerchant.ShowInventoryLeg = pressed;
			}
		}

		public const string PanelName = "Merchant2CuteII_SettingsPanel";

		private static readonly List<Action> Refreshers = new List<Action>();

		private static bool _subscribed;

		public static void Toggle(Node host)
		{
			if (host != null)
			{
				SceneTree tree = host.GetTree();
				object obj;
				if (tree == null)
				{
					obj = null;
				}
				else
				{
					Window root = tree.Root;
					obj = ((root != null) ? ((Node)root).FindChild("Merchant2CuteII_SettingsPanel", true, false) : null);
				}
				Node val = (Node)obj;
				if (val != null)
				{
					val.QueueFree();
					Refreshers.Clear();
				}
				else
				{
					Open(host);
				}
			}
		}

		public static void Open(Node host)
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				EnsureSubscribed();
				Refreshers.Clear();
				Window val = Build(host);
				((Node)val).TreeExited += delegate
				{
					Refreshers.Clear();
				};
				SceneTree tree = host.GetTree();
				((Node)(((object)((tree != null) ? tree.Root : null)) ?? ((object)host))).AddChild((Node)(object)val, false, (InternalMode)0);
				val.PopupCentered((Vector2I?)new Vector2I(1000, 820));
			}
			catch (Exception value)
			{
				GD.PrintErr($"[Merchant2CuteII] Failed to open settings panel: {value}");
			}
		}

		private static void EnsureSubscribed()
		{
			if (!_subscribed)
			{
				SettingsHost.Changed += RefreshOpenPanel;
				_subscribed = true;
			}
		}

		private static void RefreshOpenPanel()
		{
			if (Refreshers.Count == 0)
			{
				return;
			}
			List<Action> list = new List<Action>(Refreshers.Count);
			foreach (Action refresher in Refreshers)
			{
				try
				{
					refresher();
					list.Add(refresher);
				}
				catch (Exception ex)
				{
					GD.PrintErr("[Merchant2CuteII] Panel refresh failed: " + ex.Message);
				}
			}
			Refreshers.Clear();
			Refreshers.AddRange(list);
		}

		private static Window Build(Node host)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Expected O, but got Unknown
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Expected O, but got Unknown
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Expected O, but got Unknown
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Expected O, but got Unknown
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Expected O, but got Unknown
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Expected O, but got Unknown
			//IL_01e0: Expected O, but got Unknown
			AcceptDialog val = new AcceptDialog
			{
				Name = StringName.op_Implicit("Merchant2CuteII_SettingsPanel"),
				Title = string.Empty,
				Size = new Vector2I(980, 800),
				Exclusive = false,
				Theme = FindVanillaTheme(host)
			};
			MarginContainer val2 = new MarginContainer
			{
				CustomMinimumSize = new Vector2(960f, 770f)
			};
			((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("margin_left"), 44);
			((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), 44);
			((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("margin_top"), 28);
			((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("margin_bottom"), 28);
			ScrollContainer val3 = new ScrollContainer
			{
				SizeFlagsHorizontal = (SizeFlags)3,
				SizeFlagsVertical = (SizeFlags)3,
				HorizontalScrollMode = (ScrollMode)0
			};
			VBoxContainer val4 = new VBoxContainer
			{
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 6);
			((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
			((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			Label val5 = new Label
			{
				Text = "商人外观设置",
				HorizontalAlignment = (HorizontalAlignment)1,
				SizeFlagsHorizontal = (SizeFlags)3
			};
			SettingsTheme.ApplyTitle(val5);
			((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
			((Node)val4).AddChild((Node)new Control
			{
				CustomMinimumSize = new Vector2(0f, 10f)
			}, false, (InternalMode)0);
			AddSection(val4, SettingsText.Section("real"), SettingsText.Hint("real"));
			AddMerchantRows(val4, MerchantKind.Real);
			AddSection(val4, SettingsText.Section("fake"), SettingsText.Hint("fake"));
			AddMerchantRows(val4, MerchantKind.Fake);
			AddSection(val4, SettingsText.Section("voice"), SettingsText.Hint("voice"));
			AddVoiceRows(val4);
			((Node)val4).AddChild((Node)new Control
			{
				CustomMinimumSize = new Vector2(0f, 8f)
			}, false, (InternalMode)0);
			return (Window)val;
		}

		private static void AddSection(VBoxContainer column, string title, string hint)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			((Node)column).AddChild((Node)new Control
			{
				CustomMinimumSize = new Vector2(0f, 14f)
			}, false, (InternalMode)0);
			Label val = new Label
			{
				Text = title,
				SizeFlagsHorizontal = (SizeFlags)3
			};
			SettingsTheme.ApplySection(val);
			((Node)column).AddChild((Node)(object)val, false, (InternalMode)0);
			if (!string.IsNullOrEmpty(hint))
			{
				((Node)column).AddChild((Node)(object)HintLabel(hint), false, (InternalMode)0);
			}
			((Node)column).AddChild((Node)new HSeparator(), false, (InternalMode)0);
		}

		private static void AddMerchantRows(VBoxContainer column, MerchantKind kind)
		{
			if (SettingsText.BodySetOptions(kind).Length > 1)
			{
				AddRow(column, SettingsText.Field("bodySet"), () => SettingsText.BodySetOptions(kind), (string set) => SettingsText.ArtOwner(SkeletonSetAssets.IsFakeBodyArt(kind, set)), () => SettingsHost.Current.For(kind).BodySet, delegate(string value)
				{
					Apply(delegate
					{
						SettingsHost.Current.For(kind).BodySet = SkeletonSets.Normalize(value);
					});
				}, SettingsText.Hint("bodySet"));
			}
			else
			{
				AddStaticRow(column, SettingsText.Field("bodySet"), SettingsText.ArtOwner(SkeletonSetAssets.IsFakeBodyArt(kind, "own")), SettingsText.Hint("bodyLocked"));
			}
			AddRow(column, SettingsText.Field("handSet"), () => SettingsText.HandSetOptions(kind), (string set) => SettingsText.ArtOwner(SkeletonSetAssets.IsFakeHandArt(kind, set)), () => SettingsHost.Current.For(kind).HandSet, delegate(string value)
			{
				Apply(delegate
				{
					MerchantSkinSettings merchantSkinSettings = SettingsHost.Current.For(kind);
					merchantSkinSettings.HandSet = SkeletonSets.Normalize(value);
					merchantSkinSettings.HandVariant = HandVariants.NormalizeFor(kind, merchantSkinSettings.HandSet, merchantSkinSettings.HandVariant);
				});
			}, SettingsText.Hint("handSet"));
			AddRow(column, SettingsText.Field("handVariant"), () => SettingsText.HandVariantOptions(kind, SettingsHost.Current.For(kind).HandSet), (string v) => SettingsText.Option(v), () => HandVariants.NormalizeFor(kind, SettingsHost.Current.For(kind).HandSet, SettingsHost.Current.For(kind).HandVariant), delegate(string value)
			{
				Apply(delegate
				{
					MerchantSkinSettings merchantSkinSettings2 = SettingsHost.Current.For(kind);
					merchantSkinSettings2.HandVariant = HandVariants.NormalizeFor(kind, merchantSkinSettings2.HandSet, value);
				});
			}, SettingsText.Hint("handVariant"));
			if (kind == MerchantKind.Real)
			{
				AddToggleRow(column, SettingsText.Field("showLeg"), SettingsText.Hint("leg"));
			}
		}

		private static void AddVoiceRows(VBoxContainer column)
		{
			AddRow(column, SettingsText.Field("language"), () => SettingsText.VoiceLanguageOptions, (string v) => SettingsText.Option(v), () => SettingsHost.Current.Voice.Language, delegate(string value)
			{
				Apply(delegate
				{
					SettingsHost.Current.Voice.Language = VoiceLanguages.Normalize(value);
				});
			}, null);
			AddSliderRow(column, SettingsText.Field("volume"));
		}

		private static void AddRow(VBoxContainer column, string label, Func<string[]> optionsProvider, Func<string, string> optionLabelProvider, Func<string> readCurrent, Action<string> onSelected, string? hint)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Expected O, but got Unknown
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Expected O, but got Unknown
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Expected O, but got Unknown
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Expected O, but got Unknown
			HBoxContainer val = new HBoxContainer
			{
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
			Label val2 = new Label
			{
				Text = label,
				SizeFlagsHorizontal = (SizeFlags)3,
				VerticalAlignment = (VerticalAlignment)1
			};
			((Control)val2).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			OptionButton dropdown = new OptionButton
			{
				CustomMinimumSize = new Vector2(320f, 40f),
				SizeFlagsVertical = (SizeFlags)4
			};
			List<string> built = new List<string>();
			Rebuild(optionsProvider());
			dropdown.Selected = Math.Max(0, built.IndexOf(readCurrent()));
			dropdown.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
			{
				onSelected(built[(int)index]);
			};
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)dropdown, false, (InternalMode)0);
			((Node)column).AddChild((Node)(object)val, false, (InternalMode)0);
			if (!string.IsNullOrEmpty(hint))
			{
				((Node)column).AddChild((Node)(object)HintLabel(hint), false, (InternalMode)0);
			}
			((Node)column).AddChild((Node)new HSeparator(), false, (InternalMode)0);
			Refreshers.Add(delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)dropdown))
				{
					string[] array = optionsProvider();
					if (!built.SequenceEqual(array))
					{
						Rebuild(array);
					}
					int num = built.IndexOf(readCurrent());
					if (num >= 0)
					{
						dropdown.Selected = num;
					}
				}
			});
			void Rebuild(string[] options)
			{
				dropdown.Clear();
				built.Clear();
				for (int i = 0; i < options.Length; i++)
				{
					dropdown.AddItem(optionLabelProvider(options[i]), i);
					built.Add(options[i]);
				}
			}
		}

		private static void AddStaticRow(VBoxContainer column, string label, string value, string? hint = null)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Expected O, but got Unknown
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Expected O, but got Unknown
			HBoxContainer val = new HBoxContainer
			{
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
			Label val2 = new Label
			{
				Text = label,
				SizeFlagsHorizontal = (SizeFlags)3,
				VerticalAlignment = (VerticalAlignment)1
			};
			((Control)val2).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			Label val3 = new Label
			{
				Text = value,
				CustomMinimumSize = new Vector2(320f, 0f),
				HorizontalAlignment = (HorizontalAlignment)2,
				VerticalAlignment = (VerticalAlignment)1
			};
			SettingsTheme.ApplyValue(val3);
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
			((Node)column).AddChild((Node)(object)val, false, (InternalMode)0);
			if (!string.IsNullOrEmpty(hint))
			{
				((Node)column).AddChild((Node)(object)HintLabel(hint), false, (InternalMode)0);
			}
			((Node)column).AddChild((Node)new HSeparator(), false, (InternalMode)0);
		}

		private static void AddToggleRow(VBoxContainer column, string label, string hint)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Expected O, but got Unknown
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Expected O, but got Unknown
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Expected O, but got Unknown
			HBoxContainer val = new HBoxContainer
			{
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
			Label val2 = new Label
			{
				Text = label,
				SizeFlagsHorizontal = (SizeFlags)3,
				VerticalAlignment = (VerticalAlignment)1
			};
			((Control)val2).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			MerchantSkinSettings realMerchant = SettingsHost.Current.RealMerchant;
			bool flag = SettingsJson.CanToggleInventoryLeg(realMerchant, MerchantKind.Real);
			CheckBox check = new CheckBox
			{
				ButtonPressed = realMerchant.ShowInventoryLeg,
				Disabled = !flag,
				CustomMinimumSize = new Vector2(320f, 40f),
				SizeFlagsVertical = (SizeFlags)4
			};
			CheckBox obj = check;
			object obj2 = <>c.<>9__13_0;
			if (obj2 == null)
			{
				ToggledEventHandler val3 = delegate(bool pressed)
				{
					Apply(delegate
					{
						SettingsHost.Current.RealMerchant.ShowInventoryLeg = pressed;
					});
				};
				<>c.<>9__13_0 = val3;
				obj2 = (object)val3;
			}
			((BaseButton)obj).Toggled += (ToggledEventHandler)obj2;
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)check, false, (InternalMode)0);
			((Node)column).AddChild((Node)(object)val, false, (InternalMode)0);
			Label hintLabel = HintLabel(hint);
			((Node)column).AddChild((Node)(object)hintLabel, false, (InternalMode)0);
			((Node)column).AddChild((Node)new HSeparator(), false, (InternalMode)0);
			Refreshers.Add(delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)check))
				{
					MerchantSkinSettings realMerchant2 = SettingsHost.Current.RealMerchant;
					bool flag2 = SettingsJson.UsesOwnArt(realMerchant2, MerchantKind.Real);
					((BaseButton)check).Disabled = !flag2;
					((BaseButton)check).ButtonPressed = realMerchant2.ShowInventoryLeg && flag2;
					hintLabel.Text = (flag2 ? hint : SettingsText.Hint("legUnavailable"));
				}
			});
		}

		private static void AddSliderRow(VBoxContainer column, string label)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected O, but got Unknown
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected O, but got Unknown
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Expected O, but got Unknown
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Expected O, but got Unknown
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Expected O, but got Unknown
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Expected O, but got Unknown
			HBoxContainer val = new HBoxContainer
			{
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
			Label val2 = new Label
			{
				Text = label,
				SizeFlagsHorizontal = (SizeFlags)3,
				VerticalAlignment = (VerticalAlignment)1
			};
			((Control)val2).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			HSlider slider = new HSlider
			{
				MinValue = -12.0,
				MaxValue = 12.0,
				Step = 1.0,
				Value = SettingsHost.Current.Voice.VolumeDb,
				CustomMinimumSize = new Vector2(260f, 28f),
				SizeFlagsVertical = (SizeFlags)4
			};
			Label valueLabel = new Label
			{
				Text = FormatDb(SettingsHost.Current.Voice.VolumeDb),
				CustomMinimumSize = new Vector2(130f, 0f),
				VerticalAlignment = (VerticalAlignment)1
			};
			((Range)slider).ValueChanged += (ValueChangedEventHandler)delegate(double value)
			{
				valueLabel.Text = FormatDb((float)value);
				Apply(delegate
				{
					SettingsHost.Current.Voice.VolumeDb = (float)value;
				});
			};
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)slider, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)valueLabel, false, (InternalMode)0);
			((Node)column).AddChild((Node)(object)val, false, (InternalMode)0);
			((Node)column).AddChild((Node)new HSeparator(), false, (InternalMode)0);
			Refreshers.Add(delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)slider))
				{
					float volumeDb = SettingsHost.Current.Voice.VolumeDb;
					((Range)slider).SetValueNoSignal((double)volumeDb);
					valueLabel.Text = FormatDb(volumeDb);
				}
			});
		}

		private static Label HintLabel(string hint)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			//IL_0023: Expected O, but got Unknown
			Label val = new Label
			{
				Text = hint,
				AutowrapMode = (AutowrapMode)3,
				SizeFlagsHorizontal = (SizeFlags)3
			};
			SettingsTheme.ApplyHint(val);
			return val;
		}

		private static Theme? FindVanillaTheme(Node host)
		{
			try
			{
				for (Node val = host; val != null; val = val.GetParent())
				{
					Control val2 = (Control)(object)((val is Control) ? val : null);
					if (val2 != null && val2.Theme != null)
					{
						return val2.Theme;
					}
				}
				MainLoop mainLoop = Engine.GetMainLoop();
				MainLoop obj = ((mainLoop is SceneTree) ? mainLoop : null);
				Window val3 = ((obj != null) ? ((SceneTree)obj).Root : null);
				if (val3 != null && val3.Theme != null)
				{
					return val3.Theme;
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] theme lookup failed: " + ex.Message);
			}
			return null;
		}

		private static string FormatDb(float db)
		{
			float value = (float)Math.Pow(10.0, (double)db / 20.0);
			return $"{db:+0;-0;0} dB ({value:0.00}x)";
		}

		private static void Apply(Action mutate)
		{
			try
			{
				SettingsHost.Mutate(delegate
				{
					mutate();
				});
			}
			catch (Exception value)
			{
				GD.PrintErr($"[Merchant2CuteII] Applying setting failed: {value}");
			}
		}
	}
	public static class SettingsRows
	{
		public static void AddTitle(VBoxContainer column, string text)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			Label val = new Label
			{
				Text = text,
				HorizontalAlignment = (HorizontalAlignment)1,
				SizeFlagsHorizontal = (SizeFlags)3
			};
			SettingsTheme.ApplyTitle(val);
			((Node)column).AddChild((Node)(object)val, false, (InternalMode)0);
			((Node)column).AddChild((Node)new Control
			{
				CustomMinimumSize = new Vector2(0f, 8f)
			}, false, (InternalMode)0);
		}

		public static void AddSection(VBoxContainer column, string title, string? hint)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			((Node)column).AddChild((Node)new Control
			{
				CustomMinimumSize = new Vector2(0f, 10f)
			}, false, (InternalMode)0);
			Label val = new Label
			{
				Text = title,
				SizeFlagsHorizontal = (SizeFlags)3
			};
			SettingsTheme.ApplySection(val);
			((Node)column).AddChild((Node)(object)val, false, (InternalMode)0);
			if (!string.IsNullOrEmpty(hint))
			{
				((Node)column).AddChild((Node)(object)HintLabel(hint), false, (InternalMode)0);
			}
			((Node)column).AddChild((Node)new HSeparator(), false, (InternalMode)0);
		}

		public static OptionButton AddDropdownRow(VBoxContainer column, string label, Func<string[]> optionsProvider, Func<string, string> optionLabelProvider, Func<string> readCurrent, Action<string> onSelected, string? hint, List<Action>? refreshers = null)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Expected O, but got Unknown
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Expected O, but got Unknown
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Expected O, but got Unknown
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Expected O, but got Unknown
			HBoxContainer val = new HBoxContainer
			{
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
			Label val2 = new Label
			{
				Text = label,
				SizeFlagsHorizontal = (SizeFlags)3,
				VerticalAlignment = (VerticalAlignment)1
			};
			SettingsTheme.ApplyField(val2);
			OptionButton dropdown = new OptionButton
			{
				CustomMinimumSize = new Vector2(300f, 40f),
				SizeFlagsVertical = (SizeFlags)4
			};
			List<string> built = new List<string>();
			Rebuild(optionsProvider());
			dropdown.Selected = Math.Max(0, built.IndexOf(readCurrent()));
			dropdown.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
			{
				onSelected(built[(int)index]);
			};
			refreshers?.Add(delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)dropdown))
				{
					string[] array = optionsProvider();
					if (!built.SequenceEqual(array))
					{
						Rebuild(array);
					}
					int num = built.IndexOf(readCurrent());
					if (num >= 0)
					{
						dropdown.Selected = num;
					}
				}
			});
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)dropdown, false, (InternalMode)0);
			((Node)column).AddChild((Node)(object)val, false, (InternalMode)0);
			if (!string.IsNullOrEmpty(hint))
			{
				((Node)column).AddChild((Node)(object)HintLabel(hint), false, (InternalMode)0);
			}
			((Node)column).AddChild((Node)new HSeparator(), false, (InternalMode)0);
			return dropdown;
			void Rebuild(string[] options)
			{
				dropdown.Clear();
				built.Clear();
				for (int i = 0; i < options.Length; i++)
				{
					dropdown.AddItem(optionLabelProvider(options[i]), i);
					built.Add(options[i]);
				}
			}
		}

		public static void AddStaticRow(VBoxContainer column, string label, string value, string? hint)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Expected O, but got Unknown
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Expected O, but got Unknown
			HBoxContainer val = new HBoxContainer
			{
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
			Label val2 = new Label
			{
				Text = label,
				SizeFlagsHorizontal = (SizeFlags)3,
				VerticalAlignment = (VerticalAlignment)1
			};
			SettingsTheme.ApplyField(val2);
			Label val3 = new Label
			{
				Text = value,
				CustomMinimumSize = new Vector2(300f, 0f),
				HorizontalAlignment = (HorizontalAlignment)2,
				VerticalAlignment = (VerticalAlignment)1
			};
			SettingsTheme.ApplyValue(val3);
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
			((Node)column).AddChild((Node)(object)val, false, (InternalMode)0);
			if (!string.IsNullOrEmpty(hint))
			{
				((Node)column).AddChild((Node)(object)HintLabel(hint), false, (InternalMode)0);
			}
			((Node)column).AddChild((Node)new HSeparator(), false, (InternalMode)0);
		}

		public static CheckBox AddToggleRow(VBoxContainer column, string label, Func<bool> readValue, Func<bool> readEnabled, Action<bool> onToggled, Func<string> hintProvider, List<Action>? refreshers = null)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Expected O, but got Unknown
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Expected O, but got Unknown
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Expected O, but got Unknown
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Expected O, but got Unknown
			HBoxContainer val = new HBoxContainer
			{
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
			Label val2 = new Label
			{
				Text = label,
				SizeFlagsHorizontal = (SizeFlags)3,
				VerticalAlignment = (VerticalAlignment)1
			};
			SettingsTheme.ApplyField(val2);
			CheckBox check = new CheckBox
			{
				ButtonPressed = readValue(),
				Disabled = !readEnabled(),
				CustomMinimumSize = new Vector2(300f, 40f),
				SizeFlagsVertical = (SizeFlags)4
			};
			((BaseButton)check).Toggled += (ToggledEventHandler)delegate(bool pressed)
			{
				onToggled(pressed);
			};
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)check, false, (InternalMode)0);
			((Node)column).AddChild((Node)(object)val, false, (InternalMode)0);
			Label hintLabel = HintLabel(hintProvider());
			((Node)column).AddChild((Node)(object)hintLabel, false, (InternalMode)0);
			((Node)column).AddChild((Node)new HSeparator(), false, (InternalMode)0);
			refreshers?.Add(delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)check))
				{
					bool flag = readEnabled();
					((BaseButton)check).Disabled = !flag;
					((BaseButton)check).SetPressedNoSignal(readValue());
					hintLabel.Text = hintProvider();
				}
			});
			return check;
		}

		public static void AddSliderRow(VBoxContainer column, string label, float min, float max, float step, Func<float> readValue, Func<float, string> formatter, Action<float> onChanged, List<Action>? refreshers = null)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected O, but got Unknown
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Expected O, but got Unknown
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Expected O, but got Unknown
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Expected O, but got Unknown
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Expected O, but got Unknown
			HBoxContainer val = new HBoxContainer
			{
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
			Label val2 = new Label
			{
				Text = label,
				SizeFlagsHorizontal = (SizeFlags)3,
				VerticalAlignment = (VerticalAlignment)1
			};
			SettingsTheme.ApplyField(val2);
			HSlider slider = new HSlider
			{
				MinValue = min,
				MaxValue = max,
				Step = step,
				Value = readValue(),
				CustomMinimumSize = new Vector2(240f, 28f),
				SizeFlagsVertical = (SizeFlags)4
			};
			Label valueLabel = new Label
			{
				Text = formatter(readValue()),
				CustomMinimumSize = new Vector2(130f, 0f),
				VerticalAlignment = (VerticalAlignment)1
			};
			((Range)slider).ValueChanged += (ValueChangedEventHandler)delegate(double v)
			{
				valueLabel.Text = formatter((float)v);
				onChanged((float)v);
			};
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)slider, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)valueLabel, false, (InternalMode)0);
			((Node)column).AddChild((Node)(object)val, false, (InternalMode)0);
			((Node)column).AddChild((Node)new HSeparator(), false, (InternalMode)0);
			refreshers?.Add(delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)slider))
				{
					float num = readValue();
					((Range)slider).SetValueNoSignal((double)num);
					valueLabel.Text = formatter(num);
				}
			});
		}

		public static Label HintLabel(string hint)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			//IL_0023: Expected O, but got Unknown
			Label val = new Label
			{
				Text = hint,
				AutowrapMode = (AutowrapMode)3,
				SizeFlagsHorizontal = (SizeFlags)3
			};
			SettingsTheme.ApplyHint(val);
			return val;
		}

		public static void AddMerchantRows(VBoxContainer column, MerchantKind kind, List<Action>? refreshers = null)
		{
			if (SettingsText.BodySetOptions(kind).Length > 1)
			{
				AddDropdownRow(column, SettingsText.Field("bodySet"), () => SettingsText.BodySetOptions(kind), (string set) => SettingsText.ArtOwner(SkeletonSetAssets.IsFakeBodyArt(kind, set)), () => SettingsHost.Current.For(kind).BodySet, delegate(string value)
				{
					Mutate(delegate
					{
						SettingsHost.Current.For(kind).BodySet = SkeletonSets.Normalize(value);
					});
				}, SettingsText.Hint("bodySet"), refreshers);
			}
			else
			{
				AddStaticRow(column, SettingsText.Field("bodySet"), SettingsText.ArtOwner(SkeletonSetAssets.IsFakeBodyArt(kind, "own")), SettingsText.Hint("bodyLocked"));
			}
			AddDropdownRow(column, SettingsText.Field("handSet"), () => SettingsText.HandSetOptions(kind), (string set) => SettingsText.ArtOwner(SkeletonSetAssets.IsFakeHandArt(kind, set)), () => SettingsHost.Current.For(kind).HandSet, delegate(string value)
			{
				Mutate(delegate
				{
					MerchantSkinSettings merchantSkinSettings = SettingsHost.Current.For(kind);
					merchantSkinSettings.HandSet = SkeletonSets.Normalize(value);
					merchantSkinSettings.HandVariant = HandVariants.NormalizeFor(kind, merchantSkinSettings.HandSet, merchantSkinSettings.HandVariant);
				});
			}, SettingsText.Hint("handSet"), refreshers);
			AddDropdownRow(column, SettingsText.Field("handVariant"), () => SettingsText.HandVariantOptions(kind, SettingsHost.Current.For(kind).HandSet), (string v) => SettingsText.Option(v), () => HandVariants.NormalizeFor(kind, SettingsHost.Current.For(kind).HandSet, SettingsHost.Current.For(kind).HandVariant), delegate(string value)
			{
				Mutate(delegate
				{
					MerchantSkinSettings merchantSkinSettings2 = SettingsHost.Current.For(kind);
					merchantSkinSettings2.HandVariant = HandVariants.NormalizeFor(kind, merchantSkinSettings2.HandSet, value);
				});
			}, SettingsText.Hint("handVariant"), refreshers);
			if (kind != 0)
			{
				return;
			}
			SettingsJson.UsesOwnArt(SettingsHost.Current.RealMerchant, MerchantKind.Real);
			AddToggleRow(column, SettingsText.Field("showLeg"), () => SettingsHost.Current.RealMerchant.ShowInventoryLeg, () => SettingsJson.CanToggleInventoryLeg(SettingsHost.Current.RealMerchant, MerchantKind.Real), delegate(bool pressed)
			{
				Mutate(delegate
				{
					MerchantSkinSettings realMerchant = SettingsHost.Current.RealMerchant;
					if (SettingsJson.CanToggleInventoryLeg(realMerchant, MerchantKind.Real))
					{
						realMerchant.ShowInventoryLeg = pressed;
					}
				});
			}, () => SettingsJson.UsesOwnArt(SettingsHost.Current.RealMerchant, MerchantKind.Real) ? ((!SettingsJson.IsFootLike(SettingsHost.Current.RealMerchant.HandVariant)) ? SettingsText.Hint("leg") : SettingsText.Hint("legNeedsHand")) : SettingsText.Hint("legUnavailable"), refreshers);
		}

		public static void AddVoiceRows(VBoxContainer column, List<Action>? refreshers = null)
		{
			AddDropdownRow(column, SettingsText.Field("language"), () => SettingsText.VoiceLanguageOptions, (string v) => SettingsText.Option(v), () => SettingsHost.Current.Voice.Language, delegate(string value)
			{
				Mutate(delegate
				{
					SettingsHost.Current.Voice.Language = VoiceLanguages.Normalize(value);
				});
			}, null, refreshers);
			AddSliderRow(column, SettingsText.Field("volume"), -12f, 12f, 1f, () => SettingsHost.Current.Voice.VolumeDb, (float db) => $"{db:+0;-0;0} dB ({(float)Math.Pow(10.0, (double)db / 20.0):0.00}x)", delegate(float value)
			{
				Mutate(delegate
				{
					SettingsHost.Current.Voice.VolumeDb = value;
				});
			}, refreshers);
		}

		private static void Mutate(Action mutate)
		{
			try
			{
				SettingsHost.Mutate(delegate
				{
					mutate();
				});
			}
			catch (Exception value)
			{
				GD.PrintErr($"[Merchant2CuteII] applying setting failed: {value}");
			}
		}
	}
	public static class SettingsStore
	{
		public const string ConfigPath = "user://merchant2cute_config.json";

		public static bool LastLoadWasLegacy { get; private set; }

		public static ModSettings Load()
		{
			LastLoadWasLegacy = false;
			string text = ReadAllText();
			if (text == null)
			{
				return new ModSettings();
			}
			bool wasLegacy;
			ModSettings result = SettingsJson.Parse(text, out wasLegacy);
			LastLoadWasLegacy = wasLegacy;
			if (wasLegacy)
			{
				GD.Print("[Merchant2CuteII] Legacy config (schema 1) detected and migrated to schema " + 2 + ".");
			}
			return result;
		}

		public static bool Save(ModSettings settings)
		{
			try
			{
				string text = SettingsJson.Serialize(settings);
				FileAccess val = FileAccess.Open("user://merchant2cute_config.json", (ModeFlags)2);
				try
				{
					if (val == null)
					{
						GD.PrintErr("[Merchant2CuteII] Cannot open config for writing: user://merchant2cute_config.json");
						return false;
					}
					val.StoreString(text);
					return true;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Failed to save settings: " + ex.Message);
				return false;
			}
		}

		public static bool IsFootLike(string handVariant)
		{
			return SettingsJson.IsFootLike(handVariant);
		}

		private static string? ReadAllText()
		{
			try
			{
				if (!FileAccess.FileExists("user://merchant2cute_config.json"))
				{
					return null;
				}
				FileAccess val = FileAccess.Open("user://merchant2cute_config.json", (ModeFlags)1);
				try
				{
					if (val == null)
					{
						return null;
					}
					string asText = val.GetAsText(false);
					return string.IsNullOrWhiteSpace(asText) ? null : asText;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Failed to read settings: " + ex.Message);
				return null;
			}
		}
	}
	public static class SettingsText
	{
		public const string Title = "商人外观设置";

		public static string[] VoiceLanguageOptions => VoiceLanguages.All;

		public static IReadOnlyList<string> All => new string[1] { "商人外观设置" };

		public static string Section(string key)
		{
			return key switch
			{
				"real" => "商店商人", 
				"fake" => "假商人", 
				"voice" => "商人语音", 
				_ => key, 
			};
		}

		public static string Field(string key)
		{
			return key switch
			{
				"bodySet" => "身体美术", 
				"handSet" => "手部美术", 
				"handVariant" => "手部变体", 
				"showLeg" => "商店界面显示腿部贴图", 
				"language" => "语音语言", 
				"volume" => "语音音量", 
				_ => key, 
			};
		}

		public static string Option(string key)
		{
			return key switch
			{
				"hand" => "手", 
				"foot" => "裸足", 
				"white" => "白丝", 
				"black" => "黑丝", 
				"default" => "原版", 
				"jp" => "日语", 
				"zh" => "中文", 
				_ => key, 
			};
		}

		public static string ArtOwner(bool fakeArt)
		{
			if (!fakeArt)
			{
				return "商店商人美术";
			}
			return "假商人美术";
		}

		public static string Hint(string key)
		{
			return key switch
			{
				"real" => "商店里出现的商人。身体与手部可以分别选择使用哪一套美术。", 
				"fake" => "事件里出现的商人。手部可以选择使用哪一套美术。", 
				"bodySet" => "身体（含商店界面）使用的美术。", 
				"handSet" => "手部使用的美术；白丝、黑丝只有在使用商店商人的手部美术时才可选。", 
				"handVariant" => "可选项会跟随上面选择的手部美术变化。", 
				"leg" => "手部变体为“手”时可开启；裸足 / 白丝 / 黑丝 时固定不显示。", 
				"legNeedsHand" => "只有在手部变体为“手”时才可以开关这张腿部贴图。", 
				"legUnavailable" => "这张腿部贴图属于商店商人美术，选用假商人美术时不可用。", 
				"bodyLocked" => "假商人的身体固定使用假商人美术。", 
				"voice" => "替换商人的语音。", 
				"whiteBlack" => "白丝、黑丝两种变体只有商店商人的手部美术提供。", 
				_ => string.Empty, 
			};
		}

		public static string[] HandVariantOptions(MerchantKind kind, string handSet)
		{
			return HandVariants.Allowed(kind, handSet);
		}

		public static string[] BodySetOptions(MerchantKind kind)
		{
			return SkeletonSets.AllowedBodySets(kind);
		}

		public static string[] HandSetOptions(MerchantKind kind)
		{
			return SkeletonSets.All;
		}
	}
	public static class SettingsTheme
	{
		public static readonly Color GoldBright = new Color(0.937f, 0.784f, 0.318f, 1f);

		public static readonly Color Gold = new Color(0.73f, 0.543f, 0.139f, 1f);

		public static Color Hint => new Color(0.84f, 0.84f, 0.82f, 0.92f);

		public static Color Value => new Color(0.73f, 0.543f, 0.139f, 0.9f);

		public static void ApplyTitle(Label label)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			((Control)label).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 30);
			((Control)label).AddThemeColorOverride(StringName.op_Implicit("font_color"), GoldBright);
		}

		public static void ApplySection(Label label)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			((Control)label).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 21);
			((Control)label).AddThemeColorOverride(StringName.op_Implicit("font_color"), GoldBright);
		}

		public static void ApplyField(Label label)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			((Control)label).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			((Control)label).AddThemeColorOverride(StringName.op_Implicit("font_color"), Gold);
		}

		public static void ApplyHint(Label label)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			((Control)label).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 14);
			((Control)label).AddThemeColorOverride(StringName.op_Implicit("font_color"), Hint);
		}

		public static void ApplyValue(Label label)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			((Control)label).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 16);
			((Control)label).AddThemeColorOverride(StringName.op_Implicit("font_color"), Value);
		}
	}
	public static class SkeletonSetAssets
	{
		private const string RealHandTres = "res://animations/backgrounds/merchant_room/hand/merchant_hand_skel_data.tres";

		private const string RealBodyTres = "res://animations/backgrounds/merchant_room/top/shop_merchant_top.tres";

		private const string FakeHandTres = "res://animations/backgrounds/fake_merchant_room/hand/fake_merchant_hand_skel_data.tres";

		private const string FakeBodyTres = "res://animations/backgrounds/fake_merchant_room/top/fake_merchant_top.tres";

		public static string HandTresPath(MerchantKind kind, string handSet)
		{
			if (!IsFakeHandArt(kind, handSet))
			{
				return "res://animations/backgrounds/merchant_room/hand/merchant_hand_skel_data.tres";
			}
			return "res://animations/backgrounds/fake_merchant_room/hand/fake_merchant_hand_skel_data.tres";
		}

		public static string BodyTresPath(MerchantKind kind, string bodySet)
		{
			if (!IsFakeBodyArt(kind, bodySet))
			{
				return "res://animations/backgrounds/merchant_room/top/shop_merchant_top.tres";
			}
			return "res://animations/backgrounds/fake_merchant_room/top/fake_merchant_top.tres";
		}

		public static bool IsFakeHandArt(MerchantKind kind, string handSet)
		{
			bool flag = SkeletonSets.Normalize(handSet) == "other";
			if (kind != 0)
			{
				return !flag;
			}
			return flag;
		}

		public static bool IsFakeBodyArt(MerchantKind kind, string bodySet)
		{
			bool flag = SkeletonSets.Normalize(bodySet) == "other";
			if (kind != 0)
			{
				return !flag;
			}
			return flag;
		}

		public static string DescribePart(MerchantKind kind, string part, string set)
		{
			bool flag = ((part == "hand") ? IsFakeHandArt(kind, set) : IsFakeBodyArt(kind, set));
			string arg = ((kind == MerchantKind.Real) ? "real" : "fake");
			return string.Format("{0}.{1}={2}", arg, part, flag ? "fake_art" : "merchant_art");
		}

		public static string Describe(MerchantKind kind, string set)
		{
			return DescribePart(kind, "hand", set);
		}

		public static string[] AllTresPaths()
		{
			return new string[4] { "res://animations/backgrounds/merchant_room/hand/merchant_hand_skel_data.tres", "res://animations/backgrounds/merchant_room/top/shop_merchant_top.tres", "res://animations/backgrounds/fake_merchant_room/hand/fake_merchant_hand_skel_data.tres", "res://animations/backgrounds/fake_merchant_room/top/fake_merchant_top.tres" };
		}
	}
	public static class SkinApplier
	{
		public static MerchantKind? ResolveKind(Node? node)
		{
			Node val = node;
			int num = 0;
			while (val != null && num++ < 64)
			{
				if (val is NFakeMerchant)
				{
					return MerchantKind.Fake;
				}
				if (val is NMerchantRoom)
				{
					return MerchantKind.Real;
				}
				val = val.GetParent();
			}
			return null;
		}

		public static MerchantSkinSettings SettingsFor(MerchantKind kind)
		{
			return SettingsHost.Current.For(kind);
		}

		public static bool ApplyToHand(Node? handNode)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			if (handNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)handNode))
			{
				return false;
			}
			MerchantKind? merchantKind = ResolveKind(handNode);
			if (!merchantKind.HasValue)
			{
				return false;
			}
			Node parent = handNode.GetParent();
			if (parent == null)
			{
				return false;
			}
			MerchantSkinSettings merchantSkinSettings = SettingsFor(merchantKind.Value);
			try
			{
				MegaSprite sprite = new MegaSprite(Variant.op_Implicit((GodotObject)(object)parent));
				return ApplyHandVariant(parent, sprite, merchantSkinSettings.HandVariant);
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] ApplyToHand failed: " + ex.Message);
				return false;
			}
		}

		private static bool ApplyHandVariant(Node host, MegaSprite sprite, string variant)
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			string[] chain = HandVariants.AnimationFallbackChain(variant);
			if (AnimationHelper.ShouldUseLegacyHandSwitching())
			{
				string[] array = chain;
				foreach (string text in array)
				{
					if (sprite.HasAnimation(text))
					{
						((MegaSpineBinding)sprite.GetAnimationState()).BoundObject.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
						{
							Variant.op_Implicit(text),
							Variant.op_Implicit(false),
							Variant.op_Implicit(0)
						});
						return true;
					}
				}
				return false;
			}
			if (!sprite.IsAnimationStateReady())
			{
				SpineNodeExtensions.RunWhenSpineReady(host, sprite, (Action<MegaAnimationState>)delegate(MegaAnimationState animState)
				{
					SetFirstAvailable(animState, sprite, chain);
				});
				return true;
			}
			return SetFirstAvailable(sprite.GetAnimationState(), sprite, chain);
		}

		private static bool SetFirstAvailable(MegaAnimationState animState, MegaSprite sprite, string[] chain)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			if (animState == null)
			{
				return false;
			}
			foreach (string text in chain)
			{
				if (sprite.HasAnimation(text))
				{
					((MegaSpineBinding)animState).BoundObject.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
					{
						Variant.op_Implicit(text),
						Variant.op_Implicit(false),
						Variant.op_Implicit(0)
					});
					return true;
				}
			}
			GD.PrintErr("[Merchant2CuteII] No usable hand animation among: " + string.Join(", ", chain));
			return false;
		}

		public static void WarmUp()
		{
			MerchantSceneSkinner.WarmUp();
		}
	}
}
namespace Merchant2CuteII.script.Patches
{
	[HarmonyPatch(typeof(NSceneContainer), "SetCurrentScene")]
	public static class MerchantSceneSkinPatch
	{
		[HarmonyPrefix]
		public static void Prefix(Control __0)
		{
			try
			{
				MerchantSceneSkinner.ApplyBeforeEnteringTree((Node?)(object)__0);
			}
			catch (Exception value)
			{
				GD.PrintErr($"[Merchant2CuteII] pre-tree skin apply failed: {value}");
			}
		}
	}
	[HarmonyPatch(typeof(NSettingsScreen), "_Ready")]
	public static class SettingsEntryPatch
	{
		[HarmonyPostfix]
		public static void Postfix(NSettingsScreen __instance)
		{
			try
			{
				MerchantSettingsEntry.Inject((Node)(object)__instance);
			}
			catch (Exception value)
			{
				GD.PrintErr($"[Merchant2CuteII] settings entry patch failed: {value}");
			}
		}
	}
}
namespace Merchant2CuteII.script.Compat
{
	public static class GameVersion
	{
		private const string EnvOverride = "MERCHANT2CUTE_FAKE_GAME_VERSION";

		private static string? _forced;

		public static SemanticVersion? Current
		{
			get
			{
				if (_forced != null)
				{
					SemanticVersion result = default(SemanticVersion);
					if (!SemanticVersion.TryFromString(_forced, ref result))
					{
						return null;
					}
					return result;
				}
				try
				{
					ReleaseInfoManager instance = ReleaseInfoManager.Instance;
					return (instance != null) ? instance.SemVer : null;
				}
				catch (Exception)
				{
					return null;
				}
			}
		}

		public static bool Known => Current != null;

		public static void Force(string? version)
		{
			_forced = (string.IsNullOrWhiteSpace(version) ? null : version.Trim());
		}

		public static void ForceFromEnvironment()
		{
			string environmentVariable = Environment.GetEnvironmentVariable("MERCHANT2CUTE_FAKE_GAME_VERSION");
			if (!string.IsNullOrWhiteSpace(environmentVariable))
			{
				Force(environmentVariable);
			}
		}

		public static bool AtLeast(int major, int minor, int patch)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			SemanticVersion current = Current;
			if (current != null)
			{
				return current.CompareTo(new SemanticVersion(major, minor, patch, (string)null, (List<string>)null)) >= 0;
			}
			return false;
		}

		public static bool AtLeast(string version)
		{
			SemanticVersion val = default(SemanticVersion);
			if (!SemanticVersion.TryFromString(version, ref val) || val == null)
			{
				return false;
			}
			SemanticVersion current = Current;
			if (current != null)
			{
				return current.CompareTo(val) >= 0;
			}
			return false;
		}

		public static bool Below(int major, int minor, int patch)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			SemanticVersion current = Current;
			if (current != null)
			{
				return current.CompareTo(new SemanticVersion(major, minor, patch, (string)null, (List<string>)null)) < 0;
			}
			return true;
		}

		public static string Describe()
		{
			SemanticVersion current = Current;
			string text = ((_forced != null) ? " (forced)" : string.Empty);
			if (current != null)
			{
				return ((object)current)?.ToString() + text;
			}
			return "unknown" + text;
		}
	}
	public static class VersionProfile
	{
		private const int SpineApiMajor = 0;

		private const int SpineApiMinor = 108;

		private const int SpineApiPatch = 0;

		private const int VisualOwnerMajor = 0;

		private const int VisualOwnerMinor = 110;

		private const int VisualOwnerPatch = 0;

		public static bool LegacyAnimationSignature
		{
			get
			{
				if (GameVersion.Known)
				{
					return GameVersion.Below(0, 108, 0);
				}
				return false;
			}
		}

		public static bool LegacyMerchantVisualLookup
		{
			get
			{
				if (GameVersion.Known)
				{
					return GameVersion.Below(0, 110, 0);
				}
				return true;
			}
		}

		public static string Describe()
		{
			return string.Join(", ", "game=" + GameVersion.Describe(), "legacyAnimSig=" + Flag(LegacyAnimationSignature), "legacyVisualLookup=" + Flag(LegacyMerchantVisualLookup));
		}

		private static string Flag(bool value)
		{
			if (!value)
			{
				return "no";
			}
			return "yes";
		}
	}
}
[StructLayout(LayoutKind.Auto)]
[InlineArray(9)]
internal struct <>y__InlineArray9<T>
{
}
