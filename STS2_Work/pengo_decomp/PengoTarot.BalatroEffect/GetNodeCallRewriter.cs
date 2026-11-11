using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace PengoTarot.BalatroEffect;

internal static class GetNodeCallRewriter
{
	[CompilerGenerated]
	private sealed class _003CEnumerateMethods_003Ed__17 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private MethodBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Type type;

		public Type _003C_003E3__type;

		private ConstructorInfo[] _003C_003E7__wrap1;

		private int _003C_003E7__wrap2;

		private MethodInfo[] _003C_003E7__wrap3;

		MethodBase IEnumerator<MethodBase>.Current
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
		public _003CEnumerateMethods_003Ed__17(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E7__wrap1 = null;
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E7__wrap1 = type.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				_003C_003E7__wrap2 = 0;
				goto IL_0071;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E7__wrap2++;
				goto IL_0071;
			case 2:
				{
					_003C_003E1__state = -1;
					_003C_003E7__wrap2++;
					break;
				}
				IL_0071:
				if (_003C_003E7__wrap2 < _003C_003E7__wrap1.Length)
				{
					ConstructorInfo constructorInfo = _003C_003E7__wrap1[_003C_003E7__wrap2];
					_003C_003E2__current = constructorInfo;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E7__wrap1 = null;
				_003C_003E7__wrap3 = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				_003C_003E7__wrap2 = 0;
				break;
			}
			if (_003C_003E7__wrap2 < _003C_003E7__wrap3.Length)
			{
				MethodInfo methodInfo = _003C_003E7__wrap3[_003C_003E7__wrap2];
				_003C_003E2__current = methodInfo;
				_003C_003E1__state = 2;
				return true;
			}
			_003C_003E7__wrap3 = null;
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
		IEnumerator<MethodBase> IEnumerable<MethodBase>.GetEnumerator()
		{
			_003CEnumerateMethods_003Ed__17 _003CEnumerateMethods_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateMethods_003Ed__ = this;
			}
			else
			{
				_003CEnumerateMethods_003Ed__ = new _003CEnumerateMethods_003Ed__17(0);
			}
			_003CEnumerateMethods_003Ed__.type = _003C_003E3__type;
			return _003CEnumerateMethods_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	private static readonly MethodInfo GenericGetNodeDef;

	private static readonly MethodInfo NonGenericGetNode;

	private static readonly MethodInfo RedirectGenericMethod;

	private static readonly MethodInfo RedirectNonGenericMethod;

	private static readonly MethodInfo GenericGetNodeOrNullDef;

	private static readonly MethodInfo NonGenericGetNodeOrNull;

	private static readonly MethodInfo RedirectGenericOrNullMethod;

	private static readonly MethodInfo RedirectNonGenericOrNullMethod;

	private static Harmony? _harmony;

	private static bool _hasRun;

	private static int _deferAttempts;

	private static readonly ConcurrentDictionary<string, byte> LoggedSites;

	static GetNodeCallRewriter()
	{
		LoggedSites = new ConcurrentDictionary<string, byte>();
		GenericGetNodeDef = typeof(Node).GetMethods(BindingFlags.Instance | BindingFlags.Public).First((MethodInfo m) => m.IsGenericMethod && m.Name == "GetNode" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(NodePath));
		NonGenericGetNode = typeof(Node).GetMethods(BindingFlags.Instance | BindingFlags.Public).First((MethodInfo m) => !m.IsGenericMethod && m.Name == "GetNode" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(NodePath));
		RedirectGenericMethod = typeof(GetNodeCallRewriter).GetMethod("RedirectGetNodeGeneric", BindingFlags.Static | BindingFlags.NonPublic);
		RedirectNonGenericMethod = typeof(GetNodeCallRewriter).GetMethod("RedirectGetNode", BindingFlags.Static | BindingFlags.NonPublic);
		GenericGetNodeOrNullDef = typeof(Node).GetMethods(BindingFlags.Instance | BindingFlags.Public).First((MethodInfo m) => m.IsGenericMethod && m.Name == "GetNodeOrNull" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(NodePath));
		NonGenericGetNodeOrNull = typeof(Node).GetMethods(BindingFlags.Instance | BindingFlags.Public).First((MethodInfo m) => !m.IsGenericMethod && m.Name == "GetNodeOrNull" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(NodePath));
		RedirectGenericOrNullMethod = typeof(GetNodeCallRewriter).GetMethod("RedirectGetNodeOrNullGeneric", BindingFlags.Static | BindingFlags.NonPublic);
		RedirectNonGenericOrNullMethod = typeof(GetNodeCallRewriter).GetMethod("RedirectGetNodeOrNull", BindingFlags.Static | BindingFlags.NonPublic);
	}

	public static void ScheduleAfterAllModsLoaded()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Callable val = Callable.From((Action)RunDeferred);
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
		catch (Exception ex)
		{
			GD.PrintErr("[PengoTarot] Could not schedule GetNode rewrite pass: " + ex.Message);
		}
	}

