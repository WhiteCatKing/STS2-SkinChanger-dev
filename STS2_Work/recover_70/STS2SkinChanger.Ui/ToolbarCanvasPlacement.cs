using Godot;

namespace STS2SkinChanger.Ui;

internal sealed class ToolbarCanvasPlacement
{
	private bool _initialized;

	private Vector2 _localPosition;

	private Vector2 _localScale;

	private Vector2 _appliedPosition;

	private Vector2 _appliedScale;

	private float _localRotation;

	private float _appliedRotation;

	internal (Vector2 Position, Vector2 Scale, float Rotation) Resolve(Vector2 position, Vector2 scale, float rotation, Vector2 pivot, Transform2D parent)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized || !((Vector2)(ref position)).IsEqualApprox(_appliedPosition))
		{
			_localPosition = position;
		}
		if (!_initialized || !((Vector2)(ref scale)).IsEqualApprox(_appliedScale))
		{
			_localScale = scale;
		}
		if (!_initialized || !Mathf.IsEqualApprox(rotation, _appliedRotation))
		{
			_localRotation = rotation;
		}
		Transform2D val = default(Transform2D);
		((Transform2D)(ref val))._002Ector(_localRotation, _localScale, 0f, Vector2.Zero);
		val.Origin = _localPosition + pivot - ((Transform2D)(ref val)).BasisXform(pivot);
		Transform2D val2 = parent * val;
		_appliedPosition = val2.Origin - pivot + ((Transform2D)(ref val2)).BasisXform(pivot);
		_appliedScale = ((Transform2D)(ref val2)).Scale;
		_appliedRotation = ((Transform2D)(ref val2)).Rotation;
		_initialized = true;
		return (Position: _appliedPosition, Scale: _appliedScale, Rotation: _appliedRotation);
	}
}
