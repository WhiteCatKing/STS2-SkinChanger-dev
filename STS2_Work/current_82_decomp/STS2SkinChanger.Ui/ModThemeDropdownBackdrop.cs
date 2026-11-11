using System;
using System.Runtime.CompilerServices;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal sealed class ModThemeDropdownBackdrop
{
	private const int EmbeddedWindowCanvasLayer = 1024;

	private static readonly ConditionalWeakTable<PopupMenu, ModThemeDropdownBackdrop> Instances = new ConditionalWeakTable<PopupMenu, ModThemeDropdownBackdrop>();

	private readonly PopupMenu _popup;

	private Viewport? _host;

	private CanvasLayer? _layer;

	private Control? _surface;

	private ModThemeBackdrop? _backdrop;

	private bool _following;

	internal static void Attach(PopupMenu popup)
	{
		Instances.GetValue(popup, (PopupMenu node) => new ModThemeDropdownBackdrop(node));
	}

	private ModThemeDropdownBackdrop(PopupMenu popup)
	{
		_popup = popup;
		((Node)popup).TreeEntered += Refresh;
		((Window)popup).VisibilityChanged += Refresh;
		((Viewport)popup).SizeChanged += Refresh;
		((Popup)popup).PopupHide += Release;
		((Node)popup).TreeExiting += Release;
		ModThemeRuntime.Bind((Node)(object)popup, "dropdown_blur", delegate
		{
			Refresh();
		});
	}

	private void Refresh()
	{
		if (!((Node)_popup).IsInsideTree() || !((Window)_popup).Visible || ModThemeRuntime.Current.DropdownBlur <= 0f)
		{
			Release();
			return;
		}
		Viewport val = FindHost();
		if (val == null)
		{
			Release();
			return;
		}
		if (_host != val)
		{
			Release();
		}
		_host = val;
		StartFollowing();
		EnsureSurface();
		SyncGeometry();
	}

	private Viewport? FindHost()
	{
		if (!((Window)_popup).IsEmbedded())
		{
			Node parent = ((Node)_popup).GetParent();
			if (parent == null)
			{
				return null;
			}
			return (Viewport?)(object)parent.GetWindow();
		}
		Node parent2 = ((Node)_popup).GetParent();
		Viewport val = ((parent2 != null) ? parent2.GetViewport() : null);
		while (val != null)
		{
			if (val.GuiEmbedSubwindows)
			{
				return val;
			}
			Node parent3 = ((Node)val).GetParent();
			val = ((parent3 != null) ? parent3.GetViewport() : null);
		}
		return null;
	}

	private void StartFollowing()
	{
		if (!_following)
		{
			_following = true;
			RenderingServer.FramePreDraw += SyncGeometry;
		}
	}

	private void EnsureSurface()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		if (!GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			_layer = new CanvasLayer
			{
				Name = StringName.op_Implicit("SCThemeDropdownBackdrop_" + ((GodotObject)_popup).GetInstanceId()),
				Layer = 1023
			};
			((Node)_host).AddChild((Node)(object)_layer, false, (InternalMode)0);
			_surface = new Control
			{
				MouseFilter = (MouseFilterEnum)2,
				FocusMode = (FocusModeEnum)0,
				Visible = false
			};
			((Node)_layer).AddChild((Node)(object)_surface, false, (InternalMode)0);
			_backdrop = ModThemeBackdrop.For(_surface);
		}
	}

	private void SyncGeometry()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)_popup) || !((Node)_popup).IsInsideTree() || !((Window)_popup).Visible || !GodotObject.IsInstanceValid((GodotObject)(object)_host) || ModThemeRuntime.Current.DropdownBlur <= 0f)
		{
			Release();
		}
		else
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)_surface) || !GodotObject.IsInstanceValid((GodotObject)(object)_layer))
			{
				return;
			}
			bool flag = ((Window)_popup).IsEmbedded();
			Transform2D finalTransform = _host.GetFinalTransform();
			if (!flag && Math.Abs(((Transform2D)(ref finalTransform)).Determinant()) < 1E-06f)
			{
				((CanvasItem)_surface).Visible = false;
				return;
			}
			Vector2 popupPosition = Vector2I.op_Implicit(((Window)_popup).Position);
			Viewport? host = _host;
			Window val = (Window)(object)((host is Window) ? host : null);
			var (val2, val3) = Placement(flag, popupPosition, (val != null) ? Vector2I.op_Implicit(val.Position) : Vector2.Zero, finalTransform);
			if (_layer.Transform != val3)
			{
				_layer.Transform = val3;
			}
			if (_surface.Position != val2)
			{
				_surface.Position = val2;
			}
			if (_surface.Size != Vector2I.op_Implicit(((Window)_popup).Size))
			{
				_surface.Size = Vector2I.op_Implicit(((Window)_popup).Size);
			}
			ModThemeSettings current = ModThemeRuntime.Current;
			Rect2 visibleRect = ((Viewport)_popup).GetVisibleRect();
			Vector2 size = ((Rect2)(ref visibleRect)).Size;
			float num = ((size.X > 0f) ? ((float)((Window)_popup).Size.X / size.X) : 1f);
			_backdrop.Update(Colors.Transparent, current.DropdownBlur, (int)MathF.Round((float)current.DropdownCornerRadius * num));
			((CanvasItem)_surface).Visible = true;
		}
	}

	internal static (Vector2 Position, Transform2D Transform) Placement(bool embedded, Vector2 popupPosition, Vector2 hostWindowPosition, Transform2D hostTransform)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (!embedded)
		{
			return (Position: popupPosition - hostWindowPosition, Transform: ((Transform2D)(ref hostTransform)).AffineInverse());
		}
		return (Position: popupPosition, Transform: Transform2D.Identity);
	}

	private void Release()
	{
		if (_following)
		{
			RenderingServer.FramePreDraw -= SyncGeometry;
		}
		_following = false;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			_layer.Visible = false;
			((Node)_layer).QueueFree();
		}
		_layer = null;
		_surface = null;
		_backdrop = null;
		_host = null;
	}
}
