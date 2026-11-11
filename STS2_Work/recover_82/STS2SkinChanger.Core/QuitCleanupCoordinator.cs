using System;
using System.Threading;
using System.Threading.Tasks;

namespace STS2SkinChanger.Core;

internal sealed class QuitCleanupCoordinator
{
	private readonly QuitInterceptionPolicy _interception = new QuitInterceptionPolicy();

	private int _started;

	public bool AllowNativeQuit { get; private set; }

	public bool InitializationCompleted => _interception.InitializationCompleted;

	public void MarkInitializationCompleted()
	{
		_interception.MarkInitializationCompleted();
	}

	public bool HandleQuit(Func<Task<string>> cleanup, Action quit, Action<string> log)
	{
		if (!_interception.ShouldInterceptQuit())
		{
			if (_interception.ShouldReportStartupCall())
			{
				log("初始化尚未完成时收到 NGame.Quit：本次调用不按退出处理（不置退出标志、不停缓存写入、不执行退出清理），原版退出流程也不执行。");
			}
			return false;
		}
		if (AllowNativeQuit)
		{
			return true;
		}
		RunObserved(cleanup, quit, log);
		return false;
	}

	public async Task Run(Func<Task<string>> cleanup, Action quit, Action<string> log)
	{
		if (Interlocked.Exchange(ref _started, 1) != 0)
		{
			return;
		}
		try
		{
			log(await cleanup());
		}
		catch (Exception ex)
		{
			log("退出缓存清理未全部完成，仍按原流程退出：" + ex.GetBaseException().Message);
		}
		finally
		{
			AllowNativeQuit = true;
			quit();
		}
	}

	private async Task RunObserved(Func<Task<string>> cleanup, Action quit, Action<string> log)
	{
		try
		{
			await Run(cleanup, quit, log);
		}
		catch (Exception ex)
		{
			log("退出流程未完成，仍按原流程退出：" + ex.GetBaseException().Message);
		}
	}
}
