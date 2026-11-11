using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using HarmonyLib;
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
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.sts2.Core.Nodes.TopBar;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: InternalsVisibleTo("JoanofArcIroncladHpwearTests")]
[assembly: TargetFramework(".NETCoreApp,Version=v9.0", FrameworkDisplayName = ".NET 9.0")]
[assembly: AssemblyCompany("JoanofArcIroncladHpwear")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("0.3.2.0")]
[assembly: AssemblyInformationalVersion("0.3.2+36e53a5b59ed3d4505211407661d1488a210961d")]
[assembly: AssemblyProduct("JoanofArcIroncladHpwear")]
[assembly: AssemblyTitle("JoanofArcIroncladHpwear")]
[assembly: AssemblyVersion("0.3.2.0")]
[module: RefSafetyRules(11)]
namespace JoanofArcIroncladHpwear;

internal static class AppearanceHotkey
{
	private static Node? _receiver;

	private static readonly HashSet<Key> Down = new HashSet<Key>();

	internal static void Start(SceneTree tree)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (_receiver == null || !GodotObject.IsInstanceValid((GodotObject)(object)_receiver))
		{
			GDScript val = new GDScript
			{
				SourceCode = "extends Node\nsignal key_event(event)\nfunc _input(event):\n key_event.emit(event)\n"
			};
			if ((int)((Script)val).Reload(false) != 0)
			{
				throw new InvalidOperationException("appearance input script failed");
			}
			Node val2 = new Node
			{
				Name = StringName.op_Implicit("JoanAppearanceInput"),
				ProcessMode = (ProcessModeEnum)3
			};
			((GodotObject)val2).SetScript(Variant.op_Implicit((GodotObject)(object)val));
			((GodotObject)val2).Connect(StringName.op_Implicit("key_event"), Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent e)
			{
				Handle(tree, e);
			}), 0u);
			((Node)tree.Root).AddChild(val2, false, (InternalMode)0);
			val2.SetProcessInput(true);
			tree.Root.FocusExited += Down.Clear;
			_receiver = val2;
		}
	}

	private static void Handle(SceneTree tree, InputEvent input)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		InputEventKey val = (InputEventKey)(object)((input is InputEventKey) ? input : null);
		if (val == null)
		{
			return;
		}
		Key item = (((int)val.PhysicalKeycode != 0) ? val.PhysicalKeycode : val.Keycode);
		if (!val.Pressed)
		{
			Down.Remove(item);
		}
		else
		{
			if (val.Echo || !Down.Add(item))
			{
				return;
			}
			try
			{
				if (WearSettingsRuntime.PersistAppearance != null && !Editing(((Viewport)tree.Root).GuiGetFocusOwner()) && Matches(WearSettingsRuntime.Current.AppearanceHotkey, val) && SkinSelectionGate.SceneAllowed() && Palette.HasVisibleTarget())
				{
					int obj = Math.Clamp(WearSettingsRuntime.Current.Appearance, 1, Palette.Names.Length) % Palette.Names.Length + 1;
					WearSettingsRuntime.PersistAppearance(obj);
					Palette.RefreshAppearance(tree);
					((Viewport)tree.Root).SetInputAsHandled();
				}
			}
			catch (Exception ex)
			{
				ModEntry.TryInfo("appearance hotkey: " + ex.GetBaseException().Message);
			}
		}
	}

	private static bool Editing(Control? focus)
	{
		for (Node val = (Node)(object)focus; val != null; val = val.GetParent())
		{
			if ((val is LineEdit || val is TextEdit) ? true : false)
			{
				return true;
			}
			string text = ((object)val).GetType().FullName ?? "";
			if (text.Contains("KeyBinding", StringComparison.Ordinal) || text.Contains("InputBinding", StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool Matches(string? binding, InputEventKey input)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(binding))
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		Key result = (Key)0;
		string[] array = binding.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		foreach (string text in array)
		{
			switch (text.ToLowerInvariant())
			{
			case "ctrl":
			case "control":
				flag = true;
				continue;
			case "alt":
				flag2 = true;
				continue;
			case "shift":
				flag3 = true;
				continue;
			case "meta":
			case "command":
			case "cmd":
			case "win":
			case "super":
				flag4 = true;
				continue;
			}
			if ((int)result != 0)
			{
				return false;
			}
			if (text == "=")
			{
				result = (Key)61;
			}
			else if (!Enum.TryParse<Key>(text, ignoreCase: true, out result) || (int)result == 0)
			{
				return false;
			}
		}
		if ((int)result != 0 && (input.Keycode == result || input.PhysicalKeycode == result) && ((InputEventWithModifiers)input).CtrlPressed == flag && ((InputEventWithModifiers)input).AltPressed == flag2 && ((InputEventWithModifiers)input).ShiftPressed == flag3)
		{
			return ((InputEventWithModifiers)input).MetaPressed == flag4;
		}
		return false;
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
		if (!string.Equals(cardTypeName, "Omnislice", StringComparison.Ordinal))
		{
			return "Cast";
		}
		return "Attack";
	}
}
public static class CardPlayAnimTracker
{
	private const int MaxPending = 64;

	private static readonly ConcurrentDictionary<object, bool> SawByPlay = new ConcurrentDictionary<object, bool>();

	private static object? _current;

	internal static int PendingCount => SawByPlay.Count;

	public static void Begin(object play)
	{
		ArgumentNullException.ThrowIfNull(play, "play");
		_current = play;
		SawByPlay[play] = false;
		if (SawByPlay.Count > 64)
		{
			Clear();
		}
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
		if (play != null && SawByPlay.TryGetValue(play, out var value))
		{
			return !value;
		}
		return false;
	}

	public static bool ShouldInjectCast(object? play)
	{
		if (play == null || !SawByPlay.TryRemove(play, out var value))
		{
			return false;
		}
		if (_current == play)
		{
			_current = null;
		}
		return !value;
	}

	public static void Clear()
	{
		SawByPlay.Clear();
		_current = null;
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
		if (PlayAllowed(cardPlay))
		{
			CardPlayAnimTracker.Begin(cardPlay);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CreatureCmd), "TriggerAnim")]
	private static void TriggerAnimPrefix(Creature creature, ref string triggerName)
	{
		if (creature != null && creature.IsPlayer && PlayerSelected(creature.Player))
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
		try
		{
			Creature val = ((__instance != null) ? __instance.Attacker : null);
			if (((val != null) ? val.Player : null) != null && PlayerSelected(val.Player) && !((Creature)(((object)/*isinst with value type is only supported in some contexts*/) ?? ((object)val))).IsPlayer)
			{
				CardPlayAnimTracker.NotePlayerAnim();
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
		if (creature == null || !creature.IsPlayer || !PlayerSelected(creature.Player))
		{
			return;
		}
		try
		{
			CardModel val = ((cardPlay != null) ? cardPlay.Card : null);
			if (val != null && CardPlayAnimTracker.NeedsCast(cardPlay) && ShouldHandle(val))
			{
				Player owner = val.Owner;
				if (((owner != null) ? owner.Creature : null) != null && creature == owner.Creature)
				{
					InjectDefaultAnim(creature, val, "GainBlock");
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
	private static void AfterCardPlayedPrefix(CardPlay cardPlay)
	{
		TryInjectAfterPlay(cardPlay, "AfterCardPlayed");
	}

	private static void TryInjectAfterPlay(CardPlay cardPlay, string reason)
	{
		if (cardPlay == null)
		{
			return;
		}
		try
		{
			CardModel card = cardPlay.Card;
			if (card == null || !ShouldHandle(card))
			{
				CardPlayAnimTracker.ShouldInjectCast(cardPlay);
			}
			else if (CardPlayAnimTracker.ShouldInjectCast(cardPlay))
			{
				Player owner = card.Owner;
				Creature val = ((owner != null) ? owner.Creature : null);
				if (val != null && !val.IsDead)
				{
					InjectDefaultAnim(val, card, reason);
				}
			}
		}
		catch
		{
		}
	}

	private static void InjectDefaultAnim(Creature creature, CardModel card, string reason)
	{
		string text = CardAnimDefault.ResolveMissingForCard(((object)card).GetType().Name);
		NCreature creatureNode = creature.GetCreatureNode();
		if (creatureNode == null)
		{
			return;
		}
		try
		{
			Player player = creature.Player;
			CharacterModel val = ((player != null) ? player.Character : null);
			if (creature.IsPlayer && val != null)
			{
				SfxCmd.Play((text == "Attack") ? val.AttackSfx : val.CastSfx, 1f);
			}
		}
		catch
		{
		}
		creatureNode.SetAnimationTrigger(text);
		ModEntry.TryInfo($"[card-anim] {((object)card).GetType().Name} -> {text} ({reason})");
	}

	private static bool ShouldHandle(CardModel card)
	{
		try
		{
			Player owner = card.Owner;
			if (((owner != null) ? owner.Creature : null) == null || !owner.Creature.IsPlayer)
			{
				return false;
			}
			return PlayerSelected(owner);
		}
		catch
		{
			return false;
		}
	}

	private static bool PlayAllowed(CardPlay? play)
	{
		if (play == null)
		{
			return false;
		}
		try
		{
			CardModel card = play.Card;
			Player val = ((card != null) ? card.Owner : null);
			return (val != null) ? PlayerSelected(val) : SkinSelectionGate.SceneAllowed();
		}
		catch
		{
			return false;
		}
	}

	private static bool PlayerSelected(Player? player)
	{
		try
		{
			if (player == null || !(player.Character is Ironclad))
			{
				return false;
			}
			return SkinSelectionGate.PlayerAllowed(player);
		}
		catch
		{
			return false;
		}
	}
}
internal static class CharSelectCycle
{
	private const string CatcherName = "JoanofArcIroncladHpwearClickCatcher";

	private const string PollerName = "JoanofArcIroncladHpwearInputPoller";

	private const string IdleAnimation = "animation";

	private static FieldInfo? _bgContainerField;

	private static int _stage = 1;

	private static readonly Resource?[] StageResources = (Resource?[])(object)new Resource[4];

	private static readonly bool[] Loading = new bool[4];

	internal static int CurrentStage
	{
		get
		{
			if (!WearSettingsRuntime.Current.WearEnabled)
			{
				return 1;
			}
			return _stage;
		}
	}

	internal static void Attach(Control bg)
	{
		try
		{
			InstallCatcher(bg);
			if (!SkinSelectionGate.MenuAllowed())
			{
				return;
			}
			Node val = SpineSwap.FindSpine((Node?)(object)bg);
			if (val == null)
			{
				ModEntry.Log.Info("char-select: no SpineSprite found — cycling off.", 1);
				return;
			}
			if (!SpineSwap.IsOurs(val))
			{
				ModEntry.Log.Info("char-select: live skeleton is not this mod's (" + (SpineSwap.GetSkeletonPath(val) ?? "none") + ") — cycling off.", 1);
				return;
			}
			BeginPreload();
			if (!WearSettingsRuntime.Current.WearEnabled)
			{
				_stage = 1;
			}
			Apply(bg, val, _stage, replayIdle: true);
			ModEntry.Log.Info($"char-select: cycling armed (stage {_stage}/3).", 1);
		}
		catch (Exception value)
		{
			ModEntry.Log.Error($"char-select attach failed: {value}", 1);
		}
	}

	private static bool Apply(Control bg, Node spine, int stage, bool replayIdle)
	{
		Resource stageData = GetStageData(stage);
		if (stageData == null)
		{
			return false;
		}
		Resource skeletonData = SpineSwap.GetSkeletonData(spine);
		if (!SpineSwap.SetSkeletonData(spine, stageData))
		{
			ModEntry.Log.Error($"char-select: stage {stage} did not apply.", 1);
			if (skeletonData != null)
			{
				SpineSwap.SetSkeletonData(spine, skeletonData);
			}
			return false;
		}
		if (replayIdle)
		{
			SpineSwap.Play(spine, "animation", loop: true);
		}
		return true;
	}

	private static void InstallCatcher(Control bg)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		if (!((Node)bg).HasNode(new NodePath("JoanofArcIroncladHpwearClickCatcher")))
		{
			Control val = new Control
			{
				Name = StringName.op_Implicit("JoanofArcIroncladHpwearClickCatcher"),
				MouseFilter = (MouseFilterEnum)2
			};
			val.SetAnchorsPreset((LayoutPreset)15, false);
			val.GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
			{
				OnInput(bg, input);
			};
			((Node)bg).AddChild((Node)(object)val, false, (InternalMode)0);
			((Node)bg).MoveChild((Node)(object)val, ((Node)bg).GetChildCount(false) - 1);
			InstallPoller(bg);
		}
	}

	private static void InstallPoller(Control bg)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		if (((Node)bg).HasNode(new NodePath("JoanofArcIroncladHpwearInputPoller")))
		{
			return;
		}
		Timer poller = new Timer
		{
			Name = StringName.op_Implicit("JoanofArcIroncladHpwearInputPoller"),
			WaitTime = 0.03,
			OneShot = false,
			Autostart = true,
			ProcessCallback = (TimerProcessCallback)1,
			ProcessMode = (ProcessModeEnum)0
		};
		((Node)bg).AddChild((Node)(object)poller, false, (InternalMode)0);
		((Node)bg).TreeExiting += delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)poller))
			{
				poller.Stop();
				((Node)poller).QueueFree();
			}
			Array.Clear(StageResources, 0, StageResources.Length);
		};
		poller.Timeout += delegate
		{
			Poll(bg);
		};
		poller.Start(-1.0);
		ModEntry.Log.Info("char-select: click catcher + poller installed.", 1);
	}

	private static void BeginPreload()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I8
		for (int i = 1; i <= 3; i++)
		{
			if (StageResources[i] == null && !Loading[i])
			{
				Error val = ResourceLoader.LoadThreadedRequest(SkinCatalog.DataPath("charselect", i), "", false, (CacheMode)1);
				Loading[i] = (long)val == 0;
			}
		}
	}

	private static Resource? GetStageData(int stage)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I8
		if (stage < 1 || stage > 3)
		{
			return null;
		}
		if (StageResources[stage] != null && GodotObject.IsInstanceValid((GodotObject)(object)StageResources[stage]))
		{
			return StageResources[stage];
		}
		string text = SkinCatalog.DataPath("charselect", stage);
		if (Loading[stage] && (long)ResourceLoader.LoadThreadedGetStatus(text, (Array)null) == 3)
		{
			Loading[stage] = false;
			StageResources[stage] = ResourceLoader.LoadThreadedGet(text);
		}
		if (StageResources[stage] == null)
		{
			StageResources[stage] = SpineSwap.LoadStageData("charselect", stage);
		}
		return StageResources[stage];
	}

	private static void Poll(Control bg)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I8
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		try
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)bg))
			{
				return;
			}
			for (int i = 1; i <= 3; i++)
			{
				if (Loading[i] && (long)ResourceLoader.LoadThreadedGetStatus(SkinCatalog.DataPath("charselect", i), (Array)null) == 3)
				{
					StageResources[i] = ResourceLoader.LoadThreadedGet(SkinCatalog.DataPath("charselect", i));
					Loading[i] = false;
				}
			}
			if (!((CanvasItem)bg).IsVisibleInTree())
			{
				return;
			}
			Control nodeOrNull = ((Node)bg).GetNodeOrNull<Control>(new NodePath("JoanofArcIroncladHpwearClickCatcher"));
			Node val = SpineSwap.FindSpine((Node?)(object)bg);
			bool flag = SkinSelectionGate.MenuAllowed() && val != null && SpineSwap.IsOurs(val);
			if (nodeOrNull != null)
			{
				nodeOrNull.MouseFilter = (MouseFilterEnum)(flag ? 0 : 2);
			}
			if (flag)
			{
				BeginPreload();
				WearSettingsRuntime.TickHotkey();
				if (!WearSettingsRuntime.Current.WearEnabled && val != null && (_stage != 1 || SkinCatalog.StageFromPath("charselect", SpineSwap.GetSkeletonPath(val)) != 1) && Apply(bg, val, 1, replayIdle: true))
				{
					_stage = 1;
				}
			}
		}
		catch (Exception value)
		{
			ModEntry.Log.Error($"char-select poll failed: {value}", 1);
		}
	}

	private static void OnInput(Control bg, InputEvent input)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I8
		InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
		if (val == null || (long)val.ButtonIndex != 1 || !val.Pressed)
		{
			return;
		}
		try
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)bg) || !((CanvasItem)bg).IsVisibleInTree() || !SkinSelectionGate.MenuAllowed() || !WearSettingsRuntime.Current.WearEnabled)
			{
				return;
			}
			Node val2 = SpineSwap.FindSpine((Node?)(object)bg);
			if (val2 != null && SpineSwap.IsOurs(val2))
			{
				int num = HpWearRules.NextStage(_stage, 3);
				if (Apply(bg, val2, num, replayIdle: true))
				{
					int stage = _stage;
					_stage = num;
					PortraitSync.RefreshMenuNow((Node)(object)bg);
					ModEntry.Log.Info($"char-select: stage {stage}->{num}/3.", 1);
				}
			}
		}
		catch (Exception value)
		{
			ModEntry.Log.Error($"char-select click failed: {value}", 1);
		}
	}

	internal static void OnCharacterSelected(NCharacterSelectScreen screen, CharacterModel? characterModel)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		try
		{
			string text = ((characterModel != null) ? ((AbstractModel)characterModel).Id.Entry : null);
			if (string.Equals(text, "ironclad", StringComparison.OrdinalIgnoreCase))
			{
				if ((object)_bgContainerField == null)
				{
					_bgContainerField = AccessTools.Field(typeof(NCharacterSelectScreen), "_bgContainer");
				}
				object? obj = _bgContainerField?.GetValue(screen);
				object? obj2 = ((obj is Control) ? obj : null);
				Control val = ((obj2 != null) ? ((Node)obj2).GetNodeOrNull<Control>(new NodePath(text + "_bg")) : null);
				if (val == null)
				{
					ModEntry.Log.Info("char-select: ironclad_bg not found — cycling off.", 1);
				}
				else
				{
					Attach(val);
				}
			}
		}
		catch (Exception value)
		{
			ModEntry.Log.Error($"char-select hook failed: {value}", 1);
		}
	}
}
[HarmonyPatch(typeof(NCharacterSelectScreen), "SelectCharacter")]
internal static class CharSelectPatch
{
	[HarmonyPostfix]
	private static void Postfix(NCharacterSelectScreen __instance, CharacterModel characterModel)
	{
		CharSelectCycle.OnCharacterSelected(__instance, characterModel);
	}
}
internal sealed class EnemyTurnGate<T> where T : class
{
	private T? last;

