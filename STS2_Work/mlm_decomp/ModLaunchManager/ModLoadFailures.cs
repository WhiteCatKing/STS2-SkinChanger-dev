using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class ModLoadFailures
{
	internal enum Cause
	{
		None,
		VersionMismatch,
		MissingDependency,
		VersionOrDependency
	}

	internal sealed record Failure(string TypeName, string Message, Cause Cause, string? Missing);

	[CompilerGenerated]
	private sealed class _003CChain_003Ed__17 : IEnumerable<Exception>, IEnumerable, IEnumerator<Exception>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Exception _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Exception error;

		public Exception _003C_003E3__error;

		private Queue<Exception> _003Cpending_003E5__2;

		private HashSet<Exception> _003Cseen_003E5__3;

		private Exception _003Cexception_003E5__4;

		Exception IEnumerator<Exception>.Current
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
		public _003CChain_003Ed__17(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cpending_003E5__2 = null;
			_003Cseen_003E5__3 = null;
			_003Cexception_003E5__4 = null;
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
				_003Cpending_003E5__2 = new Queue<Exception>(new _003C_003Ez__ReadOnlySingleElementList<Exception>(error));
				_003Cseen_003E5__3 = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
				break;
			case 1:
				_003C_003E1__state = -1;
				if (_003Cexception_003E5__4.InnerException != null)
				{
					_003Cpending_003E5__2.Enqueue(_003Cexception_003E5__4.InnerException);
				}
				if (_003Cexception_003E5__4 is ReflectionTypeLoadException ex)
				{
					Exception[] loaderExceptions = ex.LoaderExceptions;
					foreach (Exception ex2 in loaderExceptions)
					{
						if (ex2 != null)
						{
							_003Cpending_003E5__2.Enqueue(ex2);
						}
					}
				}
				else if (_003Cexception_003E5__4 is AggregateException ex3)
				{
					foreach (Exception innerException in ex3.InnerExceptions)
					{
						_003Cpending_003E5__2.Enqueue(innerException);
					}
				}
				_003Cexception_003E5__4 = null;
				break;
			}
			while (_003Cpending_003E5__2.Count > 0 && _003Cseen_003E5__3.Count < 32)
			{
				_003Cexception_003E5__4 = _003Cpending_003E5__2.Dequeue();
				if (_003Cseen_003E5__3.Add(_003Cexception_003E5__4))
				{
					_003C_003E2__current = _003Cexception_003E5__4;
					_003C_003E1__state = 1;
					return true;
				}
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
		IEnumerator<Exception> IEnumerable<Exception>.GetEnumerator()
		{
			_003CChain_003Ed__17 _003CChain_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CChain_003Ed__ = this;
			}
			else
			{
				_003CChain_003Ed__ = new _003CChain_003Ed__17(0);
			}
			_003CChain_003Ed__.error = _003C_003E3__error;
			return _003CChain_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Exception>)this).GetEnumerator();
		}
	}

	private const int MaxMessageLength = 400;

	private static readonly ConditionalWeakTable<Assembly, Failure> Failures = new ConditionalWeakTable<Assembly, Failure>();

	[ThreadStatic]
	private static int _depth;

	[ThreadStatic]
	private static Exception? _lastThrown;

	private static bool _listening;

	internal static void Install(Harmony harmony, MethodInfo initializerMethod)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_003f: Expected O, but got Unknown
		harmony.Patch((MethodBase)initializerMethod, new HarmonyMethod(AccessTools.Method(typeof(ModLoadFailures), "EnterInitializer", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(typeof(ModLoadFailures), "LeaveInitializer", (Type[])null, (Type[])null)));
		if (!_listening)
		{
			AppDomain.CurrentDomain.FirstChanceException += Remember;
			_listening = true;
		}
	}

	internal static Failure? Find(IEnumerable<Assembly> assemblies)
	{
		foreach (Assembly assembly in assemblies)
		{
			if (Failures.TryGetValue(assembly, out Failure value))
			{
				return value;
			}
		}
		return null;
	}

	internal static Failure Describe(Exception error)
	{
		error = Unwrap(error);
		var (cause, missing) = Classify(error);
		return new Failure(error.GetType().Name, Summarize(error), cause, missing);
	}

	internal static Cause ClassifyTypeName(string? typeName)
	{
		string text = typeName?.Substring(typeName.LastIndexOf('.') + 1) ?? string.Empty;
		if (text != null)
		{
			int length = text.Length;
			if (length <= 21)
			{
				if (length != 17)
				{
					if (length == 21)
					{
						char c = text[0];
						if (c != 'F')
						{
							if (c == 'M' && text == "MissingFieldException")
							{
								goto IL_0125;
							}
						}
						else if (text == "FileNotFoundException")
						{
							goto IL_0129;
						}
					}
				}
				else
				{
					char c = text[0];
					if (c != 'F')
					{
						if (c == 'T' && text == "TypeLoadException")
						{
							goto IL_0125;
						}
					}
					else if (text == "FileLoadException")
					{
						goto IL_0129;
					}
				}
			}
			else if (length != 22)
			{
				if (length == 27)
				{
					char c = text[0];
					if (c != 'E')
					{
						if (c == 'R' && text == "ReflectionTypeLoadException")
						{
							goto IL_0129;
						}
					}
					else if (text == "EntryPointNotFoundException")
					{
						goto IL_0125;
					}
				}
			}
			else
			{
				char c = text[9];
				if (c != 'm')
				{
					if (c == 't' && text == "MissingMethodException")
					{
						goto IL_0125;
					}
				}
				else if (text == "MissingMemberException")
				{
					goto IL_0125;
				}
			}
		}
		return Cause.None;
		IL_0125:
		return Cause.VersionMismatch;
		IL_0129:
		return Cause.VersionOrDependency;
	}

	private static void EnterInitializer(out Exception? __state)
	{
		__state = _lastThrown;
		_lastThrown = null;
		_depth++;
	}

	private static void LeaveInitializer(Type initializerType, bool __result, Exception? __exception, Exception? __state)
	{
		_depth--;
		Exception ex = __exception ?? (__result ? null : _lastThrown);
		_lastThrown = __state;
		if (ex != null)
		{
			Failures.AddOrUpdate(initializerType.Assembly, Describe(ex));
		}
	}

	private static void Remember(object? sender, FirstChanceExceptionEventArgs args)
	{
		if (_depth > 0)
		{
			_lastThrown = args.Exception;
		}
	}

	private static Exception Unwrap(Exception error)
	{
		while (true)
		{
			Exception ex2;
			if (!(error is TargetInvocationException) && !(error is TypeInitializationException))
			{
				if (error is AggregateException ex)
				{
					ReadOnlyCollection<Exception> innerExceptions = ex.InnerExceptions;
					if (innerExceptions != null && innerExceptions.Count == 1)
					{
						ex2 = ex.InnerExceptions[0];
						goto IL_0049;
					}
				}
				ex2 = null;
			}
			else
			{
				ex2 = error.InnerException;
			}
			goto IL_0049;
			IL_0049:
			Exception ex3 = ex2;
			if (ex3 == null)
			{
				break;
			}
			error = ex3;
		}
		return error;
	}

	private static (Cause Cause, string? Missing) Classify(Exception error)
	{
		bool flag = false;
		bool flag2 = false;
		string text = null;
		foreach (Exception item in Chain(error))
		{
			Exception current;
			Exception ex = (current = item);
			if (current is FileNotFoundException ex2)
			{
				string fileName = ex2.FileName;
				if (fileName != null)
				{
					string text2 = AssemblyName(fileName);
					if (text2 != null && text == null)
					{
						text = text2;
					}
				}
			}
			else if (current is FileLoadException ex3)
			{
				string fileName2 = ex3.FileName;
				if (fileName2 != null)
				{
					string text3 = AssemblyName(fileName2);
					if (text3 != null && text == null)
					{
						text = text3;
					}
				}
			}
			else if (!(current is MissingMemberException) && !(current is TypeLoadException))
			{
				if (current is ReflectionTypeLoadException)
				{
					flag2 = true;
				}
			}
			else
			{
				flag = true;
			}
			if (ex.Message.Contains("Undefined target method", StringComparison.Ordinal))
			{
				flag = true;
			}
		}
		if (text != null)
		{
			return (Cause: Cause.MissingDependency, Missing: text);
		}
		if (flag)
		{
			return (Cause: Cause.VersionMismatch, Missing: null);
		}
		return (Cause: flag2 ? Cause.VersionOrDependency : Cause.None, Missing: null);
	}

	private static string? AssemblyName(string file)
	{
		if (!file.Contains(", Version=", StringComparison.Ordinal) && !file.Contains(", Culture=", StringComparison.Ordinal))
		{
			return null;
		}
		try
		{
			return new AssemblyName(file).Name;
		}
		catch (Exception)
		{
			return null;
		}
	}

	[IteratorStateMachine(typeof(_003CChain_003Ed__17))]
	private static IEnumerable<Exception> Chain(Exception error)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CChain_003Ed__17(-2)
		{
			_003C_003E3__error = error
		};
	}

	private static string Summarize(Exception error)
	{
		List<string> list = new List<string>();
		foreach (Exception item in Chain(error))
		{
			string text = item.Message.Split('\n', 2)[0].Trim();
			if (text.Length > 0 && !list.Contains(text))
			{
				list.Add(text);
			}
			if (list.Count == 3)
			{
				break;
			}
		}
		string text2 = string.Join(" → ", list);
		if (text2.Length > 400)
		{
			return text2.Substring(0, 399) + "…";
		}
		return text2;
	}

	internal static bool Install(Harmony harmony)
	{
		try
		{
			Install(harmony, AccessTools.Method(typeof(ModManager), "CallModInitializer", new Type[1] { typeof(Type) }, (Type[])null) ?? throw new MissingMethodException("ModManager", "CallModInitializer"));
			return true;
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} load errors will not say why a mod failed to start: {ex.GetType().Name} {ex.Message}");
			return false;
		}
	}
}
