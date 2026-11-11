using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using PengoTarot.ConfigFW;
using PengoTarot.Data.Divination;
using PengoTarot.Powers;

namespace PengoTarot.Patches;

public static class NMapPointMarkerIconPatch
{
	internal sealed class OrbitItem
	{
		public Control Icon;

		public Sprite2D Sprite;

		public Node2D? Stem;

		public int Flag;

		public float Phase;

		public Label? Badge;

		public int LastShown;

		public bool WasExpired;
	}

	internal sealed class OrbitState
	{
		public readonly List<OrbitItem> OrbitItems = new List<OrbitItem>();

		public (Control Icon, Sprite2D Sprite, int Flag)? LoversItem;

		public float Center;

		public float Time;

		public bool Hovered;

		public bool Sticky;

		public bool Overview;

		public float Progress;
	}

	[CompilerGenerated]
	private sealed class _003CTipsWithExtras_003Ed__41 : IEnumerable<IHoverTip>, IEnumerable, IEnumerator<IHoverTip>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private IHoverTip _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IHoverTip main;

		public IHoverTip _003C_003E3__main;

		private IReadOnlyList<IHoverTip> extras;

		public IReadOnlyList<IHoverTip> _003C_003E3__extras;

		private IEnumerator<IHoverTip> _003C_003E7__wrap1;

