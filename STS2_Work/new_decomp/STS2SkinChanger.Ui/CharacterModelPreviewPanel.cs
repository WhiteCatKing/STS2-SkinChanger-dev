using System;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class CharacterModelPreviewPanel : Control
{
	private NCharacterSelectScreen _screen;

	private Control _info;

	private Control _frame;

	private NinePatchRect _background;

	private Node2D _visualContainer;

	private Button _dragHandle;

	private CharacterModel? _character;

	private bool _refreshQueued;

	private bool _layoutQueued;

	private bool _docked;

	private Panel _dockHint;

	private StyleBoxFlat _dockHintStyle;

	private Window _window;

	private bool _hintDragging;

	internal void Initialize(NCharacterSelectScreen screen, Control info)
	{
		_screen = screen;
		_info = info;
		_frame = ((Node)info).GetNodeOrNull<Control>(NodePath.op_Implicit("NinePatchRect")) ?? info;
		((Control)this).MouseFilter = (MouseFilterEnum)1;
		((Control)this).ClipContents = true;
		BuildInterface();
		BuildDockHint();
		DraggableSkinControl.AttachWithHandle((Control)(object)screen, (Control)(object)this, _dragHandle, LoadPlacement, SavePlacement, ResetPlacement, ApplyDefaultPlacement, UpdateDockedFromDrag, OnDragStateChanged);
		((CanvasItem)_frame).ItemRectChanged += QueueLayout;
		((CanvasItem)_info).ItemRectChanged += QueueLayout;
		((Control)_screen).Resized += QueueLayout;
		((CanvasItem)this).VisibilityChanged += OnVisibilityChanged;
		_window = ((Node)screen).GetWindow();
		_window.FocusExited += CancelPreviewDrag;
		((Node)this).TreeExiting += ReleaseDockHint;
	}

	private void BuildDockHint()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		_dockHint = new Panel
		{
			Name = StringName.op_Implicit("SCModelPreviewDockHint"),
			Visible = false,
			MouseFilter = (MouseFilterEnum)2,
			FocusMode = (FocusModeEnum)0
		};
		_dockHintStyle = new StyleBoxFlat
		{
			ContentMarginLeft = 0f,
			ContentMarginRight = 0f,
			ContentMarginTop = 0f,
			ContentMarginBottom = 0f,
			BorderWidthLeft = 2,
			BorderWidthRight = 2,
			BorderWidthTop = 2,
			BorderWidthBottom = 2
		};
		((Control)_dockHint).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)_dockHintStyle);
		((Node)_info).AddChild((Node)(object)_dockHint, false, (InternalMode)0);
		ModThemeRuntime.Bind((Node)(object)_dockHint, "preview_dock", ApplyDockHintStyle);
	}

	private void ApplyDockHintStyle(ModThemeSettings theme)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		_dockHintStyle.BgColor = ModThemeRuntime.Tint(theme.AccentColor, _docked ? 0.32f : 0.14f);
		_dockHintStyle.BorderColor = ModThemeRuntime.Tint(theme.AccentColor, 0.95f);
		StyleBoxFlat dockHintStyle = _dockHintStyle;
		StyleBoxFlat dockHintStyle2 = _dockHintStyle;
		StyleBoxFlat dockHintStyle3 = _dockHintStyle;
		int num2 = (_dockHintStyle.CornerRadiusBottomRight = Math.Min(theme.CornerRadius, 6));
		int num4 = (dockHintStyle3.CornerRadiusBottomLeft = num2);
		int cornerRadiusTopLeft = (dockHintStyle2.CornerRadiusTopRight = num4);
		dockHintStyle.CornerRadiusTopLeft = cornerRadiusTopLeft;
	}

	private void OnDragStateChanged(bool dragging)
	{
		_hintDragging = dragging;
		RefreshDockHint();
	}

	private void RefreshDockHint()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)_dockHint))
		{
			Rect2 infoRect;
			Rect2 screenRect;
			Rect2? val = ((Alive() && TryGetLayoutBounds(out infoRect, out screenRect)) ? CharacterModelPreviewControls.ResolveDockHint(infoRect, _hintDragging, ((CanvasItem)this).IsVisibleInTree()) : ((Rect2?)null));
			((CanvasItem)_dockHint).Visible = val.HasValue;
			if (val.HasValue)
			{
				Rect2 valueOrDefault = val.GetValueOrDefault();
				((Control)_dockHint).Position = ((Rect2)(ref valueOrDefault)).Position;
				((Control)_dockHint).Size = ((Rect2)(ref valueOrDefault)).Size;
				ApplyDockHintStyle(ModThemeRuntime.Current);
			}
		}
	}

	private void CancelPreviewDrag()
	{
		DraggableSkinControl.CancelDrag((Control)(object)this);
	}

	private void ReleaseDockHint()
	{
		_hintDragging = false;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_window))
		{
			_window.FocusExited -= CancelPreviewDrag;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_dockHint))
		{
			((CanvasItem)_dockHint).Hide();
			((Node)_dockHint).QueueFree();
		}
	}

	private void BuildInterface()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Expected O, but got Unknown
		_background = new NinePatchRect
		{
			Name = StringName.op_Implicit("NinePatchRect"),
			MouseFilter = (MouseFilterEnum)2
		};
		Control frame = _frame;
		NinePatchRect val = (NinePatchRect)(object)((frame is NinePatchRect) ? frame : null);
		if (val != null)
		{
			_background.Texture = val.Texture;
			((CanvasItem)_background).Modulate = ((CanvasItem)val).Modulate * ((CanvasItem)val).SelfModulate;
			_background.PatchMarginLeft = val.PatchMarginLeft;
			_background.PatchMarginRight = val.PatchMarginRight;
			_background.PatchMarginTop = val.PatchMarginTop;
			_background.PatchMarginBottom = val.PatchMarginBottom;
		}
		((Node)this).AddChild((Node)(object)_background, false, (InternalMode)0);
		((Control)_background).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Control)_background).OffsetLeft = 13f;
		_visualContainer = new Node2D
		{
			Name = StringName.op_Implicit("VisualContainer")
		};
		((Node)this).AddChild((Node)(object)_visualContainer, false, (InternalMode)0);
		_dragHandle = new Button
		{
			Name = StringName.op_Implicit("SkinChangerDragHandle"),
			FocusMode = (FocusModeEnum)0,
			MouseFilter = (MouseFilterEnum)0,
			MouseDefaultCursorShape = (CursorShape)13,
			AnchorTop = 0.1f,
			AnchorBottom = 0.9f,
			SelfModulate = new Color(1f, 1f, 1f, ((CanvasItem)_background).Modulate.A * ((CanvasItem)_background).SelfModulate.A),
			OffsetRight = 12f
		};
		(string, string)[] array = new(string, string)[5]
		{
			("normal", "000000"),
			("hover", "161616"),
			("pressed", "303030"),
			("focus", "161616"),
			("disabled", "000000")
		};
		for (int i = 0; i < array.Length; i++)
		{
			var (text, text2) = array[i];
			((Control)_dragHandle).AddThemeStyleboxOverride(StringName.op_Implicit(text), (StyleBox)new StyleBoxFlat
			{
				BgColor = new Color(text2),
				ContentMarginLeft = 0f,
				ContentMarginRight = 0f,
				ContentMarginTop = 0f,
				ContentMarginBottom = 0f,
				CornerRadiusTopLeft = 4,
				CornerRadiusTopRight = 4,
				CornerRadiusBottomLeft = 4,
				CornerRadiusBottomRight = 4
			});
		}
		((Node)this).AddChild((Node)(object)_dragHandle, false, (InternalMode)0);
	}

	internal void ShowCharacter(CharacterModel character)
	{
		_character = character;
		QueueLayout();
		QueueRefresh();
	}

	internal void Suspend()
	{
		_character = null;
		((CanvasItem)this).Visible = false;
		ClearModel();
	}

	private void QueueRefresh()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (_refreshQueued || !Alive() || !CanLoadModel(requireVisible: false))
		{
			return;
		}
		_refreshQueued = true;
		Callable val = Callable.From((Action)delegate
		{
			if (!Alive())
			{
				return;
			}
			try
			{
				RefreshModel();
			}
			catch (Exception ex)
			{
				ModLog.Warn("SC 模型预览刷新失败：" + ex.GetBaseException().Message);
			}
			finally
			{
				_refreshQueued = false;
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private void RefreshModel()
	{
		if (_character == null || !CharacterModelPreviewControls.IsEnabled)
		{
			Suspend();
		}
		else if (CanLoadModel())
		{
			ApplyLayout();
			if (((CanvasItem)this).IsVisibleInTree() && CanLoadModel())
			{
				ClearModel();
				FrameworkModelPreview.Refresh((Node)(object)this, _character);
			}
		}
	}

	private bool CanLoadModel(bool requireVisible = true)
	{
		return CharacterModelPreviewControls.ShouldLoadModel(_docked, _character != null, CharacterModelPreviewControls.IsEnabled, !requireVisible || ((CanvasItem)_info).IsVisibleInTree());
	}

	private void SetDocked(bool docked)
	{
		if (_docked != docked)
		{
			_docked = docked;
			NinePatchRect background = _background;
			bool visible = (((CanvasItem)_visualContainer).Visible = !docked);
			((CanvasItem)background).Visible = visible;
			((Control)this).MouseFilter = (MouseFilterEnum)(docked ? 2 : 1);
			if (docked)
			{
				ClearModel();
			}
			else
			{
				QueueRefresh();
			}
		}
	}

	private void UpdateDockedFromDrag()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetLayoutBounds(out var infoRect, out var _))
		{
			Transform2D globalTransformWithCanvas = ((CanvasItem)_info).GetGlobalTransformWithCanvas();
			Vector2 gripCenter = ((Transform2D)(ref globalTransformWithCanvas)).AffineInverse() * (((CanvasItem)_dragHandle).GetGlobalTransformWithCanvas() * (((Control)_dragHandle).Size / 2f));
			SetDocked(CharacterModelPreviewControls.IsGripDocked(infoRect, gripCenter));
			RefreshDockHint();
		}
	}

	private (float X, float Y)? LoadPlacement()
	{
		SetDocked(SkinService.IsCharacterModelPreviewDocked());
		if (!_docked)
		{
			return SkinService.GetCharacterModelPreviewPosition();
		}
		return null;
	}

	private void SavePlacement(float x, float y)
	{
		SkinService.SetCharacterModelPreviewPlacement(x, y, _docked);
		DraggableSkinControl.RefreshPlacement((Control)(object)this);
	}

	private void ResetPlacement()
	{
		SkinService.ResetCharacterModelPreviewPosition();
		SetDocked(docked: false);
	}

	private void QueueLayout()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (_layoutQueued || !Alive())
		{
			return;
		}
		_layoutQueued = true;
		Callable val = Callable.From((Action)delegate
		{
			_layoutQueued = false;
			if (Alive())
			{
				ApplyLayout();
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private void ApplyLayout()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)_frame))
		{
			return;
		}
		Rect2? defaultLayout = GetDefaultLayout();
		((CanvasItem)this).Visible = defaultLayout.HasValue && _character != null && CharacterModelPreviewControls.IsEnabled;
		RefreshDockHint();
		if (!defaultLayout.HasValue)
		{
			return;
		}
		Rect2 valueOrDefault = defaultLayout.GetValueOrDefault();
		if (((CanvasItem)this).Visible)
		{
			Vector2 size = ((Control)this).Size;
			bool num = !((Vector2)(ref size)).IsEqualApprox(((Rect2)(ref valueOrDefault)).Size);
			((Control)this).Size = ((Rect2)(ref valueOrDefault)).Size;
			DraggableSkinControl.RefreshPlacement((Control)(object)this);
			if (num)
			{
				((Node)_visualContainer).GetNodeOrNull<FrameworkPreviewSurface>(NodePath.op_Implicit("PreviewSprite"))?.RefreshLayout();
			}
		}
	}

	private Rect2? GetDefaultLayout()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetLayoutBounds(out var infoRect, out var screenRect))
		{
			return null;
		}
		Rect2? result = CharacterModelPreviewControls.ResolveLayout(infoRect, screenRect);
		if (!_docked)
		{
			return result;
		}
		Rect2? val = CharacterModelPreviewControls.ResolveDockLayout(infoRect);
		if (val.HasValue)
		{
			Rect2 valueOrDefault = val.GetValueOrDefault();
			Vector2 position = ((Rect2)(ref valueOrDefault)).Position;
			Vector2 size;
			if (!result.HasValue)
			{
				size = ((Rect2)(ref valueOrDefault)).Size;
			}
			else
			{
				Rect2 valueOrDefault2 = result.GetValueOrDefault();
				size = ((Rect2)(ref valueOrDefault2)).Size;
			}
			return new Rect2(position, size);
		}
		return null;
	}

	private bool TryGetLayoutBounds(out Rect2 infoRect, out Rect2 screenRect)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		infoRect = (screenRect = default(Rect2));
		Transform2D globalTransformWithCanvas = ((CanvasItem)_info).GetGlobalTransformWithCanvas();
		if (!((Transform2D)(ref globalTransformWithCanvas)).IsFinite() || Mathf.IsZeroApprox(((Transform2D)(ref globalTransformWithCanvas)).Determinant()))
		{
			return false;
		}
		Transform2D val = ((Transform2D)(ref globalTransformWithCanvas)).AffineInverse();
		infoRect = val * ((CanvasItem)_frame).GetGlobalTransformWithCanvas() * new Rect2(Vector2.Zero, _frame.Size);
		screenRect = val * ((CanvasItem)_screen).GetGlobalTransformWithCanvas() * new Rect2(Vector2.Zero, ((Control)_screen).Size);
		return true;
	}

	private void ApplyDefaultPlacement()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (Alive() && GodotObject.IsInstanceValid((GodotObject)(object)_frame))
		{
			Rect2? defaultLayout = GetDefaultLayout();
			if (defaultLayout.HasValue)
			{
				Rect2 valueOrDefault = defaultLayout.GetValueOrDefault();
				float num2 = (((Control)this).AnchorBottom = 0f);
				float num4 = (((Control)this).AnchorTop = num2);
				float anchorLeft = (((Control)this).AnchorRight = num4);
				((Control)this).AnchorLeft = anchorLeft;
				((Control)this).Position = ((Rect2)(ref valueOrDefault)).Position;
			}
		}
	}

	private void OnVisibilityChanged()
	{
		RefreshDockHint();
		if (!((CanvasItem)this).IsVisibleInTree())
		{
			ClearModel();
		}
		else if (_character != null)
		{
			QueueRefresh();
		}
	}

	private void ClearModel()
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)_visualContainer))
		{
			return;
		}
		foreach (Node child in ((Node)_visualContainer).GetChildren(false))
		{
			if (child is FrameworkPreviewSurface frameworkPreviewSurface)
			{
				frameworkPreviewSurface.StopCapture();
			}
			((Node)_visualContainer).RemoveChild(child);
			child.QueueFree();
		}
	}

	public override void _ExitTree()
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)_frame))
		{
			((CanvasItem)_frame).ItemRectChanged -= QueueLayout;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_info))
		{
			((CanvasItem)_info).ItemRectChanged -= QueueLayout;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_screen))
		{
			((Control)_screen).Resized -= QueueLayout;
		}
		((CanvasItem)this).VisibilityChanged -= OnVisibilityChanged;
	}

	private bool Alive()
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)this) && ((Node)this).IsInsideTree())
		{
			return !((GodotObject)this).IsQueuedForDeletion();
		}
		return false;
	}
}
