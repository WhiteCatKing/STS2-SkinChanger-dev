using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace CznCore.Cards;

public static class CardMechanicPresentation
{
	[CompilerGenerated]
	private sealed class _003CCreateHoverTips_003Ed__0 : IEnumerable<IHoverTip>, IEnumerable, IEnumerator<IHoverTip>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private IHoverTip _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<CardMechanicDefinition> definitions;

		public IEnumerable<CardMechanicDefinition> _003C_003E3__definitions;

		private HashSet<string> _003CseenIds_003E5__2;

		private IEnumerator<CardMechanicDefinition> _003C_003E7__wrap2;

		IHoverTip IEnumerator<IHoverTip>.Current
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
		public _003CCreateHoverTips_003Ed__0(int _003C_003E1__state)
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
			_003CseenIds_003E5__2 = null;
			_003C_003E7__wrap2 = null;
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
					ArgumentNullException.ThrowIfNull(definitions, "definitions");
					_003CseenIds_003E5__2 = new HashSet<string>(StringComparer.Ordinal);
					_003C_003E7__wrap2 = definitions.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap2.MoveNext())
				{
					CardMechanicDefinition current = _003C_003E7__wrap2.Current;
					Validate(current);
					if (_003CseenIds_003E5__2.Add(current.Id))
					{
						_003C_003E2__current = CreateHoverTip(current);
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap2 = null;
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
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<IHoverTip> IEnumerable<IHoverTip>.GetEnumerator()
		{
			_003CCreateHoverTips_003Ed__0 _003CCreateHoverTips_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CCreateHoverTips_003Ed__ = this;
			}
			else
			{
				_003CCreateHoverTips_003Ed__ = new _003CCreateHoverTips_003Ed__0(0);
			}
			_003CCreateHoverTips_003Ed__.definitions = _003C_003E3__definitions;
			return _003CCreateHoverTips_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<IHoverTip>)this).GetEnumerator();
		}
	}

	public const string NameArgumentSuffix = "Name";

	[IteratorStateMachine(typeof(_003CCreateHoverTips_003Ed__0))]
	public static IEnumerable<IHoverTip> CreateHoverTips(IEnumerable<CardMechanicDefinition> definitions)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCreateHoverTips_003Ed__0(-2)
		{
			_003C_003E3__definitions = definitions
		};
	}

	public static IHoverTip CreateHoverTip(CardMechanicDefinition definition)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		Validate(definition);
		LocString val = Title(definition);
		LocString val2 = Description(definition);
		val2.Add("energyPrefix", string.Empty);
		Texture2D val3 = null;
		if (!string.IsNullOrWhiteSpace(definition.IconPath) && ResourceLoader.Exists(definition.IconPath, ""))
		{
			val3 = ResourceLoader.Load<Texture2D>(definition.IconPath, (string)null, (CacheMode)1);
		}
		HoverTip val4 = default(HoverTip);
		((HoverTip)(ref val4))._002Ector(val, val2, val3);
		((HoverTip)(ref val4)).Id = definition.Id;
		return (IHoverTip)(object)val4;
	}

	public static void AddCardTextArguments(LocString cardDescription, IEnumerable<CardMechanicDefinition> definitions)
	{
		ArgumentNullException.ThrowIfNull(cardDescription, "cardDescription");
		ArgumentNullException.ThrowIfNull(definitions, "definitions");
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		HashSet<string> hashSet2 = new HashSet<string>(StringComparer.Ordinal);
		foreach (CardMechanicDefinition definition in definitions)
		{
			Validate(definition);
			if (hashSet.Add(definition.Id) && !string.IsNullOrWhiteSpace(definition.CardTextArgumentName))
			{
				if (!hashSet2.Add(definition.CardTextArgumentName))
				{
					throw new InvalidOperationException("Card mechanic argument '" + definition.CardTextArgumentName + "' is assigned more than once.");
				}
				cardDescription.Add(definition.CardTextArgumentName, GetCardText(definition));
				cardDescription.Add(definition.CardTextArgumentName + "Name", GetCardName(definition));
			}
		}
	}

	public static string GetCardName(CardMechanicDefinition definition)
	{
		Validate(definition);
		return Title(definition).GetFormattedText();
	}

	public static string GetCardText(CardMechanicDefinition definition)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Validate(definition);
		string rawText = new LocString("card_keywords", "PERIOD").GetRawText();
		return "[gold]" + Title(definition).GetFormattedText() + "[/gold]" + rawText;
	}

	private static LocString Title(CardMechanicDefinition definition)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		return new LocString(definition.LocalizationTable, definition.LocalizationEntryStem + ".title");
	}

	private static LocString Description(CardMechanicDefinition definition)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		return new LocString(definition.LocalizationTable, definition.LocalizationEntryStem + ".description");
	}

	private static void Validate(CardMechanicDefinition definition)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(definition.Id, "definition.Id");
		ArgumentException.ThrowIfNullOrWhiteSpace(definition.LocalizationTable, "definition.LocalizationTable");
		ArgumentException.ThrowIfNullOrWhiteSpace(definition.LocalizationEntryStem, "definition.LocalizationEntryStem");
	}
}
