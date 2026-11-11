using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace ModLaunchManager;

internal static class HarmonyCallerAssembly
{
	[CompilerGenerated]
	private sealed class _003CTranspiler_003Ed__4 : IEnumerable<CodeInstruction>, IEnumerable, IEnumerator<CodeInstruction>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private CodeInstruction _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<CodeInstruction> instructions;

		public IEnumerable<CodeInstruction> _003C_003E3__instructions;

		private MethodInfo _003CgetAssembly_003E5__2;

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
		public _003CTranspiler_003Ed__4(int _003C_003E1__state)
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
			_003CgetAssembly_003E5__2 = null;
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
					_003CgetAssembly_003E5__2 = AccessTools.PropertyGetter(typeof(Type), "Assembly");
					_003Creplacement_003E5__3 = AccessTools.Method(typeof(HarmonyCallerAssembly), "ResolveCallerAssembly", (Type[])null, (Type[])null);
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
					if (CodeInstructionExtensions.Calls(current, _003CgetAssembly_003E5__2))
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
					throw new InvalidOperationException($"expected one implicit Harmony caller lookup, found {_003Creplacements_003E5__4}");
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
			_003CTranspiler_003Ed__4 _003CTranspiler_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CTranspiler_003Ed__ = this;
			}
			else
			{
				_003CTranspiler_003Ed__ = new _003CTranspiler_003Ed__4(0);
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

	[ThreadStatic]
	private static Assembly? _initializerAssembly;

	internal static void Install(Harmony harmony, MethodInfo initializerMethod)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_0060: Expected O, but got Unknown
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected O, but got Unknown
		MethodInfo methodInfo = AccessTools.Method(typeof(HarmonyCallerAssembly), "Transpiler", (Type[])null, (Type[])null);
		MethodInfo methodInfo2 = AccessTools.Method(typeof(HarmonyCallerAssembly), "EnterInitializer", (Type[])null, (Type[])null);
		MethodInfo methodInfo3 = AccessTools.Method(typeof(HarmonyCallerAssembly), "LeaveInitializer", (Type[])null, (Type[])null);
		List<MethodInfo> list = new List<MethodInfo>();
		try
		{
			harmony.Patch((MethodBase)initializerMethod, new HarmonyMethod(methodInfo2), (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(methodInfo3));
			(string, Type[])[] array = new(string, Type[])[4]
			{
				("PatchAll", Type.EmptyTypes),
				("PatchAllUncategorized", Type.EmptyTypes),
				("PatchCategory", new Type[1] { typeof(string) }),
				("UnpatchCategory", new Type[1] { typeof(string) })
			};
			for (int i = 0; i < array.Length; i++)
			{
				(string, Type[]) tuple = array[i];
				string item = tuple.Item1;
				Type[] item2 = tuple.Item2;
				MethodInfo methodInfo4 = AccessTools.DeclaredMethod(typeof(Harmony), item, item2, (Type[])null) ?? throw new MissingMethodException("Harmony", item);
				harmony.Patch((MethodBase)methodInfo4, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(methodInfo), (HarmonyMethod)null);
				list.Add(methodInfo4);
			}
		}
		catch
		{
			foreach (MethodInfo item3 in list)
			{
				harmony.Unpatch((MethodBase)item3, methodInfo);
			}
			harmony.Unpatch((MethodBase)initializerMethod, methodInfo2);
			harmony.Unpatch((MethodBase)initializerMethod, methodInfo3);
			throw;
		}
	}

	private static void EnterInitializer(Type initializerType, out Assembly? __state)
	{
		__state = _initializerAssembly;
		_initializerAssembly = initializerType.Assembly;
	}

	private static Exception? LeaveInitializer(Assembly? __state, Exception? __exception)
	{
		_initializerAssembly = __state;
		return __exception;
	}

	[IteratorStateMachine(typeof(_003CTranspiler_003Ed__4))]
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTranspiler_003Ed__4(-2)
		{
			_003C_003E3__instructions = instructions
		};
	}

	private static Assembly ResolveCallerAssembly(Type? originalCaller)
	{
		Assembly assembly = typeof(Harmony).Assembly;
		StackTrace stackTrace = new StackTrace(fNeedFileInfo: false);
		for (int i = 0; i < stackTrace.FrameCount; i++)
		{
			Type type = stackTrace.GetFrame(i)?.GetMethod()?.ReflectedType;
			if (!(type == null) && !(type.Assembly == assembly) && !(type == typeof(HarmonyCallerAssembly)))
			{
				if (type.Assembly == typeof(object).Assembly)
				{
					break;
				}
				return type.Assembly;
			}
		}
		return _initializerAssembly ?? originalCaller?.Assembly ?? throw new InvalidOperationException("could not identify the implicit Harmony caller assembly");
	}
}
