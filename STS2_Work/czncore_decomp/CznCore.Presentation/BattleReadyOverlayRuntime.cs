using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;

namespace CznCore.Presentation;

internal sealed class BattleReadyOverlayRuntime<TCard> where TCard : class
{
	private sealed class CardPlaySuppression
	{
	}

	[CompilerGenerated]
	private sealed class _003CBattleReadyIdleFireRoots_003Ed__85 : IEnumerable<Node2D>, IEnumerable, IEnumerator<Node2D>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Node2D _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public BattleReadyOverlayRuntime<TCard> _003C_003E4__this;

		Node2D IEnumerator<Node2D>.Current
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
		public _003CBattleReadyIdleFireRoots_003Ed__85(int _003C_003E1__state)
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
			BattleReadyOverlayRuntime<TCard> battleReadyOverlayRuntime = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (battleReadyOverlayRuntime._idleEffectsBackRoot != null && GodotObject.IsInstanceValid((GodotObject)(object)battleReadyOverlayRuntime._idleEffectsBackRoot))
				{
					_003C_003E2__current = battleReadyOverlayRuntime._idleEffectsBackRoot;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_005a;
			case 1:
				_003C_003E1__state = -1;
				goto IL_005a;
			case 2:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_005a:
				if (battleReadyOverlayRuntime._idleEffectsFrontRoot != null && GodotObject.IsInstanceValid((GodotObject)(object)battleReadyOverlayRuntime._idleEffectsFrontRoot))
				{
					_003C_003E2__current = battleReadyOverlayRuntime._idleEffectsFrontRoot;
					_003C_003E1__state = 2;
					return true;
				}
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
		IEnumerator<Node2D> IEnumerable<Node2D>.GetEnumerator()
		{
			_003CBattleReadyIdleFireRoots_003Ed__85 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CBattleReadyIdleFireRoots_003Ed__85(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node2D>)this).GetEnumerator();
		}
	}

	private readonly BattleReadyOverlayProfile _profile;

	private readonly IBattleReadyOverlayRuntimeContext<TCard> _context;

	private readonly HashSet<string> _missingAnimationsLogged = new HashSet<string>(StringComparer.Ordinal);

	private readonly ConditionalWeakTable<TCard, CardPlaySuppression> _cardPlaySuppressions = new ConditionalWeakTable<TCard, CardPlaySuppression>();

	private PackedScene? _scene;

	private PackedScene? _idleEffectsScene;

	private Resource? _skeletonData;

	private Node? _node;

	private ProcessModeEnum _visibleProcessMode;

	private Node2D? _transformNode;

	private MegaSprite? _sprite;

	private Node2D? _idleEffectsBackRoot;

	private Node2D? _idleEffectsFrontRoot;

	private TCard? _activeHoldCard;

	private Vector2 _basePosition;

	private Vector2 _baseScale = Vector2.One;

	private Tween? _moveTween;

	private Tween? _hideDelayTween;

	private readonly List<Tween> _idleLayerTweens = new List<Tween>();

	private int _sceneLoadAttempted;

	private int _idleEffectsSceneLoadAttempted;

	private int _skeletonLoadAttempted;

	private int _visibilityToken;

	private bool _isShown;

	private bool _outPlaying;

	private bool _cardUsePlaying;

	private bool _hasInAnim;

	private bool _hasIdleAnim;

	private bool _hasOutAnim;

	private bool _hasAttackAnim;

	private bool _hasCastingAnim;

	private string? _lastFirst;

	private string? _lastNextLoop;

	private BattleReadyOverlayAnimations Animations => _profile.Animations;

	public BattleReadyOverlayRuntime(BattleReadyOverlayProfile profile, IBattleReadyOverlayRuntimeContext<TCard> context)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		_profile = profile;
		_context = context;
	}

	public bool CanShow(TCard? card)
	{
		return _context.PresentationPolicy.CanShow(card);
	}

	public void Preload()
	{
		if (_context.PresentationPolicy.IsEnabled)
		{
			GetScene();
			GetSkeletonData();
			GetIdleEffectsScene();
			EnsureCreated();
		}
	}

	public void ShowIdleForCard(TCard? card)
	{
		try
		{
			if (_cardUsePlaying || !CanShow(card) || card == null || !EnsureCreated() || _sprite == null)
			{
				return;
			}
			if (!_hasIdleAnim)
			{
				LogMissingAnimation(Animations.Idle);
				return;
			}
			bool flag = IsCanvasVisible();
			bool restartAnimation = !flag || !IsBattleReadyIdleFireVisible();
			_activeHoldCard = card;
			_outPlaying = false;
			_cardUsePlaying = false;
			int token = ++_visibilityToken;
			KillTween(ref _hideDelayTween);
			SetCanvasVisible(visible: true);
			SetBattleReadyIdleFireVisible(visible: true, restartAnimation);
			if (_hasInAnim && !flag)
			{
				_isShown = true;
				ApplyTransformFromSettings(forceTargetPosition: true);
				PlaySequence(Animations.In, Animations.Idle);
			}
			else
			{
				ApplyScaleAndOpacityFromSettings();
				PlaySequence(Animations.Idle, Animations.Idle);
				StartSlideIn(token, !flag);
			}
		}
		catch (Exception ex)
		{
			_context.LogWarn("Battle ready overlay idle failed: " + SkinLog.FormatException(ex));
			Cleanup();
		}
	}

	public void HideForCard(TCard? card)
	{
		if (card != null && card == _activeHoldCard)
		{
			_activeHoldCard = null;
			ScheduleOut(_profile.OutDelaySeconds);
		}
	}

	public void CancelForCard(TCard? card)
	{
		if (card != null && card == _activeHoldCard)
		{
			_activeHoldCard = null;
			ScheduleOut(_profile.CancelOutDelaySeconds);
		}
	}

	public void SuppressNextPlayForCard(TCard? card)
	{
		if (card != null && _context.IsTargetCard(card))
		{
			_cardPlaySuppressions.Remove(card);
			_cardPlaySuppressions.Add(card, new CardPlaySuppression());
		}
	}

	public void CompleteCardExecution(TCard card)
	{
		_cardPlaySuppressions.Remove(card);
	}

	public void PlayForCard(TCard card)
	{
		try
		{
			if (ConsumeCardPlaySuppression(card))
			{
				_context.LogInfo("Battle ready overlay card play suppressed.");
			}
			else if (CanShow(card))
			{
				string cardAnimation = GetCardAnimation(card);
				if (cardAnimation != null)
				{
					PlayAnimationForCard(card, cardAnimation);
				}
			}
		}
		catch (Exception ex)
		{
			_context.LogWarn("Battle ready overlay play failed: " + SkinLog.FormatException(ex));
			Cleanup();
		}
	}

	private bool ConsumeCardPlaySuppression(TCard card)
	{
		if (!_cardPlaySuppressions.TryGetValue(card, out CardPlaySuppression _))
		{
			return false;
		}
		_cardPlaySuppressions.Remove(card);
		return true;
	}

	public void PlayAnimationForCard(TCard card, string animationName)
	{
		try
		{
			if (!CanShow(card) || !EnsureCreated() || string.IsNullOrWhiteSpace(animationName) || _sprite == null)
			{
				return;
			}
			bool flag = IsCanvasVisible();
			bool isShown = _isShown;
			_activeHoldCard = null;
			_outPlaying = false;
			_cardUsePlaying = true;
			_isShown = true;
			int token = ++_visibilityToken;
			KillTween(ref _hideDelayTween);
			if (!flag)
			{
				KillTween(ref _moveTween);
			}
			SetCanvasVisible(visible: true);
			SetBattleReadyIdleFireVisible(visible: false, restartAnimation: false);
			ApplyTransformForCardUse(flag, isShown, token);
			if (!HasAnimation(animationName))
			{
				LogMissingAnimation(animationName);
				_cardUsePlaying = false;
				ScheduleOut(0f);
				return;
			}
			long startTimestamp = _context.TimingProbe.StartTimingProbe();
			PlaySingle(animationName);
			_context.TimingProbe.LogTimingProbe("battle_ready_play:" + animationName, startTimestamp);
			if (_profile.CardUseOutDelaySeconds >= 0f)
			{
				ScheduleCardUseOut(_profile.CardUseOutDelaySeconds);
			}
			_context.LogInfo("Battle ready overlay card anim=" + animationName);
		}
		catch (Exception ex)
		{
			_context.LogWarn("Battle ready overlay play failed: " + SkinLog.FormatException(ex));
			Cleanup();
		}
	}

	public void Cleanup(bool releaseResources = true)
	{
		StopIdleLayerTweens();
		try
		{
			KillTween(ref _hideDelayTween);
			KillTween(ref _moveTween);
			if (_node != null && GodotObject.IsInstanceValid((GodotObject)(object)_node))
			{
				if (!releaseResources && _sprite != null)
				{
					HideNodeForReuse();
				}
				else
				{
					Node? node = _node;
					CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
					if (val != null)
					{
						val.Hide();
					}
					_node.QueueFree();
				}
			}
		}
		catch
		{
		}
		if (releaseResources || _sprite == null || _node == null || !GodotObject.IsInstanceValid((GodotObject)(object)_node))
		{
			_node = null;
			_transformNode = null;
			_sprite = null;
			_idleEffectsBackRoot = null;
			_idleEffectsFrontRoot = null;
		}
		_activeHoldCard = null;
		_isShown = false;
		_outPlaying = false;
		_cardUsePlaying = false;
		_lastFirst = null;
		_lastNextLoop = null;
		if (releaseResources)
		{
			ReleaseLoadedResources();
		}
	}

	public void ApplyTransformFromSettings()
	{
		ApplyTransformFromSettings(_isShown || _cardUsePlaying || IsCanvasVisible());
	}

	private void ApplyTransformFromSettings(bool forceTargetPosition)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (_transformNode != null && GodotObject.IsInstanceValid((GodotObject)(object)_transformNode))
		{
			ApplyScaleAndOpacityFromSettings();
			_transformNode.Position = (forceTargetPosition ? TargetPosition() : HiddenPosition());
		}
	}

	private void ApplyTransformForCardUse(bool wasVisible, bool wasShown, int token)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (_transformNode != null && GodotObject.IsInstanceValid((GodotObject)(object)_transformNode))
		{
			ApplyScaleAndOpacityFromSettings();
			if (!wasVisible)
			{
				_transformNode.Position = TargetPosition();
			}
			else if (!wasShown)
			{
				TweenTo(TargetPosition(), hideWhenDone: false, token, _profile.CardUseSlideDurationSeconds);
			}
		}
	}

	private void ApplyScaleAndOpacityFromSettings()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (_transformNode != null && GodotObject.IsInstanceValid((GodotObject)(object)_transformNode))
		{
			_transformNode.Scale = _baseScale * _context.BattleReadyScale;
			ApplyOpacity();
		}
	}

	private void ApplyOpacity()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Node? node = _node;
		CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			Color modulate = val.Modulate;
			val.Modulate = new Color(modulate.R, modulate.G, modulate.B, Mathf.Clamp(_context.BattleReadyOpacity, 0.5f, 1f));
		}
	}

	private bool EnsureCreated()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		if (_node != null && GodotObject.IsInstanceValid((GodotObject)(object)_node) && _sprite != null)
		{
			return EnsureNodeAttached();
		}
		long startTimestamp = _context.TimingProbe.StartTimingProbe();
		try
		{
			Cleanup(releaseResources: false);
			PackedScene scene = GetScene();
			if (scene == null)
			{
				return false;
			}
			Node val = BattleReadyOverlayScreen.Resolve(_context.GetOverlayParent());
			if (val == null)
			{
				return false;
			}
			Node node = scene.Instantiate((GenEditState)0);
			Node val2 = FindSpineSpriteRecursive(node) ?? node;
			if (((GodotObject)val2).GetClass() != "SpineSprite")
			{
				_context.LogWarn("Battle ready overlay scene has no SpineSprite.");
				node.QueueFree();
				return false;
			}
			MegaSprite val3 = new MegaSprite(Variant.op_Implicit((GodotObject)(object)val2));
			Resource skeletonData = GetSkeletonData();
			if (skeletonData != null)
			{
				GodotObject val4 = (GodotObject)(object)skeletonData;
				val3.SetSkeletonDataRes(new MegaSkeletonDataResource(Variant.From<GodotObject>(ref val4)));
			}
			val3.ConnectAnimationCompleted(Callable.From<GodotObject, GodotObject, GodotObject>((Action<GodotObject, GodotObject, GodotObject>)delegate
			{
				OnAnimationCompleted(node);
			}));
			Node obj = node;
			CanvasItem val5 = (CanvasItem)(object)((obj is CanvasItem) ? obj : null);
			if (val5 != null)
			{
				val5.Visible = false;
				val5.ZIndex = 0;
			}
			val.AddChild(node, false, (InternalMode)0);
			_node = node;
			_visibleProcessMode = node.ProcessMode;
			node.ProcessMode = (ProcessModeEnum)4;
			ref Node2D? transformNode = ref _transformNode;
			Node obj2 = node;
			transformNode = (Node2D?)(object)(((obj2 is Node2D) ? obj2 : null) ?? ((val2 is Node2D) ? val2 : null));
			if (_transformNode != null)
			{
				_basePosition = _transformNode.Position;
				_baseScale = _transformNode.Scale;
			}
			_sprite = val3;
			InitAnimationCache(val3);
			AttachBattleReadyIdleEffects(node, val2);
			ApplyTransformFromSettings(forceTargetPosition: false);
			return true;
		}
		finally
		{
			_context.TimingProbe.LogTimingProbe("battle_ready_create", startTimestamp);
		}
	}

	private bool EnsureNodeAttached()
	{
		if (_node == null || !GodotObject.IsInstanceValid((GodotObject)(object)_node))
		{
			return false;
		}
		Node val = BattleReadyOverlayScreen.Resolve(_context.GetOverlayParent());
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return false;
		}
		Node parent = _node.GetParent();
		if (parent == val)
		{
			return true;
		}
		if (parent != null)
		{
			parent.RemoveChild(_node);
		}
		val.AddChild(_node, false, (InternalMode)0);
		return true;
	}

	private void HideNodeForReuse()
	{
		SetBattleReadyIdleFireVisible(visible: false, restartAnimation: false);
		SetCanvasVisible(visible: false);
		if (_transformNode != null && GodotObject.IsInstanceValid((GodotObject)(object)_transformNode))
		{
			ApplyTransformFromSettings(forceTargetPosition: false);
		}
		Node? node = _node;
		Node val = ((node != null) ? node.GetParent() : null);
		if (val != null && _node != null)
		{
			val.RemoveChild(_node);
		}
	}

	private PackedScene? GetScene()
	{
		if (_scene != null)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)_scene))
			{
				return _scene;
			}
			_scene = null;
			Interlocked.Exchange(ref _sceneLoadAttempted, 0);
		}
		if (Interlocked.Exchange(ref _sceneLoadAttempted, 1) != 0)
		{
			return null;
		}
		long startTimestamp = _context.TimingProbe.StartTimingProbe();
		_scene = ResourceLoader.Load<PackedScene>(_profile.ScenePath, (string)null, (CacheMode)1);
		_context.TimingProbe.LogTimingProbe("battle_ready_scene_load", startTimestamp);
		if (_scene == null)
		{
			_context.LogWarn("Missing battle ready overlay scene: " + _profile.ScenePath);
		}
		return _scene;
	}

	private Resource? GetSkeletonData()
	{
		if (_skeletonData != null)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)_skeletonData))
			{
				return _skeletonData;
			}
			_skeletonData = null;
			Interlocked.Exchange(ref _skeletonLoadAttempted, 0);
		}
		if (Interlocked.Exchange(ref _skeletonLoadAttempted, 1) != 0)
		{
			return null;
		}
		long startTimestamp = _context.TimingProbe.StartTimingProbe();
		_skeletonData = ResourceLoader.Load<Resource>(_profile.SkeletonDataPath, (string)null, (CacheMode)1);
		_context.TimingProbe.LogTimingProbe("battle_ready_skeleton_load", startTimestamp);
		if (_skeletonData == null)
		{
			_context.LogWarn("Missing battle ready skeleton data: " + _profile.SkeletonDataPath);
		}
		return _skeletonData;
	}

	private PackedScene? GetIdleEffectsScene()
	{
		if (_idleEffectsScene != null)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)_idleEffectsScene))
			{
				return _idleEffectsScene;
			}
			_idleEffectsScene = null;
			Interlocked.Exchange(ref _idleEffectsSceneLoadAttempted, 0);
		}
		if (Interlocked.Exchange(ref _idleEffectsSceneLoadAttempted, 1) != 0)
		{
			return null;
		}
		BattleReadyOverlayIdleEffectPlan idleEffect = _profile.IdleEffect;
		if (idleEffect == null || idleEffect.Source.Kind != 0 || string.IsNullOrEmpty(idleEffect.Source.ScenePath))
		{
			return null;
		}
		long startTimestamp = _context.TimingProbe.StartTimingProbe();
		_idleEffectsScene = ResourceLoader.Load<PackedScene>(idleEffect.Source.ScenePath, (string)null, (CacheMode)1);
		_context.TimingProbe.LogTimingProbe("battle_ready_idle_effect_scene_load", startTimestamp);
		if (_idleEffectsScene == null)
		{
			_context.LogWarn("Missing battle ready idle fire scene: " + idleEffect.Source.ScenePath);
		}
		return _idleEffectsScene;
	}

	private void ReleaseLoadedResources()
	{
		_scene = null;
		_idleEffectsScene = null;
		_skeletonData = null;
		Interlocked.Exchange(ref _sceneLoadAttempted, 0);
		Interlocked.Exchange(ref _idleEffectsSceneLoadAttempted, 0);
		Interlocked.Exchange(ref _skeletonLoadAttempted, 0);
		_missingAnimationsLogged.Clear();
	}

	private static Node? FindSpineSpriteRecursive(Node root)
	{
		if (((GodotObject)root).GetClass() == "SpineSprite")
		{
			return root;
		}
		foreach (Node child in root.GetChildren(false))
		{
			Node val = FindSpineSpriteRecursive(child);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	private string? GetCardAnimation(TCard card)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		CombatPresentationCardRoute route = _context.ResolveRoute(card);
		if (_profile.TryGetCardAnimation(route, out string animationName))
		{
			return animationName;
		}
		CardModel val = (CardModel)(object)((card is CardModel) ? card : null);
		if (val == null || !_profile.TryGetCardAnimation(LeftSideCardAnimationResolver.Resolve(val.Type), out animationName))
		{
			return null;
		}
		return animationName;
	}

	private void InitAnimationCache(MegaSprite sprite)
	{
		_hasInAnim = sprite.HasAnimation(Animations.In);
		_hasIdleAnim = sprite.HasAnimation(Animations.Idle);
		_hasOutAnim = sprite.HasAnimation(Animations.Out);
		_hasAttackAnim = sprite.HasAnimation(Animations.Attack);
		_hasCastingAnim = sprite.HasAnimation(Animations.Casting);
		_lastFirst = null;
		_lastNextLoop = null;
	}

	private bool HasAnimation(string anim)
	{
		if (string.Equals(anim, Animations.In, StringComparison.Ordinal))
		{
			return _hasInAnim;
		}
		if (string.Equals(anim, Animations.Idle, StringComparison.Ordinal))
		{
			return _hasIdleAnim;
		}
		if (string.Equals(anim, Animations.Out, StringComparison.Ordinal))
		{
			return _hasOutAnim;
		}
		if (string.Equals(anim, Animations.Attack, StringComparison.Ordinal))
		{
			return _hasAttackAnim;
		}
		if (string.Equals(anim, Animations.Casting, StringComparison.Ordinal))
		{
			return _hasCastingAnim;
		}
		MegaSprite? sprite = _sprite;
		if (sprite == null)
		{
			return false;
		}
		return sprite.HasAnimation(anim);
	}

	private void PlaySequence(string first, string nextLoop)
	{
		if (_sprite == null)
		{
			return;
		}
		MegaAnimationState animationState = _sprite.GetAnimationState();
		if (!HasAnimation(first))
		{
			LogMissingAnimation(first);
			if (!HasAnimation(nextLoop))
			{
				LogMissingAnimation(nextLoop);
				return;
			}
			SpineAnimationCompat.SetAnimation(animationState, nextLoop);
			_lastFirst = nextLoop;
			_lastNextLoop = null;
		}
		else if (string.Equals(first, nextLoop, StringComparison.Ordinal))
		{
			if (!string.Equals(_lastFirst, first, StringComparison.Ordinal) || _lastNextLoop != null)
			{
				SpineAnimationCompat.SetAnimation(animationState, first);
				_lastFirst = first;
				_lastNextLoop = null;
			}
		}
		else if (!string.Equals(_lastFirst, first, StringComparison.Ordinal) || !string.Equals(_lastNextLoop, nextLoop, StringComparison.Ordinal))
		{
			SpineAnimationCompat.SetAnimation(animationState, first, loop: false);
			if (HasAnimation(nextLoop))
			{
				SpineAnimationCompat.AddAnimation(animationState, nextLoop);
				_lastNextLoop = nextLoop;
			}
			else
			{
				LogMissingAnimation(nextLoop);
				_lastNextLoop = null;
			}
			_lastFirst = first;
		}
	}

	private bool PlaySingle(string anim)
	{
		if (_sprite == null || !HasAnimation(anim))
		{
			LogMissingAnimation(anim);
			return false;
		}
		SpineAnimationCompat.SetAnimation(_sprite.GetAnimationState(), anim, loop: false);
		_lastFirst = anim;
		_lastNextLoop = null;
		return true;
	}

	private void OnAnimationCompleted(Node createdNode)
	{
		if (_node != createdNode)
		{
			return;
		}
		if (_cardUsePlaying)
		{
			_cardUsePlaying = false;
			if (_profile.CardUseOutDelaySeconds >= 0f)
			{
				BeginOut(_visibilityToken, _profile.CardUseSlideDurationSeconds, allowOutAnimation: false);
			}
			else
			{
				BeginOut(_visibilityToken, _profile.CardUseSlideDurationSeconds);
			}
		}
		else if (_outPlaying)
		{
			_outPlaying = false;
			if (!_isShown)
			{
				SetCanvasVisible(visible: false);
			}
		}
	}

	private void StartSlideIn(int token, bool resetFromHidden)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (_transformNode != null && GodotObject.IsInstanceValid((GodotObject)(object)_transformNode))
		{
			_isShown = true;
			if (resetFromHidden)
			{
				_transformNode.Position = HiddenPosition();
			}
			TweenTo(TargetPosition(), hideWhenDone: false, token, null);
		}
	}

	private void ScheduleOut(float delaySeconds)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		int token = ++_visibilityToken;
		KillTween(ref _hideDelayTween);
		if (_transformNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)_transformNode))
		{
			SetCanvasVisible(visible: false);
			return;
		}
		if (delaySeconds <= 0f)
		{
			BeginOut(token, null);
			return;
		}
		_hideDelayTween = ((Node)_transformNode).CreateTween();
		_hideDelayTween.TweenInterval((double)delaySeconds);
		_hideDelayTween.TweenCallback(Callable.From((Action)delegate
		{
			if (token == _visibilityToken && !_cardUsePlaying)
			{
				BeginOut(token, null);
			}
		}));
	}

	private void ScheduleCardUseOut(float delaySeconds)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		int token = _visibilityToken;
		KillTween(ref _hideDelayTween);
		if (delaySeconds <= 0f)
		{
			BeginCardUseOut(token);
			return;
		}
		if (_transformNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)_transformNode))
		{
			SetCanvasVisible(visible: false);
			return;
		}
		_hideDelayTween = ((Node)_transformNode).CreateTween();
		_hideDelayTween.TweenInterval((double)delaySeconds);
		_hideDelayTween.TweenCallback(Callable.From((Action)delegate
		{
			BeginCardUseOut(token);
		}));
	}

	private void BeginCardUseOut(int token)
	{
		if (token == _visibilityToken && _cardUsePlaying)
		{
			_cardUsePlaying = false;
			BeginOut(token, _profile.CardUseSlideDurationSeconds, allowOutAnimation: false);
		}
	}

	private void BeginOut(int token, float? slideDurationSeconds = null, bool allowOutAnimation = true)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (token != _visibilityToken)
		{
			return;
		}
		_isShown = false;
		KillTween(ref _moveTween);
		if (allowOutAnimation && _hasOutAnim && _sprite != null)
		{
			_outPlaying = true;
			if (PlaySingle(Animations.Out))
			{
				return;
			}
			_outPlaying = false;
		}
		TweenTo(HiddenPosition(), hideWhenDone: true, token, slideDurationSeconds);
	}

	private void TweenTo(Vector2 targetPosition, bool hideWhenDone, int token, float? durationSeconds = null)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		if (_transformNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)_transformNode))
		{
			return;
		}
		float num = Math.Max(0f, durationSeconds ?? _profile.SlideDurationSeconds);
		KillTween(ref _moveTween);
		if (num <= 0.001f)
		{
			_transformNode.Position = targetPosition;
			if (hideWhenDone && token == _visibilityToken && !_isShown && !_cardUsePlaying)
			{
				SetCanvasVisible(visible: false);
			}
			return;
		}
		_moveTween = ((Node)_transformNode).CreateTween();
		_moveTween.SetTrans((TransitionType)7);
		_moveTween.SetEase((EaseType)(hideWhenDone ? 2 : 1));
		_moveTween.TweenProperty((GodotObject)(object)_transformNode, NodePath.op_Implicit("position"), Variant.op_Implicit(targetPosition), (double)num);
		if (!hideWhenDone)
		{
			return;
		}
		_moveTween.TweenCallback(Callable.From((Action)delegate
		{
			if (token == _visibilityToken && !_isShown && !_cardUsePlaying)
			{
				SetCanvasVisible(visible: false);
			}
		}));
	}

	private Vector2 TargetPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		return _basePosition + _profile.LayoutOffset + new Vector2(_context.BattleReadyOffsetX, _context.BattleReadyOffsetY);
	}

	private Vector2 HiddenPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return TargetPosition() + new Vector2(0f - _profile.SlideOffsetX, 0f);
	}

	private bool IsCanvasVisible()
	{
		Node? node = _node;
		CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
		if (val != null)
		{
			return val.Visible;
		}
		return false;
	}

	private void SetCanvasVisible(bool visible)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Node? node = _node;
		CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			val.Visible = visible;
			_node.ProcessMode = (ProcessModeEnum)((!visible) ? 4 : ((long)_visibleProcessMode));
			if (!visible)
			{
				SetBattleReadyIdleFireVisible(visible: false, restartAnimation: false);
			}
		}
	}

	private static void KillTween(ref Tween? tween)
	{
		try
		{
			if (tween != null && GodotObject.IsInstanceValid((GodotObject)(object)tween))
			{
				tween.Kill();
			}
		}
		catch
		{
		}
		tween = null;
	}

	private void AttachBattleReadyIdleEffects(Node overlayRoot, Node spineNode)
	{
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		if ((_idleEffectsBackRoot != null && GodotObject.IsInstanceValid((GodotObject)(object)_idleEffectsBackRoot)) || (_idleEffectsFrontRoot != null && GodotObject.IsInstanceValid((GodotObject)(object)_idleEffectsFrontRoot)))
		{
			return;
		}
		PackedScene idleEffectsScene = GetIdleEffectsScene();
		BattleReadyOverlayIdleEffectPlan idleEffect = _profile.IdleEffect;
		if (idleEffectsScene == null || idleEffect == null)
		{
			return;
		}
		Node val = idleEffectsScene.Instantiate((GenEditState)0);
		Node2D val2 = (Node2D)(object)((val is Node2D) ? val : null);
		if (val2 == null)
		{
			_context.LogWarn("Battle ready idle fire scene root is not Node2D.");
			val.QueueFree();
			return;
		}
		Node2D val3 = _context.DetachIdleEffectLayer((Node)(object)val2, "Back");
		Node2D val4 = _context.DetachIdleEffectLayer((Node)(object)val2, "Front");
		((Node)val2).QueueFree();
		if (val3 == null && val4 == null)
		{
			_context.LogWarn("Battle ready idle fire scene has no Spine layers.");
			return;
		}
		if (val3 != null)
		{
			_idleEffectsBackRoot = AttachBattleReadyIdleEffectLayer(overlayRoot, spineNode, val3, "Back", idleEffect, behindParent: true);
		}
		if (val4 != null)
		{
			_idleEffectsFrontRoot = AttachBattleReadyIdleEffectLayer(overlayRoot, spineNode, val4, "Front", idleEffect, behindParent: false);
		}
		IBattleReadyOverlayRuntimeContext<TCard> context = _context;
		string[] obj = new string[10]
		{
			"Attached battle ready idle fire overlayRoot=",
			StringName.op_Implicit(overlayRoot.Name),
			" spine=",
			StringName.op_Implicit(spineNode.Name),
			" layout=",
			(val3 == null) ? "front_only" : ((val4 == null) ? "back_only" : "back_front"),
			" position=",
			null,
			null,
			null
		};
		Vector2 battleReadyIdleEffectRootPosition = GetBattleReadyIdleEffectRootPosition(idleEffect);
		obj[7] = ((object)(Vector2)(ref battleReadyIdleEffectRootPosition)/*cast due to .constrained prefix*/).ToString();
		obj[8] = " scale=";
		obj[9] = idleEffect.RootScale.ToString();
		context.LogInfo(string.Concat(obj));
	}

	private Node2D AttachBattleReadyIdleEffectLayer(Node overlayRoot, Node spineNode, Node2D layer, string layerName, BattleReadyOverlayIdleEffectPlan idleEffect, bool behindParent)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		((Node)layer).Owner = null;
		layer.Position = Vector2.Zero;
		layer.Scale = Vector2.One;
		layer.Rotation = 0f;
		((CanvasItem)layer).TopLevel = false;
		((CanvasItem)layer).ZAsRelative = true;
		((CanvasItem)layer).ZIndex = 0;
		((CanvasItem)layer).ShowBehindParent = false;
		Node battleReadyIdleEffectParent = GetBattleReadyIdleEffectParent(overlayRoot, spineNode, idleEffect, layerName);
		bool flag = battleReadyIdleEffectParent == spineNode;
		CanvasItem val = (CanvasItem)(object)((spineNode is CanvasItem) ? spineNode : null);
		int num = ((val != null) ? val.ZIndex : 0);
		Node2D val2 = new Node2D
		{
			Name = StringName.op_Implicit(_profile.RuntimeNodeName("IdleFire") + layerName),
			Position = GetBattleReadyIdleEffectRootPosition(idleEffect),
			Scale = Vector2.One * idleEffect.RootScale,
			Rotation = 0f,
			TopLevel = false,
			ZAsRelative = true,
			ZIndex = ((!flag) ? (num + ((!behindParent) ? 1 : (-1))) : ((!behindParent) ? 1 : (-1))),
			ShowBehindParent = (flag && behindParent),
			Visible = false
		};
		CanvasItem val3 = (CanvasItem)(object)((spineNode is CanvasItem) ? spineNode : null);
		if (val3 != null)
		{
			((CanvasItem)val2).VisibilityLayer = val3.VisibilityLayer;
			((CanvasItem)val2).LightMask = val3.LightMask;
		}
		battleReadyIdleEffectParent.AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)layer, false, (InternalMode)0);
		_context.ApplyIdleEffectMaterial((Node)(object)val2);
		_context.ApplyIdleEffectMaterialDeferred((Node)(object)val2, 4);
		return val2;
	}

	private Node GetBattleReadyIdleEffectParent(Node overlayRoot, Node spineNode, BattleReadyOverlayIdleEffectPlan idleEffect, string layerName)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrWhiteSpace(idleEffect.AttachBoneName) && spineNode is Node2D)
		{
			Variant val = ClassDB.Instantiate(StringName.op_Implicit("SpineBoneNode"));
			GodotObject obj = ((Variant)(ref val)).AsGodotObject();
			Node val2 = (Node)(object)((obj is Node) ? obj : null);
			Node2D val3 = (Node2D)(object)((val2 is Node2D) ? val2 : null);
			if (val3 != null)
			{
				((Node)val3).Name = StringName.op_Implicit(_profile.RuntimeNodeName("IdleEffectBone") + layerName);
				((GodotObject)val3).Set(StringName.op_Implicit("bone_name"), Variant.op_Implicit(idleEffect.AttachBoneName));
				spineNode.AddChild((Node)(object)val3, false, (InternalMode)0);
				return (Node)(object)val3;
			}
			if (val2 != null)
			{
				((GodotObject)val2).Free();
			}
			_context.LogWarn("Battle ready idle effect bone attachment is unavailable: " + idleEffect.AttachBoneName);
		}
		if (spineNode is Node2D)
		{
			return spineNode;
		}
		if (overlayRoot is Node2D)
		{
			return overlayRoot;
		}
		return spineNode.GetParent() ?? spineNode;
	}

	private static Vector2 GetBattleReadyIdleEffectRootPosition(BattleReadyOverlayIdleEffectPlan idleEffect)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(idleEffect.RootPositionX, idleEffect.RootPositionY);
	}

	private void SetBattleReadyIdleFireVisible(bool visible, bool restartAnimation)
	{
		if (!visible || restartAnimation)
		{
			StopIdleLayerTweens();
		}
		bool flag = false;
		foreach (Node2D item in BattleReadyIdleFireRoots())
		{
			((CanvasItem)item).Visible = visible;
			((Node)item).ProcessMode = (ProcessModeEnum)(visible ? 0 : 4);
			if (!visible)
			{
				CombatEffectRuntime.StopParticles((Node)(object)item);
			}
			flag = true;
		}
		if (!flag || !(visible && restartAnimation))
		{
			return;
		}
		foreach (Node2D item2 in BattleReadyIdleFireRoots())
		{
			if (_profile.IdleEffect.Layers.Count == 0)
			{
				_context.StartIdleEffects((Node)(object)item2);
			}
			else
			{
				StartIdleLayers(item2);
			}
		}
	}

	private void StopIdleLayerTweens()
	{
		foreach (Tween idleLayerTween in _idleLayerTweens)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)idleLayerTween))
			{
				idleLayerTween.Kill();
			}
		}
		_idleLayerTweens.Clear();
	}

	private void StartIdleLayers(Node2D root)
	{
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		foreach (BattleReadyIdleLayer spec in _profile.IdleEffect.Layers)
		{
			Node2D layer = ((Node)root).GetNodeOrNull<Node2D>(NodePath.op_Implicit(spec.Path));
			if (layer == null)
			{
				continue;
			}
			((CanvasItem)layer).Visible = false;
			float[] ease = new float[257];
			for (int i = 0; i < ease.Length; i++)
			{
				ease[i] = CombatRadialRgbBlurRuntime.Ease(spec.Curve, (float)i / 256f);
			}
			Tween val = ((Node)root).CreateTween();
			_idleLayerTweens.Add(val);
			if (spec.DelaySeconds > 0f)
			{
				val.TweenInterval((double)spec.DelaySeconds);
			}
			val.TweenCallback(Callable.From((Action)delegate
			{
				//IL_009d: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
				//IL_0105: Unknown result type (might be due to invalid IL or missing references)
				//IL_010f: Unknown result type (might be due to invalid IL or missing references)
				((CanvasItem)layer).Visible = true;
				Alpha(1f);
				CombatEffectRuntime.StartSpineAnimation((Node)(object)layer, spec.Animation, loop: true, 1f, _context.LogWarn);
				Tween val2 = ((Node)root).CreateTween().SetLoops(0);
				_idleLayerTweens.Add(val2);
				val2.TweenMethod(Callable.From<float>((Action<float>)delegate(float t)
				{
					Alpha(Mathf.Lerp(1f, spec.Value, Eased(t)));
				}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), (double)spec.StartSeconds);
				val2.TweenInterval((double)spec.HoldSeconds);
				val2.TweenMethod(Callable.From<float>((Action<float>)delegate(float t)
				{
					Alpha(Mathf.Lerp(spec.Value, spec.EndValue, Eased(t)));
				}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), (double)spec.EndSeconds);
				val2.TweenInterval((double)spec.RestartSeconds);
			}));
			void Alpha(float a)
			{
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				((CanvasItem)layer).Modulate = new Color(1f, 1f, 1f, a);
			}
			float Eased(float t)
			{
				float num = Mathf.Clamp(t, 0f, 1f) * 256f;
				int num2 = Math.Min((int)num, 255);
				return Mathf.Lerp(ease[num2], ease[num2 + 1], num - (float)num2);
			}
		}
	}

	private bool IsBattleReadyIdleFireVisible()
	{
		foreach (Node2D item in BattleReadyIdleFireRoots())
		{
			if (((CanvasItem)item).Visible)
			{
				return true;
			}
		}
		return false;
	}

	[IteratorStateMachine(typeof(BattleReadyOverlayRuntime<>._003CBattleReadyIdleFireRoots_003Ed__85))]
	private IEnumerable<Node2D> BattleReadyIdleFireRoots()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CBattleReadyIdleFireRoots_003Ed__85(-2)
		{
			_003C_003E4__this = this
		};
	}

	private void LogMissingAnimation(string anim)
	{
		if (_missingAnimationsLogged.Add(anim))
		{
			_context.LogWarn("Battle ready overlay missing animation: " + anim);
		}
	}
}
