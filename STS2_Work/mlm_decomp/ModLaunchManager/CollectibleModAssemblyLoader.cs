using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using Steamworks;

namespace ModLaunchManager;

internal static class CollectibleModAssemblyLoader
{
	private sealed class ModAssemblyLoadContext : AssemblyLoadContext
	{
		private readonly AssemblyDependencyResolver _resolver;

		private readonly string _modDirectory;

		private readonly AssemblyLoadContext _hostContext;

		internal ModAssemblyLoadContext(string mainAssemblyPath, AssemblyLoadContext hostContext)
			: base($"MLM:{Path.GetFileNameWithoutExtension(mainAssemblyPath)}:{Guid.NewGuid():N}", isCollectible: true)
		{
			_resolver = new AssemblyDependencyResolver(mainAssemblyPath);
			_modDirectory = Path.GetDirectoryName(mainAssemblyPath) ?? string.Empty;
			_hostContext = hostContext;
		}

		protected override Assembly? Load(AssemblyName assemblyName)
		{
			Assembly assembly = LoadSharedHostContract(assemblyName) ?? FindAlreadyLoadedAssembly(assemblyName);
			if (assembly != null)
			{
				return assembly;
			}
			string text = _resolver.ResolveAssemblyToPath(assemblyName);
			if (text == null)
			{
				string name = assemblyName.Name;
				if (name != null && name.Length > 0)
				{
					string text2 = Path.Combine(_modDirectory, name + ".dll");
					if (File.Exists(text2))
					{
						text = text2;
					}
				}
			}
			if (text != null)
			{
				return LoadFromAssemblyPath(text);
			}
			return LoadFromHostContext(assemblyName);
		}

		internal Assembly? LoadSharedHostContract(AssemblyName assemblyName)
		{
			Assembly assembly = FindHostContractAssembly(assemblyName);
			if (assembly != null)
			{
				return assembly;
			}
			if (assemblyName.Name == null || !HostDependencyAssemblies.Contains(assemblyName.Name))
			{
				return null;
			}
			return LoadFromHostContext(assemblyName);
		}

		private Assembly? LoadFromHostContext(AssemblyName assemblyName)
		{
			try
			{
				Assembly assembly = _hostContext.LoadFromAssemblyName(assemblyName);
				if (!string.Equals(assembly.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase))
				{
					return null;
				}
				ReportHostShare(assemblyName);
				return assembly;
			}
			catch (Exception ex) when (((ex is FileNotFoundException || ex is FileLoadException || ex is BadImageFormatException) ? 1 : 0) != 0)
			{
				return null;
			}
		}

		protected override nint LoadUnmanagedDll(string unmanagedDllName)
		{
			string text = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
			if (text == null)
			{
				string text2 = Path.Combine(_modDirectory, unmanagedDllName);
				if (File.Exists(text2))
				{
					text = text2;
				}
			}
			if (text != null)
			{
				return LoadUnmanagedDllFromPath(text);
			}
			return IntPtr.Zero;
		}
	}

	private sealed record ContextRegistration(Mod Owner, ModAssemblyLoadContext Context);

