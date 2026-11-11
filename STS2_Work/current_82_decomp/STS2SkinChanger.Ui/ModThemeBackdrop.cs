using System;
using System.Runtime.CompilerServices;
using Godot;

namespace STS2SkinChanger.Ui;

internal sealed class ModThemeBackdrop
{
	private static readonly ConditionalWeakTable<Control, ModThemeBackdrop> Instances = new ConditionalWeakTable<Control, ModThemeBackdrop>();

	private static readonly Lazy<Shader> FlatShader = new Lazy<Shader>((Func<Shader>)(() => new Shader
	{
		Code = "shader_type canvas_item;\nrender_mode unshaded;\nuniform vec4 tint : source_color;\nuniform vec2 surface_size = vec2(1.0);\nuniform float corner_radius = 0.0;float coverage(vec2 uv) {\n    vec2 half_size = surface_size * 0.5;\n    float r = min(corner_radius, min(half_size.x, half_size.y));\n    vec2 q = abs(uv * surface_size - half_size) - half_size + vec2(r);\n    float signed_distance = length(max(q, vec2(0.0))) + min(max(q.x, q.y), 0.0) - r;\n    float edge = max(fwidth(signed_distance), 0.5);\n    return 1.0 - smoothstep(-edge, edge, signed_distance);\n}void fragment() { COLOR = tint * vec4(1.0, 1.0, 1.0, coverage(UV)) * COLOR; }"
	}));

	private static readonly Lazy<Shader> BlurShader = new Lazy<Shader>((Func<Shader>)(() => new Shader
	{
		Code = "shader_type canvas_item;\nrender_mode unshaded;\nuniform vec4 tint : source_color;\nuniform vec2 surface_size = vec2(1.0);\nuniform float corner_radius = 0.0;uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear_mipmap;\nuniform float blur_lod = 0.0;float coverage(vec2 uv) {\n    vec2 half_size = surface_size * 0.5;\n    float r = min(corner_radius, min(half_size.x, half_size.y));\n    vec2 q = abs(uv * surface_size - half_size) - half_size + vec2(r);\n    float signed_distance = length(max(q, vec2(0.0))) + min(max(q.x, q.y), 0.0) - r;\n    float edge = max(fwidth(signed_distance), 0.5);\n    return 1.0 - smoothstep(-edge, edge, signed_distance);\n}void fragment() {\n    vec3 b = textureLod(screen_texture, SCREEN_UV, blur_lod).rgb;\n    COLOR = vec4(mix(b, tint.rgb, tint.a), coverage(UV)) * COLOR;\n}"
	}));

	private const string Header = "shader_type canvas_item;\nrender_mode unshaded;\nuniform vec4 tint : source_color;\nuniform vec2 surface_size = vec2(1.0);\nuniform float corner_radius = 0.0;";

	private const string Mask = "float coverage(vec2 uv) {\n    vec2 half_size = surface_size * 0.5;\n    float r = min(corner_radius, min(half_size.x, half_size.y));\n    vec2 q = abs(uv * surface_size - half_size) - half_size + vec2(r);\n    float signed_distance = length(max(q, vec2(0.0))) + min(max(q.x, q.y), 0.0) - r;\n    float edge = max(fwidth(signed_distance), 0.5);\n    return 1.0 - smoothstep(-edge, edge, signed_distance);\n}";

	private readonly Control _owner;

	private readonly Node2D _surface;

	private readonly BackBufferCopy _copy;

	private readonly ColorRect _fill;

	private readonly ShaderMaterial _material = new ShaderMaterial();

	private (Color Tint, float Blur, int Radius, bool Visible)? _last;

	private float _padding;

	private bool _trackingCopy;

