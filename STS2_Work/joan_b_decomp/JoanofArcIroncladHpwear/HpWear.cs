using System;
using System.Collections.Generic;
using System.Reflection;
using DesktopDeathSafety;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Runs;

namespace JoanofArcIroncladHpwear;

internal static class HpWear
{
	private sealed class State
	{
		internal int Stage;

		internal int Wait;
	}

	private static readonly Dictionary<ulong, State> CombatStates = new Dictionary<ulong, State>();

	private static readonly Dictionary<ulong, State> ShopStates = new Dictionary<ulong, State>();

	private static readonly Dictionary<ulong, State> RestStates = new Dictionary<ulong, State>();

	private static readonly List<NRestSiteCharacter> RestChars = new List<NRestSiteCharacter>();

	private static readonly Dictionary<ulong, int> ShownStage = new Dictionary<ulong, int>();

	private static FieldInfo? _merchantPlayersField;

	private static bool _merchantPlayersLookupFailed;

	private static int _errorStreak;

	private static readonly Dictionary<ulong, float> AppliedScales = new Dictionary<ulong, float>();

	internal static bool Disabled { get; private set; }

	private static int LivePortraitStage(string group, Node? spine)
	{
		if (spine != null)
		{
			return SkinCatalog.StageFromPath(group, SpineSwap.GetSkeletonPath(spine));
		}
		return 0;
	}

