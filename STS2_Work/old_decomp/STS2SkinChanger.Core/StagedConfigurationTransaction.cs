using System;

namespace STS2SkinChanger.Core;

internal static class StagedConfigurationTransaction
{
	internal static Exception? Run<T>(T original, T staged, Action<T> setCurrent, Action prepare, Action refresh, Action<T> persist, Action restoreResources)
	{
		bool flag = false;
		setCurrent(staged);
		try
		{
			prepare();
			flag = true;
			refresh();
			persist(staged);
			return null;
		}
		catch (Exception ex)
		{
			setCurrent(original);
			if (flag)
			{
				try
				{
					restoreResources();
				}
				catch (Exception ex2)
				{
					return new AggregateException(ex, ex2);
				}
			}
			return ex;
		}
	}
}
