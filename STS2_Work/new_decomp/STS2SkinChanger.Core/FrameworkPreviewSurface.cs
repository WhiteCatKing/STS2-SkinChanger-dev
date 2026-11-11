using System;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

internal class FrameworkPreviewSurface : Node2D
{
	private Control _selector;

	private Node2D _container;

	private FrameworkPreviewCreature _owner;

	private SubViewport _viewport;

	private Sprite2D _display;

	private string _groupId = "";

	private RenderedPreviewFraming? _framing;

	private bool _subscribed;

	private bool _started;

	private bool _presentNextFrame;

	private bool _finished;

	private bool _exited;

	private bool _playRequested = true;

	internal void Initialize(Control selector, Node2D container, FrameworkPreviewCreature owner, string groupId)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		_selector = selector;
		_container = container;
		_owner = owner;
		_groupId = groupId;
		Viewport viewport = ((Node)selector).GetViewport();
		_viewport = new SubViewport
		{
			Name = StringName.op_Implicit("ModelViewport"),
			Size = new Vector2I(512, 512),
			World2D = new World2D(),
			Disable3D = true,
			TransparentBg = true,
			GuiDisableInput = true,
			CanvasItemDefaultTextureFilter = viewport.CanvasItemDefaultTextureFilter,
			CanvasItemDefaultTextureRepeat = viewport.CanvasItemDefaultTextureRepeat,
			RenderTargetClearMode = (ClearMode)0,
			RenderTargetUpdateMode = (UpdateMode)0
		};
		((Node)_viewport).AddChild((Node)(object)owner, false, (InternalMode)0);
		((Node)this).AddChild((Node)(object)_viewport, false, (InternalMode)0);
		_display = new Sprite2D
		{
			Name = StringName.op_Implicit("ModelImage"),
			Centered = false,
			Visible = false,
			Texture = (Texture2D)(object)((Viewport)_viewport).GetTexture()
		};
		((Node)this).AddChild((Node)(object)_display, false, (InternalMode)0);
		((CanvasItem)this).VisibilityChanged += UpdateVisibility;
		((Node)this).TreeExiting += StopCapture;
	}

	internal void BeginCapture()
	{
		if (_exited || _started)
		{
			return;
		}
		MegaSprite spineBody = ((NCreature)_owner).Visuals.SpineBody;
		if (spineBody != null)
		{
			SpineNodeExtensions.RunWhenSpineReady((Node)(object)this, spineBody, (Action<MegaAnimationState>)delegate
			{
				StartCapture();
			});
		}
		else
		{
			StartCapture();
		}
	}

	private void StartCapture()
	{
		if (!_exited && !_started && Alive((Node)(object)this))
		{
			_started = true;
			FrameworkModelPreview.StartAnimations((Node)(object)_owner, _groupId, playEntry: false);
			UpdateVisibility();
		}
	}

	internal void PlayEntry()
	{
		if (!_exited)
		{
			if (!_finished)
			{
				_playRequested = true;
			}
			else
			{
				FrameworkModelPreview.StartAnimations((Node)(object)_owner, _groupId);
			}
		}
	}

	internal void RefreshLayout()
	{
		if (!_exited && _started && Alive((Node)(object)this))
		{
			_framing?.Cancel();
			_framing = null;
			_presentNextFrame = false;
			_finished = false;
			((CanvasItem)_display).Visible = false;
			UpdateVisibility();
		}
	}

	private void UpdateVisibility()
	{
		if (_exited || !_started || !Alive((Node)(object)this))
		{
			return;
		}
		if (!((CanvasItem)this).IsVisibleInTree())
		{
			Unsubscribe();
			_viewport.RenderTargetUpdateMode = (UpdateMode)0;
			((Node)_viewport).ProcessMode = (ProcessModeEnum)4;
			return;
		}
		((Node)_viewport).ProcessMode = (ProcessModeEnum)0;
		if (_finished)
		{
			_viewport.RenderTargetUpdateMode = (UpdateMode)2;
			return;
		}
		if (!_subscribed)
		{
			RenderingServer.FramePostDraw += OnFrameDrawn;
			_subscribed = true;
		}
		_viewport.RenderTargetUpdateMode = (UpdateMode)1;
	}

	private void OnFrameDrawn()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (_exited || !Alive((Node)(object)this) || !Alive((Node)(object)_selector) || !Alive((Node)(object)_container) || !Alive((Node)(object)_owner))
		{
			CancelCapture();
			return;
		}
		if (!((CanvasItem)this).IsVisibleInTree())
		{
			UpdateVisibility();
			return;
		}
		try
		{
			if (_presentNextFrame)
			{
				Present();
				return;
			}
			if (_framing == null)
			{
				LayoutSurface();
				Rect2 initialBounds = FrameworkModelPreview.MeasureCaptureSeed(((NCreature)_owner).Visuals);
				_framing = new RenderedPreviewFraming(initialBounds, _viewport.Size);
			}
			else
			{
				Image image = ((Texture2D)((Viewport)_viewport).GetTexture()).GetImage();
				try
				{
					Rect2? renderedPixels = ((image == null || image.IsEmpty()) ? ((Rect2?)null) : new Rect2?(Rect2I.op_Implicit(image.GetUsedRect())));
					_presentNextFrame = !_framing.Observe(renderedPixels);
				}
				finally
				{
					((IDisposable)image)?.Dispose();
				}
			}
			((Viewport)_viewport).CanvasTransform = _framing.CanvasTransform;
			_viewport.RenderTargetUpdateMode = (UpdateMode)1;
		}
		catch (Exception ex)
		{
			ModLog.Warn("统一小预览取景失败 " + _groupId + "，保留当前相机：" + ex.GetBaseException().Message);
			_framing?.Cancel();
			Present();
		}
	}

	private void LayoutSurface()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		Control val = ((Node)_selector).GetNodeOrNull<Control>(NodePath.op_Implicit("NinePatchRect")) ?? _selector;
		Transform2D globalTransform = _container.GlobalTransform;
		Transform2D val2 = ((Transform2D)(ref globalTransform)).AffineInverse();
		Rect2 panel = val2 * ((CanvasItem)val).GetGlobalTransform() * new Rect2(Vector2.Zero, val.Size);
		Control nodeOrNull = ((Node)_selector).GetNodeOrNull<Control>(NodePath.op_Implicit("HBoxContainer"));
		Rect2? footer = ((nodeOrNull == null) ? ((Rect2?)null) : new Rect2?(val2 * ((CanvasItem)nodeOrNull).GetGlobalTransform() * new Rect2(Vector2.Zero, nodeOrNull.Size)));
		Rect2 val3 = FrameworkModelPreview.PreviewArea(panel, footer);
		if (((Rect2)(ref val3)).HasArea())
		{
			Vector2 val4 = ((Rect2)(ref val3)).Position;
			if (((Vector2)(ref val4)).IsFinite())
			{
				val4 = ((Rect2)(ref val3)).Size;
				if (((Vector2)(ref val4)).IsFinite())
				{
					Transform2D screenTransform = ((CanvasItem)_container).GetScreenTransform();
					Vector2 val5 = ((Rect2)(ref val3)).Size * new Vector2(((Vector2)(ref screenTransform.X)).Length(), ((Vector2)(ref screenTransform.Y)).Length());
					if (!((Vector2)(ref val5)).IsFinite() || val5.X <= 0f || val5.Y <= 0f)
					{
						val5 = ((Rect2)(ref val3)).Size;
					}
					val5 *= Math.Min(1f, 1024f / Math.Max(val5.X, val5.Y));
					Vector2I val6 = default(Vector2I);
					((Vector2I)(ref val6))._002Ector(Math.Clamp((int)Math.Ceiling(val5.X), 32, 1024), Math.Clamp((int)Math.Ceiling(val5.Y), 32, 1024));
					_viewport.Size = val6;
					((Node2D)_display).Position = ((Rect2)(ref val3)).Position;
					((Node2D)_display).Scale = ((Rect2)(ref val3)).Size / Vector2I.op_Implicit(val6);
					return;
				}
			}
		}
		throw new InvalidOperationException("小模型预览区域尚未就绪。");
	}

	private void Present()
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		Unsubscribe();
		_finished = true;
		((CanvasItem)_display).Visible = true;
		_viewport.RenderTargetUpdateMode = (UpdateMode)2;
		RenderedPreviewFraming? framing = _framing;
		if (framing != null && framing.HasContent)
		{
			ModLog.Info($"统一小预览实际取景：{_groupId}；可见={_framing.VisibleBounds}；采样={_framing.Samples}；画布={_viewport.Size}；相机比例={_framing.CanvasTransform.X.X:0.###}。");
		}
		else
		{
			ModLog.Warn("统一小预览未取得有效像素 " + _groupId + "，已停止采样并保留初始相机。");
		}
		if (_playRequested)
		{
			_playRequested = false;
			FrameworkModelPreview.StartAnimations((Node)(object)_owner, _groupId);
		}
	}

	private void Unsubscribe()
	{
		if (_subscribed)
		{
			RenderingServer.FramePostDraw -= OnFrameDrawn;
			_subscribed = false;
		}
	}

	private void CancelCapture()
	{
		Unsubscribe();
		_framing?.Cancel();
		_exited = true;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_viewport))
		{
			_viewport.RenderTargetUpdateMode = (UpdateMode)0;
			((Node)_viewport).ProcessMode = (ProcessModeEnum)4;
		}
	}

	internal void StopCapture()
	{
		CancelCapture();
	}

	public override void _ExitTree()
	{
		CancelCapture();
		((CanvasItem)this).VisibilityChanged -= UpdateVisibility;
	}

	private static bool Alive(Node node)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)node) && !((GodotObject)node).IsQueuedForDeletion())
		{
			return node.IsInsideTree();
		}
		return false;
	}
}