	private bool finished;

	private readonly HashSet<T> lowAtStart = new HashSet<T>();

	private readonly HashSet<T> released = new HashSet<T>();

	internal void Begin(T? lastAttacker, IEnumerable<T> alreadyLow)
	{
		last = lastAttacker;
		finished = lastAttacker == null;
		lowAtStart.Clear();
		released.Clear();
		foreach (T item in alreadyLow)
		{
			lowAtStart.Add(item);
		}
	}

	internal void MoveFinished(T attacker)
	{
		if (attacker == last)
		{
			finished = true;
		}
	}

	internal void EndTurn()
	{
		finished = true;
	}

	internal void Reset()
	{
		last = null;
		finished = true;
		lowAtStart.Clear();
		released.Clear();
	}

	internal bool ShouldHold(T player, bool isLow, bool dead, string? animation)
	{
		if (dead || !isLow || last == null || lowAtStart.Contains(player) || released.Contains(player))
		{
			return false;
		}
		if (!finished)
		{
			return true;
		}
		if (string.IsNullOrEmpty(animation) || animation.Equals("hurt", StringComparison.OrdinalIgnoreCase) || animation.Equals("hit", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		released.Add(player);
		return false;
	}
}
internal static class EnemyAttackGate
{
	private static readonly EnemyTurnGate<Creature> Gate = new EnemyTurnGate<Creature>();

	private static CombatState? snapshot;

	private static int round = -1;

	private static Creature? last;

	private static bool sawPerforming;

	internal static bool IsLow(Creature creature)
	{
		int num = Math.Clamp(WearSettingsRuntime.Current.LowHealthIdlePercent, 0, 100);
		if (creature.MaxHp > 0 && creature.CurrentHp > 0 && num > 0)
		{
			return (long)creature.CurrentHp * 100L <= (long)creature.MaxHp * (long)num;
		}
		return false;
	}

	internal static void Snapshot(CombatState? state)
	{
		if (state == null || (snapshot == state && round == state.RoundNumber))
		{
			return;
		}
		snapshot = state;
		round = state.RoundNumber;
		last = null;
		sawPerforming = false;
		List<Creature> list = new List<Creature>();
		foreach (Player player in state.Players)
		{
			if (((player != null) ? player.Character : null) is Ironclad && player.Creature != null && IsLow(player.Creature))
			{
				list.Add(player.Creature);
			}
		}
		foreach (Creature enemy in state.Enemies)
		{
			if (enemy != null && !enemy.IsDead)
			{
				MonsterModel monster = enemy.Monster;
				if (monster != null && monster.IntendsToAttack)
				{
					last = enemy;
				}
			}
		}
		Gate.Begin(last, list);
	}

	internal static void EnsureArmed()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		CombatManager instance = CombatManager.Instance;
		CombatState val = ((instance != null) ? instance.DebugOnlyGetState() : null);
		if (val == null)
		{
			Reset();
			return;
		}
		if ((int)val.CurrentSide == 2)
		{
			Snapshot(val);
		}
		bool flag = false;
		if (last != null)
		{
			foreach (Creature enemy in val.Enemies)
			{
				if (enemy == last)
				{
					flag = true;
					break;
				}
			}
		}
		Creature? obj = last;
		if (((obj != null) ? obj.Monster : null) != null && !last.IsDead && flag)
		{
			if (last.Monster.IsPerformingMove)
			{
				sawPerforming = true;
			}
			else if (sawPerforming)
			{
				Gate.MoveFinished(last);
			}
		}
		else if (last != null)
		{
			Gate.MoveFinished(last);
		}
	}

	internal static void MoveFinished(MonsterModel? monster)
	{
		if (((monster != null) ? monster.Creature : null) != null)
		{
			Gate.MoveFinished(monster.Creature);
		}
	}

	internal static void EndTurn()
	{
		Gate.EndTurn();
	}

	internal static void Reset()
	{
		Gate.Reset();
		snapshot = null;
		round = -1;
		last = null;
		sawPerforming = false;
	}

	internal static bool Hold(Creature creature, string? animation)
	{
		object obj;
		if (creature == null)
		{
			obj = null;
		}
		else
		{
			Player player = creature.Player;
			obj = ((player != null) ? player.Character : null);
		}
		if (!(obj is Ironclad) || !SkinSelectionGate.PlayerAllowed(creature.Player))
		{
			return false;
		}
		EnsureArmed();
		return Gate.ShouldHold(creature, IsLow(creature), creature.IsDead || creature.CurrentHp <= 0, animation);
	}

	internal static bool BlockAnimation(MegaAnimationState animationState)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		NCombatRoom instance = NCombatRoom.Instance;
		CombatManager instance2 = CombatManager.Instance;
		CombatState val = ((instance2 != null) ? instance2.DebugOnlyGetState() : null);
		if (instance == null || val == null || animationState == null)
		{
			return false;
		}
		foreach (Player player in val.Players)
		{
			if (!(((player != null) ? player.Character : null) is Ironclad) || !SkinSelectionGate.PlayerAllowed(player))
			{
				continue;
			}
			Creature creature = player.Creature;
			NCreature creatureNode = instance.GetCreatureNode(creature);
			object obj;
			if (creatureNode == null)
			{
				obj = null;
			}
			else
			{
				NCreatureVisuals visuals = creatureNode.Visuals;
				obj = ((visuals != null) ? visuals.GetCurrentBody() : null);
			}
			Node2D val2 = (Node2D)obj;
			if (val2 != null && SpineSwap.IsOurs((Node)(object)val2))
			{
				Variant val3 = ((GodotObject)val2).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
				GodotObject val4 = ((Variant)(ref val3)).AsGodotObject();
				if (val4 != null && GodotObject.IsInstanceValid(val4) && val4.GetInstanceId() == ((MegaSpineBinding)animationState).BoundObject.GetInstanceId())
				{
					return Hold(creature, SpineSwap.CurrentAnimation((Node)(object)val2));
				}
			}
		}
		return false;
	}
}
[HarmonyPatch(typeof(CombatManager), "ExecuteEnemyTurn")]
internal static class EnemyTurnStartPatch
{
	[HarmonyPrefix]
	private static void Prefix(CombatManager __instance)
	{
		try
		{
			EnemyAttackGate.Snapshot(__instance.DebugOnlyGetState());
		}
		catch (Exception ex)
		{
			ModEntry.TryInfo("enemy gate snapshot: " + ex.Message);
		}
	}
}
[HarmonyPatch(typeof(CombatManager), "EndEnemyTurn")]
internal static class EnemyTurnEndPatch
{
	[HarmonyPrefix]
	private static void Prefix()
	{
		EnemyAttackGate.EndTurn();
	}
}
[HarmonyPatch(typeof(CombatHistory), "MonsterPerformedMove")]
internal static class EnemyMoveFinishedPatch
{
	[HarmonyPostfix]
	private static void Postfix(MonsterModel monster)
	{
		EnemyAttackGate.MoveFinished(monster);
	}
}
[HarmonyPatch]
internal static class EnemyLowAnimationPatch
{
	[CompilerGenerated]
	private sealed class <Targets>d__0 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private MethodBase <>2__current;

		private int <>l__initialThreadId;

		private string[] <>7__wrap1;

		private int <>7__wrap2;

		MethodBase IEnumerator<MethodBase>.Current
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
		public <Targets>d__0(int <>1__state)
		{
			this.<>1__state = <>1__state;
			<>l__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			<>7__wrap1 = null;
			<>1__state = -2;
		}

		private bool MoveNext()
		{
			int num = <>1__state;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				<>1__state = -1;
				goto IL_0085;
			}
			<>1__state = -1;
			<>7__wrap1 = new string[3] { "SetAnimation", "AddAnimation", "AddAnimationTracked" };
			<>7__wrap2 = 0;
			goto IL_0093;
			IL_0085:
			<>7__wrap2++;
			goto IL_0093;
			IL_0093:
			if (<>7__wrap2 < <>7__wrap1.Length)
			{
				string text = <>7__wrap1[<>7__wrap2];
				MethodInfo methodInfo = AccessTools.Method(typeof(MegaAnimationState), text, (Type[])null, (Type[])null);
				if (methodInfo != null)
				{
					<>2__current = methodInfo;
					<>1__state = 1;
					return true;
				}
				goto IL_0085;
			}
			<>7__wrap1 = null;
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<MethodBase> IEnumerable<MethodBase>.GetEnumerator()
		{
			if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
			{
				<>1__state = 0;
				return this;
			}
			return new <Targets>d__0(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	[IteratorStateMachine(typeof(<Targets>d__0))]
	[HarmonyTargetMethods]
	private static IEnumerable<MethodBase> Targets()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <Targets>d__0(-2);
	}

	[HarmonyPrefix]
	private static void Prefix(MegaAnimationState __instance, ref string __0)
	{
		if (!string.Equals(__0, "low_health_loop", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		try
		{
			if (EnemyAttackGate.BlockAnimation(__instance))
			{
				__0 = "idle_loop";
			}
		}
		catch (Exception ex)
		{
			ModEntry.TryInfo("enemy gate animation: " + ex.Message);
		}
	}
}
[HarmonyPatch(typeof(AttackCommand), "Execute")]
internal static class HeavyAttackFx
{
	private sealed class DirectionalImpact
	{
		private readonly Creature? attacker;

		internal DirectionalImpact(Creature? attacker)
		{
			this.attacker = attacker;
		}

		internal Node2D? CreateHorizontalImpact(Creature target)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			NCombatRoom instance = NCombatRoom.Instance;
			NCreature val = ((instance != null) ? instance.GetCreatureNode(target) : null);
			if (val == null)
			{
				return null;
			}
			NCreature val2 = ((attacker == null) ? null : ((instance != null) ? instance.GetCreatureNode(attacker) : null));
			float num = ((val2 != null && ((Control)val).GlobalPosition.X < ((Control)val2).GlobalPosition.X) ? 180f : 0f);
			return (Node2D?)(object)NBigSlashImpactVfx.Create(val.VfxSpawnPosition, num, Color.FromHtml("#80dbff".AsSpan()));
		}
	}

	private static readonly FieldRef<AttackCommand, string?> Animation = AccessTools.FieldRefAccess<AttackCommand, string>("_attackerAnimName");

	private static readonly FieldRef<AttackCommand, bool> PlaysAnimation = AccessTools.FieldRefAccess<AttackCommand, bool>("_shouldPlayAnimation");

	private static readonly FieldRef<AttackCommand, Creature?> VisualAttacker = AccessTools.FieldRefAccess<AttackCommand, Creature>("_visualAttacker");

	private static readonly FieldRef<AttackCommand, string?> AttackerVfx = AccessTools.FieldRefAccess<AttackCommand, string>("_attackerVfx");

	private static readonly FieldRef<AttackCommand, List<Func<Node2D?>>> AttackerNodes = AccessTools.FieldRefAccess<AttackCommand, List<Func<Node2D>>>("_customAttackerVfxNodes");

	private static readonly FieldRef<AttackCommand, List<Func<Creature, Node2D?>>> HitNodes = AccessTools.FieldRefAccess<AttackCommand, List<Func<Creature, Node2D>>>("_customHitVfxNodes");

	private static void Prefix(AttackCommand __instance)
	{
		try
		{
			Creature attacker = __instance.Attacker;
			if (__instance.ModelSource is CardModel)
			{
				object obj;
				if (attacker == null)
				{
					obj = null;
				}
				else
				{
					Player player = attacker.Player;
					obj = ((player != null) ? player.Character : null);
				}
				if (obj is Ironclad && WantsReplacement(Animation.Invoke(__instance), PlaysAnimation.Invoke(__instance), VisualAttacker.Invoke(__instance) == null || VisualAttacker.Invoke(__instance) == attacker, __instance.ModelSource is PerfectedStrike) && IsUsingThisSkin(attacker))
				{
					ReplaceEffects(__instance);
				}
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("heavy attack fx skipped: " + ex.Message, 1);
		}
	}

	internal static bool IsUsingThisSkin(Creature attacker)
	{
		if (!SkinSelectionGate.PlayerAllowed(attacker.Player))
		{
			return false;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			NCreature creatureNode = instance.GetCreatureNode(attacker);
			if (creatureNode == null)
			{
				obj = null;
			}
			else
			{
				NCreatureVisuals visuals = creatureNode.Visuals;
				obj = ((visuals != null) ? visuals.GetCurrentBody() : null);
			}
		}
		Node2D val = (Node2D)obj;
		if (val == null || ((GodotObject)val).GetClass() != "SpineSprite")
		{
			return false;
		}
		return SkinCatalog.IsOurPath(SpineSwap.GetSkeletonPath((Node)(object)val));
	}

	internal static bool WantsReplacement(string? trigger, bool playsAnimation, bool ownsVisual, bool perfectedStrike)
	{
		if (ownsVisual)
		{
			if (!perfectedStrike)
			{
				if (playsAnimation)
				{
					return trigger == "heavyAttack";
				}
				return false;
			}
			return true;
		}
		return false;
	}

	internal static void ReplaceEffects(AttackCommand attack)
	{
		AttackerVfx.Invoke(attack) = null;
		AttackerNodes.Invoke(attack).Clear();
		attack.WithHitFx((string)null, attack.HitSfx, attack.TmpHitSfx);
		HitNodes.Invoke(attack).Clear();
		attack.WithHitVfxNode((Func<Creature, Node2D>)new DirectionalImpact(attack.Attacker).CreateHorizontalImpact);
	}
}
[HarmonyPatch]
internal static class StompSpikePatch
{
	[CompilerGenerated]
	private sealed class <Transpiler>d__1 : IEnumerable<CodeInstruction>, IEnumerable, IEnumerator<CodeInstruction>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private CodeInstruction <>2__current;

		private int <>l__initialThreadId;

		private IEnumerable<CodeInstruction> instructions;

		public IEnumerable<CodeInstruction> <>3__instructions;

		private MethodInfo <replacement>5__2;

		private FieldInfo <cardField>5__3;

		private int <count>5__4;

		private IEnumerator<CodeInstruction> <>7__wrap4;

		CodeInstruction IEnumerator<CodeInstruction>.Current
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
		public <Transpiler>d__1(int <>1__state)
		{
			this.<>1__state = <>1__state;
			<>l__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			switch (<>1__state)
			{
			case -3:
			case 1:
				try
				{
				}
				finally
				{
					<>m__Finally1();
				}
				break;
			case -4:
			case 2:
			case 3:
			case 4:
			case 5:
				try
				{
				}
				finally
				{
					<>m__Finally2();
				}
				break;
			}
			<replacement>5__2 = null;
			<cardField>5__3 = null;
			<>7__wrap4 = null;
			<>1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Expected O, but got Unknown
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01eb: Expected O, but got Unknown
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Expected O, but got Unknown
			try
			{
				switch (<>1__state)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					<replacement>5__2 = AccessTools.Method(typeof(StompSpikePatch), "CreateSpike", (Type[])null, (Type[])null);
					<cardField>5__3 = AccessTools.Field(TargetMethod().DeclaringType, "<>4__this");
					if (<replacement>5__2 == null || <cardField>5__3 == null)
					{
						ModEntry.Defer("stomp spike patch: replacement or card field missing — spikes left native.");
						<>7__wrap4 = instructions.GetEnumerator();
						<>1__state = -3;
						goto IL_00d3;
					}
					<count>5__4 = 0;
					<>7__wrap4 = instructions.GetEnumerator();
					<>1__state = -4;
					break;
				case 1:
					<>1__state = -3;
					goto IL_00d3;
				case 2:
					<>1__state = -4;
					break;
				case 3:
					<>1__state = -4;
					<>2__current = new CodeInstruction(OpCodes.Ldfld, (object)<cardField>5__3);
					<>1__state = 4;
					return true;
				case 4:
					<>1__state = -4;
					<>2__current = new CodeInstruction(OpCodes.Call, (object)<replacement>5__2);
					<>1__state = 5;
					return true;
				case 5:
					{
						<>1__state = -4;
						break;
					}
					IL_00d3:
					if (<>7__wrap4.MoveNext())
					{
						CodeInstruction current = <>7__wrap4.Current;
						<>2__current = current;
						<>1__state = 1;
						return true;
					}
					<>m__Finally1();
					<>7__wrap4 = null;
					return false;
				}
				if (<>7__wrap4.MoveNext())
				{
					CodeInstruction current2 = <>7__wrap4.Current;
					if (!IsSpikeFactory(current2))
					{
						<>2__current = current2;
						<>1__state = 2;
						return true;
					}
					<count>5__4++;
					CodeInstruction val = new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					val.labels.AddRange(current2.labels);
					current2.labels.Clear();
					<>2__current = val;
					<>1__state = 3;
					return true;
				}
				<>m__Finally2();
				<>7__wrap4 = null;
				if (<count>5__4 != 1)
				{
					ModEntry.Defer($"stomp spike patch: expected 1 spike factory, found {<count>5__4} — left native.");
				}
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
			if (<>7__wrap4 != null)
			{
				<>7__wrap4.Dispose();
			}
		}

		private void <>m__Finally2()
		{
			<>1__state = -1;
			if (<>7__wrap4 != null)
			{
				<>7__wrap4.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<CodeInstruction> IEnumerable<CodeInstruction>.GetEnumerator()
		{
			<Transpiler>d__1 <Transpiler>d__;
			if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
			{
				<>1__state = 0;
				<Transpiler>d__ = this;
			}
			else
			{
				<Transpiler>d__ = new <Transpiler>d__1(0);
			}
			<Transpiler>d__.instructions = <>3__instructions;
			return <Transpiler>d__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<CodeInstruction>)this).GetEnumerator();
		}
	}

	internal static MethodBase TargetMethod()
	{
		return AccessTools.DeclaredMethod(typeof(Stomp), "OnPlay", (Type[])null, (Type[])null).GetCustomAttribute<AsyncStateMachineAttribute>().StateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	[IteratorStateMachine(typeof(<Transpiler>d__1))]
	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <Transpiler>d__1(-2)
		{
			<>3__instructions = instructions
		};
	}

	internal static bool IsSpikeFactory(CodeInstruction instruction)
	{
		if (instruction.opcode != OpCodes.Call && instruction.opcode != OpCodes.Callvirt)
		{
			return false;
		}
		if (!(instruction.operand is MethodBase methodBase))
		{
			return false;
		}
		if (methodBase.Name == "Create")
		{
			return methodBase.DeclaringType?.Name == "NSpikeSplashVfx";
		}
		return false;
	}

	private static NSpikeSplashVfx? CreateSpike(Creature target, VfxColor color, Stomp card)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (((CardModel)card).Owner.Character is Ironclad && HeavyAttackFx.IsUsingThisSkin(((CardModel)card).Owner.Creature))
		{
			return null;
		}
		return NSpikeSplashVfx.Create(target, color);
	}
}
internal static class HpWear
{
	private sealed class State
	{
		internal int Stage;

