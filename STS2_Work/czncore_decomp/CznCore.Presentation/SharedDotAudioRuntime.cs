using System;
using MegaCrit.Sts2.Core.Entities.Players;

namespace CznCore.Presentation;

internal sealed class SharedDotAudioRuntime
{
	private const float DotAudibilityGain = 3.9810717f;

	private readonly CharacterSkinSfxPlayerRuntime _player;

	private readonly CombatPresentationAudiencePolicy _audience;

	private readonly string _root;

	private readonly Action<string, bool, string>? _observe;

	public SharedDotAudioRuntime(CharacterSkinSfxPlayerRuntime player, CombatPresentationAudiencePolicy audience, string resourceRoot = "res://CznCore/", Action<string, bool, string>? observe = null)
	{
		_player = player ?? throw new ArgumentNullException("player");
		_audience = audience ?? throw new ArgumentNullException("audience");
		_root = (resourceRoot.EndsWith('/') ? resourceRoot : (resourceRoot + "/"));
		_observe = observe;
	}

	public bool TryPlay(string route, Player? applier)
	{
		if (route != "rift" && route != "pain" && route != "pain_multi")
		{
			return Report("dot:" + route, played: false, "unknown-dot-route");
		}
		if (!_player.IsEnabled || (applier != null && !_audience.Allows(applier, CombatPresentationChannel.CombatSfx)))
		{
			return Report("dot:" + route, played: false, "settings-or-audience-gate");
		}
		bool flag = route == "rift";
		int num = Random.Shared.Next(1, flag ? 5 : 6);
		string text = _root + "audio/sfx/dot/system_eff_" + (flag ? "crack" : "curse") + "_impact_" + num + ".ogg";
		return Report(text, _player.TryPlay(text, 3.9810717f), "shared-dot-player");
	}

	private bool Report(string path, bool played, string reason)
	{
		try
		{
			_observe?.Invoke(path, played, reason);
		}
		catch (Exception ex)
		{
			SkinLog.Warn("DOT audio observer failed: " + ex.Message);
		}
		return played;
	}
}