	private static void RunDeferred()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (_hasRun)
		{
			return;
		}
		if ((int)ModManager.State != 1)
		{
			if (++_deferAttempts < 300)
			{
				Callable val = Callable.From((Action)RunDeferred);
				((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			}
			else
			{
				GD.PrintErr("[PengoTarot] ModManager never Initialized; GetNode rewrite skipped.");
			}
			return;
		}
		_hasRun = true;
		try
		{
			Run();
		}
		catch (Exception value)
		{
			GD.PrintErr($"[PengoTarot] GetNode rewrite pass failed: {value}");
		}
	}

	private static void Run()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		_harmony = new Harmony("com.pengotarot.getnoderewrite");
		Assembly assembly = typeof(GetNodeCallRewriter).Assembly;
		foreach (Mod mod in ModManager.Mods)
		{
			if ((int)mod.state != 1)
			{
				continue;
			}
			IEnumerable<Assembly> assemblies = mod.assemblies;
			foreach (Assembly item in assemblies ?? Enumerable.Empty<Assembly>())
			{
				if (!(item == null) && !(item == assembly))
				{
					ScanAssembly(item);
				}
			}
		}
	}

	private static void ScanAssembly(Assembly assembly)
	{
		Type[] array;
		try
		{
			array = assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			array = ex.Types.Where((Type t) => t != null).Cast<Type>().ToArray();
		}
		catch
		{
			return;
		}
		Type[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			foreach (MethodBase item in EnumerateMethods(array2[i]))
			{
				ScanMethod(item);
			}
		}
	}

