using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ModLaunchManager;

internal static class ManagerReturnProtocol
{
	internal sealed class LogTail(string path)
	{
		private long _offset;

		internal string ReadNew()
		{
			try
			{
				using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
				if (fileStream.Length <= _offset)
				{
					return "";
				}
				fileStream.Seek(_offset, SeekOrigin.Begin);
				using StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8);
				string result = streamReader.ReadToEnd();
				_offset = fileStream.Length;
				return result;
			}
			catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
			{
				return "";
			}
		}
	}

	internal const string ReadyVariable = "MLM_RETURN_READY";

	internal const string PreviousGameVariable = "MLM_RETURN_PREVIOUS_GAME";

	internal const string AutoLaunchArgument = "--mlm-autolaunch";

	internal static List<string> Arguments(IReadOnlyList<string> engine, IReadOnlyList<string> user, string logFile)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < engine.Count; i++)
		{
			if (engine[i] == "--log-file")
			{
				i++;
			}
			else if (!engine[i].StartsWith("--log-file=", StringComparison.Ordinal) && !(engine[i] == "--mlm-autolaunch"))
			{
				list.Add(engine[i]);
			}
		}
		list.Add("--log-file");
		list.Add(logFile);
		List<string> list2 = user.Where((string argument) => argument != "--mlm-autolaunch").ToList();
		if (list2.Count > 0)
		{
			list.Add("--");
			list.AddRange(list2);
		}
		return list;
	}

	internal static string SessionLogFile(string currentLog, DateTime localTime)
	{
		return Path.Combine(Path.GetDirectoryName(currentLog), Path.GetFileNameWithoutExtension(currentLog) + localTime.ToString("yyyy-MM-dd'T'HH.mm.ss", CultureInfo.InvariantCulture) + Path.GetExtension(currentLog));
	}

	internal static string NewReadyFile()
	{
		return Path.Combine(Path.GetTempPath(), $"ModLaunchManager-return-{Guid.NewGuid():N}.ready");
	}

	internal static void WriteReady(string path)
	{
		string text = path + ".tmp";
		File.WriteAllText(text, Environment.ProcessId.ToString(CultureInfo.InvariantCulture), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		File.Move(text, path, overwrite: true);
	}

	internal static Process? ObserveProcess(int processId)
	{
		if (processId <= 0)
		{
			return null;
		}
		Process process = null;
		try
		{
			process = Process.GetProcessById(processId);
			_ = process.SafeHandle;
			return process;
		}
		catch (Exception ex) when (((ex is ArgumentException || ex is InvalidOperationException) ? 1 : 0) != 0)
		{
			process?.Dispose();
			return null;
		}
	}

	internal static bool IsRendererFailure(string logText)
	{
		if (!logText.Contains("Failed to initialize driver for device", StringComparison.Ordinal) && !logText.Contains("CreateDevice failed", StringComparison.Ordinal))
		{
			return logText.Contains("Unable to create DisplayServer", StringComparison.Ordinal);
		}
		return true;
	}

	internal static ProcessStartInfo StartAfterExit(int processId, string file, IReadOnlyList<string> arguments, string workingDirectory)
	{
		ProcessStartInfo processStartInfo;
		if (OperatingSystem.IsWindows())
		{
			processStartInfo = new ProcessStartInfo("powershell.exe");
			string[] array = new string[4] { "-NoProfile", "-NonInteractive", "-Command", "Wait-Process -Id $env:MLM_RELAUNCH_AFTER -ErrorAction SilentlyContinue; Start-Process -FilePath $env:MLM_RELAUNCH_FILE -ArgumentList $env:MLM_RELAUNCH_ARGUMENTS -WorkingDirectory $env:MLM_RELAUNCH_DIRECTORY" };
			foreach (string item in array)
			{
				processStartInfo.ArgumentList.Add(item);
			}
			processStartInfo.Environment["MLM_RELAUNCH_AFTER"] = processId.ToString(CultureInfo.InvariantCulture);
			processStartInfo.Environment["MLM_RELAUNCH_FILE"] = file;
			processStartInfo.Environment["MLM_RELAUNCH_ARGUMENTS"] = WindowsCommandLine(arguments);
			processStartInfo.Environment["MLM_RELAUNCH_DIRECTORY"] = workingDirectory;
		}
		else
		{
			processStartInfo = new ProcessStartInfo("/bin/sh");
			processStartInfo.ArgumentList.Add("-c");
			processStartInfo.ArgumentList.Add("while kill -0 \"$1\" 2>/dev/null; do sleep 0.2; done; shift; exec \"$@\"");
			processStartInfo.ArgumentList.Add("sh");
			processStartInfo.ArgumentList.Add(processId.ToString(CultureInfo.InvariantCulture));
			processStartInfo.ArgumentList.Add(file);
			foreach (string argument in arguments)
			{
				processStartInfo.ArgumentList.Add(argument);
			}
		}
		processStartInfo.UseShellExecute = false;
		processStartInfo.CreateNoWindow = true;
		processStartInfo.WorkingDirectory = workingDirectory;
		return processStartInfo;
	}

	internal static string WindowsCommandLine(IEnumerable<string> arguments)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string argument in arguments)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			if (argument.Length > 0 && argument.IndexOfAny(new char[5] { ' ', '\t', '\n', '\v', '"' }) < 0)
			{
				stringBuilder.Append(argument);
				continue;
			}
			stringBuilder.Append('"');
			int num = 0;
			string text = argument;
			foreach (char c in text)
			{
				if (c == '\\')
				{
					num++;
					continue;
				}
				stringBuilder.Append('\\', (c == '"') ? (num * 2 + 1) : num);
				num = 0;
				stringBuilder.Append(c);
			}
			stringBuilder.Append('\\', num * 2).Append('"');
		}
		return stringBuilder.ToString();
	}

	internal static void TryDelete(string path)
	{
		try
		{
			File.Delete(path);
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
		}
	}
}
