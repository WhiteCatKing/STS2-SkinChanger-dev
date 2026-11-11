using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CrusaderSkin.Code.Audio;
using CrusaderSkin.Code.Config;
using CrusaderSkin.Code.Helper;
using CrusaderSkin.Code.ModConfig;
using CrusaderSkin.Code.Nodes;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: TargetFramework(".NETCoreApp,Version=v9.0", FrameworkDisplayName = ".NET 9.0")]
[assembly: AssemblyCompany("CrusaderSkin")]
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyFileVersion("0.1.5")]
[assembly: AssemblyInformationalVersion("0.1.5+3e39fa97a6e855c4d5022e3b87b613ab05a52943")]
[assembly: AssemblyProduct("CrusaderSkin")]
[assembly: AssemblyTitle("CrusaderSkin")]
[assembly: AssemblyMetadata("RitsuLib.ModSettingsInterop.ProviderType", "CrusaderSkin.Code.ModConfig.RitsuLibModConfig")]
[assembly: AssemblyHasScripts(new Type[]
{
	typeof(EmptyNCreatureVisuals),
	typeof(EmptyNMerchantCharacter),
	typeof(EmptyNRestSiteCharacter),
	typeof(EmptyNSelectionReticle)
})]
[assembly: AssemblyVersion("0.1.5.0")]
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
namespace CrusaderSkin.Code
{
	[ModInitializer("Init")]
	public class Entry
	{
		public const string ModId = "CrusaderSkin";

		public static void Init()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			Harmony val = new Harmony("CrusaderSkin");
			val.PatchAll();
			ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);
			Log.Info("CrusaderSkinMod initialized!", 2);
		}
	}
}
namespace CrusaderSkin.Code.Singleton
{
	public class CrusaderCombatSingleton : CustomCrusaderSingletonModel
	{
		public CrusaderCombatSingleton()
			: base(HookType.Combat)
		{
		}

		public override Task BeforeCardPlayed(CardPlay cardPlay)
		{
			if (cardPlay != null && cardPlay.Card.Owner != null && CrusaderHelper.IsIronclad(cardPlay.Card.Owner))
			{
				string valueAsString = CrusaderConfigLoader.GetValueAsString(((AbstractModel)cardPlay.Card).Id.Entry);
				if (!string.IsNullOrEmpty(valueAsString))
				{
					return CreatureCmd.TriggerAnim(cardPlay.Card.Owner.Creature, "CardPlay/" + valueAsString, 0f);
				}
			}
			return Task.CompletedTask;
		}

		public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
		{
			if (CrusaderHelper.IsIronclad(creature))
			{
				if (delta > 0m)
				{
					if (!CrusaderHelper.IsLowHealth(creature) && CrusaderHelper.IsLowHealth(creature, 25m, delta) && CrusaderHelper.IsInAnyIdle(null, creature) && CrusaderSettings.UseLowHealthIdle)
					{
						return CreatureCmd.TriggerAnim(creature, "Revive", 0f);
					}
					if ((decimal)creature.CurrentHp - delta <= 0m && CrusaderHelper.IsLowHealth(creature) && CrusaderSettings.UseLowHealthIdle)
					{
						return CreatureCmd.TriggerAnim(creature, "DeathDoor", 0f);
					}
				}
				else if (delta < 0m && CrusaderHelper.IsLowHealth(creature) && !CrusaderHelper.IsLowHealth(creature, 25m, delta) && CrusaderHelper.IsInAnyIdle(null, creature) && CrusaderSettings.UseLowHealthIdle)
				{
					return CreatureCmd.TriggerAnim(creature, "DeathDoor", 0f);
				}
			}
			return Task.CompletedTask;
		}
	}
	public class CrusaderRunstateSingleton : CustomCrusaderSingletonModel
	{
		public CrusaderRunstateSingleton()
			: base(HookType.Run)
		{
		}

		public override Task AfterRoomEntered(AbstractRoom room)
		{
			CombatRoom val = (CombatRoom)(object)((room is CombatRoom) ? room : null);
			if (val != null)
			{
				foreach (Creature playerCreature in val.CombatState.PlayerCreatures)
				{
					if (CrusaderHelper.IsIronclad(playerCreature))
					{
						CrusaderHelper.InitCrusaderVisualAfterCombatStart(null, playerCreature);
					}
				}
			}
			return Task.CompletedTask;
		}
	}
	public abstract class CustomCrusaderSingletonModel : SingletonModel
	{
		public enum HookType
		{
			None,
			Combat,
			Run
		}

		public override bool ShouldReceiveCombatHooks { get; }

		public CustomCrusaderSingletonModel(HookType hookType)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Expected O, but got Unknown
			switch (hookType)
			{
			case HookType.None:
				break;
			case HookType.Combat:
				ShouldReceiveCombatHooks = true;
				ModHelper.SubscribeForCombatStateHooks(((AbstractModel)this).Id.Entry, new CombatHookSubscriptionDelegate(CombatSubModels));
				break;
			case HookType.Run:
				ModHelper.SubscribeForRunStateHooks(((AbstractModel)this).Id.Entry, new RunHookSubscriptionDelegate(RunSubModels));
				break;
			}
		}

		private IEnumerable<AbstractModel> RunSubModels(RunState runState)
		{
			return new <>z__ReadOnlySingleElementList<AbstractModel>((AbstractModel)(object)this);
		}

		private IEnumerable<AbstractModel> CombatSubModels(CombatState combatState)
		{
			return new <>z__ReadOnlySingleElementList<AbstractModel>((AbstractModel)(object)this);
		}
	}
}
namespace CrusaderSkin.Code.Patches
{
	[HarmonyPatch(typeof(NCreature), "SetAnimationTrigger")]
	public static class CrusaderAnimationPatch
	{
		private static readonly Dictionary<NCreature, Callable> _idleCallables = new Dictionary<NCreature, Callable>();

		private static readonly Callable _stateStartedCallable = Callable.From<StringName>((Action<StringName>)delegate(StringName state)
		{
			if (CrusaderSettings.PlayCardSfx)
			{
				float crusaderVolumeDB = CombatAudioCfg.GetCrusaderVolumeDB("CardPlay/" + StringName.op_Implicit(state));
				CombatAudioManager.PlayCombatSfx("CardPlay/" + StringName.op_Implicit(state), ((object)state).ToString().Contains("Recover"), bIsFullPathName: false, crusaderVolumeDB, 1);
			}
		});

		public static void Postfix(NCreature __instance, string trigger)
		{
			if (__instance.Entity == null || __instance.Entity.Player == null || !(__instance.Entity.Player.Character is Ironclad))
			{
				return;
			}
			if (!(trigger == "Hit"))
			{
				if ((trigger != null && trigger.Length == 0) || trigger == null)
				{
					return;
				}
				switch (trigger)
				{
				case "Attack":
					return;
				case "Cast":
					return;
				case "PowerUp":
					return;
				case "DoNothing":
					return;
				case "Dead":
					PlayAnim(__instance, "Dead", playImmediately: true);
					return;
				case "Revive":
					PlayAnim(__instance, "Revive");
					return;
				case "DeathDoor":
					PlayAnim(__instance, "DeathDoor");
					return;
				case "Idle":
					PlayAnim(__instance, "Idle", playImmediately: false, bHasChildStateMachine: true);
					return;
				}
				if (CrusaderSettings.PlayCardAnims)
				{
					PlayAnim(__instance, trigger, playImmediately: false, trigger.Contains("/"));
				}
			}
			else
			{
				PlayAnim(__instance, "Hit", playImmediately: true);
			}
		}

