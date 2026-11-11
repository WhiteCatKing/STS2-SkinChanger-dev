using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using HarmonyLib;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Merchant2CuteII.script;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: TargetFramework(".NETCoreApp,Version=v9.0", FrameworkDisplayName = ".NET 9.0")]
[assembly: AssemblyCompany("Merchant2CuteII")]
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+a711e8a8dc819bc5884e6131acef5e13bfa660f9")]
[assembly: AssemblyProduct("Merchant2CuteII")]
[assembly: AssemblyTitle("Merchant2CuteII")]
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
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				DllImportResolver resolver = new GodotDllImportResolver((IntPtr)godotDllHandle).OnResolveDllImport;
				Assembly assembly = typeof(GodotObject).Assembly;
				NativeLibrary.SetDllImportResolver(assembly, resolver);
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

		public override string Args => "<point|voice|foul|status>";

		public override string Description => "Switch merchant skeleton (via point) or voice/foul variant";

		public override bool IsNetworked => false;

		static MerchantConsoleCmd()
		{
			try
			{
				ConfigStore.Load();
				ApplyToExistingHands();
				UpdateLegVisibility(!ModConfig.Options.UseFootLike);
			}
			catch
			{
			}
		}

		public override CmdResult Process(Player? issuingPlayer, string[] args)
		{
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			return (CmdResult)(((args.Length != 0) ? args[0].ToLowerInvariant() : "status") switch
			{
				"status" => new CmdResult(true, $"Merchant variant: {ModConfig.Options.HandVariant}, voice: {ModConfig.Options.MerchantVoiceVariant}, foul: {ModConfig.Options.FoulPotionAnimation}"), 
				"voice" => ProcessVoiceCommand(args), 
				"point" => ProcessPointCommand(args), 
				"foul" => ProcessFoulCommand(args), 
				_ => new CmdResult(false, "Usage: merchant point hand|foot|toggle|status | merchant voice default|jp|zh|toggle|status|db | merchant foul poison|toggle|status"), 
			});
		}

		internal static int ApplyPointVariantNow()
		{
			int result = ApplyToExistingHands();
			TaskHelper.RunSafely(ApplyToExistingHandsNextFrame());
			UpdateLegVisibility(!ModConfig.Options.UseFootLike);
			return result;
		}

		private static CmdResult ProcessPointCommand(string[] args)
		{
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			string text = ((args.Length > 1) ? args[1].ToLowerInvariant() : "status");
			if (text == "status")
			{
				return new CmdResult(true, "Merchant point variant: " + ModConfig.Options.HandVariant);
			}
			if (text != "hand" && text != "foot" && text != "toggle" && text != "white" && text != "black")
			{
				return new CmdResult(false, "Usage: merchant point hand|foot|white|black|toggle|status");
			}
			string handVariant = ModConfig.Options.HandVariant;
			string value = (ModConfig.Options.HandVariant = ((!(text == "toggle")) ? text : ((handVariant == "hand") ? "foot" : "hand")));
			try
			{
				ConfigStore.Save();
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Failed to save config: " + ex.Message);
			}
			int value2 = ApplyPointVariantNow();
			return new CmdResult(true, $"Set merchant point variant to {value}. Updated {value2} node(s).");
		}

		private static CmdResult ProcessFoulCommand(string[] args)
		{
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			string text = ((args.Length > 1) ? args[1].ToLowerInvariant() : "status");
			if (text == "status")
			{
				return new CmdResult(true, "Merchant foul animation: " + ModConfig.Options.FoulPotionAnimation);
			}
			if (text != "poison" && text != "nsfw" && text != "toggle")
			{
				return new CmdResult(false, "Usage: merchant foul poison|toggle|status");
			}
			string foulPotionAnimation = ModConfig.Options.FoulPotionAnimation;
			string text3 = (ModConfig.Options.FoulPotionAnimation = ((!(text == "toggle")) ? text : ((foulPotionAnimation == "poison") ? "nsfw" : "poison")));
			try
			{
				ConfigStore.Save();
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Failed to save config: " + ex.Message);
			}
			return new CmdResult(true, "Set merchant foul animation to " + text3 + ".");
		}

		private static CmdResult ProcessVoiceCommand(string[] args)
		{
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0193: Unknown result type (might be due to invalid IL or missing references)
			string text = ((args.Length > 1) ? args[1].ToLowerInvariant() : "status");
			if (text == "status")
			{
				return new CmdResult(true, $"Merchant voice: {ModConfig.Options.MerchantVoiceVariant}, voiceDb: {ModConfig.Options.ExtraDb}");
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
				ModConfig.Options.ExtraDb = result;
				try
				{
					ConfigStore.Save();
				}
				catch (Exception ex)
				{
					GD.PrintErr("[Merchant2CuteII] Failed to save config: " + ex.Message);
				}
				return new CmdResult(true, $"Set merchant voice volume to {result} dB.");
			}
			if (text != "default" && text != "jp" && text != "zh" && text != "toggle")
			{
				return new CmdResult(false, "Usage: merchant voice default|jp|zh|toggle|status|db <value>");
			}
			string merchantVoiceVariant = ModConfig.Options.MerchantVoiceVariant;
			string text3 = (ModConfig.Options.MerchantVoiceVariant = ((!(text == "toggle")) ? text : ((merchantVoiceVariant == "default") ? "jp" : ((merchantVoiceVariant == "jp") ? "zh" : "default"))));
			try
			{
				ConfigStore.Save();
			}
			catch (Exception ex2)
			{
				GD.PrintErr("[Merchant2CuteII] Failed to save config: " + ex2.Message);
			}
			return new CmdResult(true, "Set merchant voice to " + text3 + ".");
		}

		private static int ApplyToExistingHands()
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val == null || val.Root == null)
			{
				return 0;
			}
			int updated = 0;
			FindAndApplyRecursive((Node)(object)val.Root, ref updated);
			return updated;
		}

		private static async Task ApplyToExistingHandsNextFrame()
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree tree = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (tree != null && tree.Root != null)
			{
				await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
				ApplyToExistingHands();
				UpdateLegVisibility(!ModConfig.Options.UseFootLike);
			}
		}

		private static void FindAndApplyRecursive(Node root, ref int updated)
		{
			if (root == null)
			{
				return;
			}
			foreach (Node child in root.GetChildren(false))
			{
				try
				{
					NMerchantHand val = (NMerchantHand)(object)((child is NMerchantHand) ? child : null);
					if (val != null && TryApplyToHand(val))
					{
						updated++;
					}
				}
				catch
				{
				}
				FindAndApplyRecursive(child, ref updated);
			}
		}

		private static bool TryApplyToHand(NMerchantHand hand)
		{
			try
			{
				return AnimationHelper.TryApplyVariantToHandNode((Node)(object)hand);
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] AnimationHelper failed: " + ex.Message);
			}
			return false;
		}

		private static void UpdateLegVisibility(bool show)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null && val.Root != null)
			{
				UpdateLegVisibilityStatic((Node)(object)val.Root, show);
			}
		}

		private static void UpdateLegVisibilityStatic(Node root, bool show)
		{
			if (root == null)
			{
				return;
			}
			foreach (Node child in root.GetChildren(false))
			{
				try
				{
					TextureRect val = (TextureRect)(object)((child is TextureRect) ? child : null);
					if (val != null && ((Node)val).Name == StringName.op_Implicit("MerchantInventoryLeg"))
					{
						((CanvasItem)val).Visible = show;
					}
				}
				catch
				{
				}
				UpdateLegVisibilityStatic(child, show);
			}
		}

		private static void LoadPersistedSetting(string filePath, Action<string> setter, params string[] allowedValues)
		{
			FileAccess val = FileAccess.Open(filePath, (ModeFlags)1);
			if (val == null)
			{
				return;
			}
			try
			{
				string text = val.GetLine().Trim().ToLowerInvariant();
				foreach (string text2 in allowedValues)
				{
					if (text == text2)
					{
						setter(text);
						break;
					}
				}
			}
			finally
			{
				val.Close();
			}
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
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			Harmony val = new Harmony("Merchant2CuteII");
			val.PatchAll();
			try
			{
				ResourcePreloader.PreloadAll();
				TaskHelper.RunSafely(ResourcePreloader.PreloadAudioPathsAsync(ModConfig.Paths.MerchantVoicePaths));
				TaskHelper.RunSafely(ResourcePreloader.PreloadSpinePathsAsync(ModConfig.Paths.SpinePaths));
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Resource preloading failed: " + ex.Message);
			}
			ConfigStore.Load();
		}
	}
}
namespace Merchant2CuteII.script
{
	internal static class AnimationHelper
	{
		private static readonly SemanticVersion NewSpineReadyVersion = new SemanticVersion(0, 108, 0, (string)null, (List<string>)null);

