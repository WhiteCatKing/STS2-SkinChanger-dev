using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Godot;
using Godot.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Core;

internal static class ManagedSkinModLoader
{
	private sealed record ProviderAssembly(string AssemblyPath, string Name, bool HasGodotScripts, bool HasDeclarativeCharacterAssetReplacement, bool CanActivateBehavior);

	private sealed record ProviderAssemblySourceIdentity(string Fingerprint, string AssemblyPath);

	private sealed record LoadedProviderAssemblyIdentity(string Fingerprint, Assembly Assembly);

	private sealed record ActiveProviderRuntime(Assembly Assembly, IReadOnlyList<ProviderPatch> Patches, IReadOnlyList<ProviderPatch> CharacterPresentationPatches, IReadOnlyList<ProviderPatch> NodeReadyPresentationPatches, IReadOnlyList<ProviderPatch> RunEnvironmentPatches, IReadOnlyList<object> RunEnvironmentControllers, bool InitialRunEnvironmentEnabled)
	{
		public bool RunEnvironmentEnabled { get; set; }

		public Dictionary<ulong, CharacterPresentationMutation> CharacterPresentationMutations { get; }

		public Dictionary<ulong, NodeReadyMutation> NodeReadyMutations { get; }

		public Dictionary<ulong, NodeReadyBaseline> PendingNodeReadyBaselines { get; }

		public ActiveProviderRuntime(Assembly Assembly, IReadOnlyList<ProviderPatch> Patches, IReadOnlyList<ProviderPatch> CharacterPresentationPatches, IReadOnlyList<ProviderPatch> NodeReadyPresentationPatches, IReadOnlyList<ProviderPatch> RunEnvironmentPatches, IReadOnlyList<object> RunEnvironmentControllers, bool InitialRunEnvironmentEnabled)
		{
			RunEnvironmentEnabled = InitialRunEnvironmentEnabled;
			CharacterPresentationMutations = new Dictionary<ulong, CharacterPresentationMutation>();
			NodeReadyMutations = new Dictionary<ulong, NodeReadyMutation>();
			PendingNodeReadyBaselines = new Dictionary<ulong, NodeReadyBaseline>();
			base._002Ector();
		}
	}

	private sealed record ProviderRuntimeBlueprint(IReadOnlyList<ProviderPatch> BehaviorPatches, IReadOnlyList<ProviderPatch> CharacterPresentationPatches, IReadOnlyList<ProviderPatch> NodeReadyPresentationPatches);