		private static void PlayAnim(NCreature node, string animName, bool playImmediately = false, bool bHasChildStateMachine = false)
		{
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0403: Expected O, but got Unknown
			//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fc: Expected O, but got Unknown
			//IL_053e: Unknown result type (might be due to invalid IL or missing references)
			//IL_055e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0564: Unknown result type (might be due to invalid IL or missing references)
			CrusaderHelper.SetBannerAndScriptureVisibility(node, animName);
			CrusaderHelper.SetBladeFlameVisibility(node, animName);
			NCreatureVisuals visuals = node.Visuals;
			if (visuals == null)
			{
				return;
			}
			AnimationPlayer nodeOrNull = ((Node)visuals).GetNodeOrNull<AnimationPlayer>(NodePath.op_Implicit("AnimationPlayer"));
			if (nodeOrNull == null)
			{
				return;
			}
			AnimationTree animTree = ((Node)visuals).GetNodeOrNull<AnimationTree>(NodePath.op_Implicit("AnimationTree"));
			if (animTree == null)
			{
				return;
			}
			CrusaderHelper.ResetAdvancedConditions(animTree, node.Entity);
			Variant val = ((GodotObject)animTree).Get(StringName.op_Implicit("parameters/playback"));
			AnimationNodeStateMachinePlayback state_machine = ((Variant)(ref val)).As<AnimationNodeStateMachinePlayback>();
			if (state_machine == null)
			{
				return;
			}
			if (!_idleCallables.TryGetValue(node, out var IdleCallable) || ((object)(Callable)(ref IdleCallable)/*cast due to .constrained prefix*/).Equals((object)default(Callable)) || !((GodotObject)state_machine).IsConnected(StringName.op_Implicit("state_started"), IdleCallable))
			{
				IdleCallable = Callable.From<StringName>((Action<StringName>)delegate(StringName state)
				{
					//IL_002e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0038: Unknown result type (might be due to invalid IL or missing references)
					//IL_003e: Expected O, but got Unknown
					//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
					if (state == StringName.op_Implicit("Idle"))
					{
						AnimationNodeStateMachinePlayback val2 = (AnimationNodeStateMachinePlayback)(GodotObject)((GodotObject)animTree).Get(StringName.op_Implicit("parameters/Idle/playback"));
						if (val2 != null)
						{
							val2.Travel(StringName.op_Implicit((CrusaderHelper.IsLowHealth(node.Entity) && CrusaderSettings.UseLowHealthIdle) ? "DeathIdle" : "Idle"), true);
							CrusaderHelper.SetBannerAndScriptureVisibility(node, "Idle");
							CrusaderHelper.SetBladeFlameVisibility(node, "Idle");
						}
						((GodotObject)state_machine).Disconnect(StringName.op_Implicit("state_started"), IdleCallable);
						_idleCallables.Remove(node);
					}
				});
				_idleCallables[node] = IdleCallable;
				((GodotObject)state_machine).Connect(StringName.op_Implicit("state_started"), IdleCallable, 0u);
			}
			if (!bHasChildStateMachine)
			{
				AnimationRootNode treeRoot = animTree.TreeRoot;
				AnimationNodeStateMachine val3 = (AnimationNodeStateMachine)(object)((treeRoot is AnimationNodeStateMachine) ? treeRoot : null);
				if (val3 == null || !val3.HasNode(StringName.op_Implicit(animName)))
				{
					return;
				}
				if (animName == "Hit")
				{
					if (state_machine.GetCurrentNode() == StringName.op_Implicit("Hit") || state_machine.GetCurrentNode() == StringName.op_Implicit("HitRecover") || state_machine.GetCurrentNode() == StringName.op_Implicit("DeathDoor"))
					{
						if ((state_machine.GetCurrentNode() == StringName.op_Implicit("Hit") || state_machine.GetCurrentNode() == StringName.op_Implicit("HitRecover")) && CrusaderHelper.IsLowHealth(node.Entity) && CrusaderSettings.UseLowHealthIdle)
						{
							state_machine.Start(StringName.op_Implicit("DeathDoor"), true);
						}
						return;
					}
					animName = (CrusaderHelper.IsHeavyHit(node.Entity) ? "HeavyHit" : animName);
					animName = ((CrusaderHelper.IsLowHealth(node.Entity) && CrusaderSettings.UseLowHealthIdle) ? "DeathDoor" : animName);
				}
				if (playImmediately)
				{
					state_machine.Start(StringName.op_Implicit(animName), true);
				}
				else
				{
					state_machine.Travel(StringName.op_Implicit(animName), true);
				}
			}
			if (!bHasChildStateMachine)
			{
				return;
			}
			if (animName.Contains("CardSelect/"))
			{
				AnimationNodeStateMachinePlayback val4 = (AnimationNodeStateMachinePlayback)(GodotObject)((GodotObject)animTree).Get(StringName.op_Implicit("parameters/CardSelect/playback"));
				if (val4 == null)
				{
					return;
				}
				string text = animName.Replace("CardSelect/", "");
				if (CrusaderHelper.CanTravelTo(text) && !string.IsNullOrEmpty(text) && text != "DoNothing")
				{
					state_machine.Start(StringName.op_Implicit("CardSelect"), true);
					val4.Travel(StringName.op_Implicit(text), true);
					if (CrusaderSettings.PlayCardSfx)
					{
						CombatAudioManager.PlayCombatSfx("CardSelect/" + text, bIsTemp: false, bIsFullPathName: false, CombatAudioCfg.GetCrusaderVolumeDB("CardSelect/" + text));
					}
				}
			}
			else if (animName.Contains("CardPlay/"))
			{
				AnimationNodeStateMachinePlayback val5 = (AnimationNodeStateMachinePlayback)(GodotObject)((GodotObject)animTree).Get(StringName.op_Implicit("parameters/CardPlay/playback"));
				if (val5 == null)
				{
					return;
				}
				string text2 = animName.Replace("CardPlay/", "");
				if (CrusaderHelper.CanTravelTo(text2))
				{
					if (!((GodotObject)val5).IsConnected(StringName.op_Implicit("state_started"), _stateStartedCallable))
					{
						((GodotObject)val5).Connect(StringName.op_Implicit("state_started"), _stateStartedCallable, 0u);
					}
					if (CrusaderHelper.ShouldShowBanner(animName) && CrusaderSettings.SwitchBannerImmediately)
					{
						state_machine.Start(StringName.op_Implicit("CardPlay"), true);
					}
					else
					{
						state_machine.Travel(StringName.op_Implicit("CardPlay"), true);
					}
					val5.Travel(StringName.op_Implicit(text2), true);
					CrusaderHelper.TryPlayCombatEffect(animName, node);
				}
			}
			else if (animName == "Idle" && !CrusaderHelper.IsInAnyIdle(animTree, node.Entity))
			{
				state_machine.Travel(StringName.op_Implicit("Idle"), true);
			}
		}
	}
	[HarmonyPatch(typeof(HoveredModelTracker), "OnLocalCardSelected")]
	public class CrusaderSkinOnSelectedPatch
	{
		public static void Postfix(CardModel cardModel)
		{
			if (CrusaderSettings.PlayCardAnims && cardModel != null && CrusaderHelper.IsIronclad(cardModel.Owner))
			{
				string valueAsString = CrusaderConfigLoader.GetValueAsString(((AbstractModel)cardModel).Id.Entry);
				if (!string.IsNullOrEmpty(valueAsString))
				{
					CreatureCmd.TriggerAnim(cardModel.Owner.Creature, "CardSelect/" + valueAsString, 0f);
				}
			}
		}
	}
	[HarmonyPatch(typeof(NMultiplayerPlayerIntentHandler), "RefreshHoverTips")]
	public static class CrusaderSkinOnCardSelectedPatch
	{
		private static CardModel? _lastSelectedCard;

