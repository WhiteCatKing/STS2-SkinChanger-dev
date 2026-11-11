using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Utilities;

public static class MultiEnchantmentHelper
{
	private static readonly Lazy<Func<CardModel, IEnumerable<EnchantmentModel>>?> _getEnchantmentsFunc = new Lazy<Func<CardModel, IEnumerable<EnchantmentModel>>>(delegate
	{
		try
		{
			Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "MultiEnchantmentMod");
			if (assembly == null)
			{
				return (Func<CardModel, IEnumerable<EnchantmentModel>>?)null;
			}
			Type type = assembly.GetType("MultiEnchantmentMod.MultiEnchantmentSupport");
			if (type == null)
			{
				return (Func<CardModel, IEnumerable<EnchantmentModel>>?)null;
			}
			MethodInfo method = type.GetMethod("GetEnchantments", BindingFlags.Static | BindingFlags.Public);
			if (method == null)
			{
				return (Func<CardModel, IEnumerable<EnchantmentModel>>?)null;
			}
			return (Func<CardModel, IEnumerable<EnchantmentModel>>)Delegate.CreateDelegate(typeof(Func<CardModel, IEnumerable<EnchantmentModel>>), method);
		}
		catch
		{
			return (Func<CardModel, IEnumerable<EnchantmentModel>>?)null;
		}
	});

	public static bool HasEnchantment<T>(CardModel? card) where T : EnchantmentModel
	{
		return HasEnchantment(card, typeof(T));
	}

	public static bool HasEnchantment(CardModel? card, Type enchantmentType)
	{
		if (card == null)
		{
			return false;
		}
		Func<CardModel, IEnumerable<EnchantmentModel>> value = _getEnchantmentsFunc.Value;
		if (value != null)
		{
			try
			{
				return value(card).Any((EnchantmentModel e) => ((object)e).GetType() == enchantmentType);
			}
			catch
			{
			}
		}
		return ((object)card.Enchantment)?.GetType() == enchantmentType;
	}

	public static IEnumerable<T> GetEnchantmentsOfType<T>(CardModel? card) where T : EnchantmentModel
	{
		if (card == null)
		{
			return Enumerable.Empty<T>();
		}
		Func<CardModel, IEnumerable<EnchantmentModel>> value = _getEnchantmentsFunc.Value;
		if (value != null)
		{
			try
			{
				return value(card).OfType<T>();
			}
			catch
			{
			}
		}
		EnchantmentModel enchantment = card.Enchantment;
		T val = (T)(object)((enchantment is T) ? enchantment : null);
		if (val != null)
		{
			return new T[1] { val };
		}
		return Enumerable.Empty<T>();
	}
}