		internal int Wait;
	}

	private static readonly Dictionary<ulong, State> CombatStates = new Dictionary<ulong, State>();

	private static readonly Dictionary<ulong, State> ShopStates = new Dictionary<ulong, State>();

	private static readonly Dictionary<ulong, State> RestStates = new Dictionary<ulong, State>();

	private static readonly List<NRestSiteCharacter> RestChars = new List<NRestSiteCharacter>();

	private static readonly Dictionary<ulong, int> ShownStage = new Dictionary<ulong, int>();

	private static FieldInfo? _merchantPlayersField;

	private static bool _merchantPlayersLookupFailed;

	private static int _errorStreak;

	private static readonly Dictionary<ulong, float> AppliedScales = new Dictionary<ulong, float>();

	internal static bool Disabled { get; private set; }

	private static int LivePortraitStage(string group, Node? spine)
	{
		if (spine != null)
		{
			return SkinCatalog.StageFromPath(group, SpineSwap.GetSkeletonPath(spine));
		}
		return 0;
	}

	internal static int PortraitStage(Player player)
	{
		NCombatRoom instance = NCombatRoom.Instance;
		NCreature val = ((instance != null) ? instance.GetCreatureNode(player.Creature) : null);
		Node val2 = ((val == null) ? null : SpineSwap.FindSpine((Node?)(object)val.Visuals));
		int result = LivePortraitStage("ironclad", val2);
		if (val2 != null)
		{
			return result;
		}
		foreach (NRestSiteCharacter restChar in RestChars)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)restChar) && restChar.Player == player && ((Node)restChar).IsInsideTree())
			{
				Node val3 = SpineSwap.FindSpine((Node?)(object)restChar);
				if (val3 != null)
				{
					return LivePortraitStage("restsite", val3);
				}
			}
		}
		NMerchantRoom instance2 = NMerchantRoom.Instance;
		if (instance2 != null)
		{
			List<Player> list = MerchantPlayers(instance2);
			if (list != null)
			{
				for (int i = 0; i < Math.Min(list.Count, instance2.PlayerVisuals.Count); i++)
				{
					if (list[i] == player)
					{
						Node val4 = SpineSwap.FindSpine((Node?)(object)instance2.PlayerVisuals[i]);
						if (val4 != null)
						{
							return LivePortraitStage("ironclad_shop", val4);
						}
					}
				}
			}
		}
		Creature creature = player.Creature;
		if (!creature.IsDead)
		{
			return Math.Max(1, WearSettingsRuntime.Current.ResolveStage(creature.CurrentHp, creature.MaxHp));
		}
		return WearSettingsRuntime.Current.ResolveDeathStage(creature.CurrentHp, creature.MaxHp);
	}

	internal static void Tick()
	{
		if (Disabled)
		{
			return;
		}
		try
		{
			ModEntry.FlushDeferred();
			TickCombat();
			TickShop();
			TickRest();
			PortraitSync.RefreshRun();
			if (NCombatRoom.Instance == null && NMerchantRoom.Instance == null && NRestSiteRoom.Instance == null)
			{
				ShownStage.Clear();
			}
			_errorStreak = 0;
		}
		catch (Exception value)
		{
			_errorStreak++;
			ModEntry.Log.Error($"wear tick failed ({_errorStreak}): {value}", 1);
			if (_errorStreak >= 20)
			{
				Disabled = true;
				ModEntry.Log.Error("wear disabled for this session after repeated failures.", 1);
			}
		}
	}

	internal static void OnCreatureReady(NCreature node)
	{
		Creature entity = node.Entity;
		Player val = ((entity != null) ? entity.Player : null);
		if (((val != null) ? val.Character : null) is Ironclad && SkinSelectionGate.PlayerAllowed(val))
		{
			Node val2 = SpineSwap.FindSpine((Node?)(object)node.Visuals);
			if (val2 != null && SpineSwap.IsOurs(val2))
			{
				ApplyVisualScale(node);
				UnderwearVisibility.Attach(val2);
			}
		}
	}

	internal static void TrackRest(NRestSiteCharacter character)
	{
		if (character != null && GodotObject.IsInstanceValid((GodotObject)(object)character) && !RestChars.Contains(character))
		{
			RestChars.Add(character);
		}
	}

	internal static void PrepareDeath(NCreature node)
	{
		if (Disabled || !WearSettingsRuntime.Current.WearEnabled || node == null || !GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return;
		}
		Creature entity = node.Entity;
		object obj;
		if (entity == null)
		{
			obj = null;
		}
		else
		{
			Player player = entity.Player;
			obj = ((player != null) ? player.Character : null);
		}
		if (!(obj is Ironclad) || !SkinSelectionGate.PlayerAllowed(entity.Player))
		{
			return;
		}
		Node val = SpineSwap.FindSpine((Node?)(object)node.Visuals);
		if (val == null)
		{
			return;
		}
		WearCrossfade.Cancel(val);
		int num = SkinCatalog.StageFromPath("ironclad", SpineSwap.GetSkeletonPath(val));
		int num2 = WearSettingsRuntime.Current.ResolveDeathStage(entity.CurrentHp, entity.MaxHp);
		if (num != 0 && num2 != 0 && num != num2)
		{
			Resource val2 = SpineSwap.LoadStageData("ironclad", num2);
			if (val2 != null && SpineSwap.SetSkeletonData(val, val2))
			{
				ShownStage[((GodotObject)val).GetInstanceId()] = num2;
				ModEntry.Log.Info($"combat: stage {num2} prepared before death animation.", 1);
			}
		}
	}

	internal static bool ApplyDeathLook(Node spine, string group, int hp, int maxHp)
	{
		WearSettings current = WearSettingsRuntime.Current;
		if (!current.WearEnabled || SkinCatalog.StageFromPath(group, SpineSwap.GetSkeletonPath(spine)) == 0)
		{
			return false;
		}
		int num = current.ResolveDeathStage(hp, maxHp);
		if (num == 0)
		{
			return false;
		}
		Resource val = SpineSwap.LoadStageData(group, num);
		if (val != null)
		{
			return SpineSwap.SetSkeletonData(spine, val);
		}
		return false;
	}

	internal static void OnNonCombatDeath(Creature creature)
	{
		if (Disabled || NCombatRoom.Instance != null)
		{
			return;
		}
		Player player = creature.Player;
		if (!(((player != null) ? player.Character : null) is Ironclad) || !SkinSelectionGate.PlayerAllowed(creature.Player))
		{
			return;
		}
		foreach (NRestSiteCharacter restChar in RestChars)
		{
			if (restChar != null && GodotObject.IsInstanceValid((GodotObject)(object)restChar) && restChar.Player == creature.Player)
			{
				Node val = SpineSwap.FindSpine((Node?)(object)restChar);
				if (val != null)
				{
					ApplyDeathLook(val, "restsite", creature.CurrentHp, creature.MaxHp);
				}
			}
		}
		NMerchantRoom instance = NMerchantRoom.Instance;
		if (instance == null)
		{
			return;
		}
		List<Player> list = MerchantPlayers(instance);
		IReadOnlyList<NMerchantCharacter> playerVisuals = instance.PlayerVisuals;
		if (list == null || playerVisuals == null)
		{
			return;
		}
		for (int i = 0; i < Math.Min(list.Count, playerVisuals.Count); i++)
		{
			if (list[i] == creature.Player && playerVisuals[i] != null && GodotObject.IsInstanceValid((GodotObject)(object)playerVisuals[i]))
			{
				Node val2 = SpineSwap.FindSpine((Node?)(object)playerVisuals[i]);
				if (val2 != null)
				{
					ApplyDeathLook(val2, "ironclad_shop", creature.CurrentHp, creature.MaxHp);
				}
			}
		}
	}

	private static void ApplyVisualScale(NCreature node)
	{
		double visualScale = WearSettingsRuntime.Current.VisualScale;
		float num = (float)(double.IsFinite(visualScale) ? Math.Clamp(visualScale, 0.5, 2.0) : 1.0);
		ulong instanceId = ((GodotObject)node).GetInstanceId();
		if (!AppliedScales.TryGetValue(instanceId, out var value) || !(Math.Abs(value - num) < 0.001f))
		{
			node.SetScaleAndHue(num, 0f);
			AppliedScales[instanceId] = num;
		}
	}

	private static void TickCombat()
	{
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			CombatStates.Clear();
			AppliedScales.Clear();
			EnemyAttackGate.Reset();
			return;
		}
		RunManager instance2 = RunManager.Instance;
		object obj;
		if (instance2 == null)
		{
			obj = null;
		}
		else
		{
			RunState obj2 = instance2.DebugOnlyGetState();
			obj = ((obj2 != null) ? obj2.Players : null);
		}
		IReadOnlyList<Player> readOnlyList = (IReadOnlyList<Player>)obj;
		if (readOnlyList == null)
		{
			return;
		}
		foreach (Player item in readOnlyList)
		{
			if (!(((item != null) ? item.Character : null) is Ironclad))
			{
				continue;
			}
			if (!SkinSelectionGate.PlayerAllowed(item))
			{
				NCreature creatureNode = instance.GetCreatureNode(item.Creature);
				Node val = ((creatureNode == null) ? null : SpineSwap.FindSpine((Node?)(object)creatureNode.Visuals));
				if (val != null)
				{
					WearCrossfade.Cancel(val);
				}
				continue;
			}
			Creature creature = item.Creature;
			if (creature == null || creature.IsDead)
			{
				continue;
			}
			NCreature creatureNode2 = instance.GetCreatureNode(creature);
			if (creatureNode2 == null || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode2))
			{
				continue;
			}
			Node val2 = SpineSwap.FindSpine((Node?)(object)creatureNode2.Visuals);
			if (val2 != null)
			{
				if (SpineSwap.IsOurs(val2))
				{
					ApplyVisualScale(creatureNode2);
					UnderwearVisibility.Attach(val2);
				}
				Consider(CombatStates, "ironclad", "combat", val2, creature, forceIdle: false);
			}
		}
	}

	private static void TickShop()
	{
		NMerchantRoom instance = NMerchantRoom.Instance;
		if (instance == null)
		{
			ShopStates.Clear();
			return;
		}
		IReadOnlyList<NMerchantCharacter> playerVisuals = instance.PlayerVisuals;
		List<Player> list = MerchantPlayers(instance);
		if (playerVisuals == null || list == null)
		{
			return;
		}
		int num = Math.Min(playerVisuals.Count, list.Count);
		for (int i = 0; i < num; i++)
		{
			NMerchantCharacter val = playerVisuals[i];
			Player val2 = list[i];
			if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val) || !(((val2 != null) ? val2.Character : null) is Ironclad))
			{
				continue;
			}
			if (!SkinSelectionGate.PlayerAllowed(val2))
			{
				Node val3 = SpineSwap.FindSpine((Node?)(object)val);
				if (val3 != null)
				{
					WearCrossfade.Cancel(val3);
				}
				continue;
			}
			Creature creature = val2.Creature;
			if (creature != null && !creature.IsDead)
			{
				Node val4 = SpineSwap.FindSpine((Node?)(object)val);
				if (val4 != null)
				{
					Consider(ShopStates, "ironclad_shop", $"shop[{i}]", val4, creature, forceIdle: false);
				}
			}
		}
	}

	private static void TickRest()
	{
		if (NRestSiteRoom.Instance == null)
		{
			RestStates.Clear();
			RestChars.Clear();
			return;
		}
		RestChars.RemoveAll((NRestSiteCharacter c) => c == null || !GodotObject.IsInstanceValid((GodotObject)(object)c));
		foreach (NRestSiteCharacter restChar in RestChars)
		{
			Player player = restChar.Player;
			if (!(((player != null) ? player.Character : null) is Ironclad))
			{
				continue;
			}
			if (!SkinSelectionGate.PlayerAllowed(restChar.Player))
			{
				Node val = SpineSwap.FindSpine((Node?)(object)restChar);
				if (val != null)
				{
					WearCrossfade.Cancel(val);
				}
				continue;
			}
			Creature creature = restChar.Player.Creature;
			if (creature != null && !creature.IsDead)
			{
				Node val2 = SpineSwap.FindSpine((Node?)(object)restChar);
				if (val2 != null)
				{
					Consider(RestStates, "restsite", "rest", val2, creature, forceIdle: false);
				}
			}
		}
	}

	private static void Consider(Dictionary<ulong, State> states, string group, string label, Node spine, Creature creature, bool forceIdle)
	{
		ulong instanceId = ((GodotObject)spine).GetInstanceId();
		if (!states.TryGetValue(instanceId, out State value))
		{
			State obj = new State
			{
				Stage = 1
			};
			value = obj;
			states[instanceId] = obj;
		}
		int num = WearSettingsRuntime.Current.ResolveStage(creature.CurrentHp, creature.MaxHp);
		if (num == 0)
		{
			if (value.Wait == 0)
			{
				value.Wait = 1;
				ModEntry.Log.Info($"{label}: max HP {creature.MaxHp} unusable — keeping current look.", 1);
			}
			return;
		}
		int num2 = SkinCatalog.StageFromPath(group, SpineSwap.GetSkeletonPath(spine));
		if (num2 == 0)
		{
			if (value.Stage != -1)
			{
				value.Stage = -1;
				value.Wait = 0;
				ShownStage.Remove(instanceId);
				ModEntry.Log.Info(label + ": live skeleton is not this mod's " + group + " — wear skipped.", 1);
			}
		}
		else
		{
			if (!forceIdle && WearCrossfade.IsActive(spine))
			{
				return;
			}
			if (value.Stage != num2)
			{
				value.Stage = num2;
				value.Wait = 0;
			}
			if (num == value.Stage)
			{
				value.Wait = 0;
				ShownStage[instanceId] = value.Stage;
				UpdateLowHealthIdle(group, spine, creature);
				return;
			}
			string text = SpineSwap.CurrentAnimation(spine);
			if ((forceIdle || HpWearRules.CanSwapDuring(text)) && (forceIdle || !(group == "ironclad") || !EnemyAttackGate.Hold(creature, text)) && (forceIdle || !SpineSwap.IsMixing(spine)) && (forceIdle || !UpdateLowHealthIdle(group, spine, creature)))
			{
				value.Wait = 0;
				Apply(states, value, group, label, spine, num, creature, text, forceIdle);
				UpdateLowHealthIdle(group, spine, creature);
			}
		}
	}

	private static bool UpdateLowHealthIdle(string group, Node spine, Creature creature)
	{
		if (group != "ironclad" && group != "ironclad_shop")
		{
			return false;
		}
		if (creature.CurrentHp <= 0 || creature.MaxHp <= 0)
		{
			return false;
		}
		string text = SpineSwap.CurrentAnimation(spine);
		if (text != "idle_loop" && text != "low_health_loop")
		{
			return false;
		}
		int num = Math.Clamp(WearSettingsRuntime.Current.LowHealthIdlePercent, 0, 100);
		string text2 = ((num > 0 && (long)creature.CurrentHp * 100L <= (long)creature.MaxHp * (long)num) ? "low_health_loop" : "idle_loop");
		if (text == text2 || SpineSwap.IsMixing(spine))
		{
			return false;
		}
		if (text2 == "low_health_loop" && group == "ironclad" && EnemyAttackGate.Hold(creature, text))
		{
			return false;
		}
		return SpineSwap.Play(spine, text2, loop: true);
	}

	private static void Apply(Dictionary<ulong, State> states, State state, string group, string label, Node spine, int stage, Creature creature, string? current, bool forceIdle)
	{
		Resource data = SpineSwap.LoadStageData(group, stage);
		if (data != null && (forceIdle || !WearCrossfade.BeginDeferred(spine, StillValid, Commit)))
		{
			Commit();
		}
		void Commit()
		{
			if (!SpineSwap.SetSkeletonData(spine, data))
			{
				WearCrossfade.Cancel(spine);
				ModEntry.Log.Error(label + ": skeleton data did not change to " + SkinCatalog.DataPath(group, stage) + ".", 1);
			}
			else
			{
				string text = SpineSwap.CurrentAnimation(spine);
				string text2 = HpWearRules.SafeLoopAnimation(group, string.IsNullOrEmpty(text) ? current : text);
				SpineSwap.Play(spine, text2, loop: true);
				int stage2 = state.Stage;
				state.Stage = stage;
				ShownStage[((GodotObject)spine).GetInstanceId()] = stage;
				ModEntry.Log.Info($"{label}: stage {stage2}->{stage} (hp {creature.CurrentHp}/{creature.MaxHp}, anim {text2}).", 1);
			}
		}
		bool StillValid()
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)spine) || creature.IsDead || creature.CurrentHp <= 0 || WearSettingsRuntime.Current.ResolveStage(creature.CurrentHp, creature.MaxHp) != stage || !SkinSelectionGate.PlayerAllowed(creature.Player) || !SpineSwap.IsOurs(spine))
			{
				return false;
			}
			string animation = SpineSwap.CurrentAnimation(spine);
			if (HpWearRules.CanSwapDuring(animation) && !SpineSwap.IsMixing(spine))
			{
				if (!(group != "ironclad"))
				{
					return !EnemyAttackGate.Hold(creature, animation);
				}
				return true;
			}
			return false;
		}
	}

	private static List<Player>? MerchantPlayers(NMerchantRoom room)
	{
		if (_merchantPlayersLookupFailed)
		{
			return null;
		}
		if (_merchantPlayersField == null)
		{
			_merchantPlayersField = AccessTools.Field(typeof(NMerchantRoom), "_players");
			if (_merchantPlayersField == null)
			{
				_merchantPlayersLookupFailed = true;
				ModEntry.Log.Error("shop: NMerchantRoom._players not found — shop wear disabled.", 1);
				return null;
			}
		}
		return _merchantPlayersField.GetValue(room) as List<Player>;
	}
}
[HarmonyPatch(typeof(NRun), "_Ready")]
internal static class WearTimerPatch
{
	[HarmonyPostfix]
	private static void Postfix(NRun __instance)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		try
		{
			Node globalUi = (Node)(object)__instance.GlobalUi;
			if (globalUi == null)
			{
				return;
			}
			string text = "JoanofArcIroncladHpwearTimer";
			if (globalUi.HasNode(new NodePath(text)))
			{
				return;
			}
			Timer val = new Timer
			{
				Name = StringName.op_Implicit(text),
				WaitTime = 0.15,
				OneShot = false,
				Autostart = true,
				ProcessCallback = (TimerProcessCallback)1,
				ProcessMode = (ProcessModeEnum)3
			};
			val.Timeout += HpWear.Tick;
			globalUi.AddChild((Node)(object)val, false, (InternalMode)0);
			val.Start(-1.0);
			Timer val2 = new Timer
			{
				Name = StringName.op_Implicit("JoanofArcIroncladHpwearSafetyTimer"),
				WaitTime = 0.03,
				OneShot = false,
				Autostart = true,
				ProcessCallback = (TimerProcessCallback)1,
				ProcessMode = (ProcessModeEnum)3
			};
			val2.Timeout += delegate
			{
				if (SkinSelectionGate.SceneAllowed())
				{
					WearSettingsRuntime.TickHotkey();
				}
			};
			globalUi.AddChild((Node)(object)val2, false, (InternalMode)0);
			val2.Start(-1.0);
			ModEntry.Log.Info("wear timer attached.", 1);
		}
		catch (Exception value)
		{
			ModEntry.Log.Error($"wear timer attach failed: {value}", 1);
		}
	}
}
[HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
internal static class RestTrackPatch
{
	[HarmonyPostfix]
	private static void Postfix(NRestSiteCharacter __instance)
	{
		try
		{
			HpWear.TrackRest(__instance);
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("rest track failed: " + ex.Message, 1);
		}
	}
}
[HarmonyPatch(typeof(NCreature), "StartDeathAnim")]
internal static class DeathReplayPatch
{
	[HarmonyPrefix]
	private static void Prefix(NCreature __instance)
	{
		try
		{
			HpWear.PrepareDeath(__instance);
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("death preparation failed: " + ex.Message, 1);
		}
	}
}
[HarmonyPatch(typeof(NCreature), "_Ready")]
internal static class CreatureReadyPatch
{
	[HarmonyPostfix]
	private static void Postfix(NCreature __instance)
	{
		try
		{
			HpWear.OnCreatureReady(__instance);
		}
		catch (Exception value)
		{
			ModEntry.Log.Error($"creature visual initialization failed: {value}", 1);
		}
	}
}
[HarmonyPatch(typeof(NGameOverScreen), "MoveCreaturesToDifferentLayerAndDisableUi")]
internal static class GameOverWearPatch
{
	[HarmonyPostfix]
	private static void Postfix(NGameOverScreen __instance)
	{
		try
		{
			if (NCombatRoom.Instance != null || !WearSettingsRuntime.Current.WearEnabled)
			{
				return;
			}
			object? obj = AccessTools.Field(typeof(NGameOverScreen), "_runState")?.GetValue(__instance);
			RunState val = (RunState)((obj is RunState) ? obj : null);
			object? obj2 = AccessTools.Field(typeof(NGameOverScreen), "_creatureContainer")?.GetValue(__instance);
			Node val2 = (Node)((obj2 is Node) ? obj2 : null);
			if (val == null || val2 == null || !GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				return;
			}
			List<NCreatureVisuals> list = new List<NCreatureVisuals>();
			foreach (Node child in val2.GetChildren(false))
			{
				NCreatureVisuals val3 = (NCreatureVisuals)(object)((child is NCreatureVisuals) ? child : null);
				if (val3 != null)
				{
					list.Add(val3);
				}
			}
			for (int i = 0; i < val.Players.Count && i < list.Count; i++)
			{
				Player val4 = val.Players[i];
				if (!(((val4 != null) ? val4.Character : null) is Ironclad) || !SkinSelectionGate.PlayerAllowed(val4))
				{
					continue;
				}
				Node val5 = SpineSwap.FindSpine((Node?)(object)list[i]);
				if (val5 == null || SkinCatalog.StageFromPath("ironclad", SpineSwap.GetSkeletonPath(val5)) == 0)
				{
					continue;
				}
				int num = WearSettingsRuntime.Current.ResolveDeathStage(val4.Creature.CurrentHp, val4.Creature.MaxHp);
				if (num != 0)
				{
					Resource val6 = SpineSwap.LoadStageData("ironclad", num);
					if (val6 != null && SpineSwap.SetSkeletonData(val5, val6))
					{
						SpineSwap.Play(val5, "die", loop: false);
					}
				}
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("game-over wear skipped: " + ex.Message, 1);
		}
	}
}
[HarmonyPatch(typeof(CreatureCmd), "KillWithoutCheckingWinCondition")]
internal static class NonCombatDeathWearPatch
{
	[HarmonyPostfix]
	private static void Postfix(Creature creature, ref Task __result)
	{
		if (__result != null && ((creature != null) ? creature.Player : null) != null)
		{
			__result = AfterDeath(__result, creature);
		}
	}

	private static async Task AfterDeath(Task original, Creature creature)
	{
		await original;
		try
		{
			if (creature.IsDead)
			{
				HpWear.OnNonCombatDeath(creature);
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("noncombat death wear skipped: " + ex.Message, 1);
		}
	}
}
public static class HpWearRules
{
	public const int HighStage = 1;

	public const int LowStage = 2;

	public const int InvalidStage = 0;

	public static int StageFor(int currentHp, int maxHp)
	{
		if (maxHp <= 0)
		{
			return 0;
		}
		if (currentHp <= 0)
		{
			return 2;
		}
		if ((long)currentHp * 2L <= maxHp)
		{
			return 2;
		}
		return 1;
	}

	public static int NextStage(int current, int maxStage)
	{
		if (maxStage < 1)
		{
			return 1;
		}
		if (current < 1 || current >= maxStage)
		{
			return 1;
		}
		return current + 1;
	}

	public static bool IsIdleLike(string? animation)
	{
		if (string.IsNullOrWhiteSpace(animation))
		{
			return true;
		}
		if (!animation.Contains("idle", StringComparison.OrdinalIgnoreCase) && !animation.Contains("loop", StringComparison.OrdinalIgnoreCase) && !animation.Contains("relaxed", StringComparison.OrdinalIgnoreCase) && !animation.Contains("glory", StringComparison.OrdinalIgnoreCase))
		{
			return animation.Contains("animation", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public static bool CanSwapDuring(string? animation)
	{
		return IsIdleLike(animation);
	}

	public static string SafeLoopAnimation(string group, string? current)
	{
		if (!IsIdleLike(current) || string.IsNullOrWhiteSpace(current))
		{
			return DefaultAnimationFor(group);
		}
		return current;
	}

	public static string DefaultAnimationFor(string group)
	{
		return group switch
		{
			"restsite" => "glory_loop", 
			"charselect" => "animation", 
			"ironclad_shop" => "idle_loop", 
			_ => "idle_loop", 
		};
	}

	public static string AnimationAfterSwap(string group, string? current)
	{
		if (!string.IsNullOrWhiteSpace(current))
		{
			return current;
		}
		return DefaultAnimationFor(group);
	}
}
[ModInitializer("Initialize")]
public static class ModEntry
{
	internal const string HarmonyId = "JoanofArcIroncladHpwear.Patches";

	private static Logger? _log;

	private static readonly List<string> Pending = new List<string>();

	internal static Logger Log
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			object obj = _log;
			if (obj == null)
			{
				Logger val = new Logger("JoanofArcIroncladHpwear", (LogType)0);
				_log = val;
				obj = (object)val;
			}
			return (Logger)obj;
		}
	}

	internal static void Defer(string message)
	{
		lock (Pending)
		{
			Pending.Add(message);
		}
	}

	internal static void FlushDeferred()
	{
		if (Pending.Count != 0)
		{
			string[] array;
			lock (Pending)
			{
				array = Pending.ToArray();
				Pending.Clear();
			}
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				TryInfo(array2[i]);
			}
		}
	}

	internal static void TryInfo(string message)
	{
		try
		{
			Log.Info(message, 1);
		}
		catch
		{
		}
	}

	internal static void TryError(string message)
	{
		try
		{
			Log.Error(message, 1);
		}
		catch
		{
		}
	}

	public static void Initialize()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		Harmony val = new Harmony("JoanofArcIroncladHpwear.Patches");
		int num = 0;
		int num2 = 0;
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		foreach (Type type in types)
		{
			if (DeclaresPatches(type))
			{
				try
				{
					val.CreateClassProcessor(type).Patch();
					num++;
				}
				catch (Exception ex)
				{
					num2++;
					Log.Error($"{type.Name} patch failed (continuing): {ex.GetType().Name}: {ex.Message}", 1);
				}
			}
		}
		RitsuSettingsLoader.TryRegister();
		Palette.StartDeferred();
		if (num == 0)
		{
			Log.Error("JoanofArcIroncladHpwear: no patch class applied — mod inactive.", 1);
			return;
		}
		Log.Info($"{"JoanofArcIroncladHpwear"} v{typeof(ModEntry).Assembly.GetName().Version}: HP wear armed ({num} patch classes, {num2} failed).", 1);
	}

	private static bool DeclaresPatches(Type type)
	{
		if (type.GetCustomAttributes(typeof(HarmonyPatch), inherit: true).Length == 0 && type.GetCustomAttributes(typeof(HarmonyPatchAll), inherit: true).Length == 0)
		{
			return type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Any((MethodInfo m) => m.GetCustomAttributes(typeof(HarmonyPrefix), inherit: true).Length != 0 || m.GetCustomAttributes(typeof(HarmonyPostfix), inherit: true).Length != 0 || m.GetCustomAttributes(typeof(HarmonyTranspiler), inherit: true).Length != 0);
		}
		return true;
	}
}
internal static class Palette
{
	private sealed class Page
	{
		internal required string Key;

		internal required ShaderMaterial Material;

		internal bool Emissive;
	}

	private sealed class Binding
	{
		internal Material? Previous;

		internal ShaderMaterial Applied;
	}

	[CompilerGenerated]
	private static class <>O
	{
		public static Action <0>__Start;

		public static NodeAddedEventHandler <1>__Track;

		public static NodeRemovedEventHandler <2>__Untrack;
	}

	private const string Assets = "res://joanofarcironcladhpwear/palette/";

	private const double FadeSeconds = 0.25;

	private const float NeonEmission = 0.26f;

	internal static readonly string[] Names = new string[4] { "原色", "黑贞德", "Saber 王庭蓝", "霓虹审判官" };

	private static readonly string[] PageKeys = new string[4] { "s1_ironclad", "s2_ironclad", "s1_restsite", "s2_restsite" };

	private static readonly Dictionary<string, Page> Pages = new Dictionary<string, Page>(StringComparer.Ordinal);

	private static readonly Dictionary<ulong, Binding> Bound = new Dictionary<ulong, Binding>();

	private static readonly HashSet<Node> Tracked = new HashSet<Node>();

	private static readonly List<Node> Scratch = new List<Node>();

	private static readonly Dictionary<ulong, ulong> RetryAfter = new Dictionary<ulong, ulong>();

	private static Timer? _timer;

	private static Tween? _fade;

	private static int _preset = -1;

	private static Vector4 _weights = new Vector4(1f, 0f, 0f, 0f);

	private static bool _started;

	private static bool _failed;

	internal static bool Active
	{
		get
		{
			if (_started)
			{
				return !_failed;
			}
			return false;
		}
	}

	internal static void StartDeferred()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Callable val = Callable.From((Action)Start);
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
		catch (Exception ex)
		{
			ModEntry.TryInfo("palette start deferred: " + ex.Message);
		}
	}

	private static void Start()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected O, but got Unknown
		if (_started || _failed)
		{
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree tree = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (tree != null)
		{
			try
			{
				BuildPages();
			}
			catch (Exception ex)
			{
				_failed = true;
				ModEntry.TryError("palette assets unavailable, appearance switching disabled: " + ex.GetBaseException().Message);
				return;
			}
			_started = true;
			_preset = CurrentPreset();
			SetWeights(PresetWeights(_preset));
			SceneTree obj = tree;
			object obj2 = <>O.<1>__Track;
			if (obj2 == null)
			{
				NodeAddedEventHandler val = Track;
				<>O.<1>__Track = val;
				obj2 = (object)val;
			}
			obj.NodeAdded += (NodeAddedEventHandler)obj2;
			SceneTree obj3 = tree;
			object obj4 = <>O.<2>__Untrack;
			if (obj4 == null)
			{
				NodeRemovedEventHandler val2 = Untrack;
				<>O.<2>__Untrack = val2;
				obj4 = (object)val2;
			}
			obj3.NodeRemoved += (NodeRemovedEventHandler)obj4;
			Node currentScene = tree.CurrentScene;
			if (currentScene != null)
			{
				Discover(currentScene, 0);
			}
			_timer = new Timer
			{
				Name = StringName.op_Implicit("JoanPaletteTick"),
				WaitTime = 0.1,
				Autostart = true,
				ProcessMode = (ProcessModeEnum)3
			};
			_timer.Timeout += delegate
			{
				Tick(tree);
			};
			((Node)tree.Root).AddChild((Node)(object)_timer, false, (InternalMode)0);
			AppearanceHotkey.Start(tree);
			ModEntry.TryInfo($"palette ready: {Pages.Count} pages, {Names.Length} appearances, current={Names[_preset]}.");
		}
	}

	private static void BuildPages()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		Shader shader = new Shader
		{
			Code = ReadText("shader.txt")
		};
		string[] pageKeys = PageKeys;
		foreach (string text in pageKeys)
		{
			bool flag = ((text == "s1_ironclad" || text == "s2_ironclad") ? true : false);
			bool flag2 = flag;
			ShaderMaterial val = new ShaderMaterial
			{
				Shader = shader
			};
			val.SetShaderParameter(StringName.op_Implicit("pixel_indices"), Variant.op_Implicit((GodotObject)(object)LoadTexture(text + "/index.png")));
			if (flag2)
			{
				val.SetShaderParameter(StringName.op_Implicit("emission_map"), Variant.op_Implicit((GodotObject)(object)LoadTexture(text + "/emission.png")));
			}
			for (int j = 0; j < 3; j++)
			{
				val.SetShaderParameter(StringName.op_Implicit($"palette_{j + 1}"), Variant.op_Implicit((GodotObject)(object)LoadTexture($"{text}/{j + 1}.png")));
			}
			Pages[text] = new Page
			{
				Key = text,
				Material = val,
				Emissive = flag2
			};
		}
	}

	private static string ReadText(string relative)
	{
		string text = "res://joanofarcironcladhpwear/palette/" + relative;
		string fileAsString = FileAccess.GetFileAsString(text);
		if (string.IsNullOrEmpty(fileAsString))
		{
			throw new InvalidOperationException("cannot read " + text);
		}
		return fileAsString;
	}

	private static ImageTexture LoadTexture(string relative)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		string text = "res://joanofarcironcladhpwear/palette/" + relative;
		FileAccess val = FileAccess.Open(text, (ModeFlags)1);
		try
		{
			if (val == null)
			{
				throw new InvalidOperationException($"cannot open {text} ({FileAccess.GetOpenError()})");
			}
			Image val2 = new Image();
			Error val3 = val2.LoadPngFromBuffer(val.GetBuffer((long)val.GetLength()));
			if ((int)val3 != 0)
			{
				throw new InvalidOperationException($"{text}: {val3}");
			}
			return ImageTexture.CreateFromImage(val2);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static int CurrentPreset()
	{
		return Math.Clamp(WearSettingsRuntime.Current.Appearance, 1, Names.Length) - 1;
	}

	internal static void RefreshAppearance(SceneTree tree)
	{
		int num = CurrentPreset();
		if (num != _preset)
		{
			SwitchTo(tree, num);
		}
	}

	internal static bool HasVisibleTarget()
	{
		foreach (Node item in Tracked)
		{
			try
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)item))
				{
					CanvasItem val = (CanvasItem)(object)((item is CanvasItem) ? item : null);
					if (val != null && val.IsVisibleInTree() && PageOf(item).Length != 0)
					{
						return true;
					}
				}
			}
			catch
			{
			}
		}
		return false;
	}

	private static void Tick(SceneTree tree)
	{
		try
		{
			RefreshAppearance(tree);
		}
		catch (Exception ex)
		{
			RecordFailure(0uL, ex);
		}
		Scratch.Clear();
		Scratch.AddRange(Tracked);
		foreach (Node item in Scratch)
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)item))
			{
				Tracked.Remove(item);
				continue;
			}
			ulong instanceId = ((GodotObject)item).GetInstanceId();
			if (RetryAfter.TryGetValue(instanceId, out var value) && Time.GetTicksMsec() < value)
			{
				continue;
			}
			try
			{
				if (item.IsInsideTree())
				{
					Apply(item);
				}
				RetryAfter.Remove(instanceId);
			}
			catch (Exception ex2)
			{
				RecordFailure(instanceId, ex2);
			}
		}
	}

	private static void RecordFailure(ulong id, Exception ex)
	{
		ulong ticksMsec = Time.GetTicksMsec();
		if (!RetryAfter.TryGetValue(id, out var value) || ticksMsec >= value)
		{
			ModEntry.TryInfo("palette node " + id + " temporarily unavailable: " + ex.GetBaseException().Message);
		}
		RetryAfter[id] = ticksMsec + 2000;
	}

	private static Vector4 PresetWeights(int preset)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		return (Vector4)(preset switch
		{
			1 => new Vector4(0f, 1f, 0f, 0f), 
			2 => new Vector4(0f, 0f, 1f, 0f), 
			3 => new Vector4(0f, 0f, 0f, 1f), 
			_ => new Vector4(1f, 0f, 0f, 0f), 
		});
	}

	private static void SwitchTo(SceneTree tree, int next)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Vector4 from = _weights;
		Vector4 to = PresetWeights(next);
		if (_fade != null && GodotObject.IsInstanceValid((GodotObject)(object)_fade))
		{
			_fade.Kill();
		}
		_preset = next;
		_fade = tree.CreateTween();
		_fade.TweenMethod(Callable.From<float>((Action<float>)delegate(float p)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			SetWeights(((Vector4)(ref from)).Lerp(to, p));
		}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), 0.25).SetTrans((TransitionType)1).SetEase((EaseType)2);
		ModEntry.TryInfo("[palette] " + Names[next]);
	}

	private static void SetWeights(Vector4 weights)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		_weights = weights;
		foreach (Page value in Pages.Values)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)value.Material))
			{
				try
				{
					value.Material.SetShaderParameter(StringName.op_Implicit("appearance_weights"), Variant.op_Implicit(weights));
					value.Material.SetShaderParameter(StringName.op_Implicit("emission"), Variant.op_Implicit(value.Emissive ? (weights.W * 0.26f) : 0f));
				}
				catch (Exception ex)
				{
					RecordFailure(0uL, ex);
				}
			}
		}
	}

	internal static void ApplyNow(Node spine)
	{
		if (!_started || _failed || !GodotObject.IsInstanceValid((GodotObject)(object)spine))
		{
			return;
		}
		try
		{
			Apply(spine);
		}
		catch (Exception ex)
		{
			RecordFailure(((GodotObject)spine).GetInstanceId(), ex);
		}
	}

	private static void Apply(Node spine)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		ulong instanceId = ((GodotObject)spine).GetInstanceId();
		Bound.TryGetValue(instanceId, out Binding value);
		string key = PageOf(spine);
		Pages.TryGetValue(key, out Page value2);
		if (value2 == null)
		{
			RestoreBinding(spine);
			return;
		}
		Variant val = ((GodotObject)spine).Call(StringName.op_Implicit("get_normal_material"), Array.Empty<Variant>());
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		Material val2 = (Material)(object)((obj is Material) ? obj : null);
		if (val2 != value2.Material)
		{
			Material previous = (Material)((value != null && val2 == value.Applied) ? ((object)value.Previous) : ((object)val2));
			((GodotObject)spine).Call(StringName.op_Implicit("set_normal_material"), (Variant[])(object)new Variant[1] { Variant.From<ShaderMaterial>(ref value2.Material) });
			Bound[instanceId] = new Binding
			{
				Previous = previous,
				Applied = value2.Material
			};
			if (spine.IsInsideTree())
			{
				((GodotObject)spine).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0.0) });
			}
		}
	}

	private static string PageOf(Node spine)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)spine).Get(StringName.op_Implicit("skeleton_data_res"));
		GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
		if (val2 == null)
		{
			return "";
		}
		val = val2.Get(StringName.op_Implicit("atlas_res"));
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		GodotObject obj2 = ((obj is Resource) ? obj : null);
		string text = ((obj2 != null) ? ((Resource)obj2).ResourcePath : null) ?? "";
		if (!text.EndsWith(".spatlas", StringComparison.Ordinal))
		{
			val = val2.Get(StringName.op_Implicit("skeleton_file_res"));
			GodotObject obj3 = ((Variant)(ref val)).AsGodotObject();
			GodotObject obj4 = ((obj3 is Resource) ? obj3 : null);
			text = ((obj4 != null) ? ((Resource)obj4).ResourcePath : null) ?? "";
		}
		if (!SkinCatalog.IsOurPath(text))
		{
			return "";
		}
		string text2 = StringExtensions.GetFile(text);
		int num = text2.LastIndexOf('.');
		if (num > 0)
		{
			text2 = text2.Substring(0, num);
		}
		return MapPage(text2);
	}

	private static string MapPage(string stem)
	{
		switch (stem)
		{
		case "s1_ironclad":
		case "s1_ironclad_shop":
			return "s1_ironclad";
		case "s2_ironclad":
		case "s2_ironclad_shop":
			return "s2_ironclad";
		case "s1_restsite":
			return "s1_restsite";
		case "s2_restsite":
			return "s2_restsite";
		default:
			return "";
		}
	}

	private static void Track(Node node)
	{
		if (((GodotObject)node).GetClass() == "SpineSprite")
		{
			Tracked.Add(node);
		}
	}

	private static void RestoreBinding(Node spine)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		ulong instanceId = ((GodotObject)spine).GetInstanceId();
		if (!Bound.TryGetValue(instanceId, out Binding value))
		{
			return;
		}
		Variant val = ((GodotObject)spine).Call(StringName.op_Implicit("get_normal_material"), Array.Empty<Variant>());
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		if (((obj is Material) ? obj : null) == value.Applied)
		{
			StringName obj2 = StringName.op_Implicit("set_normal_material");
			Variant[] array = new Variant[1];
			Variant val2;
			if (value.Previous != null)
			{
				val2 = Variant.From<Material>(ref value.Previous);
			}
			else
			{
				val = default(Variant);
				val2 = val;
			}
			array[0] = val2;
			((GodotObject)spine).Call(obj2, (Variant[])(object)array);
		}
		Bound.Remove(instanceId);
	}

	private static void Untrack(Node node)
	{
		if (!Tracked.Remove(node))
		{
			return;
		}
		ulong instanceId = ((GodotObject)node).GetInstanceId();
		try
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)node))
			{
				RestoreBinding(node);
			}
		}
		catch (Exception ex)
		{
			ModEntry.TryInfo("palette detach: " + ex.GetBaseException().Message);
		}
		finally
		{
			Bound.Remove(instanceId);
			RetryAfter.Remove(instanceId);
		}
	}

	private static void Discover(Node node, int depth)
	{
		if (depth <= 12 && GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			Track(node);
			int childCount = node.GetChildCount(false);
			for (int i = 0; i < childCount; i++)
			{
				Discover(node.GetChild(i, false), depth + 1);
			}
		}
	}
}
public static class PortraitNetwork
{
	private static readonly ConcurrentDictionary<ulong, int> Stages = new ConcurrentDictionary<ulong, int>();