	private sealed record PendingUnload(WeakReference[] Contexts, int RemovedHarmonyPatches, int ClearedGameRegistrations, IReadOnlyList<string> PurgeFailures)
	{
		internal bool AnyAlive => Contexts.Any((WeakReference context) => context.IsAlive);

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Contexts = ");
			builder.Append(Contexts);
			builder.Append(", RemovedHarmonyPatches = ");
			builder.Append(RemovedHarmonyPatches.ToString());
			builder.Append(", ClearedGameRegistrations = ");
			builder.Append(ClearedGameRegistrations.ToString());
			builder.Append(", PurgeFailures = ");
			builder.Append(PurgeFailures);
			return true;
		}
	}

	[CompilerGenerated]
	private sealed class _003CTranspiler_003Ed__25 : IEnumerable<CodeInstruction>, IEnumerable, IEnumerator<CodeInstruction>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private CodeInstruction _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<CodeInstruction> instructions;

		public IEnumerable<CodeInstruction> _003C_003E3__instructions;

		private MethodInfo _003CvanillaLoad_003E5__2;

		private MethodInfo _003Creplacement_003E5__3;

		private int _003Creplacements_003E5__4;

		private IEnumerator<CodeInstruction> _003C_003E7__wrap4;

		CodeInstruction IEnumerator<CodeInstruction>.Current
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
		public _003CTranspiler_003Ed__25(int _003C_003E1__state)
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
			_003CvanillaLoad_003E5__2 = null;
			_003Creplacement_003E5__3 = null;
			_003C_003E7__wrap4 = null;
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
					_003CvanillaLoad_003E5__2 = AccessTools.Method(typeof(AssemblyLoadContext), "LoadFromAssemblyPath", new Type[1] { typeof(string) }, (Type[])null) ?? throw new MissingMethodException("AssemblyLoadContext", "LoadFromAssemblyPath");
					_003Creplacement_003E5__3 = AccessTools.Method(typeof(CollectibleModAssemblyLoader), "LoadFromCollectibleContext", (Type[])null, (Type[])null) ?? throw new MissingMethodException("CollectibleModAssemblyLoader", "LoadFromCollectibleContext");
					_003Creplacements_003E5__4 = 0;
					_003C_003E7__wrap4 = instructions.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap4.MoveNext())
				{
					CodeInstruction current = _003C_003E7__wrap4.Current;
					if (CodeInstructionExtensions.Calls(current, _003CvanillaLoad_003E5__2))
					{
						current.opcode = OpCodes.Call;
						current.operand = _003Creplacement_003E5__3;
						_003Creplacements_003E5__4++;
					}
					_003C_003E2__current = current;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap4 = null;
				if (_003Creplacements_003E5__4 != 1)
				{
					throw new InvalidOperationException($"expected one vanilla DLL load call, found {_003Creplacements_003E5__4}");
				}
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
			if (_003C_003E7__wrap4 != null)
			{
				_003C_003E7__wrap4.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<CodeInstruction> IEnumerable<CodeInstruction>.GetEnumerator()
		{
			_003CTranspiler_003Ed__25 _003CTranspiler_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CTranspiler_003Ed__ = this;
			}
			else
			{
				_003CTranspiler_003Ed__ = new _003CTranspiler_003Ed__25(0);
			}
			_003CTranspiler_003Ed__.instructions = _003C_003E3__instructions;
			return _003CTranspiler_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<CodeInstruction>)this).GetEnumerator();
		}
	}

	private const int MaxReportedHolders = 3;

	private const int MaxReportedFieldsPerHolder = 3;

	private static readonly object Gate = new object();

	private static readonly HashSet<string> SharedContractAssemblies = new HashSet<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[5] { "sts2", "0Harmony", "GodotSharp", "Steamworks.NET", "ModLaunchManager" }), StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> HostDependencyAssemblies = new HashSet<string>((from name in typeof(ModManager).Assembly.GetReferencedAssemblies()
		select name.Name).OfType<string>(), StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> PreLoadErrorKeys = new HashSet<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[11]
	{
		"MOD_ERROR.CIRCULAR_DEPENDENCY", "MOD_ERROR.DEPENDENCY_MIN_VERSION_INVALID", "MOD_ERROR.DEPENDENCY_VERSION_INVALID", "MOD_ERROR.DEPENDENCY_VERSION_MISSING", "MOD_ERROR.DEPENDENCY_VERSION_UNSUPPORTED", "MOD_ERROR.DUPLICATE_ID", "MOD_ERROR.GAME_VERSION_INVALID", "MOD_ERROR.GAME_VERSION_UNSUPPORTED", "MOD_ERROR.MIGRATION_REQUIRED", "MOD_ERROR.MISSING_DEPENDENCY",
		"MOD_ERROR.STEAM_BRANCH_UNSUPPORTED"
	}), StringComparer.Ordinal);

	private static readonly HashSet<string> HostSharedReported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<Assembly, ContextRegistration> Registrations = new Dictionary<Assembly, ContextRegistration>();

	private static readonly HashSet<Mod> GameContextLoads = new HashSet<Mod>();

	private static Dictionary<Mod, ModAssemblyLoadDecision>? _loadDecisions;

	private static bool _isolationEnabled = true;

	private static readonly Dictionary<Mod, PendingUnload> PendingUnloads = new Dictionary<Mod, PendingUnload>();

	private static readonly Harmony RollbackHarmony = new Harmony("ModLaunchManager.RuntimeRollback");

	private static readonly FieldInfo? ModAssembliesField = AccessTools.Field(typeof(Mod), "assemblies");

	private static readonly FieldInfo? ModAssemblyField = AccessTools.Field(typeof(Mod), "assembly");

	private static readonly FieldInfo? HarmonyOriginalsField = typeof(Harmony).Assembly.GetType("HarmonyLib.HarmonySharedState", throwOnError: false)?.GetField("originals", BindingFlags.Static | BindingFlags.NonPublic);

	private static bool _installed;

	private static void ReportHostShare(AssemblyName assemblyName)
	{
		string text = assemblyName.Name ?? assemblyName.FullName;
		lock (Gate)
		{
			if (!HostSharedReported.Add(text))
			{
				return;
			}
		}
		try
		{
			GD.Print("[ModLaunchManager] shared " + text + " from the game's load context instead of letting the mod context load its own copy.");
		}
		catch
		{
		}
	}

	private static Assembly? FindHostContractAssembly(AssemblyName requested)
	{
		return requested.Name switch
		{
			"sts2" => typeof(ModManager).Assembly, 
			"0Harmony" => typeof(Harmony).Assembly, 
			"GodotSharp" => typeof(GD).Assembly, 
			"Steamworks.NET" => typeof(SteamAPI).Assembly, 
			"ModLaunchManager" => typeof(CollectibleModAssemblyLoader).Assembly, 
			_ => null, 
		};
	}

	private static Assembly? FindAlreadyLoadedAssembly(AssemblyName requested)
	{
		Assembly assembly = FindHostContractAssembly(requested);
		if (assembly != null)
		{
			return assembly;
		}
		bool shareBySimpleName = requested.Name != null && SharedContractAssemblies.Contains(requested.Name);
		Assembly assembly2 = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly candidate) => (!shareBySimpleName) ? AssemblyName.ReferenceMatchesDefinition(candidate.GetName(), requested) : string.Equals(candidate.GetName().Name, requested.Name, StringComparison.OrdinalIgnoreCase));
		if (assembly2 != null)
		{
			return assembly2;
		}
		lock (Gate)
		{
			return Registrations.Keys.FirstOrDefault((Assembly candidate) => AssemblyName.ReferenceMatchesDefinition(candidate.GetName(), requested));
		}
	}

	internal static bool Install(Harmony harmony)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		MethodInfo methodInfo = null;
		try
		{
			MethodInfo methodInfo2 = AccessTools.Method(typeof(ModManager), "TryLoadMod", new Type[1] { typeof(Mod) }, (Type[])null) ?? throw new MissingMethodException("ModManager", "TryLoadMod");
			MethodInfo initializerMethod = AccessTools.Method(typeof(ModManager), "CallModInitializer", new Type[1] { typeof(Type) }, (Type[])null) ?? throw new MissingMethodException("ModManager", "CallModInitializer");
			HarmonyCallerAssembly.Install(harmony, initializerMethod);
			methodInfo = AccessTools.Method(typeof(AssemblyLoadContext), "LoadFromAssemblyPath", new Type[1] { typeof(string) }, (Type[])null) ?? throw new MissingMethodException("AssemblyLoadContext", "LoadFromAssemblyPath");
			harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(AccessTools.Method(typeof(CollectibleModAssemblyLoader), "ShareExplicitHostAssembly", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			harmony.Patch((MethodBase)methodInfo2, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(typeof(CollectibleModAssemblyLoader), "Transpiler", (Type[])null, (Type[])null)), (HarmonyMethod)null);
			_installed = true;
			try
			{
				GD.Print("[ModLaunchManager] mod loading policy enabled: collectible by default, game context for compatibility requirements; native loads require restart to remove.");
			}
			catch
			{
			}
			return true;
		}
		catch (Exception ex)
		{
			_installed = false;
			if (methodInfo != null)
			{
				harmony.Unpatch((MethodBase)methodInfo, AccessTools.Method(typeof(CollectibleModAssemblyLoader), "ShareExplicitHostAssembly", (Type[])null, (Type[])null));
			}
			GD.PushWarning($"{"[ModLaunchManager]"} collectible DLL loader unavailable: {ex.GetType().Name} {ex.Message}");
			return false;
		}
	}

	private static bool ShareExplicitHostAssembly(AssemblyLoadContext __instance, string assemblyPath, ref Assembly? __result)
	{
		if (!(__instance is ModAssemblyLoadContext modAssemblyLoadContext) || string.IsNullOrEmpty(assemblyPath) || !Path.IsPathFullyQualified(assemblyPath))
		{
			return true;
		}
		AssemblyName assemblyName;
		try
		{
			assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
		}
		catch (Exception ex) when (((ex is IOException || ex is BadImageFormatException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			return true;
		}
		Assembly assembly = modAssemblyLoadContext.LoadSharedHostContract(assemblyName);
		if (assembly == null || !string.Equals(assembly.FullName, assemblyName.FullName, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		__result = assembly;
		return false;
	}

	[IteratorStateMachine(typeof(_003CTranspiler_003Ed__25))]
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTranspiler_003Ed__25(-2)
		{
			_003C_003E3__instructions = instructions
		};
	}

	internal static void PrepareLoadPlan(IEnumerable<Mod> mods, bool isolationEnabled = true)
	{
		_isolationEnabled = isolationEnabled;
		_loadDecisions = (isolationEnabled ? ModAssemblyLoadingPolicy.Build(mods) : new Dictionary<Mod, ModAssemblyLoadDecision>());
	}

	internal static bool RequiresRestartToUnload(Mod mod)
	{
		lock (Gate)
		{
			if (GameContextLoads.Contains(mod))
			{
				return true;
			}
		}
		return GetRecordedAssemblies(mod).Any((Assembly assembly) => !assembly.IsCollectible);
	}

	private static Assembly LoadFromCollectibleContext(AssemblyLoadContext vanillaContext, string assemblyPath)
	{
		string fullPath = Path.GetFullPath(assemblyPath);
		Mod val = FindOwningMod(fullPath);
		if (val == null)
		{
			return vanillaContext.LoadFromAssemblyPath(fullPath);
		}
		ModAssemblyLoadDecision value;
		if (!_isolationEnabled)
		{
			value = new ModAssemblyLoadDecision(UseGameContext: true, "isolated mod loading disabled by the player");
		}
		else
		{
			if (_loadDecisions == null)
			{
				_loadDecisions = ModAssemblyLoadingPolicy.Build(ModManager.Mods);
			}
			if (!_loadDecisions.TryGetValue(val, out value))
			{
				value = ModAssemblyLoadingPolicy.ReadRequirement(val);
			}
		}
		if (value.UseGameContext)
		{
			lock (Gate)
			{
				GameContextLoads.Add(val);
			}
			GD.Print($"{"[ModLaunchManager]"} loading {val.manifest?.id} in the game's context: {value.Reason}. Runtime removal requires restart.");
			return vanillaContext.LoadFromAssemblyPath(fullPath);
		}
		ModAssemblyLoadContext modAssemblyLoadContext = new ModAssemblyLoadContext(fullPath, vanillaContext);
		try
		{
			Assembly assembly = modAssemblyLoadContext.LoadFromAssemblyPath(fullPath);
			if (AssemblyLoadContext.GetLoadContext(assembly) != modAssemblyLoadContext)
			{
				modAssemblyLoadContext.Unload();
				lock (Gate)
				{
					GameContextLoads.Add(val);
				}
				GD.Print("[ModLaunchManager] " + val.manifest?.id + " was loaded outside its assigned context; runtime removal requires restart.");
				return assembly;
			}
			lock (Gate)
			{
				Registrations.Add(assembly, new ContextRegistration(val, modAssemblyLoadContext));
			}
			return assembly;
		}
		catch
		{
			modAssemblyLoadContext.Unload();
			throw;
		}
	}

	private static Mod? FindOwningMod(string fullPath)
	{
		foreach (Mod mod in ModManager.Mods)
		{
			ModManifest manifest = mod.manifest;
			if (manifest != null && manifest.hasDll)
			{
				string fullPath2;
				try
				{
					fullPath2 = Path.GetFullPath(Path.Combine(mod.path, mod.manifest.id + ".dll"));
				}
				catch
				{
					continue;
				}
				if (string.Equals(fullPath2, fullPath, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
				{
					return mod;
				}
			}
		}
		return null;
	}

	internal static RuntimeUnloadResult TryRollback(Mod mod)
	{
		if (RequiresRestartToUnload(mod))
		{
			return RuntimeUnloadResult.Failure("loading.error.unload.notCollectible");
		}
		if (FailedBeforeRuntimePayloadLoad(mod))
		{
			return RuntimeUnloadResult.NothingToDo;
		}
		lock (Gate)
		{
			if (PendingUnloads.TryGetValue(mod, out PendingUnload value))
			{
				return ReportMountedPck(mod, FinishCollection(mod, value));
			}
		}
		RuntimeUnloadResult? runtimeUnloadResult = PrepareUnload(mod);
		if (runtimeUnloadResult.HasValue)
		{
			return runtimeUnloadResult.Value;
		}
		PendingUnload pending;
		lock (Gate)
		{
			pending = PendingUnloads[mod];
		}
		return ReportMountedPck(mod, FinishCollection(mod, pending));
	}

	private static RuntimeUnloadResult ReportMountedPck(Mod mod, RuntimeUnloadResult result)
	{
		if (result.Success)
		{
			ModManifest manifest = mod.manifest;
			if (manifest != null && manifest.hasPck)
			{
				GD.PushWarning("[ModLaunchManager] unloaded DLL for " + mod.manifest.id + "; its PCK remains mounted for this process.");
			}
		}
		return result;
	}

	private static bool FailedBeforeRuntimePayloadLoad(Mod mod)
	{
		if (GetRecordedAssemblies(mod).Length == 0)
		{
			List<LocString> errors = mod.errors;
			if (errors != null && errors.Count > 0)
			{
				return mod.errors.All((LocString error) => PreLoadErrorKeys.Contains(error.LocEntryKey));
			}
		}
		return false;
	}

	internal static Assembly[] GetRecordedAssemblies(Mod mod)
	{
		if (ModAssembliesField?.GetValue(mod) is IEnumerable source)
		{
			return source.Cast<object>().OfType<Assembly>().Distinct()
				.ToArray();
		}
		if (!(ModAssemblyField?.GetValue(mod) is Assembly assembly))
		{
			return Array.Empty<Assembly>();
		}
		return new Assembly[1] { assembly };
	}

	private static void ClearRecordedAssemblies(Mod mod)
	{
		if (ModAssembliesField?.GetValue(mod) is IList list)
		{
			list.Clear();
		}
		ModAssemblyField?.SetValue(mod, null);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static RuntimeUnloadResult? PrepareUnload(Mod mod)
	{
		Assembly[] recordedAssemblies = GetRecordedAssemblies(mod);
		if (recordedAssemblies.Length == 0)
		{
			return RuntimeUnloadResult.NothingToDo;
		}
		if (!_installed)
		{
			return RuntimeUnloadResult.Failure("loading.error.unload.notCollectible");
		}
		ContextRegistration[] array;
		lock (Gate)
		{
			array = Registrations.Values.Where((ContextRegistration candidate) => candidate.Owner == mod).Distinct().ToArray();
		}
		if (array.Length == 0)
		{
			return RuntimeUnloadResult.Failure("loading.error.unload.foreignContext");
		}
		Assembly[] owned = array.SelectMany((ContextRegistration registration) => registration.Context.Assemblies).Distinct().ToArray();
		string[] array2 = (from assembly in recordedAssemblies
			where !owned.Contains(assembly)
			select assembly.GetName().Name ?? "<unknown>").Distinct<string>(StringComparer.Ordinal).ToArray();
		if (array2.Length != 0)
		{
			return RuntimeUnloadResult.Failure("loading.error.unload.strayAssembly", string.Join(", ", array2));
		}
		int removedHarmonyPatches = 0;
		RegistryPurgeReport registryPurgeReport;
		try
		{
			removedHarmonyPatches = RemoveHarmonyReferences((IReadOnlyCollection<Assembly>)(object)owned);
			registryPurgeReport = ModRegistryPurge.DropReferences((IReadOnlyCollection<Assembly>)(object)owned);
		}
		catch (Exception ex)
		{
			return new RuntimeUnloadResult(Success: false, removedHarmonyPatches, 0, "loading.error.unload.cleanupFailed", ex.GetType().Name + " " + ex.Message);
		}
		ClearRecordedAssemblies(mod);
		lock (Gate)
		{
			Assembly[] array3 = owned;
			foreach (Assembly key in array3)
			{
				Registrations.Remove(key);
			}
		}
		WeakReference[] contexts = array.Select((ContextRegistration registration) => new WeakReference(registration.Context, trackResurrection: true)).ToArray();
		ContextRegistration[] array4 = array;
		for (int i = 0; i < array4.Length; i++)
		{
			array4[i].Context.Unload();
		}
		PendingUnload value = new PendingUnload(contexts, removedHarmonyPatches, registryPurgeReport.ClearedRoots, registryPurgeReport.Failures);
		lock (Gate)
		{
			PendingUnloads[mod] = value;
		}
		return null;
	}

	private static RuntimeUnloadResult FinishCollection(Mod mod, PendingUnload pending)
	{
		Collect(pending);
		int num = pending.ClearedGameRegistrations;
		if (pending.AnyAlive)
		{
			num += PruneForeignRoots(pending);
			Collect(pending);
		}
		if (pending.AnyAlive)
		{
			return new RuntimeUnloadResult(Success: false, pending.RemovedHarmonyPatches, num, "loading.error.unload.stillReferenced", DescribeRetainers(pending));
		}
		lock (Gate)
		{
			PendingUnloads.Remove(mod);
		}
		return new RuntimeUnloadResult(Success: true, pending.RemovedHarmonyPatches, num, null, null);
	}

	private static void Collect(PendingUnload pending)
	{
		for (int i = 0; i < 6; i++)
		{
			if (!pending.AnyAlive)
			{
				break;
			}
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static int PruneForeignRoots(PendingUnload pending)
	{
		try
		{
			return ModRootScanner.Sweep((IReadOnlyCollection<WeakReference>)(object)pending.Contexts, prune: true).Sum((ModRootHit hit) => hit.Removed);
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not sweep other mods' static caches: {ex.GetType().Name} {ex.Message}");
			return 0;
		}
	}

	private static string DescribeRetainers(PendingUnload pending)
	{
		List<string> list = new List<string>(ModRegistryPurge.DescribeRetainers((IReadOnlyCollection<WeakReference>)(object)pending.Contexts));
		string text = DescribeForeignHolders((IReadOnlyCollection<WeakReference>)(object)pending.Contexts);
		if (text != null)
		{
			list.Add(MlmLocManager.Get("loading.error.unload.retainerForeignStatics", ("holders", text)));
		}
		if (pending.PurgeFailures.Count > 0)
		{
			list.Add(MlmLocManager.Get("loading.error.unload.retainerPurgeFailed", ("registries", string.Join("; ", pending.PurgeFailures))));
		}
		if (list.Count <= 0)
		{
			return MlmLocManager.Get("loading.error.unload.retainerUnknown");
		}
		return string.Join(" ", list);
	}

	private static string? DescribeForeignHolders(IReadOnlyCollection<WeakReference> contexts)
	{
		ModRootHit[] array;
		try
		{
			array = ModRootScanner.Sweep(contexts, prune: false).ToArray();
		}
		catch
		{
			return null;
		}
		if (array.Length == 0)
		{
			return null;
		}
		return string.Join("; ", (from @group in array.GroupBy<ModRootHit, string>((ModRootHit hit) => hit.HolderAssembly, StringComparer.Ordinal)
			orderby @group.Sum((ModRootHit hit) => hit.References) descending
			select @group).Take(3).Select(delegate(IGrouping<string, ModRootHit> group)
		{
			string text = string.Join(", ", from hit in @group.OrderByDescending((ModRootHit hit) => hit.References).Take(3)
				select hit.Field);
			return group.Key + " (" + text + ")";
		}));
	}

	private static int RemoveHarmonyReferences(IReadOnlyCollection<Assembly> assemblies)
	{
		HashSet<Assembly> owned = new HashSet<Assembly>(assemblies, ReferenceEqualityComparer.Instance);
		int num = 0;
		MethodBase[] array = Harmony.GetAllPatchedMethods().ToArray();
		foreach (MethodBase original in array)
		{
			Patches patchInfo = Harmony.GetPatchInfo(original);
			if (patchInfo == null)
			{
				continue;
			}
			if (owned.Contains(original.Module.Assembly))
			{
				num += patchInfo.Prefixes.Count + patchInfo.Postfixes.Count + patchInfo.Transpilers.Count + patchInfo.Finalizers.Count + patchInfo.InnerPrefixes.Count + patchInfo.InnerPostfixes.Count;
				UnpatchAndForgetSupersededWrappers(original, delegate
				{
					RollbackHarmony.Unpatch(original, (HarmonyPatchType)0, "*");
				});
				continue;
			}
			MethodInfo[] array2 = (from patch in patchInfo.Prefixes.Concat(patchInfo.Postfixes).Concat(patchInfo.Transpilers).Concat(patchInfo.Finalizers)
					.Concat(patchInfo.InnerPrefixes)
					.Concat(patchInfo.InnerPostfixes)
				select patch.PatchMethod into method
				where owned.Contains(method.Module.Assembly)
				select method).Distinct().ToArray();
			foreach (MethodInfo patchMethod in array2)
			{
				UnpatchAndForgetSupersededWrappers(original, delegate
				{
					RollbackHarmony.Unpatch(original, patchMethod);
				});
				num++;
			}
		}
		return num;
	}

	private static void UnpatchAndForgetSupersededWrappers(MethodBase original, Action unpatch)
	{
		IDictionary dictionary = HarmonyOriginalsField?.GetValue(null) as IDictionary;
		object[] array = Array.Empty<object>();
		if (dictionary != null)
		{
			lock (dictionary.SyncRoot)
			{
				List<object> list = new List<object>();
				IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (object.Equals(enumerator.Value, original) && enumerator.Key != null)
					{
						list.Add(enumerator.Key);
					}
				}
				array = list.ToArray();
			}
		}
		unpatch();
		if (dictionary == null || array.Length == 0)
		{
			return;
		}
		lock (dictionary.SyncRoot)
		{
			object[] array2 = array;
			foreach (object key in array2)
			{
				dictionary.Remove(key);
			}
		}
	}
}
