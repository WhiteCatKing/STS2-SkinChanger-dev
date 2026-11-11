using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Judgement;
using PengoTarot.Patches;

namespace PengoTarot.ConfigFW;

[ScriptPath("res://configFW/Scripts/NConfigFloatingWindowEntryButton.cs")]
public class NConfigFloatingWindowEntryButton : NButton
{
	private enum DockSide
	{
		None,
		Left,
		Right,
		Top,
		Bottom
	}

	public class MethodName : MethodName
	{
		public static readonly StringName PlayPortableTarotFullFeedback = StringName.op_Implicit("PlayPortableTarotFullFeedback");

		public static readonly StringName PreparePortableTarotDiscardAnimation = StringName.op_Implicit("PreparePortableTarotDiscardAnimation");

		public static readonly StringName SetRunEntryVisible = StringName.op_Implicit("SetRunEntryVisible");

		public static readonly StringName _Ready = StringName.op_Implicit("_Ready");

		public static readonly StringName OnMouseEntered = StringName.op_Implicit("OnMouseEntered");

		public static readonly StringName OnMouseExited = StringName.op_Implicit("OnMouseExited");

		public static readonly StringName _Input = StringName.op_Implicit("_Input");

		public static readonly StringName _Process = StringName.op_Implicit("_Process");

		public static readonly StringName _ExitTree = StringName.op_Implicit("_ExitTree");

		public static readonly StringName CreatePortableShelfAndRefresh = StringName.op_Implicit("CreatePortableShelfAndRefresh");

		public static readonly StringName CreatePortableShelf = StringName.op_Implicit("CreatePortableShelf");

		public static readonly StringName UpdatePortableShelfAvailability = StringName.op_Implicit("UpdatePortableShelfAvailability");

		public static readonly StringName UpdateEntryLayerForContext = StringName.op_Implicit("UpdateEntryLayerForContext");

		public static readonly StringName SyncPortableShelfLayerTransform = StringName.op_Implicit("SyncPortableShelfLayerTransform");

		public static readonly StringName ClosePortablePopup = StringName.op_Implicit("ClosePortablePopup");

		public static readonly StringName UpdatePortableShelfPlacement = StringName.op_Implicit("UpdatePortableShelfPlacement");

		public static readonly StringName GetPortableShelfBasePosition = StringName.op_Implicit("GetPortableShelfBasePosition");

		public static readonly StringName PlayShelfFullFeedback = StringName.op_Implicit("PlayShelfFullFeedback");

		public static readonly StringName RestorePortableShelfState = StringName.op_Implicit("RestorePortableShelfState");

		public static readonly StringName BeginDrag = StringName.op_Implicit("BeginDrag");

		public static readonly StringName RootGlobalPos = StringName.op_Implicit("RootGlobalPos");

		public static readonly StringName DragTo = StringName.op_Implicit("DragTo");

		public static readonly StringName TrySnapAndDock = StringName.op_Implicit("TrySnapAndDock");

		public static readonly StringName GetDockPosition = StringName.op_Implicit("GetDockPosition");

		public static readonly StringName GetPeekPosition = StringName.op_Implicit("GetPeekPosition");

		public static readonly StringName GetDockHoverRect = StringName.op_Implicit("GetDockHoverRect");

		public static readonly StringName DockIn = StringName.op_Implicit("DockIn");

		public static readonly StringName PeekOut = StringName.op_Implicit("PeekOut");

		public static readonly StringName AnimatePortableShelfForDock = StringName.op_Implicit("AnimatePortableShelfForDock");

		public static readonly StringName ApplyPortableShelfDockState = StringName.op_Implicit("ApplyPortableShelfDockState");

		public static readonly StringName AnimateDock = StringName.op_Implicit("AnimateDock");

		public static readonly StringName SaveState = StringName.op_Implicit("SaveState");

		public static readonly StringName ApplySavedState = StringName.op_Implicit("ApplySavedState");

		public static readonly StringName ScaleTo = StringName.op_Implicit("ScaleTo");

		public static readonly StringName RestartAutoHide = StringName.op_Implicit("RestartAutoHide");

		public static readonly StringName CancelAutoHide = StringName.op_Implicit("CancelAutoHide");

		public static readonly StringName OnAutoHideTimer = StringName.op_Implicit("OnAutoHideTimer");
	}

	public class PropertyName : PropertyName
	{
		public static readonly StringName _root = StringName.op_Implicit("_root");

		public static readonly StringName _dragging = StringName.op_Implicit("_dragging");

		public static readonly StringName _entryVisible = StringName.op_Implicit("_entryVisible");

		public static readonly StringName _dragOffset = StringName.op_Implicit("_dragOffset");

		public static readonly StringName _dockSide = StringName.op_Implicit("_dockSide");

		public static readonly StringName _isDocked = StringName.op_Implicit("_isDocked");

		public static readonly StringName _dockedHovered = StringName.op_Implicit("_dockedHovered");

		public static readonly StringName _hoverTween = StringName.op_Implicit("_hoverTween");

		public static readonly StringName _dockTween = StringName.op_Implicit("_dockTween");

		public static readonly StringName _autoHideTween = StringName.op_Implicit("_autoHideTween");

		public static readonly StringName _portableShelf = StringName.op_Implicit("_portableShelf");

		public static readonly StringName _portableShelfLayer = StringName.op_Implicit("_portableShelfLayer");

		public static readonly StringName _portableShelfContent = StringName.op_Implicit("_portableShelfContent");

		public static readonly StringName _portableCardsRow = StringName.op_Implicit("_portableCardsRow");

		public static readonly StringName _portableCount = StringName.op_Implicit("_portableCount");

		public static readonly StringName _portableFeedbackTween = StringName.op_Implicit("_portableFeedbackTween");

		public static readonly StringName _portableDockTween = StringName.op_Implicit("_portableDockTween");

		public static readonly StringName _portablePopup = StringName.op_Implicit("_portablePopup");

		public static readonly StringName _portableAvailabilityRefreshElapsed = StringName.op_Implicit("_portableAvailabilityRefreshElapsed");
	}

	public class SignalName : SignalName
	{
	}

	private const float DragThreshold = 6f;

	private const float DockRatio = 0.7f;

	private const float DockedAlpha = 0.2f;

	private const float DockTriggerRatio = 0.5f;

	private const float AutoHideDelay = 8f;

	private static readonly Vector2 DefaultEntryPosition = new Vector2(1775f, 189f);

	private static Vector2? _savedPos;

	private static DockSide _savedDockSide = DockSide.None;

	private static bool _savedDocked;

	private Control? _root;

	private bool _dragging;

	private bool _entryVisible = true;

	private Vector2 _dragOffset;

	private DockSide _dockSide;

	private bool _isDocked;

	private bool _dockedHovered;

	private Tween? _hoverTween;

	private Tween? _dockTween;

	private Tween? _autoHideTween;

	private Control? _portableShelf;

	private Control? _portableShelfLayer;

	private Control? _portableShelfContent;

	private HBoxContainer? _portableCardsRow;

	private Label? _portableCount;

	private readonly List<TextureRect> _portableCards = new List<TextureRect>();

	private Tween? _portableFeedbackTween;

	private Tween? _portableDockTween;

	private NPortableTarotPopup? _portablePopup;

	private int? _pendingDiscardAnimationSlot;

	private bool? _isAboveRestSiteDeck;

	private int? _portableAvailabilityMask;

	private double _portableAvailabilityRefreshElapsed;

	private const string PortablePopupScenePath = "res://configFW/Scenes/portable_tarot_popup.tscn";

	private static NConfigFloatingWindowEntryButton? _runInstance;

	public static void SetPortableTarots(IReadOnlyList<CardModel> cards)
	{
		if (_runInstance != null)
		{
			if (!ConfigFloatingWindowRunData.GetTarFlag(20))
			{
				_runInstance.RefreshPortableShelf(Array.Empty<CardModel>());
			}
			else
			{
				_runInstance.RefreshPortableShelf(cards);
			}
		}
	}