	private static object? lobby;

	public static Func<object, int, bool>? Send { get; set; }

	public static void Receive(ulong sender, int stage)
	{
		if (stage >= 0 && stage <= 3)
		{
			Stages[sender] = stage;
		}
	}

	public static int StageFor(ulong sender)
	{
		if (!Stages.TryGetValue(sender, out var value))
		{
			return 0;
		}
		return value;
	}

	public static void Reset()
	{
		Stages.Clear();
		lobby = null;
	}

	internal static void Begin(object current)
	{
		if (lobby != current)
		{
			Stages.Clear();
			lobby = current;
		}
	}
}
internal static class PortraitSync
{
	private sealed class Original
	{
		internal Texture2D? Texture;

		internal Texture2D? Applied;
	}

	private sealed class RefreshTarget
	{
		internal Action Refresh;
	}

	[CompilerGenerated]
	private sealed class <Descendants>d__25<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T : notnull, Node
	{
		private int <>1__state;

		private T <>2__current;

		private int <>l__initialThreadId;

		private Node root;

		public Node <>3__root;

		private IEnumerator<Node> <>7__wrap1;

		private Node <child>5__3;

		private IEnumerator<T> <>7__wrap3;

		T IEnumerator<T>.Current
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
		public <Descendants>d__25(int <>1__state)
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
					goto IL_00f9;
				case 1:
					<>1__state = -3;
					goto IL_0092;
				case 2:
					{
						<>1__state = -4;
						goto IL_00d8;
					}
					IL_00f9:
					if (<>7__wrap1.MoveNext())
					{
						<child>5__3 = <>7__wrap1.Current;
						Node obj = <child>5__3;
						T val = (T)(object)((obj is T) ? obj : null);
						if (val != null)
						{
							<>2__current = val;
							<>1__state = 1;
							return true;
						}
						goto IL_0092;
					}
					<>m__Finally1();
					<>7__wrap1 = null;
					return false;
					IL_0092:
					<>7__wrap3 = Descendants<T>(<child>5__3).GetEnumerator();
					<>1__state = -4;
					goto IL_00d8;
					IL_00d8:
					if (<>7__wrap3.MoveNext())
					{
						T current = <>7__wrap3.Current;
						<>2__current = current;
						<>1__state = 2;
						return true;
					}
					<>m__Finally2();
					<>7__wrap3 = null;
					<child>5__3 = null;
					goto IL_00f9;
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
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			<Descendants>d__25<T> <Descendants>d__;
			if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
			{
				<>1__state = 0;
				<Descendants>d__ = this;
			}
			else
			{
				<Descendants>d__ = new <Descendants>d__25<T>(0);
			}
			<Descendants>d__.root = <>3__root;
			return <Descendants>d__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}

