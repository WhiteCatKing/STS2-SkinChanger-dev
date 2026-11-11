using Godot;

namespace CznCore.Presentation.Vfx;

internal struct VfxSpawnParams
{
	public Texture2D? Texture;

	public Vector2 Position;

	public float Rotation;

	public Vector2 Scale;

	public Color Modulate;

	public float Lifetime;

	public Vector2 Size;

	public Rect2 SourceRect;

	public int SortingOrder;

	public bool AdditiveBlend;

	public VfxFadeMode FadeMode;

	public float ScaleFrom;

	public float ScaleTo;

	public Vector2 Velocity;

	public float AngularVelocity;

	public VfxSpawnParams()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		Texture = null;
		Position = Vector2.Zero;
		Rotation = 0f;
		Scale = Vector2.One;
		Modulate = Colors.White;
		Lifetime = 0.35f;
		Size = Vector2.Zero;
		SourceRect = new Rect2(0f, 0f, 0f, 0f);
		SortingOrder = 0;
		AdditiveBlend = false;
		FadeMode = VfxFadeMode.FadeOut;
		ScaleFrom = 1f;
		ScaleTo = 1f;
		Velocity = Vector2.Zero;
		AngularVelocity = 0f;
	}
}