		public static void Postfix(NMultiplayerPlayerIntentHandler __instance)
		{
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Invalid comparison between Unknown and I4
			if (!CrusaderSettings.ShowTeammateCardSelectAnim)
			{
				return;
			}
			Player value = Traverse.Create((object)__instance).Field("_player").GetValue<Player>();
			if (!CrusaderHelper.IsIronclad(value) || LocalContext.IsMe(value))
			{
				return;
			}
			NMultiplayerCardIntent value2 = Traverse.Create((object)__instance).Field("_cardIntent").GetValue<NMultiplayerCardIntent>();
			if (value2 != null && value2.Card != null && ((CanvasItem)value2).Visible)
			{
				CardModel card = value2.Card;
				if (card != null && !string.IsNullOrEmpty(CrusaderConfigLoader.GetValueAsString(((AbstractModel)card).Id.Entry)))
				{
					CreatureCmd.TriggerAnim(value.Creature, "CardSelect/" + CrusaderConfigLoader.GetValueAsString(((AbstractModel)card).Id.Entry), 0f);
				}
				else
				{
					CreatureCmd.TriggerAnim(value.Creature, "Idle", 0f);
				}
			}
			else
			{
				CardModel? lastSelectedCard = _lastSelectedCard;
				if (lastSelectedCard != null)
				{
					CardPile pile = lastSelectedCard.Pile;
					if ((int)((pile != null) ? new PileType?(pile.Type) : ((PileType?)null)).GetValueOrDefault() == 2)
					{
						CreatureCmd.TriggerAnim(value.Creature, "Idle", 0f);
					}
				}
			}
			_lastSelectedCard = ((value2 != null) ? value2.Card : null);
		}
	}
	[HarmonyPatch(typeof(NPlayerHand), "StartCardPlay")]
	public class TestCardPlayPatch
	{
		public static void Postfix(NPlayerHand __instance)
		{
		}
	}
	[HarmonyPatch(typeof(NCardPlay), "CancelPlayCard")]
	public class CrusaderSkinCancelPlayCardPatch
	{
		public static void Prefix(NCardPlay __instance)
		{
			if (CrusaderSettings.PlayCardAnims && GodotObject.IsInstanceValid((GodotObject)(object)__instance))
			{
				CardModel value = Traverse.Create((object)__instance).Property("Card", (object[])null).GetValue<CardModel>();
				if (value != null && CrusaderHelper.IsIronclad(value.Owner) && (MouseAndControllerPatch.isMouseCanceled || MouseAndControllerPatch.isControllerCanceled || !value.CanPlay()))
				{
					CreatureCmd.TriggerAnim(value.Owner.Creature, "Idle", 0f);
				}
			}
		}
	}
	[HarmonyPatch(typeof(NCardPlay), "OnCreatureUnhover")]
	public class CrusaderSkinOnCreatureUnhoverPatch
	{
		public static void Postfix(NCardPlay __instance, NCreature _)
		{
			if (__instance is NControllerCardPlay)
			{
				CardModel value = Traverse.Create((object)__instance).Property("Card", (object[])null).GetValue<CardModel>();
				if (value != null && CrusaderHelper.IsIronclad(value.Owner))
				{
					CreatureCmd.TriggerAnim(value.Owner.Creature, "Idle", 0f);
				}
			}
		}
	}
	[HarmonyPatch(typeof(AttackCommand), "Execute")]
	public class CrusaderSkinAttackCommandPatch
	{
		public static void Prefix(AttackCommand __instance)
		{
			if (CrusaderHelper.IsIronclad(__instance.Attacker))
			{
				__instance.WithNoAttackerAnim();
				if (CrusaderSettings.PlayCardAnims && CrusaderSettings.PlayCardSfx)
				{
					__instance.WithHitFx(__instance.HitVfx, (string)null, (string)null);
				}
			}
		}
	}
	[HarmonyPatch(typeof(NCreature), "StartDeathAnim")]
	public class CrusaderSkinDeathAnimPatch
	{
		public static void Postfix(NCreature __instance)
		{
			if (CrusaderHelper.IsIronclad(__instance.Entity.Player))
			{
				__instance.SetAnimationTrigger("Dead");
			}
		}
	}
	[HarmonyPatch(typeof(NCreature), "StartReviveAnim")]
	public class CrusaderSkinReviveAnimPatch
	{
		public static void Postfix(NCreature __instance)
		{
			if (CrusaderHelper.IsIronclad(__instance.Entity.Player))
			{
				__instance.SetAnimationTrigger("Revive");
			}
		}
	}
	public static class TscnPathGetter
	{
		public static string GetPath()
		{
			string currentSkin = CrusaderSettings.CurrentSkin;
			if (1 == 0)
			{
			}
			string result = currentSkin switch
			{
				"默认皮肤" => "", 
				"Default" => "", 
				"侵略者" => "_aggressor", 
				"Aggressor" => "_aggressor", 
				"方旗骑士" => "_banneret", 
				"Banneret" => "_banneret", 
				"圣殿骑士" => "_templar", 
				"Templar" => "_templar", 
				"渊狱" => "_infernal", 
				"Infernal" => "_infernal", 
				_ => "", 
			};
			if (1 == 0)
			{
			}
			return result;
		}
	}
	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	internal class CustomCharacterVisualPath
	{
		[HarmonyPrefix]
		private static bool UseCustomScene(CharacterModel __instance, ref string? __result)
		{
			if (!(__instance is Ironclad))
			{
				return true;
			}
			if (string.IsNullOrEmpty(CrusaderSettings.CurrentSkin))
			{
				return true;
			}
			if (CrusaderSettings.CurrentSkin == "默认皮肤" || CrusaderSettings.CurrentSkin == "Default")
			{
				return true;
			}
			__result = SceneHelper.GetScenePath("creature_visuals/ironclad" + TscnPathGetter.GetPath());
			return false;
		}
	}
	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	internal class RestSiteAnimPath
	{
		[HarmonyPrefix]
		private static bool Custom(CharacterModel __instance, ref string? __result)
		{
			if (!(__instance is Ironclad))
			{
				return true;
			}
			if (string.IsNullOrEmpty(CrusaderSettings.CurrentSkin))
			{
				return true;
			}
			if (CrusaderSettings.CurrentSkin == "默认皮肤" || CrusaderSettings.CurrentSkin == "Default")
			{
				return true;
			}
			__result = SceneHelper.GetScenePath("rest_site/characters/ironclad" + TscnPathGetter.GetPath() + "_rest_site");
			return false;
		}
	}
	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	internal class MerchantAnimPath
	{
		[HarmonyPrefix]
		private static bool Custom(CharacterModel __instance, ref string? __result)
		{
			if (!(__instance is Ironclad))
			{
				return true;
			}
			if (string.IsNullOrEmpty(CrusaderSettings.CurrentSkin))
			{
				return true;
			}
			if (CrusaderSettings.CurrentSkin == "默认皮肤" || CrusaderSettings.CurrentSkin == "Default")
			{
				return true;
			}
			__result = SceneHelper.GetScenePath("merchant/characters/ironclad" + TscnPathGetter.GetPath() + "_merchant");
			return false;
		}
	}
	public static class MouseAndControllerPatch
	{
		[HarmonyPatch(typeof(NMouseCardPlay), "_Input")]
		public static class MouseInputPatch
		{
			public static bool Prefix(InputEvent inputEvent)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Invalid comparison between Unknown and I8
				InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
				if (val != null)
				{
					MouseButton buttonIndex = val.ButtonIndex;
					if (true && (long)buttonIndex == 2 && ((InputEvent)val).IsPressed())
					{
						isMouseCanceled = true;
						return true;
					}
				}
				isMouseCanceled = false;
				return true;
			}
		}

		[HarmonyPatch(typeof(NControllerCardPlay), "_Input")]
		public static class ControllerInputPatch
		{
			public static bool Prefix(InputEvent inputEvent)
			{
				InputEventAction val = (InputEventAction)(object)((inputEvent is InputEventAction) ? inputEvent : null);
				if (val != null && (((InputEvent)val).IsActionPressed(MegaInput.cancel, false, false) || ((InputEvent)val).IsActionPressed(MegaInput.topPanel, false, false)))
				{
					isControllerCanceled = true;
					return true;
				}
				isControllerCanceled = false;
				return true;
			}
		}

		public static bool isMouseCanceled;

		public static bool isControllerCanceled;
	}
}
namespace CrusaderSkin.Code.Nodes
{
	[GlobalClass]
	[ScriptPath("res://CrusaderSkin/Code/Nodes/EmptyNCreatureVisuals.cs")]
	public class EmptyNCreatureVisuals : NCreatureVisuals
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
	[ScriptPath("res://CrusaderSkin/Code/Nodes/EmptyNMerchantCharacter.cs")]
	public class EmptyNMerchantCharacter : NMerchantCharacter
	{
		public class MethodName : MethodName
		{
			public static readonly StringName _Ready = StringName.op_Implicit("_Ready");
		}

		public class PropertyName : PropertyName
		{
		}

		public class SignalName : SignalName
		{
		}

