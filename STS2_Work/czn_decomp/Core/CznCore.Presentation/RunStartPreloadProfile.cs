using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;

namespace CznCore.Presentation;

internal sealed class RunStartPreloadProfile
{
	private readonly Func<string, string> _nodeName;

	public string OwnerId { get; }

	public string DisplayName { get; }

	public Func<CharacterModel?, bool> AppliesTo { get; }

	public CharacterSkinProfile? SkinProfile { get; }

	public Func<IReadOnlyList<RunStartPreloadItem>> BuildItems { get; }

	public IReadOnlyList<RunStartPreloadStep> FinalSteps { get; init; } = Array.Empty<RunStartPreloadStep>();

	public string LoadingTitle { get; init; }

	public string PreparingStatus { get; init; }

	public string ReadyStatus { get; init; } = "Ready";

	public float MinimumVisibleSeconds { get; init; } = 0.35f;

	public float ItemProgressEnd { get; init; } = 0.95f;

	public RunStartPreloadProfile(CharacterSkinProfile skinProfile, string displayName, Func<IReadOnlyList<RunStartPreloadItem>> buildItems)
		: this(skinProfile.ProfileId, displayName, skinProfile.AppliesTo, buildItems, skinProfile.RuntimeNodeName)
	{
		SkinProfile = skinProfile;
	}

	public RunStartPreloadProfile(string ownerId, string displayName, Func<CharacterModel?, bool> appliesTo, Func<IReadOnlyList<RunStartPreloadItem>> buildItems)
		: this(ownerId, displayName, appliesTo, buildItems, (string suffix) => "Czn" + ownerId + suffix)
	{
	}

	private RunStartPreloadProfile(string ownerId, string displayName, Func<CharacterModel?, bool> appliesTo, Func<IReadOnlyList<RunStartPreloadItem>> buildItems, Func<string, string> nodeName)
	{
		OwnerId = ownerId;
		DisplayName = displayName;
		AppliesTo = appliesTo;
		BuildItems = buildItems;
		_nodeName = nodeName;
		LoadingTitle = "Loading " + displayName;
		PreparingStatus = "Preparing " + displayName + " assets";
	}

	public string RuntimeNodeName(string suffix)
	{
		return _nodeName(suffix);
	}
}