	[IteratorStateMachine(typeof(_003CEnumerateMethods_003Ed__17))]
	private static IEnumerable<MethodBase> EnumerateMethods(Type type)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateMethods_003Ed__17(-2)
		{
			_003C_003E3__type = type
		};
	}

	private static void ScanMethod(MethodBase method)
	{
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		if (method.IsAbstract || method.ContainsGenericParameters)
		{
			return;
		}
		Type? declaringType = method.DeclaringType;
		if ((object)declaringType != null && declaringType.ContainsGenericParameters)
		{
			return;
		}
		byte[] array;
		try
		{
			array = method.GetMethodBody()?.GetILAsByteArray();
		}
		catch
		{
			return;
		}
		if (array == null)
		{
			return;
		}
		bool flag = false;
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			if (b == 111 || b == 40)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		List<KeyValuePair<OpCode, object>> list;
		try
		{
			list = PatchProcessor.ReadMethodBody(method).ToList();
		}
		catch
		{
			return;
		}
		bool flag2 = false;
		foreach (KeyValuePair<OpCode, object> item in list)
		{
			if ((item.Key == OpCodes.Callvirt || item.Key == OpCodes.Call) && item.Value is MethodInfo method2 && IsGetNodeLike(method2))
			{
				flag2 = true;
				break;
			}
		}
		if (!flag2)
		{
			return;
		}
		try
		{
			_harmony.Patch(method, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(GetNodeCallRewriter), "Transpiler", (Type[])null), (HarmonyMethod)null);
		}
		catch
		{
		}
	}

	private static bool IsGetNodeLike(MethodInfo method)
	{
		if (!method.IsGenericMethod)
		{
			if (!method.Equals(NonGenericGetNode))
			{
				return method.Equals(NonGenericGetNodeOrNull);
			}
			return true;
		}
		MethodInfo genericMethodDefinition = method.GetGenericMethodDefinition();
		if (!genericMethodDefinition.Equals(GenericGetNodeDef))
		{
			return genericMethodDefinition.Equals(GenericGetNodeOrNullDef);
		}
		return true;
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		List<CodeInstruction> list = instructions.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			CodeInstruction val = list[i];
			if ((!(val.opcode != OpCodes.Callvirt) || !(val.opcode != OpCodes.Call)) && val.operand is MethodInfo method)
			{
				MethodInfo methodInfo = ResolveRedirectTarget(method);
				if (!(methodInfo == null))
				{
					CodeInstruction val2 = new CodeInstruction(OpCodes.Call, (object)methodInfo);
					val2.labels.AddRange(val.labels);
					val2.blocks.AddRange(val.blocks);
					list[i] = val2;
				}
			}
		}
		return list;
	}

	private static MethodInfo? ResolveRedirectTarget(MethodInfo method)
	{
		if (method.IsGenericMethod)
		{
			MethodInfo genericMethodDefinition = method.GetGenericMethodDefinition();
			if (genericMethodDefinition.Equals(GenericGetNodeDef))
			{
				return RedirectGenericMethod.MakeGenericMethod(method.GetGenericArguments());
			}
			if (genericMethodDefinition.Equals(GenericGetNodeOrNullDef))
			{
				return RedirectGenericOrNullMethod.MakeGenericMethod(method.GetGenericArguments());
			}
			return null;
		}
		if (method.Equals(NonGenericGetNode))
		{
			return RedirectNonGenericMethod;
		}
		if (method.Equals(NonGenericGetNodeOrNull))
		{
			return RedirectNonGenericOrNullMethod;
		}
		return null;
	}

	private static Node RedirectGetNode(Node instance, NodePath path)
	{
		return RedirectGetNodeInternal<object>(instance, path, generic: false) ?? instance.GetNode(path);
	}

	private static T RedirectGetNodeGeneric<T>(Node instance, NodePath path) where T : class
	{
		if (RedirectGetNodeInternal<T>(instance, path, generic: true) is T result)
		{
			return result;
		}
		return instance.GetNode<T>(path);
	}

	private static Node? RedirectGetNodeOrNull(Node instance, NodePath path)
	{
		return RedirectGetNodeOrNullInternal(instance, path) ?? instance.GetNodeOrNull(path);
	}

	private static T? RedirectGetNodeOrNullGeneric<T>(Node instance, NodePath path) where T : class
	{
		if (RedirectGetNodeOrNullInternal(instance, path) is T result)
		{
			return result;
		}
		return instance.GetNodeOrNull<T>(path);
	}

	private static Node? RedirectGetNodeOrNullInternal(Node instance, NodePath path)
	{
		string text = ((object)path).ToString();
		if (text.Contains('/'))
		{
			return null;
		}
		if (!(instance is NCard) && !(instance.GetParent() is NCard))
		{
			return null;
		}
		string text2 = (text.StartsWith("%") ? text.Substring(1) : text);
		Node val = instance.FindChild(text2, true, false);
		string callerDescription = GetCallerDescription();
		string key = callerDescription + "|or-null:" + text;
		if (val != null && LoggedSites.TryAdd(key, 0))
		{
			string value = (val.IsInsideTree() ? ((object)val.GetPath()).ToString() : "(not in tree)");
			GD.Print($"[PengoTarot] GetNodeOrNull{{\"{text}\"}} from [{callerDescription}] -> found '{val.Name}' at {value}");
		}
		return val;
	}

	private static Node? RedirectGetNodeInternal<T>(Node instance, NodePath path, bool generic)
	{
		if (!(instance is NCard) && !(instance.GetParent() is NCard))
		{
			return null;
		}
		string text = ((object)path).ToString();
		if (text.Contains('/'))
		{
			return null;
		}
		string text2 = text;
		if (text2.StartsWith("%"))
		{
			text2 = text2.Substring(1);
		}
		Node val = instance.FindChild(text2, true, false);
		string callerDescription = GetCallerDescription();
		string key = callerDescription + "|" + text;
		if (LoggedSites.TryAdd(key, 0))
		{
			if (val != null)
			{
				string value = (val.IsInsideTree() ? ((object)val.GetPath()).ToString() : "(not in tree)");
				GD.Print($"[PengoTarot] GetNode{{\"{text}\"}} from [{callerDescription}] -> found '{val.Name}' at {value}");
			}
			else
			{
				GD.Print($"[PengoTarot] GetNode{{\"{text}\"}} from [{callerDescription}] -> NOT FOUND on card body. Triggering emergency throttle.");
				if (!Config.IsPerformanceThrottled)
				{
					Config.SetPerformanceThrottled(value: true);
				}
			}
		}
		return val;
	}

	private static string GetCallerDescription()
	{
		StackTrace stackTrace = new StackTrace(3, fNeedFileInfo: false);
		for (int i = 0; i < stackTrace.FrameCount; i++)
		{
			MethodBase methodBase = stackTrace.GetFrame(i)?.GetMethod();
			if (!(methodBase == null) && !(methodBase.DeclaringType == typeof(GetNodeCallRewriter)))
			{
				string value = methodBase.DeclaringType?.Assembly.GetName().Name ?? "?";
				string value2 = methodBase.DeclaringType?.FullName ?? "?";
				return $"{value} | {value2}.{methodBase.Name}";
			}
		}
		return "unknown";
	}
}
