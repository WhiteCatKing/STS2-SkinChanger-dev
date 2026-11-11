using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

internal static class VisualPatchGuard
{
	private enum ProviderPatchKind
	{
		Prefix,
		Postfix,
		Transpiler,
		Finalizer
	}

	[CompilerGenerated]
	private sealed class _003CEnumeratePatches_003Ed__5 : IEnumerable<(Patch Patch, ProviderPatchKind Kind)>, IEnumerable, IEnumerator<(Patch Patch, ProviderPatchKind Kind)>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private (Patch Patch, ProviderPatchKind Kind) _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Patches patches;

		public Patches _003C_003E3__patches;

		private IEnumerator<Patch> _003C_003E7__wrap1;

		(Patch Patch, ProviderPatchKind Kind) IEnumerator<(Patch, ProviderPatchKind)>.Current
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
		public _003CEnumeratePatches_003Ed__5(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			switch (_003C_003E1__state)
			{
			case -3:
			case 1:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
				break;
			case -4:
			case 2:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally2();
				}
				break;
			case -5:
			case 3:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally3();
				}
				break;
			case -6:
			case 4:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally4();
				}
				break;
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
					_003C_003E7__wrap1 = patches.Prefixes.GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_007e;
				case 1:
					_003C_003E1__state = -3;
					goto IL_007e;
				case 2:
					_003C_003E1__state = -4;
					goto IL_00e7;
				case 3:
					_003C_003E1__state = -5;
					goto IL_0152;
				case 4:
					{
						_003C_003E1__state = -6;
						break;
					}
					IL_007e:
					if (_003C_003E7__wrap1.MoveNext())
					{
						Patch current = _003C_003E7__wrap1.Current;
						_003C_003E2__current = (Patch: current, Kind: ProviderPatchKind.Prefix);
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					_003C_003E7__wrap1 = patches.Postfixes.GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_00e7;
					IL_0152:
					if (_003C_003E7__wrap1.MoveNext())
					{
						Patch current2 = _003C_003E7__wrap1.Current;
						_003C_003E2__current = (Patch: current2, Kind: ProviderPatchKind.Transpiler);
						_003C_003E1__state = 3;
						return true;
					}
					_003C_003Em__Finally3();
					_003C_003E7__wrap1 = null;
					_003C_003E7__wrap1 = patches.Finalizers.GetEnumerator();
					_003C_003E1__state = -6;
					break;
					IL_00e7:
					if (_003C_003E7__wrap1.MoveNext())
					{
						Patch current3 = _003C_003E7__wrap1.Current;
						_003C_003E2__current = (Patch: current3, Kind: ProviderPatchKind.Postfix);
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap1 = null;
					_003C_003E7__wrap1 = patches.Transpilers.GetEnumerator();
					_003C_003E1__state = -5;
					goto IL_0152;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					Patch current4 = _003C_003E7__wrap1.Current;
					_003C_003E2__current = (Patch: current4, Kind: ProviderPatchKind.Finalizer);
					_003C_003E1__state = 4;
					return true;
				}
				_003C_003Em__Finally4();
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

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		private void _003C_003Em__Finally3()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		private void _003C_003Em__Finally4()
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
		IEnumerator<(Patch Patch, ProviderPatchKind Kind)> IEnumerable<(Patch, ProviderPatchKind)>.GetEnumerator()
		{
			_003CEnumeratePatches_003Ed__5 _003CEnumeratePatches_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumeratePatches_003Ed__ = this;
			}
			else
			{
				_003CEnumeratePatches_003Ed__ = new _003CEnumeratePatches_003Ed__5(0);
			}
			_003CEnumeratePatches_003Ed__.patches = _003C_003E3__patches;
			return _003CEnumeratePatches_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<(Patch, ProviderPatchKind)>)this).GetEnumerator();
		}
	}

	private static readonly Harmony Harmony = new Harmony("Gurio.SkinChanger");

	private static readonly string[] VisualNameTokens = new string[14]
	{
		"Visual", "Scene", "Portrait", "Icon", "Texture", "Sprite", "Atlas", "Background", "MapMarker", "Animation",
		"Sfx", "Sound", "Audio", "Voice"
	};

	private static readonly string[] VisualTypeNameTokens = new string[13]
	{
		"Visual", "Vfx", "Spine", "Sprite", "Portrait", "Icon", "Character", "Creature", "Merchant", "Card",
		"Animation", "Audio", "Sound"
	};

	public static int RemoveProviderVisualPatches(IEnumerable<string> providerRoots, IEnumerable<string>? preservedRuntimeRoots = null)
	{
		string[] roots = (from root in providerRoots.Select(NormalizeRoot)
			where root != null
			select root).Cast<string>().Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		if (roots.Length == 0)
		{
			return 0;
		}
		IReadOnlySet<string> preservedRoots = ResolveProtectedRoots(preservedRuntimeRoots);
		int num = 0;
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
		MethodBase[] array = Harmony.GetAllPatchedMethods().Where(IsVisualTarget).ToArray();
		foreach (MethodBase methodBase in array)
		{
			Patches patchInfo = Harmony.GetPatchInfo(methodBase);
			if (patchInfo == null)
			{
				continue;
			}
			(Patch, ProviderPatchKind, string)[] array2 = (from entry in EnumeratePatches(patchInfo)
				select (Patch: entry.Patch, Kind: entry.Kind, Root: GetProviderRoot(entry.Patch, roots, preservedRoots)) into entry
				where entry.Root != null && !ManagedSkinModLoader.IsProviderAssemblyActive(entry.Patch.PatchMethod.Module.Assembly)
				select entry).DistinctBy<(Patch, ProviderPatchKind, string), (MethodInfo, ProviderPatchKind)>(((Patch Patch, ProviderPatchKind Kind, string Root) entry) => (PatchMethod: entry.Patch.PatchMethod, Kind: entry.Kind)).ToArray();
			for (int j = 0; j < array2.Length; j++)
			{
				(Patch, ProviderPatchKind, string) tuple = array2[j];
				try
				{
					Harmony.Unpatch(methodBase, tuple.Item1.PatchMethod);
					num++;
					hashSet.Add(methodBase.DeclaringType?.Name + "." + methodBase.Name);
					string key = tuple.Item1.PatchMethod.Module.Assembly.GetName().Name ?? tuple.Item1.owner;
					dictionary[key] = dictionary.GetValueOrDefault(key) + 1;
				}
				catch (Exception ex)
				{
					ModLog.Warn($"无法移除视觉补丁 {tuple.Item1.owner}/{tuple.Item1.PatchMethod.DeclaringType?.FullName}.{tuple.Item1.PatchMethod.Name}：" + ex.Message);
				}
			}
		}
		if (num > 0)
		{
			ModLog.Info($"已移除 {num} 个提供者皮肤呈现补丁，保留其余 DLL 功能；目标：" + string.Join("、", hashSet.OrderBy((string name) => name)) + "；来源：" + string.Join("、", from pair in dictionary
				orderby pair.Key
				select $"{pair.Key}={pair.Value}"));
		}
		return num;
	}

	internal static IReadOnlySet<string> ResolveProtectedRoots(IEnumerable<string>? preservedRuntimeRoots)
	{
		return new HashSet<string>((from root in (preservedRuntimeRoots ?? Array.Empty<string>()).Concat(ManagedSkinModLoader.PartTakeoverRoots).Select(NormalizeRoot)
			where root != null
			select root).Cast<string>(), StringComparer.OrdinalIgnoreCase);
	}

	[IteratorStateMachine(typeof(_003CEnumeratePatches_003Ed__5))]
	private static IEnumerable<(Patch Patch, ProviderPatchKind Kind)> EnumeratePatches(Patches patches)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumeratePatches_003Ed__5(-2)
		{
			_003C_003E3__patches = patches
		};
	}

	private static bool IsVisualTarget(MethodBase target)
	{
		Type declaringType = target.DeclaringType;
		if (declaringType == null)
		{
			return false;
		}
		if (declaringType == typeof(AssetCache))
		{
			switch (target.Name)
			{
			case "GetAsset":
			case "GetScene":
			case "GetTexture2D":
				return true;
			default:
				return false;
			}
		}
		if (declaringType == typeof(AtlasManager))
		{
			switch (target.Name)
			{
			case "GetSprite":
			case "LoadAtlas":
			case "LoadAtlasInternal":
				return true;
			default:
				return false;
			}
		}
		if (declaringType == typeof(SceneHelper) || declaringType == typeof(ImageHelper))
		{
			if (!target.Name.Contains("Path", StringComparison.OrdinalIgnoreCase) && !target.Name.Contains("Load", StringComparison.OrdinalIgnoreCase))
			{
				return target.Name.Contains("Instantiate", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		if (typeof(CharacterModel).IsAssignableFrom(declaringType) || typeof(MonsterModel).IsAssignableFrom(declaringType) || typeof(EventModel).IsAssignableFrom(declaringType) || typeof(CardModel).IsAssignableFrom(declaringType))
		{
			bool flag = declaringType == typeof(CharacterModel);
			if (flag)
			{
				bool flag2;
				switch (target.Name)
				{
				case "get_VisualsPath":
				case "get_MerchantAnimPath":
				case "get_RestSiteAnimPath":
					flag2 = true;
					break;
				default:
					flag2 = false;
					break;
				}
				flag = flag2;
			}
			if (flag)
			{
				return false;
			}
			if (!HasVisualName(target.Name))
			{
				return ReturnsVisualValue(target);
			}
			return true;
		}
		if (typeof(NCreatureVisuals).IsAssignableFrom(declaringType))
		{
			return true;
		}
		if (typeof(NCard).IsAssignableFrom(declaringType))
		{
			bool flag;
			switch (target.Name)
			{
			case "Reload":
			case "UpdateVisuals":
			case "UpdatePortrait":
			case "_EnterTree":
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (!flag)
			{
				return HasVisualName(target.Name);
			}
			return true;
		}
		if (declaringType.Name.EndsWith("MerchantRoom", StringComparison.OrdinalIgnoreCase) || declaringType.Name.EndsWith("RestSiteRoom", StringComparison.OrdinalIgnoreCase))
		{
			string name = target.Name;
			if ((!(name == "_Ready") && !(name == "Ready")) || 1 == 0)
			{
				return HasVisualName(target.Name);
			}
			return true;
		}
		if (declaringType.Name.EndsWith("CombatRoom", StringComparison.OrdinalIgnoreCase) && target.Name.Equals("AddCreature", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (declaringType.Name.EndsWith("CombatManager", StringComparison.OrdinalIgnoreCase) && target.Name.StartsWith("EndCombat", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		string text = declaringType.Namespace ?? string.Empty;
		if (text.StartsWith("MegaCrit.Sts2.Core.Bindings.MegaSpine", StringComparison.Ordinal))
		{
			return true;
		}
		if (!text.StartsWith("MegaCrit.Sts2.Core.Nodes", StringComparison.Ordinal))
		{
			return false;
		}
		if (text.Contains(".Vfx", StringComparison.OrdinalIgnoreCase) || VisualTypeNameTokens.Any((string token) => declaringType.Name.Contains(token, StringComparison.OrdinalIgnoreCase)))
		{
			return true;
		}
		if (HasVisualName(target.Name))
		{
			if (!ReturnsVisualValue(target))
			{
				return target.GetParameters().Any((ParameterInfo parameter) => IsVisualValueType(parameter.ParameterType));
			}
			return true;
		}
		return false;
	}

	private static bool HasVisualName(string name)
	{
		return VisualNameTokens.Any((string token) => name.Contains(token, StringComparison.OrdinalIgnoreCase));
	}

	private static bool ReturnsVisualValue(MethodBase target)
	{
		if (target is MethodInfo methodInfo)
		{
			return IsVisualValueType(methodInfo.ReturnType);
		}
		return false;
	}

	private static bool IsVisualValueType(Type type)
	{
		if (type.IsByRef)
		{
			type = type.GetElementType();
		}
		if (!typeof(Texture2D).IsAssignableFrom(type) && !typeof(PackedScene).IsAssignableFrom(type))
		{
			return typeof(NCreatureVisuals).IsAssignableFrom(type);
		}
		return true;
	}

	private static string? GetProviderRoot(Patch patch, IReadOnlyList<string> roots, IReadOnlySet<string> preservedRoots)
	{
		string location = patch.PatchMethod.Module.Assembly.Location;
		if (string.IsNullOrWhiteSpace(location))
		{
			return null;
		}
		string assemblyPath = NormalizeRoot(location);
		if (assemblyPath == null)
		{
			return null;
		}
		string text = (from root in roots.Concat(preservedRoots)
			where assemblyPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
			orderby root.Length descending
			select root).FirstOrDefault();
		if (text == null || preservedRoots.Contains(text))
		{
			return null;
		}
		return text;
	}

	private static string? NormalizeRoot(string path)
	{
		try
		{
			return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}
		catch
		{
			return null;
		}
	}
}
