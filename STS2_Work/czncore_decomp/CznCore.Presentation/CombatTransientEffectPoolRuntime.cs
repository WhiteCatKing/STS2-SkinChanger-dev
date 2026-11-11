using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using Godot;
using Godot.Collections;

namespace CznCore.Presentation;

internal sealed class CombatTransientEffectPoolRuntime
{
	private readonly Dictionary<string, PackedScene> _sceneCache = new Dictionary<string, PackedScene>(StringComparer.Ordinal);

	private readonly HashSet<string> _threadedSceneLoads = new HashSet<string>(StringComparer.Ordinal);

	private readonly Dictionary<string, Stack<Node2D>> _pool = new Dictionary<string, Stack<Node2D>>(StringComparer.Ordinal);

	private readonly ConditionalWeakTable<CombatPresentationEffectCue, string> _poolKeyCache = new ConditionalWeakTable<CombatPresentationEffectCue, string>();

	private readonly string _rootMeta;

	private readonly string _groupMeta;

	private readonly string _poolKeyMeta;

	private readonly string _poolStateMeta;

	private readonly string _reservedStateMeta;

	private readonly string _visibleStateMeta;

	private readonly string _generationMeta;

	private readonly string _poolCapMeta;

	private readonly string _poolEpochMeta;

	private readonly int _defaultPoolCap;

	private readonly int _maxPoolCap;

	private readonly Func<Node2D, Node, int> _attachLayers;

	private readonly Action<Node, CombatPresentationEffectCue> _applyMaterial;

	private readonly Func<CombatPresentationEffectCue, int> _poolCapForCue;

	private readonly ISkinTimingProbe _timingProbe;

	private readonly Dictionary<string, int> _poolKeyLastPlaybackBoundary = new Dictionary<string, int>(StringComparer.Ordinal);

	private int _poolEpoch;

	private int _reservedPlaybackCount;

	private int _visiblePlaybackCount;

	private int _roomBoundary;

	private const int IdlePoolTrimRoomBoundaries = 3;

	private const int IdlePoolTrimRetainedCountPerKey = 1;

	public int ActivePlaybackCount => VisiblePlaybackCount;

	public int ReservedPlaybackCount => Math.Max(0, Volatile.Read(in _reservedPlaybackCount));

	public int VisiblePlaybackCount => Math.Max(0, Volatile.Read(in _visiblePlaybackCount));

	public CombatTransientEffectPoolRuntime(string metaKeyPrefix, int defaultPoolCap, int maxPoolCap, Func<Node2D, Node, int> attachLayers, Action<Node, CombatPresentationEffectCue> applyMaterial, Func<CombatPresentationEffectCue, int> poolCapForCue, ISkinTimingProbe timingProbe)
	{
		string text = (string.IsNullOrWhiteSpace(metaKeyPrefix) ? "character_skin" : metaKeyPrefix.Trim());
		_rootMeta = text + "_combat_transient_effects_root";
		_groupMeta = text + "_combat_transient_effects_group";
		_poolKeyMeta = text + "_combat_transient_effect_pool_key";
		_poolStateMeta = text + "_combat_transient_effect_in_pool";
		_reservedStateMeta = text + "_combat_transient_effect_reserved";
		_visibleStateMeta = text + "_combat_transient_effect_active";
		_generationMeta = text + "_combat_transient_effect_generation";
		_poolCapMeta = text + "_combat_transient_effect_pool_cap";
		_poolEpochMeta = text + "_combat_transient_effect_pool_epoch";
		_defaultPoolCap = defaultPoolCap;
		_maxPoolCap = maxPoolCap;
		_attachLayers = attachLayers;
		_applyMaterial = applyMaterial;
		_poolCapForCue = poolCapForCue;
		_timingProbe = timingProbe ?? throw new ArgumentNullException("timingProbe");
	}