	public static void PlayPortableTarotFullFeedback()
	{
		_runInstance?.PlayShelfFullFeedback();
	}

	public static void PreparePortableTarotDiscardAnimation(int slotIndex)
	{
		if (_runInstance != null)
		{
			_runInstance._pendingDiscardAnimationSlot = slotIndex;
		}
	}

	public void SetRunEntryVisible(bool visible)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		_entryVisible = visible;
		((CanvasItem)this).Visible = visible;
		if (_root != null)
		{
			Tween? dockTween = _dockTween;
			if (dockTween != null)
			{
				dockTween.Kill();
			}
			Tween? hoverTween = _hoverTween;
			if (hoverTween != null)
			{
				hoverTween.Kill();
			}
			Tween? portableDockTween = _portableDockTween;
			if (portableDockTween != null)
			{
				portableDockTween.Kill();
			}
			if (visible)
			{
				ApplySavedState();
				RestartAutoHide();
				return;
			}
			CancelAutoHide();
			_dragging = false;
			_dockSide = DockSide.None;
			_isDocked = false;
			_dockedHovered = false;
			_root.GlobalPosition = DefaultEntryPosition;
			_root.Scale = Vector2.One;
			((CanvasItem)this).Modulate = Colors.White;
			UpdatePortableShelfPlacement();
			SyncPortableShelfLayerTransform();
		}
	}

	public override void _Ready()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		((NClickableControl)this).ConnectSignals();
		((NClickableControl)this)._ignoreDragThreshold = 6f;
		ref Control? root = ref _root;
		Node parent = ((Node)this).GetParent();
		root = (Control?)(object)((parent is Control) ? parent : null);
		if (RunManager.Instance.IsInProgress)
		{
			_runInstance = this;
		}
		Callable val = Callable.From((Action)CreatePortableShelfAndRefresh);
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		if (_root != null)
		{
			ApplySavedState();
			((GodotObject)this).Connect(SignalName.Released, Callable.From<NClickableControl>((Action<NClickableControl>)delegate
			{
				ConfigFloatingWindow.OnEntryClicked();
			}), 0u);
			((Control)this).MouseEntered += OnMouseEntered;
			((Control)this).MouseExited += OnMouseExited;
			RestartAutoHide();
		}
	}

	private void OnMouseEntered()
	{
		if (_entryVisible && !_dragging && !_isDocked)
		{
			CancelAutoHide();
			ScaleTo(1.05f);
		}
	}

	private void OnMouseExited()
	{
		if (_entryVisible && !_dragging && !_isDocked)
		{
			RestartAutoHide();
			ScaleTo(1f);
			TrySnapAndDock();
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I8
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (!_entryVisible || !((CanvasItem)this).IsVisibleInTree())
		{
			return;
		}
		((NButton)this)._Input(inputEvent);
		InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
		if (val != null && (long)val.ButtonIndex == 1)
		{
			Rect2 globalRect;
			if (val.Pressed)
			{
				globalRect = ((Control)this).GetGlobalRect();
				if (((Rect2)(ref globalRect)).HasPoint(((CanvasItem)this).GetGlobalMousePosition()))
				{
					BeginDrag();
					return;
				}
			}
			if (!val.Pressed && _dragging)
			{
				_dragging = false;
				SaveState();
				globalRect = ((Control)this).GetGlobalRect();
				if (!((Rect2)(ref globalRect)).HasPoint(((CanvasItem)this).GetGlobalMousePosition()))
				{
					TrySnapAndDock();
				}
			}
		}
		else if (inputEvent is InputEventMouseMotion && _dragging)
		{
			DragTo(((CanvasItem)this).GetGlobalMousePosition() - _dragOffset);
		}
	}

	public override void _Process(double delta)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		UpdateEntryLayerForContext();
		SyncPortableShelfLayerTransform();
		_portableAvailabilityRefreshElapsed += delta;
		if (_portableAvailabilityRefreshElapsed >= 0.15)
		{
			_portableAvailabilityRefreshElapsed = 0.0;
			UpdatePortableShelfAvailability();
		}
		if (!_entryVisible || !((CanvasItem)this).IsVisibleInTree() || !_isDocked || _root == null)
		{
			return;
		}
		Rect2 dockHoverRect = GetDockHoverRect();
		bool flag = ((Rect2)(ref dockHoverRect)).HasPoint(((CanvasItem)this).GetGlobalMousePosition());
		if (flag != _dockedHovered)
		{
			_dockedHovered = flag;
			if (flag)
			{
				ClosePortablePopup(immediate: true);
				PeekOut();
				CancelAutoHide();
			}
			else
			{
				DockIn();
			}
		}
	}

	public override void _ExitTree()
	{
		if (_runInstance == this)
		{
			_runInstance = null;
		}
	}

	private void CreatePortableShelfAndRefresh()
	{
		if (((Node)this).IsInsideTree() && _root != null && GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			CreatePortableShelf();
			PortableTarotInventoryUi.RefreshLocal(null);
		}
	}

	private void CreatePortableShelf()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected O, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Expected O, but got Unknown
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Expected O, but got Unknown
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		if (_root != null && _portableShelf == null)
		{
			Control val = new Control
			{
				Name = StringName.op_Implicit("PortableTarotShelfLayer"),
				MouseFilter = (MouseFilterEnum)2,
				Size = _root.Size
			};
			PanelContainer val2 = new PanelContainer
			{
				Name = StringName.op_Implicit("PortableTarotShelf"),
				Visible = false,
				MouseFilter = (MouseFilterEnum)2,
				CustomMinimumSize = new Vector2(104f, _root.Size.Y)
			};
			StyleBoxFlat val3 = new StyleBoxFlat
			{
				BgColor = new Color(1f, 1f, 1f, 0.25f),
				CornerRadiusTopLeft = 6,
				CornerRadiusTopRight = 6,
				CornerRadiusBottomLeft = 6,
				CornerRadiusBottomRight = 6,
				ContentMarginLeft = 4f,
				ContentMarginRight = 4f,
				ContentMarginTop = 4f,
				ContentMarginBottom = 4f
			};
			((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)val3);
			Control val4 = new Control
			{
				Name = StringName.op_Implicit("Content"),
				MouseFilter = (MouseFilterEnum)2
			};
			CenterContainer val5 = new CenterContainer
			{
				Name = StringName.op_Implicit("CardCenter"),
				MouseFilter = (MouseFilterEnum)2,
				AnchorRight = 1f,
				AnchorBottom = 1f,
				GrowHorizontal = (GrowDirection)2,
				GrowVertical = (GrowDirection)2
			};
			HBoxContainer val6 = new HBoxContainer
			{
				Name = StringName.op_Implicit("Cards"),
				MouseFilter = (MouseFilterEnum)2,
				Alignment = (AlignmentMode)1
			};
			((Control)val6).AddThemeConstantOverride(StringName.op_Implicit("separation"), 4);
			_portableCount = new Label
			{
				Text = $"0/{4}",
				HorizontalAlignment = (HorizontalAlignment)2,
				VerticalAlignment = (VerticalAlignment)2,
				MouseFilter = (MouseFilterEnum)2,
				AnchorLeft = 1f,
				AnchorTop = 1f,
				AnchorRight = 1f,
				AnchorBottom = 1f,
				OffsetLeft = -42f,
				OffsetTop = -27f,
				OffsetRight = -2f,
				OffsetBottom = -1f,
				GrowHorizontal = (GrowDirection)0,
				GrowVertical = (GrowDirection)0
			};
			((Control)_portableCount).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 17);
			((Control)_portableCount).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 5);
			((Control)_portableCount).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color(0f, 0f, 0f, 0.7f));
			((Node)val5).AddChild((Node)(object)val6, false, (InternalMode)0);
			((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
			((Node)val4).AddChild((Node)(object)_portableCount, false, (InternalMode)0);
			((Node)val2).AddChild((Node)(object)val4, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)_root).AddChild((Node)(object)val, false, (InternalMode)0);
			if (((Node)val).GetParent() != _root)
			{
				((Node)val).QueueFree();
				return;
			}
			((Node)_root).MoveChild((Node)(object)val, 0);
			_portableShelfLayer = val;
			_portableShelf = (Control?)(object)val2;
			_portableShelfContent = val4;
			_portableCardsRow = val6;
		}
	}

	private void RefreshPortableShelf(IReadOnlyList<CardModel> models)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Expected O, but got Unknown
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Expected O, but got Unknown
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Expected O, but got Unknown
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		if (_portableShelf == null || _portableCount == null || _portableCardsRow == null)
		{
			return;
		}
		TextureRect val = null;
		int? pendingDiscardAnimationSlot = _pendingDiscardAnimationSlot;
		if (pendingDiscardAnimationSlot.HasValue)
		{
			int valueOrDefault = pendingDiscardAnimationSlot.GetValueOrDefault();
			if (valueOrDefault >= 0 && valueOrDefault < _portableCards.Count)
			{
				val = _portableCards[valueOrDefault];
				Node parent = ((Node)val).GetParent();
				Vector2 globalPosition = ((Control)val).GlobalPosition;
				((Node)val).Reparent((Node)(object)(_portableShelfLayer ?? _root), true);
				if (parent != null)
				{
					parent.QueueFree();
				}
				((Control)val).GlobalPosition = globalPosition;
				((Node)(_portableShelfLayer ?? _root)).MoveChild((Node)(object)val, 0);
				Tween obj = ((Node)this).CreateTween().SetParallel(true);
				obj.TweenProperty((GodotObject)(object)val, NodePath.op_Implicit("position:y"), Variant.op_Implicit(((Control)val).Position.Y - 100f), 0.4000000059604645).SetEase((EaseType)0).SetTrans((TransitionType)10);
				obj.TweenProperty((GodotObject)(object)val, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 0.4000000059604645);
				obj.Chain().TweenCallback(Callable.From((Action)((Node)val).QueueFree));
			}
		}
		_pendingDiscardAnimationSlot = null;
		foreach (TextureRect portableCard in _portableCards)
		{
			if (portableCard != val)
			{
				((Node)(((object)((Node)portableCard).GetParent()) ?? ((object)portableCard))).QueueFree();
			}
		}
		_portableCards.Clear();
		int num = Mathf.Min(models.Count, 4);
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			CardModel model = models[i];
			Texture2D val2 = ResourceLoader.Load<Texture2D>(model.PortraitPath, (string)null, (CacheMode)1);
			float num3 = ((val2 != null && val2.GetHeight() > 0) ? Mathf.Round(152f * (float)val2.GetWidth() / (float)val2.GetHeight()) : 114f);
			TextureRect card = new TextureRect
			{
				Texture = val2,
				CustomMinimumSize = new Vector2(num3, 152f),
				ExpandMode = (ExpandModeEnum)1,
				StretchMode = (StretchModeEnum)5,
				MouseFilter = (MouseFilterEnum)0,
				AnchorRight = 1f,
				AnchorBottom = 1f,
				GrowHorizontal = (GrowDirection)2,
				GrowVertical = (GrowDirection)2
			};
			Control val3 = new Control
			{
				Name = StringName.op_Implicit($"PortableTarotSlot{i}"),
				CustomMinimumSize = new Vector2(num3, 152f),
				MouseFilter = (MouseFilterEnum)2
			};
			NCardRareGlow val4 = NCardRareGlow.Create();
			if (val4 != null)
			{
				((Node)val4).Name = StringName.op_Implicit("RareGlow");
				((Node2D)val4).Position = new Vector2(num3 * 0.5f, 76f);
				((Node2D)val4).Scale = Vector2.One * (19f / 60f);
				((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
			}
			((Node)val3).AddChild((Node)(object)card, false, (InternalMode)0);
			((Control)card).MouseEntered += delegate
			{
				ShowPortableCardHoverTips((Control)(object)card, model);
			};
			((Control)card).MouseExited += delegate
			{
				NHoverTipSet.Remove((Control)(object)card);
			};
			((Node)card).TreeExiting += delegate
			{
				NHoverTipSet.Remove((Control)(object)card);
			};
			int slotIndex = i;
			((Control)card).GuiInput += (GuiInputEventHandler)delegate(InputEvent inputEvent)
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Invalid comparison between Unknown and I8
				InputEventMouseButton val5 = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
				if (val5 != null && (long)val5.ButtonIndex == 1 && !val5.Pressed)
				{
					OpenPortablePopup((Control)(object)card, model, slotIndex);
				}
			};
			((Node)_portableCardsRow).AddChild((Node)(object)val3, false, (InternalMode)0);
			_portableCards.Add(card);
			num2 += num3;
		}
		_portableCount.Text = $"{num}/{4}";
		if (num > 1)
		{
			num2 += 4f * (float)(num - 1);
		}
		_portableShelf.CustomMinimumSize = new Vector2(num2 + 8f, _root.Size.Y);
		((CanvasItem)_portableShelf).Visible = num > 0;
		_portableAvailabilityMask = null;
		UpdatePortableShelfAvailability();
		if (num > 0 && _isDocked && _dockSide != 0)
		{
			_root.GlobalPosition = (_dockedHovered ? GetPeekPosition(_dockSide) : GetDockPosition(_dockSide));
			ApplyPortableShelfDockState(_dockedHovered, animate: false);
		}
		else
		{
			UpdatePortableShelfPlacement();
		}
	}

	private void UpdatePortableShelfAvailability()
	{
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		if (_portableShelf == null || !((CanvasItem)_portableShelf).Visible || !RunManager.Instance.IsInProgress)
		{
			return;
		}
		RunState val = RunManager.Instance.DebugOnlyGetState();
		Player val2 = ((val == null) ? null : LocalContext.GetMe((IPlayerCollection)(object)val));
		IReadOnlyList<string> readOnlyList;
		if (val2 != null)
		{
			readOnlyList = JudgementTarotInventoryState.GetTarotIds(val2.NetId);
		}
		else
		{
			IReadOnlyList<string> readOnlyList2 = Array.Empty<string>();
			readOnlyList = readOnlyList2;
		}
		IReadOnlyList<string> tarotIds = readOnlyList;
		bool flag = ((val2 != null) ? val2.RunState.CurrentRoom : null) is RestSiteRoom;
		int num = (flag ? 65536 : 0);
		int num2 = ((val2 != null) ? PortableTarotUseRouter.GetUsableMask(val2, tarotIds) : 0);
		num |= num2;
		bool flag2 = num2 != 0;
		if (_portableAvailabilityMask == num)
		{
			return;
		}
		_portableAvailabilityMask = num;
		Color val3 = (Color)(flag2 ? Colors.White : new Color(0.6f, 0.6f, 0.6f, 0.6f));
		((CanvasItem)_portableShelf).SelfModulate = val3;
		if (_portableShelfContent != null)
		{
			((CanvasItem)_portableShelfContent).Modulate = val3;
		}
		for (int i = 0; i < _portableCards.Count; i++)
		{
			bool flag3 = (num & (1 << i)) != 0;
			Node parent = ((Node)_portableCards[i]).GetParent();
			Control val4 = (Control)(object)((parent is Control) ? parent : null);
			if (val4 != null)
			{
				((CanvasItem)val4).Modulate = (Color)((flag2 && !flag3) ? new Color(0.6f, 0.6f, 0.6f, 0.6f) : Colors.White);
				NCardRareGlow nodeOrNull = ((Node)val4).GetNodeOrNull<NCardRareGlow>(NodePath.op_Implicit("RareGlow"));
				if (nodeOrNull != null)
				{
					((CanvasItem)nodeOrNull).Visible = flag;
				}
			}
		}
		ClosePortablePopup(immediate: true);
	}

	private void UpdateEntryLayerForContext()
	{
		if (_root == null || _portableShelfLayer == null)
		{
			return;
		}
		NRun instance = NRun.Instance;
		NGlobalUi val = ((instance != null) ? instance.GlobalUi : null);
		if (val == null)
		{
			return;
		}
		NCapstoneContainer capstoneContainer = val.CapstoneContainer;
		if (((Node)_root).GetParent() == capstoneContainer && ((Node)_root).GetIndex(false) != ((Node)capstoneContainer).GetChildCount(false) - 1)
		{
			((Node)capstoneContainer).MoveChild((Node)(object)_root, ((Node)capstoneContainer).GetChildCount(false) - 1);
		}
		int num;
		if (capstoneContainer.CurrentCapstoneScreen is NDeckViewScreen && !CombatManager.Instance.IsInProgress)
		{
			RunState val2 = RunManager.Instance.DebugOnlyGetState();
			if (val2 != null)
			{
				Player me = LocalContext.GetMe((IPlayerCollection)(object)val2);
				num = ((((me != null) ? me.RunState.CurrentRoom : null) is RestSiteRoom) ? 1 : 0);
				goto IL_00b0;
			}
		}
		num = 0;
		goto IL_00b0;
		IL_00b0:
		bool flag = (byte)num != 0;
		if (_isAboveRestSiteDeck == flag)
		{
			return;
		}
		_isAboveRestSiteDeck = flag;
		if (flag)
		{
			if (((Node)_portableShelfLayer).GetParent() != capstoneContainer)
			{
				((Node)_portableShelfLayer).Reparent((Node)(object)capstoneContainer, true);
			}
			((Node)capstoneContainer).MoveChild((Node)(object)_portableShelfLayer, ((Node)capstoneContainer).GetChildCount(false) - 1);
		}
		else
		{
			if (((Node)_portableShelfLayer).GetParent() != val)
			{
				((Node)_portableShelfLayer).Reparent((Node)(object)val, true);
			}
			int index = ((Node)val.MapScreen).GetIndex(false);
			if (((Node)_portableShelfLayer).GetIndex(false) != index)
			{
				((Node)val).MoveChild((Node)(object)_portableShelfLayer, index);
			}
			ClosePortablePopup(immediate: true);
		}
		SyncPortableShelfLayerTransform();
	}

	private void SyncPortableShelfLayerTransform()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (_root != null && _portableShelfLayer != null && ((Node)_portableShelfLayer).GetParent() != _root)
		{
			_portableShelfLayer.GlobalPosition = _root.GlobalPosition;
			_portableShelfLayer.Scale = _root.Scale;
			_portableShelfLayer.Size = _root.Size;
		}
	}

	private static void ShowPortableCardHoverTips(Control owner, CardModel model)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		NHoverTipSet.Remove(owner);
		List<IHoverTip> list = new List<IHoverTip> { HoverTipFactory.FromCard(model, false) };
		list.AddRange(model.HoverTips);
		NHoverTipSet obj = NHoverTipSet.CreateAndShow(owner, (IEnumerable<IHoverTip>)list, (HoverTipAlignment)0);
		if (obj != null)
		{
			obj.SetAlignment(owner, HoverTip.GetHoverTipAlignment(owner, 0.75f));
		}
	}

	private void OpenPortablePopup(Control card, CardModel model, int slotIndex)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		ClosePortablePopup(immediate: true);
		if (!RunManager.Instance.IsInProgress)
		{
			return;
		}
		Player me = LocalContext.GetMe((IPlayerCollection)(object)RunManager.Instance.DebugOnlyGetState());
		if (me == null)
		{
			return;
		}
		PackedScene obj = GD.Load<PackedScene>("res://configFW/Scenes/portable_tarot_popup.tscn");
		NPortableTarotPopup nPortableTarotPopup = ((obj != null) ? obj.Instantiate<NPortableTarotPopup>((GenEditState)0) : null);
		if (nPortableTarotPopup != null)
		{
			((Node)(_portableShelfLayer ?? _root)).AddChild((Node)(object)nPortableTarotPopup, false, (InternalMode)0);
			IReadOnlyList<string> tarotIds = JudgementTarotInventoryState.GetTarotIds(me.NetId);
			if (slotIndex < 0 || slotIndex >= tarotIds.Count)
			{
				((Node)nPortableTarotPopup).QueueFree();
				return;
			}
			nPortableTarotPopup.Bind(me, slotIndex, tarotIds[slotIndex], card.GlobalPosition, card.Size);
			_portablePopup = nPortableTarotPopup;
		}
	}

	private void ClosePortablePopup(bool immediate)
	{
		if (_portablePopup != null && GodotObject.IsInstanceValid((GodotObject)(object)_portablePopup))
		{
			_portablePopup.Remove(immediate);
		}
		_portablePopup = null;
	}

	private void UpdatePortableShelfPlacement()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (_portableShelf != null && _root != null && ((CanvasItem)_portableShelf).Visible)
		{
			float x = _portableShelf.CustomMinimumSize.X;
			_portableShelf.Size = new Vector2(x, _root.Size.Y);
			_portableShelf.Position = GetPortableShelfBasePosition();
		}
	}

	private Vector2 GetPortableShelfBasePosition()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (_portableShelf == null || _root == null)
		{
			return Vector2.Zero;
		}
		float num = _root.GlobalPosition.X + _root.Size.X * 0.5f;
		Rect2 viewportRect = ((CanvasItem)this).GetViewportRect();
		bool num2 = num >= ((Rect2)(ref viewportRect)).Size.X * 0.5f;
		float x = _portableShelf.CustomMinimumSize.X;
		return new Vector2(num2 ? (0f - x + 16f) : (_root.Size.X - 16f), 0f);
	}

	private void PlayShelfFullFeedback()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		if (_portableShelf != null && ((CanvasItem)_portableShelf).Visible)
		{
			Tween? portableFeedbackTween = _portableFeedbackTween;
			if (portableFeedbackTween != null)
			{
				portableFeedbackTween.Kill();
			}
			_portableShelf.PivotOffset = _portableShelf.Size / 2f;
			_portableFeedbackTween = ((Node)this).CreateTween();
			_portableFeedbackTween.TweenProperty((GodotObject)(object)_portableShelf, NodePath.op_Implicit("modulate"), Variant.op_Implicit(new Color(1f, 0.2f, 0.2f, 1f)), 0.05999999865889549);
			float[] array = new float[5] { -9f, 9f, -6f, 6f, 0f };
			foreach (float num in array)
			{
				_portableFeedbackTween.TweenProperty((GodotObject)(object)_portableShelf, NodePath.op_Implicit("position:x"), Variant.op_Implicit(_portableShelf.Position.X + num), 0.03500000014901161);
			}
			_portableFeedbackTween.TweenProperty((GodotObject)(object)_portableShelf, NodePath.op_Implicit("modulate"), Variant.op_Implicit(Colors.White), 0.18000000715255737);
			_portableFeedbackTween.TweenCallback(Callable.From((Action)RestorePortableShelfState));
		}
	}

	private void RestorePortableShelfState()
	{
		if (_isDocked && _dockSide != 0)
		{
			ApplyPortableShelfDockState(_dockedHovered, animate: false);
		}
		else
		{
			UpdatePortableShelfPlacement();
		}
	}

	private void BeginDrag()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (((CanvasItem)this).IsVisibleInTree())
		{
			ClosePortablePopup(immediate: true);
			Tween? dockTween = _dockTween;
			if (dockTween != null)
			{
				dockTween.Kill();
			}
			Tween? hoverTween = _hoverTween;
			if (hoverTween != null)
			{
				hoverTween.Kill();
			}
			_isDocked = false;
			_dockSide = DockSide.None;
			_dragging = true;
			((CanvasItem)this).Modulate = Colors.White;
			((Control)this).Scale = Vector2.One;
			CancelAutoHide();
			_dragOffset = ((CanvasItem)this).GetGlobalMousePosition() - RootGlobalPos();
			NMapPointMarkerIconPatch.SetOverview(on: true);
			if (_portableShelf != null)
			{
				((CanvasItem)_portableShelf).Visible = _portableCards.Count > 0;
				UpdatePortableShelfPlacement();
			}
		}
	}

	private Vector2 RootGlobalPos()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (_root == null)
		{
			return ((Control)this).GlobalPosition;
		}
		return _root.GlobalPosition;
	}

	private void DragTo(Vector2 target)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (_root != null)
		{
			Rect2 viewportRect = ((CanvasItem)this).GetViewportRect();
			Vector2 size = ((Rect2)(ref viewportRect)).Size;
			Vector2 size2 = _root.Size;
			target.X = Mathf.Clamp(target.X, 0f, Mathf.Max(0f, size.X - size2.X));
			target.Y = Mathf.Clamp(target.Y, 0f, Mathf.Max(0f, size.Y - size2.Y));
			_root.GlobalPosition = target;
			UpdatePortableShelfPlacement();
		}
	}

	private DockSide FindNearestSide(out float minDist)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Rect2 viewportRect = ((CanvasItem)this).GetViewportRect();
		Vector2 size = ((Rect2)(ref viewportRect)).Size;
		Vector2 globalPosition = _root.GlobalPosition;
		Vector2 size2 = _root.Size;
		float x = globalPosition.X;
		float num = size.X - (globalPosition.X + size2.X);
		float y = globalPosition.Y;
		float num2 = size.Y - (globalPosition.Y + size2.Y);
		if (x <= num && x <= y && x <= num2)
		{
			minDist = x;
			return DockSide.Left;
		}
		if (num <= y && num <= num2)
		{
			minDist = num;
			return DockSide.Right;
		}
		if (y <= num2)
		{
			minDist = y;
			return DockSide.Top;
		}
		minDist = num2;
		return DockSide.Bottom;
	}

	private void TrySnapAndDock()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (_root != null)
		{
			Vector2 size = _root.Size;
			float minDist;
			DockSide dockSide = FindNearestSide(out minDist);
			bool flag = (uint)(dockSide - 1) <= 1u;
			float num = (flag ? (size.X * 0.5f) : (size.Y * 0.5f));
			if (minDist >= num)
			{
				_isDocked = false;
				_dockSide = DockSide.None;
			}
			else
			{
				_dockSide = dockSide;
				_isDocked = true;
				DockIn();
			}
		}
	}

	private Vector2 GetDockPosition(DockSide side)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		Rect2 viewportRect = ((CanvasItem)this).GetViewportRect();
		Vector2 size = ((Rect2)(ref viewportRect)).Size;
		Vector2 size2 = _root.Size;
		return (Vector2)(side switch
		{
			DockSide.Left => new Vector2((0f - size2.X) * 0.7f, _root.GlobalPosition.Y), 
			DockSide.Right => new Vector2(size.X - size2.X * 0.3f, _root.GlobalPosition.Y), 
			DockSide.Top => new Vector2(_root.GlobalPosition.X, (_portableCards.Count > 0) ? (0f - size2.Y) : ((0f - size2.Y) * 0.7f)), 
			DockSide.Bottom => new Vector2(_root.GlobalPosition.X, (_portableCards.Count > 0) ? size.Y : (size.Y - size2.Y * 0.3f)), 
			_ => _root.GlobalPosition, 
		});
	}

	private Vector2 GetPeekPosition(DockSide side)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		Rect2 viewportRect = ((CanvasItem)this).GetViewportRect();
		Vector2 size = ((Rect2)(ref viewportRect)).Size;
		Vector2 size2 = _root.Size;
		return (Vector2)(side switch
		{
			DockSide.Left => new Vector2(0f, _root.GlobalPosition.Y), 
			DockSide.Right => new Vector2(size.X - size2.X, _root.GlobalPosition.Y), 
			DockSide.Top => new Vector2(_root.GlobalPosition.X, 0f), 
			DockSide.Bottom => new Vector2(_root.GlobalPosition.X, size.Y - size2.Y), 
			_ => _root.GlobalPosition, 
		});
	}

	private Rect2 GetDockHoverRect()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return new Rect2(GetPeekPosition(_dockSide), _root.Size);
	}

	private void DockIn()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (_root != null && _dockSide != 0)
		{
			NMapPointMarkerIconPatch.SetOverview(on: false);
			AnimateDock(GetDockPosition(_dockSide), 0.2f, 0.97f, 0.35f, (TransitionType)7, (EaseType)1);
			AnimatePortableShelfForDock(peeking: false);
		}
	}

	private void PeekOut()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (_root != null && _dockSide != 0)
		{
			AnimateDock(GetPeekPosition(_dockSide), 1f, 1.08f, 0.3f, (TransitionType)10, (EaseType)1);
			AnimatePortableShelfForDock(peeking: true);
		}
	}

	private void AnimatePortableShelfForDock(bool peeking)
	{
		ApplyPortableShelfDockState(peeking, animate: true);
	}

	private void ApplyPortableShelfDockState(bool peeking, bool animate)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (_portableShelf == null || _portableCards.Count == 0)
		{
			return;
		}
		Tween? portableDockTween = _portableDockTween;
		if (portableDockTween != null)
		{
			portableDockTween.Kill();
		}
		DockSide dockSide = _dockSide;
		if ((uint)(dockSide - 3) <= 1u)
		{
			((CanvasItem)_portableShelf).Visible = peeking;
			return;
		}
		((CanvasItem)_portableShelf).Visible = true;
		float x = _portableShelf.CustomMinimumSize.X;
		_portableShelf.Size = new Vector2(x, _root.Size.Y);
		Vector2 portableShelfBasePosition = GetPortableShelfBasePosition();
		float num = _portableShelf.Size.X + 18f;
		Vector2 val = (peeking ? portableShelfBasePosition : (portableShelfBasePosition + new Vector2((_dockSide == DockSide.Left) ? num : (0f - num), 0f)));
		if (!animate)
		{
			_portableShelf.Position = val;
			return;
		}
		_portableDockTween = ((Node)this).CreateTween();
		_portableDockTween.TweenProperty((GodotObject)(object)_portableShelf, NodePath.op_Implicit("position"), Variant.op_Implicit(val), 0.25).SetTrans((TransitionType)7).SetEase((EaseType)1);
	}

	private void AnimateDock(Vector2 targetPos, float targetAlpha, float peakScale, float duration, TransitionType trans, EaseType ease)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		Tween? dockTween = _dockTween;
		if (dockTween != null)
		{
			dockTween.Kill();
		}
		Tween? hoverTween = _hoverTween;
		if (hoverTween != null)
		{
			hoverTween.Kill();
		}
		((Control)this).PivotOffset = ((Control)this).Size / 2f;
		_dockTween = ((Node)this).CreateTween().SetParallel(true);
		_dockTween.TweenProperty((GodotObject)(object)_root, NodePath.op_Implicit("global_position"), Variant.op_Implicit(targetPos), (double)duration).SetTrans(trans).SetEase(ease);
		_dockTween.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(targetAlpha), (double)duration).SetTrans((TransitionType)4).SetEase((EaseType)1);
		_dockTween.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("scale"), Variant.op_Implicit(Vector2.One), (double)duration).SetTrans((TransitionType)10).SetEase((EaseType)1)
			.From(Variant.op_Implicit(Vector2.One * peakScale));
		_dockTween.Finished += SaveState;
	}

	private void SaveState()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (_root != null)
		{
			_savedPos = _root.GlobalPosition;
			_savedDockSide = _dockSide;
			_savedDocked = _isDocked;
		}
	}

	public void ApplySavedState()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (_root == null)
		{
			return;
		}
		Vector2? savedPos = _savedPos;
		if (savedPos.HasValue)
		{
			Vector2 valueOrDefault = savedPos.GetValueOrDefault();
			Tween? dockTween = _dockTween;
			if (dockTween != null)
			{
				dockTween.Kill();
			}
			Tween? hoverTween = _hoverTween;
			if (hoverTween != null)
			{
				hoverTween.Kill();
			}
			_root.GlobalPosition = valueOrDefault;
			if (_savedDocked && _savedDockSide != 0)
			{
				_dockSide = _savedDockSide;
				_isDocked = true;
				((CanvasItem)this).Modulate = new Color(1f, 1f, 1f, 0.2f);
			}
			else
			{
				_dockSide = DockSide.None;
				_isDocked = false;
				((CanvasItem)this).Modulate = Colors.White;
			}
			((Control)this).Scale = Vector2.One;
		}
	}

	private void ScaleTo(float scale)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).PivotOffset = ((Control)this).Size / 2f;
		Tween? hoverTween = _hoverTween;
		if (hoverTween != null)
		{
			hoverTween.Kill();
		}
		_hoverTween = ((Node)this).CreateTween();
		_hoverTween.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("scale"), Variant.op_Implicit(Vector2.One * scale), 0.10000000149011612).SetTrans((TransitionType)4).SetEase((EaseType)1);
	}

	private void RestartAutoHide()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (_root != null)
		{
			Tween? autoHideTween = _autoHideTween;
			if (autoHideTween != null)
			{
				autoHideTween.Kill();
			}
			_autoHideTween = ((Node)this).CreateTween();
			_autoHideTween.TweenCallback(Callable.From((Action)OnAutoHideTimer)).SetDelay(8.0);
		}
	}

	private void CancelAutoHide()
	{
		Tween? autoHideTween = _autoHideTween;
		if (autoHideTween != null)
		{
			autoHideTween.Kill();
		}
	}

	private void OnAutoHideTimer()
	{
		if (_root != null && !_isDocked)
		{
			TrySnapAndDock();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Expected O, but got Unknown
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0518: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		//IL_059b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05be: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_061e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0627: Unknown result type (might be due to invalid IL or missing references)
		//IL_064d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		//IL_067c: Unknown result type (might be due to invalid IL or missing references)
		//IL_069f: Unknown result type (might be due to invalid IL or missing references)
		//IL_06aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0714: Unknown result type (might be due to invalid IL or missing references)
		//IL_071f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0745: Unknown result type (might be due to invalid IL or missing references)
		//IL_0768: Unknown result type (might be due to invalid IL or missing references)
		//IL_0789: Unknown result type (might be due to invalid IL or missing references)
		//IL_07aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_080d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0818: Unknown result type (might be due to invalid IL or missing references)
		//IL_083e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0847: Unknown result type (might be due to invalid IL or missing references)
		//IL_086d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0876: Unknown result type (might be due to invalid IL or missing references)
		//IL_089c: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_091f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0928: Unknown result type (might be due to invalid IL or missing references)
		//IL_094e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0957: Unknown result type (might be due to invalid IL or missing references)
		return new List<MethodInfo>(37)
		{
			new MethodInfo(MethodName.PlayPortableTarotFullFeedback, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.PreparePortableTarotDiscardAnimation, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("slotIndex"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.SetRunEntryVisible, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)1, StringName.op_Implicit("visible"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName._Ready, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnMouseEntered, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnMouseExited, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName._Input, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("inputEvent"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("InputEvent"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName._Process, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)3, StringName.op_Implicit("delta"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.CreatePortableShelfAndRefresh, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.CreatePortableShelf, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.UpdatePortableShelfAvailability, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.UpdateEntryLayerForContext, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.SyncPortableShelfLayerTransform, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.ClosePortablePopup, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)1, StringName.op_Implicit("immediate"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.UpdatePortableShelfPlacement, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.GetPortableShelfBasePosition, new PropertyInfo((Type)5, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.PlayShelfFullFeedback, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.RestorePortableShelfState, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.BeginDrag, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.RootGlobalPos, new PropertyInfo((Type)5, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.DragTo, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)5, StringName.op_Implicit("target"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.TrySnapAndDock, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.GetDockPosition, new PropertyInfo((Type)5, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("side"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.GetPeekPosition, new PropertyInfo((Type)5, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("side"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.GetDockHoverRect, new PropertyInfo((Type)7, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.DockIn, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.PeekOut, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.AnimatePortableShelfForDock, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)1, StringName.op_Implicit("peeking"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.ApplyPortableShelfDockState, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)1, StringName.op_Implicit("peeking"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)1, StringName.op_Implicit("animate"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.AnimateDock, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)5, StringName.op_Implicit("targetPos"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)3, StringName.op_Implicit("targetAlpha"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)3, StringName.op_Implicit("peakScale"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)3, StringName.op_Implicit("duration"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)2, StringName.op_Implicit("trans"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)2, StringName.op_Implicit("ease"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.SaveState, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.ApplySavedState, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.ScaleTo, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)3, StringName.op_Implicit("scale"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.RestartAutoHide, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.CancelAutoHide, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnAutoHideTimer, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0471: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_0565: Unknown result type (might be due to invalid IL or missing references)
		//IL_0572: Unknown result type (might be due to invalid IL or missing references)
		//IL_057d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_061f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0673: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_0669: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.PlayPortableTarotFullFeedback && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			PlayPortableTarotFullFeedback();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.PreparePortableTarotDiscardAnimation && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			PreparePortableTarotDiscardAnimation(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SetRunEntryVisible && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			SetRunEntryVisible(VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName._Ready && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((Node)this)._Ready();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnMouseEntered && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnMouseEntered();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnMouseExited && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnMouseExited();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName._Input && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			((Node)this)._Input(VariantUtils.ConvertTo<InputEvent>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName._Process && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			((Node)this)._Process(VariantUtils.ConvertTo<double>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName._ExitTree && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((Node)this)._ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.CreatePortableShelfAndRefresh && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			CreatePortableShelfAndRefresh();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.CreatePortableShelf && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			CreatePortableShelf();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.UpdatePortableShelfAvailability && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			UpdatePortableShelfAvailability();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.UpdateEntryLayerForContext && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			UpdateEntryLayerForContext();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SyncPortableShelfLayerTransform && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			SyncPortableShelfLayerTransform();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ClosePortablePopup && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			ClosePortablePopup(VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.UpdatePortableShelfPlacement && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			UpdatePortableShelfPlacement();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.GetPortableShelfBasePosition && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Vector2 portableShelfBasePosition = GetPortableShelfBasePosition();
			ret = VariantUtils.CreateFrom<Vector2>(ref portableShelfBasePosition);
			return true;
		}
		if ((ref method) == MethodName.PlayShelfFullFeedback && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			PlayShelfFullFeedback();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.RestorePortableShelfState && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			RestorePortableShelfState();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.BeginDrag && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			BeginDrag();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.RootGlobalPos && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Vector2 val = RootGlobalPos();
			ret = VariantUtils.CreateFrom<Vector2>(ref val);
			return true;
		}
		if ((ref method) == MethodName.DragTo && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			DragTo(VariantUtils.ConvertTo<Vector2>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.TrySnapAndDock && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			TrySnapAndDock();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.GetDockPosition && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Vector2 dockPosition = GetDockPosition(VariantUtils.ConvertTo<DockSide>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<Vector2>(ref dockPosition);
			return true;
		}
		if ((ref method) == MethodName.GetPeekPosition && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Vector2 peekPosition = GetPeekPosition(VariantUtils.ConvertTo<DockSide>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<Vector2>(ref peekPosition);
			return true;
		}
		if ((ref method) == MethodName.GetDockHoverRect && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Rect2 dockHoverRect = GetDockHoverRect();
			ret = VariantUtils.CreateFrom<Rect2>(ref dockHoverRect);
			return true;
		}
		if ((ref method) == MethodName.DockIn && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			DockIn();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.PeekOut && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			PeekOut();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.AnimatePortableShelfForDock && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			AnimatePortableShelfForDock(VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ApplyPortableShelfDockState && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			ApplyPortableShelfDockState(VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.AnimateDock && ((NativeVariantPtrArgs)(ref args)).Count == 6)
		{
			AnimateDock(VariantUtils.ConvertTo<Vector2>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<float>(ref ((NativeVariantPtrArgs)(ref args))[1]), VariantUtils.ConvertTo<float>(ref ((NativeVariantPtrArgs)(ref args))[2]), VariantUtils.ConvertTo<float>(ref ((NativeVariantPtrArgs)(ref args))[3]), VariantUtils.ConvertTo<TransitionType>(ref ((NativeVariantPtrArgs)(ref args))[4]), VariantUtils.ConvertTo<EaseType>(ref ((NativeVariantPtrArgs)(ref args))[5]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SaveState && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			SaveState();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ApplySavedState && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ApplySavedState();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ScaleTo && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			ScaleTo(VariantUtils.ConvertTo<float>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.RestartAutoHide && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			RestartAutoHide();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.CancelAutoHide && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			CancelAutoHide();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnAutoHideTimer && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnAutoHideTimer();
			ret = default(godot_variant);
			return true;
		}
		return ((NButton)this).InvokeGodotClassMethod(ref method, args, ref ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.PlayPortableTarotFullFeedback && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			PlayPortableTarotFullFeedback();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.PreparePortableTarotDiscardAnimation && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			PreparePortableTarotDiscardAnimation(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if ((ref method) == MethodName.PlayPortableTarotFullFeedback)
		{
			return true;
		}
		if ((ref method) == MethodName.PreparePortableTarotDiscardAnimation)
		{
			return true;
		}
		if ((ref method) == MethodName.SetRunEntryVisible)
		{
			return true;
		}
		if ((ref method) == MethodName._Ready)
		{
			return true;
		}
		if ((ref method) == MethodName.OnMouseEntered)
		{
			return true;
		}
		if ((ref method) == MethodName.OnMouseExited)
		{
			return true;
		}
		if ((ref method) == MethodName._Input)
		{
			return true;
		}
		if ((ref method) == MethodName._Process)
		{
			return true;
		}
		if ((ref method) == MethodName._ExitTree)
		{
			return true;
		}
		if ((ref method) == MethodName.CreatePortableShelfAndRefresh)
		{
			return true;
		}
		if ((ref method) == MethodName.CreatePortableShelf)
		{
			return true;
		}
		if ((ref method) == MethodName.UpdatePortableShelfAvailability)
		{
			return true;
		}
		if ((ref method) == MethodName.UpdateEntryLayerForContext)
		{
			return true;
		}
		if ((ref method) == MethodName.SyncPortableShelfLayerTransform)
		{
			return true;
		}
		if ((ref method) == MethodName.ClosePortablePopup)
		{
			return true;
		}
		if ((ref method) == MethodName.UpdatePortableShelfPlacement)
		{
			return true;
		}
		if ((ref method) == MethodName.GetPortableShelfBasePosition)
		{
			return true;
		}
		if ((ref method) == MethodName.PlayShelfFullFeedback)
		{
			return true;
		}
		if ((ref method) == MethodName.RestorePortableShelfState)
		{
			return true;
		}
		if ((ref method) == MethodName.BeginDrag)
		{
			return true;
		}
		if ((ref method) == MethodName.RootGlobalPos)
		{
			return true;
		}
		if ((ref method) == MethodName.DragTo)
		{
			return true;
		}
		if ((ref method) == MethodName.TrySnapAndDock)
		{
			return true;
		}
		if ((ref method) == MethodName.GetDockPosition)
		{
			return true;
		}
		if ((ref method) == MethodName.GetPeekPosition)
		{
			return true;
		}
		if ((ref method) == MethodName.GetDockHoverRect)
		{
			return true;
		}
		if ((ref method) == MethodName.DockIn)
		{
			return true;
		}
		if ((ref method) == MethodName.PeekOut)
		{
			return true;
		}
		if ((ref method) == MethodName.AnimatePortableShelfForDock)
		{
			return true;
		}
		if ((ref method) == MethodName.ApplyPortableShelfDockState)
		{
			return true;
		}
		if ((ref method) == MethodName.AnimateDock)
		{
			return true;
		}
		if ((ref method) == MethodName.SaveState)
		{
			return true;
		}
		if ((ref method) == MethodName.ApplySavedState)
		{
			return true;
		}
		if ((ref method) == MethodName.ScaleTo)
		{
			return true;
		}
		if ((ref method) == MethodName.RestartAutoHide)
		{
			return true;
		}
		if ((ref method) == MethodName.CancelAutoHide)
		{
			return true;
		}
		if ((ref method) == MethodName.OnAutoHideTimer)
		{
			return true;
		}
		return ((NButton)this).HasGodotClassMethod(ref method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if ((ref name) == PropertyName._root)
		{
			_root = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._dragging)
		{
			_dragging = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._entryVisible)
		{
			_entryVisible = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._dragOffset)
		{
			_dragOffset = VariantUtils.ConvertTo<Vector2>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._dockSide)
		{
			_dockSide = VariantUtils.ConvertTo<DockSide>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._isDocked)
		{
			_isDocked = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._dockedHovered)
		{
			_dockedHovered = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._dockTween)
		{
			_dockTween = VariantUtils.ConvertTo<Tween>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._autoHideTween)
		{
			_autoHideTween = VariantUtils.ConvertTo<Tween>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._portableShelf)
		{
			_portableShelf = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._portableShelfLayer)
		{
			_portableShelfLayer = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._portableShelfContent)
		{
			_portableShelfContent = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._portableCardsRow)
		{
			_portableCardsRow = VariantUtils.ConvertTo<HBoxContainer>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._portableCount)
		{
			_portableCount = VariantUtils.ConvertTo<Label>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._portableFeedbackTween)
		{
			_portableFeedbackTween = VariantUtils.ConvertTo<Tween>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._portableDockTween)
		{
			_portableDockTween = VariantUtils.ConvertTo<Tween>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._portablePopup)
		{
			_portablePopup = VariantUtils.ConvertTo<NPortableTarotPopup>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._portableAvailabilityRefreshElapsed)
		{
			_portableAvailabilityRefreshElapsed = VariantUtils.ConvertTo<double>(ref value);
			return true;
		}
		return ((NButton)this).SetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		if ((ref name) == PropertyName._root)
		{
			value = VariantUtils.CreateFrom<Control>(ref _root);
			return true;
		}
		if ((ref name) == PropertyName._dragging)
		{
			value = VariantUtils.CreateFrom<bool>(ref _dragging);
			return true;
		}
		if ((ref name) == PropertyName._entryVisible)
		{
			value = VariantUtils.CreateFrom<bool>(ref _entryVisible);
			return true;
		}
		if ((ref name) == PropertyName._dragOffset)
		{
			value = VariantUtils.CreateFrom<Vector2>(ref _dragOffset);
			return true;
		}
		if ((ref name) == PropertyName._dockSide)
		{
			value = VariantUtils.CreateFrom<DockSide>(ref _dockSide);
			return true;
		}
		if ((ref name) == PropertyName._isDocked)
		{
			value = VariantUtils.CreateFrom<bool>(ref _isDocked);
			return true;
		}
		if ((ref name) == PropertyName._dockedHovered)
		{
			value = VariantUtils.CreateFrom<bool>(ref _dockedHovered);
			return true;
		}
		if ((ref name) == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom<Tween>(ref _hoverTween);
			return true;
		}
		if ((ref name) == PropertyName._dockTween)
		{
			value = VariantUtils.CreateFrom<Tween>(ref _dockTween);
			return true;
		}
		if ((ref name) == PropertyName._autoHideTween)
		{
			value = VariantUtils.CreateFrom<Tween>(ref _autoHideTween);
			return true;
		}
		if ((ref name) == PropertyName._portableShelf)
		{
			value = VariantUtils.CreateFrom<Control>(ref _portableShelf);
			return true;
		}
		if ((ref name) == PropertyName._portableShelfLayer)
		{
			value = VariantUtils.CreateFrom<Control>(ref _portableShelfLayer);
			return true;
		}
		if ((ref name) == PropertyName._portableShelfContent)
		{
			value = VariantUtils.CreateFrom<Control>(ref _portableShelfContent);
			return true;
		}
		if ((ref name) == PropertyName._portableCardsRow)
		{
			value = VariantUtils.CreateFrom<HBoxContainer>(ref _portableCardsRow);
			return true;
		}
		if ((ref name) == PropertyName._portableCount)
		{
			value = VariantUtils.CreateFrom<Label>(ref _portableCount);
			return true;
		}
		if ((ref name) == PropertyName._portableFeedbackTween)
		{
			value = VariantUtils.CreateFrom<Tween>(ref _portableFeedbackTween);
			return true;
		}
		if ((ref name) == PropertyName._portableDockTween)
		{
			value = VariantUtils.CreateFrom<Tween>(ref _portableDockTween);
			return true;
		}
		if ((ref name) == PropertyName._portablePopup)
		{
			value = VariantUtils.CreateFrom<NPortableTarotPopup>(ref _portablePopup);
			return true;
		}
		if ((ref name) == PropertyName._portableAvailabilityRefreshElapsed)
		{
			value = VariantUtils.CreateFrom<double>(ref _portableAvailabilityRefreshElapsed);
			return true;
		}
		return ((NButton)this).GetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		return new List<PropertyInfo>
		{
			new PropertyInfo((Type)24, PropertyName._root, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._dragging, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._entryVisible, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)5, PropertyName._dragOffset, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)2, PropertyName._dockSide, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._isDocked, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._dockedHovered, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._hoverTween, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._dockTween, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._autoHideTween, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._portableShelf, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._portableShelfLayer, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._portableShelfContent, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._portableCardsRow, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._portableCount, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._portableFeedbackTween, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._portableDockTween, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._portablePopup, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)3, PropertyName._portableAvailabilityRefreshElapsed, (PropertyHint)0, "", (PropertyUsageFlags)4096, false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		((NButton)this).SaveGodotObjectData(info);
		info.AddProperty(PropertyName._root, Variant.From<Control>(ref _root));
		info.AddProperty(PropertyName._dragging, Variant.From<bool>(ref _dragging));
		info.AddProperty(PropertyName._entryVisible, Variant.From<bool>(ref _entryVisible));
		info.AddProperty(PropertyName._dragOffset, Variant.From<Vector2>(ref _dragOffset));
		info.AddProperty(PropertyName._dockSide, Variant.From<DockSide>(ref _dockSide));
		info.AddProperty(PropertyName._isDocked, Variant.From<bool>(ref _isDocked));
		info.AddProperty(PropertyName._dockedHovered, Variant.From<bool>(ref _dockedHovered));
		info.AddProperty(PropertyName._hoverTween, Variant.From<Tween>(ref _hoverTween));
		info.AddProperty(PropertyName._dockTween, Variant.From<Tween>(ref _dockTween));
		info.AddProperty(PropertyName._autoHideTween, Variant.From<Tween>(ref _autoHideTween));
		info.AddProperty(PropertyName._portableShelf, Variant.From<Control>(ref _portableShelf));
		info.AddProperty(PropertyName._portableShelfLayer, Variant.From<Control>(ref _portableShelfLayer));
		info.AddProperty(PropertyName._portableShelfContent, Variant.From<Control>(ref _portableShelfContent));
		info.AddProperty(PropertyName._portableCardsRow, Variant.From<HBoxContainer>(ref _portableCardsRow));
		info.AddProperty(PropertyName._portableCount, Variant.From<Label>(ref _portableCount));
		info.AddProperty(PropertyName._portableFeedbackTween, Variant.From<Tween>(ref _portableFeedbackTween));
		info.AddProperty(PropertyName._portableDockTween, Variant.From<Tween>(ref _portableDockTween));
		info.AddProperty(PropertyName._portablePopup, Variant.From<NPortableTarotPopup>(ref _portablePopup));
		info.AddProperty(PropertyName._portableAvailabilityRefreshElapsed, Variant.From<double>(ref _portableAvailabilityRefreshElapsed));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		((NButton)this).RestoreGodotObjectData(info);
		Variant val = default(Variant);
		if (info.TryGetProperty(PropertyName._root, ref val))
		{
			_root = ((Variant)(ref val)).As<Control>();
		}
		Variant val2 = default(Variant);
		if (info.TryGetProperty(PropertyName._dragging, ref val2))
		{
			_dragging = ((Variant)(ref val2)).As<bool>();
		}
		Variant val3 = default(Variant);
		if (info.TryGetProperty(PropertyName._entryVisible, ref val3))
		{
			_entryVisible = ((Variant)(ref val3)).As<bool>();
		}
		Variant val4 = default(Variant);
		if (info.TryGetProperty(PropertyName._dragOffset, ref val4))
		{
			_dragOffset = ((Variant)(ref val4)).As<Vector2>();
		}
		Variant val5 = default(Variant);
		if (info.TryGetProperty(PropertyName._dockSide, ref val5))
		{
			_dockSide = ((Variant)(ref val5)).As<DockSide>();
		}
		Variant val6 = default(Variant);
		if (info.TryGetProperty(PropertyName._isDocked, ref val6))
		{
			_isDocked = ((Variant)(ref val6)).As<bool>();
		}
		Variant val7 = default(Variant);
		if (info.TryGetProperty(PropertyName._dockedHovered, ref val7))
		{
			_dockedHovered = ((Variant)(ref val7)).As<bool>();
		}
		Variant val8 = default(Variant);
		if (info.TryGetProperty(PropertyName._hoverTween, ref val8))
		{
			_hoverTween = ((Variant)(ref val8)).As<Tween>();
		}
		Variant val9 = default(Variant);
		if (info.TryGetProperty(PropertyName._dockTween, ref val9))
		{
			_dockTween = ((Variant)(ref val9)).As<Tween>();
		}
		Variant val10 = default(Variant);
		if (info.TryGetProperty(PropertyName._autoHideTween, ref val10))
		{
			_autoHideTween = ((Variant)(ref val10)).As<Tween>();
		}
		Variant val11 = default(Variant);
		if (info.TryGetProperty(PropertyName._portableShelf, ref val11))
		{
			_portableShelf = ((Variant)(ref val11)).As<Control>();
		}
		Variant val12 = default(Variant);
		if (info.TryGetProperty(PropertyName._portableShelfLayer, ref val12))
		{
			_portableShelfLayer = ((Variant)(ref val12)).As<Control>();
		}
		Variant val13 = default(Variant);
		if (info.TryGetProperty(PropertyName._portableShelfContent, ref val13))
		{
			_portableShelfContent = ((Variant)(ref val13)).As<Control>();
		}
		Variant val14 = default(Variant);
		if (info.TryGetProperty(PropertyName._portableCardsRow, ref val14))
		{
			_portableCardsRow = ((Variant)(ref val14)).As<HBoxContainer>();
		}
		Variant val15 = default(Variant);
		if (info.TryGetProperty(PropertyName._portableCount, ref val15))
		{
			_portableCount = ((Variant)(ref val15)).As<Label>();
		}
		Variant val16 = default(Variant);
		if (info.TryGetProperty(PropertyName._portableFeedbackTween, ref val16))
		{
			_portableFeedbackTween = ((Variant)(ref val16)).As<Tween>();
		}
		Variant val17 = default(Variant);
		if (info.TryGetProperty(PropertyName._portableDockTween, ref val17))
		{
			_portableDockTween = ((Variant)(ref val17)).As<Tween>();
		}
		Variant val18 = default(Variant);
		if (info.TryGetProperty(PropertyName._portablePopup, ref val18))
		{
			_portablePopup = ((Variant)(ref val18)).As<NPortableTarotPopup>();
		}
		Variant val19 = default(Variant);
		if (info.TryGetProperty(PropertyName._portableAvailabilityRefreshElapsed, ref val19))
		{
			_portableAvailabilityRefreshElapsed = ((Variant)(ref val19)).As<double>();
		}
	}
}