	private sealed record ProviderNodeProcessState(WeakReference<Node> Node, ProcessModeEnum ProcessMode)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Node = ");
			builder.Append(Node);
			builder.Append(", ProcessMode = ");
			ProcessModeEnum processMode = ProcessMode;
			builder.Append(((object)(ProcessModeEnum)(ref processMode)/*cast due to .constrained prefix*/).ToString());
			return true;
		}

		[CompilerGenerated]
		public void Deconstruct(out WeakReference<Node> Node, out ProcessModeEnum ProcessMode)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected I8, but got Unknown
			Node = this.Node;
			ProcessMode = (ProcessModeEnum)(long)this.ProcessMode;
		}
	}

	private sealed record CharacterPresentationNodeState(Node Node, bool? Visible, string? Text, bool? ClipContents, IReadOnlyDictionary<string, Variant> Metadata, IReadOnlyList<Callable> ResizeConnections);

	private sealed record CharacterPresentationMutation(WeakReference<NCharacterSelectScreen> Screen, IReadOnlyList<WeakReference<Node>> AddedRoots, IReadOnlyList<CharacterPresentationVisibilityChange> VisibilityChanges, IReadOnlyList<CharacterPresentationTextChange> TextChanges, IReadOnlyList<CharacterPresentationClipChange> ClipChanges, IReadOnlyList<CharacterPresentationMetadataChange> MetadataChanges, IReadOnlyList<(WeakReference<Node> Node, Callable Callback)> ResizeConnections);

	private sealed record CharacterPresentationMetadataChange(WeakReference<Node> Node, PresentationMetadataChange<Variant> Change);

	private sealed record CharacterPresentationVisibilityChange(WeakReference<CanvasItem> Node, bool OriginalVisibility, bool AppliedVisibility);

	private sealed record CharacterPresentationTextChange(WeakReference<Node> Node, string OriginalText, string AppliedText);

	private sealed record CharacterPresentationClipChange(WeakReference<Control> Node, bool OriginalClipContents, bool AppliedClipContents);

	private sealed record NodeReadyBaseline(WeakReference<Node> Node, IReadOnlyDictionary<ulong, NodeReadyVisualState> States);

	private sealed record NodeReadyVisualState(WeakReference<Node> Node, bool? Visible, Color? Modulate, Color? SelfModulate, int? ZIndex, bool? ZAsRelative, Vector2? Node2DPosition, Vector2? Node2DScale, float? Node2DRotation, Vector2? ControlPosition, Vector2? ControlSize, Vector2? ControlScale, float? ControlRotation, Vector2? ControlPivotOffset, MouseFilterEnum? ControlMouseFilter);

	private sealed record NodeReadyVisualChange(WeakReference<Node> Node, NodeReadyVisualState Original, NodeReadyVisualState Applied);

	private sealed record NodeReadyMutation(IReadOnlyList<WeakReference<Node>> AddedRoots, IReadOnlyList<NodeReadyVisualChange> Changes);

	private sealed record ProviderPatch(MethodBase Target, MethodInfo Callback, ProviderPatchKind Kind, int Priority, string[] Before, string[] After, bool Debug);

	private enum ProviderPatchKind
	{
		Prefix,
		Postfix,
		Transpiler,
		Finalizer
	}

	private sealed record ManagedVisualPostfix(Type TargetType, MethodInfo Method);

	private sealed record AliasedSpineResourceSnapshot(Node SpineNode, Resource Resource, string CanonicalPath);

	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static Func<MethodInfo, bool> _003C0_003E__IsScopedMonsterIsEnabledMethod;

		public static Func<MethodInfo, bool> _003C1_003E__IsScopedMonsterSetEnabledMethod;

		public static Func<MethodInfo, bool> _003C2_003E__IsDirectCharacterSkinPathMethod;

		public static Func<Mod, bool> _003C3_003E__IsProviderLoadCandidate;

		public static Func<Mod, bool> _003C4_003E__IsIsolatedProvider;

		public static Func<Mod, SkinModDescriptor> _003C5_003E__ToDescriptor;

		public static Func<ProviderPatch, bool> _003C6_003E__IsReplayableCharacterPresentationPatch;

		public static Func<ProviderPatch, bool> _003C7_003E__IsRunEnvironmentPatch;

		public static NodeAddedEventHandler _003C8_003E__OnProviderNodeAdded;

		public static Func<MethodBase, bool> _003C9_003E__IsNodeReadyPresentationTarget;

		public static Func<MethodInfo, bool> _003C10_003E__IsPostfixMethod;
	}

	[CompilerGenerated]
	private sealed class _003CEnumerateProviderPatches_003Ed__107 : IEnumerable<ProviderPatch>, IEnumerable, IEnumerator<ProviderPatch>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private ProviderPatch _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Patches patches;

		public Patches _003C_003E3__patches;

		private Assembly assembly;

		public Assembly _003C_003E3__assembly;

		private MethodBase target;

		public MethodBase _003C_003E3__target;

		private IEnumerator<(Patch Patch, ProviderPatchKind Kind)> _003C_003E7__wrap1;

		ProviderPatch IEnumerator<ProviderPatch>.Current
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
		public _003CEnumerateProviderPatches_003Ed__107(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					if (patches == null)
					{
						return false;
					}
					_003C_003E7__wrap1 = patches.Prefixes.Select((Patch patch) => (Patch: patch, Kind: ProviderPatchKind.Prefix)).Concat(patches.Postfixes.Select((Patch patch) => (Patch: patch, Kind: ProviderPatchKind.Postfix))).Concat(patches.Transpilers.Select((Patch patch) => (Patch: patch, Kind: ProviderPatchKind.Transpiler)))
						.Concat(patches.Finalizers.Select((Patch patch) => (Patch: patch, Kind: ProviderPatchKind.Finalizer)))
						.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap1.MoveNext())
				{
					(Patch, ProviderPatchKind) current = _003C_003E7__wrap1.Current;
					if ((object)current.Item1.PatchMethod.Module.Assembly == assembly)
					{
						_003C_003E2__current = new ProviderPatch(target, current.Item1.PatchMethod, current.Item2, current.Item1.priority, current.Item1.before, current.Item1.after, current.Item1.debug);
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<ProviderPatch> IEnumerable<ProviderPatch>.GetEnumerator()
		{
			_003CEnumerateProviderPatches_003Ed__107 _003CEnumerateProviderPatches_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateProviderPatches_003Ed__ = this;
			}
			else
			{
				_003CEnumerateProviderPatches_003Ed__ = new _003CEnumerateProviderPatches_003Ed__107(0);
			}
			_003CEnumerateProviderPatches_003Ed__.target = _003C_003E3__target;
			_003CEnumerateProviderPatches_003Ed__.patches = _003C_003E3__patches;
			_003CEnumerateProviderPatches_003Ed__.assembly = _003C_003E3__assembly;
			return _003CEnumerateProviderPatches_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<ProviderPatch>)this).GetEnumerator();
		}
	}

	private static readonly MethodInfo? InvokeOnModDetectedMethod = AccessTools.Method(typeof(ModManager), "InvokeOnModDetected", (Type[])null, (Type[])null);

	private static readonly FieldInfo? OnModDetectedField = AccessTools.Field(typeof(ModManager), "OnModDetected");

	private static readonly FieldInfo? GameVersionField = AccessTools.Field(typeof(ModManager), "_gameVersion");

	private static readonly FieldInfo? CircularDependenciesField = AccessTools.Field(typeof(ModManager), "_circularDependencies");

	private static readonly FieldInfo? ModSettingsField = AccessTools.Field(typeof(ModManager), "_settings");

	private static readonly HashSet<Mod> ManagedModInstances = new HashSet<Mod>(ReferenceEqualityComparer.Instance);

	private static readonly Dictionary<string, SkinProviderProbe> ProvidersByIdentity = new Dictionary<string, SkinProviderProbe>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> ProviderRootPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> ProviderManifestIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> ArtPackProviderIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly List<ProviderPatch> SuspendedPartTakeoverFeedback = new List<ProviderPatch>();

	private static bool _partTakeoverFeedbackSuspended;

	private static readonly HashSet<string> PartTakeoverLogged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> NegativeProviderIdentities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, bool> SelectableCosmeticProbeResults = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<Assembly> RegisteredProviderAssemblies = new HashSet<Assembly>();

	private static readonly Dictionary<string, ProviderAssembly> ProviderAssemblies = new Dictionary<string, ProviderAssembly>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, ProviderAssemblySourceIdentity> ProviderAssemblySourcesByIdentity = new Dictionary<string, ProviderAssemblySourceIdentity>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, Assembly> LoadedProviderAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, Assembly> LoadedProviderAssembliesByFingerprint = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, LoadedProviderAssemblyIdentity> LoadedProviderAssembliesByIdentity = new Dictionary<string, LoadedProviderAssemblyIdentity>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> ReportedProviderAssemblyIdentityConflicts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, IReadOnlyList<ManagedVisualPostfix>> VisualPostfixesByProvider = new Dictionary<string, IReadOnlyList<ManagedVisualPostfix>>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, ActiveProviderRuntime> ActiveProviderRuntimes = new Dictionary<string, ActiveProviderRuntime>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, ProviderRuntimeBlueprint> ProviderRuntimeBlueprints = new Dictionary<string, ProviderRuntimeBlueprint>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<Assembly, string> ScopedMonsterProviderAssemblies = new Dictionary<Assembly, string>();

	private static readonly Dictionary<Assembly, HashSet<string>> ProviderIdsByAssembly = new Dictionary<Assembly, HashSet<string>>();

	private static readonly Dictionary<Assembly, Dictionary<ulong, ProviderNodeProcessState>> SuspendedProviderNodes = new Dictionary<Assembly, Dictionary<ulong, ProviderNodeProcessState>>();

	private static readonly HashSet<Assembly> ActivatingProviderAssemblies = new HashSet<Assembly>();

	private static readonly HashSet<MethodBase> ScopedMonsterIsEnabledMethods = new HashSet<MethodBase>();

	private static readonly HashSet<MethodBase> ScopedMonsterSetEnabledMethods = new HashSet<MethodBase>();

	private static readonly HashSet<MethodBase> RunEnvironmentCallbackMethods = new HashSet<MethodBase>();

	private static readonly HashSet<MethodBase> DirectCharacterSkinPathMethods = new HashSet<MethodBase>();

	private static readonly Dictionary<MethodBase, Assembly> DirectCharacterSkinPathProviders = new Dictionary<MethodBase, Assembly>();

	private static readonly HashSet<string> RunEnvironmentProviderIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly ConcurrentDictionary<MethodBase, Func<object, string?>> ScopedMonsterIdAccessors = new ConcurrentDictionary<MethodBase, Func<object, string>>();

	private static readonly Harmony ScopedMonsterSelectionHarmony = new Harmony("Gurio.SkinChanger.scoped-monster-selection");

	private static readonly Harmony RunEnvironmentCallbackHarmony = new Harmony("Gurio.SkinChanger.run-environment-callbacks");

	private static readonly Harmony DirectCharacterSkinPathHarmony = new Harmony("Gurio.SkinChanger.direct-character-runtime");

	private static readonly HashSet<string> InvokedProviderInitializers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> ReportedVisualPostfixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> FailedVisualPostfixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, bool> ProviderSettingsEligibility = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, OptionalSkinFrameworkEvidence> OptionalFrameworkEvidence = new Dictionary<string, OptionalSkinFrameworkEvidence>(StringComparer.OrdinalIgnoreCase);

	private static bool _initialized;

	private static bool _reflectionTargetsReady;

	private static SceneTree? _providerNodeMonitorTree;

	private static long _runEnvironmentRefreshGeneration;

	public static bool IsBeforeAllSkinProviders { get; private set; } = true;

	public static IReadOnlyList<Mod> SkinProvidersBeforeSelf { get; private set; } = Array.Empty<Mod>();

	public static IReadOnlyList<Mod> SkinProvidersInLoadOrder { get; private set; } = Array.Empty<Mod>();

	public static IReadOnlyCollection<string> ProviderRoots => ProviderRootPaths;

	public static IReadOnlyCollection<string> PartTakeoverRoots
	{
		get
		{
			List<string> list = new List<string>();
			list.AddRange((from provider in ProvidersByIdentity.Values
				where provider.IsBaselinePartTakeover && !string.IsNullOrWhiteSpace(provider.RootPath)
				select NormalizePath(provider.RootPath)).Distinct<string>(StringComparer.OrdinalIgnoreCase));
			return new _003C_003Ez__ReadOnlyList<string>(list);
		}
	}

	public static IReadOnlyCollection<string> GetPreservedRuntimeRoots(IEnumerable<Mod> sourceMods)
	{
		Mod[] mods = sourceMods.ToArray();
		return (IReadOnlyCollection<string>)(object)(from mod in mods
			where mod.manifest != null && (Entry.IsSelfModId(mod.manifest.id) || ShouldTreatAsGameplayBaseline(mod, mods))
			select NormalizePath(mod.path)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	public static bool IsManagedProviderForDisplay(Mod? mod)
	{
		if (mod == null || Entry.IsSelfModId(mod.manifest?.id))
		{
			return false;
		}
		try
		{
			if (SkinWorkshopService.IsHotRegisteredResourceMod(mod))
			{
				return true;
			}
			bool isFunctionalHost = FrameworkCompatibilityLayer.IsKnownFrameworkHost(mod.manifest?.id);
			return ManagedProviderDisplayPolicy.IsManaged(mod.manifest?.id, ProviderManifestIds, isFunctionalHost) || ManagedProviderDisplayPolicy.IsManaged(mod.manifest?.id, ArtPackProviderIds, isFunctionalHost);
		}
		catch
		{
			return false;
		}
	}

	public static void RememberArtPackProviders(IEnumerable<string> packIds)
	{
		foreach (string packId in packIds)
		{
			if (!string.IsNullOrWhiteSpace(packId))
			{
				ArtPackProviderIds.Add(packId);
			}
		}
	}

	public static bool IsProviderAssemblyActive(Assembly assembly)
	{
		return ActiveProviderRuntimes.Values.Any((ActiveProviderRuntime runtime) => (object)runtime.Assembly == assembly);
	}

	internal static bool IsProviderAssemblyFor(string providerId, Assembly assembly)
	{
		if (ProviderIdsByAssembly.TryGetValue(assembly, out HashSet<string> value))
		{
			return value.Contains(providerId);
		}
		return false;
	}

	internal static Assembly? GetActiveProviderAssembly(string providerId)
	{
		if (!ActiveProviderRuntimes.TryGetValue(providerId, out ActiveProviderRuntime value))
		{
			return null;
		}
		return value.Assembly;
	}

	internal static Assembly? GetStatefulCardArtAssembly(string providerId)
	{
		if (!ProviderAssemblies.TryGetValue(providerId, out ProviderAssembly value) || StatefulCardArtContract.Read(value.AssemblyPath) == null)
		{
			return null;
		}
		Assembly? orLoadProviderAssembly = GetOrLoadProviderAssembly(value);
		if (orLoadProviderAssembly != null)
		{
			EnsureProviderGodotScripts(providerId);
		}
		return orLoadProviderAssembly;
	}

	internal static void EnsureProviderSettings(Mod mod)
	{
		if (!ManagedModInstances.Contains(mod) || mod.manifest?.id == null || !ProvidersByIdentity.TryGetValue(ProviderLookupKey(mod.path, mod.manifest.id), out SkinProviderProbe value) || !ProviderAssemblies.TryGetValue(value.Id, out ProviderAssembly value2))
		{
			return;
		}
		try
		{
			if (!ProviderSettingsEligibility.TryGetValue(value2.AssemblyPath, out var value3))
			{
				using FileStream input = File.OpenRead(value2.AssemblyPath);
				int changed;
				using MemoryStream memoryStream = MerchantSettingsAssemblyCompatibility.Rewrite(input, out changed);
				value3 = memoryStream != null && changed > 0;
				ProviderSettingsEligibility[value2.AssemblyPath] = value3;
			}
			if (value3)
			{
				Assembly orLoadProviderAssembly = GetOrLoadProviderAssembly(value2);
				if (orLoadProviderAssembly != null)
				{
					RegisterProviderAssembly(value.Id, orLoadProviderAssembly);
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("读取皮肤原设置失败：" + ex.GetBaseException().Message);
		}
	}

	public static void ConfigureRunEnvironmentProviders(IEnumerable<string> providerIds)
	{
		HashSet<string> next = providerIds.Where((string providerId) => !string.IsNullOrWhiteSpace(providerId)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (RunEnvironmentProviderIds.SetEquals(next))
		{
			return;
		}
		ActiveProviderRuntime[] array = (from pair in ActiveProviderRuntimes
			where pair.Value.RunEnvironmentEnabled && !next.Contains(pair.Key)
			select pair.Value).ToArray();
		ActiveProviderRuntime[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ResetRunEnvironmentControllers(array2[i].RunEnvironmentControllers);
		}
		RunEnvironmentProviderIds.Clear();
		RunEnvironmentProviderIds.UnionWith(next);
		bool flag = array.Length != 0;
		KeyValuePair<string, ActiveProviderRuntime>[] array3 = ActiveProviderRuntimes.ToArray();
		for (int i = 0; i < array3.Length; i++)
		{
			KeyValuePair<string, ActiveProviderRuntime> keyValuePair = array3[i];
			bool flag2 = next.Contains(keyValuePair.Key);
			if (keyValuePair.Value.RunEnvironmentEnabled != flag2)
			{
				if (flag2)
				{
					PatchProviderCallbacks(keyValuePair.Value.RunEnvironmentPatches);
				}
				else
				{
					UnpatchProviderCallbacks(keyValuePair.Value.RunEnvironmentPatches);
					StopRunEnvironmentControllers(keyValuePair.Value.RunEnvironmentControllers);
				}
				keyValuePair.Value.RunEnvironmentEnabled = flag2;
				flag = true;
				ModLog.Info($"已{(flag2 ? "启用" : "停用")} {keyValuePair.Key} 的地区背景与音乐行为；" + "怪物模型、动作和 Boss 图标仍按各自皮肤选择独立生效。");
			}
		}
		if (flag)
		{
			ScheduleRunEnvironmentPresentationRefresh();
		}
	}

	public static void RefreshRunEnvironmentPresentation()
	{
		Interlocked.Increment(ref _runEnvironmentRefreshGeneration);
		RefreshRunEnvironmentPresentationCore();
	}

	private static void ScheduleRunEnvironmentPresentationRefresh()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		long generation = Interlocked.Increment(ref _runEnvironmentRefreshGeneration);
		if (!(Engine.GetMainLoop() is SceneTree))
		{
			return;
		}
		Callable val = Callable.From((Action)delegate
		{
			if (generation == Volatile.Read(in _runEnvironmentRefreshGeneration))
			{
				RefreshRunEnvironmentPresentationCore();
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static void RefreshRunEnvironmentPresentationCore()
	{
		NRunMusicController instance = NRunMusicController.Instance;
		if (instance == null)
		{
			return;
		}
		try
		{
			CombatManager instance2 = CombatManager.Instance;
			CombatState val = instance2.DebugOnlyGetState();
			int isCombatInProgress;
			if (instance2.IsInProgress)
			{
				NCombatRoom instance3 = NCombatRoom.Instance;
				if (instance3 != null)
				{
					isCombatInProgress = (((Node)instance3).IsInsideTree() ? 1 : 0);
					goto IL_0030;
				}
			}
			isCombatInProgress = 0;
			goto IL_0030;
			IL_0030:
			RunEnvironmentMusicMode num = RunEnvironmentRefreshPolicy.SelectMusicMode((byte)isCombatInProgress != 0, val != null);
			object[] controllers = ActiveProviderRuntimes.Values.Where((ActiveProviderRuntime runtime) => runtime.RunEnvironmentEnabled).SelectMany((ActiveProviderRuntime runtime) => runtime.RunEnvironmentControllers).Distinct<object>(ReferenceEqualityComparer.Instance)
				.ToArray();
			ResetRunEnvironmentControllers(controllers);
			instance.StopCustomMusic();
			if (num == RunEnvironmentMusicMode.Combat && val != null)
			{
				instance.UpdateTrack();
				instance.UpdateAmbience();
				RestartRunEnvironmentControllersForCombat(controllers, val);
				ModLog.Info("已按当前遭遇重新加载战斗 BGM 与环境音。");
			}
			else
			{
				instance.UpdateMusic();
				instance.UpdateTrack();
				instance.UpdateAmbience();
				ModLog.Info("已按当前地区重新加载地图 BGM 与环境音。");
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("重新加载当前背景音乐失败：" + ex.GetBaseException().Message);
		}
	}

	public static void EnsureScopedMonsterSelectionRouter(string providerId)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		if (!ProviderAssemblies.TryGetValue(providerId, out ProviderAssembly value))
		{
			return;
		}
		try
		{
			Assembly orLoadProviderAssembly = GetOrLoadProviderAssembly(value);
			if (orLoadProviderAssembly == null)
			{
				return;
			}
			ScopedMonsterProviderAssemblies[orLoadProviderAssembly] = providerId;
			HarmonyMethod val = new HarmonyMethod(AccessTools.Method(typeof(ManagedSkinModLoader), "ScopedMonsterIsEnabledPrefix", (Type[])null, (Type[])null));
			HarmonyMethod val2 = new HarmonyMethod(AccessTools.Method(typeof(ManagedSkinModLoader), "ScopedMonsterSetEnabledPostfix", (Type[])null, (Type[])null));
			int num = 0;
			MethodInfo[] source = GetLoadableTypes(orLoadProviderAssembly).SelectMany((Type type) => type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)).ToArray();
			foreach (MethodInfo item in source.Where(IsScopedMonsterIsEnabledMethod))
			{
				if (ScopedMonsterIsEnabledMethods.Add(item))
				{
					ScopedMonsterIdAccessors[item] = ScopedMonsterRoutePolicy.CreateMonsterIdAccessor(item.GetParameters()[0].ParameterType);
					ScopedMonsterSelectionHarmony.Patch((MethodBase)item, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					num++;
				}
			}
			int num2 = 0;
			foreach (MethodInfo item2 in source.Where(IsScopedMonsterSetEnabledMethod))
			{
				if (ScopedMonsterSetEnabledMethods.Add(item2))
				{
					ScopedMonsterSelectionHarmony.Patch((MethodBase)item2, (HarmonyMethod)null, val2, (HarmonyMethod)null, (HarmonyMethod)null);
					num2++;
				}
			}
			if (num > 0 || num2 > 0)
			{
				ModLog.Info($"已接管 {value.Name} 的逐怪物启用判断；{num} 个读取入口、{num2} 个写入入口现在直接跟随" + "每个怪物在皮肤切换器中的选择。");
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("接管 " + value.Name + " 的逐怪物启用判断失败，将保留原作者配置：" + ex.GetBaseException().Message);
		}
	}

	private static bool IsScopedMonsterIsEnabledMethod(MethodInfo method)
	{
		if (method.Name.Equals("IsEnabled", StringComparison.Ordinal) && !(method.ReturnType != typeof(bool)))
		{
			ParameterInfo[] parameters = method.GetParameters();
			if (parameters != null && parameters.Length == 1)
			{
				ParameterInfo parameterInfo = parameters[0];
				if (parameterInfo != null)
				{
					Type parameterType = parameterInfo.ParameterType;
					return parameterType.GetProperty("Target", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.PropertyType.GetProperty("MonsterId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.PropertyType == typeof(string);
				}
			}
		}
		return false;
	}

	private static bool IsScopedMonsterSetEnabledMethod(MethodInfo method)
	{
		if (method.Name.Equals("SetEnabled", StringComparison.Ordinal) && !(method.ReturnType != typeof(void)))
		{
			ParameterInfo[] parameters = method.GetParameters();
			if (parameters != null && parameters.Length == 2)
			{
				ParameterInfo parameterInfo = parameters[0];
				if (parameterInfo != null)
				{
					Type parameterType = parameterInfo.ParameterType;
					ParameterInfo parameterInfo2 = parameters[1];
					if (parameterInfo2 != null)
					{
						Type parameterType2 = parameterInfo2.ParameterType;
						if (!(parameterType2 != typeof(bool)))
						{
							return parameterType.GetProperty("Target", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.PropertyType.GetProperty("MonsterId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.PropertyType == typeof(string);
						}
					}
				}
			}
		}
		return false;
	}

	private static bool ScopedMonsterIsEnabledPrefix(MethodBase __originalMethod, object __0, ref bool __result)
	{
		try
		{
			Assembly assembly = __originalMethod.DeclaringType?.Assembly;
			if (assembly == null || !ScopedMonsterProviderAssemblies.TryGetValue(assembly, out string value))
			{
				return true;
			}
			if (!ScopedMonsterIdAccessors.TryGetValue(__originalMethod, out Func<object, string> value2))
			{
				return true;
			}
			string text = value2(__0);
			if (string.IsNullOrWhiteSpace(text))
			{
				return true;
			}
			__result = SkinService.IsScopedMonsterRuntimeProviderSelected(value, text);
			return false;
		}
		catch
		{
			return true;
		}
	}

	private static void EnsureDirectCharacterSkinPathRouter(string providerId, Assembly assembly)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		HarmonyMethod val = new HarmonyMethod(AccessTools.Method(typeof(ManagedSkinModLoader), "DirectCharacterSkinPathPrefix", (Type[])null, (Type[])null));
		int num = 0;
		foreach (MethodInfo item in GetLoadableTypes(assembly).SelectMany((Type type) => type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).Where(IsDirectCharacterSkinPathMethod))
		{
			if (DirectCharacterSkinPathMethods.Add(item))
			{
				DirectCharacterSkinPathProviders[item] = assembly;
				DirectCharacterSkinPathHarmony.Patch((MethodBase)item, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				num++;
			}
		}
		if (num > 0)
		{
			ModLog.Info($"已为 {providerId} 接管 {num} 个按角色分发的 Spine 外观路径；" + "未选中的角色会得到空路径，不再被同一套运行时皮肤污染。 ");
		}
	}

	private static bool IsDirectCharacterSkinPathMethod(MethodInfo method)
	{
		if (method.ReturnType == typeof(string))
		{
			ParameterInfo[] parameters = method.GetParameters();
			if (parameters != null && parameters.Length == 1)
			{
				ParameterInfo parameterInfo = parameters[0];
				if (parameterInfo != null)
				{
					Type parameterType = parameterInfo.ParameterType;
					if (parameterType == typeof(string))
					{
						return method.Name.Contains("SkinPath", StringComparison.OrdinalIgnoreCase);
					}
				}
			}
		}
		return false;
	}

	internal static IReadOnlyList<MethodInfo> GetSelectedCharacterSkinPathMethods(string groupId)
	{
		string selectedCreatureRuntimeProvider = SkinService.GetSelectedCreatureRuntimeProvider(groupId);
		if (selectedCreatureRuntimeProvider == null || !ActiveProviderRuntimes.TryGetValue(selectedCreatureRuntimeProvider, out ActiveProviderRuntime runtime))
		{
			return Array.Empty<MethodInfo>();
		}
		return (from pair in DirectCharacterSkinPathProviders
			where (object)pair.Value == runtime.Assembly
			select (MethodInfo)pair.Key).ToArray();
	}

	private static bool DirectCharacterSkinPathPrefix(MethodBase __originalMethod, object[] __args, ref string __result)
	{
		try
		{
			if (!DirectCharacterSkinPathProviders.TryGetValue(__originalMethod, out Assembly value) || !ProviderIdsByAssembly.TryGetValue(value, out HashSet<string> value2))
			{
				return true;
			}
			string characterName = __args.OfType<string>().FirstOrDefault();
			if (string.IsNullOrWhiteSpace(characterName) || value2.Any((string providerId) => !ActiveProviderRuntimes.ContainsKey(providerId) || SkinService.IsDirectCharacterRuntimeProviderSelected(providerId, characterName)))
			{
				return true;
			}
			__result = string.Empty;
			return false;
		}
		catch
		{
			return true;
		}
	}

	private static void ScopedMonsterSetEnabledPostfix(MethodBase __originalMethod, object __0, bool __1)
	{
		try
		{
			Assembly assembly = __originalMethod.DeclaringType?.Assembly;
			if (!(assembly == null) && ScopedMonsterProviderAssemblies.TryGetValue(assembly, out string value))
			{
				object obj = __0.GetType().GetProperty("Target", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(__0);
				string text = obj?.GetType().GetProperty("MonsterId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj) as string;
				if (!string.IsNullOrWhiteSpace(text))
				{
					SkinService.ApplyScopedMonsterRuntimeProviderSelection(value, text, __1);
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("同步提供者怪物开关失败：" + ex.GetBaseException().Message);
		}
	}

	public static void Initialize()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		_reflectionTargetsReady = (InvokeOnModDetectedMethod != null || OnModDetectedField != null) && GameVersionField != null && CircularDependenciesField != null;
		if (!_reflectionTargetsReady)
		{
			ModLog.Error("无法解析游戏内部加载器接口，托管加载模式已禁用（游戏版本可能不兼容）。皮肤切换仍可工作，但 DLL 皮肤提供者的呈现补丁不会被接管。");
		}
		Mod[] array = ModManager.Mods.ToArray();
		SkinModDescriptor[] providerProbeDescriptors = GetProviderProbeDescriptors(array);
		string gamePckPath = null;
		try
		{
			gamePckPath = GamePackLocator.Resolve(OS.GetExecutablePath(), GameCommandLine.Arguments(), delegate(string message)
			{
				ModLog.Warn("定位游戏主资源包：" + message);
			});
		}
		catch (Exception ex)
		{
			ModLog.Warn("加载前无法建立游戏资源映射，少数仅含导入载荷的皮肤可能无法接管：" + ex.GetBaseException().Message);
		}
		foreach (SkinProviderProbe item in SkinCatalog.ProbeSkinProviders(providerProbeDescriptors, gamePckPath))
		{
			if (item.RootPath != null)
			{
				RememberProviderProbe(item);
			}
		}
		int num = Array.FindIndex(array, (Mod mod) => Entry.IsSelfModId(mod.manifest?.id));
		SkinProvidersInLoadOrder = (from mod in array.Where(IsProviderLoadCandidate)
			where !Entry.IsSelfModId(mod.manifest?.id)
			where IsManagedProvider(mod, out SkinProviderProbe _)
			select mod).ToArray();
		SkinProvidersBeforeSelf = ((num < 0) ? SkinProvidersInLoadOrder.Where(IsIsolatedProvider).ToArray() : (from mod in array.Take(num).Where(IsProviderLoadCandidate)
			where !Entry.IsSelfModId(mod.manifest?.id)
			select mod).Where(IsIsolatedProvider).ToArray());
		IsBeforeAllSkinProviders = num >= 0 && SkinProvidersBeforeSelf.Count == 0;
		if (SkinProvidersBeforeSelf.Count > 0)
		{
			string[] value = SkinProvidersBeforeSelf.Select((Mod mod) => mod.manifest?.name ?? mod.manifest?.id ?? mod.path).ToArray();
			ModLog.Warn("托管加载模式仅能拦截排在本 Mod 后面的皮肤提供者。请把皮肤切换器-Skin Changer 移到所有皮肤 Mod 之前并重启，无需移到全部 Mod 最前。 本次已提前加载：" + string.Join("、", value));
		}
		ModLog.Info($"托管加载模式已识别 {ProvidersByIdentity.Count} 个皮肤提供者；" + "PCK 资源由本 Mod 按选择接管；运行时私有依赖会在首次选中时低优先级挂载，DLL 行为补丁仅在选中期间启用。");
	}

	private static bool IsIsolatedProvider(Mod mod)
	{
		if (IsManagedProvider(mod, out SkinProviderProbe provider))
		{
			return !provider.IsBaselinePartTakeover;
		}
		return false;
	}

	public static bool TryManage(Mod mod)
	{
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Invalid comparison between Unknown and I4
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Invalid comparison between Unknown and I4
		bool flag = !_reflectionTargetsReady;
		if (!flag)
		{
			ModLoadState state = mod.state;
			bool flag2 = (((int)state == 0 || (int)state == 4) ? true : false);
			flag = !flag2;
		}
		if (flag || !IsProviderLoadCandidate(mod) || !IsManagedProvider(mod, out SkinProviderProbe provider))
		{
			return false;
		}
		if (provider.IsBaselinePartTakeover)
		{
			RememberPartTakeover(provider, mod.manifest?.name ?? provider.ResourceNamespaceId);
			return false;
		}
		if (ManagedModInstances.Contains(mod))
		{
			return true;
		}
		ModLoadState state2 = mod.state;
		try
		{
			if (!CanBypassOriginalLoader(mod))
			{
				return false;
			}
			SemanticVersion version = default(SemanticVersion);
			if (mod.manifest?.version != null && SemanticVersion.TryFromString(mod.manifest.version, ref version))
			{
				mod.version = version;
			}
			RememberProviderAssembly(mod, provider);
			mod.state = (ModLoadState)((!ModManager.Mods.Any((Mod other) => other != mod && (int)other.state == 1 && string.Equals(other.manifest?.id, mod.manifest?.id, StringComparison.OrdinalIgnoreCase))) ? 1 : 4);
			ManagedModInstances.Add(mod);
			NotifyModDetected(mod);
			ProviderManifestCompatibility.AcknowledgeManagedBranch(mod, provider.HasResourceBackedCosmetics);
			if ((int)state2 == 4 || !provider.Id.Equals(mod.manifest?.id, StringComparison.OrdinalIgnoreCase))
			{
				ModLog.Info($"已接管同 ID 皮肤来源：{provider.Id}；目录={mod.path}；原状态={state2}；注册方式={(((int)mod.state == 1) ? "原生代表" : "仅内部差分")}。");
			}
			ModLog.Info($"已隔离皮肤提供者 {mod.manifest?.name ?? mod.manifest?.id}：视觉组={provider.VisualGroupCount}, 卡图={provider.CardAssetCount}, 卡牌呈现={provider.CardPresentationCount}, 独立图片={provider.RuntimeImageCount}, 场景脚本={provider.ManagedScriptCount}, 交互场景={(provider.HasInteractiveScenes ? "是" : "否")}；" + "原 PCK 未全局挂载；只有选中该皮肤时，才会按需调用与模型 CreateVisuals 明确绑定的视觉后处理；" + ((provider.ManagedScriptCount > 0) ? "只有选中该皮肤时才注册场景实例化所需的 Godot 脚本类型；" : string.Empty) + "完整视觉整包或含交互场景的提供者会在选中期间临时启用原作者行为，其资源替换与卡牌呈现仍由皮肤切换器自身接管。");
			return true;
		}
		catch (Exception ex)
		{
			ManagedModInstances.Remove(mod);
			mod.state = state2;
			ModLog.Warn("托管 " + (mod.manifest?.name ?? mod.manifest?.id) + " 失败，将交回游戏原加载器：" + ex.GetBaseException().Message);
			return false;
		}
	}

	private static SkinModDescriptor[] GetProviderProbeDescriptors(IEnumerable<Mod> mods)
	{
		return (from mod in mods.Where(IsProviderLoadCandidate)
			where !Entry.IsSelfModId(mod.manifest?.id)
			select mod).Select(ToDescriptor).ToArray();
	}

	public static Mod[] GetCatalogMods()
	{
		return ModManager.Mods.Where((Mod mod) => (int)mod.state == 1 || ((int)mod.state == 4 && ManagedModInstances.Contains(mod))).ToArray();
	}

	private static bool IsProviderLoadCandidate(Mod mod)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Invalid comparison between Unknown and I4
		bool flag = mod.manifest?.id == null;
		if (!flag)
		{
			ModLoadState state = mod.state;
			bool flag2 = (((int)state <= 1 || (int)state == 4) ? true : false);
			flag = !flag2;
		}
		if (!flag)
		{
			object? obj = ModSettingsField?.GetValue(null);
			object? obj2 = ((obj is ModSettings) ? obj : null);
			if (obj2 == null || !((ModSettings)obj2).IsModDisabled(mod))
			{
				string workshopId = GetWorkshopSourceId(mod.path);
				if ((int)mod.state == 4 && workshopId != null)
				{
					return !ModManager.Mods.Any(delegate(Mod other)
					{
						//IL_0011: Unknown result type (might be due to invalid IL or missing references)
						//IL_0016: Unknown result type (might be due to invalid IL or missing references)
						//IL_0017: Unknown result type (might be due to invalid IL or missing references)
						//IL_0019: Invalid comparison between Unknown and I4
						bool flag3 = other != mod;
						if (flag3)
						{
							ModLoadState state2 = other.state;
							bool flag4 = (int)state2 <= 1;
							flag3 = flag4;
						}
						return flag3 && string.Equals(other.manifest?.id, mod.manifest.id, StringComparison.OrdinalIgnoreCase) && GetWorkshopSourceId(other.path) == workshopId;
					});
				}
				return true;
			}
		}
		return false;
	}

	private static string? GetWorkshopSourceId(string root)
	{
		string[] array = root.Replace('\\', '/').TrimEnd('/').Split('/');
		if (array.Length < 2 || !ulong.TryParse(array[^1], out var result) || result == 0L)
		{
			return null;
		}
		if (!array[^2].Equals("_workshop_formal_cache", StringComparison.OrdinalIgnoreCase) && (array.Length < 4 || !array[^3].Equals("content", StringComparison.OrdinalIgnoreCase) || !array[^4].Equals("workshop", StringComparison.OrdinalIgnoreCase)))
		{
			return null;
		}
		return array[^1];
	}

	private static void RememberProviderAssembly(Mod mod, SkinProviderProbe provider)
	{
		ModManifest manifest = mod.manifest;
		if (manifest == null || !manifest.hasDll || manifest.id == null || FrameworkCompatibilityLayer.IsBundledFrameworkHost(mod.manifest.id))
		{
			return;
		}
		string fullPath = Path.GetFullPath(SkinPackagePaths.Resolve(mod.path, mod.manifest.id, ".dll"));
		if (!File.Exists(fullPath))
		{
			ModLog.Warn("找不到皮肤提供者程序集 " + fullPath + "。");
			return;
		}
		if (!Path.GetFileNameWithoutExtension(fullPath).Equals(mod.manifest.id, StringComparison.OrdinalIgnoreCase))
		{
			ModLog.Info($"已按清单文件配对定位 {mod.manifest.id} 的皮肤包：{Path.GetFileName(fullPath)}；逻辑 ID 和原文件保持不变。");
		}
		bool flag = ManagedCharacterAssetReplacementScanner.Scan(mod.path, mod.manifest.id).Count > 0 || FrameworkSkinContractScanner.Scan(mod.path, mod.manifest.id).Count > 0;
		string text = ComputeProviderAssemblyFingerprint(fullPath);
		string text2 = TryReadProviderAssemblyIdentity(fullPath);
		bool flag2 = true;
		if (text2 != null)
		{
			if (ProviderAssemblySourcesByIdentity.TryGetValue(text2, out ProviderAssemblySourceIdentity value))
			{
				flag2 = value.Fingerprint.Equals(text, StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				ProviderAssemblySourcesByIdentity[text2] = new ProviderAssemblySourceIdentity(text, fullPath);
			}
		}
		ProviderAssembly providerAssembly = new ProviderAssembly(fullPath, mod.manifest.name ?? mod.manifest.id, provider.ManagedScriptCount > 0 || flag, flag, flag2);
		ProviderAssemblies[provider.Id] = providerAssembly;
		if (!flag2)
		{
			ReportProviderAssemblyIdentityConflict(providerAssembly, text2, text, ProviderAssemblySourcesByIdentity[text2].Fingerprint);
		}
	}

	public static bool EnsureProviderGodotScripts(string providerId)
	{
		if (!ProviderAssemblies.TryGetValue(providerId, out ProviderAssembly value) || !value.HasGodotScripts)
		{
			return false;
		}
		Assembly assembly = null;
		try
		{
			assembly = GetOrLoadProviderAssembly(value);
			if (assembly == null)
			{
				return false;
			}
			if (!RegisteredProviderAssemblies.Add(assembly))
			{
				return true;
			}
			MethodInfo methodInfo = typeof(GodotObject).Assembly.GetType("Godot.Bridge.ScriptManagerBridge")?.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(delegate(MethodInfo method)
			{
				if (method.Name.Equals("LookupScriptsInAssembly", StringComparison.Ordinal))
				{
					ParameterInfo[] parameters = method.GetParameters();
					if (parameters != null && parameters.Length == 1)
					{
						ParameterInfo parameterInfo = parameters[0];
						if (parameterInfo != null)
						{
							Type parameterType = parameterInfo.ParameterType;
							return parameterType == typeof(Assembly);
						}
					}
				}
				return false;
			});
			if (methodInfo == null)
			{
				RegisteredProviderAssemblies.Remove(assembly);
				ModLog.Warn("无法找到 Godot 场景脚本注册入口，" + value.Name + " 的自定义场景脚本可能无法实例化。");
				return false;
			}
			methodInfo.Invoke(null, new object[1] { assembly });
			StatefulCardArtContract statefulCardArtContract = StatefulCardArtContract.Read(value.AssemblyPath);
			if ((object)statefulCardArtContract != null)
			{
				StatefulCardArtRuntime.Ensure(providerId, statefulCardArtContract);
			}
			ModLog.Info("已按当前选择注册 " + value.Name + " 的 Godot 场景脚本类型，此注册步骤本身未执行其初始化器或 Harmony 补丁。");
			return true;
		}
		catch (Exception ex)
		{
			if (IsAlreadyRegisteredGodotScriptException(ex))
			{
				ModLog.Info(value.Name + " 的 Godot 场景脚本已经注册，复用现有注册结果。");
				return true;
			}
			if (assembly != null)
			{
				RegisteredProviderAssemblies.Remove(assembly);
			}
			ModLog.Warn("注册 " + value.Name + " 的 Godot 场景脚本失败；仍会隔离其全局视觉补丁：" + ex.GetBaseException().Message);
			return false;
		}
	}

	private static bool IsAlreadyRegisteredGodotScriptException(Exception exception)
	{
		return GodotScriptPathRegistrationPolicy.IsHarmlessDuplicate(exception);
	}

	public static void DeactivateProvidersExcept(IEnumerable<string> selectedProviderIds)
	{
		HashSet<string> selected = selectedProviderIds.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		string[] array = ActiveProviderRuntimes.Keys.Where((string providerId) => !selected.Contains(providerId)).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			DeactivateProvider(array[i]);
		}
	}

	public static void ActivateSelectedProviders(IEnumerable<string> selectedProviderIds)
	{
		foreach (string item in selectedProviderIds.Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			if (!ActiveProviderRuntimes.ContainsKey(item) && ProviderAssemblies.TryGetValue(item, out ProviderAssembly value))
			{
				ActivateProvider(item, value);
			}
		}
	}

	private static void ActivateProvider(string providerId, ProviderAssembly provider)
	{
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		StatefulCardArtContract statefulCardArtContract = StatefulCardArtContract.Read(provider.AssemblyPath);
		if ((object)statefulCardArtContract != null)
		{
			StatefulCardArtRuntime.Ensure(providerId, statefulCardArtContract);
			return;
		}
		Assembly assembly = null;
		IReadOnlyList<object> readOnlyList = Array.Empty<object>();
		bool runEnvironmentEnabled = RunEnvironmentProviderIds.Contains(providerId);
		long timestamp = Stopwatch.GetTimestamp();
		long num = timestamp;
		long num2 = timestamp;
		try
		{
			assembly = GetOrLoadProviderAssembly(provider);
			num = Stopwatch.GetTimestamp();
			if (assembly == null)
			{
				return;
			}
			RegisterProviderAssembly(providerId, assembly);
			SkinCatalog? catalog = SkinService.Catalog;
			if (catalog != null && catalog.ProviderUsesDirectCharacterRuntime(providerId))
			{
				EnsureDirectCharacterSkinPathRouter(providerId, assembly);
			}
			ActivatingProviderAssemblies.Add(assembly);
			EnsureProviderNodeMonitor();
			RestoreProviderNodes(assembly);
			bool flag = FrameworkProviderInitializerPolicy.RunsAuthorInitializer(provider.HasDeclarativeCharacterAssetReplacement, FrameworkRegistryCooperation.IsNativeProvider(providerId));
			(Type, ModInitializerAttribute)[] array = (flag ? (from type in GetLoadableTypes(assembly)
				select (Type: type, Attribute: ((MemberInfo)type).GetCustomAttribute<ModInitializerAttribute>()) into pair
				where pair.Attribute != null
				select pair).ToArray() : Array.Empty<(Type, ModInitializerAttribute)>());
			bool flag2 = false;
			bool flag3 = false;
			int num3 = -1;
			if (array.Length == 0)
			{
				EnsureProviderGodotScripts(providerId);
				new Harmony("Gurio.SkinChanger.selected." + NormalizeHarmonyId(providerId)).PatchAll(assembly);
				if (provider.HasDeclarativeCharacterAssetReplacement && !flag)
				{
					ModLog.Info(provider.Name + " 使用框架式角色资源注册；资源已由皮肤切换器直接路由，其原初始化器由原皮肤管理器执行，本 Mod 不重复执行。");
				}
			}
			else
			{
				(Type, ModInitializerAttribute)[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					(Type, ModInitializerAttribute) tuple = array2[i];
					MethodInfo method = tuple.Item1.GetMethod(tuple.Item2.initializerMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (method == null || method.GetParameters().Length != 0)
					{
						throw new MissingMethodException(tuple.Item1.FullName, tuple.Item2.initializerMethod);
					}
					string item = BuildInitializerKey(assembly, tuple.Item1, method);
					if (InvokedProviderInitializers.Contains(item))
					{
						flag2 = true;
						ModLog.Info($"已跳过 {provider.Name} 的重复初始化器 {tuple.Item1.FullName}.{method.Name}。");
						continue;
					}
					num3 = CaptureProviderPatches(assembly).Count;
					try
					{
						method.Invoke(null, null);
						InvokedProviderInitializers.Add(item);
					}
					catch (Exception ex) when (FrameworkProviderInitializerPolicy.KeepsSessionWhenAuthorInitializerFails(provider.HasDeclarativeCharacterAssetReplacement))
					{
						flag3 = true;
						InvokedProviderInitializers.Add(item);
						ModLog.Warn("执行 " + provider.Name + " 的原初始化器失败；其资源与场景创建已由本 Mod 接管，因此保留本次视觉会话并改用等价步骤：" + ex.GetBaseException().Message);
						break;
					}
					if (provider.HasDeclarativeCharacterAssetReplacement)
					{
						ModLog.Info(provider.Name + " 的原初始化器已执行：作者的补丁、场景脚本类型、自带设置页与资源预载按原样生效；框架注册写入已由本 Mod 拦截。");
					}
				}
				if (flag3)
				{
					EnsureProviderGodotScripts(providerId);
					if (CaptureProviderPatches(assembly).Count == num3)
					{
						new Harmony("Gurio.SkinChanger.selected." + NormalizeHarmonyId(providerId)).PatchAll(assembly);
					}
					else
					{
						ModLog.Info(provider.Name + " 的原初始化器在失败前已装入作者补丁，不再重复应用。");
					}
				}
				else if (flag2)
				{
					if (ProviderRuntimeBlueprints.TryGetValue(providerId, out ProviderRuntimeBlueprint value))
					{
						PatchProviderCallbacks(value.BehaviorPatches.Where((ProviderPatch patch) => runEnvironmentEnabled || !IsRunEnvironmentPatch(patch)));
					}
					else
					{
						new Harmony("Gurio.SkinChanger.selected." + NormalizeHarmonyId(providerId)).PatchAll(assembly);
					}
				}
			}
			FrameworkCompatibilityLayer.NotifyProviderActivated(assembly);
			num2 = Stopwatch.GetTimestamp();
			IReadOnlyList<ProviderPatch> source = CaptureProviderPatches(assembly);
			ProviderPatch[] characterPresentationPatches = source.Where(IsReplayableCharacterPresentationPatch).ToArray();
			ProviderPatch[] nodeReadyPresentationPatches = source.Where((ProviderPatch patch) => IsNodeReadyPresentationTarget(patch.Target)).ToArray();
			ProviderPatch[] array3 = source.Where((ProviderPatch patch) => IsManagedResourceOwnershipPatch(patch) || IsManagedCharacterPresentationPatch(patch) || IsManagedNodeReadyPresentationPatch(patch) || ManagedMerchantSettingsBridge.OwnsSettingsPatch(patch.Callback)).ToArray();
			IReadOnlyList<ProviderPatch> readOnlyList2 = source.Where((ProviderPatch patch) => !IsManagedResourceOwnershipPatch(patch) && !IsManagedCharacterPresentationPatch(patch) && !IsManagedNodeReadyPresentationPatch(patch) && !ManagedMerchantSettingsBridge.OwnsSettingsPatch(patch.Callback)).ToArray();
			if (flag2 && ProviderRuntimeBlueprints.TryGetValue(providerId, out ProviderRuntimeBlueprint value2))
			{
				readOnlyList2 = value2.BehaviorPatches;
			}
			int num4;
			ProviderPatch[] array4;
			if (ScopedMonsterProviderAssemblies.TryGetValue(assembly, out string value3))
			{
				num4 = (value3.Equals(providerId, StringComparison.OrdinalIgnoreCase) ? 1 : 0);
				if (num4 != 0)
				{
					array4 = readOnlyList2.Where(IsRunEnvironmentPatch).ToArray();
					goto IL_04d6;
				}
			}
			else
			{
				num4 = 0;
			}
			array4 = Array.Empty<ProviderPatch>();
			goto IL_04d6;
			IL_04d6:
			ProviderPatch[] array5 = array4;
			UnpatchProviderCallbacks(array3);
			if (!runEnvironmentEnabled)
			{
				UnpatchProviderCallbacks(array5);
			}
			ProviderPatch[] array6 = (from patch in CaptureProviderPatches(assembly)
				where IsManagedResourceOwnershipPatch(patch) || IsManagedCharacterPresentationPatch(patch) || IsManagedNodeReadyPresentationPatch(patch) || ManagedMerchantSettingsBridge.OwnsSettingsPatch(patch.Callback)
				select patch).ToArray();
			if (array6.Length != 0)
			{
				throw new InvalidOperationException($"仍有 {array6.Length} 个资源/选角呈现补丁未能隔离");
			}
			if (num4 != 0)
			{
				EnsureRunEnvironmentCallbackGuards(providerId, assembly);
				readOnlyList = DiscoverRunEnvironmentControllers(assembly);
				if (!runEnvironmentEnabled)
				{
					StopRunEnvironmentControllers(readOnlyList);
				}
			}
			ProviderRuntimeBlueprint value4;
			if (!ProviderRuntimeBlueprints.ContainsKey(providerId))
			{
				ProviderRuntimeBlueprints[providerId] = new ProviderRuntimeBlueprint(readOnlyList2, characterPresentationPatches, nodeReadyPresentationPatches);
			}
			else if (flag2 && ProviderRuntimeBlueprints.TryGetValue(providerId, out value4))
			{
				characterPresentationPatches = value4.CharacterPresentationPatches.ToArray();
				nodeReadyPresentationPatches = value4.NodeReadyPresentationPatches.ToArray();
			}
			ActiveProviderRuntimes[providerId] = new ActiveProviderRuntime(assembly, readOnlyList2, characterPresentationPatches, nodeReadyPresentationPatches, array5, readOnlyList, runEnvironmentEnabled);
			ModLog.Info($"已按当前选择启用 {provider.Name} 的完整视觉会话：所需资源已由本 Mod 挂载，{array3.Length} 个资源/选角呈现入口已交由本 Mod 接管，保留 {readOnlyList2.Count - ((!runEnvironmentEnabled) ? array5.Length : 0)} 个原作者动画/场景行为补丁，地区环境行为={(runEnvironmentEnabled ? "启用" : "停用")}；" + "切换离开后会自动卸载行为补丁。");
		}
		catch (Exception ex2)
		{
			if (assembly != null)
			{
				UnpatchProviderCallbacks(CaptureProviderPatches(assembly));
			}
			ModLog.Warn("启用 " + provider.Name + " 的完整视觉会话失败，已回滚其行为补丁并继续使用资源皮肤：" + ex2.GetBaseException().Message);
		}
		finally
		{
			if (assembly != null)
			{
				ActivatingProviderAssemblies.Remove(assembly);
				if (!IsProviderAssemblyActive(assembly))
				{
					SuspendProviderNodes(assembly);
				}
			}
			TimeSpan elapsedTime = Stopwatch.GetElapsedTime(timestamp);
			if (elapsedTime >= TimeSpan.FromMilliseconds(50L, 0L))
			{
				TimeSpan elapsedTime2 = Stopwatch.GetElapsedTime(timestamp, num);
				TimeSpan elapsedTime3 = Stopwatch.GetElapsedTime(num, num2);
				TimeSpan elapsedTime4 = Stopwatch.GetElapsedTime(num2, Stopwatch.GetTimestamp());
				ModLog.Info($"{provider.Name} 皮肤代码激活耗时：程序集={elapsedTime2.TotalMilliseconds:F1} ms，初始化={elapsedTime3.TotalMilliseconds:F1} ms，隔离={elapsedTime4.TotalMilliseconds:F1} ms，总计={elapsedTime.TotalMilliseconds:F1} ms。");
			}
		}
	}

	private static string BuildInitializerKey(Assembly assembly, Type initializerType, MethodInfo method)
	{
		return assembly.ManifestModule.ModuleVersionId.ToString("N") + "|" + initializerType.FullName + "|" + method.MetadataToken;
	}

	private static void DeactivateProvider(string providerId)
	{
		if (!ActiveProviderRuntimes.Remove(providerId, out ActiveProviderRuntime runtime))
		{
			return;
		}
		IReadOnlyList<ActionFailure> readOnlyList = FailureIsolatedActionRunner.Run(new global::_003C_003Ez__ReadOnlyArray<(string, Action)>(new(string, Action)[5]
		{
			("恢复场景节点", delegate
			{
				RestoreNodeReadyBehaviors(runtime);
			}),
			("恢复角色呈现", delegate
			{
				RestoreCharacterPresentations(runtime);
			}),
			("卸载行为补丁", delegate
			{
				UnpatchProviderCallbacks(runtime.Patches);
			}),
			("停止地区控制器", delegate
			{
				StopRunEnvironmentControllers(runtime.RunEnvironmentControllers);
			}),
			("停用提供者节点", delegate
			{
				if (!IsProviderAssemblyActive(runtime.Assembly))
				{
					SuspendProviderNodes(runtime.Assembly);
				}
			})
		}));
		foreach (ActionFailure item in readOnlyList)
		{
			ModLog.Warn($"停用皮肤提供者 {providerId} 的“{item.Stage}”阶段失败；" + "其余清理阶段已继续执行：" + item.Exception);
		}
		ModLog.Info($"已停用未选中皮肤提供者 {providerId} 的 {runtime.Patches.Count} 个行为补丁" + ((readOnlyList.Count == 0) ? "。" : $"；{readOnlyList.Count} 个清理阶段失败并已记录。"));
	}

	private static void RegisterProviderAssembly(string providerId, Assembly assembly)
	{
		if (!ProviderIdsByAssembly.TryGetValue(assembly, out HashSet<string> value))
		{
			value = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			ProviderIdsByAssembly[assembly] = value;
		}
		value.Add(providerId);
		ManagedMerchantSettingsBridge.Install(assembly);
		ManagedSkinBehaviorBridge.Install(assembly);
	}

	private static void EnsureProviderNodeMonitor()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null || val == _providerNodeMonitorTree)
		{
			return;
		}
		if (_providerNodeMonitorTree != null)
		{
			SceneTree? providerNodeMonitorTree = _providerNodeMonitorTree;
			object obj = _003C_003EO._003C8_003E__OnProviderNodeAdded;
			if (obj == null)
			{
				NodeAddedEventHandler val2 = OnProviderNodeAdded;
				_003C_003EO._003C8_003E__OnProviderNodeAdded = val2;
				obj = (object)val2;
			}
			providerNodeMonitorTree.NodeAdded -= (NodeAddedEventHandler)obj;
		}
		_providerNodeMonitorTree = val;
		object obj2 = _003C_003EO._003C8_003E__OnProviderNodeAdded;
		if (obj2 == null)
		{
			NodeAddedEventHandler val3 = OnProviderNodeAdded;
			_003C_003EO._003C8_003E__OnProviderNodeAdded = val3;
			obj2 = (object)val3;
		}
		val.NodeAdded += (NodeAddedEventHandler)obj2;
	}

	private static void OnProviderNodeAdded(Node node)
	{
		Assembly assembly = ((object)node).GetType().Assembly;
		if (ProviderIdsByAssembly.ContainsKey(assembly) && !ActivatingProviderAssemblies.Contains(assembly) && !IsProviderAssemblyActive(assembly))
		{
			SuspendProviderNode(assembly, node);
		}
	}

	private static void SuspendProviderNodes(Assembly assembly)
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val.Root))
		{
			return;
		}
		foreach (Node item in from node in EnumerateNodeTree((Node)(object)val.Root)
			where (object)((object)node).GetType().Assembly == assembly
			select node)
		{
			SuspendProviderNode(assembly, item);
		}
	}

	private static void SuspendProviderNode(Assembly assembly, Node node)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			if (!SuspendedProviderNodes.TryGetValue(assembly, out Dictionary<ulong, ProviderNodeProcessState> value))
			{
				value = new Dictionary<ulong, ProviderNodeProcessState>();
				SuspendedProviderNodes[assembly] = value;
			}
			ulong instanceId = ((GodotObject)node).GetInstanceId();
			if (!value.ContainsKey(instanceId))
			{
				value[instanceId] = new ProviderNodeProcessState(new WeakReference<Node>(node), node.ProcessMode);
			}
			node.ProcessMode = (ProcessModeEnum)4;
		}
	}

	private static void RestoreProviderNodes(Assembly assembly)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (!SuspendedProviderNodes.Remove(assembly, out Dictionary<ulong, ProviderNodeProcessState> value))
		{
			return;
		}
		foreach (ProviderNodeProcessState value2 in value.Values)
		{
			if (value2.Node.TryGetTarget(out Node target) && GodotObject.IsInstanceValid((GodotObject)(object)target))
			{
				target.ProcessMode = value2.ProcessMode;
			}
		}
	}

	private static IReadOnlyList<ProviderPatch> CaptureProviderPatches(Assembly assembly)
	{
		return Harmony.GetAllPatchedMethods().SelectMany((MethodBase target) => EnumerateProviderPatches(target, Harmony.GetPatchInfo(target), assembly)).DistinctBy((ProviderPatch patch) => (Target: patch.Target, Callback: patch.Callback, Kind: patch.Kind))
			.ToArray();
	}

	[IteratorStateMachine(typeof(_003CEnumerateProviderPatches_003Ed__107))]
	private static IEnumerable<ProviderPatch> EnumerateProviderPatches(MethodBase target, Patches? patches, Assembly assembly)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateProviderPatches_003Ed__107(-2)
		{
			_003C_003E3__target = target,
			_003C_003E3__patches = patches,
			_003C_003E3__assembly = assembly
		};
	}

	private static bool IsManagedResourceOwnershipPatch(ProviderPatch patch)
	{
		Type declaringType = patch.Target.DeclaringType;
		if (declaringType == null)
		{
			return false;
		}
		if (declaringType == typeof(Creature) && patch.Target.Name == "CreateVisuals")
		{
			if (patch.Kind == ProviderPatchKind.Postfix)
			{
				Type declaringType2 = patch.Callback.DeclaringType;
				if ((object)declaringType2 != null && GetVisualPatchTarget(declaringType2) == typeof(Creature))
				{
					return CanReplayVisualPostfix(patch.Callback, typeof(Creature));
				}
			}
			return false;
		}
		if (declaringType.Name.Equals("NRelic", StringComparison.Ordinal) && patch.Target.Name.Equals("_Ready", StringComparison.Ordinal))
		{
			return true;
		}
		if (declaringType == typeof(AssetCache) || declaringType == typeof(ResourceLoader) || declaringType == typeof(AtlasManager) || declaringType == typeof(SceneHelper) || declaringType == typeof(ImageHelper))
		{
			if (!patch.Target.Name.StartsWith("Get", StringComparison.Ordinal) && !patch.Target.Name.StartsWith("Load", StringComparison.Ordinal) && !patch.Target.Name.StartsWith("Instantiate", StringComparison.Ordinal))
			{
				return patch.Target.Name.Contains("Path", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		if (!typeof(CharacterModel).IsAssignableFrom(declaringType) && !typeof(MonsterModel).IsAssignableFrom(declaringType))
		{
			return false;
		}
		if (patch.Target.Name.Equals("CreateVisuals", StringComparison.Ordinal) && (typeof(CharacterModel).IsAssignableFrom(declaringType) || typeof(MonsterModel).IsAssignableFrom(declaringType)))
		{
			return true;
		}
		if (!patch.Target.Name.StartsWith("get_", StringComparison.Ordinal))
		{
			return false;
		}
		string name = patch.Target.Name;
		string propertyName = name.Substring(4, name.Length - 4);
		return new string[8] { "Visual", "Scene", "Portrait", "Icon", "Texture", "Background", "MapMarker", "Path" }.Any((string token) => propertyName.Contains(token, StringComparison.OrdinalIgnoreCase));
	}

	private static bool IsManagedCharacterPresentationPatch(ProviderPatch patch)
	{
		return IsCharacterPresentationTarget(patch.Target);
	}

	private static bool IsReplayableCharacterPresentationPatch(ProviderPatch patch)
	{
		if (patch.Kind == ProviderPatchKind.Postfix)
		{
			return IsCharacterPresentationTarget(patch.Target);
		}
		return false;
	}

	private static bool IsManagedNodeReadyPresentationPatch(ProviderPatch patch)
	{
		return IsNodeReadyPresentationTarget(patch.Target);
	}

	private static bool IsNodeReadyPresentationTarget(MethodBase target)
	{
		bool flag = target.Name.Equals("_Ready", StringComparison.Ordinal) && target.DeclaringType != null;
		if (flag)
		{
			bool flag2 = typeof(NCreature).IsAssignableFrom(target.DeclaringType) || typeof(NMerchantRoom).IsAssignableFrom(target.DeclaringType) || typeof(NRestSiteRoom).IsAssignableFrom(target.DeclaringType) || typeof(NBossMapPoint).IsAssignableFrom(target.DeclaringType);
			if (!flag2)
			{
				bool flag3;
				switch (target.DeclaringType.Name)
				{
				case "NMerchantButton":
				case "NMerchantHand":
				case "NMerchantInventory":
				case "NMerchantSlot":
				case "NMerchantDialogue":
				case "NFakeMerchant":
					flag3 = true;
					break;
				default:
					flag3 = false;
					break;
				}
				flag2 = flag3;
			}
			flag = flag2;
		}
		return flag;
	}

	public static void ReplaySelectedCharacterPresentation(string providerId, NCharacterSelectScreen screen, NCharacterSelectButton button, CharacterModel character, Action? afterReplay = null)
	{
		if (!ActiveProviderRuntimes.TryGetValue(providerId, out ActiveProviderRuntime value))
		{
			return;
		}
		RestoreCharacterPresentation(value, screen);
		Dictionary<ulong, CharacterPresentationNodeState> baseline = CaptureCharacterPresentationState(screen);
		int num = 0;
		foreach (ProviderPatch characterPresentationPatch in value.CharacterPresentationPatches)
		{
			object instance = (typeof(NCharacterSelectScreen).IsAssignableFrom(characterPresentationPatch.Target.DeclaringType) ? ((object)screen) : ((object)button));
			if (TryBuildCharacterPresentationArguments(characterPresentationPatch.Callback, characterPresentationPatch.Target, instance, button, character, out object[] arguments))
			{
				try
				{
					characterPresentationPatch.Callback.Invoke(null, arguments);
					num++;
				}
				catch (Exception ex)
				{
					ModLog.Warn($"重放 {providerId} 的选角呈现 {characterPresentationPatch.Callback.DeclaringType?.FullName}.{characterPresentationPatch.Callback.Name} 失败：{ex.GetBaseException().Message}");
				}
			}
		}
		afterReplay?.Invoke();
		TrackCharacterPresentationMutation(value, screen, baseline);
		if (num > 0)
		{
			ModLog.Info($"已在恢复游戏基线后重放 {providerId} 的 {num} 个选角呈现步骤。");
		}
	}

	public static void RestoreCharacterPresentation(NCharacterSelectScreen screen)
	{
		foreach (ActiveProviderRuntime value in ActiveProviderRuntimes.Values)
		{
			RestoreCharacterPresentation(value, screen);
		}
	}

	private static Dictionary<ulong, CharacterPresentationNodeState> CaptureCharacterPresentationState(NCharacterSelectScreen screen)
	{
		Dictionary<ulong, CharacterPresentationNodeState> dictionary = new Dictionary<ulong, CharacterPresentationNodeState>();
		foreach (Node item in EnumerateNodeTree((Node)(object)screen))
		{
			ulong instanceId = ((GodotObject)item).GetInstanceId();
			CanvasItem val = (CanvasItem)(object)((item is CanvasItem) ? item : null);
			bool? visible = ((val != null) ? new bool?(val.Visible) : ((bool?)null));
			string? characterPresentationText = GetCharacterPresentationText(item);
			Control val2 = (Control)(object)((item is Control) ? item : null);
			dictionary[instanceId] = new CharacterPresentationNodeState(item, visible, characterPresentationText, (val2 != null) ? new bool?(val2.ClipContents) : ((bool?)null), CaptureCharacterPresentationMetadata(item), CaptureCharacterPresentationResizeConnections(item));
		}
		return dictionary;
	}

	private static void TrackCharacterPresentationMutation(ActiveProviderRuntime runtime, NCharacterSelectScreen screen, IReadOnlyDictionary<ulong, CharacterPresentationNodeState> baseline)
	{
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		List<WeakReference<Node>> list = new List<WeakReference<Node>>();
		foreach (Node item in EnumerateNodeTree((Node)(object)screen))
		{
			ulong instanceId = ((GodotObject)item).GetInstanceId();
			if (!baseline.ContainsKey(instanceId))
			{
				Node parent = item.GetParent();
				if (parent == null || baseline.ContainsKey(((GodotObject)parent).GetInstanceId()))
				{
					list.Add(new WeakReference<Node>(item));
				}
			}
		}
		List<CharacterPresentationVisibilityChange> list2 = new List<CharacterPresentationVisibilityChange>();
		List<CharacterPresentationTextChange> list3 = new List<CharacterPresentationTextChange>();
		List<CharacterPresentationClipChange> list4 = new List<CharacterPresentationClipChange>();
		List<CharacterPresentationMetadataChange> list5 = new List<CharacterPresentationMetadataChange>();
		List<(WeakReference<Node>, Callable)> list6 = new List<(WeakReference<Node>, Callable)>();
		foreach (CharacterPresentationNodeState value in baseline.Values)
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)value.Node))
			{
				continue;
			}
			bool? visible = value.Visible;
			if (visible.HasValue)
			{
				bool valueOrDefault = visible == true;
				Node node = value.Node;
				CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
				if (val != null && val.Visible != valueOrDefault)
				{
					list2.Add(new CharacterPresentationVisibilityChange(new WeakReference<CanvasItem>(val), valueOrDefault, val.Visible));
				}
			}
			PresentationMetadataChange<Variant>[] array = PresentationMetadataChanges.Capture(value.Metadata, CaptureCharacterPresentationMetadata(value.Node));
			foreach (PresentationMetadataChange<Variant> change in array)
			{
				list5.Add(new CharacterPresentationMetadataChange(new WeakReference<Node>(value.Node), change));
			}
			foreach (Callable item2 in CaptureCharacterPresentationResizeConnections(value.Node).Except(value.ResizeConnections))
			{
				list6.Add((new WeakReference<Node>(value.Node), item2));
			}
			string text = value.Text;
			if (text != null)
			{
				string characterPresentationText = GetCharacterPresentationText(value.Node);
				if (characterPresentationText != null && characterPresentationText != text)
				{
					list3.Add(new CharacterPresentationTextChange(new WeakReference<Node>(value.Node), text, characterPresentationText));
				}
			}
			visible = value.ClipContents;
			if (visible.HasValue)
			{
				bool valueOrDefault2 = visible == true;
				Node node2 = value.Node;
				Control val2 = (Control)(object)((node2 is Control) ? node2 : null);
				if (val2 != null && val2.ClipContents != valueOrDefault2)
				{
					list4.Add(new CharacterPresentationClipChange(new WeakReference<Control>(val2), valueOrDefault2, val2.ClipContents));
				}
			}
		}
		if (list.Count != 0 || list2.Count != 0 || list3.Count != 0 || list4.Count != 0 || list5.Count != 0 || list6.Count != 0)
		{
			runtime.CharacterPresentationMutations[((GodotObject)screen).GetInstanceId()] = new CharacterPresentationMutation(new WeakReference<NCharacterSelectScreen>(screen), list, list2, list3, list4, list5, list6);
		}
	}

	private static Dictionary<string, Variant> CaptureCharacterPresentationMetadata(Node node)
	{
		return ((IEnumerable<StringName>)((GodotObject)node).GetMetaList()).Select((StringName key) => (Key: ((object)key).ToString(), Value: ((GodotObject)node).GetMeta(key, default(Variant)))).Where<(string, Variant)>(delegate((string Key, Variant Value) pair)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Invalid comparison between Unknown and I8
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Invalid comparison between Unknown and I8
			Type variantType = ((Variant)(ref pair.Value)).VariantType;
			return ((long)(variantType - 1) <= 1L || (long)variantType == 4) ? true : false;
		}).ToDictionary<(string, Variant), string, Variant>(((string Key, Variant Value) pair) => pair.Key, ((string Key, Variant Value) pair) => pair.Value, StringComparer.Ordinal);
	}

	private static Callable[] CaptureCharacterPresentationResizeConnections(Node node)
	{
		if (!(node is Control))
		{
			return Array.Empty<Callable>();
		}
		return ((IEnumerable<Dictionary>)((GodotObject)node).GetSignalConnectionList(SignalName.Resized)).Where((Dictionary connection) => connection.ContainsKey(Variant.op_Implicit("callable"))).Select(delegate(Dictionary connection)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			Variant val = connection[Variant.op_Implicit("callable")];
			return ((Variant)(ref val)).As<Callable>();
		}).ToArray();
	}

	private static IEnumerable<Node> EnumerateNodeTree(Node root)
	{
		return PresentationNodeOwnership.Walk(root, (Node node) => ((GodotObject)node).GetClass().ToString(), (Node node) => (IEnumerable<Node>)node.GetChildren(false));
	}

	private static void RestoreCharacterPresentations(ActiveProviderRuntime runtime)
	{
		CharacterPresentationMutation[] array = runtime.CharacterPresentationMutations.Values.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			RestoreCharacterPresentationMutation(array[i]);
		}
		runtime.CharacterPresentationMutations.Clear();
	}

	private static void RestoreCharacterPresentation(ActiveProviderRuntime runtime, NCharacterSelectScreen screen)
	{
		ulong instanceId = ((GodotObject)screen).GetInstanceId();
		if (runtime.CharacterPresentationMutations.Remove(instanceId, out CharacterPresentationMutation value) && value.Screen.TryGetTarget(out NCharacterSelectScreen target) && target == screen)
		{
			RestoreCharacterPresentationMutation(value);
		}
	}

	private static void RestoreCharacterPresentationMutation(CharacterPresentationMutation mutation)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		foreach (var (weakReference, val) in mutation.ResizeConnections)
		{
			if (weakReference.TryGetTarget(out var target) && GodotObject.IsInstanceValid((GodotObject)(object)target) && ((GodotObject)target).IsConnected(SignalName.Resized, val))
			{
				((GodotObject)target).Disconnect(SignalName.Resized, val);
			}
		}
		foreach (CharacterPresentationMetadataChange metadataChange in mutation.MetadataChanges)
		{
			if (!metadataChange.Node.TryGetTarget(out Node target2) || !GodotObject.IsInstanceValid((GodotObject)(object)target2))
			{
				continue;
			}
			PresentationMetadataChange<Variant> change = metadataChange.Change;
			bool flag = ((GodotObject)target2).HasMeta(StringName.op_Implicit(change.Key));
			Variant current2 = (Variant)(flag ? ((GodotObject)target2).GetMeta(StringName.op_Implicit(change.Key), default(Variant)) : default(Variant));
			if (PresentationMetadataChanges.CanRestore(change, flag, current2))
			{
				if (change.HadOriginal)
				{
					((GodotObject)target2).SetMeta(StringName.op_Implicit(change.Key), change.Original);
				}
				else
				{
					((GodotObject)target2).RemoveMeta(StringName.op_Implicit(change.Key));
				}
			}
		}
		foreach (WeakReference<Node> addedRoot in mutation.AddedRoots)
		{
			if (addedRoot.TryGetTarget(out var target3) && GodotObject.IsInstanceValid((GodotObject)(object)target3) && !PresentationNodeOwnership.IsRendererOwned(((GodotObject)target3).GetClass().ToString()))
			{
				Node parent = target3.GetParent();
				if (parent != null && GodotObject.IsInstanceValid((GodotObject)(object)parent))
				{
					GodotTreeExtensions.RemoveChildSafely(parent, target3);
				}
				GodotTreeExtensions.QueueFreeSafely(target3);
			}
		}
		foreach (CharacterPresentationVisibilityChange visibilityChange in mutation.VisibilityChanges)
		{
			if (visibilityChange.Node.TryGetTarget(out CanvasItem target4) && GodotObject.IsInstanceValid((GodotObject)(object)target4) && !PresentationNodeOwnership.IsRendererOwned(((GodotObject)target4).GetClass().ToString()) && target4.Visible == visibilityChange.AppliedVisibility)
			{
				target4.Visible = visibilityChange.OriginalVisibility;
			}
		}
		foreach (CharacterPresentationTextChange textChange in mutation.TextChanges)
		{
			if (textChange.Node.TryGetTarget(out Node target5) && GodotObject.IsInstanceValid((GodotObject)(object)target5) && !(GetCharacterPresentationText(target5) != textChange.AppliedText))
			{
				SetCharacterPresentationText(target5, textChange.OriginalText);
			}
		}
		foreach (CharacterPresentationClipChange clipChange in mutation.ClipChanges)
		{
			if (clipChange.Node.TryGetTarget(out Control target6) && GodotObject.IsInstanceValid((GodotObject)(object)target6) && target6.ClipContents == clipChange.AppliedClipContents)
			{
				target6.ClipContents = clipChange.OriginalClipContents;
			}
		}
	}

	private static string? GetCharacterPresentationText(Node node)
	{
		Label val = (Label)(object)((node is Label) ? node : null);
		if (val == null)
		{
			RichTextLabel val2 = (RichTextLabel)(object)((node is RichTextLabel) ? node : null);
			if (val2 != null)
			{
				return val2.Text;
			}
			return null;
		}
		return val.Text;
	}

	private static void SetCharacterPresentationText(Node node, string text)
	{
		Label val = (Label)(object)((node is Label) ? node : null);
		if (val == null)
		{
			RichTextLabel val2 = (RichTextLabel)(object)((node is RichTextLabel) ? node : null);
			if (val2 != null)
			{
				val2.Text = text;
			}
		}
		else
		{
			val.Text = text;
		}
	}

	public static void ReplaySelectedCreatureReady(string providerId, NCreature creature)
	{
		if (!ActiveProviderRuntimes.TryGetValue(providerId, out ActiveProviderRuntime value))
		{
			return;
		}
		NCombatRoom room = NCombatRoom.Instance;
		int num = 0;
		foreach (ProviderPatch item in value.Patches.Where((ProviderPatch patch) => patch.Kind == ProviderPatchKind.Postfix && IsLiveCreatureInitializationPatch(patch.Target, creature, room)))
		{
			if (TryBuildCreatureInitializationArguments(item.Callback, item.Target, creature, room, out object[] arguments))
			{
				try
				{
					item.Callback.Invoke(null, arguments);
					num++;
				}
				catch (Exception ex)
				{
					ModLog.Warn($"重放 {providerId} 的实战生物初始化 {item.Callback.DeclaringType?.FullName}.{item.Callback.Name} 失败：{ex.GetBaseException().Message}");
				}
			}
		}
		if (num > 0)
		{
			ModLog.Info($"已为 {creature.Entity.ModelId.Entry} 重放 {providerId} 的 {num} 个实战生物初始化步骤。");
		}
	}

	public static IReadOnlyList<Node> ReplaySelectedNodeReadyBehavior(string providerId, Node node, bool includePrefixes = false)
	{
		if (!ActiveProviderRuntimes.TryGetValue(providerId, out ActiveProviderRuntime value) || !GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return Array.Empty<Node>();
		}
		BeginSelectedNodeReadyTracking(providerId, node);
		int num = 0;
		try
		{
			num = InvokeSelectedNodeReadyCallbacks(providerId, value, node, includePrefixes, includePostfixes: true);
		}
		finally
		{
			EndSelectedNodeReadyTracking(providerId, node);
		}
		IReadOnlyList<Node> trackedNodeReadyAdditions = GetTrackedNodeReadyAdditions(value, node);
		if (num > 0)
		{
			ModLog.Info($"已为现有场景节点重放 {providerId} 的 {num} 个外观初始化步骤。");
		}
		return trackedNodeReadyAdditions;
	}

	public static IReadOnlyList<Node> ReplaySelectedNodeReadyPrefixes(string providerId, Node node)
	{
		if (!ActiveProviderRuntimes.TryGetValue(providerId, out ActiveProviderRuntime value) || !GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return Array.Empty<Node>();
		}
		ulong instanceId = ((GodotObject)node).GetInstanceId();
		RestoreNodeReadyMutation(value, instanceId);
		NodeReadyBaseline baseline = new NodeReadyBaseline(new WeakReference<Node>(node), CaptureNodeReadyState(node));
		int num = InvokeSelectedNodeReadyCallbacks(providerId, value, node, includePrefixes: true, includePostfixes: false);
		NodeReadyMutation nodeReadyMutation = BuildNodeReadyMutation(node, baseline);
		if (nodeReadyMutation != null)
		{
			value.NodeReadyMutations[instanceId] = nodeReadyMutation;
		}
		if (num > 0)
		{
			ModLog.Info($"已在原生 _Ready 前重放 {providerId} 的 {num} 个外观初始化步骤。");
		}
		return GetTrackedNodeReadyAdditions(value, node);
	}

	public static IReadOnlyList<Node> ReplaySelectedNodeReadyPostfixes(string providerId, Node node)
	{
		if (!ActiveProviderRuntimes.TryGetValue(providerId, out ActiveProviderRuntime value) || !GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return Array.Empty<Node>();
		}
		ulong instanceId = ((GodotObject)node).GetInstanceId();
		NodeReadyBaseline baseline = new NodeReadyBaseline(new WeakReference<Node>(node), CaptureNodeReadyState(node));
		int num = InvokeSelectedNodeReadyCallbacks(providerId, value, node, includePrefixes: false, includePostfixes: true);
		NodeReadyMutation nodeReadyMutation = BuildNodeReadyMutation(node, baseline);
		if (nodeReadyMutation != null)
		{
			value.NodeReadyMutations[instanceId] = (value.NodeReadyMutations.TryGetValue(instanceId, out NodeReadyMutation value2) ? MergeNodeReadyMutations(value2, nodeReadyMutation) : nodeReadyMutation);
		}
		if (num > 0)
		{
			ModLog.Info($"已在原生 _Ready 后重放 {providerId} 的 {num} 个外观初始化步骤。");
		}
		return GetTrackedNodeReadyAdditions(value, node);
	}

	public static void RestoreSelectedNodeReadyBehavior(string providerId, Node node)
	{
		if (ActiveProviderRuntimes.TryGetValue(providerId, out ActiveProviderRuntime value) && GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			RestoreNodeReadyMutation(value, ((GodotObject)node).GetInstanceId());
		}
	}

	public static void ReplaySelectedCompanionReveal(string providerId, NCreature creature)
	{
		if (!ActiveProviderRuntimes.TryGetValue(providerId, out ActiveProviderRuntime value))
		{
			return;
		}
		Creature entity = creature.Entity;
		if (entity.PetOwner == null || entity.IsDead || !creature.Visuals.HasSpineAnimation)
		{
			return;
		}
		MegaSprite spineBody = creature.Visuals.SpineBody;
		GodotObject obj = ((spineBody != null) ? ((MegaSpineBinding)spineBody).BoundObject : null);
		CanvasItem val = (CanvasItem)(object)((obj is CanvasItem) ? obj : null);
		bool hiddenBody = val != null && !val.Visible;
		foreach (ProviderPatch item in value.Patches.Where((ProviderPatch patch) => IsCompanionRevealPresentationPatch(patch, entity.PetOwner != null, entity.IsDead, hiddenBody)))
		{
			if (TryBuildCreatureInitializationArguments(item.Callback, item.Target, creature, NCombatRoom.Instance, out object[] arguments))
			{
				try
				{
					item.Callback.Invoke(null, arguments);
					ModLog.Info($"已为 {entity.ModelId.Entry} 重放 {providerId} 的随从显示初始化：{item.Callback.DeclaringType?.Name}。");
				}
				catch (Exception ex)
				{
					ModLog.Warn("重放 " + providerId + " 的随从显示初始化失败：" + ex.GetBaseException().Message);
				}
			}
		}
	}

	private static bool IsCompanionRevealPresentationPatch(ProviderPatch patch, bool isCompanion, bool isDead, bool bodyHidden)
	{
		if (isCompanion && !isDead && bodyHidden && patch.Kind == ProviderPatchKind.Prefix && patch.Target.DeclaringType == typeof(NCreature))
		{
			return patch.Target.Name == "StartReviveAnim";
		}
		return false;
	}

	private static int InvokeSelectedNodeReadyCallbacks(string providerId, ActiveProviderRuntime runtime, Node node, bool includePrefixes, bool includePostfixes)
	{
		int num = 0;
		foreach (ProviderPatch item in (from patch in runtime.Patches.Concat(runtime.NodeReadyPresentationPatches)
			where ((includePrefixes && patch.Kind == ProviderPatchKind.Prefix) || (includePostfixes && patch.Kind == ProviderPatchKind.Postfix)) && patch.Target.Name.Equals("_Ready", StringComparison.Ordinal) && (patch.Target.DeclaringType?.IsInstanceOfType(node) ?? false)
			select patch).DistinctBy((ProviderPatch patch) => (Target: patch.Target, Callback: patch.Callback, Kind: patch.Kind)))
		{
			if (TryBuildNodeReadyArguments(item.Callback, item.Target, node, out object[] arguments))
			{
				try
				{
					item.Callback.Invoke(null, arguments);
					num++;
				}
				catch (Exception ex)
				{
					ModLog.Warn($"重放 {providerId} 的场景外观初始化 {item.Callback.DeclaringType?.FullName}.{item.Callback.Name} 失败：" + ex.GetBaseException().Message);
				}
			}
		}
		return num;
	}

	private static IReadOnlyList<Node> GetTrackedNodeReadyAdditions(ActiveProviderRuntime runtime, Node node)
	{
		if (!runtime.NodeReadyMutations.TryGetValue(((GodotObject)node).GetInstanceId(), out NodeReadyMutation value))
		{
			return Array.Empty<Node>();
		}
		Node target;
		return (from reference in value.AddedRoots
			select (!reference.TryGetTarget(out target)) ? null : target into addedNode
			where addedNode != null && GodotObject.IsInstanceValid((GodotObject)(object)addedNode)
			select addedNode).Cast<Node>().ToArray();
	}

	private static NodeReadyMutation MergeNodeReadyMutations(NodeReadyMutation prefix, NodeReadyMutation postfix)
	{
		Node target;
		Node target2;
		WeakReference<Node>[] addedRoots = (from reference in postfix.AddedRoots.Concat(prefix.AddedRoots)
			where reference.TryGetTarget(out target) && GodotObject.IsInstanceValid((GodotObject)(object)target)
			select reference).DistinctBy((WeakReference<Node> reference) => (!reference.TryGetTarget(out target2)) ? 0 : ((GodotObject)target2).GetInstanceId()).ToArray();
		NodeReadyVisualChange[] changes = postfix.Changes.Concat(prefix.Changes).ToArray();
		return new NodeReadyMutation(addedRoots, changes);
	}

	public static IReadOnlyList<(string ProviderId, IReadOnlyList<Node> AddedRoots)> ReplaySelectedRoomReadyBehaviors(Node room, string? onlyProviderId = null)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)room))
		{
			return Array.Empty<(string, IReadOnlyList<Node>)>();
		}
		List<(string, IReadOnlyList<Node>)> list = new List<(string, IReadOnlyList<Node>)>();
		string[] array = (from pair in ActiveProviderRuntimes
			where pair.Value.NodeReadyPresentationPatches.Any((ProviderPatch patch) => patch.Target.DeclaringType?.IsInstanceOfType(room) ?? false) && (onlyProviderId == null || pair.Key.Equals(onlyProviderId, StringComparison.OrdinalIgnoreCase))
			select pair.Key).ToArray();
		foreach (string text in array)
		{
			IReadOnlyList<Node> item = ReplaySelectedNodeReadyBehavior(text, room);
			list.Add((text, item));
		}
		return list;
	}

	public static int ReplayPartTakeoverRoomPresentation(Node room)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)room) || PartTakeoverRoots.Count == 0)
		{
			return 0;
		}
		int num = 0;
		MethodBase[] array = Harmony.GetAllPatchedMethods().Where(IsNodeReadyPresentationTarget).ToArray();
		foreach (MethodBase methodBase in array)
		{
			Type? declaringType = methodBase.DeclaringType;
			if ((object)declaringType == null || !declaringType.IsInstanceOfType(room))
			{
				continue;
			}
			Patches patchInfo = Harmony.GetPatchInfo(methodBase);
			if (patchInfo == null)
			{
				continue;
			}
			foreach (Patch postfix in patchInfo.Postfixes)
			{
				if (IsFromPartTakeoverRoot(postfix.PatchMethod.Module.Assembly) && TryBuildNodeReadyArguments(postfix.PatchMethod, methodBase, room, out object[] arguments))
				{
					try
					{
						postfix.PatchMethod.Invoke(null, arguments);
						num++;
					}
					catch (Exception ex)
					{
						ModLog.Warn("重放部件接管包的商店呈现 " + postfix.PatchMethod.DeclaringType?.FullName + "." + postfix.PatchMethod.Name + " 失败：" + ex.GetBaseException().Message);
					}
				}
			}
		}
		return num;
	}

	public static int SuspendPartTakeoverMerchantFeedback()
	{
		if (_partTakeoverFeedbackSuspended)
		{
			return 0;
		}
		IReadOnlyList<ProviderPatch> readOnlyList = ResolvePartTakeoverMerchantFeedback();
		if (readOnlyList.Count == 0)
		{
			return 0;
		}
		UnpatchProviderCallbacks(readOnlyList);
		SuspendedPartTakeoverFeedback.Clear();
		SuspendedPartTakeoverFeedback.AddRange(readOnlyList);
		_partTakeoverFeedbackSuspended = true;
		ModLog.Info($"已停用部件接管包的 {readOnlyList.Count} 个商人语音反馈：当前选的不是它的商人部件，" + "由游戏原版商人发声。");
		return readOnlyList.Count;
	}

	public static int RestorePartTakeoverMerchantFeedback()
	{
		if (!_partTakeoverFeedbackSuspended)
		{
			return 0;
		}
		ProviderPatch[] array = SuspendedPartTakeoverFeedback.ToArray();
		SuspendedPartTakeoverFeedback.Clear();
		_partTakeoverFeedbackSuspended = false;
		if (array.Length == 0)
		{
			return 0;
		}
		PatchProviderCallbacks(array);
		ModLog.Info($"已恢复部件接管包的 {array.Length} 个商人语音反馈：选中的是它的商人部件，" + "按作者原样发声。");
		return array.Length;
	}

	private static IReadOnlyList<ProviderPatch> ResolvePartTakeoverMerchantFeedback()
	{
		if (PartTakeoverRoots.Count == 0)
		{
			return Array.Empty<ProviderPatch>();
		}
		List<ProviderPatch> list = new List<ProviderPatch>();
		MethodBase[] array = Harmony.GetAllPatchedMethods().ToArray();
		foreach (MethodBase methodBase in array)
		{
			Patches patchInfo = Harmony.GetPatchInfo(methodBase);
			if (patchInfo == null)
			{
				continue;
			}
			foreach (var item in patchInfo.Prefixes.Select((Patch patch) => (Patch: patch, Kind: ProviderPatchKind.Prefix)).Concat(patchInfo.Postfixes.Select((Patch patch) => (Patch: patch, Kind: ProviderPatchKind.Postfix))).Concat(patchInfo.Transpilers.Select((Patch patch) => (Patch: patch, Kind: ProviderPatchKind.Transpiler)))
				.Concat(patchInfo.Finalizers.Select((Patch patch) => (Patch: patch, Kind: ProviderPatchKind.Finalizer))))
			{
				if (IsFromPartTakeoverRoot(item.Patch.PatchMethod.Module.Assembly) && PartTakeoverFeedbackScopePolicy.IsMerchantSelectionFeedback(methodBase.DeclaringType?.Name, methodBase.Name, item.Patch.PatchMethod.DeclaringType?.FullName))
				{
					list.Add(new ProviderPatch(methodBase, item.Patch.PatchMethod, item.Kind, item.Patch.priority, item.Patch.before, item.Patch.after, item.Patch.debug));
				}
			}
		}
		return list.DistinctBy((ProviderPatch patch) => (Target: patch.Target, Callback: patch.Callback, Kind: patch.Kind)).ToArray();
	}

	private static bool IsFromPartTakeoverRoot(Assembly assembly)
	{
		string location = assembly.Location;
		if (string.IsNullOrWhiteSpace(location))
		{
			return false;
		}
		string assemblyPath = NormalizePath(location);
		return PartTakeoverRoots.Any((string root) => assemblyPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
	}

	public static void BeginSelectedNodeReadyTracking(string providerId, Node node)
	{
		if (ActiveProviderRuntimes.TryGetValue(providerId, out ActiveProviderRuntime value) && GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			ulong instanceId = ((GodotObject)node).GetInstanceId();
			RestoreNodeReadyMutation(value, instanceId);
			value.PendingNodeReadyBaselines[instanceId] = new NodeReadyBaseline(new WeakReference<Node>(node), CaptureNodeReadyState(node));
		}
	}

	public static void EndSelectedNodeReadyTracking(string providerId, Node node)
	{
		if (ActiveProviderRuntimes.TryGetValue(providerId, out ActiveProviderRuntime value) && GodotObject.IsInstanceValid((GodotObject)(object)node) && value.PendingNodeReadyBaselines.Remove(((GodotObject)node).GetInstanceId(), out NodeReadyBaseline value2))
		{
			NodeReadyMutation nodeReadyMutation = BuildNodeReadyMutation(node, value2);
			if (nodeReadyMutation != null)
			{
				value.NodeReadyMutations[((GodotObject)node).GetInstanceId()] = nodeReadyMutation;
			}
		}
	}

	public static void RestoreUnselectedNodeReadyBehaviors(Node node, IEnumerable<string> selectedProviderIds)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return;
		}
		HashSet<string> hashSet = selectedProviderIds.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		ulong instanceId = ((GodotObject)node).GetInstanceId();
		KeyValuePair<string, ActiveProviderRuntime>[] array = ActiveProviderRuntimes.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, ActiveProviderRuntime> keyValuePair = array[i];
			if (!hashSet.Contains(keyValuePair.Key))
			{
				RestoreNodeReadyMutation(keyValuePair.Value, instanceId);
			}
		}
	}

	public static void RestoreAllNodeReadyBehaviors(Node node)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			ulong instanceId = ((GodotObject)node).GetInstanceId();
			ActiveProviderRuntime[] array = ActiveProviderRuntimes.Values.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				RestoreNodeReadyMutation(array[i], instanceId);
			}
		}
	}

	private static NodeReadyMutation? BuildNodeReadyMutation(Node node, NodeReadyBaseline baseline)
	{
		HashSet<ulong> baselineIds = baseline.States.Keys.ToHashSet();
		WeakReference<Node>[] array = (from candidate in (from candidate in EnumerateNodeTree(node)
				where !baselineIds.Contains(((GodotObject)candidate).GetInstanceId())
				select candidate).Where(delegate(Node candidate)
			{
				Node parent = candidate.GetParent();
				return parent == null || baselineIds.Contains(((GodotObject)parent).GetInstanceId());
			})
			select new WeakReference<Node>(candidate)).ToArray();
		Dictionary<ulong, NodeReadyVisualState> dictionary = CaptureNodeReadyState(node);
		List<NodeReadyVisualChange> list = new List<NodeReadyVisualChange>();
		foreach (NodeReadyVisualState value2 in baseline.States.Values)
		{
			if (value2.Node.TryGetTarget(out Node target) && GodotObject.IsInstanceValid((GodotObject)(object)target) && dictionary.TryGetValue(((GodotObject)target).GetInstanceId(), out var value) && HasNodeReadyVisualChanged(value2, value))
			{
				list.Add(new NodeReadyVisualChange(value2.Node, value2, value));
			}
		}
		if (array.Length != 0 || list.Count != 0)
		{
			return new NodeReadyMutation(array, list);
		}
		return null;
	}

	private static bool HasNodeReadyVisualChanged(NodeReadyVisualState baseline, NodeReadyVisualState current)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		if (baseline.Visible == current.Visible)
		{
			Color? modulate = baseline.Modulate;
			Color? modulate2 = current.Modulate;
			if (modulate.HasValue == modulate2.HasValue && (!modulate.HasValue || !(modulate.GetValueOrDefault() != modulate2.GetValueOrDefault())))
			{
				modulate2 = baseline.SelfModulate;
				modulate = current.SelfModulate;
				if (modulate2.HasValue == modulate.HasValue && (!modulate2.HasValue || !(modulate2.GetValueOrDefault() != modulate.GetValueOrDefault())) && baseline.ZIndex == current.ZIndex && baseline.ZAsRelative == current.ZAsRelative)
				{
					Vector2? node2DPosition = baseline.Node2DPosition;
					Vector2? node2DPosition2 = current.Node2DPosition;
					if (node2DPosition.HasValue == node2DPosition2.HasValue && (!node2DPosition.HasValue || !(node2DPosition.GetValueOrDefault() != node2DPosition2.GetValueOrDefault())))
					{
						node2DPosition2 = baseline.Node2DScale;
						node2DPosition = current.Node2DScale;
						if (node2DPosition2.HasValue == node2DPosition.HasValue && (!node2DPosition2.HasValue || !(node2DPosition2.GetValueOrDefault() != node2DPosition.GetValueOrDefault())) && baseline.Node2DRotation == current.Node2DRotation)
						{
							node2DPosition = baseline.ControlPosition;
							node2DPosition2 = current.ControlPosition;
							if (node2DPosition.HasValue == node2DPosition2.HasValue && (!node2DPosition.HasValue || !(node2DPosition.GetValueOrDefault() != node2DPosition2.GetValueOrDefault())))
							{
								node2DPosition2 = baseline.ControlSize;
								node2DPosition = current.ControlSize;
								if (node2DPosition2.HasValue == node2DPosition.HasValue && (!node2DPosition2.HasValue || !(node2DPosition2.GetValueOrDefault() != node2DPosition.GetValueOrDefault())))
								{
									node2DPosition = baseline.ControlScale;
									node2DPosition2 = current.ControlScale;
									if (node2DPosition.HasValue == node2DPosition2.HasValue && (!node2DPosition.HasValue || !(node2DPosition.GetValueOrDefault() != node2DPosition2.GetValueOrDefault())) && baseline.ControlRotation == current.ControlRotation)
									{
										node2DPosition2 = baseline.ControlPivotOffset;
										node2DPosition = current.ControlPivotOffset;
										if (node2DPosition2.HasValue == node2DPosition.HasValue && (!node2DPosition2.HasValue || !(node2DPosition2.GetValueOrDefault() != node2DPosition.GetValueOrDefault())))
										{
											return baseline.ControlMouseFilter != current.ControlMouseFilter;
										}
									}
								}
							}
						}
					}
				}
			}
		}
		return true;
	}

	private static Dictionary<ulong, NodeReadyVisualState> CaptureNodeReadyState(Node root)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<ulong, NodeReadyVisualState> dictionary = new Dictionary<ulong, NodeReadyVisualState>();
		foreach (Node item in EnumerateNodeTree(root))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item))
			{
				CanvasItem val = (CanvasItem)(object)((item is CanvasItem) ? item : null);
				Node2D val2 = (Node2D)(object)((item is Node2D) ? item : null);
				Control val3 = (Control)(object)((item is Control) ? item : null);
				dictionary[((GodotObject)item).GetInstanceId()] = new NodeReadyVisualState(new WeakReference<Node>(item), (val != null) ? new bool?(val.Visible) : ((bool?)null), (val != null) ? new Color?(val.Modulate) : ((Color?)null), (val != null) ? new Color?(val.SelfModulate) : ((Color?)null), (val != null) ? new int?(val.ZIndex) : ((int?)null), (val != null) ? new bool?(val.ZAsRelative) : ((bool?)null), (val2 != null) ? new Vector2?(val2.Position) : ((Vector2?)null), (val2 != null) ? new Vector2?(val2.Scale) : ((Vector2?)null), (val2 != null) ? new float?(val2.Rotation) : ((float?)null), (val3 != null) ? new Vector2?(val3.Position) : ((Vector2?)null), (val3 != null) ? new Vector2?(val3.Size) : ((Vector2?)null), (val3 != null) ? new Vector2?(val3.Scale) : ((Vector2?)null), (val3 != null) ? new float?(val3.Rotation) : ((float?)null), (val3 != null) ? new Vector2?(val3.PivotOffset) : ((Vector2?)null), (val3 != null) ? new MouseFilterEnum?(val3.MouseFilter) : ((MouseFilterEnum?)null));
			}
		}
		return dictionary;
	}

	private static void RestoreNodeReadyBehaviors(ActiveProviderRuntime runtime)
	{
		NodeReadyMutation[] array = runtime.NodeReadyMutations.Values.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			RestoreNodeReadyMutation(array[i]);
		}
		runtime.NodeReadyMutations.Clear();
		runtime.PendingNodeReadyBaselines.Clear();
	}

	private static void RestoreNodeReadyMutation(ActiveProviderRuntime runtime, ulong nodeId)
	{
		if (runtime.NodeReadyMutations.Remove(nodeId, out NodeReadyMutation value))
		{
			RestoreNodeReadyMutation(value);
		}
		runtime.PendingNodeReadyBaselines.Remove(nodeId);
	}

	private static void RestoreNodeReadyMutation(NodeReadyMutation mutation)
	{
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0512: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0520: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		foreach (WeakReference<Node> addedRoot in mutation.AddedRoots)
		{
			if (addedRoot.TryGetTarget(out var target) && GodotObject.IsInstanceValid((GodotObject)(object)target) && !PresentationNodeOwnership.IsRendererOwned(((GodotObject)target).GetClass().ToString()))
			{
				Node parent = target.GetParent();
				if (parent != null)
				{
					GodotTreeExtensions.RemoveChildSafely(parent, target);
				}
				GodotTreeExtensions.QueueFreeSafely(target);
			}
		}
		foreach (NodeReadyVisualChange change in mutation.Changes)
		{
			if (!change.Node.TryGetTarget(out Node target2) || !GodotObject.IsInstanceValid((GodotObject)(object)target2) || PresentationNodeOwnership.IsRendererOwned(((GodotObject)target2).GetClass().ToString()))
			{
				continue;
			}
			NodeReadyVisualState original = change.Original;
			NodeReadyVisualState applied = change.Applied;
			CanvasItem val = (CanvasItem)(object)((target2 is CanvasItem) ? target2 : null);
			if (val != null)
			{
				bool? visible = original.Visible;
				if (visible.HasValue)
				{
					bool valueOrDefault = visible == true;
					visible = applied.Visible;
					if (visible.HasValue)
					{
						bool valueOrDefault2 = visible == true;
						if (val.Visible == valueOrDefault2)
						{
							val.Visible = valueOrDefault;
						}
					}
				}
				Color? modulate = original.Modulate;
				if (modulate.HasValue)
				{
					Color valueOrDefault3 = modulate.GetValueOrDefault();
					modulate = applied.Modulate;
					if (modulate.HasValue)
					{
						Color valueOrDefault4 = modulate.GetValueOrDefault();
						if (val.Modulate == valueOrDefault4)
						{
							val.Modulate = valueOrDefault3;
						}
					}
				}
				modulate = original.SelfModulate;
				if (modulate.HasValue)
				{
					Color valueOrDefault5 = modulate.GetValueOrDefault();
					modulate = applied.SelfModulate;
					if (modulate.HasValue)
					{
						Color valueOrDefault6 = modulate.GetValueOrDefault();
						if (val.SelfModulate == valueOrDefault6)
						{
							val.SelfModulate = valueOrDefault5;
						}
					}
				}
				int? zIndex = original.ZIndex;
				if (zIndex.HasValue)
				{
					int valueOrDefault7 = zIndex.GetValueOrDefault();
					zIndex = applied.ZIndex;
					if (zIndex.HasValue)
					{
						int valueOrDefault8 = zIndex.GetValueOrDefault();
						if (val.ZIndex == valueOrDefault8)
						{
							val.ZIndex = valueOrDefault7;
						}
					}
				}
				visible = original.ZAsRelative;
				if (visible.HasValue)
				{
					bool valueOrDefault9 = visible == true;
					visible = applied.ZAsRelative;
					if (visible.HasValue)
					{
						bool valueOrDefault10 = visible == true;
						if (val.ZAsRelative == valueOrDefault10)
						{
							val.ZAsRelative = valueOrDefault9;
						}
					}
				}
			}
			Node2D val2 = (Node2D)(object)((target2 is Node2D) ? target2 : null);
			float? node2DRotation;
			Vector2? node2DPosition;
			if (val2 != null)
			{
				node2DPosition = original.Node2DPosition;
				if (node2DPosition.HasValue)
				{
					Vector2 valueOrDefault11 = node2DPosition.GetValueOrDefault();
					node2DPosition = applied.Node2DPosition;
					if (node2DPosition.HasValue)
					{
						Vector2 valueOrDefault12 = node2DPosition.GetValueOrDefault();
						if (val2.Position == valueOrDefault12)
						{
							val2.Position = valueOrDefault11;
						}
					}
				}
				node2DPosition = original.Node2DScale;
				if (node2DPosition.HasValue)
				{
					Vector2 valueOrDefault13 = node2DPosition.GetValueOrDefault();
					node2DPosition = applied.Node2DScale;
					if (node2DPosition.HasValue)
					{
						Vector2 valueOrDefault14 = node2DPosition.GetValueOrDefault();
						if (val2.Scale == valueOrDefault14)
						{
							val2.Scale = valueOrDefault13;
						}
					}
				}
				node2DRotation = original.Node2DRotation;
				if (node2DRotation.HasValue)
				{
					float valueOrDefault15 = node2DRotation.GetValueOrDefault();
					node2DRotation = applied.Node2DRotation;
					if (node2DRotation.HasValue)
					{
						float valueOrDefault16 = node2DRotation.GetValueOrDefault();
						if (Mathf.IsEqualApprox(val2.Rotation, valueOrDefault16))
						{
							val2.Rotation = valueOrDefault15;
						}
					}
				}
			}
			Control val3 = (Control)(object)((target2 is Control) ? target2 : null);
			if (val3 == null)
			{
				continue;
			}
			node2DPosition = original.ControlPosition;
			if (node2DPosition.HasValue)
			{
				Vector2 valueOrDefault17 = node2DPosition.GetValueOrDefault();
				node2DPosition = applied.ControlPosition;
				if (node2DPosition.HasValue)
				{
					Vector2 valueOrDefault18 = node2DPosition.GetValueOrDefault();
					if (val3.Position == valueOrDefault18)
					{
						val3.Position = valueOrDefault17;
					}
				}
			}
			node2DPosition = original.ControlSize;
			if (node2DPosition.HasValue)
			{
				Vector2 valueOrDefault19 = node2DPosition.GetValueOrDefault();
				node2DPosition = applied.ControlSize;
				if (node2DPosition.HasValue)
				{
					Vector2 valueOrDefault20 = node2DPosition.GetValueOrDefault();
					if (val3.Size == valueOrDefault20)
					{
						val3.Size = valueOrDefault19;
					}
				}
			}
			node2DPosition = original.ControlScale;
			if (node2DPosition.HasValue)
			{
				Vector2 valueOrDefault21 = node2DPosition.GetValueOrDefault();
				node2DPosition = applied.ControlScale;
				if (node2DPosition.HasValue)
				{
					Vector2 valueOrDefault22 = node2DPosition.GetValueOrDefault();
					if (val3.Scale == valueOrDefault22)
					{
						val3.Scale = valueOrDefault21;
					}
				}
			}
			node2DRotation = original.ControlRotation;
			if (node2DRotation.HasValue)
			{
				float valueOrDefault23 = node2DRotation.GetValueOrDefault();
				node2DRotation = applied.ControlRotation;
				if (node2DRotation.HasValue)
				{
					float valueOrDefault24 = node2DRotation.GetValueOrDefault();
					if (Mathf.IsEqualApprox(val3.Rotation, valueOrDefault24))
					{
						val3.Rotation = valueOrDefault23;
					}
				}
			}
			node2DPosition = original.ControlPivotOffset;
			if (node2DPosition.HasValue)
			{
				Vector2 valueOrDefault25 = node2DPosition.GetValueOrDefault();
				node2DPosition = applied.ControlPivotOffset;
				if (node2DPosition.HasValue)
				{
					Vector2 valueOrDefault26 = node2DPosition.GetValueOrDefault();
					if (val3.PivotOffset == valueOrDefault26)
					{
						val3.PivotOffset = valueOrDefault25;
					}
				}
			}
			MouseFilterEnum? controlMouseFilter = original.ControlMouseFilter;
			if (!controlMouseFilter.HasValue)
			{
				continue;
			}
			MouseFilterEnum valueOrDefault27 = controlMouseFilter.GetValueOrDefault();
			controlMouseFilter = applied.ControlMouseFilter;
			if (controlMouseFilter.HasValue)
			{
				MouseFilterEnum valueOrDefault28 = controlMouseFilter.GetValueOrDefault();
				if (val3.MouseFilter == valueOrDefault28)
				{
					val3.MouseFilter = valueOrDefault27;
				}
			}
		}
	}

	private static bool IsLiveCreatureInitializationPatch(MethodBase target, NCreature creature, NCombatRoom? room)
	{
		if (target.Name.Equals("_Ready", StringComparison.Ordinal))
		{
			Type? declaringType = target.DeclaringType;
			if ((object)declaringType != null && declaringType.IsInstanceOfType(creature))
			{
				return true;
			}
		}
		if (room != null && target.Name.Equals("AddCreature", StringComparison.Ordinal))
		{
			return target.DeclaringType?.IsInstanceOfType(room) ?? false;
		}
		return false;
	}

	private static bool IsCharacterPresentationTarget(MethodBase target)
	{
		if (target.DeclaringType != null)
		{
			if (!typeof(NCharacterSelectScreen).IsAssignableFrom(target.DeclaringType) || !target.Name.Equals("SelectCharacter", StringComparison.Ordinal))
			{
				if (typeof(NCharacterSelectButton).IsAssignableFrom(target.DeclaringType))
				{
					return target.Name.Equals("Init", StringComparison.Ordinal);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private static bool TryBuildCharacterPresentationArguments(MethodInfo callback, MethodBase target, object instance, NCharacterSelectButton button, CharacterModel character, out object?[] arguments)
	{
		ParameterInfo[] parameters = callback.GetParameters();
		arguments = new object[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			Type type = (parameterInfo.ParameterType.IsByRef ? parameterInfo.ParameterType.GetElementType() : parameterInfo.ParameterType);
			switch (parameterInfo.Name)
			{
			case "__instance":
				if (type.IsInstanceOfType(instance))
				{
					arguments[i] = instance;
					continue;
				}
				break;
			case "__originalMethod":
				if (type == typeof(MethodBase))
				{
					arguments[i] = target;
					continue;
				}
				break;
			case "__runOriginal":
				if (type == typeof(bool))
				{
					arguments[i] = true;
					continue;
				}
				break;
			case "__state":
				arguments[i] = (type.IsValueType ? Activator.CreateInstance(type) : null);
				continue;
			case "charSelectButton":
			case "button":
				if (type.IsInstanceOfType(button))
				{
					arguments[i] = button;
					continue;
				}
				break;
			case "characterModel":
			case "character":
			case "model":
				if (type.IsInstanceOfType(character))
				{
					arguments[i] = character;
					continue;
				}
				break;
			}
			if (parameterInfo.HasDefaultValue)
			{
				arguments[i] = parameterInfo.DefaultValue;
				continue;
			}
			return false;
		}
		return true;
	}

	private static bool TryBuildCreatureInitializationArguments(MethodInfo callback, MethodBase target, NCreature creature, NCombatRoom? room, out object?[] arguments)
	{
		ParameterInfo[] parameters = callback.GetParameters();
		arguments = new object[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			Type type = (parameterInfo.ParameterType.IsByRef ? parameterInfo.ParameterType.GetElementType() : parameterInfo.ParameterType);
			switch (parameterInfo.Name)
			{
			case "__instance":
				if (type.IsInstanceOfType(creature))
				{
					arguments[i] = creature;
					continue;
				}
				if (room != null && type.IsInstanceOfType(room))
				{
					arguments[i] = room;
					continue;
				}
				break;
			case "creature":
				if (type.IsInstanceOfType(creature.Entity))
				{
					arguments[i] = creature.Entity;
					continue;
				}
				break;
			case "__originalMethod":
				if (type == typeof(MethodBase))
				{
					arguments[i] = target;
					continue;
				}
				break;
			case "__runOriginal":
				if (type == typeof(bool))
				{
					arguments[i] = true;
					continue;
				}
				break;
			case "__state":
				arguments[i] = (type.IsValueType ? Activator.CreateInstance(type) : null);
				continue;
			}
			if (parameterInfo.HasDefaultValue)
			{
				arguments[i] = parameterInfo.DefaultValue;
				continue;
			}
			return false;
		}
		return true;
	}

	private static bool TryBuildNodeReadyArguments(MethodInfo callback, MethodBase target, Node node, out object?[] arguments)
	{
		ParameterInfo[] parameters = callback.GetParameters();
		arguments = new object[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			Type type = (parameterInfo.ParameterType.IsByRef ? parameterInfo.ParameterType.GetElementType() : parameterInfo.ParameterType);
			switch (parameterInfo.Name)
			{
			case "__instance":
				if (type.IsInstanceOfType(node))
				{
					arguments[i] = node;
					continue;
				}
				break;
			case "__originalMethod":
				if (type == typeof(MethodBase))
				{
					arguments[i] = target;
					continue;
				}
				break;
			case "__runOriginal":
				if (type == typeof(bool))
				{
					arguments[i] = true;
					continue;
				}
				break;
			case "__state":
				arguments[i] = (type.IsValueType ? Activator.CreateInstance(type) : null);
				continue;
			}
			if (parameterInfo.HasDefaultValue)
			{
				arguments[i] = parameterInfo.DefaultValue;
				continue;
			}
			return false;
		}
		return true;
	}

	private static bool IsRunEnvironmentPatch(ProviderPatch patch)
	{
		return RuntimeProviderScopePolicy.IsRunEnvironmentPatchTarget(patch.Target.DeclaringType?.Name ?? string.Empty, patch.Target.Name);
	}

	private static void EnsureRunEnvironmentCallbackGuards(string providerId, Assembly assembly)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		HarmonyMethod val = new HarmonyMethod(AccessTools.Method(typeof(ManagedSkinModLoader), "RunEnvironmentCallbackPrefix", (Type[])null, (Type[])null));
		int num = 0;
		foreach (MethodInfo item in from method in (from type in GetLoadableTypes(assembly)
				where RuntimeProviderScopePolicy.IsRunEnvironmentControllerType(type.Name)
				select type).SelectMany((Type type) => type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
			where method.ReturnType == typeof(void) && !method.IsAbstract && !method.ContainsGenericParameters && method.GetMethodBody() != null && RuntimeProviderScopePolicy.IsRunEnvironmentCallback(method.DeclaringType?.Name ?? string.Empty, method.Name)
			select method)
		{
			if (RunEnvironmentCallbackMethods.Add(item))
			{
				RunEnvironmentCallbackHarmony.Patch((MethodBase)item, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				num++;
			}
		}
		if (num > 0)
		{
			ModLog.Info($"已为 {providerId} 的 {num} 个背景/音乐直接回调安装地区授权保护。 ");
		}
	}

	private static bool RunEnvironmentCallbackPrefix(MethodBase __originalMethod)
	{
		Assembly assembly = __originalMethod.DeclaringType?.Assembly;
		if (assembly != null && ProviderIdsByAssembly.TryGetValue(assembly, out HashSet<string> value))
		{
			return value.Any((string providerId) => RunEnvironmentProviderIds.Contains(providerId) && ActiveProviderRuntimes.ContainsKey(providerId));
		}
		return false;
	}

	private static IReadOnlyList<object> DiscoverRunEnvironmentControllers(Assembly assembly)
	{
		HashSet<object> hashSet = new HashSet<object>(ReferenceEqualityComparer.Instance);
		foreach (FieldInfo item in from field in GetLoadableTypes(assembly).SelectMany((Type type) => type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
			where RuntimeProviderScopePolicy.IsRunEnvironmentControllerType(field.FieldType.Name)
			select field)
		{
			try
			{
				object value = item.GetValue(null);
				if (value != null)
				{
					hashSet.Add(value);
				}
			}
			catch
			{
			}
		}
		return hashSet.ToArray();
	}

	private static void StopRunEnvironmentControllers(IEnumerable<object> controllers)
	{
		foreach (object controller in controllers)
		{
			Type type = controller.GetType();
			while (type != null)
			{
				FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo in fields)
				{
					try
					{
						object? value = fieldInfo.GetValue(controller);
						AudioStreamPlayer val = (AudioStreamPlayer)((value is AudioStreamPlayer) ? value : null);
						if (val != null)
						{
							if (GodotObject.IsInstanceValid((GodotObject)(object)val))
							{
								val.Stop();
								((Node)val).QueueFree();
							}
							if (!fieldInfo.IsInitOnly)
							{
								fieldInfo.SetValue(controller, null);
							}
						}
					}
					catch (Exception ex)
					{
						ModLog.Warn($"停止第三方地区音乐节点失败：{type.FullName}.{fieldInfo.Name}：" + ex.GetBaseException().Message);
					}
				}
				type = type.BaseType;
			}
		}
	}

	private static void ResetRunEnvironmentControllers(IEnumerable<object> controllers)
	{
		foreach (object controller in controllers)
		{
			try
			{
				MethodInfo method = controller.GetType().GetMethod("OnNativeStopRequested", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
				if (method?.ReturnType == typeof(void))
				{
					method.Invoke(controller, null);
				}
			}
			catch (Exception ex)
			{
				ModLog.Warn("重置第三方地区音乐状态失败：" + controller.GetType().FullName + "：" + ex.GetBaseException().Message);
			}
		}
	}

	private static void RestartRunEnvironmentControllersForCombat(IEnumerable<object> controllers, CombatState combatState)
	{
		string[] source = new string[3] { "OnCombatBegan", "OnCombatStarted", "OnCombatSetUp" };
		foreach (object controller in controllers)
		{
			try
			{
				source.SelectMany((string name) => from method in controller.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					where method.Name.Equals(name, StringComparison.Ordinal)
					select method).FirstOrDefault(delegate(MethodInfo method)
				{
					ParameterInfo[] parameters = method.GetParameters();
					return method.ReturnType == typeof(void) && parameters.Length == 1 && parameters[0].ParameterType.IsInstanceOfType(combatState);
				})?.Invoke(controller, new object[1] { combatState });
			}
			catch (Exception ex)
			{
				ModLog.Warn("重新进入第三方战斗音乐状态失败：" + controller.GetType().FullName + "：" + ex.GetBaseException().Message);
			}
		}
	}

	private static void UnpatchProviderCallbacks(IEnumerable<ProviderPatch> patches)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		Harmony val = new Harmony("Gurio.SkinChanger.provider_runtime");
		foreach (ProviderPatch patch in patches)
		{
			try
			{
				val.Unpatch(patch.Target, patch.Callback);
			}
			catch (Exception ex)
			{
				ModLog.Warn($"卸载提供者行为补丁 {patch.Callback.DeclaringType?.FullName}.{patch.Callback.Name} 失败：" + ex.GetBaseException().Message);
			}
		}
	}

	private static void PatchProviderCallbacks(IEnumerable<ProviderPatch> patches)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		Harmony val = new Harmony("Gurio.SkinChanger.provider_runtime");
		foreach (ProviderPatch patch in patches)
		{
			try
			{
				HarmonyMethod val2 = new HarmonyMethod(patch.Callback, patch.Priority, patch.Before, patch.After, (bool?)patch.Debug);
				switch (patch.Kind)
				{
				case ProviderPatchKind.Prefix:
					val.Patch(patch.Target, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					break;
				case ProviderPatchKind.Postfix:
					val.Patch(patch.Target, (HarmonyMethod)null, val2, (HarmonyMethod)null, (HarmonyMethod)null);
					break;
				case ProviderPatchKind.Transpiler:
					val.Patch(patch.Target, (HarmonyMethod)null, (HarmonyMethod)null, val2, (HarmonyMethod)null);
					break;
				case ProviderPatchKind.Finalizer:
					val.Patch(patch.Target, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, val2);
					break;
				default:
					throw new ArgumentOutOfRangeException("Kind", patch.Kind, null);
				}
			}
			catch (Exception ex)
			{
				ModLog.Warn($"恢复提供者行为补丁 {patch.Callback.DeclaringType?.FullName}.{patch.Callback.Name} 失败：" + ex.GetBaseException().Message);
			}
		}
	}

	private static string NormalizeHarmonyId(string providerId)
	{
		return new string(providerId.Select(delegate(char character)
		{
			bool flag = char.IsLetterOrDigit(character);
			if (!flag)
			{
				bool flag2 = ((character == '-' || character == '.' || character == '_') ? true : false);
				flag = flag2;
			}
			return (!flag) ? '_' : character;
		}).ToArray());
	}

	public static void ApplySelectedVisualPostfix(string providerId, object model, ref NCreatureVisuals visuals)
	{
		if (!ProviderAssemblies.TryGetValue(providerId, out ProviderAssembly value) || !GodotObject.IsInstanceValid((GodotObject)(object)visuals))
		{
			return;
		}
		try
		{
			Assembly orLoadProviderAssembly = GetOrLoadProviderAssembly(value);
			if (orLoadProviderAssembly == null)
			{
				return;
			}
			if (!VisualPostfixesByProvider.TryGetValue(providerId, out IReadOnlyList<ManagedVisualPostfix> value2))
			{
				value2 = DiscoverVisualPostfixes(orLoadProviderAssembly);
				VisualPostfixesByProvider[providerId] = value2;
			}
			foreach (ManagedVisualPostfix item2 in value2.Where((ManagedVisualPostfix candidate) => candidate.TargetType.IsInstanceOfType(model)))
			{
				InvokeVisualPostfix(value, item2, model, ref visuals);
			}
		}
		catch (Exception ex)
		{
			string item = providerId + ":discovery";
			if (FailedVisualPostfixes.Add(item))
			{
				ModLog.Warn("读取 " + value.Name + " 的按需视觉后处理失败，已继续使用资源皮肤：" + ex.GetBaseException().Message);
			}
		}
	}

	private static Assembly? GetOrLoadProviderAssembly(ProviderAssembly provider)
	{
		if (!provider.CanActivateBehavior)
		{
			return null;
		}
		if (LoadedProviderAssemblies.TryGetValue(provider.AssemblyPath, out Assembly value))
		{
			return value;
		}
		string text = ComputeProviderAssemblyFingerprint(provider.AssemblyPath);
		string assemblyIdentity = TryReadProviderAssemblyIdentity(provider.AssemblyPath);
		if (LoadedProviderAssembliesByFingerprint.TryGetValue(text, out value))
		{
			CacheLoadedProviderAssembly(provider.AssemblyPath, text, assemblyIdentity, value);
			return value;
		}
		if (assemblyIdentity != null && LoadedProviderAssembliesByIdentity.TryGetValue(assemblyIdentity, out LoadedProviderAssemblyIdentity value2))
		{
			if (!value2.Fingerprint.Equals(text, StringComparison.OrdinalIgnoreCase))
			{
				ReportProviderAssemblyIdentityConflict(provider, assemblyIdentity, text, value2.Fingerprint);
				return null;
			}
			CacheLoadedProviderAssembly(provider.AssemblyPath, text, assemblyIdentity, value2.Assembly);
			return value2.Assembly;
		}
		Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(delegate(Assembly candidate)
		{
			try
			{
				return !candidate.IsDynamic && !string.IsNullOrEmpty(candidate.Location) && Path.GetFullPath(candidate.Location).Equals(provider.AssemblyPath, StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		});
		if (assembly != null)
		{
			CacheLoadedProviderAssembly(provider.AssemblyPath, text, assemblyIdentity, assembly);
			return assembly;
		}
		if (assemblyIdentity != null)
		{
			assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly candidate) => !candidate.IsDynamic && !string.IsNullOrEmpty(candidate.Location) && string.Equals(candidate.FullName, assemblyIdentity, StringComparison.OrdinalIgnoreCase));
			if (assembly != null)
			{
				string text2 = TryComputeLoadedAssemblyFingerprint(assembly);
				if (text2 == null || !text2.Equals(text, StringComparison.OrdinalIgnoreCase))
				{
					ReportProviderAssemblyIdentityConflict(provider, assemblyIdentity, text, text2);
					return null;
				}
				CacheLoadedProviderAssembly(provider.AssemblyPath, text, assemblyIdentity, assembly);
				return assembly;
			}
		}
		AssemblyLoadContext loadContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
		ProviderCompatibilityResult providerCompatibilityResult = ProviderAssemblyCompatibility.PrepareForCurrentGame(provider.AssemblyPath);
		ProviderCompatibilityReport report = providerCompatibilityResult.Report;
		if (report.UnresolvedReferences.Count > 0)
		{
			ModLog.Warn($"{provider.Name} 在游戏 {report.Target} 仍有 {report.UnresolvedReferences.Count} 处静态接口引用未解析：" + string.Join("；", report.UnresolvedReferences.Take(8)) + "。仅报告差异，不猜测替代调用；未执行路径也可能包含这些引用，不能据此判定整款皮肤失效。");
		}
		MemoryStream assembly2 = providerCompatibilityResult.Assembly;
		if (assembly2 != null)
		{
			using (assembly2)
			{
				using MemoryStream memoryStream = RewriteProviderSettings(assembly2, provider.Name, provider.AssemblyPath);
				MemoryStream memoryStream2 = memoryStream ?? assembly2;
				assembly = loadContext?.LoadFromStream(memoryStream2) ?? Assembly.Load(memoryStream2.ToArray());
			}
			CacheLoadedProviderAssembly(provider.AssemblyPath, text, assemblyIdentity, assembly);
			ModLog.Info($"已为 {provider.Name} 转换 {report.Changes.Count} 处已知跨版本接口差异，目标 {report.Target}：" + string.Join("；", report.Changes.Distinct().Take(8)) + "。仅改内存副本；按接口签名匹配，不代表未知接口或运行逻辑已兼容。");
			return assembly;
		}
		if (!string.IsNullOrWhiteSpace(report.Failure))
		{
			ModLog.Warn("检查 " + provider.Name + " 的跨版本运行时接口失败：" + report.Failure);
		}
		using (FileStream source = File.OpenRead(provider.AssemblyPath))
		{
			using MemoryStream memoryStream4 = RewriteProviderSettings(source, provider.Name, provider.AssemblyPath);
			assembly = ((memoryStream4 == null) ? (loadContext?.LoadFromAssemblyPath(provider.AssemblyPath) ?? Assembly.LoadFrom(provider.AssemblyPath)) : (loadContext?.LoadFromStream(memoryStream4) ?? Assembly.Load(memoryStream4.ToArray())));
		}
		CacheLoadedProviderAssembly(provider.AssemblyPath, text, assemblyIdentity, assembly);
		return assembly;
	}

	private static MemoryStream? RewriteProviderSettings(Stream source, string name, string assemblyPath)
	{
		StatefulCardArtContract statefulCardArtContract = StatefulCardArtContract.Read(assemblyPath);
		if ((object)statefulCardArtContract != null)
		{
			int changed;
			MemoryStream? result = StatefulCardArtAssemblyCompatibility.Rewrite(source, statefulCardArtContract, out changed);
			ModLog.Info($"已隔离 {name} 的 {changed} 个全局卡牌工厂，动态显示将按单卡选择启用；原文件未修改。");
			return result;
		}
		try
		{
			int changed2;
			MemoryStream? result2 = MerchantSettingsAssemblyCompatibility.Rewrite(source, out changed2);
			if (changed2 > 0)
			{
				ModLog.Info($"已为 {name} 隔离 {changed2} 个原设置全场景入口；原订阅文件未修改。");
			}
			return result2;
		}
		catch (Exception ex)
		{
			ModLog.Warn("检查 " + name + " 的原设置接口失败，未放开设置入口：" + ex.GetBaseException().Message);
			return null;
		}
	}

	private static string ComputeProviderAssemblyFingerprint(string assemblyPath)
	{
		using FileStream source = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		return Convert.ToHexString(SHA256.HashData(source));
	}

	private static string? TryComputeLoadedAssemblyFingerprint(Assembly assembly)
	{
		try
		{
			return string.IsNullOrWhiteSpace(assembly.Location) ? null : ComputeProviderAssemblyFingerprint(assembly.Location);
		}
		catch
		{
			return null;
		}
	}

	private static string? TryReadProviderAssemblyIdentity(string assemblyPath)
	{
		try
		{
			return AssemblyName.GetAssemblyName(assemblyPath).FullName;
		}
		catch
		{
			return null;
		}
	}

	private static void CacheLoadedProviderAssembly(string assemblyPath, string fingerprint, string? assemblyIdentity, Assembly assembly)
	{
		LoadedProviderAssemblies[assemblyPath] = assembly;
		LoadedProviderAssembliesByFingerprint[fingerprint] = assembly;
		if (assemblyIdentity != null && !LoadedProviderAssembliesByIdentity.ContainsKey(assemblyIdentity))
		{
			LoadedProviderAssembliesByIdentity[assemblyIdentity] = new LoadedProviderAssemblyIdentity(fingerprint, assembly);
		}
	}

	private static void ReportProviderAssemblyIdentityConflict(ProviderAssembly provider, string assemblyIdentity, string requestedFingerprint, string? loadedFingerprint)
	{
		if ((loadedFingerprint == null || !loadedFingerprint.Equals(requestedFingerprint, StringComparison.OrdinalIgnoreCase)) && ReportedProviderAssemblyIdentityConflicts.Add(assemblyIdentity + "|" + requestedFingerprint))
		{
			ModLog.Warn(provider.Name + " 的差分包 DLL 与已加载程序集同名但内容不同；.NET 无法在游戏默认加载域中安全并存这两个行为程序集。为避免串用错误代码，当前差分包只接管可独立隔离的 PCK、图片、场景与卡面资源，不执行冲突 DLL。");
		}
	}

	private static IReadOnlyList<ManagedVisualPostfix> DiscoverVisualPostfixes(Assembly assembly)
	{
		return (from postfix in (from type in GetLoadableTypes(assembly)
				select (Type: type, Target: GetVisualPatchTarget(type)) into pair
				where pair.Target != null
				select pair).SelectMany<(Type, Type), ManagedVisualPostfix>(((Type Type, Type Target) pair) => from method in pair.Type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(IsPostfixMethod)
				select new ManagedVisualPostfix(pair.Target, method))
			orderby postfix.Method.MetadataToken
			select postfix).ToArray();
	}

	private static Type? GetVisualPatchTarget(Type patchType)
	{
		Type type = null;
		string a = null;
		foreach (CustomAttributeData item in patchType.CustomAttributes.Where((CustomAttributeData attribute) => attribute.AttributeType.FullName == "HarmonyLib.HarmonyPatch"))
		{
			foreach (CustomAttributeTypedArgument constructorArgument in item.ConstructorArguments)
			{
				if (constructorArgument.ArgumentType == typeof(Type))
				{
					type = constructorArgument.Value as Type;
				}
				else if (constructorArgument.ArgumentType == typeof(string))
				{
					a = constructorArgument.Value as string;
				}
			}
			foreach (CustomAttributeNamedArgument namedArgument in item.NamedArguments)
			{
				if (namedArgument.TypedValue.ArgumentType == typeof(Type))
				{
					type = namedArgument.TypedValue.Value as Type;
				}
				else if (namedArgument.TypedValue.ArgumentType == typeof(string))
				{
					a = namedArgument.TypedValue.Value as string;
				}
			}
		}
		if (!string.Equals(a, "CreateVisuals", StringComparison.Ordinal) || type == null || (type != typeof(Creature) && !typeof(CharacterModel).IsAssignableFrom(type) && !typeof(MonsterModel).IsAssignableFrom(type)))
		{
			return null;
		}
		return type;
	}

	private static bool CanReplayVisualPostfix(MethodInfo method, Type targetType)
	{
		if (method.IsStatic)
		{
			return method.GetParameters().All(delegate(ParameterInfo parameter)
			{
				Type type = (parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType() : parameter.ParameterType);
				bool flag = parameter.HasDefaultValue;
				if (!flag)
				{
					flag = parameter.Name switch
					{
						"__instance" => type.IsAssignableFrom(targetType), 
						"__result" => type.IsAssignableFrom(typeof(NCreatureVisuals)) || typeof(NCreatureVisuals).IsAssignableFrom(type), 
						"__originalMethod" => type == typeof(MethodBase), 
						"__runOriginal" => type == typeof(bool), 
						_ => false, 
					};
				}
				return flag;
			});
		}
		return false;
	}

	private static bool IsPostfixMethod(MethodInfo method)
	{
		if (!method.Name.Equals("Postfix", StringComparison.Ordinal))
		{
			return method.CustomAttributes.Any((CustomAttributeData attribute) => attribute.AttributeType.FullName == "HarmonyLib.HarmonyPostfix");
		}
		return true;
	}

	private static void InvokeVisualPostfix(ProviderAssembly provider, ManagedVisualPostfix postfix, object model, ref NCreatureVisuals visuals)
	{
		ParameterInfo[] parameters = postfix.Method.GetParameters();
		object[] array = new object[parameters.Length];
		int num = -1;
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			Type type = (parameterInfo.ParameterType.IsByRef ? parameterInfo.ParameterType.GetElementType() : parameterInfo.ParameterType);
			switch (parameterInfo.Name)
			{
			case "__instance":
				if (type.IsInstanceOfType(model))
				{
					array[i] = model;
					continue;
				}
				break;
			case "__result":
				if (type.IsAssignableFrom(typeof(NCreatureVisuals)) || typeof(NCreatureVisuals).IsAssignableFrom(type))
				{
					array[i] = visuals;
					num = i;
					continue;
				}
				break;
			case "__originalMethod":
				if (type == typeof(MethodBase))
				{
					array[i] = AccessTools.Method(postfix.TargetType, "CreateVisuals", (Type[])null, (Type[])null);
					continue;
				}
				break;
			case "__runOriginal":
				if (type == typeof(bool))
				{
					array[i] = true;
					continue;
				}
				break;
			}
			if (parameterInfo.HasDefaultValue)
			{
				array[i] = parameterInfo.DefaultValue;
				continue;
			}
			return;
		}
		string item = provider.AssemblyPath + ":" + postfix.Method.MetadataToken;
		try
		{
			AliasedSpineResourceSnapshot? snapshot = CaptureAliasedSpineResource(visuals);
			object obj = postfix.Method.Invoke(null, array);
			if (num >= 0)
			{
				object obj2 = array[num];
				NCreatureVisuals val = (NCreatureVisuals)((obj2 is NCreatureVisuals) ? obj2 : null);
				if (val != null)
				{
					visuals = val;
					goto IL_01ad;
				}
			}
			NCreatureVisuals val2 = (NCreatureVisuals)((obj is NCreatureVisuals) ? obj : null);
			if (val2 != null)
			{
				visuals = val2;
			}
			goto IL_01ad;
			IL_01ad:
			RestoreAliasedSpineResource(snapshot);
			if (ReportedVisualPostfixes.Add(item))
			{
				ModLog.Info($"已按当前选择应用 {provider.Name} 的视觉后处理：{postfix.Method.DeclaringType?.FullName}.{postfix.Method.Name}。");
			}
		}
		catch (Exception ex)
		{
			if (FailedVisualPostfixes.Add(item))
			{
				ModLog.Warn(provider.Name + " 的视觉后处理与当前游戏版本不兼容，已跳过且不会中断游戏：" + ex.GetBaseException().Message);
			}
		}
	}

	private static AliasedSpineResourceSnapshot? CaptureAliasedSpineResource(NCreatureVisuals visuals)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Node nodeOrNull = ((Node)visuals).GetNodeOrNull<Node>(NodePath.op_Implicit("%Visuals"));
		if (nodeOrNull == null)
		{
			return null;
		}
		Variant val = ((GodotObject)nodeOrNull).Get(StringName.op_Implicit("skeleton_data_res"));
		Resource val2 = ((Variant)(ref val)).As<Resource>();
		if (val2 == null || !TryGetCanonicalAliasPath(val2.ResourcePath, out string canonicalPath))
		{
			return null;
		}
		return new AliasedSpineResourceSnapshot(nodeOrNull, val2, canonicalPath);
	}

	private static void RestoreAliasedSpineResource(AliasedSpineResourceSnapshot? snapshot)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (!(snapshot == null) && GodotObject.IsInstanceValid((GodotObject)(object)snapshot.SpineNode))
		{
			Variant val = ((GodotObject)snapshot.SpineNode).Get(StringName.op_Implicit("skeleton_data_res"));
			Resource val2 = ((Variant)(ref val)).As<Resource>();
			if (val2 != null && val2.ResourcePath.Equals(snapshot.CanonicalPath, StringComparison.OrdinalIgnoreCase))
			{
				((GodotObject)snapshot.SpineNode).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)snapshot.Resource));
				TryRefreshSpineAttachments(snapshot.SpineNode);
			}
		}
	}

	private static bool TryGetCanonicalAliasPath(string resourcePath, out string canonicalPath)
	{
		canonicalPath = string.Empty;
		if (!resourcePath.StartsWith("res://sts2_skin_runtime/", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		string text = resourcePath;
		int length = "res://sts2_skin_runtime/".Length;
		string text2 = text.Substring(length, text.Length - length);
		int num = text2.IndexOf('/');
		int num2 = ((num < 0) ? (-1) : text2.IndexOf('/', num + 1));
		if (num2 < 0 || num2 + 1 >= text2.Length)
		{
			return false;
		}
		text = text2;
		length = num2 + 1;
		canonicalPath = "res://" + text.Substring(length, text.Length - length);
		return true;
	}

	private static void TryRefreshSpineAttachments(Node spineNode)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Variant val = ((GodotObject)spineNode).Call(StringName.op_Implicit("get_skeleton"), Array.Empty<Variant>());
			GodotObject obj = ((Variant)(ref val)).As<GodotObject>();
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				val = obj.Call(StringName.op_Implicit("get_skin"), Array.Empty<Variant>());
				obj2 = ((Variant)(ref val)).As<GodotObject>();
			}
			GodotObject val2 = (GodotObject)obj2;
			object obj3;
			if (obj == null)
			{
				obj3 = null;
			}
			else
			{
				val = obj.Call(StringName.op_Implicit("get_slots"), Array.Empty<Variant>());
				obj3 = ((Variant)(ref val)).As<Array>();
			}
			Array val3 = (Array)obj3;
			object obj4;
			if (val2 == null)
			{
				obj4 = null;
			}
			else
			{
				val = val2.Call(StringName.op_Implicit("get_attachments"), Array.Empty<Variant>());
				obj4 = ((Variant)(ref val)).As<Array>();
			}
			Array val4 = (Array)obj4;
			if (val3 == null || val4 == null)
			{
				return;
			}
			foreach (Variant item in val4)
			{
				Variant current = item;
				GodotObject val5 = ((Variant)(ref current)).As<GodotObject>();
				if (val5 == null)
				{
					continue;
				}
				val = val5.Call(StringName.op_Implicit("get_slot_index"), Array.Empty<Variant>());
				int num = ((Variant)(ref val)).AsInt32();
				Variant val6 = val5.Call(StringName.op_Implicit("get_attachment"), Array.Empty<Variant>());
				if ((int)((Variant)(ref val6)).VariantType != 0 && num >= 0 && num < val3.Count)
				{
					val = val3[num];
					GodotObject obj5 = ((Variant)(ref val)).As<GodotObject>();
					if (obj5 != null)
					{
						obj5.Call(StringName.op_Implicit("set_attachment"), (Variant[])(object)new Variant[1] { val6 });
					}
				}
			}
		}
		catch
		{
		}
	}

	private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types.OfType<Type>();
		}
	}

	private static void NotifyModDetected(Mod mod)
	{
		if (InvokeOnModDetectedMethod != null)
		{
			InvokeOnModDetectedMethod.Invoke(null, new object[1] { mod });
			return;
		}
		Delegate[] array = (OnModDetectedField?.GetValue(null) as Delegate)?.GetInvocationList() ?? Array.Empty<Delegate>();
		foreach (Delegate @delegate in array)
		{
			try
			{
				@delegate.DynamicInvoke(mod);
			}
			catch (Exception ex)
			{
				ModLog.Warn($"通知 Mod 加载监听器 {@delegate.Method.DeclaringType?.FullName}.{@delegate.Method.Name} 失败：" + ex.GetBaseException().Message);
			}
		}
	}

	private static bool CanBypassOriginalLoader(Mod mod)
	{
		ModManifest manifest = mod.manifest;
		if (manifest?.id == null)
		{
			return false;
		}
		IReadOnlyList<Mod> mods = ModManager.Mods;
		if (IsRequiredByAnotherMod(mod, mods))
		{
			return false;
		}
		if (mods.Any((Mod other) => other != mod && string.Equals(other.manifest?.id, manifest.id, StringComparison.OrdinalIgnoreCase) && (int)other.state == 1 && !ManagedModInstances.Contains(other)))
		{
			return false;
		}
		if (FrameworkCompatibilityLayer.IsKnownFrameworkHost(manifest.id) && !FrameworkCompatibilityLayer.IsBundledFrameworkHost(manifest.id))
		{
			return false;
		}
		if (FrameworkCompatibilityLayer.IsBundledFrameworkHost(manifest.id))
		{
			return true;
		}
		List<ModDependency> dependencies = manifest.dependencies;
		if (dependencies != null && dependencies.Any((ModDependency dependency) => !DependencyIsSatisfied(mods, dependency) && !IsOptionalFrameworkDependencySatisfied(mod, dependency)))
		{
			return false;
		}
		IReadOnlyDictionary<string, string> obj = CircularDependenciesField.GetValue(null) as IReadOnlyDictionary<string, string>;
		if (obj != null && obj.ContainsKey(manifest.id))
		{
			return false;
		}
		string declaredMinimum = manifest.minGameVersion;
		if (declaredMinimum == null)
		{
			return true;
		}
		ProviderRuntimeAudit runtimeAudit = null;
		object? value = GameVersionField.GetValue(null);
		string text = ((value is SemanticVersion) ? value : null)?.ToString();
		string text2 = manifest.name ?? manifest.id;
		switch (ProviderMinimumVersionPolicy.Decide(declaredMinimum, text, manifest.hasDll, RunsOnCurrentGame))
		{
		case ProviderMinimumVersionOutcome.VerifiedTakeOver:
			ModLog.Info($"{text2} 声明的游戏版本 {declaredMinimum} 高于当前 {text}；已按当前版本核验其皮肤资源与代码（{runtimeAudit?.Describe()}），" + "本次由本 Mod 接管并桥接跨版本接口，原清单未修改。");
			return true;
		case ProviderMinimumVersionOutcome.UnreadableRule:
			ModLog.Warn(text2 + " 声明了无法解析的最低游戏版本“" + declaredMinimum + "”；不猜测该约束，保持游戏原加载器的处理。");
			return false;
		case ProviderMinimumVersionOutcome.Unsupported:
			ModLog.Warn($"{text2} 声明的最低游戏版本 {declaredMinimum} 高于当前 {text}，且其代码无法在当前版本运行（{runtimeAudit?.Describe()}）；" + "保持游戏原加载器的处理，该皮肤本次不可用。");
			return false;
		default:
			return true;
		}
		bool RunsOnCurrentGame()
		{
			runtimeAudit = AuditProviderAssembly(mod, declaredMinimum);
			return runtimeAudit.CanRun;
		}
	}

	private static ProviderRuntimeAudit AuditProviderAssembly(Mod mod, string declaredMinimum)
	{
		ModManifest manifest = mod.manifest;
		ProviderRuntimeAudit providerRuntimeAudit = ProviderAssemblyCompatibility.AuditForCurrentGame(SkinPackagePaths.Resolve(mod.path, manifest.id, ".dll"));
		ModLog.Info((manifest.name ?? manifest.id) + " 的跨版本运行时核验（清单声明最低游戏版本 " + declaredMinimum + "）：" + providerRuntimeAudit.Describe() + "。");
		return providerRuntimeAudit;
	}

	private static bool DependencyIsSatisfied(IEnumerable<Mod> mods, ModDependency dependency)
	{
		Mod val = mods.FirstOrDefault((Func<Mod, bool>)((Mod candidate) => candidate.manifest?.id == dependency.id && (int)candidate.state == 1));
		if (val == null)
		{
			return false;
		}
		if (dependency.minVersion == null)
		{
			return true;
		}
		SemanticVersion val2 = default(SemanticVersion);
		if (SemanticVersion.TryFromString(dependency.minVersion, ref val2) && val.version != null)
		{
			return val.version.CompareTo(val2) >= 0;
		}
		return false;
	}

	private static bool IsManagedProvider(Mod mod, out SkinProviderProbe provider)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(mod.manifest?.id))
			{
				provider = null;
				return false;
			}
			string text = ProviderLookupKey(mod.path, mod.manifest.id);
			if (ProvidersByIdentity.TryGetValue(text, out provider))
			{
				return true;
			}
			if (FrameworkCompatibilityLayer.IsKnownFrameworkHost(mod.manifest?.id))
			{
				provider = new SkinProviderProbe(mod.manifest.id, mod.path, 0, 0, 0, 0, 0, HasInteractiveScenes: false);
				RememberProviderProbe(provider);
				if (FrameworkCompatibilityLayer.IsBundledFrameworkHost(mod.manifest?.id) && !IsRequiredByAnotherMod(mod, ModManager.Mods))
				{
					ModLog.Info("已由内置兼容层接管皮肤框架 " + (mod.manifest?.name ?? mod.manifest?.id) + "；原框架 DLL/PCK 不会执行或全局挂载。");
				}
				else
				{
					ModLog.Info("已把皮肤框架 " + (mod.manifest?.name ?? mod.manifest?.id) + " 纳入加载顺序检查；本次不满足安全替代条件，保留原框架执行。");
				}
				return true;
			}
			if (!NegativeProviderIdentities.Add(text))
			{
				provider = null;
				return false;
			}
			SkinProviderProbe skinProviderProbe = SkinCatalog.ProbeSkinProviders(new _003C_003Ez__ReadOnlySingleElementList<SkinModDescriptor>(ToDescriptor(mod))).FirstOrDefault((SkinProviderProbe probe) => probe.RootPath != null);
			if (skinProviderProbe == null)
			{
				provider = null;
				return false;
			}
			RememberProviderProbe(skinProviderProbe);
			provider = skinProviderProbe;
			ModLog.Info("加载时补充识别皮肤提供者：" + (mod.manifest?.name ?? mod.manifest?.id) + "。");
			return true;
		}
		catch
		{
			provider = null;
			return false;
		}
	}

	private static string ProviderLookupKey(string root, string manifestId)
	{
		return NormalizePath(root) + "\n" + manifestId;
	}

	private static void RememberProviderProbe(SkinProviderProbe provider)
	{
		if (provider.RootPath != null)
		{
			string text = ProviderLookupKey(provider.RootPath, provider.ResourceNamespaceId);
			ProvidersByIdentity[text] = provider;
			ProviderRootPaths.Add(NormalizePath(provider.RootPath));
			ProviderManifestIds.Add(provider.ResourceNamespaceId);
			NegativeProviderIdentities.Remove(text);
		}
	}

	private static void RememberPartTakeover(SkinProviderProbe provider, string displayName)
	{
		if (PartTakeoverLogged.Add(provider.ResourceNamespaceId))
		{
			string obj = $"按部件接管 {displayName}：覆盖 {provider.BaselinePartGroupCount} 个外观分组的部件";
			IReadOnlyList<string> baselinePartGroupIds = provider.BaselinePartGroupIds;
			ModLog.Info(obj + ((baselinePartGroupIds != null && baselinePartGroupIds.Count > 0) ? ("（" + string.Join("、", baselinePartGroupIds) + "）") : string.Empty) + "；原 PCK 与 DLL 由游戏按作者原意完整加载，仅这些部件可在此选择。");
		}
	}

	private static SkinModDescriptor ToDescriptor(Mod mod)
	{
		ModManifest manifest = mod.manifest;
		bool flag = IsRequiredByAnotherMod(mod, ModManager.Mods);
		bool exposesSelectableCosmetics = flag && ExposesSelectableCosmetics(mod);
		return new SkinModDescriptor(manifest.id, manifest.name ?? manifest.id, manifest.hasPck ? SkinPackagePaths.Resolve(mod.path, manifest.id, ".pck") : null, OptionalSkinFrameworkPolicy.ShouldTreatAsGameplayBaseline(manifest.affectsGameplay, flag, exposesSelectableCosmetics), mod.path, manifest.hasDll);
	}

	public static bool ShouldTreatAsGameplayBaseline(Mod mod, IEnumerable<Mod> mods)
	{
		ModManifest manifest = mod.manifest;
		if (manifest == null || manifest.id == null)
		{
			return true;
		}
		bool flag = IsRequiredByAnotherMod(mod, mods);
		return OptionalSkinFrameworkPolicy.ShouldTreatAsGameplayBaseline(manifest.affectsGameplay, flag, flag && ExposesSelectableCosmetics(mod));
	}

	private static bool ExposesSelectableCosmetics(Mod mod)
	{
		ModManifest manifest = mod.manifest;
		if (manifest == null || manifest.id == null || !manifest.hasPck)
		{
			return false;
		}
		string key = ProviderLookupKey(mod.path, manifest.id);
		if (ProvidersByIdentity.TryGetValue(key, out SkinProviderProbe value))
		{
			return HasSelectableCosmetics(value);
		}
		if (SelectableCosmeticProbeResults.TryGetValue(key, out var value2))
		{
			return value2;
		}
		string text = SkinPackagePaths.Resolve(mod.path, manifest.id, ".pck");
		if (!File.Exists(text))
		{
			return false;
		}
		SkinProviderProbe skinProviderProbe = SkinCatalog.ProbeSkinProviders(new _003C_003Ez__ReadOnlySingleElementList<SkinModDescriptor>(new SkinModDescriptor(manifest.id, manifest.name ?? manifest.id, text, AffectsGameplay: false, mod.path, manifest.hasDll))).FirstOrDefault((SkinProviderProbe candidate) => candidate.RootPath != null);
		bool flag = skinProviderProbe != null && HasSelectableCosmetics(skinProviderProbe);
		SelectableCosmeticProbeResults[key] = flag;
		return flag;
	}

	private static bool HasSelectableCosmetics(SkinProviderProbe probe)
	{
		return probe.HasResourceBackedCosmetics;
	}

	public static bool IsRequiredByAnotherMod(Mod mod, IEnumerable<Mod> mods)
	{
		string modId = mod.manifest?.id;
		if (modId == null)
		{
			return false;
		}
		OptionalSkinFrameworkEvidence[] array = mods.Where((Mod other) => other != mod).SelectMany((Mod other) => (from dependency in other.manifest?.dependencies?.Where((ModDependency dependency) => dependency.id.Equals(modId, StringComparison.OrdinalIgnoreCase))
			select BuildOptionalFrameworkEvidence(other, dependency.id)) ?? Array.Empty<OptionalSkinFrameworkEvidence>()).ToArray();
		if (array.Length == 0)
		{
			return false;
		}
		return OptionalSkinFrameworkPolicy.IsFrameworkHostRequired(modId, array, FrameworkCompatibilityLayer.CompatibilityAssemblyNames);
	}

	public static bool CanInstallFrameworkCompatibilityAssembly(string assemblyName)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Invalid comparison between Unknown and I4
		List<OptionalSkinFrameworkEvidence> list = new List<OptionalSkinFrameworkEvidence>();
		bool flag = false;
		foreach (Mod mod in ModManager.Mods)
		{
			ModManifest manifest = mod.manifest;
			string text = manifest?.id;
			if (string.IsNullOrWhiteSpace(text) || Entry.IsSelfModId(text))
			{
				continue;
			}
			if (text.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
			{
				bool flag2 = flag;
				ModLoadState state = mod.state;
				bool flag3 = (int)state <= 1;
				flag = flag2 | (flag3 && IsProviderLoadCandidate(mod));
				continue;
			}
			bool num = manifest.dependencies?.Any((ModDependency dependency) => dependency.id.Equals(assemblyName, StringComparison.OrdinalIgnoreCase)) ?? false;
			bool flag4 = FrameworkSkinContractScanner.Scan(mod.path, text).Any((FrameworkCharacterSkinContract contract) => contract.FrameworkAssemblyName.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
			if (num || flag4)
			{
				list.Add(BuildOptionalFrameworkEvidence(mod, assemblyName));
			}
		}
		return OptionalSkinFrameworkPolicy.CanInstallCompatibilityAssembly(assemblyName, list, flag);
	}

	private static bool IsOptionalFrameworkDependencySatisfied(Mod dependent, ModDependency dependency)
	{
		return OptionalSkinFrameworkPolicy.CanSatisfyMissingDependency(BuildOptionalFrameworkEvidence(dependent, dependency.id), FrameworkCompatibilityLayer.CompatibilityAssemblyNames);
	}

	private static OptionalSkinFrameworkEvidence BuildOptionalFrameworkEvidence(Mod dependent, string dependencyId)
	{
		string text = dependent.manifest?.id ?? string.Empty;
		string key = text + "\n" + dependencyId;
		if (OptionalFrameworkEvidence.TryGetValue(key, out OptionalSkinFrameworkEvidence value))
		{
			return value;
		}
		IReadOnlyList<FrameworkCharacterSkinContract> source = FrameworkSkinContractScanner.Scan(dependent.path, text);
		FrameworkCharacterSkinContract[] array = source.Where((FrameworkCharacterSkinContract contract) => contract.FrameworkAssemblyName.Equals(dependencyId, StringComparison.OrdinalIgnoreCase)).ToArray();
		string referencedAssemblyName = array.FirstOrDefault()?.FrameworkAssemblyName ?? source.FirstOrDefault()?.FrameworkAssemblyName ?? string.Empty;
		bool resourceClosureComplete = false;
		ModManifest manifest = dependent.manifest;
		string text2 = ((manifest != null && manifest.hasPck) ? Path.Combine(dependent.path, text + ".pck") : null);
		if (array.Length != 0 && text2 != null && File.Exists(text2))
		{
			try
			{
				using PckArchive pckArchive = PckArchive.Open(text2);
				HashSet<string> paths = pckArchive.Paths.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
				resourceClosureComplete = array.SelectMany((FrameworkCharacterSkinContract contract) => contract.ResourcePaths).Distinct<string>(StringComparer.OrdinalIgnoreCase).All((string path) => paths.Contains(path) || paths.Contains(path + ".remap") || paths.Contains(path + ".import") || ResourceLoader.Exists(path, ""));
			}
			catch (Exception ex)
			{
				ModLog.Warn("验证 " + (dependent.manifest?.name ?? text) + " 的框架皮肤资源闭包失败：" + ex.GetBaseException().Message);
			}
		}
		OptionalSkinFrameworkEvidence optionalSkinFrameworkEvidence = new OptionalSkinFrameworkEvidence(text, dependencyId, referencedAssemblyName, array.Length != 0, resourceClosureComplete);
		OptionalFrameworkEvidence[key] = optionalSkinFrameworkEvidence;
		return optionalSkinFrameworkEvidence;
	}

	private static string NormalizePath(string path)
	{
		return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
	}
}
