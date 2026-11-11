using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using DesktopDeathSafety;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.sts2.Core.Nodes.TopBar;

namespace JoanofArcIroncladHpwear;

internal static class PortraitSync
{
	private sealed class Original
	{
		internal Texture2D? Texture;

		internal Texture2D? Applied;

		internal bool Automatic;

		internal int Stage;

		internal bool Selection;
	}

	private sealed class RefreshTarget
	{
		internal Action Refresh;
	}

	[CompilerGenerated]
	private sealed class _003CDescendants_003Ed__32<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T : notnull, Node
	{
		private int _003C_003E1__state;

		private T _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node root;

		public Node _003C_003E3__root;

		private IEnumerator<Node> _003C_003E7__wrap1;

		private Node _003Cchild_003E5__3;

		private IEnumerator<T> _003C_003E7__wrap3;

		T IEnumerator<T>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CDescendants_003Ed__32(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
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
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003Cchild_003E5__3 = null;
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = root.GetChildren(false).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00f9;
				case 1:
					_003C_003E1__state = -3;
					goto IL_0092;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00d8;
					}
					IL_00f9:
					if (_003C_003E7__wrap1.MoveNext())
					{
						_003Cchild_003E5__3 = _003C_003E7__wrap1.Current;
						Node obj = _003Cchild_003E5__3;
						T val = (T)(object)((obj is T) ? obj : null);
						if (val != null)
						{
							_003C_003E2__current = val;
							_003C_003E1__state = 1;
							return true;
						}
						goto IL_0092;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					return false;
					IL_0092:
					_003C_003E7__wrap3 = Descendants<T>(_003Cchild_003E5__3).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_00d8;
					IL_00d8:
					if (_003C_003E7__wrap3.MoveNext())
					{
						T current = _003C_003E7__wrap3.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap3 = null;
					_003Cchild_003E5__3 = null;
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

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap3 != null)
			{
				_003C_003E7__wrap3.Dispose();
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
			_003CDescendants_003Ed__32<T> _003CDescendants_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDescendants_003Ed__ = this;
			}
			else
			{
				_003CDescendants_003Ed__ = new _003CDescendants_003Ed__32<T>(0);
			}
			_003CDescendants_003Ed__.root = _003C_003E3__root;
			return _003CDescendants_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}

	private static readonly ConditionalWeakTable<TextureRect, Original> Originals = new ConditionalWeakTable<TextureRect, Original>();

	private static readonly Dictionary<ulong, WeakReference<TextureRect>> Tracked = new Dictionary<ulong, WeakReference<TextureRect>>();

	private static readonly List<ulong> PortraitScratch = new List<ulong>();

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

	internal static void Track(Node node)
	{
		TextureRect val = (TextureRect)(object)((node is TextureRect) ? node : null);
		if (val != null && !((GodotObject)val).HasMeta(StringName.op_Implicit("JoanPortraitSource")))
		{
			Tracked[((GodotObject)val).GetInstanceId()] = new WeakReference<TextureRect>(val);
		}
	}

	internal static void Untrack(Node node)
	{
		TextureRect val = (TextureRect)(object)((node is TextureRect) ? node : null);
		if (val != null)
		{
			Tracked.Remove(((GodotObject)val).GetInstanceId());
			Restore(val);
		}
	}

