using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch]
internal static class StatefulCardTooltipPatch
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
				_003C_003E2__current = AccessTools.PropertyGetter(typeof(CardModel), "HoverTips");
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = AccessTools.PropertyGetter(typeof(CardModel), "ExtraHoverTips");
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

	private static void Postfix(CardModel __instance, MethodBase __originalMethod, ref IEnumerable<IHoverTip> __result)
	{
		StatefulCardArtRuntime.Runtime runtime = StatefulCardArtRuntime.For(__instance);
		if (runtime == null || !runtime.Enabled)
		{
			return;
		}
		try
		{
			object obj;
			if (!runtime.Setting("HideTooltips", fallback: true))
			{
				obj = __result;
			}
			else
			{
				IEnumerable<IHoverTip> enumerable = Array.Empty<IHoverTip>();
				obj = enumerable;
			}
			IEnumerable<IHoverTip> enumerable2 = (IEnumerable<IHoverTip>)obj;
			if (__originalMethod.Name == "get_HoverTips" && runtime.Setting("ShowCreditsTooltip", fallback: true))
			{
				enumerable2 = runtime.Credits(__instance).Concat(enumerable2);
			}
			__result = enumerable2.ToArray();
		}
		catch (Exception exception)
		{
			StatefulCardArtRuntime.Warn("tooltips", exception);
		}
	}
}