		public override void _Ready()
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static List<MethodInfo> GetGodotMethodList()
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			List<MethodInfo> list = new List<MethodInfo>(1);
			list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null));
			return list;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if ((ref method) == MethodName._Ready && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				((Node)this)._Ready();
				ret = default(godot_variant);
				return true;
			}
			return ((NMerchantCharacter)this).InvokeGodotClassMethod(ref method, args, ref ret);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool HasGodotClassMethod(in godot_string_name method)
		{
			if ((ref method) == MethodName._Ready)
			{
				return true;
			}
			return ((NMerchantCharacter)this).HasGodotClassMethod(ref method);
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
	[ScriptPath("res://CrusaderSkin/Code/Nodes/EmptyNRestSiteCharacter.cs")]
	public class EmptyNRestSiteCharacter : NRestSiteCharacter
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
	[ScriptPath("res://CrusaderSkin/Code/Nodes/EmptyNSelectionReticle.cs")]
	public class EmptyNSelectionReticle : NSelectionReticle
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
namespace CrusaderSkin.Code.ModConfig
{
	public static class CrusaderSettings
	{
		public static float CardVolumeOffset => (float)RitsuLibModConfig.GetRitsuLibSettingDouble("CardVolumeOffset");

		public static bool PlayCardSfx => RitsuLibModConfig.GetRitsuLibSettingBool("PlayCardSfx");

		public static bool PlayCardAnims => RitsuLibModConfig.GetRitsuLibSettingBool("PlayCardAnims");

		public static bool PlayCardVfx => RitsuLibModConfig.GetRitsuLibSettingBool("PlayCardVfx");

		public static bool UseLowHealthIdle => RitsuLibModConfig.GetRitsuLibSettingBool("UseLowHealthIdle");

		public static bool SwitchBannerImmediately => RitsuLibModConfig.GetRitsuLibSettingBool("SwitchBannerImmediately");

		public static bool ShowTeammateCardSelectAnim => RitsuLibModConfig.GetRitsuLibSettingBool("ShowTeammateCardSelectAnim");

		public static string CurrentSkin => RitsuLibModConfig.GetRitsuLibSettingString("CurrentSkin");

		public static bool ToggleEnabled(string key)
		{
			return RitsuLibModConfig.GetRitsuLibSettingBool(key);
		}
	}
	public struct DropdownEntry
	{
		public string id { get; set; }

		public string type { get; }

		public string presentation { get; set; }

		public string key { get; set; }

		public string label { get; set; }

		public string defaultValue { get; set; }

		public string selected { get; set; }

		public Dictionary<string, string> options { get; set; }

		public RLMCScope scope { get; set; }

		public DropdownEntry()
		{
			options = null;
			id = null;
			type = "choice";
			presentation = "dropdown";
			key = null;
			label = null;
			defaultValue = "Default";
			selected = null;
			scope = RLMCScope.global;
		}
	}
	public static class RitsuLibModConfig
	{
		private const string ModId = "CrusaderSkin";

		private static readonly object FileLock = new object();

		private static readonly ConcurrentDictionary<string, JsonNode?> Hot = new ConcurrentDictionary<string, JsonNode>(StringComparer.Ordinal);

		private static readonly Dictionary<string, object> Defaults = new Dictionary<string, object>
		{
			["CardVolumeOffset"] = 0,
			["PlayCardSfx"] = true,
			["PlayCardAnims"] = true,
			["PlayCardVfx"] = true,
			["UseLowHealthIdle"] = true,
			["SwitchBannerImmediately"] = false,
			["ShowTeammateCardSelectAnim"] = true,
			["CurrentSkin"] = "Default"
		};

		private static string DataDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SlayTheSpire2", "CrusaderSkin");

		private static string SchemaPath => Path.Combine(DataDir, "ritsu_interop_schema.json");

		private static string StatePath => Path.Combine(DataDir, "ritsu_interop_state.json");

		public static object CreateRitsuLibSettingsSchema()
		{
			Directory.CreateDirectory(DataDir);
			File.WriteAllText(SchemaPath, BuildDefaultSchemaJson());
			return SchemaPath;
		}

		private static string BuildDefaultSchemaJson()
		{
			RitsuLibModConfigEntity ritsuLibModConfigEntity = new RitsuLibModConfigEntity();
			ritsuLibModConfigEntity.modDisplayName = SimpleLocUtil.Simple("十字军皮肤", "Crusader Skin");
			RitsuLibModConfigEntity value = ritsuLibModConfigEntity;
			RLMCPage item = new RLMCPage();
			item.pageId = "main";
			item.title = SimpleLocUtil.Simple("主要设置", "Main");
			item.description = SimpleLocUtil.Simple("主要设置", "Main");
			item.sortOrder = 1;
			RLMCSection item2 = new RLMCSection();
			item2.id = "skin_change";
			item2.title = SimpleLocUtil.Simple("切换十字军皮肤", "Change Crusader Skin");
			DropdownEntry dropdownEntry = new DropdownEntry();
			dropdownEntry.id = "CurrentSkin";
			dropdownEntry.key = dropdownEntry.id;
			dropdownEntry.label = SimpleLocUtil.Simple("十字军皮肤类型", "Crusader Skin Type");
			dropdownEntry.options = new Dictionary<string, string>
			{
				{
					"Initial",
					SimpleLocUtil.Simple("默认皮肤", "Default")
				},
				{
					"Aggressor",
					SimpleLocUtil.Simple("侵略者", "Aggressor")
				},
				{
					"Banneret",
					SimpleLocUtil.Simple("方旗骑士", "Banneret")
				},
				{
					"Templar",
					SimpleLocUtil.Simple("圣殿骑士", "Templar")
				},
				{
					"Infernal",
					SimpleLocUtil.Simple("渊狱", "Infernal")
				}
			};
			item2.entries.Add(dropdownEntry);
			RLMCSection item3 = new RLMCSection();
			item3.id = "card_sfx";
			item3.title = SimpleLocUtil.Simple("卡牌音效", "Card Sfx");
			SliderEntry sliderEntry = new SliderEntry();
			sliderEntry.id = "CardVolumeOffset";
			sliderEntry.key = sliderEntry.id;
			sliderEntry.label = SimpleLocUtil.Simple("卡牌音量调节(分贝)", "Card Volume DB Offset");
			sliderEntry.description = SimpleLocUtil.Simple("默认值0已经很大声，注意保护耳朵", "Default Value 0 DB is already loud. Be careful to protect your ears.");
			sliderEntry.min = -20.0;
			sliderEntry.max = 20.0;
			sliderEntry.step = 1.0;
			item3.entries.Add(sliderEntry);
			ToggleEntry toggleEntry = new ToggleEntry();
			toggleEntry.id = "PlayCardSfx";
			toggleEntry.key = toggleEntry.id;
			toggleEntry.label = SimpleLocUtil.Simple("播放卡牌音效", "Play Card Sfx");
			toggleEntry.description = SimpleLocUtil.Simple("关闭后会恢复至原版游戏默认攻击音效", "If unchecked, attack cards will use the original SFX.");
			item3.entries.Add(toggleEntry);
			RLMCSection item4 = new RLMCSection();
			item4.id = "card_vfx";
			item4.title = SimpleLocUtil.Simple("卡牌动画和特效", "Card Anim And Vfx");
			ToggleEntry toggleEntry2 = new ToggleEntry();
			toggleEntry2.id = "PlayCardAnims";
			toggleEntry2.key = toggleEntry2.id;
			toggleEntry2.label = SimpleLocUtil.Simple("播放卡牌动画", "Play Card Anims");
			toggleEntry2.description = SimpleLocUtil.Simple("关闭后会无法播放卡牌的选择和打出动画,卡牌特效以及音效", "If unchecked, card select/play anims, VFX and SFX wil not play.");
			item4.entries.Add(toggleEntry2);
			ToggleEntry toggleEntry3 = new ToggleEntry();
			toggleEntry3.id = "PlayCardVfx";
			toggleEntry3.key = toggleEntry3.id;
			toggleEntry3.label = SimpleLocUtil.Simple("播放卡牌特效", "Play Card Vfx");
			toggleEntry3.description = SimpleLocUtil.Simple("关闭后会无法播放卡牌的打出特效", "If unchecked, card play VFX wil not play.");
			item4.entries.Add(toggleEntry3);
			RLMCSection item5 = new RLMCSection();
			item5.id = "character_anim";
			item5.title = SimpleLocUtil.Simple("角色动画", "Character Anim");
			ToggleEntry toggleEntry4 = new ToggleEntry();
			toggleEntry4.id = "UseLowHealthIdle";
			toggleEntry4.key = toggleEntry4.id;
			toggleEntry4.label = SimpleLocUtil.Simple("展示低血量站立动画", "Show Low Health Idle Anim");
			toggleEntry4.description = SimpleLocUtil.Simple("关闭后不会使用低血量站立动画", "If unchecked, low health idle anim wil not play.");
			item5.entries.Add(toggleEntry4);
			ToggleEntry toggleEntry5 = new ToggleEntry();
			toggleEntry5.id = "SwitchBannerImmediately";
			toggleEntry5.key = toggleEntry5.id;
			toggleEntry5.label = SimpleLocUtil.Simple("立即切换旗帜动画", "Switch Banner Immediately");
			toggleEntry5.description = SimpleLocUtil.Simple("开启后播放旗帜动画时不会混合过渡", "If checked, banner anims will have no mixed transition.");
			item5.entries.Add(toggleEntry5);
			ToggleEntry toggleEntry6 = new ToggleEntry();
			toggleEntry6.id = "ShowTeammateCardSelectAnim";
			toggleEntry6.key = toggleEntry6.id;
			toggleEntry6.label = SimpleLocUtil.Simple("展示队友十字军的选卡动画", "Show Teammate Crusader Card Select Anim");
			toggleEntry6.description = SimpleLocUtil.Simple("关闭后不会播放队友十字军的选卡动画", "If unchecked, teammate Crusader card select anims will not play.");
			item5.entries.Add(toggleEntry6);
			item.sections.Add(item2);
			item.sections.Add(item3);
			item.sections.Add(item4);
			item.sections.Add(item5);
			value.pages.Add(item);
			return JsonSerializer.Serialize(value);
		}

		public static void SetRitsuLibSettingValue(string key, object? value)
		{
			SetCore(key, value);
		}

		public static object? GetRitsuLibSettingValue(string key)
		{
			return GetCore(key);
		}

		public static void SaveRitsuLibSettings()
		{
			lock (FileLock)
			{
				string statePath = StatePath;
				JsonObject jsonObject = new JsonObject((JsonNodeOptions?)null);
				foreach (KeyValuePair<string, JsonNode> item in Hot)
				{
					jsonObject[item.Key] = ((item.Value == null) ? null : JsonNode.Parse(item.Value.ToJsonString(), null));
				}
				File.WriteAllText(statePath, jsonObject.ToJsonString(new JsonSerializerOptions
				{
					WriteIndented = true
				}));
			}
		}

		public static bool GetRitsuLibSettingBool(string key)
		{
			return CoerceBool(GetCore(key));
		}

		public static void SetRitsuLibSettingBool(string key, bool value)
		{
			SetCore(key, value);
		}

		public static double GetRitsuLibSettingDouble(string key)
		{
			return CoerceDouble(GetCore(key));
		}

		public static void SetRitsuLibSettingDouble(string key, double value)
		{
			SetCore(key, value);
		}

		public static int GetRitsuLibSettingInt(string key)
		{
			return CoerceInt(GetCore(key));
		}

		public static void SetRitsuLibSettingInt(string key, int value)
		{
			SetCore(key, value);
		}

		public static string? GetRitsuLibSettingString(string key)
		{
			return GetCore(key)?.ToString();
		}

		public static void SetRitsuLibSettingString(string key, string value)
		{
			SetCore(key, value);
		}

		public static void InvokeRitsuLibSettingAction(string key)
		{
			if (!(key == "reset_all"))
			{
				return;
			}
			Hot.Clear();
			try
			{
				if (File.Exists(StatePath))
				{
					File.Delete(StatePath);
				}
			}
			catch
			{
			}
		}

		private static void SetCore(string key, object? value)
		{
			LoadIfNeeded();
			Hot[key] = JsonSerializer.SerializeToNode(value);
		}

		private static object? GetCore(string key)
		{
			LoadIfNeeded();
			if (!Hot.TryGetValue(key, out JsonNode value) || value == null)
			{
				return Defaults.GetValueOrDefault(key);
			}
			if (value is JsonValue jsonValue)
			{
				return jsonValue.GetValue<object>();
			}
			return value;
		}

		private static void LoadIfNeeded()
		{
			if (Hot.Count > 0)
			{
				return;
			}
			lock (FileLock)
			{
				if (Hot.Count > 0 || !File.Exists(StatePath))
				{
					return;
				}
				JsonObject jsonObject = JsonNode.Parse(File.ReadAllText(StatePath), null)?.AsObject();
				if (jsonObject == null)
				{
					return;
				}
				foreach (KeyValuePair<string, JsonNode> item in jsonObject)
				{
					Hot[item.Key] = item.Value;
				}
			}
		}

		private static bool CoerceBool(object? o)
		{
			if (o is bool)
			{
				if ((bool)o)
				{
					return true;
				}
				return false;
			}
			if (o != null)
			{
				if (o is JsonValue jsonValue)
				{
					bool value;
					return jsonValue.TryGetValue<bool>(out value) && value;
				}
				bool result;
				return bool.TryParse(o.ToString(), out result) && result;
			}
			return false;
		}

		private static double CoerceDouble(object? o)
		{
			if (o != null)
			{
				if (!(o is JsonValue jsonValue))
				{
					if (o is IConvertible value)
					{
						return Convert.ToDouble(value);
					}
					double result;
					return double.TryParse(o.ToString(), out result) ? result : 0.0;
				}
				return jsonValue.GetValue<double>();
			}
			return 0.0;
		}

		private static int CoerceInt(object? o)
		{
			if (o != null)
			{
				if (!(o is JsonValue jsonValue))
				{
					if (o is IConvertible value)
					{
						return Convert.ToInt32(value);
					}
					int result;
					return int.TryParse(o.ToString(), out result) ? result : 0;
				}
				return jsonValue.GetValue<int>();
			}
			return 0;
		}
	}
	public struct RitsuLibModConfigEntity
	{
		public string modId { get; set; }

		public string modDisplayName { get; set; }

		public int modSidebarOrder { get; set; }

		public List<RLMCPage> pages { get; set; }

		public RitsuLibModConfigEntity()
		{
			modId = "CrusaderSkin";
			modDisplayName = null;
			modSidebarOrder = 50;
			pages = new List<RLMCPage>();
		}
	}
	public struct RLMCPage
	{
		public string pageId { get; set; }

		public string title { get; set; }

		public string description { get; set; }

		public int sortOrder { get; set; }

		public List<RLMCSection> sections { get; set; }

		public RLMCPage()
		{
			pageId = null;
			title = null;
			description = null;
			sortOrder = 50;
			sections = new List<RLMCSection>();
		}
	}
	public enum RLMCScope
	{
		global,
		profile
	}
	public struct RLMCSection
	{
		public string id { get; set; }

		public string title { get; set; }

		public List<object> entries { get; set; }

		public RLMCSection()
		{
			id = null;
			title = null;
			entries = new List<object>();
		}
	}
	public static class SimpleLocUtil
	{
		public static string Simple(string chs, string eng)
		{
			LocManager instance = LocManager.Instance;
			string text = ((instance != null) ? instance.Language : null);
			if (!(text == "zhs") && !(text == "zht"))
			{
				return eng;
			}
			return chs;
		}
	}
	public struct SliderEntry
	{
		public string id { get; set; }

		public string type { get; }

		public string key { get; set; }

		public string label { get; set; }

		public string description { get; set; }

		public double min { get; set; }

		public double max { get; set; }

		public double step { get; set; }

		public RLMCScope scope { get; set; }

		public SliderEntry()
		{
			id = null;
			type = "slider";
			key = null;
			label = null;
			description = null;
			min = 0.0;
			max = 0.0;
			step = 0.0;
			scope = RLMCScope.global;
		}
	}
	public struct ToggleEntry
	{
		public string id { get; set; }

		public string type { get; }

		public string key { get; set; }

		public string label { get; set; }

		public string description { get; set; }

		public RLMCScope scope { get; set; }

		public ToggleEntry()
		{
			id = null;
			type = "toggle";
			key = null;
			label = null;
			description = null;
			scope = RLMCScope.global;
		}
	}
}
namespace CrusaderSkin.Code.Helper
{
	public static class CrusaderHelper
	{
		public static bool IsIronclad(Creature? creature)
		{
			return creature != null && creature.Player != null && creature.Player.Character is Ironclad;
		}

		public static bool IsIronclad(Player? player)
		{
			return player != null && player.Character is Ironclad;
		}

		public static bool IsCardOfIronclad(CardModel? cardModel)
		{
			return cardModel != null && cardModel.Pool is IroncladCardPool;
		}

		public static bool IsLowHealth(Creature? creature, decimal lowHealthPercent = 25m, decimal delta = 0m)
		{
			return creature != null && ((decimal)creature.CurrentHp - delta) * 100m / (decimal)creature.MaxHp <= lowHealthPercent;
		}

		public static bool IsHeavyHit(Creature? creature, decimal threshold = 30m)
		{
			if (creature == null)
			{
				return false;
			}
			return CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>().LastOrDefault((Func<DamageReceivedEntry, bool>)((DamageReceivedEntry e) => (decimal)e.Result.TotalDamage >= threshold && (decimal)e.Result.UnblockedDamage > 0m && e.Receiver == creature)) != null;
		}

		public static void ResetAdvancedConditions(AnimationTree? animTree, Creature creature)
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			AnimationTree val = animTree;
			if (val == null && creature != null)
			{
				NCreature creatureNode = creature.GetCreatureNode();
				if (creatureNode != null)
				{
					val = ((Node)creatureNode.Visuals).GetNodeOrNull<AnimationTree>(NodePath.op_Implicit("AnimationTree"));
				}
			}
			if (val != null)
			{
				((GodotObject)val).Set(StringName.op_Implicit("parameters/conditions/HitToIdle"), Variant.op_Implicit(true));
				((GodotObject)val).Set(StringName.op_Implicit("parameters/conditions/HitToDeathIdle"), Variant.op_Implicit(false));
			}
		}

		public static bool IsInAnyIdle(AnimationTree? animTree, Creature creature)
		{
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Expected O, but got Unknown
			AnimationTree val = animTree;
			if (val == null && creature != null)
			{
				NCreature creatureNode = creature.GetCreatureNode();
				if (creatureNode != null)
				{
					val = ((Node)creatureNode.Visuals).GetNodeOrNull<AnimationTree>(NodePath.op_Implicit("AnimationTree"));
				}
			}
			if (val != null)
			{
				AnimationNodeStateMachinePlayback val2 = (AnimationNodeStateMachinePlayback)(GodotObject)((GodotObject)val).Get(StringName.op_Implicit("parameters/playback"));
				if (val2 != null)
				{
					return val2.GetCurrentNode() == StringName.op_Implicit("Idle");
				}
			}
			return false;
		}

		public static void InitCrusaderVisualAfterCombatStart(AnimationTree? tree, Creature creature)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Expected O, but got Unknown
			AnimationTree val = tree;
			NCreature val2 = null;
			if (val == null && creature != null)
			{
				val2 = creature.GetCreatureNode();
				if (val2 != null)
				{
					val = ((Node)val2.Visuals).GetNodeOrNull<AnimationTree>(NodePath.op_Implicit("AnimationTree"));
				}
			}
			if (val == null || val2 == null)
			{
				return;
			}
			AnimationNodeStateMachinePlayback val3 = (AnimationNodeStateMachinePlayback)(GodotObject)((GodotObject)val).Get(StringName.op_Implicit("parameters/playback"));
			if (val3 != null)
			{
				AnimationNodeStateMachinePlayback val4 = (AnimationNodeStateMachinePlayback)(GodotObject)((GodotObject)val).Get(StringName.op_Implicit("parameters/Idle/playback"));
				if (val4 != null && IsLowHealth(val2.Entity) && CrusaderSettings.UseLowHealthIdle)
				{
					val3.Start(StringName.op_Implicit("Idle"), true);
					val4.Travel(StringName.op_Implicit("DeathIdle"), true);
				}
			}
			SetBannerAndScriptureVisibility(val2, "Idle");
			SetBladeFlameVisibility(val2, "Idle");
		}

		public static void SetBannerAndScriptureVisibility(NCreature nCreature, string animName)
		{
			if (nCreature != null)
			{
				Node3D nodeOrNull = ((Node)nCreature.Visuals).GetNodeOrNull<Node3D>(NodePath.op_Implicit("Visuals/SubViewportContainer/SubViewport/mdl_crusader/mdl_crusader (merge)/mdl_crusader/SHJntGrp/Skeleton3D/msh_crusader_wpn_banner"));
				Node3D nodeOrNull2 = ((Node)nCreature.Visuals).GetNodeOrNull<Node3D>(NodePath.op_Implicit("Visuals/SubViewportContainer/SubViewport/mdl_crusader/mdl_crusader (merge)/mdl_crusader/SHJntGrp/Skeleton3D/msh_crusader_wpn_scripture"));
				if (nodeOrNull != null)
				{
					nodeOrNull.Visible = ShouldShowBanner(animName);
				}
				if (nodeOrNull2 != null)
				{
					nodeOrNull2.Visible = ShouldShowScripture(animName);
				}
			}
		}

		public static void SetBladeFlameVisibility(NCreature nCreature, string animName)
		{
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			if (nCreature != null)
			{
				GpuParticles3D nodeOrNull = ((Node)nCreature.Visuals).GetNodeOrNull<GpuParticles3D>(NodePath.op_Implicit("Visuals/SubViewportContainer/SubViewport/mdl_crusader/mdl_crusader (merge)/mdl_crusader/SHJntGrp/Skeleton3D/BladeAttachment/BladeFlameLeft"));
				GpuParticles3D nodeOrNull2 = ((Node)nCreature.Visuals).GetNodeOrNull<GpuParticles3D>(NodePath.op_Implicit("Visuals/SubViewportContainer/SubViewport/mdl_crusader/mdl_crusader (merge)/mdl_crusader/SHJntGrp/Skeleton3D/BladeAttachment/BladeFlameRight"));
				MeshInstance3D nodeOrNull3 = ((Node)nCreature.Visuals).GetNodeOrNull<MeshInstance3D>(NodePath.op_Implicit("Visuals/SubViewportContainer/SubViewport/mdl_crusader/mdl_crusader (merge)/mdl_crusader/SHJntGrp/Skeleton3D/BladeAttachment/BladeGlowBody"));
				if (nodeOrNull != null && nodeOrNull2 != null)
				{
					nodeOrNull.Emitting = ShouldShowBladeFlame(animName);
					nodeOrNull2.Emitting = ShouldShowBladeFlame(animName);
					((Node3D)nodeOrNull).Position = new Vector3(((Node3D)nodeOrNull).Position.X, GetBladeFlameParticlePositionY(animName), ((Node3D)nodeOrNull).Position.Z);
					((Node3D)nodeOrNull2).Position = new Vector3(((Node3D)nodeOrNull2).Position.X, GetBladeFlameParticlePositionY(animName), ((Node3D)nodeOrNull2).Position.Z);
				}
				if (nodeOrNull3 != null)
				{
					((Node3D)nodeOrNull3).Visible = ShouldShowBladeFlame(animName);
				}
			}
		}

		public static bool ShouldShowScripture(string animName)
		{
			if (!animName.Contains("CardPlay/"))
			{
				return false;
			}
			if (animName.Contains("Accus") || animName.Contains("Heal"))
			{
				return true;
			}
			return false;
		}

		public static bool ShouldShowBanner(string animName)
		{
			if (!animName.Contains("CardPlay/"))
			{
				return false;
			}
			if (animName.Contains("Insp") || animName.Contains("Rally") || animName.Contains("Tenacity"))
			{
				return true;
			}
			return false;
		}

		public static bool ShouldShowBladeFlame(string animName)
		{
			if (animName == "CardSelect/Holy" || animName == "CardPlay/Holy" || animName == "CardPlay/Radiance")
			{
				return true;
			}
			return false;
		}

		public static void TryPlayCombatEffect(string animName, NCreature nCreature)
		{
			//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02aa: Expected O, but got Unknown
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			if (!CrusaderSettings.PlayCardVfx)
			{
				return;
			}
			string text = "";
			bool flag = false;
			bool flag2 = true;
			switch (animName)
			{
			default:
				return;
			case "CardPlay/Smite":
				text = "res://crusader_assets/Effects/SmiteEffect.tscn";
				flag2 = false;
				break;
			case "CardPlay/Reap":
				text = "res://crusader_assets/Effects/ReapEffect.tscn";
				flag2 = false;
				break;
			case "CardPlay/Bulwark":
				text = "res://crusader_assets/Effects/SelfGlow.tscn";
				flag = true;
				break;
			case "CardPlay/Tenacity":
				text = "res://crusader_assets/Effects/SelfGlow.tscn";
				flag = true;
				break;
			case "CardPlay/Insp":
				text = "res://crusader_assets/Effects/BirdGlow.tscn";
				flag = true;
				flag2 = false;
				break;
			case "CardPlay/Rally":
				text = "res://crusader_assets/Effects/DustFlow.tscn";
				flag2 = false;
				break;
			}
			Node2D effectNode = PreloadManager.Cache.GetScene(text).Instantiate<Node2D>((GenEditState)0);
			if (effectNode == null)
			{
				return;
			}
			if (effectNode != null && ((Node)effectNode).GetParent() == null && NCombatRoom.Instance != null && nCreature != null)
			{
				Control val = (flag ? NCombatRoom.Instance.BackCombatVfxContainer : NCombatRoom.Instance.CombatVfxContainer);
				((Node)val).AddChild((Node)(object)effectNode, false, (InternalMode)0);
				Marker2D nodeOrNull = ((Node)nCreature.Visuals).GetNodeOrNull<Marker2D>(NodePath.op_Implicit("%CenterPos"));
				effectNode.GlobalPosition = ((nodeOrNull != null && flag2) ? (((Node2D)nodeOrNull).GlobalPosition + GetEffectNodeOffSet(animName, nCreature)) : ((Control)nCreature).GlobalPosition);
				Node2D obj = effectNode;
				obj.Scale *= ((Node2D)nCreature.Visuals).Scale;
				Node2D nodeOrNull2 = ((Node)nCreature.Visuals).GetNodeOrNull<Node2D>(NodePath.op_Implicit("%Visuals"));
				if (nodeOrNull2 != null && nodeOrNull2.Scale.X < 0f)
				{
					Node2D obj2 = effectNode;
					obj2.Scale *= new Vector2(-1f, 1f);
				}
				if (!flag)
				{
					Node2D obj3 = effectNode;
					obj3.Scale *= nCreature.Entity.CombatState.Encounter.GetCameraScaling();
				}
			}
			AnimationPlayer nodeOrNull3 = ((Node)effectNode).GetNodeOrNull<AnimationPlayer>(NodePath.op_Implicit("%AnimationPlayer"));
			if (nodeOrNull3 == null)
			{
				return;
			}
			nodeOrNull3.Stop(false);
			nodeOrNull3.Play(StringName.op_Implicit("Show"), -1.0, 1f, false);
			((AnimationMixer)nodeOrNull3).AnimationFinished += (AnimationFinishedEventHandler)delegate
			{
				if (effectNode != null && !((GodotObject)effectNode).IsQueuedForDeletion())
				{
					GodotTreeExtensions.QueueFreeSafely((Node)(object)effectNode);
				}
			};
		}

		private static Vector2 GetEffectNodeOffSet(string animName, NCreature nCreature)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			if (nCreature == null)
			{
				return Vector2.Zero;
			}
			Control nodeOrNull = ((Node)nCreature.Visuals).GetNodeOrNull<Control>(NodePath.op_Implicit("%Bounds"));
			if (nodeOrNull == null)
			{
				return Vector2.Zero;
			}
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			if (!(animName == "CardPlay/Bulwark"))
			{
				if (animName == "CardPlay/Tenacity")
				{
					num3 = 0f;
					num4 = -2.5f;
				}
			}
			else
			{
				num3 = 1.2f;
				num4 = -2.5f;
			}
			num = ((num3 != 0f) ? (nodeOrNull.Size.X * (num3 / 10f) * Math.Min(1f, ((Node2D)nCreature.Visuals).Scale.X)) : 0f);
			num2 = ((num4 != 0f) ? (nodeOrNull.Size.Y * (num4 / 10f) * Math.Min(1f, ((Node2D)nCreature.Visuals).Scale.Y)) : 0f);
			return new Vector2(num, num2);
		}

		private static float GetBladeFlameParticlePositionY(string animName)
		{
			float num = 0f;
			if (animName == "CardPlay/Radiance")
			{
				return -0.1f;
			}
			return 0.1f;
		}

		public static bool CanTravelTo(string nodeName)
		{
			switch (nodeName)
			{
			case "Smite":
			case "Stun":
			case "Accus":
			case "Heal":
			case "Insp":
			case "Rally":
			case "Tenacity":
			case "Reap":
			case "Bulwark":
			case "Radiance":
			case "Holy":
			case "Mercy":
				return true;
			default:
				return false;
			}
		}
	}
}
namespace CrusaderSkin.Code.Config
{
	public static class CrusaderConfigLoader
	{
		private const string InternalConfigPath = "res://CrusaderAnimConfig.json";

