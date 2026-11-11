using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace CznCore.Presentation;

internal sealed class CharacterEffectEngine : ICombatWarmupHost, ICombatWarmupCueFilterHost
{
	private sealed class AuthoredState
	{
		private Transform2D _transform;

		private int _zIndex;

		private bool _zAsRelative;

		private bool _topLevel;

		private Color _modulate;

		private (CanvasItem Item, bool Visible)[] _descendants = Array.Empty<(CanvasItem, bool)>();

		public static AuthoredState Capture(Node2D root)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			return new AuthoredState
			{
				_transform = root.Transform,
				_zIndex = ((CanvasItem)root).ZIndex,
				_zAsRelative = ((CanvasItem)root).ZAsRelative,
				_topLevel = ((CanvasItem)root).TopLevel,
				_modulate = ((CanvasItem)root).Modulate,
				_descendants = (from item in ((IEnumerable)((Node)root).FindChildren("*", "CanvasItem", true, false)).OfType<CanvasItem>()
					select (item: item, Visible: item.Visible)).ToArray()
			};
		}

		public void Restore(Node2D root)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			root.Transform = _transform;
			((CanvasItem)root).ZIndex = _zIndex;
			((CanvasItem)root).ZAsRelative = _zAsRelative;
			((CanvasItem)root).TopLevel = _topLevel;
			((CanvasItem)root).Modulate = _modulate;
			(CanvasItem, bool)[] descendants = _descendants;
			for (int i = 0; i < descendants.Length; i++)
			{
				var (val, visible) = descendants[i];
				if (GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					val.Visible = visible;
				}
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003CRunStartSteps_003Ed__20 : IEnumerable<RunStartPreloadStep>, IEnumerable, IEnumerator<RunStartPreloadStep>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private RunStartPreloadStep _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public CharacterEffectEngine _003C_003E4__this;

		RunStartPreloadStep IEnumerator<RunStartPreloadStep>.Current
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
		public _003CRunStartSteps_003Ed__20(int _003C_003E1__state)
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
			CharacterEffectEngine CS_0024_003C_003E8__locals6 = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new RunStartPreloadStep(0.93f, "Preparing effects", CS_0024_003C_003E8__locals6.SeedStarterPoolsAsync, CS_0024_003C_003E8__locals6._options.EffectsEnabled, CS_0024_003C_003E8__locals6._options.SeedEffectsDuringLimitedPreload);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = new RunStartPreloadStep(0.94f, "Preparing sounds", CS_0024_003C_003E8__locals6.PrewarmSfxAsync, () => CS_0024_003C_003E8__locals6._options.EffectsEnabled() && CS_0024_003C_003E8__locals6._options.SfxEnabled(), runDuringLimitedPreload: true);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				return false;
			}
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
		IEnumerator<RunStartPreloadStep> IEnumerable<RunStartPreloadStep>.GetEnumerator()
		{
			_003CRunStartSteps_003Ed__20 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CRunStartSteps_003Ed__20(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<RunStartPreloadStep>)this).GetEnumerator();
		}
	}

	private readonly CharacterEffectEngineOptions _options;

	private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

	private readonly ConditionalWeakTable<Node2D, AuthoredState> _authored = new ConditionalWeakTable<Node2D, AuthoredState>();

	private readonly HashSet<string> _disposableKeys = new HashSet<string>(StringComparer.Ordinal);

	private readonly Dictionary<string, CombatPresentationEffectCue> _sceneCues = new Dictionary<string, CombatPresentationEffectCue>(StringComparer.Ordinal);

	private Func<float> _entryDurationSeconds;

	private const float EntrySettleSeconds = 0.15f;

	public CombatTransientEffectPoolRuntime Pool { get; }

	public CombatWarmupRuntime Warmups { get; }

	private string WarmupSurfaceName => "Czn" + _options.OwnerId + "EffectWarmup";

	public CharacterEffectEngine(CharacterEffectEngineOptions options)
	{
		_options = options;
		_entryDurationSeconds = options.EntryDurationSeconds ?? ((Func<float>)(() => 0f));
		Warmups = new CombatWarmupRuntime(options.Catalog, options.Policy, this, options.Timing, SkinLog.Warn);
		Pool = new CombatTransientEffectPoolRuntime(options.MetaKeyPrefix ?? options.OwnerId, options.Policy.TransientEffectPoolSizePerKey, options.Policy.TransientTargetEffectPoolSizePerKey, options.AttachLayers, options.ApplyMaterial, options.PoolCap ?? ((Func<CombatPresentationEffectCue, int>)((CombatPresentationEffectCue cue) => Warmups.GetEffectPoolCap(cue))), options.Timing);
	}

	public void Register(string displayName, Func<CharacterModel?, bool> appliesTo, params RunStartPreloadStep[] extraSteps)
	{
		RunStartPreloadProfile runStartPreloadProfile = new RunStartPreloadProfile(_options.OwnerId, displayName, appliesTo, () => Array.Empty<RunStartPreloadItem>());
		List<RunStartPreloadStep> list = new List<RunStartPreloadStep>();
		list.AddRange(RunStartSteps());
		list.AddRange(extraSteps);
		runStartPreloadProfile.FinalSteps = new _003C_003Ez__ReadOnlyList<RunStartPreloadStep>(list);
		Register(runStartPreloadProfile);
	}

	public void Register(CharacterSkinProfile skin, string displayName, Func<IReadOnlyList<RunStartPreloadItem>> buildItems, params RunStartPreloadStep[] extraSteps)
	{
		if (_options.EntryDurationSeconds == null)
		{
			Lazy<float> entry = new Lazy<float>(() => SkinEntryDurationSeconds(skin));
			_entryDurationSeconds = () => (!skin.Features.CombatEntryAnimation) ? 0f : entry.Value;
		}
		RunStartPreloadProfile runStartPreloadProfile = new RunStartPreloadProfile(skin, displayName, buildItems);
		List<RunStartPreloadStep> list = new List<RunStartPreloadStep>();
		list.AddRange(RunStartSteps());
		list.AddRange(extraSteps);
		runStartPreloadProfile.FinalSteps = new _003C_003Ez__ReadOnlyList<RunStartPreloadStep>(list);
		Register(runStartPreloadProfile);
	}

	private static float SkinEntryDurationSeconds(CharacterSkinProfile skin)
	{
		IReadOnlyList<CombatPresentationBodyStep> readOnlyList = CombatBodyEntryPlaybackRuntime.BuildEntrySteps(skin.CombatBody);
		if (readOnlyList.Count != 0)
		{
			return new CombatBodySequenceRuntime(skin.CombatPresentation, skin.CombatBody.ReadyExitAnimation, 0f).GetSequenceDelaySeconds(readOnlyList);
		}
		return 0f;
	}

	private void Register(RunStartPreloadProfile profile)
	{
		Func<CharacterModel?, bool> appliesTo = profile.AppliesTo;
		RunStartPreloadRuntime.Configure(profile);
		_subscriptions.Add(SkinEventHub.Subscribe(_options.OwnerId + " effect warmup", delegate(CombatStartingEvent e)
		{
			ICombatState? combatState = e.CombatState;
			if (combatState != null && combatState.Players.Any((Player player) => appliesTo(player.Character)))
			{
				GodotScheduler.Delay(0.15f + Math.Max(0f, _entryDurationSeconds()), WarmAfterEntry);
			}
		}));
		_subscriptions.Add(SkinEventHub.Subscribe<RoomEnteringEvent>(_options.OwnerId + " effect pool trim", delegate
		{
			Pool.TrimIdlePools();
		}));
		_subscriptions.Add(SkinEventHub.Subscribe<RunEndedEvent>(_options.OwnerId + " effect release", delegate
		{
			Reset(clearSceneCache: true);
		}));
	}

	[IteratorStateMachine(typeof(_003CRunStartSteps_003Ed__20))]
	public IEnumerable<RunStartPreloadStep> RunStartSteps()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CRunStartSteps_003Ed__20(-2)
		{
			_003C_003E4__this = this
		};
	}

	public Node2D? Acquire(CombatPresentationEffectCue cue, out int generation)
	{
		generation = 0;
		PackedScene val = LoadScene(cue.Source.ScenePath);
		if (val != null)
		{
			Node2D val2 = TakeRoot(cue, val, allowPoolReuse: true);
			if (val2 != null)
			{
				generation = Pool.BeginPlayback(val2, cue);
				((CanvasItem)val2).Visible = true;
				return val2;
			}
		}
		return null;
	}

	public Node2D? AcquireScene(string scenePath, out int generation)
	{
		CombatPresentationEffectCue combatPresentationEffectCue2;
		if (!_sceneCues.TryGetValue(scenePath, out CombatPresentationEffectCue value))
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(scenePath);
			if (_options.Catalog.TryGetTransientEffects(fileNameWithoutExtension, out IReadOnlyList<CombatPresentationEffectCue> effects))
			{
				CombatPresentationEffectCue combatPresentationEffectCue = effects.FirstOrDefault((CombatPresentationEffectCue candidate) => candidate.Source.ScenePath == scenePath);
				if ((object)combatPresentationEffectCue != null)
				{
					combatPresentationEffectCue2 = combatPresentationEffectCue;
					goto IL_006b;
				}
			}
			combatPresentationEffectCue2 = SceneEffectCatalog.Cue(scenePath, fileNameWithoutExtension);
			goto IL_006b;
		}
		goto IL_007e;
		IL_007e:
		return Acquire(value, out generation);
		IL_006b:
		value = combatPresentationEffectCue2;
		_sceneCues[scenePath] = value;
		goto IL_007e;
	}

	public bool IsCurrent(Node2D root, int generation)
	{
		return Pool.IsActive(root, generation);
	}

	public void Release(Node2D root, int generation)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)root) || !IsDisposable(root))
		{
			Pool.Release(root, generation);
		}
		else if (Pool.IsActive(root, generation))
		{
			Pool.MarkQueuedForFree(root);
			((Node)root).QueueFree();
		}
	}

	private void WarmAfterEntry()
	{
		if (_options.EffectsEnabled() && _options.CombatWarmupEnabled())
		{
			Warmups.ScheduleTransientEffectSceneCachePreload();
			Warmups.ScheduleTransientEffectPoolWarmup();
			Warmups.ScheduleSfxPrewarm();
		}
		_options.AfterEntryWarmups?.Invoke();
	}

	public void Reset(bool clearSceneCache)
	{
		Warmups.Stop();
		Pool.AdvancePoolEpoch();
		Pool.ClearPool();
		if (clearSceneCache)
		{
			Warmups.ResetSceneCachePreload();
			Pool.ClearSceneCache();
		}
	}

	private async Task SeedStarterPoolsAsync(Node host)
	{
		List<CombatPresentationEffectCue> source = _options.Catalog.EnumerateTransientEffectCues().ToList();
		List<CombatPresentationEffectCue> cues = source.Where(Warmups.IsStarterCue).ToList();
		if (_options.BuildScene == null)
		{
			await RunStartResourcePreloader.PreloadAllAsync(host, from cue in source.Where(Warmups.IsSceneCacheStarterCue).Concat(cues)
				select cue.Source.ScenePath, _options.EffectsEnabled, delegate(string path, Resource _)
			{
				Pool.LoadScene(path);
			});
		}
		await RunStartResourcePreloader.AwaitFrame(host);
		HashSet<string> seenPoolKeys = new HashSet<string>(StringComparer.Ordinal);
		foreach (CombatPresentationEffectCue item in cues)
		{
			if (!_options.EffectsEnabled())
			{
				return;
			}
			if (await RunStartCombatPrewarm.PrewarmEffectPipelineCue(host, item, seenPoolKeys, this, Warmups))
			{
				await RunStartResourcePreloader.AwaitFrame(host);
			}
		}
	}

	private async Task PrewarmSfxAsync(Node host)
	{
		foreach (string path in Warmups.SfxPrewarmPaths)
		{
			await RunStartResourcePreloader.PreloadResource(host, path);
			_options.PrewarmSfx(path);
		}
	}

	public bool AreEffectsEnabled()
	{
		return _options.EffectsEnabled();
	}

	public bool IsSfxEnabled()
	{
		return _options.SfxEnabled();
	}

	public int CountEffectTargets()
	{
		return _options.CountEffectTargets();
	}

	public string GetPoolKey(CombatPresentationEffectCue cue)
	{
		return Pool.GetPoolKey(_options.ResolveWarmupCue(cue));
	}

	public int GetUsablePooledCount(string poolKey)
	{
		return Pool.GetUsablePooledCount(poolKey);
	}

	public PackedScene? LoadScene(string scenePath)
	{
		Func<string, PackedScene> buildScene = _options.BuildScene;
		if (buildScene != null && !Pool.HasScene(scenePath))
		{
			PackedScene val = buildScene(scenePath);
			if (val != null)
			{
				Pool.AddScene(scenePath, val);
			}
		}
		return Pool.LoadScene(scenePath);
	}

	public bool PreloadScene(string scenePath, bool allowSynchronousFallback)
	{
		if (_options.BuildScene == null)
		{
			return Pool.PreloadScene(scenePath, allowSynchronousFallback);
		}
		return LoadScene(scenePath) != null;
	}

	public Node2D? AcquireRoot(CombatPresentationEffectCue cue, PackedScene scene, bool allowPoolReuse)
	{
		return TakeRoot(allowPoolReuse ? cue : _options.ResolveWarmupCue(cue), scene, allowPoolReuse);
	}

	private Node2D? TakeRoot(CombatPresentationEffectCue cue, PackedScene scene, bool allowPoolReuse)
	{
		Node2D val = Pool.AcquireRoot(cue, scene, allowPoolReuse);
		if (val == null)
		{
			return null;
		}
		if (IsDisposable(val))
		{
			_disposableKeys.Add(Pool.GetPoolKey(cue));
		}
		else
		{
			_authored.GetValue(val, AuthoredState.Capture).Restore(val);
		}
		return val;
	}

	private static bool IsDisposable(Node2D root)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I8
		Variant script = ((GodotObject)root).GetScript();
		return (long)((Variant)(ref script)).VariantType > 0L;
	}

	public bool PrimeRoot(Node2D effectRoot, CombatPresentationEffectCue cue)
	{
		if (!CombatRenderWarmupSurface.MountTransient(effectRoot, WarmupSurfaceName, _options.OwnerId + "EffectWarmupRoot"))
		{
			return false;
		}
		_options.Start(effectRoot, cue);
		return true;
	}

	public void ReleaseRoot(Node2D effectRoot)
	{
		if (IsDisposable(effectRoot))
		{
			((Node)effectRoot).QueueFree();
		}
		else
		{
			Pool.Release(effectRoot, null);
		}
	}

	public bool PrewarmSfx(string path)
	{
		return _options.PrewarmSfx(path);
	}

	public bool ShouldDeferWarmup()
	{
		return _options.ShouldDeferWarmup();
	}

	public bool ShouldWarmupCue(CombatPresentationEffectCue cue)
	{
		if (!_disposableKeys.Contains(Pool.GetPoolKey(cue)))
		{
			return _options.ShouldWarmupCue(cue);
		}
		return false;
	}
}
