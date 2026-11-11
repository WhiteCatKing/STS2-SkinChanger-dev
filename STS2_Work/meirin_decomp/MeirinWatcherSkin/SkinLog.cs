using System;
using CznCore.Presentation;

namespace MeirinWatcherSkin;

internal static class SkinLog
{
	private static readonly SkinLogger Logger = new SkinLogger("[MeirinWatcherSkin]", (Func<bool>)(() => Entry.IsSkinDebugEnabled));

	public static void Configure(string? prefix)
	{
		Logger.Configure(prefix);
	}

	public static void ConfigureDebug(Func<bool>? isDebugEnabled)
	{
		Logger.ConfigureDebug(isDebugEnabled);
	}

	public static void Debug(string message, int priority = 2)
	{
		Logger.Debug(message, priority);
	}

	public static void Debug(Func<string> messageFactory, int priority = 2)
	{
		Logger.Debug(messageFactory, priority);
	}

	public static void Debug(string message, bool enabled, int priority = 2)
	{
		Logger.Debug(message, enabled, priority);
	}

	public static void Info(string message, int priority = 2)
	{
		Logger.Info(message, priority);
	}

	public static void InfoOnce(string key, string message, int priority = 2)
	{
		Logger.InfoOnce(key, message, priority);
	}

	public static void Warn(string message, int priority = 2)
	{
		Logger.Warn(message, priority);
	}

	public static void Error(string message, int priority = 2)
	{
		Logger.Error(message, priority);
	}

	public static void WarnException(string context, Exception ex, int priority = 2)
	{
		Logger.WarnException(context, ex, priority);
	}

	public static void ErrorException(string context, Exception ex, int priority = 2)
	{
		Logger.ErrorException(context, ex, priority);
	}

	public static string Format(string message)
	{
		return Logger.Format(message);
	}

	public static string FormatException(Exception ex)
	{
		return Logger.FormatException(ex);
	}
}
