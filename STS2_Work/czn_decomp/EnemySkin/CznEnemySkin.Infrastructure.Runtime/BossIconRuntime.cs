using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CznEnemySkin.Domain;
using CznEnemySkin.Localization;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.sts2.Core.Nodes.TopBar;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class BossIconRuntime
{
	private sealed record BossIconDefinition(string EncounterId, bool HasMapSilhouette, IReadOnlyList<string> RequiredMonsterIds, IReadOnlyList<string> DisplayMonsterIds);

	private const string ResourceRoot = "res://CznEnemySkin/images/boss_icons";

	private const string MapFrameNodeName = "CznBossMapFrame";

	private const string MapFrameRoot = "res://CznEnemySkin/images/boss_icons/map_frames";

	private static readonly LocString BossHoverTipTitle = new LocString("static_hover_tips", "BOSS.title");

	private static readonly LocString BossHoverTipDescription = new LocString("static_hover_tips", "BOSS.description");

	private static readonly LocString DoubleBossHoverTipTitle = new LocString("static_hover_tips", "DOUBLE_BOSS.title");

	private static readonly LocString DoubleBossHoverTipDescription = new LocString("static_hover_tips", "DOUBLE_BOSS.description");

	private static readonly IReadOnlyDictionary<string, BossIconDefinition> Definitions = BuildDefinitions();

	private static readonly FieldInfo? MapRunStateField = AccessTools.Field(typeof(NMapPoint), "_runState");

	private static readonly FieldInfo? MapUsesSpineField = AccessTools.Field(typeof(NBossMapPoint), "_usesSpine");

	private static readonly FieldInfo? MapPlaceholderImageField = AccessTools.Field(typeof(NBossMapPoint), "_placeholderImage");

	private static readonly FieldInfo? MapPlaceholderOutlineField = AccessTools.Field(typeof(NBossMapPoint), "_placeholderOutline");

	private static readonly FieldInfo? TopBarRunStateField = AccessTools.Field(typeof(NTopBarBossIcon), "_runState");

	private static readonly FieldInfo? TopBarBossIconField = AccessTools.Field(typeof(NTopBarBossIcon), "_bossIcon");

	private static readonly FieldInfo? TopBarBossOutlineField = AccessTools.Field(typeof(NTopBarBossIcon), "_bossIconOutline");

	private static readonly FieldInfo? TopBarSecondBossIconField = AccessTools.Field(typeof(NTopBarBossIcon), "_secondBossIcon");

	private static readonly FieldInfo? TopBarSecondBossOutlineField = AccessTools.Field(typeof(NTopBarBossIcon), "_secondBossIconOutline");

	private static readonly FieldInfo? HistoryEntryField = AccessTools.Field(typeof(NMapPointHistoryEntry), "_entry");

	private readonly Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>(StringComparer.Ordinal);

	private readonly HashSet<string> _reportedFailures = new HashSet<string>(StringComparer.Ordinal);

	public void ApplyMapPoint(NBossMapPoint mapPoint)
	{
		try
		{
			if (MapUsesSpineField == null || MapPlaceholderImageField == null || MapPlaceholderOutlineField == null)
			{
				ReportOnce("map-fields", "Boss map icon adapter did not find the STS2 placeholder fields; keeping the native boss node.");
				return;
			}
			IRunState val = ReadField<IRunState>(MapRunStateField, mapPoint);
			if (val == null)
			{
				ReportOnce("map-reflection", "Boss map icon adapter could not read the STS2 run state; keeping the native boss node.");
				return;
			}
			EncounterModel val2 = ((((NMapPoint)mapPoint).Point == val.Map.SecondBossMapPoint) ? val.Act.SecondBossEncounter : val.Act.BossEncounter);
			if (val2 != null && TryResolveTextures(((AbstractModel)val2).Id.Entry, "map", "_icon", "_icon_outline", out Texture2D icon, out Texture2D outline))
			{
				Node2D node = ((Node)mapPoint).GetNode<Node2D>(NodePath.op_Implicit("%SpineSprite"));
				TextureRect node2 = ((Node)mapPoint).GetNode<TextureRect>(NodePath.op_Implicit("%PlaceholderImage"));
				TextureRect node3 = ((Node)mapPoint).GetNode<TextureRect>(NodePath.op_Implicit("%PlaceholderOutline"));
				((CanvasItem)node).Visible = false;
				GetOrCreateMapFrame(((Node)mapPoint).GetNode<Node2D>(NodePath.op_Implicit("%SpriteContainer")));
				node2.Texture = icon;
				node3.Texture = outline;
				((CanvasItem)node2).Visible = true;
				((CanvasItem)node3).Visible = true;
				MapUsesSpineField.SetValue(mapPoint, false);
				MapPlaceholderImageField.SetValue(mapPoint, node2);
				MapPlaceholderOutlineField.SetValue(mapPoint, node3);
				RefreshMapPointState(mapPoint);
			}
		}
		catch (Exception ex)
		{
			ReportOnce("map-runtime", "Boss map icon adapter failed; keeping the native boss node [error=" + ex.Message + "].");
		}
	}

	public void RefreshMapPointState(NBossMapPoint mapPoint)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!IsReplacementMapPoint(mapPoint))
			{
				return;
			}
			TextureRect nodeOrNull = ((Node)((Node)mapPoint).GetNode<Node2D>(NodePath.op_Implicit("%SpriteContainer"))).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("CznBossMapFrame"));
			if (nodeOrNull != null)
			{
				MapPointState state = ((NMapPoint)mapPoint).State;
				string text = (((int)state == 1) ? "travelable" : (((int)state != 2) ? "untravelable" : "traveled"));
				string text2 = text;
				Texture2D val = LoadTexture("res://CznEnemySkin/images/boss_icons/map_frames/" + text2 + ".png");
				if (val != null)
				{
					nodeOrNull.Texture = val;
					((CanvasItem)nodeOrNull).Visible = true;
				}
				TextureRect node = ((Node)mapPoint).GetNode<TextureRect>(NodePath.op_Implicit("%PlaceholderImage"));
				TextureRect node2 = ((Node)mapPoint).GetNode<TextureRect>(NodePath.op_Implicit("%PlaceholderOutline"));
				((CanvasItem)node).SelfModulate = Colors.White;
				IRunState val2 = ReadField<IRunState>(MapRunStateField, mapPoint);
				if (val2 != null)
				{
					((CanvasItem)node2).SelfModulate = val2.Act.MapBgColor;
				}
			}
		}
		catch (Exception ex)
		{
			ReportOnce("map-state-runtime", "Boss map state adapter failed; keeping its last valid state [error=" + ex.Message + "].");
		}
	}

	public void ApplyTopBar(NTopBarBossIcon topBar)
	{
		try
		{
			IRunState val = ReadField<IRunState>(TopBarRunStateField, topBar);
			TextureRect val2 = ReadField<TextureRect>(TopBarBossIconField, topBar);
			TextureRect val3 = ReadField<TextureRect>(TopBarBossOutlineField, topBar);
			if (val == null || val2 == null || val3 == null)
			{
				ReportOnce("topbar-reflection", "Boss top-bar icon adapter could not read the STS2 controls; keeping native icons.");
				return;
			}
			bool flag = val.Map.SecondBossMapPoint != null && val.CurrentMapPoint == val.Map.BossMapPoint;
			EncounterModel val4 = (flag ? val.Act.SecondBossEncounter : val.Act.BossEncounter);
			if (val4 != null)
			{
				ApplyRunHistoryTextures(((AbstractModel)val4).Id.Entry, val2, val3);
			}
			EncounterModel secondBossEncounter = val.Act.SecondBossEncounter;
			if (!(secondBossEncounter == null || flag))
			{
				TextureRect val5 = ReadField<TextureRect>(TopBarSecondBossIconField, topBar);
				TextureRect val6 = ReadField<TextureRect>(TopBarSecondBossOutlineField, topBar);
				if (val5 != null && val6 != null)
				{
					ApplyRunHistoryTextures(((AbstractModel)secondBossEncounter).Id.Entry, val5, val6);
				}
			}
		}
		catch (Exception ex)
		{
			ReportOnce("topbar-runtime", "Boss top-bar icon adapter failed; keeping native icons [error=" + ex.Message + "].");
		}
	}

	public bool TryShowTopBarHoverTip(NTopBarBossIcon topBar)
	{
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			IRunState val = ReadField<IRunState>(TopBarRunStateField, topBar);
			TextureRect val2 = ReadField<TextureRect>(TopBarBossIconField, topBar);
			if (val == null || val2 == null)
			{
				return false;
			}
			bool flag = ShouldOnlyShowSecondBoss(val);
			EncounterModel bossEncounter = val.Act.BossEncounter;
			EncounterModel secondBossEncounter = val.Act.SecondBossEncounter;
			HoverTip val3 = default(HoverTip);
			if (secondBossEncounter != null && !flag)
			{
				if (!TryResolveEncounterDisplayName(bossEncounter, out string displayName) || !TryResolveEncounterDisplayName(secondBossEncounter, out string displayName2))
				{
					return false;
				}
				DoubleBossHoverTipTitle.Add("BossName1", displayName);
				DoubleBossHoverTipTitle.Add("BossName2", displayName2);
				DoubleBossHoverTipDescription.Add("BossName1", displayName);
				DoubleBossHoverTipDescription.Add("BossName2", displayName2);
				((HoverTip)(ref val3))._002Ector(DoubleBossHoverTipTitle, DoubleBossHoverTipDescription, (Texture2D)null);
			}
			else
			{
				if (!TryResolveEncounterDisplayName(flag ? secondBossEncounter : bossEncounter, out string displayName3))
				{
					return false;
				}
				BossHoverTipTitle.Add("BossName", displayName3);
				BossHoverTipDescription.Add("BossName", displayName3);
				((HoverTip)(ref val3))._002Ector(BossHoverTipTitle, BossHoverTipDescription, (Texture2D)null);
			}
			NHoverTipSet obj = NHoverTipSet.CreateAndShow((Control)(object)topBar, (IHoverTip)(object)val3, (HoverTipAlignment)0);
			if (obj != null)
			{
				((Control)obj).SetGlobalPosition(((Control)val2).GlobalPosition + new Vector2(0f, ((Control)topBar).Size.Y + 20f), false);
			}
			return true;
		}
		catch (Exception ex)
		{
			ReportOnce("topbar-name-runtime", "Boss top-bar name adapter failed; keeping the native name [error=" + ex.Message + "].");
			return false;
		}
	}

	public void ApplyRunHistory(NMapPointHistoryEntry historyNode)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		try
		{
			MapPointHistoryEntry val = ReadField<MapPointHistoryEntry>(HistoryEntryField, historyNode);
			if (val != null && (int)val.MapPointType == 7)
			{
				MapPointRoomHistoryEntry? obj = ((IEnumerable<MapPointRoomHistoryEntry>)val.Rooms).FirstOrDefault((Func<MapPointRoomHistoryEntry, bool>)((MapPointRoomHistoryEntry room) => (int)room.RoomType == 3));
				ModelId val2 = ((obj != null) ? obj.ModelId : null);
				if (!(val2 == (ModelId)null))
				{
					TextureRect node = ((Node)historyNode).GetNode<TextureRect>(NodePath.op_Implicit("%Icon"));
					TextureRect node2 = ((Node)historyNode).GetNode<TextureRect>(NodePath.op_Implicit("%Outline"));
					ApplyRunHistoryTextures(val2.Entry, node, node2);
				}
			}
		}
		catch (Exception ex)
		{
			ReportOnce("history-runtime", "Run-history boss icon adapter failed; keeping the native icon [error=" + ex.Message + "].");
		}
	}

	private void ApplyRunHistoryTextures(string encounterId, TextureRect iconNode, TextureRect outlineNode)
	{
		if (TryResolveTextures(encounterId, "run_history", string.Empty, "_outline", out Texture2D icon, out Texture2D outline))
		{
			iconNode.Texture = icon;
			outlineNode.Texture = outline;
			((CanvasItem)iconNode).Visible = true;
			((CanvasItem)outlineNode).Visible = true;
		}
	}

	private static TextureRect GetOrCreateMapFrame(Node2D spriteContainer)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		TextureRect nodeOrNull = ((Node)spriteContainer).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("CznBossMapFrame"));
		if (nodeOrNull != null)
		{
			return nodeOrNull;
		}
		TextureRect val = new TextureRect
		{
			Name = StringName.op_Implicit("CznBossMapFrame"),
			Position = new Vector2(-140f, -168f),
			Size = new Vector2(280f, 336f),
			CustomMinimumSize = new Vector2(280f, 336f),
			MouseFilter = (MouseFilterEnum)2,
			ExpandMode = (ExpandModeEnum)1,
			StretchMode = (StretchModeEnum)5,
			ZIndex = -1
		};
		((Node)spriteContainer).AddChild((Node)(object)val, false, (InternalMode)0);
		return val;
	}

	private static bool IsReplacementMapPoint(NBossMapPoint mapPoint)
	{
		IRunState val = ReadField<IRunState>(MapRunStateField, mapPoint);
		EncounterModel val2 = ((val == null) ? null : ((((NMapPoint)mapPoint).Point == val.Map.SecondBossMapPoint) ? val.Act.SecondBossEncounter : val.Act.BossEncounter));
		if (val2 != null && Definitions.TryGetValue(((AbstractModel)val2).Id.Entry, out BossIconDefinition value) && value.HasMapSilhouette)
		{
			return value.RequiredMonsterIds.All(IsReplacementEnabled);
		}
		return false;
	}

	private bool TryResolveTextures(string encounterId, string surface, string iconSuffix, string outlineSuffix, out Texture2D icon, out Texture2D outline)
	{
		icon = null;
		outline = null;
		if (!Definitions.TryGetValue(encounterId, out BossIconDefinition value) || !value.RequiredMonsterIds.All(IsReplacementEnabled))
		{
			return false;
		}
		if (surface == "map" && !value.HasMapSilhouette)
		{
			return false;
		}
		string value2 = value.EncounterId.ToLowerInvariant();
		string path = $"{"res://CznEnemySkin/images/boss_icons"}/{surface}/{value2}{iconSuffix}.png";
		string path2 = $"{"res://CznEnemySkin/images/boss_icons"}/{surface}/{value2}{outlineSuffix}.png";
		Texture2D val = LoadTexture(path);
		Texture2D val2 = LoadTexture(path2);
		if (val == null || val2 == null)
		{
			return false;
		}
		icon = val;
		outline = val2;
		return true;
	}

	private static bool IsReplacementEnabled(string monsterId)
	{
		EnemyReplacementProfile profile;
		return Entry.Replacements.TryResolveMonster(monsterId, out profile);
	}

	private static bool ShouldOnlyShowSecondBoss(IRunState runState)
	{
		if (runState.Map.SecondBossMapPoint != null)
		{
			return runState.CurrentMapPoint == runState.Map.BossMapPoint;
		}
		return false;
	}

	private static bool TryResolveEncounterDisplayName(EncounterModel encounter, out string displayName)
	{
		displayName = string.Empty;
		if (!Definitions.TryGetValue(((AbstractModel)encounter).Id.Entry, out BossIconDefinition value) || !value.RequiredMonsterIds.All(IsReplacementEnabled))
		{
			return false;
		}
		List<string> list = new List<string>();
		foreach (string displayMonsterId in value.DisplayMonsterIds)
		{
			if (!Entry.Replacements.TryResolveMonster(displayMonsterId, out EnemyReplacementProfile profile) || !EnemyLocalization.TryGetEnemyName(profile, out string name))
			{
				return false;
			}
			list.Add(name);
		}
		displayName = string.Join(" / ", list.Distinct<string>(StringComparer.Ordinal));
		return !string.IsNullOrWhiteSpace(displayName);
	}

	private Texture2D? LoadTexture(string path)
	{
		if (_textures.TryGetValue(path, out Texture2D value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			return value;
		}
		_textures.Remove(path);
		if (!ResourceLoader.Exists(path, ""))
		{
			ReportOnce("resource:" + path, "Boss icon resource is unavailable; keeping STS2 native [path=" + path + "].");
			return null;
		}
		Texture2D val = ResourceLoader.Load<Texture2D>(path, (string)null, (CacheMode)1);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			ReportOnce("load:" + path, "Boss icon resource failed to load; keeping STS2 native [path=" + path + "].");
			return null;
		}
		_textures[path] = val;
		return val;
	}

	private static T? ReadField<T>(FieldInfo? field, object instance) where T : class
	{
		return field?.GetValue(instance) as T;
	}

	private void ReportOnce(string key, string message)
	{
		if (_reportedFailures.Add(key))
		{
			Entry.Log.Warn(message, 1);
		}
	}

	private static IReadOnlyDictionary<string, BossIconDefinition> BuildDefinitions()
	{
		return new BossIconDefinition[12]
		{
			new BossIconDefinition("CEREMONIAL_BEAST_BOSS", HasMapSilhouette: true, new _003C_003Ez__ReadOnlySingleElementList<string>("CEREMONIAL_BEAST"), new _003C_003Ez__ReadOnlySingleElementList<string>("CEREMONIAL_BEAST")),
			new BossIconDefinition("THE_KIN_BOSS", HasMapSilhouette: true, new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "KIN_FOLLOWER", "KIN_PRIEST" }), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "KIN_PRIEST", "KIN_FOLLOWER" })),
			new BossIconDefinition("VANTOM_BOSS", HasMapSilhouette: true, new _003C_003Ez__ReadOnlySingleElementList<string>("VANTOM"), new _003C_003Ez__ReadOnlySingleElementList<string>("VANTOM")),
			new BossIconDefinition("LAGAVULIN_MATRIARCH_BOSS", HasMapSilhouette: true, new _003C_003Ez__ReadOnlySingleElementList<string>("LAGAVULIN_MATRIARCH"), new _003C_003Ez__ReadOnlySingleElementList<string>("LAGAVULIN_MATRIARCH")),
			new BossIconDefinition("SOUL_FYSH_BOSS", HasMapSilhouette: true, new _003C_003Ez__ReadOnlySingleElementList<string>("SOUL_FYSH"), new _003C_003Ez__ReadOnlySingleElementList<string>("SOUL_FYSH")),
			new BossIconDefinition("WATERFALL_GIANT_BOSS", HasMapSilhouette: true, new _003C_003Ez__ReadOnlySingleElementList<string>("WATERFALL_GIANT"), new _003C_003Ez__ReadOnlySingleElementList<string>("WATERFALL_GIANT")),
			new BossIconDefinition("KAISER_CRAB_BOSS", HasMapSilhouette: true, new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "CRUSHER", "ROCKET" }), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "CRUSHER", "ROCKET" })),
			new BossIconDefinition("KNOWLEDGE_DEMON_BOSS", HasMapSilhouette: true, new _003C_003Ez__ReadOnlySingleElementList<string>("KNOWLEDGE_DEMON"), new _003C_003Ez__ReadOnlySingleElementList<string>("KNOWLEDGE_DEMON")),
			new BossIconDefinition("THE_INSATIABLE_BOSS", HasMapSilhouette: true, new _003C_003Ez__ReadOnlySingleElementList<string>("THE_INSATIABLE"), new _003C_003Ez__ReadOnlySingleElementList<string>("THE_INSATIABLE")),
			new BossIconDefinition("AEONGLASS_BOSS", HasMapSilhouette: true, new _003C_003Ez__ReadOnlySingleElementList<string>("AEONGLASS"), new _003C_003Ez__ReadOnlySingleElementList<string>("AEONGLASS")),
			new BossIconDefinition("QUEEN_BOSS", HasMapSilhouette: true, new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "QUEEN", "TORCH_HEAD_AMALGAM" }), new _003C_003Ez__ReadOnlySingleElementList<string>("QUEEN")),
			new BossIconDefinition("TEST_SUBJECT_BOSS", HasMapSilhouette: true, new _003C_003Ez__ReadOnlySingleElementList<string>("TEST_SUBJECT"), new _003C_003Ez__ReadOnlySingleElementList<string>("TEST_SUBJECT"))
		}.ToDictionary<BossIconDefinition, string>((BossIconDefinition definition) => definition.EncounterId, StringComparer.Ordinal);
	}
}
