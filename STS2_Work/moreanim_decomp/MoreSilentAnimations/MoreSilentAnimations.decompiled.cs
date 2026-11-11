using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Actions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: InternalsVisibleTo("MoreSilentAnimations.NativeTests")]
[assembly: InternalsVisibleTo("MoreSilentAnimations.Settings")]
[assembly: InternalsVisibleTo("MoreSilentAnimations.BaseLib")]
[assembly: TargetFramework(".NETCoreApp,Version=v9.0", FrameworkDisplayName = ".NET 9.0")]
[assembly: AssemblyCompany("MoreSilentAnimations")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("0.1.35.0")]
[assembly: AssemblyInformationalVersion("0.1.35")]
[assembly: AssemblyProduct("MoreSilentAnimations")]
[assembly: AssemblyTitle("MoreSilentAnimations")]
[assembly: AssemblyVersion("0.1.35.0")]
[module: RefSafetyRules(11)]
namespace MoreSilentAnimations;

public sealed record AnimationSettings
{
	public bool Enabled { get; init; } = true;

	public bool ShowBulletTimeButton { get; init; } = true;

	public string[] DisabledCards { get; init; } = Array.Empty<string>();

	public static AnimationSettings Current { get; internal set; } = new AnimationSettings();

	internal static string FilePath => ProjectSettings.GlobalizePath("user://mod_settings/MoreSilentAnimations.json");

	internal const int BulletTimeDarkness = 200;

	internal const int BulletTimeDistortion = 250;

	internal static bool Allows(AbstractModel? card)
	{
		if (card != null)
		{
			return Allows(((object)card).GetType().Name);
		}
		return false;
	}

	internal static bool Allows(string card)
	{
		if (Current.Enabled)
		{
			return !Current.DisabledCards.Contains<string>(card, StringComparer.Ordinal);
		}
		return false;
	}

	internal static void Load()
	{
		try
		{
			if (File.Exists(FilePath))
			{
				AnimationSettings animationSettings = JsonSerializer.Deserialize<AnimationSettings>(File.ReadAllText(FilePath)) ?? new AnimationSettings();
				Current = animationSettings with
				{
					DisabledCards = (animationSettings.DisabledCards?.Where((string x) => !string.IsNullOrWhiteSpace(x)).Distinct<string>(StringComparer.Ordinal).ToArray() ?? Array.Empty<string>())
				};
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	internal static void Save(AnimationSettings value, bool resetVisuals = true)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
		string text = FilePath + ".tmp";
		File.WriteAllText(text, JsonSerializer.Serialize(value, new JsonSerializerOptions
		{
			WriteIndented = true
		}));
		File.Move(text, FilePath, overwrite: true);
		Current = value;
		if (resetVisuals)
		{
			VisualCleanup.Reset();
		}
		else
		{
			ComboPlanning.Ui?.Refresh();
		}
	}

	[CompilerGenerated]
	private AnimationSettings(AnimationSettings original)
	{
		Enabled = original.Enabled;
		ShowBulletTimeButton = original.ShowBulletTimeButton;
		DisabledCards = original.DisabledCards;
	}

	public AnimationSettings()
	{
	}
}
internal static class ComboAudio
{
	private static NAudioManager? _owner;

	private static float? _music;

	private static float? _ambience;

	private static bool _writing;

	internal static bool Muted { get; private set; }

	private static void Owner(NAudioManager manager)
	{
		if (_owner != manager)
		{
			_owner = manager;
			_music = null;
			_ambience = null;
		}
	}

	internal static void Requested(NAudioManager manager, bool music, ref float volume)
	{
		if (!_writing)
		{
			Owner(manager);
			if (music)
			{
				_music = volume;
			}
			else
			{
				_ambience = volume;
			}
			if (Muted)
			{
				volume = 0f;
			}
		}
	}

	internal static void Begin()
	{
		if (Muted)
		{
			return;
		}
		NAudioManager instance = NAudioManager.Instance;
		if (instance != null)
		{
			Owner(instance);
			float valueOrDefault = _music.GetValueOrDefault();
			if (!_music.HasValue)
			{
				valueOrDefault = SaveManager.Instance.SettingsSave.VolumeBgm;
				_music = valueOrDefault;
			}
			valueOrDefault = _ambience.GetValueOrDefault();
			if (!_ambience.HasValue)
			{
				valueOrDefault = SaveManager.Instance.SettingsSave.VolumeAmbience;
				_ambience = valueOrDefault;
			}
			Muted = true;
			Write(instance, 0f, 0f);
		}
	}

	internal static void Restore()
	{
		if (Muted)
		{
			Muted = false;
			NAudioManager owner = _owner;
			if (owner != null && GodotObject.IsInstanceValid((GodotObject)(object)owner))
			{
				Write(owner, _music ?? SaveManager.Instance.SettingsSave.VolumeBgm, _ambience ?? SaveManager.Instance.SettingsSave.VolumeAmbience);
			}
		}
	}

	private static void Write(NAudioManager manager, float music, float ambience)
	{
		_writing = true;
		try
		{
			VisualCleanup.Safely(delegate
			{
				manager.SetBgmVol(music);
			});
			VisualCleanup.Safely(delegate
			{
				manager.SetAmbienceVol(ambience);
			});
		}
		finally
		{
			_writing = false;
		}
	}
}
[HarmonyPatch(typeof(NAudioManager), "SetBgmVol")]
internal static class ComboMusicVolumePatch
{
	[HarmonyPrefix]
	private static void Prefix(NAudioManager __instance, ref float volume)
	{
		ComboAudio.Requested(__instance, music: true, ref volume);
	}
}
[HarmonyPatch(typeof(NAudioManager), "SetAmbienceVol")]
internal static class ComboAmbienceVolumePatch
{
	[HarmonyPrefix]
	private static void Prefix(NAudioManager __instance, ref float volume)
	{
		ComboAudio.Requested(__instance, music: false, ref volume);
	}
}
internal static class ComboChoiceContinuation
{
	private sealed record Intent(PlayCardAction? Original, CardModel? Card, Creature? Target, ComboShivPlan.Source? Source, ComboShivPlan.Knife? Knife);

	private static readonly List<Intent> Plan = new List<Intent>();

	private static Intent? _current;

	private static GameAction? _waiting;

	private static ActionQueueSet? _queues;

	private static bool _sending;

	private static int _next;

	internal static bool Playing { get; private set; }

	internal static bool HasPlan => Plan.Count > 0;

	internal static ComboShivPlan.Source? CurrentSource
	{
		get
		{
			if (Playing)
			{
				ComboShivPlan.Source source = _current?.Source;
				if (source != null && _waiting == RunManager.Instance.ActionExecutor.CurrentlyRunningAction)
				{
					return source;
				}
			}
			return null;
		}
	}

	internal static void Record(PlayCardAction action, ComboShivPlan.Source? source)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!ComboPlanning.Network && ComboPlanning.Active)
		{
			NetCombatCard netCombatCard = action.NetCombatCard;
			CardModel val = ((NetCombatCard)(ref netCombatCard)).ToCardModelOrNull();
			if (val != null)
			{
				Plan.Add(new Intent(action, val, action.Target, source, null));
			}
		}
	}

	internal static void Record(ComboShivPlan.Knife knife)
	{
		if (!ComboPlanning.Network && ComboPlanning.Active)
		{
			Plan.Add(new Intent(null, null, knife.Target, null, knife));
		}
	}

	internal static void BeforeRequest(GameAction action, bool shivSending)
	{
		if (!(Plan.Count == 0 || _sending || shivSending))
		{
			ulong ownerId = action.OwnerId;
			ComboPlanningUi? ui = ComboPlanning.Ui;
			if (ownerId == ((ui != null) ? new ulong?(ui.Player.NetId) : ((ulong?)null)) && ActionQueueSet.IsGameActionPlayerDriven(action) && (!ComboPlanning.Active || !(action is PlayCardAction)))
			{
				Clear();
			}
		}
	}

	internal static void PauseForChoice(GameAction action)
	{
		if (!ComboPlanning.Network && Plan.Count != 0 && !Playing)
		{
			int num = Plan.FindIndex((Intent item) => item.Original == action);
			if (num < 0)
			{
				Clear();
				return;
			}
			_current = Plan[num];
			_next = num + 1;
			_waiting = action;
			Playing = true;
			_queues = RunManager.Instance.ActionQueueSet;
			_queues.ActionEnqueued += OnEnqueued;
		}
	}

	private static bool Finished(GameAction action)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		GameActionState state = action.State;
		if (state - 5 <= 1)
		{
			return true;
		}
		return false;
	}

	private static void OnEnqueued(GameAction action)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!_sending)
		{
			return;
		}
		PlayCardAction val = (PlayCardAction)(object)((action is PlayCardAction) ? action : null);
		if (val == null)
		{
			return;
		}
		Intent current = _current;
		if ((object)current != null)
		{
			NetCombatCard netCombatCard = val.NetCombatCard;
			CardModel obj = ((NetCombatCard)(ref netCombatCard)).ToCardModelOrNull();
			object obj2 = current.Card;
			if (obj2 == null)
			{
				ComboShivPlan.Knife knife = current.Knife;
				obj2 = ((knife != null) ? ComboShivPlan.Resolve(knife) : null);
			}
			if (obj == obj2)
			{
				_waiting = (GameAction?)(object)val;
			}
		}
	}

	internal static void Tick()
	{
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Invalid comparison between Unknown and I4
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Invalid comparison between Unknown and I4
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Invalid comparison between Unknown and I4
		if (Plan.Count == 0)
		{
			return;
		}
		ComboPlanningUi ui = ComboPlanning.Ui;
		if (ui == null || !ui.Eligible || ComboPlanning.Network)
		{
			Clear();
			return;
		}
		if (!Playing)
		{
			if (!ComboPlanning.Active && !ComboShivPlan.Playing && Plan.All((Intent item) => item.Original == null || Finished((GameAction)(object)item.Original)))
			{
				Clear();
			}
			return;
		}
		ActionExecutor actionExecutor = RunManager.Instance.ActionExecutor;
		GameAction waiting = _waiting;
		if (waiting != null)
		{
			if (!Finished(waiting))
			{
				return;
			}
			ComboShivPlan.Source source = _current?.Source;
			if (source != null)
			{
				source.Done = true;
			}
			_waiting = null;
			_current = null;
		}
		if (actionExecutor.IsRunning || actionExecutor.IsPaused || ui.Native.Hand.InCardPlay || ui.Native.Hand.IsInCardSelection)
		{
			return;
		}
		while (_next < Plan.Count)
		{
			Intent intent = Plan[_next++];
			CardModel val = intent.Card;
			ComboShivPlan.Knife knife = intent.Knife;
			if (knife != null)
			{
				if (!knife.Source.Done)
				{
					Clear();
					return;
				}
				val = ComboShivPlan.Resolve(knife);
			}
			TargetType? val2 = ((val != null) ? new TargetType?(val.TargetType) : ((TargetType?)null));
			bool flag;
			if (val2.HasValue)
			{
				TargetType valueOrDefault = val2.GetValueOrDefault();
				if ((int)valueOrDefault == 2 || (int)valueOrDefault == 6)
				{
					flag = true;
					goto IL_017c;
				}
			}
			flag = false;
			goto IL_017c;
			IL_017c:
			Creature val3 = (flag ? intent.Target : null);
			if (val != null)
			{
				CardPile pile = val.Pile;
				if (pile != null && (int)pile.Type == 2 && val.CanPlayTargeting(val3))
				{
					ComboShivPlan.Source source2 = intent.Source;
					if (source2 != null)
					{
						source2.Generated.Clear();
						source2.FinalGenerated = null;
						source2.Done = false;
					}
					_current = intent;
					_sending = true;
					bool flag2;
					try
					{
						flag2 = val.TryManualPlay(val3);
					}
					finally
					{
						_sending = false;
					}
					if (flag2 && _waiting != null)
					{
						return;
					}
					ComboShivPlan.Source source3 = intent.Source;
					if (source3 != null)
					{
						source3.Done = true;
					}
					_current = null;
					continue;
				}
			}
			ComboShivPlan.Source source4 = intent.Source;
			if (source4 != null)
			{
				source4.Generated.Clear();
				source4.Done = true;
			}
		}
		Clear();
	}

	internal static void Clear()
	{
		ClearIntents();
		ComboDiscardPlan.Clear();
	}

	private static void ClearIntents()
	{
		ActionQueueSet queues = _queues;
		if (queues != null)
		{
			queues.ActionEnqueued -= OnEnqueued;
		}
		_queues = null;
		Plan.Clear();
		_current = null;
		_waiting = null;
		_next = 0;
		Playing = false;
	}
}
internal static class ComboDiscardOverlay
{
	private static readonly ShaderMaterial Material = new ShaderMaterial
	{
		Shader = new Shader
		{
			Code = "shader_type canvas_item;\ninstance uniform vec2 card_size = vec2(300.0, 422.0);\nvarying vec2 card_pixel;\nvarying vec4 card_tint;\nvoid vertex() { card_pixel = VERTEX; card_tint = COLOR; }\nfloat line_distance(vec2 p, vec2 a, vec2 b) {\n    vec2 d = b-a;\n    return length(p-a-d*clamp(dot(p-a,d)/dot(d,d),0.0,1.0));\n}\nvoid fragment() {\n    vec2 p = card_pixel / card_size;\n    // The original frame contains a portrait cutout; fill only its\n    // safe interior while preserving the exact outer alpha contour.\n    float interior = step(0.08,p.x)*step(p.x,0.92)*step(0.08,p.y)*step(p.y,0.92);\n    float mask = max(texture(TEXTURE,UV).a,interior);\n    float band = mod(card_pixel.x+card_pixel.y,23.0);\n    float stripes = smoothstep(0.0,1.2,band)*(1.0-smoothstep(4.8,6.0,band));\n    float distance = min(line_distance(card_pixel,vec2(0.0),card_size),\n        line_distance(card_pixel,vec2(0.0,card_size.y),vec2(card_size.x,0.0)));\n    float cross = 1.0-smoothstep(4.0,5.5,distance);\n    COLOR=vec4(vec3(0.88,0.42,0.40),max(stripes*0.5,cross*0.85)*mask*card_tint.a);\n}"
		}
	};

	internal static TextureRect Create(NCard card)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		TextureRect node = ((Node)card).GetNode<TextureRect>(NodePath.op_Implicit("%Frame"));
		if (!((CanvasItem)node).Visible)
		{
			node = ((Node)card).GetNode<TextureRect>(NodePath.op_Implicit("%AncientBorder"));
		}
		TextureRect val = new TextureRect
		{
			Name = StringName.op_Implicit("MoreSilentDiscardCross"),
			ExpandMode = node.ExpandMode,
			StretchMode = node.StretchMode,
			Texture = node.Texture,
			Material = (Material)(object)Material,
			Position = ((Control)node).Position,
			Size = ((Control)node).Size,
			Scale = ((Control)node).Scale,
			Rotation = ((Control)node).Rotation,
			PivotOffset = ((Control)node).PivotOffset,
			MouseFilter = (MouseFilterEnum)2
		};
		((CanvasItem)val).SetInstanceShaderParameter(StringName.op_Implicit("card_size"), Variant.op_Implicit(((Control)node).Size));
		((Node)node).GetParent().AddChild((Node)(object)val, false, (InternalMode)0);
		return val;
	}
}
internal static class ComboDiscardPlan
{
	internal sealed record Pick(CardModel Card, ComboShivPlan.Knife? Knife)
	{
		internal CardModel? Resolve()
		{
			ComboShivPlan.Knife knife = Knife;
			if (knife == null)
			{
				return Card;
			}
			return ComboShivPlan.Resolve(knife);
		}
	}

	internal sealed class Choice(CardModel source, int ordinal, int play, int count)
	{
		internal readonly CardModel Source = source;

		internal readonly int Ordinal = ordinal;

		internal readonly int Play = play;

		internal readonly int Count = count;

		internal readonly List<Pick> Picks = new List<Pick>();

		internal bool Deferred;

		internal bool Used;
	}

	internal sealed record NativeScope(CardModel? Source, int Play, NativeScope? Previous);

	private sealed record Mark(NCard Node, CardModel Card, Color Color, Node Cross, CanvasItem? ShivStripes, bool StripesVisible)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Node = ");
			builder.Append(Node);
			builder.Append(", Card = ");
			builder.Append(Card);
			builder.Append(", Color = ");
			Color color = Color;
			builder.Append(((object)(Color)(ref color)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Cross = ");
			builder.Append(Cross);
			builder.Append(", ShivStripes = ");
			builder.Append(ShivStripes);
			builder.Append(", StripesVisible = ");
			builder.Append(StripesVisible.ToString());
			return true;
		}
	}

	private static readonly List<Choice> Choices = new List<Choice>();

	private static readonly List<(CardModel Card, int Ordinal)> WholeHand = new List<(CardModel, int)>();

	private static readonly List<Mark> Marks = new List<Mark>();

	private static readonly AsyncLocal<NativeScope?> Native = new AsyncLocal<NativeScope>();

	internal static Choice? Editing { get; private set; }

	internal static IReadOnlyList<Choice> Entries => Choices;

	internal static bool HasEntries => Choices.Count > 0;

	internal static int Applied { get; private set; }

	internal static int Fallbacks { get; private set; }

	internal static bool Picking
	{
		get
		{
			if (ComboPlanning.Active && !ComboPlanning.Network)
			{
				return Editing != null;
			}
			return false;
		}
	}

	internal static bool Reserved(CardModel card)
	{
		if (!Choices.Any((Choice c) => c.Picks.Any((Pick p) => p.Card == card)))
		{
			return WholeHand.Any<(CardModel, int)>(((CardModel Card, int Ordinal) p) => p.Card == card);
		}
		return true;
	}

	internal static bool ReservedBefore(CardModel card, int ordinal)
	{
		if (!Choices.Any((Choice c) => c.ordinal < ordinal && c.Picks.Any((Pick p) => p.Card == card)))
		{
			return WholeHand.Any<(CardModel, int)>(((CardModel Card, int Ordinal) p) => p.Ordinal < ordinal && p.Card == card);
		}
		return true;
	}

	internal static void Begin()
	{
		Clear();
		Applied = 0;
		Fallbacks = 0;
	}

	internal static void Record(CardModel card, int ordinal, int repeats)
	{
		if (ComboPlanning.Network)
		{
			return;
		}
		int num = ((card is Survivor || card is Acrobatics || card is DaggerThrow) ? 1 : ((card is Prepared || card is HiddenDaggers) ? ((DynamicVar)card.DynamicVars.Cards).IntValue : 0));
		int num2 = num;
		if (num2 > 0)
		{
			for (int i = 0; i < repeats; i++)
			{
				if (Choices.Count >= 60)
				{
					break;
				}
				Choices.Add(new Choice(card, ordinal, i, num2));
			}
		}
		bool flag;
		switch (((object)card).GetType().Name)
		{
		case "CalculatedGamble":
		case "StormOfSteel":
		case "ShadowStep":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			foreach (CardModel card2 in PileTypeExtensions.GetPile((PileType)2, card.Owner).Cards)
			{
				if (card2 != card && ComboForecast.Index(card2) < 0 && !Reserved(card2))
				{
					WholeHand.Add((card2, ordinal));
				}
			}
			foreach (ComboShivPlan.Knife preview in ComboShivPlan.Previews)
			{
				if (!preview.Selected && !Reserved(preview.Preview))
				{
					WholeHand.Add((preview.Preview, ordinal));
				}
			}
		}
		Editing = Choices.FirstOrDefault((Choice c) => !c.Deferred && c.Picks.Count < c.count);
		Refresh();
	}

	internal static bool CanChoose(CardModel card, Choice choice)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Invalid comparison between Unknown and I4
		if (card.Owner != choice.source.Owner || card == choice.source || Reserved(card))
		{
			return false;
		}
		if (ComboForecast.Index(card) >= 0)
		{
			return false;
		}
		ComboShivPlan.Knife knife = ComboShivPlan.Find(card);
		if (knife != null)
		{
			if (!knife.Selected)
			{
				return ComboForecast.Index(knife.Source.Card) < choice.ordinal;
			}
			return false;
		}
		CardPile pile = card.Pile;
		if (pile == null)
		{
			return false;
		}
		return (int)pile.Type == 2;
	}

	internal static bool Choose(CardModel card)
	{
		if (Picking)
		{
			Choice editing = Editing;
			if (editing != null && CanChoose(card, editing))
			{
				editing.Picks.Add(new Pick(card, ComboShivPlan.Find(card)));
				if (editing.Picks.Count == editing.count)
				{
					Editing = Choices.FirstOrDefault((Choice c) => !c.Deferred && c.Picks.Count < c.count);
				}
				Refresh();
				return true;
			}
		}
		return false;
	}

	internal static void Defer()
	{
		ComboDiscardSelection.Cancel();
		Choice editing = Editing;
		if (editing != null)
		{
			editing.Picks.Clear();
			editing.Deferred = true;
		}
		Editing = Choices.FirstOrDefault((Choice c) => !c.Deferred && c.Picks.Count < c.count);
		Refresh();
	}

	internal static void Edit(Choice choice)
	{
		if (ComboPlanning.Active && !ComboPlanning.Network && Choices.Contains(choice))
		{
			ComboDiscardSelection.Cancel();
			choice.Picks.Clear();
			choice.Deferred = false;
			Editing = choice;
			Refresh();
		}
	}

	internal static bool HandleHand(CardModel? card)
	{
		if (!ComboPlanning.Active || ComboPlanning.Network || card == null)
		{
			return false;
		}
		if (Picking)
		{
			return true;
		}
		Choice choice = Choices.FirstOrDefault((Choice c) => c.Picks.Any((Pick p) => p.Card == card));
		if (choice != null)
		{
			Edit(choice);
			return true;
		}
		return Reserved(card);
	}

	internal static void Refresh()
	{
		RefreshMarks();
	}

	internal static void RefreshMarks()
	{
		RestoreMarks();
		if (!ComboPlanning.Active)
		{
			return;
		}
		NPlayerHand instance = NPlayerHand.Instance;
		if (instance == null)
		{
			return;
		}
		foreach (NHandCardHolder item in ((IEnumerable)((Node)instance.CardHolderContainer).GetChildren(false)).OfType<NHandCardHolder>())
		{
			NCard cardNode = ((NCardHolder)item).CardNode;
			if (cardNode != null)
			{
				CardModel cardModel = ((NCardHolder)item).CardModel;
				if (cardModel != null)
				{
					Decorate(cardNode, cardModel);
				}
			}
		}
		foreach (CardModel item2 in ComboDiscardSelection.Selected)
		{
			NCardHolder cardHolder = instance.GetCardHolder(item2);
			NCard val = ((cardHolder != null) ? cardHolder.CardNode : null);
			if (val != null)
			{
				Decorate(val, item2);
			}
		}
	}

	internal static void Decorate(NCard node, CardModel card)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (ComboPlanning.Active && (Reserved(card) || ComboDiscardSelection.Selected.Contains(card)) && !Marks.Any((Mark m) => m.Node == node))
		{
			TextureRect cross = ComboDiscardOverlay.Create(node);
			CanvasItem nodeOrNull = ((Node)node).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("PendingStripes"));
			bool stripesVisible = nodeOrNull != null && nodeOrNull.Visible;
			if (nodeOrNull != null)
			{
				nodeOrNull.Hide();
			}
			Color modulate = ((CanvasItem)node).Modulate;
			((CanvasItem)node).Modulate = new Color(modulate.R, modulate.G, modulate.B, Math.Min(modulate.A, 0.55f));
			Marks.Add(new Mark(node, card, modulate, (Node)(object)cross, nodeOrNull, stripesVisible));
		}
	}

	private static void RestoreMarks()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		foreach (Mark mark in Marks)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)mark.Node) && mark.Node.Model == mark.Card)
			{
				((CanvasItem)mark.Node).Modulate = mark.Color;
			}
			CanvasItem shivStripes = mark.ShivStripes;
			if (shivStripes != null && GodotObject.IsInstanceValid((GodotObject)(object)shivStripes))
			{
				shivStripes.Visible = mark.StripesVisible;
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)mark.Cross) && !((GodotObject)mark.Cross).IsQueuedForDeletion())
			{
				Node cross = mark.Cross;
				Node obj = ((cross is CanvasItem) ? cross : null);
				if (obj != null)
				{
					((CanvasItem)obj).Hide();
				}
				Node parent = mark.Cross.GetParent();
				if (parent != null)
				{
					parent.RemoveChild(mark.Cross);
				}
				mark.Cross.QueueFree();
			}
		}
		Marks.Clear();
	}

	internal static void Release()
	{
		ComboDiscardSelection.Cancel();
		foreach (Choice item in Choices.Where((Choice c) => c.Picks.Count != c.count))
		{
			item.Picks.Clear();
			item.Deferred = true;
		}
		Editing = null;
		RestoreMarks();
	}

	internal static void Clear()
	{
		Release();
		Choices.Clear();
		WholeHand.Clear();
	}

	internal static NativeScope PushNative(AbstractModel source)
	{
		AbstractModel obj = ((source is CardModel) ? source : null);
		NativeScope nativeScope = new NativeScope((CardModel?)(object)obj, (obj != null) ? ((CardModel)obj).CurrentPlayIndex : 0, Native.Value);
		Native.Value = nativeScope;
		return nativeScope;
	}

	internal static void PopNative(NativeScope scope)
	{
		Native.Value = scope.Previous;
	}

	internal static bool TryNative(CardSelectorPrefs prefs, Func<CardModel, bool>? filter, AbstractModel? source, out IEnumerable<CardModel> selected)
	{
		selected = Array.Empty<CardModel>();
		if (!ComboPlanning.Network && !ComboPlanning.Active)
		{
			NativeScope scope = Native.Value;
			if ((object)scope != null)
			{
				CardModel card = scope.Source;
				if (card != null && source == card)
				{
					Choice choice = Choices.FirstOrDefault((Choice c) => c.source == card && c.play == scope.Play && !c.Used);
					if (choice == null)
					{
						return false;
					}
					choice.Used = true;
					if (choice.Deferred || choice.Picks.Count != choice.count)
					{
						Fallbacks++;
						return false;
					}
					CardModel[] array = choice.Picks.Select((Pick p) => p.Resolve()).ToArray();
					if (array.Any(delegate(CardModel c)
					{
						//IL_0023: Unknown result type (might be due to invalid IL or missing references)
						//IL_0029: Invalid comparison between Unknown and I4
						if (c != null && c.Owner == card.Owner)
						{
							CardPile pile = c.Pile;
							if (pile != null && (int)pile.Type == 2)
							{
								return !(filter?.Invoke(c) ?? true);
							}
						}
						return true;
					}) || array.Distinct().Count() != array.Length || array.Length < ((CardSelectorPrefs)(ref prefs)).MinSelect || array.Length > ((CardSelectorPrefs)(ref prefs)).MaxSelect)
					{
						Fallbacks++;
						return false;
					}
					selected = array.Select((CardModel c) => c).ToArray();
					Applied++;
					return true;
				}
			}
		}
		return false;
	}
}
[HarmonyPatch(typeof(CardSelectCmd), "FromHandForDiscard")]
internal static class ComboDiscardScopePatch
{
	[HarmonyPrefix]
	private static void Prefix(AbstractModel source, out ComboDiscardPlan.NativeScope? __state)
	{
		__state = (ComboDiscardPlan.HasEntries ? ComboDiscardPlan.PushNative(source) : null);
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, ComboDiscardPlan.NativeScope? __state)
	{
		if ((object)__state != null)
		{
			ComboDiscardPlan.PopNative(__state);
		}
		return __exception;
	}
}
[HarmonyPatch(typeof(NPlayerHand), "SelectCards")]
internal static class ComboDiscardSubmitPatch
{
	[HarmonyPrefix]
	private static bool Prefix(CardSelectorPrefs prefs, Func<CardModel, bool>? filter, AbstractModel? source, ref Task<IEnumerable<CardModel>> __result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!ComboDiscardPlan.TryNative(prefs, filter, source, out IEnumerable<CardModel> selected))
		{
			return true;
		}
		__result = Task.FromResult(selected);
		return false;
	}
}
internal static class ComboDiscardSelection
{
	private sealed class Session(ComboDiscardPlan.Choice choice, NPlayerHand hand)
	{
		internal readonly ComboDiscardPlan.Choice Choice = choice;

		internal readonly NPlayerHand Hand = hand;

		internal readonly NConfirmButton Confirm = (NConfirmButton)ConfirmButton.GetValue(hand);

		internal TaskCompletionSource<IEnumerable<CardModel>>? Completion;

		internal bool Cancelled;
	}

	private static Session? _session;

	private static readonly FieldInfo Completion = AccessTools.Field(typeof(NPlayerHand), "_selectionCompletionSource");

	private static readonly FieldInfo SelectedCards = AccessTools.Field(typeof(NPlayerHand), "_selectedCards");

	private static readonly FieldInfo ConfirmButton = AccessTools.Field(typeof(NPlayerHand), "_selectModeConfirmButton");

	internal static bool Running => _session != null;

	internal static IEnumerable<CardModel> Selected
	{
		get
		{
			Session session = _session;
			if (session == null || !GodotObject.IsInstanceValid((GodotObject)(object)session.hand))
			{
				return Array.Empty<CardModel>();
			}
			return (IEnumerable<CardModel>)SelectedCards.GetValue(session.hand);
		}
	}

	internal static bool Owns(Node node)
	{
		Session session = _session;
		if (session != null)
		{
			if (node != session.hand && node != session.Confirm)
			{
				NSelectedHandCardHolder val = (NSelectedHandCardHolder)(object)((node is NSelectedHandCardHolder) ? node : null);
				if (val != null)
				{
					CardModel cardModel = ((NCardHolder)val).CardModel;
					if (((cardModel != null) ? cardModel.Owner : null) == session.choice.source.Owner)
					{
						goto IL_007a;
					}
				}
				NCard val2 = (NCard)(object)((node is NCard) ? node : null);
				if (val2 != null)
				{
					CardModel model = val2.Model;
					return ((model != null) ? model.Owner : null) == session.choice.source.Owner;
				}
				return false;
			}
			goto IL_007a;
		}
		return false;
		IL_007a:
		return true;
	}

	internal static void Tick()
	{
		if (_session != null || !ComboDiscardPlan.Picking)
		{
			return;
		}
		ComboDiscardPlan.Choice choice = ComboDiscardPlan.Editing;
		if (choice == null)
		{
			return;
		}
		NPlayerHand instance = NPlayerHand.Instance;
		if (instance != null && !instance.InCardPlay && !instance.IsInCardSelection)
		{
			if (((IEnumerable)((Node)instance.CardHolderContainer).GetChildren(false)).OfType<NHandCardHolder>().Count(delegate(NHandCardHolder h)
			{
				CardModel cardModel = ((NCardHolder)h).CardModel;
				return cardModel != null && ComboDiscardPlan.CanChoose(cardModel, choice);
			}) < choice.count)
			{
				ComboDiscardPlan.Defer();
			}
			else
			{
				Select(_session = new Session(choice, instance));
			}
		}
	}

	private static async Task Select(Session session)
	{
		try
		{
			CardSelectorPrefs val = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, session.choice.count);
			UnplayableReason val2 = default(UnplayableReason);
			AbstractModel val3 = default(AbstractModel);
			((CardSelectorPrefs)(ref val)).ShouldGlowGold = (CardModel card) => card.IsSlyThisTurn && (card.CanPlay(ref val2, ref val3) || (val2 & 0x30) > 0);
			CardSelectorPrefs val4 = val;
			Task<IEnumerable<CardModel>> task = session.hand.SelectCards(val4, (Func<CardModel, bool>)((CardModel c) => ComboDiscardPlan.CanChoose(c, session.choice)), (AbstractModel)null, (Mode)2);
			session.Completion = (TaskCompletionSource<IEnumerable<CardModel>>)Completion.GetValue(session.hand);
			CardModel[] array = (await task).ToArray();
			ComboNativeHand.RebindPreviews();
			if (session.Cancelled || _session != session || !ComboPlanning.Active || ComboDiscardPlan.Editing != session.choice)
			{
				return;
			}
			_session = null;
			if (array.Length != session.choice.count || array.Any((CardModel c) => !ComboDiscardPlan.CanChoose(c, session.choice)))
			{
				ComboDiscardPlan.Defer();
				return;
			}
			CardModel[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				ComboDiscardPlan.Choose(array2[i]);
			}
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			if (ComboDiscardPlan.Editing == session.choice)
			{
				ComboDiscardPlan.Defer();
			}
		}
		finally
		{
			if (_session == session)
			{
				_session = null;
			}
			ComboNativeHand.RebindPreviews();
		}
	}

	internal static void Cancel()
	{
		Session session = _session;
		if (session == null)
		{
			return;
		}
		session.Cancelled = true;
		if (GodotObject.IsInstanceValid((GodotObject)(object)session.hand) && session.hand.IsInCardSelection)
		{
			TaskCompletionSource<IEnumerable<CardModel>> completion = session.Completion;
			if (completion != null && Completion.GetValue(session.hand) == completion)
			{
				completion.TrySetResult(Array.Empty<CardModel>());
			}
		}
		if (_session == session)
		{
			_session = null;
		}
		ComboNativeHand.RebindPreviews();
	}
}
[HarmonyPatch]
internal static class ComboDiscardSelectionFeedback
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return new string[2] { "SelectCardInSimpleMode", "DeselectCard" }.Select((string name) => AccessTools.Method(typeof(NPlayerHand), name, (Type[])null, (Type[])null));
	}

	[HarmonyPostfix]
	private static void Postfix(NPlayerHand __instance)
	{
		if (ComboDiscardSelection.Owns((Node)(object)__instance))
		{
			ComboNativeHand.RebindPreviews();
			ComboDiscardPlan.RefreshMarks();
		}
	}
}
[HarmonyPatch]
internal static class ComboDiscardSelectionTiming
{
	[CompilerGenerated]
	private sealed class <TargetMethods>d__0 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private MethodBase <>2__current;

		private int <>l__initialThreadId;

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
		public <TargetMethods>d__0(int <>1__state)
		{
			this.<>1__state = <>1__state;
			<>l__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
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
				<>2__current = AccessTools.Method(AccessTools.Method(typeof(NPlayerHand), "SelectCards", (Type[])null, (Type[])null).GetCustomAttribute<AsyncStateMachineAttribute>().StateMachineType, "MoveNext", (Type[])null, (Type[])null);
				<>1__state = 1;
				return true;
			case 1:
				<>1__state = -1;
				<>2__current = AccessTools.Method(typeof(NPlayerHand), "AfterCardsSelected", (Type[])null, (Type[])null);
				<>1__state = 2;
				return true;
			case 2:
				<>1__state = -1;
				<>2__current = AccessTools.Method(typeof(NPlayerHand), "UpdateSelectedCardContainer", (Type[])null, (Type[])null);
				<>1__state = 3;
				return true;
			case 3:
				<>1__state = -1;
				<>2__current = AccessTools.Method(typeof(NSelectedHandCardHolder), "_Ready", (Type[])null, (Type[])null);
				<>1__state = 4;
				return true;
			case 4:
				<>1__state = -1;
				<>2__current = AccessTools.Method(typeof(NHandCardHolder), "SetCard", (Type[])null, (Type[])null);
				<>1__state = 5;
				return true;
			case 5:
				<>1__state = -1;
				<>2__current = AccessTools.Method(typeof(NConfirmButton), "OnEnable", (Type[])null, (Type[])null);
				<>1__state = 6;
				return true;
			case 6:
				<>1__state = -1;
				<>2__current = AccessTools.Method(typeof(NConfirmButton), "OnDisable", (Type[])null, (Type[])null);
				<>1__state = 7;
				return true;
			case 7:
				<>1__state = -1;
				return false;
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
		IEnumerator<MethodBase> IEnumerable<MethodBase>.GetEnumerator()
		{
			if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
			{
				<>1__state = 0;
				return this;
			}
			return new <TargetMethods>d__0(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class <Transpiler>d__1 : IEnumerable<CodeInstruction>, IEnumerable, IEnumerator<CodeInstruction>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private CodeInstruction <>2__current;

		private int <>l__initialThreadId;

		private IEnumerable<CodeInstruction> instructions;

		public IEnumerable<CodeInstruction> <>3__instructions;

		private MethodInfo <create>5__2;

		private int <found>5__3;

		private IEnumerator<CodeInstruction> <>7__wrap3;

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
			<create>5__2 = null;
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
					<create>5__2 = AccessTools.Method(typeof(Node), "CreateTween", (Type[])null, (Type[])null);
					<found>5__3 = 0;
					<>7__wrap3 = instructions.GetEnumerator();
					<>1__state = -3;
					break;
				case 1:
					<>1__state = -3;
					break;
				}
				if (<>7__wrap3.MoveNext())
				{
					CodeInstruction current = <>7__wrap3.Current;
					if (CodeInstructionExtensions.Calls(current, <create>5__2))
					{
						current.opcode = OpCodes.Call;
						current.operand = AccessTools.Method(typeof(ComboDiscardSelectionTiming), "CreateTween", (Type[])null, (Type[])null);
						<found>5__3++;
					}
					<>2__current = current;
					<>1__state = 1;
					return true;
				}
				<>m__Finally1();
				<>7__wrap3 = null;
				if (<found>5__3 == 0)
				{
					throw new InvalidOperationException("Native selection tween layout changed");
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

	[IteratorStateMachine(typeof(<TargetMethods>d__0))]
	private static IEnumerable<MethodBase> TargetMethods()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <TargetMethods>d__0(-2);
	}

	[IteratorStateMachine(typeof(<Transpiler>d__1))]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <Transpiler>d__1(-2)
		{
			<>3__instructions = instructions
		};
	}

	private static Tween CreateTween(Node node)
	{
		Tween val = node.CreateTween();
		if (ComboDiscardSelection.Owns(node))
		{
			val.SetIgnoreTimeScale(true);
		}
		return val;
	}
}
internal static class ComboEndTurn
{
	private static EndPlayerTurnAction? _action;

	private static ActionQueueSet? _queues;

	private static int _turn;

	internal static bool Pending => _action != null;

	internal static bool Hold(GameAction action)
	{
		EndPlayerTurnAction val = (EndPlayerTurnAction)(object)((action is EndPlayerTurnAction) ? action : null);
		if (val != null && !ComboPlanning.Network)
		{
			ComboPlanningUi ui = ComboPlanning.Ui;
			if (ui != null && action.OwnerId == ui.Player.NetId)
			{
				if (Pending)
				{
					return true;
				}
				if (!ComboChoiceContinuation.HasPlan && !ComboShivPlan.Playing)
				{
					return false;
				}
				_action = val;
				_queues = RunManager.Instance.ActionQueueSet;
				_turn = ui.Player.PlayerCombatState.TurnNumber;
				ComboPlanning.Release();
				return true;
			}
		}
		return false;
	}

	internal static void Tick()
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		EndPlayerTurnAction action = _action;
		if (action == null)
		{
			return;
		}
		if (!ComboPlanning.Network)
		{
			ComboPlanningUi ui = ComboPlanning.Ui;
			if (ui != null && ui.Eligible && _queues == RunManager.Instance.ActionQueueSet)
			{
				PlayerCombatState playerCombatState = ui.Player.PlayerCombatState;
				if (((playerCombatState != null) ? new int?(playerCombatState.TurnNumber) : ((int?)null)) == _turn)
				{
					ActionExecutor actionExecutor = RunManager.Instance.ActionExecutor;
					if (!ComboPlanning.Active && !ComboChoiceContinuation.HasPlan && !ComboShivPlan.Playing && !actionExecutor.IsRunning && !actionExecutor.IsPaused && _queues.IsEmpty && !ui.Native.Hand.InCardPlay && !ui.Native.Hand.IsInCardSelection)
					{
						Clear();
						if ((int)((GameAction)action).State == 0)
						{
							RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue((GameAction)(object)action);
						}
					}
					return;
				}
			}
		}
		Clear();
	}

	internal static void Clear()
	{
		_action = null;
		_queues = null;
		_turn = 0;
	}
}
[HarmonyPatch]
internal static class ComboExitPatch
{
	[CompilerGenerated]
	private sealed class <TargetMethods>d__0 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private MethodBase <>2__current;

		private int <>l__initialThreadId;

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
		public <TargetMethods>d__0(int <>1__state)
		{
			this.<>1__state = <>1__state;
			<>l__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
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
				<>2__current = AccessTools.Method(typeof(NGame), "ReturnToMainMenu", (Type[])null, (Type[])null);
				<>1__state = 1;
				return true;
			case 1:
				<>1__state = -1;
				<>2__current = AccessTools.Method(typeof(NGame), "GoToTimeline", (Type[])null, (Type[])null);
				<>1__state = 2;
				return true;
			case 2:
				<>1__state = -1;
				return false;
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
		IEnumerator<MethodBase> IEnumerable<MethodBase>.GetEnumerator()
		{
			if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
			{
				<>1__state = 0;
				return this;
			}
			return new <TargetMethods>d__0(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	[IteratorStateMachine(typeof(<TargetMethods>d__0))]
	private static IEnumerable<MethodBase> TargetMethods()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <TargetMethods>d__0(-2);
	}

	[HarmonyPrefix]
	private static void Prefix()
	{
		VisualCleanup.Safely(ComboPlanning.PrepareForExit);
	}
}
internal static class ComboForecast
{
	private static readonly List<CardModel> Order = new List<CardModel>();

	internal static void Clear()
	{
		Order.Clear();
	}

	internal static int Index(CardModel card)
	{
		return Order.IndexOf(card);
	}

	internal static int Record(CardModel card)
	{
		int count = Order.Count;
		int num = CombatManager.Instance.History.CardPlaysStarted.Count((CardPlayStartedEntry e) => ((CombatHistoryEntry)e).Actor == card.Owner.Creature && e.CardPlay.IsFirstInSeries && ((CombatHistoryEntry)e).HappenedThisTurn(card.CombatState));
		int powerAmount = card.Owner.Creature.GetPowerAmount<EchoFormPower>();
		int num2 = Math.Clamp(1 + Math.Max(0, card.GetEnchantedReplayCount()) + ((num + count < powerAmount) ? 1 : 0), 1, 60);
		Order.Add(card);
		if (!ComboPlanning.Network)
		{
			ComboDiscardPlan.Record(card, count, num2);
		}
		return num2;
	}

	internal static int KnownHandBefore(CardModel source)
	{
		int at = Index(source);
		return PileTypeExtensions.GetPile((PileType)2, source.Owner).Cards.Count((CardModel c) => c != source && (Index(c) < 0 || Index(c) >= at) && !ComboDiscardPlan.ReservedBefore(c, at)) + ComboShivPlan.Previews.Count((ComboShivPlan.Knife k) => !k.Selected && Index(k.Source.Card) < at && !ComboDiscardPlan.ReservedBefore(k.Preview, at));
	}
}
[HarmonyPatch]
internal static class ComboHandTiming
{
	[CompilerGenerated]
	private sealed class <Transpiler>d__2 : IEnumerable<CodeInstruction>, IEnumerable, IEnumerator<CodeInstruction>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private CodeInstruction <>2__current;

		private int <>l__initialThreadId;

		private IEnumerable<CodeInstruction> instructions;

		public IEnumerable<CodeInstruction> <>3__instructions;

		private int <found>5__2;

		private IEnumerator<CodeInstruction> <>7__wrap2;

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
		public <Transpiler>d__2(int <>1__state)
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
			<>7__wrap2 = null;
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
					<found>5__2 = 0;
					<>7__wrap2 = instructions.GetEnumerator();
					<>1__state = -3;
					break;
				case 1:
					<>1__state = -3;
					break;
				}
				if (<>7__wrap2.MoveNext())
				{
					CodeInstruction current = <>7__wrap2.Current;
					if (CodeInstructionExtensions.Calls(current, Delta))
					{
						current.opcode = OpCodes.Call;
						current.operand = AccessTools.Method(typeof(ComboHandTiming), "HandDelta", (Type[])null, (Type[])null);
						<found>5__2++;
					}
					<>2__current = current;
					<>1__state = 1;
					return true;
				}
				<>m__Finally1();
				<>7__wrap2 = null;
				if (<found>5__2 != 1)
				{
					throw new InvalidOperationException("Native hand clock layout changed: " + <found>5__2);
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
			if (<>7__wrap2 != null)
			{
				<>7__wrap2.Dispose();
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
			<Transpiler>d__2 <Transpiler>d__;
			if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
			{
				<>1__state = 0;
				<Transpiler>d__ = this;
			}
			else
			{
				<Transpiler>d__ = new <Transpiler>d__2(0);
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

	private static readonly MethodInfo Delta = AccessTools.Method(typeof(Node), "GetProcessDeltaTime", (Type[])null, (Type[])null);

	private static IEnumerable<MethodBase> TargetMethods()
	{
		return new string[3] { "AnimPosition", "AnimAngle", "AnimScale" }.Select((string name) => AccessTools.Method(AccessTools.Method(typeof(NHandCardHolder), name, (Type[])null, (Type[])null).GetCustomAttribute<AsyncStateMachineAttribute>().StateMachineType, "MoveNext", (Type[])null, (Type[])null));
	}

	[IteratorStateMachine(typeof(<Transpiler>d__2))]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <Transpiler>d__2(-2)
		{
			<>3__instructions = instructions
		};
	}

	internal static double HandDelta(Node node)
	{
		double processDeltaTime = node.GetProcessDeltaTime();
		if (ComboPlanning.Active && !ComboPlanning.Network && Engine.TimeScale > 0.0)
		{
			NHandCardHolder val = (NHandCardHolder)(object)((node is NHandCardHolder) ? node : null);
			if (val != null)
			{
				CardModel cardModel = ((NCardHolder)val).CardModel;
				if (((cardModel != null) ? cardModel.Owner : null) == ComboPlanning.Ui?.Player)
				{
					return processDeltaTime / Engine.TimeScale;
				}
			}
		}
		return processDeltaTime;
	}
}
internal static class ComboHotkey
{
	internal const string Action = "more_silent_bullet_time";

	internal const string Title = "moreSilentBulletTime";

	internal static readonly StringName Input = StringName.op_Implicit("more_silent_bullet_time");

	private static readonly MethodInfo ReadKey = AccessTools.Method(typeof(NInputManager), "GetCurrentHotkey", (Type[])null, (Type[])null) ?? AccessTools.Method(typeof(NInputManager), "GetShortcutKey", (Type[])null, (Type[])null);

	private static readonly FieldInfo Listening = AccessTools.Field(typeof(NInputSettingsPanel), "_listeningEntry");

	private static NGenericPopup? _notice;

	internal static Dictionary<StringName, string> Titles => (Dictionary<StringName, string>)(AccessTools.Field(typeof(NInputSettingsEntry), "_commandToLocTitle") ?? AccessTools.Field(typeof(NInputSettingsEntry), "commandToLocTitle")).GetValue(null);

	internal static Key CurrentKey
	{
		get
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			NInputManager instance = NInputManager.Instance;
			if (instance == null)
			{
				return (Key)0;
			}
			return (Key)ReadKey.Invoke(instance, new object[1] { Input });
		}
	}

	internal static void Install()
	{
		if (!InputMap.HasAction(Input))
		{
			InputMap.AddAction(Input, 0.2f);
		}
		string[] array = new string[3] { "remappableKeyboardInputs", "remappableMKbInputs", "remappableKbOnlyInputs" };
		foreach (string text in array)
		{
			if (AccessTools.Field(typeof(NInputManager), text)?.GetValue(null) is IList<StringName> list && !list.Contains(Input))
			{
				list.Add(Input);
			}
		}
		Titles[Input] = "moreSilentBulletTime";
	}

	internal static void Localize(Dictionary<string, string> text)
	{
		string language = LocManager.Instance.Language;
		bool flag = ((language == "zhs" || language == "zht") ? true : false);
		bool flag2 = flag;
		text["INPUT_SETTINGS.INPUT_TITLE.moreSilentBulletTime"] = (flag2 ? "子弹时间" : "Bullet Time");
		text[SettingsLocalization.TitleKey("BulletTimeHotkey")] = (flag2 ? "子弹时间快捷键" : "Bullet Time hotkey");
		text[SettingsLocalization.DescriptionKey("BulletTimeHotkey")] = (flag2 ? "点击后按下快捷键。Esc 取消；Backspace / Delete 清除绑定。" : "Click, then press a key. Esc cancels; Backspace / Delete clears the binding.");
		text["MORESILENTANIMATIONS.keyConflict.title"] = (flag2 ? "按键已被绑定" : "Key already bound");
		text["MORESILENTANIMATIONS.keyConflict.body"] = (flag2 ? "{Key} 已用于「{Command}」。请选用其他按键，原绑定保持不变。" : "{Key} is already bound to {Command}. Choose another key. The existing binding has been kept.");
		text["MORESILENTANIMATIONS.keyConflict.ok"] = (flag2 ? "确定" : "OK");
	}

	internal static bool BeforeKeyChange(NInputManager manager, MethodBase method, StringName input, Key shortcutKey)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		string name = method.Name;
		string text = ((name == "ModifyMKbKey") ? "_mKbInputMap" : ((!(name == "ModifyKbOnlyKey")) ? "_keyboardInputMap" : "_fKbInputMap"));
		string text2 = text;
		Dictionary<StringName, Key> dictionary = (Dictionary<StringName, Key>)AccessTools.Field(typeof(NInputManager), text2).GetValue(manager);
		if (input == Input && (int)shortcutKey == 0)
		{
			dictionary[Input] = (Key)0;
			text = method.Name;
			name = ((text == "ModifyMKbKey") ? "SaveMKbInputMapping" : ((!(text == "ModifyKbOnlyKey")) ? "SaveKeyboardInputMapping" : "SaveFKbInputMapping"));
			string text3 = name;
			AccessTools.Method(typeof(NInputManager), text3, (Type[])null, (Type[])null).Invoke(manager, null);
			((GodotObject)manager).EmitSignal(SignalName.InputRebound, Array.Empty<Variant>());
			return false;
		}
		if ((int)shortcutKey == 0)
		{
			return true;
		}
		KeyValuePair<StringName, Key> keyValuePair = dictionary.FirstOrDefault((KeyValuePair<StringName, Key> pair) => pair.Key != input && pair.Value == shortcutKey && (input == Input || pair.Key == Input));
		if (keyValuePair.Key == null)
		{
			return true;
		}
		ShowConflict(shortcutKey, keyValuePair.Key);
		return false;
	}

	private static void ShowConflict(Key key, StringName command)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		if (_notice != null && GodotObject.IsInstanceValid((GodotObject)(object)_notice) && !((GodotObject)_notice).IsQueuedForDeletion())
		{
			return;
		}
		NModalContainer instance = NModalContainer.Instance;
		if (instance != null && instance.OpenModal == null)
		{
			NGenericPopup val = NGenericPopup.Create();
			if (val != null)
			{
				_notice = val;
				string value;
				string text = (Titles.TryGetValue(command, out value) ? new LocString("settings_ui", "INPUT_SETTINGS.INPUT_TITLE." + value).GetFormattedText() : ((object)command).ToString());
				LocString val2 = new LocString("settings_ui", "MORESILENTANIMATIONS.keyConflict.body");
				val2.AddObj("Key", (object)((object)(Key)(ref key)/*cast due to .constrained prefix*/).ToString());
				val2.AddObj("Command", (object)text);
				instance.Add((Node)(object)val, true);
				FinishNotice(instance, val, val2);
			}
		}
	}

	private static async Task FinishNotice(NModalContainer container, NGenericPopup popup, LocString body)
	{
		try
		{
			await popup.WaitForConfirmation(body, new LocString("settings_ui", "MORESILENTANIMATIONS.keyConflict.title"), (LocString)null, new LocString("settings_ui", "MORESILENTANIMATIONS.keyConflict.ok"));
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
		finally
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)container) && container.OpenModal == popup)
			{
				container.Clear();
			}
			if (_notice == popup)
			{
				_notice = null;
			}
		}
	}

	internal static bool BeforeBindingInput(NInputSettingsPanel panel, InputEvent inputEvent)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I8
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Invalid comparison between Unknown and I8
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Invalid comparison between Unknown and I8
		object? value = Listening.GetValue(panel);
		NInputSettingsEntry val = (NInputSettingsEntry)((value is NInputSettingsEntry) ? value : null);
		if (val == null || val.InputName != Input)
		{
			return true;
		}
		InputEventKey val2 = (InputEventKey)(object)((inputEvent is InputEventKey) ? inputEvent : null);
		if (val2 == null)
		{
			return true;
		}
		((Node)panel).GetViewport().SetInputAsHandled();
		if (!val2.Pressed || val2.Echo)
		{
			return false;
		}
		if ((long)val2.Keycode == 4194305)
		{
			Listening.SetValue(panel, null);
			object? obj = AccessTools.Field(typeof(NInputSettingsPanel), "_listeningPrompt")?.GetValue(panel);
			CanvasItem val3 = (CanvasItem)((obj is CanvasItem) ? obj : null);
			if (val3 != null)
			{
				val3.Visible = false;
			}
			AccessTools.Method(typeof(NControllerManager), "StopListeningForRebind", (Type[])null, (Type[])null)?.Invoke(NControllerManager.Instance, null);
			return false;
		}
		Key keycode = val2.Keycode;
		if (((long)keycode == 4194308 || (long)keycode == 4194312) ? true : false)
		{
			val2.Keycode = (Key)0;
		}
		return true;
	}
}
[HarmonyPatch]
internal static class ComboHotkeyDefaultsPatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return new string[3] { "DefaultKeyboardInputMap", "DefaultHotkeyInputMap", "DefaultKbOnlyInputMap" }.Select((string name) => AccessTools.PropertyGetter(typeof(NInputManager), name)).OfType<MethodBase>();
	}

	[HarmonyPostfix]
	private static void Postfix(Dictionary<StringName, Key> __result)
	{
		__result[ComboHotkey.Input] = (Key)0;
	}
}
[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class ComboHotkeyModalPatch
{
	[HarmonyPostfix]
	private static void Postfix(ref string[] __result)
	{
		if (!__result.Contains("more_silent_bullet_time"))
		{
			string[] array = __result;
			int num = 0;
			string[] array2 = new string[1 + array.Length];
			ReadOnlySpan<string> readOnlySpan = new ReadOnlySpan<string>(array);
			readOnlySpan.CopyTo(new Span<string>(array2).Slice(num, readOnlySpan.Length));
			num += readOnlySpan.Length;
			array2[num] = "more_silent_bullet_time";
			__result = array2;
		}
	}
}
[HarmonyPatch]
internal static class ComboHotkeyConflictPatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return new string[3] { "ModifyShortcutKey", "ModifyMKbKey", "ModifyKbOnlyKey" }.Select((string name) => AccessTools.Method(typeof(NInputManager), name, (Type[])null, (Type[])null)).OfType<MethodBase>();
	}

	[HarmonyPrefix]
	private static bool Prefix(NInputManager __instance, MethodBase __originalMethod, StringName input, Key shortcutKey)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return ComboHotkey.BeforeKeyChange(__instance, __originalMethod, input, shortcutKey);
	}
}
[HarmonyPatch(typeof(NInputSettingsPanel), "_UnhandledKeyInput")]
internal static class ComboHotkeyCapturePatch
{
	[HarmonyPrefix]
	private static bool Prefix(NInputSettingsPanel __instance, InputEvent inputEvent)
	{
		return ComboHotkey.BeforeBindingInput(__instance, inputEvent);
	}
}
[HarmonyPatch(typeof(NInputSettingsEntry), "_Ready")]
internal static class ComboHotkeyEntryPatch
{
	[HarmonyPostfix]
	private static void Postfix(NInputSettingsEntry __instance)
	{
		if (!(__instance.InputName != ComboHotkey.Input))
		{
			string language = LocManager.Instance.Language;
			bool flag = ((language == "zhs" || language == "zht") ? true : false);
			((Control)__instance).TooltipText = (flag ? "点击后按下快捷键。Esc 取消；Backspace / Delete 清除绑定。" : "Click, then press a key. Esc cancels; Backspace / Delete clears the binding.");
		}
	}
}
internal static class ComboKeySetting
{
	[CompilerGenerated]
	private sealed class <Descendants>d__6 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private Node <>2__current;

		private int <>l__initialThreadId;

		private Node node;

		public Node <>3__node;

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
		public <Descendants>d__6(int <>1__state)
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
					<>7__wrap1 = node.GetChildren(false).GetEnumerator();
					<>1__state = -3;
					break;
				case 1:
					<>1__state = -3;
					<>7__wrap3 = Descendants(<child>5__3).GetEnumerator();
					<>1__state = -4;
					goto IL_00c4;
				case 2:
					{
						<>1__state = -4;
						goto IL_00c4;
					}
					IL_00c4:
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
				if (<>7__wrap1.MoveNext())
				{
					<child>5__3 = <>7__wrap1.Current;
					<>2__current = <child>5__3;
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
			<Descendants>d__6 <Descendants>d__;
			if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
			{
				<>1__state = 0;
				<Descendants>d__ = this;
			}
			else
			{
				<Descendants>d__ = new <Descendants>d__6(0);
			}
			<Descendants>d__.node = <>3__node;
			return <Descendants>d__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	internal const string Key = "BulletTimeHotkey";

	private static readonly FieldInfo Listening = AccessTools.Field(typeof(NInputSettingsPanel), "_listeningEntry");

	private static bool KeyboardOnly => AccessTools.Property(typeof(NControllerManager), "InputType")?.GetValue(NControllerManager.Instance)?.ToString() == "KeyboardOnlyMode";

	internal static Control Create()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		NInputSettingsEntry row = NInputSettingsEntry.Create("more_silent_bullet_time");
		((Control)row).CustomMinimumSize = new Vector2(460f, 56f);
		((Control)row).SizeFlagsHorizontal = (SizeFlags)3;
		NInputSettingsPanel panel = null;
		Key consumed = (Key)0;
		GDScript val = new GDScript
		{
			SourceCode = "extends Node\nsignal binding_input(event)\nfunc _input(event):\n\tif event is InputEventKey or event is InputEventMouseButton:\n\t\tbinding_input.emit(event)\n"
		};
		((Script)val).Reload(false);
		Node relay = new Node();
		((GodotObject)relay).SetScript(Variant.op_Implicit((GodotObject)(object)val));
		((Node)row).AddChild(relay, false, (InternalMode)0);
		relay.SetProcessInput(false);
		((NClickableControl)row).Released += (ReleasedEventHandler)delegate
		{
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			Cancel();
			panel = Descendants((Node)(object)((Node)row).GetTree().Root).OfType<NInputSettingsPanel>().FirstOrDefault();
			if (panel == null)
			{
				ModEntry.Log(new InvalidOperationException("Native input settings panel unavailable"));
			}
			else
			{
				((GodotObject)panel).Call(StringName.op_Implicit("SetAsListeningEntry"), (Variant[])(object)new Variant[1] { Variant.op_Implicit((GodotObject)(object)row) });
				if (Listening.GetValue(panel) == row)
				{
					consumed = (Key)0;
					string text = (((Node)row).HasNode(NodePath.op_Implicit("%KeyBindingInputLabel")) ? "%KeyBindingInputLabel" : (KeyboardOnly ? "%KbModeBindingInputLabel" : "%MKbBindingInputLabel"));
					Node node = ((Node)row).GetNode(NodePath.op_Implicit(text));
					StringName val2 = StringName.op_Implicit("text");
					string language = LocManager.Instance.Language;
					bool flag = ((language == "zhs" || language == "zht") ? true : false);
					((GodotObject)node).Set(val2, Variant.op_Implicit(flag ? "请按键…" : "Press a key…"));
					relay.SetProcessInput(true);
				}
			}
		};
		((GodotObject)relay).Connect(StringName.op_Implicit("binding_input"), Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent input)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			if (!((CanvasItem)row).IsVisibleInTree())
			{
				Cancel();
			}
			else
			{
				InputEventKey val3 = (InputEventKey)(object)((input is InputEventKey) ? input : null);
				if (val3 != null)
				{
					((Node)row).GetViewport().SetInputAsHandled();
					if ((int)consumed != 0)
					{
						if (!val3.Pressed && val3.Keycode == consumed)
						{
							Cancel();
						}
					}
					else if (val3.Pressed && !val3.Echo && panel != null)
					{
						consumed = val3.Keycode;
						((Node)panel)._UnhandledKeyInput((InputEvent)(object)val3);
						((GodotObject)row).Call(StringName.op_Implicit("UpdateInput"), Array.Empty<Variant>());
					}
				}
				else
				{
					InputEventMouseButton val4 = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
					if (val4 != null && val4.Pressed)
					{
						Cancel();
					}
				}
			}
		}), 0u);
		((CanvasItem)row).VisibilityChanged += delegate
		{
			if (!((CanvasItem)row).IsVisibleInTree())
			{
				Cancel();
			}
		};
		((Node)row).TreeExiting += Cancel;
		((Node)row).Ready += delegate
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			((GodotObject)row).Call(StringName.op_Implicit("UpdateInput"), Array.Empty<Variant>());
		};
		return (Control)(object)row;
		void Cancel()
		{
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			relay.SetProcessInput(false);
			if (panel != null && GodotObject.IsInstanceValid((GodotObject)(object)panel) && Listening.GetValue(panel) == row)
			{
				Listening.SetValue(panel, null);
				object? obj = AccessTools.Field(typeof(NInputSettingsPanel), "_listeningPrompt")?.GetValue(panel);
				CanvasItem val5 = (CanvasItem)((obj is CanvasItem) ? obj : null);
				if (val5 != null)
				{
					val5.Visible = false;
				}
				AccessTools.Method(typeof(NControllerManager), "StopListeningForRebind", (Type[])null, (Type[])null)?.Invoke(NControllerManager.Instance, null);
			}
			panel = null;
			if (((Node)row).IsInsideTree())
			{
				((GodotObject)row).Call(StringName.op_Implicit("UpdateInput"), Array.Empty<Variant>());
			}
		}
	}

	internal static void Save(Key key)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		MethodInfo methodInfo = AccessTools.Method(typeof(NInputManager), "ModifyShortcutKey", (Type[])null, (Type[])null);
		if ((object)methodInfo == null)
		{
			methodInfo = AccessTools.Method(typeof(NInputManager), KeyboardOnly ? "ModifyKbOnlyKey" : "ModifyMKbKey", (Type[])null, (Type[])null);
		}
		methodInfo.Invoke(NInputManager.Instance, new object[2]
		{
			ComboHotkey.Input,
			key
		});
	}

	[IteratorStateMachine(typeof(<Descendants>d__6))]
	private static IEnumerable<Node> Descendants(Node node)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <Descendants>d__6(-2)
		{
			<>3__node = node
		};
	}
}
[HarmonyPatch(typeof(NInputSettingsPanel), "_Ready")]
internal static class ComboModOnlyInputRowPatch
{
	[HarmonyPrefix]
	private static void Prefix(out List<IList<StringName>> __state)
	{
		__state = new List<IList<StringName>>();
		string[] array = new string[3] { "remappableKeyboardInputs", "remappableMKbInputs", "remappableKbOnlyInputs" };
		foreach (string text in array)
		{
			if (AccessTools.Field(typeof(NInputManager), text)?.GetValue(null) is IList<StringName> list && list.Remove(ComboHotkey.Input))
			{
				__state.Add(list);
			}
		}
	}

	[HarmonyFinalizer]
	private static void Finalizer(List<IList<StringName>> __state)
	{
		foreach (IList<StringName> item in __state)
		{
			if (!item.Contains(ComboHotkey.Input))
			{
				item.Add(ComboHotkey.Input);
			}
		}
	}
}
internal static class ComboLocalPlan
{
	private sealed class Intent(CardModel? card, Creature? target, ComboShivPlan.Knife? knife = null)
	{
		internal readonly CardModel? Card = card;

		internal readonly Creature? Target = target;

		internal readonly ComboShivPlan.Knife? Knife = knife;

		internal ComboShivPlan.Source? Source;

		internal NCard? View;

		internal NCardHolder? Holder;

		internal bool Dropped;
	}

	private static readonly List<Intent> Plan = new List<Intent>();

	private static readonly List<ComboShivPlan.Knife> Knives = new List<ComboShivPlan.Knife>();

	private static ActionQueueSet? _queues;

	private static Intent? _current;

	private static CardModel? _requested;

	private static uint? _requestedTarget;

	private static PlayCardAction? _running;

	private static ulong _requestedAt;

	private static int _next;

	private static bool _sending;

	private static bool _pumping;

	internal static bool InUse => _queues != null;

	internal static bool Playing { get; private set; }

	internal static int Count => Plan.Count((Intent p) => !p.Dropped);

	internal static IReadOnlyList<ComboShivPlan.Knife> Previews => Knives;

	internal static ComboShivPlan.Source? CurrentSource
	{
		get
		{
			if (_running == null || RunManager.Instance.ActionExecutor.CurrentlyRunningAction != _running)
			{
				return null;
			}
			return _current?.Source;
		}
	}

	internal static void Begin()
	{
		Clear();
		_queues = RunManager.Instance.ActionQueueSet;
		_queues.ActionEnqueued += OnEnqueued;
	}

	internal static ComboShivPlan.Knife? Find(CardModel card)
	{
		return Knives.FirstOrDefault((ComboShivPlan.Knife k) => k.Preview == card);
	}

	internal static int DisplayIndex(CardModel card, int fallback)
	{
		int num = Plan.FindIndex((Intent p) => (p.card ?? p.knife?.Preview) == card);
		if (num >= 0)
		{
			return num;
		}
		return fallback;
	}

	internal static void ArrangeHand(NPlayerHand hand)
	{
		if (!InUse)
		{
			return;
		}
		Control cardHolderContainer = hand.CardHolderContainer;
		NHandCardHolder[] array = (from h in ((IEnumerable)((Node)cardHolderContainer).GetChildren(false)).OfType<NHandCardHolder>()
			orderby ((CanvasItem)h).Visible descending
			select h).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (((Node)array[i]).GetIndex(false) != i)
			{
				((Node)cardHolderContainer).MoveChild((Node)(object)array[i], i);
			}
		}
	}

	internal static bool Stage(CardModel card, Creature? target)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Invalid comparison between Unknown and I4
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Expected O, but got Unknown
		TargetType targetType = card.TargetType;
		if (((int)targetType != 2 && (int)targetType != 6) || 1 == 0)
		{
			target = null;
		}
		if (InUse && ComboPlanning.Active)
		{
			ComboPlanningUi ui = ComboPlanning.Ui;
			if (ui != null && card.Owner == ui.Player)
			{
				CardPile pile = card.Pile;
				if (pile != null && (int)pile.Type == 2 && !Plan.Any((Intent p) => !p.Dropped && p.card == card) && card.CanPlayTargeting(target))
				{
					Intent intent = new Intent(card, target);
					NCard val = NCard.Create(card, (ModelVisibility)1);
					if (val == null)
					{
						return false;
					}
					intent.View = val;
					intent.Holder = ui.Native.Hand.GetCardHolder(card);
					((Node)ui.Native.PlayQueue).AddChild((Node)(object)val, false, (InternalMode)0);
					((Node)val).Name = StringName.op_Implicit("MoreSilentPlannedCard");
					val.SetPreviewTarget(target);
					val.UpdateVisuals((PileType)2, (CardPreviewMode)1);
					NCardHolder? holder = intent.Holder;
					NCard val2 = ((holder != null) ? holder.CardNode : null);
					if (val2 != null)
					{
						((Control)val).GlobalPosition = ((Control)val2).GlobalPosition;
						Transform2D globalTransform = ((CanvasItem)val2).GetGlobalTransform();
						Vector2 scale = ((Transform2D)(ref globalTransform)).Scale;
						globalTransform = ((CanvasItem)ui.Native.PlayQueue).GetGlobalTransform();
						((Control)val).Scale = scale / ((Transform2D)(ref globalTransform)).Scale;
					}
					Plan.Add(intent);
					NCardHolder? holder2 = intent.Holder;
					if (holder2 != null)
					{
						((CanvasItem)holder2).Hide();
					}
					((GodotObject)ui.Native.Hand).Call(StringName.op_Implicit("RefreshLayout"), Array.Empty<Variant>());
					ComboNativeHand.Place(val, Plan.Count - 1);
					ComboShivPlan.Source source = new ComboShivPlan.Source(new PlayCardAction(card, target), card)
					{
						Repeats = ComboForecast.Record(card)
					};
					List<ComboShivPlan.Knife> list = ComboShivPlan.CreatePreviews(source);
					if (list.Count > 0)
					{
						intent.Source = source;
						Knives.AddRange(list);
					}
					ComboNativeHand.Refresh(Knives);
					return true;
				}
			}
		}
		return false;
	}

	internal static bool Select(ComboShivPlan.Knife knife, Creature? target)
	{
		if (!InUse || !ComboPlanning.Active || knife.Selected || knife.Source.Done || !knife.Preview.IsValidTarget(target))
		{
			return false;
		}
		knife.Target = target;
		knife.Selected = true;
		Plan.Add(new Intent(null, target, knife));
		ComboForecast.Record(knife.Preview);
		ComboNativeHand.Queue(knife);
		ComboNativeHand.Refresh(Knives);
		return true;
	}

	internal static void Release(bool cancel)
	{
		if (!InUse)
		{
			return;
		}
		if (cancel)
		{
			Clear();
			return;
		}
		if (ComboNativeHand.Targeting)
		{
			NTargetManager.Instance.CancelTargeting();
		}
		foreach (ComboShivPlan.Knife knife in Knives)
		{
			ComboNativeHand.Free(knife);
		}
		Playing = true;
		Tick();
	}

	internal static void BeforeRequest(GameAction action)
	{
		if (!InUse || _sending)
		{
			return;
		}
		ulong ownerId = action.OwnerId;
		ComboPlanningUi? ui = ComboPlanning.Ui;
		if (ownerId == ((ui != null) ? new ulong?(ui.Player.NetId) : ((ulong?)null)) && ActionQueueSet.IsGameActionPlayerDriven(action))
		{
			if (ComboPlanning.Active)
			{
				ComboPlanning.Release(cancel: true);
			}
			else
			{
				Clear();
			}
		}
	}

	private static void OnEnqueued(GameAction action)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (_requested == null || _current == null)
		{
			return;
		}
		PlayCardAction val = (PlayCardAction)(object)((action is PlayCardAction) ? action : null);
		if (val != null && val.Player == _requested.Owner)
		{
			NetCombatCard netCombatCard = val.NetCombatCard;
			if (((NetCombatCard)(ref netCombatCard)).ToCardModelOrNull() == _requested && val.TargetId == _requestedTarget)
			{
				_running = val;
				_requested = null;
				((GameAction)val).AfterFinished += Finished;
				((GameAction)val).BeforeCancelled += Finished;
			}
		}
	}

	private static void Finished(GameAction action)
	{
		if (action == _running)
		{
			Unwatch();
			ComboShivPlan.Source source = _current?.Source;
			if (source != null)
			{
				source.Done = true;
			}
			_current = null;
		}
	}

	private static void Unwatch()
	{
		PlayCardAction running = _running;
		if (running != null)
		{
			((GameAction)running).AfterFinished -= Finished;
			((GameAction)running).BeforeCancelled -= Finished;
		}
		_running = null;
	}

	internal static void Tick()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Invalid comparison between Unknown and I4
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Invalid comparison between Unknown and I4
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Invalid comparison between Unknown and I4
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Invalid comparison between Unknown and I4
		if (!InUse || _pumping)
		{
			return;
		}
		ComboPlanningUi ui = ComboPlanning.Ui;
		if (ui == null || !ui.Eligible)
		{
			ComboPlanning.Release(cancel: true);
			Clear();
		}
		else if (ComboPlanning.Active)
		{
			foreach (Intent item in Plan)
			{
				CardModel card = item.card;
				if (card == null || item.Dropped)
				{
					continue;
				}
				CardPile pile = card.Pile;
				if (pile != null && (int)pile.Type == 2)
				{
					Creature target = item.target;
					if (target == null || target.IsAlive)
					{
						continue;
					}
				}
				Drop(item);
			}
			foreach (ComboShivPlan.Knife knife2 in Knives)
			{
				if (knife2.Source.Done)
				{
					ComboNativeHand.Free(knife2);
				}
			}
			ComboNativeHand.Refresh(Knives);
		}
		else
		{
			if (!Playing)
			{
				return;
			}
			if (_requested != null)
			{
				if (Time.GetTicksMsec() - _requestedAt > 30000)
				{
					Clear();
				}
			}
			else
			{
				if (_running != null || RunManager.Instance.ActionExecutor.IsPaused || ui.Native.Hand.InCardPlay || ui.Native.Hand.IsInCardSelection)
				{
					return;
				}
				_pumping = true;
				try
				{
					while (_next < Plan.Count)
					{
						Intent intent = Plan[_next++];
						if (intent.Dropped)
						{
							continue;
						}
						CardModel val = intent.card;
						ComboShivPlan.Knife knife = intent.knife;
						if (knife != null)
						{
							if (!knife.Source.Done)
							{
								_next--;
								return;
							}
							val = ComboShivPlan.Resolve(knife);
						}
						RestoreView(intent);
						TargetType? val2 = ((val != null) ? new TargetType?(val.TargetType) : ((TargetType?)null));
						bool flag;
						if (val2.HasValue)
						{
							TargetType valueOrDefault = val2.GetValueOrDefault();
							if ((int)valueOrDefault == 2 || (int)valueOrDefault == 6)
							{
								flag = true;
								goto IL_0218;
							}
						}
						flag = false;
						goto IL_0218;
						IL_0218:
						Creature val3 = (flag ? intent.target : null);
						if (val != null)
						{
							CardPile pile2 = val.Pile;
							if (pile2 != null && (int)pile2.Type == 2 && val.CanPlayTargeting(val3))
							{
								_current = intent;
								_requested = val;
								_requestedTarget = ((val3 != null) ? val3.CombatId : ((uint?)null));
								_requestedAt = Time.GetTicksMsec();
								_sending = true;
								bool flag2;
								try
								{
									flag2 = val.TryManualPlay(val3);
								}
								finally
								{
									_sending = false;
								}
								if (!flag2)
								{
									_requested = null;
									_current = null;
									Drop(intent);
									continue;
								}
								return;
							}
						}
						Drop(intent);
					}
					Clear();
				}
				finally
				{
					_pumping = false;
				}
			}
		}
	}

	private static void Drop(Intent item)
	{
		item.Dropped = true;
		ComboShivPlan.Source source = item.Source;
		if (source != null)
		{
			source.Done = true;
		}
		RestoreView(item);
	}

	private static void RestoreView(Intent item)
	{
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Invalid comparison between Unknown and I4
		NCard view = item.View;
		if (view != null && GodotObject.IsInstanceValid((GodotObject)(object)view))
		{
			((Node)view).QueueFree();
		}
		item.View = null;
		NCardHolder holder = item.Holder;
		if (holder != null && GodotObject.IsInstanceValid((GodotObject)(object)holder) && !((GodotObject)holder).IsQueuedForDeletion() && holder.CardModel == item.card)
		{
			((CanvasItem)holder).Show();
			if (((Node)holder).IsInsideTree())
			{
				NPlayerHand instance = NPlayerHand.Instance;
				if (instance != null)
				{
					Control cardHolderContainer = instance.CardHolderContainer;
					if (((Node)holder).GetParent() == cardHolderContainer)
					{
						CardModel? card = item.card;
						if (card != null)
						{
							CardPile pile = card.Pile;
							if ((int)((pile != null) ? new PileType?(pile.Type) : ((PileType?)null)).GetValueOrDefault() == 2)
							{
								List<CardModel> cards = item.card.Pile.Cards.ToList();
								int target = cards.IndexOf(item.card);
								int num2 = ((IEnumerable)((Node)cardHolderContainer).GetChildren(false)).OfType<NHandCardHolder>().Count(delegate(NHandCardHolder h)
								{
									if (h != holder && ((CanvasItem)h).Visible)
									{
										CardModel cardModel = ((NCardHolder)h).CardModel;
										if (cardModel != null)
										{
											int num = cards.IndexOf(cardModel);
											if (num >= 0)
											{
												return num < target;
											}
										}
									}
									return false;
								});
								((Node)cardHolderContainer).MoveChild((Node)(object)holder, num2);
							}
						}
					}
					((GodotObject)instance).Call(StringName.op_Implicit("RefreshLayout"), Array.Empty<Variant>());
				}
			}
		}
		item.Holder = null;
	}

	internal static void Clear()
	{
		if (ComboNativeHand.Targeting)
		{
			NTargetManager.Instance.CancelTargeting();
		}
		ActionQueueSet queues = _queues;
		if (queues != null)
		{
			queues.ActionEnqueued -= OnEnqueued;
		}
		_queues = null;
		Unwatch();
		foreach (ComboShivPlan.Knife knife in Knives)
		{
			ComboNativeHand.Free(knife);
		}
		foreach (Intent item in Plan)
		{
			RestoreView(item);
		}
		Plan.Clear();
		Knives.Clear();
		_next = 0;
		_current = null;
		_requested = null;
		Playing = false;
	}
}
internal static class ComboNativeHand
{
	private static readonly FieldInfo Current = AccessTools.Field(typeof(NPlayerHand), "_currentCardPlay");

	private static readonly FieldInfo Trying = AccessTools.Field(typeof(NCardPlay), "_isTryingToPlayCard");

	private static readonly MethodInfo Cleanup = AccessTools.Method(typeof(NCardPlay), "Cleanup", (Type[])null, (Type[])null);

	private static bool _refreshing;

	private static readonly ShaderMaterial StripeMaterial = new ShaderMaterial
	{
		Shader = new Shader
		{
			Code = "shader_type canvas_item;\nvoid fragment() {\n    vec2 p = UV * vec2(270.0, 376.0);\n    float band = mod(p.x + p.y, 23.0);\n    float line = smoothstep(0.0, 1.2, band) * (1.0 - smoothstep(4.8, 6.0, band));\n    vec2 q = abs(p - vec2(135.0, 188.0)) - vec2(123.0, 176.0);\n    float edge = length(max(q, vec2(0.0))) + min(max(q.x,q.y),0.0) - 12.0;\n    COLOR = vec4(0.86, 0.95, 0.94, line * 0.32 * (1.0 - smoothstep(-1.0, 0.0, edge)));\n}"
		}
	};

	internal static bool Targeting
	{
		get
		{
			NPlayerHand instance = NPlayerHand.Instance;
			if (instance != null && instance.InCardPlay)
			{
				object? value = Current.GetValue(instance);
				NCardPlay val = (NCardPlay)((value is NCardPlay) ? value : null);
				if (val != null)
				{
					NHandCardHolder holder = val.Holder;
					return ComboShivPlan.Find((holder != null) ? ((NCardHolder)holder).CardModel : null) != null;
				}
			}
			return false;
		}
	}

	internal static void Refresh(IReadOnlyList<ComboShivPlan.Knife> knives)
	{
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		if (knives.Count == 0 || _refreshing || !ComboPlanning.Active)
		{
			return;
		}
		ComboPlanningUi ui = ComboPlanning.Ui;
		if (ui == null)
		{
			return;
		}
		NPlayerHand hand = ui.Native.Hand;
		if (hand.InCardPlay || hand.IsInCardSelection)
		{
			return;
		}
		_refreshing = true;
		try
		{
			int num = ((IEnumerable)((Node)hand.CardHolderContainer).GetChildren(false)).OfType<CanvasItem>().Count((CanvasItem n) => n.Visible);
			int num2 = knives.Count - 1;
			while (num > 10 && num2 >= 0)
			{
				if (knives[num2].View is NHandCardHolder && !knives[num2].Selected)
				{
					Free(knives[num2]);
					num--;
				}
				num2--;
			}
			foreach (ComboShivPlan.Knife knife in knives)
			{
				if (!knife.Selected && !knife.Source.Done && knife.View == null && num < 10)
				{
					NCard val = (knife.Node = NCard.Create(knife.Preview, (ModelVisibility)1));
					((CanvasItem)val).Modulate = new Color(1f, 1f, 1f, 0.55f);
					NHandCardHolder val2 = hand.Add(val, -1);
					((Node)val2).Name = StringName.op_Implicit("PendingShiv");
					knife.View = (Control?)(object)val2;
					knife.HandIndex = ((Node)val2).GetIndex(false);
					((Node)val).AddChild((Node)(object)CreateStripes(), false, (InternalMode)0);
					ComboDiscardPlan.Decorate(val, knife.Preview);
					num++;
				}
			}
		}
		finally
		{
			_refreshing = false;
		}
	}

	internal static void Queue(ComboShivPlan.Knife knife)
	{
		Control? view = knife.View;
		NHandCardHolder val = (NHandCardHolder)(object)((view is NHandCardHolder) ? view : null);
		if (val == null)
		{
			return;
		}
		NCard node = knife.Node;
		if (node == null)
		{
			return;
		}
		ComboPlanningUi ui = ComboPlanning.Ui;
		if (ui == null)
		{
			return;
		}
		NPlayerHand hand = ui.Native.Hand;
		if (hand.InCardPlay)
		{
			object? value = Current.GetValue(hand);
			NCardPlay val2 = (NCardPlay)((value is NCardPlay) ? value : null);
			if (val2 != null && val2.Holder == val && !((GodotObject)val2).IsQueuedForDeletion())
			{
				return;
			}
		}
		NCardPlayQueue playQueue = ui.Native.PlayQueue;
		((Node)node).Reparent((Node)(object)playQueue, true);
		knife.View = (Control?)(object)node;
		object? value2 = Current.GetValue(hand);
		NCardPlay val3 = (NCardPlay)((value2 is NCardPlay) ? value2 : null);
		if (val3 != null && GodotObject.IsInstanceValid((GodotObject)(object)val3) && val3.Holder == val)
		{
			Trying.SetValue(val3, true);
			try
			{
				hand.RemoveCardHolder((NCardHolder)(object)val);
			}
			finally
			{
				Trying.SetValue(val3, false);
			}
		}
		else
		{
			hand.RemoveCardHolder((NCardHolder)(object)val);
		}
		((Node)playQueue).MoveChild((Node)(object)node, 0);
		int index = ComboShivPlan.DisplayIndex(knife.Preview, 0);
		Place(node, index);
	}

	internal static void Place(NCard card, int index)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		NCardPlayQueue playQueue = ComboPlanning.Ui.Native.PlayQueue;
		Tween val = ((Node)card).CreateTween().SetIgnoreTimeScale(true).SetParallel(true);
		val.TweenProperty((GodotObject)(object)card, NodePath.op_Implicit("position"), ((GodotObject)playQueue).Call(StringName.op_Implicit("GetPositionForQueueIndex"), (Variant[])(object)new Variant[2]
		{
			Variant.op_Implicit((GodotObject)(object)card),
			Variant.op_Implicit(index)
		}), 0.35).SetEase((EaseType)1).SetTrans((TransitionType)7);
		val.TweenProperty((GodotObject)(object)card, NodePath.op_Implicit("scale"), ((GodotObject)playQueue).Call(StringName.op_Implicit("GetScaleForQueueIndex"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(index) }), 0.35).SetEase((EaseType)1).SetTrans((TransitionType)7);
		val.TweenProperty((GodotObject)(object)card, NodePath.op_Implicit("rotation"), Variant.op_Implicit(0f), 0.35);
	}

	internal static bool TryPlay(NCardPlay play, Creature? target)
	{
		if (ComboDiscardPlan.HandleHand(((NCardHolder)play.Holder).CardModel))
		{
			play.CancelPlayCard();
			return false;
		}
		ComboShivPlan.Knife knife = ComboShivPlan.Find(((NCardHolder)play.Holder).CardModel);
		if (knife == null)
		{
			if (!ComboPlanning.Active || !ComboPlanning.Network)
			{
				return true;
			}
			CardModel cardModel = ((NCardHolder)play.Holder).CardModel;
			play.CancelPlayCard();
			if (cardModel != null)
			{
				ComboLocalPlan.Stage(cardModel, target);
			}
			ActiveScreenContext.Instance.FocusOnDefaultControl();
			return false;
		}
		if (!knife.Preview.IsValidTarget(target) || !ComboShivPlan.SelectForTarget(knife, target))
		{
			play.CancelPlayCard();
			return false;
		}
		NCombatRoom.Instance.LastTargetedCreature = target;
		Cleanup.Invoke(play, new object[1] { true });
		Queue(knife);
		ActiveScreenContext.Instance.FocusOnDefaultControl();
		return false;
	}

	internal static void Free(ComboShivPlan.Knife knife)
	{
		Control view = knife.View;
		if (view != null && GodotObject.IsInstanceValid((GodotObject)(object)view))
		{
			NHandCardHolder val = (NHandCardHolder)(object)((view is NHandCardHolder) ? view : null);
			if (val != null)
			{
				NPlayerHand instance = NPlayerHand.Instance;
				if (instance != null)
				{
					instance.RemoveCardHolder((NCardHolder)(object)val);
					goto IL_0034;
				}
			}
			((Node)view).QueueFree();
		}
		goto IL_0034;
		IL_0034:
		knife.View = null;
		knife.Node = null;
	}

	internal static ColorRect CreateStripes()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		return new ColorRect
		{
			Name = StringName.op_Implicit("PendingStripes"),
			Position = new Vector2(-135f, -188f),
			Size = new Vector2(270f, 376f),
			MouseFilter = (MouseFilterEnum)2,
			Material = (Material)(object)StripeMaterial
		};
	}

	internal static void RebindPreviews()
	{
		NPlayerHand instance = NPlayerHand.Instance;
		if (instance == null)
		{
			return;
		}
		foreach (ComboShivPlan.Knife preview in ComboShivPlan.Previews)
		{
			if (preview.Selected)
			{
				continue;
			}
			NCardHolder cardHolder = instance.GetCardHolder(preview.Preview);
			if (cardHolder != null)
			{
				preview.View = (Control?)(object)cardHolder;
				preview.Node = cardHolder.CardNode;
				if (cardHolder is NHandCardHolder)
				{
					preview.HandIndex = ((Node)cardHolder).GetIndex(false);
				}
			}
		}
	}
}
[HarmonyPatch(typeof(PeerInputSynchronizer), "SyncLocalHoveredModel")]
internal static class ComboPreviewHoverIdentityPatch
{
	[HarmonyPrefix]
	private static void Prefix(ref AbstractModel? model)
	{
		AbstractModel? obj = model;
		CardModel val = (CardModel)(object)((obj is CardModel) ? obj : null);
		if (val != null && ComboShivPlan.Find(val) != null)
		{
			model = null;
		}
	}
}
[HarmonyPatch(typeof(NCardPlay), "TryPlayCard")]
internal static class ComboNativeShivPlayPatch
{
	[HarmonyPrefix]
	private static bool Prefix(NCardPlay __instance, Creature? target)
	{
		return ComboNativeHand.TryPlay(__instance, target);
	}
}
[HarmonyPatch(typeof(CardModel), "TryManualPlay")]
internal static class ComboPreviewNeverEnqueuePatch
{
	[HarmonyPrefix]
	private static bool Prefix(CardModel __instance, Creature? target, ref bool __result)
	{
		if (ComboDiscardPlan.HandleHand(__instance))
		{
			__result = false;
			return false;
		}
		ComboShivPlan.Knife knife = ComboShivPlan.Find(__instance);
		if (knife == null)
		{
			if (!ComboPlanning.Active || !ComboPlanning.Network || __instance.Owner != ComboPlanning.Ui?.Player)
			{
				return true;
			}
			__result = ComboLocalPlan.Stage(__instance, target);
			return false;
		}
		__result = __instance.IsValidTarget(target) && ComboShivPlan.SelectForTarget(knife, target);
		return false;
	}
}
[HarmonyPatch(typeof(NPlayerHand), "SelectCards")]
internal static class ComboLocalHandSelectionPatch
{
	[HarmonyPrefix]
	private static void Prefix()
	{
		if (ComboLocalPlan.InUse)
		{
			ComboPlanning.Release(cancel: true);
		}
	}
}
[HarmonyPatch(typeof(NPlayerHand), "RefreshLayout")]
internal static class ComboLocalHandOrderPatch
{
	[HarmonyPrefix]
	private static void Prefix(NPlayerHand __instance)
	{
		ComboLocalPlan.ArrangeHand(__instance);
	}
}
[HarmonyPatch(typeof(NPlayerHand), "StartCardPlay")]
internal static class ComboPreviewRememberSlotPatch
{
	[HarmonyPrefix]
	private static bool Prefix(NHandCardHolder holder)
	{
		if (ComboDiscardPlan.HandleHand(((NCardHolder)holder).CardModel))
		{
			return false;
		}
		ComboShivPlan.Knife knife = ComboShivPlan.Find(((NCardHolder)holder).CardModel);
		if (knife != null)
		{
			knife.HandIndex = ((Node)holder).GetIndex(false);
		}
		return true;
	}
}
[HarmonyPatch(typeof(NPlayerHand), "GetHandInsertIndex")]
internal static class ComboPreviewReturnSlotPatch
{
	[HarmonyPrefix]
	private static bool Prefix(NPlayerHand __instance, CardModel card, ref int __result)
	{
		ComboShivPlan.Knife knife = ComboShivPlan.Find(card);
		if (knife == null)
		{
			return true;
		}
		__result = Math.Clamp(knife.HandIndex, 0, Math.Max(0, ((Node)__instance.CardHolderContainer).GetChildCount(false) - 1));
		return false;
	}
}
internal static class ComboPlanning
{
	private static readonly List<PlayCardAction> Pending = new List<PlayCardAction>();

	private static ActionExecutor? _executor;

	private static ActionQueueSet? _queues;

	private static double _restoreScale = 1.0;

	private static double _writtenScale = 1.0;

	private const double SlowScale = 0.1;

	private static bool _exiting;

	internal static ComboPlanningUi? Ui { get; private set; }

	internal static bool Active { get; private set; }

	internal static bool Network => !RunManager.Instance.IsSingleplayerOrFakeMultiplayer;

	internal static int Count
	{
		get
		{
			if (!ComboLocalPlan.InUse)
			{
				return Pending.Count((PlayCardAction a) => (int)((GameAction)a).State == 1) + ComboShivPlan.DeferredCount;
			}
			return ComboLocalPlan.Count;
		}
	}

	internal static bool CanEnter
	{
		get
		{
			if (!_exiting && !ComboEndTurn.Pending)
			{
				ComboPlanningUi ui = Ui;
				if (ui != null && ui.Eligible && !ComboShivPlan.Playing && !ComboLocalPlan.Playing && !ComboChoiceContinuation.Playing && !CombatManager.Instance.IsPaused && !RunManager.Instance.ActionExecutor.IsRunning && !RunManager.Instance.ActionExecutor.IsPaused && !Ui.Native.Hand.InCardPlay && !Ui.Native.Hand.IsInCardSelection)
				{
					return Engine.TimeScale > 0.0;
				}
			}
			return false;
		}
	}

	internal static void Attach(NCombatUi ui, CombatState state)
	{
		Detach();
		try
		{
			Ui = ComboPlanningUi.Create(ui, state);
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	internal static bool Toggle()
	{
		if (Active)
		{
			Release();
			return true;
		}
		if (!CanEnter)
		{
			return false;
		}
		if (!Network)
		{
			ComboSpeedCompatibility.InstallIfAvailable();
		}
		ComboShivPlan.Begin();
		ComboChoiceContinuation.Clear();
		ComboForecast.Clear();
		ComboDiscardPlan.Begin();
		if (Network)
		{
			ComboLocalPlan.Begin();
		}
		_executor = RunManager.Instance.ActionExecutor;
		_queues = RunManager.Instance.ActionQueueSet;
		Pending.Clear();
		_queues.ActionEnqueued += OnEnqueued;
		_restoreScale = Engine.TimeScale;
		Active = true;
		if (!Network)
		{
			WriteSlowScale(_restoreScale);
		}
		VisualCleanup.Safely(ComboAudio.Begin);
		try
		{
			Ui.SetDark(active: true);
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
		Ui.PlayTransition(entering: true);
		Ui.Refresh();
		return true;
	}

	private static void OnEnqueued(GameAction action)
	{
		if (Active && !Network)
		{
			PlayCardAction val = (PlayCardAction)(object)((action is PlayCardAction) ? action : null);
			if (val != null && val.Player == Ui?.Player)
			{
				Pending.Add(val);
				ComboShivPlan.NativeQueued(val);
			}
			else
			{
				Release();
			}
		}
	}

	internal static bool Holds(ActionExecutor executor)
	{
		if (Active && !Network)
		{
			return _executor == executor;
		}
		return false;
	}

	internal static async Task WaitForRelease(ActionExecutor executor, Task nativeWait)
	{
		await nativeWait;
		SceneTree tree = (SceneTree)Engine.GetMainLoop();
		while (RunManager.Instance.ActionExecutor == executor && (Holds(executor) || executor.IsPaused))
		{
			await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
		}
	}

	internal static void Tick()
	{
		if (_exiting)
		{
			return;
		}
		ComboLocalPlan.Tick();
		ComboChoiceContinuation.Tick();
		ComboEndTurn.Tick();
		ComboPlanningUi ui;
		if (!Active)
		{
			if (ComboShivPlan.Playing)
			{
				ui = Ui;
				if (ui == null || !ui.Eligible)
				{
					ComboShivPlan.Clear(cancel: true);
				}
			}
			return;
		}
		ui = Ui;
		if (ui == null || !ui.Eligible || _executor != RunManager.Instance.ActionExecutor)
		{
			Release(cancel: true);
			return;
		}
		ComboShivPlan.RefreshHand();
		ComboDiscardSelection.Tick();
		if (!Network && !Mathf.IsEqualApprox(Engine.TimeScale, _writtenScale))
		{
			_restoreScale = Engine.TimeScale;
			WriteSlowScale(_restoreScale);
		}
	}

	private static void WriteSlowScale(double requested)
	{
		_writtenScale = Math.Min(requested, 0.1);
		Engine.TimeScale = _writtenScale;
	}

	internal static void NativeHitStop()
	{
		if (Active && !Network)
		{
			_restoreScale = Engine.TimeScale;
			WriteSlowScale(_restoreScale);
		}
	}

	internal static bool BeforeExternalClockUpdate()
	{
		if (!Active || Network)
		{
			return false;
		}
		if (Mathf.IsEqualApprox(Engine.TimeScale, _writtenScale))
		{
			Engine.TimeScale = _restoreScale;
		}
		return true;
	}

	internal static void AfterExternalClockUpdate()
	{
		if (Active && !Network)
		{
			_restoreScale = Engine.TimeScale;
			WriteSlowScale(_restoreScale);
		}
	}

	internal static void Release(bool cancel = false)
	{
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Invalid comparison between Unknown and I4
		ComboDiscardSelection.Cancel();
		VisualCleanup.Safely(ComboAudio.Restore);
		if (cancel)
		{
			ComboEndTurn.Clear();
			ComboChoiceContinuation.Clear();
			ComboShivPlan.Release(cancel: true);
			ComboLocalPlan.Clear();
			Ui?.StopTransition();
		}
		if (!Active)
		{
			return;
		}
		Active = false;
		ComboDiscardPlan.Release();
		VisualCleanup.Safely(delegate
		{
			Ui?.SetDark(active: false);
		});
		if (_queues != null)
		{
			_queues.ActionEnqueued -= OnEnqueued;
		}
		_queues = null;
		_executor = null;
		if (!Network && Mathf.IsEqualApprox(Engine.TimeScale, _writtenScale))
		{
			Engine.TimeScale = _restoreScale;
		}
		try
		{
			if (!cancel)
			{
				return;
			}
			foreach (PlayCardAction item in Pending)
			{
				if ((int)((GameAction)item).State == 1)
				{
					VisualCleanup.Safely((Action)((GameAction)item).Cancel);
				}
			}
		}
		finally
		{
			Pending.Clear();
			if (!cancel)
			{
				if (Network)
				{
					ComboLocalPlan.Release(cancel: false);
				}
				else
				{
					ComboShivPlan.Release(cancel: false);
				}
			}
			if (!cancel)
			{
				Ui?.PlayTransition(entering: false);
			}
			Ui?.Refresh();
		}
	}

	internal static void PrepareForExit()
	{
		_exiting = true;
		Release(cancel: true);
		ComboDiscardPlan.Clear();
		ComboForecast.Clear();
	}

	internal static void Detach()
	{
		Release(cancel: true);
		ComboDiscardPlan.Clear();
		ComboForecast.Clear();
		ComboPlanningUi? ui = Ui;
		Ui = null;
		ui?.Dispose();
		_exiting = false;
	}
}
[HarmonyPatch(typeof(NCombatUi), "Activate")]
internal static class ComboPlanningAttachPatch
{
	[HarmonyPostfix]
	private static void Postfix(NCombatUi __instance, CombatState state)
	{
		ComboPlanning.Attach(__instance, state);
	}
}
[HarmonyPatch(typeof(NCombatUi), "Deactivate")]
internal static class ComboPlanningDeactivatePatch
{
	[HarmonyPrefix]
	private static void Prefix()
	{
		VisualCleanup.Safely(ComboPlanning.Detach);
	}
}
[HarmonyPatch(typeof(ActionExecutor), "WaitForUnpause")]
internal static class ComboPlanningGatePatch
{
	[HarmonyPostfix]
	private static void Postfix(ActionExecutor __instance, ref Task __result)
	{
		if (ComboPlanning.Holds(__instance))
		{
			__result = ComboPlanning.WaitForRelease(__instance, __result);
		}
	}
}
[HarmonyPatch(typeof(NHitStop), "SetTimeScale")]
internal static class ComboPlanningHitStopPatch
{
	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix()
	{
		ComboPlanning.NativeHitStop();
	}
}
internal sealed class ComboPlanningUi : IDisposable
{
	internal const string ButtonScene = "res://scenes/pause_menu/pause_menu_button.tscn";

	private static readonly string[] EnergyLayers = new string[5] { "res://images/ui/combat/energy_counters/silent/silent_orb_layer_1.png", "res://images/ui/combat/energy_counters/silent/silent_orb_layer_2.png", "res://images/ui/combat/energy_counters/silent/silent_orb_layer_3.png", "res://images/ui/combat/energy_counters/silent/silent_orb_layer_4.png", "res://images/ui/combat/energy_counters/silent/silent_orb_layer_5.png" };

	private readonly SceneTree _tree;

	private readonly NCombatRoom _room;

	private readonly ShaderMaterial _material;

	private Node2D? _dark;

	private (CanvasItem Node, Color Color)[] _darkLayers = Array.Empty<(CanvasItem, Color)>();

	private Tween? _darkTween;

	private ComboTransition? _transition;

	private Tween? _unavailableTween;

	private readonly TextureRect _icon;

	private readonly float _iconX;

	private bool? _lastState;

	private bool _disposed;

	private const string IconShader = "shader_type canvas_item;\r\nuniform sampler2D layer1 : source_color, filter_linear;\r\nuniform sampler2D layer2 : source_color, filter_linear;\r\nuniform sampler2D layer3 : source_color, filter_linear;\r\nuniform sampler2D layer4 : source_color, filter_linear;\r\nuniform sampler2D layer5 : source_color, filter_linear;\r\nuniform float s = 0.8;\r\nuniform float v = 0.9;\r\nuniform float active = 0.0;\r\nvarying vec4 tint;\r\nvoid vertex() { tint = COLOR; }\r\nfloat rounded_box(vec2 p, vec2 extent, float radius) {\r\n    vec2 q = abs(p) - extent + vec2(radius);\r\n    return length(max(q, vec2(0.0))) + min(max(q.x, q.y), 0.0) - radius;\r\n}\r\nfloat segment(vec2 p, vec2 a, vec2 b) {\r\n    vec2 q = p - a;\r\n    vec2 d = b - a;\r\n    return length(q - d * clamp(dot(q, d) / dot(d, d), 0.0, 1.0));\r\n}\r\nvoid fragment() {\r\n    vec2 p = UV - vec2(0.5);\r\n    vec2 uv = p * 0.66 + vec2(0.5);\r\n    vec4 a = texture(layer1, uv);\r\n    vec3 fill = mix(vec3(0.045, 0.28, 0.19), a.rgb, a.a);\r\n    a = texture(layer2, uv); fill = mix(fill, a.rgb, a.a);\r\n    a = texture(layer3, uv); fill = mix(fill, a.rgb, a.a);\r\n    a = texture(layer4, uv); fill = mix(fill, a.rgb, a.a);\r\n    a = texture(layer5, uv); fill = mix(fill, a.rgb, a.a);\r\n    fill = mix(vec3(dot(fill, vec3(0.299, 0.587, 0.114))), fill, s);\r\n    fill *= v * mix(0.78, 1.3, active);\r\n    float outer = rounded_box(p, vec2(0.49), 0.13);\r\n    float inner = rounded_box(p, vec2(0.435), 0.085);\r\n    float aa = max(fwidth(outer), 0.001);\r\n    vec3 rim = mix(vec3(0.025, 0.10, 0.075), vec3(0.04, 0.20, 0.15), active);\r\n    vec3 color = mix(rim, fill, 1.0 - smoothstep(-aa, aa, inner));\r\n    // A simple stopwatch: round dial, short top bar and upright hand.\r\n    float dial = abs(length(p - vec2(0.0, 0.025)) - 0.185);\r\n    float top = segment(p, vec2(-0.064, -0.235), vec2(0.064, -0.235));\r\n    float hand = segment(p, vec2(0.0, 0.03), vec2(0.0, -0.07));\r\n    float mark = min(dial, min(top, hand));\r\n    color = mix(color, vec3(0.025, 0.10, 0.075), 0.8 * (1.0 - smoothstep(0.024-aa, 0.024+aa, mark)));\r\n    color = mix(color, mix(vec3(0.91, 0.95, 0.83), vec3(0.91, 1.0, 0.93), active), 1.0 - smoothstep(0.012-aa, 0.012+aa, mark));\r\n    COLOR = vec4(color, 1.0 - smoothstep(-aa, aa, outer)) * tint;\r\n}";

	internal NCombatUi Native { get; }

	internal Player Player { get; }

	internal NPauseMenuButton Button { get; }

	private bool HasCombatUi
	{
		get
		{
			if (!_disposed && GodotObject.IsInstanceValid((GodotObject)(object)Native) && ((CanvasItem)Native).IsVisibleInTree() && NCombatRoom.Instance == _room)
			{
				return AnimationSettings.Current.Enabled;
			}
			return false;
		}
	}

	private bool InPlayerTurn
	{
		get
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Invalid comparison between Unknown and I4
			if (HasCombatUi && Player.Creature.IsAlive)
			{
				PlayerCombatState playerCombatState = Player.PlayerCombatState;
				if (playerCombatState != null && (int)playerCombatState.Phase == 3 && CombatManager.Instance.IsInProgress)
				{
					return !CombatManager.Instance.IsOverOrEnding;
				}
			}
			return false;
		}
	}

	internal bool Eligible
	{
		get
		{
			if (InPlayerTurn)
			{
				return !CombatManager.Instance.PlayerActionsDisabled;
			}
			return false;
		}
	}

	internal static ComboPlanningUi? Create(NCombatUi ui, CombatState state)
	{
		Player me = LocalContext.GetMe((ICombatState)(object)state);
		if (!(((me != null) ? me.Character : null) is Silent))
		{
			return null;
		}
		return new ComboPlanningUi(ui, me);
	}

	private ComboPlanningUi(NCombatUi ui, Player player)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Expected O, but got Unknown
		//IL_0155: Expected O, but got Unknown
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Expected O, but got Unknown
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Expected O, but got Unknown
		Native = ui;
		Player = player;
		_room = NCombatRoom.Instance;
		_tree = ((Node)ui).GetTree();
		Button = ResourceLoader.Load<PackedScene>("res://scenes/pause_menu/pause_menu_button.tscn", (string)null, (CacheMode)1).Instantiate<NPauseMenuButton>((GenEditState)0);
		((Node)Button).Name = StringName.op_Implicit("MoreSilentBulletTime");
		((Control)Button).Position = new Vector2(140f, 28f);
		((Control)Button).CustomMinimumSize = new Vector2(72f, 72f);
		((Control)Button).Size = new Vector2(72f, 72f);
		((Control)Button).FocusMode = (FocusModeEnum)2;
		TextureRect node = ((Node)Button).GetNode<TextureRect>(NodePath.op_Implicit("ButtonImage"));
		node.ExpandMode = (ExpandModeEnum)1;
		node.StretchMode = (StretchModeEnum)0;
		((Control)node).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		_icon = node;
		_iconX = ((Control)node).Position.X;
		((Control)node).PivotOffset = new Vector2(36f, 36f);
		((Control)node).Scale = new Vector2(0.9f, 0.9f);
		_material = new ShaderMaterial
		{
			Shader = new Shader
			{
				Code = "shader_type canvas_item;\r\nuniform sampler2D layer1 : source_color, filter_linear;\r\nuniform sampler2D layer2 : source_color, filter_linear;\r\nuniform sampler2D layer3 : source_color, filter_linear;\r\nuniform sampler2D layer4 : source_color, filter_linear;\r\nuniform sampler2D layer5 : source_color, filter_linear;\r\nuniform float s = 0.8;\r\nuniform float v = 0.9;\r\nuniform float active = 0.0;\r\nvarying vec4 tint;\r\nvoid vertex() { tint = COLOR; }\r\nfloat rounded_box(vec2 p, vec2 extent, float radius) {\r\n    vec2 q = abs(p) - extent + vec2(radius);\r\n    return length(max(q, vec2(0.0))) + min(max(q.x, q.y), 0.0) - radius;\r\n}\r\nfloat segment(vec2 p, vec2 a, vec2 b) {\r\n    vec2 q = p - a;\r\n    vec2 d = b - a;\r\n    return length(q - d * clamp(dot(q, d) / dot(d, d), 0.0, 1.0));\r\n}\r\nvoid fragment() {\r\n    vec2 p = UV - vec2(0.5);\r\n    vec2 uv = p * 0.66 + vec2(0.5);\r\n    vec4 a = texture(layer1, uv);\r\n    vec3 fill = mix(vec3(0.045, 0.28, 0.19), a.rgb, a.a);\r\n    a = texture(layer2, uv); fill = mix(fill, a.rgb, a.a);\r\n    a = texture(layer3, uv); fill = mix(fill, a.rgb, a.a);\r\n    a = texture(layer4, uv); fill = mix(fill, a.rgb, a.a);\r\n    a = texture(layer5, uv); fill = mix(fill, a.rgb, a.a);\r\n    fill = mix(vec3(dot(fill, vec3(0.299, 0.587, 0.114))), fill, s);\r\n    fill *= v * mix(0.78, 1.3, active);\r\n    float outer = rounded_box(p, vec2(0.49), 0.13);\r\n    float inner = rounded_box(p, vec2(0.435), 0.085);\r\n    float aa = max(fwidth(outer), 0.001);\r\n    vec3 rim = mix(vec3(0.025, 0.10, 0.075), vec3(0.04, 0.20, 0.15), active);\r\n    vec3 color = mix(rim, fill, 1.0 - smoothstep(-aa, aa, inner));\r\n    // A simple stopwatch: round dial, short top bar and upright hand.\r\n    float dial = abs(length(p - vec2(0.0, 0.025)) - 0.185);\r\n    float top = segment(p, vec2(-0.064, -0.235), vec2(0.064, -0.235));\r\n    float hand = segment(p, vec2(0.0, 0.03), vec2(0.0, -0.07));\r\n    float mark = min(dial, min(top, hand));\r\n    color = mix(color, vec3(0.025, 0.10, 0.075), 0.8 * (1.0 - smoothstep(0.024-aa, 0.024+aa, mark)));\r\n    color = mix(color, mix(vec3(0.91, 0.95, 0.83), vec3(0.91, 1.0, 0.93), active), 1.0 - smoothstep(0.012-aa, 0.012+aa, mark));\r\n    COLOR = vec4(color, 1.0 - smoothstep(-aa, aa, outer)) * tint;\r\n}"
			}
		};
		_material.SetShaderParameter(StringName.op_Implicit("s"), Variant.op_Implicit(0.8f));
		_material.SetShaderParameter(StringName.op_Implicit("v"), Variant.op_Implicit(0.9f));
		for (int i = 1; i <= 5; i++)
		{
			_material.SetShaderParameter(StringName.op_Implicit("layer" + i), Variant.op_Implicit((GodotObject)(object)ResourceLoader.Load<Texture2D>(EnergyLayers[i - 1], (string)null, (CacheMode)1)));
		}
		((CanvasItem)node).Material = (Material)(object)_material;
		Label node2 = ((Node)Button).GetNode<Label>(NodePath.op_Implicit("Label"));
		node2.Text = "";
		((CanvasItem)node2).Visible = false;
		((Node)ui.EnergyCounterContainer).AddChild((Node)(object)Button, false, (InternalMode)0);
		((GodotObject)Button).SetDeferred(PropertyName.Size, Variant.op_Implicit(new Vector2(72f, 72f)));
		((NClickableControl)Button).Released += new ReleasedEventHandler(OnReleased);
		NHotkeyManager.Instance.PushHotkeyReleasedBinding("more_silent_bullet_time", (Action)OnHotkey);
		NInputManager.Instance.InputRebound += new InputReboundEventHandler(OnBindingChanged);
		((Node)ui).TreeExiting += OnExit;
		_tree.ProcessFrame += OnFrame;
		Refresh();
	}

	private void OnReleased(NClickableControl _)
	{
		try
		{
			if (ComboPlanning.Toggle())
			{
				ClearUnavailable();
			}
			else if (HasCombatUi && ((CanvasItem)Button).IsVisibleInTree())
			{
				ShowUnavailable();
			}
		}
		catch (Exception error)
		{
			ComboPlanning.Release(cancel: true);
			ModEntry.Log(error);
		}
	}

	private void OnHotkey()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (!HasCombatUi)
		{
			return;
		}
		NModalContainer instance = NModalContainer.Instance;
		if (((instance != null) ? instance.OpenModal : null) != null)
		{
			return;
		}
		if (!((CanvasItem)Button).Visible)
		{
			OnReleased((NClickableControl)(object)Button);
			return;
		}
		((GodotObject)Button).Call(StringName.op_Implicit("OnPressHandler"), Array.Empty<Variant>());
		((GodotObject)Button).Call(StringName.op_Implicit("OnReleaseHandler"), Array.Empty<Variant>());
		((Tweener)((Node)Button).CreateTween().SetIgnoreTimeScale(true).TweenInterval(0.08)).Finished += delegate
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			NPauseMenuButton button = Button;
			Variant val = ((GodotObject)Button).Get(StringName.op_Implicit("IsFocused"));
			((GodotObject)button).Call(StringName.op_Implicit(((Variant)(ref val)).AsBool() ? "OnFocus" : "OnUnfocus"), Array.Empty<Variant>());
		};
	}

	private void OnBindingChanged()
	{
		_lastState = null;
		Refresh();
	}

	private void OnExit()
	{
		ComboPlanning.Detach();
	}

	private void ShowUnavailable()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		ClearUnavailable();
		_unavailableTween = ((Node)Button).CreateTween().SetIgnoreTimeScale(true);
		_unavailableTween.TweenProperty((GodotObject)(object)_icon, NodePath.op_Implicit("position:x"), Variant.op_Implicit(_iconX + 4f), 0.04);
		_unavailableTween.TweenProperty((GodotObject)(object)_icon, NodePath.op_Implicit("position:x"), Variant.op_Implicit(_iconX - 4f), 0.06);
		_unavailableTween.TweenProperty((GodotObject)(object)_icon, NodePath.op_Implicit("position:x"), Variant.op_Implicit(_iconX + 3f), 0.06);
		_unavailableTween.TweenProperty((GodotObject)(object)_icon, NodePath.op_Implicit("position:x"), Variant.op_Implicit(_iconX), 0.04);
		_unavailableTween.Finished += delegate
		{
			_unavailableTween = null;
		};
	}

	private void ClearUnavailable()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (_unavailableTween != null)
		{
			_unavailableTween.Kill();
			_unavailableTween = null;
			if (GodotObject.IsInstanceValid((GodotObject)(object)_icon))
			{
				((Control)_icon).Position = new Vector2(_iconX, ((Control)_icon).Position.Y);
			}
		}
	}

	private void OnFrame()
	{
		try
		{
			ComboPlanning.Tick();
			_transition?.Update();
			Refresh();
		}
		catch (Exception error)
		{
			VisualCleanup.Safely(ComboPlanning.Detach);
			ModEntry.Log(error);
		}
	}

	internal void Refresh()
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (!_disposed && GodotObject.IsInstanceValid((GodotObject)(object)Button))
		{
			((CanvasItem)Button).Visible = AnimationSettings.Current.Enabled && AnimationSettings.Current.ShowBulletTimeButton;
			((NClickableControl)Button).SetEnabled(true);
			if (!HasCombatUi || !((CanvasItem)Button).Visible)
			{
				ClearUnavailable();
			}
			bool active = ComboPlanning.Active;
			if (_lastState != active)
			{
				_lastState = active;
				_material.SetShaderParameter(StringName.op_Implicit("active"), Variant.op_Implicit(ComboPlanning.Active ? 1f : 0f));
			}
		}
	}

	internal void SetDark(bool active)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		if (_dark == null && active)
		{
			_dark = CreateDark();
			((Node)_dark).Name = StringName.op_Implicit("MoreSilentBulletTimeDark");
			((CanvasItem)_dark).Modulate = new Color(1f, 1f, 1f, 0f);
			_dark.Position = new Vector2(((Control)Native).Size.X / 2f, 0f);
			_darkLayers = (from n in ((IEnumerable)((Node)_dark).GetChildren(false)).OfType<CanvasItem>()
				select (n: n, SelfModulate: n.SelfModulate)).ToArray();
			RefreshIntensity();
			((Node)Native).AddChild((Node)(object)_dark, false, (InternalMode)0);
			((Node)Native).MoveChild((Node)(object)_dark, 0);
		}
		if (_dark != null)
		{
			Tween? darkTween = _darkTween;
			if (darkTween != null)
			{
				darkTween.Kill();
			}
			_darkTween = ((Node)_dark).CreateTween().SetIgnoreTimeScale(true);
			_darkTween.TweenProperty((GodotObject)(object)_dark, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(active ? 1f : 0f), active ? 1.0 : 0.5);
		}
	}

	internal static void WarmEffects(SubViewport viewport)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		Node2D val = CreateDark();
		val.Position = new Vector2(8f, 8f);
		val.Scale = Vector2.One * 0.01f;
		((Node)viewport).AddChild((Node)(object)val, false, (InternalMode)0);
		((Node)viewport).AddChild((Node)new BackBufferCopy
		{
			CopyMode = (CopyModeEnum)2
		}, false, (InternalMode)0);
		ShaderMaterial material;
		Sprite2D val2 = ComboTransition.CreateRing(out material);
		((Node2D)val2).Position = new Vector2(8f, 8f);
		((Node2D)val2).Scale = Vector2.One * (16f / (float)val2.Texture.GetWidth());
		((Node)viewport).AddChild((Node)(object)val2, false, (InternalMode)0);
	}

	private static Node2D CreateDark()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		Node2D root = new Node2D();
		try
		{
			Add("add", "grand_finale_spotlight.png", 4f / 85f);
			Add("darken", "grand_finale_spotlight_inverted.png", 0.1254902f);
			return root;
		}
		catch
		{
			((GodotObject)root).Free();
			throw;
		}
		void Add(string layer, string texture, float alpha)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Expected O, but got Unknown
			Material material = ResourceLoader.Load<Material>("res://materials/vfx/grand_finale/vfx_grand_finale_spotlight_" + layer + ".tres", (string)null, (CacheMode)1);
			Texture2D texture2 = ResourceLoader.Load<Texture2D>("res://images/vfx/grand_finale/" + texture, (string)null, (CacheMode)1);
			((Node)root).AddChild((Node)new Sprite2D
			{
				Name = StringName.op_Implicit("vfx_grand_finale_spotlight_" + layer),
				SelfModulate = new Color(1f, 1f, 1f, alpha),
				Material = material,
				Scale = new Vector2(4.5f, 4.5f),
				Texture = texture2,
				Offset = new Vector2(0f, 128f)
			}, false, (InternalMode)0);
		}
	}

	internal void RefreshIntensity()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (!_disposed)
		{
			float num = 2f;
			(CanvasItem, Color)[] darkLayers = _darkLayers;
			for (int i = 0; i < darkLayers.Length; i++)
			{
				(CanvasItem, Color) tuple = darkLayers[i];
				CanvasItem item = tuple.Item1;
				Color item2 = tuple.Item2;
				float num2 = (((object)((Node)item).Name).ToString().Contains("darken", StringComparison.Ordinal) ? num : Math.Min(num, 1f));
				item.SelfModulate = new Color(item2.R, item2.G, item2.B, item2.A * num2);
			}
			_transition?.SetStrength();
		}
	}

	internal void PlayTransition(bool entering)
	{
		if (_disposed)
		{
			return;
		}
		VisualCleanup.Safely(delegate
		{
			SfxCmd.Play(entering ? "event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_intangible" : "event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_reappear", 1f);
		});
		VisualCleanup.Safely(delegate
		{
			if (_transition == null)
			{
				_transition = new ComboTransition(Native, Player);
			}
			_transition.Play(entering);
		});
	}

	internal void StopTransition()
	{
		_transition?.Stop();
	}

	public void Dispose()
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		if (!_disposed)
		{
			_disposed = true;
			ClearUnavailable();
			Tween? darkTween = _darkTween;
			if (darkTween != null)
			{
				darkTween.Kill();
			}
			_transition?.Dispose();
			if (_dark != null && GodotObject.IsInstanceValid((GodotObject)(object)_dark))
			{
				((Node)_dark).QueueFree();
			}
			NHotkeyManager instance = NHotkeyManager.Instance;
			if (instance != null)
			{
				instance.RemoveHotkeyReleasedBinding("more_silent_bullet_time", (Action)OnHotkey);
			}
			NInputManager instance2 = NInputManager.Instance;
			if (instance2 != null)
			{
				instance2.InputRebound -= new InputReboundEventHandler(OnBindingChanged);
			}
			_tree.ProcessFrame -= OnFrame;
			if (GodotObject.IsInstanceValid((GodotObject)(object)Native))
			{
				((Node)Native).TreeExiting -= OnExit;
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)Button))
			{
				((NClickableControl)Button).Released -= new ReleasedEventHandler(OnReleased);
				((Node)Button).QueueFree();
			}
		}
	}
}
[HarmonyPatch]
internal static class ComboPlanningButtonFeedbackPatch
{
	private static readonly FieldInfo TweenField = AccessTools.Field(typeof(NPauseMenuButton), "_tween");

	private static IEnumerable<MethodBase> TargetMethods()
	{
		return new string[3] { "OnFocus", "OnUnfocus", "OnPress" }.Select((string n) => AccessTools.Method(typeof(NPauseMenuButton), n, (Type[])null, (Type[])null));
	}

	[HarmonyPostfix]
	private static void Postfix(NPauseMenuButton __instance)
	{
		if (__instance == ComboPlanning.Ui?.Button)
		{
			object? value = TweenField.GetValue(__instance);
			Tween val = (Tween)((value is Tween) ? value : null);
			if (val != null)
			{
				val.SetIgnoreTimeScale(true);
			}
		}
	}
}
[HarmonyPatch(typeof(NCardPlayQueue), "TweenCardToQueuePosition")]
internal static class ComboPlanningQueueFeedbackPatch
{
	private static readonly FieldInfo TweenField = AccessTools.Field(AccessTools.Inner(typeof(NCardPlayQueue), "QueueItem"), "currentTween");

	[HarmonyPostfix]
	private static void Postfix(object item)
	{
		if (ComboPlanning.Active)
		{
			object? value = TweenField.GetValue(item);
			Tween val = (Tween)((value is Tween) ? value : null);
			if (val != null)
			{
				val.SetIgnoreTimeScale(true);
			}
		}
	}
}
internal static class ComboShivPlan
{
	internal sealed class Source(PlayCardAction action, CardModel card)
	{
		internal readonly PlayCardAction Action = action;

		internal readonly CardModel Card = card;

		internal readonly List<CardModel> Generated = new List<CardModel>();

		internal List<CardModel>? FinalGenerated;

		internal int Repeats = 1;

		internal bool Done;
	}

	internal sealed class Knife(Source source, int index, CardModel preview)
	{
		internal readonly Source Source = source;

		internal readonly int Index = index;

		internal readonly CardModel Preview = preview;

		internal Control? View;

		internal int HandIndex;

		internal NCard? Node;

		internal Creature? Target;

		internal bool Selected;
	}

	private sealed record Intent(GameAction? Action, Knife? Knife);

	private static readonly Dictionary<PlayCardAction, Source> Sources = new Dictionary<PlayCardAction, Source>();

	private static readonly List<Knife> Knives = new List<Knife>();

	private static readonly List<Intent> Deferred = new List<Intent>();

	private static readonly HashSet<GameAction> Displayed = new HashSet<GameAction>();

	private static readonly List<CardModel> VisualOrder = new List<CardModel>();

	private static PlayCardAction? _lastNative;

	private static PlayCardAction? _anchor;

	private static bool _defer;

	private static bool _sending;

	private static bool _pumping;

	internal static bool Playing { get; private set; }

	internal static bool Targeting => ComboNativeHand.Targeting;

	internal static IReadOnlyList<Knife> Previews
	{
		get
		{
			if (!ComboLocalPlan.InUse)
			{
				return Knives;
			}
			return ComboLocalPlan.Previews;
		}
	}

	internal static int DeferredCount => Deferred.Count;

	internal static Source? CurrentSource
	{
		get
		{
			Source source = ComboLocalPlan.CurrentSource;
			if (source == null)
			{
				source = ComboChoiceContinuation.CurrentSource;
				if (source == null)
				{
					if (Sources.Count > 0)
					{
						GameAction currentlyRunningAction = RunManager.Instance.ActionExecutor.CurrentlyRunningAction;
						PlayCardAction val = (PlayCardAction)(object)((currentlyRunningAction is PlayCardAction) ? currentlyRunningAction : null);
						if (val != null && Sources.TryGetValue(val, out Source value))
						{
							source = value;
							goto IL_0049;
						}
					}
					return null;
				}
			}
			goto IL_0049;
			IL_0049:
			return source;
		}
	}

	internal static void Begin()
	{
		Clear(cancel: true);
	}

	internal static void NativeQueued(PlayCardAction action)
	{
		_lastNative = action;
		Register(action);
	}

	private static void Register(PlayCardAction action)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		NetCombatCard netCombatCard = action.NetCombatCard;
		CardModel val = ((NetCombatCard)(ref netCombatCard)).ToCardModelOrNull();
		if (val == null)
		{
			return;
		}
		VisualOrder.Add(val);
		if (!Sources.ContainsKey(action))
		{
			Source source = new Source(action, val)
			{
				Repeats = ComboForecast.Record(val)
			};
			List<Knife> list = CreatePreviews(source);
			ComboChoiceContinuation.Record(action, (list.Count > 0) ? source : null);
			if (list.Count != 0)
			{
				Sources.Add(action, source);
				((GameAction)action).AfterFinished += OnSourceFinished;
				((GameAction)action).BeforeCancelled += OnSourceCancelled;
				Knives.AddRange(list);
				LayoutViews();
			}
		}
	}

	internal static List<Knife> CreatePreviews(Source source)
	{
		CardModel card = source.card;
		List<Knife> list = new List<Knife>();
		int num = ((!(card is LeadingStrike) && !(card is FanOfKnives) && !(card is HiddenDaggers)) ? ((!(card is BladeOfInk) && !(card is BladeDance) && !(card is CloakAndDagger) && !(card is UpMySleeve)) ? ((card is StormOfSteel) ? ComboForecast.KnownHandBefore(card) : 0) : ((DynamicVar)card.DynamicVars.Cards).IntValue) : card.DynamicVars["Shivs"].IntValue);
		int num2 = num;
		if (!(card is StormOfSteel))
		{
			num2 = (int)Math.Min(60L, (long)num2 * (long)source.Repeats);
		}
		num2 = Math.Clamp(num2, 0, Math.Max(0, 60 - Previews.Count));
		for (int i = 0; i < num2; i++)
		{
			CardModel val = ((CardModel)ModelDb.Card<Shiv>()).ToMutable();
			val.Owner = card.Owner;
			if (card is BladeOfInk)
			{
				val.EnchantInternal(((EnchantmentModel)ModelDb.Enchantment<Inky>()).ToMutable(), 1m);
			}
			bool flag = card.IsUpgraded;
			if (flag)
			{
				bool flag2 = ((card is StormOfSteel || card is HiddenDaggers) ? true : false);
				flag = flag2;
			}
			if (flag)
			{
				val.UpgradeInternal();
				val.FinalizeUpgradeInternal();
			}
			Knife item = new Knife(source, i, val);
			list.Add(item);
		}
		return list;
	}

	internal static CardModel? Resolve(Knife knife)
	{
		Source source = knife.source;
		if (!(source.card is StormOfSteel))
		{
			return source.Generated.ElementAtOrDefault(knife.index);
		}
		if (!source.Done)
		{
			return null;
		}
		Source source2 = source;
		if (source2.FinalGenerated == null)
		{
			source2.FinalGenerated = source.Generated.Where(delegate(CardModel c)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Invalid comparison between Unknown and I4
				CardPile pile = c.Pile;
				return pile != null && (int)pile.Type == 2;
			}).ToList();
		}
		return source.FinalGenerated.ElementAtOrDefault(knife.index);
	}

	internal static Knife? Find(CardModel? card)
	{
		if (card != null)
		{
			Knife knife = ComboLocalPlan.Find(card);
			if (knife != null)
			{
				return knife;
			}
		}
		if (card != null)
		{
			for (int i = 0; i < Knives.Count; i++)
			{
				if (Knives[i].preview == card)
				{
					return Knives[i];
				}
			}
		}
		return null;
	}

	internal static void RefreshHand()
	{
		ComboNativeHand.Refresh(Previews);
	}

	private static void LayoutViews()
	{
		foreach (Knife knife in Knives)
		{
			if (knife.Selected)
			{
				ComboNativeHand.Queue(knife);
			}
		}
		RefreshHand();
	}

	internal static bool SelectForTarget(Knife knife, Creature? target)
	{
		if (ComboDiscardPlan.HandleHand(knife.preview))
		{
			return false;
		}
		if (ComboLocalPlan.InUse)
		{
			return ComboLocalPlan.Select(knife, target);
		}
		if (!ComboPlanning.Active || knife.Selected || knife.source.Done || !knife.preview.IsValidTarget(target))
		{
			return false;
		}
		if (!_defer)
		{
			_defer = true;
			_anchor = _lastNative;
			if (_anchor != null)
			{
				((GameAction)_anchor).AfterFinished += OnAnchorFinished;
				((GameAction)_anchor).BeforeCancelled += OnAnchorFinished;
			}
		}
		knife.Target = target;
		knife.Selected = true;
		Deferred.Add(new Intent(null, knife));
		ComboChoiceContinuation.Record(knife);
		VisualOrder.Add(knife.preview);
		ComboForecast.Record(knife.preview);
		LayoutViews();
		return true;
	}

	internal static bool DeferRequest(GameAction action)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Invalid comparison between Unknown and I4
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		if (ComboEndTurn.Hold(action))
		{
			return true;
		}
		ComboLocalPlan.BeforeRequest(action);
		ComboChoiceContinuation.BeforeRequest(action, _sending);
		if (ComboPlanning.Ui != null && ComboPlanning.Network)
		{
			return false;
		}
		if (!_sending && _defer && (ComboPlanning.Active || Playing))
		{
			ulong ownerId = action.OwnerId;
			ComboPlanningUi? ui = ComboPlanning.Ui;
			if (ownerId == ((ui != null) ? new ulong?(ui.Player.NetId) : ((ulong?)null)) && ActionQueueSet.IsGameActionPlayerDriven(action))
			{
				GameActionType actionType = action.ActionType;
				if (actionType - 1 > 1)
				{
					return false;
				}
				Deferred.Add(new Intent(action, null));
				PlayCardAction val = (PlayCardAction)(object)((action is PlayCardAction) ? action : null);
				if (val != null)
				{
					if (ComboPlanning.Active)
					{
						Register(val);
					}
					NetCombatCard netCombatCard = val.NetCombatCard;
					CardModel val2 = ((NetCombatCard)(ref netCombatCard)).ToCardModelOrNull();
					if (val2 != null)
					{
						NCardPlayQueue instance = NCardPlayQueue.Instance;
						if (instance != null)
						{
							Displayed.Add(action);
							NPlayerHand instance2 = NPlayerHand.Instance;
							instance.OnLocalCardPlayed(val, (instance2 != null) ? instance2.GetCardHolder(val2) : null, val2);
						}
					}
				}
				else if (ComboPlanning.Active)
				{
					ComboPlanning.Release();
				}
				Pump();
				return true;
			}
		}
		return false;
	}

	internal static bool SkipDuplicateDisplay(GameAction action)
	{
		return Displayed.Remove(action);
	}

	internal static int DisplayIndex(CardModel? card, int fallback)
	{
		if (!ComboLocalPlan.InUse || card == null)
		{
			if (ComboPlanning.Active && card != null)
			{
				int num = VisualOrder.IndexOf(card);
				if (num >= 0)
				{
					return num;
				}
			}
			return fallback;
		}
		return ComboLocalPlan.DisplayIndex(card, fallback);
	}

	internal static void Release(bool cancel)
	{
		if (Targeting)
		{
			NTargetManager.Instance.CancelTargeting();
		}
		foreach (Knife knife in Knives)
		{
			FreeView(knife);
		}
		if (cancel || !_defer)
		{
			Clear(cancel);
			return;
		}
		Playing = true;
		Pump();
	}

	private static bool Finished(GameAction? action)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		bool flag = action == null;
		if (!flag)
		{
			GameActionState state = action.State;
			bool flag2 = state - 5 <= 1;
			flag = flag2;
		}
		return flag;
	}

	private static void OnAnchorFinished(GameAction _)
	{
		Pump();
	}

	private static void OnSourceFinished(GameAction action)
	{
		PlayCardAction val = (PlayCardAction)(object)((action is PlayCardAction) ? action : null);
		if (val != null && Sources.TryGetValue(val, out Source value))
		{
			value.Done = true;
		}
		Pump();
	}

	private static void OnSourceCancelled(GameAction action)
	{
		PlayCardAction val = (PlayCardAction)(object)((action is PlayCardAction) ? action : null);
		if (val != null && Sources.TryGetValue(val, out Source source))
		{
			source.Done = true;
			foreach (Knife item in Knives.Where((Knife k) => k.source == source))
			{
				FreeView(item);
			}
		}
		Pump();
	}

	private static void Pump()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Invalid comparison between Unknown and I4
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Invalid comparison between Unknown and I4
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Invalid comparison between Unknown and I4
		if (!Playing || ComboPlanning.Active || _pumping || !Finished((GameAction?)(object)_anchor))
		{
			return;
		}
		_pumping = true;
		try
		{
			while (Deferred.Count > 0)
			{
				Intent intent = Deferred[0];
				Knife knife = intent.Knife;
				if (knife != null)
				{
					Source source = knife.source;
					if (source != null && !source.Done)
					{
						break;
					}
				}
				Deferred.RemoveAt(0);
				_sending = true;
				try
				{
					GameAction action = intent.Action;
					if (action != null)
					{
						if ((int)action.State == 0)
						{
							RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(action);
						}
						continue;
					}
					Knife knife2 = intent.Knife;
					if (knife2 == null)
					{
						continue;
					}
					CardModel val = Resolve(knife2);
					if (val != null)
					{
						CardPile pile = val.Pile;
						if (pile != null && (int)pile.Type == 2)
						{
							CardModel val2 = val;
							TargetType targetType = val.TargetType;
							bool flag = (((int)targetType == 2 || (int)targetType == 6) ? true : false);
							val2.TryManualPlay(flag ? knife2.Target : null);
						}
					}
				}
				finally
				{
					_sending = false;
				}
			}
			if (Deferred.Count == 0)
			{
				Clear(cancel: false);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			Clear(cancel: true);
		}
		finally
		{
			_pumping = false;
		}
	}

	private static void FreeView(Knife knife)
	{
		ComboNativeHand.Free(knife);
	}

	internal static void Clear(bool cancel, bool preserveNonCardActions = false)
	{
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		if (Targeting)
		{
			NTargetManager.Instance.CancelTargeting();
		}
		Playing = false;
		_defer = false;
		if (_anchor != null)
		{
			((GameAction)_anchor).AfterFinished -= OnAnchorFinished;
			((GameAction)_anchor).BeforeCancelled -= OnAnchorFinished;
		}
		_anchor = null;
		_lastNative = null;
		foreach (Source value in Sources.Values)
		{
			((GameAction)value.action).AfterFinished -= OnSourceFinished;
			((GameAction)value.action).BeforeCancelled -= OnSourceCancelled;
		}
		Sources.Clear();
		foreach (Knife knife in Knives)
		{
			FreeView(knife);
		}
		Knives.Clear();
		VisualOrder.Clear();
		Intent[] array = Deferred.ToArray();
		Deferred.Clear();
		if (cancel)
		{
			Intent[] array2 = array;
			foreach (Intent intent in array2)
			{
				GameAction action = intent.Action;
				if (action == null || (int)action.State != 0)
				{
					continue;
				}
				if (preserveNonCardActions && !(action is PlayCardAction))
				{
					VisualCleanup.Safely(delegate
					{
						RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(action);
					});
				}
				else
				{
					VisualCleanup.Safely((Action)action.Cancel);
				}
			}
		}
		Displayed.Clear();
	}
}
[HarmonyPatch(typeof(ActionQueueSynchronizer), "RequestEnqueue")]
internal static class ComboShivRequestPatch
{
	[HarmonyPrefix]
	private static bool Prefix(GameAction action)
	{
		return !ComboShivPlan.DeferRequest(action);
	}
}
[HarmonyPatch(typeof(NCardPlayQueue), "OnActionEnqueued")]
internal static class ComboShivQueueDisplayPatch
{
	[HarmonyPrefix]
	private static bool Prefix(GameAction action)
	{
		return !ComboShivPlan.SkipDuplicateDisplay(action);
	}
}
[HarmonyPatch(typeof(NCardPlayQueue), "TweenCardToQueuePosition")]
internal static class ComboShivQueueIndexPatch
{
	private static readonly FieldInfo CardField = AccessTools.Field(AccessTools.Inner(typeof(NCardPlayQueue), "QueueItem"), "card");

	[HarmonyPrefix]
	private static void Prefix(object item, ref int queueIndex)
	{
		if (ComboPlanning.Active)
		{
			object? value = CardField.GetValue(item);
			object? obj = ((value is NCard) ? value : null);
			queueIndex = ComboShivPlan.DisplayIndex((obj != null) ? ((NCard)obj).Model : null, queueIndex);
		}
	}
}
[HarmonyPatch(typeof(ActionQueueSet), "PauseActionForPlayerChoice")]
internal static class ComboShivChoicePatch
{
	[HarmonyPrefix]
	private static void Prefix(GameAction action, PlayerChoiceOptions options)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!((Enum)options).HasFlag((Enum)(object)(PlayerChoiceOptions)1))
		{
			return;
		}
		ComboPlanningUi ui = ComboPlanning.Ui;
		if (ui != null && ComboPlanning.Network)
		{
			if (ComboLocalPlan.InUse && action.OwnerId == ui.Player.NetId)
			{
				ComboPlanning.Release(cancel: true);
			}
		}
		else
		{
			ComboChoiceContinuation.PauseForChoice(action);
			ComboShivPlan.Clear(cancel: true, preserveNonCardActions: true);
		}
	}
}
[HarmonyPatch]
internal static class ComboShivCreatedPatch
{
	private static readonly AsyncLocal<int> Depth = new AsyncLocal<int>();

	private static MethodBase TargetMethod()
	{
		return typeof(Shiv).GetMethods().Single((MethodInfo m) => m.Name == "CreateInHand" && m.GetParameters().Length >= 3 && m.GetParameters()[1].ParameterType == typeof(int));
	}

	[HarmonyPrefix]
	private static void Prefix(out (int Depth, ComboShivPlan.Source? Source) __state)
	{
		__state = (Depth: Depth.Value, Source: (Depth.Value == 0) ? ComboShivPlan.CurrentSource : null);
		if (__state.Source != null)
		{
			Depth.Value++;
		}
	}

	[HarmonyPostfix]
	private static void Postfix(ref Task<IEnumerable<CardModel>> __result, (int Depth, ComboShivPlan.Source? Source) __state)
	{
		ComboShivPlan.Source item = __state.Source;
		if (item != null)
		{
			Depth.Value = __state.Depth;
			__result = Capture(__result, item);
		}
	}

	private static async Task<IEnumerable<CardModel>> Capture(Task<IEnumerable<CardModel>> original, ComboShivPlan.Source source)
	{
		IEnumerable<CardModel> enumerable = await original;
		source.Generated.AddRange(enumerable.Where((CardModel c) => c.Owner == source.card.Owner));
		return enumerable;
	}
}
internal static class ComboSpeedCompatibility
{
	private static bool _installed;

	internal static void InstallIfAvailable()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00e6: Expected O, but got Unknown
		if (_installed)
		{
			return;
		}
		Type type = (from a in AppDomain.CurrentDomain.GetAssemblies()
			select a.GetType("STS2QuickAnimationMode.Utils.SpeedManager")).FirstOrDefault((Type t) => (object)t != null);
		if ((object)type == null)
		{
			return;
		}
		MethodInfo methodInfo = AccessTools.DeclaredMethod(type, "ApplySpeed", Type.EmptyTypes, (Type[])null);
		MethodInfo methodInfo2 = AccessTools.DeclaredMethod(type, "ObserveExternalTimeScale", Type.EmptyTypes, (Type[])null);
		if ((object)methodInfo == null || !methodInfo.IsStatic || methodInfo.ReturnType != typeof(void) || (object)methodInfo2 == null)
		{
			return;
		}
		try
		{
			new Harmony("page.more_silent_animations").Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(ComboSpeedCompatibility), "Before", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(ComboSpeedCompatibility), "After", (Type[])null));
			_installed = true;
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	private static void Before(out bool __state)
	{
		__state = ComboPlanning.BeforeExternalClockUpdate();
	}

	private static void After(bool __state)
	{
		if (__state)
		{
			ComboPlanning.AfterExternalClockUpdate();
		}
	}
}
internal sealed class ComboTransition : IDisposable
{
	internal const string ScenePath = "res://scenes/vfx/distortions/vfx_outward_screen_distortion.tscn";

	internal const string EnterSound = "event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_intangible";

	internal const string ExitSound = "event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_reappear";

	private readonly NCombatUi _ui;

	private readonly Player _player;

	private readonly Node2D _root;

	private readonly Sprite2D _ring;

	private readonly ShaderMaterial _material;

	private readonly float _nativeIntensity;

	private readonly Curve _scaleCurve;

	private readonly Curve _alphaCurve;

	private readonly float _nativeScale;

	private ulong _started;

	private double _duration;

	private float _extent;

	private static readonly Curve ScaleCurve = CreateScaleCurve();

	private static readonly Curve AlphaCurve = CreateAlphaCurve();

	private static Curve CreateScaleCurve()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		Curve val = new Curve();
		val.AddPoint(new Vector2(0f, 0.25144f), 0f, 5.73351f, (TangentMode)0, (TangentMode)0);
		val.AddPoint(new Vector2(0.250646f, 0.850288f), 0f, 0.216437f, (TangentMode)0, (TangentMode)0);
		val.AddPoint(new Vector2(1f, 1f), 0.19707f, 0f, (TangentMode)0, (TangentMode)0);
		return val;
	}

	private static Curve CreateAlphaCurve()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		Curve val = new Curve();
		val.AddPoint(new Vector2(0f, 1f), 0f, -1.75571f, (TangentMode)0, (TangentMode)0);
		val.AddPoint(new Vector2(1f, 0f), 0f, 0f, (TangentMode)0, (TangentMode)0);
		return val;
	}

	internal ComboTransition(NCombatUi ui, Player player)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		_ui = ui;
		_player = player;
		_scaleCurve = ScaleCurve;
		_alphaCurve = AlphaCurve;
		_nativeScale = 15f;
		_ring = CreateRing(out _material);
		Variant shaderParameter = _material.GetShaderParameter(StringName.op_Implicit("distortion_base_intensity"));
		_nativeIntensity = ((Variant)(ref shaderParameter)).AsSingle();
		_root = new Node2D
		{
			Name = StringName.op_Implicit("MoreSilentBulletTimeRipple"),
			Visible = false
		};
		((Node)_root).AddChild((Node)new BackBufferCopy
		{
			Name = StringName.op_Implicit("BattlefieldCopy"),
			CopyMode = (CopyModeEnum)2
		}, false, (InternalMode)0);
		((Node)_root).AddChild((Node)(object)_ring, false, (InternalMode)0);
		((Node)ui).AddChild((Node)(object)_root, false, (InternalMode)0);
		Node2D root = _root;
		Node2D nodeOrNull = ((Node)ui).GetNodeOrNull<Node2D>(NodePath.op_Implicit("MoreSilentBulletTimeDark"));
		((Node)ui).MoveChild((Node)(object)root, (nodeOrNull != null) ? (((Node)nodeOrNull).GetIndex(false) + 1) : 0);
	}

	internal static Sprite2D CreateRing(out ShaderMaterial material)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		material = (ShaderMaterial)((Resource)ResourceLoader.Load<ShaderMaterial>("res://materials/vfx/distortion/vfx_screen_distortion_ring.tres", (string)null, (CacheMode)1)).Duplicate(false);
		return new Sprite2D
		{
			Name = StringName.op_Implicit("NativeRing"),
			Texture = ResourceLoader.Load<Texture2D>("res://images/vfx/distortion/distortion_ring.png", (string)null, (CacheMode)1),
			Material = (Material)(object)material
		};
	}

	internal void Play(bool entering)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		SetStrength();
		NCreature creatureNode = _player.Creature.GetCreatureNode();
		Vector2 val = ((creatureNode != null) ? creatureNode.VfxSpawnPosition : (((Control)_ui).GlobalPosition + ((Control)_ui).Size / 2f));
		Sprite2D ring = _ring;
		Transform2D globalTransform = ((CanvasItem)_ui).GetGlobalTransform();
		((Node2D)ring).Position = ((Transform2D)(ref globalTransform)).AffineInverse() * val;
		float num = 0f;
		Vector2[] array = (Vector2[])(object)new Vector2[4]
		{
			Vector2.Zero,
			new Vector2(((Control)_ui).Size.X, 0f),
			((Control)_ui).Size,
			new Vector2(0f, ((Control)_ui).Size.Y)
		};
		for (int i = 0; i < array.Length; i++)
		{
			Vector2 val2 = array[i];
			num = Math.Max(num, ((Vector2)(ref val2)).DistanceTo(((Node2D)_ring).Position));
		}
		_extent = Math.Max(_nativeScale, 2.2f * num / (float)_ring.Texture.GetWidth());
		_duration = (entering ? 0.45 : 0.3);
		_started = Time.GetTicksUsec();
		((CanvasItem)_root).Visible = true;
		Update();
	}

	internal void SetStrength()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		_material.SetShaderParameter(StringName.op_Implicit("distortion_base_intensity"), Variant.op_Implicit(_nativeIntensity * 250f / 100f));
	}

	internal void Update()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (((CanvasItem)_root).Visible)
		{
			float num = (float)((double)(Time.GetTicksUsec() - _started) / 1000000.0 / _duration);
			if (num >= 1f)
			{
				Stop();
				return;
			}
			((Node2D)_ring).Scale = Vector2.One * (_extent * _scaleCurve.Sample(num));
			((CanvasItem)_ring).SelfModulate = new Color(1f, 1f, 1f, _alphaCurve.Sample(num));
		}
	}

	internal void Stop()
	{
		((CanvasItem)_root).Visible = false;
	}

	public void Dispose()
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			Stop();
			((Node)_root).QueueFree();
		}
	}
}
internal sealed class ActorPresentation : IDisposable
{
	private static readonly Dictionary<NCreature, ActorPresentation> Active = new Dictionary<NCreature, ActorPresentation>(ReferenceEqualityComparer.Instance);

	private readonly NCreature _actor;

	private readonly NCombatRoom _room;

	private readonly SceneTree _tree;

	private readonly object? _combat;

	private readonly DisplayLease _display;

	private readonly MotionBank _motion;

	private readonly AfterimageTrail _shadows;

	private readonly DepartureTrail _departures;

	private readonly ThrustSurge _surges;

	private readonly DownStrikeEffects _downEffects;

	private ThrustSurge.Burst? _surge;

	private DownStrikeEffects.Burst? _downBurst;

	private NativePose? _downLines;

	private readonly List<PunchGhost> _punchGhosts = new List<PunchGhost>();

	private readonly List<FinaleGhost> _finaleGhosts = new List<FinaleGhost>();

	private string _card = "";

	private ulong _lastFrame;

	private bool _disposed;

	private bool _dead;

	private bool _expectAttack;

	private bool _replacedReturnAttack;

	private double _clock;

	private double _started;

	private double _hitAt;

	private double _teleportAt;

	private double _releaseAt;

	private double _upperAt;

	private double _deathAt;

	private bool _hit;

	private bool _departed;

	private bool _returnStartup;

	private bool _down;

	private bool _nativeAttack;

	private bool _finisher;

	private bool _echoingSlash;

	private int _facing = 1;

	private int _attackFacing = 1;

	private double _poseHit;

	private double _clipDuration;

	private string _clip = "";

	private string _upper = "";

	private string _groundShiv = "";

	private bool _steel;

	private string _special = "";

	private double _destination;

	private double _x;

	private double _landingAt = double.NegativeInfinity;

	private float _upperFromOpacity;

	private float _landingOpacity;

	internal const double CardRevealDuration = 0.5;

	private double _cardRevealAt = double.NegativeInfinity;

	private float _cardRevealFrom;

	private long _generation;

	internal int FinaleGhostCount => _finaleGhosts.Count((FinaleGhost g) => g.Active);

	internal Vector2 FinaleSlashOrigin { get; private set; }

	internal int FinaleSlashFacing { get; private set; } = 1;

	internal string Special => _special;

	internal ReturnJourney? Journey { get; private set; }

	internal double Clock => _clock;

	internal bool Dead => _dead;

	internal long Generation => _generation;

	internal Vector2 Position => _display.Root.Position;

	internal Vector2 Home => _display.Home;

	internal Vector2 DisplayWorldOrigin => _display.Root.GlobalPosition;

	internal NativePose Pose => _display.Pose;

	internal int SurgeCount => _surges.Count;

	internal int DownEffectCount => _downEffects.Count;

	internal string Card => _card;

	internal string Upper => _upper;

	internal float BodyOpacity => ((CanvasItem)_display.Root).Modulate.A;

	internal int BodyLayer => PresentationLayer.Effective((CanvasItem)(object)_display.Root);

	internal bool Attacking => _clip.Length > 0;

	internal string ShivForm
	{
		get
		{
			if (_groundShiv.Length <= 0)
			{
				return "shiv";
			}
			return _groundShiv;
		}
	}

	internal int PunchGhostCount => _punchGhosts.Count((PunchGhost g) => g.Active);

	internal static int Count => Active.Count;

	internal bool Away
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			if (Journey == null)
			{
				return Math.Abs(_x - (double)Home.X) > 0.01;
			}
			return true;
		}
	}

	internal static ActorPresentation? For(NCreature actor)
	{
		return Active.GetValueOrDefault(actor);
	}

	internal DisplayLease? BeginExternalTint()
	{
		if (!_display.BeginExternalTint())
		{
			return null;
		}
		return _display;
	}

	private ActorPresentation(NCreature actor, NCombatRoom room)
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		_actor = actor;
		_room = room;
		_tree = ((Node)room).GetTree();
		_combat = actor.Entity.CombatState;
		_display = new DisplayLease(actor);
		_facing = _display.NativeFacing;
		try
		{
			_motion = MotionBank.Get(_display.Pose.Version);
			_x = Home.X;
			_shadows = new AfterimageTrail(_display);
			_departures = new DepartureTrail(_display);
			_surges = new ThrustSurge(_display);
			_downEffects = new DownStrikeEffects(_display);
		}
		catch
		{
			_display.Dispose();
			throw;
		}
		_lastFrame = Engine.GetProcessFrames();
		((Node)actor).TreeExiting += Dispose;
		((Node)room).TreeExiting += Dispose;
		_tree.ProcessFrame += Frame;
	}

	internal static (ActorPresentation Session, long Generation)? Begin(Creature attacker, Creature target, ThrustProfile profile, double lead)
	{
		if (attacker.IsAlive && target.IsAlive)
		{
			Player player = attacker.Player;
			if (!(((player != null) ? ((object)player.Character).GetType().Name : null) != "Silent"))
			{
				NCreature creatureNode = attacker.GetCreatureNode();
				NCreature creatureNode2 = target.GetCreatureNode();
				NCombatRoom instance = NCombatRoom.Instance;
				if (creatureNode == null || creatureNode2 == null || instance == null || !((Node)creatureNode).IsInsideTree() || !((Node)creatureNode2).IsInsideTree() || ((GodotObject)creatureNode).IsQueuedForDeletion() || ((GodotObject)creatureNode2).IsQueuedForDeletion() || creatureNode.IsPlayingDeathAnimation)
				{
					return null;
				}
				if (CharacterRig.ForPresentation(creatureNode.Body) == null)
				{
					return null;
				}
				ActorPresentation actorPresentation = For(creatureNode);
				if (actorPresentation != null && !actorPresentation._display.Current)
				{
					actorPresentation.Dispose();
					actorPresentation = null;
				}
				if (actorPresentation == null)
				{
					actorPresentation = new ActorPresentation(creatureNode, instance);
					Active.Add(creatureNode, actorPresentation);
				}
				try
				{
					actorPresentation.Start(creatureNode2, profile, lead);
					return (actorPresentation, actorPresentation.Generation);
				}
				catch
				{
					actorPresentation.Dispose();
					throw;
				}
			}
		}
		return null;
	}

	private void Start(NCreature target, ThrustProfile profile, double lead)
	{
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		Synchronize();
		if (_dead)
		{
			return;
		}
		_returnStartup = Journey != null && _clock < Journey.End;
		if (_returnStartup)
		{
			Render();
			Pose.BeginEntry();
			_upperFromOpacity = _display.Opacity;
		}
		_cardRevealAt = double.NegativeInfinity;
		_replacedReturnAttack = false;
		CancelPendingSurge();
		_surge = null;
		_downBurst = null;
		_card = profile.Card;
		bool punch = profile.Punch;
		_down = profile.DownStab;
		_nativeAttack = profile.NativeAttack;
		_upper = (_groundShiv = (_special = ""));
		_steel = false;
		_landingAt = double.NegativeInfinity;
		_generation++;
		_started = _clock;
		_hitAt = _clock + lead;
		_clip = ((!_down) ? (_nativeAttack ? "native-attack" : (punch ? "punch" : "thrust")) : (profile.Normal ? "down-normal" : "down-fast"));
		_display.TargetLayer(target);
		_poseHit = ((!_down) ? (_nativeAttack ? 0.15 : 0.08) : (profile.Normal ? 0.15 : 0.075));
		_clipDuration = _motion.ClipDuration(_clip);
		_teleportAt = _clock + lead * (_down ? 0.4 : (2.0 / 3.0));
		_releaseAt = _clock + (_down ? (lead + _clipDuration - _poseHit) : Math.Min(0.75, lead + _clipDuration - _poseHit));
		_finisher = profile.Card == "Finisher";
		_echoingSlash = profile.Card == "EchoingSlash";
		if (_finisher || _echoingSlash)
		{
			_releaseAt = double.PositiveInfinity;
		}
		_hit = (_departed = false);
		_expectAttack = !_echoingSlash;
		Vector2 val = _display.ParentPoint(target.VfxSpawnPosition);
		_attackFacing = ((val.X >= Home.X) ? 1 : (-1));
		Rect2 globalRect = target.Visuals.Bounds.GetGlobalRect();
		double num = Math.Abs((_display.ParentPoint(((Rect2)(ref globalRect)).Position + new Vector2(((Rect2)(ref globalRect)).Size.X, 0f)) - _display.ParentPoint(((Rect2)(ref globalRect)).Position)).X);
		double num2 = Pose.Number(_nativeAttack ? "nativeAttackReach" : (punch ? "punchReach" : "thrustReach")) * (double)_display.ScaleX;
		_destination = (_down ? ((double)val.X + (double)_attackFacing * (num * 0.12 + Pose.Number("downTipX") * (double)_display.ScaleX)) : ((double)val.X - (double)_attackFacing * (num * 0.2 + num2)));
		if (_down && _card != "Backstab")
		{
			try
			{
				_downBurst = _downEffects.Begin(_card, val, num, (double)(9101 + _generation * 997) + Math.Round(_clock * 1000.0), _started, _hitAt, _attackFacing);
			}
			catch (Exception error)
			{
				ModEntry.Log(error);
			}
		}
		Render();
		_display.Acquire();
	}

	internal bool ContinueFinisher(long generation, double lead)
	{
		if (_disposed || _dead || generation != _generation || !_finisher || _clip.Length == 0)
		{
			return false;
		}
		Synchronize();
		Render();
		_started = _clock;
		_hitAt = _clock + lead;
		_hit = false;
		_expectAttack = true;
		_upper = "";
		_releaseAt = double.PositiveInfinity;
		Render();
		return true;
	}

	internal void EndFinisher(long generation)
	{
		if (!_disposed && !_dead && generation == _generation && _finisher)
		{
			Synchronize();
			_finisher = false;
			if (!_hit)
			{
				CancelUnhit(generation);
				return;
			}
			_releaseAt = Math.Max(_clock, _started + Math.Min(0.75, _hitAt - _started + _clipDuration - _poseHit));
			Render();
		}
	}

	internal bool ContinueEchoingSlash(long generation)
	{
		if (_disposed || _dead || generation != _generation || !_echoingSlash || _clip.Length == 0 || !_display.Current)
		{
			return false;
		}
		Synchronize();
		Render();
		_started = (_hitAt = _clock);
		_hit = true;
		_expectAttack = false;
		_upper = "";
		_releaseAt = double.PositiveInfinity;
		Render();
		return true;
	}

	internal void EndEchoingSlash(long generation)
	{
		if (!_disposed && !_dead && generation == _generation && _echoingSlash)
		{
			Synchronize();
			_echoingSlash = false;
			if (!_hit)
			{
				CancelUnhit(generation);
				return;
			}
			_releaseAt = Math.Max(_clock, _hitAt + Math.Min(0.75, _clipDuration - _poseHit));
			Render();
		}
	}

	internal void Hit(long generation)
	{
		if (_disposed || _dead || generation != _generation)
		{
			return;
		}
		Synchronize();
		bool num = !_hit;
		_hit = true;
		_hitAt = _clock;
		_teleportAt = Math.Min(_teleportAt, _clock);
		_releaseAt = (_down ? (_clock + _clipDuration - _poseHit) : Math.Max(_clock, _started + Math.Min(0.75, _clock - _started + _clipDuration - _poseHit)));
		if (_finisher || _echoingSlash)
		{
			_releaseAt = double.PositiveInfinity;
		}
		Render();
		_surge?.Hit(_clock);
		_downBurst?.Hit(_clock);
		if (!num || !(_card == "SuckerPunch") || Pose.IsSkin)
		{
			return;
		}
		try
		{
			_punchGhosts.Add(new PunchGhost(_display, _clock));
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	internal void CancelUnhit(long generation)
	{
		if (!_disposed && !_dead && generation == _generation && !_hit)
		{
			Synchronize();
			CancelPendingSurge();
			_clip = "";
			_returnStartup = false;
			if (_departed || Journey == null)
			{
				BeginReturn(_clock);
			}
			Render();
		}
	}

	internal void Trigger(string trigger)
	{
		if (_disposed)
		{
			return;
		}
		if (!_display.Current)
		{
			Dispose();
			return;
		}
		Synchronize();
		if (trigger == "Dead")
		{
			if (!_dead)
			{
				CancelPendingSurge();
				_dead = true;
				_deathAt = _clock;
				Journey = null;
				_clip = (_upper = (_special = ""));
				Render();
			}
			return;
		}
		if (_dead)
		{
			if (trigger == "Revive")
			{
				Dispose();
			}
			return;
		}
		if (trigger == "Attack" && _expectAttack)
		{
			_expectAttack = false;
			return;
		}
		Render();
		bool away = Away;
		string text;
		switch (trigger)
		{
		case "Hit":
			text = "hurt";
			break;
		case "Attack":
			text = (away ? "shiv" : "attack");
			break;
		case "Cast":
		case "PowerUp":
			text = "cast";
			break;
		case "Shiv":
			text = "shiv";
			break;
		case "Relaxed":
			text = "relaxed_loop";
			break;
		default:
			text = "";
			break;
		}
		string text2 = text;
		if (text2.Length == 0 || Pose.Duration(text2) <= 0.0)
		{
			return;
		}
		_replacedReturnAttack = away && trigger == "Attack";
		Pose.BeginEntry(Journey != null);
		_cardRevealAt = double.NegativeInfinity;
		if (_returnStartup && !_departed)
		{
			CancelPendingSurge();
			_clip = "";
			_returnStartup = false;
		}
		_groundShiv = (_special = "");
		_steel = false;
		_upperFromOpacity = _display.Opacity;
		_upper = text2;
		_upperAt = _clock;
		if ((_finisher || _echoingSlash) && _departed && trigger == "Hit")
		{
			Pose.BeginEntry(upperOnly: false);
			Render();
			return;
		}
		if (Journey == null)
		{
			CancelPendingSurge();
			_clip = "";
			BeginReturn(_clock);
			if (Journey == null)
			{
				_landingAt = _clock;
				_landingOpacity = _upperFromOpacity;
			}
		}
		Render();
		if (_replacedReturnAttack)
		{
			ReturnKnifePatch.Replaced(_actor, this);
		}
	}

	internal static string BeginShiv(NCreature actor, int roll)
	{
		if (CharacterRig.ForPresentation(actor.Body) == null)
		{
			return "shiv";
		}
		if (actor.Entity.IsAlive && !actor.IsPlayingDeathAnimation)
		{
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance != null)
			{
				ActorPresentation actorPresentation = For(actor);
				if (actorPresentation != null && !actorPresentation._display.Current)
				{
					actorPresentation.Dispose();
					actorPresentation = null;
				}
				if (actorPresentation?.Journey != null)
				{
					actorPresentation.Trigger("Shiv");
					return "shiv";
				}
				string text = roll switch
				{
					2 => "shiv-low", 
					0 => "shiv-high", 
					_ => "shiv", 
				};
				if (actorPresentation == null && text == "shiv")
				{
					return text;
				}
				bool flag = actorPresentation == null;
				if (flag)
				{
					actorPresentation = new ActorPresentation(actor, instance);
					Active.Add(actor, actorPresentation);
				}
				try
				{
					actorPresentation.Synchronize();
					if (flag)
					{
						actorPresentation.Pose.BeginOriginalEntry();
					}
					else
					{
						actorPresentation.Render();
						actorPresentation.Pose.BeginEntry(upperOnly: false);
					}
					actorPresentation.CancelPendingSurge();
					actorPresentation._clip = (actorPresentation._special = "");
					actorPresentation._steel = false;
					actorPresentation._replacedReturnAttack = false;
					actorPresentation._groundShiv = ((text == "shiv") ? "" : text);
					actorPresentation._upper = "shiv";
					actorPresentation._upperAt = actorPresentation._clock;
					actorPresentation._landingAt = actorPresentation._clock;
					actorPresentation._landingOpacity = 1f;
					actorPresentation._card = "Shiv";
					actorPresentation._expectAttack = false;
					actorPresentation.BeginReturn(actorPresentation._clock);
					if (actorPresentation.Journey != null)
					{
						actorPresentation._groundShiv = "";
					}
					actorPresentation._landingAt = actorPresentation._clock;
					actorPresentation.Render();
					actorPresentation._display.Acquire();
					return actorPresentation.ShivForm;
				}
				catch
				{
					actorPresentation.Dispose();
					throw;
				}
			}
		}
		return "shiv";
	}

	internal static bool BeginFinale(NCreature actor, string mode)
	{
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		if (CharacterRig.ForPresentation(actor.Body) == null)
		{
			return false;
		}
		if (actor.Entity.IsAlive && !actor.IsPlayingDeathAnimation)
		{
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance != null)
			{
				ICombatState combatState = actor.Entity.CombatState;
				object obj;
				if (combatState == null)
				{
					obj = null;
				}
				else
				{
					Creature? obj2 = combatState.HittableEnemies.FirstOrDefault();
					obj = ((obj2 != null) ? obj2.GetCreatureNode() : null);
				}
				NCreature val = (NCreature)obj;
				if (val == null)
				{
					return false;
				}
				ActorPresentation actorPresentation = For(actor);
				if (actorPresentation != null && !actorPresentation._display.Current)
				{
					actorPresentation.Dispose();
					actorPresentation = null;
				}
				SnakeVisual.Stop(actor);
				bool flag = actorPresentation == null;
				if (flag)
				{
					actorPresentation = new ActorPresentation(actor, instance);
					Active.Add(actor, actorPresentation);
				}
				try
				{
					actorPresentation.Synchronize();
					actorPresentation.Pose.LoadFinaleSlash();
					if (flag)
					{
						actorPresentation.Pose.BeginOriginalEntry();
					}
					else
					{
						actorPresentation.Render();
						actorPresentation.Pose.BeginEntry(upperOnly: false);
					}
					actorPresentation.CancelPendingSurge();
					actorPresentation._surge = null;
					actorPresentation._downBurst = null;
					actorPresentation._clip = "finale-slash";
					actorPresentation._upper = (actorPresentation._groundShiv = (actorPresentation._special = ""));
					actorPresentation._steel = (actorPresentation._down = (actorPresentation._nativeAttack = (actorPresentation._finisher = (actorPresentation._echoingSlash = (actorPresentation._replacedReturnAttack = false)))));
					actorPresentation._returnStartup = actorPresentation.Journey != null;
					actorPresentation._upperFromOpacity = actorPresentation._display.Opacity;
					actorPresentation._generation++;
					actorPresentation._started = actorPresentation._clock;
					actorPresentation._poseHit = 0.072;
					actorPresentation._clipDuration = actorPresentation.Pose.AuthoredDuration("finale-slash");
					actorPresentation._teleportAt = actorPresentation._clock + ((mode == "instant") ? 0.0 : 1.49);
					actorPresentation._hitAt = actorPresentation._clock + ((mode == "instant") ? 0.0 : 1.5820658385709536);
					actorPresentation._releaseAt = actorPresentation._hitAt + actorPresentation._clipDuration - actorPresentation._poseHit;
					actorPresentation._hit = true;
					actorPresentation._departed = false;
					actorPresentation._cardRevealAt = double.NegativeInfinity;
					actorPresentation._card = "GrandFinale";
					actorPresentation._expectAttack = false;
					actorPresentation._landingAt = actorPresentation._clock;
					actorPresentation._landingOpacity = 1f;
					actorPresentation._display.TargetLayer(val);
					Vector2 val2 = actorPresentation._display.ParentPoint(val.VfxSpawnPosition);
					actorPresentation._attackFacing = ((val2.X >= actorPresentation.Home.X) ? 1 : (-1));
					actorPresentation.FinaleSlashFacing = actorPresentation._attackFacing;
					Rect2 globalRect = val.Visuals.Bounds.GetGlobalRect();
					double num = Math.Abs((actorPresentation._display.ParentPoint(((Rect2)(ref globalRect)).Position + new Vector2(((Rect2)(ref globalRect)).Size.X, 0f)) - actorPresentation._display.ParentPoint(((Rect2)(ref globalRect)).Position)).X);
					actorPresentation._destination = (double)val2.X - (double)actorPresentation._attackFacing * (num * 0.2 + actorPresentation.Pose.Number("thrustReach") * (double)actorPresentation._display.ScaleX);
					Transform2D val3 = actorPresentation._display.Facing(actorPresentation._attackFacing);
					val3.Origin = new Vector2((float)actorPresentation._destination, actorPresentation.Home.Y);
					Transform2D globalTransform = actorPresentation._display.Root.GlobalTransform;
					Transform2D transform = actorPresentation._display.Root.Transform;
					Transform2D val4 = globalTransform * ((Transform2D)(ref transform)).AffineInverse();
					ActorPresentation actorPresentation2 = actorPresentation;
					Transform2D val5 = val4 * val3;
					transform = actorPresentation._display.Body.Transform;
					actorPresentation2.FinaleSlashOrigin = val5 * ((Transform2D)(ref transform)).AffineInverse() * actorPresentation._display.ParentPoint(actor.VfxSpawnPosition);
					if (actorPresentation.Pose.IsSkin)
					{
						foreach (FinaleGhost finaleGhost in actorPresentation._finaleGhosts)
						{
							finaleGhost.Dispose();
						}
						actorPresentation._finaleGhosts.Clear();
					}
					actorPresentation._finaleGhosts.Add(new FinaleGhost(actorPresentation._display, mode, actorPresentation._clock));
					actorPresentation.Render();
					actorPresentation._display.Acquire();
					return true;
				}
				catch
				{
					actorPresentation.Dispose();
					throw;
				}
			}
		}
		return false;
	}

	internal static ActorPresentation? BeginSteel(NCreature actor)
	{
		if (CharacterRig.ForPresentation(actor.Body) == null)
		{
			return null;
		}
		if (actor.Entity.IsAlive && !actor.IsPlayingDeathAnimation)
		{
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance != null)
			{
				SnakeVisual.Stop(actor);
				ActorPresentation actorPresentation = For(actor);
				if (actorPresentation != null && !actorPresentation._display.Current)
				{
					actorPresentation.Dispose();
					actorPresentation = null;
				}
				bool flag = actorPresentation == null;
				if (flag)
				{
					actorPresentation = new ActorPresentation(actor, instance);
					Active.Add(actor, actorPresentation);
				}
				try
				{
					actorPresentation.Synchronize();
					actorPresentation.Pose.LoadSteel();
					if (flag)
					{
						actorPresentation.Pose.BeginOriginalEntry();
					}
					else
					{
						actorPresentation.Render();
						actorPresentation.Pose.BeginEntry(actorPresentation.Journey != null);
					}
					bool away = actorPresentation.Away;
					actorPresentation._cardRevealAt = double.NegativeInfinity;
					actorPresentation.CancelPendingSurge();
					actorPresentation._clip = (actorPresentation._special = (actorPresentation._groundShiv = ""));
					actorPresentation._returnStartup = false;
					actorPresentation._upperFromOpacity = actorPresentation._display.Opacity;
					actorPresentation._upper = (away ? "shiv" : "attack");
					actorPresentation._upperAt = actorPresentation._clock;
					actorPresentation._steel = !away;
					actorPresentation._card = "StormOfSteel";
					actorPresentation._expectAttack = false;
					actorPresentation._replacedReturnAttack = false;
					if (actorPresentation.Journey == null)
					{
						actorPresentation.BeginReturn(actorPresentation._clock);
						actorPresentation._landingAt = actorPresentation._clock;
						actorPresentation._landingOpacity = 1f;
					}
					actorPresentation.Render();
					actorPresentation._display.Acquire();
					return actorPresentation;
				}
				catch
				{
					actorPresentation.Dispose();
					throw;
				}
			}
		}
		return null;
	}

	internal Vector2 ShivOrigin(Vector2 origin, string form, double launch)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = MapPoint(origin);
		if (form == "shiv" || _groundShiv != form)
		{
			return val;
		}
		Vector2 val2 = Pose.ClipOffset(form, launch, "hand_l");
		return val + _display.Root.GlobalTransform.X * val2.X + _display.Root.GlobalTransform.Y * val2.Y;
	}

	private void CancelPendingSurge()
	{
		ThrustSurge.Burst surge = _surge;
		if (surge != null && !surge.Confirmed)
		{
			_surge.Dispose();
		}
		DownStrikeEffects.Burst downBurst = _downBurst;
		if (downBurst != null && !downBurst.Confirmed)
		{
			_downBurst.Dispose();
		}
	}

	private void BeginReturn(double start)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		_cardRevealAt = double.NegativeInfinity;
		_returnStartup = false;
		_landingAt = double.NegativeInfinity;
		Journey = new ReturnJourney(_x, Home.X, start, (Pose.IsSkin ? Pose.Number("hopDistance") : _motion.HopDistance) * (double)_display.ScaleX, _motion);
		if (Journey.Steps == 0)
		{
			Journey = null;
		}
	}

	private void Synchronize()
	{
		if (_disposed || _tree.Paused || !((Node)_actor).CanProcess())
		{
			return;
		}
		ulong processFrames = Engine.GetProcessFrames();
		if (processFrames != _lastFrame)
		{
			_lastFrame = processFrames;
			_clock += ((Node)_actor).GetProcessDeltaTime();
			if (!_dead && Journey != null)
			{
				_x = Journey.Position(_clock);
			}
		}
	}

	private static float Smooth(double t)
	{
		float num = (float)Math.Clamp(t, 0.0, 1.0);
		return num * num * (3f - 2f * num);
	}

	private float UpperOpacity(double time)
	{
		if (_upper.Length == 0)
		{
			return 0f;
		}
		double num = time - _upperAt;
		return (_upperFromOpacity + (1f - _upperFromOpacity) * Smooth(num / 0.08)) * (1f - Smooth((num - Pose.Duration(_upper)) / 0.1));
	}

	internal void RevealCard()
	{
		if (!_disposed && !_dead)
		{
			Synchronize();
			if (Journey != null && !(_clock >= Journey.End))
			{
				Render();
				_cardRevealFrom = BodyOpacity;
				_cardRevealAt = _clock;
			}
		}
	}

	private float ReturnOpacity(double time)
	{
		double num = time - _cardRevealAt;
		float val = ((num < 0.0 || num >= 0.5) ? 0f : ((_cardRevealFrom + (1f - _cardRevealFrom) * Smooth(num / 0.08)) * (1f - Smooth((num - 0.4) / 0.1))));
		return Math.Max(UpperOpacity(time), val);
	}

	private void Render()
	{
		//IL_08d1: Unknown result type (might be due to invalid IL or missing references)
		if (_downLines != null)
		{
			((CanvasItem)_downLines.Sprite).Visible = false;
		}
		if (_dead)
		{
			_display.AttackLayer(active: false);
			_display.Place((float)_x, _facing);
			Pose.Sample("die", _clock - _deathAt);
			return;
		}
		_display.AttackLayer(_clip.Length > 0);
		if (_clip.Length > 0 && _clock >= _teleportAt && !_departed)
		{
			try
			{
				_departures.Begin(_down ? "DownStab" : (_nativeAttack ? "NativeAttack" : ((_clip == "punch") ? "SuckerPunch" : "Thrust")), _x, _destination, _facing, _teleportAt, _attackFacing);
			}
			catch (Exception error)
			{
				ModEntry.Log(error);
			}
			_departed = true;
			_returnStartup = false;
			_x = _destination;
			Journey = null;
			_facing = ((!_down) ? 1 : (-1)) * _attackFacing;
			_display.Place((float)_x, _facing);
			try
			{
				if (!_down && !_nativeAttack)
				{
					_surge = _surges.Begin((_clip == "finale-slash") ? "Neutralize" : _card, _teleportAt, _hitAt, _clip == "finale-slash");
					if (_hit)
					{
						_surge.Hit(_hitAt);
					}
				}
			}
			catch (Exception error2)
			{
				ModEntry.Log(error2);
			}
		}
		if (_clip.Length > 0 && _hit && _clock >= _releaseAt)
		{
			_clip = "";
			BeginReturn(_releaseAt);
			_display.AttackLayer(active: false);
		}
		if (_clip.Length > 0 && _departed)
		{
			double num = ((_down || _nativeAttack) ? _started : _teleportAt);
			double num2 = ((_clock < _hitAt) ? (_poseHit * (_clock - num) / Math.Max(1E-06, _hitAt - num)) : (_poseHit + _clock - _hitAt));
			if ((_card == "Finisher" || _echoingSlash) && _upper == "hurt")
			{
				Pose.EntryMix(Smooth((_clock - _upperAt) / 0.08));
				Pose.Grounded("hurt", _clock - _upperAt);
			}
			else
			{
				Pose.Custom(_clip, Math.Clamp(num2, 0.0, _clipDuration));
			}
			if (_down && !Pose.IsSkin)
			{
				if (_downLines == null)
				{
					_downLines = new NativePose(_display.Body, (Node)(object)_display.Root);
				}
				_downLines.Custom(_clip.Replace("down-", "down-lines-"), num2);
				((CanvasItem)_downLines.Sprite).Visible = true;
			}
			_display.Place((float)_x, _facing);
			return;
		}
		if (Journey != null && _clock < Journey.End)
		{
			_x = Journey.Position(_clock);
			double num3 = _clock - _upperAt;
			double num4 = ((_upper.Length > 0) ? Pose.Duration(_upper) : 0.0);
			float weight = ((_upper.Length == 0) ? 0f : (1f - Smooth((num3 - num4) / 0.1)));
			Pose.EntryMix(Smooth(num3 / 0.08));
			(MixedReturnPlan.Step, int, double, string, string) tuple = Journey.At(_clock);
			if (_returnStartup && _clip.Length > 0 && !_departed && (_clip != "finale-slash" || _clock >= _teleportAt - 0.1))
			{
				double num5 = Math.Max(0.0, _clock - ((_clip == "finale-slash") ? (_teleportAt - 0.1) : _started));
				double age = ((_down || _nativeAttack) ? (_poseHit * num5 / Math.Max(1E-06, _hitAt - _started)) : (0.16 * num5 / Math.Max(1E-06, (_clip == "finale-slash") ? 0.1 : (_teleportAt - _started))));
				Pose.EntryMix(Smooth(num5 / 0.08));
				Pose.ReturningCustom(tuple.Item3, (_down || _nativeAttack) ? _clip : "charge", age, tuple.Item4, tuple.Item5, tuple.Item1.Last);
				_facing = ((Journey.Home <= Journey.From) ? 1 : (-1));
				_display.Place((float)_x, _facing, _upperFromOpacity + (1f - _upperFromOpacity) * Smooth(num5 / 0.08));
			}
			else
			{
				if (_steel)
				{
					Pose.ReturningCustom(tuple.Item3, "steel", Math.Min(num3, num4), tuple.Item4, tuple.Item5, tuple.Item1.Last, weight);
				}
				else
				{
					Pose.Returning(tuple.Item3, _upper, Math.Min(num3, num4), weight, tuple.Item4, tuple.Item5, tuple.Item1.Last);
				}
				_facing = ((Journey.Home <= Journey.From) ? 1 : (-1));
				_display.Place((float)_x, _facing, Pose.IsSkin ? Math.Max(0.15f, ReturnOpacity(_clock)) : ReturnOpacity(_clock));
			}
			return;
		}
		if (Journey != null)
		{
			_landingAt = Journey.End;
			_landingOpacity = ReturnOpacity(_landingAt);
			(MixedReturnPlan.Step, int, double, string, string) tuple2 = Journey.At(Journey.End);
			double num6 = Math.Max(0.0, _landingAt - _upperAt);
			double num7 = ((_upper.Length > 0) ? Pose.Duration(_upper) : 0.0);
			Pose.EntryMix(Smooth(num6 / 0.08));
			float weight2 = ((_upper.Length == 0) ? 0f : (1f - Smooth((num6 - num7) / 0.1)));
			if (_steel)
			{
				Pose.ReturningCustom(tuple2.Item3, "steel", Math.Min(num6, num7), tuple2.Item4, tuple2.Item5, last: true, weight2);
			}
			else
			{
				Pose.Returning(tuple2.Item3, _upper, Math.Min(num6, num7), weight2, tuple2.Item4, tuple2.Item5, last: true);
			}
			Pose.BeginEntry(upperOnly: false);
			_x = Home.X;
			Journey = null;
		}
		if (_clip.Length > 0)
		{
			if (_clip == "finale-slash")
			{
				double num8 = _clock - (_teleportAt - 0.1);
				if (num8 < 0.0)
				{
					Pose.Grounded("idle_loop", _clock - _started);
				}
				else
				{
					Pose.EntryMix(Smooth(num8 / 0.045));
					Pose.CustomEntry("charge", 0.16 * num8 / 0.1);
				}
			}
			else if (_down || _nativeAttack)
			{
				Pose.Custom(_clip, _poseHit * (_clock - _started) / Math.Max(1E-06, _hitAt - _started));
			}
			else
			{
				Pose.Custom("charge", 0.16 * (_clock - _started) / Math.Max(1E-06, _teleportAt - _started));
			}
		}
		else
		{
			Pose.EntryMix(Smooth((_clock - _landingAt) / 0.08));
			if (_upper.Length > 0 && _clock - _upperAt < Pose.Duration(_upper) + 0.1)
			{
				if (_steel && _clock - _upperAt < Pose.Duration(_upper))
				{
					Pose.CustomEntry("steel", _clock - _upperAt);
				}
				else if (_upper == "shiv" && _groundShiv.Length > 0 && _clock - _upperAt < Pose.Duration(_upper))
				{
					Pose.CustomEntry(_groundShiv, _clock - _upperAt);
				}
				else
				{
					Pose.Grounded(_upper, _clock - _upperAt);
				}
			}
			else
			{
				_upper = "";
				Pose.Grounded("idle_loop", 0.0);
			}
		}
		_facing = ((_clip.Length > 0) ? _attackFacing : _display.NativeFacing);
		_display.Place((float)_x, _facing, _landingOpacity + (1f - _landingOpacity) * Smooth((_clock - _landingAt) / 0.08));
	}

	private void Frame()
	{
		if (_disposed)
		{
			return;
		}
		try
		{
			if (!_display.Current || _actor.Entity.CombatState != _combat)
			{
				Dispose();
			}
			else
			{
				if (_tree.Paused || !((Node)_actor).CanProcess())
				{
					return;
				}
				Synchronize();
				Render();
				_shadows.Update(_dead ? null : Journey, _clock);
				_departures.Update(_clock);
				_surges.Update(_clock);
				_downEffects.Update(_clock);
				foreach (PunchGhost punchGhost in _punchGhosts)
				{
					punchGhost.Update(_clock);
				}
				_punchGhosts.RemoveAll((PunchGhost g) => !g.Active);
				foreach (FinaleGhost finaleGhost in _finaleGhosts)
				{
					if (_dead)
					{
						finaleGhost.Dispose();
					}
					else
					{
						finaleGhost.Update(_clock);
					}
				}
				_finaleGhosts.RemoveAll((FinaleGhost g) => !g.Active);
				if (!_dead && _special.Length == 0 && _clip.Length == 0 && _upper.Length == 0 && Journey == null && _clock >= _landingAt + 0.08 && (!_replacedReturnAttack || !Pose.OriginalAttackPending))
				{
					_replacedReturnAttack = false;
					_display.Release();
					if (!_shadows.Active && !_departures.Active && !_surges.Active && _downEffects.Count == 0 && _punchGhosts.Count == 0 && _finaleGhosts.Count == 0)
					{
						Dispose();
					}
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			VisualCleanup.Safely(Dispose);
		}
	}

	internal Vector2 MapPoint(Vector2 native)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (!_display.Current)
		{
			return native;
		}
		return _display.MapPoint(native);
	}

	internal static void Reset()
	{
		ActorPresentation[] array = Active.Values.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			VisualCleanup.Safely(array[i].Dispose);
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_tree))
		{
			_tree.ProcessFrame -= Frame;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_actor))
		{
			((Node)_actor).TreeExiting -= Dispose;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_room))
		{
			((Node)_room).TreeExiting -= Dispose;
		}
		if (For(_actor) == this)
		{
			Active.Remove(_actor);
		}
		VisualCleanup.Safely(_display.Dispose);
		foreach (PunchGhost punchGhost in _punchGhosts)
		{
			VisualCleanup.Safely(punchGhost.Dispose);
		}
		_punchGhosts.Clear();
		foreach (FinaleGhost finaleGhost in _finaleGhosts)
		{
			VisualCleanup.Safely(finaleGhost.Dispose);
		}
		_finaleGhosts.Clear();
		VisualCleanup.Safely(delegate
		{
			_downLines?.Dispose();
		});
		VisualCleanup.Safely(_downEffects.Dispose);
		VisualCleanup.Safely(_surges.Dispose);
		VisualCleanup.Safely(_shadows.Dispose);
		VisualCleanup.Safely(_departures.Dispose);
	}
}
internal sealed class AfterimageTrail(DisplayLease display) : IDisposable
{
	private sealed class Echo(NativeEchoPool.Item item, double at) : IDisposable
	{
		internal readonly double At = at;

		internal Node2D Root => item.Root;

		internal NativePose Pose => item.Pose;

		internal ShaderMaterial Material => item.Material;

		public void Dispose()
		{
			item.Dispose();
		}
	}

	private static readonly StringName AgeUniform = StringName.op_Implicit("age");

	private static readonly StringName LocalScaleUniform = StringName.op_Implicit("local_scale");

	private static readonly StringName OpacityUniform = StringName.op_Implicit("opacity");

	private static readonly StringName SeedUniform = StringName.op_Implicit("seed");

	internal static readonly Shader Shader = LoadShader();

	private readonly List<Echo> _echoes = new List<Echo>();

	private readonly List<ReturnSmoke> _smoke = new List<ReturnSmoke>();

	private Echo? _marker;

	private ReturnJourney? _journey;

	private double _emittedUntil = double.NegativeInfinity;

	internal bool Active
	{
		get
		{
			if (_echoes.Count <= 0)
			{
				return _smoke.Count > 0;
			}
			return true;
		}
	}

	private static Shader LoadShader()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		using Stream stream = typeof(AfterimageTrail).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets.body-smoke-r2.gdshader") ?? throw new InvalidDataException("Approved body smoke shader missing");
		using StreamReader streamReader = new StreamReader(stream);
		return new Shader
		{
			Code = streamReader.ReadToEnd()
		};
	}

	internal void Update(ReturnJourney? journey, double now)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		if (!display.Pose.IsSkin && journey != null && now >= journey.Start && now < journey.End)
		{
			if (_marker == null)
			{
				_marker = Create(journey, now, 3.1f, sample: false);
			}
			((GodotObject)_marker.Root).SetMeta(StringName.op_Implicit("silent_return_marker"), Variant.op_Implicit(true));
			Transform2D transform = display.Facing((journey.Home <= journey.From) ? 1 : (-1));
			transform.Origin = new Vector2((float)journey.Position(now), display.Home.Y);
			_marker.Root.Transform = transform;
			(MixedReturnPlan.Step, int, double, string, string) tuple = journey.At(now);
			float num = (float)ReturnEchoPolicy.Marker(tuple.Item1.Short, tuple.Item3 / tuple.Item1.Duration, tuple.Item1.Last);
			float num2 = 0.15f * num * (1f - display.Opacity);
			((CanvasItem)_marker.Root).Visible = num2 > 0f;
			if (num2 > 0f)
			{
				Sample(_marker.Pose, journey, now);
			}
			_marker.Material.SetShaderParameter(AgeUniform, Variant.op_Implicit(0.18f));
			_marker.Material.SetShaderParameter(OpacityUniform, Variant.op_Implicit(num2));
			_marker.Material.SetShaderParameter(LocalScaleUniform, Variant.op_Implicit(new Vector2(display.ScaleX * (float)((journey.Home <= journey.From) ? 1 : (-1)), 0f - display.Body.Scale.Y)));
		}
		else if (_marker != null)
		{
			_marker.Dispose();
			_marker = null;
		}
		if (journey != _journey)
		{
			foreach (ReturnSmoke item in _smoke)
			{
				if (item.Journey == _journey)
				{
					item.End = Math.Min(item.End, now);
				}
			}
			_journey = journey;
			_emittedUntil = ((journey != null) ? (journey.Start - 1E-08) : now);
			if (journey != null)
			{
				_smoke.Add(new ReturnSmoke(display, journey));
			}
		}
		for (int num3 = _smoke.Count - 1; num3 >= 0; num3--)
		{
			if (!_smoke[num3].Update(now))
			{
				_smoke[num3].Dispose();
				_smoke.RemoveAt(num3);
			}
		}
		if (journey != null && now >= journey.Start)
		{
			double num4 = Math.Min(now, journey.End);
			double num5 = Math.Max(_emittedUntil, now - 0.5);
			(double, int)[] echoes = journey.Echoes;
			for (int i = 0; i < echoes.Length; i++)
			{
				(double, int) tuple2 = echoes[i];
				if (tuple2.Item1 > num4)
				{
					break;
				}
				if (tuple2.Item1 > num5 && tuple2.Item1 < journey.End - 1E-08)
				{
					Emit(journey, tuple2.Item1, 5.1f + (float)tuple2.Item2 * 0.73f);
				}
			}
			_emittedUntil = num4;
		}
		for (int num6 = _echoes.Count - 1; num6 >= 0; num6--)
		{
			Echo echo = _echoes[num6];
			double num7 = now - echo.at;
			if (num7 >= 0.5)
			{
				echo.Dispose();
				_echoes.RemoveAt(num6);
			}
			else
			{
				float num8 = (float)(num7 / 0.5);
				echo.Material.SetShaderParameter(AgeUniform, Variant.op_Implicit(num8));
				echo.Material.SetShaderParameter(OpacityUniform, Variant.op_Implicit(0.48f * (1f - num8) * (1f - num8) * (float)Math.Min(1.0, num7 / 0.035)));
			}
		}
	}

	private void Emit(ReturnJourney journey, double at, float seed)
	{
		NativeEchoPool.Item item = NativeEchoPool.TryRentTravel(display.Body, "return");
		if (item != null)
		{
			_echoes.Add(Create(journey, at, seed, sample: true, item));
		}
	}

	private Echo Create(ReturnJourney journey, double at, float seed, bool sample = true, NativeEchoPool.Item? reserved = null)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		Transform2D transform = display.Facing((journey.Home <= journey.From) ? 1 : (-1));
		transform.Origin = new Vector2((float)journey.Position(at), display.Home.Y);
		NativeEchoPool.Item item = reserved ?? NativeEchoPool.Rent(display.Body, "return");
		try
		{
			((Node)item.Root).Name = StringName.op_Implicit("MoreSilentReturnEcho");
			item.Root.Transform = transform;
			((GodotObject)item.Root).SetMeta(StringName.op_Implicit("silent_effect"), Variant.op_Implicit("return-echo"));
			((Node)display.Body).GetParent().MoveChild((Node)(object)item.Root, ((Node)display.Body).GetIndex(false));
			item.Material.SetShaderParameter(SeedUniform, Variant.op_Implicit(seed));
			item.Material.SetShaderParameter(LocalScaleUniform, Variant.op_Implicit(new Vector2(display.ScaleX * (float)((journey.Home <= journey.From) ? 1 : (-1)), 0f - display.Body.Scale.Y)));
			if (sample)
			{
				Sample(item.Pose, journey, at);
			}
			return new Echo(item, at);
		}
		catch
		{
			item.Dispose();
			throw;
		}
	}

	private static void Sample(NativePose pose, ReturnJourney journey, double now)
	{
		(MixedReturnPlan.Step, int, double, string, string) tuple = journey.At(now);
		pose.Shadow(tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item1.Last);
	}

	public void Dispose()
	{
		_marker?.Dispose();
		_marker = null;
		foreach (Echo echo in _echoes)
		{
			echo.Dispose();
		}
		_echoes.Clear();
		foreach (ReturnSmoke item in _smoke)
		{
			item.Dispose();
		}
		_smoke.Clear();
	}
}
internal sealed class BottleRecipe
{
	internal sealed record Trail(Texture2D Texture, Gradient Gradient, Curve WidthCurve, float Width, Color Tint)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Texture = ");
			builder.Append(Texture);
			builder.Append(", Gradient = ");
			builder.Append(Gradient);
			builder.Append(", WidthCurve = ");
			builder.Append(WidthCurve);
			builder.Append(", Width = ");
			builder.Append(Width.ToString());
			builder.Append(", Tint = ");
			Color tint = Tint;
			builder.Append(((object)(Color)(ref tint)/*cast due to .constrained prefix*/).ToString());
			return true;
		}
	}

	internal readonly record struct Drop(Vector2 Position, Vector2 Velocity, Vector2 InitialVelocity, float Size, float Angle, Color Color)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("Position = ");
			Vector2 val = Position;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Velocity = ");
			val = Velocity;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", InitialVelocity = ");
			val = InitialVelocity;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Size = ");
			builder.Append(Size.ToString());
			builder.Append(", Angle = ");
			builder.Append(Angle.ToString());
			builder.Append(", Color = ");
			Color color = Color;
			builder.Append(((object)(Color)(ref color)/*cast due to .constrained prefix*/).ToString());
			return true;
		}
	}

	[CompilerGenerated]
	private sealed class <Walk>d__15 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private Node <>2__current;

		private int <>l__initialThreadId;

		private Node n;

		public Node <>3__n;

		private IEnumerator<Node> <>7__wrap1;

		private IEnumerator<Node> <>7__wrap2;

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
		public <Walk>d__15(int <>1__state)
		{
			this.<>1__state = <>1__state;
			<>l__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = <>1__state;
			if ((uint)(num - -4) <= 1u || num == 2)
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
			<>7__wrap2 = null;
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
					<>2__current = n;
					<>1__state = 1;
					return true;
				case 1:
					<>1__state = -1;
					<>7__wrap1 = n.GetChildren(false).GetEnumerator();
					<>1__state = -3;
					goto IL_00d0;
				case 2:
					{
						<>1__state = -4;
						goto IL_00b6;
					}
					IL_00d0:
					if (<>7__wrap1.MoveNext())
					{
						Node current = <>7__wrap1.Current;
						<>7__wrap2 = Walk(current).GetEnumerator();
						<>1__state = -4;
						goto IL_00b6;
					}
					<>m__Finally1();
					<>7__wrap1 = null;
					return false;
					IL_00b6:
					if (<>7__wrap2.MoveNext())
					{
						Node current2 = <>7__wrap2.Current;
						<>2__current = current2;
						<>1__state = 2;
						return true;
					}
					<>m__Finally2();
					<>7__wrap2 = null;
					goto IL_00d0;
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
			if (<>7__wrap2 != null)
			{
				<>7__wrap2.Dispose();
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
			<Walk>d__15 <Walk>d__;
			if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
			{
				<>1__state = 0;
				<Walk>d__ = this;
			}
			else
			{
				<Walk>d__ = new <Walk>d__15(0);
			}
			<Walk>d__.n = <>3__n;
			return <Walk>d__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	internal readonly Curve Horizontal;

	internal readonly Curve Vertical;

	internal readonly Curve Rotation;

	internal readonly double Duration;

	internal readonly double Height;

	internal readonly double RotationSpeed;

	internal readonly double Gravity;

	internal readonly Texture2D Bottle;

	internal readonly Texture2D Droplet;

	internal readonly Gradient DropColors;

	internal readonly Trail[] Trails;

	private static BottleRecipe? _instance;

	internal static BottleRecipe Get()
	{
		return _instance ?? (_instance = new BottleRecipe());
	}

	[IteratorStateMachine(typeof(<Walk>d__15))]
	private static IEnumerable<Node> Walk(Node n)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <Walk>d__15(-2)
		{
			<>3__n = n
		};
	}

	private static T Read<T>(GodotObject node, string name) where T : GodotObject
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Variant val = node.Get(StringName.op_Implicit(name));
		try
		{
			return (T)(object)((Variant)(ref val)).AsGodotObject();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private static double Number(GodotObject node, string name)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Variant val = node.Get(StringName.op_Implicit(name));
		try
		{
			return ((Variant)(ref val)).AsDouble();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private BottleRecipe()
	{
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Expected O, but got Unknown
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		Node val = ResourceLoader.Load<PackedScene>(NItemThrowVfx.scenePath, (string)null, (CacheMode)1).Instantiate((GenEditState)0);
		try
		{
			Horizontal = Read<Curve>((GodotObject)(object)val, "_horizontalCurve");
			Vertical = Read<Curve>((GodotObject)(object)val, "_verticalCurve");
			Rotation = Read<Curve>((GodotObject)(object)val, "_rotationInfluenceCurve");
			Duration = Number((GodotObject)(object)val, "_flightTime");
			Height = Number((GodotObject)(object)val, "_heightMultiplier");
			RotationSpeed = Number((GodotObject)(object)val, "_rotationMultiplier");
		}
		finally
		{
			((GodotObject)val).Free();
		}
		Bottle = ((PotionModel)ModelDb.Potion<PoisonPotion>()).Image;
		Node val2 = ResourceLoader.Load<PackedScene>("res://scenes/vfx/card_trail_silent.tscn", (string)null, (CacheMode)1).Instantiate((GenEditState)0);
		try
		{
			Trails = (from n in Walk(val2).OfType<Line2D>()
				select new Trail(n.Texture, n.Gradient, n.WidthCurve, n.Width * 0.34f, ((CanvasItem)n).Modulate * n.DefaultColor)).ToArray();
		}
		finally
		{
			((GodotObject)val2).Free();
		}
		Node val3 = ResourceLoader.Load<PackedScene>(NPoisonImpactVfx.scenePath, (string)null, (CacheMode)1).Instantiate((GenEditState)0);
		try
		{
			GpuParticles2D val4 = Walk(val3).OfType<GpuParticles2D>().First((GpuParticles2D p) => ((Node)p).Name == StringName.op_Implicit("vfx_liquid_droplets"));
			ParticleProcessMaterial val5 = (ParticleProcessMaterial)val4.ProcessMaterial;
			Droplet = val4.Texture;
			Gravity = val5.Gravity.Y;
			DropColors = ((GradientTexture1D)(val5.ColorInitialRamp ?? val5.ColorRamp)).Gradient;
		}
		finally
		{
			((GodotObject)val3).Free();
		}
		if (Trails.Length != 2 || Duration <= 0.0)
		{
			throw new InvalidDataException("Original bottle resources changed");
		}
	}

	internal static double Random(double seed)
	{
		double num = Math.Sin(seed * 127.1 + 311.7) * 43758.5453123;
		return num - Math.Floor(num);
	}

	internal Vector2 Position(Vector2 from, Vector2 to, double duration, double age, double playbackRate = 1.0)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		duration *= playbackRate;
		age *= playbackRate;
		double num = ((duration > 0.0) ? Math.Clamp(age / duration, 0.0, 1.0) : 1.0);
		double num2 = Height * Math.Pow(duration / Duration, 2.0) * (double)Vertical.Sample((float)num);
		return ((Vector2)(ref from)).Lerp(to, Horizontal.Sample((float)num)) + Vector2.Up * (float)num2;
	}

	internal float Angle(double duration, double age, double seed, double playbackRate = 1.0)
	{
		duration *= playbackRate;
		age *= playbackRate;
		double num = ((duration > 0.0) ? Math.Clamp(age / duration, 0.0, 1.0) : 1.0);
		double num2 = 0.0;
		for (int i = 0; i < 80; i++)
		{
			num2 += (double)Rotation.Sample((float)(num * ((double)i + 0.5) / 80.0)) * age / 80.0;
		}
		return (float)((Random(seed) * 360.0 + num2 * RotationSpeed) * Math.PI / 180.0);
	}

	internal Vector2 Velocity(Vector2 from, Vector2 to, double duration, double age, double playbackRate = 1.0)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		double num = Math.Min(0.0005, duration / 200.0);
		double num2 = Math.Max(0.0, age - num);
		double num3 = Math.Min(duration, age + num);
		if (!(num3 <= num2))
		{
			return (Position(from, to, duration, num3, playbackRate) - Position(from, to, duration, num2, playbackRate)) / (float)(num3 - num2);
		}
		return Vector2.Zero;
	}

	internal Drop SampleDrop(Vector2 from, Vector2 to, double duration, double age, double seed, int index, double born, double playbackRate = 1.0)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		double num = age - born;
		double num2 = Random(seed + (double)(index * 37));
		Vector2 val = Position(from, to, duration, born, playbackRate);
		Vector2 val2 = Velocity(from, to, duration, born, playbackRate);
		double num3 = ((Vector2)(ref val2)).Length();
		Vector2 val3 = ((num3 > 0.0) ? (new Vector2(0f - val2.Y, val2.X) / (float)num3) : Vector2.Right);
		Vector2 val4 = val2 + val3 * (float)((Random(seed + (double)index + 9.0) - 0.5) * 60.0);
		double num4 = 14.0 + 6.0 * num2 + 0.004 * num3;
		double num5 = Math.Exp((0.0 - num4) * num);
		double num6 = (1.0 - num5) / num4;
		Vector2 velocity = val4 * (float)num5 + Vector2.Down * (float)(Gravity * num6);
		Vector2 position = val + val4 * (float)num6 + Vector2.Down * (float)(Gravity / num4 * (num - num6));
		Color color = DropColors.Sample((float)num2);
		color.A *= (float)((1.0 - num / 0.18) * 0.8);
		return new Drop(position, velocity, val4, (float)(7.0 + num2 * 8.0), ((Vector2)(ref velocity)).Angle() - (float)Math.PI / 2f, color);
	}
}
internal sealed class BottleFlight : IDisposable
{
	private static readonly HashSet<BottleFlight> Active = new HashSet<BottleFlight>();

	private static readonly CanvasItemMaterial Additive = new CanvasItemMaterial
	{
		BlendMode = (BlendModeEnum)1
	};

	private readonly BottleRecipe _recipe;

	private readonly Node2D _root;

	private readonly SceneTree _tree;

	private readonly Sprite2D? _bottle;

	private readonly Line2D[] _lines;

	private readonly Sprite2D[] _drops;

	private readonly Vector2[] _trailPoints = (Vector2[])(object)new Vector2[33];

	private readonly Vector2 _from;

	private readonly Vector2 _to;

	private readonly double _duration;

	private readonly double _seed;

	private readonly double _history;

	private readonly double _rate;

	private readonly double _playbackRate;

	private Action? _onArrival;

	private bool _disposed;

	private bool _arrived;

	private double _age;

	internal static int Count => Active.Count;

	internal double Age => _age;

	internal Vector2 Head => _recipe.Position(_from, _to, _duration, Math.Min(_age, _duration), _playbackRate);

	internal BottleFlight(Node parent, Vector2 from, Vector2 to, double duration, double seed, bool quick)
		: this(parent, from, to, duration, seed, quick, null)
	{
	}//IL_0002: Unknown result type (might be due to invalid IL or missing references)
	//IL_0003: Unknown result type (might be due to invalid IL or missing references)


	internal BottleFlight(Node parent, Vector2 from, Vector2 to, double duration, double seed, bool quick, Action? onArrival)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Expected O, but got Unknown
		//IL_01c8: Expected O, but got Unknown
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Expected O, but got Unknown
		//IL_021a: Expected O, but got Unknown
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Expected O, but got Unknown
		_recipe = BottleRecipe.Get();
		_from = from;
		_to = to;
		_playbackRate = (quick ? 0.5 : 1.0);
		_duration = duration / _playbackRate;
		_seed = seed;
		_onArrival = onArrival;
		_history = (quick ? (duration * 0.416) : 0.16) * 2.0;
		_rate = (quick ? (8.0 / _duration) : 40.0);
		if (duration <= 0.0)
		{
			throw new ArgumentOutOfRangeException("duration");
		}
		_root = new Node2D
		{
			Name = StringName.op_Implicit("MoreSilentBottleFlight")
		};
		parent.AddChild((Node)(object)_root, false, (InternalMode)0);
		_root.GlobalTransform = Transform2D.Identity;
		_tree = parent.GetTree();
		_lines = (Line2D[])(object)new Line2D[_recipe.Trails.Length];
		_drops = (Sprite2D[])(object)new Sprite2D[(int)Math.Ceiling(_duration * _rate)];
		try
		{
			for (int i = 0; i < _lines.Length; i++)
			{
				BottleRecipe.Trail trail = _recipe.Trails[i];
				Line2D[] lines = _lines;
				int num = i;
				Line2D val = new Line2D
				{
					Texture = trail.Texture,
					Gradient = trail.Gradient,
					WidthCurve = trail.WidthCurve,
					Width = trail.Width,
					Modulate = trail.Tint,
					TextureMode = (LineTextureMode)2,
					Material = (Material)(object)Additive,
					TextureFilter = (TextureFilterEnum)2
				};
				Line2D val2 = val;
				lines[num] = val;
				Line2D val3 = val2;
				((Node)_root).AddChild((Node)(object)val3, false, (InternalMode)0);
			}
			for (int j = 0; j < _drops.Length; j++)
			{
				Sprite2D[] drops = _drops;
				int num2 = j;
				Sprite2D val4 = new Sprite2D
				{
					Texture = _recipe.Droplet,
					TextureFilter = (TextureFilterEnum)2
				};
				Sprite2D val5 = val4;
				drops[num2] = val4;
				Sprite2D val6 = val5;
				((Node)_root).AddChild((Node)(object)val6, false, (InternalMode)0);
			}
			if (quick)
			{
				_bottle = new Sprite2D
				{
					Texture = _recipe.Bottle,
					Scale = Vector2.One * (80f / (float)_recipe.Bottle.GetWidth()),
					TextureFilter = (TextureFilterEnum)2
				};
				((Node)_root).AddChild((Node)(object)_bottle, false, (InternalMode)0);
			}
			Draw();
			((Node)_root).TreeExiting += Dispose;
			_tree.ProcessFrame += Frame;
			Active.Add(this);
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	internal void Arrive()
	{
		if (!_arrived)
		{
			_arrived = true;
			Sprite2D? bottle = _bottle;
			if (bottle != null)
			{
				((CanvasItem)bottle).Hide();
			}
			Action? onArrival = _onArrival;
			_onArrival = null;
			onArrival?.Invoke();
		}
	}

	private void Frame()
	{
		if (_disposed || _tree.Paused || !((Node)_root).CanProcess())
		{
			return;
		}
		try
		{
			_age += ((Node)_root).GetProcessDeltaTime();
			if (_age >= _duration)
			{
				Arrive();
			}
			if (_age >= _duration + Math.Max(0.18, _history))
			{
				Dispose();
			}
			else
			{
				Draw();
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			VisualCleanup.Safely(Dispose);
		}
	}

	private void Draw()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		double num = Math.Min(_age, _duration);
		double num2 = Math.Max(0.0, _age - _history);
		double num3 = Math.Max(0.0, 1.0 - Math.Max(0.0, _age - _duration) / _history);
		bool flag = num > num2 + 1E-09;
		if (flag)
		{
			for (int i = 0; i < _trailPoints.Length; i++)
			{
				_trailPoints[i] = _recipe.Position(_from, _to, _duration, num2 + (num - num2) * (double)i / 32.0, _playbackRate);
			}
		}
		Vector2[] points = (flag ? _trailPoints : Array.Empty<Vector2>());
		for (int j = 0; j < _lines.Length; j++)
		{
			_lines[j].Points = points;
			Color tint = _recipe.Trails[j].Tint;
			tint.A *= (float)num3;
			((CanvasItem)_lines[j]).Modulate = tint;
		}
		for (int k = 0; k < _drops.Length; k++)
		{
			Sprite2D val = _drops[k];
			double num4 = (double)k / _rate;
			double num5 = _age - num4;
			((CanvasItem)val).Visible = num5 >= 0.0 && num5 < 0.18;
			if (((CanvasItem)val).Visible)
			{
				BottleRecipe.Drop drop = _recipe.SampleDrop(_from, _to, _duration, _age, _seed, k, num4, _playbackRate);
				((Node2D)val).Position = drop.Position;
				((Node2D)val).Rotation = drop.Angle;
				((CanvasItem)val).Modulate = drop.Color;
				((Node2D)val).Scale = new Vector2(drop.Size, drop.Size * 1.6f) / _recipe.Droplet.GetSize();
			}
		}
		if (_bottle != null)
		{
			((CanvasItem)_bottle).Visible = !_arrived && _age < _duration;
			((Node2D)_bottle).Position = Head;
			((Node2D)_bottle).Rotation = _recipe.Angle(_duration, Math.Min(_age, _duration), _seed, _playbackRate);
		}
	}

	internal static void Reset()
	{
		BottleFlight[] array = Active.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			VisualCleanup.Safely(array[i].Dispose);
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		VisualCleanup.Safely(Arrive);
		if (GodotObject.IsInstanceValid((GodotObject)(object)_tree))
		{
			_tree.ProcessFrame -= Frame;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			((Node)_root).TreeExiting -= Dispose;
			((CanvasItem)_root).Hide();
			if (!((GodotObject)_root).IsQueuedForDeletion())
			{
				((Node)_root).QueueFree();
			}
		}
		Active.Remove(this);
	}
}
internal sealed class CharacterRig : IDisposable
{
	private static readonly ConditionalWeakTable<Node2D, CharacterRig> Bodies = new ConditionalWeakTable<Node2D, CharacterRig>();

	private static readonly GDScript Script = Compile();

	private static readonly StringName CurrentCall = StringName.op_Implicit("current");

	private readonly RefCounted _reader;

	private readonly Node2D _body;

	private bool _disposed;

	internal string? Version { get; }

	internal bool Adaptable { get; }

	internal bool Current
	{
		get
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (_disposed)
			{
				return false;
			}
			Variant val = ((GodotObject)_reader).Call(CurrentCall, Array.Empty<Variant>());
			try
			{
				return ((Variant)(ref val)).AsBool();
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	internal static CharacterRig? For(Node2D body)
	{
		return Find(body, allowSkin: false);
	}

	internal static CharacterRig? ForPresentation(Node2D body)
	{
		return Find(body, allowSkin: true);
	}

	private static CharacterRig? Find(Node2D body, bool allowSkin)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)body) || !((GodotObject)body).HasMethod(StringName.op_Implicit("get_skeleton_data_res")))
		{
			return null;
		}
		if (Bodies.TryGetValue(body, out CharacterRig value))
		{
			if (value.Current)
			{
				if (value.Version == null && (!allowSkin || !value.Adaptable))
				{
					return null;
				}
				return value;
			}
			value.Dispose();
		}
		CharacterRig characterRig = new CharacterRig(body);
		Bodies.Add(body, characterRig);
		if (characterRig.Version == null && (!allowSkin || !characterRig.Adaptable))
		{
			return null;
		}
		return characterRig;
	}

	private CharacterRig(Node2D body)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		_body = body;
		Variant val = ((GodotObject)Script).Call(StringName.op_Implicit("new"), Array.Empty<Variant>());
		try
		{
			_reader = (RefCounted)((Variant)(ref val)).AsGodotObject();
			try
			{
				Variant val2 = ((GodotObject)_reader).Call(StringName.op_Implicit("bind"), (Variant[])(object)new Variant[1] { Variant.op_Implicit((GodotObject)(object)body) });
				try
				{
					string text = MotionBank.VersionFor(((Variant)(ref val2)).AsString());
					if (text != null)
					{
						Variant val3 = ((GodotObject)_reader).Call(StringName.op_Implicit("compatible"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(GameContent.Identity(text).GetProperty("sha256").GetString()) });
						try
						{
							if (((Variant)(ref val3)).AsBool())
							{
								Version = text;
							}
						}
						finally
						{
							((IDisposable)(Variant)(ref val3)/*cast due to .constrained prefix*/).Dispose();
						}
					}
					if (Version == null)
					{
						Variant val4 = ((GodotObject)_reader).Call(StringName.op_Implicit("adaptable"), Array.Empty<Variant>());
						try
						{
							Adaptable = ((Variant)(ref val4)).AsBool();
						}
						finally
						{
							((IDisposable)(Variant)(ref val4)/*cast due to .constrained prefix*/).Dispose();
						}
					}
					((Node)body).TreeExiting += Exited;
				}
				finally
				{
					((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
				}
			}
			catch
			{
				Dispose();
				throw;
			}
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private void Exited()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From((Action)delegate
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)_body) || !((Node)_body).IsInsideTree() || ((GodotObject)_body).IsQueuedForDeletion())
			{
				Dispose();
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	public void Dispose()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_body))
		{
			((Node)_body).TreeExiting -= Exited;
		}
		Bodies.Remove(_body);
		try
		{
			Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("release"), Array.Empty<Variant>());
			try
			{
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
		finally
		{
			((GodotObject)_reader).Dispose();
		}
	}

	private static GDScript Compile()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		using Stream stream = typeof(CharacterRig).Assembly.GetManifestResourceStream("MoreSilentAnimations.src.Core.skin_guard.gd");
		using StreamReader streamReader = new StreamReader(stream);
		GDScript val = new GDScript
		{
			SourceCode = streamReader.ReadToEnd()
		};
		if ((int)((Script)val).Reload(false) != 0)
		{
			throw new InvalidDataException("Skin compatibility bridge");
		}
		return val;
	}
}
internal static class GameContent
{
	private static Resource? _atlas;

	private static Texture2D? _texture;

	private static readonly JsonDocument Identities = Read();

	private static string? _version;

	internal static Texture2D OriginalTexture
	{
		get
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			//IL_005c: Expected O, but got Unknown
			if (_texture != null)
			{
				return _texture;
			}
			_atlas = ResourceLoader.Load<Resource>("res://animations/characters/silent/silent.atlas", (string)null, (CacheMode)1);
			Variant val = ((GodotObject)_atlas).Call(StringName.op_Implicit("get_textures"), Array.Empty<Variant>());
			try
			{
				Array val2 = ((Variant)(ref val)).AsGodotArray();
				try
				{
					Variant val3 = val2[0];
					Texture2D val4 = (Texture2D)((Variant)(ref val3)).AsGodotObject();
					_texture = val4;
					return val4;
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	internal static string Version
	{
		get
		{
			if (_version != null)
			{
				return _version;
			}
			foreach (JsonProperty item in Identities.RootElement.GetProperty("versions").EnumerateObject())
			{
				if (FileAccess.GetSha256(item.Value.GetProperty("path").GetString()) == item.Value.GetProperty("sha256").GetString())
				{
					return _version = item.Name;
				}
			}
			throw new InvalidDataException("Unsupported installed Silent source version");
		}
	}

	private static JsonDocument Read()
	{
		using Stream utf8Json = typeof(GameContent).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets.rig-identity.json");
		return JsonDocument.Parse(utf8Json);
	}

	internal static JsonElement Identity(string version)
	{
		return Identities.RootElement.GetProperty("versions").GetProperty(version);
	}
}
internal sealed class DepartureTrail(DisplayLease display) : IDisposable
{
	private sealed record Ghost(NativeEchoPool.Item Item, float Fraction, float Opacity, int Facing)
	{
		internal Node2D Root => Item.Root;

		internal ShaderMaterial Material => Item.Material;

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Item = ");
			builder.Append(Item);
			builder.Append(", Fraction = ");
			builder.Append(Fraction.ToString());
			builder.Append(", Opacity = ");
			builder.Append(Opacity.ToString());
			builder.Append(", Facing = ");
			builder.Append(Facing.ToString());
			return true;
		}
	}

	private sealed class Burst : IDisposable
	{
		internal double At;

		internal float From;

		internal float To;

		internal float Seed;

		internal float Height;

		internal float Bottom;

		internal bool Arrival;

		internal readonly List<Ghost> Ghosts = new List<Ghost>();

		internal MeshInstance2D? Smoke;

		internal ShaderMaterial? Material;

		internal QuadMesh? Mesh;

		public void Dispose()
		{
			foreach (Ghost ghost in Ghosts)
			{
				ghost.Item.Dispose();
			}
			Ghosts.Clear();
			if (Smoke != null)
			{
				((CanvasItem)Smoke).Hide();
				((Node)Smoke).QueueFree();
			}
			ShaderMaterial? material = Material;
			if (material != null)
			{
				((GodotObject)material).Dispose();
			}
			QuadMesh? mesh = Mesh;
			if (mesh != null)
			{
				((GodotObject)mesh).Dispose();
			}
		}
	}

	private static readonly StringName AgeUniform = StringName.op_Implicit("age");

	private static readonly StringName AgeEndUniform = StringName.op_Implicit("age_end");

	private static readonly StringName BasisXUniform = StringName.op_Implicit("basis_x");

	private static readonly StringName BasisYUniform = StringName.op_Implicit("basis_y");

	private static readonly StringName LocalScaleUniform = StringName.op_Implicit("local_scale");

	private static readonly StringName OpacityUniform = StringName.op_Implicit("opacity");

	private static readonly StringName OriginUniform = StringName.op_Implicit("origin");

	private static readonly StringName SeedUniform = StringName.op_Implicit("seed");

	private static readonly StringName WorldSmokeUniform = StringName.op_Implicit("world_smoke");

	private readonly List<Burst> _bursts = new List<Burst>();

	internal bool Active => _bursts.Count > 0;

	internal void Begin(string kind, double from, double to, int homeFacing, double at, int attackFacing = 1)
	{
		AddBurst(kind, from, to, homeFacing, at, attackFacing, arrival: false);
		if (kind == "DownStab" && !display.Pose.IsSkin)
		{
			AddBurst(kind, to, to, homeFacing, at, attackFacing, arrival: true);
		}
	}

	private void AddBurst(string kind, double from, double to, int homeFacing, double at, int attackFacing, bool arrival)
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected O, but got Unknown
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Expected O, but got Unknown
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		if (arrival)
		{
			for (int num = _bursts.Count - 1; num >= 0; num--)
			{
				if (_bursts[num].Arrival && Math.Abs((double)_bursts[num].To - to) < (double)(_bursts[num].Height * 0.25f))
				{
					_bursts[num].Dispose();
					_bursts.RemoveAt(num);
				}
			}
		}
		Burst burst = new Burst
		{
			From = (float)from,
			To = (float)to,
			At = at,
			Arrival = arrival,
			Seed = (float)((at * 97.13 + ((kind == "DownStab") ? 9.1 : 3.7)) % 17.0)
		};
		Node parent = ((Node)display.Body).GetParent();
		int num2 = ((to >= from) ? 1 : (-1));
		try
		{
			if (!arrival)
			{
				burst.Material = new ShaderMaterial
				{
					Shader = ReturnSmoke.Shader
				};
				burst.Material.SetShaderParameter(WorldSmokeUniform, Variant.op_Implicit(false));
				burst.Material.SetShaderParameter(SeedUniform, Variant.op_Implicit(burst.Seed));
				burst.Mesh = new QuadMesh
				{
					Size = Vector2.One
				};
				burst.Smoke = new MeshInstance2D
				{
					Name = StringName.op_Implicit("MoreSilentDepartureSmoke"),
					Mesh = (Mesh)(object)burst.Mesh,
					Material = (Material)(object)burst.Material
				};
				parent.AddChild((Node)(object)burst.Smoke, false, (InternalMode)0);
				parent.MoveChild((Node)(object)burst.Smoke, 0);
				display.AttachAttack((Node2D)(object)burst.Smoke, 5);
				((GodotObject)burst.Smoke).SetMeta(StringName.op_Implicit("silent_effect"), Variant.op_Implicit("departure-smoke"));
			}
			float[] array = new float[3] { 0f, 0.3f, 0.64f };
			float[] array2 = new float[3] { 0.62f, 0.43f, 0.27f };
			for (int i = 0; i < 3; i++)
			{
				if (display.Pose.IsSkin && i == 1)
				{
					continue;
				}
				NativeEchoPool.Item item = NativeEchoPool.TryRentTravel(display.Body, kind + ":" + i);
				if (item == null)
				{
					continue;
				}
				try
				{
					Node2D root = item.Root;
					((Node)root).Name = StringName.op_Implicit("MoreSilentDepartureEcho");
					parent.MoveChild((Node)(object)root, ((Node)display.Body).GetIndex(false));
					display.AttachAttack(root, 10);
					((GodotObject)root).SetMeta(StringName.op_Implicit("silent_effect"), Variant.op_Implicit(arrival ? "down-stab-arrival-echo" : "departure-echo"));
					((GodotObject)root).SetMeta(StringName.op_Implicit("silent_departure_kind"), Variant.op_Implicit(kind));
					((GodotObject)root).SetMeta(StringName.op_Implicit("silent_prefab_index"), Variant.op_Implicit(i));
					if (!item.Prepared)
					{
						item.Dimensions = item.Pose.Departure(kind, i);
						item.Prepared = true;
					}
					Vector2 dimensions = item.Dimensions;
					if (i == 0)
					{
						burst.Height = dimensions.X * Math.Abs(display.Body.Scale.Y);
						burst.Bottom = dimensions.Y * Math.Abs(display.Body.Scale.Y);
					}
					burst.Ghosts.Add(new Ghost(item, array[i], array2[i], (!arrival) ? ((i == 0) ? homeFacing : ((i == 2 && kind == "DownStab") ? (-attackFacing) : num2)) : ((i == 1) ? 1 : (-1))));
				}
				catch
				{
					item.Dispose();
					throw;
				}
			}
			if (kind == "DownStab" && !arrival && Math.Abs(to - from) < (double)(burst.Height * 0.25f))
			{
				foreach (Ghost ghost in burst.Ghosts)
				{
					ghost.Item.Dispose();
				}
				burst.Ghosts.Clear();
			}
			_bursts.Add(burst);
			while (_bursts.Count > 12)
			{
				_bursts[0].Dispose();
				_bursts.RemoveAt(0);
			}
			Update(at);
		}
		catch
		{
			burst.Dispose();
			throw;
		}
	}

	internal void Update(double now)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		Vector2 val2 = default(Vector2);
		for (int num = _bursts.Count - 1; num >= 0; num--)
		{
			Burst burst = _bursts[num];
			double num2 = now - burst.At;
			if (num2 >= 0.5)
			{
				burst.Dispose();
				_bursts.RemoveAt(num);
			}
			else
			{
				float num3 = (float)Math.Max(0.0, num2 / 0.5);
				float num4 = burst.To - burst.From;
				int num5 = ((num4 >= 0f) ? 1 : (-1));
				float num6 = burst.Height * 0.85f;
				float num7 = burst.Bottom + burst.Height * 0.52f;
				((Vector2)(ref val))..ctor(num4, 0f);
				((Vector2)(ref val2))..ctor(0f, (float)(-num5) * num6);
				Vector2 val3 = new Vector2(burst.From, display.Home.Y - num7) - val2 * 0.5f;
				if (burst.Smoke != null)
				{
					((Node2D)burst.Smoke).Transform = new Transform2D(val, val2, val3 + (val + val2) * 0.5f);
					((CanvasItem)burst.Smoke).Visible = Math.Abs(num4) > 1f;
					burst.Material.SetShaderParameter(AgeUniform, Variant.op_Implicit(num3));
					burst.Material.SetShaderParameter(AgeEndUniform, Variant.op_Implicit(num3));
					burst.Material.SetShaderParameter(OpacityUniform, Variant.op_Implicit(0.64f * (1f - Smooth(num3))));
					burst.Material.SetShaderParameter(OriginUniform, Variant.op_Implicit(val3));
					burst.Material.SetShaderParameter(BasisXUniform, Variant.op_Implicit(val));
					burst.Material.SetShaderParameter(BasisYUniform, Variant.op_Implicit(val2));
				}
				foreach (Ghost ghost in burst.Ghosts)
				{
					float num8 = (float)((num2 - (double)ghost.Fraction * 0.055) / (0.5 - (double)ghost.Fraction * 0.055));
					((CanvasItem)ghost.Root).Visible = num8 >= 0f;
					if (!(num8 < 0f))
					{
						float num9 = 0.12f + ghost.Fraction * 0.38f + num8 * 0.14f;
						Transform2D transform = display.Facing(ghost.Facing);
						if (burst.Arrival)
						{
							ref Vector2 y = ref transform.Y;
							y *= 1f + num9;
							transform.Origin = new Vector2(burst.To, display.Home.Y + burst.Height * (ghost.Fraction * 0.04f + num8 * 0.035f) - num8 * 14f);
						}
						else
						{
							ref Vector2 x = ref transform.X;
							x *= 1f + num9;
							transform.Origin = new Vector2(burst.From + num4 * ghost.Fraction - (float)num5 * num8 * 14f, display.Home.Y - num8 * burst.Height * 0.035f);
						}
						ghost.Root.Transform = transform;
						ghost.Material.SetShaderParameter(AgeUniform, Variant.op_Implicit(num8));
						ghost.Material.SetShaderParameter(OpacityUniform, Variant.op_Implicit(ghost.Opacity * (1f - Smooth(num8))));
						ghost.Material.SetShaderParameter(SeedUniform, Variant.op_Implicit(burst.Seed + ghost.Fraction * 7f));
						ghost.Material.SetShaderParameter(LocalScaleUniform, Variant.op_Implicit(new Vector2(display.ScaleX * (float)ghost.Facing * (burst.Arrival ? 1f : (1f + num9)), (0f - display.Body.Scale.Y) * (burst.Arrival ? (1f + num9) : 1f))));
					}
				}
			}
		}
	}

	private static float Smooth(float value)
	{
		float num = Math.Clamp(value, 0f, 1f);
		return num * num * (3f - 2f * num);
	}

	public void Dispose()
	{
		foreach (Burst burst in _bursts)
		{
			burst.Dispose();
		}
		_bursts.Clear();
	}
}
internal sealed class DisplayLease : IDisposable
{
	private readonly NCreature _actor;

	private readonly Node2D _body;

	private readonly Node _parent;

	internal readonly Node2D Foreground;

	private Color _before;

	private Color _hidden;

	private int _externalTintDepth;

	private bool _disposed;

	private bool _owned;

	private HiddenBodyMeshes? _hiddenMeshes;

	private readonly NativeEchoPool.Item _item;

	internal readonly Node2D Root;

	internal readonly NativePose Pose;

	internal readonly Vector2 Home;

	internal Node2D Body => _body;

	internal float Opacity { get; private set; } = 1f;

	internal bool Current
	{
		get
		{
			if (!_disposed && GodotObject.IsInstanceValid((GodotObject)(object)_actor) && GodotObject.IsInstanceValid((GodotObject)(object)_body) && GodotObject.IsInstanceValid((GodotObject)(object)Root) && _actor.Body == _body && ((Node)_body).GetParent() == _parent && (((Node)Root).GetParent() == _parent || ((Node)Root).GetParent() == Foreground) && ((Node)Foreground).IsInsideTree() && ((Node)Root).IsInsideTree() && !((GodotObject)_body).IsQueuedForDeletion())
			{
				return Pose.Current;
			}
			return false;
		}
	}

	internal int NativeFacing
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			if (!(_body.Transform.X.X < 0f))
			{
				return 1;
			}
			return -1;
		}
	}

	internal float ScaleX
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Transform2D transform = _body.Transform;
			return ((Vector2)(ref transform.X)).Length();
		}
	}

	internal DisplayLease(NCreature actor)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Expected O, but got Unknown
		_actor = actor;
		_body = actor.Body;
		_parent = ((Node)_body).GetParent();
		_before = ((CanvasItem)_body).Modulate;
		Color before = _before;
		before.A = 0f;
		_hidden = before;
		_item = NativeEchoPool.Rent(_body, "body");
		Root = _item.Root;
		Pose = _item.Pose;
		((Node)Root).Name = StringName.op_Implicit("MoreSilentDisplay");
		Root.Transform = _body.Transform;
		Node2D root = Root;
		before = _before;
		before.A = 0f;
		((CanvasItem)root).Modulate = before;
		((CanvasItem)Root).TextureFilter = ((CanvasItem)_body).TextureFilter;
		((CanvasItem)Root).Visible = true;
		((CanvasItem)Root).ZIndex = ((CanvasItem)_body).ZIndex;
		((CanvasItem)Root).ZAsRelative = ((CanvasItem)_body).ZAsRelative;
		_parent.MoveChild((Node)(object)Root, ((Node)_body).GetIndex(false) + 1);
		Home = _body.Position;
		Foreground = new Node2D
		{
			Name = StringName.op_Implicit("MoreSilentCombatPresentation")
		};
		NCombatRoom instance = NCombatRoom.Instance;
		Control obj = ((instance != null) ? instance.CombatVfxContainer : null) ?? throw new InvalidOperationException("Display outside combat");
		((Node)obj).AddChild((Node)(object)Foreground, false, (InternalMode)0);
		((Node)obj).MoveChild((Node)(object)Foreground, 0);
		UpdateForeground();
	}

	internal void Acquire()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (!_owned)
		{
			Color val = _before * ((CanvasItem)_body).SelfModulate;
			Node2D root = Root;
			Color modulate = val;
			modulate.A = val.A * Opacity;
			((CanvasItem)root).Modulate = modulate;
			((CanvasItem)_body).Modulate = _hidden;
			_owned = true;
			if (_hiddenMeshes == null)
			{
				_hiddenMeshes = HiddenBodyMeshes.Acquire(_body);
			}
		}
	}

	internal bool BeginExternalTint()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed || !GodotObject.IsInstanceValid((GodotObject)(object)_body))
		{
			return false;
		}
		if (_externalTintDepth++ == 0 && _owned)
		{
			((CanvasItem)_body).Modulate = _before;
		}
		return true;
	}

	internal void EndExternalTint()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (_externalTintDepth != 0 && --_externalTintDepth == 0 && !_disposed && GodotObject.IsInstanceValid((GodotObject)(object)_body))
		{
			_before = ((CanvasItem)_body).Modulate;
			Color before = _before;
			before.A = 0f;
			_hidden = before;
			if (_owned)
			{
				((CanvasItem)_body).Modulate = _hidden;
				Color val = _before * ((CanvasItem)_body).SelfModulate;
				Node2D root = Root;
				before = val;
				before.A = val.A * Opacity;
				((CanvasItem)root).Modulate = before;
			}
		}
	}

	internal void Place(float x, int facing, float opacity = 1f)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		_hiddenMeshes?.CheckVisibility();
		UpdateForeground();
		Transform2D transform = Facing(facing);
		transform.Origin = new Vector2(x, Home.Y);
		Root.Transform = transform;
		Opacity = Math.Clamp(opacity, 0f, 1f);
		Color val = _before * ((CanvasItem)_body).SelfModulate;
		Node2D root = Root;
		Color modulate = val;
		modulate.A = val.A * Opacity;
		((CanvasItem)root).Modulate = modulate;
	}

	internal Transform2D Facing(int facing)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Transform2D transform = _body.Transform;
		ref Vector2 x = ref transform.X;
		x *= (float)(NativeFacing * facing);
		return transform;
	}

	private void UpdateForeground()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Foreground.GlobalTransform = ((CanvasItem)_parent).GetGlobalTransform();
	}

	internal void AttachAttack(Node2D node, int order)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		UpdateForeground();
		PresentationLayer.Local((CanvasItem)(object)node);
		((GodotObject)node).SetMeta(StringName.op_Implicit("silent_local_order"), Variant.op_Implicit(order));
		if (((Node)node).GetParent() == null)
		{
			((Node)Foreground).AddChild((Node)(object)node, false, (InternalMode)0);
		}
		else if (((Node)node).GetParent() != Foreground)
		{
			((Node)node).Reparent((Node)(object)Foreground, false);
		}
		int num = 0;
		foreach (Node child in ((Node)Foreground).GetChildren(false))
		{
			if (child != node)
			{
				Variant meta = ((GodotObject)child).GetMeta(StringName.op_Implicit("silent_local_order"), Variant.op_Implicit(0));
				if (((Variant)(ref meta)).AsInt32() <= order)
				{
					num++;
				}
			}
		}
		((Node)Foreground).MoveChild((Node)(object)node, num);
	}

	internal void TargetLayer(NCreature target)
	{
	}

	internal void AttackLayer(bool active)
	{
		if (active)
		{
			if (((Node)Root).GetParent() != Foreground)
			{
				AttachAttack(Root, 20);
			}
			return;
		}
		if (((Node)Root).GetParent() != _parent)
		{
			((Node)Root).Reparent(_parent, false);
			_parent.MoveChild((Node)(object)Root, ((Node)_body).GetIndex(false) + 1);
		}
		((CanvasItem)Root).ZIndex = ((CanvasItem)_body).ZIndex;
		((CanvasItem)Root).ZAsRelative = ((CanvasItem)_body).ZAsRelative;
	}

	internal Vector2 ParentPoint(Vector2 global)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Transform2D transform = Root.Transform;
		Transform2D globalTransform = Root.GlobalTransform;
		return transform * (((Transform2D)(ref globalTransform)).AffineInverse() * global);
	}

	internal Vector2 MapPoint(Vector2 native)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Transform2D globalTransform = Root.GlobalTransform;
		Transform2D globalTransform2 = _body.GlobalTransform;
		return globalTransform * (((Transform2D)(ref globalTransform2)).AffineInverse() * native);
	}

	private void ReleaseHiddenMeshes()
	{
		HiddenBodyMeshes hiddenMeshes = _hiddenMeshes;
		_hiddenMeshes = null;
		if (hiddenMeshes != null)
		{
			VisualCleanup.Safely(hiddenMeshes.Dispose);
		}
	}

	internal void Release()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		ReleaseHiddenMeshes();
		if (_owned && GodotObject.IsInstanceValid((GodotObject)(object)_body) && ((CanvasItem)_body).Modulate == _hidden)
		{
			((CanvasItem)_body).Modulate = _before;
		}
		_owned = false;
		Node2D root = Root;
		Color modulate = ((CanvasItem)Root).Modulate;
		modulate.A = 0f;
		((CanvasItem)root).Modulate = modulate;
	}

	public void Dispose()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		ReleaseHiddenMeshes();
		if (_owned && GodotObject.IsInstanceValid((GodotObject)(object)_body) && ((CanvasItem)_body).Modulate == _hidden)
		{
			((CanvasItem)_body).Modulate = _before;
		}
		try
		{
			_item.Dispose();
		}
		finally
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)Foreground))
			{
				((CanvasItem)Foreground).Hide();
				((Node)Foreground).QueueFree();
			}
		}
	}
}
internal sealed class DownStrikeRecipe
{
	internal sealed record Source(Texture2D Texture, Texture2D Lut, Curve Frames, Curve? Alpha, double Lifetime, double Randomness, double Fps, double Speed);

	internal sealed record Blade(int Index, int Frame, double Life, double Duration, Vector2 Position, Vector2 Size, float Angle, float Alpha, Vector2 Pivot)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Index = ");
			builder.Append(Index.ToString());
			builder.Append(", Frame = ");
			builder.Append(Frame.ToString());
			builder.Append(", Life = ");
			builder.Append(Life.ToString());
			builder.Append(", Duration = ");
			builder.Append(Duration.ToString());
			builder.Append(", Position = ");
			Vector2 val = Position;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Size = ");
			val = Size;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Angle = ");
			builder.Append(Angle.ToString());
			builder.Append(", Alpha = ");
			builder.Append(Alpha.ToString());
			builder.Append(", Pivot = ");
			val = Pivot;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			return true;
		}
	}

	[CompilerGenerated]
	private sealed class <<Read>g__Walk|7_0>d : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private Node <>2__current;

		private int <>l__initialThreadId;

		private Node n;

		public Node <>3__n;

		private IEnumerator<Node> <>7__wrap1;

		private IEnumerator<Node> <>7__wrap2;

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
		public <<Read>g__Walk|7_0>d(int <>1__state)
		{
			this.<>1__state = <>1__state;
			<>l__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = <>1__state;
			if ((uint)(num - -4) <= 1u || num == 2)
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
			<>7__wrap2 = null;
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
					<>2__current = n;
					<>1__state = 1;
					return true;
				case 1:
					<>1__state = -1;
					<>7__wrap1 = n.GetChildren(false).GetEnumerator();
					<>1__state = -3;
					goto IL_00d0;
				case 2:
					{
						<>1__state = -4;
						goto IL_00b6;
					}
					IL_00d0:
					if (<>7__wrap1.MoveNext())
					{
						Node current = <>7__wrap1.Current;
						<>7__wrap2 = <Read>g__Walk|7_0(current).GetEnumerator();
						<>1__state = -4;
						goto IL_00b6;
					}
					<>m__Finally1();
					<>7__wrap1 = null;
					return false;
					IL_00b6:
					if (<>7__wrap2.MoveNext())
					{
						Node current2 = <>7__wrap2.Current;
						<>2__current = current2;
						<>1__state = 2;
						return true;
					}
					<>m__Finally2();
					<>7__wrap2 = null;
					goto IL_00d0;
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
			if (<>7__wrap2 != null)
			{
				<>7__wrap2.Dispose();
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
			<<Read>g__Walk|7_0>d <<Read>g__Walk|7_0>d;
			if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
			{
				<>1__state = 0;
				<<Read>g__Walk|7_0>d = this;
			}
			else
			{
				<<Read>g__Walk|7_0>d = new <<Read>g__Walk|7_0>d(0);
			}
			<<Read>g__Walk|7_0>d.n = <>3__n;
			return <<Read>g__Walk|7_0>d;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	internal readonly Source Ribbon;

	internal readonly Source Hook;

	private static DownStrikeRecipe? _instance;

	internal static DownStrikeRecipe Get()
	{
		return _instance ?? (_instance = new DownStrikeRecipe());
	}

	private DownStrikeRecipe()
	{
		Ribbon = Read("res://scenes/vfx/vfx_grand_finale_impact.tscn", "res://images/vfx/ribbon_flipbook/ribbon_flipbook_1.png");
		Hook = Read("res://scenes/vfx/slash/vfx_slash_core.tscn", "res://images/vfx/slash/slash_flipbook.png")with
		{
			Lut = Ribbon.Lut
		};
	}

	private static Source Read(string scene, string texture)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		Node val = (ResourceLoader.Load<PackedScene>(scene, (string)null, (CacheMode)1) ?? throw new InvalidDataException("Missing native scene: " + scene)).Instantiate((GenEditState)0);
		try
		{
			GpuParticles2D val2 = Walk(val).OfType<GpuParticles2D>().First(delegate(GpuParticles2D p)
			{
				Texture2D texture2 = p.Texture;
				return ((texture2 != null) ? ((Resource)texture2).ResourcePath : null) == texture && ((CanvasItem)p).Material is ShaderMaterial;
			});
			ShaderMaterial val3 = (ShaderMaterial)((CanvasItem)val2).Material;
			ParticleProcessMaterial val4 = (ParticleProcessMaterial)val2.ProcessMaterial;
			Variant shaderParameter = val3.GetShaderParameter(StringName.op_Implicit("lut"));
			try
			{
				Variant shaderParameter2 = val3.GetShaderParameter(StringName.op_Implicit("flipbook_curve"));
				try
				{
					Texture2D texture3 = val2.Texture;
					Texture2D val5 = (Texture2D)((Variant)(ref shaderParameter)).AsGodotObject();
					Curve curve = ((CurveTexture)((Variant)(ref shaderParameter2)).AsGodotObject()).Curve;
					Texture2D alphaCurve = val4.AlphaCurve;
					Texture2D obj = ((alphaCurve is CurveTexture) ? alphaCurve : null);
					return new Source(texture3, val5, curve, (obj != null) ? ((CurveTexture)obj).Curve : null, val2.Lifetime, val4.LifetimeRandomness, val2.FixedFps, val2.SpeedScale);
				}
				finally
				{
					((IDisposable)(Variant)(ref shaderParameter2)/*cast due to .constrained prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)(Variant)(ref shaderParameter)/*cast due to .constrained prefix*/).Dispose();
			}
		}
		finally
		{
			((GodotObject)val).Free();
		}
		[IteratorStateMachine(typeof(<<Read>g__Walk|7_0>d))]
		static IEnumerable<Node> Walk(Node n)
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <<Read>g__Walk|7_0>d(-2)
			{
				<>3__n = n
			};
		}
	}

	private static double Random(double seed)
	{
		double num = Math.Sin(seed * 127.1 + 311.7) * 43758.5453123;
		return num - Math.Floor(num);
	}

	internal IReadOnlyList<Blade> Sample(string card, double age, double width, double seed)
	{
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		bool flag = card == "TheHunt";
		double num = (flag ? Hook.Lifetime : ((card == "Suppress") ? 0.25 : 0.2));
		if (age < 0.0 || age >= num - (flag ? 1E-09 : 0.0))
		{
			return Array.Empty<Blade>();
		}
		Source source = (flag ? Hook : Ribbon);
		double num2 = Math.Floor((age + 1E-09) * source.Fps) / source.Fps * (flag ? 1.0 : source.Speed);
		double num3 = Math.Clamp(width * 2.2, 420.0, 540.0) * (flag ? 0.8 : 1.0);
		double num4 = Math.PI * 7.0 / 30.0;
		List<Blade> list = new List<Blade>();
		Vector2 position = default(Vector2);
		for (int i = 0; i < ((!(card == "Suppress")) ? 1 : 2); i++)
		{
			double num5 = source.Lifetime * (flag ? 1.0 : (1.0 - source.Randomness * Random(seed + (double)(i * 37) + 7.0)));
			double num6 = num2 - (double)i * 0.05;
			if (!(num6 < 0.0) && !(num6 >= num5))
			{
				double num7 = num6 / num5;
				int frame = Math.Min(5, (int)Math.Floor((double)(source.Frames.Sample((float)num7) * 6f) * 0.99));
				double num8 = (num7 - (flag ? 0.4 : 0.42)) * num3 * (flag ? 0.28 : 0.32);
				((Vector2)(ref position))..ctor((float)((flag ? (num3 * 0.06) : 0.0) - Math.Sin(num4) * num8), (float)(Math.Cos(num4) * num8 * (double)((i != 1) ? 1 : (-1))));
				Vector2 size = (Vector2)(flag ? new Vector2((float)(num3 * 0.76), (float)(num3 * 1.05)) : (new Vector2((float)(num3 * 0.5), (float)(num3 * 1.9)) * (float)((i == 1) ? 1 : (-1))));
				list.Add(new Blade(i, frame, num7, num5, position, size, (float)(num4 * (double)((i != 1) ? 1 : (-1))), flag ? source.Alpha.Sample((float)num7) : 1f, (Vector2)(flag ? Vector2.Zero : new Vector2(-0.1f, 0.04f))));
			}
		}
		return list;
	}

	[IteratorStateMachine(typeof(<<Read>g__Walk|7_0>d))]
	[CompilerGenerated]
	internal static IEnumerable<Node> <Read>g__Walk|7_0(Node n)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <<Read>g__Walk|7_0>d(-2)
		{
			<>3__n = n
		};
	}
}
internal sealed class DownStrikeEffects : IDisposable
{
	internal sealed class Burst : IDisposable
	{
		private readonly Node2D _root;

		private readonly Sprite2D[] _sprites;

		private readonly ShaderMaterial[] _materials;

		private readonly DownStrikeRecipe _recipe;

		private readonly string _card;

		private readonly double _width;

		private readonly double _seed;

		private readonly double _command;

		private double _hit;

		private bool _disposed;

		internal bool Confirmed { get; private set; }

		internal bool Alive => !_disposed;

		internal Burst(DisplayLease display, string card, Vector2 center, double width, double seed, double command, double hit, int facing)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Expected O, but got Unknown
			//IL_00ed: Expected O, but got Unknown
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Expected O, but got Unknown
			//IL_0152: Expected O, but got Unknown
			_recipe = DownStrikeRecipe.Get();
			_card = card;
			_width = width;
			_seed = seed;
			_command = command;
			_hit = hit;
			_root = new Node2D
			{
				Name = StringName.op_Implicit("MoreSilentDownStrike"),
				Position = center,
				Scale = new Vector2((float)facing, 1f)
			};
			display.AttachAttack(_root, 30);
			int num = ((!(card == "Suppress")) ? 1 : 2);
			_sprites = (Sprite2D[])(object)new Sprite2D[num];
			_materials = (ShaderMaterial[])(object)new ShaderMaterial[num];
			try
			{
				DownStrikeRecipe.Source source = ((card == "TheHunt") ? _recipe.Hook : _recipe.Ribbon);
				for (int i = 0; i < num; i++)
				{
					ShaderMaterial[] materials = _materials;
					int num2 = i;
					ShaderMaterial val = new ShaderMaterial
					{
						Shader = Shader
					};
					ShaderMaterial val2 = val;
					materials[num2] = val;
					ShaderMaterial val3 = val2;
					val3.SetShaderParameter(StringName.op_Implicit("lut"), Variant.op_Implicit((GodotObject)(object)source.Lut));
					Sprite2D[] sprites = _sprites;
					int num3 = i;
					Sprite2D val4 = new Sprite2D
					{
						Name = StringName.op_Implicit("NativeFlipbook" + i),
						Texture = source.Texture,
						Material = (Material)(object)val3,
						TextureFilter = (TextureFilterEnum)2
					};
					Sprite2D val5 = val4;
					sprites[num3] = val4;
					Sprite2D val6 = val5;
					((Node)_root).AddChild((Node)(object)val6, false, (InternalMode)0);
				}
				Update(command);
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		internal void Hit(double time)
		{
			Confirmed = true;
			_hit = time;
			Update(time);
		}

		internal void Update(double time)
		{
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0193: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			if (_disposed)
			{
				return;
			}
			double num = _hit - ((_card == "TheHunt") ? (1.0 / 12.0) : 0.0);
			double num2 = ((_card == "TheHunt") ? 0.3 : ((_card == "Suppress") ? 0.25 : 0.2));
			if (!GodotObject.IsInstanceValid((GodotObject)(object)_root) || !((Node)_root).IsInsideTree() || (Confirmed && time >= num + num2))
			{
				Dispose();
				return;
			}
			Sprite2D[] sprites = _sprites;
			for (int i = 0; i < sprites.Length; i++)
			{
				((CanvasItem)sprites[i]).Hide();
			}
			if (time < _command || (_card != "TheHunt" && !Confirmed))
			{
				return;
			}
			foreach (DownStrikeRecipe.Blade item in _recipe.Sample(_card, time - num, _width, _seed))
			{
				Sprite2D obj = _sprites[item.Index];
				Vector2 size = obj.Texture.GetSize();
				((Node2D)obj).Position = item.Position;
				((Node2D)obj).Rotation = item.Angle;
				((Node2D)obj).Scale = item.Size / size;
				obj.Offset = item.Pivot * size;
				Color white = Colors.White;
				white.A = item.Alpha;
				((CanvasItem)obj).Modulate = white;
				_materials[item.Index].SetShaderParameter(StringName.op_Implicit("frame"), Variant.op_Implicit(item.Frame));
				((CanvasItem)obj).Show();
			}
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			if (GodotObject.IsInstanceValid((GodotObject)(object)_root))
			{
				((CanvasItem)_root).Hide();
				((Node)_root).QueueFree();
			}
			ShaderMaterial[] materials = _materials;
			foreach (ShaderMaterial obj in materials)
			{
				if (obj != null)
				{
					((GodotObject)obj).Dispose();
				}
			}
		}
	}

	private static readonly Shader Shader = new Shader
	{
		Code = "shader_type canvas_item;\nrender_mode blend_mix;\nuniform sampler2D lut;\nuniform float frame = 0.0;\nvarying vec4 tint;\nvoid vertex(){tint=COLOR;}\nvoid fragment(){\n    vec2 cell=vec2(mod(frame,3.0),floor(frame/3.0));\n    vec2 uv=(clamp(UV,vec2(.003),vec2(.997))+cell)/vec2(3.0,2.0);\n    vec4 mask=texture(TEXTURE,uv);vec4 color=texture(lut,mask.rr);\n    COLOR=vec4(color.rgb*tint.rgb,mask.a*tint.a);\n}"
	};

	private readonly DisplayLease _display;

	private readonly List<Burst> _bursts = new List<Burst>();

	internal int Count => _bursts.Count((Burst b) => b.Alive);

	internal DownStrikeEffects(DisplayLease display)
	{
		_display = display;
	}

	internal Burst Begin(string card, Vector2 center, double width, double seed, double command, double hit, int facing = 1)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Burst burst = new Burst(_display, card, center, width, seed, command, hit, facing);
		_bursts.Add(burst);
		return burst;
	}

	internal void Update(double time)
	{
		foreach (Burst burst in _bursts)
		{
			burst.Update(time);
		}
		_bursts.RemoveAll((Burst b) => !b.Alive);
	}

	public void Dispose()
	{
		foreach (Burst burst in _bursts)
		{
			burst.Dispose();
		}
		_bursts.Clear();
	}
}
internal static class EffectPosition
{
	internal sealed record Scope(ActorPresentation? Session, Scope? Previous);

	[ThreadStatic]
	internal static Scope? Current;

	internal static void Exit(Scope? owned)
	{
		if ((object)owned != null && (object)Current == owned)
		{
			Current = owned.Previous;
		}
	}

	internal static Scope Enter(Creature target)
	{
		NCreature creatureNode = target.GetCreatureNode();
		return Current = new Scope((creatureNode != null) ? ActorPresentation.For(creatureNode) : null, Current);
	}

	internal static void Spark(Node2D effect, NCreature? actor)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (actor != null)
		{
			ActorPresentation actorPresentation = ActorPresentation.For(actor);
			if (actorPresentation != null)
			{
				effect.GlobalPosition = actorPresentation.MapPoint(effect.GlobalPosition);
			}
		}
	}
}
[HarmonyPatch(typeof(VfxCmd), "PlayOnCreatureCenter")]
internal static class CreatureCenterEffectPatch
{
	[HarmonyPrefix]
	private static void Prefix(Creature target, out EffectPosition.Scope? __state)
	{
		__state = null;
		try
		{
			__state = EffectPosition.Enter(target);
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyPostfix]
	private static void Postfix(EffectPosition.Scope? __state)
	{
		EffectPosition.Exit(__state);
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, EffectPosition.Scope? __state)
	{
		EffectPosition.Exit(__state);
		return __exception;
	}
}
[HarmonyPatch(typeof(NHitSparkVfx), "_Ready")]
internal static class HitSparkPositionPatch
{
	private static readonly FieldInfo? Actor = AccessTools.Field(typeof(NHitSparkVfx), "_creatureNode");

	[HarmonyPostfix]
	private static void Postfix(NHitSparkVfx __instance)
	{
		try
		{
			object? obj = Actor?.GetValue(__instance);
			EffectPosition.Spark((Node2D)(object)__instance, (NCreature?)((obj is NCreature) ? obj : null));
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch(typeof(NBlockSparkVfx), "_Ready")]
internal static class BlockSparkPositionPatch
{
	private static readonly FieldInfo? Actor = AccessTools.Field(typeof(NBlockSparkVfx), "_creatureNode");

	[HarmonyPostfix]
	private static void Postfix(NBlockSparkVfx __instance)
	{
		try
		{
			object? obj = Actor?.GetValue(__instance);
			EffectPosition.Spark((Node2D)(object)__instance, (NCreature?)((obj is NCreature) ? obj : null));
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch(typeof(NDamageNumVfx), "Create", new Type[]
{
	typeof(Creature),
	typeof(int),
	typeof(bool)
})]
internal static class DamageNumberContextPatch
{
	[HarmonyPrefix]
	private static void Prefix(Creature target, out EffectPosition.Scope? __state)
	{
		__state = null;
		try
		{
			__state = EffectPosition.Enter(target);
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyPostfix]
	private static void Postfix(EffectPosition.Scope? __state)
	{
		EffectPosition.Exit(__state);
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, EffectPosition.Scope? __state)
	{
		EffectPosition.Exit(__state);
		return __exception;
	}
}
[HarmonyPatch(typeof(NDamageNumVfx), "Create", new Type[]
{
	typeof(Vector2),
	typeof(int)
})]
internal static class DamageNumberPositionPatch
{
	[HarmonyPrefix]
	private static void Prefix(ref Vector2 globalPosition)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			ActorPresentation actorPresentation = EffectPosition.Current?.Session;
			if (actorPresentation != null)
			{
				globalPosition = actorPresentation.MapPoint(globalPosition);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch(typeof(VfxCmd), "PlayVfx")]
internal static class NativeEffectPositionPatch
{
	[HarmonyPrefix]
	private static void Prefix(ref Vector2 position)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			ActorPresentation actorPresentation = EffectPosition.Current?.Session;
			if (actorPresentation != null)
			{
				position = actorPresentation.MapPoint(position);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
internal sealed class FinaleGhost : IDisposable
{
	private static readonly Shader FlowShader = LoadShader();

	private static readonly StringName Age = StringName.op_Implicit("age");

	private static readonly StringName Opacity = StringName.op_Implicit("opacity");

	private static readonly StringName Flow = StringName.op_Implicit("flow");

	private readonly NativeEchoPool.Item _item;

	private readonly string _clip;

	private readonly double _at;

	private readonly double _bodyStart;

	private bool _disposed;

	internal bool Active => !_disposed;

	private static void Prepare(NativeEchoPool.Item item)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		item.Material.Shader = (item.Pose.IsSkin ? SkinSmoke.Shader : FlowShader);
		item.Material.SetShaderParameter(StringName.op_Implicit("seed"), Variant.op_Implicit(6.3f));
		item.Pose.LoadFinale();
	}

	internal static async Task Warm(SceneTree tree, Node2D body, SubViewport viewport)
	{
		using NativeEchoPool.Item item = NativeEchoPool.Rent(body, "finale-ghost");
		Prepare(item);
		((Node)item.Root).Reparent((Node)(object)viewport, false);
		item.Root.Position = new Vector2(8f, 15f);
		item.Root.Scale = Vector2.One * 0.02f;
		item.Material.SetShaderParameter(StringName.op_Implicit("local_scale"), Variant.op_Implicit(new Vector2(0.29f, 0.29f)));
		item.Material.SetShaderParameter(Age, Variant.op_Implicit(0.04f));
		item.Material.SetShaderParameter(Opacity, Variant.op_Implicit(0.68f));
		item.Material.SetShaderParameter(Flow, Variant.op_Implicit(1f));
		item.Pose.SampleFinaleGhost("finale-fast", 1.5);
		await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
	}

	internal FinaleGhost(DisplayLease display, string mode, double at)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		_at = at;
		_clip = "finale-" + mode;
		_bodyStart = ((mode == "normal") ? 0.4416666666666667 : ((mode == "fast") ? 0.3666666666666667 : 0.0));
		_item = NativeEchoPool.Rent(display.Body, "finale-ghost");
		try
		{
			((Node)_item.Root).Name = StringName.op_Implicit("MoreSilentFinaleGhost");
			((GodotObject)_item.Root).SetMeta(StringName.op_Implicit("silent_effect"), Variant.op_Implicit("finale-ghost"));
			Transform2D transform = display.Facing(display.NativeFacing);
			transform.Origin = display.Home;
			_item.Root.Transform = transform;
			((Node)display.Body).GetParent().MoveChild((Node)(object)_item.Root, ((Node)display.Body).GetIndex(false));
			Prepare(_item);
			_item.Material.SetShaderParameter(StringName.op_Implicit("local_scale"), Variant.op_Implicit(new Vector2(display.ScaleX * (float)display.NativeFacing, 0f - display.Body.Scale.Y)));
			Update(at);
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	private static Shader LoadShader()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		using Stream stream = typeof(FinaleGhost).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets.finale-smoke.gdshader");
		using StreamReader streamReader = new StreamReader(stream);
		return new Shader
		{
			Code = streamReader.ReadToEnd()
		};
	}

	private static float Smooth(double value)
	{
		float num = (float)Math.Clamp(value, 0.0, 1.0);
		return num * num * (3f - 2f * num);
	}

	internal void Update(double now)
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		double num = now - _at;
		if (num >= 3.5 || !GodotObject.IsInstanceValid((GodotObject)(object)_item.Root) || !((Node)_item.Root).IsInsideTree())
		{
			Dispose();
			return;
		}
		float num2 = (float)Math.Clamp(num - 2.5, 0.0, 1.0);
		float num3 = 0.68f * Smooth((num - _bodyStart) / 0.1) * (1f - Smooth(num2));
		((CanvasItem)_item.Root).Visible = num3 > 0f;
		if (!(num3 <= 0f))
		{
			_item.Material.SetShaderParameter(Age, Variant.op_Implicit(0.04f + 0.96f * num2));
			_item.Material.SetShaderParameter(Opacity, Variant.op_Implicit(num3));
			_item.Material.SetShaderParameter(Flow, Variant.op_Implicit((float)Math.Max(0.0, num - _bodyStart)));
			_item.Pose.SampleFinaleGhost(_clip, num);
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_item.Dispose();
		}
	}
}
internal sealed class HiddenBodyMeshes : IDisposable
{
	private sealed class State(Node2D body)
	{
		internal readonly Node2D Body = body;

		internal int Users;

		private readonly List<CanvasItem> _changed = new List<CanvasItem>();

		private bool _suspended;

		internal void Suspend()
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			if (_suspended || !GodotObject.IsInstanceValid((GodotObject)(object)body) || ((CanvasItem)body).Modulate.A != 0f)
			{
				return;
			}
			_suspended = true;
			foreach (CanvasItem item in ((IEnumerable)((Node)body).GetChildren(false)).OfType<CanvasItem>())
			{
				if (((GodotObject)item).GetClass() == "SpineMesh2D" && !item.IsSetAsTopLevel() && ((Node)item).IsProcessingInternal())
				{
					_changed.Add(item);
					((Node)item).SetProcessInternal(false);
				}
			}
		}

		internal void Resume()
		{
			if (!_suspended)
			{
				return;
			}
			_suspended = false;
			foreach (CanvasItem item in _changed)
			{
				if (!GodotObject.IsInstanceValid((GodotObject)(object)item))
				{
					continue;
				}
				try
				{
					if (!((Node)item).IsProcessingInternal())
					{
						((Node)item).SetProcessInternal(true);
					}
					item.QueueRedraw();
				}
				catch (Exception error)
				{
					ModEntry.Log(error);
				}
			}
			_changed.Clear();
		}
	}

	private static readonly Dictionary<Node2D, State> Active = new Dictionary<Node2D, State>(ReferenceEqualityComparer.Instance);

	private State? _state;

	internal static int Count => Active.Count;

	private HiddenBodyMeshes(State state)
	{
		_state = state;
		state.Users++;
	}

	internal static HiddenBodyMeshes Acquire(Node2D body)
	{
		if (!Active.TryGetValue(body, out State value))
		{
			value = new State(body);
			Active.Add(body, value);
		}
		HiddenBodyMeshes hiddenBodyMeshes = new HiddenBodyMeshes(value);
		try
		{
			value.Suspend();
			return hiddenBodyMeshes;
		}
		catch
		{
			hiddenBodyMeshes.Dispose();
			throw;
		}
	}

	internal void CheckVisibility()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		State state = _state;
		if (state != null && (!GodotObject.IsInstanceValid((GodotObject)(object)state.body) || ((CanvasItem)state.body).Modulate.A != 0f))
		{
			state.Resume();
		}
	}

	public void Dispose()
	{
		State state = _state;
		if (state == null)
		{
			return;
		}
		_state = null;
		if (--state.Users != 0)
		{
			return;
		}
		try
		{
			state.Resume();
		}
		finally
		{
			Active.Remove(state.body);
		}
	}
}
internal static class KnifeTrapGeometry
{
	internal sealed record Pass(float Rotation, float ScaleX, float ScaleY, float Opacity, int Rank, float HeadOpacity, float TailOpacity);

	internal sealed class Volley
	{
		internal readonly int Index;

		internal readonly int Count;

		internal readonly double Direction;

		internal readonly int[][] Orders;

		internal readonly Pass[] Passes;

		internal readonly double Seed = Random.Shared.Next(1, 10000000);

		internal Volley(int index, int count)
		{
			Index = index;
			Count = count;
			Direction = ((index % 2 == 0) ? 0.48 : (-0.48)) + (Random.Shared.NextDouble() * 2.0 - 1.0) * Math.PI / 9.0;
			Orders = Enumerable.Range(0, 2).Select((Func<int, int[]>)delegate
			{
				int[] array = new int[5] { 0, 1, 2, 3, 4 };
				Random.Shared.Shuffle(array);
				if (array.SequenceEqual(new int[5] { 0, 1, 2, 3, 4 }) || array.SequenceEqual(new int[5] { 4, 3, 2, 1, 0 }))
				{
					ref int reference = ref array[1];
					ref int reference2 = ref array[3];
					int num = array[3];
					int num2 = array[1];
					reference = num;
					reference2 = num2;
				}
				return array;
			}).ToArray();
			Passes = Create(index, count, Direction, Orders);
		}
	}

	internal static Pass[] Create(int index, int count, double direction, int[][] orders)
	{
		double weight = ((count <= 2 || index <= 0 || index >= count - 1) ? 0.0 : (Math.Pow(Math.Sin(Math.PI * (double)index / (double)(count - 1)), 2.0) * Math.Min(1.0, (double)(count - 2) / 4.0)));
		return Enumerable.Range(0, 10).Select(delegate(int i)
		{
			int num = i / 2;
			int num2 = i % 2;
			int num3 = orders[num2][num];
			double num4 = (double)num3 / 4.0;
			return new Pass((float)(direction + ((double)num / 4.0 - 0.5) * 0.28 + (double)num2 * Math.PI), (float)(0.18 + num4 * 0.28), (float)(0.5 + num4 * 0.45), (float)(0.22 + 0.78 * Math.Pow(num4, 1.4)), num3, (float)((num3 == 4) ? 1.0 : (1.0 - 0.2 * weight)), (float)(1.0 - 0.22 * weight));
		}).ToArray();
	}

	internal static float RibbonOpacity(float v, float head, float tail)
	{
		float num = Math.Clamp((v - 0.1f) / 0.55f, 0f, 1f);
		num = num * num * (3f - 2f * num);
		return head + (tail - head) * num;
	}

	internal static Vector3 Point(Vector3 point, Vector2 contact, Pass pass)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = new Vector2((point.X - contact.X) * pass.ScaleX, (point.Y - contact.Y) * pass.ScaleY);
		Vector2 val2 = ((Vector2)(ref val)).Rotated(pass.Rotation);
		return new Vector3(val2.X, val2.Y, 0f);
	}
}
internal sealed class KnifeTrapVisual : IDisposable
{
	private static readonly Dictionary<NShivThrowVfx, KnifeTrapVisual> Active = new Dictionary<NShivThrowVfx, KnifeTrapVisual>(ReferenceEqualityComparer.Instance);

	private readonly NShivThrowVfx _native;

	private readonly SceneTree _tree;

	private readonly Node2D _root;

	private readonly ShivVisual.Dagger _dagger;

	private readonly MeshInstance2D[] _ribbons;

	private readonly Vector3[][] _points;

	private readonly Color[][] _colors;

	private readonly bool _originalVisible;

	private readonly GpuParticles2D _specks;

	private readonly double _speckSpeed;

	private readonly bool _speckVisible;

	private double _sourceClock;

	private bool _disposed;

	internal readonly KnifeTrapGeometry.Volley Volley;

	private const string DaggerPath = "throw_container/vfx_dagger_spray_dagger";

	internal static int Count => Active.Count;

	internal double Clock { get; private set; }

	internal int BladeCount => _ribbons.Length;

	internal static void Reset()
	{
		KnifeTrapVisual[] array = Active.Values.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			VisualCleanup.Safely(array[i].Dispose);
		}
	}

	internal static KnifeTrapVisual? For(NShivThrowVfx node)
	{
		return Active.GetValueOrDefault(node);
	}

	internal static double? FlightAge(double age)
	{
		if (!(age < 0.024999998999999995))
		{
			return (age < 0.15) ? Math.Max(0.0, (age - 0.024999999999999994) * 1.2) : age;
		}
		return null;
	}

	private static ShaderMaterial Gold(ShaderMaterial source, double depth)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		ShaderMaterial val = (ShaderMaterial)((Resource)source).Duplicate(false);
		Variant shaderParameter = source.GetShaderParameter(StringName.op_Implicit("lut"));
		try
		{
			GodotObject obj = ((Variant)(ref shaderParameter)).AsGodotObject();
			GradientTexture1D val2 = (GradientTexture1D)(object)((obj is GradientTexture1D) ? obj : null);
			if (val2 == null)
			{
				return val;
			}
			GradientTexture1D val3 = (GradientTexture1D)((Resource)val2).Duplicate(false);
			Gradient val5 = (val3.Gradient = (Gradient)((Resource)val2.Gradient).Duplicate(false));
			Color[] gold = (Color[])(object)new Color[5]
			{
				new Color(0.24f, 0.05f, 0.008f, 1f),
				new Color(0.68f, 0.16f, 0.018f, 1f),
				new Color(1f, 0.42f, 0.045f, 1f),
				new Color(1f, 0.77f, 0.34f, 1f),
				new Color(1.22f, 1.16f, 0.89f, 1f)
			};
			Color[] colors = val5.Colors;
			float shade = (float)(0.84 + 0.16 * depth);
			val5.Colors = ((IEnumerable<Color>)colors).Select((Func<Color, int, Color>)((Color c, int i) => new Color(gold[Math.Min(i, 4)].R * shade, gold[Math.Min(i, 4)].G * shade, gold[Math.Min(i, 4)].B * shade, c.A))).ToArray();
			val.SetShaderParameter(StringName.op_Implicit("lut"), Variant.op_Implicit((GodotObject)(object)val3));
			return val;
		}
		finally
		{
			((IDisposable)(Variant)(ref shaderParameter)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal static void StyleNative(NShivThrowVfx node)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Expected O, but got Unknown
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Expected O, but got Unknown
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Expected O, but got Unknown
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		string[] array = new string[2] { "throw_container", "impact_container" };
		foreach (string text in array)
		{
			foreach (GpuParticles2D item in ((IEnumerable)((Node)node).GetNode(NodePath.op_Implicit(text)).GetChildren(false)).OfType<GpuParticles2D>())
			{
				Material processMaterial = item.ProcessMaterial;
				ParticleProcessMaterial val = (ParticleProcessMaterial)(object)((processMaterial is ParticleProcessMaterial) ? processMaterial : null);
				if (val == null)
				{
					continue;
				}
				ParticleProcessMaterial val3 = (ParticleProcessMaterial)(object)(item.ProcessMaterial = (Material)(ParticleProcessMaterial)((Resource)val).Duplicate(false));
				((CanvasItem)item).SelfModulate = new Color(1f, 1f, 1f, ((CanvasItem)item).SelfModulate.A);
				float a = val3.Color.A;
				val3.Color = new Color(1f, 0.68f, 0.2f, a);
				if (item.TrailEnabled)
				{
					val3.Color = new Color(1f, 1f, 1f, a);
					Material material = ((CanvasItem)item).Material;
					ShaderMaterial val4 = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
					if (val4 != null)
					{
						((CanvasItem)item).Material = (Material)(object)Gold(val4, 1.0);
					}
					continue;
				}
				if (((Node)item).Name == StringName.op_Implicit("vfx_poof"))
				{
					val3.Color = new Color(0.68f, 0.4f, 0.19f, a);
					continue;
				}
				string text2 = ((object)((Node)item).Name).ToString();
				if ((!(text2 == "vfx_dagger_impact_core") && !(text2 == "vfx_common_hit_flare")) || 1 == 0)
				{
					continue;
				}
				val3.Color = new Color(1f, 1f, 1f, a);
				Material material2 = ((CanvasItem)item).Material;
				ShaderMaterial val5 = (ShaderMaterial)(object)((material2 is ShaderMaterial) ? material2 : null);
				if (val5 == null)
				{
					continue;
				}
				Variant shaderParameter = val5.GetShaderParameter(StringName.op_Implicit("lut"));
				try
				{
					GodotObject obj = ((Variant)(ref shaderParameter)).AsGodotObject();
					GradientTexture1D val6 = (GradientTexture1D)(object)((obj is GradientTexture1D) ? obj : null);
					if (val6 != null)
					{
						ShaderMaterial val7 = (ShaderMaterial)((Resource)val5).Duplicate(false);
						GradientTexture1D val8 = (GradientTexture1D)((Resource)val6).Duplicate(false);
						Gradient val10 = (val8.Gradient = (Gradient)((Resource)val6.Gradient).Duplicate(false));
						Color[] gold = (Color[])(object)((!(((Node)item).Name == StringName.op_Implicit("vfx_dagger_impact_core"))) ? new Color[2]
						{
							new Color(1f, 0.78f, 0.34f, 1f),
							new Color(1.32f, 1.22f, 0.94f, 1f)
						} : new Color[2]
						{
							new Color(1f, 0.48f, 0.06f, 1f),
							new Color(1.4f, 1.24f, 0.88f, 1f)
						});
						val10.Colors = val10.Colors.Select(delegate(Color c, int i)
						{
							//IL_000d: Unknown result type (might be due to invalid IL or missing references)
							//IL_0012: Unknown result type (might be due to invalid IL or missing references)
							//IL_0015: Unknown result type (might be due to invalid IL or missing references)
							//IL_0020: Unknown result type (might be due to invalid IL or missing references)
							Color result = gold[Math.Min(i, 1)];
							result.A = c.A;
							return result;
						}).ToArray();
						val7.SetShaderParameter(StringName.op_Implicit("lut"), Variant.op_Implicit((GodotObject)(object)val8));
						((CanvasItem)item).Material = (Material)(object)val7;
					}
				}
				finally
				{
					((IDisposable)(Variant)(ref shaderParameter)/*cast due to .constrained prefix*/).Dispose();
				}
			}
		}
	}

	internal KnifeTrapVisual(NShivThrowVfx native, double seed, KnifeTrapGeometry.Volley volley)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		_native = native;
		_tree = ((Node)native).GetTree();
		Volley = volley;
		_dagger = new ShivVisual.Dagger(native, seed);
		_root = new Node2D
		{
			Name = StringName.op_Implicit("MoreSilentKnifeTrap"),
			Rotation = 0f - ((Node2D)native).Rotation
		};
		((Node)native).AddChild((Node)(object)_root, false, (InternalMode)0);
		_ribbons = ((IEnumerable<KnifeTrapGeometry.Pass>)volley.Passes).Select((Func<KnifeTrapGeometry.Pass, int, MeshInstance2D>)((KnifeTrapGeometry.Pass p, int i) => new MeshInstance2D
		{
			Name = StringName.op_Implicit("Pincer" + i),
			Texture = _dagger.Texture,
			Material = (Material)(object)Gold(_dagger.Material, (double)p.Rank / 4.0),
			Mesh = (Mesh)new ArrayMesh()
		})).ToArray();
		_points = volley.Passes.Select((KnifeTrapGeometry.Pass _) => (Vector3[])(object)new Vector3[(_dagger.Sections + 1) * 2]).ToArray();
		_colors = volley.Passes.Select((KnifeTrapGeometry.Pass _) => (Color[])(object)new Color[(_dagger.Sections + 1) * 2]).ToArray();
		GpuParticles2D node = ((Node)native).GetNode<GpuParticles2D>(NodePath.op_Implicit("throw_container/vfx_dagger_spray_dagger"));
		_originalVisible = ((CanvasItem)node).Visible;
		_specks = ((Node)native).GetNode<GpuParticles2D>(NodePath.op_Implicit("throw_container/vfx_common_specks_spray"));
		_speckSpeed = _specks.SpeedScale;
		_speckVisible = ((CanvasItem)_specks).Visible;
		try
		{
			MeshInstance2D[] ribbons = _ribbons;
			foreach (MeshInstance2D val in ribbons)
			{
				((Node)_root).AddChild((Node)(object)val, false, (InternalMode)0);
			}
			((CanvasItem)node).Visible = false;
			_specks.SpeedScale = 0.0;
			((CanvasItem)_specks).Visible = false;
			((Node)native).TreeExiting += Dispose;
			_tree.ProcessFrame += Frame;
			Active.Add(native, this);
			Update();
		}
		catch
		{
			((CanvasItem)node).Visible = _originalVisible;
			Dispose();
			throw;
		}
	}

	private void Update()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Expected O, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		double? num = FlightAge(Clock);
		((CanvasItem)_root).Visible = num.HasValue;
		((CanvasItem)_specks).Visible = _speckVisible && num.HasValue;
		if (!num.HasValue)
		{
			return;
		}
		double valueOrDefault = num.GetValueOrDefault();
		(Vector3[], Vector2[], Color[], int[]) tuple = _dagger.Ribbon(valueOrDefault);
		Vector2 contact = _dagger.Head(0.15);
		for (int i = 0; i < _ribbons.Length; i++)
		{
			KnifeTrapGeometry.Pass pass = Volley.Passes[i];
			ArrayMesh val = (ArrayMesh)_ribbons[i].Mesh;
			val.ClearSurfaces();
			Vector3[] array = _points[i];
			Color[] array2 = _colors[i];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = KnifeTrapGeometry.Point(tuple.Item1[j], contact, pass);
				Color val2 = tuple.Item3[j];
				int num2 = j;
				Color val3 = val2;
				val3.A = val2.A * pass.Opacity * KnifeTrapGeometry.RibbonOpacity(tuple.Item2[j].Y, pass.HeadOpacity, pass.TailOpacity);
				array2[num2] = val3;
			}
			Array val4 = new Array();
			try
			{
				val4.Resize(13);
				val4[0] = Variant.op_Implicit(array);
				val4[4] = Variant.op_Implicit(tuple.Item2);
				val4[3] = Variant.op_Implicit(array2);
				val4[12] = Variant.op_Implicit(tuple.Item4);
				val.AddSurfaceFromArrays((PrimitiveType)3, val4, (Array<Array>)null, (Dictionary)null, (ArrayFormat)0);
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
		}
	}

	private void Frame()
	{
		if (_disposed)
		{
			return;
		}
		try
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)_native) || !((Node)_native).IsInsideTree() || ((GodotObject)_native).IsQueuedForDeletion())
			{
				Dispose();
			}
			else if (((Node)_native).CanProcess())
			{
				double processDeltaTime = ((Node)_native).GetProcessDeltaTime();
				Clock += processDeltaTime;
				double valueOrDefault = FlightAge(Clock).GetValueOrDefault();
				if (processDeltaTime > 0.0)
				{
					_specks.SpeedScale = _speckSpeed * Math.Max(0.0, valueOrDefault - _sourceClock) / processDeltaTime;
				}
				_sourceClock = valueOrDefault;
				if (Clock > _dagger.Life + _dagger.History)
				{
					Dispose();
				}
				else
				{
					Update();
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			VisualCleanup.Safely(Dispose);
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_tree))
		{
			_tree.ProcessFrame -= Frame;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_native))
		{
			((Node)_native).TreeExiting -= Dispose;
			if (GodotObject.IsInstanceValid((GodotObject)(object)_specks))
			{
				_specks.SpeedScale = _speckSpeed;
				((CanvasItem)_specks).Visible = _speckVisible;
			}
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			((CanvasItem)_root).Hide();
			((Node)_root).QueueFree();
		}
		Active.Remove(_native);
	}
}
internal sealed class MixedReturnPlan
{
	internal sealed record Step(bool Short, double Start, double Duration, double Distance, double Before, bool Last);

	internal const double MaxScale = 1.6;

	internal const double SingleLongReach = 2.0;

	internal const double ShortReach = 0.55;

	internal readonly Step[] Steps;

	internal readonly double Duration;

	internal readonly double Scale;

	internal readonly double Distance;

	internal MixedReturnPlan(double distance, double nominal)
	{
		MixedReturnPlan mixedReturnPlan = this;
		if (!double.IsFinite(distance) || distance < 0.0 || !double.IsFinite(nominal) || nominal <= 0.0)
		{
			throw new ArgumentOutOfRangeException("distance");
		}
		Distance = distance;
		if (distance <= 1E-08)
		{
			Steps = Array.Empty<Step>();
			Scale = 1.0;
			return;
		}
		double num = double.PositiveInfinity;
		double scale = 1.0;
		int num2 = int.MaxValue;
		bool[] chosen = Array.Empty<bool>();
		int num3 = ((distance <= nominal * 2.0 + 1E-08) ? 1 : Math.Max(1, (int)Math.Ceiling(distance / (nominal * 1.6) - 1E-08)));
		for (int i = 0; i <= num3; i++)
		{
			if (num3 == 1 && i == 0 != distance <= nominal * 0.55 + 1E-08)
			{
				continue;
			}
			int num4 = num3 - i;
			double num5 = distance / (nominal * ((double)i + (double)num4 * 0.5));
			if (!(num5 > ((num3 == 1 && i == 1) ? 2.0 : 1.6) + 1E-08))
			{
				bool[] array = Order(i, num4);
				int num6 = Math.Max(0, Math.Abs(i - num4) - 1);
				double num7 = Math.Abs(Math.Log(num5));
				if (num7 < num - 1E-10 || (Math.Abs(num7 - num) < 1E-10 && num6 < num2))
				{
					num = num7;
					num2 = num6;
					chosen = array;
					scale = num5;
				}
			}
		}
		if (chosen.Length == 0)
		{
			throw new InvalidOperationException("No feasible mixed return plan");
		}
		Scale = scale;
		double start = 0.0;
		double before = 0.0;
		Steps = chosen.Select(delegate(bool shortStep, int index)
		{
			double num8 = (shortStep ? 0.48 : (8.0 / 15.0));
			double num9 = nominal * (shortStep ? 0.5 : 1.0) * mixedReturnPlan.Scale;
			Step result = new Step(shortStep, start, num8, num9, before, index == chosen.Length - 1);
			start += num8;
			before += num9;
			return result;
		}).ToArray();
		Duration = start;
	}

	private static bool[] Order(int longs, int shorts)
	{
		List<bool> list = new List<bool>();
		bool? flag = false;
		while (longs + shorts > 0)
		{
			bool flag2 = (flag == false && shorts > 0) || ((flag != true || longs <= 0) && (longs < shorts || longs <= 0));
			list.Add(flag2);
			if (flag2)
			{
				shorts--;
			}
			else
			{
				longs--;
			}
			flag = flag2;
		}
		return list.ToArray();
	}

	internal int IndexAt(double time)
	{
		for (int num = Steps.Length - 1; num >= 0; num--)
		{
			if (time >= Steps[num].Start - 1E-09)
			{
				return num;
			}
		}
		if (Steps.Length == 0)
		{
			return -1;
		}
		return 0;
	}
}
internal sealed class MotionBank
{
	private static readonly Dictionary<string, MotionBank> Cache = new Dictionary<string, MotionBank>();

	internal readonly double HopDistance;

	internal readonly double AttackDuration;

	internal readonly double[] Progress;

	internal readonly double[] ShortProgress;

	internal readonly int[] LowerBones;

	private readonly Dictionary<string, double> _durations;

	internal readonly string SkeletonHash;

	private MotionBank(string version)
	{
		using Stream utf8Json = typeof(MotionBank).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets.motion-index.json");
		using JsonDocument jsonDocument = JsonDocument.Parse(utf8Json);
		JsonElement property = jsonDocument.RootElement.GetProperty("versions").GetProperty(version);
		SkeletonHash = property.GetProperty("skeletonHash").GetString();
		HopDistance = property.GetProperty("hopDistance").GetDouble();
		AttackDuration = property.GetProperty("clips").GetProperty("thrust").GetDouble();
		_durations = property.GetProperty("clips").EnumerateObject().ToDictionary((JsonProperty p) => p.Name, (JsonProperty p) => p.Value.GetDouble());
		Progress = (from x in property.GetProperty("returnProgress").EnumerateArray()
			select x.GetDouble()).ToArray();
		ShortProgress = (from x in property.GetProperty("shortReturnProgress").EnumerateArray()
			select x.GetDouble()).ToArray();
		LowerBones = (from x in property.GetProperty("lowerBones").EnumerateArray()
			select x.GetInt32()).ToArray();
	}

	internal static MotionBank Get(string version)
	{
		if (!Cache.TryGetValue(version, out MotionBank value))
		{
			return Cache[version] = new MotionBank(version);
		}
		return value;
	}

	internal double ClipDuration(string clip)
	{
		return _durations[clip];
	}

	internal static string? VersionFor(string hash)
	{
		return new string[2] { "stable", "beta" }.FirstOrDefault((string v) => Get(v).SkeletonHash == hash);
	}

	internal double At(double fraction, bool shortStep = false)
	{
		double[] array = (shortStep ? ShortProgress : Progress);
		double num = Math.Clamp(fraction, 0.0, 1.0) * (double)(array.Length - 1);
		int num2 = (int)num;
		int num3 = Math.Min(num2 + 1, array.Length - 1);
		return array[num2] + (array[num3] - array[num2]) * (num - (double)num2);
	}
}
internal sealed class NativeEchoPool : IDisposable
{
	internal sealed class Item : IDisposable
	{
		private readonly NativeEchoPool _owner;

		internal readonly string Key;

		internal readonly Node2D Root;

		internal readonly NativePose Pose;

		private readonly ShaderMaterial? _material;

		internal bool Leased;

		internal bool Prepared;

		internal Vector2 Dimensions;

		internal ShaderMaterial Material => _material ?? throw new InvalidOperationException("Body displays have no echo material");

		internal Item(NativeEchoPool owner, string key)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Expected O, but got Unknown
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			_owner = owner;
			Key = key;
			Root = new Node2D
			{
				Name = StringName.op_Implicit("MoreSilentPooledEcho"),
				Modulate = new Color(1f, 1f, 1f, 0f)
			};
			((Node)owner._body).GetParent().AddChild((Node)(object)Root, false, (InternalMode)0);
			NativePose nativePose = null;
			ShaderMaterial val = null;
			try
			{
				nativePose = new NativePose(owner._body, (Node)(object)Root);
				if (key != "body")
				{
					val = new ShaderMaterial
					{
						Shader = (nativePose.IsSkin ? SkinSmoke.Shader : AfterimageTrail.Shader)
					};
					if (!nativePose.IsSkin)
					{
						val.SetShaderParameter(StringName.op_Implicit("stable"), Variant.op_Implicit(nativePose.Version == "stable"));
					}
					nativePose.Material(val);
				}
				Pose = nativePose;
				_material = val;
				Created++;
			}
			catch
			{
				nativePose?.Dispose();
				if (val != null)
				{
					((GodotObject)val).Dispose();
				}
				((Node)Root).QueueFree();
				throw;
			}
		}

		public void Dispose()
		{
			_owner.Return(this);
		}

		internal void Destroy()
		{
			Leased = false;
			VisualCleanup.Safely(Pose.Dispose);
			VisualCleanup.Safely(delegate
			{
				ShaderMaterial? material = _material;
				if (material != null)
				{
					((GodotObject)material).Dispose();
				}
			});
			if (GodotObject.IsInstanceValid((GodotObject)(object)Root))
			{
				((CanvasItem)Root).Hide();
				((Node)Root).QueueFree();
			}
		}
	}

	private static readonly StringName AgeUniform = StringName.op_Implicit("age");

	private static readonly StringName OpacityUniform = StringName.op_Implicit("opacity");

	private static readonly Dictionary<Node2D, NativeEchoPool> Pools = new Dictionary<Node2D, NativeEchoPool>(ReferenceEqualityComparer.Instance);

	private readonly Node2D _body;

	private readonly Node2D _parking;

	private readonly List<Item> _items = new List<Item>();

	private bool _disposed;

	private static readonly string[] DepartureKinds = new string[4] { "Thrust", "DownStab", "SuckerPunch", "NativeAttack" };

	internal static int Created { get; private set; }

	internal static int Reused { get; private set; }

	internal static int Count => Pools.Count;

	internal static int LeasedCount => Pools.Values.Sum((NativeEchoPool p) => p._items.Count((Item i) => i.Leased && i.Key != "body"));

	private NativeEchoPool(Node2D body)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		_body = body;
		_parking = new Node2D
		{
			Name = StringName.op_Implicit("MoreSilentEchoPool"),
			Visible = false,
			ProcessMode = (ProcessModeEnum)4
		};
		((Node)body).GetParent().AddChild((Node)(object)_parking, false, (InternalMode)0);
		((Node)body).TreeExiting += BodyExiting;
	}

	private void BodyExiting()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From((Action)CheckLifetime);
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private void CheckLifetime()
	{
		if (!_disposed && (!GodotObject.IsInstanceValid((GodotObject)(object)_body) || !((Node)_body).IsInsideTree() || ((GodotObject)_body).IsQueuedForDeletion() || !GodotObject.IsInstanceValid((GodotObject)(object)_parking) || !((Node)_parking).IsInsideTree() || ((GodotObject)_parking).IsQueuedForDeletion()))
		{
			Dispose();
		}
	}

	private static NativeEchoPool For(Node2D body)
	{
		if (Pools.TryGetValue(body, out NativeEchoPool value))
		{
			if (!value._disposed && GodotObject.IsInstanceValid((GodotObject)(object)value._parking) && (value._items.Count == 0 || value._items[0].Pose.Current))
			{
				return value;
			}
			value.Dispose();
		}
		value = new NativeEchoPool(body);
		Pools.Add(body, value);
		return value;
	}

	private string Normalize(string key)
	{
		if (CharacterRig.For(_body) != null || !key.Contains(':'))
		{
			return key;
		}
		return "skin-departure:" + key.Split(':')[1];
	}

	internal static Item Rent(Node2D body, string key)
	{
		return For(body).Take(key);
	}

	internal static Item? TryRentTravel(Node2D body, string key)
	{
		NativeEchoPool nativeEchoPool = For(body);
		if (CharacterRig.For(body) == null && nativeEchoPool._items.Count(delegate(Item i)
		{
			bool flag = i.Leased;
			if (flag)
			{
				string key2 = i.Key;
				bool flag2 = ((key2 == "body" || key2 == "finale-ghost") ? true : false);
				flag = !flag2;
			}
			return flag;
		}) >= 2)
		{
			return null;
		}
		return nativeEchoPool.Take(key);
	}

	private Item Take(string key)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		key = Normalize(key);
		Item item = _items.Find((Item i) => !i.Leased && i.Key == key);
		if (item == null)
		{
			item = new Item(this, key);
			_items.Add(item);
		}
		else
		{
			Reused++;
		}
		item.Leased = true;
		if (((Node)item.Root).GetParent() != ((Node)_body).GetParent())
		{
			((Node)item.Root).Reparent(((Node)_body).GetParent(), false);
		}
		item.Root.Transform = Transform2D.Identity;
		((CanvasItem)item.Root).Modulate = Colors.White;
		((CanvasItem)item.Root).ZIndex = 0;
		((CanvasItem)item.Root).ZAsRelative = true;
		((CanvasItem)item.Root).Visible = true;
		if (((GodotObject)item.Root).HasMeta(StringName.op_Implicit("silent_return_marker")))
		{
			((GodotObject)item.Root).RemoveMeta(StringName.op_Implicit("silent_return_marker"));
		}
		try
		{
			if (key == "body")
			{
				item.Pose.Sample("idle_loop", 0.0);
			}
			else
			{
				item.Material.SetShaderParameter(OpacityUniform, Variant.op_Implicit(0f));
				item.Material.SetShaderParameter(AgeUniform, Variant.op_Implicit(0f));
			}
			return item;
		}
		catch
		{
			Return(item);
			throw;
		}
	}

	private void Return(Item item)
	{
		if (!_disposed && item.Leased)
		{
			item.Leased = false;
			((CanvasItem)item.Root).Hide();
			if (_items.Count((Item i) => !i.Leased && i.Key == item.Key) > ((!item.Pose.IsSkin) ? ((item.Key == "return") ? 8 : ((item.Key == "body") ? 1 : 2)) : ((!(item.Key == "return")) ? 1 : 2)))
			{
				_items.Remove(item);
				item.Destroy();
			}
			else if (((Node)item.Root).GetParent() != _parking)
			{
				((Node)item.Root).Reparent((Node)(object)_parking, false);
			}
		}
	}

	internal static void Warm(Node2D body)
	{
		for (int i = 0; i < 18; i++)
		{
			WarmStep(body, i);
		}
	}

	internal static void WarmStep(Node2D body, int index)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		NativeEchoPool nativeEchoPool = For(body);
		if (CharacterRig.For(body) == null)
		{
			if ((uint)index > 1u)
			{
				return;
			}
			using Item item = nativeEchoPool.Take("Thrust:" + ((index != 0) ? 2 : 0));
			if (!item.Prepared)
			{
				item.Dimensions = item.Pose.Departure("Thrust", (index != 0) ? 2 : 0);
				item.Prepared = true;
			}
			return;
		}
		if (index < 12)
		{
			string text = DepartureKinds[index / 3];
			int index2 = index % 3;
			using Item item2 = nativeEchoPool.Take(text + ":" + index2);
			if (!item2.Prepared)
			{
				item2.Dimensions = item2.Pose.Departure(text, index2);
				item2.Prepared = true;
			}
			if (!(text == "DownStab"))
			{
				return;
			}
			using Item item3 = nativeEchoPool.Take(text + ":" + index2);
			if (!item3.Prepared)
			{
				item3.Dimensions = item3.Pose.Departure(text, index2);
				item3.Prepared = true;
			}
			return;
		}
		if (nativeEchoPool._items.Count((Item i) => i.Key == "return") >= index - 11)
		{
			return;
		}
		Item item4 = new Item(nativeEchoPool, "return")
		{
			Leased = true
		};
		nativeEchoPool._items.Add(item4);
		try
		{
			item4.Pose.Shadow(0.2);
		}
		finally
		{
			item4.Dispose();
		}
	}

	internal static void Reset()
	{
		NativeEchoPool[] array = Pools.Values.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			VisualCleanup.Safely(array[i].Dispose);
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_body))
		{
			((Node)_body).TreeExiting -= BodyExiting;
		}
		foreach (Item item in _items)
		{
			VisualCleanup.Safely(item.Destroy);
		}
		_items.Clear();
		if (GodotObject.IsInstanceValid((GodotObject)(object)_parking))
		{
			((Node)_parking).QueueFree();
		}
		if (Pools.GetValueOrDefault(_body) == this)
		{
			Pools.Remove(_body);
		}
	}
}
internal sealed class NativeGeometry
{
	private readonly record struct Influence(int Bone, double X, double Y, double Weight, int Offset);

	private sealed record Mesh(string Slot, bool Weighted, int Length, Influence[][] Points);

	private readonly record struct Matrix(double A, double B, double C, double D, double X, double Y);

	private sealed record Boundary(int Hip, int Waist, int Thigh, string Leg, int[] Triangles);

	private readonly record struct Point(double X, double Y);

	private Boundary? _boundary;

	private readonly Point[] _edge = new Point[27];

	private double[]? _leg;

	private readonly Dictionary<string, Mesh> _meshes;

	private readonly Dictionary<string, Dictionary<string, double[][]>> _shadows;

	private readonly Dictionary<Mesh, double[]> _worldBuffers = new Dictionary<Mesh, double[]>();

	private readonly Dictionary<Mesh, float[]> _deformBuffers = new Dictionary<Mesh, float[]>();

	private readonly Dictionary<Mesh, double[]> _shadowBases = new Dictionary<Mesh, double[]>();

	private readonly Dictionary<Mesh, double[]> _shadowDesired = new Dictionary<Mesh, double[]>();

	private readonly Dictionary<Mesh, double[]> _upperDesired = new Dictionary<Mesh, double[]>();

	private readonly Dictionary<string, string> _upperKeys = new Dictionary<string, string>();

	private ReturnUpperCloth? _upperCloth;

	private readonly Matrix[] _basis = new Matrix[141];

	private static readonly Dictionary<string, NativeGeometry> Versions = new Dictionary<string, NativeGeometry>();

	private NativeGeometry(Dictionary<string, Mesh>? meshes = null, Dictionary<string, Dictionary<string, double[][]>>? shadows = null, Boundary? boundary = null)
	{
		_meshes = meshes ?? new Dictionary<string, Mesh>();
		_shadows = shadows ?? new Dictionary<string, Dictionary<string, double[][]>>();
		_boundary = boundary;
	}

	internal static NativeGeometry Get(string version, RefCounted reader)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (Versions.TryGetValue(version, out NativeGeometry value))
		{
			return new NativeGeometry(value._meshes, value._shadows, value._boundary);
		}
		Variant val = ((GodotObject)reader).Call(StringName.op_Implicit("geometry_kernel_data"), Array.Empty<Variant>());
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(((Variant)(ref val)).AsString());
			NativeGeometry nativeGeometry = new NativeGeometry();
			foreach (JsonProperty item in jsonDocument.RootElement.GetProperty("meshes").EnumerateObject())
			{
				foreach (JsonProperty item2 in item.Value.EnumerateObject())
				{
					JsonElement value2 = item2.Value;
					Influence[][] points = (from p in value2.GetProperty("points").EnumerateArray()
						select (from v in p.EnumerateArray()
							select new Influence((int)v[0].GetDouble(), v[1].GetDouble(), v[2].GetDouble(), v[3].GetDouble(), (int)v[4].GetDouble())).ToArray()).ToArray();
					nativeGeometry._meshes.Add(item.Name + ":" + item2.Name, new Mesh(item.Name, value2.GetProperty("weighted").GetBoolean(), (int)value2.GetProperty("deform_length").GetDouble(), points));
				}
			}
			foreach (JsonProperty item3 in jsonDocument.RootElement.GetProperty("shadows").EnumerateObject())
			{
				JsonElement[] array = item3.Value.EnumerateArray().ToArray();
				Dictionary<string, double[][]> dictionary = new Dictionary<string, double[][]>();
				foreach (JsonProperty slot in array[0].EnumerateObject())
				{
					dictionary.Add(slot.Name, array.Select((JsonElement f) => (from v in f.GetProperty(slot.Name).EnumerateArray()
						select v.GetDouble()).ToArray()).ToArray());
				}
				nativeGeometry._shadows.Add(item3.Name, dictionary);
			}
			JsonElement property = jsonDocument.RootElement.GetProperty("boundary");
			string @string = property.GetProperty("leg").GetString();
			nativeGeometry._boundary = new Boundary(property.GetProperty("hip").GetInt32(), property.GetProperty("waist").GetInt32(), property.GetProperty("thigh").GetInt32(), @string, ReadBoundary(version, @string));
			Versions.Add(version, nativeGeometry);
			return new NativeGeometry(nativeGeometry._meshes, nativeGeometry._shadows, nativeGeometry._boundary);
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private static int[] ReadBoundary(string version, string leg)
	{
		JsonElement jsonElement = GameContent.Identity(version);
		JsonElement property = jsonElement.GetProperty("upperClothBoundary");
		if (leg != $"{property.GetProperty("slot").GetInt32()}:{property.GetProperty("attachment").GetString()}")
		{
			throw new InvalidDataException("Original leg attachment changed");
		}
		string? @string = jsonElement.GetProperty("path").GetString();
		if (FileAccess.GetSha256(@string) != jsonElement.GetProperty("sha256").GetString())
		{
			throw new InvalidDataException("Original leg source changed");
		}
		byte[] fileAsBytes = FileAccess.GetFileAsBytes(@string);
		int @int = property.GetProperty("offset").GetInt32();
		int[] array = new int[property.GetProperty("count").GetInt32()];
		for (int i = 0; i < array.Length; i++)
		{
			int num = 0;
			int num2 = 0;
			byte b;
			do
			{
				b = fileAsBytes[@int++];
				num |= (b & 0x7F) << num2;
				num2 += 7;
				if (num2 > 35)
				{
					throw new InvalidDataException("Original mesh index overflow");
				}
			}
			while ((b & 0x80) != 0);
			array[i] = num;
		}
		return array;
	}

	internal void ReadBasis(double[] basis)
	{
		for (int i = 0; i < _basis.Length; i++)
		{
			int num = i * 6;
			_basis[i] = new Matrix(basis[num], basis[num + 1], basis[num + 2], basis[num + 3], basis[num + 4], basis[num + 5]);
		}
	}

	internal void LoadUpperCloth(string version)
	{
		_upperCloth = ReturnUpperCloth.Get(version);
	}

	internal void PrepareUpperCloth(float[] legDeform)
	{
		_leg = World(_meshes[_boundary.Leg], legDeform);
	}

	internal float[] UpperCloth(string key, string slot, double age, double sx, double sy, float[] deform)
	{
		if (!_upperKeys.TryGetValue(key, out string value))
		{
			Dictionary<string, string> upperKeys = _upperKeys;
			int num = key.IndexOf(':') + 1;
			upperKeys.Add(key, value = slot + ":" + key.Substring(num, key.Length - num));
		}
		Mesh mesh = _meshes[key];
		double[] array = World(mesh, deform);
		if (!_upperDesired.TryGetValue(mesh, out double[] value2))
		{
			_upperDesired.Add(mesh, value2 = new double[array.Length]);
		}
		Array.Copy(array, value2, array.Length);
		bool flag = _upperCloth.Apply(value, age, sx, sy, value2);
		if (slot == "cape_front_attack" && value2.Length == 84)
		{
			LimitCape(value2, sx, sy);
			flag = true;
		}
		if (!flag)
		{
			return deform;
		}
		return Apply(key, deform, value2);
	}

	private void LimitCape(double[] desired, double sx, double sy)
	{
		if (_leg == null || Math.Abs(sx) < 1E-08 || Math.Abs(sy) < 1E-08)
		{
			return;
		}
		Boundary boundary = _boundary;
		Matrix matrix = _basis[boundary.Hip];
		Matrix matrix2 = _basis[boundary.Waist];
		Matrix matrix3 = _basis[boundary.Thigh];
		_edge[0] = new Point((desired[48] - matrix.X) / sx, (desired[49] - matrix.Y) / sy);
		_edge[1] = new Point((matrix2.X - matrix.X) / sx - 85.0, (matrix2.Y - matrix.Y) / sy);
		double num = (matrix3.Y - matrix.Y) / sy - 12.0;
		double num2 = double.PositiveInfinity;
		for (int i = 1; i < _leg.Length; i += 2)
		{
			num2 = Math.Min(num2, (_leg[i] - matrix.Y) / sy);
		}
		for (int j = 0; j <= 24; j++)
		{
			double num3 = num + (num2 - num) * (double)j / 24.0;
			double num4 = double.PositiveInfinity;
			for (int k = 0; k < boundary.Triangles.Length; k += 3)
			{
				for (int l = 0; l < 3; l++)
				{
					int num5 = boundary.Triangles[k + l] * 2;
					int num6 = boundary.Triangles[k + (l + 1) % 3] * 2;
					double num7 = (_leg[num5] - matrix.X) / sx;
					double num8 = (_leg[num6] - matrix.X) / sx;
					double num9 = (_leg[num5 + 1] - matrix.Y) / sy;
					double num10 = (_leg[num6 + 1] - matrix.Y) / sy;
					if (!(num3 < Math.Min(num9, num10) - 1E-07) && !(num3 > Math.Max(num9, num10) + 1E-07))
					{
						num4 = Math.Min(num4, (Math.Abs(num10 - num9) < 1E-07) ? Math.Min(num7, num8) : (num7 + (num8 - num7) * (num3 - num9) / (num10 - num9)));
					}
				}
			}
			if (!double.IsFinite(num4))
			{
				return;
			}
			_edge[j + 2] = new Point(num4 - 6.0, num3);
		}
		for (int m = 15; m <= 23; m++)
		{
			desired[m * 2] = matrix.X + Math.Min((desired[m * 2] - matrix.X) / sx, Limit((desired[m * 2 + 1] - matrix.Y) / sy)) * sx;
		}
		for (int n = 15; n < 24; n++)
		{
			int num11 = n + 1;
			double num12 = ((num11 < 24) ? 1 : 0);
			double num13 = (desired[n * 2] - matrix.X) / sx;
			double num14 = (desired[n * 2 + 1] - matrix.Y) / sy;
			double num15 = (desired[num11 * 2] - matrix.X) / sx;
			double num16 = (desired[num11 * 2 + 1] - matrix.Y) / sy;
			if (Math.Abs(num16 - num14) < 1E-08)
			{
				continue;
			}
			double num17 = 0.0;
			Point[] edge = _edge;
			for (int num18 = 0; num18 < edge.Length; num18++)
			{
				Point point = edge[num18];
				double num19 = (point.Y - num14) / (num16 - num14);
				double num20 = 1.0 - num19 + num12 * num19;
				if (!(num19 < 0.0) && !(num19 > 1.0) && !(num20 < 1E-08))
				{
					num17 = Math.Max(num17, (num13 + (num15 - num13) * num19 - point.X) / num20);
				}
			}
			desired[n * 2] -= num17 * sx;
			desired[num11 * 2] -= num17 * num12 * sx;
		}
		double Limit(double y)
		{
			if (y >= _edge[0].Y)
			{
				return _edge[0].X;
			}
			for (int num21 = 1; num21 < _edge.Length; num21++)
			{
				if (y >= _edge[num21].Y)
				{
					double num22 = (_edge[num21 - 1].Y - y) / (_edge[num21 - 1].Y - _edge[num21].Y);
					return _edge[num21 - 1].X + (_edge[num21].X - _edge[num21 - 1].X) * num22;
				}
			}
			return _edge[^1].X;
		}
	}

	private double[] World(Mesh mesh, float[] deform)
	{
		if (!_worldBuffers.TryGetValue(mesh, out double[] value))
		{
			_worldBuffers.Add(mesh, value = new double[mesh.Points.Length * 2]);
		}
		int num = 0;
		Influence[][] points = mesh.Points;
		foreach (Influence[] obj in points)
		{
			double num2 = 0.0;
			double num3 = 0.0;
			Influence[] array = obj;
			for (int j = 0; j < array.Length; j++)
			{
				Influence influence = array[j];
				double num4 = influence.X;
				double num5 = influence.Y;
				if (deform.Length > influence.Offset + 1)
				{
					num4 = (mesh.Weighted ? (num4 + (double)deform[influence.Offset]) : ((double)deform[influence.Offset]));
					num5 = (mesh.Weighted ? (num5 + (double)deform[influence.Offset + 1]) : ((double)deform[influence.Offset + 1]));
				}
				Matrix matrix = _basis[influence.Bone];
				num2 += (matrix.A * num4 + matrix.B * num5 + matrix.X) * influence.Weight;
				num3 += (matrix.C * num4 + matrix.D * num5 + matrix.Y) * influence.Weight;
			}
			value[num++] = num2;
			value[num++] = num3;
		}
		return value;
	}

	internal double[] World(string key, float[] deform)
	{
		return World(_meshes[key], deform);
	}

	internal float[] Apply(string key, float[] deform, double[] desired)
	{
		Mesh mesh = _meshes[key];
		double[] array = World(mesh, deform);
		int num = deform.Length;
		int num2 = 0;
		if (!_deformBuffers.TryGetValue(mesh, out float[] value))
		{
			_deformBuffers.Add(mesh, value = new float[mesh.Length]);
		}
		if (num < value.Length)
		{
			Array.Clear(value);
		}
		Array.Copy(deform, value, Math.Min(num, value.Length));
		Influence[][] points = mesh.Points;
		foreach (Influence[] array2 in points)
		{
			double num3 = desired[num2] - array[num2];
			double num4 = desired[num2 + 1] - array[num2 + 1];
			double num5 = 0.0;
			Influence[] array3 = array2;
			foreach (Influence influence in array3)
			{
				num5 += influence.Weight;
			}
			array3 = array2;
			for (int j = 0; j < array3.Length; j++)
			{
				Influence influence2 = array3[j];
				Matrix matrix = _basis[influence2.Bone];
				double num6 = (matrix.A * matrix.D - matrix.B * matrix.C) * num5;
				int offset = influence2.Offset;
				if (offset >= num)
				{
					value[offset] = (mesh.Weighted ? 0f : ((float)influence2.X));
					value[offset + 1] = (mesh.Weighted ? 0f : ((float)influence2.Y));
				}
				value[offset] = (float)((double)value[offset] + (matrix.D * num3 - matrix.B * num4) / num6);
				value[offset + 1] = (float)((double)value[offset + 1] + (matrix.A * num4 - matrix.C * num3) / num6);
			}
			num2 += 2;
		}
		return value;
	}

	internal void SeedShadow(string key, float[] deform)
	{
		Mesh mesh = _meshes[key];
		_shadowBases[mesh] = World(mesh, deform).ToArray();
		_shadowDesired[mesh] = new double[mesh.Points.Length * 2];
	}

	internal float[] Shadow(string key, string variant, int first, int second, double weight, double sx, double sy, float[] deform)
	{
		Mesh mesh = _meshes[key];
		double[][] array = _shadows[variant][mesh.Slot];
		double[] array2 = array[first];
		double[] array3 = array[second];
		double[] array4 = _shadowBases[mesh];
		double[] array5 = _shadowDesired[mesh];
		for (int i = 0; i < array5.Length; i++)
		{
			array5[i] = array4[i] + (array2[i] + (array3[i] - array2[i]) * weight) * ((i % 2 == 0) ? sx : sy);
		}
		return Apply(key, deform, array5);
	}
}
internal sealed class NativePose : IDisposable
{
	private static readonly GDScript Script = Compile();

	private static readonly Lazy<GDScript> SkinScript = new Lazy<GDScript>((Func<GDScript>)(() => Compile("skin_pose")));

	private static readonly StringName BeginEntryCall = StringName.op_Implicit("begin_entry");

	private static readonly StringName BeginOriginalEntryCall = StringName.op_Implicit("begin_original_entry");

	private static readonly StringName BindCall = StringName.op_Implicit("bind");

	private static readonly StringName ClipOffsetCall = StringName.op_Implicit("clip_offset");

	private static readonly StringName CurrentCall = StringName.op_Implicit("current");

	private static readonly StringName CustomCall = StringName.op_Implicit("custom");

	private static readonly StringName CustomEntryCall = StringName.op_Implicit("custom_entry");

	private static readonly StringName DepartureCall = StringName.op_Implicit("departure");

	private static readonly StringName DisplayStateCall = StringName.op_Implicit("display_state");

	private static readonly StringName DurationCall = StringName.op_Implicit("duration");

	private static readonly StringName ClipDurationCall = StringName.op_Implicit("clip_duration");

	private static readonly StringName EntryMixCall = StringName.op_Implicit("entry_mix");

	private static readonly StringName GeometryStateCall = StringName.op_Implicit("geometry_state");

	private static readonly StringName GroundedCall = StringName.op_Implicit("grounded");

	private static readonly StringName FinaleGhostCall = StringName.op_Implicit("finale_ghost");

	private static readonly StringName HasClipCall = StringName.op_Implicit("has_clip");

	private static readonly StringName InspectCall = StringName.op_Implicit("inspect");

	private static readonly StringName InspectGeometryCall = StringName.op_Implicit("inspect_geometry");

	private static readonly StringName LoadBankCall = StringName.op_Implicit("load_bank");

	private static readonly StringName LoadCachedBankCall = StringName.op_Implicit("load_cached_bank");

	private static readonly StringName LoadClipsCall = StringName.op_Implicit("load_clips");

	private static readonly StringName LoadExtraCall = StringName.op_Implicit("load_extra");

	private static readonly StringName MaterialCall = StringName.op_Implicit("material");

	private static readonly StringName MatricesCall = StringName.op_Implicit("matrices");

	private static readonly StringName NumberCall = StringName.op_Implicit("number");

	private static readonly StringName OriginalAttackPendingCall = StringName.op_Implicit("original_attack_pending");

	private static readonly StringName PunchGhostCall = StringName.op_Implicit("punch_ghost");

	private static readonly StringName RefreshCall = StringName.op_Implicit("refresh");

	private static readonly StringName ReleaseCall = StringName.op_Implicit("release");

	private static readonly StringName ReturningCall = StringName.op_Implicit("returning");

	private static readonly StringName ReturningCustomCall = StringName.op_Implicit("returning_custom");

	private static readonly StringName SampleCall = StringName.op_Implicit("sample");

	private static readonly StringName SetGeometryKernelCall = StringName.op_Implicit("set_geometry_kernel");

	private static readonly StringName ShadowCall = StringName.op_Implicit("shadow");

	private static readonly StringName SkeletonHashCall = StringName.op_Implicit("skeleton_hash");

	private static readonly StringName SlotStateCall = StringName.op_Implicit("slot_state");

	private static readonly StringName StartupDiagnosticsCall = StringName.op_Implicit("startup_diagnostics");

	private static readonly StringName VisibleSlotsCall = StringName.op_Implicit("visible_slots");

	private static readonly StringName WarmReturnActionsCall = StringName.op_Implicit("warm_return_actions");

	private readonly RefCounted _reader;

	private readonly CharacterRig _rig;

	private bool _disposed;

	private static ImageTexture? _palm;

	internal Node2D Sprite { get; }

	internal string Version { get; }

	internal bool IsSkin => _rig.Version == null;

	internal bool OriginalAttackPending
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			Variant val = ((GodotObject)_reader).Call(OriginalAttackPendingCall, Array.Empty<Variant>());
			try
			{
				return ((Variant)(ref val)).AsBool();
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	internal bool Current
	{
		get
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (!_rig.Current)
			{
				return false;
			}
			Variant val = ((GodotObject)_reader).Call(CurrentCall, Array.Empty<Variant>());
			try
			{
				return ((Variant)(ref val)).AsBool();
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	private static GDScript Compile(string name = "native_pose")
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		using Stream stream = typeof(NativePose).Assembly.GetManifestResourceStream("MoreSilentAnimations.src.Core." + name + ".gd") ?? throw new InvalidDataException("Missing native pose bridge");
		using StreamReader streamReader = new StreamReader(stream);
		GDScript val = new GDScript
		{
			SourceCode = streamReader.ReadToEnd()
		};
		if ((int)((Script)val).Reload(false) != 0)
		{
			((GodotObject)val).Dispose();
			throw new InvalidDataException("Native pose bridge compilation failed");
		}
		return val;
	}

	internal NativePose(Node2D body, Node parent)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		_rig = CharacterRig.ForPresentation(body) ?? throw new InvalidDataException("Unsupported replacement skin keeps its native body animations");
		Variant val = ((GodotObject)(IsSkin ? SkinScript.Value : Script)).Call(StringName.op_Implicit("new"), Array.Empty<Variant>());
		try
		{
			_reader = (RefCounted)((Variant)(ref val)).AsGodotObject();
			try
			{
				Variant val2 = ((GodotObject)_reader).Call(BindCall, (Variant[])(object)new Variant[2]
				{
					Variant.op_Implicit((GodotObject)(object)body),
					Variant.op_Implicit((GodotObject)(object)parent)
				});
				try
				{
					ref Node2D reference = ref Sprite;
					GodotObject obj = ((Variant)(ref val2)).AsGodotObject();
					reference = (Node2D)(object)(((obj is Node2D) ? obj : null) ?? throw new InvalidDataException("Native pose bind failed"));
					Variant val3 = ((GodotObject)_reader).Call(SkeletonHashCall, Array.Empty<Variant>());
					try
					{
						Version = (IsSkin ? GameContent.Version : (MotionBank.VersionFor(((Variant)(ref val3)).AsString()) ?? throw new InvalidDataException("Unrecognized native Silent skeleton; keep original display: " + Inspect())));
						Variant val4 = ((GodotObject)_reader).Call(LoadCachedBankCall, (Variant[])(object)new Variant[1] { Variant.op_Implicit(Version) });
						try
						{
							if (!((Variant)(ref val4)).AsBool())
							{
								using Stream stream = typeof(NativePose).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets." + Version + ".motion.native.br") ?? throw new InvalidDataException("Missing authored motion differences");
								using BrotliStream brotliStream = new BrotliStream(stream, CompressionMode.Decompress);
								using MemoryStream memoryStream = new MemoryStream();
								brotliStream.CopyTo(memoryStream);
								Variant val5 = ((GodotObject)_reader).Call(LoadBankCall, (Variant[])(object)new Variant[2]
								{
									Variant.op_Implicit(memoryStream.ToArray()),
									Variant.op_Implicit(Version)
								});
								try
								{
									if (!((Variant)(ref val5)).AsBool())
									{
										throw new InvalidDataException("Native Silent layout does not match authored differences");
									}
								}
								finally
								{
									((IDisposable)(Variant)(ref val5)/*cast due to .constrained prefix*/).Dispose();
								}
							}
							if (IsSkin)
							{
								return;
							}
							NativeGeometry nativeGeometry = NativeGeometry.Get(Version, _reader);
							nativeGeometry.LoadUpperCloth(Version);
							Variant val6 = ((GodotObject)_reader).Call(SetGeometryKernelCall, (Variant[])(object)new Variant[7]
							{
								Variant.op_Implicit(Callable.From<double[]>((Action<double[]>)nativeGeometry.ReadBasis)),
								Variant.op_Implicit(Callable.From<string, float[], double[]>((Func<string, float[], double[]>)nativeGeometry.World)),
								Variant.op_Implicit(Callable.From<string, float[], double[], float[]>((Func<string, float[], double[], float[]>)nativeGeometry.Apply)),
								Variant.op_Implicit(Callable.From<string, float[]>((Action<string, float[]>)nativeGeometry.SeedShadow)),
								Variant.op_Implicit(Callable.From<string, string, int, int, double, double, double, float[], float[]>((Func<string, string, int, int, double, double, double, float[], float[]>)nativeGeometry.Shadow)),
								Variant.op_Implicit(Callable.From<string, string, double, double, double, float[], float[]>((Func<string, string, double, double, double, float[], float[]>)nativeGeometry.UpperCloth)),
								Variant.op_Implicit(Callable.From<float[]>((Action<float[]>)nativeGeometry.PrepareUpperCloth))
							});
							try
							{
								if (((Variant)(ref val6)).AsInt32() != 7)
								{
									throw new InvalidDataException("Return upper cloth must bind all seven original garment slots");
								}
							}
							finally
							{
								((IDisposable)(Variant)(ref val6)/*cast due to .constrained prefix*/).Dispose();
							}
						}
						finally
						{
							((IDisposable)(Variant)(ref val4)/*cast due to .constrained prefix*/).Dispose();
						}
					}
					finally
					{
						((IDisposable)(Variant)(ref val3)/*cast due to .constrained prefix*/).Dispose();
					}
				}
				finally
				{
					((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
				}
			}
			catch
			{
				Dispose();
				throw;
			}
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal string Inspect()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(InspectCall, Array.Empty<Variant>());
		try
		{
			return ((Variant)(ref val)).AsString();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal string InspectGeometry()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(InspectGeometryCall, Array.Empty<Variant>());
		try
		{
			return ((Variant)(ref val)).AsString();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal string DisplayState()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(DisplayStateCall, Array.Empty<Variant>());
		try
		{
			return ((Variant)(ref val)).AsString();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void LoadSteel()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(HasClipCall, (Variant[])(object)new Variant[1] { Variant.op_Implicit("steel") });
		try
		{
			if (((Variant)(ref val)).AsBool())
			{
				return;
			}
			using Stream stream = typeof(NativePose).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets." + Version + ".steel.native.br");
			using BrotliStream brotliStream = new BrotliStream(stream, CompressionMode.Decompress);
			using MemoryStream memoryStream = new MemoryStream();
			brotliStream.CopyTo(memoryStream);
			Variant val2 = ((GodotObject)_reader).Call(LoadClipsCall, (Variant[])(object)new Variant[1] { Variant.op_Implicit(memoryStream.ToArray()) });
			try
			{
				if (!((Variant)(ref val2)).AsBool())
				{
					throw new InvalidDataException("Steel authored motion version mismatch");
				}
			}
			finally
			{
				((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
			}
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void LoadFinale()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (IsSkin)
		{
			return;
		}
		if (_palm == null)
		{
			using Stream stream = typeof(NativePose).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets.finale-palm.png");
			using MemoryStream memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			Image val = new Image();
			try
			{
				if ((int)val.LoadPngFromBuffer(memoryStream.ToArray()) != 0)
				{
					throw new InvalidDataException("User palm unavailable");
				}
				_palm = ImageTexture.CreateFromImage(val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		Variant val2 = ((GodotObject)_reader).Call(HasClipCall, (Variant[])(object)new Variant[1] { Variant.op_Implicit("finale-fast") });
		try
		{
			byte[] array = Array.Empty<byte>();
			if (!((Variant)(ref val2)).AsBool())
			{
				using Stream stream2 = typeof(NativePose).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets." + Version + ".finale.native.br");
				using BrotliStream brotliStream = new BrotliStream(stream2, CompressionMode.Decompress);
				using MemoryStream memoryStream2 = new MemoryStream();
				brotliStream.CopyTo(memoryStream2);
				array = memoryStream2.ToArray();
			}
			Variant val3 = ((GodotObject)_reader).Call(LoadExtraCall, (Variant[])(object)new Variant[2]
			{
				Variant.op_Implicit(array),
				Variant.op_Implicit((GodotObject)(object)_palm)
			});
			try
			{
				if (!((Variant)(ref val3)).AsBool())
				{
					throw new InvalidDataException("Private Finale renderer layout unsupported");
				}
			}
			finally
			{
				((IDisposable)(Variant)(ref val3)/*cast due to .constrained prefix*/).Dispose();
			}
		}
		finally
		{
			((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void LoadFinaleSlash()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(HasClipCall, (Variant[])(object)new Variant[1] { Variant.op_Implicit("finale-slash") });
		try
		{
			if (((Variant)(ref val)).AsBool())
			{
				return;
			}
			using Stream stream = typeof(NativePose).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets." + Version + ".finale-slash.native.br");
			using BrotliStream brotliStream = new BrotliStream(stream, CompressionMode.Decompress);
			using MemoryStream memoryStream = new MemoryStream();
			brotliStream.CopyTo(memoryStream);
			Variant val2 = ((GodotObject)_reader).Call(LoadClipsCall, (Variant[])(object)new Variant[1] { Variant.op_Implicit(memoryStream.ToArray()) });
			try
			{
				if (!((Variant)(ref val2)).AsBool())
				{
					throw new InvalidDataException("Finale slash authored motion version mismatch");
				}
			}
			finally
			{
				((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
			}
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal string GeometryState()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(GeometryStateCall, Array.Empty<Variant>());
		try
		{
			return ((Variant)(ref val)).AsString();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void Shadow(double phase, string kind = "long", string next = "long", bool last = false)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(ShadowCall, (Variant[])(object)new Variant[4]
		{
			Variant.op_Implicit(phase),
			Variant.op_Implicit(kind),
			Variant.op_Implicit(next),
			Variant.op_Implicit(last)
		});
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void BeginEntry(bool upperOnly = true)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(BeginEntryCall, (Variant[])(object)new Variant[1] { Variant.op_Implicit(upperOnly) });
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void BeginOriginalEntry()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(BeginOriginalEntryCall, Array.Empty<Variant>());
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void CustomEntry(string name, double age)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(CustomEntryCall, (Variant[])(object)new Variant[2]
		{
			Variant.op_Implicit(name),
			Variant.op_Implicit(NativeTime(age))
		});
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal Vector2 ClipOffset(string name, double age, string bone)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(ClipOffsetCall, (Variant[])(object)new Variant[3]
		{
			Variant.op_Implicit(name),
			Variant.op_Implicit(age),
			Variant.op_Implicit(bone)
		});
		try
		{
			return ((Variant)(ref val)).AsVector2();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void Grounded(string name, double age)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(GroundedCall, (Variant[])(object)new Variant[2]
		{
			Variant.op_Implicit(name),
			Variant.op_Implicit(age)
		});
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void EntryMix(float weight)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(EntryMixCall, (Variant[])(object)new Variant[1] { Variant.op_Implicit(weight) });
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void Sample(string name, double time)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)time;
		if ((double)num > time)
		{
			num = MathF.BitDecrement(num);
		}
		Variant val = ((GodotObject)_reader).Call(SampleCall, (Variant[])(object)new Variant[2]
		{
			Variant.op_Implicit(name),
			Variant.op_Implicit(num)
		});
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private static float NativeTime(double time)
	{
		float num = (float)time;
		if (!((double)num > time))
		{
			return num;
		}
		return MathF.BitDecrement(num);
	}

	internal void Custom(string name, double time)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(CustomCall, (Variant[])(object)new Variant[2]
		{
			Variant.op_Implicit(name),
			Variant.op_Implicit((name == "finale-slash") ? time : ((double)NativeTime(time)))
		});
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void SampleFinaleGhost(string name, double time)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(FinaleGhostCall, (Variant[])(object)new Variant[2]
		{
			Variant.op_Implicit(name),
			Variant.op_Implicit(NativeTime(time))
		});
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void Returning(double phase, string upper, double age, float weight, string kind = "long", string next = "long", bool last = false)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(ReturningCall, (Variant[])(object)new Variant[7]
		{
			Variant.op_Implicit(NativeTime(phase)),
			Variant.op_Implicit(upper),
			Variant.op_Implicit(NativeTime(age)),
			Variant.op_Implicit(weight),
			Variant.op_Implicit(kind),
			Variant.op_Implicit(next),
			Variant.op_Implicit(last)
		});
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void ReturningCustom(double phase, string clip, double age, string kind, string next, bool last, float weight = 1f)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(ReturningCustomCall, (Variant[])(object)new Variant[7]
		{
			Variant.op_Implicit(phase),
			Variant.op_Implicit(clip),
			Variant.op_Implicit(age),
			Variant.op_Implicit(kind),
			Variant.op_Implicit(next),
			Variant.op_Implicit(last),
			Variant.op_Implicit(weight)
		});
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal float[] Matrices()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(MatricesCall, Array.Empty<Variant>());
		try
		{
			return ((Variant)(ref val)).AsFloat32Array();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal double Number(string key)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(NumberCall, (Variant[])(object)new Variant[1] { Variant.op_Implicit(key) });
		try
		{
			return ((Variant)(ref val)).AsDouble();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal double Duration(string name)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(DurationCall, (Variant[])(object)new Variant[1] { Variant.op_Implicit(name) });
		try
		{
			return ((Variant)(ref val)).AsDouble();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal double AuthoredDuration(string name)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(ClipDurationCall, (Variant[])(object)new Variant[1] { Variant.op_Implicit(name) });
		try
		{
			return ((Variant)(ref val)).AsDouble();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void Material(ShaderMaterial material)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(MaterialCall, (Variant[])(object)new Variant[1] { Variant.op_Implicit((GodotObject)(object)material) });
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void Refresh()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(RefreshCall, Array.Empty<Variant>());
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal Vector2 Departure(string kind, int index)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(DepartureCall, (Variant[])(object)new Variant[2]
		{
			Variant.op_Implicit(kind),
			Variant.op_Implicit(index)
		});
		try
		{
			return ((Variant)(ref val)).AsVector2();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal string StartupDiagnostics()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(StartupDiagnosticsCall, Array.Empty<Variant>());
		try
		{
			return ((Variant)(ref val)).AsString();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal void PunchGhost(double age, Vector2 center)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(PunchGhostCall, (Variant[])(object)new Variant[2]
		{
			Variant.op_Implicit((float)age),
			Variant.op_Implicit(center)
		});
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal string VisibleSlots()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(VisibleSlotsCall, Array.Empty<Variant>());
		try
		{
			return ((Variant)(ref val)).AsString();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal string SlotState()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(SlotStateCall, Array.Empty<Variant>());
		try
		{
			return ((Variant)(ref val)).AsString();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal static void Warm(Node2D body)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		using NativeEchoPool.Item item = NativeEchoPool.Rent(body, "body");
		((CanvasItem)item.Root).Modulate = new Color(1f, 1f, 1f, 0f);
		NativePose pose = item.Pose;
		if (pose.IsSkin)
		{
			pose.Sample("idle_loop", 0.0);
			return;
		}
		MotionBank.Get(pose.Version);
		pose.LoadFinale();
		pose.LoadFinaleSlash();
		pose.LoadSteel();
		pose.Custom("finale-slash", 0.075);
		pose.Custom("return", 0.0);
		(string, string, bool)[] array = new(string, string, bool)[3]
		{
			("long", "short", false),
			("short", "long", false),
			("long", "long", true)
		};
		for (int i = 0; i < array.Length; i++)
		{
			var (text, next, last) = array[i];
			pose.Returning((text == "short") ? 0.42 : 0.5, "", 0.0, 0f, text, next, last);
		}
		Variant val = ((GodotObject)pose._reader).Call(WarmReturnActionsCall, Array.Empty<Variant>());
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
		pose.Sample("idle_loop", 0.0);
	}

	public void Dispose()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		try
		{
			Variant val = ((GodotObject)_reader).Call(ReleaseCall, Array.Empty<Variant>());
			try
			{
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
		finally
		{
			((GodotObject)_reader).Dispose();
		}
	}
}
internal static class PresentationLayer
{
	internal static int Effective(CanvasItem node)
	{
		int num = node.ZIndex;
		if (!node.ZAsRelative || node.IsSetAsTopLevel())
		{
			return num;
		}
		Node parent = ((Node)node).GetParent();
		while (parent != null && ((!(parent is CanvasLayer) && !(parent is Viewport)) || 1 == 0))
		{
			CanvasItem val = (CanvasItem)(object)((parent is CanvasItem) ? parent : null);
			if (val != null)
			{
				num = Math.Clamp(num + val.ZIndex, -4096, 4096);
				if (!val.ZAsRelative || val.IsSetAsTopLevel())
				{
					break;
				}
			}
			parent = parent.GetParent();
		}
		return num;
	}

	internal static int Highest(Node node)
	{
		CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
		int num = ((val != null) ? Effective(val) : (-4096));
		foreach (Node child in node.GetChildren(false))
		{
			num = Math.Max(num, Highest(child));
		}
		return num;
	}

	internal static void Local(CanvasItem node)
	{
		node.ZAsRelative = true;
		node.ZIndex = 0;
	}
}
internal static class PresentationPreloader
{
	private static int _generation;

	private static bool _resourcesReady;

	private static Task? _resourceTask;

	internal static void Cancel()
	{
		_generation++;
	}

	internal static void Schedule(NCreature actor)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		int generation = _generation;
		Callable val = Callable.From((Action)delegate
		{
			Run(actor, generation);
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static bool Current(NCreature actor, int generation)
	{
		if (generation == _generation && AnimationSettings.Current.Enabled && GodotObject.IsInstanceValid((GodotObject)(object)actor) && ((Node)actor).IsInsideTree())
		{
			return !((GodotObject)actor).IsQueuedForDeletion();
		}
		return false;
	}

	private static async Task Run(NCreature actor, int generation)
	{
		_ = 2;
		try
		{
			if (!Current(actor, generation))
			{
				return;
			}
			SceneTree tree = ((Node)actor).GetTree();
			if (!_resourcesReady)
			{
				if (_resourceTask == null)
				{
					_resourceTask = Resources(tree);
				}
				await _resourceTask;
			}
			if (!Current(actor, generation) || CharacterRig.ForPresentation(actor.Body) == null)
			{
				return;
			}
			Node2D body = actor.Body;
			NativePose.Warm(body);
			for (int i = 0; i < ((CharacterRig.For(body) == null) ? 2 : 18); i++)
			{
				await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
				if (!Current(actor, generation) || actor.Body != body || CharacterRig.ForPresentation(body) == null)
				{
					return;
				}
				NativeEchoPool.WarmStep(body, i);
			}
			if (Current(actor, generation) && actor.Body == body)
			{
				await WarmEchoMesh(tree, body);
			}
		}
		catch (Exception error)
		{
			if (_resourceTask?.IsFaulted ?? false)
			{
				_resourceTask = null;
			}
			ModEntry.Log(error);
		}
	}

	private static async Task WarmEchoMesh(SceneTree tree, Node2D body)
	{
		SubViewport viewport = new SubViewport
		{
			Size = new Vector2I(16, 16),
			TransparentBg = true,
			Disable3D = true,
			RenderTargetUpdateMode = (UpdateMode)4
		};
		((Node)tree.Root).AddChild((Node)(object)viewport, false, (InternalMode)0);
		try
		{
			using NativeEchoPool.Item item = NativeEchoPool.Rent(body, "NativeAttack:0");
			if (!item.Prepared)
			{
				item.Dimensions = item.Pose.Departure("NativeAttack", 0);
				item.Prepared = true;
			}
			((Node)item.Root).Reparent((Node)(object)viewport, false);
			item.Root.Position = new Vector2(8f, 15f);
			item.Root.Scale = Vector2.One * 0.02f;
			item.Material.SetShaderParameter(StringName.op_Implicit("local_scale"), Variant.op_Implicit(new Vector2(0.29f, 0.29f)));
			item.Material.SetShaderParameter(StringName.op_Implicit("opacity"), Variant.op_Implicit(0.6f));
			await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
			if (GodotObject.IsInstanceValid((GodotObject)(object)body) && ((Node)body).IsInsideTree() && !((GodotObject)body).IsQueuedForDeletion())
			{
				await FinaleGhost.Warm(tree, body, viewport);
			}
		}
		finally
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)viewport))
			{
				((Node)viewport).QueueFree();
			}
		}
	}

	private static async Task Resources(SceneTree tree)
	{
		SubViewport viewport = null;
		try
		{
			string version = GameContent.Version;
			List<Shader> shaders = new List<Shader>
			{
				AfterimageTrail.Shader,
				ReturnSmoke.Shader
			};
			BottleRecipe.Get();
			DownStrikeRecipe.Get();
			await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
			ReturnKnifeVisual.Warm();
			SurgeRecipe surgeRecipe = SurgeRecipe.Get(version);
			foreach (SurgeRecipe.Asset value in surgeRecipe.Assets.Values)
			{
				ResourceLoader.Load<Texture2D>(value.Path, (string)null, (CacheMode)1);
			}
			ResourceLoader.Load<ShaderMaterial>(surgeRecipe.RibbonMaterial, (string)null, (CacheMode)1);
			shaders.AddRange(ShivVisual.Warm());
			await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
			shaders.AddRange(TrackingVisual.Warm());
			shaders.Add(SteelWind.Warm(version));
			SnakeVisual.Warm(version);
			viewport = new SubViewport
			{
				Name = StringName.op_Implicit("MoreSilentShaderWarmup"),
				Size = new Vector2I(16, 16),
				TransparentBg = true,
				Disable3D = true,
				RenderTargetUpdateMode = (UpdateMode)4
			};
			((Node)tree.Root).AddChild((Node)(object)viewport, false, (InternalMode)0);
			foreach (Shader item in shaders)
			{
				((Node)viewport).AddChild((Node)new ColorRect
				{
					Size = new Vector2(16f, 16f),
					MouseFilter = (MouseFilterEnum)2,
					Material = (Material)new ShaderMaterial
					{
						Shader = item
					}
				}, false, (InternalMode)0);
			}
			if (RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
			{
				ComboPlanningUi.WarmEffects(viewport);
			}
			await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
			_resourcesReady = true;
		}
		catch
		{
			_resourceTask = null;
			throw;
		}
		finally
		{
			if (viewport != null && GodotObject.IsInstanceValid((GodotObject)(object)viewport))
			{
				((Node)viewport).QueueFree();
			}
		}
	}
}
internal sealed class PunchGhost : IDisposable
{
	private static readonly ShaderMaterial Tint = new ShaderMaterial
	{
		Shader = new Shader
		{
			Code = "shader_type canvas_item;\nrender_mode blend_mix;\nvarying vec4 tint;\nvoid vertex(){tint=COLOR;}\nvoid fragment(){vec4 source=texture(TEXTURE,UV);COLOR=vec4(source.rgb*vec3(0.66,0.97,1.0)+(vec3(1.0)-source.rgb)*vec3(0.06,0.38,0.48),source.a*tint.a);}"
		}
	};

	private readonly Node2D _root;

	private readonly NativePose _pose;

	private readonly SurgeRecipe _recipe;

	private readonly double _start;

	private readonly Color _tint;

	private bool _disposed;

	internal bool Active => !_disposed;

	internal PunchGhost(DisplayLease display, double now)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		_start = now;
		_recipe = SurgeRecipe.Get(display.Pose.Version);
		_root = new Node2D
		{
			Name = StringName.op_Implicit("MoreSilentPunchGhost"),
			Transform = display.Root.Transform,
			Modulate = ((CanvasItem)display.Root).Modulate
		};
		_tint = ((CanvasItem)_root).Modulate;
		((GodotObject)_root).SetMeta(StringName.op_Implicit("silent_effect"), Variant.op_Implicit("punch-ghost"));
		display.AttachAttack(_root, 10);
		try
		{
			_pose = new NativePose(display.Body, (Node)(object)_root);
			_pose.Material(Tint);
			Update(now);
		}
		catch
		{
			_pose?.Dispose();
			((Node)_root).QueueFree();
			throw;
		}
	}

	internal void Update(double now)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (!_disposed)
		{
			double num = Math.Max(0.0, now - _start);
			if (num >= 0.4 || !GodotObject.IsInstanceValid((GodotObject)(object)_root) || !((Node)_root).IsInsideTree())
			{
				Dispose();
				return;
			}
			_pose.PunchGhost(num, _recipe.PunchCenter(num));
			Node2D root = _root;
			Color tint = _tint;
			tint.A = _tint.A * 0.6f * (float)(1.0 - Math.Clamp((num - 0.12) / 0.28, 0.0, 1.0));
			((CanvasItem)root).Modulate = tint;
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_pose.Dispose();
			if (GodotObject.IsInstanceValid((GodotObject)(object)_root))
			{
				((CanvasItem)_root).Hide();
				((Node)_root).QueueFree();
			}
		}
	}
}
internal static class ReturnEchoPolicy
{
	internal static (double A, double B) Window(bool shortStep)
	{
		if (!shortStep)
		{
			return (A: 0.17500000000000002, B: 0.7875);
		}
		return (A: 0.125, B: 0.75);
	}

	private static double Smooth(double u)
	{
		u = Math.Clamp(u, 0.0, 1.0);
		return u * u * (3.0 - 2.0 * u);
	}

	internal static double Marker(bool shortStep, double phase, bool last)
	{
		var (num, num2) = Window(shortStep);
		if (phase < num)
		{
			return 0.25 + 0.75 * Smooth(phase / num);
		}
		if (phase <= num2)
		{
			return 1.0;
		}
		return 1.0 - (last ? 1.0 : 0.75) * Smooth((phase - num2) / (1.0 - num2));
	}

	internal static (double At, int Index)[] Births(ReturnJourney journey)
	{
		List<(double, int)> list = new List<(double, int)>();
		double num = 0.0;
		for (int i = 0; i < journey.Steps; i++)
		{
			MixedReturnPlan.Step step = journey.Plan.Steps[i];
			(double A, double B) tuple = Window(step.Short);
			double item = tuple.A;
			double item2 = tuple.B;
			List<(double, int, double)> list2 = new List<(double, int, double)>();
			double num2 = Math.Max(step.Distance * 0.25, journey.NominalStepDistance * 0.12);
			double num3 = Math.Max(num2, step.Distance * (2.0 / 15.0) / step.Duration);
			for (int j = 0; (double)j * num3 < step.Distance + num3; j++)
			{
				int item3 = i * 100 + j;
				double num4 = num3 * ((double)j + 0.15 * Math.Sin((double)i * 2.3999632297));
				if (!(num4 < 0.0) && !(num4 >= step.Distance))
				{
					double num5 = journey.TimeAtDistance(step.Before + num4);
					double num6 = (num5 - journey.Start - step.Start) / step.Duration;
					if (num6 >= item && num6 <= item2)
					{
						list2.Add((num5, item3, num6));
					}
				}
			}
			double num7 = 1E-06 * Math.Max(1.0, journey.NominalStepDistance);
			foreach (var item4 in list2.OrderBy<(double, int, double), double>(((double At, int Index, double Phase) e) => e.At))
			{
				double num8 = journey.Progress(i, item4.Item3) * step.Distance;
				double num9 = step.Before + num8;
				if (!(num8 < num2 - num7) && !(num9 - num < num2 - num7))
				{
					list.Add((item4.Item1, item4.Item2));
					num = num9;
				}
			}
		}
		return list.OrderBy<(double, int), double>(((double At, int Index) e) => e.At).ToArray();
	}
}
internal sealed class ReturnJourney
{
	internal const double HopDuration = 8.0 / 15.0;

	internal readonly double From;

	internal readonly double Home;

	internal readonly double Start;

	internal readonly double Distance;

	internal readonly double StepDistance;

	internal readonly double NominalStepDistance;

	internal readonly double End;

	internal readonly int Steps;

	internal readonly MixedReturnPlan Plan;

	internal readonly (double At, int Index)[] Echoes;

	private readonly MotionBank _motion;

	internal ReturnJourney(double from, double home, double start, double stepDistance, MotionBank motion)
	{
		if (!double.IsFinite(from + home + start + stepDistance) || stepDistance <= 0.0)
		{
			throw new ArgumentOutOfRangeException("stepDistance");
		}
		From = from;
		Home = home;
		Start = start;
		NominalStepDistance = stepDistance;
		_motion = motion;
		Distance = Math.Abs(from - home);
		Plan = new MixedReturnPlan(Distance, stepDistance);
		Steps = Plan.Steps.Length;
		StepDistance = ((Steps > 0) ? (Distance / (double)Steps) : 0.0);
		End = Start + Plan.Duration;
		Echoes = ReturnEchoPolicy.Births(this);
	}

	internal (MixedReturnPlan.Step Step, int Index, double Age, string Kind, string Next) At(double now)
	{
		int num = Plan.IndexAt(Math.Max(0.0, now - Start));
		MixedReturnPlan.Step step = Plan.Steps[num];
		return (Step: step, Index: num, Age: Math.Clamp(now - Start - step.Start, 0.0, step.Duration), Kind: step.Short ? "short" : "long", Next: Plan.Steps[Math.Min(num + 1, Steps - 1)].Short ? "short" : "long");
	}

	internal double Position(double now)
	{
		if (now >= End || Steps == 0)
		{
			return Home;
		}
		(MixedReturnPlan.Step, int, double, string, string) tuple = At(now);
		MixedReturnPlan.Step item = tuple.Item1;
		double num = item.Before + item.Distance * Progress(tuple.Item2, tuple.Item3 / item.Duration);
		return From + (double)Math.Sign(Home - From) * num;
	}

	internal double Phase(double now)
	{
		if (Steps <= 0)
		{
			return 0.0;
		}
		return At(now).Age;
	}

	internal double TimeAtDistance(double distance)
	{
		distance = Math.Clamp(distance, 0.0, Distance);
		if (distance >= Distance - 1E-08)
		{
			return End;
		}
		MixedReturnPlan.Step step = Plan.Steps.Last((MixedReturnPlan.Step s) => distance >= s.Before);
		double num = (distance - step.Before) / step.Distance;
		double num2 = 0.0;
		double num3 = 1.0;
		int index = Array.IndexOf(Plan.Steps, step);
		for (int i = 0; i < 26; i++)
		{
			double num4 = (num2 + num3) / 2.0;
			if (Progress(index, num4) < num)
			{
				num2 = num4;
			}
			else
			{
				num3 = num4;
			}
		}
		return Start + step.Start + (num2 + num3) / 2.0 * step.Duration;
	}

	internal double Progress(int index, double u)
	{
		MixedReturnPlan.Step step = Plan.Steps[index];
		u = Math.Clamp(u, 0.0, 1.0);
		if (index > 0 && Plan.Steps[index - 1].Short != step.Short && u < 0.1)
		{
			return Hermite(0.0, Value(0.1), Math.Min(Speed(Plan.Steps[index - 1], end: true), Speed(step, end: false)) * step.Duration / step.Distance, Slope(0.1), u / 0.1);
		}
		if (index + 1 < Steps && Plan.Steps[index + 1].Short != step.Short && u > 0.9)
		{
			return Hermite(Value(0.9), 1.0, Slope(0.9), Math.Min(Speed(step, end: true), Speed(Plan.Steps[index + 1], end: false)) * step.Duration / step.Distance, (u - 1.0 + 0.1) / 0.1);
		}
		return Value(u);
		static double Hermite(double a, double b, double va, double vb, double t)
		{
			return (2.0 * t * t * t - 3.0 * t * t + 1.0) * a + (t * t * t - 2.0 * t * t + t) * 0.1 * va + (-2.0 * t * t * t + 3.0 * t * t) * b + (t * t * t - t * t) * 0.1 * vb;
		}
		double Slope(double t)
		{
			return (Value(t + 1.0 / 512.0) - Value(t - 1.0 / 512.0)) / (1.0 / 256.0);
		}
		double Speed(MixedReturnPlan.Step s, bool end)
		{
			return s.Distance / s.Duration * (_motion.At(end ? 1.0 : (1.0 / 512.0), s.Short) - _motion.At(end ? (511.0 / 512.0) : 0.0, s.Short)) / (1.0 / 512.0);
		}
		double Value(double t)
		{
			return _motion.At(t, step.Short);
		}
	}
}
internal sealed class ReturnKnifeVisual : IDisposable
{
	private static Node2D? _template;

	private static readonly HashSet<ReturnKnifeVisual> Active = new HashSet<ReturnKnifeVisual>();

	private readonly Node2D _root;

	private readonly GpuParticles2D[] _throw;

	private readonly GpuParticles2D[] _impact;

	private SceneTree? _tree;

	private double _clock;

	private readonly double _end;

	private bool _impacted;

	private bool _disposed;

	internal static int Count => Active.Count;

	internal static void Reset()
	{
		ReturnKnifeVisual[] array = Active.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			VisualCleanup.Safely(array[i].Dispose);
		}
	}

	internal static void Warm()
	{
		Template();
	}

	private static Color Gray(Color c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		float num = Math.Max(c.R, Math.Max(c.G, c.B));
		return new Color(num, num, num, c.A);
	}

	private static GradientTexture1D GrayRamp(GradientTexture1D original, bool softerGray = false)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		GradientTexture1D val = (GradientTexture1D)((Resource)original).Duplicate(false);
		Gradient val2 = (Gradient)((Resource)original.Gradient).Duplicate(false);
		val2.Colors = ((IEnumerable<Color>)val2.Colors).Select((Func<Color, Color>)delegate(Color c)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			Color val3 = Gray(c);
			float num = (softerGray ? (0.78f * val3.R / (val3.R + 0.4f)) : val3.R);
			return new Color(num, num, num, c.A);
		}).ToArray();
		val.Gradient = val2;
		return val;
	}

	private static Node2D Template()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Expected O, but got Unknown
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		if (_template != null)
		{
			return _template;
		}
		Node2D val = new Node2D
		{
			Name = StringName.op_Implicit("MoreSilentReturnKnife"),
			ShowBehindParent = true
		};
		Node2D val2 = PreloadManager.Cache.GetScene(NShivThrowVfx.scenePath).Instantiate<Node2D>((GenEditState)0);
		try
		{
			string[] array = new string[2] { "throw_container", "impact_container" };
			foreach (string text in array)
			{
				Node2D node = ((Node)val2).GetNode<Node2D>(NodePath.op_Implicit(text));
				((Node)val2).RemoveChild((Node)(object)node);
				((Node)val).AddChild((Node)(object)node, false, (InternalMode)0);
				foreach (GpuParticles2D item in ((IEnumerable)((Node)node).GetChildren(false)).OfType<GpuParticles2D>())
				{
					item.Emitting = false;
					((CanvasItem)item).Modulate = Gray(((CanvasItem)item).Modulate);
					((CanvasItem)item).SelfModulate = Gray(((CanvasItem)item).SelfModulate);
					ParticleProcessMaterial val3 = (ParticleProcessMaterial)((Resource)item.ProcessMaterial).Duplicate(false);
					val3.Color = Gray(val3.Color);
					Texture2D colorRamp = val3.ColorRamp;
					GradientTexture1D val4 = (GradientTexture1D)(object)((colorRamp is GradientTexture1D) ? colorRamp : null);
					if (val4 != null)
					{
						val3.ColorRamp = (Texture2D)(object)GrayRamp(val4);
					}
					Texture2D colorInitialRamp = val3.ColorInitialRamp;
					GradientTexture1D val5 = (GradientTexture1D)(object)((colorInitialRamp is GradientTexture1D) ? colorInitialRamp : null);
					if (val5 != null)
					{
						val3.ColorInitialRamp = (Texture2D)(object)GrayRamp(val5);
					}
					item.ProcessMaterial = (Material)(object)val3;
					Material material = ((CanvasItem)item).Material;
					ShaderMaterial val6 = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
					if (val6 == null)
					{
						continue;
					}
					ShaderMaterial val7 = (ShaderMaterial)((Resource)val6).Duplicate(false);
					Variant shaderParameter = val6.GetShaderParameter(StringName.op_Implicit("lut"));
					try
					{
						GodotObject obj = ((Variant)(ref shaderParameter)).AsGodotObject();
						GradientTexture1D val8 = (GradientTexture1D)(object)((obj is GradientTexture1D) ? obj : null);
						if (val8 != null)
						{
							val7.SetShaderParameter(StringName.op_Implicit("lut"), Variant.op_Implicit((GodotObject)(object)GrayRamp(val8, softerGray: true)));
						}
						((CanvasItem)item).Material = (Material)(object)val7;
					}
					finally
					{
						((IDisposable)(Variant)(ref shaderParameter)/*cast due to .constrained prefix*/).Dispose();
					}
				}
			}
			return _template = val;
		}
		catch
		{
			((GodotObject)val).Free();
			throw;
		}
		finally
		{
			((GodotObject)val2).Free();
		}
	}

	internal static Node2D Create(Vector2 origin, Vector2 target)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Node2D val = (Node2D)((Node)Template()).Duplicate(0);
		val.Position = target;
		Vector2 val2 = target - origin;
		val.Rotation = ((Vector2)(ref val2)).Angle();
		try
		{
			new ReturnKnifeVisual(val);
			return val;
		}
		catch
		{
			((GodotObject)val).Free();
			throw;
		}
	}

	private ReturnKnifeVisual(Node2D root)
	{
		_root = root;
		_throw = ((IEnumerable)((Node)root).GetNode(NodePath.op_Implicit("throw_container")).GetChildren(false)).OfType<GpuParticles2D>().ToArray();
		_impact = ((IEnumerable)((Node)root).GetNode(NodePath.op_Implicit("impact_container")).GetChildren(false)).OfType<GpuParticles2D>().ToArray();
		_end = Math.Max(_throw.Max((GpuParticles2D p) => p.Lifetime + p.TrailLifetime), 0.15 + _impact.Max((GpuParticles2D p) => p.Lifetime + p.TrailLifetime)) + 0.1;
		((Node)root).Ready += Ready;
		((Node)root).TreeExiting += Dispose;
		Active.Add(this);
	}

	private void Ready()
	{
		_tree = ((Node)_root).GetTree();
		_tree.ProcessFrame += Frame;
		GpuParticles2D[] @throw = _throw;
		for (int i = 0; i < @throw.Length; i++)
		{
			@throw[i].Restart();
		}
	}

	private void Frame()
	{
		if (_disposed)
		{
			return;
		}
		try
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)_root) || !((Node)_root).IsInsideTree() || ((GodotObject)_root).IsQueuedForDeletion())
			{
				Dispose();
			}
			else
			{
				if (!((Node)_root).CanProcess())
				{
					return;
				}
				_clock += ((Node)_root).GetProcessDeltaTime();
				if (!_impacted && _clock >= 0.15)
				{
					_impacted = true;
					GpuParticles2D[] impact = _impact;
					for (int i = 0; i < impact.Length; i++)
					{
						impact[i].Restart();
					}
				}
				if (_clock >= _end)
				{
					Dispose();
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			VisualCleanup.Safely(Dispose);
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			Active.Remove(this);
			if (_tree != null && GodotObject.IsInstanceValid((GodotObject)(object)_tree))
			{
				_tree.ProcessFrame -= Frame;
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)_root))
			{
				((Node)_root).Ready -= Ready;
				((Node)_root).TreeExiting -= Dispose;
				((CanvasItem)_root).Hide();
				((Node)_root).QueueFree();
			}
		}
	}
}
internal sealed class ReturnSmoke : IDisposable
{
	private static readonly StringName AgeUniform = StringName.op_Implicit("age");

	private static readonly StringName AgeEndUniform = StringName.op_Implicit("age_end");

	private static readonly StringName BasisXUniform = StringName.op_Implicit("basis_x");

	private static readonly StringName BasisYUniform = StringName.op_Implicit("basis_y");

	private static readonly StringName FlowUniform = StringName.op_Implicit("flow");

	private static readonly StringName OriginUniform = StringName.op_Implicit("origin");

	internal static readonly Shader Shader = new Shader
	{
		Code = "shader_type canvas_item;\r\nrender_mode unshaded;\r\nuniform float age = 0.0;\r\nuniform float age_end = 0.0;\r\nuniform float flow = 0.0;\r\nuniform bool world_smoke = true;\r\nuniform float seed = 4.7;\r\nuniform float opacity = 0.48;\r\nuniform vec2 origin;\r\nuniform vec2 basis_x;\r\nuniform vec2 basis_y;\r\nvarying vec2 world_point;\r\nfloat hash(vec2 p){vec3 q=fract(vec3(p.xyx)*.1031);q+=dot(q,q.yzx+33.33);return fract((q.x+q.y)*q.z);}\r\nfloat noise(vec2 p){vec2 i=floor(p),f=fract(p);f=f*f*(3.0-2.0*f);return mix(mix(hash(i),hash(i+vec2(1.0,0.0)),f.x),mix(hash(i+vec2(0.0,1.0)),hash(i+vec2(1.0,1.0)),f.x),f.y);}\r\nfloat smoke(vec2 p){return .57*noise(p)+.28*noise(p*2.07+11.3)+.15*noise(p*4.13+7.8);}\r\nvoid vertex(){world_point=origin+basis_x*(VERTEX.x+.5)+basis_y*(VERTEX.y+.5);world_point.y=-world_point.y;}\r\nvoid fragment(){\r\n    float a=mix(age,age_end,UV.x);\r\n    vec2 p=world_smoke?world_point*.035+vec2(-flow*.6,seed):vec2(UV.x*11.0-age*2.4,UV.y*3.5+seed);\r\n    float n=smoke(p);\r\n    float center=world_smoke?.5+.12*(noise(p*.72)-.5):.50+.15*sin(UV.x*8.0+seed)+.15*(noise(p*.72)-.5);\r\n    float width=mix(.27,.10,UV.x)*(1.0+.6*a);\r\n    float body=1.0-smoothstep(width*.15,width,abs(UV.y-center));\r\n    float wave=world_smoke?p.x*2.2:UV.x*24.0+seed;\r\n    float curl=1.0-smoothstep(.012,.055,abs(UV.y-center-width*.82-.045*sin(wave)));\r\n    float tips=smoothstep(0.0,.09,UV.x)*(1.0-smoothstep(.87,1.0,UV.x));\r\n    float dissolve=smoothstep(.55,1.0,a);\r\n    float erosion=world_smoke?smoothstep(.15+dissolve*.48,.43+dissolve*.48,n):smoothstep(.15+a*.48,.43+a*.48,n);\r\n    float alpha=(body*.78+curl*.28)*tips*erosion*opacity*(world_smoke?1.0-dissolve:1.0);\r\n    vec3 tint=mix(vec3(.018,.021,.025),vec3(.12,.14,.15),clamp(curl*.40+n*.12,0.0,1.0));\r\n    float soft_edge=smoothstep(.03,.18,body)*(1.0-smoothstep(.18,.60,body));\r\n    float veil=1.0-smoothstep(.20,.55,n);\r\n    tint=mix(tint,vec3(.45,.50,.52),veil*veil*soft_edge*.55);\r\n    COLOR=vec4(tint,alpha);\r\n}"
	};

	internal readonly ReturnJourney Journey;

	internal double End;

	private readonly MeshInstance2D _node;

	private readonly ShaderMaterial _material;

	private readonly QuadMesh _mesh;

	private readonly float _width;

	private readonly float _homeY;

	internal ReturnSmoke(DisplayLease display, ReturnJourney journey)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		Journey = journey;
		End = journey.End;
		_width = (float)display.Pose.Number("shadowHeight") * Math.Abs(display.Body.Scale.Y) * 0.6f;
		_homeY = display.Home.Y;
		_material = new ShaderMaterial
		{
			Shader = Shader
		};
		_mesh = new QuadMesh
		{
			Size = Vector2.One
		};
		_node = new MeshInstance2D
		{
			Name = StringName.op_Implicit("MoreSilentReturnSmoke"),
			Mesh = (Mesh)(object)_mesh,
			Material = (Material)(object)_material,
			Visible = false
		};
		((GodotObject)_node).SetMeta(StringName.op_Implicit("silent_effect"), Variant.op_Implicit("return-smoke"));
		Node parent = ((Node)display.Body).GetParent();
		parent.AddChild((Node)(object)_node, false, (InternalMode)0);
		parent.MoveChild((Node)(object)_node, 0);
	}

	internal bool Update(double now)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		if (now >= End + 0.8)
		{
			return false;
		}
		double num = Math.Max(Journey.Start, now - 0.8);
		double num2 = Math.Min(End, now);
		if (num2 <= num)
		{
			((CanvasItem)_node).Hide();
			return true;
		}
		float num3 = (float)Journey.Position(num);
		float num4 = (float)(Journey.Position(num2) - (double)num3);
		if (Math.Abs(num4) <= 1f)
		{
			((CanvasItem)_node).Hide();
			return true;
		}
		Vector2 val = default(Vector2);
		((Vector2)(ref val))..ctor(num4, 0f);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))..ctor(0f, (float)(-Math.Sign(num4)) * _width);
		Vector2 val3 = new Vector2(num3, _homeY - _width * 0.7f) - val2 * 0.5f;
		((Node2D)_node).Transform = new Transform2D(val, val2, val3 + (val + val2) * 0.5f);
		_material.SetShaderParameter(OriginUniform, Variant.op_Implicit(val3));
		_material.SetShaderParameter(BasisXUniform, Variant.op_Implicit(val));
		_material.SetShaderParameter(BasisYUniform, Variant.op_Implicit(val2));
		_material.SetShaderParameter(AgeUniform, Variant.op_Implicit((float)((now - num) / 0.8)));
		_material.SetShaderParameter(AgeEndUniform, Variant.op_Implicit((float)((now - num2) / 0.8)));
		_material.SetShaderParameter(FlowUniform, Variant.op_Implicit((float)now));
		((CanvasItem)_node).Show();
		return true;
	}

	public void Dispose()
	{
		((CanvasItem)_node).Hide();
		((Node)_node).QueueFree();
		((GodotObject)_material).Dispose();
		((GodotObject)_mesh).Dispose();
	}
}
internal sealed class ReturnUpperCloth
{
	private sealed record Curve(double[] Shape, double[] Times, double[] Values, double[] Slopes)
	{
		internal double At(double t)
		{
			if (t <= Times[0])
			{
				return Values[0];
			}
			if (t >= Times[^1])
			{
				return Values[^1];
			}
			int i;
			for (i = 0; Times[i + 1] < t; i++)
			{
			}
			double num = Times[i + 1] - Times[i];
			double num2 = (t - Times[i]) / num;
			double num3 = num2 * num2;
			double num4 = num3 * num2;
			return (2.0 * num4 - 3.0 * num3 + 1.0) * Values[i] + (num4 - 2.0 * num3 + num2) * num * Slopes[i] + (-2.0 * num4 + 3.0 * num3) * Values[i + 1] + (num4 - num3) * num * Slopes[i + 1];
		}
	}

	private sealed record Panel(int Count, int[] Free, double Start, double End, Curve[] Curves);

	private readonly Dictionary<string, Panel[]> _panels;

	private readonly double _start;

	private static readonly Dictionary<string, ReturnUpperCloth> Cache = new Dictionary<string, ReturnUpperCloth>();

	internal static ReturnUpperCloth Get(string version)
	{
		if (!Cache.TryGetValue(version, out ReturnUpperCloth value))
		{
			return Cache[version] = new ReturnUpperCloth(version);
		}
		return value;
	}

	private ReturnUpperCloth(string version)
	{
		using Stream stream = typeof(ReturnUpperCloth).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets." + version + ".upper-cloth.bin") ?? throw new InvalidDataException("Missing approved return upper cloth");
		using BinaryReader binaryReader = new BinaryReader(stream);
		if (binaryReader.ReadUInt32() != 843205699)
		{
			throw new InvalidDataException("Invalid compact upper cloth");
		}
		using JsonDocument jsonDocument = JsonDocument.Parse((ReadOnlyMemory<byte>)binaryReader.ReadBytes(binaryReader.ReadInt32()), default(JsonDocumentOptions));
		JsonElement rootElement = jsonDocument.RootElement;
		double @double = rootElement.GetProperty("returnTime").GetDouble();
		_start = rootElement.GetProperty("start").GetDouble();
		Dictionary<string, List<Panel>> dictionary = new Dictionary<string, List<Panel>>();
		foreach (JsonElement item in rootElement.GetProperty("panels").EnumerateArray())
		{
			int[] array = (from x in item.GetProperty("free").EnumerateArray()
				select x.GetInt32()).ToArray();
			List<Curve> list = new List<Curve>();
			foreach (JsonElement item2 in item.GetProperty("modes").EnumerateArray())
			{
				int @int = item2[0].GetInt32();
				double double2 = item2[1].GetDouble();
				double double3 = item2[2].GetDouble();
				double double4 = item2[3].GetDouble();
				double[] array2 = new double[array.Length * 2];
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i] = (double)binaryReader.ReadInt16() * double2;
				}
				double[] array3 = new double[@int];
				double[] array4 = new double[@int];
				double[] array5 = new double[@int];
				for (int j = 0; j < @int; j++)
				{
					array3[j] = (double)(int)binaryReader.ReadUInt16() / 65535.0 * @double;
					array4[j] = (double)binaryReader.ReadInt16() * double3;
					array5[j] = (double)binaryReader.ReadInt16() * double4;
				}
				list.Add(new Curve(array2, array3, array4, array5));
			}
			string key = item.GetProperty("slot").GetString() + ":" + item.GetProperty("attachment").GetString();
			if (!dictionary.TryGetValue(key, out var value))
			{
				dictionary.Add(key, value = new List<Panel>());
			}
			value.Add(new Panel(item.GetProperty("nativeCount").GetInt32(), array, item.GetProperty("range")[0].GetDouble(), item.GetProperty("range")[1].GetDouble(), list.ToArray()));
		}
		if (stream.Position != stream.Length)
		{
			throw new InvalidDataException("Trailing upper cloth payload");
		}
		_panels = dictionary.ToDictionary((KeyValuePair<string, List<Panel>> p) => p.Key, (KeyValuePair<string, List<Panel>> p) => p.Value.ToArray());
	}

	internal bool Apply(string key, double age, double sx, double sy, double[] desired)
	{
		if (!_panels.TryGetValue(key, out Panel[] value))
		{
			return false;
		}
		double num = age + _start;
		bool result = false;
		Panel[] array = value;
		foreach (Panel panel in array)
		{
			if (num < panel.Start || num >= panel.End || panel.Count * 2 != desired.Length)
			{
				continue;
			}
			Curve[] curves = panel.Curves;
			foreach (Curve curve in curves)
			{
				double num2 = curve.At(num);
				for (int k = 0; k < panel.Free.Length; k++)
				{
					int num3 = panel.Free[k] * 2;
					desired[num3] += curve.Shape[k * 2] * num2 * sx;
					desired[num3 + 1] += curve.Shape[k * 2 + 1] * num2 * sy;
				}
			}
			result = true;
		}
		return result;
	}
}
internal static class ShivGeometry
{
	internal readonly record struct InkQuad(string Kind, Vector2 Position, Vector2 Size, float Angle, int Tile, float Alpha, float Erosion)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("Kind = ");
			builder.Append((object?)Kind);
			builder.Append(", Position = ");
			Vector2 val = Position;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Size = ");
			val = Size;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Angle = ");
			builder.Append(Angle.ToString());
			builder.Append(", Tile = ");
			builder.Append(Tile.ToString());
			builder.Append(", Alpha = ");
			builder.Append(Alpha.ToString());
			builder.Append(", Erosion = ");
			builder.Append(Erosion.ToString());
			return true;
		}
	}

	internal readonly record struct Echo(double Age, float Offset, float Alpha);

	private static readonly (double Time, float Offset, double Alpha)[] EchoPlan = new(double, float, double)[3]
	{
		(0.055, -12f, 0.2),
		(0.085, 12f, 0.26),
		(0.115, 0f, 0.32)
	};

	internal static double Random(double n)
	{
		double num = Math.Sin(n * 127.1 + 311.7) * 43758.5453123;
		return num - Math.Floor(num);
	}

	private static double Clamp(double x)
	{
		return Math.Clamp(x, 0.0, 1.0);
	}

	internal static int Echoes(double age, Span<Echo> output)
	{
		int result = 0;
		(double, float, double)[] echoPlan = EchoPlan;
		for (int i = 0; i < echoPlan.Length; i++)
		{
			(double, float, double) tuple = echoPlan[i];
			if (age >= tuple.Item1 && age < tuple.Item1 + 0.42)
			{
				output[result++] = new Echo(tuple.Item1, tuple.Item2, (float)(tuple.Item3 * Math.Pow(1.0 - Clamp((age - tuple.Item1 - 0.035) / 0.385), 1.7)));
			}
		}
		return result;
	}

	internal static Echo[] Echoes(double age)
	{
		Echo[] array = new Echo[3];
		return array[..Echoes(age, array)];
	}

	internal static (Vector3[] Points, Vector2[] UV) Shards(double age, double seed, Vector3[]? points = null, Vector2[]? uv = null)
	{
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		if (age < 0.0 || age >= 0.3)
		{
			return (Points: Array.Empty<Vector3>(), UV: Array.Empty<Vector2>());
		}
		double num = Math.Pow(1.0 - age / 0.3, 1.8);
		double num2 = 1.0 - Math.Pow(1.0 - Clamp(age / 0.3), 2.0);
		if (points == null)
		{
			points = (Vector3[]?)(object)new Vector3[39];
		}
		if (uv == null)
		{
			uv = (Vector2[]?)(object)new Vector2[39];
		}
		int num3 = 0;
		int i;
		for (i = 0; i < 13; i++)
		{
			double num4 = (K(1) - 0.5) * Math.PI * 2.0;
			double num5 = 12.0 + num2 * (28.0 + K(2) * 80.0);
			double num6 = Math.Cos(num4);
			double num7 = Math.Sin(num4);
			double num8 = num6 * num5;
			double num9 = num7 * num5 * 0.7;
			double num10 = (7.0 + K(3) * 13.0) * (1.0 - num2 * 0.6);
			double num11 = 1.6 + K(4) * 2.0;
			for (int j = 0; j < 3; j++)
			{
				double num12 = j switch
				{
					1 => (0.0 - num10) * 0.6, 
					0 => num10, 
					_ => (0.0 - num10) * 0.35, 
				};
				double num13 = j switch
				{
					1 => num11, 
					0 => 0.0, 
					_ => 0.0 - num11, 
				};
				points[num3] = new Vector3((float)(num8 + num6 * num12 - num7 * num13), (float)(num9 + num7 * num12 + num6 * num13), 0f);
				uv[num3++] = new Vector2((float)(0.55 + K(5) * 0.45), (float)(num * 0.8));
			}
		}
		return (Points: points, UV: uv);
		double K(int n)
		{
			return Random(seed + 601.0 + (double)(i * 71) + (double)n);
		}
	}

	internal static List<InkQuad> Ink(double age, double impactAge, double seed, Func<double, Vector2> pose, List<InkQuad>? quads = null)
	{
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		if (quads == null)
		{
			quads = new List<InkQuad>(12);
		}
		quads.Clear();
		int i;
		for (i = 0; i < 5; i++)
		{
			double num = 0.026 + (double)i * 0.02 + (K(0) - 0.5) * 0.012;
			double num2 = age - num;
			int num3 = (i + (int)Math.Floor(Random(seed + 937.0) * 5.0)) % 5;
			double num4 = ((num3 < 2) ? (43.0 + K(4) * 14.0) : ((num3 < 4) ? (28.0 + K(4) * 11.0) : (18.0 + K(4) * 6.0)));
			double num5 = 0.36 + K(1) * 0.12 + num4 * 0.0015;
			if (!(num2 < 0.0) && !(num2 >= num5))
			{
				Vector2 val = pose(num);
				Vector2 val2 = pose(num - 0.003);
				double num6 = ((num3 < 2) ? 0.09 : ((num3 < 4) ? 0.05 : 0.03)) + K(2) * 0.06;
				double num7 = (K(3) - 0.5) * (double)((num3 < 2) ? 140 : 280);
				double num8 = (double)(val.X - val2.X) / 0.003 * num6;
				double num9 = (double)(val.Y - val2.Y) / 0.003 * num6 + num7;
				double num10 = ((num3 < 2) ? 2.8 : ((num3 < 4) ? 4.5 : 6.5)) + K(5) * 1.7;
				double num11 = 430.0 + K(6) * 360.0;
				double num12 = num2 / num5;
				double num13 = Math.Exp((0.0 - num10) * num2);
				double num14 = (1.0 - num13) / num10;
				double num15 = 1.0 + ((num3 < 2) ? 1.15 : 0.7) * num13;
				double num16 = 1.0 - Clamp((num12 - 0.58) / 0.42);
				Add("flight", (double)val.X + num8 * num14, (double)val.Y + (K(7) - 0.5) * 32.0 + num9 * num14 + 0.5 * num11 * num2 * num2, num4 * num15, num4 / num15, Math.Atan2(num9 * num13 + num11 * num2, num8 * num13), (int)Math.Floor(K(8) * 4.0), (0.84 + K(9) * 0.12) * num16, 0.12 * Clamp((num12 - 0.6) / 0.4));
			}
		}
		int j;
		if (impactAge >= 0.0 && impactAge < 0.48)
		{
			double num17 = 1.0 - Math.Pow(1.0 - Clamp(impactAge / 0.065), 3.0);
			double num18 = 1.0 - Clamp((impactAge - 0.17) / 0.31);
			Add("stain", 3.0, 4.0, 92.0 * (0.35 + 0.65 * num17), 69.0 * (0.35 + 0.65 * num17), -0.2 + Random(seed + 3.0) * 0.4, 8 + (int)Math.Floor(Random(seed + 4.0) * 4.0), 0.9 * num18, 0.3 * Clamp((impactAge - 0.2) / 0.28));
			if (impactAge < 0.16)
			{
				Add("splash", 14.0, 0.0, 120.0 * (0.3 + 0.7 * num17), 70.0 * (0.3 + 0.7 * num17), -0.15, 4 + (int)Math.Floor(Random(seed + 5.0) * 4.0), 0.7 * (1.0 - impactAge / 0.16), 0.2 * impactAge / 0.16);
			}
			for (j = 0; j < 5; j++)
			{
				double num19 = (double)(j - 2) * 0.83 + (K(1) - 0.5) * 0.85;
				double num20 = 150.0 + K(2) * 260.0;
				double num21 = Math.Cos(num19) * num20;
				double num22 = Math.Sin(num19) * num20 * 0.7 - 30.0;
				double num23 = ((j % 2 != 0) ? (13.0 + K(3) * 10.0) : (26.0 + K(3) * 13.0));
				double num24 = 1.0 + 0.7 * (1.0 - Clamp(impactAge / 0.12));
				Add("impact", num21 * impactAge, num22 * impactAge + 340.0 * impactAge * impactAge, num23 * num24, num23 / num24, Math.Atan2(num22 + 680.0 * impactAge, num21), (int)Math.Floor(K(4) * 4.0), 0.9 * num18, 0.15 * Clamp((impactAge - 0.25) / 0.23));
			}
		}
		return quads;
		void Add(string kind, double x, double y, double w, double h, double angle, int tile, double alpha, double erosion = 0.0)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			quads.Add(new InkQuad(kind, new Vector2((float)x, (float)y), new Vector2((float)w, (float)h), (float)angle, tile, (float)alpha, (float)erosion));
		}
		double K(int n)
		{
			return Random(seed + (double)(i * 131) + (double)n);
		}
		double K(int n)
		{
			return Random(seed + 703.0 + (double)(j * 79) + (double)n);
		}
	}
}
internal sealed class ShivVisual : IDisposable
{
	internal sealed class Dagger
	{
		internal readonly Texture2D Texture;

		internal readonly ShaderMaterial Material;

		internal readonly double Life;

		internal readonly double History;

		internal readonly double Velocity;

		internal readonly double Width;

		internal readonly int Sections;

		internal readonly Vector2 Origin;

		private readonly Curve? _alpha;

		private readonly Color _color;

		private readonly Vector3[] _points;

		private readonly Vector2[] _uv;

		private readonly Color[] _colors;

		private readonly int[] _indices;

		internal Dagger(NShivThrowVfx parent, double seed)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Expected O, but got Unknown
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0203: Unknown result type (might be due to invalid IL or missing references)
			GpuParticles2D node = ((Node)parent).GetNode<GpuParticles2D>(NodePath.op_Implicit(Paths[0]));
			ParticleProcessMaterial val = (ParticleProcessMaterial)node.ProcessMaterial;
			Texture = node.Texture;
			Material = (ShaderMaterial)((CanvasItem)node).Material;
			Life = node.Lifetime;
			History = node.TrailLifetime;
			Sections = node.TrailSections;
			double num = Math.Sqrt(ShivGeometry.Random(seed + 2.0)) * (double)val.EmissionSphereRadius;
			double num2 = ShivGeometry.Random(seed + 1.0) * Math.PI * 2.0;
			Origin = ((Node2D)node).Position + ((Node2D)((Node)node).GetParent()).Position + new Vector2((float)(Math.Cos(num2) * num), (float)(Math.Sin(num2) * num));
			Velocity = (double)val.InitialVelocityMin + (double)(val.InitialVelocityMax - val.InitialVelocityMin) * ShivGeometry.Random(seed + (double)("initial_velocity".Length * 31));
			Width = (double)Texture.GetWidth() * ((double)val.ScaleMin + (double)(val.ScaleMax - val.ScaleMin) * ShivGeometry.Random(seed + (double)("scale".Length * 31)));
			Texture2D alphaCurve = val.AlphaCurve;
			Texture2D obj = ((alphaCurve is CurveTexture) ? alphaCurve : null);
			_alpha = ((obj != null) ? ((CurveTexture)obj).Curve : null);
			_color = val.Color * ((CanvasItem)node).SelfModulate;
			_points = (Vector3[])(object)new Vector3[(Sections + 1) * 2];
			_uv = (Vector2[])(object)new Vector2[_points.Length];
			_colors = (Color[])(object)new Color[_points.Length];
			_indices = new int[Sections * 6];
			for (int i = 0; i <= Sections; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					_uv[i * 2 + j] = new Vector2((float)j, (float)i / (float)Sections);
				}
			}
			for (int k = 0; k < Sections; k++)
			{
				int num3 = k * 6;
				int num4 = k * 2;
				_indices[num3] = num4;
				_indices[num3 + 1] = num4 + 1;
				_indices[num3 + 2] = num4 + 2;
				_indices[num3 + 3] = num4 + 1;
				_indices[num3 + 4] = num4 + 3;
				_indices[num3 + 5] = num4 + 2;
			}
		}

		internal Vector2 Head(double age)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			return Origin + new Vector2((float)(Velocity * age), 0f);
		}

		internal (Vector3[] Points, Vector2[] UV, Color[] Colors, int[] Indices) Ribbon(double age, float offset = 0f, float opacity = 1f)
		{
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			Vector3[] points = _points;
			Vector2[] uv = _uv;
			Color[] colors = _colors;
			int[] indices = _indices;
			for (int i = 0; i <= Sections; i++)
			{
				double num = age - (double)i * History / (double)(Sections + 1);
				double num2 = Math.Clamp(num, 0.0, Life - 1E-08);
				Vector2 val = Head(num2);
				float num3;
				if (!(num < 0.0) && !(num >= Life))
				{
					Curve? alpha = _alpha;
					num3 = ((alpha != null) ? alpha.Sample((float)(num2 / Life)) : 1f) * opacity;
				}
				else
				{
					num3 = 0f;
				}
				float num4 = num3;
				for (int j = 0; j < 2; j++)
				{
					int num5 = i * 2 + j;
					points[num5] = new Vector3(val.X, val.Y + offset + (float)(((double)j - 0.5) * Width), 0f);
					uv[num5] = new Vector2((float)j, (float)i / (float)Sections);
					Color color = _color;
					color.A = _color.A * num4;
					colors[num5] = color;
				}
				if (i < Sections)
				{
					int num6 = i * 6;
					int num7 = (indices[num6] = i * 2);
					indices[num6 + 1] = num7 + 1;
					indices[num6 + 2] = num7 + 2;
					indices[num6 + 3] = num7 + 1;
					indices[num6 + 4] = num7 + 3;
					indices[num6 + 5] = num7 + 2;
				}
			}
			return (Points: points, UV: uv, Colors: colors, Indices: indices);
		}
	}

	private static readonly string[] Paths = new string[6] { "throw_container/vfx_dagger_spray_dagger", "throw_container/vfx_common_specks_spray", "impact_container/vfx_dagger_impact_core", "impact_container/vfx_common_specks_impact", "impact_container/vfx_common_hit_flare", "impact_container/vfx_poof" };

	private static readonly Dictionary<NShivThrowVfx, ShivVisual> Active = new Dictionary<NShivThrowVfx, ShivVisual>(ReferenceEqualityComparer.Instance);

	internal readonly string Style;

	internal readonly double Seed;

	private readonly NShivThrowVfx _native;

	private readonly SceneTree _tree;

	private readonly Node2D _root;

	private readonly Dagger _dagger;

	private readonly MeshInstance2D[] _ribbons;

	private readonly MeshInstance2D? _shards;

	private readonly Sprite2D[] _ink = Array.Empty<Sprite2D>();

	private readonly ShivGeometry.Echo[] _echoBuffer = new ShivGeometry.Echo[3];

	private readonly Vector3[] _shardPoints = (Vector3[])(object)new Vector3[39];

	private readonly Vector2[] _shardUV = (Vector2[])(object)new Vector2[39];

	private readonly List<ShivGeometry.InkQuad> _inkQuads = new List<ShivGeometry.InkQuad>(12);

	private readonly Func<double, Vector2> _head;

	private double _clock;

	private bool _disposed;

	private bool _daggerReplaced;

	private bool _daggerVisible;

	private static readonly ShaderMaterial ShardMaterial = new ShaderMaterial
	{
		Shader = new Shader
		{
			Code = "shader_type canvas_item;\nrender_mode blend_mix;\nvoid fragment(){vec3 dark=vec3(.012,.035,.08),blue=vec3(.025,.42,.64),cyan=vec3(.19,.84,1.0),silver=vec3(.86,.98,1.0);\nvec3 c=UV.x<.45?mix(dark,blue,UV.x/.45):UV.x<.8?mix(blue,cyan,(UV.x-.45)/.35):mix(cyan,silver,(UV.x-.8)/.2);COLOR=vec4(c,UV.y);}"
		}
	};

	private static Texture2D? _inkTexture;

	private static readonly Shader InkShader = new Shader
	{
		Code = "shader_type canvas_item;\nrender_mode blend_mix;\nuniform float tile;uniform float opacity;uniform float erosion;uniform vec4 basis;\nvec2 coord(vec2 p){return (vec2(mod(tile,4.0),floor(tile/4.0))+clamp(p,vec2(.5/128.0),vec2(127.5/128.0)))/vec2(4.0,3.0);}\nfloat mask(sampler2D tex,vec2 p){return texture(tex,coord(p)).a;}\nvoid fragment(){float a=mask(TEXTURE,UV),stepUV=2.0/128.0;\nvec2 gradient=vec2(mask(TEXTURE,UV+vec2(stepUV,0.0))-mask(TEXTURE,UV-vec2(stepUV,0.0)),mask(TEXTURE,UV+vec2(0.0,stepUV))-mask(TEXTURE,UV-vec2(0.0,stepUV)));\nvec2 localNormal=-gradient*3.8+(UV-vec2(.48,.46))*vec2(1.6,1.2);vec2 direction=basis.xy*localNormal.x+basis.zw*localNormal.y;\nfloat shine=pow(max(dot(normalize(vec3(direction,1.0)),normalize(vec3(-.30,.40,1.0))),0.0),12.0);\nfloat edge=max(smoothstep(.035,.19,length(gradient))*smoothstep(.15,.45,a),.35*smoothstep(.32,.7,a));\nvec3 color=mix(vec3(.012,.014,.018),vec3(.035,.043,.055),smoothstep(.2,.95,a));color+=vec3(.38,.40,.44)*shine*edge;\ncolor+=vec3(.10,.11,.12)*smoothstep(.045,.24,length(gradient))*.15;float coverage=a*smoothstep(erosion,erosion+.13,a);COLOR=vec4(color,coverage*opacity);}"
	};

	internal static int Count => Active.Count;

	internal double Clock => _clock;

	internal int InkCount { get; private set; }

	internal int EchoCount { get; private set; }

	private static Texture2D InkTexture
	{
		get
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Expected O, but got Unknown
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			if (_inkTexture != null)
			{
				return _inkTexture;
			}
			using Stream stream = typeof(ShivVisual).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets.bloody-ink-atlas.png") ?? throw new InvalidDataException("Missing custom ink shapes");
			using MemoryStream memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			Image val = new Image();
			try
			{
				if ((int)val.LoadPngFromBuffer(memoryStream.ToArray()) != 0)
				{
					throw new InvalidDataException("Invalid custom ink shapes");
				}
				return _inkTexture = (Texture2D?)(object)ImageTexture.CreateFromImage(val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	internal static void Reset()
	{
		ShivVisual[] array = Active.Values.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			VisualCleanup.Safely(array[i].Dispose);
		}
	}

	internal static ShivVisual? For(NShivThrowVfx node)
	{
		return Active.GetValueOrDefault(node);
	}

	internal static Shader[] Warm()
	{
		_ = InkTexture;
		return (Shader[])(object)new Shader[2] { InkShader, ShardMaterial.Shader };
	}

	internal static void StyleNative(NShivThrowVfx node, string style)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Expected O, but got Unknown
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Expected O, but got Unknown
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Expected O, but got Unknown
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		if (style == "native")
		{
			return;
		}
		string[] paths = Paths;
		foreach (string text in paths)
		{
			GpuParticles2D node2 = ((Node)node).GetNode<GpuParticles2D>(NodePath.op_Implicit(text));
			ParticleProcessMaterial val2 = (ParticleProcessMaterial)(object)(node2.ProcessMaterial = (Material)(ParticleProcessMaterial)((Resource)node2.ProcessMaterial).Duplicate(false));
			bool trailEnabled = node2.TrailEnabled;
			bool flag = text.StartsWith("impact_container/", StringComparison.Ordinal);
			val2.Color = ((style == "ink") ? new Color(0.42f, 0.22f, 0.7f, 0.66f) : ((((Node)node2).Name == StringName.op_Implicit("vfx_poof")) ? new Color(0.25f, 0.56f, 0.7f, 0.55f) : new Color(0.32f, 0.86f, 1f, 0.85f)));
			if (trailEnabled)
			{
				val2.Color = new Color(1f, 1f, 1f, (style == "ink") ? 0.94f : 0.88f);
			}
			else
			{
				bool flag2 = style == "ink";
				if (flag2)
				{
					bool flag3;
					switch (((object)((Node)node2).Name).ToString())
					{
					case "vfx_poof":
					case "vfx_common_specks_spray":
					case "vfx_common_specks_impact":
						flag3 = true;
						break;
					default:
						flag3 = false;
						break;
					}
					flag2 = flag3;
				}
				if (flag2)
				{
					((CanvasItem)node2).Visible = false;
				}
				else if (flag)
				{
					val2.ScaleMin *= ((style == "ink") ? 0.42f : 0.78f);
					val2.ScaleMax *= ((style == "ink") ? 0.42f : 0.78f);
				}
			}
			Material material = ((CanvasItem)node2).Material;
			ShaderMaterial val3 = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
			if (val3 == null)
			{
				continue;
			}
			Variant shaderParameter = val3.GetShaderParameter(StringName.op_Implicit("lut"));
			try
			{
				GodotObject obj = ((Variant)(ref shaderParameter)).AsGodotObject();
				GradientTexture1D val4 = (GradientTexture1D)(object)((obj is GradientTexture1D) ? obj : null);
				if (val4 == null)
				{
					continue;
				}
				ShaderMaterial val5 = (ShaderMaterial)((Resource)val3).Duplicate(false);
				GradientTexture1D val6 = (GradientTexture1D)((Resource)val4).Duplicate(false);
				Gradient val8 = (val6.Gradient = (Gradient)((Resource)val4.Gradient).Duplicate(false));
				if (trailEnabled)
				{
					val8.Offsets = new float[5] { 0f, 0.49f, 0.56f, 0.78f, 0.85f };
					val8.Colors = (Color[])(object)((!(style == "ink")) ? new Color[5]
					{
						new Color(0.015f, 0.035f, 0.085f, 0f),
						new Color(0.02f, 0.12f, 0.22f, 0.25f),
						new Color(0.025f, 0.26f, 0.4f, 0.85f),
						new Color(0.42f, 0.88f, 1f, 1f),
						new Color(1.2f, 1.35f, 1.45f, 1f)
					} : new Color[5]
					{
						new Color(0.025f, 0.018f, 0.055f, 0f),
						new Color(0.055f, 0.025f, 0.12f, 0.4f),
						new Color(0.13f, 0.075f, 0.23f, 0.92f),
						new Color(0.52f, 0.49f, 0.68f, 1f),
						new Color(1.15f, 1.18f, 1.3f, 1f)
					});
				}
				else if (flag)
				{
					float limit = ((style == "ink") ? 1.2f : 1.4f);
					val8.Colors = ((IEnumerable<Color>)val8.Colors).Select((Func<Color, Color>)((Color c) => new Color(Math.Min(c.R, limit), Math.Min(c.G, limit), Math.Min(c.B, limit), c.A))).ToArray();
				}
				val5.SetShaderParameter(StringName.op_Implicit("lut"), Variant.op_Implicit((GodotObject)(object)val6));
				((CanvasItem)node2).Material = (Material)(object)val5;
			}
			finally
			{
				((IDisposable)(Variant)(ref shaderParameter)/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	internal ShivVisual(NShivThrowVfx native, string style, double seed)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected O, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Expected O, but got Unknown
		_native = native;
		Style = style;
		Seed = seed;
		_tree = ((Node)native).GetTree();
		_dagger = new Dagger(native, seed);
		_head = _dagger.Head;
		_root = new Node2D
		{
			Name = StringName.op_Implicit("MoreSilentShiv")
		};
		((Node)native).AddChild((Node)(object)_root, false, (InternalMode)0);
		_ribbons = Enumerable.Range(0, (!(style == "phantom")) ? 1 : 4).Select((Func<int, MeshInstance2D>)((int i) => new MeshInstance2D
		{
			Name = StringName.op_Implicit("Dagger" + i),
			Texture = _dagger.Texture,
			Material = (Material)(object)_dagger.Material,
			Mesh = (Mesh)new ArrayMesh()
		})).ToArray();
		try
		{
			MeshInstance2D[] ribbons = _ribbons;
			foreach (MeshInstance2D val in ribbons)
			{
				((Node)_root).AddChild((Node)(object)val, false, (InternalMode)0);
			}
			if (style == "phantom")
			{
				((Node)_root).MoveChild((Node)(object)_ribbons[0], _ribbons.Length - 1);
			}
			if (style == "phantom")
			{
				_shards = new MeshInstance2D
				{
					Name = StringName.op_Implicit("PhantomFlecks"),
					Mesh = (Mesh)new ArrayMesh(),
					Material = (Material)(object)ShardMaterial
				};
				((Node)_root).AddChild((Node)(object)_shards, false, (InternalMode)0);
			}
			else
			{
				_ink = Enumerable.Range(0, 12).Select((Func<int, Sprite2D>)((int i) => new Sprite2D
				{
					Name = StringName.op_Implicit("Ink" + i),
					Texture = InkTexture,
					Material = (Material)new ShaderMaterial
					{
						Shader = InkShader
					}
				})).ToArray();
				Sprite2D[] ink = _ink;
				foreach (Sprite2D val2 in ink)
				{
					((Node)_root).AddChild((Node)(object)val2, false, (InternalMode)0);
				}
			}
			GpuParticles2D node = ((Node)native).GetNode<GpuParticles2D>(NodePath.op_Implicit(Paths[0]));
			_daggerVisible = ((CanvasItem)node).Visible;
			((CanvasItem)node).Visible = false;
			_daggerReplaced = true;
			((Node)native).TreeExiting += Dispose;
			_tree.ProcessFrame += Frame;
			Active.Add(native, this);
			Update();
		}
		catch
		{
			if (_daggerReplaced && GodotObject.IsInstanceValid((GodotObject)(object)native))
			{
				((CanvasItem)((Node)native).GetNode<GpuParticles2D>(NodePath.op_Implicit(Paths[0]))).Visible = _daggerVisible;
			}
			Dispose();
			throw;
		}
	}

	private static void MeshData(MeshInstance2D target, Vector3[] points, Vector2[] uv, Color[]? colors = null, int[]? indices = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		ArrayMesh val = (ArrayMesh)target.Mesh;
		val.ClearSurfaces();
		((CanvasItem)target).Visible = points.Length != 0;
		if (points.Length == 0)
		{
			return;
		}
		Array val2 = new Array();
		try
		{
			val2.Resize(13);
			val2[0] = Variant.op_Implicit(points);
			val2[4] = Variant.op_Implicit(uv);
			if (colors != null)
			{
				val2[3] = Variant.op_Implicit(colors);
			}
			if (indices != null)
			{
				val2[12] = Variant.op_Implicit(indices);
			}
			val.AddSurfaceFromArrays((PrimitiveType)3, val2, (Array<Array>)null, (Dictionary)null, (ArrayFormat)0);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void Ribbon(MeshInstance2D target, double age, float offset = 0f, float opacity = 1f)
	{
		(Vector3[], Vector2[], Color[], int[]) tuple = _dagger.Ribbon(age, offset, opacity);
		MeshData(target, tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
	}

	private void Frame()
	{
		if (_disposed)
		{
			return;
		}
		try
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)_native) || !((Node)_native).IsInsideTree() || ((GodotObject)_native).IsQueuedForDeletion())
			{
				Dispose();
			}
			else if (((Node)_native).CanProcess())
			{
				_clock += ((Node)_native).GetProcessDeltaTime();
				if (_clock > Math.Max(_dagger.Life + _dagger.History, 0.8))
				{
					Dispose();
				}
				else
				{
					Update();
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			VisualCleanup.Safely(Dispose);
		}
	}

	private void Update()
	{
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		Ribbon(_ribbons[0], _clock);
		if (Style == "phantom")
		{
			EchoCount = ShivGeometry.Echoes(_clock, _echoBuffer);
			for (int i = 1; i < _ribbons.Length; i++)
			{
				((CanvasItem)_ribbons[i]).Visible = i <= EchoCount;
				if (i <= EchoCount)
				{
					ShivGeometry.Echo echo = _echoBuffer[i - 1];
					Ribbon(_ribbons[i], echo.Age, echo.Offset, echo.Alpha);
				}
			}
			(Vector3[], Vector2[]) tuple = ShivGeometry.Shards(_clock - 0.15, Seed, _shardPoints, _shardUV);
			MeshData(_shards, tuple.Item1, tuple.Item2);
			return;
		}
		List<ShivGeometry.InkQuad> list = ShivGeometry.Ink(_clock, _clock - 0.15, Seed, _head, _inkQuads);
		InkCount = list.Count;
		for (int j = 0; j < _ink.Length; j++)
		{
			Sprite2D val = _ink[j];
			((CanvasItem)val).Visible = j < list.Count;
			if (j < list.Count)
			{
				ShivGeometry.InkQuad inkQuad = list[j];
				((Node2D)val).Position = inkQuad.Position;
				((Node2D)val).Scale = inkQuad.Size / InkTexture.GetSize();
				((Node2D)val).Rotation = inkQuad.Angle;
				ShaderMaterial val2 = (ShaderMaterial)((CanvasItem)val).Material;
				val2.SetShaderParameter(StringName.op_Implicit("tile"), Variant.op_Implicit(inkQuad.Tile));
				val2.SetShaderParameter(StringName.op_Implicit("opacity"), Variant.op_Implicit(inkQuad.Alpha));
				val2.SetShaderParameter(StringName.op_Implicit("erosion"), Variant.op_Implicit(inkQuad.Erosion));
				float globalRotation = ((Node2D)val).GlobalRotation;
				float num = MathF.Cos(globalRotation);
				float num2 = MathF.Sin(globalRotation);
				val2.SetShaderParameter(StringName.op_Implicit("basis"), Variant.op_Implicit(new Vector4(num, 0f - num2, 0f - num2, 0f - num)));
			}
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			if (GodotObject.IsInstanceValid((GodotObject)(object)_tree))
			{
				_tree.ProcessFrame -= Frame;
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)_native))
			{
				((Node)_native).TreeExiting -= Dispose;
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)_root))
			{
				((CanvasItem)_root).Hide();
				((Node)_root).QueueFree();
			}
			Active.Remove(_native);
		}
	}
}
internal static class SkinSmoke
{
	private static readonly Lazy<Shader> Resource = new Lazy<Shader>((Func<Shader>)delegate
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		using Stream stream = typeof(SkinSmoke).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets.skin-smoke.gdshader");
		using StreamReader streamReader = new StreamReader(stream);
		return new Shader
		{
			Code = streamReader.ReadToEnd()
		};
	});

	internal static Shader Shader => Resource.Value;
}
internal sealed class SnakeGeometry
{
	internal readonly record struct Anchors(Vector2 Hand, Vector2 Elbow, Vector2 Upper, double Angle, double Scale = 1.0)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("Hand = ");
			Vector2 val = Hand;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Elbow = ");
			val = Elbow;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Upper = ");
			val = Upper;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Angle = ");
			builder.Append(Angle.ToString());
			builder.Append(", Scale = ");
			builder.Append(Scale.ToString());
			return true;
		}
	}

	internal sealed record Frame(Vector3[] Points, float Alpha, Vector2 Impact, double CloseAt)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Points = ");
			builder.Append(Points);
			builder.Append(", Alpha = ");
			builder.Append(Alpha.ToString());
			builder.Append(", Impact = ");
			Vector2 impact = Impact;
			builder.Append(((object)(Vector2)(ref impact)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", CloseAt = ");
			builder.Append(CloseAt.ToString());
			return true;
		}
	}

	private readonly record struct Point(double X, double Y)
	{
		public static implicit operator Point(Vector2 p)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return new Point(p.X, p.Y);
		}

		public static Point operator +(Point a, Point b)
		{
			return new Point(a.X + b.X, a.Y + b.Y);
		}

		public static Point operator -(Point a, Point b)
		{
			return new Point(a.X - b.X, a.Y - b.Y);
		}

		public static Point operator *(Point p, double s)
		{
			return new Point(p.X * s, p.Y * s);
		}

		public static Point operator /(Point p, double s)
		{
			return new Point(p.X / s, p.Y / s);
		}

		internal double Length()
		{
			return Math.Sqrt(X * X + Y * Y);
		}

		internal Point Lerp(Point other, double t)
		{
			return this + (other - this) * t;
		}

		internal Point Rotated(double a)
		{
			return new Point(X * Math.Cos(a) - Y * Math.Sin(a), X * Math.Sin(a) + Y * Math.Cos(a));
		}
	}

	private readonly record struct Pose(Point Anchor, Point Fixed, Anchors Arm, double Angle, double Scale, double Gather, Strike Motion);

	private readonly record struct Strike(double Time, double CloseAt, double X, double Y, double Pitch, double Jaw, double Unfold, double Recoil, double Follow, double Response, double Alpha, string Mode);

	private static readonly JsonDocument Data = Read();

	internal readonly Vector2[] UV;

	internal readonly Vector2[] Source;

	internal readonly int[] Front;

	internal readonly int[] Back;

	private readonly JsonElement _rows;

	private readonly int _headStart;

	private readonly Point _reference;

	private readonly double _referenceAngle;

	private readonly double _headY;

	private readonly double _headAngle;

	private readonly Dictionary<double, Point> _coils = new Dictionary<double, Point>();

	private readonly Vector3[] _points;

	private static readonly double[][][] Attack = new double[5][][]
	{
		new double[6][]
		{
			new double[2] { 0.0, -138.0 },
			new double[2] { 0.16, -150.0 },
			new double[2] { 0.32, -132.0 },
			new double[2] { 0.56, -30.0 },
			new double[2] { 0.82, 14.0 },
			new double[2] { 1.0, 10.0 }
		},
		new double[6][]
		{
			new double[2] { 0.0, -8.0 },
			new double[2] { 0.16, -14.0 },
			new double[2] { 0.38, 13.0 },
			new double[2] { 0.6, 34.0 },
			new double[2] { 0.8, 22.0 },
			new double[2] { 1.0, 0.0 }
		},
		new double[6][]
		{
			new double[2],
			new double[2] { 0.16, 5.0 },
			new double[2] { 0.38, 15.0 },
			new double[2] { 0.62, 12.0 },
			new double[2] { 0.82, 2.0 },
			new double[2] { 1.0, -7.0 }
		},
		new double[6][]
		{
			new double[2] { 0.0, 0.65 },
			new double[2] { 0.15, 0.72 },
			new double[2] { 0.32, 0.38 },
			new double[2] { 0.6, -0.06 },
			new double[2] { 0.8, 0.0 },
			new double[2] { 1.0, 1.0 }
		},
		new double[5][]
		{
			new double[2],
			new double[2] { 0.2, 0.12 },
			new double[2] { 0.5, 0.8 },
			new double[2] { 0.8, 1.0 },
			new double[2] { 1.0, 0.8 }
		}
	};

	private static readonly double[][][] Recovery = new double[6][][]
	{
		new double[6][]
		{
			new double[2] { 0.0, 10.0 },
			new double[2] { 0.035, 4.0 },
			new double[2] { 0.09, -22.0 },
			new double[2] { 0.18, -10.0 },
			new double[2] { 0.3, 3.0 },
			new double[2] { 0.46, 0.0 }
		},
		new double[6][]
		{
			new double[2],
			new double[2] { 0.035, -4.0 },
			new double[2] { 0.09, -10.0 },
			new double[2] { 0.18, 4.0 },
			new double[2] { 0.3, 5.0 },
			new double[2] { 0.46, 0.0 }
		},
		new double[6][]
		{
			new double[2] { 0.0, -7.0 },
			new double[2] { 0.035, -10.0 },
			new double[2] { 0.09, -3.0 },
			new double[2] { 0.18, 5.0 },
			new double[2] { 0.3, 1.0 },
			new double[2] { 0.46, 0.0 }
		},
		new double[5][]
		{
			new double[2] { 0.0, 1.0 },
			new double[2] { 0.055, 1.0 },
			new double[2] { 0.12, 0.85 },
			new double[2] { 0.22, 0.35 },
			new double[2] { 0.42, 0.55 }
		},
		new double[4][]
		{
			new double[2] { 0.0, 0.8 },
			new double[2] { 0.055, 0.95 },
			new double[2] { 0.18, 0.3 },
			new double[2] { 0.36, 0.0 }
		},
		new double[4][]
		{
			new double[2],
			new double[2] { 0.09, 1.0 },
			new double[2] { 0.2, 0.25 },
			new double[2] { 0.36, 0.0 }
		}
	};

	private static JsonDocument Read()
	{
		return JsonDocument.Parse(typeof(SnakeGeometry).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets.snakebite.json"));
	}

	internal SnakeGeometry(string version)
	{
		JsonElement property = Data.RootElement.GetProperty("geometry");
		JsonElement property2 = Data.RootElement.GetProperty("versions").GetProperty(version);
		Source = ((IEnumerable<JsonElement>)property.GetProperty("vertices").EnumerateArray()).Select((Func<JsonElement, Vector2>)((JsonElement p) => new Vector2(p.GetProperty("x").GetSingle(), p.GetProperty("y").GetSingle()))).ToArray();
		_points = (Vector3[])(object)new Vector3[Source.Length];
		UV = ((IEnumerable<JsonElement>)property.GetProperty("vertices").EnumerateArray()).Select((Func<JsonElement, Vector2>)((JsonElement p) => new Vector2(p.GetProperty("u").GetSingle(), p.GetProperty("v").GetSingle()))).ToArray();
		Front = (from p in property.GetProperty("front").EnumerateArray()
			select p.GetInt32()).ToArray();
		Back = (from p in property.GetProperty("back").EnumerateArray()
			select p.GetInt32()).ToArray();
		_rows = property.GetProperty("rows");
		_headStart = property.GetProperty("headStart").GetInt32();
		JsonElement property3 = property2.GetProperty("referenceWrist");
		_reference = new Point(property3.GetProperty("x").GetDouble(), property3.GetProperty("y").GetDouble());
		_referenceAngle = property2.GetProperty("referenceAngle").GetDouble();
		_headY = property2.GetProperty("headCenterY").GetDouble();
		_headAngle = property2.GetProperty("sourceHeadAngle").GetDouble();
	}

	private static double Smooth(double t)
	{
		t = Math.Clamp(t, 0.0, 1.0);
		return t * t * (3.0 - 2.0 * t);
	}

	private static double Curve(double[][] k, double t)
	{
		if (t <= k[0][0])
		{
			return k[0][1];
		}
		if (t >= k[^1][0])
		{
			return k[^1][1];
		}
		int i;
		for (i = 0; k[i + 1][0] < t; i++)
		{
		}
		double num = k[i + 1][0] - k[i][0];
		double num2 = (t - k[i][0]) / num;
		return (2.0 * num2 * num2 * num2 - 3.0 * num2 * num2 + 1.0) * k[i][1] + (num2 * num2 * num2 - 2.0 * num2 * num2 + num2) * num * Slope(i) + (-2.0 * num2 * num2 * num2 + 3.0 * num2 * num2) * k[i + 1][1] + (num2 * num2 * num2 - num2 * num2) * num * Slope(i + 1);
		double Slope(int j)
		{
			if (j == 0 || j == k.Length - 1)
			{
				return 0.0;
			}
			double num3 = k[j][0] - k[j - 1][0];
			double num4 = k[j + 1][0] - k[j][0];
			double num5 = (k[j][1] - k[j - 1][1]) / num3;
			double num6 = (k[j + 1][1] - k[j][1]) / num4;
			if (!(num5 * num6 <= 0.0))
			{
				return 3.0 * (num3 + num4) / ((2.0 * num4 + num3) / num5 + (num4 + 2.0 * num3) / num6);
			}
			return 0.0;
		}
	}

	private static Strike Motion(double t, string mode)
	{
		double num = ((mode == "normal") ? 0.25 : ((mode == "fast") ? 0.125 : 0.0)) + 2.0 / 15.0;
		double num2 = Math.Min(0.22, num - 0.01);
		double num3 = Math.Max(0.0, t - num);
		bool flag = t <= num;
		double t2 = (flag ? Math.Clamp((t - (num - num2)) / num2, 0.0, 1.0) : num3);
		double[][][] array = (flag ? Attack : Recovery);
		return new Strike(t, num, Curve(array[0], t2), Curve(array[1], t2), Curve(array[2], t2) * Math.PI / 180.0, Curve(array[3], t2), Curve(array[4], t2), flag ? 0.0 : Curve(Recovery[5], num3), Smooth((t - 0.86) / 0.26), Smooth((num3 - 0.035) / 0.145) * 0.065, Smooth(t / 0.045) * (1.0 - Smooth((t - 1.07) / 0.13)), mode);
	}

	private static Point Unit(Point v)
	{
		return v / Math.Max(1E-08, v.Length());
	}

	private static Point Hermite(Point a, Point b, Point ta, Point tb, double u)
	{
		return a * (2.0 * u * u * u - 3.0 * u * u + 1.0) + b * (-2.0 * u * u * u + 3.0 * u * u) + ta * (u * u * u - 2.0 * u * u + u) + tb * (u * u * u - u * u);
	}

	private static Point Axis(double t, Anchors arm)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		Point point = ((Point)arm.Upper).Lerp(arm.Elbow, 0.12);
		Point point2 = Unit(arm.Elbow - point);
		Point point3 = Unit((Point)arm.Hand - (Point)arm.Elbow);
		Point point4 = arm.Hand + point3 * (15.0 * arm.Scale);
		double num = (arm.Elbow - point).Length();
		double num2 = (point4 - arm.Elbow).Length();
		double num3 = num / (num + num2);
		Point point5 = Unit(point2 + point3);
		if (!(t <= num3))
		{
			return Hermite(arm.Elbow, point4, point5 * num2, point3 * num2, (t - num3) / (1.0 - num3));
		}
		return Hermite(point, arm.Elbow, point2 * num, point5 * num, t / num3);
	}

	private static Point Jaw(double x, double y, double closure)
	{
		double num = 430.0 + 60.0 * Math.Clamp((x - 1320.0) / 350.0, 0.0, 1.0);
		double num2 = Smooth((x - 1240.0) / 150.0) * Smooth((y - num) / 100.0);
		double a = -0.6 * closure * num2;
		return new Point(x - 1340.0, y - 420.0).Rotated(a) + new Point(1340.0, 420.0);
	}

	private Point Head(double x, double y, Pose p)
	{
		Point point = Jaw(x, y, p.Motion.Jaw);
		Point point2 = new Point((point.X - 1340.0) * 0.32, (420.0 - point.Y) * 0.32).Rotated(p.Motion.Pitch);
		point2 += new Point(6.799999999999997 + p.Motion.X, 10.0 + (_headY - 420.0) * 0.32 + p.Motion.Y);
		return p.Anchor + point2.Rotated(p.Angle) * p.Scale;
	}

	internal Frame Sample(Anchors arm, double time, string mode)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		Strike motion = Motion(time, mode);
		double num = motion.Follow + (1.0 - motion.Follow) * motion.Response;
		Point point = _reference * arm.Scale;
		Pose p = new Pose(point.Lerp(arm.Hand, num), point, arm, (arm.Angle - _referenceAngle) * num, arm.Scale, 1.0 - 0.38 * Math.Min(1.0, (arm.Hand - point).Length() / 200.0 / arm.Scale), motion);
		Dictionary<double, Point> coils = _coils;
		coils.Clear();
		Vector3[] points = _points;
		foreach (JsonElement item in _rows.EnumerateArray())
		{
			double @double = item.GetProperty("x").GetDouble();
			int @int = item.GetProperty("index").GetInt32();
			if (@double == 1100.0)
			{
				for (int j = 0; j < 2; j++)
				{
					Store(@int + j, Head(Source[@int + j].X, Source[@int + j].Y, p));
				}
				continue;
			}
			Point point2 = Center(@double);
			Point point3 = Center(Math.Min(1100.0, @double + 1.0)) - Center(Math.Max(64.0, @double - 1.0));
			double num2 = Math.Atan2(point3.Y, point3.X) - item.GetProperty("sourceAngle").GetDouble();
			for (int k = 0; k < 2; k++)
			{
				Store(@int + k, point2 + new Point(0.0 - Math.Sin(num2), Math.Cos(num2)) * (item.GetProperty("offsets")[k].GetDouble() * p.Scale));
			}
		}
		for (int l = _headStart; l < points.Length; l++)
		{
			Store(l, Head(Source[l].X, Source[l].Y, p));
		}
		Point point4 = Head(1690.0, 430.0, p with
		{
			Anchor = point,
			Angle = 0.0,
			Motion = Motion(motion.CloseAt, mode)
		});
		return new Frame(points, (float)motion.Alpha, new Vector2((float)point4.X, (float)(0.0 - point4.Y)), motion.CloseAt);
		Point Center(double x)
		{
			if (x <= 900.0)
			{
				return Coil((x - 64.0) / 836.0);
			}
			Point point5 = Coil(1.0);
			Point point6 = Head(1100.0, _headY, p);
			double num3 = (x - 900.0) / 200.0;
			double num4 = 1.0 - num3;
			Point point7 = Unit(Axis(1.0, arm) - Axis(0.9999, arm));
			Strike strike = Motion(time - 0.018 - 0.052 * (1.0 - num3), mode);
			double num5 = Math.Max(28.0 * p.Scale, (point6 - point5).Length() * (0.34 - 0.12 * strike.Unfold));
			double num6 = p.Angle + p.Motion.Pitch + _headAngle;
			Point point8 = point5 + point7 * num5;
			Point point9 = point6 - new Point(Math.Cos(num6), Math.Sin(num6)) * num5;
			Point point10 = point5 * (num4 * num4 * num4) + point8 * (3.0 * num4 * num4 * num3) + point9 * (3.0 * num4 * num3 * num3) + point6 * (num3 * num3 * num3);
			double num7 = Math.Sin(Math.PI * 2.0 * num3) * Math.Pow(Math.Sin(Math.PI * num3), 2.0) * (25.0 * (1.0 - strike.Unfold) + 15.0 * strike.Recoil) * p.Scale;
			return point10 + new Point(0.0 - Math.Sin(p.Angle), Math.Cos(p.Angle)) * num7;
		}
		Point Coil(double t)
		{
			if (coils.TryGetValue(t, out var value))
			{
				return value;
			}
			Strike strike2 = Motion(time - 0.03 - 0.055 * (1.0 - t), mode);
			double num8 = p.Gather * (1.0 - 0.2 * strike2.Unfold + 0.1 * strike2.Recoil);
			Point point11 = Axis(t, arm);
			Point point12 = Unit(Axis(Math.Min(1.0, t + 0.0001), arm) - Axis(Math.Max(0.0, t - 0.0001), arm));
			return coils[t] = point11 + new Point(0.0 - point12.Y, point12.X) * ((46.0 - 14.0 * t) * p.Scale * num8 * Math.Sin(-Math.PI / 2.0 + Math.PI * 5.0 * t));
		}
		void Store(int i, Point v)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			points[i] = new Vector3((float)v.X, (float)(0.0 - v.Y), 0f);
		}
	}
}
internal sealed class SnakeVisual : IDisposable
{
	private static readonly Dictionary<NCreature, SnakeVisual> Active = new Dictionary<NCreature, SnakeVisual>(ReferenceEqualityComparer.Instance);

	private static readonly GDScript Script = Compile();

	private static ImageTexture? Texture;

	private readonly NCreature _actor;

	private readonly SceneTree _tree;

	private readonly object? _combat;

	private readonly string _mode;

	private readonly CharacterRig _rig;

	private readonly RefCounted _reader;

	private readonly Node2D _front = new Node2D
	{
		Name = StringName.op_Implicit("MoreSilentSnakeFront")
	};

	private readonly Node2D _back = new Node2D
	{
		Name = StringName.op_Implicit("MoreSilentSnakeBack")
	};

	private readonly MeshInstance2D _frontMesh = new MeshInstance2D();

	private readonly MeshInstance2D _backMesh = new MeshInstance2D();

	private SnakeGeometry? _geometry;

	private Color[] _colors = Array.Empty<Color>();

	private ulong _lastFrame;

	private bool _disposed;

	internal static bool MouthEnabled = false;

	internal double Age { get; private set; }

	internal SnakeGeometry.Frame? Latest { get; private set; }

	private Node2D Source => ActorPresentation.For(_actor)?.Pose.Sprite ?? _actor.Body;

	internal static SnakeVisual? For(NCreature actor)
	{
		return Active.GetValueOrDefault(actor);
	}

	internal static void Stop(NCreature actor)
	{
		For(actor)?.Dispose();
	}

	internal static void Reset()
	{
		SnakeVisual[] array = Active.Values.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			VisualCleanup.Safely(array[i].Dispose);
		}
	}

	internal static void Warm(string version)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (Texture == null)
		{
			using Stream stream = typeof(SnakeVisual).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets.snakebite.png");
			using MemoryStream memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			Image val = new Image();
			try
			{
				if ((int)val.LoadPngFromBuffer(memoryStream.ToArray()) != 0)
				{
					throw new InvalidDataException("Authored snake texture unavailable");
				}
				Texture = ImageTexture.CreateFromImage(val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		new SnakeGeometry(version);
	}

	private static GDScript Compile()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		using Stream stream = typeof(SnakeVisual).Assembly.GetManifestResourceStream("MoreSilentAnimations.src.Core.snake_native.gd");
		using StreamReader streamReader = new StreamReader(stream);
		GDScript val = new GDScript
		{
			SourceCode = streamReader.ReadToEnd()
		};
		if ((int)((Script)val).Reload(false) != 0)
		{
			throw new InvalidDataException("Snake native binding failed to compile");
		}
		return val;
	}

	internal static void Begin(NCreature actor, string mode)
	{
		Stop(actor);
		if (actor.Entity.IsAlive && !actor.IsPlayingDeathAnimation && CharacterRig.For(actor.Body) != null)
		{
			new SnakeVisual(actor, mode);
		}
	}

	private SnakeVisual(NCreature actor, string mode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		_rig = CharacterRig.For(actor.Body) ?? throw new InvalidDataException("Snake needs the approved native arm rig");
		_actor = actor;
		_tree = ((Node)actor).GetTree();
		_combat = actor.Entity.CombatState;
		_mode = mode;
		_lastFrame = Engine.GetProcessFrames();
		Variant val = ((GodotObject)Script).Call(StringName.op_Implicit("new"), Array.Empty<Variant>());
		try
		{
			_reader = (RefCounted)((Variant)(ref val)).AsGodotObject();
			try
			{
				if (Texture == null)
				{
					Warm(GameContent.Version);
				}
				MeshInstance2D frontMesh = _frontMesh;
				Texture2D texture2 = (_backMesh.Texture = (Texture2D)(object)Texture);
				frontMesh.Texture = texture2;
				_frontMesh.Mesh = (Mesh)new ArrayMesh();
				_backMesh.Mesh = (Mesh)new ArrayMesh();
				((Node)_front).AddChild((Node)(object)_frontMesh, false, (InternalMode)0);
				((Node)_back).AddChild((Node)(object)_backMesh, false, (InternalMode)0);
				Follow();
				Variant val2 = ((GodotObject)_reader).Call(StringName.op_Implicit("bind"), (Variant[])(object)new Variant[2]
				{
					Variant.op_Implicit((GodotObject)(object)Source),
					Variant.op_Implicit(Callable.From<float[]>((Action<float[]>)Render))
				});
				try
				{
					_geometry = new SnakeGeometry(MotionBank.VersionFor(((Variant)(ref val2)).AsString()) ?? throw new InvalidDataException("Unknown Snakebite rig"));
					((Node)actor).TreeExiting += Dispose;
					_tree.ProcessFrame += Frame;
					Active.Add(actor, this);
					Variant val3 = ((GodotObject)_reader).Call(StringName.op_Implicit("anchors"), Array.Empty<Variant>());
					try
					{
						Render(((Variant)(ref val3)).AsFloat32Array());
					}
					finally
					{
						((IDisposable)(Variant)(ref val3)/*cast due to .constrained prefix*/).Dispose();
					}
				}
				finally
				{
					((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
				}
			}
			catch
			{
				Dispose();
				throw;
			}
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private void Follow()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		Node2D source = Source;
		Node parent = ((Node)source).GetParent();
		Node2D[] array = (Node2D[])(object)new Node2D[2] { _back, _front };
		foreach (Node2D val in array)
		{
			if (((Node)val).GetParent() == null)
			{
				parent.AddChild((Node)(object)val, false, (InternalMode)0);
			}
			else if (((Node)val).GetParent() != parent)
			{
				((Node)val).Reparent(parent, false);
			}
			val.Transform = source.Transform;
			((CanvasItem)val).ZIndex = ((CanvasItem)source).ZIndex;
			((CanvasItem)val).ZAsRelative = ((CanvasItem)source).ZAsRelative;
			((CanvasItem)val).Modulate = ((CanvasItem)source).Modulate * ((CanvasItem)source).SelfModulate;
			((CanvasItem)val).Visible = ((CanvasItem)source).Visible;
		}
		if (((Node)_back).GetIndex(false) > ((Node)source).GetIndex(false))
		{
			parent.MoveChild((Node)(object)_back, ((Node)source).GetIndex(false));
		}
		if (((Node)_front).GetIndex(false) < ((Node)source).GetIndex(false))
		{
			parent.MoveChild((Node)(object)_front, ((Node)source).GetIndex(false));
		}
		if (_geometry != null)
		{
			Variant val2 = ((GodotObject)_reader).Call(StringName.op_Implicit("follow"), (Variant[])(object)new Variant[1] { Variant.op_Implicit((GodotObject)(object)source) });
			try
			{
			}
			finally
			{
				((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	private void Synchronize()
	{
		ulong processFrames = Engine.GetProcessFrames();
		if (processFrames != _lastFrame && ((Node)_actor).CanProcess() && !_tree.Paused)
		{
			_lastFrame = processFrames;
			Age += ((Node)_actor).GetProcessDeltaTime();
		}
	}

	private void Render(float[] values)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed || _geometry == null)
		{
			return;
		}
		if (values.Length < 8 || !_rig.Current)
		{
			VisualCleanup.Safely(Dispose);
			return;
		}
		try
		{
			Synchronize();
			if (Age >= 1.2)
			{
				Dispose();
				return;
			}
			SnakeGeometry.Anchors arm = new SnakeGeometry.Anchors(new Vector2(values[0], values[1]), new Vector2(values[2], values[3]), new Vector2(values[4], values[5]), values[6], values[7]);
			Latest = _geometry.Sample(arm, Age, _mode);
			Draw(_backMesh, Latest, _geometry.Back);
			Draw(_frontMesh, Latest, _geometry.Front);
			if (!MouthEnabled)
			{
				return;
			}
			Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("mouth"), (Variant[])(object)new Variant[4]
			{
				Variant.op_Implicit((GodotObject)(object)_front),
				Variant.op_Implicit(Age - Latest.CloseAt),
				Variant.op_Implicit(Latest.Impact),
				Variant.op_Implicit(arm.Scale)
			});
			try
			{
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			VisualCleanup.Safely(Dispose);
		}
	}

	private void Draw(MeshInstance2D node, SnakeGeometry.Frame frame, int[] indices)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		ArrayMesh val = (ArrayMesh)node.Mesh;
		val.ClearSurfaces();
		Array val2 = new Array();
		try
		{
			val2.Resize(13);
			val2[0] = Variant.op_Implicit(frame.Points);
			val2[4] = Variant.op_Implicit(_geometry.UV);
			if (_colors.Length != frame.Points.Length)
			{
				_colors = (Color[])(object)new Color[frame.Points.Length];
			}
			Array.Fill(_colors, new Color(1f, 1f, 1f, frame.Alpha));
			val2[3] = Variant.op_Implicit(_colors);
			val2[12] = Variant.op_Implicit(indices);
			val.AddSurfaceFromArrays((PrimitiveType)3, val2, (Array<Array>)null, (Dictionary)null, (ArrayFormat)0);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void Frame()
	{
		if (_disposed)
		{
			return;
		}
		try
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)_actor) || !((Node)_actor).IsInsideTree() || _combat != _actor.Entity.CombatState || !_actor.Entity.IsAlive || !_rig.Current)
			{
				Dispose();
				return;
			}
			Synchronize();
			if (Age >= 1.2)
			{
				Dispose();
			}
			else
			{
				Follow();
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			VisualCleanup.Safely(Dispose);
		}
	}

	private void ReleaseBinding()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("release"), Array.Empty<Variant>());
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		if (For(_actor) == this)
		{
			Active.Remove(_actor);
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_tree))
		{
			_tree.ProcessFrame -= Frame;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_actor))
		{
			((Node)_actor).TreeExiting -= Dispose;
		}
		VisualCleanup.Safely(ReleaseBinding);
		VisualCleanup.Safely((Action)((GodotObject)_reader).Dispose);
		Node2D[] array = (Node2D[])(object)new Node2D[2] { _front, _back };
		foreach (Node2D node in array)
		{
			VisualCleanup.Safely(delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)node))
				{
					((CanvasItem)node).Hide();
					((Node)node).QueueFree();
				}
			});
		}
	}
}
internal sealed class SteelGeometry
{
	internal readonly record struct Point(double X, double Y, double Z);

	internal readonly record struct Vertex(double X, double Y, double Z, double U, double V, double Alpha);

	internal sealed record Ribbon(Vertex[] Vertices, int Band, double Exit, int Kind = 0);

	internal readonly record struct Knife(double X, double Y, double Z, double Angle, double Size, double Alpha, int Band, bool Flying);

	private sealed record Launch(JsonElement Spec, double Angle, Point Start, Vector2 End, Vector2 First, Vector2 Second)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Spec = ");
			builder.Append(Spec.ToString());
			builder.Append(", Angle = ");
			builder.Append(Angle.ToString());
			builder.Append(", Start = ");
			builder.Append(Start.ToString());
			builder.Append(", End = ");
			Vector2 val = End;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", First = ");
			val = First;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Second = ");
			val = Second;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			return true;
		}
	}

	private static readonly JsonDocument Data = JsonDocument.Parse(typeof(SteelGeometry).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets.steel.json"));

	private static readonly JsonElement Bands = Data.RootElement.GetProperty("bands");

	private static readonly JsonElement Flights = Data.RootElement.GetProperty("flights");

	internal readonly Vector2 Origin;

	internal readonly int Facing;

	internal readonly double Seed;

	private readonly Launch[] _launches;

	private static readonly Dictionary<int, int[]> IndexCache = new Dictionary<int, int[]>();

	internal static string TexturePath => Data.RootElement.GetProperty("texture").GetString();

	internal static JsonElement Version(string version)
	{
		return Data.RootElement.GetProperty("versions").GetProperty(version);
	}

	private static double N(JsonElement e, string key, double fallback = 0.0)
	{
		if (!e.TryGetProperty(key, out var value))
		{
			return fallback;
		}
		return value.GetDouble();
	}

	private static double Smooth(double t)
	{
		t = Math.Clamp(t, 0.0, 1.0);
		return t * t * (3.0 - 2.0 * t);
	}

	private static double Noise(double x)
	{
		double num = Math.Sin(x * 127.1 + 311.7) * 43758.5453;
		return num - Math.Floor(num);
	}

	internal SteelGeometry(Vector2 origin, int facing, double seed)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Origin = origin;
		Facing = facing;
		Seed = seed;
		_launches = Enumerable.Range(0, 15).Select(Release).ToArray();
	}

	internal static double Exit(double age, int band)
	{
		return Smooth((age - (N(Bands[band], "end") - 0.1)) / 0.1);
	}

	internal static double Alpha(double age, int band)
	{
		return ((age < 0.0 || age >= 0.7) ? 0.0 : Smooth(age / 0.045)) * (1.0 - Exit(age, band));
	}

	private Point Orbit(int band, double age, double angle, double radial = 0.0)
	{
		JsonElement e = Bands[band];
		double num = Exit(age, band);
		double num2 = (N(e, "radius") + radial + num * 14.0) * (0.82 + 0.18 * Smooth(age / 0.05));
		double num3 = Math.Cos(angle) * num2;
		double num4 = Math.Sin(angle) * num2;
		return new Point((double)Origin.X + (double)Facing * (45.0 + N(e, "height") * 0.38 + N(e, "offsetX") + num3), (double)(Origin.Y - 24f) - N(e, "height") + num4 * N(e, "depth", 0.27) - num3 * N(e, "slant", 0.31) - num * 7.0, num4);
	}

	internal Ribbon Band(double age, int band, bool accent = false, int shard = -1, Vertex[]? buffer = null)
	{
		JsonElement s = Bands[band];
		double num = Exit(age, band);
		double alpha = Alpha(age, band);
		double phase = N(s, "phase") - 2.2 * age * N(s, "rate") + Noise(Seed) * 0.32 + (accent ? 2.5 : 0.0) + ((shard >= 0) ? ((double)shard * 1.81) : 0.0);
		double sweep = ((shard >= 0) ? (0.14 + Noise(Seed + (double)shard) * 0.28) : (accent ? (N(s, "sweep") * 0.63) : N(s, "sweep"))) * (1.0 - num * 0.78);
		return new Ribbon(Strip((shard >= 0) ? 10 : 80, Edge, buffer), band, num);
		Vertex Edge(double u, double v)
		{
			double num2 = phase - (u - 1.0) * sweep;
			Point point = Orbit(band, age, num2, accent ? 16 : ((shard >= 0) ? (28 + shard * 2) : 0));
			Point point2 = Orbit(band, age, num2 + 0.001);
			Point point3 = Orbit(band, age, num2);
			double num3 = point2.X - point3.X;
			double num4 = point2.Y - point3.Y;
			double num5 = Math.Max(1E-10, Math.Sqrt(num3 * num3 + num4 * num4));
			double num6 = Math.Pow(Math.Sin(Math.PI * u), 0.72) * (0.5 + 0.5 * u);
			double num7 = 1.0 - 0.58 * Math.Max(0.0, 1.0 - Math.Abs(u - (0.34 + (double)band * 0.06)) / 0.038) - 0.32 * Math.Max(0.0, 1.0 - Math.Abs(u - 0.77) / 0.025);
			double num8 = N(s, "width") * (accent ? 0.16 : ((shard >= 0) ? 0.12 : 1.3)) * num6 * num7;
			double num9 = (v - 0.5) * num8;
			return new Vertex(point.X - num4 / num5 * num9, point.Y + num3 / num5 * num9, point.Z, u, v, alpha * (accent ? 0.48 : ((shard >= 0) ? 0.6 : 1.0)));
		}
	}

	private static Vertex[] Strip(int steps, Func<double, double, Vertex> edge, Vertex[]? buffer = null)
	{
		Vertex[] array = buffer ?? new Vertex[(steps + 1) * 2];
		for (int i = 0; i <= steps; i++)
		{
			array[i * 2] = edge((double)i / (double)steps, 0.0);
			array[i * 2 + 1] = edge((double)i / (double)steps, 1.0);
		}
		return array;
	}

	internal static int[] Indices(int count)
	{
		if (IndexCache.TryGetValue(count, out int[] value))
		{
			return value;
		}
		int num = count / 2 - 1;
		int[] array = new int[num * 6];
		for (int i = 0; i < num; i++)
		{
			int num2 = i * 6;
			int num3 = (array[num2] = i * 2);
			array[num2 + 1] = num3 + 1;
			array[num2 + 2] = num3 + 2;
			array[num2 + 3] = num3 + 2;
			array[num2 + 4] = num3 + 1;
			array[num2 + 5] = num3 + 3;
		}
		return IndexCache[count] = array;
	}

	private Launch Release(int i)
	{
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		JsonElement jsonElement = Flights[i];
		int num = (int)N(jsonElement, "band");
		JsonElement e = Bands[num];
		double num2 = Seed + (double)(i * 139);
		double num3 = (N(jsonElement, "direction") + (Noise(num2) - 0.5) * 12.0) * Math.PI / 180.0;
		double num4 = Math.Cos(num3) / (double)Facing;
		double x = (0.0 - (Math.Sin(num3) + N(e, "slant", 0.31) * num4)) / N(e, "depth", 0.27);
		double angle = Math.Atan2(num4, x);
		Point start = Orbit(num, N(jsonElement, "release"), angle, 5.0);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))..ctor((float)(jsonElement.GetProperty("target")[0].GetDouble() + (Noise(num2 + 8.0) - 0.5) * 90.0), (float)(jsonElement.GetProperty("target")[1].GetDouble() + (Noise(num2 + 9.0) - 0.5) * 50.0));
		return new Launch(jsonElement, angle, start, val, new Vector2((float)(start.X + Math.Cos(num3) * N(jsonElement, "lead")), (float)(start.Y + Math.Sin(num3) * N(jsonElement, "lead"))), new Vector2((float)((double)val.X + (Noise(num2 + 10.0) - 0.5) * 180.0), val.Y - 170f));
	}

	private Point Flight(int index, double age)
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		Launch launch = _launches[index];
		JsonElement spec = launch.Spec;
		int num = (int)N(spec, "band");
		double num2 = N(spec, "release");
		if (age <= num2)
		{
			return Orbit(num, age, launch.Angle - 2.2 * N(Bands[num], "rate") * (age - num2), 5.0);
		}
		double num3 = Math.Clamp((age - num2) / N(spec, "duration"), 0.0, 1.0);
		double num4 = 1.0 - num3;
		return new Point(num4 * num4 * num4 * launch.Start.X + 3.0 * num4 * num4 * num3 * (double)launch.First.X + 3.0 * num4 * num3 * num3 * (double)launch.Second.X + num3 * num3 * num3 * (double)launch.End.X, num4 * num4 * num4 * launch.Start.Y + 3.0 * num4 * num4 * num3 * (double)launch.First.Y + 3.0 * num4 * num3 * num3 * (double)launch.Second.Y + num3 * num3 * num3 * (double)launch.End.Y, launch.Start.Z * (1.0 - Smooth(num3 / 0.18)) + 60.0 * Smooth(num3 / 0.18));
	}

	internal Knife Blade(int index, double age)
	{
		JsonElement e = Flights[index];
		Point point = Flight(index, age);
		Point point2 = Flight(index, age + 0.0001);
		double num = Math.Clamp((age - N(e, "release")) / N(e, "duration"), 0.0, 1.0);
		bool flag = age >= N(e, "release");
		return new Knife(point.X, point.Y, point.Z, Math.Atan2(point2.Y - point.Y, point2.X - point.X), N(e, "size") * (flag ? (1.0 - num * 0.18) : ((point.Z > 0.0) ? 1.0 : 0.82)), (age < 0.0) ? 0.0 : (flag ? (1.0 - Smooth((num - 0.66) / 0.34)) : (((age >= 0.7) ? 0.0 : Smooth(age / 0.045)) * ((point.Z > 0.0) ? 1.0 : 0.7))), (int)N(e, "band"), flag);
	}

	internal Ribbon? Tail(int index, double age, Vertex[]? buffer = null)
	{
		double num = N(Flights[index], "release");
		if (age < num)
		{
			return null;
		}
		Knife end = Blade(index, age);
		if (end.Alpha < 0.001)
		{
			return null;
		}
		double start = Math.Max(num, age - 0.055);
		return new Ribbon(Strip(12, Edge, buffer), end.Band, 0.0, 2);
		Vertex Edge(double u, double v)
		{
			Knife knife = Blade(index, start + (age - start) * u);
			double num2 = (v - 0.5) * 6.0 * Math.Sin(Math.PI * u * 0.88);
			return new Vertex(knife.X - Math.Sin(knife.Angle) * num2, knife.Y + Math.Cos(knife.Angle) * num2, knife.Z, u, v, end.Alpha * 0.6 * u);
		}
	}
}
internal sealed class SteelWind : IDisposable
{
	private sealed class Batch
	{
		internal readonly List<Vector3> Points = new List<Vector3>();

		internal readonly List<Vector2> UV = new List<Vector2>();

		internal readonly List<Color> Colors = new List<Color>();

		internal readonly List<int> Indices = new List<int>();

		private Vector3[] _points = Array.Empty<Vector3>();

		private Vector2[] _uv = Array.Empty<Vector2>();

		private Color[] _colors = Array.Empty<Color>();

		private int[] _indices = Array.Empty<int>();

		internal void Clear()
		{
			Points.Clear();
			UV.Clear();
			Colors.Clear();
			Indices.Clear();
		}

		internal void Submit(ArrayMesh mesh)
		{
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Expected O, but got Unknown
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			mesh.ClearSurfaces();
			if (Points.Count == 0)
			{
				return;
			}
			if (_points.Length != Points.Count)
			{
				_points = (Vector3[])(object)new Vector3[Points.Count];
				_uv = (Vector2[])(object)new Vector2[Points.Count];
				_colors = (Color[])(object)new Color[Points.Count];
			}
			if (_indices.Length != Indices.Count)
			{
				_indices = new int[Indices.Count];
			}
			Points.CopyTo(_points);
			UV.CopyTo(_uv);
			Colors.CopyTo(_colors);
			Indices.CopyTo(_indices);
			Array val = new Array();
			try
			{
				val.Resize(13);
				val[0] = Variant.op_Implicit(_points);
				val[4] = Variant.op_Implicit(_uv);
				val[3] = Variant.op_Implicit(_colors);
				val[12] = Variant.op_Implicit(_indices);
				mesh.AddSurfaceFromArrays((PrimitiveType)3, val, (Array<Array>)null, (Dictionary)null, (ArrayFormat)0);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}

		internal void Add(SteelGeometry.Vertex[] vertices, int[] indices, int band, double exit, Vector2 scale)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			int count = Points.Count;
			for (int i = 0; i < vertices.Length; i++)
			{
				SteelGeometry.Vertex vertex = vertices[i];
				Points.Add(new Vector3((float)vertex.X * scale.X, (float)vertex.Y * scale.Y, 0f));
				UV.Add(new Vector2((float)vertex.U, (float)vertex.V));
				Colors.Add(new Color((float)(0.5 + vertex.Z / 1024.0), (float)band / 12f, (float)exit, (float)vertex.Alpha));
			}
			foreach (int num in indices)
			{
				Indices.Add(num + count);
			}
		}
	}

	private static readonly List<SteelWind> Active = new List<SteelWind>();

	private readonly SceneTree _tree;

	private readonly NCreature _actor;

	private readonly object? _combat;

	private readonly Node2D _back = new Node2D
	{
		Name = StringName.op_Implicit("MoreSilentSteelBack")
	};

	private readonly Node2D _front = new Node2D
	{
		Name = StringName.op_Implicit("MoreSilentSteelFront")
	};

	private readonly ArrayMesh[] _meshes = (ArrayMesh[])(object)new ArrayMesh[3]
	{
		new ArrayMesh(),
		new ArrayMesh(),
		new ArrayMesh()
	};

	private readonly ShaderMaterial[] _materials = (ShaderMaterial[])(object)new ShaderMaterial[6];

	private readonly Vector2[] _knifePoints;

	private readonly Vector2[] _knifeUV;

	private readonly int[] _knifeIndices;

	private readonly double _knifeHeading;

	private readonly Batch[] _batches = new Batch[3]
	{
		new Batch(),
		new Batch(),
		new Batch()
	};

	private readonly SteelGeometry.Vertex[][] _bands = (from i in Enumerable.Range(0, 36)
		select new SteelGeometry.Vertex[(i % 3 == 2) ? 22 : 162]).ToArray();

	private readonly SteelGeometry.Vertex[] _knifeWork;

	private readonly SteelGeometry.Vertex[] _tailWork = new SteelGeometry.Vertex[26];

	private readonly Vector2 _worldScale;

	private bool _disposed;

	internal readonly SteelGeometry Geometry;

	private static readonly Shader MaterialShader = new Shader
	{
		Code = "shader_type canvas_item;\nuniform float side=1.0;\nuniform float kind=0.0;\nuniform float age=0.0;\nvarying vec4 meta;\nvoid vertex(){meta=COLOR;}\nvoid fragment(){\n    float z=(meta.r-.5)*1024.0;\n    if((side<0.0&&z>0.0)||(side>0.0&&z<=0.0))discard;\n    if(kind>1.5){float edge=smoothstep(0.0,.22,UV.y)*(1.0-smoothstep(.7,1.0,UV.y));COLOR=vec4(.74,.80,.78,meta.a*edge);}\n    else if(kind>.5){vec4 c=texture(TEXTURE,UV);float luma=dot(c.rgb,vec3(.2126,.7152,.0722));COLOR=vec4(mix(vec3(luma),c.rgb,.5),c.a*meta.a);}\n    else{\n        float u=UV.x,v=UV.y,band=round(meta.g*12.0);\n        vec4 mask=texture(TEXTURE,vec2(.34+v*.32,mix(.12,.78,u)));\n        float edge=smoothstep(0.0,.12,v)*(1.0-smoothstep(.76,1.0,v));\n        float grain=sin(u*69.0-age*28.6+sin(v*16.0+band)*1.7)*sin(u*31.0+v*14.0+band);\n        float tear=smoothstep(-.98+meta.b*1.64,-.77+meta.b*1.64,grain+(1.0-u)*.24);\n        float rim=1.0-smoothstep(.10,.32,v);\n        vec3 color=mix(vec3(.25,.74,.62),vec3(.88,1.0,.95),rim*.9+pow(1.0-v,2.0)*.18);\n        color=mix(vec3(dot(color,vec3(.2126,.7152,.0722))),color,.35);\n        float alpha=mix(.68,1.0,mask.r*mask.a)*edge*tear*meta.a*(z>0.0?.93:.50);\n        COLOR=vec4(color,alpha);\n    }\n}"
	};

	internal static IReadOnlyList<SteelWind> Instances => Active;

	internal double Age { get; private set; }

	internal Vector2 Birth { get; }

	internal static void Reset()
	{
		SteelWind[] array = Active.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			VisualCleanup.Safely(array[i].Dispose);
		}
	}

	internal static Shader Warm(string version)
	{
		SteelGeometry.Version(version);
		ResourceLoader.Load<Texture2D>(SteelGeometry.TexturePath, (string)null, (CacheMode)1);
		_ = GameContent.OriginalTexture;
		return MaterialShader;
	}

	internal SteelWind(NCreature actor, string version, double seed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Expected O, but got Unknown
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Expected O, but got Unknown
		_actor = actor;
		_tree = ((Node)actor).GetTree();
		_combat = actor.Entity.CombatState;
		Birth = ActorPresentation.For(actor)?.DisplayWorldOrigin ?? actor.Body.GlobalPosition;
		Rect2 viewportRect = ((CanvasItem)actor).GetViewportRect();
		_worldScale = ((Rect2)(ref viewportRect)).Size / new Vector2(1920f, 1080f);
		Vector2 origin = Birth / _worldScale;
		ActorPresentation? actorPresentation = ActorPresentation.For(actor);
		Geometry = new SteelGeometry(origin, (actorPresentation != null && ((CanvasItem)actorPresentation.Pose.Sprite).GetGlobalTransform().X.X < 0f) ? (-1) : ((!(((CanvasItem)actor.Body).GetGlobalTransform().X.X < 0f)) ? 1 : (-1)), seed);
		JsonElement recipe = SteelGeometry.Version(version);
		_knifePoints = Points("points");
		_knifeUV = Points("uvs");
		_knifeIndices = (from v in recipe.GetProperty("indices").EnumerateArray()
			select v.GetInt32()).ToArray();
		_knifeHeading = recipe.GetProperty("heading").GetDouble();
		_knifeWork = new SteelGeometry.Vertex[_knifePoints.Length];
		try
		{
			NCombatRoom obj = NCombatRoom.Instance ?? throw new InvalidOperationException("Steel wind outside combat");
			((Node)((Node)obj).GetNode<CanvasItem>(NodePath.op_Implicit("CombatSceneContainer/BackCombatVfxContainer"))).AddChild((Node)(object)_back, false, (InternalMode)0);
			((Node)obj.CombatVfxContainer).AddChild((Node)(object)_front, false, (InternalMode)0);
			Node2D back = _back;
			Transform2D globalTransform = (_front.GlobalTransform = Transform2D.Identity);
			back.GlobalTransform = globalTransform;
			Texture2D val = ResourceLoader.Load<Texture2D>(SteelGeometry.TexturePath, (string)null, (CacheMode)1);
			Texture2D originalTexture = GameContent.OriginalTexture;
			for (int j = 0; j < 2; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					ShaderMaterial val2 = new ShaderMaterial
					{
						Shader = MaterialShader
					};
					val2.SetShaderParameter(StringName.op_Implicit("side"), Variant.op_Implicit((j == 0) ? (-1f) : 1f));
					val2.SetShaderParameter(StringName.op_Implicit("kind"), Variant.op_Implicit((float)k));
					_materials[j * 3 + k] = val2;
					((Node)((j == 0) ? _back : _front)).AddChild((Node)new MeshInstance2D
					{
						Mesh = (Mesh)(object)_meshes[k],
						Texture = ((k == 1) ? originalTexture : val),
						Material = (Material)(object)val2,
						TextureFilter = (TextureFilterEnum)2
					}, false, (InternalMode)0);
				}
			}
			Active.Add(this);
			((Node)actor).TreeExiting += Dispose;
			_tree.ProcessFrame += Frame;
			Render();
		}
		catch
		{
			Dispose();
			throw;
		}
		Vector2[] Points(string key)
		{
			float[] a = (from v in recipe.GetProperty(key).EnumerateArray()
				select v.GetSingle()).ToArray();
			return Enumerable.Range(0, a.Length / 2).Select((Func<int, Vector2>)((int i) => new Vector2(a[i * 2], a[i * 2 + 1]))).ToArray();
		}
	}

	private void Render()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		Batch[] batches = _batches;
		Batch[] array = batches;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Clear();
		}
		for (int j = 0; j < 12; j++)
		{
			if (SteelGeometry.Alpha(Age, j) >= 0.001)
			{
				for (int k = 0; k < 3; k++)
				{
					SteelGeometry.Ribbon ribbon = Geometry.Band(Age, j, k == 1, (k == 2) ? j : (-1), _bands[j * 3 + k]);
					batches[0].Add(ribbon.Vertices, SteelGeometry.Indices(ribbon.Vertices.Length), j, ribbon.Exit, _worldScale);
				}
			}
		}
		for (int l = 0; l < 15; l++)
		{
			SteelGeometry.Knife knife = Geometry.Blade(l, Age);
			if (!(knife.Alpha < 0.001))
			{
				SteelGeometry.Vertex[] knifeWork = _knifeWork;
				for (int m = 0; m < knifeWork.Length; m++)
				{
					Vector2 val = ((Vector2)(ref _knifePoints[m])).Rotated((float)(knife.Angle - _knifeHeading)) * (float)knife.Size;
					knifeWork[m] = new SteelGeometry.Vertex(knife.X + (double)val.X, knife.Y + (double)val.Y, knife.Z, _knifeUV[m].X, _knifeUV[m].Y, knife.Alpha);
				}
				batches[1].Add(knifeWork, _knifeIndices, knife.Band, 0.0, _worldScale);
				SteelGeometry.Ribbon ribbon2 = Geometry.Tail(l, Age, _tailWork);
				if ((object)ribbon2 != null)
				{
					batches[2].Add(ribbon2.Vertices, SteelGeometry.Indices(ribbon2.Vertices.Length), ribbon2.Band, 0.0, _worldScale);
				}
			}
		}
		for (int n = 0; n < 3; n++)
		{
			batches[n].Submit(_meshes[n]);
		}
		ShaderMaterial[] materials = _materials;
		for (int i = 0; i < materials.Length; i++)
		{
			materials[i].SetShaderParameter(StringName.op_Implicit("age"), Variant.op_Implicit((float)Age));
		}
	}

	private void Frame()
	{
		if (_disposed)
		{
			return;
		}
		try
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)_actor) || !((Node)_actor).IsInsideTree() || _combat != _actor.Entity.CombatState)
			{
				Dispose();
			}
			else if (!_tree.Paused && ((Node)_actor).CanProcess())
			{
				Age += ((Node)_actor).GetProcessDeltaTime();
				if (Age >= 0.78)
				{
					Dispose();
				}
				else
				{
					Render();
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			VisualCleanup.Safely(Dispose);
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		Active.Remove(this);
		if (GodotObject.IsInstanceValid((GodotObject)(object)_tree))
		{
			_tree.ProcessFrame -= Frame;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_actor))
		{
			((Node)_actor).TreeExiting -= Dispose;
		}
		Node2D[] array = (Node2D[])(object)new Node2D[2] { _front, _back };
		foreach (Node2D val in array)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((CanvasItem)val).Hide();
				((Node)val).QueueFree();
			}
		}
	}
}
internal sealed class SurgeRecipe
{
	internal sealed record Asset(string Path, Vector2 Size, Rect2 Crop)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Path = ");
			builder.Append((object?)Path);
			builder.Append(", Size = ");
			Vector2 size = Size;
			builder.Append(((object)(Vector2)(ref size)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Crop = ");
			Rect2 crop = Crop;
			builder.Append(((object)(Rect2)(ref crop)/*cast due to .constrained prefix*/).ToString());
			return true;
		}
	}

	internal sealed record Anchor(Vector2 Tip, Vector2 Direction, Vector2 Normal, double Reach, double Angle, double Duration, double Length, double Spread, double CoreWidth, double RibbonWidth, double TrailLife, bool Ribbons)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Tip = ");
			Vector2 val = Tip;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Direction = ");
			val = Direction;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Normal = ");
			val = Normal;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Reach = ");
			builder.Append(Reach.ToString());
			builder.Append(", Angle = ");
			builder.Append(Angle.ToString());
			builder.Append(", Duration = ");
			builder.Append(Duration.ToString());
			builder.Append(", Length = ");
			builder.Append(Length.ToString());
			builder.Append(", Spread = ");
			builder.Append(Spread.ToString());
			builder.Append(", CoreWidth = ");
			builder.Append(CoreWidth.ToString());
			builder.Append(", RibbonWidth = ");
			builder.Append(RibbonWidth.ToString());
			builder.Append(", TrailLife = ");
			builder.Append(TrailLife.ToString());
			builder.Append(", Ribbons = ");
			builder.Append(Ribbons.ToString());
			return true;
		}
	}

	internal readonly record struct Quad(string Asset, bool Front, Vector2 Position, Vector2 Size, float Angle, Color Color, float Life)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("Asset = ");
			builder.Append((object?)Asset);
			builder.Append(", Front = ");
			builder.Append(Front.ToString());
			builder.Append(", Position = ");
			Vector2 val = Position;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Size = ");
			val = Size;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Angle = ");
			builder.Append(Angle.ToString());
			builder.Append(", Color = ");
			Color color = Color;
			builder.Append(((object)(Color)(ref color)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Life = ");
			builder.Append(Life.ToString());
			return true;
		}
	}

	internal const double LifeScale = 2.0;

	internal const double AccentLifeScale = 1.25;

	internal readonly Dictionary<string, Asset> Assets = new Dictionary<string, Asset>();

	internal readonly Dictionary<string, Anchor> Anchors = new Dictionary<string, Anchor>();

	internal readonly string RibbonMaterial;

	private readonly (double Age, Vector2 Center)[] _punchCenters;

	private static readonly Dictionary<string, SurgeRecipe> Cache = new Dictionary<string, SurgeRecipe>();

	private static Vector2 Vector(JsonElement x)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(x[0].GetSingle(), x[1].GetSingle());
	}

	private SurgeRecipe(string version)
	{
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		using Stream utf8Json = typeof(SurgeRecipe).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets.surge.json");
		using JsonDocument jsonDocument = JsonDocument.Parse(utf8Json);
		JsonElement property = jsonDocument.RootElement.GetProperty("versions").GetProperty(version);
		RibbonMaterial = property.GetProperty("ribbonMaterial").GetString();
		_punchCenters = (from p in property.GetProperty("punchCenters").EnumerateArray()
			select ((double, Vector2))(p[0].GetDouble(), new Vector2(p[1].GetSingle(), p[2].GetSingle()))).ToArray();
		foreach (JsonProperty item in property.GetProperty("assets").EnumerateObject())
		{
			JsonElement value = item.Value;
			JsonElement property2 = value.GetProperty("crop");
			Assets[item.Name] = new Asset(value.GetProperty("path").GetString(), new Vector2(value.GetProperty("width").GetSingle(), value.GetProperty("height").GetSingle()), new Rect2(property2[0].GetSingle(), property2[1].GetSingle(), property2[2].GetSingle() - property2[0].GetSingle(), property2[3].GetSingle() - property2[1].GetSingle()));
		}
		foreach (JsonProperty item2 in property.GetProperty("anchors").EnumerateObject())
		{
			JsonElement value2 = item2.Value;
			JsonElement s = value2.GetProperty("spec");
			Anchors[item2.Name] = new Anchor(Vector(value2.GetProperty("tip")), Vector(value2.GetProperty("direction")), Vector(value2.GetProperty("normal")), value2.GetProperty("reach").GetDouble(), value2.GetProperty("angle").GetDouble(), N("duration"), N("length"), N("spread"), N("coreWidth"), N("ribbonWidth"), N("trailLife"), !s.TryGetProperty("ribbons", out var value3) || value3.GetBoolean());
			double N(string name)
			{
				return s.GetProperty(name).GetDouble();
			}
		}
	}

	internal static SurgeRecipe Get(string version)
	{
		if (!Cache.TryGetValue(version, out SurgeRecipe value))
		{
			return Cache[version] = new SurgeRecipe(version);
		}
		return value;
	}

	internal Vector2 PunchCenter(double age)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		int num = Math.Clamp((int)(age * 240.0), 0, _punchCenters.Length - 2);
		(double, Vector2) tuple = _punchCenters[num];
		(double, Vector2) tuple2 = _punchCenters[num + 1];
		return ((Vector2)(ref tuple.Item2)).Lerp(tuple2.Item2, (float)Math.Clamp((age - tuple.Item1) / (tuple2.Item1 - tuple.Item1), 0.0, 1.0));
	}

	private static double Smooth(double t)
	{
		t = Math.Clamp(t, 0.0, 1.0);
		return t * t * (3.0 - 2.0 * t);
	}

	internal IReadOnlyList<Quad> Sample(string card, double age, double start, double hit, List<Quad>? buffer = null)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_064b: Unknown result type (might be due to invalid IL or missing references)
		Anchor a = Anchors[card];
		List<Quad> result = buffer ?? new List<Quad>(7);
		result.Clear();
		double num = ((age > 0.0) ? (age / 1.25) : age);
		age = ((age > 0.0) ? (age / 2.0) : age);
		Color color2 = ((card == "Dash") ? new Color(1f, 0.11764706f, 0f, 1f) : new Color(12f / 85f, 32f / 51f, 36f / 85f, 1f));
		Color val = ((card == "Dash") ? new Color(1f, 0.95686275f, 81f / 85f, 1f) : new Color(0.8090962f, 1f, 0.87317574f, 1f));
		Color color3 = (Color)((card == "Dash") ? new Color(1f, 0.56078434f, 0.5019608f, 1f) : val);
		double num2 = ((age < 0.0) ? Smooth((age + hit - start) / Math.Max(1E-06, hit - start)) : 1.0);
		double num3 = Smooth(age / a.Duration);
		double num4 = (0.22 + 0.78 * num2) * (1.0 - Smooth((age - 0.025) / (a.Duration - 0.025)));
		double num5 = (0.22 + 0.78 * num2) * (1.0 - Smooth((num - 0.025) / (a.Duration - 0.025)));
		double num6 = a.Reach * a.Length;
		double num7 = a.Reach * a.Spread * (0.75 + 0.25 * num2 + 0.25 * num3);
		double num8 = a.Reach * a.Spread * (0.75 + 0.25 * num2 + 0.25 * Smooth(num / a.Duration));
		Vector2 head = a.Tip - a.Direction * (float)(num6 * 0.2 * (1.0 - num2));
		Add("core", front: false, (0.0 - num6) * 0.38, 0.0, num6 * 0.86, a.Reach * a.CoreWidth * 2.8, a.Angle, color2, 0.22 * num4);
		double num9 = num4 * (1.0 - Smooth(Math.Max(0.0, age) / a.TrailLife));
		double life2 = Math.Clamp((hit + num - start) / (hit + a.Duration - start), 0.0, 1.0);
		for (int i = -1; i <= 1; i += 2)
		{
			Add("trail", front: false, (0.0 - num6) * 0.67, (double)i * num7 * 0.5, num6 * 0.065, num6 * 0.9, a.Angle + Math.PI / 2.0, color3, 0.5 * num9);
			if (a.Ribbons)
			{
				Add("ribbon", i > 0, (0.0 - num6) * 0.5, (double)i * num8 * 0.55, (double)i * a.Reach * a.RibbonWidth, num6 * 1.5, a.Angle + Math.PI / 2.0, Colors.White, num5 * ((i < 0) ? 0.88 : 0.76), life2);
			}
		}
		double num10 = num4 * (1.0 - Smooth(Math.Max(0.0, age - 0.015) / (0.11 + ((card == "Pounce") ? 0.025 : 0.0))));
		Add("core", front: true, (0.0 - num6) * 0.35, 0.0, num6 * 0.78, a.Reach * a.CoreWidth, a.Angle, val, 0.87 * num10);
		double num11 = num5 * (1.0 - Smooth(Math.Max(0.0, num - 0.015) / (0.11 + ((card == "Pounce") ? 0.025 : 0.0))));
		if (a.Ribbons)
		{
			Add("tip", front: true, (0.0 - num6) * 0.32 / 2.0, 0.0, a.Reach * 0.075, num6 * 0.32, a.Angle + Math.PI / 2.0, val, 0.95 * num11);
		}
		return result;
		void Add(string asset, bool front, double along, double side, double w, double h, double angle, Color color, double opacity, double life = 0.0)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			List<Quad> list = result;
			Vector2 position = head + a.Direction * (float)along + a.Normal * (float)side;
			Vector2 size = new Vector2((float)w, (float)h);
			float angle2 = (float)angle;
			Color color4 = color;
			color4.A = (float)opacity;
			list.Add(new Quad(asset, front, position, size, angle2, color4, (float)life));
		}
	}
}
internal sealed record ThrustProfile(string Card, bool Punch = false, bool DownStab = false, bool Normal = false, bool NativeAttack = false)
{
	internal static ThrustProfile? For(AbstractModel? model)
	{
		if (!AnimationSettings.Allows(model))
		{
			return null;
		}
		return (model is LeadingStrike) ? new ThrustProfile("LeadingStrike", Punch: false, DownStab: false, Normal: false, NativeAttack: true) : ((model is Slice) ? new ThrustProfile("Slice", Punch: false, DownStab: false, Normal: false, NativeAttack: true) : ((model is PreciseCut) ? new ThrustProfile("PreciseCut", Punch: false, DownStab: false, Normal: false, NativeAttack: true) : ((model is Finisher) ? new ThrustProfile("Finisher", Punch: false, DownStab: false, Normal: false, NativeAttack: true) : ((model is Predator) ? new ThrustProfile("Predator") : ((model is Pounce) ? new ThrustProfile("Pounce") : ((model is PoisonedStab) ? new ThrustProfile("PoisonedStab") : ((model is Neutralize) ? new ThrustProfile("Neutralize") : ((model is Assassinate) ? new ThrustProfile("Assassinate") : ((model is Dash) ? new ThrustProfile("Dash") : ((model is SuckerPunch) ? new ThrustProfile("SuckerPunch", Punch: true) : ((model is Backstab) ? new ThrustProfile("Backstab", Punch: false, DownStab: true) : ((model is Murder) ? new ThrustProfile("Murder", Punch: false, DownStab: true) : ((model is Suppress) ? new ThrustProfile("Suppress", Punch: false, DownStab: true) : ((!(model is TheHunt)) ? null : new ThrustProfile("TheHunt", Punch: false, DownStab: true)))))))))))))));
	}
}
internal sealed class ThrustSurge : IDisposable
{
	internal sealed class Burst : IDisposable
	{
		private readonly Node2D _back;

		private readonly Node2D _front;

		private readonly Sprite2D[] _sprites;

		private readonly SurgeRecipe _recipe;

		private readonly string _card;

		private readonly double _start;

		private readonly bool _rearOnly;

		private double _hit;

		private readonly ShaderMaterial _ribbon;

		private bool _disposed;

		private readonly List<SurgeRecipe.Quad> _quads = new List<SurgeRecipe.Quad>(7);

		internal bool Confirmed { get; private set; }

		internal bool Alive => !_disposed;

		internal Burst(DisplayLease display, SurgeRecipe recipe, string card, double start, double hit, bool rearOnly = false)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Expected O, but got Unknown
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Expected O, but got Unknown
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_0193: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Expected O, but got Unknown
			_recipe = recipe;
			_card = card;
			_start = start;
			_hit = hit;
			_rearOnly = rearOnly;
			_ribbon = Ribbon(recipe);
			_back = new Node2D
			{
				Name = StringName.op_Implicit("MoreSilentSurgeBack"),
				Transform = display.Root.Transform,
				Modulate = ((CanvasItem)display.Root).Modulate
			};
			_front = new Node2D
			{
				Name = StringName.op_Implicit("MoreSilentSurgeFront"),
				Transform = display.Root.Transform,
				Modulate = ((CanvasItem)display.Root).Modulate
			};
			display.AttachAttack(_back, 10);
			display.AttachAttack(_front, 30);
			IReadOnlyList<SurgeRecipe.Quad> readOnlyList = Sample(start);
			_sprites = (Sprite2D[])(object)new Sprite2D[readOnlyList.Count];
			try
			{
				for (int i = 0; i < readOnlyList.Count; i++)
				{
					SurgeRecipe.Quad quad = readOnlyList[i];
					SurgeRecipe.Asset asset = recipe.Assets[quad.Asset];
					Texture2D val = ResourceLoader.Load<Texture2D>(asset.Path, (string)null, (CacheMode)1) ?? throw new InvalidDataException("Missing native effect texture: " + asset.Path);
					if (val.GetSize() != asset.Size)
					{
						throw new InvalidDataException("Native effect dimensions changed: " + asset.Path);
					}
					Sprite2D val2 = new Sprite2D
					{
						Texture = val,
						Name = StringName.op_Implicit("MoreSilent_" + quad.Asset),
						Material = (Material)((quad.Asset == "ribbon") ? ((object)_ribbon) : ((object)Additive)),
						TextureFilter = (TextureFilterEnum)2,
						RegionEnabled = (quad.Asset != "ribbon"),
						RegionRect = asset.Crop,
						RegionFilterClipEnabled = true
					};
					((Node)(quad.Front ? _front : _back)).AddChild((Node)(object)val2, false, (InternalMode)0);
					_sprites[i] = val2;
				}
				Update(start);
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		internal void Hit(double time)
		{
			_hit = time;
			Confirmed = true;
		}

		private IReadOnlyList<SurgeRecipe.Quad> Sample(double time)
		{
			_recipe.Sample(_card, time - _hit, _start, _hit, _quads);
			if (_rearOnly)
			{
				_quads.RemoveAll((SurgeRecipe.Quad q) => q.Asset != "trail");
			}
			return _quads;
		}

		internal void Update(double time)
		{
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			if (_disposed)
			{
				return;
			}
			if (!GodotObject.IsInstanceValid((GodotObject)(object)_back) || !((Node)_back).IsInsideTree() || time >= _hit + _recipe.Anchors[_card].Duration * 2.0)
			{
				Dispose();
				return;
			}
			IReadOnlyList<SurgeRecipe.Quad> readOnlyList = Sample(time);
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				SurgeRecipe.Quad quad = readOnlyList[i];
				SurgeRecipe.Asset asset = _recipe.Assets[quad.Asset];
				Sprite2D obj = _sprites[i];
				((Node2D)obj).Position = quad.Position;
				((Node2D)obj).Rotation = quad.Angle;
				Vector2 size = quad.Size;
				Vector2 size2;
				if (!(quad.Asset == "ribbon"))
				{
					Rect2 crop = asset.Crop;
					size2 = ((Rect2)(ref crop)).Size;
				}
				else
				{
					size2 = asset.Size;
				}
				((Node2D)obj).Scale = size / size2;
				((CanvasItem)obj).Modulate = quad.Color;
				if (quad.Asset == "ribbon")
				{
					_ribbon.SetShaderParameter(StringName.op_Implicit("presentation_life"), Variant.op_Implicit(quad.Life));
				}
			}
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			Node2D[] array = (Node2D[])(object)new Node2D[2] { _back, _front };
			foreach (Node2D val in array)
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					((CanvasItem)val).Hide();
					((Node)val).QueueFree();
				}
			}
			((GodotObject)_ribbon).Dispose();
		}
	}

	private static readonly CanvasItemMaterial Additive = new CanvasItemMaterial
	{
		BlendMode = (BlendModeEnum)1
	};

	private static readonly Dictionary<string, Shader> RibbonShaders = new Dictionary<string, Shader>();

	private readonly DisplayLease _display;

	private readonly SurgeRecipe _recipe;

	private readonly List<Burst> _bursts = new List<Burst>();

	internal bool Active => _bursts.Any((Burst b) => b.Alive);

	internal int Count => _bursts.Count((Burst b) => b.Alive);

	private static ShaderMaterial Ribbon(SurgeRecipe recipe)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00b0: Expected O, but got Unknown
		ShaderMaterial val = ResourceLoader.Load<ShaderMaterial>(recipe.RibbonMaterial, (string)null, (CacheMode)1) ?? throw new InvalidDataException("Missing original ribbon material");
		if (!RibbonShaders.TryGetValue(recipe.RibbonMaterial, out Shader value))
		{
			string code = val.Shader.Code;
			if (!code.Contains("INSTANCE_CUSTOM.y/INSTANCE_CUSTOM.w") || !code.Contains("INSTANCE_CUSTOM.z"))
			{
				throw new InvalidDataException("Original ribbon particle clock changed");
			}
			code = code.Replace("varying float lifetime;", "uniform float presentation_life = 0.0;\nvarying float lifetime;").Replace("INSTANCE_CUSTOM.y/INSTANCE_CUSTOM.w", "presentation_life").Replace("INSTANCE_CUSTOM.z", "0.0");
			Dictionary<string, Shader> ribbonShaders = RibbonShaders;
			string ribbonMaterial = recipe.RibbonMaterial;
			Shader val2 = new Shader
			{
				Code = code
			};
			value = val2;
			ribbonShaders[ribbonMaterial] = val2;
		}
		ShaderMaterial val3 = (ShaderMaterial)((Resource)val).Duplicate(false);
		val3.Shader = value;
		return val3;
	}

	internal ThrustSurge(DisplayLease display)
	{
		_display = display;
		_recipe = SurgeRecipe.Get(display.Pose.Version);
	}

	internal Burst Begin(string card, double start, double hit, bool rearOnly = false)
	{
		Burst burst = new Burst(_display, _recipe, card, start, hit, rearOnly);
		_bursts.Add(burst);
		return burst;
	}

	internal void Update(double time)
	{
		foreach (Burst burst in _bursts)
		{
			burst.Update(time);
		}
		_bursts.RemoveAll((Burst b) => !b.Alive);
	}

	public void Dispose()
	{
		foreach (Burst burst in _bursts)
		{
			burst.Dispose();
		}
		_bursts.Clear();
	}
}
[HarmonyPatch(typeof(NSpriteAnimator), "_Ready")]
internal static class WhiteSlashPatch
{
	private static readonly ShaderMaterial White = new ShaderMaterial
	{
		Shader = new Shader
		{
			Code = "shader_type canvas_item;\nrender_mode blend_mix;\nvarying vec4 tint;\nvoid vertex(){tint=COLOR;}\nvoid fragment(){vec4 source=texture(TEXTURE,UV);float intensity=max(source.r,max(source.g,source.b));COLOR=vec4(vec3(1.0),source.a*intensity*tint.a);}"
		}
	};

	[HarmonyPrefix]
	private static void Prefix(NSpriteAnimator __instance)
	{
		try
		{
			if (AttackPresentationPatch.WhiteSlash && ((Node)__instance).SceneFilePath == "res://scenes/vfx/vfx_attack_slash.tscn")
			{
				((CanvasItem)__instance).Material = (Material)(object)White;
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
internal static class TrackingGeometry
{
	internal sealed record Track(string Asset, string Gait, double Length, double MinLength, double Stride, double HalfWidth, double SupportSpan, int Contacts, double FarFloor, double FadePower, Vector2 Start, Vector2 End)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Asset = ");
			builder.Append((object?)Asset);
			builder.Append(", Gait = ");
			builder.Append((object?)Gait);
			builder.Append(", Length = ");
			builder.Append(Length.ToString());
			builder.Append(", MinLength = ");
			builder.Append(MinLength.ToString());
			builder.Append(", Stride = ");
			builder.Append(Stride.ToString());
			builder.Append(", HalfWidth = ");
			builder.Append(HalfWidth.ToString());
			builder.Append(", SupportSpan = ");
			builder.Append(SupportSpan.ToString());
			builder.Append(", Contacts = ");
			builder.Append(Contacts.ToString());
			builder.Append(", FarFloor = ");
			builder.Append(FarFloor.ToString());
			builder.Append(", FadePower = ");
			builder.Append(FadePower.ToString());
			builder.Append(", Start = ");
			Vector2 val = Start;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", End = ");
			val = End;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			return true;
		}
	}

	internal sealed record Print(string Asset, double T, double X, double Y, double Length, double Angle, double Side, double Alpha, double RevealAt, double FadeAt);

	internal sealed record Residue(string Kind, double Width, double Phase, double Distance, double Pitch, Vector3[] Points, Vector2[] UV, int[] Indices);

	private sealed record PathPoint(double T, double X, double Y, double Distance);

	private sealed class RandomStream(uint seed)
	{
		private uint _seed = seed;

		internal double Next()
		{
			seed += 1831565813u;
			uint num = (seed ^ (seed >> 15)) * (1 | seed);
			num ^= num + (num ^ (num >> 7)) * (0x3D | num);
			return (double)(num ^ (num >> 14)) / 4294967296.0;
		}
	}

	internal static readonly JsonDocument Data = JsonDocument.Parse(typeof(TrackingGeometry).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets.tracking.json"));

	private static double Clamp(double t)
	{
		return Math.Clamp(t, 0.0, 1.0);
	}

	internal static double Smooth(double t)
	{
		t = Clamp(t);
		return t * t * (3.0 - 2.0 * t);
	}

	internal static double Number(JsonElement e, string key, double fallback = 0.0)
	{
		if (!e.TryGetProperty(key, out var value))
		{
			return fallback;
		}
		return value.GetDouble();
	}

	internal static uint Hash(string s)
	{
		uint num = 2166136261u;
		foreach (char c in s)
		{
			num = (num ^ c) * 16777619;
		}
		return num;
	}

	internal static Track ReadTrack(JsonElement t, Vector2 start, Vector2 end)
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		return new Track(t.GetProperty("asset").GetString(), t.GetProperty("gait").GetString(), Number(t, "length"), Number(t, "minLength"), Number(t, "stride"), Number(t, "halfWidth"), Number(t, "supportSpan"), (int)Number(t, "contactCount"), Number(t, "farFadeFloor"), Number(t, "fadePower", 1.5), start, end);
	}

	internal static List<Print> Build(string id, IEnumerable<Track> tracks)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		List<Print> list = new List<Print>();
		int num = 0;
		foreach (Track track in tracks)
		{
			RandomStream randomStream = new RandomStream(Hash(id + ":" + num++));
			List<PathPoint> path = new List<PathPoint>
			{
				new PathPoint(0.0, track.Start.X, track.Start.Y, 0.0)
			};
			for (int j = 1; j <= 240; j++)
			{
				double num2 = (double)j / 240.0;
				double num3 = (double)track.Start.X + (double)(track.End.X - track.Start.X) * num2;
				double num4 = (double)track.Start.Y + (double)(track.End.Y - track.Start.Y) * num2 - Math.Sin(Math.PI * num2) * 28.0 + Math.Sin(Math.PI * 2.0 * num2) * 12.0;
				List<PathPoint> list2 = path;
				PathPoint pathPoint = list2[list2.Count - 1];
				double num5 = num3 - pathPoint.X;
				double num6 = (num4 - pathPoint.Y) / 0.38;
				path.Add(new PathPoint(num2, num3, num4, pathPoint.Distance + Math.Sqrt(num5 * num5 + num6 * num6)));
			}
			List<PathPoint> list3 = path;
			double total = list3[list3.Count - 1].Distance;
			if (total < 30.0)
			{
				continue;
			}
			List<Print> list4 = new List<Print>();
			double num7 = Math.Max(track.MinLength, track.Length);
			double num8 = track.Stride / (double)((track.Gait == "bound") ? 1 : 2);
			double num9 = 18.0;
			double num10 = 0.0;
			int num11 = 0;
			while (num9 < total - 12.0 && num11 < 70)
			{
				int side = ((num11 % 2 != 0) ? 1 : (-1));
				num10 = 0.65 * num10 + (randomStream.Next() - 0.5) * 0.07;
				int group = Math.Max(2, (int)Math.Ceiling((double)track.Contacts / 2.0));
				IEnumerable<(double, int)> enumerable2;
				if (!(track.Gait == "multi"))
				{
					IEnumerable<(double, int)> enumerable = ((!(track.Gait == "bound")) ? new(double, int)[1] { (0.0, side) } : new(double, int)[2]
					{
						((0.0 - num7) * 0.12, -1),
						(num7 * 0.12, 1)
					});
					enumerable2 = enumerable;
				}
				else
				{
					enumerable2 = from i in Enumerable.Range(0, @group)
						select (((double)i / (double)(@group - 1) - 0.5) * track.SupportSpan, (i % 2 == 0) ? side : (-side));
				}
				foreach (var (num12, num13) in enumerable2)
				{
					if (!(num9 + num12 < 0.0) && !(num9 + num12 > total - 12.0))
					{
						(double, double, double, double, double) tuple2 = At(num9 + num12);
						double num14 = (double)num13 * track.HalfWidth + num7 * num10;
						double angle = Math.Atan2(tuple2.Item5, tuple2.Item4) + (double)num13 * 0.065 + (randomStream.Next() - 0.5) * 0.07;
						list4.Add(new Print(track.Asset, tuple2.Item1, tuple2.Item2 - tuple2.Item5 * num14, tuple2.Item3 + tuple2.Item4 * num14 * 0.38, num7, angle, num13, 0.0, 0.03 + tuple2.Item1 * 0.72, 1.1 + Math.Sqrt(Clamp(tuple2.Item1)) * 0.72));
					}
				}
				num9 += num8 * (0.97 + (randomStream.Next() - 0.5) * 0.1);
				num11++;
			}
			list4 = list4.OrderBy((Print p) => p.T).ToList();
			double first = list4.FirstOrDefault()?.T ?? 0.0;
			double lastT = list4.LastOrDefault()?.T ?? 1.0;
			double far = Math.Max(0.07, track.FarFloor);
			list.AddRange(list4.Select((Print p) => p with
			{
				Alpha = far + (1.0 - far) * Math.Pow(1.0 - Clamp((p.T - first) / Math.Max(0.001, lastT - first)), track.FadePower)
			}));
			(double T, double X, double Y, double Tx, double Ty) At(double s)
			{
				int num15 = Math.Max(1, path.FindIndex((PathPoint p) => p.Distance >= Math.Clamp(s, 0.0, total)));
				PathPoint pathPoint2 = path[num15 - 1];
				PathPoint pathPoint3 = path[num15];
				double num16 = Clamp((s - pathPoint2.Distance) / Math.Max(1E-10, pathPoint3.Distance - pathPoint2.Distance));
				double num17 = pathPoint3.X - pathPoint2.X;
				double num18 = (pathPoint3.Y - pathPoint2.Y) / 0.38;
				double num19 = Math.Sqrt(num17 * num17 + num18 * num18);
				return (T: pathPoint2.T + (pathPoint3.T - pathPoint2.T) * num16, X: pathPoint2.X + num17 * num16, Y: pathPoint2.Y + (pathPoint3.Y - pathPoint2.Y) * num16, Tx: num17 / num19, Ty: num18 / num19);
			}
		}
		return list.OrderBy((Print p) => p.T).ToList();
	}

	internal static double Alpha(Print p, double age)
	{
		return p.Alpha * Smooth((age - p.RevealAt) / 0.1) * (1.0 - Smooth((age - p.FadeAt) / 0.12));
	}

	internal static Residue Trace(string kind, double width, double phase, double pitch, Vector2 start, Vector2 end)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		bool flag = kind == "settle";
		if (flag)
		{
			((Vector2)(ref start))..ctor(end.X - (float)(width * 1.5), end.Y);
		}
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i <= 160; i++)
		{
			double num = (double)i / 160.0;
			double num2 = (double)start.X + (double)(end.X - start.X) * num;
			double num3 = (double)start.Y + (double)(end.Y - start.Y) * num - Math.Sin(Math.PI * num) * 28.0 * (flag ? 0.12 : 1.0);
			if (kind == "serpentine")
			{
				num3 += Math.Sin(num * 15.0 + phase) * Math.Sin(Math.PI * num) * width * 0.32 * 0.38;
			}
			list.Add(new Vector2((float)num2, (float)num3));
		}
		Vector3[] array = (Vector3[])(object)new Vector3[322];
		Vector2[] array2 = (Vector2[])(object)new Vector2[array.Length];
		double num4 = 0.0;
		for (int j = 0; j <= 160; j++)
		{
			Vector2 val = list[Math.Max(0, j - 1)];
			Vector2 val2 = list[Math.Min(160, j + 1)];
			double num5 = val2.X - val.X;
			double num6 = (double)(val2.Y - val.Y) / 0.38;
			double num7 = Math.Max(1E-10, Math.Sqrt(num5 * num5 + num6 * num6));
			if (j > 0)
			{
				double num8 = list[j].X - list[j - 1].X;
				double num9 = (double)(list[j].Y - list[j - 1].Y) / 0.38;
				num4 += Math.Sqrt(num8 * num8 + num9 * num9);
			}
			for (int k = 0; k < 2; k++)
			{
				double num10 = ((double)k - 0.5) * width * 1.7;
				int num11 = j * 2 + k;
				array[num11] = new Vector3((float)((double)list[j].X - num6 / num7 * num10), (float)(0.0 - ((double)list[j].Y + num5 / num7 * num10 * 0.38)), 0f);
				array2[num11] = new Vector2((float)j / 160f, (float)k);
			}
		}
		return new Residue(kind, width, phase, num4, pitch, array, array2, SteelGeometry.Indices(array.Length));
	}
}
internal sealed class TrackingVisual : IDisposable
{
	private static readonly List<TrackingVisual> Active = new List<TrackingVisual>();

	private static readonly Dictionary<string, ImageTexture> Textures = new Dictionary<string, ImageTexture>();

	private static readonly Dictionary<string, Shader> Shaders = new Dictionary<string, Shader>();

	private readonly SceneTree _tree;

	private readonly NCreature _actor;

	private readonly object? _combat;

	private readonly Node2D _root = new Node2D
	{
		Name = StringName.op_Implicit("MoreSilentTracking")
	};

	private readonly List<(TrackingGeometry.Print Print, ShaderMaterial Material)> _prints = new List<(TrackingGeometry.Print, ShaderMaterial)>();

	private readonly List<ShaderMaterial> _residue = new List<ShaderMaterial>();

	private bool _disposed;

	internal readonly List<string> Kinds = new List<string>();

	internal readonly Vector2 Start;

	internal static IReadOnlyList<TrackingVisual> Instances => Active;

	internal double Age { get; private set; }

	internal int PrintCount => _prints.Count;

	internal int ResidueCount => _residue.Count;

	internal static void Reset()
	{
		TrackingVisual[] array = Active.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			VisualCleanup.Safely(array[i].Dispose);
		}
	}

	internal static Shader[] Warm()
	{
		foreach (JsonElement item in TrackingGeometry.Data.RootElement.GetProperty("assets").EnumerateArray())
		{
			Texture(item);
		}
		return (Shader[])(object)new Shader[2]
		{
			Shader("footprint"),
			Shader("residue")
		};
	}

	internal static Creature[] VisualTargets(IEnumerable<Creature> candidates)
	{
		Creature[] array = candidates.Where((Creature c) => c.IsAlive && c.GetCreatureNode() != null).ToArray();
		Creature[] segments = (from c in array.Where(delegate(Creature c)
			{
				MonsterModel monster = c.Monster;
				return (monster is DecimillipedeSegmentFront || monster is DecimillipedeSegmentMiddle || monster is DecimillipedeSegmentBack) ? true : false;
			})
			orderby ((Control)c.GetCreatureNode()).GlobalPosition.X
			select c).ToArray();
		if (segments.Length >= 2)
		{
			return array.Where((Creature c) => !segments.Contains(c) || c == segments[0]).ToArray();
		}
		return array;
	}

	private static Stream Resource(string name)
	{
		return typeof(TrackingVisual).Assembly.GetManifestResourceStream("MoreSilentAnimations.assets." + name) ?? throw new InvalidDataException("Missing tracking recipe: " + name);
	}

	private static Shader Shader(string kind)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_004c: Expected O, but got Unknown
		if (Shaders.TryGetValue(kind, out Shader value))
		{
			return value;
		}
		using Stream stream = Resource("tracking-" + kind + ".gdshader");
		using StreamReader streamReader = new StreamReader(stream);
		Dictionary<string, Shader> shaders = Shaders;
		Shader val = new Shader
		{
			Code = streamReader.ReadToEnd()
		};
		Shader result = val;
		shaders[kind] = val;
		return result;
	}

	private static ImageTexture Texture(JsonElement asset)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		string @string = asset.GetProperty("file").GetString();
		if (Textures.TryGetValue(@string, out ImageTexture value))
		{
			return value;
		}
		using Stream stream = Resource("tracking." + @string);
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		Image val = new Image();
		try
		{
			if ((int)val.LoadPngFromBuffer(memoryStream.ToArray()) != 0)
			{
				throw new InvalidDataException("Invalid authored footprint");
			}
			return Textures[@string] = ImageTexture.CreateFromImage(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal static JsonElement? Recipe(string version, Creature target)
	{
		MonsterModel monster = target.Monster;
		if (monster == null)
		{
			return null;
		}
		string type = ((object)monster).GetType().Name;
		ToughEgg val = (ToughEgg)(object)((monster is ToughEgg) ? monster : null);
		string text = ((val != null && val.IsHatched) ? "hatched" : "base");
		foreach (JsonElement item in TrackingGeometry.Data.RootElement.GetProperty("versions").GetProperty(version).EnumerateArray())
		{
			if (item.GetProperty("form").GetString() == text && item.GetProperty("models").EnumerateArray().Any((JsonElement m) => m.GetString() == type))
			{
				return item;
			}
		}
		return null;
	}

	internal TrackingVisual(NCreature actor, string version, IEnumerable<Creature> targets)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		_actor = actor;
		_tree = ((Node)actor).GetTree();
		_combat = actor.Entity.CombatState;
		Node2D val = (Node2D)((Node)actor.Body).GetParent();
		Vector2 val2 = (ActorPresentation.For(actor)?.DisplayWorldOrigin ?? actor.Body.GlobalPosition) - actor.Body.GlobalPosition;
		Vector2 val3 = val.ToGlobal(new Vector2(55f, 20f)) + val2;
		Start = new Vector2(val3.X, 0f - val3.Y);
		try
		{
			((Node)((Node)(NCombatRoom.Instance ?? throw new InvalidOperationException("Tracking outside combat"))).GetNode<CanvasItem>(NodePath.op_Implicit("CombatSceneContainer/BackCombatVfxContainer"))).AddChild((Node)(object)_root, false, (InternalMode)0);
			_root.GlobalTransform = Transform2D.Identity;
			Creature[] array = VisualTargets(targets);
			foreach (Creature val4 in array)
			{
				JsonElement? jsonElement = Recipe(version, val4);
				if (!jsonElement.HasValue)
				{
					continue;
				}
				JsonElement valueOrDefault = jsonElement.GetValueOrDefault();
				NCreature creatureNode = val4.GetCreatureNode();
				string @string = valueOrDefault.GetProperty("id").GetString();
				Transform2D globalTransform;
				double num;
				if (!(@string == "decimillipede"))
				{
					globalTransform = creatureNode.Body.GlobalTransform;
					num = (double)((Vector2)(ref globalTransform.X)).Length() / TrackingGeometry.Number(valueOrDefault, "scale", 1.0);
				}
				else
				{
					globalTransform = ((Node2D)((Node)creatureNode.Body).GetParent()).GlobalTransform;
					num = ((Vector2)(ref globalTransform.X)).Length();
				}
				double num2 = num;
				if (valueOrDefault.GetProperty("residue").ValueKind != JsonValueKind.Null)
				{
					JsonElement property = valueOrDefault.GetProperty("residue");
					string string2 = property.GetProperty("kind").GetString();
					Vector2 val5 = ((Node2D)((Node)creatureNode.Body).GetParent()).ToGlobal(new Vector2((float)TrackingGeometry.Number(property, "endX"), (float)(0.0 - TrackingGeometry.Number(property, "endY"))));
					TrackingGeometry.Residue r = TrackingGeometry.Trace(string2, TrackingGeometry.Number(property, "width") * num2, TrackingGeometry.Number(property, "phase"), TrackingGeometry.Number(property, "pitch") * num2, Start, new Vector2(val5.X, 0f - val5.Y));
					AddResidue(r);
					Kinds.Add(@string + ":" + string2);
					continue;
				}
				List<TrackingGeometry.Track> list = new List<TrackingGeometry.Track>();
				foreach (JsonElement item in valueOrDefault.GetProperty("tracks").EnumerateArray())
				{
					Vector2 val6 = creatureNode.Body.ToGlobal(new Vector2((float)TrackingGeometry.Number(item, "endX"), (float)(0.0 - TrackingGeometry.Number(item, "endY"))));
					TrackingGeometry.Track track = TrackingGeometry.ReadTrack(item, Start, new Vector2(val6.X, 0f - val6.Y));
					list.Add(track with
					{
						Length = track.Length * num2,
						MinLength = track.MinLength * num2,
						Stride = track.Stride * num2,
						HalfWidth = track.HalfWidth * num2,
						SupportSpan = track.SupportSpan * num2
					});
				}
				foreach (TrackingGeometry.Print item2 in TrackingGeometry.Build(@string, list))
				{
					AddPrint(item2);
				}
				Kinds.Add(@string + ":footprints");
			}
			Active.Add(this);
			((Node)actor).TreeExiting += Dispose;
			_tree.ProcessFrame += Frame;
			Update();
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	private static ArrayMesh Mesh(Vector3[] points, Vector2[] uv, int[] indices)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		ArrayMesh val = new ArrayMesh();
		Array val2 = new Array();
		try
		{
			val2.Resize(13);
			val2[0] = Variant.op_Implicit(points);
			val2[4] = Variant.op_Implicit(uv);
			val2[12] = Variant.op_Implicit(indices);
			val.AddSurfaceFromArrays((PrimitiveType)3, val2, (Array<Array>)null, (Dictionary)null, (ArrayFormat)0);
			return val;
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void AddPrint(TrackingGeometry.Print p)
	{
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Expected O, but got Unknown
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Expected O, but got Unknown
		JsonElement jsonElement = TrackingGeometry.Data.RootElement.GetProperty("assets").EnumerateArray().First((JsonElement a) => a.GetProperty("id").GetString() == p.Asset);
		JsonElement property = jsonElement.GetProperty("alphaBounds");
		double num = property[2].GetDouble() - property[0].GetDouble();
		double num2 = property[3].GetDouble() - property[1].GetDouble();
		double num3 = TrackingGeometry.Number(jsonElement, "displayAspect", num / num2);
		double num4 = p.Length * 1.32;
		double num5 = num4 * num3;
		double num6 = Math.Cos(p.Angle);
		double num7 = Math.Sin(p.Angle);
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		Vector2[] array2 = (Vector2[])(object)new Vector2[4];
		for (int i = 0; i < 4; i++)
		{
			int num8 = i % 2;
			int num9 = ((i >= 2) ? 1 : 0);
			double num10 = (0.5 - (double)num9) * num4;
			double num11 = ((double)num8 - 0.5) * num5 * p.Side;
			array[i] = new Vector3((float)(p.X + num6 * num10 - num7 * num11), (float)(0.0 - (p.Y + (num7 * num10 + num6 * num11) * 0.38)), 0f);
			array2[i] = new Vector2((float)num8, (float)num9);
		}
		ShaderMaterial val = new ShaderMaterial
		{
			Shader = Shader("footprint")
		};
		val.SetShaderParameter(StringName.op_Implicit("bounds"), Variant.op_Implicit(new Vector4((float)(property[0].GetDouble() / jsonElement.GetProperty("width").GetDouble()), (float)(property[1].GetDouble() / jsonElement.GetProperty("height").GetDouble()), (float)(num / jsonElement.GetProperty("width").GetDouble()), (float)(num2 / jsonElement.GetProperty("height").GetDouble()))));
		double num12 = (new string[6] { "slender", "bird", "fossil", "root", "skeletal", "mechanical" }.Any(p.Asset.Contains) ? 0.28 : ((p.Asset.Contains("arthropod") || p.Asset.Contains("hopper")) ? 0.5 : 1.0));
		val.SetShaderParameter(StringName.op_Implicit("glow"), Variant.op_Implicit(1.2f));
		val.SetShaderParameter(StringName.op_Implicit("depth"), Variant.op_Implicit((float)(1.2 * num12)));
		val.SetShaderParameter(StringName.op_Implicit("insetShift"), Variant.op_Implicit(new Vector2((float)(-0.105 * num6 / p.Side), (float)(0.105 * num7 * num3))));
		ImageTexture val2 = Texture(jsonElement);
		val.SetShaderParameter(StringName.op_Implicit("footprintTexture"), Variant.op_Implicit((GodotObject)(object)val2));
		Node2D root = _root;
		MeshInstance2D val3 = new MeshInstance2D();
		val3.Mesh = (Mesh)(object)Mesh(array, array2, new int[6] { 0, 1, 2, 2, 1, 3 });
		val3.Texture = (Texture2D)(object)val2;
		((CanvasItem)val3).Material = (Material)(object)val;
		((CanvasItem)val3).TextureFilter = (TextureFilterEnum)2;
		((Node)root).AddChild((Node)(object)val3, false, (InternalMode)0);
		_prints.Add((p, val));
	}

	private void AddResidue(TrackingGeometry.Residue r)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Expected O, but got Unknown
		ShaderMaterial val = new ShaderMaterial
		{
			Shader = Shader("residue")
		};
		val.SetShaderParameter(StringName.op_Implicit("timing"), Variant.op_Implicit(new Vector4(0.03f, 0.72f, 0.1f, 1.1f)));
		val.SetShaderParameter(StringName.op_Implicit("dissolve"), Variant.op_Implicit(new Vector2(0.72f, 0.12f)));
		val.SetShaderParameter(StringName.op_Implicit("fadeExponent"), Variant.op_Implicit(0.5f));
		val.SetShaderParameter(StringName.op_Implicit("kind"), Variant.op_Implicit((float)Array.IndexOf(new string[7] { "water", "slime", "serpentine", "scrape", "spore", "mist", "settle" }, r.Kind)));
		val.SetShaderParameter(StringName.op_Implicit("farFade"), Variant.op_Implicit(0.07f));
		val.SetShaderParameter(StringName.op_Implicit("glow"), Variant.op_Implicit(1.2f));
		val.SetShaderParameter(StringName.op_Implicit("depth"), Variant.op_Implicit(1.2f));
		val.SetShaderParameter(StringName.op_Implicit("full"), Variant.op_Implicit(0f));
		val.SetShaderParameter(StringName.op_Implicit("cycles"), Variant.op_Implicit((float)(r.Distance / r.Pitch)));
		val.SetShaderParameter(StringName.op_Implicit("seed"), Variant.op_Implicit((float)r.Phase));
		((Node)_root).AddChild((Node)new MeshInstance2D
		{
			Mesh = (Mesh)(object)Mesh(r.Points, r.UV, r.Indices),
			Material = (Material)(object)val
		}, false, (InternalMode)0);
		_residue.Add(val);
	}

	private void Update()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		foreach (var (p, val) in _prints)
		{
			val.SetShaderParameter(StringName.op_Implicit("opacity"), Variant.op_Implicit((float)TrackingGeometry.Alpha(p, Age)));
			val.SetShaderParameter(StringName.op_Implicit("elapsed"), Variant.op_Implicit((float)Age));
		}
		foreach (ShaderMaterial item in _residue)
		{
			item.SetShaderParameter(StringName.op_Implicit("elapsed"), Variant.op_Implicit((float)Age));
		}
	}

	private void Frame()
	{
		if (_disposed)
		{
			return;
		}
		try
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)_actor) || !((Node)_actor).IsInsideTree() || _combat != _actor.Entity.CombatState)
			{
				Dispose();
			}
			else if (!_tree.Paused && ((Node)_actor).CanProcess())
			{
				Age += ((Node)_actor).GetProcessDeltaTime();
				if (Age >= 2.0)
				{
					Dispose();
				}
				else
				{
					Update();
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			VisualCleanup.Safely(Dispose);
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			Active.Remove(this);
			if (GodotObject.IsInstanceValid((GodotObject)(object)_tree))
			{
				_tree.ProcessFrame -= Frame;
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)_actor))
			{
				((Node)_actor).TreeExiting -= Dispose;
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)_root))
			{
				((CanvasItem)_root).Hide();
				((Node)_root).QueueFree();
			}
		}
	}
}
internal static class VisualCleanup
{
	internal static void Safely(Action cleanup)
	{
		try
		{
			cleanup();
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	internal static void Reset()
	{
		Safely(delegate
		{
			ComboPlanning.Release(cancel: true);
		});
		PresentationPreloader.Cancel();
		Safely(SnakeVisual.Reset);
		Safely(SteelWind.Reset);
		Safely(TrackingVisual.Reset);
		Safely(ActorPresentation.Reset);
		Safely(NativeEchoPool.Reset);
		Safely(BottleFlight.Reset);
		Safely(ShivVisual.Reset);
		Safely(KnifeTrapVisual.Reset);
		Safely(ReturnKnifeVisual.Reset);
	}
}
[HarmonyPatch(typeof(EchoingSlash), "OnPlay")]
internal static class EchoingSlashPresentationPatch
{
	internal sealed class Invocation(EchoingSlash card, Invocation? previous)
	{
		internal readonly EchoingSlash Card = card;

		internal readonly Invocation? Previous = previous;

		private Creature? _target;

		private (ActorPresentation Session, long Generation)? _shot;

		internal void Round(IEnumerable<Creature>? targets)
		{
			if (!AnimationSettings.Allows((AbstractModel?)(object)card) || !((CardModel)card).Owner.Creature.IsAlive)
			{
				return;
			}
			Creature val = targets?.FirstOrDefault((Func<Creature, bool>)((Creature c) => c.IsAlive && c.GetCreatureNode() != null));
			if (val == null)
			{
				return;
			}
			(ActorPresentation, long)? shot;
			if (val == _target)
			{
				shot = _shot;
				if (shot.HasValue)
				{
					(ActorPresentation, long) valueOrDefault = shot.GetValueOrDefault();
					if (valueOrDefault.Item1.ContinueEchoingSlash(valueOrDefault.Item2))
					{
						return;
					}
				}
			}
			_target = val;
			_shot = ActorPresentation.Begin(((CardModel)card).Owner.Creature, val, new ThrustProfile("EchoingSlash", Punch: false, DownStab: false, Normal: false, NativeAttack: true), 0.0);
			shot = _shot;
			if (shot.HasValue)
			{
				(ActorPresentation, long) valueOrDefault2 = shot.GetValueOrDefault();
				valueOrDefault2.Item1.Hit(valueOrDefault2.Item2);
			}
		}

		internal async Task Observe(Task original)
		{
			try
			{
				await original;
			}
			catch
			{
			}
			finally
			{
				try
				{
					(ActorPresentation, long)? shot = _shot;
					if (shot.HasValue)
					{
						(ActorPresentation, long) valueOrDefault = shot.GetValueOrDefault();
						valueOrDefault.Item1.EndEchoingSlash(valueOrDefault.Item2);
					}
				}
				catch (Exception error)
				{
					ModEntry.Log(error);
				}
				_shot = null;
				_target = null;
			}
		}
	}

	internal static readonly AsyncLocal<Invocation?> Current = new AsyncLocal<Invocation>();

	[HarmonyPrefix]
	private static void Prefix(EchoingSlash __instance, out Invocation? __state)
	{
		__state = null;
		if (AnimationSettings.Allows((AbstractModel?)(object)__instance) && ((object)((CardModel)__instance).Owner.Character).GetType().Name == "Silent")
		{
			Current.Value = (__state = new Invocation(__instance, Current.Value));
		}
	}

	[HarmonyPostfix]
	private static void Postfix(Task __result, Invocation? __state)
	{
		if (__state != null)
		{
			Current.Value = __state.previous;
			__state.Observe(__result);
		}
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, Invocation? __state)
	{
		if (__state != null && Current.Value == __state)
		{
			Current.Value = __state.previous;
		}
		return __exception;
	}
}
[HarmonyPatch]
internal static class EchoingSlashRoundPatch
{
	private static readonly AsyncLocal<bool> InsideDamage = new AsyncLocal<bool>();

	internal static MethodBase TargetMethod()
	{
		return typeof(CreatureCmd).GetMethods().Single((MethodInfo m) => m.Name == "Damage" && m.GetParameters().Any((ParameterInfo p) => p.Name == "targets" && p.ParameterType == typeof(IEnumerable<Creature>)) && m.GetParameters().Any((ParameterInfo p) => p.ParameterType == typeof(DamageVar)) && m.GetParameters().Any((ParameterInfo p) => p.Name == "cardSource"));
	}

	[HarmonyPrefix]
	private static void Prefix(IEnumerable<Creature>? targets, CardModel? cardSource, out bool __state)
	{
		__state = false;
		if (InsideDamage.Value)
		{
			return;
		}
		EchoingSlashPresentationPatch.Invocation value = EchoingSlashPresentationPatch.Current.Value;
		if (value == null || cardSource != value.card)
		{
			return;
		}
		InsideDamage.Value = (__state = true);
		try
		{
			value.Round(targets);
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyPostfix]
	private static void Postfix(bool __state)
	{
		if (__state)
		{
			InsideDamage.Value = false;
		}
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, bool __state)
	{
		if (__state)
		{
			InsideDamage.Value = false;
		}
		return __exception;
	}
}
[HarmonyPatch(typeof(GrandFinale), "OnPlay")]
internal static class FinalePresentationPatch
{
	internal sealed class Invocation(GrandFinale card, Invocation? previous)
	{
		internal readonly GrandFinale Card = card;

		internal readonly Invocation? Previous = previous;

		internal bool EffectsOnly;

		internal bool BodyStarted;

		internal Vector2 SlashOrigin;

		internal int SlashFacing = 1;
	}

	internal static readonly AsyncLocal<Invocation?> Current = new AsyncLocal<Invocation>();

	internal static bool Suppress(NCreature actor, string trigger)
	{
		if (trigger == "Attack")
		{
			Invocation value = Current.Value;
			if (value != null && ((CardModel)value.card).Owner.Creature == actor.Entity)
			{
				if (!value.EffectsOnly)
				{
					return value.BodyStarted;
				}
				return true;
			}
		}
		return false;
	}

	[HarmonyPrefix]
	private static void Prefix(GrandFinale __instance, out Invocation? __state)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		__state = null;
		try
		{
			if (!AnimationSettings.Allows((AbstractModel?)(object)__instance) || ((object)((CardModel)__instance).Owner.Character).GetType().Name != "Silent")
			{
				return;
			}
			NCreature creatureNode = ((CardModel)__instance).Owner.Creature.GetCreatureNode();
			if (creatureNode != null)
			{
				Current.Value = (__state = new Invocation(__instance, Current.Value));
				Invocation? obj = __state;
				FastModeType fastMode = SaveManager.Instance.PrefsSave.FastMode;
				obj.BodyStarted = ActorPresentation.BeginFinale(creatureNode, ((object)(FastModeType)(ref fastMode)/*cast due to .constrained prefix*/).ToString().ToLowerInvariant());
				if (__state.BodyStarted)
				{
					ActorPresentation actorPresentation = ActorPresentation.For(creatureNode);
					__state.SlashOrigin = actorPresentation.FinaleSlashOrigin;
					__state.SlashFacing = actorPresentation.FinaleSlashFacing;
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyPostfix]
	private static void Postfix(Invocation? __state)
	{
		if (__state != null)
		{
			Current.Value = __state.previous;
		}
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, Invocation? __state)
	{
		if (__state != null && Current.Value == __state)
		{
			Current.Value = __state.previous;
		}
		return __exception;
	}
}
[HarmonyPatch(typeof(NGrandFinaleVfx), "Create", new Type[] { typeof(Creature) })]
internal static class FinaleFactoryPatch
{
	internal static readonly ConditionalWeakTable<NGrandFinaleVfx, FinalePresentationPatch.Invocation> Owned = new ConditionalWeakTable<NGrandFinaleVfx, FinalePresentationPatch.Invocation>();

	[HarmonyPostfix]
	private static void Postfix(Creature creature, NGrandFinaleVfx? __result)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (__result == null)
		{
			return;
		}
		FinalePresentationPatch.Invocation value = FinalePresentationPatch.Current.Value;
		if (value == null || creature != ((CardModel)value.card).Owner.Creature)
		{
			return;
		}
		Owned.Add(__result, value);
		if (value.EffectsOnly)
		{
			((CanvasItem)__result).Visible = false;
		}
		if (value.BodyStarted)
		{
			Node2D nodeOrNull = ((Node)__result).GetNodeOrNull<Node2D>(NodePath.op_Implicit("slash_container"));
			if (nodeOrNull != null)
			{
				nodeOrNull.GlobalPosition = value.SlashOrigin;
				nodeOrNull.Scale = new Vector2(Math.Abs(nodeOrNull.Scale.X) * (float)value.SlashFacing, 0f - nodeOrNull.Scale.Y);
			}
		}
	}
}
[HarmonyPatch(typeof(AttackCommand), "Execute")]
internal static class AttackPresentationPatch
{
	private sealed class Invocation(AttackCommand command, Invocation? previous)
	{
		internal readonly AttackCommand Command = command;

		internal readonly Invocation? Previous = previous;

		internal readonly List<(ActorPresentation Session, long Generation)> Started = new List<(ActorPresentation, long)>();

		internal (ActorPresentation Session, long Generation)? Shot;

		internal async Task Observe(Task original)
		{
			try
			{
				await original;
			}
			catch
			{
			}
			finally
			{
				foreach (var item in Started)
				{
					try
					{
						if (command.ModelSource is Finisher)
						{
							item.Session.EndFinisher(item.Generation);
						}
						else
						{
							item.Session.CancelUnhit(item.Generation);
						}
					}
					catch (Exception error)
					{
						ModEntry.Log(error);
					}
				}
				Started.Clear();
				Shot = null;
			}
		}
	}

	private sealed class Execution(AttackCommand command)
	{
		internal Node2D? Begin()
		{
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Invocation value = Current.Value;
				if (value == null || value.command != command)
				{
					return null;
				}
				Creature attacker = command.Attacker;
				if (attacker == null)
				{
					goto IL_007f;
				}
				object? obj = Target?.GetValue(command);
				Creature val = (Creature)((obj is Creature) ? obj : null);
				if (val == null || !(Delay?.GetValue(command) is float num))
				{
					goto IL_007f;
				}
				FastModeType fastMode = SaveManager.Instance.PrefsSave.FastMode;
				string text = ((object)(FastModeType)(ref fastMode)/*cast due to .constrained prefix*/).ToString();
				double lead = ((text == "Instant") ? 0.0 : ((text == "Normal") ? ((double)num) : Math.Min((double)num * 0.5, 0.25)));
				(ActorPresentation, long)? shot;
				if (command.ModelSource is Finisher)
				{
					shot = value.Shot;
					if (shot.HasValue)
					{
						(ActorPresentation, long) valueOrDefault = shot.GetValueOrDefault();
						if (valueOrDefault.Item1.ContinueFinisher(valueOrDefault.Item2, lead))
						{
							return null;
						}
					}
				}
				shot = value.Shot;
				if (shot.HasValue)
				{
					(ActorPresentation, long) valueOrDefault2 = shot.GetValueOrDefault();
					valueOrDefault2.Item1.CancelUnhit(valueOrDefault2.Item2);
				}
				value.Shot = null;
				ThrustProfile thrustProfile = ThrustProfile.For(command.ModelSource);
				if ((object)thrustProfile == null)
				{
					return null;
				}
				value.Shot = ActorPresentation.Begin(attacker, val, thrustProfile with
				{
					Normal = (text == "Normal")
				}, lead);
				shot = value.Shot;
				if (shot.HasValue)
				{
					(ActorPresentation, long) valueOrDefault3 = shot.GetValueOrDefault();
					value.Started.Add(valueOrDefault3);
				}
				goto end_IL_0000;
				IL_007f:
				return null;
				end_IL_0000:;
			}
			catch (Exception error)
			{
				ModEntry.Log(error);
			}
			return null;
		}

		internal Node2D? Hit(Creature target)
		{
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Invocation value = Current.Value;
				if (value != null && value.command == command && Target?.GetValue(command) == target)
				{
					(ActorPresentation, long)? shot = value.Shot;
					if (shot.HasValue)
					{
						(ActorPresentation, long) valueOrDefault = shot.GetValueOrDefault();
						valueOrDefault.Item1.Hit(valueOrDefault.Item2);
					}
					if (command.ModelSource is PoisonedStab)
					{
						return (Node2D?)(object)NGaseousImpactVfx.Create(target, new Color("83eb85"));
					}
				}
			}
			catch (Exception error)
			{
				ModEntry.Log(error);
			}
			return null;
		}
	}

	private static readonly FieldInfo? Target = AccessTools.Field(typeof(AttackCommand), "_singleTarget");

	private static readonly FieldInfo? Delay = AccessTools.Field(typeof(AttackCommand), "_attackerAnimDelay");

	private static readonly ConditionalWeakTable<AttackCommand, Execution> Executions = new ConditionalWeakTable<AttackCommand, Execution>();

	private static readonly AsyncLocal<Invocation?> Current = new AsyncLocal<Invocation>();

	internal static bool WhiteSlash
	{
		get
		{
			Invocation value = Current.Value;
			if (value != null)
			{
				(ActorPresentation, long)? shot = value.Shot;
				if (shot.HasValue)
				{
					AttackCommand command = value.command;
					if (command != null)
					{
						AbstractModel modelSource = command.ModelSource;
						if (modelSource is Predator || modelSource is Pounce || modelSource is Murder || modelSource is TheHunt)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}

	internal static ActorPresentation? Presentation => Current.Value?.Shot?.Session;

	[HarmonyPrefix]
	private static void Prefix(AttackCommand __instance, out Invocation? __state)
	{
		__state = null;
		try
		{
			if ((object)ThrustProfile.For(__instance.ModelSource) != null && (object)Target != null && (object)Delay != null)
			{
				if (!Executions.TryGetValue(__instance, out Execution _))
				{
					Execution execution = new Execution(__instance);
					__instance.WithAttackerFx((Func<Node2D>)execution.Begin);
					__instance.WithHitVfxNode((Func<Creature, Node2D>)execution.Hit);
					Executions.Add(__instance, execution);
				}
				Current.Value = (__state = new Invocation(__instance, Current.Value));
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyPostfix]
	private static void Postfix(Task __result, Invocation? __state)
	{
		if (__state == null)
		{
			return;
		}
		Current.Value = __state.previous;
		try
		{
			__state.Observe(__result);
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, Invocation? __state)
	{
		if (__state != null && Current.Value == __state)
		{
			Current.Value = __state.previous;
		}
		return __exception;
	}
}
[HarmonyPatch(typeof(NCreature), "SetAnimationTrigger")]
internal static class NativeActionPatch
{
	[HarmonyPrefix]
	private static bool Prefix(NCreature __instance, string trigger)
	{
		return !FinalePresentationPatch.Suppress(__instance, trigger);
	}

	[HarmonyPostfix]
	private static void Postfix(NCreature __instance, string trigger)
	{
		try
		{
			if (!FinalePresentationPatch.Suppress(__instance, trigger) && !ShivPresentationPatch.Trigger(__instance, trigger))
			{
				ActorPresentation.For(__instance)?.Trigger(trigger);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			VisualCleanup.Safely(delegate
			{
				ActorPresentation.For(__instance)?.Dispose();
			});
		}
	}
}
[HarmonyPatch(typeof(NCreature), "StartDeathAnim")]
internal static class NativeDeathPresentationPatch
{
	[HarmonyPostfix]
	private static void Postfix(NCreature __instance)
	{
		try
		{
			ActorPresentation.For(__instance)?.Trigger("Dead");
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			VisualCleanup.Safely(delegate
			{
				ActorPresentation.For(__instance)?.Dispose();
			});
		}
	}
}
[HarmonyPatch(typeof(RunManager), "CleanUp")]
internal static class RunCleanupPatch
{
	[HarmonyPrefix]
	private static void Prefix()
	{
		VisualCleanup.Reset();
	}
}
[HarmonyPatch(typeof(NCreature), "_Ready")]
internal static class CreaturePreloadPatch
{
	[HarmonyPostfix]
	private static void Postfix(NCreature __instance)
	{
		try
		{
			SilentFxCompatibility.InstallIfAvailable();
			Player player = __instance.Entity.Player;
			if (((player != null) ? ((object)player.Character).GetType().Name : null) == "Silent")
			{
				PresentationPreloader.Schedule(__instance);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch(typeof(KnifeTrap), "OnPlay")]
internal static class KnifeTrapPresentationPatch
{
	internal sealed class Invocation(KnifeTrap card, Invocation? previous)
	{
		internal readonly KnifeTrap Card = card;

		internal readonly Invocation? Previous = previous;

		internal readonly int Count = PileTypeExtensions.GetPile((PileType)4, ((CardModel)card).Owner).Cards.Count((CardModel c) => c.Tags.Contains((CardTag)5));

		private int _index;

		private Creature? _lastVisualTarget;

		internal KnifeTrapGeometry.Volley Next()
		{
			return new KnifeTrapGeometry.Volley(_index++, Count);
		}

		internal Creature Pick(Creature[] targets, Creature fallback)
		{
			Creature[] array = targets.Where((Creature t) => t.IsAlive && t != _lastVisualTarget).ToArray();
			if (array.Length == 0)
			{
				array = targets.Where((Creature t) => t.IsAlive).ToArray();
			}
			return _lastVisualTarget = (Creature?)((array.Length != 0) ? ((object)array[Random.Shared.Next(array.Length)]) : ((object)fallback));
		}
	}

	internal static readonly AsyncLocal<Invocation?> Current = new AsyncLocal<Invocation>();

	[HarmonyPrefix]
	private static void Prefix(KnifeTrap __instance, out Invocation? __state)
	{
		__state = null;
		try
		{
			if (AnimationSettings.Allows((AbstractModel?)(object)__instance) && ((object)((CardModel)__instance).Owner.Character).GetType().Name == "Silent")
			{
				Current.Value = (__state = new Invocation(__instance, Current.Value));
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyPostfix]
	private static void Postfix(Invocation? __state)
	{
		if (__state != null)
		{
			Current.Value = __state.previous;
		}
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, Invocation? __state)
	{
		if (__state != null && Current.Value == __state)
		{
			Current.Value = __state.previous;
		}
		return __exception;
	}
}
[HarmonyPatch(typeof(AttackCommand), "WithHitVfxNode")]
internal static class KnifeTrapHitTargetPatch
{
	internal static readonly AsyncLocal<Creature?> Target = new AsyncLocal<Creature>();

	private static readonly MethodInfo PossibleTargets = AccessTools.Method(typeof(AttackCommand), "GetPossibleTargets", (Type[])null, (Type[])null);

	[HarmonyPrefix]
	private static void Prefix(AttackCommand __instance, ref Func<Creature, Node2D?> createHitVfxNode)
	{
		try
		{
			KnifeTrapPresentationPatch.Invocation trap = KnifeTrapPresentationPatch.Current.Value;
			if (trap == null)
			{
				return;
			}
			AbstractModel modelSource = __instance.ModelSource;
			Shiv val = (Shiv)(object)((modelSource is Shiv) ? modelSource : null);
			if (val == null || ((CardModel)trap.card).Owner != ((CardModel)val).Owner)
			{
				return;
			}
			Func<Creature, Node2D?> original = createHitVfxNode;
			createHitVfxNode = delegate(Creature target)
			{
				Creature value = Target.Value;
				Target.Value = target;
				try
				{
					try
					{
						ShivPresentationPatch.Invocation value2 = ShivPresentationPatch.Current.Value;
						if (value2 != null && value2.Trap != null && value2.TrapTarget == null)
						{
							IEnumerable<Creature> enumerable = PossibleTargets.Invoke(__instance, null) as IEnumerable<Creature>;
							value2.TrapTarget = trap.Pick((Creature[])(((object)enumerable?.ToArray()) ?? ((object)new Creature[1] { target })), target);
						}
					}
					catch (Exception error)
					{
						ModEntry.Log(error);
					}
					return original(target);
				}
				finally
				{
					Target.Value = value;
				}
			};
		}
		catch (Exception error2)
		{
			ModEntry.Log(error2);
		}
	}
}
[HarmonyPatch(typeof(NShivThrowVfx), "Create", new Type[]
{
	typeof(Creature),
	typeof(Creature),
	typeof(Color)
})]
internal static class KnifeTrapFactoryTargetPatch
{
	[HarmonyPrefix]
	private static void Prefix(Creature owner, ref Creature? target)
	{
		ShivPresentationPatch.Invocation value = ShivPresentationPatch.Current.Value;
		if (value != null && value.Trap != null && value.TrapTarget != null && owner == ((CardModel)value.Card).Owner.Creature)
		{
			target = value.TrapTarget;
		}
	}
}
[ModInitializer("Initialize")]
public static class ModEntry
{
	internal const string Id = "MoreSilentAnimations";

	internal const string HarmonyId = "page.more_silent_animations";

	public static void Initialize()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		Harmony val = new Harmony("page.more_silent_animations");
		try
		{
			val.PatchAll(typeof(ModEntry).Assembly);
		}
		catch (Exception error)
		{
			val.UnpatchAll("page.more_silent_animations");
			Log(error);
		}
		ComboHotkey.Install();
		SettingsLocalization.Install();
		OptionalModSettings.Install();
		SilentFxCompatibility.InstallIfAvailable();
	}

	internal static void Log(Exception error)
	{
		Console.Error.WriteLine("[MoreSilentAnimations] Visual skipped safely: " + error);
	}
}
internal static class OptionalModSettings
{
	private static bool _watching;

	private static readonly HashSet<string> Attempted = new HashSet<string>();

	internal static bool Available { get; private set; }

	internal static void Install()
	{
		if (_watching)
		{
			return;
		}
		_watching = true;
		AnimationSettings.Current = new AnimationSettings();
		ModManager.OnModDetected += OnModDetected;
		foreach (Mod mod in ModManager.Mods)
		{
			OnModDetected(mod);
		}
	}

	private static void OnModDetected(Mod mod)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		string text = mod.manifest?.id;
		bool flag = ((text == "JmcModLib" || text == "BaseLib") ? true : false);
		if (!flag || (int)mod.state != 1 || !Attempted.Add(text))
		{
			return;
		}
		AnimationSettings current = AnimationSettings.Current;
		try
		{
			Version version = ((text == "BaseLib") ? new Version(3, 4, 7) : new Version(1, 9, 0));
			List<LocString> errors = mod.errors;
			if ((errors != null && errors.Count > 0) || !Version.TryParse(mod.manifest.version?.TrimStart('v', 'V'), out Version result) || result < version)
			{
				Console.WriteLine($"[MoreSilentAnimations] Optional {text} menu needs version {version}+; retaining current animation settings.");
				return;
			}
			Assembly assembly = typeof(ModEntry).Assembly;
			if (!Available)
			{
				AnimationSettings.Load();
			}
			string assemblyPath = Path.Combine(Path.GetDirectoryName(assembly.Location), (text == "BaseLib") ? "MoreSilentAnimations.BaseLib.dll" : "MoreSilentAnimations.Settings.dll");
			AssemblyLoadContext.GetLoadContext(assembly).LoadFromAssemblyPath(assemblyPath).GetType((text == "BaseLib") ? "MoreSilentAnimations.BaseLibSettingsMenu" : "MoreSilentAnimations.ModSettingsMenu", throwOnError: true)
				.GetMethod("Install", BindingFlags.Static | BindingFlags.Public)
				.Invoke(null, null);
			Available = true;
			Console.WriteLine("[MoreSilentAnimations] Optional " + text + " menu ready: Silent animations and effects.");
		}
		catch (Exception ex)
		{
			AnimationSettings.Current = current;
			Console.Error.WriteLine("[MoreSilentAnimations] Optional " + text + " menu unavailable; retaining current animation settings: " + ex.GetBaseException().Message);
		}
	}
}
[HarmonyPatch]
internal static class PoisonPresentationPatch
{
	internal sealed class Invocation(CardModel card, Invocation? previous)
	{
		internal readonly CardModel Card = card;

		internal readonly Invocation? Previous = previous;

		internal readonly long Sequence = ++_sequence;

		internal BottleFlight? Flight;

		internal int Throws;

		internal bool CastObserved;

		internal bool CastComplete;

		private Node2D? _impact;

		private bool _impactVisible;

		private ProcessModeEnum _impactProcess;

		internal bool Quick
		{
			get
			{
				CardModel card = card;
				if (card is BubbleBubble || card is DeadlyPoison)
				{
					return true;
				}
				return false;
			}
		}

		internal void Start(CardPlay play)
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			if (!Quick)
			{
				return;
			}
			Creature target = play.Target;
			NCreature val = ((target != null) ? target.GetCreatureNode() : null);
			if (val == null)
			{
				return;
			}
			NCreature creatureNode = card.Owner.Creature.GetCreatureNode();
			if (creatureNode == null)
			{
				return;
			}
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance != null)
			{
				double num = card.Owner.Character.CastAnimDelay;
				FastModeType fastMode = SaveManager.Instance.PrefsSave.FastMode;
				string text = ((object)(FastModeType)(ref fastMode)/*cast due to .constrained prefix*/).ToString();
				num = ((text == "Instant") ? 0.0 : ((text == "Normal") ? num : Math.Min(num * 0.5, 0.25)));
				if (!(num <= 0.0))
				{
					Vector2 from = ActorPresentation.For(creatureNode)?.MapPoint(creatureNode.VfxSpawnPosition) ?? creatureNode.VfxSpawnPosition;
					Flight = new BottleFlight((Node)(object)instance.CombatVfxContainer, from, val.VfxSpawnPosition, num, Sequence * 997 + 9101, quick: true, Release);
				}
			}
		}

		internal bool Defer(Node2D node)
		{
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			if (!Quick || Flight == null || CastComplete || _impact != null || (card is BubbleBubble && !(node is NGaseousImpactVfx)) || (card is DeadlyPoison && !(node is NPoisonImpactVfx)))
			{
				return false;
			}
			_impact = node;
			_impactVisible = ((CanvasItem)node).Visible;
			_impactProcess = ((Node)node).ProcessMode;
			((CanvasItem)node).Visible = false;
			((Node)node).ProcessMode = (ProcessModeEnum)4;
			return true;
		}

		private void Release()
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Invalid comparison between Unknown and I8
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			if (CastComplete)
			{
				return;
			}
			CastComplete = true;
			Flight?.Arrive();
			Node2D impact = _impact;
			_impact = null;
			if (impact != null && GodotObject.IsInstanceValid((GodotObject)(object)impact) && ((Node)impact).IsInsideTree() && !((GodotObject)impact).IsQueuedForDeletion())
			{
				if (!((CanvasItem)impact).Visible)
				{
					((CanvasItem)impact).Visible = _impactVisible;
				}
				if ((long)((Node)impact).ProcessMode == 4)
				{
					((Node)impact).ProcessMode = _impactProcess;
				}
				((Node)impact)._Ready();
			}
		}

		internal async Task ObserveCast(Task original)
		{
			try
			{
				await original;
			}
			catch
			{
			}
			finally
			{
				try
				{
					if (Flight == null)
					{
						Release();
					}
				}
				catch (Exception error)
				{
					ModEntry.Log(error);
				}
			}
		}

		internal async Task ObservePlay(Task original)
		{
			try
			{
				await original;
			}
			catch
			{
			}
			finally
			{
				try
				{
					if (!CastComplete && (Flight == null || original.IsFaulted || original.IsCanceled))
					{
						Release();
					}
				}
				catch (Exception error)
				{
					ModEntry.Log(error);
				}
			}
		}
	}

	[CompilerGenerated]
	private sealed class <TargetMethods>d__3 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private MethodBase <>2__current;

		private int <>l__initialThreadId;

		private Type[] <>7__wrap1;

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
		public <TargetMethods>d__3(int <>1__state)
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
			switch (<>1__state)
			{
			default:
				return false;
			case 0:
				<>1__state = -1;
				<>7__wrap1 = new Type[3]
				{
					typeof(BubbleBubble),
					typeof(DeadlyPoison),
					typeof(BouncingFlask)
				};
				<>7__wrap2 = 0;
				break;
			case 1:
				<>1__state = -1;
				<>7__wrap2++;
				break;
			}
			if (<>7__wrap2 < <>7__wrap1.Length)
			{
				Type type = <>7__wrap1[<>7__wrap2];
				<>2__current = AccessTools.Method(type, "OnPlay", (Type[])null, (Type[])null);
				<>1__state = 1;
				return true;
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
			return new <TargetMethods>d__3(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	internal static readonly AsyncLocal<Invocation?> Current = new AsyncLocal<Invocation>();

	private static long _sequence;

	[IteratorStateMachine(typeof(<TargetMethods>d__3))]
	private static IEnumerable<MethodBase> TargetMethods()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <TargetMethods>d__3(-2);
	}

	[HarmonyPrefix]
	private static void Prefix(CardModel __instance, CardPlay cardPlay, out Invocation? __state)
	{
		__state = null;
		try
		{
			if (AnimationSettings.Allows((AbstractModel?)(object)__instance) && !(((object)__instance.Owner.Character).GetType().Name != "Silent") && NCombatRoom.Instance != null)
			{
				Current.Value = (__state = new Invocation(__instance, Current.Value));
				__state.Start(cardPlay);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyPostfix]
	private static void Postfix(Task __result, Invocation? __state)
	{
		if (__state == null)
		{
			return;
		}
		Current.Value = __state.previous;
		try
		{
			__state.ObservePlay(__result);
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, Invocation? __state)
	{
		if (__state != null && Current.Value == __state)
		{
			Current.Value = __state.previous;
		}
		return __exception;
	}
}
[HarmonyPatch(typeof(CreatureCmd), "TriggerAnim")]
internal static class PoisonCastCompletionPatch
{
	[HarmonyPostfix]
	private static void Postfix(Creature creature, string triggerName, Task __result)
	{
		try
		{
			PoisonPresentationPatch.Invocation value = PoisonPresentationPatch.Current.Value;
			if (value != null && value.Quick && !value.CastObserved && creature == value.card.Owner.Creature && triggerName == "Cast")
			{
				value.CastObserved = true;
				value.ObserveCast(__result);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch]
internal static class PoisonImpactStartupPatch
{
	[CompilerGenerated]
	private sealed class <TargetMethods>d__0 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private MethodBase <>2__current;

		private int <>l__initialThreadId;

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
		public <TargetMethods>d__0(int <>1__state)
		{
			this.<>1__state = <>1__state;
			<>l__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
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
				<>2__current = AccessTools.Method(typeof(NGaseousImpactVfx), "_Ready", (Type[])null, (Type[])null);
				<>1__state = 1;
				return true;
			case 1:
				<>1__state = -1;
				<>2__current = AccessTools.Method(typeof(NPoisonImpactVfx), "_Ready", (Type[])null, (Type[])null);
				<>1__state = 2;
				return true;
			case 2:
				<>1__state = -1;
				return false;
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
		IEnumerator<MethodBase> IEnumerable<MethodBase>.GetEnumerator()
		{
			if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
			{
				<>1__state = 0;
				return this;
			}
			return new <TargetMethods>d__0(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	[IteratorStateMachine(typeof(<TargetMethods>d__0))]
	private static IEnumerable<MethodBase> TargetMethods()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <TargetMethods>d__0(-2);
	}

	[HarmonyPrefix]
	private static bool Prefix(Node2D __instance)
	{
		try
		{
			return !(PoisonPresentationPatch.Current.Value?.Defer(__instance) ?? false);
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			return true;
		}
	}
}
[HarmonyPatch(typeof(NItemThrowVfx), "Create")]
internal static class NativeBottleCapturePatch
{
	internal static readonly ConditionalWeakTable<NItemThrowVfx, PoisonPresentationPatch.Invocation> Throws = new ConditionalWeakTable<NItemThrowVfx, PoisonPresentationPatch.Invocation>();

	[HarmonyPrefix]
	private static void Prefix(ref Vector2 sourcePosition, Texture2D? itemTexture, out PoisonPresentationPatch.Invocation? __state)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		__state = null;
		try
		{
			PoisonPresentationPatch.Invocation value = PoisonPresentationPatch.Current.Value;
			if (value == null || !(value.card is BouncingFlask) || itemTexture != ((PotionModel)ModelDb.Potion<PoisonPotion>()).Image)
			{
				return;
			}
			__state = value;
			if (value.Throws != 0)
			{
				return;
			}
			NCreature creatureNode = value.card.Owner.Creature.GetCreatureNode();
			if (creatureNode != null)
			{
				ActorPresentation actorPresentation = ActorPresentation.For(creatureNode);
				if (actorPresentation != null)
				{
					sourcePosition = actorPresentation.MapPoint(sourcePosition);
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyPostfix]
	private static void Postfix(NItemThrowVfx? __result, PoisonPresentationPatch.Invocation? __state)
	{
		try
		{
			if (__result != null && __state != null)
			{
				__state.Throws++;
				Throws.Add(__result, __state);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch(typeof(NItemThrowVfx), "_Ready")]
internal static class NativeBottleTrailPatch
{
	[HarmonyPostfix]
	private static void Postfix(NItemThrowVfx __instance)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!NativeBottleCapturePatch.Throws.TryGetValue(__instance, out PoisonPresentationPatch.Invocation value))
			{
				return;
			}
			Variant val = ((GodotObject)__instance).Get(StringName.op_Implicit("_sourcePosition"));
			try
			{
				Variant val2 = ((GodotObject)__instance).Get(StringName.op_Implicit("_targetPosition"));
				try
				{
					Variant val3 = ((GodotObject)__instance).Get(StringName.op_Implicit("_flightTime"));
					try
					{
						new BottleFlight(((Node)__instance).GetParent(), ((Variant)(ref val)).AsVector2(), ((Variant)(ref val2)).AsVector2(), ((Variant)(ref val3)).AsDouble(), value.Sequence * 997 + value.Throws * 619, quick: false);
					}
					finally
					{
						((IDisposable)(Variant)(ref val3)/*cast due to .constrained prefix*/).Dispose();
					}
				}
				finally
				{
					((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch(typeof(AttackCommand), "Execute")]
internal static class ReturnKnifePatch
{
	private sealed class Invocation(AttackCommand command, Invocation? previous)
	{
		internal readonly AttackCommand Command = command;

		internal readonly Invocation? Previous = previous;

		internal ActorPresentation? Session;
	}

	private static readonly ConditionalWeakTable<AttackCommand, object> Hooked = new ConditionalWeakTable<AttackCommand, object>();

	private static readonly AsyncLocal<Invocation?> Current = new AsyncLocal<Invocation>();

	internal static void Replaced(NCreature actor, ActorPresentation session)
	{
		Invocation value = Current.Value;
		if (value != null && value.command.Attacker == actor.Entity)
		{
			value.Session = session;
		}
	}

	private static Node2D? Hit(AttackCommand command, Creature target)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Invocation value = Current.Value;
			if (value != null)
			{
				ActorPresentation session = value.Session;
				if (session != null && value.command == command && AnimationSettings.Allows(command.ModelSource))
				{
					Creature attacker = command.Attacker;
					if (attacker != null && attacker.IsAlive && target.IsAlive)
					{
						NCreature creatureNode = attacker.GetCreatureNode();
						if (creatureNode != null)
						{
							NCreature creatureNode2 = target.GetCreatureNode();
							if (creatureNode2 != null)
							{
								return ReturnKnifeVisual.Create((ActorPresentation.For(creatureNode) == session) ? session.ShivOrigin(creatureNode.VfxSpawnPosition, "shiv", 0.0) : creatureNode.VfxSpawnPosition, creatureNode2.VfxSpawnPosition);
							}
						}
					}
				}
			}
			return null;
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			return null;
		}
	}

	[HarmonyPrefix]
	private static void Prefix(AttackCommand __instance, out Invocation? __state)
	{
		__state = null;
		try
		{
			AbstractModel modelSource = __instance.ModelSource;
			CardModel val = (CardModel)(object)((modelSource is CardModel) ? modelSource : null);
			if (val == null || val is Shiv || ((object)val.Owner.Character).GetType().Name != "Silent" || !AnimationSettings.Allows((AbstractModel?)(object)val) || (object)ThrustProfile.For((AbstractModel?)(object)val) != null)
			{
				return;
			}
			if (!Hooked.TryGetValue(__instance, out object _))
			{
				__instance.WithAttackerFx((Func<Node2D>)delegate
				{
					Invocation value2 = Current.Value;
					if (value2 != null && value2.command == __instance)
					{
						value2.Session = null;
					}
					return (Node2D?)null;
				});
				__instance.WithHitVfxNode((Func<Creature, Node2D>)((Creature target) => Hit(__instance, target)));
				Hooked.Add(__instance, new object());
			}
			Current.Value = (__state = new Invocation(__instance, Current.Value));
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyPostfix]
	private static void Postfix(Invocation? __state)
	{
		if (__state != null)
		{
			Current.Value = __state.previous;
		}
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, Invocation? __state)
	{
		if (__state != null && Current.Value == __state)
		{
			Current.Value = __state.previous;
		}
		return __exception;
	}
}
[HarmonyPatch(typeof(CombatHistory), "CardPlayStarted")]
internal static class ReturnRevealPatch
{
	[HarmonyPostfix]
	private static void Postfix(CardPlay cardPlay)
	{
		try
		{
			if (((object)cardPlay.Card.Owner.Character).GetType().Name == "Silent")
			{
				NCreature creatureNode = cardPlay.Card.Owner.Creature.GetCreatureNode();
				if (creatureNode != null)
				{
					ActorPresentation.For(creatureNode)?.RevealCard();
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch(typeof(LocManager), "Initialize")]
internal static class SettingsLocalization
{
	[CompilerGenerated]
	private static class <>O
	{
		public static LocaleChangeCallback <0>__Refresh;
	}

	internal const string Table = "settings_ui";

	private static LocManager? _subscribed;

	internal static string TitleKey(string key)
	{
		return "MORESILENTANIMATIONS." + key + ".title";
	}

	internal static string DescriptionKey(string key)
	{
		return "MORESILENTANIMATIONS." + key + ".description";
	}

	[HarmonyPostfix]
	internal static void Install()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		try
		{
			LocManager instance = LocManager.Instance;
			if (instance == null)
			{
				return;
			}
			if (instance != _subscribed)
			{
				LocManager? subscribed = _subscribed;
				if (subscribed != null)
				{
					object obj = <>O.<0>__Refresh;
					if (obj == null)
					{
						LocaleChangeCallback val = Refresh;
						<>O.<0>__Refresh = val;
						obj = (object)val;
					}
					subscribed.UnsubscribeToLocaleChange((LocaleChangeCallback)obj);
				}
				object obj2 = <>O.<0>__Refresh;
				if (obj2 == null)
				{
					LocaleChangeCallback val2 = Refresh;
					<>O.<0>__Refresh = val2;
					obj2 = (object)val2;
				}
				instance.SubscribeToLocaleChange((LocaleChangeCallback)obj2);
				_subscribed = instance;
			}
			Refresh();
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	private static void Refresh()
	{
		try
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string> { 
			{
				"MORESILENTANIMATIONS.mod_title",
				SettingsState.Text("Title")
			} };
			ComboHotkey.Localize(dictionary);
			SettingsState.Option[] options = SettingsState.Options;
			foreach (SettingsState.Option option in options)
			{
				dictionary[TitleKey(option.Key)] = option.Title;
				dictionary[DescriptionKey(option.Key)] = SettingsState.Text(option.Key + "Description");
			}
			LocManager.Instance.GetTable("settings_ui").MergeWith(dictionary);
			ModManifest val = ((IEnumerable<Mod>)ModManager.Mods).FirstOrDefault((Func<Mod, bool>)((Mod mod) => mod.manifest?.id == "MoreSilentAnimations"))?.manifest;
			if (val != null)
			{
				val.name = SettingsState.Text("Title");
				val.description = SettingsState.Text("Description");
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
internal static class SettingsState
{
	internal sealed record Option(string Key, bool Default)
	{
		internal string Title => Text(Key);

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Key = ");
			builder.Append((object?)Key);
			builder.Append(", Default = ");
			builder.Append(Default.ToString());
			return true;
		}
	}

	internal static readonly string[] Cards = new string[25]
	{
		"LeadingStrike", "Slice", "PreciseCut", "Finisher", "EchoingSlash", "Predator", "Pounce", "PoisonedStab", "Neutralize", "Assassinate",
		"Dash", "SuckerPunch", "Backstab", "Murder", "Suppress", "TheHunt", "BubbleBubble", "DeadlyPoison", "BouncingFlask", "Shiv",
		"GrandFinale", "Snakebite", "Tracking", "KnifeTrap", "StormOfSteel"
	};

	internal static readonly Option[] Options;

	internal static event Action? Changed;

	internal static string Text(string key, string? language = null)
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		object obj = language;
		if (obj == null)
		{
			LocManager instance = LocManager.Instance;
			obj = ((instance != null) ? instance.Language : null);
		}
		string text = (string)obj;
		bool flag = ((text == "zhs" || text == "zht") ? true : false);
		bool flag2 = flag;
		switch (key)
		{
		case "Title":
			if (!flag2)
			{
				return "More Silent Animations";
			}
			return "更多猎人动画";
		case "Description":
			if (!flag2)
			{
				return "Silent card animations and effects; original card resolution is preserved.";
			}
			return "猎人卡牌动画与特效；保留原游戏结算。";
		case "Enabled":
			if (!flag2)
			{
				return "Enable Silent animations";
			}
			return "启用猎人动画";
		case "ShowBulletTimeButton":
			if (!flag2)
			{
				return "Show Bullet Time button";
			}
			return "显示子弹时间按钮";
		case "ShowBulletTimeButtonDescription":
			if (!flag2)
			{
				return "Hide the combat button when off. An assigned hotkey remains available.";
			}
			return "关闭后隐藏战斗中的按钮；已设置的快捷键仍可使用。";
		default:
		{
			if (key.EndsWith("Description", StringComparison.Ordinal))
			{
				if (!flag2)
				{
					return "Controls this mod's animation and effects without changing card behavior.";
				}
				return "控制本模组的动画和特效，不改变卡牌效果。";
			}
			string text2;
			if (!key.EndsWith("Enabled", StringComparison.Ordinal))
			{
				text2 = key;
			}
			else
			{
				text = key;
				text2 = text.Substring(0, text.Length - 7);
			}
			string text3 = text2;
			if (language == "eng")
			{
				return text3;
			}
			try
			{
				return new LocString("cards", StringHelper.Slugify(text3) + ".title").GetFormattedText();
			}
			catch
			{
				return text3;
			}
		}
		}
	}

	internal static object Get(string key)
	{
		bool flag;
		if (!(key == "Enabled"))
		{
			if (key == "ShowBulletTimeButton")
			{
				flag = AnimationSettings.Current.ShowBulletTimeButton;
			}
			else
			{
				flag = !AnimationSettings.Current.DisabledCards.Contains<string>(key.Substring(0, key.Length - 7), StringComparer.Ordinal);
			}
		}
		else
		{
			flag = AnimationSettings.Current.Enabled;
		}
		return flag;
	}

	internal static void Save(string key, object? value)
	{
		if (!(value is bool flag) || !Options.Any((Option o) => o.Key == key))
		{
			throw new ArgumentException("Unknown Silent setting or toggle value");
		}
		AnimationSettings current = AnimationSettings.Current;
		if (key == "Enabled")
		{
			AnimationSettings.Save(current with
			{
				Enabled = flag
			});
		}
		else if (key == "ShowBulletTimeButton")
		{
			AnimationSettings.Save(current with
			{
				ShowBulletTimeButton = flag
			}, resetVisuals: false);
		}
		else
		{
			HashSet<string> hashSet = new HashSet<string>(current.DisabledCards, StringComparer.Ordinal);
			string text = key;
			string item = text.Substring(0, text.Length - 7);
			if (flag)
			{
				hashSet.Remove(item);
			}
			else
			{
				hashSet.Add(item);
			}
			AnimationSettings.Save(current with
			{
				DisabledCards = hashSet.Order<string>(StringComparer.Ordinal).ToArray()
			});
		}
		SettingsState.Changed?.Invoke();
	}

	internal static void Restore()
	{
		AnimationSettings.Save(new AnimationSettings());
		SettingsState.Changed?.Invoke();
	}

	static SettingsState()
	{
		List<Option> list = new List<Option>();
		list.Add(new Option("Enabled", Default: true));
		list.Add(new Option("ShowBulletTimeButton", Default: true));
		list.AddRange(Cards.Select((string c) => new Option(c + "Enabled", Default: true)));
		Options = list.ToArray();
	}
}
[HarmonyPatch(typeof(AttackCommand), "Execute")]
internal static class ShivPresentationPatch
{
	internal sealed class Invocation(Shiv card, Invocation? previous)
	{
		internal readonly Shiv Card = card;

		internal readonly Invocation? Previous = previous;

		internal readonly string Style = StyleFor(card);

		internal KnifeTrapGeometry.Volley? Trap;

		internal Creature? TrapTarget;

		internal bool TrapEmitted;

		internal readonly double Seed = Random.Shared.Next(1, 10000000);

		internal string Form = "shiv";

		internal double Launch;
	}

	internal static readonly AsyncLocal<Invocation?> Current = new AsyncLocal<Invocation>();

	private static readonly Func<CardPlay, Player>? PlayedBy = typeof(CardPlay).GetProperty("Player")?.GetMethod?.CreateDelegate<Func<CardPlay, Player>>();

	internal static string StyleFor(Shiv card)
	{
		Creature creature = ((CardModel)card).Owner.Creature;
		if (creature.GetPowerAmount<PhantomBladesPower>() > 0)
		{
			ICombatState combatState = creature.CombatState;
			bool flag = false;
			foreach (CardPlayFinishedEntry item in CombatManager.Instance.History.CardPlaysFinished)
			{
				CardPlay cardPlay = item.CardPlay;
				if (((CombatHistoryEntry)item).HappenedThisTurn(combatState) && cardPlay.Card.Tags.Contains((CardTag)5))
				{
					Func<CardPlay, Player> playedBy = PlayedBy;
					if ((playedBy != null) ? (playedBy(cardPlay) == ((CardModel)card).Owner) : (cardPlay.Card.Owner.Creature == creature))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return "phantom";
			}
		}
		if (!(((CardModel)card).Enchantment is Inky))
		{
			return "native";
		}
		return "ink";
	}

	internal static bool Trigger(NCreature actor, string trigger)
	{
		if (!(trigger != "Shiv"))
		{
			Invocation value = Current.Value;
			if (value != null && actor.Entity == ((CardModel)value.card).Owner.Creature)
			{
				value.Form = ActorPresentation.BeginShiv(actor, Random.Shared.Next(3));
				return true;
			}
		}
		return false;
	}

	[HarmonyPrefix]
	private static void Prefix(AttackCommand __instance, out Invocation? __state)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		__state = null;
		try
		{
			AbstractModel modelSource = __instance.ModelSource;
			Shiv val = (Shiv)(object)((modelSource is Shiv) ? modelSource : null);
			if (val != null && AnimationSettings.Current.Enabled && !(((object)((CardModel)val).Owner.Character).GetType().Name != "Silent") && (AnimationSettings.Allows((AbstractModel?)(object)val) || KnifeTrapPresentationPatch.Current.Value != null))
			{
				Current.Value = (__state = new Invocation(val, Current.Value));
				KnifeTrapPresentationPatch.Invocation value = KnifeTrapPresentationPatch.Current.Value;
				if (value != null && ((CardModel)value.card).Owner == ((CardModel)val).Owner)
				{
					__state.Trap = value.Next();
				}
				FastModeType fastMode = SaveManager.Instance.PrefsSave.FastMode;
				string text = ((object)(FastModeType)(ref fastMode)/*cast due to .constrained prefix*/).ToString();
				__state.Launch = ((text == "Instant") ? 0.0 : ((text == "Normal") ? 0.2 : 0.1));
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyPostfix]
	private static void Postfix(Invocation? __state)
	{
		if (__state != null)
		{
			Current.Value = __state.previous;
		}
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, Invocation? __state)
	{
		if (__state != null && Current.Value == __state)
		{
			Current.Value = __state.previous;
		}
		return __exception;
	}
}
[HarmonyPatch(typeof(NShivThrowVfx), "Create", new Type[]
{
	typeof(Creature),
	typeof(Creature),
	typeof(Color)
})]
internal static class ShivFactoryPatch
{
	internal static readonly ConditionalWeakTable<NShivThrowVfx, ShivPresentationPatch.Invocation> Owned = new ConditionalWeakTable<NShivThrowVfx, ShivPresentationPatch.Invocation>();

	internal static readonly ConditionalWeakTable<NShivThrowVfx, KnifeTrapGeometry.Volley> TrapLayouts = new ConditionalWeakTable<NShivThrowVfx, KnifeTrapGeometry.Volley>();

	[HarmonyPostfix]
	private static void Postfix(Creature owner, Creature? target, NShivThrowVfx? __result)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (__result == null)
			{
				return;
			}
			ShivPresentationPatch.Invocation value = ShivPresentationPatch.Current.Value;
			if (value == null || owner != ((CardModel)value.card).Owner.Creature)
			{
				return;
			}
			NCreature creatureNode = owner.GetCreatureNode();
			if (creatureNode != null)
			{
				ActorPresentation actorPresentation = ActorPresentation.For(creatureNode);
				if (actorPresentation != null)
				{
					__result.ApplyRotation(actorPresentation.ShivOrigin(creatureNode.VfxSpawnPosition, value.Form, value.Launch), ((Node2D)__result).GlobalPosition);
				}
			}
			Owned.Add(__result, value);
			if (value.Trap != null)
			{
				if (value.TrapEmitted)
				{
					((CanvasItem)__result).Visible = false;
					return;
				}
				value.TrapEmitted = true;
				TrapLayouts.Add(__result, value.Trap);
				KnifeTrapVisual.StyleNative(__result);
			}
			else
			{
				ShivVisual.StyleNative(__result, value.Style);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch(typeof(NShivThrowVfx), "_Ready")]
internal static class ShivVisualReadyPatch
{
	[HarmonyPostfix]
	private static void Postfix(NShivThrowVfx __instance)
	{
		try
		{
			if (ShivFactoryPatch.Owned.TryGetValue(__instance, out ShivPresentationPatch.Invocation value) && value.Launch > 0.0)
			{
				if (ShivFactoryPatch.TrapLayouts.TryGetValue(__instance, out KnifeTrapGeometry.Volley value2))
				{
					new KnifeTrapVisual(__instance, value2.Seed, value2);
				}
				else if (value.Trap == null && value.Style != "native")
				{
					new ShivVisual(__instance, value.Style, value.Seed);
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
internal static class SilentFxCompatibility
{
	private const string PatchId = "page.more_silent_animations.silentfx";

	private static bool _installed;

	private static bool _failed;

	private static readonly HashSet<string> Cards = new HashSet<string>(SettingsState.Cards, StringComparer.Ordinal);

	private static FieldInfo _contextCard = null;

	private static FieldInfo _contextChest = null;

	private static FieldInfo _contextFeet = null;

	private static FieldInfo _anchorChest = null;

	private static FieldInfo _anchorFeet = null;

	private static FieldInfo _powerAnchor = null;

	private static FieldInfo _anchorOwner = null;

	private static Func<CardModel?> _currentShiv = null;

	internal static bool Installed => _installed;

	internal static void InstallIfAvailable()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Expected O, but got Unknown
		if (_installed || _failed)
		{
			return;
		}
		Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "SilentFX");
		if ((object)assembly == null)
		{
			return;
		}
		Harmony val = new Harmony("page.more_silent_animations.silentfx");
		try
		{
			_contextCard = Field("Cards.CardFxContext", "Card");
			_contextChest = Field("Cards.CardFxContext", "Chest");
			_contextFeet = Field("Cards.CardFxContext", "Feet");
			_anchorChest = Field("Powers.FxAnchor", "Chest");
			_anchorFeet = Field("Powers.FxAnchor", "Feet");
			_powerAnchor = Field("Powers.PowerFx", "Anchor");
			_anchorOwner = Field("Powers.FxAnchor", "Owner");
			_currentShiv = Method("Cards.ShivFx", "CurrentShiv").CreateDelegate<Func<CardModel>>();
			(MethodInfo, string, string, string)[] array = new(MethodInfo, string, string, string)[13]
			{
				(Method("Cards.CardFxManager", "CanShow"), null, "CardAllowed", null),
				(Method("Cards.HeldFxManager", "StartSession"), "HeldAllowed", null, null),
				(Method("Cards.ShivFx", "OnShivHit"), "ShivHitAllowed", null, null),
				(Method("Cards.ShivFx", "AdjustVanillaThrow"), "ShivThrowAllowed", null, null),
				(Method("Cards.ShivFan", "Begin"), "FanAllowed", null, null),
				(Method("Cards.ShivFan", "Open"), "FanOpenAllowed", null, null),
				(Method("Cards.ShivFan", "RedirectThrow"), "ShivThrowAllowed", null, null),
				(Method("Powers.PhantomBladesFx", "OnShivPlayed"), "PhantomLaunchAllowed", null, null),
				(Method("Cards.CardFxManager", "BuildContext"), null, "MoveContext", null),
				(Method("Powers.PowerFxManager", "BuildAnchor"), null, "MoveAnchor", null),
				(Method("Powers.IntangibleFx", "Start"), "BeginTint", null, "EndTint"),
				(Method("Powers.IntangibleFx", "Apply"), "BeginTint", null, "EndTint"),
				(Method("Powers.IntangibleFx", "Dispose"), "BeginTint", null, "EndTint")
			};
			for (int i = 0; i < array.Length; i++)
			{
				(MethodInfo, string, string, string) tuple = array[i];
				val.Patch((MethodBase)tuple.Item1, Hook(tuple.Item2), Hook(tuple.Item3), (HarmonyMethod)null, Hook(tuple.Item4));
			}
			MethodInfo methodInfo = AccessTools.Method(typeof(NShivThrowVfx), "Create", new Type[3]
			{
				typeof(Creature),
				typeof(Creature),
				typeof(Color)
			}, (Type[])null);
			HarmonyMethod val2 = new HarmonyMethod(typeof(SilentFxCompatibility), "SyncTrapFactoryTarget", (Type[])null);
			val2.priority = 0;
			val2.after = new string[2] { "sts2.silentfx", "page.more_silent_animations" };
			val.Patch((MethodBase)methodInfo, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			_installed = true;
			Console.WriteLine("[MoreSilentAnimations] SilentFX compatibility active: shared cards, moving anchors and Intangible tint.");
		}
		catch (Exception innerException)
		{
			_failed = true;
			val.UnpatchAll("page.more_silent_animations.silentfx");
			ModEntry.Log(new InvalidOperationException("SilentFX integration unavailable; its original effects retained.", innerException));
		}
		FieldInfo Field(string name, string field)
		{
			return AccessTools.Field(Type(name), field) ?? throw new MissingFieldException(name, field);
		}
		static HarmonyMethod? Hook(string? name)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Expected O, but got Unknown
			if (name != null)
			{
				return new HarmonyMethod(typeof(SilentFxCompatibility), name, (Type[])null);
			}
			return null;
		}
		MethodInfo Method(string name, string method)
		{
			return AccessTools.Method(Type(name), method, (Type[])null, (Type[])null) ?? throw new MissingMethodException(name, method);
		}
		Type Type(string name)
		{
			return assembly.GetType("SilentFX." + name, throwOnError: true);
		}
	}

	private static bool Owns(CardModel? card)
	{
		if (card != null && Cards.Contains(((object)card).GetType().Name) && AnimationSettings.Allows((AbstractModel?)(object)card))
		{
			Player owner = card.Owner;
			if (!(((owner != null) ? ((object)owner.Character).GetType().Name : null) != "Silent"))
			{
				if ((object)ThrustProfile.For((AbstractModel?)(object)card) != null)
				{
					NCreature creatureNode = card.Owner.Creature.GetCreatureNode();
					if (creatureNode != null)
					{
						return CharacterRig.ForPresentation(creatureNode.Body) != null;
					}
					return false;
				}
				return true;
			}
		}
		return false;
	}

	private static void CardAllowed(CardModel card, ref bool __result)
	{
		if (__result && Owns(card))
		{
			__result = false;
		}
	}

	private static bool HeldAllowed(object? c)
	{
		if (c != null)
		{
			object? value = _contextCard.GetValue(c);
			return !Owns((CardModel?)((value is CardModel) ? value : null));
		}
		return true;
	}

	private static bool ShivHitAllowed(CardModel shiv)
	{
		return !Owns(shiv);
	}

	private static bool ShivThrowAllowed()
	{
		return !Owns(_currentShiv());
	}

	private static bool FanAllowed(CardPlay? play)
	{
		return !Owns((play != null) ? play.Card : null);
	}

	private static bool FanOpenAllowed(CardModel card)
	{
		return !Owns(card);
	}

	private static bool PhantomLaunchAllowed(CardModel card)
	{
		return !Owns(card);
	}

	private static void SyncTrapFactoryTarget(Creature owner, ref Creature? target, object[] __args)
	{
		ShivPresentationPatch.Invocation value = ShivPresentationPatch.Current.Value;
		if (value != null && value.Trap != null && value.TrapTarget != null && owner == ((CardModel)value.card).Owner.Creature)
		{
			__args[1] = (target = value.TrapTarget);
		}
	}

	private static Vector2 Offset(NCreature? node)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (node == null || !GodotObject.IsInstanceValid((GodotObject)(object)node) || !GodotObject.IsInstanceValid((GodotObject)(object)node.Body))
		{
			return Vector2.Zero;
		}
		ActorPresentation actorPresentation = ActorPresentation.For(node);
		if (actorPresentation == null || actorPresentation.Dead)
		{
			return Vector2.Zero;
		}
		return actorPresentation.DisplayWorldOrigin - node.Body.GlobalPosition;
	}

	private static void MoveContext(Creature caster, object? __result)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (__result != null)
		{
			Vector2 val = Offset(caster.GetCreatureNode());
			if (!(val == Vector2.Zero))
			{
				_contextChest.SetValue(__result, (Vector2)_contextChest.GetValue(__result) + val);
				_contextFeet.SetValue(__result, (Vector2)_contextFeet.GetValue(__result) + val);
			}
		}
	}

	private static void MoveAnchor(NCreature node, object? __result)
	{
		if (__result == null)
		{
			return;
		}
		Player player = node.Entity.Player;
		if (!(((player != null) ? ((object)player.Character).GetType().Name : null) != "Silent"))
		{
			Func<Vector2> chest = (Func<Vector2>)_anchorChest.GetValue(__result);
			Func<Vector2> feet = (Func<Vector2>)_anchorFeet.GetValue(__result);
			_anchorChest.SetValue(__result, (Func<Vector2>)(() => chest() + Offset(node)));
			_anchorFeet.SetValue(__result, (Func<Vector2>)(() => feet() + Offset(node)));
		}
	}

	private static void BeginTint(object __instance, out DisplayLease? __state)
	{
		__state = null;
		object value = _powerAnchor.GetValue(__instance);
		if (value == null)
		{
			return;
		}
		object? value2 = _anchorOwner.GetValue(value);
		Creature val = (Creature)((value2 is Creature) ? value2 : null);
		if (val != null)
		{
			NCreature creatureNode = val.GetCreatureNode();
			if (creatureNode != null)
			{
				__state = ActorPresentation.For(creatureNode)?.BeginExternalTint();
			}
		}
	}

	private static Exception? EndTint(Exception? __exception, DisplayLease? __state)
	{
		__state?.EndExternalTint();
		return __exception;
	}
}
[HarmonyPatch(typeof(Snakebite), "OnPlay")]
internal static class SnakebitePresentationPatch
{
	internal sealed record Invocation(Snakebite Card, Invocation? Previous);

	internal static readonly AsyncLocal<Invocation?> Current = new AsyncLocal<Invocation>();

	[HarmonyPrefix]
	private static void Prefix(Snakebite __instance, out Invocation? __state)
	{
		__state = null;
		if (AnimationSettings.Allows((AbstractModel?)(object)__instance) && ((object)((CardModel)__instance).Owner.Character).GetType().Name == "Silent")
		{
			Current.Value = (__state = new Invocation(__instance, Current.Value));
		}
	}

	[HarmonyPostfix]
	private static void Postfix(Invocation? __state)
	{
		if ((object)__state != null)
		{
			Current.Value = __state.Previous;
		}
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, Invocation? __state)
	{
		if ((object)__state != null && (object)Current.Value == __state)
		{
			Current.Value = __state.Previous;
		}
		return __exception;
	}
}
[HarmonyPatch(typeof(NCreature), "SetAnimationTrigger")]
internal static class SnakebiteActionPatch
{
	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(NCreature __instance, string trigger)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (FinalePresentationPatch.Suppress(__instance, trigger))
			{
				return;
			}
			if (trigger == "Cast")
			{
				SnakebitePresentationPatch.Invocation value = SnakebitePresentationPatch.Current.Value;
				if ((object)value != null && ((CardModel)value.Card).Owner.Creature == __instance.Entity)
				{
					FastModeType fastMode = SaveManager.Instance.PrefsSave.FastMode;
					SnakeVisual.Begin(__instance, ((object)(FastModeType)(ref fastMode)/*cast due to .constrained prefix*/).ToString().ToLowerInvariant());
					return;
				}
			}
			bool flag;
			switch (trigger)
			{
			case "Attack":
			case "Revive":
			case "Cast":
			case "Dead":
			case "Shiv":
			case "PowerUp":
			case "Relaxed":
			case "Hit":
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				SnakeVisual.Stop(__instance);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			SnakeVisual.Stop(__instance);
		}
	}
}
[HarmonyPatch(typeof(StormOfSteel), "OnPlay")]
internal static class SteelPresentationPatch
{
	[HarmonyPrefix]
	private static void Prefix(StormOfSteel __instance)
	{
		try
		{
			if (!AnimationSettings.Allows((AbstractModel?)(object)__instance) || ((object)((CardModel)__instance).Owner.Character).GetType().Name != "Silent")
			{
				return;
			}
			NCreature actor = ((CardModel)__instance).Owner.Creature.GetCreatureNode();
			if (actor != null)
			{
				VisualCleanup.Safely(delegate
				{
					ActorPresentation.BeginSteel(actor);
				});
				new SteelWind(actor, GameContent.Version, Random.Shared.Next(1, 1000000));
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch(typeof(Tracking), "OnPlay")]
internal static class TrackingPresentationPatch
{
	internal enum TargetMode
	{
		Unconfirmed,
		All,
		Random
	}

	internal static TargetMode Mode = TargetMode.Random;

	[HarmonyPrefix]
	private static void Prefix(Tracking __instance)
	{
		try
		{
			if (!AnimationSettings.Allows((AbstractModel?)(object)__instance) || Mode == TargetMode.Unconfirmed || ((object)((CardModel)__instance).Owner.Character).GetType().Name != "Silent")
			{
				return;
			}
			NCreature creatureNode = ((CardModel)__instance).Owner.Creature.GetCreatureNode();
			if (creatureNode == null)
			{
				return;
			}
			string version = GameContent.Version;
			Creature[] array = TrackingVisual.VisualTargets(((CardModel)__instance).CombatState.Enemies);
			if (array.Length != 0)
			{
				if (Mode == TargetMode.Random)
				{
					array = (Creature[])(object)new Creature[1] { array[Random.Shared.Next(array.Length)] };
				}
				new TrackingVisual(creatureNode, version, array);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