	private ModThemeBackdrop(Control owner)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		_owner = owner;
		_material.Shader = FlatShader.Value;
		_surface = new Node2D
		{
			Name = StringName.op_Implicit("SCThemeSurface"),
			ShowBehindParent = true
		};
		_copy = new BackBufferCopy
		{
			CopyMode = (CopyModeEnum)0
		};
		_fill = new ColorRect
		{
			Color = Colors.White,
			Material = (Material)(object)_material,
			MouseFilter = (MouseFilterEnum)2,
			FocusMode = (FocusModeEnum)0
		};
		((Node)_surface).TreeEntered += TrackCopy;
		((Node)_surface).TreeExiting += StopTrackingCopy;
		((CanvasItem)_surface).VisibilityChanged += TrackCopy;
		((Node)_surface).AddChild((Node)(object)_copy, false, (InternalMode)0);
		((Node)_surface).AddChild((Node)(object)_fill, false, (InternalMode)0);
		((Node)owner).AddChild((Node)(object)_surface, false, (InternalMode)0);
		((Node)owner).MoveChild((Node)(object)_surface, 0);
		owner.Resized += Resize;
		((Node)owner).TreeEntered += Resize;
		((Node)owner).TreeExiting += StopTrackingCopy;
		((CanvasItem)owner).VisibilityChanged += TrackCopy;
		Resize();
	}

	internal static ModThemeBackdrop For(Control owner)
	{
		return Instances.GetValue(owner, (Control node) => new ModThemeBackdrop(node));
	}

	internal void Update(Color tint, float blur, int radius, bool visible = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		(Color, float, int, bool) tuple = (tint, blur, radius, visible);
		(Color, float, int, bool)? last = _last;
		(Color, float, int, bool) tuple2 = tuple;
		bool hasValue = last.HasValue;
		if (hasValue)
		{
			if (!hasValue)
			{
				return;
			}
			(Color, float, int, bool) valueOrDefault = last.GetValueOrDefault();
			if (valueOrDefault.Item1 == tuple2.Item1 && valueOrDefault.Item2 == tuple2.Item2 && valueOrDefault.Item3 == tuple2.Item3 && valueOrDefault.Item4 == tuple2.Item4)
			{
				return;
			}
		}
		_last = tuple;
		_material.Shader = ((blur > 0f) ? BlurShader.Value : FlatShader.Value);
		_material.SetShaderParameter(StringName.op_Implicit("tint"), Variant.op_Implicit(tint));
		_material.SetShaderParameter(StringName.op_Implicit("corner_radius"), Variant.op_Implicit(radius));
		var (num, padding) = BlurSampling(blur);
		if (blur > 0f)
		{
			_material.SetShaderParameter(StringName.op_Implicit("blur_lod"), Variant.op_Implicit(num));
		}
		_padding = padding;
		((CanvasItem)_surface).Visible = visible && (blur > 0f || tint.A > 0f);
		Resize();
	}

	private void Resize()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)_owner) && !((GodotObject)_owner).IsQueuedForDeletion())
		{
			((Control)_fill).Size = _owner.Size;
			_material.SetShaderParameter(StringName.op_Implicit("surface_size"), Variant.op_Implicit(_owner.Size));
			TrackCopy();
		}
	}

	private void TrackCopy()
	{
		if (((Node)_owner).IsInsideTree() && ((CanvasItem)_surface).IsVisibleInTree())
		{
			(Color, float, int, bool)? last = _last;
			if (last.HasValue)
			{
				(Color, float, int, bool) valueOrDefault = last.GetValueOrDefault();
				if (valueOrDefault.Item2 > 0f && valueOrDefault.Item4)
				{
					if (!_trackingCopy)
					{
						_trackingCopy = true;
						RenderingServer.FramePreDraw += SyncCopy;
					}
					SyncCopy();
					return;
				}
			}
		}
		StopTrackingCopy();
	}

	private void StopTrackingCopy()
	{
		if (_trackingCopy)
		{
			RenderingServer.FramePreDraw -= SyncCopy;
		}
		_trackingCopy = false;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_copy))
		{
			_copy.CopyMode = (CopyModeEnum)0;
		}
	}

	private void SyncCopy()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Invalid comparison between Unknown and I8
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)_owner) || ((GodotObject)_owner).IsQueuedForDeletion() || !((Node)_owner).IsInsideTree())
		{
			return;
		}
		Transform2D toViewport = ((CanvasItem)_owner).GetViewportTransform() * ((CanvasItem)_owner).GetGlobalTransform();
		if (Math.Abs(((Transform2D)(ref toViewport)).Determinant()) < 1E-06f || _owner.Size.X <= 0f || _owner.Size.Y <= 0f)
		{
			_copy.CopyMode = (CopyModeEnum)0;
			return;
		}
		var (val, val2) = CopyGeometry(_owner.Size, toViewport, _padding);
		if (((Node2D)_copy).Transform != val2)
		{
			((Node2D)_copy).Transform = val2;
		}
		if (_copy.Rect != val)
		{
			_copy.Rect = val;
		}
		if ((long)_copy.CopyMode != 1)
		{
			_copy.CopyMode = (CopyModeEnum)1;
		}
	}

	internal static (Rect2 Rect, Transform2D Transform) CopyGeometry(Vector2 size, Transform2D toViewport, float padding)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Rect2 val = toViewport * new Rect2(Vector2.Zero, size);
		Vector2 val2 = ((Rect2)(ref val)).Position;
		Vector2 val3 = ((Vector2)(ref val2)).Floor();
		val2 = ((Rect2)(ref val)).End;
		Vector2 val4 = ((Vector2)(ref val2)).Ceil();
		Rect2 val5 = new Rect2(val3, val4 - val3);
		return (Rect: ((Rect2)(ref val5)).Grow(padding), Transform: ((Transform2D)(ref toViewport)).AffineInverse());
	}

	internal static (float Lod, float Padding) BlurSampling(float strength)
	{
		float num = (float.IsFinite(strength) ? Math.Clamp(strength, 0f, 5f) : 0f);
		return (Lod: num, Padding: 4f * (MathF.Pow(2f, MathF.Ceiling(num)) - 1f) + 2f);
	}
}
