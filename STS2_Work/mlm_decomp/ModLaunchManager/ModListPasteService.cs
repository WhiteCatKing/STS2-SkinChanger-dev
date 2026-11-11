using System;
using System.Linq;
using System.Text;
using Godot;

namespace ModLaunchManager;

internal static class ModListPasteService
{
	internal const string ServiceHost = "paste.rs";

	private const string ServiceUrl = "https://paste.rs/";

	private const int TimeoutSeconds = 20;

	private const int MaxPasteIdLength = 32;

	internal static string DisplayName => "paste.rs";

	internal static void Upload(Node owner, string payload, Action<bool, string> onFinished)
	{
		Send(owner, "https://paste.rs/", (Method)2, payload, delegate(bool success, string body)
		{
			if (!success)
			{
				onFinished(arg1: false, body);
			}
			else
			{
				string text = body.Trim();
				if (!text.StartsWith("http", StringComparison.OrdinalIgnoreCase))
				{
					onFinished(arg1: false, MlmLocManager.Get("transfer.export.linkUnexpected", ("response", Excerpt(text))));
				}
				else
				{
					onFinished(arg1: true, text);
				}
			}
		});
	}

	internal static void Download(Node owner, string url, Action<bool, string> onFinished)
	{
		Send(owner, url, (Method)0, string.Empty, onFinished);
	}

	internal static bool TryResolveUrl(string input, out string url)
	{
		string text = input.Trim();
		url = string.Empty;
		if (text.Length == 0)
		{
			return false;
		}
		if (text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			url = text;
			return true;
		}
		if (text.Length <= 32 && text.All(char.IsAsciiLetterOrDigit))
		{
			url = "https://paste.rs/" + text;
			return true;
		}
		return false;
	}

	private static void Send(Node owner, string url, Method method, string payload, Action<bool, string> onFinished)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Invalid comparison between Unknown and I8
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Invalid comparison between Unknown and I8
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)owner))
		{
			onFinished(arg1: false, MlmLocManager.Get("transfer.network.cancelled"));
			return;
		}
		HttpRequest request = new HttpRequest
		{
			Timeout = 20.0,
			UseThreads = false
		};
		owner.AddChild((Node)(object)request, false, (InternalMode)0);
		bool answered = false;
		request.RequestCompleted += (RequestCompletedEventHandler)delegate(long result, long responseCode, string[] _, byte[] body)
		{
			if (!answered)
			{
				answered = true;
				((Node)request).QueueFree();
				if (result != 0L)
				{
					onFinished(arg1: false, MlmLocManager.Get("transfer.network.failed", ("result", (object)(Result)result)));
				}
				else
				{
					string @string = Encoding.UTF8.GetString(body ?? Array.Empty<byte>());
					if ((responseCode < 200 || responseCode > 299) ? true : false)
					{
						onFinished(arg1: false, MlmLocManager.Get("transfer.network.status", ("status", responseCode), ("response", Excerpt(@string))));
					}
					else
					{
						onFinished(arg1: true, @string);
					}
				}
			}
		};
		string[] array = (((long)method != 2) ? new string[1] { "Accept: text/plain" } : new string[1] { "Content-Type: text/plain; charset=utf-8" });
		Error val = (((long)method == 2) ? request.RequestRaw(url, array, method, Encoding.UTF8.GetBytes(payload)) : request.Request(url, array, method, ""));
		if ((int)val != 0)
		{
			answered = true;
			((Node)request).QueueFree();
			onFinished(arg1: false, MlmLocManager.Get("transfer.network.failed", ("result", val)));
		}
	}

	private static string Excerpt(string text)
	{
		string text2 = text.Replace('\n', ' ').Replace('\r', ' ').Trim();
		if (text2.Length > 120)
		{
			return text2.Substring(0, 120) + "…";
		}
		return text2;
	}
}
