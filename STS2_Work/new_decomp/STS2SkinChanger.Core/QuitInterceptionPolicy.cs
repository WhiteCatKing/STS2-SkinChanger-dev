using System.Threading;

namespace STS2SkinChanger.Core;

internal sealed class QuitInterceptionPolicy
{
	private int _initializationCompleted;

	private int _startupCallReported;

	public bool InitializationCompleted => Volatile.Read(in _initializationCompleted) == 1;

	public void MarkInitializationCompleted()
	{
		Interlocked.Exchange(ref _initializationCompleted, 1);
	}

	public bool ShouldInterceptQuit()
	{
		return InitializationCompleted;
	}

	public bool ShouldReportStartupCall()
	{
		if (!InitializationCompleted)
		{
			return Interlocked.Exchange(ref _startupCallReported, 1) == 0;
		}
		return false;
	}
}
