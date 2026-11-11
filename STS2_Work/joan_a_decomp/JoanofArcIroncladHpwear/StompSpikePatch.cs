using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch]
internal static class StompSpikePatch
{
	[CompilerGenerated]
	private sealed class _003CTranspiler_003Ed__1 : IEnumerable<CodeInstruction>, IEnumerable, IEnumerator<CodeInstruction>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private CodeInstruction _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<CodeInstruction> instructions;

		public IEnumerable<CodeInstruction> _003C_003E3__instructions;

		private MethodInfo _003Creplacement_003E5__2;

		private FieldInfo _003CcardField_003E5__3;

		private int _003Ccount_003E5__4;

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
		public _003CTranspiler_003Ed__1(int _003C_003E1__state)
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
			case 3:
			case 4:
			case 5:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally2();
				}
				break;
			}
			_003Creplacement_003E5__2 = null;
			_003CcardField_003E5__3 = null;
			_003C_003E7__wrap4 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Expected O, but got Unknown
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01eb: Expected O, but got Unknown
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Expected O, but got Unknown
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003Creplacement_003E5__2 = AccessTools.Method(typeof(StompSpikePatch), "CreateSpike", (Type[])null, (Type[])null);
					_003CcardField_003E5__3 = AccessTools.Field(TargetMethod().DeclaringType, "<>4__this");
					if (_003Creplacement_003E5__2 == null || _003CcardField_003E5__3 == null)
					{
						ModEntry.Defer("stomp spike patch: replacement or card field missing — spikes left native.");
						_003C_003E7__wrap4 = instructions.GetEnumerator();
						_003C_003E1__state = -3;
						goto IL_00d3;
					}
					_003Ccount_003E5__4 = 0;
					_003C_003E7__wrap4 = instructions.GetEnumerator();
					_003C_003E1__state = -4;
					break;
				case 1:
					_003C_003E1__state = -3;
					goto IL_00d3;
				case 2:
					_003C_003E1__state = -4;
					break;
				case 3:
					_003C_003E1__state = -4;
					_003C_003E2__current = new CodeInstruction(OpCodes.Ldfld, (object)_003CcardField_003E5__3);
					_003C_003E1__state = 4;
					return true;
				case 4:
					_003C_003E1__state = -4;
					_003C_003E2__current = new CodeInstruction(OpCodes.Call, (object)_003Creplacement_003E5__2);
					_003C_003E1__state = 5;
					return true;
				case 5:
					{
						_003C_003E1__state = -4;
						break;
					}
					IL_00d3:
					if (_003C_003E7__wrap4.MoveNext())
					{
						CodeInstruction current = _003C_003E7__wrap4.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap4 = null;
					return false;
				}
				if (_003C_003E7__wrap4.MoveNext())
				{
					CodeInstruction current2 = _003C_003E7__wrap4.Current;
					if (!IsSpikeFactory(current2))
					{
						_003C_003E2__current = current2;
						_003C_003E1__state = 2;
						return true;
					}
					_003Ccount_003E5__4++;
					CodeInstruction val = new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					val.labels.AddRange(current2.labels);
					current2.labels.Clear();
					_003C_003E2__current = val;
					_003C_003E1__state = 3;
					return true;
				}
				_003C_003Em__Finally2();
				_003C_003E7__wrap4 = null;
				if (_003Ccount_003E5__4 != 1)
				{
					ModEntry.Defer($"stomp spike patch: expected 1 spike factory, found {_003Ccount_003E5__4} — left native.");
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

		private void _003C_003Em__Finally2()
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
			_003CTranspiler_003Ed__1 _003CTranspiler_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CTranspiler_003Ed__ = this;
			}
			else
			{
				_003CTranspiler_003Ed__ = new _003CTranspiler_003Ed__1(0);
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

	internal static MethodBase TargetMethod()
	{
		return AccessTools.DeclaredMethod(typeof(Stomp), "OnPlay", (Type[])null, (Type[])null).GetCustomAttribute<AsyncStateMachineAttribute>().StateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	[IteratorStateMachine(typeof(_003CTranspiler_003Ed__1))]
	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTranspiler_003Ed__1(-2)
		{
			_003C_003E3__instructions = instructions
		};
	}

	internal static bool IsSpikeFactory(CodeInstruction instruction)
	{
		if (instruction.opcode != OpCodes.Call && instruction.opcode != OpCodes.Callvirt)
		{
			return false;
		}
		if (!(instruction.operand is MethodBase methodBase))
		{
			return false;
		}
		if (methodBase.Name == "Create")
		{
			return methodBase.DeclaringType?.Name == "NSpikeSplashVfx";
		}
		return false;
	}

	private static NSpikeSplashVfx? CreateSpike(Creature target, VfxColor color, Stomp card)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (((CardModel)card).Owner.Character is Ironclad && HeavyAttackFx.IsUsingThisSkin(((CardModel)card).Owner.Creature))
		{
			return null;
		}
		return NSpikeSplashVfx.Create(target, color);
	}
}