	private static readonly ConditionalWeakTable<TextureRect, Original> Originals = new ConditionalWeakTable<TextureRect, Original>();

	private static readonly ConditionalWeakTable<Node, RefreshTarget> RefreshTargets = new ConditionalWeakTable<Node, RefreshTarget>();

	private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

	private static readonly FieldInfo? LobbyCharacter = AccessTools.Field(typeof(NRemoteLobbyPlayer), "_character");

	private static readonly FieldInfo? TopBarPlayer = AccessTools.Field(typeof(NTopBar), "_player");

	private static WeakReference<NCharacterSelectScreen>? CurrentMenu;

	private static object? SendLobby;

	private static bool? LastSendSucceeded;

	private static readonly FieldInfo? ActiveVotes = AccessTools.Field(typeof(NMultiplayerVoteContainer), "_votes");

	private static readonly FieldInfo? OutgoingVotes = AccessTools.Field(typeof(NMultiplayerVoteContainer), "_iconsAnimatingOut");

	private static readonly Type? VoteType = typeof(NMultiplayerVoteContainer).GetNestedType("VoteIcon", BindingFlags.NonPublic);

	private static readonly FieldInfo? VotePlayer = VoteType?.GetField("player");

	private static readonly FieldInfo? VoteNode = VoteType?.GetField("node");

