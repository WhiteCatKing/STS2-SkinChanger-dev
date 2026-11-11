using Godot;

namespace STS2SkinChanger.Ui;

internal class ProviderToolbarCanvas : Node
{
	private const string BindingName = "SkinChangerToolbarCanvas";

	private readonly ToolbarCanvasPlacement _placement = new ToolbarCanvasPlacement();

	private readonly ToolbarCanvasTint _tint = new ToolbarCanvasTint();

	private Control _control;

	private bool _updating;

	internal static void Bind(Control control)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		ProviderToolbarCanvas nodeOrNull = ((Node)control).GetNodeOrNull<ProviderToolbarCanvas>(NodePath.op_Implicit("SkinChangerToolbarCanvas"));
		if (nodeOrNull != null)
		{
			nodeOrNull.UpdateCanvas();
		}
		else
		{
			if (((CanvasItem)control).TopLevel)
			{
				return;
			}
			Rect2 globalRect = control.GetGlobalRect();
			bool flag = false;
			Node parent = ((Node)control).GetParent();
			CanvasItem val = (CanvasItem)(object)((parent is CanvasItem) ? parent : null);
			while (val != null)
			{
				Control val2 = (Control)(object)((val is Control) ? val : null);
				if (val2 != null && val2.ClipContents)
				{
					Rect2 globalRect2 = val2.GetGlobalRect();
					if (!((Rect2)(ref globalRect2)).Encloses(globalRect))
					{
						flag = true;
					}
				}
				if (val.TopLevel)
				{
					break;
				}
				Node parent2 = ((Node)val).GetParent();
				val = (CanvasItem)(object)((parent2 is CanvasItem) ? parent2 : null);
			}
			if (flag)
			{
				ProviderToolbarCanvas providerToolbarCanvas = new ProviderToolbarCanvas();
				((Node)providerToolbarCanvas).Name = StringName.op_Implicit("SkinChangerToolbarCanvas");
				providerToolbarCanvas._control = control;
				ProviderToolbarCanvas providerToolbarCanvas2 = providerToolbarCanvas;
				((Node)control).AddChild((Node)(object)providerToolbarCanvas2, false, (InternalMode)0);
				providerToolbarCanvas2.UpdateCanvas();
			}
		}
	}

	public override void _EnterTree()
	{
		((CanvasItem)_control).ItemRectChanged += UpdateCanvas;
	}

	public override void _Process(double delta)
	{
		UpdateCanvas();
	}

	public override void _ExitTree()
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)_control))
		{
			((CanvasItem)_control).ItemRectChanged -= UpdateCanvas;
		}
	}

	private void UpdateCanvas()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		if (_updating || !GodotObject.IsInstanceValid((GodotObject)(object)_control) || ((GodotObject)_control).IsQueuedForDeletion())
		{
			return;
		}
		_updating = true;
		try
		{
			Node parent = ((Node)_control).GetParent();
			CanvasItem val = (CanvasItem)(object)((parent is CanvasItem) ? parent : null);
			(Vector2 Position, Vector2 Scale, float Rotation) tuple = _placement.Resolve(_control.Position, _control.Scale, _control.Rotation, _control.PivotOffset, (val != null) ? val.GetGlobalTransform() : Transform2D.Identity);
			Vector2 item = tuple.Position;
			Vector2 item2 = tuple.Scale;
			float item3 = tuple.Rotation;
			Color val2 = Colors.White;
			CanvasItem val3 = val;
			while (val3 != null)
			{
				val2 *= val3.Modulate;
				if (val3.TopLevel)
				{
					break;
				}
				Node parent2 = ((Node)val3).GetParent();
				val3 = (CanvasItem)(object)((parent2 is CanvasItem) ? parent2 : null);
			}
			Color val4 = _tint.Resolve(((CanvasItem)_control).Modulate, val2);
			((CanvasItem)_control).TopLevel = true;
			Vector2 val5 = _control.Scale;
			if (!((Vector2)(ref val5)).IsEqualApprox(item2))
			{
				_control.Scale = item2;
			}
			if (!Mathf.IsEqualApprox(_control.Rotation, item3))
			{
				_control.Rotation = item3;
			}
			val5 = _control.Position;
			if (!((Vector2)(ref val5)).IsEqualApprox(item))
			{
				_control.Position = item;
			}
			Color modulate = ((CanvasItem)_control).Modulate;
			if (!((Color)(ref modulate)).IsEqualApprox(val4))
			{
				((CanvasItem)_control).Modulate = val4;
			}
		}
		finally
		{
			_updating = false;
		}
	}
}
