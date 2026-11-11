using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

internal sealed class ScopedSkinBehavior
{
	private static readonly ConcurrentDictionary<MethodBase, ScopedSkinBehavior> Owners = new ConcurrentDictionary<MethodBase, ScopedSkinBehavior>();

	[ThreadStatic]
	private static Player? _player;

	private readonly Harmony _harmony = new Harmony(harmonyId);

	private readonly Func<Assembly, CharacterModel, Player?, bool?> _isSelected = isSelected;

	private readonly HashSet<Assembly> _installed = new HashSet<Assembly>();

	private readonly ConcurrentDictionary<MethodBase, byte> _reportedFailures = new ConcurrentDictionary<MethodBase, byte>();

	public ScopedSkinBehavior(string harmonyId, Func<Assembly, CharacterModel, Player?, bool?> isSelected)
	{
	}//IL_0002: Unknown result type (might be due to invalid IL or missing references)
	//IL_000c: Expected O, but got Unknown


	public int Install(Assembly assembly)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		//IL_00ce: Expected O, but got Unknown
		//IL_00ce: Expected O, but got Unknown
		if (!_installed.Add(assembly))
		{
			return 0;
		}
		Type[] source;
		try
		{
			source = assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			source = ex.Types.OfType<Type>().ToArray();
		}
		int num = 0;
		foreach (MethodInfo item in source.SelectMany(SkinBehaviorContract.Find))
		{
			if (Owners.TryAdd(item, this))
			{
				try
				{
					_harmony.Patch((MethodBase)item, new HarmonyMethod(typeof(ScopedSkinBehavior), "Prefix", (Type[])null)
					{
						priority = 800
					}, new HarmonyMethod(typeof(ScopedSkinBehavior), "Postfix", (Type[])null)
					{
						priority = 0
					}, (HarmonyMethod)null, new HarmonyMethod(typeof(ScopedSkinBehavior), "Finalizer", (Type[])null));
					num++;
				}
				catch
				{
					Owners.TryRemove(item, out ScopedSkinBehavior _);
					throw;
				}
			}
		}
		return num;
	}

	private static void Prefix(object? __0, out Player? __state)
	{
		__state = _player;
		Player val = (Player)((__0 is Player) ? __0 : null);
		if (val != null)
		{
			_player = val;
		}
	}

	private static void Postfix(MethodBase __originalMethod, object? __0, ref bool __result)
	{
		if (!__result || !Owners.TryGetValue(__originalMethod, out ScopedSkinBehavior value))
		{
			return;
		}
		object obj = ((__0 is CharacterModel) ? __0 : null);
		if (obj == null)
		{
			object? obj2 = ((__0 is Player) ? __0 : null);
			obj = ((obj2 != null) ? ((Player)obj2).Character : null);
		}
		CharacterModel val = (CharacterModel)obj;
		if (val == null)
		{
			return;
		}
		Player val2 = (Player)((__0 is Player) ? __0 : null);
		if (val2 == null && _player != null && _player.Character == val)
		{
			val2 = _player;
		}
		try
		{
			if (value.isSelected(__originalMethod.Module.Assembly, val, val2) == false)
			{
				__result = false;
			}
		}
		catch (Exception ex)
		{
			if (value._reportedFailures.TryAdd(__originalMethod, 0))
			{
				ModLog.Warn("检查皮肤行为归属失败，保留原判断：" + ex.GetBaseException().Message);
			}
		}
	}

	private static void Finalizer(Player? __state)
	{
		_player = __state;
	}
}
