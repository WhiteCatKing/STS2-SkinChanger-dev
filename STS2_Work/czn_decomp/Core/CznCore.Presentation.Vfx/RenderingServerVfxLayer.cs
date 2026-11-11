using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace CznCore.Presentation.Vfx;

internal sealed class RenderingServerVfxLayer : Node2D
{
	private readonly List<VfxInstance> _pool = new List<VfxInstance>();

	private readonly List<VfxInstance> _drawScratch = new List<VfxInstance>();

	private static readonly Comparison<VfxInstance> SortByOrder = (VfxInstance a, VfxInstance b) => a.SortingOrder.CompareTo(b.SortingOrder);

	private Rid _normalCanvasItem;

	private Rid _additiveCanvasItem;

	private CanvasItemMaterial? _additiveMaterial;

	private bool _canvasReady;

	private bool _clearedWhileEmpty;

	private int _capacity;

	private bool _poolExhaustedWarned;

	private readonly Stopwatch _frameStopwatch = new Stopwatch();

	private int _peakActive;

	private double _lastUpdateMs;

	private double _peakUpdateMs;

	private double _statsAccum;

	public bool Enabled { get; set; } = true;

	public int PrewarmCount { get; set; } = 128;

	public int MaxCapacity { get; set; } = 512;

	public int BaseZIndex { get; set; }

	public float StatsLogIntervalSeconds { get; set; }

	public int ActiveCount { get; private set; }

	public int Capacity => _capacity;

	public double LastUpdateMs => _lastUpdateMs;

	public override void _Ready()
	{
		EnsureCanvasItems();
		Prewarm(PrewarmCount);
	}

	public override void _Process(double delta)
	{
		if (Enabled && _canvasReady)
		{
			_frameStopwatch.Restart();
			UpdateAndDraw((float)delta);
			_frameStopwatch.Stop();
			_lastUpdateMs = _frameStopwatch.Elapsed.TotalMilliseconds;
			if (_lastUpdateMs > _peakUpdateMs)
			{
				_peakUpdateMs = _lastUpdateMs;
			}
			if (ActiveCount > _peakActive)
			{
				_peakActive = ActiveCount;
			}
			MaybeLogStats(delta);
		}
	}

	public RenderingServerVfxLayer()
	{
		((Node)this).TreeExiting += ReleaseOwnedResources;
	}

	private void ReleaseOwnedResources()
	{
		ClearAll();
		_drawScratch.Clear();
		FreeCanvasItems();
	}