		IHoverTip IEnumerator<IHoverTip>.Current
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
		public _003CTipsWithExtras_003Ed__41(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 2)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
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
					_003C_003E2__current = main;
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = extras.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 2:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					IHoverTip current = _003C_003E7__wrap1.Current;
					_003C_003E2__current = current;
					_003C_003E1__state = 2;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
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

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<IHoverTip> IEnumerable<IHoverTip>.GetEnumerator()
		{
			_003CTipsWithExtras_003Ed__41 _003CTipsWithExtras_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CTipsWithExtras_003Ed__ = this;
			}
			else
			{
				_003CTipsWithExtras_003Ed__ = new _003CTipsWithExtras_003Ed__41(0);
			}
			_003CTipsWithExtras_003Ed__.main = _003C_003E3__main;
			_003CTipsWithExtras_003Ed__.extras = _003C_003E3__extras;
			return _003CTipsWithExtras_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<IHoverTip>)this).GetEnumerator();
		}
	}

	public const float OrbitSize = 64f;

	public const float IconSize = 48f;

	public const float OrbitRadius = 76.8f;

	public const float OrbitSpeed = 0.48f;

	public const float MinVisible = 0.06f;

	public const float InitialPhase = (float)Math.PI * 3f / 4f;

	public const float SparkleScale = 0.32f;

	public const float SparkleAlpha = 1f;

	public const float PopDuration = 0.35f;

	public const float LoversOffset = 12f;

	public const int LoversFlag = 6;

	public const float LoversScale = 0.8f;

	public const float IconBaseTexSize = 64f;

	public const float StickyThreshold = 130f;

	public const float IconHoverScale = 1.05f;

	private const string BeamScenePath = "res://scenes/vfx/monsters/kin_priest_beam_vfx.tscn";

	private const float BeamLengthScale = 0.2f;

	private const float BeamThinScale = 0.16f;

	private const float BeamAngleOffset = -(float)Math.PI;

	private static readonly ConditionalWeakTable<NNormalMapPoint, OrbitState> _states = new ConditionalWeakTable<NNormalMapPoint, OrbitState>();

	private static readonly HashSet<NNormalMapPoint> _activeNodes = new HashSet<NNormalMapPoint>();

	private static readonly HashSet<Control> _iconHovered = new HashSet<Control>();

	private const string MarkerLayerName = "PengoTarotMarkerLayer";

	private static readonly string[] _flagNames = new string[22]
	{
		"FOOL", "MAGICIAN", "HIGHPRIESTESS", "EMPRESS", "EMPEROR", "HIEROPHANT", "LOVERS", "CHARIOT", "STRENGTH", "HERMIT",
		"WHEELOFFORTUNE", "JUSTICE", "HANGEDMAN", "DEATH", "TEMPERANCE", "DEVIL", "TOWER", "STAR", "MOON", "SUN",
		"JUDGEMENT", "WORLD"
	};

	internal static void RegisterNode(NNormalMapPoint node)
	{
		_activeNodes.Add(node);
	}

	internal static void UnregisterNode(NNormalMapPoint node)
	{
		_activeNodes.Remove(node);
	}

	public static void SetOverview(bool on)
	{
		foreach (NNormalMapPoint activeNode in _activeNodes)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)activeNode) && TryGetState(activeNode, out OrbitState state))
			{
				state.Overview = on;
			}
		}
	}

	internal static bool TryGetState(NNormalMapPoint node, out OrbitState state)
	{
		return _states.TryGetValue(node, out state);
	}

	internal static void OnExit(NNormalMapPoint node)
	{
		UnregisterNode(node);
		if (!_states.TryGetValue(node, out OrbitState value))
		{
			return;
		}
		_states.Remove(node);
		foreach (OrbitItem orbitItem in value.OrbitItems)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)orbitItem.Icon))
			{
				_iconHovered.Remove(orbitItem.Icon);
				((Node)orbitItem.Icon).QueueFree();
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)orbitItem.Stem))
			{
				((Node)orbitItem.Stem).QueueFree();
			}
		}
		(Control, Sprite2D, int)? loversItem = value.LoversItem;
		if (loversItem.HasValue)
		{
			(Control, Sprite2D, int) valueOrDefault = loversItem.GetValueOrDefault();
			if (GodotObject.IsInstanceValid((GodotObject)(object)valueOrDefault.Item1))
			{
				_iconHovered.Remove(valueOrDefault.Item1);
				((Node)valueOrDefault.Item1).QueueFree();
			}
		}
	}

	private static Control? GetOrCreateMarkerLayer()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		NMapScreen instance = NMapScreen.Instance;
		if (instance == null || !GodotObject.IsInstanceValid((GodotObject)(object)instance))
		{
			return null;
		}
		Control nodeOrNull = ((Node)instance).GetNodeOrNull<Control>(NodePath.op_Implicit("TheMap"));
		if (nodeOrNull == null)
		{
			return null;
		}
		Control val = ((Node)nodeOrNull).GetNodeOrNull<Control>(NodePath.op_Implicit("PengoTarotMarkerLayer"));
		if (val == null)
		{
			val = new Control
			{
				Name = StringName.op_Implicit("PengoTarotMarkerLayer"),
				MouseFilter = (MouseFilterEnum)2
			};
			((Node)nodeOrNull).AddChild((Node)(object)val, false, (InternalMode)0);
			Control nodeOrNull2 = ((Node)nodeOrNull).GetNodeOrNull<Control>(NodePath.op_Implicit("Points"));
			if (nodeOrNull2 != null)
			{
				((Node)nodeOrNull).MoveChild((Node)(object)val, ((Node)nodeOrNull2).GetIndex(false) + 1);
			}
		}
		return val;
	}

	private static float EaseOut(float t)
	{
		return 1f - (1f - t) * (1f - t);
	}

	private static float EaseIn(float t)
	{
		return t * t;
	}

	private static bool HasIcon(int flag)
	{
		string markerIconPath = TarotMarkerSystem.GetMarkerIconPath(flag);
		if (!string.IsNullOrEmpty(markerIconPath))
		{
			return ResourceLoader.Exists(markerIconPath, "");
		}
		return false;
	}

	private static (Control Icon, Sprite2D Sprite) CreateIcon(int flag)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		Texture2D texture = GD.Load<Texture2D>(TarotMarkerSystem.GetMarkerIconPath(flag));
		Sprite2D val = new Sprite2D
		{
			Name = StringName.op_Implicit($"TarotMarkerSprite{flag}"),
			Texture = texture
		};
		((Node2D)val).Position = new Vector2(24f, 24f);
		Control val2 = new Control
		{
			Name = StringName.op_Implicit($"TarotMarker{flag}"),
			CustomMinimumSize = new Vector2(48f, 48f),
			MouseFilter = (MouseFilterEnum)0
		};
		((Node)val2).AddChild((Node)(object)val, false, (InternalMode)0);
		return (Icon: val2, Sprite: val);
	}

	private static Label CreateBadge(int flag)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		Label val = new Label
		{
			Name = StringName.op_Implicit($"TarotBadge{flag}"),
			Text = "0",
			HorizontalAlignment = (HorizontalAlignment)1,
			VerticalAlignment = (VerticalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 17);
		((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_color"), Colors.White);
		((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), Colors.Black);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 5);
		((Control)val).Position = new Vector2(30f, 26f);
		((Control)val).PivotOffset = new Vector2(24f, 24f) - ((Control)val).Position;
		return val;
	}

	private static void ApplyReversedTexture(Sprite2D sprite, int flag)
	{
		string reversedMarkerIconPath = TarotMarkerSystem.GetReversedMarkerIconPath(flag);
		if (!string.IsNullOrEmpty(reversedMarkerIconPath) && ResourceLoader.Exists(reversedMarkerIconPath, ""))
		{
			sprite.Texture = GD.Load<Texture2D>(reversedMarkerIconPath);
		}
	}

	private static string GetFlagName(int flag)
	{
		if (flag < 0 || flag >= _flagNames.Length)
		{
			return string.Empty;
		}
		return _flagNames[flag];
	}

	internal static IHoverTip[] ExtraTipsForFlag(int flag)
	{
		return flag switch
		{
			7 => (IHoverTip[])(object)new IHoverTip[1] { HoverTipFactory.FromPower<VulnerablePower>((int?)null) }, 
			8 => (IHoverTip[])(object)new IHoverTip[1] { HoverTipFactory.FromPower<WeakPower>((int?)null) }, 
			9 => (IHoverTip[])(object)new IHoverTip[1] { HoverTipFactory.FromPower<TarHermitReversedPower>((int?)null) }, 
			11 => (IHoverTip[])(object)new IHoverTip[1] { HoverTipFactory.FromKeyword((CardKeyword)1) }, 
			12 => (IHoverTip[])(object)new IHoverTip[1] { HoverTipFactory.FromKeyword((CardKeyword)1) }, 
			_ => Array.Empty<IHoverTip>(), 
		};
	}

	private static void ShowIconTip(Control icon, int flag)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		string flagName = GetFlagName(flag);
		if (string.IsNullOrEmpty(flagName))
		{
			return;
		}
		LocString val = ConfigFloatingWindowLoc.BuildMapDescription(flag);
		Texture2D val2 = null;
		string markerIconPath = TarotMarkerSystem.GetMarkerIconPath(flag);
		if (!string.IsNullOrEmpty(markerIconPath) && ResourceLoader.Exists(markerIconPath, ""))
		{
			val2 = GD.Load<Texture2D>(markerIconPath);
		}
		HoverTip val3 = ((val != null) ? new HoverTip(val, val2) : new HoverTip(new LocString("gameplay_ui", "BAL_CFW_FLAG_" + flagName + "_DESC"), val2));
		IHoverTip[] array = (IHoverTip[])((ConfigFloatingWindowLoc.IsEliteDivination(flag) && TarotMarkerSystem.IsExpired(flag)) ? ((Array)Array.Empty<IHoverTip>()) : ((Array)ExtraTipsForFlag(flag)));
		NHoverTipSet tip = ((array.Length != 0) ? NHoverTipSet.CreateAndShow(icon, TipsWithExtras((IHoverTip)(object)val3, array), (HoverTipAlignment)0) : NHoverTipSet.CreateAndShow(icon, (IHoverTip)(object)val3, (HoverTipAlignment)0));
		if (tip != null)
		{
			HoverTipAlignment alignment = HoverTip.GetHoverTipAlignment(icon, 0.75f);
			Callable val4 = Callable.From((Action)delegate
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				tip.SetAlignment(icon, alignment);
			});
			((Callable)(ref val4)).CallDeferred(Array.Empty<Variant>());
		}
	}

	[IteratorStateMachine(typeof(_003CTipsWithExtras_003Ed__41))]
	private static IEnumerable<IHoverTip> TipsWithExtras(IHoverTip main, IReadOnlyList<IHoverTip> extras)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTipsWithExtras_003Ed__41(-2)
		{
			_003C_003E3__main = main,
			_003C_003E3__extras = extras
		};
	}

	private static void HideIconTip(Control icon)
	{
		NHoverTipSet.Remove(icon);
	}

	private static Node2D? CreateBeamStem(Control container)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		PackedScene val = GD.Load<PackedScene>("res://scenes/vfx/monsters/kin_priest_beam_vfx.tscn");
		if (val == null)
		{
			return null;
		}
		Node2D val2 = val.Instantiate<Node2D>((GenEditState)0);
		Node2D node = ((Node)val2).GetNode<Node2D>(NodePath.op_Implicit("BeamHolder"));
		GpuParticles2D node2 = ((Node)val2).GetNode<GpuParticles2D>(NodePath.op_Implicit("BeamHolder/StaticParticles"));
		node2.LocalCoords = true;
		node2.Emitting = false;
		((CanvasItem)node2).Visible = false;
		((CanvasItem)node).Visible = false;
		val2.Position = new Vector2(32f, 32f);
		((Node)container).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)container).MoveChild((Node)(object)val2, 0);
		return val2;
	}

	internal static void OnReady(NNormalMapPoint node)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		MapPoint point = ((NMapPoint)node).Point;
		if (point == null)
		{
			return;
		}
		List<int> displayedFlagsAt = TarotMarkerSystem.GetDisplayedFlagsAt(point.coord);
		if (displayedFlagsAt.Count == 0)
		{
			return;
		}
		Control nodeOrNull = ((Node)node).GetNodeOrNull<Control>(NodePath.op_Implicit("%IconContainer"));
		if (nodeOrNull == null)
		{
			return;
		}
		float num = 32f;
		OrbitState state = new OrbitState
		{
			Center = num
		};
		PackedScene val = GD.Load<PackedScene>("res://scenes/vfx/card_sparkles_vfx.tscn");
		if (val != null)
		{
			GpuParticles2D val2 = val.Instantiate<GpuParticles2D>((GenEditState)0);
			val2.LocalCoords = true;
			((Node2D)val2).Position = new Vector2(num, num);
			((Node2D)val2).Scale = Vector2.One * 0.32f;
			((CanvasItem)val2).SelfModulate = new Color(1f, 1f, 1f, 1f);
			((Node)nodeOrNull).AddChild((Node)(object)val2, false, (InternalMode)0);
		}
		Control orCreateMarkerLayer = GetOrCreateMarkerLayer();
		if (orCreateMarkerLayer == null)
		{
			return;
		}
		List<int> list = new List<int>();
		bool flag = false;
		foreach (int item in displayedFlagsAt)
		{
			if (item == 6)
			{
				flag = true;
			}
			else if (HasIcon(item))
			{
				list.Add(item);
			}
		}
		int count = list.Count;
		int num2 = 0;
		foreach (int flag2 in list)
		{
			var (icon, val3) = CreateIcon(flag2);
			((Node2D)val3).Scale = Vector2.Zero;
			((Node)orCreateMarkerLayer).AddChild((Node)(object)icon, false, (InternalMode)0);
			Node2D stem = CreateBeamStem(nodeOrNull);
			OrbitItem orbitItem = new OrbitItem
			{
				Icon = icon,
				Sprite = val3,
				Stem = stem,
				Flag = flag2,
				Phase = (float)Math.PI * 3f / 4f + (float)Math.PI * 2f * (float)num2 / (float)count
			};
			if (TarotMarkerSystem.IsExpired(flag2))
			{
				orbitItem.WasExpired = true;
				ApplyReversedTexture(val3, flag2);
			}
			else
			{
				orbitItem.Badge = CreateBadge(flag2);
				orbitItem.LastShown = TarotMarkerSystem.GetProgressForDisplay(flag2);
				orbitItem.Badge.Text = orbitItem.LastShown.ToString();
				((CanvasItem)orbitItem.Badge).Visible = false;
				((Node)icon).AddChild((Node)(object)orbitItem.Badge, false, (InternalMode)0);
			}
			state.OrbitItems.Add(orbitItem);
			icon.MouseEntered += delegate
			{
				_iconHovered.Add(icon);
				ShowIconTip(icon, flag2);
			};
			icon.MouseExited += delegate
			{
				_iconHovered.Remove(icon);
				HideIconTip(icon);
			};
			num2++;
		}
		if (flag)
		{
			var (val4, val5) = CreateIcon(6);
			((Node2D)val5).Scale = Vector2.Zero;
			float num3 = -20f;
			float num4 = 20f;
			val4.Position = new Vector2(32f, 32f) + new Vector2(num3, num4) - new Vector2(24f, 24f);
			val4.MouseFilter = (MouseFilterEnum)2;
			((Node)nodeOrNull).AddChild((Node)(object)val4, false, (InternalMode)0);
			state.LoversItem = (val4, val5, 6);
		}
		((Control)node).MouseEntered += delegate
		{
			state.Hovered = true;
			state.Sticky = false;
		};
		((Control)node).MouseExited += delegate
		{
			state.Hovered = false;
			state.Sticky = true;
		};
		_states.AddOrUpdate(node, state);
		RegisterNode(node);
	}

	internal static void OnProcess(NNormalMapPoint node, double delta)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0536: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetState(node, out OrbitState state))
		{
			return;
		}
		Rect2 globalRect;
		if (state.Hovered || state.Sticky)
		{
			globalRect = ((Control)node).GetGlobalRect();
			Vector2 val = ((Rect2)(ref globalRect)).GetCenter() - ((CanvasItem)node).GetGlobalMousePosition();
			if (((Vector2)(ref val)).Length() > 130f)
			{
				state.Hovered = false;
				state.Sticky = false;
			}
		}
		bool flag = state.Hovered || state.Sticky || state.Overview;
		if (flag && state.Progress < 1f)
		{
			state.Progress = Mathf.Min(1f, state.Progress + (float)delta / 0.35f);
		}
		else if (!flag && state.Progress > 0f)
		{
			state.Progress = Mathf.Max(0f, state.Progress - (float)delta / 0.35f);
		}
		bool overview = state.Overview;
		if (state.Hovered || (!overview && !state.Sticky && state.Progress > 0f))
		{
			state.Time += (float)delta * Mathf.SmoothStep(0f, 1f, state.Progress);
		}
		(Control, Sprite2D, int)? loversItem = state.LoversItem;
		if (loversItem.HasValue)
		{
			(Control, Sprite2D, int) valueOrDefault = loversItem.GetValueOrDefault();
			if (GodotObject.IsInstanceValid((GodotObject)(object)valueOrDefault.Item1))
			{
				globalRect = valueOrDefault.Item1.GetGlobalRect();
				bool flag2 = ((Rect2)(ref globalRect)).HasPoint(((CanvasItem)valueOrDefault.Item1).GetGlobalMousePosition());
				if (flag2 && !_iconHovered.Contains(valueOrDefault.Item1))
				{
					_iconHovered.Add(valueOrDefault.Item1);
					ShowIconTip(valueOrDefault.Item1, 6);
				}
				else if (!flag2 && _iconHovered.Contains(valueOrDefault.Item1))
				{
					_iconHovered.Remove(valueOrDefault.Item1);
					HideIconTip(valueOrDefault.Item1);
				}
			}
		}
		if (state.Progress <= 0f)
		{
			return;
		}
		float num = (state.Hovered ? EaseOut(state.Progress) : EaseIn(state.Progress));
		float num2 = ((num < 0.06f) ? 0f : num);
		globalRect = ((Control)node).GetGlobalRect();
		Vector2 center = ((Rect2)(ref globalRect)).GetCenter();
		foreach (OrbitItem orbitItem in state.OrbitItems)
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)orbitItem.Icon))
			{
				continue;
			}
			float num3 = state.Time * 0.48f + orbitItem.Phase;
			Vector2 val2 = center + new Vector2(Mathf.Cos(num3), Mathf.Sin(num3)) * 76.8f;
			orbitItem.Icon.GlobalPosition = val2 - new Vector2(24f, 24f);
			bool flag3 = num2 > 0f;
			orbitItem.Icon.MouseFilter = (MouseFilterEnum)(flag3 ? 0 : 2);
			float num4 = (_iconHovered.Contains(orbitItem.Icon) ? 1.05f : 1f);
			((Node2D)orbitItem.Sprite).Scale = Vector2.One * 0.75f * num2 * num4;
			((Node2D)orbitItem.Sprite).Rotation = 0f;
			UpdateStem(orbitItem.Stem, num3, num);
			bool num5 = TarotMarkerSystem.IsExpired(orbitItem.Flag);
			if (num5 && !orbitItem.WasExpired)
			{
				orbitItem.WasExpired = true;
				ApplyReversedTexture(orbitItem.Sprite, orbitItem.Flag);
				if (orbitItem.Badge != null)
				{
					((CanvasItem)orbitItem.Badge).Visible = false;
				}
			}
			if (!num5 && orbitItem.Badge != null)
			{
				int progressForDisplay = TarotMarkerSystem.GetProgressForDisplay(orbitItem.Flag);
				if (progressForDisplay != orbitItem.LastShown)
				{
					orbitItem.LastShown = progressForDisplay;
					orbitItem.Badge.Text = progressForDisplay.ToString();
				}
				((CanvasItem)orbitItem.Badge).Visible = flag3;
				((Control)orbitItem.Badge).Scale = Vector2.One * (num2 * num4);
			}
		}
		loversItem = state.LoversItem;
		if (!loversItem.HasValue)
		{
			return;
		}
		(Control, Sprite2D, int) valueOrDefault2 = loversItem.GetValueOrDefault();
		if (GodotObject.IsInstanceValid((GodotObject)(object)valueOrDefault2.Item1))
		{
			valueOrDefault2.Item1.PivotOffset = new Vector2(24f, 24f);
			float num6 = (_iconHovered.Contains(valueOrDefault2.Item1) ? 1.05f : 1f);
			float num7 = 1f;
			float num8 = 1f;
			Control nodeOrNull = ((Node)node).GetNodeOrNull<Control>(NodePath.op_Implicit("%IconContainer"));
			if (nodeOrNull != null && nodeOrNull.Scale.X != 0f)
			{
				num7 = nodeOrNull.Scale.X;
			}
			TextureRect nodeOrNull2 = ((Node)node).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%Icon"));
			if (nodeOrNull2 != null)
			{
				num8 = ((Control)nodeOrNull2).Scale.X;
			}
			valueOrDefault2.Item1.Scale = Vector2.One * (1f / num7);
			((Node2D)valueOrDefault2.Item2).Scale = Vector2.One * 0.75f * num2 * num6 * 0.8f * num7 * num8;
			((Node2D)valueOrDefault2.Item2).Rotation = 0f;
		}
	}

	private static void UpdateStem(Node2D? vfx, float angle, float p)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (vfx != null && GodotObject.IsInstanceValid((GodotObject)(object)vfx))
		{
			Node parent = ((Node)vfx).GetParent();
			Control val = (Control)(object)((parent is Control) ? parent : null);
			vfx.Scale = ((val != null) ? (Vector2.One / val.Scale) : Vector2.One);
			Node2D node = ((Node)vfx).GetNode<Node2D>(NodePath.op_Implicit("BeamHolder"));
			GpuParticles2D node2 = ((Node)vfx).GetNode<GpuParticles2D>(NodePath.op_Implicit("BeamHolder/StaticParticles"));
			bool flag2 = (((CanvasItem)node).Visible = p > 0.06f);
			((CanvasItem)node2).Visible = flag2;
			node2.Emitting = flag2;
			if (flag2)
			{
				vfx.Rotation = angle + -(float)Math.PI;
				node.Scale = new Vector2(p * 0.2f, 0.16f);
			}
		}
	}
}
