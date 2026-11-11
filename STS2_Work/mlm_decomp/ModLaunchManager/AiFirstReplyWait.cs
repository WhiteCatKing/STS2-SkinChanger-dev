using System;

namespace ModLaunchManager;

internal sealed class AiFirstReplyWait(ulong startedMs)
{
	private int _startDone = -1;

	private ulong _retryShownUntil;

	internal bool Replied { get; private set; }

	internal void Progress(int done)
	{
		if (_startDone < 0)
		{
			_startDone = done;
		}
		else if (done > _startDone)
		{
			Replied = true;
		}
	}

	internal void Retry(AiTranslationRetryNotice notice, ulong nowMs)
	{
		if (notice.Kind == AiTranslationRetryKind.Format)
		{
			Replied = true;
		}
		else
		{
			_retryShownUntil = nowMs + (ulong)Math.Max(0.0, notice.Delay.TotalMilliseconds);
		}
	}

	internal int? Seconds(ulong nowMs)
	{
		if (!Replied && _startDone >= 0 && nowMs >= _retryShownUntil)
		{
			return (int)((nowMs - Math.Min(nowMs, startedMs)) / 1000);
		}
		return null;
	}
}