	public bool PlayVfx(Texture2D texture, Vector2 position, float rotation, Vector2 scale, Color color, float lifetime)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		return PlayVfx(new VfxSpawnParams
		{
			Texture = texture,
			Position = position,
			Rotation = rotation,
			Scale = scale,
			Modulate = color,
			Lifetime = lifetime
		});
	}

	public bool PlayVfx(VfxSpawnParams p)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		if (!Enabled || p.Texture == null)
		{
			return false;
		}
		EnsureCanvasItems();
		VfxInstance vfxInstance = RentInstance();
		if (vfxInstance == null)
		{
			if (!_poolExhaustedWarned)
			{
				_poolExhaustedWarned = true;
				SkinLog.Warn("RenderingServerVfxLayer pool exhausted (capacity=" + _capacity + "); spawn dropped. Raise MaxCapacity or PrewarmCount.");
			}
			return false;
		}
		Vector2 size = ((p.Size != Vector2.Zero) ? p.Size : p.Texture.GetSize());
		vfxInstance.Active = true;
		vfxInstance.Texture = p.Texture;
		vfxInstance.TextureRid = ((Resource)p.Texture).GetRid();
		vfxInstance.Position = p.Position;
		vfxInstance.Rotation = p.Rotation;
		vfxInstance.Scale = p.Scale;
		vfxInstance.Modulate = p.Modulate;
		vfxInstance.Lifetime = ((p.Lifetime > 0f) ? p.Lifetime : 0.0001f);
		vfxInstance.Age = 0f;
		vfxInstance.Size = size;
		vfxInstance.SourceRect = p.SourceRect;
		vfxInstance.SortingOrder = p.SortingOrder;
		vfxInstance.AdditiveBlend = p.AdditiveBlend;
		vfxInstance.FadeMode = p.FadeMode;
		vfxInstance.ScaleFrom = p.ScaleFrom;
		vfxInstance.ScaleTo = p.ScaleTo;
		vfxInstance.Velocity = p.Velocity;
		vfxInstance.AngularVelocity = p.AngularVelocity;
		vfxInstance.CurrentAlpha = p.Modulate.A;
		ActiveCount++;
		_clearedWhileEmpty = false;
		return true;
	}

	public void ClearAll()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		foreach (VfxInstance item in _pool)
		{
			item.Reset();
		}
		ActiveCount = 0;
		if (_canvasReady)
		{
			RenderingServer.CanvasItemClear(_normalCanvasItem);
			RenderingServer.CanvasItemClear(_additiveCanvasItem);
		}
		_clearedWhileEmpty = true;
	}

	public void WarmUpPipelines(Texture2D texture)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (texture != null)
		{
			EnsureCanvasItems();
			Color modulate = default(Color);
			((Color)(ref modulate))._002Ector(1f, 1f, 1f, 0f);
			Rect2 rect = default(Rect2);
			((Rect2)(ref rect))._002Ector(new Vector2(-1f, -1f), new Vector2(2f, 2f));
			Rid rid = ((Resource)texture).GetRid();
			WarmUpPipelineCanvasItem(_normalCanvasItem, rect, rid, modulate);
			WarmUpPipelineCanvasItem(_additiveCanvasItem, rect, rid, modulate);
			_clearedWhileEmpty = false;
		}
	}

	public void ResetStats()
	{
		_peakActive = 0;
		_peakUpdateMs = 0.0;
		_lastUpdateMs = 0.0;
	}

	public string DescribeStats()
	{
		return "active=" + ActiveCount + " peakActive=" + _peakActive + " capacity=" + _capacity + " lastUpdateMs=" + _lastUpdateMs.ToString("0.###") + " peakUpdateMs=" + _peakUpdateMs.ToString("0.###");
	}

	private void Prewarm(int count)
	{
		int num = Math.Clamp(count, 0, MaxCapacity);
		while (_pool.Count < num)
		{
			_pool.Add(new VfxInstance());
		}
		_capacity = _pool.Count;
	}

	private VfxInstance? RentInstance()
	{
		for (int i = 0; i < _pool.Count; i++)
		{
			if (!_pool[i].Active)
			{
				return _pool[i];
			}
		}
		if (_pool.Count < MaxCapacity)
		{
			VfxInstance vfxInstance = new VfxInstance();
			_pool.Add(vfxInstance);
			_capacity = _pool.Count;
			return vfxInstance;
		}
		return null;
	}

	private void UpdateAndDraw(float delta)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		_drawScratch.Clear();
		int num = 0;
		for (int i = 0; i < _pool.Count; i++)
		{
			VfxInstance vfxInstance = _pool[i];
			if (vfxInstance.Active)
			{
				vfxInstance.Age += delta;
				if (vfxInstance.Age >= vfxInstance.Lifetime)
				{
					vfxInstance.Reset();
					continue;
				}
				float t = vfxInstance.Age / vfxInstance.Lifetime;
				vfxInstance.Position += vfxInstance.Velocity * delta;
				vfxInstance.Rotation += vfxInstance.AngularVelocity * delta;
				vfxInstance.CurrentAlpha = vfxInstance.Modulate.A * FadeFactor(vfxInstance.FadeMode, t);
				num++;
				_drawScratch.Add(vfxInstance);
			}
		}
		ActiveCount = num;
		if (num == 0)
		{
			if (!_clearedWhileEmpty)
			{
				RenderingServer.CanvasItemClear(_normalCanvasItem);
				RenderingServer.CanvasItemClear(_additiveCanvasItem);
				_clearedWhileEmpty = true;
			}
			return;
		}
		_clearedWhileEmpty = false;
		if (_drawScratch.Count > 1)
		{
			_drawScratch.Sort(SortByOrder);
		}
		RenderingServer.CanvasItemClear(_normalCanvasItem);
		RenderingServer.CanvasItemClear(_additiveCanvasItem);
		for (int j = 0; j < _drawScratch.Count; j++)
		{
			DrawInstance(_drawScratch[j]);
		}
	}

	private void DrawInstance(VfxInstance instance)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		Rid val = (instance.AdditiveBlend ? _additiveCanvasItem : _normalCanvasItem);
		float num = Mathf.Lerp(instance.ScaleFrom, instance.ScaleTo, instance.Age / instance.Lifetime);
		Vector2 val2 = instance.Scale * num;
		float num2 = Mathf.Cos(instance.Rotation);
		float num3 = Mathf.Sin(instance.Rotation);
		Vector2 val3 = new Vector2(num2, num3) * val2.X;
		Vector2 val4 = new Vector2(0f - num3, num2) * val2.Y;
		Transform2D val5 = default(Transform2D);
		((Transform2D)(ref val5))._002Ector(val3, val4, instance.Position);
		RenderingServer.CanvasItemAddSetTransform(val, val5);
		Color modulate = instance.Modulate;
		modulate.A = instance.CurrentAlpha;
		Rect2 val6 = default(Rect2);
		((Rect2)(ref val6))._002Ector(-instance.Size * 0.5f, instance.Size);
		if (((Rect2)(ref instance.SourceRect)).Size.X > 0f && ((Rect2)(ref instance.SourceRect)).Size.Y > 0f)
		{
			RenderingServer.CanvasItemAddTextureRectRegion(val, val6, instance.TextureRid, instance.SourceRect, (Color?)modulate, false, true);
		}
		else
		{
			RenderingServer.CanvasItemAddTextureRect(val, val6, instance.TextureRid, false, (Color?)modulate, false);
		}
	}

	private static void WarmUpPipelineCanvasItem(Rid canvasItem, Rect2 rect, Rid texRid, Color modulate)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		RenderingServer.CanvasItemClear(canvasItem);
		RenderingServer.CanvasItemAddSetTransform(canvasItem, Transform2D.Identity);
		RenderingServer.CanvasItemAddTextureRect(canvasItem, rect, texRid, false, (Color?)modulate, false);
	}

	private static float FadeFactor(VfxFadeMode mode, float t)
	{
		switch (mode)
		{
		case VfxFadeMode.FadeOut:
			return 1f - t;
		case VfxFadeMode.FadeInOut:
			if (t < 0.2f)
			{
				return t / 0.2f;
			}
			if (t > 0.65f)
			{
				return (1f - t) / 0.35f;
			}
			return 1f;
		default:
			return 1f;
		}
	}

	private void EnsureCanvasItems()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (!_canvasReady)
		{
			Rid canvasItem = ((CanvasItem)this).GetCanvasItem();
			_normalCanvasItem = RenderingServer.CanvasItemCreate();
			RenderingServer.CanvasItemSetParent(_normalCanvasItem, canvasItem);
			RenderingServer.CanvasItemSetZIndex(_normalCanvasItem, BaseZIndex);
			_additiveMaterial = new CanvasItemMaterial
			{
				BlendMode = (BlendModeEnum)1
			};
			_additiveCanvasItem = RenderingServer.CanvasItemCreate();
			RenderingServer.CanvasItemSetParent(_additiveCanvasItem, canvasItem);
			RenderingServer.CanvasItemSetZIndex(_additiveCanvasItem, BaseZIndex + 1);
			RenderingServer.CanvasItemSetMaterial(_additiveCanvasItem, ((Resource)_additiveMaterial).GetRid());
			_canvasReady = true;
			_clearedWhileEmpty = true;
		}
	}

	private void FreeCanvasItems()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (_canvasReady)
		{
			if (((Rid)(ref _normalCanvasItem)).IsValid)
			{
				RenderingServer.FreeRid(_normalCanvasItem);
				_normalCanvasItem = default(Rid);
			}
			if (((Rid)(ref _additiveCanvasItem)).IsValid)
			{
				RenderingServer.FreeRid(_additiveCanvasItem);
				_additiveCanvasItem = default(Rid);
			}
			CanvasItemMaterial? additiveMaterial = _additiveMaterial;
			if (additiveMaterial != null)
			{
				((GodotObject)additiveMaterial).Dispose();
			}
			_additiveMaterial = null;
			_canvasReady = false;
		}
	}

	private void MaybeLogStats(double delta)
	{
		if (StatsLogIntervalSeconds <= 0f)
		{
			return;
		}
		_statsAccum += delta;
		if (!(_statsAccum < (double)StatsLogIntervalSeconds))
		{
			_statsAccum = 0.0;
			SkinLog.Debug(() => "RenderingServerVfxLayer " + DescribeStats());
		}
	}
}