	internal static void RefreshVotePortraits(NMultiplayerVoteContainer container)
	{
		bool flag = false;
		FieldInfo[] array = new FieldInfo[2] { ActiveVotes, OutgoingVotes };
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i]?.GetValue(container) is IEnumerable enumerable))
			{
				continue;
			}
			foreach (object item in enumerable)
			{
				object? obj = VotePlayer?.GetValue(item);
				Player val = (Player)((obj is Player) ? obj : null);
				if (val == null)
				{
					continue;
				}
				object? obj2 = VoteNode?.GetValue(item);
				TextureRect val2 = (TextureRect)((obj2 is TextureRect) ? obj2 : null);
				if (val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2) && !((GodotObject)val2).IsQueuedForDeletion())
				{
					flag = true;
					TextureRect nodeOrNull = ((Node)val2).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("Outline"));
					int num = ((val.Character is Ironclad && SkinSelectionGate.PlayerAllowed(val)) ? HpWear.PortraitStage(val) : 0);
					if (num > 0)
					{
						Apply(val2, num, selection: false);
						Apply(nodeOrNull, num, selection: false, outline: true);
					}
					else
					{
						Restore(val2);
						Restore(nodeOrNull);
					}
				}
			}
		}
		if (!flag)
		{
			Timer nodeOrNull2 = ((Node)container).GetNodeOrNull<Timer>(NodePath.op_Implicit("JoanofArcIroncladHpwearPortraitTimer"));
			if (nodeOrNull2 != null)
			{
				nodeOrNull2.Stop();
			}
		}
	}

	internal static void TrackVotePortraits(NMultiplayerVoteContainer container)
	{
		RefreshVotePortraits(container);
		if (ActiveVotes?.GetValue(container) is ICollection { Count: >0 } || OutgoingVotes?.GetValue(container) is ICollection { Count: >0 })
		{
			Timer((Node)(object)container, delegate
			{
				RefreshVotePortraits(container);
			});
			Timer nodeOrNull = ((Node)container).GetNodeOrNull<Timer>(NodePath.op_Implicit("JoanofArcIroncladHpwearPortraitTimer"));
			if (nodeOrNull != null)
			{
				nodeOrNull.Start(-1.0);
			}
		}
	}

	internal static void RefreshTopBar(NTopBar? topBar)
	{
		if (topBar == null || !GodotObject.IsInstanceValid((GodotObject)(object)topBar))
		{
			return;
		}
		object? obj = TopBarPlayer?.GetValue(topBar);
		Player val = (Player)((obj is Player) ? obj : null);
		if (val == null)
		{
			return;
		}
		NTopBarPortrait portrait = topBar.Portrait;
		if (portrait == null || !GodotObject.IsInstanceValid((GodotObject)(object)portrait))
		{
			return;
		}
		foreach (TextureRect item in PortraitSync.Descendants<TextureRect>((Node)(object)portrait))
		{
			RunIcon(item, val);
		}
	}

	internal static void RefreshRun()
	{
		try
		{
			NRun instance = NRun.Instance;
			NGlobalUi val = ((instance != null) ? instance.GlobalUi : null);
			if (val == null)
			{
				return;
			}
			RefreshTopBar(val.TopBar);
			NMultiplayerPlayerStateContainer multiplayerPlayerContainer = val.MultiplayerPlayerContainer;
			if (multiplayerPlayerContainer == null)
			{
				return;
			}
			foreach (NMultiplayerPlayerState item in PortraitSync.Descendants<NMultiplayerPlayerState>((Node)(object)multiplayerPlayerContainer))
			{
				RunIcon(((Node)item).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%CharacterIcon")), item.Player);
			}
		}
		catch (Exception ex)
		{
			ModEntry.TryInfo("portrait run refresh: " + ex.GetBaseException().Message);
		}
	}

	internal static void RefreshMenuNow(Node source)
	{
		for (Node val = source; val != null; val = val.GetParent())
		{
			NCharacterSelectScreen val2 = (NCharacterSelectScreen)(object)((val is NCharacterSelectScreen) ? val : null);
			if (val2 != null)
			{
				CurrentMenu = new WeakReference<NCharacterSelectScreen>(val2);
				break;
			}
		}
		if (CurrentMenu != null && CurrentMenu.TryGetTarget(out NCharacterSelectScreen target) && GodotObject.IsInstanceValid((GodotObject)(object)target))
		{
			long nextSend = 0L;
			int lastSent = -1;
			Menu(target, ref nextSend, ref lastSent);
		}
	}

	internal static void ApplyLobbyRow(NRemoteLobbyPlayer row, ulong localId, int localStage)
	{
		TextureRect nodeOrNull = ((Node)row).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%CharacterIcon"));
		int num = ((row.PlayerId == localId) ? localStage : PortraitNetwork.StageFor(row.PlayerId));
		if (LobbyCharacter?.GetValue(row) is Ironclad && num > 0 && SkinSelectionGate.AllowPlayer(row.PlayerId))
		{
			Texture2D val = ((nodeOrNull != null) ? nodeOrNull.Texture : null);
			Apply(nodeOrNull, num, selection: false);
			if (nodeOrNull != null && val != nodeOrNull.Texture)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 3);
				defaultInterpolatedStringHandler.AppendLiteral("portrait lobby: player=");
				defaultInterpolatedStringHandler.AppendFormatted(row.PlayerId);
				defaultInterpolatedStringHandler.AppendLiteral(" stage=");
				defaultInterpolatedStringHandler.AppendFormatted(num);
				defaultInterpolatedStringHandler.AppendLiteral(" texture=");
				Texture2D texture = nodeOrNull.Texture;
				defaultInterpolatedStringHandler.AppendFormatted((texture != null) ? ((Resource)texture).ResourcePath : null);
				ModEntry.TryInfo(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}
		else
		{
			Restore(nodeOrNull);
		}
	}

	internal static void RefreshLobbyRow(NRemoteLobbyPlayer row)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentMenu != null && CurrentMenu.TryGetTarget(out NCharacterSelectScreen target) && GodotObject.IsInstanceValid((GodotObject)(object)target) && ((CanvasItem)target).IsVisibleInTree())
		{
			StartRunLobby lobby = target.Lobby;
			if (lobby != null)
			{
				Node val = ((lobby.LocalPlayer.character is Ironclad) ? MenuSpine((Node)(object)target, ((AbstractModel)lobby.LocalPlayer.character).Id.Entry) : null);
				int localStage = ((val != null && SpineSwap.IsOurs(val) && SkinSelectionGate.MenuAllowed()) ? CharSelectCycle.CurrentStage : 0);
				ApplyLobbyRow(row, lobby.NetService.NetId, localStage);
			}
		}
	}

	internal static Texture2D? Texture(int stage, bool selection, bool outline = false)
	{
		stage = Math.Clamp(stage, 1, 3);
		string text = "res://joanofarcironcladhpwear/" + $"s{stage}_" + (selection ? "select_icon" : (outline ? "run_outline" : "run_icon")) + ".ctex";
		if (Textures.TryGetValue(text, out Texture2D value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			return value;
		}
		Texture2D val = ResourceLoader.Load<Texture2D>(text, (string)null, (CacheMode)1);
		if (val != null)
		{
			Textures[text] = val;
		}
		return val;
	}

	internal static void Apply(TextureRect? node, int stage, bool selection, bool outline = false)
	{
		if (node == null || !GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return;
		}
		Texture2D val = Texture(stage, selection, outline);
		if (val != null)
		{
			Original value = Originals.GetValue(node, (TextureRect n) => new Original
			{
				Texture = n.Texture
			});
			if (value.Applied != null && node.Texture != value.Applied)
			{
				value.Texture = node.Texture;
			}
			if (node.Texture != val)
			{
				node.Texture = val;
			}
			value.Applied = val;
		}
	}

	internal static void Restore(TextureRect? node)
	{
		if (node != null && GodotObject.IsInstanceValid((GodotObject)(object)node) && Originals.TryGetValue(node, out Original value))
		{
			if (node.Texture == value.Applied)
			{
				node.Texture = value.Texture;
			}
			Originals.Remove(node);
		}
	}

	[IteratorStateMachine(typeof(<Descendants>d__25<>))]
	internal static IEnumerable<T> Descendants<T>(Node root) where T : Node
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <Descendants>d__25<T>(-2)
		{
			<>3__root = root
		};
	}

	internal static void Timer(Node owner, Action refresh)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		RefreshTargets.GetOrCreateValue(owner).Refresh = refresh;
		string text = "JoanofArcIroncladHpwearPortraitTimer";
		if (owner.HasNode(NodePath.op_Implicit(text)))
		{
			return;
		}
		Timer val = new Timer
		{
			Name = StringName.op_Implicit(text),
			WaitTime = 0.1,
			OneShot = false,
			ProcessMode = (ProcessModeEnum)3
		};
		val.Timeout += delegate
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)owner) || !owner.IsInsideTree())
			{
				return;
			}
			try
			{
				if (RefreshTargets.TryGetValue(owner, out RefreshTarget value))
				{
					value.Refresh();
				}
			}
			catch (Exception ex)
			{
				ModEntry.TryInfo("portrait refresh: " + ex.GetBaseException().Message);
			}
		};
		owner.AddChild((Node)(object)val, false, (InternalMode)0);
		val.Start(-1.0);
		try
		{
			refresh();
		}
		catch
		{
		}
	}

	internal static Node? MenuSpine(Node screen, string characterEntry)
	{
		Control nodeOrNull = screen.GetNodeOrNull<Control>(NodePath.op_Implicit("AnimatedBg/" + characterEntry + "_bg"));
		if (nodeOrNull != null)
		{
			return SpineSwap.FindSpine((Node?)(object)nodeOrNull);
		}
		return null;
	}

	internal static void Menu(NCharacterSelectScreen screen, ref long nextSend, ref int lastSent)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		if (!((CanvasItem)screen).IsVisibleInTree())
		{
			return;
		}
		CurrentMenu = new WeakReference<NCharacterSelectScreen>(screen);
		StartRunLobby lobby = screen.Lobby;
		if (lobby == null)
		{
			return;
		}
		PortraitNetwork.Begin(lobby);
		bool flag = SkinSelectionGate.MenuAllowed();
		if (lobby.LocalPlayer.character is Ironclad)
		{
			Node val = MenuSpine((Node)(object)screen, ((AbstractModel)lobby.LocalPlayer.character).Id.Entry);
			flag &= val != null && SpineSwap.IsOurs(val);
		}
		int currentStage = CharSelectCycle.CurrentStage;
		foreach (NCharacterSelectButton item in PortraitSync.Descendants<NCharacterSelectButton>((Node)(object)screen))
		{
			if (item.Character is Ironclad)
			{
				TextureRect nodeOrNull = ((Node)item).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%Icon"));
				TextureRect nodeOrNull2 = ((Node)item).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%IconAdd"));
				if (flag && !item.IsLocked)
				{
					Apply(nodeOrNull, currentStage, selection: true);
					Apply(nodeOrNull2, currentStage, selection: true);
				}
				else
				{
					Restore(nodeOrNull);
					Restore(nodeOrNull2);
				}
			}
		}
		int num = ((flag && lobby.LocalPlayer.character is Ironclad) ? currentStage : 0);
		if (num != lastSent || Environment.TickCount64 >= nextSend)
		{
			try
			{
				bool flag2 = PortraitNetwork.Send?.Invoke(lobby.NetService, num) ?? false;
				if (SendLobby != lobby)
				{
					SendLobby = lobby;
					LastSendSucceeded = null;
				}
				if (lobby.Players.Count > 1 && LastSendSucceeded != flag2)
				{
					ModEntry.TryInfo($"portrait lobby network: send={(flag2 ? "ready" : "unavailable")} stage={num}; local UI refresh remains active");
				}
				LastSendSucceeded = flag2;
			}
			catch (Exception ex)
			{
				ModEntry.TryInfo("portrait lobby network: " + ex.GetBaseException().Message);
			}
			nextSend = Environment.TickCount64 + 1500;
			lastSent = num;
		}
		foreach (NRemoteLobbyPlayer item2 in PortraitSync.Descendants<NRemoteLobbyPlayer>((Node)(object)screen))
		{
			ApplyLobbyRow(item2, lobby.NetService.NetId, num);
		}
	}

	internal static void RunIcon(TextureRect? icon, Player player)
	{
		if (player.Character is Ironclad && SkinSelectionGate.PlayerAllowed(player))
		{
			int num = HpWear.PortraitStage(player);
			if (num > 0)
			{
				Texture2D val = ((icon != null) ? icon.Texture : null);
				Apply(icon, num, selection: false);
				if (icon != null && val != icon.Texture)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 5);
					defaultInterpolatedStringHandler.AppendLiteral("portrait run: player=");
					defaultInterpolatedStringHandler.AppendFormatted(player.NetId);
					defaultInterpolatedStringHandler.AppendLiteral(" stage=");
					defaultInterpolatedStringHandler.AppendFormatted(num);
					defaultInterpolatedStringHandler.AppendLiteral(" hp=");
					defaultInterpolatedStringHandler.AppendFormatted(player.Creature.CurrentHp);
					defaultInterpolatedStringHandler.AppendLiteral("/");
					defaultInterpolatedStringHandler.AppendFormatted(player.Creature.MaxHp);
					defaultInterpolatedStringHandler.AppendLiteral(" texture=");
					Texture2D texture = icon.Texture;
					defaultInterpolatedStringHandler.AppendFormatted((texture != null) ? ((Resource)texture).ResourcePath : null);
					ModEntry.TryInfo(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			else
			{
				Restore(icon);
			}
		}
		else
		{
			Restore(icon);
		}
	}
}
[HarmonyPatch(typeof(NMultiplayerVoteContainer), "RefreshPlayerVotes")]
internal static class PortraitVotePatch
{
	[HarmonyPostfix]
	private static void Postfix(NMultiplayerVoteContainer __instance)
	{
		PortraitSync.TrackVotePortraits(__instance);
	}
}
[HarmonyPatch(typeof(NRemoteLobbyPlayer), "_Ready")]
internal static class PortraitLobbyRowPatch
{
	[HarmonyPostfix]
	private static void Postfix(NRemoteLobbyPlayer __instance)
	{
		PortraitSync.Timer((Node)(object)__instance, delegate
		{
			PortraitSync.RefreshLobbyRow(__instance);
		});
	}
}
[HarmonyPatch(typeof(NCharacterSelectScreen), "_Ready")]
internal static class PortraitMenuPatch
{
	[HarmonyPostfix]
	private static void Postfix(NCharacterSelectScreen __instance)
	{
		long next = 0L;
		int last = -1;
		PortraitSync.Timer((Node)(object)__instance, delegate
		{
			PortraitSync.Menu(__instance, ref next, ref last);
		});
	}
}
[HarmonyPatch(typeof(NCharacterSelectScreen), "CleanUpLobby")]
internal static class PortraitLobbyCleanupPatch
{
	[HarmonyPostfix]
	private static void Postfix()
	{
		PortraitNetwork.Reset();
	}
}
[HarmonyPatch(typeof(NTopBarPortrait), "Initialize")]
internal static class PortraitTopBarPatch
{
	[HarmonyPostfix]
	private static void Postfix(NTopBarPortrait __instance, Player player)
	{
		PortraitSync.Timer((Node)(object)__instance, delegate
		{
			foreach (TextureRect item in PortraitSync.Descendants<TextureRect>((Node)(object)__instance))
			{
				PortraitSync.RunIcon(item, player);
			}
		});
	}
}
[HarmonyPatch(typeof(NMultiplayerPlayerState), "_Ready")]
internal static class PortraitPeerStatePatch
{
	[HarmonyPostfix]
	private static void Postfix(NMultiplayerPlayerState __instance)
	{
		PortraitSync.Timer((Node)(object)__instance, delegate
		{
			PortraitSync.RunIcon(((Node)__instance).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%CharacterIcon")), __instance.Player);
		});
	}
}
internal static class RitsuSettingsLoader
{
	private static bool registered;

