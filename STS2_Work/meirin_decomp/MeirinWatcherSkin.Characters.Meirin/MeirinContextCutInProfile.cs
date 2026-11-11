using System;
using System.Collections.Generic;
using CznCore.Presentation;
using Godot;

namespace MeirinWatcherSkin.Characters.Meirin;

internal static class MeirinContextCutInProfile
{
	public const string CurseCollapseStateId = "meirin.combat.cutin.collapse_curse";

	private const string CurseCollapseVideoPath = "res://MeirinWatcherSkin/videos/cutin/1027_collapse_cutin.ogv";

	private const float CurseCollapseCutInLifetimeSeconds = 0.92f;

	private static readonly Dictionary<string, CombatVideoCutInProfile> ProfileMap = new Dictionary<string, CombatVideoCutInProfile>(StringComparer.Ordinal) { ["meirin.combat.cutin.collapse_curse"] = new CombatVideoCutInProfile("meirin.combat.cutin.collapse_curse", "res://MeirinWatcherSkin/videos/cutin/1027_collapse_cutin.ogv", new Vector2(1580f, 720f), 0.92f, 0f, 0f, 0.92f, 0f, 0f, 980) };

	public static IReadOnlyDictionary<string, CombatVideoCutInProfile> Profiles => ProfileMap;
}