	internal static bool HasVisibleTarget()
	{
		foreach (KeyValuePair<TextureRect, Original> item in (IEnumerable<KeyValuePair<TextureRect, Original>>)Originals)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item.Key) && ((CanvasItem)item.Key).IsVisibleInTree() && item.Key.Texture == item.Value.Applied)
			{
				return true;
			}
		}
		return false;
	}

	internal static void RefreshTracked()
	{
		bool flag = SkinSelectionGate.MenuAllowed();
		PortraitScratch.Clear();
		PortraitScratch.AddRange(Tracked.Keys);
		foreach (ulong item in PortraitScratch)
		{
			if (!Tracked.TryGetValue(item, out WeakReference<TextureRect> value) || !value.TryGetTarget(out var target) || !GodotObject.IsInstanceValid((GodotObject)(object)target))
			{
				Tracked.Remove(item);
			}
			else
			{
				if (!((Node)target).IsInsideTree())
				{
					continue;
				}
				Originals.TryGetValue(target, out Original value2);
				if (value2 != null && !value2.Automatic)
				{
					continue;
				}
				if (!flag)
				{
					if (value2 != null)
					{
						Restore(target);
					}
					continue;
				}
				int stage;
				bool selection;
				if (value2 != null && target.Texture == value2.Applied)
				{
					stage = value2.Stage;
					selection = value2.Selection;
				}
				else if (!TryIdentify(target.Texture, out stage, out selection))
				{
					if (value2 != null)
					{
						Restore(target);
					}
					continue;
				}
				Apply(target, stage, selection);
				if (Originals.TryGetValue(target, out Original value3))
				{
					value3.Automatic = true;
					value3.Stage = stage;
					value3.Selection = selection;
				}
			}
		}
	}

	private static bool TryIdentify(Texture2D? texture, out int stage, out bool selection)
	{
		stage = 1;
		selection = false;
		string text = ((texture != null) ? ((Resource)texture).ResourcePath : null) ?? "";
		string file = StringExtensions.GetFile(text);
		if (file == "character_icon_ironclad.png")
		{
			return true;
		}
		if (file == "char_select_ironclad.png")
		{
			selection = true;
			return true;
		}
		if (!SkinCatalog.IsOurPath(text))
		{
			return false;
		}
		for (int i = 1; i <= 3; i++)
		{
			if (file == $"s{i}_run_icon.ctex")
			{
				stage = i;
				return true;
			}
			if (file == $"s{i}_select_icon.ctex")
			{
				stage = i;
				selection = true;
				return true;
			}
		}
		return false;
	}

	internal static Texture2D? Texture(int stage, bool selection, bool outline = false)
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		stage = Math.Clamp(stage, 1, 3);
		if (!outline)
		{
			Texture2D val = Palette.PortraitTexture(stage, selection);
			if (val != null)
			{
				return val;
			}
		}
		string text = $"s{stage}_" + (selection ? "select_icon" : (outline ? "run_outline" : "run_icon"));
		string text2 = "res://joanofarcironcladhpwear/" + text + ".ctex";
		int num = Math.Clamp(WearSettingsRuntime.Current.Appearance, 1, Palette.Names.Length);
		string text3 = "res://joanofarcironcladhpwear/" + $"portrait_palettes/{num - 1}/{text}.png";
		bool flag = num > 1 && !outline && FileAccess.FileExists(text3);
		if (flag)
		{
			text2 = text3;
		}
		if (Textures.TryGetValue(text2, out Texture2D value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			return value;
		}
		Texture2D val3;
		if (flag)
		{
			Image val2 = new Image();
			if ((int)val2.LoadPngFromBuffer(FileAccess.GetFileAsBytes(text2)) != 0)
			{
				return null;
			}
			val3 = (Texture2D)(object)ImageTexture.CreateFromImage(val2);
		}
		else
		{
			val3 = ResourceLoader.Load<Texture2D>(text2, (string)null, (CacheMode)1);
		}
		if (val3 != null)
		{
			Textures[text2] = val3;
		}
		return val3;
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
			value.Automatic = false;
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
				node.Texture = Runtime.SafePortraitTexture(value.Texture);
			}
			Originals.Remove(node);
		}
	}

	[IteratorStateMachine(typeof(_003CDescendants_003Ed__32<>))]
	internal static IEnumerable<T> Descendants<T>(Node root) where T : Node
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDescendants_003Ed__32<T>(-2)
		{
			_003C_003E3__root = root
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