		private static Dictionary _cachedConfig;

		public static Dictionary GetCachedConfig()
		{
			if (_cachedConfig == null)
			{
				_cachedConfig = LoadConfig();
			}
			return _cachedConfig;
		}

		public static Dictionary LoadConfig()
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Invalid comparison between Unknown and I8
			string text = GetExternalConfigJson();
			if (text == null)
			{
				text = LoadInternalConfig();
				if (text != null)
				{
					GD.Print("Loaded config from internal (res://)");
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				GD.PushError("Config file not found in either external or internal path.");
				return null;
			}
			Variant val = Json.ParseString(text);
			if ((long)((Variant)(ref val)).VariantType == 0)
			{
				GD.PushError("Invalid JSON format.");
				return null;
			}
			return ((Variant)(ref val)).As<Dictionary>();
		}

		private static string? GetExternalConfigJson()
		{
			string result = null;
			Mod val = ModManager.GetLoadedMods().FirstOrDefault((Func<Mod, bool>)((Mod m) => m.manifest?.id == "CrusaderSkin"));
			if (val != null)
			{
				string path = Path.Combine(val.path, "CrusaderAnimConfig.json");
				if (File.Exists(path))
				{
					result = File.ReadAllText(path);
				}
			}
			return result;
		}

		private static string LoadInternalConfig()
		{
			if (!ResourceLoader.Exists("res://CrusaderAnimConfig.json", ""))
			{
				GD.PushError("Internal config not found: res://CrusaderAnimConfig.json");
				return null;
			}
			FileAccess val = FileAccess.Open("res://CrusaderAnimConfig.json", (ModeFlags)1);
			try
			{
				if (val == null)
				{
					return null;
				}
				return val.GetAsText(false);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}

		public static Variant GetValue(string key, Variant defaultValue = default(Variant))
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			Dictionary cachedConfig = GetCachedConfig();
			if (cachedConfig == null || !cachedConfig.ContainsKey(Variant.op_Implicit(key)))
			{
				return defaultValue;
			}
			return cachedConfig[Variant.op_Implicit(key)];
		}

