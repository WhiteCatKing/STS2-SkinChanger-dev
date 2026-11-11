using Godot;

namespace STS2SkinChanger.Ui;

internal sealed class ToolbarCanvasTint
{
	private Color? _applied;

	private Color _local;

	internal Color Resolve(Color current, Color inherited)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Color? applied = _applied;
		if (applied.HasValue)
		{
			Color valueOrDefault = applied.GetValueOrDefault();
			if (((Color)(ref current)).IsEqualApprox(valueOrDefault))
			{
				goto IL_0029;
			}
		}
		_local = current;
		goto IL_0029;
		IL_0029:
		Color val = _local * inherited;
		_applied = val;
		return val;
	}
}
