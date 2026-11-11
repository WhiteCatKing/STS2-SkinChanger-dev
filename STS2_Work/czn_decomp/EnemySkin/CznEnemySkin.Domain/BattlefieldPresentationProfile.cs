using System;

namespace CznEnemySkin.Domain;

internal sealed class BattlefieldPresentationProfile
{
	public string Id { get; }

	public CznBattlefieldSource Source { get; }

	public BattlefieldBackgroundPresentation Background { get; }

	public BattlefieldBgmPresentation Bgm { get; }

	public BattlefieldCameraPresentation Camera { get; }

	public BattlefieldPresentationProfile(string id, CznBattlefieldSource source, BattlefieldBackgroundPresentation background, BattlefieldBgmPresentation bgm, BattlefieldCameraPresentation camera)
	{
		Id = RequireIdentifier(id, "id");
		Source = source ?? throw new ArgumentNullException("source");
		Background = background ?? throw new ArgumentNullException("background");
		Bgm = bgm ?? throw new ArgumentNullException("bgm");
		Camera = camera ?? throw new ArgumentNullException("camera");
		RequireIdentifier(Source.BattlefieldId, "BattlefieldId");
		if (Background.Mode != PresentationMode.ReplaceReady || Bgm.Mode != PresentationMode.ReplaceReady)
		{
			throw new ArgumentException("A battlefield profile must replace its background and BGM as one presentation mapping.");
		}
		ValidateBgm(Bgm);
	}

	private static string RequireIdentifier(string value, string parameterName)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("Identifier cannot be empty.", parameterName);
		}
		return value.Trim();
	}

	private static void ValidateBgm(BattlefieldBgmPresentation bgm)
	{
		if (bgm.Mode != PresentationMode.ReplaceReady || !string.IsNullOrWhiteSpace(bgm.TrackId))
		{
			return;
		}
		throw new ArgumentException("A replace-ready battlefield BGM requires a music track id.", "bgm");
	}
}
