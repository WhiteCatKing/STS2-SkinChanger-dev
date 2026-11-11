using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch]
internal static class StatefulCardPlayCosmeticPatch
{
	[CompilerGenerated]
	private sealed class _003CTargetMethods_003Ed__0 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private MethodBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

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
		public _003CTargetMethods_003Ed__0(int _003C_003E1__state)
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
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = AccessTools.Method(typeof(Neurosurge), "OnPlay", (Type[])null, (Type[])null);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = AccessTools.Method(typeof(Clash), "OnPlay", (Type[])null, (Type[])null);
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
		IEnumerator<MethodBase> IEnumerable<MethodBase>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CTargetMethods_003Ed__0(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	[IteratorStateMachine(typeof(_003CTargetMethods_003Ed__0))]
	private static IEnumerable<MethodBase> TargetMethods()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTargetMethods_003Ed__0(-2);
	}

	private static void Prefix(CardModel __instance, CardPlay cardPlay)
	{
		StatefulCardArtRuntime.Runtime runtime = StatefulCardArtRuntime.For(__instance);
		if (runtime == null || !runtime.Enabled || runtime.Simple)
		{
			return;
		}
		try
		{
			if (__instance is Neurosurge && runtime.Setting("EnableNeurosurgeYippee", fallback: true))
			{
				AccessTools.Method(runtime.Assembly.GetType(runtime.Contract.SettingsType.Substring(0, runtime.Contract.SettingsType.LastIndexOf('.')) + ".Patches.NeurosurgeYippe"), "Prefix", (Type[])null, (Type[])null)?.Invoke(null, new object[1] { cardPlay });
			}
			else
			{
				if (!(__instance is Clash) || !runtime.Setting("EnableClashAsGrandFinale", fallback: true))
				{
					return;
				}
				NCombatRoom instance = NCombatRoom.Instance;
				if (instance != null)
				{
					NGrandFinaleVfx val = NGrandFinaleVfx.Create(__instance.Owner.Creature);
					if (val != null)
					{
						((Node)instance.CombatVfxContainer).AddChild((Node)(object)val, false, (InternalMode)0);
					}
				}
			}
		}
		catch (Exception exception)
		{
			StatefulCardArtRuntime.Warn("play cosmetic", exception);
		}
	}
}