	public string DescribeState()
	{
		int num = 0;
		int num2 = 0;
		foreach (Stack<Node2D> value in _pool.Values)
		{
			num += value.Count;
			foreach (Node2D item in value)
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)item) && IsPoolEpochCurrent(item))
				{
					num2++;
				}
			}
		}
		return "poolKeys=" + _pool.Count + " pooledRoots=" + num + " usablePooledRoots=" + num2 + " cachedScenes=" + _sceneCache.Count + " pendingThreadedLoads=" + _threadedSceneLoads.Count + " reserved=" + ReservedPlaybackCount + " visible=" + VisiblePlaybackCount + " epoch=" + Volatile.Read(in _poolEpoch);
	}

	public PackedScene? LoadScene(string scenePath, bool allowSynchronousFallback = true)
	{
		if (TryGetCachedScene(scenePath, out PackedScene scene))
		{
			return scene;
		}
		if (_threadedSceneLoads.Contains(scenePath))
		{
			TryCompleteThreadedSceneLoad(scenePath, allowSynchronousFallback, allowSynchronousFallback ? "transient_scene_threaded_wait:" : "transient_scene_threaded_poll:");
			if (TryGetCachedScene(scenePath, out scene))
			{
				return scene;
			}
		}
		if (!allowSynchronousFallback)
		{
			PreloadScene(scenePath, allowSynchronousFallback: false);
			return null;
		}
		long startTimestamp = _timingProbe.StartTimingProbe();
		PackedScene val = ResourceLoader.Load<PackedScene>(scenePath, (string)null, (CacheMode)1);
		if (val != null)
		{
			_sceneCache[scenePath] = val;
		}
		_timingProbe.LogTimingProbe("transient_scene_load:" + scenePath, startTimestamp);
		return val;
	}

	public bool PreloadScene(string scenePath, bool allowSynchronousFallback = true)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(scenePath) || TryGetCachedScene(scenePath, out PackedScene _))
		{
			return true;
		}
		if (_threadedSceneLoads.Contains(scenePath))
		{
			return TryCompleteThreadedSceneLoad(scenePath, allowBlocking: false, "transient_scene_threaded_preload:");
		}
		long startTimestamp = _timingProbe.StartTimingProbe();
		Error val = ResourceLoader.LoadThreadedRequest(scenePath, string.Empty, CznRuntimeCapabilities.UseSubThreadsForWarmupResourceLoads, (CacheMode)1);
		_timingProbe.LogTimingProbe("transient_scene_threaded_request:" + scenePath, startTimestamp);
		if ((int)val == 0)
		{
			_threadedSceneLoads.Add(scenePath);
			return false;
		}
		if (!allowSynchronousFallback)
		{
			return false;
		}
		return LoadScene(scenePath) != null;
	}

	public void ClearSceneCache()
	{
		_sceneCache.Clear();
	}

	public bool HasScene(string scenePath)
	{
		PackedScene scene;
		return TryGetCachedScene(scenePath, out scene);
	}

	public void AddScene(string scenePath, PackedScene scene)
	{
		_sceneCache[scenePath] = scene;
	}

	private bool TryGetCachedScene(string scenePath, out PackedScene? scene)
	{
		if (_sceneCache.TryGetValue(scenePath, out scene) && scene != null && GodotObject.IsInstanceValid((GodotObject)(object)scene))
		{
			return true;
		}
		scene = null;
		_sceneCache.Remove(scenePath);
		return false;
	}

	private bool TryCompleteThreadedSceneLoad(string scenePath, bool allowBlocking, string timingName)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I8
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I8
		ThreadLoadStatus val = ResourceLoader.LoadThreadedGetStatus(scenePath, (Array)null);
		if ((long)val == 1)
		{
			if (!allowBlocking)
			{
				return false;
			}
			return CompleteThreadedSceneLoad(scenePath, timingName);
		}
		if ((long)val == 3)
		{
			return CompleteThreadedSceneLoad(scenePath, timingName);
		}
		_threadedSceneLoads.Remove(scenePath);
		return false;
	}

	private bool CompleteThreadedSceneLoad(string scenePath, string timingName)
	{
		long startTimestamp = _timingProbe.StartTimingProbe();
		Resource obj = ResourceLoader.LoadThreadedGet(scenePath);
		_threadedSceneLoads.Remove(scenePath);
		PackedScene val = (PackedScene)(object)((obj is PackedScene) ? obj : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			_sceneCache[scenePath] = val;
			_timingProbe.LogTimingProbe(timingName + scenePath, startTimestamp);
			return true;
		}
		_timingProbe.LogTimingProbe(timingName + scenePath, startTimestamp);
		return false;
	}

	public string GetPoolKey(CombatPresentationEffectCue cue)
	{
		return _poolKeyCache.GetValue(cue, CreatePoolKey);
	}

	private static string CreatePoolKey(CombatPresentationEffectCue cue)
	{
		return (cue.Source.ScenePath ?? string.Empty) + "|palette=" + cue.Palette.ToString() + "|neutral=" + cue.UseNeutralPmaMaterial + "|attack=" + cue.UseAttackBoost + "|alpha=" + cue.Alpha.ToString("0.###", CultureInfo.InvariantCulture) + "|preserveRoot=" + cue.PreserveSceneRoot;
	}

	public int GetUsablePooledCount(string poolKey)
	{
		if (!_pool.TryGetValue(poolKey, out Stack<Node2D> value))
		{
			return 0;
		}
		while (value.Count > 0)
		{
			Node2D val = value.Peek();
			if (GodotObject.IsInstanceValid((GodotObject)(object)val) && IsPoolEpochCurrent(val))
			{
				break;
			}
			value.Pop();
			if (GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((Node)val).QueueFree();
			}
		}
		int num = 0;
		foreach (Node2D item in value)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item) && IsPoolEpochCurrent(item))
			{
				num++;
			}
		}
		return num;
	}

	public Node2D? AcquireRoot(CombatPresentationEffectCue cue, PackedScene scene, bool allowPoolReuse = true)
	{
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Expected O, but got Unknown
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		long startTimestamp = _timingProbe.StartTimingProbe();
		string poolKey = GetPoolKey(cue);
		bool flag;
		if (allowPoolReuse && _pool.TryGetValue(poolKey, out Stack<Node2D> value))
		{
			while (value.Count > 0)
			{
				Node2D val = value.Pop();
				if (GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					if (IsPoolEpochCurrent(val))
					{
						StringName obj = StringName.op_Implicit(_poolStateMeta);
						flag = false;
						((GodotObject)val).SetMeta(obj, Variant.From<bool>(ref flag));
						StringName obj2 = StringName.op_Implicit(_reservedStateMeta);
						flag = false;
						((GodotObject)val).SetMeta(obj2, Variant.From<bool>(ref flag));
						StringName obj3 = StringName.op_Implicit(_visibleStateMeta);
						flag = false;
						((GodotObject)val).SetMeta(obj3, Variant.From<bool>(ref flag));
						_applyMaterial((Node)(object)val, cue);
						_timingProbe.LogTimingProbe("effect_pool_acquire:" + (cue.GroupName ?? cue.Source.ScenePath ?? "<null>"), startTimestamp);
						return val;
					}
					((Node)val).QueueFree();
				}
			}
		}
		Node val2 = scene.Instantiate((GenEditState)0);
		Node2D val3 = (Node2D)(object)((val2 is Node2D) ? val2 : null);
		if (val3 == null)
		{
			val2.QueueFree();
			return null;
		}
		Node2D val4;
		if (cue.PreserveSceneRoot)
		{
			val4 = val3;
		}
		else
		{
			val4 = new Node2D();
			int num = _attachLayers(val4, (Node)(object)val3);
			((Node)val3).QueueFree();
			if (num == 0)
			{
				((Node)val4).QueueFree();
				return null;
			}
		}
		CombatEffectRuntime.PrimeNodeCaches((Node)(object)val4);
		_applyMaterial((Node)(object)val4, cue);
		((GodotObject)val4).SetMeta(StringName.op_Implicit(_poolKeyMeta), Variant.From<string>(ref poolKey));
		Node2D obj4 = val4;
		StringName obj5 = StringName.op_Implicit(_poolStateMeta);
		flag = false;
		((GodotObject)obj4).SetMeta(obj5, Variant.From<bool>(ref flag));
		Node2D obj6 = val4;
		StringName obj7 = StringName.op_Implicit(_reservedStateMeta);
		flag = false;
		((GodotObject)obj6).SetMeta(obj7, Variant.From<bool>(ref flag));
		Node2D obj8 = val4;
		StringName obj9 = StringName.op_Implicit(_visibleStateMeta);
		flag = false;
		((GodotObject)obj8).SetMeta(obj9, Variant.From<bool>(ref flag));
		Node2D obj10 = val4;
		StringName obj11 = StringName.op_Implicit(_generationMeta);
		int num2 = 0;
		((GodotObject)obj10).SetMeta(obj11, Variant.From<int>(ref num2));
		Node2D obj12 = val4;
		StringName obj13 = StringName.op_Implicit(_poolCapMeta);
		num2 = _poolCapForCue(cue);
		((GodotObject)obj12).SetMeta(obj13, Variant.From<int>(ref num2));
		SetPoolEpochCurrent(val4);
		_timingProbe.LogTimingProbe("effect_instantiate:" + (cue.GroupName ?? cue.Source.ScenePath ?? "<null>"), startTimestamp);
		return val4;
	}

	public int BeginPlayback(Node2D effectRoot, CombatPresentationEffectCue cue)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		_poolKeyLastPlaybackBoundary[GetPoolKey(cue)] = _roomBoundary;
		Node parent = ((Node)effectRoot).GetParent();
		if (parent != null)
		{
			parent.RemoveChild((Node)(object)effectRoot);
		}
		int result = GetGeneration(effectRoot) + 1;
		((GodotObject)effectRoot).SetMeta(StringName.op_Implicit(_generationMeta), Variant.From<int>(ref result));
		StringName obj = StringName.op_Implicit(_poolStateMeta);
		bool flag = false;
		((GodotObject)effectRoot).SetMeta(obj, Variant.From<bool>(ref flag));
		StringName obj2 = StringName.op_Implicit(_visibleStateMeta);
		flag = false;
		((GodotObject)effectRoot).SetMeta(obj2, Variant.From<bool>(ref flag));
		MarkPlaybackReserved(effectRoot);
		StringName obj3 = StringName.op_Implicit(_rootMeta);
		flag = true;
		((GodotObject)effectRoot).SetMeta(obj3, Variant.From<bool>(ref flag));
		StringName obj4 = StringName.op_Implicit(_groupMeta);
		string text = cue.GroupName ?? string.Empty;
		((GodotObject)effectRoot).SetMeta(obj4, Variant.From<string>(ref text));
		return result;
	}

	public bool MarkPlaybackVisible(Node2D effectRoot, int expectedGeneration)
	{
		if (!IsActive(effectRoot, expectedGeneration))
		{
			return false;
		}
		MarkPlaybackVisible(effectRoot);
		return true;
	}

	public void Release(Node2D effectRoot, int? expectedGeneration = null)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		long startTimestamp = _timingProbe.StartTimingProbe();
		if (!GodotObject.IsInstanceValid((GodotObject)(object)effectRoot) || IsInPool(effectRoot) || (expectedGeneration.HasValue && GetGeneration(effectRoot) != expectedGeneration.Value))
		{
			return;
		}
		MarkPlaybackInactive(effectRoot);
		int num = GetGeneration(effectRoot) + 1;
		((GodotObject)effectRoot).SetMeta(StringName.op_Implicit(_generationMeta), Variant.From<int>(ref num));
		StringName obj = StringName.op_Implicit(_poolStateMeta);
		bool flag = true;
		((GodotObject)effectRoot).SetMeta(obj, Variant.From<bool>(ref flag));
		CombatEffectRuntime.StopParticles((Node)(object)effectRoot);
		((CanvasItem)effectRoot).Visible = false;
		Node parent = ((Node)effectRoot).GetParent();
		if (parent != null)
		{
			parent.RemoveChild((Node)(object)effectRoot);
		}
		if (!IsPoolEpochCurrent(effectRoot))
		{
			((Node)effectRoot).QueueFree();
			_timingProbe.LogTimingProbe("effect_release_free:expired_epoch", startTimestamp);
			return;
		}
		string text = string.Empty;
		try
		{
			if (((GodotObject)effectRoot).HasMeta(StringName.op_Implicit(_poolKeyMeta)))
			{
				StringName obj2 = StringName.op_Implicit(_poolKeyMeta);
				Variant val = default(Variant);
				val = ((GodotObject)effectRoot).GetMeta(obj2, val);
				text = ((Variant)(ref val)).AsString();
			}
		}
		catch
		{
		}
		if (string.IsNullOrEmpty(text))
		{
			((Node)effectRoot).QueueFree();
			_timingProbe.LogTimingProbe("effect_release_free:missing_pool_key", startTimestamp);
			return;
		}
		if (!_pool.TryGetValue(text, out Stack<Node2D> value))
		{
			value = new Stack<Node2D>();
			_pool[text] = value;
		}
		if (value.Count >= GetPoolCap(effectRoot))
		{
			((Node)effectRoot).QueueFree();
			_timingProbe.LogTimingProbe("effect_release_free:pool_cap:" + text, startTimestamp);
		}
		else
		{
			value.Push(effectRoot);
			_timingProbe.LogTimingProbe("effect_release_pool:" + text, startTimestamp);
		}
	}

	public void AdvancePoolEpoch()
	{
		Interlocked.Increment(ref _poolEpoch);
	}

	public void TrimIdlePools()
	{
		_roomBoundary++;
		int num = 0;
		int num2 = 0;
		foreach (var (key, stack2) in _pool)
		{
			if (!_poolKeyLastPlaybackBoundary.TryGetValue(key, out var value))
			{
				_poolKeyLastPlaybackBoundary[key] = _roomBoundary;
			}
			else
			{
				if (_roomBoundary - value < 3 || stack2.Count <= 1)
				{
					continue;
				}
				num++;
				while (stack2.Count > 1)
				{
					Node2D val = stack2.Pop();
					if (GodotObject.IsInstanceValid((GodotObject)(object)val))
					{
						((Node)val).QueueFree();
						num2++;
					}
				}
			}
		}
		int boundary = _roomBoundary;
		int keys = num;
		int freed = num2;
		SkinLog.Debug(() => "transient_pool_state reason=room_boundary boundary=" + boundary + " trimmedKeys=" + keys + " freedRoots=" + freed + " " + DescribeState());
	}

	public void ClearPool()
	{
		foreach (Stack<Node2D> value in _pool.Values)
		{
			while (value.Count > 0)
			{
				Node2D val = value.Pop();
				if (GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					((Node)val).QueueFree();
				}
			}
		}
		_pool.Clear();
	}

	public void MarkQueuedForFree(Node2D effectRoot)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)effectRoot) && IsTransientRoot((Node)(object)effectRoot))
		{
			int num = GetGeneration(effectRoot) + 1;
			((GodotObject)effectRoot).SetMeta(StringName.op_Implicit(_generationMeta), Variant.From<int>(ref num));
			StringName obj = StringName.op_Implicit(_poolStateMeta);
			bool flag = true;
			((GodotObject)effectRoot).SetMeta(obj, Variant.From<bool>(ref flag));
			MarkPlaybackInactive(effectRoot);
		}
	}

	public bool IsTransientRoot(Node node)
	{
		try
		{
			return ((GodotObject)node).HasMeta(StringName.op_Implicit(_rootMeta));
		}
		catch
		{
			return false;
		}
	}

	public int GetGeneration(Node2D effectRoot)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (((GodotObject)effectRoot).HasMeta(StringName.op_Implicit(_generationMeta)))
			{
				StringName obj = StringName.op_Implicit(_generationMeta);
				Variant val = default(Variant);
				val = ((GodotObject)effectRoot).GetMeta(obj, val);
				return ((Variant)(ref val)).AsInt32();
			}
		}
		catch
		{
		}
		return 0;
	}

	public bool IsInPool(Node2D effectRoot)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			int result;
			if (((GodotObject)effectRoot).HasMeta(StringName.op_Implicit(_poolStateMeta)))
			{
				StringName obj = StringName.op_Implicit(_poolStateMeta);
				Variant val = default(Variant);
				val = ((GodotObject)effectRoot).GetMeta(obj, val);
				result = (((Variant)(ref val)).AsBool() ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}
		catch
		{
			return false;
		}
	}

	public bool IsActive(Node2D effectRoot, int generation)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)effectRoot) && !IsInPool(effectRoot))
		{
			return GetGeneration(effectRoot) == generation;
		}
		return false;
	}

	private int GetPoolCap(Node2D effectRoot)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (((GodotObject)effectRoot).HasMeta(StringName.op_Implicit(_poolCapMeta)))
			{
				StringName obj = StringName.op_Implicit(_poolCapMeta);
				Variant val = default(Variant);
				val = ((GodotObject)effectRoot).GetMeta(obj, val);
				int num = ((Variant)(ref val)).AsInt32();
				if (num > 0)
				{
					return Math.Clamp(num, _defaultPoolCap, _maxPoolCap);
				}
			}
		}
		catch
		{
		}
		return _defaultPoolCap;
	}

	private bool IsPlaybackReserved(Node2D effectRoot)
	{
		return GetPlaybackState(effectRoot, _reservedStateMeta);
	}

	private bool IsPlaybackVisible(Node2D effectRoot)
	{
		return GetPlaybackState(effectRoot, _visibleStateMeta);
	}

	private bool GetPlaybackState(Node2D effectRoot, string metaKey)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			int result;
			if (((GodotObject)effectRoot).HasMeta(StringName.op_Implicit(metaKey)))
			{
				StringName obj = StringName.op_Implicit(metaKey);
				Variant val = default(Variant);
				val = ((GodotObject)effectRoot).GetMeta(obj, val);
				result = (((Variant)(ref val)).AsBool() ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}
		catch
		{
			return false;
		}
	}

	private void MarkPlaybackReserved(Node2D effectRoot)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!IsPlaybackReserved(effectRoot))
		{
			StringName obj = StringName.op_Implicit(_reservedStateMeta);
			bool flag = true;
			((GodotObject)effectRoot).SetMeta(obj, Variant.From<bool>(ref flag));
			Interlocked.Increment(ref _reservedPlaybackCount);
		}
	}

	private void MarkPlaybackVisible(Node2D effectRoot)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!IsPlaybackVisible(effectRoot))
		{
			StringName obj = StringName.op_Implicit(_visibleStateMeta);
			bool flag = true;
			((GodotObject)effectRoot).SetMeta(obj, Variant.From<bool>(ref flag));
			Interlocked.Increment(ref _visiblePlaybackCount);
		}
	}

	private void MarkPlaybackInactive(Node2D effectRoot)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (IsPlaybackVisible(effectRoot))
		{
			StringName obj = StringName.op_Implicit(_visibleStateMeta);
			bool flag = false;
			((GodotObject)effectRoot).SetMeta(obj, Variant.From<bool>(ref flag));
			if (Interlocked.Decrement(ref _visiblePlaybackCount) < 0)
			{
				Interlocked.Exchange(ref _visiblePlaybackCount, 0);
			}
		}
		if (IsPlaybackReserved(effectRoot))
		{
			StringName obj2 = StringName.op_Implicit(_reservedStateMeta);
			bool flag = false;
			((GodotObject)effectRoot).SetMeta(obj2, Variant.From<bool>(ref flag));
			if (Interlocked.Decrement(ref _reservedPlaybackCount) < 0)
			{
				Interlocked.Exchange(ref _reservedPlaybackCount, 0);
			}
		}
	}

	private int GetPoolEpoch(Node2D effectRoot)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (((GodotObject)effectRoot).HasMeta(StringName.op_Implicit(_poolEpochMeta)))
			{
				StringName obj = StringName.op_Implicit(_poolEpochMeta);
				Variant val = default(Variant);
				val = ((GodotObject)effectRoot).GetMeta(obj, val);
				return ((Variant)(ref val)).AsInt32();
			}
		}
		catch
		{
		}
		return Volatile.Read(in _poolEpoch);
	}

	private bool IsPoolEpochCurrent(Node2D effectRoot)
	{
		return GetPoolEpoch(effectRoot) == Volatile.Read(in _poolEpoch);
	}

	private void SetPoolEpochCurrent(Node2D effectRoot)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		StringName obj = StringName.op_Implicit(_poolEpochMeta);
		int num = Volatile.Read(in _poolEpoch);
		((GodotObject)effectRoot).SetMeta(obj, Variant.From<int>(ref num));
	}
}
