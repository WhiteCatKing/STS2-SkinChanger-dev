namespace CznEnemySkin.Domain;

internal sealed record MusicTrackProfile(string Id, BgmTransport Transport, string Locator, float SourceGainLinear = 1f, float ReferenceGainLinear = 1f, float LoopOffsetSeconds = 0f)
{
	public float PlaybackGainLinear => SourceGainLinear / ReferenceGainLinear;
}