	private static bool watching;

	private static bool registering;

	private static bool retryQueued;

	private static Assembly? bridge;

	private static readonly object Sync = new object();

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
		if (name.Name == typeof(RitsuSettingsLoader).Assembly.GetName().Name)
		{
			return typeof(RitsuSettingsLoader).Assembly;
		}
		string? name2 = name.Name;
		if (name2 == null || !name2.StartsWith("STS2-RitsuLib", StringComparison.Ordinal))
		{
			return null;
		}
		return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => Compatible(name, a.GetName()));
	}

	private static void Loaded(object? sender, AssemblyLoadEventArgs args)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		string? name = args.LoadedAssembly.GetName().Name;
		if (name == null || !name.StartsWith("STS2-RitsuLib", StringComparison.Ordinal))
		{
			return;
		}
		lock (Sync)
		{
			if (registered || retryQueued)
			{
				return;
			}
			retryQueued = true;
		}
		try
		{
			Callable val = Callable.From((Action)Retry);
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
		catch
		{
			lock (Sync)
			{
				retryQueued = false;
			}
		}
	}

	private static void Retry()
	{
		lock (Sync)
		{
			retryQueued = false;
		}
		TryRegister();
	}

	internal static void TryRegister()
	{
		if (registered || registering)
		{
			return;
		}
		if (!watching)
		{
			AppDomain.CurrentDomain.AssemblyLoad += Loaded;
			watching = true;
		}
		registering = true;
		try
		{
			Assembly assembly = typeof(RitsuSettingsLoader).Assembly;
			AssemblyLoadContext assemblyLoadContext = AssemblyLoadContext.GetLoadContext(assembly) ?? AssemblyLoadContext.Default;
			using Stream stream = assembly.GetManifestResourceStream("Joan.RitsuDependencies") ?? throw new InvalidOperationException("Ritsu dependency manifest missing");
			using StreamReader streamReader = new StreamReader(stream);
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					AssemblyName assemblyName = new AssemblyName(text);
					Assembly assembly2 = ResolveLoaded(assemblyLoadContext, assemblyName) ?? assemblyLoadContext.LoadFromAssemblyName(new AssemblyName(assemblyName.Name));
					if (!Compatible(assemblyName, assembly2.GetName()))
					{
						throw new InvalidOperationException("Incompatible RitsuLib: " + assembly2.FullName);
					}
				}
			}
			assemblyLoadContext.Resolving -= ResolveLoaded;
			assemblyLoadContext.Resolving += ResolveLoaded;
			if (bridge == null)
			{
				using Stream assembly3 = assembly.GetManifestResourceStream("Joan.RitsuBridge") ?? throw new InvalidOperationException("Ritsu settings bridge missing");
				bridge = assemblyLoadContext.LoadFromStream(assembly3);
			}
			bridge.GetType("JoanofArcIroncladHpwear.SettingsBridge.RitsuSettingsBridge", throwOnError: true).GetMethod("Register", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);
			registered = true;
			AppDomain.CurrentDomain.AssemblyLoad -= Loaded;
			watching = false;
			ModEntry.TryInfo("RitsuLib 贞德血量换装设置已注册。");
		}
		catch (Exception ex)
		{
			ModEntry.TryInfo("RitsuLib 设置暂不可用，保留默认换装：" + ex.GetBaseException().Message);
		}
		finally
		{
			registering = false;
		}
	}
}
public static class SkinCatalog
{
	[CompilerGenerated]
	private sealed class <AllResources>d__17 : IEnumerable<(string Group, int Stage)>, IEnumerable, IEnumerator<(string Group, int Stage)>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private (string Group, int Stage) <>2__current;

		private int <>l__initialThreadId;

		private string[] <>7__wrap1;

		private int <>7__wrap2;

		private string <group>5__4;

		(string Group, int Stage) IEnumerator<(string, int)>.Current
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
		public <AllResources>d__17(int <>1__state)
		{
			this.<>1__state = <>1__state;
			<>l__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			<>7__wrap1 = null;
			<group>5__4 = null;
			<>1__state = -2;
		}

		private bool MoveNext()
		{
			switch (<>1__state)
			{
			default:
				return false;
			case 0:
				<>1__state = -1;
				<>7__wrap1 = BattleGroups;
				<>7__wrap2 = 0;
				goto IL_00ae;
			case 1:
				<>1__state = -1;
				<>2__current = (Group: <group>5__4, Stage: 2);
				<>1__state = 2;
				return true;
			case 2:
				<>1__state = -1;
				<group>5__4 = null;
				<>7__wrap2++;
				goto IL_00ae;
			case 3:
				<>1__state = -1;
				<>2__current = (Group: "charselect", Stage: 2);
				<>1__state = 4;
				return true;
			case 4:
				<>1__state = -1;
				<>2__current = (Group: "charselect", Stage: 3);
				<>1__state = 5;
				return true;
			case 5:
				{
					<>1__state = -1;
					return false;
				}
				IL_00ae:
				if (<>7__wrap2 < <>7__wrap1.Length)
				{
					<group>5__4 = <>7__wrap1[<>7__wrap2];
					<>2__current = (Group: <group>5__4, Stage: 1);
					<>1__state = 1;
					return true;
				}
				<>7__wrap1 = null;
				<>2__current = (Group: "charselect", Stage: 1);
				<>1__state = 3;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<(string Group, int Stage)> IEnumerable<(string, int)>.GetEnumerator()
		{
			if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
			{
				<>1__state = 0;
				return this;
			}
			return new <AllResources>d__17(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<(string, int)>)this).GetEnumerator();
		}
	}

	public const string ModId = "JoanofArcIroncladHpwear";

	public const string Root = "res://joanofarcironcladhpwear/";

	public const string TargetCharacter = "ironclad";

	public const string CombatGroup = "ironclad";

	public const string ShopGroup = "ironclad_shop";

	public const string RestGroup = "restsite";

	public const string CharSelectGroup = "charselect";

	public const int CharSelectMaxStage = 3;

	public static readonly string[] BattleGroups = new string[3] { "ironclad", "ironclad_shop", "restsite" };

	public static string Stem(string group, int stage)
	{
		return $"s{stage}_{group}";
	}

	public static string DataPath(string group, int stage)
	{
		return "res://joanofarcironcladhpwear/" + Stem(group, stage) + "_data.tres";
	}

	public static string SkelPath(string group, int stage)
	{
		return "res://joanofarcironcladhpwear/" + Stem(group, stage) + ".spskel";
	}

	public static string AtlasPath(string group, int stage)
	{
		return "res://joanofarcironcladhpwear/" + Stem(group, stage) + ".spatlas";
	}

	public static string TexturePath(string group, int stage)
	{
		return "res://joanofarcironcladhpwear/" + Stem(group, stage) + ".ctex";
	}

	public static bool IsOurPath(string? resourcePath)
	{
		if (string.IsNullOrEmpty(resourcePath))
		{
			return false;
		}
		if (resourcePath.StartsWith("res://joanofarcironcladhpwear/", StringComparison.Ordinal))
		{
			return true;
		}
		if (resourcePath.StartsWith("res://sts2_skin_runtime/", StringComparison.Ordinal) && resourcePath.Contains("/joanofarcironcladhpwear/", StringComparison.Ordinal))
		{
			return !resourcePath.Contains("/../", StringComparison.Ordinal);
		}
		return false;
	}

	public static int StageFromPath(string group, string? resourcePath)
	{
		for (int i = 1; i <= MaxStageFor(group); i++)
		{
			if (IsOurPath(resourcePath) && resourcePath.EndsWith("/" + Stem(group, i) + ".spskel", StringComparison.Ordinal))
			{
				return i;
			}
		}
		return 0;
	}

	public static int MaxStageFor(string group)
	{
		if (!(group == "charselect"))
		{
			return 2;
		}
		return 3;
	}

	[IteratorStateMachine(typeof(<AllResources>d__17))]
	public static IEnumerable<(string Group, int Stage)> AllResources()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <AllResources>d__17(-2);
	}
}
internal static class SkinSelectionGate
{
	private const BindingFlags Flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private static Type? service;

	private static MethodInfo? selected;

	private static MethodInfo? direct;

	private static MethodInfo? playerScope;

	private static bool managerPresent;

	private static long searchAfter;

	static SkinSelectionGate()
	{
		AppDomain.CurrentDomain.AssemblyLoad += delegate
		{
			searchAfter = 0L;
		};
	}

	private static bool HasManager()
	{
		if (service != null)
		{
			return true;
		}
		if (Environment.TickCount64 < searchAfter)
		{
			return managerPresent;
		}
		searchAfter = Environment.TickCount64 + 1000;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type type = assembly.GetType("STS2SkinChanger.Core.SkinService", throwOnError: false);
			if (type == null)
			{
				string name = assembly.GetName().Name;
				if ((name == "Gurio.SkinChanger" || name == "STS2SkinChanger") ? true : false)
				{
					managerPresent = true;
				}
				continue;
			}
			managerPresent = true;
			service = type;
			selected = service.GetMethod("GetSelectedRuntimeProvider", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(string) }, null);
			direct = service.GetMethod("IsDirectCharacterRuntimeProviderSelected", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
			{
				typeof(string),
				typeof(string)
			}, null);
			playerScope = assembly.GetType("STS2SkinChanger.Core.MultiplayerSkinSync", throwOnError: false)?.GetMethod("BeginPlayerSelectionScope", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(ulong) }, null);
			break;
		}
		return managerPresent;
	}

	private static bool Query(bool perPlayer, ulong id)
	{
		if (!HasManager())
		{
			return true;
		}
		if (selected == null || direct == null || (perPlayer && playerScope == null))
		{
			return false;
		}
		try
		{
			using (perPlayer ? (playerScope.Invoke(null, new object[1] { id }) as IDisposable) : null)
			{
				string text = selected.Invoke(null, new object[1] { "ironclad" }) as string;
				if (string.IsNullOrEmpty(text) || (!text.Equals("JoanofArcIroncladHpwear", StringComparison.OrdinalIgnoreCase) && !text.StartsWith("JoanofArcIroncladHpwear::source:", StringComparison.OrdinalIgnoreCase)))
				{
					return false;
				}
				object obj = direct.Invoke(null, new object[2] { text, "ironclad" });
				return obj is bool && (bool)obj;
			}
		}
		catch
		{
			return false;
		}
	}

	internal static bool MenuAllowed()
	{
		return Query(perPlayer: false, 0uL);
	}

	internal static bool AllowPlayer(ulong id)
	{
		return Query(perPlayer: true, id);
	}

	internal static bool PlayerAllowed(Player? player)
	{
		if (HasManager())
		{
			if (player != null)
			{
				return AllowPlayer(player.NetId);
			}
			return false;
		}
		return true;
	}

	internal static bool SceneAllowed()
	{
		if (!HasManager() || MenuAllowed())
		{
			return true;
		}
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
				RunState obj2 = instance.DebugOnlyGetState();
				obj = ((obj2 != null) ? obj2.Players : null);
			}
			IReadOnlyList<Player> readOnlyList = (IReadOnlyList<Player>)obj;
			if (readOnlyList != null)
			{
				foreach (Player item in readOnlyList)
				{
					if (((item != null) ? item.Character : null) is Ironclad && AllowPlayer(item.NetId))
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
}
internal static class SpineSwap
{
	internal readonly struct MegaScope : IDisposable
	{
		internal readonly MegaSprite Sprite;

		internal MegaScope(Node spine)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			Sprite = new MegaSprite(Variant.op_Implicit((GodotObject)(object)spine));
		}

		public void Dispose()
		{
		}
	}

	internal static Node? FindSpine(Node? root, int depth = 0)
	{
		if (root == null || depth > 6 || !GodotObject.IsInstanceValid((GodotObject)(object)root))
		{
			return null;
		}
		if (root is Node2D && ((GodotObject)root).GetClass() == "SpineSprite")
		{
			return root;
		}
		foreach (Node child in root.GetChildren(false))
		{
			Node val = FindSpine(child, depth + 1);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	internal static Resource? GetSkeletonData(Node spine)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)spine).Get(StringName.op_Implicit("skeleton_data_res"));
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		return (Resource?)(object)((obj is Resource) ? obj : null);
	}

	internal static string? GetSkeletonPath(Node spine)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Resource skeletonData = GetSkeletonData(spine);
		if (skeletonData == null)
		{
			return null;
		}
		Variant val = ((GodotObject)skeletonData).Get(StringName.op_Implicit("skeleton_file_res"));
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		GodotObject obj2 = ((obj is Resource) ? obj : null);
		if (obj2 == null)
		{
			return null;
		}
		return ((Resource)obj2).ResourcePath;
	}

	internal static bool IsOurs(Node spine)
	{
		return SkinCatalog.IsOurPath(GetSkeletonPath(spine));
	}

	internal static Resource? LoadStageData(string group, int stage)
	{
		string text = SkinCatalog.DataPath(group, stage);
		try
		{
			Resource obj = GD.Load<Resource>(text);
			if (obj == null)
			{
				ModEntry.Log.Error("missing skeleton data resource: " + text, 1);
			}
			return obj;
		}
		catch (Exception ex)
		{
			ModEntry.Log.Error("cannot load " + text + ": " + ex.Message, 1);
			return null;
		}
	}

	internal static bool SetSkeletonData(Node spine, Resource data)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		string skeletonPath = GetSkeletonPath(spine);
		Variant val = ((GodotObject)data).Get(StringName.op_Implicit("skeleton_file_res"));
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		GodotObject obj2 = ((obj is Resource) ? obj : null);
		string text = ((obj2 != null) ? ((Resource)obj2).ResourcePath : null);
		if (text == null)
		{
			return false;
		}
		if (string.Equals(skeletonPath, text, StringComparison.Ordinal))
		{
			return true;
		}
		((GodotObject)spine).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)data));
		if (SkinCatalog.IsOurPath(text))
		{
			UnderwearVisibility.Attach(spine);
			Palette.ApplyNow(spine);
		}
		return string.Equals(GetSkeletonPath(spine), text, StringComparison.Ordinal);
	}

	internal static string? CurrentAnimation(Node spine)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Variant val = ((GodotObject)spine).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
			GodotObject obj = ((Variant)(ref val)).AsGodotObject();
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				val = obj.Call(StringName.op_Implicit("get_current"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0) });
				obj2 = ((Variant)(ref val)).AsGodotObject();
			}
			object obj3;
			if (obj2 == null)
			{
				obj3 = null;
			}
			else
			{
				val = ((GodotObject)obj2).Call(StringName.op_Implicit("get_animation"), Array.Empty<Variant>());
				obj3 = ((Variant)(ref val)).AsGodotObject();
			}
			object result;
			if (obj3 == null)
			{
				result = null;
			}
			else
			{
				val = ((GodotObject)obj3).Call(StringName.op_Implicit("get_name"), Array.Empty<Variant>());
				result = ((Variant)(ref val)).AsString();
			}
			return (string?)result;
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("read animation failed: " + ex.Message, 1);
			return null;
		}
	}

	internal static bool IsMixing(Node spine)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Variant val = ((GodotObject)spine).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
			GodotObject obj = ((Variant)(ref val)).AsGodotObject();
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				val = obj.Call(StringName.op_Implicit("get_current"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0) });
				obj2 = ((Variant)(ref val)).AsGodotObject();
			}
			GodotObject val2 = (GodotObject)obj2;
			if (val2 == null || !GodotObject.IsInstanceValid(val2))
			{
				return false;
			}
			val = val2.Call(StringName.op_Implicit("get_mixing_from"), Array.Empty<Variant>());
			GodotObject val3 = ((Variant)(ref val)).AsGodotObject();
			if (val3 == null || !GodotObject.IsInstanceValid(val3))
			{
				return false;
			}
			val = val2.Call(StringName.op_Implicit("get_mix_duration"), Array.Empty<Variant>());
			double num = ((Variant)(ref val)).AsDouble();
			val = val2.Call(StringName.op_Implicit("get_mix_time"), Array.Empty<Variant>());
			double num2 = ((Variant)(ref val)).AsDouble();
			return num > 0.0 && num2 + 0.001 < num;
		}
		catch
		{
			return false;
		}
	}

	internal static bool Play(Node spine, string animation, bool loop)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(animation))
		{
			return false;
		}
		try
		{
			Variant val = ((GodotObject)spine).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
			GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
			if (val2 == null)
			{
				return false;
			}
			val2.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
			{
				Variant.op_Implicit(animation),
				Variant.op_Implicit(loop),
				Variant.op_Implicit(0)
			});
			return true;
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("play '" + animation + "' failed: " + ex.Message, 1);
			return false;
		}
	}
}
internal static class UnderwearVisibility
{
	private sealed class State
	{
		internal Color? HiddenColor;