		public static string GetValueAsString(string key)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			Dictionary cachedConfig = GetCachedConfig();
			if (cachedConfig == null || !cachedConfig.ContainsKey(Variant.op_Implicit(key)))
			{
				return "";
			}
			Variant val = cachedConfig[Variant.op_Implicit(key)];
			return ((Variant)(ref val)).AsString();
		}
	}
}
namespace CrusaderSkin.Code.Audio
{
	public static class CombatAudioCfg
	{
		public static string GetCrusaderPath(string AudioName)
		{
			string text = "";
			string text2 = "res://crusader_assets/Sound_Crusader/";
			string text3 = ".wav";
			switch (AudioName)
			{
			case "CardSelect/Smite":
				text = "sfx_hero_cru_smite_antic";
				break;
			case "CardSelect/Stun":
				text = "sfx_hero_cru_stun_antic";
				break;
			case "CardSelect/Accus":
			case "CardSelect/Heal":
				text = "sfx_hero_cru_heal_antic";
				break;
			case "CardSelect/Insp":
			case "CardSelect/Rally":
			case "CardSelect/Tenacity":
				text = "sfx_hero_cru_insp_antic";
				break;
			case "CardSelect/Reap":
				text = "sfx_hero_cru_reap_antic";
				break;
			case "CardSelect/Bulwark":
			case "CardSelect/Radiance":
				text = "sfx_hero_cru_bulwark_antic";
				break;
			case "CardSelect/Holy":
				text = "sfx_hero_cru_holy_antic";
				break;
			case "CardSelect/Mercy":
				text = "sfx_hero_cru_mercy_antic";
				break;
			case "CardPlay/Smite":
				text = "sfx_hero_cru_smite_use";
				break;
			case "CardPlay/Stun":
				text = "sfx_hero_cru_stun_use";
				break;
			case "CardPlay/Accus":
				text = "sfx_hero_cru_accus_use";
				break;
			case "CardPlay/Insp":
				text = "sfx_hero_cru_insp_use";
				break;
			case "CardPlay/Rally":
				text = "sfx_hero_cru_rallying_use";
				break;
			case "CardPlay/Reap":
				text = "sfx_hero_cru_reap_use";
				break;
			case "CardPlay/Heal":
				text = "sfx_hero_cru_heal_use";
				break;
			case "CardPlay/Bulwark":
				text = "sfx_hero_cru_bulwark_use";
				break;
			case "CardPlay/Holy":
				text = "sfx_hero_cru_holy_use";
				break;
			case "CardPlay/Radiance":
				text = "sfx_hero_cru_radiance_use";
				break;
			case "CardPlay/Tenacity":
				text = "sfx_hero_cru_tenacity_use";
				break;
			case "CardPlay/Mercy":
				text = "sfx_hero_cru_insp_use";
				break;
			case "CardPlay/Smite_Recover":
				text = "sfx_hero_cru_smite_return";
				break;
			case "CardPlay/Stun_Recover":
				text = "sfx_hero_cru_stun_return";
				break;
			case "CardPlay/Accus_Recover":
			case "CardPlay/Heal_Recover":
				text = "sfx_hero_cru_heal_return";
				break;
			case "CardPlay/Insp_Recover":
			case "CardPlay/Rally_Recover":
			case "CardPlay/Tenacity_Recover":
				text = "sfx_hero_cru_insp_return";
				break;
			case "CardPlay/Reap_Recover":
				text = "sfx_hero_cru_bulwark_return";
				break;
			case "CardPlay/Bulwark_Recover":
			case "CardPlay/Radiance_Recover":
				text = "sfx_hero_cru_bulwark_return";
				break;
			case "CardPlay/Holy_Recover":
				text = "sfx_hero_cru_holy_return";
				break;
			case "CardPlay/Mercy_Recover":
				text = "sfx_hero_cru_insp_return";
				break;
			}
			return (text == "") ? text : (text2 + text + text3);
		}

