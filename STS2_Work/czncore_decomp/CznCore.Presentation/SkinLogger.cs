using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;

namespace CznCore.Presentation;

internal sealed class SkinLogger
{
	private const string DefaultPrefix = "[CharacterSkin]";

	private string _prefix = "[CharacterSkin]";

	private Func<bool>? _isDebugEnabled;

	private readonly object OnceSync = new object();

	private readonly HashSet<string> InfoOnceKeys = new HashSet<string>(StringComparer.Ordinal);

	public SkinLogger(string prefix, Func<bool>? debug = null)
	{
		Configure(prefix);
		ConfigureDebug(debug);
	}

	public void Configure(string? prefix)
	{
		_prefix = (string.IsNullOrWhiteSpace(prefix) ? "[CharacterSkin]" : prefix.Trim());
	}

	public void ConfigureDebug(Func<bool>? isDebugEnabled)
	{
		_isDebugEnabled = isDebugEnabled;
	}

	public void Debug(string message, int priority = 2)
	{
		Func<bool>? isDebugEnabled = _isDebugEnabled;
		if (isDebugEnabled != null && isDebugEnabled())
		{
			Info(message, priority);
		}
	}

	public void Debug(Func<string> messageFactory, int priority = 2)
	{
		Func<bool>? isDebugEnabled = _isDebugEnabled;
		if (isDebugEnabled != null && isDebugEnabled())
		{
			Info(messageFactory(), priority);
		}
	}

	public void Info(string message, int priority = 2)
	{
		Log.Info(Format(message), priority);
	}

	public void InfoOnce(string key, string message, int priority = 2)
	{
		if (!string.IsNullOrWhiteSpace(key) && TryMarkInfoOnce(key))
		{
			Info(message, priority);
		}
	}

	public void Warn(string message, int priority = 2)
	{
		Log.Warn(Format(message), priority);
	}

	public void Error(string message, int priority = 2)
	{
		Log.Error(Format(message), priority);
	}

	public void Debug(string message, bool enabled, int priority = 2)
	{
		if (enabled)
		{
			Info(message, priority);
		}
	}

	public void WarnException(string context, Exception ex, int priority = 2)
	{
		Warn(context + FormatException(ex), priority);
	}

	public void ErrorException(string context, Exception ex, int priority = 2)
	{
		Error(context + FormatException(ex), priority);
	}

	public string Format(string message)
	{
		string text = StripExistingPrefix(message);
		if (string.IsNullOrWhiteSpace(text))
		{
			return _prefix;
		}
		return _prefix + " " + text.TrimStart();
	}

	public string FormatException(Exception ex)
	{
		return ex.GetType().Name + ": " + ex.Message;
	}

	private bool TryMarkInfoOnce(string key)
	{
		lock (OnceSync)
		{
			return InfoOnceKeys.Add(key);
		}
	}

	private string StripExistingPrefix(string message)
	{
		if (string.IsNullOrEmpty(message) || message[0] != '[')
		{
			return message;
		}
		int num = message.IndexOf(']');
		if (num <= 0 || num + 1 >= message.Length || message[num + 1] != ' ')
		{
			return message;
		}
		int num2 = num + 2;
		return message.Substring(num2, message.Length - num2);
	}
}
