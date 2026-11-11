using Godot;

namespace MeirinWatcherSkin;

internal sealed class WatcherBodyVfxFollowAnchor : Node2D
{
	private Node2D? _spine;

	private Vector2 _homeSpineGlobalPosition;

	internal void Initialize(Node2D spine, Vector2 currentDisplacement)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		_spine = spine;
		_homeSpineGlobalPosition = spine.GlobalPosition - currentDisplacement;
		SyncPosition();
	}

	public override void _Process(double delta)
	{
		if (_spine == null || !GodotObject.IsInstanceValid((GodotObject)(object)_spine))
		{
			((Node)this).QueueFree();
		}
		else if (((Node)this).GetChildCount(false) == 0)
		{
			((Node)this).QueueFree();
		}
		else
		{
			SyncPosition();
		}
	}

	private void SyncPosition()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (_spine != null)
		{
			Node parent = ((Node)this).GetParent();
			CanvasItem val = (CanvasItem)(object)((parent is CanvasItem) ? parent : null);
			if (val != null)
			{
				Vector2 val2 = _spine.GlobalPosition - _homeSpineGlobalPosition;
				Transform2D globalTransform = val.GetGlobalTransform();
				Transform2D val3 = ((Transform2D)(ref globalTransform)).AffineInverse();
				((Node2D)this).Position = val3 * val2 - val3 * Vector2.Zero;
			}
		}
	}
}