		public static float GetCrusaderVolumeDB(string AudioName)
		{
			float num = -10f;
			switch (AudioName)
			{
			case "CardSelect/Smite":
			case "CardSelect/Stun":
			case "CardSelect/Accus":
			case "CardSelect/Heal":
			case "CardSelect/Insp":
			case "CardSelect/Rally":
			case "CardSelect/Tenacity":
			case "CardSelect/Reap":
			case "CardSelect/Bulwark":
			case "CardSelect/Radiance":
			case "CardSelect/Holy":
			case "CardSelect/Mercy":
				return -10f;
			case "CardPlay/Smite":
			case "CardPlay/Stun":
				return -10f;
			case "CardPlay/Accus":
				return -10f;
			case "CardPlay/Insp":
				return -6f;
			case "CardPlay/Rally":
				return -4f;
			case "CardPlay/Reap":
				return -10f;
			case "CardPlay/Heal":
				return -8f;
			case "CardPlay/Bulwark":
				return -8f;
			case "CardPlay/Holy":
				return -6f;
			case "CardPlay/Radiance":
				return -7f;
			case "CardPlay/Tenacity":
				return -8f;
			case "CardPlay/Mercy":
				return -6f;
			case "CardPlay/Smite_Recover":
			case "CardPlay/Stun_Recover":
				return -14f;
			case "CardPlay/Accus_Recover":
			case "CardPlay/Heal_Recover":
				return -4f;
			case "CardPlay/Insp_Recover":
			case "CardPlay/Rally_Recover":
			case "CardPlay/Tenacity_Recover":
			case "CardPlay/Mercy_Recover":
				return -10f;
			case "CardPlay/Reap_Recover":
				return -4f;
			case "CardPlay/Bulwark_Recover":
			case "CardPlay/Radiance_Recover":
				return -4f;
			case "CardPlay/Holy_Recover":
				return -12f;
			default:
				return -10f;
			}
		}
	}
	public static class CombatAudioManager
	{
		private static AudioStreamPlayer? _playerAudioPlayer;