		internal ulong DataId;
	}

	private static readonly ConditionalWeakTable<Node, State> States = new ConditionalWeakTable<Node, State>();

	internal static void Attach(Node spine)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)spine))
		{
			return;
		}
		if (!States.TryGetValue(spine, out State value))
		{
			value = new State();
			States.Add(spine, value);
			State captured = value;
			((GodotObject)spine).Connect(StringName.op_Implicit("before_animation_state_apply"), Callable.From<GodotObject>((Action<GodotObject>)delegate
			{
				Restore(spine, captured);
			}), 0u);
			((GodotObject)spine).Connect(StringName.op_Implicit("before_world_transforms_change"), Callable.From<GodotObject>((Action<GodotObject>)delegate
			{
				Apply(spine, captured);
			}), 0u);
			((GodotObject)spine).Connect(StringName.op_Implicit("_internal_spine_objects_invalidated"), Callable.From((Action)delegate
			{
				captured.HiddenColor = null;
			}), 0u);
			spine.TreeExiting += delegate
			{
				Restore(spine, captured);
			};
		}
		Restore(spine, value);
		Apply(spine, value);
	}

	private static GodotObject? Slot(Node spine)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)spine).Call(StringName.op_Implicit("get_skeleton"), Array.Empty<Variant>());
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		if (obj == null)
		{
			return null;
		}
		val = obj.Call(StringName.op_Implicit("find_slot"), (Variant[])(object)new Variant[1] { Variant.op_Implicit("neiku") });
		return ((Variant)(ref val)).AsGodotObject();
	}

	private static void Restore(Node spine, State state)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (!state.HiddenColor.HasValue)
		{
			return;
		}
		Color value = state.HiddenColor.Value;
		state.HiddenColor = null;
		if (!GodotObject.IsInstanceValid((GodotObject)(object)spine))
		{
			return;
		}
		Resource? skeletonData = SpineSwap.GetSkeletonData(spine);
		if (((skeletonData != null) ? new ulong?(((GodotObject)skeletonData).GetInstanceId()) : ((ulong?)null)) == state.DataId)
		{
			GodotObject? obj = Slot(spine);
			if (obj != null)
			{
				obj.Call(StringName.op_Implicit("set_color"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(value) });
			}
		}
	}

	private static void Apply(Node spine, State state)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (WearSettingsRuntime.Current.ShowUnderwear || !GodotObject.IsInstanceValid((GodotObject)(object)spine))
		{
			return;
		}
		string skeletonPath = SpineSwap.GetSkeletonPath(spine);
		if (SkinCatalog.IsOurPath(skeletonPath) && skeletonPath.EndsWith("/s2_ironclad.spskel", StringComparison.Ordinal))
		{
			GodotObject val = Slot(spine);
			if (val != null)
			{
				Variant val2 = val.Call(StringName.op_Implicit("get_color"), Array.Empty<Variant>());
				Color val3 = ((Variant)(ref val2)).AsColor();
				state.HiddenColor = val3;
				state.DataId = ((GodotObject)SpineSwap.GetSkeletonData(spine)).GetInstanceId();
				val3.A = 0f;
				val.Call(StringName.op_Implicit("set_color"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(val3) });
			}
		}
	}
}
internal static class WearCrossfade
{
	private sealed class Transition
	{
		internal required Node2D Overlay;

		internal required Tween Tween;
	}

	private static readonly Dictionary<ulong, Transition> Active = new Dictionary<ulong, Transition>();

	internal static bool Begin(Node node)
	{
		return BeginDeferred(node, () => true, delegate
		{
		});
	}

	internal static bool IsActive(Node node)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return Active.ContainsKey(((GodotObject)node).GetInstanceId());
		}
		return false;
	}

	internal static bool BeginDeferred(Node node, Func<bool> validate, Action commit)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Expected O, but got Unknown
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		Node2D body = (Node2D)(object)((node is Node2D) ? node : null);
		if (body == null || !GodotObject.IsInstanceValid((GodotObject)(object)body) || !((Node)body).IsInsideTree())
		{
			return false;
		}
		Node parent = ((Node)body).GetParent();
		Resource skeletonData = SpineSwap.GetSkeletonData((Node)(object)body);
		if (parent == null || skeletonData == null)
		{
			return false;
		}
		Cancel((Node)(object)body);
		Node2D overlay = null;
		try
		{
			Variant val = ClassDB.Instantiate(StringName.op_Implicit("SpineSprite"));
			Node2D ghost = default(Node2D);
			ref Node2D reference = ref ghost;
			GodotObject obj = ((Variant)(ref val)).AsGodotObject();
			reference = (Node2D)(object)((obj is Node2D) ? obj : null);
			if (ghost == null)
			{
				return false;
			}
			((Node)ghost).Name = StringName.op_Implicit("PreviousWear");
			((CanvasItem)ghost).Modulate = ((CanvasItem)body).Modulate;
			((CanvasItem)ghost).SelfModulate = ((CanvasItem)body).SelfModulate;
			((CanvasItem)ghost).TextureFilter = ((CanvasItem)body).TextureFilter;
			((CanvasItem)ghost).TextureRepeat = ((CanvasItem)body).TextureRepeat;
			((CanvasItem)ghost).Material = ((CanvasItem)body).Material;
			overlay = new Node2D
			{
				Name = StringName.op_Implicit("JoanWearCrossfade"),
				Transform = body.Transform,
				Visible = false,
				ZIndex = ((CanvasItem)body).ZIndex,
				ZAsRelative = ((CanvasItem)body).ZAsRelative,
				LightMask = ((CanvasItem)body).LightMask,
				VisibilityLayer = ((CanvasItem)body).VisibilityLayer
			};
			((Node)overlay).AddChild((Node)(object)ghost, false, (InternalMode)0);
			parent.AddChild((Node)(object)overlay, false, (InternalMode)0);
			parent.MoveChild((Node)(object)overlay, Math.Min(((Node)body).GetIndex(false) + 1, parent.GetChildCount(false) - 1));
			((GodotObject)ghost).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)skeletonData));
			UnderwearVisibility.Attach((Node)(object)ghost);
			Palette.ApplyNow((Node)(object)ghost);
			string text = SpineSwap.CurrentAnimation((Node)(object)body);
			if (!string.IsNullOrEmpty(text))
			{
				SpineSwap.Play((Node)(object)ghost, text, loop: true);
			}
			if (((GodotObject)ghost).HasMethod(StringName.op_Implicit("update_skeleton")))
			{
				((GodotObject)ghost).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
			}
			ulong id = ((GodotObject)body).GetInstanceId();
			Tween val2 = ((Node)overlay).CreateTween();
			Transition transition = new Transition
			{
				Overlay = overlay,
				Tween = val2
			};
			Active[id] = transition;
			((Node)overlay).TreeExiting += delegate
			{
				if (Active.TryGetValue(id, out Transition value) && value == transition)
				{
					Active.Remove(id);
				}
			};
			val2.TweenInterval(0.02);
			val2.TweenCallback(Callable.From((Action)delegate
			{
				//IL_008d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0092: Unknown result type (might be due to invalid IL or missing references)
				//IL_0097: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					if (!GodotObject.IsInstanceValid((GodotObject)(object)body) || !((Node)body).IsInsideTree() || !validate())
					{
						CancelById(id);
					}
					else
					{
						string text2 = SpineSwap.CurrentAnimation((Node)(object)body);
						if (!string.IsNullOrEmpty(text2))
						{
							SpineSwap.Play((Node)(object)ghost, text2, loop: true);
						}
						if (((GodotObject)ghost).HasMethod(StringName.op_Implicit("update_skeleton")))
						{
							((GodotObject)ghost).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
						}
						((CanvasItem)overlay).Visible = true;
						commit();
						if (((GodotObject)body).HasMethod(StringName.op_Implicit("update_skeleton")))
						{
							((GodotObject)body).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
						}
					}
				}
				catch (Exception ex)
				{
					ModEntry.TryInfo("wear crossfade commit: " + ex.Message);
					CancelById(id);
				}
			}));
			val2.TweenProperty((GodotObject)(object)overlay, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0.0), 0.25).SetTrans((TransitionType)1).SetEase((EaseType)2);
			val2.TweenCallback(Callable.From((Action)delegate
			{
				CancelById(id);
			}));
			return true;
		}
		catch (Exception ex2)
		{
			if (overlay != null && GodotObject.IsInstanceValid((GodotObject)(object)overlay))
			{
				((Node)overlay).QueueFree();
			}
			ModEntry.TryInfo("wear crossfade unavailable: " + ex2.Message);
			return false;
		}
	}

	internal static void Cancel(Node body)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)body))
		{
			CancelById(((GodotObject)body).GetInstanceId());
		}
	}

	private static void CancelById(ulong id)
	{
		if (Active.Remove(id, out Transition value))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)value.Tween))
			{
				value.Tween.Kill();
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)value.Overlay))
			{
				((Node)value.Overlay).QueueFree();
			}
		}
	}
}
public sealed class WearSettings
{
	public bool ShowUnderwear { get; set; } = true;

	public int LowHealthIdlePercent { get; set; } = 25;

	public bool WearEnabled { get; set; } = true;

	public bool UseAbsoluteHp { get; set; }

	public int PercentThreshold { get; set; } = 50;

	public int AbsoluteThreshold { get; set; } = 40;

	public bool ForceDeepestOnDeath { get; set; } = true;

	public string SafetyHotkey { get; set; } = "F6";

	public double VisualScale { get; set; } = 1.0;

	public int Appearance { get; set; } = 1;

	public string AppearanceHotkey { get; set; } = "=";

	public int ResolveStage(int hp, int maxHp)
	{
		if (maxHp <= 0)
		{
			return 0;
		}
		if (!WearEnabled)
		{
			return 1;
		}
		if (UseAbsoluteHp)
		{
			if (hp <= Math.Clamp(AbsoluteThreshold, 0, 999))
			{
				return 2;
			}
			return 1;
		}
		if ((long)hp * 100L <= (long)maxHp * (long)Math.Clamp(PercentThreshold, 0, 100))
		{
			return 2;
		}
		return 1;
	}

	public int ResolveDeathStage(int hp, int maxHp)
	{
		if (!WearEnabled)
		{
			return 1;
		}
		if (!ForceDeepestOnDeath)
		{
			return ResolveStage(hp, maxHp);
		}
		return 2;
	}

	public void Reset()
	{
		ShowUnderwear = true;
		LowHealthIdlePercent = 25;
		WearEnabled = true;
		UseAbsoluteHp = false;
		PercentThreshold = 50;
		AbsoluteThreshold = 40;
		ForceDeepestOnDeath = true;
		SafetyHotkey = "F6";
		VisualScale = 1.0;
		Appearance = 1;
		AppearanceHotkey = "=";
	}
}
public static class WearSettingsRuntime
{
	private static readonly WearSettings Defaults = new WearSettings();

	private static bool pressed;

	public static Func<WearSettings>? Provider { get; set; }

	public static Action<bool>? PersistEnabled { get; set; }

	public static Action<int>? PersistAppearance { get; set; }

	public static WearSettings Current
	{
		get
		{
			try
			{
				return Provider?.Invoke() ?? Defaults;
			}
			catch
			{
				return Defaults;
			}
		}
	}

	public static string PreviewSummary(WearSettings settings)
	{
		int value = Math.Clamp(settings.PercentThreshold, 0, 100);
		int value2 = Math.Clamp(settings.AbsoluteThreshold, 0, 999);
		string value3 = (settings.UseAbsoluteHp ? $"{value2} HP" : $"{value}%");
		return $"换装{(settings.WearEnabled ? "开启" : "关闭")}；分界 {value3}；高于分界第1档，等于或低于第2档。";
	}

	public static void LogPreview()
	{
		ModEntry.TryInfo("[wear-stage] " + PreviewSummary(Current));
	}

	internal static void TickHotkey()
	{
		UpdateHotkey(BindingMatches(Current.SafetyHotkey, (Key key) => Input.IsKeyPressed(key) || Input.IsPhysicalKeyPressed(key)));
	}

	internal static bool BindingMatches(string? binding, Func<Key, bool> isDown)
	{
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(binding))
		{
			return false;
		}
		bool flag = false;
		string[] array = binding.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		foreach (string text in array)
		{
			Key result;
			switch (text.ToLowerInvariant())
			{
			case "ctrl":
			case "control":
				result = (Key)4194326;
				break;
			case "alt":
				result = (Key)4194328;
				break;
			case "shift":
				result = (Key)4194325;
				break;
			case "meta":
			case "command":
			case "cmd":
			case "win":
			case "super":
				result = (Key)4194327;
				break;
			default:
				if (flag || !Enum.TryParse<Key>(text, ignoreCase: true, out result) || (int)result == 0)
				{
					return false;
				}
				flag = true;
				break;
			}
			if (!isDown(result))
			{
				return false;
			}
		}
		return flag;
	}

	internal static void UpdateHotkey(bool down)
	{
		WearSettings current = Current;
		if (down && !pressed)
		{
			bool flag = !current.WearEnabled;
			if (PersistEnabled != null)
			{
				PersistEnabled(flag);
			}
			else
			{
				current.WearEnabled = flag;
			}
		}
		pressed = down;
	}
}