		public static bool TryApplyVariantToHandNode(Node handNode)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Expected O, but got Unknown
			if (handNode == null)
			{
				return false;
			}
			Node parent = handNode.GetParent();
			if (parent == null)
			{
				return false;
			}
			try
			{
				MegaSprite ms = new MegaSprite(Variant.op_Implicit((GodotObject)(object)parent));
				string variant = ModConfig.Options.HandVariant;
				if (ShouldUseLegacyHandSwitching())
				{
					return ApplyVariantLegacy(ms, variant);
				}
				if (!ms.IsAnimationStateReady())
				{
					SpineNodeExtensions.RunWhenSpineReady(handNode, ms, (Action<MegaAnimationState>)delegate(MegaAnimationState animState)
					{
						ApplyVariantToAnimationState(animState, ms, variant);
					});
					return true;
				}
				return ApplyVariantToAnimationState(ms.GetAnimationState(), ms, variant);
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] AnimationHelper.TryApplyVariantToHandNode failed: " + ex.Message);
			}
			return false;
		}

		public static bool ShouldUseLegacyHandSwitching()
		{
			SemanticVersion semVer = ReleaseInfoManager.Instance.SemVer;
			if (semVer == null)
			{
				return false;
			}
			return semVer.CompareTo(NewSpineReadyVersion) < 0;
		}

		private static bool ApplyVariantLegacy(MegaSprite sprite, string variant)
		{
			string text = ((variant == "hand") ? "default" : variant);
			if (sprite.HasAnimation(text))
			{
				return TrySetAnimationLegacy(sprite.GetAnimationState(), text, loop: false);
			}
			if ((variant == "white" || variant == "black") && sprite.HasAnimation("foot"))
			{
				return TrySetAnimationLegacy(sprite.GetAnimationState(), "foot", loop: false);
			}
			if (sprite.HasAnimation("default"))
			{
				return TrySetAnimationLegacy(sprite.GetAnimationState(), "default", loop: false);
			}
			return false;
		}

		private static bool ApplyVariantToAnimationState(MegaAnimationState animState, MegaSprite sprite, string variant)
		{
			if (animState == null)
			{
				return false;
			}
			string text = ((variant == "hand") ? "default" : variant);
			if (sprite.HasAnimation(text))
			{
				return TrySetAnimationNew(animState, text, loop: false);
			}
			if ((variant == "white" || variant == "black") && sprite.HasAnimation("foot"))
			{
				return TrySetAnimationNew(animState, "foot", loop: false);
			}
			if (sprite.HasAnimation("default"))
			{
				return TrySetAnimationNew(animState, "default", loop: false);
			}
			return false;
		}

		private static bool TrySetAnimationLegacy(MegaAnimationState animState, string animationName, bool loop, int trackId = 0)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			if (animState == null)
			{
				return false;
			}
			((MegaSpineBinding)animState).BoundObject.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
			{
				Variant.op_Implicit(animationName),
				Variant.op_Implicit(loop),
				Variant.op_Implicit(trackId)
			});
			return true;
		}

		private static bool TrySetAnimationNew(MegaAnimationState animState, string animationName, bool loop, int trackId = 0)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			if (animState == null)
			{
				return false;
			}
			((MegaSpineBinding)animState).BoundObject.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
			{
				Variant.op_Implicit(animationName),
				Variant.op_Implicit(loop),
				Variant.op_Implicit(trackId)
			});
			return true;
		}

		public static bool TrySetAnimationOnTarget(Node target, string animationName)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			if (target == null || string.IsNullOrEmpty(animationName))
			{
				return false;
			}
			try
			{
				MegaSprite val = new MegaSprite(Variant.op_Implicit((GodotObject)(object)target));
				if (val.HasAnimation(animationName))
				{
					if (ShouldUseLegacyHandSwitching())
					{
						return TrySetAnimationLegacy(val.GetAnimationState(), animationName, loop: false);
					}
					return TrySetAnimationNew(val.GetAnimationState(), animationName, loop: false);
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] AnimationHelper.TrySetAnimationOnTarget failed: " + ex.Message);
			}
			return false;
		}
	}
	public static class ConfigStore
	{
		private class ConfigData
		{
			public string HandVariant { get; set; } = "hand";

			public string MerchantVoiceVariant { get; set; } = "default";

			public float ExtraDb { get; set; } = 0f;

			public string FoulPotionAnimation { get; set; } = "poison";
		}

		private const string ConfigPath = "user://merchant2cute_config.json";

		public static void Load()
		{
			try
			{
				FileAccess val = FileAccess.Open("user://merchant2cute_config.json", (ModeFlags)1);
				if (val == null)
				{
					return;
				}
				string asText = val.GetAsText(false);
				val.Close();
				if (!string.IsNullOrEmpty(asText))
				{
					ConfigData configData = JsonSerializer.Deserialize<ConfigData>(asText);
					if (configData != null)
					{
						ModConfig.Options.HandVariant = configData.HandVariant;
						ModConfig.Options.MerchantVoiceVariant = configData.MerchantVoiceVariant;
						ModConfig.Options.ExtraDb = configData.ExtraDb;
						ModConfig.Options.FoulPotionAnimation = configData.FoulPotionAnimation;
					}
				}
			}
			catch (Exception)
			{
			}
		}

		public static void Save()
		{
			try
			{
				ConfigData value = new ConfigData
				{
					HandVariant = ModConfig.Options.HandVariant,
					MerchantVoiceVariant = ModConfig.Options.MerchantVoiceVariant,
					ExtraDb = ModConfig.Options.ExtraDb,
					FoulPotionAnimation = ModConfig.Options.FoulPotionAnimation
				};
				string text = JsonSerializer.Serialize(value, new JsonSerializerOptions
				{
					WriteIndented = true
				});
				FileAccess val = FileAccess.Open("user://merchant2cute_config.json", (ModeFlags)2);
				if (val != null)
				{
					val.StoreString(text);
					val.Close();
				}
			}
			catch (Exception)
			{
			}
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
		private static readonly SemanticVersion MerchantVisualOwnerChangeVersion = new SemanticVersion(0, 110, 0, (string)null, (List<string>)null);

		public static bool ShouldUseLegacyMerchantVisualLookup()
		{
			SemanticVersion semVer = ReleaseInfoManager.Instance.SemVer;
			if (semVer == null)
			{
				return true;
			}
			return semVer.CompareTo(MerchantVisualOwnerChangeVersion) < 0;
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
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
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
				val2.Position += GetMerchantVisualPositionOffset(__instance);
				val2.Scale = GetMerchantVisualScale(__instance);
			}
			return true;
		}

		private static Vector2 GetMerchantVisualPositionOffset(NMerchantButton button)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			return MerchantContextResolver.IsFakeMerchantButton(button) ? ModConfig.FakeMerchant.VisualPositionOffset : ModConfig.Merchant.VisualPositionOffset;
		}

		private static Vector2 GetMerchantVisualScale(NMerchantButton button)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			return MerchantContextResolver.IsFakeMerchantButton(button) ? ModConfig.FakeMerchant.VisualScale : ModConfig.Merchant.VisualScale;
		}
	}
	[HarmonyPatch(typeof(NMerchantRoom), "FoulPotionThrown")]
	public static class MerchantRoomFoulPotionPatch
	{
		[HarmonyPrefix]
		public static bool Prefix(NMerchantRoom __instance)
		{
			try
			{
				if (__instance == null)
				{
					return true;
				}
				string merchantVoicePath = ModConfig.Voice.GetMerchantVoicePath("merchant_thank_yous");
				if (!string.IsNullOrEmpty(merchantVoicePath) && ModConfig.Options.MerchantVoiceVariant != "default")
				{
					MerchantVoiceSfxPatch.TryPlayVoice(merchantVoicePath, 1f, ModConfig.Options.ExtraDb);
				}
				else
				{
					SfxCmd.Play("event:/sfx/npcs/merchant/merchant_thank_yous", 1f);
				}
				Node val = (Node)(MerchantVisualResolver.ShouldUseLegacyMerchantVisualLookup() ? ((object)((Node)__instance).GetNodeOrNull(NodePath.op_Implicit("%MerchantVisual"))) : ((object)(MerchantVisualResolver.TryGetMerchantVisual((Node?)(object)__instance.MerchantButton) ?? MerchantVisualResolver.TryGetMerchantVisual((Node?)(object)__instance))));
				if (val == null)
				{
					GD.PrintErr("[Merchant2CuteII] Cannot find MerchantVisual in MerchantRoom or MerchantButton");
					return false;
				}
				if (!AnimationHelper.TrySetAnimationOnTarget(val, ModConfig.Options.FoulPotionAnimation))
				{
					GD.PrintErr("[Merchant2CuteII] Merchant animation '" + ModConfig.Options.FoulPotionAnimation + "' does not exist or is not ready");
				}
				LocString val2 = Rng.Chaotic.NextItem<LocString>((IEnumerable<LocString>)MerchantRoom.Dialogue.FoulPotionLines);
				if (val2 != null)
				{
					NSpeechBubbleVfx val3 = __instance.MerchantButton.PlayDialogue(val2, 2.0);
					if (val3 != null)
					{
						NGame instance = NGame.Instance;
						if (instance != null)
						{
							instance.ScreenRumble((ShakeStrength)3, (ShakeDuration)1, (RumbleStyle)1);
						}
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Error adjusting FoulPotionThrown: " + ex.Message);
				return true;
			}
		}
	}
	[HarmonyPatch(typeof(SfxCmd), "Play", new Type[]
	{
		typeof(string),
		typeof(float)
	})]
	public static class MerchantVoiceSfxPatch
	{
		[HarmonyPrefix]
		public static bool Prefix(string sfx, float volume)
		{
			string merchantVoicePath = ModConfig.Voice.GetMerchantVoicePath(sfx);
			if (string.IsNullOrEmpty(merchantVoicePath))
			{
				return true;
			}
			try
			{
				if (TryPlayVoice(merchantVoicePath, volume, ModConfig.Options.ExtraDb))
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Merchant voice replacement failed: " + ex.Message);
			}
			return true;
		}

		public static bool TryPlayVoice(string voicePath, float volume, float extraDb = 0f)
		{
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Expected O, but got Unknown
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
	[HarmonyPatch(typeof(NMerchantCharacter), "PlayAnimation")]
	public static class MerchantCharacterPlayAnimationPatch
	{
		[HarmonyPrefix]
		public static bool Prefix(NMerchantCharacter __instance, string anim, bool loop)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected O, but got Unknown
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			if (__instance == null)
			{
				return false;
			}
			try
			{
				Node val = TryGetMerchantSpineNode(__instance);
				if (val == null)
				{
					GD.PrintErr("[Merchant2CuteII] Cannot find a compatible merchant spine node.");
					return false;
				}
				MegaSprite val2 = new MegaSprite(Variant.op_Implicit((GodotObject)(object)val));
				MegaAnimationState animationState = val2.GetAnimationState();
				if (AnimationHelper.ShouldUseLegacyHandSwitching())
				{
					((MegaSpineBinding)animationState).BoundObject.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[2]
					{
						Variant.op_Implicit(anim),
						Variant.op_Implicit(loop)
					});
				}
				else
				{
					((MegaSpineBinding)animationState).BoundObject.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
					{
						Variant.op_Implicit(anim),
						Variant.op_Implicit(loop),
						Variant.op_Implicit(0)
					});
				}
				if (loop)
				{
					MegaTrackEntry current = animationState.GetCurrent(0);
					if (current == null)
					{
						return false;
					}
					current.SetTrackTime(current.GetAnimationEnd() * Rng.Chaotic.NextFloat(1f));
				}
				return false;
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Safe merchant animation failed: " + ex.Message);
				return false;
			}
		}

		private static Node? TryGetMerchantSpineNode(NMerchantCharacter merchantCharacter)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
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
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Expected O, but got Unknown
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
					Visible = !ModConfig.Options.UseFootLike,
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
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			if (inventory != null)
			{
				Control nodeOrNull = ((Node)inventory).GetNodeOrNull<Control>(NodePath.op_Implicit("%SlotsContainer"));
				TextureRect nodeOrNull2 = ((Node)inventory).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("MerchantInventoryLeg"));
				if (nodeOrNull != null && nodeOrNull2 != null)
				{
					Vector2 val = default(Vector2);
					((Vector2)(ref val))..ctor(nodeOrNull.Position.X + ModConfig.Merchant.LegPosition.X, slotsTargetY + ModConfig.Merchant.LegPosition.Y);
					Tween val2 = ((Node)inventory).CreateTween();
					val2.TweenProperty((GodotObject)(object)nodeOrNull2, NodePath.op_Implicit("position"), Variant.op_Implicit(val), duration).SetEase(ease).SetTrans(transition)
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
			private static string _handVariant = "hand";

			private static string _merchantVoiceVariant = "default";

			private static float _extraDb = 0f;

			private static string _foulPotionAnimation = "poison";

			public static string HandVariant
			{
				get
				{
					return _handVariant;
				}
				set
				{
					_handVariant = NormalizeVariant(value);
				}
			}

			public static bool UseFoot => HandVariant == "foot";

			public static bool UseFootLike => HandVariant == "foot" || HandVariant == "white" || HandVariant == "black";

			public static string MerchantVoiceVariant
			{
				get
				{
					return _merchantVoiceVariant;
				}
				set
				{
					_merchantVoiceVariant = NormalizeVoiceVariant(value);
				}
			}

			public static bool UseMerchantJpVoice => MerchantVoiceVariant == "jp";

			public static bool UseMerchantZhVoice => MerchantVoiceVariant == "zh";

			public static float ExtraDb
			{
				get
				{
					return _extraDb;
				}
				set
				{
					_extraDb = Mathf.Clamp(value, -12f, 12f);
				}
			}

			public static string FoulPotionAnimation
			{
				get
				{
					return _foulPotionAnimation;
				}
				set
				{
					_foulPotionAnimation = (string.IsNullOrEmpty(value) ? "poison" : value);
				}
			}

			private static string NormalizeVariant(string? variant)
			{
				return (variant ?? string.Empty).Trim().ToLowerInvariant() switch
				{
					"foot" => "foot", 
					"white" => "white", 
					"black" => "black", 
					_ => "hand", 
				};
			}

			private static string NormalizeVoiceVariant(string? variant)
			{
				string text = (variant ?? string.Empty).Trim().ToLowerInvariant();
				if (text == "jp")
				{
					return "jp";
				}
				if (text == "zh")
				{
					return "zh";
				}
				return "default";
			}
		}

		public static class Voice
		{
			public static string? GetMerchantVoicePath(string sfxEvent)
			{
				string merchantVoiceVariant = Options.MerchantVoiceVariant;
				if (1 == 0)
				{
				}
				string[] array = ((merchantVoiceVariant == "jp") ? GetJpCandidates(sfxEvent) : ((!(merchantVoiceVariant == "zh")) ? Array.Empty<string>() : GetZhCandidates(sfxEvent)));
				if (1 == 0)
				{
				}
				string[] array2 = array;
				string[] array3 = array2;
				foreach (string text in array3)
				{
					if (ResourceLoader.Exists("res://debug_audio/" + text, ""))
					{
						return text;
					}
				}
				if (array2.Length != 0)
				{
					return array2[0];
				}
				return null;
			}

			private static string[] GetJpCandidates(string sfxEvent)
			{
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
				if (sfxEvent.Contains("merchant_welcome"))
				{
					return new string[1] { "zh/welcome1.wav" };
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
			return Mathf.DbToLinear(Options.ExtraDb);
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
					Resource resource = ResourceLoader.Load<Resource>(path, (string)null, (CacheMode)1);
					_cache[path] = resource;
					if (resource != null)
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
					Exception ex2 = ex;
					GD.PrintErr("[Merchant2CuteII] Preload audio exception for " + path + ": " + ex2.Message);
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
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Expected O, but got Unknown
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
							MegaSkeletonDataResource val2 = new MegaSkeletonDataResource(Variant.op_Implicit((GodotObject)(object)val));
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
			foreach (string p in paths)
			{
				if (string.IsNullOrEmpty(p) || _cache.ContainsKey(p))
				{
					continue;
				}
				try
				{
					Resource raw = ResourceLoader.Load<Resource>(p, (string)null, (CacheMode)1);
					if (raw != null)
					{
						try
						{
							new MegaSkeletonDataResource(Variant.op_Implicit((GodotObject)(object)raw));
							_cache[p] = raw;
							loaded++;
							GD.Print("[Merchant2CuteII] Preloaded spine resource: " + p);
						}
						catch
						{
							_cache[p] = raw;
							loaded++;
							GD.Print("[Merchant2CuteII] Preloaded raw resource (no wrapper): " + p);
						}
					}
					else
					{
						_cache[p] = null;
						GD.PrintErr("[Merchant2CuteII] Preload spine: resource not found " + p);
					}
				}
				catch (Exception ex)
				{
					Exception ex2 = ex;
					GD.PrintErr("[Merchant2CuteII] Preload spine exception for " + p + ": " + ex2.Message);
				}
				batch++;
				if (perFrame > 0 && batch >= perFrame && tree != null)
				{
					batch = 0;
					await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
				}
			}
			long elapsed = sw.ElapsedMilliseconds;
			GD.Print($"[Merchant2CuteII] PreloadSpinePathsAsync completed: loaded={loaded} timeMs={elapsed}");
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
namespace Merchant2CuteII.script.Patches
{
	[HarmonyPatch(typeof(NModInfoContainer), "Fill")]
	public static class ModdingScreenUiPatch
	{
		private const string ModId = "Merchant2CuteII";

		private const string PointButtonName = "Merchant2CuteII_PointVariant";

		private const string VoiceButtonName = "Merchant2CuteII_VoiceVariant";

		private const string VoiceVolumeButtonName = "Merchant2CuteII_VoiceVolume";

		private static readonly float[] VoiceVolumePresets = new float[6] { -12f, -6f, -3f, 0f, 3f, 6f };

		[HarmonyPostfix]
		public static void Postfix(NModInfoContainer __instance, Mod mod)
		{
			if (__instance == null)
			{
				return;
			}
			bool flag = string.Equals(mod?.manifest?.id, "Merchant2CuteII", StringComparison.OrdinalIgnoreCase);
			Button pointBtn = ((Node)__instance).GetNodeOrNull<Button>(NodePath.op_Implicit("Merchant2CuteII_PointVariant"));
			Button voiceBtn = ((Node)__instance).GetNodeOrNull<Button>(NodePath.op_Implicit("Merchant2CuteII_VoiceVariant"));
			Button volumeBtn = ((Node)__instance).GetNodeOrNull<Button>(NodePath.op_Implicit("Merchant2CuteII_VoiceVolume"));
			if (!flag)
			{
				if (pointBtn != null)
				{
					((CanvasItem)pointBtn).Visible = false;
				}
				if (voiceBtn != null)
				{
					((CanvasItem)voiceBtn).Visible = false;
				}
				if (volumeBtn != null)
				{
					((CanvasItem)volumeBtn).Visible = false;
				}
				return;
			}
			if (pointBtn == null)
			{
				pointBtn = CreateButton("Merchant2CuteII_PointVariant");
				((BaseButton)pointBtn).Pressed += delegate
				{
					try
					{
						TogglePointVariant();
						MerchantConsoleCmd.ApplyPointVariantNow();
						RefreshButtonText(pointBtn, voiceBtn, volumeBtn);
					}
					catch (Exception ex)
					{
						GD.PrintErr("[Merchant2CuteII] Failed to toggle point variant: " + ex.Message);
					}
				};
				((Node)__instance).AddChild((Node)(object)pointBtn, false, (InternalMode)0);
			}
			if (voiceBtn == null)
			{
				voiceBtn = CreateButton("Merchant2CuteII_VoiceVariant");
				((BaseButton)voiceBtn).Pressed += delegate
				{
					try
					{
						ToggleVoiceVariant();
						RefreshButtonText(pointBtn, voiceBtn, volumeBtn);
					}
					catch (Exception ex2)
					{
						GD.PrintErr("[Merchant2CuteII] Failed to toggle voice variant: " + ex2.Message);
					}
				};
				((Node)__instance).AddChild((Node)(object)voiceBtn, false, (InternalMode)0);
			}
			if (volumeBtn == null)
			{
				volumeBtn = CreateButton("Merchant2CuteII_VoiceVolume");
				((BaseButton)volumeBtn).Pressed += delegate
				{
					try
					{
						ToggleVoiceVolume();
						RefreshButtonText(pointBtn, voiceBtn, volumeBtn);
					}
					catch (Exception ex3)
					{
						GD.PrintErr("[Merchant2CuteII] Failed to toggle voice volume: " + ex3.Message);
					}
				};
				((Node)__instance).AddChild((Node)(object)volumeBtn, false, (InternalMode)0);
			}
			RefreshButtonText(pointBtn, voiceBtn, volumeBtn);
			UpdateButtonPositions((Control)(object)__instance, pointBtn, voiceBtn, volumeBtn);
			((CanvasItem)pointBtn).Visible = true;
			((CanvasItem)voiceBtn).Visible = true;
			((CanvasItem)volumeBtn).Visible = true;
		}

		private static Button CreateButton(string name)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			return new Button
			{
				Name = StringName.op_Implicit(name),
				FocusMode = (FocusModeEnum)2,
				Size = new Vector2(280f, 44f),
				ZIndex = 100,
				ZAsRelative = true
			};
		}

		private static void RefreshButtonText(Button? pointBtn, Button? voiceBtn, Button? volumeBtn)
		{
			string handVariant = ModConfig.Options.HandVariant;
			string merchantVoiceVariant = ModConfig.Options.MerchantVoiceVariant;
			float extraDb = ModConfig.Options.ExtraDb;
			float merchantVoiceVolumeLinear = ModConfig.GetMerchantVoiceVolumeLinear();
			if (pointBtn != null)
			{
				pointBtn.Text = "指向：" + handVariant + "（点击切换）";
			}
			if (voiceBtn != null)
			{
				voiceBtn.Text = "语音：" + merchantVoiceVariant + "（点击切换）";
			}
			if (volumeBtn != null)
			{
				volumeBtn.Text = $"语音音量：{extraDb:+0.##;-0.##;0} dB ({merchantVoiceVolumeLinear:0.00}x)";
			}
		}

		private static void UpdateButtonPositions(Control container, Button pointBtn, Button voiceBtn, Button volumeBtn)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			float num = 22f;
			float num2 = 18f;
			float num3 = 8f;
			Vector2 size = ((Control)pointBtn).Size;
			float num4 = size.Y + num3 + ((Control)voiceBtn).Size.Y + num3 + ((Control)volumeBtn).Size.Y;
			float y = ((container.Size.Y > 0f) ? (container.Size.Y - num4 - num2) : 930f);
			y = MathF.Max(0f, y);
			((Control)pointBtn).Position = new Vector2(num, y);
			((Control)voiceBtn).Position = new Vector2(num, y + size.Y + num3);
			((Control)volumeBtn).Position = new Vector2(num, y + (size.Y + num3) * 2f);
		}

		private static void TogglePointVariant()
		{
			string handVariant = ModConfig.Options.HandVariant;
			if (1 == 0)
			{
			}
			string text = handVariant switch
			{
				"hand" => "foot", 
				"foot" => "white", 
				"white" => "black", 
				_ => "hand", 
			};
			if (1 == 0)
			{
			}
			string handVariant2 = text;
			ModConfig.Options.HandVariant = handVariant2;
			TrySaveConfig();
		}

		private static void ToggleVoiceVariant()
		{
			string merchantVoiceVariant = ModConfig.Options.MerchantVoiceVariant;
			string merchantVoiceVariant2 = ((merchantVoiceVariant == "default") ? "jp" : ((merchantVoiceVariant == "jp") ? "zh" : "default"));
			ModConfig.Options.MerchantVoiceVariant = merchantVoiceVariant2;
			TrySaveConfig();
		}

		private static void ToggleVoiceVolume()
		{
			float extraDb = ModConfig.Options.ExtraDb;
			int num = Array.IndexOf(VoiceVolumePresets, extraDb);
			if (num < 0)
			{
				num = 0;
			}
			int num2 = (num + 1) % VoiceVolumePresets.Length;
			ModConfig.Options.ExtraDb = VoiceVolumePresets[num2];
			TrySaveConfig();
		}

		private static void TrySaveConfig()
		{
			try
			{
				ConfigStore.Save();
			}
			catch (Exception ex)
			{
				GD.PrintErr("[Merchant2CuteII] Failed to save config: " + ex.Message);
			}
		}
	}
}