		private static AudioStreamPlayer? _secondPlayerAudioPlayer;

		public static AudioStreamPlayer PlayerAudioPlayer
		{
			get
			{
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Expected O, but got Unknown
				if (!GodotObject.IsInstanceValid((GodotObject)(object)_playerAudioPlayer))
				{
					_playerAudioPlayer = new AudioStreamPlayer();
				}
				return _playerAudioPlayer;
			}
		}

		public static AudioStreamPlayer SecondPlayerAudioPlayer
		{
			get
			{
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Expected O, but got Unknown
				if (!GodotObject.IsInstanceValid((GodotObject)(object)_secondPlayerAudioPlayer))
				{
					_secondPlayerAudioPlayer = new AudioStreamPlayer();
				}
				return _secondPlayerAudioPlayer;
			}
		}

		public static void PlayCombatSfx(string AudioName, bool bIsTemp = false, bool bIsFullPathName = false, float VolumeDb = -10f, int audioPlayerIndex = 0)
		{
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			if (NonInteractiveMode.IsActive)
			{
				return;
			}
			string text = (bIsFullPathName ? AudioName : CombatAudioCfg.GetCrusaderPath(AudioName));
			if (text == null || text == "")
			{
				return;
			}
			AudioStream asset;
			try
			{
				asset = PreloadManager.Cache.GetAsset<AudioStream>(text);
			}
			catch
			{
				Log.Info("[FlagellantCombatAudioManager] Could not load audio: " + text, 2);
				return;
			}
			AudioStreamPlayer audioPlayer = (AudioStreamPlayer)((!bIsTemp) ? ((object)GetPlayerAudioPlayer(audioPlayerIndex)) : ((object)new AudioStreamPlayer()));
			audioPlayer.VolumeDb = 0f;
			AudioStreamPlayer obj2 = audioPlayer;
			obj2.VolumeDb += VolumeDb + CrusaderSettings.CardVolumeOffset;
			audioPlayer.Stream = asset;
			audioPlayer.Bus = StringName.op_Implicit("SFX");
			if (bIsTemp)
			{
				audioPlayer.Finished += delegate
				{
					GodotTreeExtensions.QueueFreeSafely((Node)(object)audioPlayer);
				};
			}
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance != null)
			{
				if (((Node)audioPlayer).GetParent() == null)
				{
					((Node)instance).AddChild((Node)(object)audioPlayer, false, (InternalMode)0);
				}
				audioPlayer.Play(0f);
			}
			else if (bIsTemp)
			{
				GodotTreeExtensions.QueueFreeSafely((Node)(object)audioPlayer);
			}
		}

		private static AudioStreamPlayer GetPlayerAudioPlayer(int index = 0)
		{
			if (index == 0 || index != 1)
			{
				return PlayerAudioPlayer;
			}
			return SecondPlayerAudioPlayer;
		}
	}
}
[CompilerGenerated]
internal sealed class <>z__ReadOnlySingleElementList<T> : IEnumerable, ICollection, IList, IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection<T>, IList<T>
{
	private sealed class Enumerator : IDisposable, IEnumerator, IEnumerator<T>
	{
		object IEnumerator.Current => _item;

		T IEnumerator<T>.Current => _item;

		public Enumerator(T item)
		{
			_item = item;
		}

		bool IEnumerator.MoveNext()
		{
			return !_moveNextCalled && (_moveNextCalled = true);
		}

		void IEnumerator.Reset()
		{
			_moveNextCalled = false;
		}

		void IDisposable.Dispose()
		{
		}
	}

	int ICollection.Count => 1;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	object? IList.this[int index]
	{
		get
		{
			if (index != 0)
			{
				throw new IndexOutOfRangeException();
			}
			return _item;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	bool IList.IsFixedSize => true;

	bool IList.IsReadOnly => true;

	int IReadOnlyCollection<T>.Count => 1;

	T IReadOnlyList<T>.this[int index]
	{
		get
		{
			if (index != 0)
			{
				throw new IndexOutOfRangeException();
			}
			return _item;
		}
	}

	int ICollection<T>.Count => 1;

	bool ICollection<T>.IsReadOnly => true;

	T IList<T>.this[int index]
	{
		get
		{
			if (index != 0)
			{
				throw new IndexOutOfRangeException();
			}
			return _item;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public <>z__ReadOnlySingleElementList(T item)
	{
		_item = item;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(_item);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		array.SetValue(_item, index);
	}

	int IList.Add(object? value)
	{
		throw new NotSupportedException();
	}

	void IList.Clear()
	{
		throw new NotSupportedException();
	}

	bool IList.Contains(object? value)
	{
		return EqualityComparer<T>.Default.Equals(_item, (T)value);
	}

	int IList.IndexOf(object? value)
	{
		return (!EqualityComparer<T>.Default.Equals(_item, (T)value)) ? (-1) : 0;
	}

	void IList.Insert(int index, object? value)
	{
		throw new NotSupportedException();
	}

	void IList.Remove(object? value)
	{
		throw new NotSupportedException();
	}

	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(_item);
	}

	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException();
	}

	bool ICollection<T>.Contains(T item)
	{
		return EqualityComparer<T>.Default.Equals(_item, item);
	}

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		array[arrayIndex] = _item;
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException();
	}

	int IList<T>.IndexOf(T item)
	{
		return (!EqualityComparer<T>.Default.Equals(_item, item)) ? (-1) : 0;
	}

	void IList<T>.Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}
}
