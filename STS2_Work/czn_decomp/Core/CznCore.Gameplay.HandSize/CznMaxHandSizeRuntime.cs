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
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Gameplay.HandSize;

public static class CznMaxHandSizeRuntime
{
	[CompilerGenerated]
	private sealed class _003CHandLimitTranspiler_003Ed__18 : IEnumerable<CodeInstruction>, IEnumerable, IEnumerator<CodeInstruction>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private CodeInstruction _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private MethodBase __originalMethod;

		public MethodBase _003C_003E3____originalMethod;

		private IEnumerable<CodeInstruction> instructions;

		public IEnumerable<CodeInstruction> _003C_003E3__instructions;

		private MethodInfo _003Cgetter_003E5__2;

		private CodeInstruction[] _003CloadPlayer_003E5__3;

		private bool _003Cpatched_003E5__4;

		private IEnumerator<CodeInstruction> _003C_003E7__wrap4;

		private CodeInstruction _003Cinstruction_003E5__6;

		private CodeInstruction[] _003C_003E7__wrap6;

		private int _003C_003E7__wrap7;

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
		public _003CHandLimitTranspiler_003Ed__18(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || (uint)(num - 1) <= 2u)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003Cgetter_003E5__2 = null;
			_003CloadPlayer_003E5__3 = null;
			_003C_003E7__wrap4 = null;
			_003Cinstruction_003E5__6 = null;
			_003C_003E7__wrap6 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Expected O, but got Unknown
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003Cgetter_003E5__2 = AccessTools.PropertyGetter(typeof(CardPile), "MaxCardsInHand");
					_003CloadPlayer_003E5__3 = LoadPlayer(__originalMethod);
					_003Cpatched_003E5__4 = false;
					_003C_003E7__wrap4 = instructions.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					if (!CodeInstructionExtensions.Calls(_003Cinstruction_003E5__6, _003Cgetter_003E5__2))
					{
						break;
					}
					_003C_003E7__wrap6 = _003CloadPlayer_003E5__3;
					_003C_003E7__wrap7 = 0;
					goto IL_0117;
				case 2:
					_003C_003E1__state = -3;
					_003C_003E7__wrap7++;
					goto IL_0117;
				case 3:
					{
						_003C_003E1__state = -3;
						_003Cpatched_003E5__4 = true;
						_003Cinstruction_003E5__6 = null;
						break;
					}
					IL_0117:
					if (_003C_003E7__wrap7 < _003C_003E7__wrap6.Length)
					{
						CodeInstruction val = _003C_003E7__wrap6[_003C_003E7__wrap7];
						_003C_003E2__current = val.Clone();
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003E7__wrap6 = null;
					_003C_003E2__current = new CodeInstruction(OpCodes.Call, (object)ApplyModifiersMethod);
					_003C_003E1__state = 3;
					return true;
				}
				if (_003C_003E7__wrap4.MoveNext())
				{
					_003Cinstruction_003E5__6 = _003C_003E7__wrap4.Current;
					_003C_003E2__current = _003Cinstruction_003E5__6;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap4 = null;
				if (!_003Cpatched_003E5__4)
				{
					throw new InvalidOperationException("No STS2 v0.111 hand-limit read found in " + GeneralExtensions.FullDescription(__originalMethod) + ".");
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
			_003CHandLimitTranspiler_003Ed__18 _003CHandLimitTranspiler_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CHandLimitTranspiler_003Ed__ = this;
			}
			else
			{
				_003CHandLimitTranspiler_003Ed__ = new _003CHandLimitTranspiler_003Ed__18(0);
			}
			_003CHandLimitTranspiler_003Ed__.instructions = _003C_003E3__instructions;
			_003CHandLimitTranspiler_003Ed__.__originalMethod = _003C_003E3____originalMethod;
			return _003CHandLimitTranspiler_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<CodeInstruction>)this).GetEnumerator();
		}
	}

	public const int DefaultMaxHandSize = 10;

	private const string RegistryKey = "CznCore.Gameplay.HandSize.Modifiers.v1";

	private const string PatchGateKey = "CznCore.Gameplay.HandSize.Patched.v1";

	private static readonly MethodInfo HandLimitTranspilerMethod = RequireOwnMethod("HandLimitTranspiler");

	private static readonly MethodInfo ApplyModifiersMethod = RequireOwnMethod("ApplyModifiers");

	private static readonly MethodInfo StartCardPlayTranspilerMethod = RequireOwnMethod("StartCardPlayTranspiler");

	private static readonly MethodInfo GetPositionPrefixMethod = RequireOwnMethod("GetPositionPrefix");

	private static readonly MethodInfo GetAnglePrefixMethod = RequireOwnMethod("GetAnglePrefix");

	private static readonly MethodInfo GetScalePrefixMethod = RequireOwnMethod("GetScalePrefix");

	private static readonly MethodInfo GetShortcutOrDefaultMethod = RequireOwnMethod("GetShortcutOrDefault");

	public static void RegisterModifier(string id, Func<Player, int, int> modifier)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id, "id");
		ArgumentNullException.ThrowIfNull(modifier, "modifier");
		GetRegistry()[id] = modifier;
	}

	public static int Calculate(Player player)
	{
		ArgumentNullException.ThrowIfNull(player, "player");
		return ApplyModifiers(CardPile.MaxCardsInHand, player);
	}

	public static void EnsurePatched(Harmony harmony)
	{
		ArgumentNullException.ThrowIfNull(harmony, "harmony");
		object data = AppDomain.CurrentDomain.GetData("CznCore.Gameplay.HandSize.Patched.v1");
		if (!(data is bool) || !(bool)data)
		{
			PatchNativeHandLimitSites(harmony);
			AppDomain.CurrentDomain.SetData("CznCore.Gameplay.HandSize.Patched.v1", true);
		}
	}

	private static ConcurrentDictionary<string, Func<Player, int, int>> GetRegistry()
	{
		if (AppDomain.CurrentDomain.GetData("CznCore.Gameplay.HandSize.Modifiers.v1") is ConcurrentDictionary<string, Func<Player, int, int>> result)
		{
			return result;
		}
		ConcurrentDictionary<string, Func<Player, int, int>> concurrentDictionary = new ConcurrentDictionary<string, Func<Player, int, int>>(StringComparer.Ordinal);
		AppDomain.CurrentDomain.SetData("CznCore.Gameplay.HandSize.Modifiers.v1", concurrentDictionary);
		return concurrentDictionary;
	}

	private static int ApplyModifiers(int baseLimit, Player player)
	{
		int num = baseLimit;
		foreach (KeyValuePair<string, Func<Player, int, int>> item in GetRegistry().OrderBy<KeyValuePair<string, Func<Player, int, int>>, string>((KeyValuePair<string, Func<Player, int, int>> pair) => pair.Key, StringComparer.Ordinal))
		{
			num = item.Value(player, num);
		}
		return Math.Max(0, num);
	}

	private static void PatchNativeHandLimitSites(Harmony harmony)
	{
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Expected O, but got Unknown
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Expected O, but got Unknown
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Expected O, but got Unknown
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Expected O, but got Unknown
		PatchHandLimit(harmony, AccessTools.Method(typeof(CardPileCmd), "CheckIfDrawIsPossibleAndShowThoughtBubbleIfNot", new Type[1] { typeof(Player) }, (Type[])null));
		PatchHandLimit(harmony, AccessTools.AsyncMoveNext((MethodBase)FindSetupPlayerTurn()));
		PatchHandLimit(harmony, AccessTools.AsyncMoveNext((MethodBase)AccessTools.Method(typeof(CardPileCmd), "DrawInternal", new Type[4]
		{
			typeof(PlayerChoiceContext),
			typeof(decimal),
			typeof(Player),
			typeof(bool)
		}, (Type[])null)));
		PatchHandLimit(harmony, AccessTools.AsyncMoveNext((MethodBase)AccessTools.Method(typeof(CardPileCmd), "Add", new Type[6]
		{
			typeof(IEnumerable<CardModel>),
			typeof(CardPile),
			typeof(CardPilePosition),
			typeof(AbstractModel),
			typeof(bool),
			typeof(bool)
		}, (Type[])null)));
		PatchHandLimit(harmony, AccessTools.Method(typeof(CardConsoleCmd), "Process", new Type[2]
		{
			typeof(Player),
			typeof(string[])
		}, (Type[])null));
		Type[] array = new Type[6]
		{
			typeof(Scrawl),
			typeof(Anointed),
			typeof(Dredge),
			typeof(NeowsFury),
			typeof(CrashLanding),
			typeof(Pillage)
		};
		foreach (Type type in array)
		{
			PatchHandLimit(harmony, AccessTools.AsyncMoveNext((MethodBase)AccessTools.Method(type, "OnPlay", new Type[2]
			{
				typeof(PlayerChoiceContext),
				typeof(CardPlay)
			}, (Type[])null)));
		}
		harmony.Patch((MethodBase)AccessTools.Method(typeof(NPlayerHand), "StartCardPlay", new Type[2]
		{
			typeof(NHandCardHolder),
			typeof(bool)
		}, (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(StartCardPlayTranspilerMethod)
		{
			priority = 0
		}, (HarmonyMethod)null);
		harmony.Patch((MethodBase)AccessTools.Method(typeof(HandPosHelper), "GetPosition", new Type[2]
		{
			typeof(int),
			typeof(int)
		}, (Type[])null), new HarmonyMethod(GetPositionPrefixMethod), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		harmony.Patch((MethodBase)AccessTools.Method(typeof(HandPosHelper), "GetAngle", new Type[2]
		{
			typeof(int),
			typeof(int)
		}, (Type[])null), new HarmonyMethod(GetAnglePrefixMethod), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		harmony.Patch((MethodBase)AccessTools.Method(typeof(HandPosHelper), "GetScale", new Type[1] { typeof(int) }, (Type[])null), new HarmonyMethod(GetScalePrefixMethod), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
	}

	private static void PatchHandLimit(Harmony harmony, MethodInfo? target)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if (target == null)
		{
			throw new MissingMethodException("STS2 v0.111 maximum-hand-size patch target was not found.");
		}
		harmony.Patch((MethodBase)target, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(HandLimitTranspilerMethod)
		{
			priority = 800
		}, (HarmonyMethod)null);
	}

	private static MethodInfo? FindSetupPlayerTurn()
	{
		return typeof(CombatManager).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).SingleOrDefault(delegate(MethodInfo method)
		{
			if (method.Name != "SetupPlayerTurn")
			{
				return false;
			}
			ParameterInfo[] parameters = method.GetParameters();
			return parameters.Length == 3 && parameters[1].ParameterType == typeof(Player) && parameters[2].ParameterType == typeof(HookPlayerChoiceContext);
		});
	}

	[IteratorStateMachine(typeof(_003CHandLimitTranspiler_003Ed__18))]
	private static IEnumerable<CodeInstruction> HandLimitTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CHandLimitTranspiler_003Ed__18(-2)
		{
			_003C_003E3__instructions = instructions,
			_003C_003E3____originalMethod = __originalMethod
		};
	}

	private static CodeInstruction[] LoadPlayer(MethodBase method)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected O, but got Unknown
		ParameterInfo parameterInfo = method.GetParameters().SingleOrDefault((ParameterInfo parameter) => parameter.ParameterType == typeof(Player));
		if (parameterInfo == null)
		{
			FieldInfo[] fields = method.DeclaringType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			FieldInfo fieldInfo = fields.SingleOrDefault((FieldInfo field) => field.FieldType == typeof(Player));
			if (!(fieldInfo != null))
			{
				FieldInfo fieldInfo2 = fields.Single((FieldInfo field) => typeof(CardModel).IsAssignableFrom(field.FieldType));
				return (CodeInstruction[])(object)new CodeInstruction[3]
				{
					new CodeInstruction(OpCodes.Ldarg_0, (object)null),
					new CodeInstruction(OpCodes.Ldfld, (object)fieldInfo2),
					new CodeInstruction(OpCodes.Callvirt, (object)AccessTools.PropertyGetter(typeof(CardModel), "Owner"))
				};
			}
			return (CodeInstruction[])(object)new CodeInstruction[2]
			{
				new CodeInstruction(OpCodes.Ldarg_0, (object)null),
				new CodeInstruction(OpCodes.Ldfld, (object)fieldInfo)
			};
		}
		return (CodeInstruction[])(object)new CodeInstruction[1] { CodeInstruction.LoadArgument(parameterInfo.Position + ((!method.IsStatic) ? 1 : 0), false) };
	}

	private static IEnumerable<CodeInstruction> StartCardPlayTranspiler(IEnumerable<CodeInstruction> instructions)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		List<CodeInstruction> list = instructions.ToList();
		FieldInfo shortcutsField = AccessTools.Field(typeof(NPlayerHand), "_selectCardShortcuts");
		FieldInfo fieldInfo = AccessTools.Field(typeof(NPlayerHand), "_draggedHolderIndex");
		bool flag = false;
		for (int i = 4; i < list.Count; i++)
		{
			if (!(list[i].opcode != OpCodes.Ldelem_Ref) && CodeInstructionExtensions.LoadsField(list[i - 3], shortcutsField, false) && CodeInstructionExtensions.LoadsField(list[i - 1], fieldInfo, false))
			{
				CodeInstruction val = new CodeInstruction(OpCodes.Call, (object)GetShortcutOrDefaultMethod);
				val.labels.AddRange(list[i].labels);
				val.blocks.AddRange(list[i].blocks);
				list[i] = val;
				flag = true;
			}
		}
		if (!flag && !list.Any((CodeInstruction instruction) => CodeInstructionExtensions.Calls(instruction, GetShortcutOrDefaultMethod)) && list.Any((CodeInstruction instruction) => CodeInstructionExtensions.LoadsField(instruction, shortcutsField, false)))
		{
			GD.PushWarning("[CznCore.HandSize] NPlayerHand shortcut pattern was not found; cards above slot 10 may not support mouse drag.");
		}
		return list;
	}

	private static StringName GetShortcutOrDefault(StringName[] shortcuts, int index)
	{
		if (index < 0 || index >= shortcuts.Length)
		{
			return MegaInput.down;
		}
		return shortcuts[index];
	}

	private static bool GetPositionPrefix(int handSize, int cardIndex, ref Vector2 __result)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (handSize <= 10)
		{
			return true;
		}
		ValidateCardIndex(handSize, cardIndex);
		float halfSpread = GetHalfSpread(handSize);
		float edgeLift = GetEdgeLift(handSize);
		float num = ((handSize <= 1) ? 0f : (2f * (float)cardIndex / ((float)handSize - 1f) - 1f));
		__result = new Vector2(halfSpread * num, Math.Min(18f, -64f + edgeLift * num * num));
		return false;
	}

	private static bool GetAnglePrefix(int handSize, int cardIndex, ref float __result)
	{
		if (handSize <= 10)
		{
			return true;
		}
		ValidateCardIndex(handSize, cardIndex);
		float num = ((handSize <= 1) ? 0f : (2f * (float)cardIndex / ((float)handSize - 1f) - 1f));
		float num2 = Mathf.RadToDeg(Mathf.Atan2(2f * GetEdgeLift(handSize) * num, Math.Max(1f, GetHalfSpread(handSize))));
		__result = Mathf.Clamp(num2, -18f, 18f);
		return false;
	}

	private static bool GetScalePrefix(int handSize, ref Vector2 __result)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (handSize <= 10)
		{
			return true;
		}
		float val = 0.64f * MathF.Pow(0.95f, handSize - 11);
		__result = Vector2.One * Math.Max(0.48f, val);
		return false;
	}

	private static float GetHalfSpread(int handSize)
	{
		float num = Mathf.Clamp((float)(Math.Min(handSize, 14) - 10) / 4f, 0f, 1f);
		return Mathf.Lerp(610f, 690f, num);
	}

	private static float GetEdgeLift(int handSize)
	{
		return Math.Max(72f, 88f - (float)(handSize - 10) * 1.5f);
	}

	private static void ValidateCardIndex(int handSize, int cardIndex)
	{
		if (cardIndex < 0 || cardIndex >= handSize)
		{
			throw new ArgumentOutOfRangeException("cardIndex", $"Card index {cardIndex} is outside hand size {handSize}.");
		}
	}

	private static MethodInfo RequireOwnMethod(string name)
	{
		return AccessTools.Method(typeof(CznMaxHandSizeRuntime), name, (Type[])null, (Type[])null) ?? throw new MissingMethodException(typeof(CznMaxHandSizeRuntime).FullName, name);
	}
}
