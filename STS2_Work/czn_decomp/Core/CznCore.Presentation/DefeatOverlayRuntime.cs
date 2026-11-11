using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznCore.Presentation;

internal sealed class DefeatOverlayRuntime
{
	[CompilerGenerated]
	private sealed class _003CResolvePrewarmCgTexturePaths_003Ed__30 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private DefeatCgOverlayMode mode;

		public DefeatCgOverlayMode _003C_003E3__mode;

		public DefeatOverlayRuntime _003C_003E4__this;

		string IEnumerator<string>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CResolvePrewarmCgTexturePaths_003Ed__30(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			DefeatOverlayRuntime defeatOverlayRuntime = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				switch (mode)
				{
				case DefeatCgOverlayMode.Type1:
					_003C_003E2__current = defeatOverlayRuntime._profile.CgTexturePath;
					_003C_003E1__state = 1;
					return true;
				case DefeatCgOverlayMode.Type2:
					_003C_003E2__current = defeatOverlayRuntime.ResolveType2TexturePath();
					_003C_003E1__state = 2;
					return true;
				case DefeatCgOverlayMode.Random:
					_003C_003E2__current = defeatOverlayRuntime._profile.CgTexturePath;
					_003C_003E1__state = 3;
					return true;
				}
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			case 2:
				_003C_003E1__state = -1;
				break;
			case 3:
			{
				_003C_003E1__state = -1;
				string a = defeatOverlayRuntime.ResolveType2TexturePath();
				if (!string.Equals(a, defeatOverlayRuntime._profile.CgTexturePath, StringComparison.Ordinal))
				{
					_003C_003E2__current = a;
					_003C_003E1__state = 4;
					return true;
				}
				break;
			}
			case 4:
				_003C_003E1__state = -1;
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CResolvePrewarmCgTexturePaths_003Ed__30 _003CResolvePrewarmCgTexturePaths_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CResolvePrewarmCgTexturePaths_003Ed__ = this;
			}
			else
			{
				_003CResolvePrewarmCgTexturePaths_003Ed__ = new _003CResolvePrewarmCgTexturePaths_003Ed__30(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003CResolvePrewarmCgTexturePaths_003Ed__.mode = _003C_003E3__mode;
			return _003CResolvePrewarmCgTexturePaths_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	private readonly Func<string, string> _runtimeNodeName;

	private readonly DefeatPresentationProfile _profile;

	private readonly IDefeatOverlayRuntimeContext _context;

	private readonly Dictionary<string, Texture2D> _cgTextures = new Dictionary<string, Texture2D>(StringComparer.Ordinal);

	public DefeatOverlayRuntime(CharacterSkinProfile skinProfile, DefeatPresentationProfile profile, IDefeatOverlayRuntimeContext context)
		: this(RequireRuntimeNodeName(skinProfile), profile, context)
	{
	}

	private static Func<string, string> RequireRuntimeNodeName(CharacterSkinProfile? skinProfile)
	{
		ArgumentNullException.ThrowIfNull(skinProfile, "skinProfile");
		return skinProfile.RuntimeNodeName;
	}

	public DefeatOverlayRuntime(Func<string, string> runtimeNodeName, DefeatPresentationProfile profile, IDefeatOverlayRuntimeContext context)
	{
		_runtimeNodeName = runtimeNodeName ?? throw new ArgumentNullException("runtimeNodeName");
		_profile = profile ?? throw new ArgumentNullException("profile");
		_context = context ?? throw new ArgumentNullException("context");
	}

	public void Prewarm()
	{
		DefeatCgOverlayMode overlayMode = _context.OverlayMode;
		if (!_context.IsEnabled || overlayMode == DefeatCgOverlayMode.Off)
		{
			return;
		}
		string officialScenePath = _profile.OfficialScenePath;
		if (officialScenePath != null)
		{
			ResourceLoader.Load<PackedScene>(officialScenePath, (string)null, (CacheMode)1);
			return;
		}
		foreach (DefeatSfxCue effectiveSfxCue in _profile.EffectiveSfxCues)
		{
			if (!string.IsNullOrWhiteSpace(effectiveSfxCue.Path))
			{
				_context.SfxPlayer.Prewarm(effectiveSfxCue.Path);
			}
		}
		foreach (string item in ResolvePrewarmCgTexturePaths(overlayMode))
		{
			GetDefeatCgTexture(item);
		}
		foreach (DefeatFrameSequenceLayerProfile effectiveFrameSequenceLayer in _profile.EffectiveFrameSequenceLayers)
		{
			foreach (string framePath in effectiveFrameSequenceLayer.FramePaths)
			{
				GetDefeatCgTexture(framePath);
			}
		}
	}

	public void Play(float startDelaySeconds)
	{
		DefeatCgOverlayMode overlayMode = _context.OverlayMode;
		if (!_context.IsEnabled || overlayMode == DefeatCgOverlayMode.Off)
		{
			return;
		}
		Viewport viewport;
		DefeatCgOverlayMode selectedMode;
		string scenePath;
		try
		{
			viewport = GetOverlayViewport();
			if (viewport == null)
			{
				_context.LogWarn("Defeat overlay skipped: no viewport.");
				return;
			}
			(string, float, DefeatCgOverlayMode) tuple = ResolveCgTextureSelection(overlayMode);
			string item = tuple.Item1;
			float item2 = tuple.Item2;
			selectedMode = tuple.Item3;
			scenePath = _profile.OfficialScenePath;
			if (scenePath != null)
			{
				RunNowOrOneShot((Node)(object)viewport, startDelaySeconds, ShowScene);
			}
			else
			{
				PlayTextureOverlay(viewport, GetDefeatCgTexture(item), item2, selectedMode, startDelaySeconds);
			}
		}
		catch (Exception ex)
		{
			_context.LogWarn("Defeat overlay failed: " + SkinLog.FormatException(ex));
		}
		void ShowScene()
		{
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)viewport) && _context.IsEnabled && _context.OverlayMode != 0)
			{
				PackedScene val = ResourceLoader.Load<PackedScene>(scenePath, (string)null, (CacheMode)1);
				if (val == null)
				{
					_context.LogWarn("Missing defeat scene: " + scenePath);
				}
				else
				{
					CanvasLayer val2 = val.Instantiate<CanvasLayer>((GenEditState)0);
					((Node)val2).Name = StringName.op_Implicit(_runtimeNodeName("BattleDeadOverlayLayer"));
					((GodotObject)val2).Set(StringName.op_Implicit("variant"), Variant.op_Implicit((selectedMode != DefeatCgOverlayMode.Type2) ? 1 : 2));
					_context.ApplySceneSettings((Node)(object)val2);
					((Node)viewport).AddChild((Node)(object)val2, false, (InternalMode)0);
				}
			}
		}
	}

	private static Viewport? GetOverlayViewport()
	{
		NCombatRoom instance = NCombatRoom.Instance;
		Viewport val = ((instance != null) ? ((Node)instance).GetViewport() : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return val;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val2 = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2.Root))
		{
			return (Viewport?)(object)val2.Root;
		}
		return null;
	}

	private void PlayTextureOverlay(Viewport viewport, Texture2D? texture, float textureScaleAdapter, DefeatCgOverlayMode selectedMode, float startDelaySeconds)
	{
		Node parent = (Node)(object)viewport;
		if (startDelaySeconds <= 0.01f)
		{
			ShowOverlay();
		}
		else
		{
			RunNowOrOneShot(parent, startDelaySeconds, ShowOverlay);
		}
		void ShowOverlay()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Expected O, but got Unknown
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Expected O, but got Unknown
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_028d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)parent) && GodotObject.IsInstanceValid((GodotObject)(object)viewport))
			{
				CanvasLayer layer = new CanvasLayer
				{
					Name = StringName.op_Implicit(_runtimeNodeName("BattleDeadOverlayLayer")),
					Layer = 1000
				};
				parent.AddChild((Node)(object)layer, false, (InternalMode)0);
				Rect2 visibleRect = viewport.GetVisibleRect();
				Vector2 size = ((Rect2)(ref visibleRect)).Size;
				Vector2 position = size / 2f;
				float overlayLifetimeSeconds = _profile.OverlayLifetimeSeconds;
				ColorRect val = new ColorRect
				{
					Name = StringName.op_Implicit(_runtimeNodeName("BattleDeadDim")),
					Color = new Color(0f, 0f, 0f, 0f),
					Size = size,
					MouseFilter = (MouseFilterEnum)2,
					ZIndex = 0,
					ZAsRelative = false
				};
				((Node)layer).AddChild((Node)(object)val, false, (InternalMode)0);
				if (_profile.DimAlpha > 0.001f && _profile.DimFadeSeconds > 0.001f)
				{
					Tween obj = ((Node)val).CreateTween();
					obj.SetEase((EaseType)1);
					obj.SetTrans((TransitionType)1);
					obj.TweenProperty((GodotObject)(object)val, NodePath.op_Implicit("color"), Variant.op_Implicit(new Color(0f, 0f, 0f, _profile.DimAlpha)), (double)_profile.DimFadeSeconds);
				}
				else
				{
					val.Color = new Color(0f, 0f, 0f, _profile.DimAlpha);
				}
				Node2D val2 = CreateEffectRoot();
				if (val2 != null)
				{
					val2.Position = position;
					if (_profile.EffectReferenceHeight > 0.001f)
					{
						val2.Scale = Vector2.One * (size.Y / _profile.EffectReferenceHeight);
					}
					((Node)layer).AddChild((Node)(object)val2, false, (InternalMode)0);
					StartEffectLayers(val2, (Node)(object)layer);
				}
				List<Sprite2D> cgSprites = new List<Sprite2D>();
				cgSprites.AddRange(CreateFrameSequenceSprites(size, (Node)(object)layer));
				if (texture != null)
				{
					cgSprites.AddRange(CreateCollapseCgSprites(texture, size, textureScaleAdapter, selectedMode, (Node)(object)layer));
				}
				StartCollapseSfx((Node)(object)layer);
				GodotScheduler.OneShot((Node)(object)layer, overlayLifetimeSeconds, delegate
				{
					foreach (Sprite2D item in cgSprites)
					{
						if (GodotObject.IsInstanceValid((GodotObject)(object)item))
						{
							((CanvasItem)item).Visible = false;
						}
					}
				});
				GodotScheduler.OneShot((Node)(object)layer, overlayLifetimeSeconds + 0.05f, delegate
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)layer))
					{
						((Node)layer).QueueFree();
					}
				});
			}
		}
	}

	private IReadOnlyList<Sprite2D> CreateCollapseCgSprites(Texture2D texture, Vector2 viewportSize, float textureScaleAdapter, DefeatCgOverlayMode selectedMode, Node timerParent)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		List<Sprite2D> list = new List<Sprite2D>();
		Vector2 size = texture.GetSize();
		if (size.X <= 0f || size.Y <= 0f)
		{
			return list;
		}
		float num = MathF.Min(viewportSize.X / size.X, viewportSize.Y / size.Y) * MathF.Max(0.001f, textureScaleAdapter);
		foreach (DefeatCgMotionLayerProfile item in DefeatCgFraming.Reframe((selectedMode == DefeatCgOverlayMode.Type2) ? _profile.EffectiveCgType2MotionLayers : _profile.EffectiveCgMotionLayers, viewportSize, size, DefeatCgFraming.ContentRect(texture), num * _profile.RuntimeCgScaleAdapter))
		{
			Sprite2D val = new Sprite2D
			{
				Name = StringName.op_Implicit(_runtimeNodeName("BattleDeadCg" + item.Name)),
				Texture = texture,
				Centered = true,
				Visible = false,
				ZIndex = item.ZIndex,
				ZAsRelative = false
			};
			if (item.Additive)
			{
				((CanvasItem)val).Material = (Material)(object)CreateAdditiveCanvasMaterial();
			}
			timerParent.AddChild((Node)(object)val, false, (InternalMode)0);
			if (item.Keyframes.Count > 0)
			{
				ApplyCgMotionKey(val, viewportSize, num, item, item.Keyframes[0]);
			}
			StartCgMotion(val, viewportSize, num, item, timerParent);
			list.Add(val);
		}
		return list;
	}

	private IReadOnlyList<Sprite2D> CreateFrameSequenceSprites(Vector2 viewportSize, Node timerParent)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected O, but got Unknown
		List<Sprite2D> list = new List<Sprite2D>();
		foreach (DefeatFrameSequenceLayerProfile effectiveFrameSequenceLayer in _profile.EffectiveFrameSequenceLayers)
		{
			if (effectiveFrameSequenceLayer.FramePaths.Count == 0)
			{
				continue;
			}
			Texture2D firstFrameSequenceTexture = GetFirstFrameSequenceTexture(effectiveFrameSequenceLayer);
			if (firstFrameSequenceTexture == null)
			{
				continue;
			}
			Vector2 size = firstFrameSequenceTexture.GetSize();
			if (!(size.X <= 0f) && !(size.Y <= 0f))
			{
				float num = MathF.Max(viewportSize.X / size.X, viewportSize.Y / size.Y) * MathF.Max(0.001f, effectiveFrameSequenceLayer.RuntimeScaleAdapter);
				Sprite2D val = new Sprite2D
				{
					Name = StringName.op_Implicit(_runtimeNodeName("BattleDeadFrameSequence" + effectiveFrameSequenceLayer.Name)),
					Texture = firstFrameSequenceTexture,
					Centered = true,
					Position = viewportSize / 2f,
					Scale = Vector2.One * num,
					Visible = false,
					ZIndex = effectiveFrameSequenceLayer.ZIndex,
					ZAsRelative = false
				};
				if (effectiveFrameSequenceLayer.Additive)
				{
					((CanvasItem)val).Material = (Material)(object)CreateAdditiveCanvasMaterial();
				}
				timerParent.AddChild((Node)(object)val, false, (InternalMode)0);
				StartFrameSequence(val, effectiveFrameSequenceLayer, timerParent);
				list.Add(val);
			}
		}
		return list;
	}

	private Texture2D? GetFirstFrameSequenceTexture(DefeatFrameSequenceLayerProfile spec)
	{
		foreach (string framePath in spec.FramePaths)
		{
			Texture2D defeatCgTexture = GetDefeatCgTexture(framePath);
			if (defeatCgTexture != null && GodotObject.IsInstanceValid((GodotObject)(object)defeatCgTexture))
			{
				return defeatCgTexture;
			}
		}
		return null;
	}

	private void StartFrameSequence(Sprite2D sprite, DefeatFrameSequenceLayerProfile spec, Node timerParent)
	{
		float num = MathF.Max(0.001f, spec.FrameSeconds);
		float num2 = ((spec.VisibleSeconds > 0.001f) ? spec.VisibleSeconds : ((float)spec.FramePaths.Count * num));
		float num3 = spec.StartDelaySeconds + num2;
		for (int i = 0; i < spec.FramePaths.Count; i++)
		{
			float num4 = spec.StartDelaySeconds + (float)i * num;
			if (num4 > num3)
			{
				break;
			}
			string framePath = spec.FramePaths[i];
			RunNowOrOneShot(timerParent, num4, delegate
			{
				//IL_007f: Unknown result type (might be due to invalid IL or missing references)
				if (GodotObject.IsInstanceValid((GodotObject)(object)sprite))
				{
					Texture2D defeatCgTexture = GetDefeatCgTexture(framePath);
					if (defeatCgTexture != null && GodotObject.IsInstanceValid((GodotObject)(object)defeatCgTexture))
					{
						sprite.Texture = defeatCgTexture;
					}
					((CanvasItem)sprite).Modulate = new Color(1f, 1f, 1f, Math.Clamp(spec.Alpha, 0f, 1f));
					((CanvasItem)sprite).Visible = true;
				}
			});
		}
		GodotScheduler.OneShot(timerParent, MathF.Max(0.05f, num3), delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)sprite))
			{
				((CanvasItem)sprite).Visible = false;
			}
		});
	}

	private void StartCgMotion(Sprite2D sprite, Vector2 viewportSize, float baseScale, DefeatCgMotionLayerProfile spec, Node timerParent)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float num = MathF.Max(0.001f, _profile.CgMotionSeconds);
		for (int i = 0; i < spec.Keyframes.Count; i++)
		{
			DefeatCgMotionKeyProfile key = spec.Keyframes[i];
			float delaySeconds = _profile.CgIntroDelaySeconds + key.Offset * num;
			RunNowOrOneShot(timerParent, delaySeconds, delegate
			{
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				if (GodotObject.IsInstanceValid((GodotObject)(object)sprite))
				{
					ApplyCgMotionKey(sprite, viewportSize, baseScale, spec, key);
				}
			});
			if (i >= spec.Keyframes.Count - 1)
			{
				continue;
			}
			DefeatCgMotionKeyProfile nextKey = spec.Keyframes[i + 1];
			float durationSeconds = (nextKey.Offset - key.Offset) * num;
			if (durationSeconds <= 0.001f)
			{
				continue;
			}
			RunNowOrOneShot(timerParent, delaySeconds, delegate
			{
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				if (GodotObject.IsInstanceValid((GodotObject)(object)sprite))
				{
					TweenCgMotionKey(sprite, viewportSize, baseScale, spec, nextKey, durationSeconds);
				}
			});
		}
	}

	private void ApplyCgMotionKey(Sprite2D sprite, Vector2 viewportSize, float baseScale, DefeatCgMotionLayerProfile spec, DefeatCgMotionKeyProfile key)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		((Node2D)sprite).Position = new Vector2(viewportSize.X * (0.5f + key.OffsetXRatio), viewportSize.Y * (0.5f + key.OffsetYRatio));
		((Node2D)sprite).Scale = Vector2.One * baseScale * key.ScaleMultiplier * _profile.RuntimeCgScaleAdapter;
		((CanvasItem)sprite).Modulate = new Color(spec.Tint.R, spec.Tint.G, spec.Tint.B, key.Alpha * spec.Tint.A);
		((CanvasItem)sprite).Visible = key.Alpha > 0.001f;
	}

	private void TweenCgMotionKey(Sprite2D sprite, Vector2 viewportSize, float baseScale, DefeatCgMotionLayerProfile spec, DefeatCgMotionKeyProfile key, float durationSeconds)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(viewportSize.X * (0.5f + key.OffsetXRatio), viewportSize.Y * (0.5f + key.OffsetYRatio));
		Vector2 val2 = Vector2.One * baseScale * key.ScaleMultiplier * _profile.RuntimeCgScaleAdapter;
		Color val3 = default(Color);
		((Color)(ref val3))._002Ector(spec.Tint.R, spec.Tint.G, spec.Tint.B, key.Alpha * spec.Tint.A);
		((CanvasItem)sprite).Visible = ((CanvasItem)sprite).Visible || key.Alpha > 0.001f;
		Tween obj = ((Node)sprite).CreateTween();
		obj.SetParallel(true);
		obj.TweenProperty((GodotObject)(object)sprite, NodePath.op_Implicit("position"), Variant.op_Implicit(val), (double)durationSeconds);
		obj.TweenProperty((GodotObject)(object)sprite, NodePath.op_Implicit("scale"), Variant.op_Implicit(val2), (double)durationSeconds);
		obj.TweenProperty((GodotObject)(object)sprite, NodePath.op_Implicit("modulate"), Variant.op_Implicit(val3), (double)durationSeconds);
	}

	private static CanvasItemMaterial CreateAdditiveCanvasMaterial()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		CanvasItemMaterial val = new CanvasItemMaterial();
		StringName obj = StringName.op_Implicit("blend_mode");
		int num = 1;
		((GodotObject)val).Set(obj, Variant.From<int>(ref num));
		StringName obj2 = StringName.op_Implicit("light_mode");
		num = 0;
		((GodotObject)val).Set(obj2, Variant.From<int>(ref num));
		return val;
	}

	private void StartCollapseSfx(Node parent)
	{
		foreach (DefeatSfxCue cue in _profile.EffectiveSfxCues)
		{
			if (string.IsNullOrWhiteSpace(cue.Path))
			{
				continue;
			}
			RunNowOrOneShot(parent, MathF.Max(0f, cue.DelaySeconds), delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)parent))
				{
					PlayDefeatSfx(cue);
				}
			});
		}
	}

	private void PlayDefeatSfx(DefeatSfxCue cue)
	{
		try
		{
			if (!_context.SfxPlayer.PlayVolumeDb(cue.Path, cue.VolumeDb))
			{
				_context.LogWarn("Missing defeat sfx " + cue.Name + ": " + cue.Path);
			}
		}
		catch (Exception ex)
		{
			_context.LogWarn("Defeat sfx failed " + cue.Name + ": " + SkinLog.FormatException(ex));
		}
	}

	private void StartEffectLayers(Node2D effectRoot, Node timerParent)
	{
		RunNowOrOneShot(timerParent, MathF.Max(0f, _profile.EffectIntroDelaySeconds), delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)effectRoot))
			{
				PlayEffectLayers((Node)(object)effectRoot, timerParent);
			}
		});
	}

	private void PlayEffectLayers(Node root, Node timerParent)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		foreach (Node child in root.GetChildren(false))
		{
			CanvasItem canvas = (CanvasItem)(object)((child is CanvasItem) ? child : null);
			if (canvas == null)
			{
				continue;
			}
			DefeatEffectLayerProfile defeatEffectLayerProfile = null;
			foreach (DefeatEffectLayerProfile effectLayer in _profile.EffectLayers)
			{
				if (StringName.op_Implicit(effectLayer.Name) == child.Name)
				{
					defeatEffectLayerProfile = effectLayer;
					break;
				}
			}
			if (defeatEffectLayerProfile == null)
			{
				continue;
			}
			canvas.Visible = true;
			if (((GodotObject)child).GetClass() == "SpineSprite")
			{
				try
				{
					SpineAnimationCompat.SetAnimation(new MegaSprite(Variant.op_Implicit((GodotObject)(Node2D)child)).GetAnimationState(), "animation", loop: false);
				}
				catch (Exception ex)
				{
					_context.LogWarn("Defeat effect layer animation failed: " + defeatEffectLayerProfile.Name + " " + SkinLog.FormatException(ex));
				}
			}
			GodotScheduler.OneShot(timerParent, MathF.Max(0.05f, defeatEffectLayerProfile.VisibleSeconds), delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)canvas))
				{
					canvas.Visible = false;
				}
			});
		}
	}

	private static void RunNowOrOneShot(Node parent, float delaySeconds, Action action)
	{
		if (delaySeconds <= 0.001f)
		{
			action();
		}
		else
		{
			GodotScheduler.OneShot(parent, delaySeconds, action);
		}
	}

	private Node2D? CreateEffectRoot()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		try
		{
			Node2D val = new Node2D
			{
				Name = StringName.op_Implicit(_runtimeNodeName("BattleDeadEffectRoot"))
			};
			int num = 0;
			foreach (DefeatEffectLayerProfile effectLayer in _profile.EffectLayers)
			{
				Node val2 = CreateEffectLayerNode(effectLayer);
				if (val2 != null)
				{
					CanvasItem val3 = (CanvasItem)(object)((val2 is CanvasItem) ? val2 : null);
					if (val3 != null)
					{
						val3.Visible = false;
					}
					((Node)val).AddChild(val2, false, (InternalMode)0);
					num++;
				}
			}
			if (num == 0)
			{
				((Node)val).QueueFree();
				return null;
			}
			_context.ApplyEffectMaterial((Node)(object)val);
			_context.ApplyEffectMaterialDeferred((Node)(object)val, 4);
			return val;
		}
		catch (Exception ex)
		{
			_context.LogWarn("Defeat effect root instantiate failed: " + SkinLog.FormatException(ex));
			return null;
		}
	}

	private Node? CreateEffectLayerNode(DefeatEffectLayerProfile spec)
	{
		Node val = CreateEffectLayerFromSkeletonData(spec);
		if (val != null)
		{
			return val;
		}
		Node val2 = CreateEffectLayerFromRawFiles(spec);
		if (val2 != null)
		{
			return val2;
		}
		return CreateEffectFallbackSprite(spec);
	}

	private Node? CreateEffectLayerFromSkeletonData(DefeatEffectLayerProfile spec)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(spec.SkeletonDataPath))
		{
			return null;
		}
		try
		{
			Resource val = ResourceLoader.Load<Resource>(spec.SkeletonDataPath, (string)null, (CacheMode)0);
			Variant val2 = ClassDB.Instantiate(StringName.op_Implicit("SpineSprite"));
			GodotObject obj = ((Variant)(ref val2)).AsGodotObject();
			Node val3 = (Node)(object)((obj is Node) ? obj : null);
			if (val == null || val3 == null)
			{
				return null;
			}
			val3.Name = StringName.op_Implicit(spec.Name);
			StringName obj2 = StringName.op_Implicit("skeleton_data_res");
			GodotObject val4 = (GodotObject)(object)val;
			((GodotObject)val3).Set(obj2, Variant.From<GodotObject>(ref val4));
			ConfigureEffectLayerNode(val3, spec);
			return val3;
		}
		catch (Exception ex)
		{
			_context.LogWarn("Defeat effect layer skeleton data load failed: " + spec.Name + " " + SkinLog.FormatException(ex));
			return null;
		}
	}

	private Node? CreateEffectLayerFromRawFiles(DefeatEffectLayerProfile spec)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(spec.AtlasPath) || string.IsNullOrWhiteSpace(spec.SkeletonPath))
		{
			return null;
		}
		try
		{
			Variant val = ClassDB.Instantiate(StringName.op_Implicit("SpineAtlasResource"));
			GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
			val = ClassDB.Instantiate(StringName.op_Implicit("SpineSkeletonFileResource"));
			GodotObject val3 = ((Variant)(ref val)).AsGodotObject();
			val = ClassDB.Instantiate(StringName.op_Implicit("SpineSkeletonDataResource"));
			GodotObject val4 = ((Variant)(ref val)).AsGodotObject();
			val = ClassDB.Instantiate(StringName.op_Implicit("SpineSprite"));
			GodotObject obj = ((Variant)(ref val)).AsGodotObject();
			Node val5 = (Node)(object)((obj is Node) ? obj : null);
			if (val2 == null || val3 == null || val4 == null || val5 == null)
			{
				return null;
			}
			Variant val6 = val2.Call(StringName.op_Implicit("load_from_atlas_file"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(spec.AtlasPath) });
			Variant val7 = val3.Call(StringName.op_Implicit("load_from_file"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(spec.SkeletonPath) });
			if (((Variant)(ref val6)).AsInt32() != 0 || ((Variant)(ref val7)).AsInt32() != 0)
			{
				_context.LogWarn("Defeat effect layer load failed: " + spec.Name + " atlasErr=" + ((Variant)(ref val6)).AsInt32() + " skelErr=" + ((Variant)(ref val7)).AsInt32());
				return null;
			}
			val4.Set(StringName.op_Implicit("atlas_res"), Variant.From<GodotObject>(ref val2));
			val4.Set(StringName.op_Implicit("skeleton_file_res"), Variant.From<GodotObject>(ref val3));
			val5.Name = StringName.op_Implicit(spec.Name);
			((GodotObject)val5).Set(StringName.op_Implicit("skeleton_data_res"), Variant.From<GodotObject>(ref val4));
			ConfigureEffectLayerNode(val5, spec);
			return val5;
		}
		catch (Exception ex)
		{
			_context.LogWarn("Defeat effect layer instantiate failed: " + spec.Name + " " + SkinLog.FormatException(ex));
			return null;
		}
	}

	private Node? CreateEffectFallbackSprite(DefeatEffectLayerProfile spec)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		if (string.IsNullOrWhiteSpace(spec.PngPath))
		{
			return null;
		}
		try
		{
			Image val = Image.LoadFromFile(spec.PngPath);
			if (val == null || val.IsEmpty())
			{
				return null;
			}
			Sprite2D val2 = new Sprite2D
			{
				Name = StringName.op_Implicit(spec.Name),
				Texture = (Texture2D)(object)ImageTexture.CreateFromImage(val),
				Centered = true,
				Scale = Vector2.One * spec.FallbackScale
			};
			ConfigureEffectLayerNode((Node)(object)val2, spec);
			_context.LogWarn("Defeat effect layer using PNG fallback: " + spec.Name);
			return (Node?)(object)val2;
		}
		catch (Exception ex)
		{
			_context.LogWarn("Defeat effect layer PNG fallback failed: " + spec.Name + " " + SkinLog.FormatException(ex));
			return null;
		}
	}

	private void ConfigureEffectLayerNode(Node node, DefeatEffectLayerProfile spec)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		StringName obj = StringName.op_Implicit("z_index");
		int zIndex = spec.ZIndex;
		((GodotObject)node).Set(obj, Variant.From<int>(ref zIndex));
		StringName obj2 = StringName.op_Implicit("z_as_relative");
		bool flag = false;
		((GodotObject)node).Set(obj2, Variant.From<bool>(ref flag));
		Node2D val = (Node2D)(object)((node is Node2D) ? node : null);
		if (val != null && MathF.Abs(spec.RuntimeScale - 1f) > 0.001f)
		{
			val.Scale *= spec.RuntimeScale;
		}
		CanvasItem val2 = (CanvasItem)(object)((node is CanvasItem) ? node : null);
		if (val2 != null)
		{
			val2.SelfModulate = new Color(1f, 1f, 1f, spec.Alpha);
		}
		_context.ApplyNodeMaterial(node);
	}

	[IteratorStateMachine(typeof(_003CResolvePrewarmCgTexturePaths_003Ed__30))]
	private IEnumerable<string> ResolvePrewarmCgTexturePaths(DefeatCgOverlayMode mode)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CResolvePrewarmCgTexturePaths_003Ed__30(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__mode = mode
		};
	}

	private (string? TexturePath, float ScaleAdapter, DefeatCgOverlayMode SelectedMode) ResolveCgTextureSelection(DefeatCgOverlayMode mode)
	{
		return mode switch
		{
			DefeatCgOverlayMode.Off => (TexturePath: null, ScaleAdapter: 1f, SelectedMode: DefeatCgOverlayMode.Off), 
			DefeatCgOverlayMode.Type2 => (TexturePath: ResolveType2TexturePath(), ScaleAdapter: ResolveType2ScaleAdapter(), SelectedMode: DefeatCgOverlayMode.Type2), 
			DefeatCgOverlayMode.Random => (Random.Shared.Next(2) == 0) ? (TexturePath: _profile.CgTexturePath, ScaleAdapter: _profile.CgType1ScaleAdapter, SelectedMode: DefeatCgOverlayMode.Type1) : (TexturePath: ResolveType2TexturePath(), ScaleAdapter: ResolveType2ScaleAdapter(), SelectedMode: DefeatCgOverlayMode.Type2), 
			_ => (TexturePath: _profile.CgTexturePath, ScaleAdapter: _profile.CgType1ScaleAdapter, SelectedMode: DefeatCgOverlayMode.Type1), 
		};
	}

	private string ResolveType2TexturePath()
	{
		if (!string.IsNullOrWhiteSpace(_profile.CgType2TexturePath))
		{
			return _profile.CgType2TexturePath;
		}
		return _profile.CgTexturePath;
	}

	private float ResolveType2ScaleAdapter()
	{
		if (!string.IsNullOrWhiteSpace(_profile.CgType2TexturePath))
		{
			return _profile.CgType2ScaleAdapter;
		}
		return _profile.CgType1ScaleAdapter;
	}

	private Texture2D? GetDefeatCgTexture(string? texturePath)
	{
		if (string.IsNullOrWhiteSpace(texturePath))
		{
			return null;
		}
		if (_cgTextures.TryGetValue(texturePath, out Texture2D value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			return value;
		}
		Texture2D val = LoadTexture2D(texturePath);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			_cgTextures[texturePath] = val;
		}
		return val;
	}

	private static Texture2D? LoadTexture2D(string texturePath)
	{
		if (string.IsNullOrWhiteSpace(texturePath))
		{
			return null;
		}
		if (IsRawImagePath(texturePath))
		{
			Texture2D val = LoadImageTexture2D(texturePath);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				return val;
			}
		}
		try
		{
			return ResourceLoader.Load<Texture2D>(texturePath, (string)null, (CacheMode)1);
		}
		catch
		{
		}
		if (!IsRawImagePath(texturePath))
		{
			return LoadImageTexture2D(texturePath);
		}
		return null;
	}

	private static bool IsRawImagePath(string texturePath)
	{
		if (!texturePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && !texturePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && !texturePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
		{
			return texturePath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static Texture2D? LoadImageTexture2D(string texturePath)
	{
		try
		{
			if (!FileAccess.FileExists(texturePath))
			{
				return null;
			}
			Image val = Image.LoadFromFile(texturePath);
			if (val == null || val.IsEmpty())
			{
				return null;
			}
			return (Texture2D?)(object)ImageTexture.CreateFromImage(val);
		}
		catch
		{
			return null;
		}
	}
}