	internal static int PortraitStage(Player player)
	{
		NCombatRoom instance = NCombatRoom.Instance;
		NCreature val = ((instance != null) ? instance.GetCreatureNode(player.Creature) : null);
		Node val2 = ((val == null) ? null : SpineSwap.FindSpine((Node?)(object)val.Visuals));
		int result = LivePortraitStage("ironclad", val2);
		if (val2 != null)
		{
			return result;
		}
		foreach (NRestSiteCharacter restChar in RestChars)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)restChar) && restChar.Player == player && ((Node)restChar).IsInsideTree())
			{
				Node val3 = SpineSwap.FindSpine((Node?)(object)restChar);
				if (val3 != null)
				{
					return LivePortraitStage("restsite", val3);
				}
			}
		}
		NMerchantRoom instance2 = NMerchantRoom.Instance;
		if (instance2 != null)
		{
			List<Player> list = MerchantPlayers(instance2);
			if (list != null)
			{
				for (int i = 0; i < Math.Min(list.Count, instance2.PlayerVisuals.Count); i++)
				{
					if (list[i] == player)
					{
						Node val4 = SpineSwap.FindSpine((Node?)(object)instance2.PlayerVisuals[i]);
						if (val4 != null)
						{
							return LivePortraitStage("ironclad_shop", val4);
						}
					}
				}
			}
		}
		Creature creature = player.Creature;
		if (!creature.IsDead)
		{
			return Math.Max(1, WearSettingsRuntime.Current.ResolveStage(creature.CurrentHp, creature.MaxHp));
		}
		return WearSettingsRuntime.Current.ResolveDeathStage(creature.CurrentHp, creature.MaxHp);
	}

	internal static void Tick()
	{
		if (Disabled)
		{
			return;
		}
		try
		{
			ModEntry.FlushDeferred();
			TickCombat();
			TickShop();
			TickRest();
			PortraitSync.RefreshRun();
			if (NCombatRoom.Instance == null && NMerchantRoom.Instance == null && NRestSiteRoom.Instance == null)
			{
				ShownStage.Clear();
			}
			_errorStreak = 0;
		}
		catch (Exception value)
		{
			_errorStreak++;
			ModEntry.Log.Error($"wear tick failed ({_errorStreak}): {value}", 1);
			if (_errorStreak >= 20)
			{
				Disabled = true;
				ModEntry.Log.Error("wear disabled for this session after repeated failures.", 1);
			}
		}
	}

	internal static void OnCreatureReady(NCreature node)
	{
		Creature entity = node.Entity;
		Player val = ((entity != null) ? entity.Player : null);
		if (((val != null) ? val.Character : null) is Ironclad && SkinSelectionGate.PlayerAllowed(val))
		{
			Node val2 = SpineSwap.FindSpine((Node?)(object)node.Visuals);
			if (val2 != null && SpineSwap.IsOurs(val2))
			{
				ApplyVisualScale(node);
				UnderwearVisibility.Attach(val2);
			}
		}
	}

	internal static void TrackRest(NRestSiteCharacter character)
	{
		if (character != null && GodotObject.IsInstanceValid((GodotObject)(object)character) && !RestChars.Contains(character))
		{
			RestChars.Add(character);
		}
	}

	internal static void PrepareDeath(NCreature node)
	{
		if (Disabled || !WearSettingsRuntime.Current.WearEnabled || node == null || !GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return;
		}
		Creature entity = node.Entity;
		object obj;
		if (entity == null)
		{
			obj = null;
		}
		else
		{
			Player player = entity.Player;
			obj = ((player != null) ? player.Character : null);
		}
		if (!(obj is Ironclad) || !SkinSelectionGate.PlayerAllowed(entity.Player))
		{
			return;
		}
		Node val = SpineSwap.FindSpine((Node?)(object)node.Visuals);
		if (val == null)
		{
			return;
		}
		WearCrossfade.Cancel(val);
		int num = SkinCatalog.StageFromPath("ironclad", SpineSwap.GetSkeletonPath(val));
		int num2 = WearSettingsRuntime.Current.ResolveDeathStage(entity.CurrentHp, entity.MaxHp);
		if (num != 0 && num2 != 0 && num != num2)
		{
			Resource val2 = SpineSwap.LoadStageData("ironclad", num2);
			if (val2 != null && SpineSwap.SetSkeletonData(val, val2))
			{
				ShownStage[((GodotObject)val).GetInstanceId()] = num2;
				ModEntry.Log.Info($"combat: stage {num2} prepared before death animation.", 1);
			}
		}
	}

	internal static void RefreshDeathSafety(Node spine)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		if (SpineSwap.CurrentAnimation(spine) != "die" || !SpineSwap.IsOurs(spine))
		{
			return;
		}
		string[] battleGroups = SkinCatalog.BattleGroups;
		foreach (string group in battleGroups)
		{
			int num = SkinCatalog.StageFromPath(group, SpineSwap.GetSkeletonPath(spine));
			if (num == 0)
			{
				continue;
			}
			int num2 = WearSettingsRuntime.Current.ResolveDeathStage(0, 1);
			if (num == num2)
			{
				break;
			}
			Variant val = ((GodotObject)spine).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
			GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
			object obj;
			if (val2 == null)
			{
				obj = null;
			}
			else
			{
				val = val2.Call(StringName.op_Implicit("get_current"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0) });
				obj = ((Variant)(ref val)).AsGodotObject();
			}
			double num3;
			if (obj == null)
			{
				num3 = 0.0;
			}
			else
			{
				val = ((GodotObject)obj).Call(StringName.op_Implicit("get_track_time"), Array.Empty<Variant>());
				num3 = ((Variant)(ref val)).AsDouble();
			}
			double num4 = num3;
			double num5;
			if (val2 == null)
			{
				num5 = 1.0;
			}
			else
			{
				val = val2.Call(StringName.op_Implicit("get_time_scale"), Array.Empty<Variant>());
				num5 = ((Variant)(ref val)).AsDouble();
			}
			double num6 = num5;
			Resource val3 = SpineSwap.LoadStageData(group, num2);
			if (val3 == null)
			{
				break;
			}
			WearCrossfade.Cancel(spine);
			Runtime.FadeBeforeSafetySwap(spine);
			if (SpineSwap.SetSkeletonData(spine, val3) && SpineSwap.Play(spine, "die", loop: false))
			{
				val = ((GodotObject)spine).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
				GodotObject val4 = ((Variant)(ref val)).AsGodotObject();
				object obj2;
				if (val4 == null)
				{
					obj2 = null;
				}
				else
				{
					val = val4.Call(StringName.op_Implicit("get_current"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0) });
					obj2 = ((Variant)(ref val)).AsGodotObject();
				}
				GodotObject val5 = (GodotObject)obj2;
				if (val4 != null)
				{
					val4.Call(StringName.op_Implicit("set_time_scale"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(num6) });
				}
				if (val5 != null)
				{
					val5.Call(StringName.op_Implicit("set_track_time"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(num4) });
				}
				((GodotObject)spine).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0.0) });
				ShownStage[((GodotObject)spine).GetInstanceId()] = num2;
			}
			break;
		}
	}

	internal static bool ApplyDeathLook(Node spine, string group, int hp, int maxHp)
	{
		WearSettings current = WearSettingsRuntime.Current;
		if (!current.WearEnabled || SkinCatalog.StageFromPath(group, SpineSwap.GetSkeletonPath(spine)) == 0)
		{
			return false;
		}
		int num = current.ResolveDeathStage(hp, maxHp);
		if (num == 0)
		{
			return false;
		}
		Resource val = SpineSwap.LoadStageData(group, num);
		if (val != null)
		{
			return SpineSwap.SetSkeletonData(spine, val);
		}
		return false;
	}

	internal static void OnNonCombatDeath(Creature creature)
	{
		if (Disabled || NCombatRoom.Instance != null)
		{
			return;
		}
		Player player = creature.Player;
		if (!(((player != null) ? player.Character : null) is Ironclad) || !SkinSelectionGate.PlayerAllowed(creature.Player))
		{
			return;
		}
		foreach (NRestSiteCharacter restChar in RestChars)
		{
			if (restChar != null && GodotObject.IsInstanceValid((GodotObject)(object)restChar) && restChar.Player == creature.Player)
			{
				Node val = SpineSwap.FindSpine((Node?)(object)restChar);
				if (val != null)
				{
					ApplyDeathLook(val, "restsite", creature.CurrentHp, creature.MaxHp);
				}
			}
		}
		NMerchantRoom instance = NMerchantRoom.Instance;
		if (instance == null)
		{
			return;
		}
		List<Player> list = MerchantPlayers(instance);
		IReadOnlyList<NMerchantCharacter> playerVisuals = instance.PlayerVisuals;
		if (list == null || playerVisuals == null)
		{
			return;
		}
		for (int i = 0; i < Math.Min(list.Count, playerVisuals.Count); i++)
		{
			if (list[i] == creature.Player && playerVisuals[i] != null && GodotObject.IsInstanceValid((GodotObject)(object)playerVisuals[i]))
			{
				Node val2 = SpineSwap.FindSpine((Node?)(object)playerVisuals[i]);
				if (val2 != null)
				{
					ApplyDeathLook(val2, "ironclad_shop", creature.CurrentHp, creature.MaxHp);
				}
			}
		}
	}

	private static void ApplyVisualScale(NCreature node)
	{
		double visualScale = WearSettingsRuntime.Current.VisualScale;
		float num = (float)(double.IsFinite(visualScale) ? Math.Clamp(visualScale, 0.5, 2.0) : 1.0);
		ulong instanceId = ((GodotObject)node).GetInstanceId();
		if (!AppliedScales.TryGetValue(instanceId, out var value) || !(Math.Abs(value - num) < 0.001f))
		{
			node.SetScaleAndHue(num, 0f);
			AppliedScales[instanceId] = num;
		}
	}

	private static void TickCombat()
	{
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			CombatStates.Clear();
			AppliedScales.Clear();
			EnemyAttackGate.Reset();
			return;
		}
		RunManager instance2 = RunManager.Instance;
		object obj;
		if (instance2 == null)
		{
			obj = null;
		}
		else
		{
			RunState obj2 = instance2.DebugOnlyGetState();
			obj = ((obj2 != null) ? obj2.Players : null);
		}
		IReadOnlyList<Player> readOnlyList = (IReadOnlyList<Player>)obj;
		if (readOnlyList == null)
		{
			return;
		}
		foreach (Player item in readOnlyList)
		{
			if (!(((item != null) ? item.Character : null) is Ironclad))
			{
				continue;
			}
			if (!SkinSelectionGate.PlayerAllowed(item))
			{
				NCreature creatureNode = instance.GetCreatureNode(item.Creature);
				Node val = ((creatureNode == null) ? null : SpineSwap.FindSpine((Node?)(object)creatureNode.Visuals));
				if (val != null)
				{
					WearCrossfade.Cancel(val);
				}
				continue;
			}
			Creature creature = item.Creature;
			if (creature == null || creature.IsDead)
			{
				continue;
			}
			NCreature creatureNode2 = instance.GetCreatureNode(creature);
			if (creatureNode2 == null || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode2))
			{
				continue;
			}
			Node val2 = SpineSwap.FindSpine((Node?)(object)creatureNode2.Visuals);
			if (val2 != null)
			{
				if (SpineSwap.IsOurs(val2))
				{
					ApplyVisualScale(creatureNode2);
					UnderwearVisibility.Attach(val2);
				}
				Consider(CombatStates, "ironclad", "combat", val2, creature, forceIdle: false);
			}
		}
	}

	private static void TickShop()
	{
		NMerchantRoom instance = NMerchantRoom.Instance;
		if (instance == null)
		{
			ShopStates.Clear();
			return;
		}
		IReadOnlyList<NMerchantCharacter> playerVisuals = instance.PlayerVisuals;
		List<Player> list = MerchantPlayers(instance);
		if (playerVisuals == null || list == null)
		{
			return;
		}
		int num = Math.Min(playerVisuals.Count, list.Count);
		for (int i = 0; i < num; i++)
		{
			NMerchantCharacter val = playerVisuals[i];
			Player val2 = list[i];
			if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val) || !(((val2 != null) ? val2.Character : null) is Ironclad))
			{
				continue;
			}
			if (!SkinSelectionGate.PlayerAllowed(val2))
			{
				Node val3 = SpineSwap.FindSpine((Node?)(object)val);
				if (val3 != null)
				{
					WearCrossfade.Cancel(val3);
				}
				continue;
			}
			Creature creature = val2.Creature;
			if (creature != null && !creature.IsDead)
			{
				Node val4 = SpineSwap.FindSpine((Node?)(object)val);
				if (val4 != null)
				{
					Consider(ShopStates, "ironclad_shop", $"shop[{i}]", val4, creature, forceIdle: false);
				}
			}
		}
	}

	private static void TickRest()
	{
		if (NRestSiteRoom.Instance == null)
		{
			RestStates.Clear();
			RestChars.Clear();
			return;
		}
		RestChars.RemoveAll((NRestSiteCharacter c) => c == null || !GodotObject.IsInstanceValid((GodotObject)(object)c));
		foreach (NRestSiteCharacter restChar in RestChars)
		{
			Player player = restChar.Player;
			if (!(((player != null) ? player.Character : null) is Ironclad))
			{
				continue;
			}
			if (!SkinSelectionGate.PlayerAllowed(restChar.Player))
			{
				Node val = SpineSwap.FindSpine((Node?)(object)restChar);
				if (val != null)
				{
					WearCrossfade.Cancel(val);
				}
				continue;
			}
			Creature creature = restChar.Player.Creature;
			if (creature != null && !creature.IsDead)
			{
				Node val2 = SpineSwap.FindSpine((Node?)(object)restChar);
				if (val2 != null)
				{
					Consider(RestStates, "restsite", "rest", val2, creature, forceIdle: false);
				}
			}
		}
	}

	private static void Consider(Dictionary<ulong, State> states, string group, string label, Node spine, Creature creature, bool forceIdle)
	{
		ulong instanceId = ((GodotObject)spine).GetInstanceId();
		if (!states.TryGetValue(instanceId, out State value))
		{
			State obj = new State
			{
				Stage = 1
			};
			value = obj;
			states[instanceId] = obj;
		}
		int num = WearSettingsRuntime.Current.ResolveStage(creature.CurrentHp, creature.MaxHp);
		if (num == 0)
		{
			if (value.Wait == 0)
			{
				value.Wait = 1;
				ModEntry.Log.Info($"{label}: max HP {creature.MaxHp} unusable — keeping current look.", 1);
			}
			return;
		}
		int num2 = SkinCatalog.StageFromPath(group, SpineSwap.GetSkeletonPath(spine));
		if (num2 == 0)
		{
			if (value.Stage != -1)
			{
				value.Stage = -1;
				value.Wait = 0;
				ShownStage.Remove(instanceId);
				ModEntry.Log.Info(label + ": live skeleton is not this mod's " + group + " — wear skipped.", 1);
			}
		}
		else
		{
			if (!forceIdle && WearCrossfade.IsActive(spine))
			{
				return;
			}
			if (value.Stage != num2)
			{
				value.Stage = num2;
				value.Wait = 0;
			}
			if (num == value.Stage)
			{
				value.Wait = 0;
				ShownStage[instanceId] = value.Stage;
				UpdateLowHealthIdle(group, spine, creature);
				return;
			}
			string text = SpineSwap.CurrentAnimation(spine);
			if ((forceIdle || HpWearRules.CanSwapDuring(text)) && (forceIdle || !(group == "ironclad") || !EnemyAttackGate.Hold(creature, text)) && (forceIdle || !SpineSwap.IsMixing(spine)) && (forceIdle || !UpdateLowHealthIdle(group, spine, creature)))
			{
				value.Wait = 0;
				Apply(states, value, group, label, spine, num, creature, text, forceIdle);
				UpdateLowHealthIdle(group, spine, creature);
			}
		}
	}

	private static bool UpdateLowHealthIdle(string group, Node spine, Creature creature)
	{
		if (group != "ironclad" && group != "ironclad_shop")
		{
			return false;
		}
		if (creature.CurrentHp <= 0 || creature.MaxHp <= 0)
		{
			return false;
		}
		string text = SpineSwap.CurrentAnimation(spine);
		if (text != "idle_loop" && text != "low_health_loop")
		{
			return false;
		}
		int num = Math.Clamp(WearSettingsRuntime.Current.LowHealthIdlePercent, 0, 100);
		string text2 = ((num > 0 && (long)creature.CurrentHp * 100L <= (long)creature.MaxHp * (long)num) ? "low_health_loop" : "idle_loop");
		if (text == text2 || SpineSwap.IsMixing(spine))
		{
			return false;
		}
		if (text2 == "low_health_loop" && group == "ironclad" && EnemyAttackGate.Hold(creature, text))
		{
			return false;
		}
		return SpineSwap.Play(spine, text2, loop: true);
	}

	private static void Apply(Dictionary<ulong, State> states, State state, string group, string label, Node spine, int stage, Creature creature, string? current, bool forceIdle)
	{
		Resource data = SpineSwap.LoadStageData(group, stage);
		if (data != null && (forceIdle || !WearCrossfade.BeginDeferred(spine, StillValid, Commit)))
		{
			Commit();
		}
		void Commit()
		{
			if (!SpineSwap.SetSkeletonData(spine, data))
			{
				WearCrossfade.Cancel(spine);
				ModEntry.Log.Error(label + ": skeleton data did not change to " + SkinCatalog.DataPath(group, stage) + ".", 1);
			}
			else
			{
				string text = SpineSwap.CurrentAnimation(spine);
				string text2 = HpWearRules.SafeLoopAnimation(group, string.IsNullOrEmpty(text) ? current : text);
				SpineSwap.Play(spine, text2, loop: true);
				int stage2 = state.Stage;
				state.Stage = stage;
				ShownStage[((GodotObject)spine).GetInstanceId()] = stage;
				ModEntry.Log.Info($"{label}: stage {stage2}->{stage} (hp {creature.CurrentHp}/{creature.MaxHp}, anim {text2}).", 1);
			}
		}
		bool StillValid()
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)spine) || creature.IsDead || creature.CurrentHp <= 0 || WearSettingsRuntime.Current.ResolveStage(creature.CurrentHp, creature.MaxHp) != stage || !SkinSelectionGate.PlayerAllowed(creature.Player) || !SpineSwap.IsOurs(spine))
			{
				return false;
			}
			string animation = SpineSwap.CurrentAnimation(spine);
			if (HpWearRules.CanSwapDuring(animation) && !SpineSwap.IsMixing(spine))
			{
				if (!(group != "ironclad"))
				{
					return !EnemyAttackGate.Hold(creature, animation);
				}
				return true;
			}
			return false;
		}
	}

	private static List<Player>? MerchantPlayers(NMerchantRoom room)
	{
		if (_merchantPlayersLookupFailed)
		{
			return null;
		}
		if (_merchantPlayersField == null)
		{
			_merchantPlayersField = AccessTools.Field(typeof(NMerchantRoom), "_players");
			if (_merchantPlayersField == null)
			{
				_merchantPlayersLookupFailed = true;
				ModEntry.Log.Error("shop: NMerchantRoom._players not found — shop wear disabled.", 1);
				return null;
			}
		}
		return _merchantPlayersField.GetValue(room) as List<Player>;
	}
}
